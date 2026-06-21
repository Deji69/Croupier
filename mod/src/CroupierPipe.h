#pragma once
#include <atomic>
#include <initializer_list>
#include <memory>
#include <string>
#include <thread>
#include <queue>
#include <string_view>
#include "CroupierClient.h"
#include <mutex>
#include <stop_token>
#include <utility>
#include <condition_variable>
#include <Logging.h>
#include <chrono>
#include <vector>
#include <algorithm>
#include <WinSock2.h>
#include "FixMinMax.h"
#include <cstdint>

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
		Logger::Info("Connecting to pipe: {}...", m_pipeName);

		while (!stopToken.stop_requested()) {
			HANDLE hPipe = CreateFileA(
				m_pipeName.c_str(),
				GENERIC_READ | GENERIC_WRITE,
				0,
				NULL,
				OPEN_EXISTING,
				0,
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
				continue;
			}

			DWORD dwMode = PIPE_READMODE_BYTE | PIPE_NOWAIT;
			SetNamedPipeHandleState(hPipe, &dwMode, NULL, NULL);

			Logger::Info("Connected to pipe server.", m_pipeName);
			m_connected = true;

			std::stop_source connectionStopSource;

			m_writeThread = std::jthread([this, hPipe, connectionStopSource](std::stop_token sToken) {
				writeLoop(hPipe, sToken, connectionStopSource);
			});

			readLoop(hPipe, stopToken, connectionStopSource);

			m_connected = false;
			m_writeThread.request_stop();
			m_sendQueue.notify_all();
			if (m_writeThread.joinable()) {
				m_writeThread.join();
			}

			CloseHandle(hPipe);
			Logger::Info("Connection dropped. Restarting...");
		}
	}

	void readLoop(HANDLE hPipe, std::stop_token stopToken, std::stop_source connectionStopSource) {
		std::vector<char> buffer(512);
		std::string accumulator;
		DWORD err = 0;

		while (!stopToken.stop_requested() && !connectionStopSource.stop_requested() && (err == 0 || err == ERROR_NO_DATA || err == ERROR_MORE_DATA)) {
			DWORD bytesRead = 0;

			BOOL success = ReadFile(hPipe, buffer.data(), 2, &bytesRead, NULL);

			if (!success) {
				err = GetLastError();
				if (err == ERROR_NO_DATA)
					std::this_thread::sleep_for(std::chrono::milliseconds(10)); // Yield CPU slightly
				continue;
			}

			if (bytesRead == 0) {
				break;
			}

			if (bytesRead != 2) {
				Logger::Warn("Expected 2 bytes for pipe message header but {} received.", bytesRead);
				continue;
			}

			uint16_t targetSize = static_cast<unsigned char>(buffer[0]) + static_cast<unsigned char>(buffer[1]) * 256;
			size_t triesUntilReset = 500;
			
			// This sus attempts counting is because the data may not arrive right away but it should do after some cycles, unless there's a disconnect or something weird happened, so...
			for (; accumulator.size() < targetSize && triesUntilReset > 0; --triesUntilReset) {
				success = ReadFile(hPipe, buffer.data(), static_cast<DWORD>(buffer.size()), &bytesRead, NULL);

				if (!success) {
					err = GetLastError();
					if (err == ERROR_NO_DATA)
						std::this_thread::sleep_for(std::chrono::milliseconds(10));
					else if (err != ERROR_MORE_DATA)
						break;
				}

				accumulator.append(buffer.data(), bytesRead);
			}

			if (!accumulator.empty() && triesUntilReset != 0) {
				handleReceivedMessage(accumulator);
			}

			accumulator.clear();
		}

		connectionStopSource.request_stop();
	}

	void writeLoop(HANDLE hPipe, std::stop_token stopToken, std::stop_source connectionStopSource) {
		std::string message;
		char size[2] = {};
		DWORD err = 0;
		
		while (!stopToken.stop_requested() && !connectionStopSource.stop_requested()) {
			if (!m_sendQueue.dequeue(message, stopToken))
				break;

			DWORD bytesWritten = 0;
			auto intSize = static_cast<uint16_t>(message.size());
			uint8_t header[2] {};
			header[0] = intSize & 0xFF;
			header[1] = (intSize >> 8) & 0xFF;

			for (auto i = 0; i < 2; i += bytesWritten) {
				BOOL result = WriteFile(hPipe, header + i, 2, &bytesWritten, NULL);

				if (!result) {
					err = GetLastError();
					Logger::Error("Pipe message header write failed (error: {}).", err);
					break;
				}

				// Buffer full? Sleep and retry.
				if (bytesWritten == 0) {
					std::this_thread::sleep_for(std::chrono::milliseconds(10));
					continue;
				}
			}

			if (err != 0) break;

			DWORD chunkWritten = 0;
			for (DWORD written = 0; written < message.size(); written += chunkWritten) {
				BOOL result = WriteFile(
					hPipe,
					message.data() + written,
					static_cast<DWORD>(message.size() - written),
					&chunkWritten,
					NULL
				);

				if (!result) {
					err = GetLastError();
					Logger::Error("Pipe write failed (error: {}).", err);
					connectionStopSource.request_stop();
					break;
				}

				// Buffer full? Sleep and retry.
				if (chunkWritten == 0) {
					std::this_thread::sleep_for(std::chrono::milliseconds(10));
					continue;
				}

				written += chunkWritten;
			}

			if (err != 0) break;

			if (!FlushFileBuffers(hPipe))
				Logger::Error("Pipe flush failed.");
		}

		connectionStopSource.request_stop();
	}

public:
	BiDirectionalPipeClient(const std::string& pipeName) : m_pipeName("\\\\.\\pipe\\" + pipeName) {}

	~BiDirectionalPipeClient() {
		stop();
	}

	auto start() -> void {
		m_workerThread = std::jthread([this](std::stop_token stopToken) {
			runEngine(stopToken);
		});
	}

	auto stop() -> void {
		m_workerThread.request_stop();
		m_writeThread.request_stop();
		m_sendQueue.notify_all();
	}

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
