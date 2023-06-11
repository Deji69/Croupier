#pragma once
#include <stack>
#include <unordered_map>
#include <IPluginInterface.h>
#include <Glacier/Enums.h>
#include <Glacier/SGameUpdateEvent.h>
#include <Glacier/SOnlineEvent.h>
#include <Glacier/ZGameUIManager.h>
#include <Glacier/ZObject.h>
#include <Glacier/ZString.h>
#include "CroupierWindow.h"
#include "EventSystem.h"
#include "Roulette.h"

class Croupier : public IPluginInterface {
public:
	Croupier();
	~Croupier() override;
	auto OnEngineInitialized() -> void override;
	auto OnDrawMenu() -> void override;
	auto OnDrawUI(bool p_HasFocus) -> void override;
	auto OnMissionSelect(eMission) -> void;
	auto DrawEditSpinUI(bool focused) -> void;
	auto DrawSpinUI(bool focused) -> void;
	auto Respin() -> void;

private:
	static std::unordered_map<std::string, eMission> MissionContractIds;

	static auto getMissionFromContractId(const std::string&) -> eMission;

	auto LogSpin() -> void;
	auto SetupEvents() -> void;
	auto SetupMissions() -> void;
	auto GetMission(eMission mission) -> const RouletteMission*;

	DECLARE_PLUGIN_DETOUR(Croupier, void, OnEventReceived, ZAchievementManagerSimple* th, const SOnlineEvent& event);
	DECLARE_PLUGIN_DETOUR(Croupier, void, OnEventSent, ZAchievementManagerSimple* th, uint32_t eventIndex, const ZDynamicObject& event);
	DECLARE_PLUGIN_DETOUR(Croupier, void, OnWinHttpCallback, void* dwContext, void* hInternet, void* param_3, int dwInternetStatus, void* param_5, int param_6);

private:
	CroupierWindow window;
	std::vector<RouletteMission> missions;
	RouletteSpinGenerator generator;
	RouletteSpin spin;
	SharedRouletteSpin sharedSpin;
	std::stack<RouletteSpin> spinHistory;
	eMission currentMission = eMission::NONE;
	EventSystem events;
	int uiMissionSelectIndex = 0;
	bool showUI = false;
	bool showManualModeUI = false;
	bool externalWindowEnabled = true;
	bool inGameWindowEnabled = false;
	bool externalWindowOnTop = true;
	bool externalWindowTextOnly = false;
	bool spinCompleted = false;
};

DEFINE_ZHM_PLUGIN(Croupier)
