#include <WinSock2.h>
#include "CroupierClient.h"
#include <chrono>
#include <format>
#include <map>
#include <shared_mutex>
#include <WS2tcpip.h>
#include <Logging.h>
#include "util.h"

#pragma comment(lib, "Ws2_32.lib")

using namespace std::chrono_literals;

static constexpr const char* pipeName = "\\\\.\\pipe\\CroupierPipe";
std::map<eClientMessage, std::string> clientMessageTypeMap = {
	{eClientMessage::Respin, "Respin"},
	{eClientMessage::Spin, "Spin"},
	{eClientMessage::SpinData, "SpinData"},
};
std::map<std::string, eClientMessage> clientMessageTypeMapRev = {
	{"Respin", eClientMessage::Respin},
	{"Spin", eClientMessage::Spin},
	{"SpinData", eClientMessage::SpinData},
};

auto ClientMessage::toString() const -> std::string {
	auto it = clientMessageTypeMap.find(this->type);
	if (it == end(clientMessageTypeMap)) return "";
	//std::string args;
	//for (auto const& arg : this->args) {
	//	if (!args.empty()) args += "\t";
	//	args += arg;
	//}
	return std::format("{}:{}", it->second, this->args);
}

CroupierClient::CroupierClient() {
}

auto CroupierClient::reconnect() -> bool {
	if (!this->keepOpen) return false;
	if (this->connected) return true;

	if (!connectionMutex.try_lock()) return false;

	if (!this->connected) {
		addrinfo* addressInfo = nullptr;
		auto connectInfo = addrinfo {};
		connectInfo.ai_family = AF_INET;
		connectInfo.ai_socktype = SOCK_STREAM;
		connectInfo.ai_protocol = IPPROTO_TCP;

		if (getaddrinfo("127.0.0.1", "8898", &connectInfo, &addressInfo) != 0) {
			Logger::Error("getaddrinfo failed");
		}

		auto addrInfo = addressInfo;

		if (this->sock != INVALID_SOCKET) {
			closesocket(this->sock);
			this->sock = INVALID_SOCKET;
		}

		for (; addrInfo != nullptr; addrInfo = addrInfo->ai_next) {
			this->sock = socket(addrInfo->ai_family, addrInfo->ai_socktype, addrInfo->ai_protocol);
			if (this->sock == INVALID_SOCKET) {
				Logger::Error("Error creating socket");
				continue;
			}

			if (connect(this->sock, addrInfo->ai_addr, static_cast<int>(addrInfo->ai_addrlen)) != SOCKET_ERROR)
				break;
			closesocket(this->sock);
			this->sock = INVALID_SOCKET;
			Logger::Error("Error connecting to socket");
		}

		this->connected = this->sock != INVALID_SOCKET;
		freeaddrinfo(addressInfo);
	}

	connectionMutex.unlock();
	return this->connected;
}

auto CroupierClient::start() -> bool {
	if (this->keepOpen) return false;

	this->keepOpen = true;
	this->connected = false;

	WSADATA wsaData = {};
	int res = WSAStartup(MAKEWORD(2, 2), &wsaData);
	if (res != 0) {
		Logger::Error("WSAStartup failed {}", res);
		return false;
	}

	writeThread = std::thread([this] {
		std::vector<ClientMessage> writeQueue;

		while (this->keepOpen) {
			if (!this->reconnect()) {
				std::this_thread::sleep_for(3s);
				continue;
			}

			// Process the outward message queue
			if (this->queueMutex.try_lock()) {
				while (!this->queue.empty()) {
					auto& msg = this->queue.front();
					writeQueue.push_back(std::move(msg));
					this->queue.pop_front();
				}
				this->queueMutex.unlock();
			}

			// Send out messages to socket
			while (!writeQueue.empty()) {
				auto& msg = writeQueue.back();
				if (!this->writeMessage(msg)) break;
				writeQueue.pop_back();
			}

			if (this->connected) std::this_thread::sleep_for(100ms);
		}
	});
	readThread = std::thread([this] {
		char buffer[1024] = {0};
		while (this->keepOpen) {
			if (!this->reconnect()) {
				std::this_thread::sleep_for(3s);
				continue;
			}
	
			auto read = recv(this->sock, buffer, sizeof(buffer), 0);
			if (read == -1) {
				this->connected = false;
				continue;
			}
	
			this->processMessage(std::string(buffer, read));
		}
	});
	/*clientThread = std::thread([this] {
		while (true) {
			this->pipe = CreateFile(pipeName, GENERIC_WRITE | GENERIC_READ | FILE_WRITE_ATTRIBUTES, 0, NULL, OPEN_EXISTING, 0, NULL);
			this->overlapped = {};
			if (this->pipe == INVALID_HANDLE_VALUE) {
				std::this_thread::sleep_for(5s);
				continue;
			}

			DWORD mode = PIPE_READMODE_MESSAGE;
			auto success = SetNamedPipeHandleState(&this->pipe, &mode, NULL, NULL);
			if (!success) { 
				Logger::Error("SetNamedPipeHandleState failed. GLE={}", GetLastError());
				CloseHandle(this->pipe);
				continue;
			}

			char buffer[512] = "";
			DWORD read = 0;
			this->connected = true;

			while (this->connected) {
				do {
					// Read from the pipe
					success = ReadFile(this->pipe, buffer, 512, &read, NULL);
					auto err = GetLastError();

					if (!success && err != ERROR_MORE_DATA) {
						this->connected = false;
						this->pipe = INVALID_HANDLE_VALUE;
						CloseHandle(this->pipe);
						break;
					}
				} while (!success);

				// Process the outward message queue
				if (this->queueMutex.try_lock()) {
					while (!this->queue.empty()) {
						auto& msg = this->queue.front();
						pipeMessageOutQueue.push_back(std::move(msg));
						this->queue.pop_front();
					}
					this->queueMutex.unlock();
				}

				if (!this->connected) break;

				// Write out messages to pipe
				while (!pipeMessageOutQueue.empty()) {
					auto& msg = pipeMessageOutQueue.back();
					if (!this->writeMessage(msg)) break;
					pipeMessageOutQueue.pop_back();
				}
			}

			if (!this->connected)
				std::this_thread::sleep_for(250ms);
		}

		CloseHandle(pipe);
	});*/
	return true;
}

auto CroupierClient::stop() -> void {
	this->keepOpen = false;
	this->readThread.join();
	this->writeThread.join();
	this->connected = false;

	if (this->sock != INVALID_SOCKET) {
		if (shutdown(this->sock, SD_SEND) == SOCKET_ERROR)
			Logger::Error("Shutdown failed");

		closesocket(this->sock);
		this->sock = INVALID_SOCKET;
	}

	WSACleanup();
}

CroupierClient::~CroupierClient() {
	if (this->keepOpen) this->stop();
}

auto CroupierClient::writeMessage(const ClientMessage& msg) -> bool {
	auto data = msg.toString() + "\n";
	DWORD written = 0;
	int bytes_sent = ::send(this->sock, data.c_str(), data.size(), 0);
	if (bytes_sent == SOCKET_ERROR) {
		this->connected = false;
		return false;
	}
	return true;
	//BOOL success = WriteFile(this->pipe, data.data(), data.size(), &written, NULL);
	//DWORD err = GetLastError();
	//if (err == ERROR_NO_DATA || err == ERROR_PIPE_NOT_CONNECTED) {
	//	this->connected = false;
	//	return false;
	//}
	//return success;
}

auto CroupierClient::processMessage(const std::string& msg) -> void {
	auto toks = split(msg, ":", 2);
	if (toks.size() < 2) return;
	auto it = clientMessageTypeMapRev.find(std::string(toks[0]));
	if (it == end(clientMessageTypeMapRev)) return;
	auto msgType = it->second;
	auto lock = std::unique_lock(this->messagesMutex);
	auto message = ClientMessage();
	message.type = msgType;
	message.args = toks[1];
	this->messages.push_back(std::move(message));
}

auto CroupierClient::tryTakeMessage(ClientMessage& messageOut) -> bool {
	if (this->messages.empty()) return false;
	if (!this->messagesMutex.try_lock()) return false;
	messageOut = std::move(this->messages.front());
	this->messages.pop_front();
	this->messagesMutex.unlock();
	return true;
}

auto CroupierClient::send(eClientMessage type, std::initializer_list<std::string> args) -> void {
	if (!this->connected) return;
	ClientMessage message;
	message.type = type;
	std::string argStr;
	for (auto const& arg : args)
		argStr += (argStr.empty() ? "" : " ") + arg;
	message.args = std::move(argStr);
	this->queue.push_back(std::move(message));
}
