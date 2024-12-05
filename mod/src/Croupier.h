#pragma once
#include "CroupierClient.h"
#include "Events.h"
#include "EventSystem.h"
#include "InputUtil.h"
#include "KillConfirmation.h"
#include "Roulette.h"
#include <IPluginInterface.h>
#include <Glacier/Enums.h>
#include <Glacier/SGameUpdateEvent.h>
#include <Glacier/SOnlineEvent.h>
#include <Glacier/ZGameUIManager.h>
#include <Glacier/ZInput.h>
#include <Glacier/ZObject.h>
#include <Glacier/ZString.h>
#include <filesystem>
#include <stack>
#include <unordered_map>

enum class DockMode {
	None,
	TopLeft,
	TopRight,
	BottomLeft,
	BottomRight,
};

struct KeyBindAssign {
	KeyBind key1;
	KeyBind key2;
	bool assigning1 = false;
	bool assigning2 = false;
};

struct KillSetpieceEvent {
	std::string id;
	std::string name;
	std::string type;
	double timestamp;
};

struct SharedRouletteSpin {
	const RouletteSpin& spin;
	std::set<std::string, InsensitiveCompareLexicographic> killed;
	std::set<std::string, InsensitiveCompareLexicographic> spottedNotKilled;
	std::vector<DisguiseChange> disguiseChanges;
	std::vector<KillConfirmation> killValidations;
	std::vector<KillSetpieceEvent> killSetpieceEvents;
	std::vector<LoadoutItemEventValue> loadout;
	std::string locationId;
	std::chrono::steady_clock::time_point timeStarted;
	std::chrono::seconds timeElapsed = std::chrono::seconds(0);
	double startIGT = 0;
	double exitIGT = 0;
	bool isSA = true;
	bool isCaughtOnCams = false;
	bool isCamsDestroyed = false;
	bool isPlaying = false;
	bool isFinished = false;
	bool hasLoadedGame = false;		// current play session is from a loaded game
	LONG windowX = 0;
	LONG windowY = 0;

	SharedRouletteSpin(const RouletteSpin& spin) : spin(spin), timeElapsed(0) {
		timeStarted = std::chrono::steady_clock().now();
		this->resetKillValidations();
	}

	auto getTargetKillValidation(eTargetID target) const -> KillConfirmation {
		if (hasLoadedGame) return KillConfirmation(target, eKillValidationType::Unknown);
		for (auto& kc : killValidations) {
			if (kc.target == target)
				return kc;
		}
		return KillConfirmation(target, eKillValidationType::Incomplete);
	}

	auto getKillConfirmation(size_t idx) -> KillConfirmation& {
		if (killValidations.size() < spin.getConditions().size())
			this->resetKillValidations();
		if (idx > killValidations.size()) throw std::exception("Invalid kill confirmation index.");
		return killValidations[idx];
	}

	auto getLastDisguiseChangeAtTimestamp(float timestamp) const -> const DisguiseChange* {
		for (auto i = disguiseChanges.size(); i > 0; --i) {
			if (disguiseChanges[i - 1].timestamp < timestamp)
				return &disguiseChanges[i - 1];
		}
		return nullptr;
	}

	auto getSetpieceEventAtTimestamp(double timestamp) const -> const KillSetpieceEvent* {
		for (auto i = killSetpieceEvents.size(); i > 0; --i) {
			if (std::abs(killSetpieceEvents[i - 1].timestamp - timestamp) < 0.1)
				return &killSetpieceEvents[i - 1];
		}
		return nullptr;
	}

	auto getLastDisguiseChange() const -> const DisguiseChange* {
		return disguiseChanges.empty() ? nullptr : &disguiseChanges.back();
	}

	auto getTimeElapsed() const -> std::chrono::seconds {
		if (!this->isFinished && this->isPlaying) {
			return std::chrono::duration_cast<std::chrono::seconds>(std::chrono::steady_clock().now() - timeStarted);
		}
		return this->isFinished ? this->timeElapsed : std::chrono::seconds::zero();
	}

	auto voidSA() {
		if (this->isFinished) return;
		this->isSA = false;
	}

	auto playerSelectMission() {
		this->isPlaying = false;
		this->isFinished = false;
	}

	auto playerStart() {
		this->resetKillValidations();

		if (!this->isPlaying) {
			this->isPlaying = true;
			this->timeStarted = std::chrono::steady_clock().now();
			this->isFinished = false;
		}

		this->killed.clear();
		this->spottedNotKilled.clear();
		this->exitIGT = 0;
		this->isSA = true;
		this->isCaughtOnCams = false;
		this->isCamsDestroyed = false;
		this->hasLoadedGame = false;
	}

	auto playerCutsceneEnd(double igt) {
		this->startIGT = igt;
		this->isPlaying = true;
	}

	auto playerLoad() {
		this->isPlaying = true;
		this->hasLoadedGame = true;
		this->resetKillValidations();
	}

	auto playerExit(double timestamp = 0) {
		this->timeElapsed = this->getTimeElapsed();
		this->isPlaying = false;
		this->isFinished = true;
		if (this->spottedNotKilled.size() > 0)
			this->isSA = false;
		this->exitIGT = timestamp - this->startIGT;
		this->isSA = this->isSA && !this->isCaughtOnCams && !this->hasLoadedGame;
		this->killed.clear();
		this->spottedNotKilled.clear();
	}

	auto resetKillValidations() -> void {
		killValidations.resize(spin.getConditions().size());
		disguiseChanges.clear();
		killSetpieceEvents.clear();

		for (auto& kv : killValidations)
			kv = KillConfirmation {};
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
	bool timer = false;
	bool streak = false;
	int streakCurrent = 0;
	bool spinOverlay = false;
	bool overlayKillConfirmations = true;
	DockMode overlayDockMode = DockMode::None;
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
	auto DrawEditHotkeysUI(bool focused) -> void;
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
	auto SendMissionStart(const std::string& locationId, const std::string& entranceId, const std::vector<LoadoutItemEventValue>& loadout) -> void;
	auto SendMissionFailed() -> void;
	auto SendMissionComplete() -> void;
	auto SendMissionOutroBegin() -> void;
	auto SendKillValidationUpdate() -> void;
	auto SendResetStreak() -> void;
	auto SendResetTimer() -> void;
	auto SendStartTimer() -> void;
	auto SendToggleTimer(bool enable) -> void;
	auto SendPauseTimer(bool pause) -> void;
	auto SendLoadStarted() -> void;
	auto SendLoadFinished() -> void;

	auto InstallHooks() -> void;
	auto UninstallHooks() -> void;
	auto ProcessSpinState() -> void;
	auto ProcessClientMessages() -> void;
	auto ValidateKillMethod(eTargetID target, const ServerEvent<Events::Kill>& ev, eKillMethod method, eKillType type) -> eKillValidationType;
	auto ValidateKillMethod(eTargetID target, const ServerEvent<Events::Kill>& ev, eMapKillMethod method, eKillType type) -> eKillValidationType;

private:
	static std::unordered_map<std::string, eMission> MissionContractIds;

	auto LogSpin() -> void;
	auto SetupEvents() -> void;
	auto ProcessHotkeys() -> void;
	auto ProcessMissionsMessage(const ClientMessage& message) -> void;
	auto ProcessSpinDataMessage(const ClientMessage& message) -> void;
	auto ProcessLoadRemoval() -> void;
	auto ParseSpin(std::string_view str) -> std::optional<RouletteSpin>;

	DECLARE_PLUGIN_DETOUR(Croupier, void*, OnZLevelManagerStateCondition, void* th, __int64 a2);
	DECLARE_PLUGIN_DETOUR(Croupier, void*, OnLoadingScreenActivated, void* th, void* a1);
	DECLARE_PLUGIN_DETOUR(Croupier, void, OnEventReceived, ZAchievementManagerSimple* th, const SOnlineEvent& event);
	DECLARE_PLUGIN_DETOUR(Croupier, void, OnEventSent, ZAchievementManagerSimple* th, uint32_t eventIndex, const ZDynamicObject& event);
	DECLARE_PLUGIN_DETOUR(Croupier, void, OnWinHttpCallback, void* dwContext, void* hInternet, void* param_3, int dwInternetStatus, void* param_5, int param_6);

private:
	std::unique_ptr<CroupierClient> client;
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
	ImVec2 overlaySize = {};
	int uiMissionSelectIndex = 0;
	bool currentSpinSaved = true;
	bool showUI = false;
	bool showManualModeUI = false;
	bool showEditMissionPoolUI = false;
	bool showCustomRulesetUI = false;
	bool showEditHotkeysUI = false;
	bool spinCompleted = false;
	bool spinLocked = false;
	bool appTimerEnable = false;
	bool hooksInstalled = false;
	bool loadRemovalActive = false;
	bool isLoadingScreenCheckHasBeenTrue = false;
	bool loadingScreenActivated = false;
	bool respinKeybindWasPressed = false;
	bool shuffleKeybindWasPressed = false;
	KeyBindAssign respinKeyBind;
	KeyBindAssign shuffleKeyBind;
	ZInputAction respinAction;
	ZInputAction shuffleAction;
	Configuration config;
};

DEFINE_ZHM_PLUGIN(Croupier)
