#pragma once
#include <filesystem>
#include <stack>
#include <unordered_map>
#include <IPluginInterface.h>
#include <Glacier/Enums.h>
#include <Glacier/SGameUpdateEvent.h>
#include <Glacier/SOnlineEvent.h>
#include <Glacier/ZGameUIManager.h>
#include <Glacier/ZObject.h>
#include <Glacier/ZString.h>
#include "CroupierClient.h"
#include "EventSystem.h"
#include "Roulette.h"

struct RouletteSpinKill {
	std::string targetName;
	bool validMethod = true;
	bool validDisguise = true;

	RouletteSpinKill(std::string targetName) : targetName(targetName)
	{ }
};

struct SharedRouletteSpin {
	const RouletteSpin& spin;
	std::vector<RouletteSpinKill> kills;
	std::chrono::steady_clock::time_point timeStarted;
	std::chrono::seconds timeElapsed = std::chrono::seconds(0);
	bool isPlaying = false;
	bool isFinished = false;
	LONG windowX = 0;
	LONG windowY = 0;

	SharedRouletteSpin(const RouletteSpin& spin) : spin(spin), timeElapsed(0) {
		timeStarted = std::chrono::steady_clock().now();
	}

	auto getTimeElapsed() const -> std::chrono::seconds {
		if (!this->isFinished && this->isPlaying) {
			return std::chrono::duration_cast<std::chrono::seconds>(std::chrono::steady_clock().now() - timeStarted);
		}
		return this->isFinished ? this->timeElapsed : std::chrono::seconds::zero();
	}

	auto playerSelectMission() {
		this->isPlaying = false;
	}

	auto playerStart() {
		this->kills.clear();
		if (!this->isPlaying)
			this->timeStarted = std::chrono::steady_clock().now();
		this->isPlaying = true;
		this->isFinished = false;
	}

	auto playerExit() {
		this->timeElapsed = this->getTimeElapsed();
		this->isPlaying = false;
		this->isFinished = true;
	}
};

struct SerializedSpin {
	struct Condition {
		std::string targetName;
		std::string killMethod;
		std::string disguise;
	};

	std::vector<Condition> conditions;
};

struct Configuration {
	bool spinOverlay = false;
	bool timer = false;
	RouletteRuleset customRules;
	eRouletteRuleset ruleset = eRouletteRuleset::Default;
	std::vector<eMission> missionPool;

	std::vector<SerializedSpin> spinHistory;
};

class Croupier : public IPluginInterface {
public:
	Croupier();
	~Croupier() override;
	auto OnEngineInitialized() -> void override;
	auto OnDrawMenu() -> void override;
	auto OnDrawUI(bool p_HasFocus) -> void override;
	auto OnFrameUpdate(const SGameUpdateEvent&) -> void;
	auto OnMissionSelect(eMission, bool isAuto = true) -> void;
	auto OnRulesetSelect(eRouletteRuleset) -> void;
	auto OnRulesetCustomised() -> void;
	auto SaveSpinHistory() -> void;
	auto OnFinishMission() -> void;
	auto DrawEditSpinUI(bool focused) -> void;
	auto DrawCustomRulesetUI(bool focused) -> void;
	auto DrawEditMissionPoolUI(bool focused) -> void;
	auto DrawSpinUI(bool focused) -> void;
	auto Random() -> void;
	auto Respin(bool isAuto = true) -> void;
	auto PreviousSpin() -> void;
	auto LoadConfiguration() -> void;
	auto SaveConfiguration() -> void;
	auto SetDefaultMissionPool() -> void;
	auto SendAutoSpin(eMission = eMission::NONE) -> void;
	auto SendRespin(eMission = eMission::NONE) -> void;
	auto SendSpinData() -> void;
	auto SendNext() -> void;
	auto SendPrev() -> void;
	auto SendRandom() -> void;
	auto SendMissions() -> void;
	auto SendToggleSpinLock() -> void;

private:
	static std::unordered_map<std::string, eMission> MissionContractIds;

	static auto getMissionFromContractId(const std::string&) -> eMission;

	auto LogSpin() -> void;
	auto SetupEvents() -> void;
	auto SetupMissions() -> void;
	auto GetMission(eMission mission) -> const RouletteMission*;
	auto ProcessMissionsMessage(const ClientMessage& message) -> void;
	auto ProcessSpinDataMessage(const ClientMessage& message) -> void;
	auto ParseSpin(std::string_view str) -> std::optional<RouletteSpin>;

	DECLARE_PLUGIN_DETOUR(Croupier, void, OnEventReceived, ZAchievementManagerSimple* th, const SOnlineEvent& event);
	DECLARE_PLUGIN_DETOUR(Croupier, void, OnEventSent, ZAchievementManagerSimple* th, uint32_t eventIndex, const ZDynamicObject& event);
	DECLARE_PLUGIN_DETOUR(Croupier, void, OnWinHttpCallback, void* dwContext, void* hInternet, void* param_3, int dwInternetStatus, void* param_5, int param_6);

private:
	std::unique_ptr<CroupierClient> client;
	std::vector<RouletteMission> missions;
	RouletteSpinGenerator generator;
	RouletteRuleset rules;
	RouletteSpin spin;
	SharedRouletteSpin sharedSpin;
	std::stack<RouletteSpin> spinHistory;
	eMission currentMission = eMission::NONE;
	eRouletteRuleset ruleset = eRouletteRuleset::RRWC2023;
	EventSystem events;
	std::fstream file;
	std::filesystem::path modulePath;
	int uiMissionSelectIndex = 0;
	bool currentSpinSaved = true;
	bool showUI = false;
	bool showManualModeUI = false;
	bool showEditMissionPoolUI = false;
	bool showCustomRulesetUI = false;
	bool spinCompleted = false;
	bool spinLocked = false;
	Configuration config;
};

DEFINE_ZHM_PLUGIN(Croupier)
