#pragma once
#include "Config.h"
#include "CroupierClient.h"
#include "Events.h"
#include "EventSystem.h"
#include "json.hpp"
#include "KillConfirmation.h"
#include "Roulette.h"
#include "State.h"
#include "UI.h"
#include <Hooks.h>
#include <IPluginInterface.h>
#include <Glacier/Enums.h>
#include <Glacier/SGameUpdateEvent.h>
#include <Glacier/SOnlineEvent.h>
#include <Glacier/ZEntity.h>
#include <Glacier/ZGameUIManager.h>
#include <Glacier/ZInput.h>
#include <Glacier/ZObject.h>
#include <Glacier/ZOutfit.h>
#include <Glacier/ZResource.h>
#include <Glacier/ZString.h>
#include <Logging.h>
#include <filesystem>
#include <stack>
#include <unordered_map>

namespace Croupier {
	class CroupierPlugin : public IPluginInterface {
	public:
		CroupierPlugin();
		~CroupierPlugin() override;
		auto OnEngineInitialized() -> void override;
		auto OnDrawMenu() -> void override;
		auto OnDrawUI(bool p_HasFocus) -> void override;
		auto OnFrameUpdate(const SGameUpdateEvent&) -> void;
		auto OnFrameUpdate_PlayMode(const SGameUpdateEvent&) -> void;
		auto SaveSpinHistory() -> void;
		auto OnFinishMission() -> void;
		auto Random() -> void;
		auto Respin(bool isAuto = true) -> void;
		auto PreviousSpin() -> void;
		auto SetDefaultMissionPool() -> void;
		auto GetOutfitByRepoId(std::string_view repoId) -> const ZGlobalOutfitKit*;
		auto GetOutfitByRepoId(ZRepositoryID repoId) -> const ZGlobalOutfitKit*;
		auto SendCustomEvent(std::string_view name, nlohmann::json eventValue) -> void;
		auto ImbueItemEvent(const ItemEventValue& ev, EActionType actionType) -> std::optional<nlohmann::json>;
		auto ImbuePacifyEvent(const PacifyEventValue& ev) -> std::optional<nlohmann::json>;
		auto ImbueDisguiseEvent(const std::string& repoId) -> nlohmann::json;
		auto ImbuePlayerLocation(nlohmann::json& json, bool asHero = false) const -> void;
		auto ImbueItemInfo(ZEntityRef entity, nlohmann::json& json) -> void;
		auto ImbueActorInfo(TEntityRef<ZActor> actor, nlohmann::json& json, bool asActor = true) const -> void;
		auto ImbuedPlayerLocation(nlohmann::json&& json = {}, bool asHero = false) const -> nlohmann::json;
		auto ImbuedItemInfo(ZEntityRef entity, nlohmann::json&& json = {}) -> nlohmann::json;
		auto ImbuedActorInfo(TEntityRef<ZActor> actor, nlohmann::json&& json = {}, bool asActor = true) const -> nlohmann::json;

		auto InstallHooks() -> void;
		auto UninstallHooks() -> void;
		auto ProcessSpinState() -> void;
		auto ProcessPlayerState() -> void;
		auto ProcessClientMessages() -> void;
		auto ProcessClientEvent(std::string_view name, const nlohmann::json& json) -> void;
		auto ValidateKillMethod(eTargetID target, const ServerEvent<Events::Kill>& ev, eKillMethod method, eKillType type) -> eKillValidationType;
		auto ValidateKillMethod(eTargetID target, const ServerEvent<Events::Kill>& ev, eMapKillMethod method, eKillType type) -> eKillValidationType;

	private:
		static std::unordered_map<std::string, eMission> MissionContractIds;

		auto SetupEvents() -> void;
		auto ProcessMissionsMessage(const ClientMessage& message) -> void;
		auto ProcessSpinDataMessage(const ClientMessage& message) -> void;
		auto ProcessLoadRemoval() -> void;

		DECLARE_PLUGIN_DETOUR(CroupierPlugin, void*, OnZLevelManagerStateCondition, void* th, __int64 a2);
		DECLARE_PLUGIN_DETOUR(CroupierPlugin, void*, OnLoadingScreenActivated, void* th, void* a1);
		DECLARE_PLUGIN_DETOUR(CroupierPlugin, void, OnEventReceived, ZAchievementManagerSimple* th, const SOnlineEvent& event);
		DECLARE_PLUGIN_DETOUR(CroupierPlugin, void, OnEventSent, ZAchievementManagerSimple* th, uint32_t eventIndex, const ZDynamicObject& event);
		DECLARE_PLUGIN_DETOUR(CroupierPlugin, void, OnWinHttpCallback, void* dwContext, void* hInternet, void* param_3, int dwInternetStatus, void* param_5, int param_6);
		DECLARE_PLUGIN_DETOUR(CroupierPlugin, bool, OnPinOutput, ZEntityRef entity, uint32_t pinId, const ZObjectRef& data);

	private:
		GameplayData gameplay;
		eMission currentMission = eMission::NONE;
		EventSystem events;
		std::filesystem::path modulePath;
		int uiMissionSelectIndex = 0;
		bool currentSpinSaved = true;
		bool appTimerEnable = false;
		bool hooksInstalled = false;
		bool loadRemovalActive = false;
		bool isLoadingScreenCheckHasBeenTrue = false;
		bool loadingScreenActivated = false;
		bool respinKeybindWasPressed = false;
		bool shuffleKeybindWasPressed = false;
		TResourcePtr<ZTemplateEntityFactory> repositoryResource;
		ZInputAction respinAction;
		ZInputAction shuffleAction;
		Configuration config;
		Croupier::UI ui;
	};

	DEFINE_ZHM_PLUGIN(CroupierPlugin)
}
