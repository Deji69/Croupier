#pragma once
#include "Config.h"
#include "CroupierClient.h"
#include "Enums.h"
#include "Events.h"
#include "EventSystem.h"
#include "json.hpp"
#include "KillConfirmation.h"
#include "KillMethod.h"
#include "RouletteMission.h"
#include "State.h"
#include "Target.h"
#include "UI.h"
#include <cstdint>
#include <filesystem>
#include <fmt/base.h>
#include <Glacier/EntityFactory.h>
#include <Glacier/Enums.h>
#include <Glacier/Pins.h>
#include <Glacier/SGameUpdateEvent.h>
#include <Glacier/ZActor.h>
#include <Glacier/ZEntity.h>
#include <Glacier/ZInput.h>
#include <Glacier/ZMath.h>
#include <Glacier/ZObject.h>
#include <Glacier/ZOutfit.h>
#include <Glacier/ZPrimitives.h>
#include <Glacier/ZResource.h>
#include <Glacier/ZScene.h>
#include <Hooks.h>
#include <IPluginInterface.h>
#include <Logging.h>
#include <NavPower.h>
#include <optional>
#include <spdlog/common.h>
#include <string>
#include <string_view>
#include <unordered_map>
#include <vector>
#include <chrono>
#include <utility>
#include <memory>
#include <functional>

class PinListeners
{
protected:
	auto call(ZEntityRef entity, const ZObjectRef& data, ZHMPin pin) const -> bool {
		for (auto& handler : this->handlers)
			handler(entity, data, pin);

		return this->handlers.size() > 0;
	}

public:
	using HandlerFunc = void(ZEntityRef entity, const ZObjectRef& data, ZHMPin pin);

	PinListeners()
	{ }

	template<typename TFunc>
	auto add(TFunc&& func) -> void {
		this->handlers.emplace_back(std::forward<TFunc>(func));
	}

	auto operator()(ZEntityRef entity, const ZObjectRef& data, ZHMPin pin) const -> bool {
		return this->call(entity, data, pin);
	}

	auto handle(ZEntityRef entity, const ZObjectRef& data, ZHMPin pin) const -> bool {
		return this->call(entity, data, pin);
	}

private:
	std::vector<std::function<HandlerFunc>> handlers;
};

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
		auto GetOutfitByRepoId(std::string_view repoId) const -> const ZGlobalOutfitKit*;
		auto GetOutfitByRepoId(ZRepositoryID repoId) const -> const ZGlobalOutfitKit*;
		//auto GetItemContainer(ZEntityRef item) -> TEntityRef<IItemContainer>;
		auto SendCustomEvent(std::string_view name, nlohmann::json eventValue) const -> void;
		auto ImbueItemEvent(const ItemEventValue& ev, EActionType actionType) const -> std::optional<nlohmann::json>;
		auto ImbuePacifyEvent(const PacifyEventValue& ev) const -> std::optional<nlohmann::json>;
		auto ImbueDisguiseEvent(const std::string& repoId) -> nlohmann::json;
		auto ImbuePlayerLocation(nlohmann::json& json, bool asHero = false) const -> void;
		auto ImbuePlayerInfo(nlohmann::json& json, bool asHero = false) const -> void;
		auto ImbueItemInfo(ZEntityRef entity, nlohmann::json& json, std::string prefix = "Item") -> void;
		auto ImbueActorInfo(TEntityRef<ZActor> actor, nlohmann::json& json, bool asActor = true) const -> void;
		auto ImbueActorInfo(ZRepositoryID repoId, nlohmann::json& json, bool asActor = true) const -> void;
		auto ImbueItemRepositoryInfo(nlohmann::json& json, ZRepositoryID repoId) -> void;
		auto ImbuePositionInfo(nlohmann::json& json, SVector3 vec, std::string prefix = "") -> void;
		auto ImbueSetpieceInfo(ZEntityRef entity, nlohmann::json& j) -> bool;
		auto ImbueSetpieceActivatorInfo(ZEntityRef entity, nlohmann::json& j) -> bool;
		auto ImbuedPlayerLocation(nlohmann::json&& json = {}, bool asHero = false) const -> nlohmann::json;
		auto ImbuedPlayerInfo(nlohmann::json&& json = {}, bool asHero = false) const -> nlohmann::json;
		auto ImbuedItemInfo(ZEntityRef entity, nlohmann::json&& json = {}, std::string prefix = "Item") -> nlohmann::json;
		auto ImbuedActorInfo(TEntityRef<ZActor> actor, nlohmann::json&& json = {}, bool asActor = true) const -> nlohmann::json;
		auto ImbuedActorInfo(ZRepositoryID repoId, nlohmann::json&& json = {}, bool asActor = true) const -> nlohmann::json;
		auto ImbuedPositionInfo(SVector3 vec, std::string prefix = "", nlohmann::json&& json = {}) -> nlohmann::json;
		auto ImbuedSetepieceInfo(ZEntityRef, nlohmann::json&& j = {}) -> nlohmann::json;
		auto ImbuedSetpieceActivatorInfo(ZEntityRef entity, nlohmann::json&& j) -> nlohmann::json;

		auto InstallHooks() -> void;
		auto UninstallHooks() -> void;
		auto ProcessSpinState() -> void;
		auto ProcessPlayerState() const -> void;
		auto ProcessClientMessages() -> void;
		auto ProcessClientEvent(std::string_view name, const nlohmann::json& json) -> void;
		auto ValidateKillMethod(eTargetID target, const ServerEvent<Events::Kill>& ev, eKillMethod method, eKillType type) -> eKillValidationType;
		auto ValidateKillMethod(eTargetID target, const ServerEvent<Events::Kill>& ev, eMapKillMethod method, eKillType type) -> eKillValidationType;

	private:
		static std::unordered_map<std::string, eMission> MissionContractIds;

		auto ImbueActorInfoWithRepoID(ZRepositoryID repoId, nlohmann::json& json, bool asActor = true, bool repoDataOnly = false) const -> void;
		auto ImbueActorInfoWithReference(TEntityRef<ZActor> actor, nlohmann::json& json, bool asActor = false, bool referenceDataOnly = false) const -> void;

		auto SetupEvents() -> void;
		auto SetupPins() -> void;
		auto ProcessMissionsMessage(const ClientMessage& message) -> void;
		auto ProcessSpinDataMessage(const ClientMessage& message) -> void;
		auto ProcessBingoDataMessage(const ClientMessage& message) -> void;
		auto ProcessGameModeMessage(const ClientMessage& message) -> void;
		auto ProcessLoadRemoval() -> void;
		auto AddPinListener(ZHMPin pinId, std::function<PinListeners::HandlerFunc> func) -> void;
		auto GetPinListeners(ZHMPin pinId) -> PinListeners*;
		auto GetOrMakePinListeners(ZHMPin pinId) -> PinListeners&;

		template<typename... Args>
		inline void LogDebug(spdlog::format_string_t<Args...> p_Format, const Args&... p_Args) const {
#ifndef _DEBUG
			if (!config.debug) return;
#endif

			const auto s_Loggers = GetLoggers();

			for (size_t i = 0; i < s_Loggers.Count; ++i)
				s_Loggers.Loggers[i]->info(fmt::runtime(p_Format), p_Args...);
		}

		DECLARE_PLUGIN_DETOUR(CroupierPlugin, void*, OnZLevelManagerStateCondition, void* th, __int64 a2);
		DECLARE_PLUGIN_DETOUR(CroupierPlugin, void*, OnLoadingScreenActivated, void* th, void* a1);
		DECLARE_PLUGIN_DETOUR(CroupierPlugin, void, OnEventReceived, ZAchievementManagerSimple* th, const SOnlineEvent& event);
		DECLARE_PLUGIN_DETOUR(CroupierPlugin, void, OnEventSent, ZAchievementManagerSimple* th, uint32_t eventIndex, const ZDynamicObject& event);
		DECLARE_PLUGIN_DETOUR(CroupierPlugin, void, OnWinHttpCallback, void* dwContext, void* hInternet, void* param_3, int dwInternetStatus, void* param_5, int param_6);
		DECLARE_PLUGIN_DETOUR(CroupierPlugin, bool, OnPinOutput, ZEntityRef entity, uint32_t pinId, const ZObjectRef& data);
		DECLARE_PLUGIN_DETOUR(CroupierPlugin, void, OnClearScene, ZEntitySceneContext* th, bool p_FullyUnloadScene);

	private:
		GameplayData gameplay;
		eMission currentMission = eMission::NONE;
		EventSystem events;
		std::unordered_map<ZHMPin, std::unique_ptr<PinListeners>> pinListeners;
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
		Config config;
		Croupier::UI ui;
		NavPower::NavMesh m_NavMesh;
		std::vector<std::pair<ZEntityRef, int>> entitiesPutInContainer;
		std::chrono::system_clock::time_point lastContainersCheckTime;
	};

	DEFINE_ZHM_PLUGIN(CroupierPlugin)
}
