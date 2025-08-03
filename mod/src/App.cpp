#include "App.h"
#include "Config.h"
#include "CroupierClient.h"
#include "Roulette.h"
#include "State.h"
#include <format>
#include <string>
#include <string_view>

using namespace Croupier;
using namespace std::string_literals;
using namespace std::string_view_literals;

auto Croupier::SendAutoSpin(eMission mission) -> void {
	State::current.client.send(eClientMessage::AutoSpin, {getMissionCodename(mission).value_or("").data()});
}

auto Croupier::SendRespin(eMission mission) -> void {
	State::current.client.send(eClientMessage::Respin, {getMissionCodename(mission).value_or("").data()});
}

auto Croupier::SendMissions() -> void {
	std::string buffer;

	for (auto const id : Config::main.missionPool) {
		auto codename = getMissionCodename(id);
		if (!codename.has_value()) continue;
		if (!buffer.empty()) buffer += ",";
		buffer += *codename;
	}

	State::current.client.send(eClientMessage::Missions, {buffer});
}

auto Croupier::SendToggleSpinLock() -> void {
	State::current.client.send(eClientMessage::ToggleSpinLock);
}

auto Croupier::SendToggleTimer(bool enable) -> void {
	State::current.client.send(eClientMessage::ToggleTimer, {enable ? "1" : "0"});
}

auto Croupier::SendLoadStarted() -> void {
	State::current.client.send(eClientMessage::LoadStarted);
}

auto Croupier::SendLoadFinished() -> void {
	State::current.client.send(eClientMessage::LoadFinished);
}

auto Croupier::SendResetStreak() -> void {
	State::current.client.send(eClientMessage::ResetStreak);
}

auto Croupier::SendResetTimer() -> void {
	State::current.client.send(eClientMessage::ResetTimer);
}

auto Croupier::SendStartTimer() -> void {
	State::current.client.send(eClientMessage::StartTimer);
}

auto Croupier::SendPauseTimer(bool pause) -> void {
	State::current.client.send(eClientMessage::PauseTimer, {pause ? "1" : "0"});
}

auto Croupier::SendSplitTimer() -> void {
	State::current.client.send(eClientMessage::SplitTimer);
}

auto Croupier::SendSpinData() -> void {
	std::string data;
	std::string prefixes;
	for (const auto& cond : State::current.spin.getConditions()) {
		if (!data.empty()) data += ", "sv;
		if (cond.killComplication == eKillComplication::Live)
			prefixes += "(Live) "sv;

		switch (cond.killType) {
			case eKillType::Silenced:
				prefixes += "Sil "sv;
				break;
			case eKillType::Loud:
				prefixes += "Ld "sv;
				break;
			case eKillType::LoudRemote:
				prefixes += "Ld Remote "sv;
				break;
			case eKillType::Remote:
				prefixes += "Remote "sv;
				break;
			case eKillType::Impact:
				prefixes += "Impact "sv;
				break;
			case eKillType::Melee:
				prefixes += "Melee "sv;
				break;
			case eKillType::Thrown:
				prefixes += "Thrown "sv;
				break;
		}

		data += std::format("{}: {}{} / {}"sv, Keyword::getForTarget(cond.target.get().getName()), prefixes, cond.killMethod.name, cond.disguise.get().name);
		prefixes.clear();
	}
	State::current.client.send(eClientMessage::SpinData, { data });
}

auto Croupier::SendNext() -> void {
	State::current.client.send(eClientMessage::Next);
}

auto Croupier::SendPrev() -> void {
	State::current.client.send(eClientMessage::Prev);
}

auto Croupier::SendRandom() -> void {
	State::current.client.send(eClientMessage::Random);
}

auto Croupier::SendMissionFailed() -> void {
	State::current.client.send(eClientMessage::MissionFailed);
}

auto Croupier::SendMissionComplete() -> void {
	State::current.client.send(eClientMessage::MissionComplete, {State::current.isSA ? "1" : "0", std::to_string(State::current.exitIGT)});
}

auto Croupier::SendMissionOutroBegin() -> void {
	State::current.client.send(eClientMessage::MissionOutroBegin);
}

auto Croupier::SendMissionStart(const std::string& locationId, const std::string& entranceId, const std::vector<LoadoutItemEventValue>& loadout) -> void {
	std::string loadoutStr = "";
	for (const auto& item : loadout) {
		if (!loadoutStr.empty()) loadoutStr += ",";
		loadoutStr += std::format("\"{}\"", item.RepositoryId);
	}
	State::current.client.send(eClientMessage::MissionStart, {entranceId, "[" + loadoutStr + "]"});
}

auto Croupier::SendKillValidationUpdate() -> void {
	auto data = ""s;
	for (const auto& cond : State::current.spin.getConditions()) {
		auto kc = State::current.getTargetKillValidation(cond.target.get().getID());
		if (!data.empty()) data += ",";

		data += std::format(
			"{}:{}:{}:{}",
			Keyword::getForTarget(cond.target.get().getName()),
			static_cast<int>(kc.correctMethod),
			kc.correctDisguise ? 1 : 0,
			Keyword::getForTarget(kc.specificTarget)
		);
	}
	State::current.client.send(eClientMessage::KillValidation, { data });
}
