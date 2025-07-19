#pragma once
#include "Events.h"
#include "Roulette.h"
#include <string>
#include <vector>

namespace Croupier
{
	extern auto SendAutoSpin(eMission = eMission::NONE) -> void;
	extern auto SendRespin(eMission = eMission::NONE) -> void;
	extern auto SendSpinData() -> void;
	extern auto SendNext() -> void;
	extern auto SendPrev() -> void;
	extern auto SendRandom() -> void;
	extern auto SendMissions() -> void;
	extern auto SendToggleSpinLock() -> void;
	extern auto SendMissionStart(const std::string& locationId, const std::string& entranceId, const std::vector<LoadoutItemEventValue>& loadout) -> void;
	extern auto SendMissionFailed() -> void;
	extern auto SendMissionComplete() -> void;
	extern auto SendMissionOutroBegin() -> void;
	extern auto SendKillValidationUpdate() -> void;
	extern auto SendResetStreak() -> void;
	extern auto SendResetTimer() -> void;
	extern auto SendStartTimer() -> void;
	extern auto SendToggleTimer(bool enable) -> void;
	extern auto SendPauseTimer(bool pause) -> void;
	extern auto SendSplitTimer() -> void;
	extern auto SendLoadStarted() -> void;
	extern auto SendLoadFinished() -> void;
}
