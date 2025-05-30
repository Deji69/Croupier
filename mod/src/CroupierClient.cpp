#include <WinSock2.h>
#include <chrono>
#include <format>
#include <map>
#include <shared_mutex>
#include <WS2tcpip.h>
#include <Logging.h>
#include "CroupierClient.h"
#include "util.h"

#pragma comment(lib, "Ws2_32.lib")

using namespace std::chrono_literals;

std::map<eClientMessage, std::string> clientMessageTypeMap = {
	{eClientMessage::Respin, "Respin"},
	{eClientMessage::AutoSpin, "AutoSpin"},
	{eClientMessage::Spin, "Spin"},
	{eClientMessage::SpinData, "SpinData"},
	{eClientMessage::Prev, "Prev"},
	{eClientMessage::Next, "Next"},
	{eClientMessage::Random, "Random"},
	{eClientMessage::Missions, "Missions"},
	{eClientMessage::SpinLock, "SpinLock"},
	{eClientMessage::ToggleSpinLock, "ToggleSpinLock"},
	{eClientMessage::ToggleTimer, "ToggleTimer"},
	{eClientMessage::Timer, "Timer"},
	{eClientMessage::KillValidation, "KillValidation"},
	{eClientMessage::MissionStart, "MissionStart"},
	{eClientMessage::MissionComplete, "MissionComplete"},
	{eClientMessage::MissionOutroBegin, "MissionOutroBegin"},
	{eClientMessage::MissionFailed, "MissionFailed"},
	{eClientMessage::Streak, "Streak"},
	{eClientMessage::ResetStreak, "ResetStreak"},
	{eClientMessage::ResetTimer, "ResetTimer"},
	{eClientMessage::StartTimer, "StartTimer"},
	{eClientMessage::StopTimer, "StopTimer"},
	{eClientMessage::SplitTimer, "SplitTimer"},
	{eClientMessage::PauseTimer, "PauseTimer"},
	{eClientMessage::LoadStarted, "LoadStarted"},
	{eClientMessage::LoadFinished, "LoadFinished"},
};
std::map<std::string, eClientMessage> clientMessageTypeMapRev = {
	{"Respin", eClientMessage::Respin},
	{"AutoSpin", eClientMessage::AutoSpin},
	{"Spin", eClientMessage::Spin},
	{"SpinData", eClientMessage::SpinData},
	{"Prev", eClientMessage::Prev},
	{"Next", eClientMessage::Next},
	{"Random", eClientMessage::Random},
	{"Missions", eClientMessage::Missions},
	{"SpinLock", eClientMessage::SpinLock},
	{"ToggleSpinLock", eClientMessage::ToggleSpinLock},
	{"ToggleTimer", eClientMessage::ToggleTimer},
	{"Timer", eClientMessage::Timer},
	{"KillValidation", eClientMessage::KillValidation},
	{"MissionStart", eClientMessage::MissionStart},
	{"MissionComplete", eClientMessage::MissionComplete},
	{"MissionOutroBegin", eClientMessage::MissionOutroBegin},
	{"MissionFailed", eClientMessage::MissionFailed},
	{"Streak", eClientMessage::Streak},
	{"ResetStreak", eClientMessage::ResetStreak},
	{"ResetTimer", eClientMessage::ResetTimer},
	{"StartTimer", eClientMessage::StartTimer},
	{"StopTimer", eClientMessage::StopTimer},
	{"SplitTimer", eClientMessage::SplitTimer},
	{"PauseTimer", eClientMessage::PauseTimer},
	{"LoadStarted", eClientMessage::LoadStarted},
	{"LoadFinished", eClientMessage::LoadFinished},
};

auto ClientMessage::toString() const -> std::string {
	auto it = clientMessageTypeMap.find(this->type);
	if (it == end(clientMessageTypeMap)) return "";
	return std::format("{}:{}", it->second, this->args);
}

CroupierClient::CroupierClient() {}

auto CroupierClient::reconnect() -> bool {
	if (!this->keepOpen) return false;
	if (this->connected) return true;

	// Make this thread-safe (?)
	if (!connectionMutex.try_lock()) return false;

	if (!this->connected) {
		addrinfo* addressInfo = nullptr;
		auto connectInfo = addrinfo {};
		connectInfo.ai_family = AF_INET;
		connectInfo.ai_socktype = SOCK_STREAM;
		connectInfo.ai_protocol = IPPROTO_TCP;

		if (getaddrinfo("127.0.0.1", "4747", &connectInfo, &addressInfo) != 0) {
			addressInfo = nullptr;
			Logger::Error("getaddrinfo failed");
		}

		auto addrInfo = addressInfo;

		if (this->sock != INVALID_SOCKET) {
			closesocket(this->sock);
			this->sock = INVALID_SOCKET;
		}

		for (; addrInfo != nullptr; addrInfo = addrInfo->ai_next) {
			this->sock = socket(addrInfo->ai_family, addrInfo->ai_socktype, addrInfo->ai_protocol);
			if (this->sock == INVALID_SOCKET)
				continue;

			if (connect(this->sock, addrInfo->ai_addr, static_cast<int>(addrInfo->ai_addrlen)) != SOCKET_ERROR)
				break;
			closesocket(this->sock);
			this->sock = INVALID_SOCKET;

			auto error = WSAGetLastError();
			if (error != 0) {
				LPSTR buffer = nullptr;
			
				FormatMessageA(
					FORMAT_MESSAGE_ALLOCATE_BUFFER | FORMAT_MESSAGE_FROM_SYSTEM | FORMAT_MESSAGE_IGNORE_INSERTS,
					NULL, error,
					MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT),
					(LPSTR)&buffer, 0, NULL
				);

				Logger::Error("Error connecting socket ({})", trim(buffer));
			}
		}

		this->connected = this->sock != INVALID_SOCKET;
		if (addressInfo) freeaddrinfo(addressInfo);
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
			if (!this->queue.empty()) {
				this->queueMutex.lock();
				while (!this->queue.empty()) {
					auto& msg = this->queue.front();
					writeQueue.push_back(std::move(msg));
					this->queue.pop_front();
				}
				this->queueMutex.unlock();

				// Send out messages to socket
				while (!writeQueue.empty()) {
					auto& msg = writeQueue.back();
					if (!this->writeMessage(msg)) break;
					writeQueue.pop_back();
				}
			}

			if (this->connected) std::this_thread::sleep_for(5ms);
		}
	});
	readThread = std::thread([this] {
		char buffer[1024] = {0};

		while (this->keepOpen) {
			if (!this->reconnect()) {
				std::this_thread::sleep_for(3s);
				continue;
			}

			// Read message from socket
			auto read = recv(this->sock, buffer, sizeof(buffer), 0);
			if (read == SOCKET_ERROR) {
				if (WSAGetLastError() != WSAETIMEDOUT)
					this->connected = false;
				continue;
			}

			// Process message and add to queue
			auto msg = std::string(buffer, read);
			this->processMessage(msg);
		}
	});
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

auto CroupierClient::abort() -> void {
	this->keepOpen = false;
	this->readThread.detach();
	this->writeThread.detach();
	this->connected = false;

	if (this->sock != INVALID_SOCKET)
	{
		if (shutdown(this->sock, SD_SEND) == SOCKET_ERROR)
			Logger::Error("Shutdown failed");

		closesocket(this->sock);
		this->sock = INVALID_SOCKET;
	}

	WSACleanup();
}

CroupierClient::~CroupierClient() {
	if (this->keepOpen) this->abort();
}

auto CroupierClient::writeMessage(const ClientMessage& msg) -> bool {
	auto data = msg.toString() + "\n";
	DWORD written = 0;
	int bytes_sent = ::send(this->sock, data.c_str(), data.size(), 0);
	if (bytes_sent == SOCKET_ERROR) {
		if (WSAGetLastError() != WSAETIMEDOUT)
			this->connected = false;
		return false;
	}
	return true;
}

auto CroupierClient::processMessage(const std::string& msg) -> void {
	// Check for multiple messages
	auto msgs = split(msg, "\n");
	if (msgs.size() > 1) {
		for (auto& msg : msgs) {
			if (trim(msg).empty()) continue;
			this->processMessage(std::string(msg));
		}
		return;
	}

	// Try to parse message
	auto toks = split(msg, ":", 2);
	if (toks.size() < 2) return;

	auto it = clientMessageTypeMapRev.find(std::string(toks[0]));
	if (it == end(clientMessageTypeMapRev)) return;

	// Put message in the queue
	auto lock = std::unique_lock(this->messagesMutex);
	auto message = ClientMessage();
	message.type = it->second;
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
		argStr += (argStr.empty() ? "" : "\t") + arg;
	message.args = std::move(argStr);
	this->queue.push_back(std::move(message));
}
