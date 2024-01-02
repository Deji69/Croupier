#pragma once
#include <atomic>
#include <deque>
#include <mutex>
#include <shared_mutex>
#include <string>
#include <thread>
#include <vector>
#include <Windows.h>

enum class eClientMessage {
	Spin,
	Respin,
	SpinData,
	Prev,
};

class ClientMessage {
public:
	auto toString() const -> std::string;

	eClientMessage type;
	std::string args;
};

class CroupierClient {
public:
	CroupierClient();
	~CroupierClient();

	auto start() -> bool;
	auto stop() -> void;
	auto isConnected() const -> bool { return this->connected; }
	auto send(eClientMessage type, std::initializer_list<std::string> args = {}) -> void;
	auto tryTakeMessage(ClientMessage&) -> bool;

protected:
	auto reconnect() -> bool;
	auto writeMessage(const ClientMessage&) -> bool;
	auto processMessage(const std::string&) -> void;

private:
	std::thread clientThread;
	std::thread readThread;
	std::thread writeThread;
	mutable std::shared_mutex queueMutex;
	mutable std::shared_mutex messagesMutex;
	mutable std::shared_mutex connectionMutex;
	std::deque<ClientMessage> queue;
	std::deque<ClientMessage> messages;
	std::atomic<bool> connected = false;
	std::atomic<bool> keepOpen = false;
	HANDLE pipe = INVALID_HANDLE_VALUE;
	SOCKET sock = INVALID_SOCKET;
	OVERLAPPED overlapped = {};
};
