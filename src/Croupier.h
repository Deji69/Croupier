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
#include "CroupierWindow.h"
#include "EventSystem.h"
#include "Roulette.h"

struct SerializedSpin {
	struct Condition {
		std::string targetName;
		std::string killMethod;
		std::string disguise;
	};

	std::vector<Condition> conditions;
};

struct Configuration {
	bool externalWindow = true;
	bool externalWindowOnTop = true;
	bool externalWindowTextOnly = false;
	bool spinOverlay = false;
	bool timer = false;
	RouletteRuleset customRules;
	eRouletteRuleset ruleset = eRouletteRuleset::Default;
	std::optional<LONG> windowPosX = std::nullopt;
	std::optional<LONG> windowPosY = std::nullopt;
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
	auto OnMissionSelect(eMission) -> void;
	auto OnRulesetSelect(eRouletteRuleset) -> void;
	auto OnRulesetCustomised() -> void;
	auto SaveSpinHistory() -> void;
	auto OnFinishMission() -> void;
	auto DrawEditSpinUI(bool focused) -> void;
	auto DrawCustomRulesetUI(bool focused) -> void;
	auto DrawEditMissionPoolUI(bool focused) -> void;
	auto DrawSpinUI(bool focused) -> void;
	auto Random() -> void;
	auto Respin() -> void;
	auto PreviousSpin() -> void;
	auto LoadConfiguration() -> void;
	auto SaveConfiguration() -> void;
	auto SetDefaultMissionPool() -> void;
	auto SendRespin(eMission = eMission::NONE) -> void;
	auto SendPrev() -> void;

private:
	static std::unordered_map<std::string, eMission> MissionContractIds;

	static auto getMissionFromContractId(const std::string&) -> eMission;

	auto LogSpin() -> void;
	auto SetupEvents() -> void;
	auto SetupMissions() -> void;
	auto GetMission(eMission mission) -> const RouletteMission*;
	auto ProcessSpinDataMessage(const ClientMessage& message) -> void;
	auto ParseSpin(std::string_view str) -> std::optional<RouletteSpin>;

	DECLARE_PLUGIN_DETOUR(Croupier, void, OnEventReceived, ZAchievementManagerSimple* th, const SOnlineEvent& event);
	DECLARE_PLUGIN_DETOUR(Croupier, void, OnEventSent, ZAchievementManagerSimple* th, uint32_t eventIndex, const ZDynamicObject& event);
	DECLARE_PLUGIN_DETOUR(Croupier, void, OnWinHttpCallback, void* dwContext, void* hInternet, void* param_3, int dwInternetStatus, void* param_5, int param_6);

private:
	std::unique_ptr<CroupierClient> client;
	CroupierWindow window;
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
	Configuration config;
};

DEFINE_ZHM_PLUGIN(Croupier)
