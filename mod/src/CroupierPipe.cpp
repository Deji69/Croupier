#include "CroupierPipe.h"

#include <initializer_list>
#include <vector>
#include <utility>
#include <iterator>
#include <map>
#include "CroupierClient.h"
#include "util.h"
#include <string>
#include <stop_token>

#pragma comment(lib, "Ws2_32.lib")

using namespace std::chrono_literals;

std::map<eClientMessage, std::string> pipeMessageTypeMap = {
	{eClientMessage::Respin, "Respin"},
	{eClientMessage::AutoSpin, "AutoSpin"},
	{eClientMessage::Spin, "Spin"},
	{eClientMessage::SpinData, "SpinData"},
	{eClientMessage::BingoData, "BingoData"},
	{eClientMessage::GameMode, "GameMode"},
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
	{eClientMessage::Event, "Event"},
};
std::map<std::string, eClientMessage> pipeMessageTypeMapRev = {
	{"Respin", eClientMessage::Respin},
	{"AutoSpin", eClientMessage::AutoSpin},
	{"Spin", eClientMessage::Spin},
	{"SpinData", eClientMessage::SpinData},
	{"BingoData", eClientMessage::BingoData},
	{"GameMode", eClientMessage::GameMode},
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
	{"Event", eClientMessage::Event},
};

auto BiDirectionalPipeClient::handleReceivedMessage(const std::string& msg) -> void
{
	// Try to parse message
	auto toks = split(msg, ":", 2);
	if (toks.size() < 2) return;

	auto it = pipeMessageTypeMapRev.find(std::string(toks[0]));
	if (it == end(pipeMessageTypeMapRev)) return;

	// Put message in the queue
	auto message = ClientMessage();
	message.type = it->second;
	message.args = toks[1];
	this->m_receiveQueue.enqueue(std::move(message));
}

auto BiDirectionalPipeClient::tryTakeMessage(ClientMessage& messageOut) -> bool
{
	if (this->m_receiveQueue.empty()) return false;
	return this->m_receiveQueue.dequeue(messageOut, std::stop_token{});
}

auto BiDirectionalPipeClient::send(eClientMessage type, std::initializer_list<std::string> args) -> void
{
	ClientMessage message;
	message.type = type;
	std::string argStr;
	for (auto const& arg : args)
		argStr += (argStr.empty() ? "" : "\t") + arg;
	message.args = std::move(argStr);
	this->m_sendQueue.enqueue(message.toString());
}

auto BiDirectionalPipeClient::sendRaw(std::string msg) -> void
{
	ClientMessage message;
	message.raw = msg;
	message.isRaw = true;
	this->m_sendQueue.enqueue(message.toString());
}
