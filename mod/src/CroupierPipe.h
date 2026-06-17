#pragma once
#include <atomic>
#include <initializer_list>
#include <memory>
#include <string>
#include <thread>
#include <queue>
#include <string_view>
#include <Windows.h>
#include "FixMinMax.h"
#include "CroupierClient.h"
#include <mutex>
#include <stop_token>
#include <utility>
#include <condition_variable>
#include <Logging.h>
#include <chrono>
#include <vector>
#include <algorithm>

constexpr const DWORD PIPE_BUFFER_SIZE = 8192;

namespace details
{
	static inline std::string format_name(std::string_view name) {
		std::string formatted = R"(\\.\pipe\)";
		formatted += name;
		return formatted;
	}

	struct handle_deleter {
		void operator()(HANDLE handle) {
			if (handle != NULL && handle != INVALID_HANDLE_VALUE)
				CloseHandle(handle);
		}
	};

	using unique_handle = std::unique_ptr<void, handle_deleter>;
}

template<typename T>
class ThreadSafeQueue {
public:
	auto enqueue(const T& item) -> void {
		{
			std::lock_guard<std::mutex> lock(m_mutex);
			m_queue.push(item);
		}
		m_cv.notify_one();
	}

	auto dequeue(T& item, std::stop_token stopToken) -> bool {
		std::unique_lock<std::mutex> lock(m_mutex);
		
		m_cv.wait(lock, [this, stopToken]() { 
			return stopToken.stop_requested() || !m_queue.empty(); 
		});

		if (stopToken.stop_requested() && m_queue.empty()) {
			return false;
		}

		item = std::move(m_queue.front());
		m_queue.pop();
		return true;
	}

	auto notify_all() -> void {
		m_cv.notify_all();
	}

	auto empty() -> bool {
		std::lock_guard<std::mutex> lock(m_mutex);
		return m_queue.empty();
	}

private:
	std::queue<T> m_queue;
	std::mutex m_mutex;
	std::condition_variable m_cv;
};

class BiDirectionalPipeClient {
private:
	std::string m_pipeName;
	ThreadSafeQueue<std::string> m_sendQueue;
	ThreadSafeQueue<ClientMessage> m_receiveQueue;
	
	std::jthread m_workerThread;
	std::jthread m_writeThread;
	std::atomic_bool m_connected;

	auto runEngine(std::stop_token stopToken) -> void {
		while (!stopToken.stop_requested()) {
			Logger::Info("Connecting to pipe: {}...", m_pipeName);

			HANDLE hPipe = CreateFileA(
				m_pipeName.c_str(),
				GENERIC_READ | GENERIC_WRITE,
				0,
				NULL,
				OPEN_EXISTING,
				FILE_FLAG_WRITE_THROUGH,
				NULL
			);

			if (hPipe == INVALID_HANDLE_VALUE) {
				m_connected = false;
				DWORD err = GetLastError();
				// 2 = File not found (server not ready); 231 = Pipe busy
				if (err == ERROR_FILE_NOT_FOUND || err == ERROR_PIPE_BUSY) {
					WaitNamedPipeA(m_pipeName.c_str(), 1000);
				} else {
					std::this_thread::sleep_for(std::chrono::milliseconds(1000));
				}
				continue; // Retry loop
			}

			DWORD dwMode = PIPE_READMODE_BYTE | PIPE_NOWAIT;
			SetNamedPipeHandleState(hPipe, &dwMode, NULL, NULL);

			Logger::Info("Connected to pipe server.", m_pipeName);
			m_connected = true;

			// Start up the dedicated writing loop thread assigned to this file handle
			m_writeThread = std::jthread([this, hPipe](std::stop_token sToken) {
				writeLoop(hPipe, sToken);
			});

			// Execute the read sequence blocking block on the main execution worker
			readLoop(hPipe, stopToken);

			// If the read loop collapses (disconnection), violently shut down the write thread
			m_connected = false;
			m_writeThread.request_stop();
			m_sendQueue.notify_all(); // Wake write loop condition variable
			if (m_writeThread.joinable()) {
				m_writeThread.join();
			}

			CloseHandle(hPipe);
			Logger::Info("Connection dropped. Re-initializing loop context...");
		}
	}

	void readLoop(HANDLE hPipe, std::stop_token stopToken) {
		std::vector<char> buffer(512);
		std::string accumulator;

		while (!stopToken.stop_requested()) {
			DWORD bytesRead = 0;
			
			// Blocking read statement execution
			BOOL success = ReadFile(hPipe, buffer.data(), static_cast<DWORD>(buffer.size()), &bytesRead, NULL);

			// If ReadFile fails or returns 0 bytes, the server disconnected
			if (!success) {
				DWORD err = GetLastError();
				// ERROR_NO_DATA means there is nothing to read right now (non-blocking status)
				if (err == ERROR_NO_DATA) {
					std::this_thread::sleep_for(std::chrono::milliseconds(10)); // Yield CPU slightly
					continue;
				}
				break; // A real error occurred, disconnect
			}

			if (bytesRead == 0) {
				break; 
			}

			// Stream processing matching structural split design logic
			for (DWORD i = 0; i < bytesRead; ++i) {
				if (buffer[i] == '\0') {
					if (!accumulator.empty()) {
						handleReceivedMessage(accumulator);
						accumulator.clear();
					}
				} else {
					accumulator.push_back(buffer[i]);
				}
			}
		}
	}

	void writeLoop(HANDLE hPipe, std::stop_token stopToken) {
		std::string message;

		while (!stopToken.stop_requested()) {
			if (!m_sendQueue.dequeue(message, stopToken)) {
				break; // Stop token raised, safely drop out
			}

			// Append the matching structural null character terminator
			message.push_back('\0');

			DWORD bytesWritten = 0;
			BOOL success = WriteFile(hPipe, message.data(), static_cast<DWORD>(message.size()), &bytesWritten, NULL);

			if (!success) {
				Logger::Info("[Write Error]: Failed writing down stream pipe.");
				break; // Fall back and break loop out to force re-connection
			}

			FlushFileBuffers(hPipe);
		}
	}

public:
	BiDirectionalPipeClient(const std::string& pipeName) : m_pipeName("\\\\.\\pipe\\" + pipeName) {}

	~BiDirectionalPipeClient() {
		stop();
	}

	// Start background threading processing asynchronously
	auto start() -> void {
		m_workerThread = std::jthread([this](std::stop_token stopToken) {
			runEngine(stopToken);
		});
	}

	// Forceful clean closure
	auto stop() -> void {
		m_workerThread.request_stop();
		m_writeThread.request_stop();
		m_sendQueue.notify_all();
	}

	// Add string content safely to outbound buffer system from main thread 
	auto enqueueMessage(const std::string& message) -> void {
		m_sendQueue.enqueue(message);
	}

	auto send(eClientMessage type, std::initializer_list<std::string> args = {}) -> void;
	auto sendRaw(std::string msg) -> void;
	auto tryTakeMessage(ClientMessage& out) -> bool;
	auto handleReceivedMessage(const std::string& message) -> void;

	auto isConnected() const -> bool {
		return m_connected;
	}
};
