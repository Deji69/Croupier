#pragma once
#include <atomic>
#include <deque>
#include <mutex>
#include <shared_mutex>
#include <string>
#include <thread>
#include <vector>
#include <Windows.h>
#include <winsock.h>
#include "Events.h"
#include "RouletteMission.h"

enum class eClientMessage {
	Spin,
	Respin,
	AutoSpin,
	SpinData,
	Next,
	Prev,
	Random,
	Missions,
	SpinLock,
	Streak,
	ToggleSpinLock,
	ToggleTimer,
	Timer,
	KillValidation,
	MissionStart,
	MissionFailed,
	MissionComplete,
	MissionOutroBegin,
	ResetStreak,
	ResetTimer,
	StartTimer,
	StopTimer,
	SplitTimer,
	PauseTimer,
	LoadStarted,
	LoadFinished,
	Event,
};

class ClientMessage {
public:
	auto toString() const -> std::string;

	eClientMessage type;
	std::string args;
	std::string raw;
	bool isRaw = false;
};

class CroupierClient {
public:
	CroupierClient();
	~CroupierClient();

	auto start() -> bool;
	auto stop() -> void;
	auto abort() -> void;
	auto isConnected() const -> bool { return this->connected; }
	auto send(eClientMessage type, std::initializer_list<std::string> args = {}) -> void;
	auto sendRaw(std::string msg) -> void;
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
	std::atomic_bool connected = false;
	std::atomic_bool keepOpen = false;
	SOCKET sock = INVALID_SOCKET;
	OVERLAPPED overlapped = {};
};
