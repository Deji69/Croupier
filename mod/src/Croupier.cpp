#include "App.h"
#include "Bingo.h"
#include "Config.h"
#include "Croupier.h"
#include "CroupierClient.h"
#include "Disguise.h"
#include "Enums.h"
#include "Events.h"
#include "EventSystem.h"
#include "json.hpp"
#include "KillConfirmation.h"
#include "KillMethod.h"
#include "Roulette.h"
#include "RouletteMission.h"
#include "RouletteRuleset.h"
#include "SpinParser.h"
#include "State.h"
#include "Target.h"
#include "util.h"
#include "ZHMUtils.h"
#include <algorithm>
#include <charconv>
#include <chrono>
#include <Common.h>
#include <cstdarg>
#include <cstddef>
#include <cstdint>
#include <format>
#include <functional>
#include <Functions.h>
#include <Glacier/CompileReflection.h>
#include <Glacier/Enums.h>
#include <Glacier/EUpdateMode.h>
#include <Glacier/Pins.h>
#include <Glacier/SGameUpdateEvent.h>
#include <Glacier/TArray.h>
#include <Glacier/THashMap.h>
#include <Glacier/ZAction.h>
#include <Glacier/ZActor.h>
#include <Glacier/ZContentKitManager.h>
#include <Glacier/ZDelegate.h>
#include <Glacier/ZEntity.h>
#include <Glacier/ZGameLoopManager.h>
#include <Glacier/ZItem.h>
#include <Glacier/ZMath.h>
#include <Glacier/ZObject.h>
#include <Glacier/ZOutfit.h>
#include <Glacier/ZPrimitives.h>
#include <Glacier/ZRender.h>
#include <Glacier/ZResource.h>
#include <Glacier/ZScene.h>
#include <Glacier/ZSpatialEntity.h>
#include <Glacier/ZString.h>
#include <Globals.h>
#include <Hook.h>
#include <Hooks.h>
#include <IModSDK.h>
#include <iomanip>
#include <IPluginInterface.h>
#include <iterator>
#include <Logging.h>
#include <mutex>
#include <optional>
#include <random>
#include <set>
#include <sstream>
#include <stdexcept>
#include <string>
#include <string_view>
#include <system_error>
#include <utility>
#include <vector>
#include "FixMinMax.h"
#include <winhttp.h>
#include <Glacier/EntityFactory.h>
#include <memory>
#include <Glacier/ZRepository.h>

#pragma comment(lib, "Winhttp.lib")

using namespace std::string_literals;
using namespace std::string_view_literals;
using nlohmann::json;
using namespace Croupier;

class ZGlobalOutfitKit;

std::random_device rd;
std::mt19937 gen(rd());

template<typename T>
static auto randomVectorElement(const std::vector<T>& vec) -> const T&
{
	std::uniform_int_distribution<> dist(0, vec.size() - 1);
	return vec[dist(gen)];
}

CroupierPlugin::CroupierPlugin() : respinAction("Respin"), shuffleAction("Shuffle") {
	ZHMExtension::Init();
	State::current.rules = makeRouletteRuleset(State::current.ruleset);
	Commands::Respin = std::bind(&CroupierPlugin::Respin, this, std::placeholders::_1);
	Commands::Random = std::bind(&CroupierPlugin::Random, this);
	Commands::PreviousSpin = std::bind(&CroupierPlugin::PreviousSpin, this);
	Config::main.plugin = this;
}

CroupierPlugin::~CroupierPlugin() {
	State::current.client.stop();
	//Config::Save();
	this->UninstallHooks();
}

auto CroupierPlugin::OnEngineInitialized() -> void {
	Logger::Info("Croupier has been initialized!");

	this->SetupEvents();
	this->SetupPins();

	State::current.client.start();

	this->InstallHooks();
	Config::Load();

	if (Config::main.missionPool.empty())
		this->SetDefaultMissionPool();

	this->PreviousSpin();
}

auto CroupierPlugin::InstallHooks() -> void {
	if (this->hooksInstalled) return;

	const ZMemberDelegate<CroupierPlugin, void(const SGameUpdateEvent&)> frameUpdateDelegate(this, &CroupierPlugin::OnFrameUpdate);
	const ZMemberDelegate<CroupierPlugin, void(const SGameUpdateEvent&)> frameUpdateDelegatePlay(this, &CroupierPlugin::OnFrameUpdate_PlayMode);
	Globals::GameLoopManager->RegisterFrameUpdate(frameUpdateDelegate, 0, EUpdateMode::eUpdateAlways);
	Globals::GameLoopManager->RegisterFrameUpdate(frameUpdateDelegatePlay, 0, EUpdateMode::eUpdatePlayMode);

	Hooks::ZLoadingScreenVideo_ActivateLoadingScreen->AddDetour(this, &CroupierPlugin::OnLoadingScreenActivated);
	Hooks::ZAchievementManagerSimple_OnEventReceived->AddDetour(this, &CroupierPlugin::OnEventReceived);
	Hooks::ZAchievementManagerSimple_OnEventSent->AddDetour(this, &CroupierPlugin::OnEventSent);
	Hooks::Http_WinHttpCallback->AddDetour(this, &CroupierPlugin::OnWinHttpCallback);
	Hooks::SignalOutputPin->AddDetour(this, &CroupierPlugin::OnPinOutput);

	this->hooksInstalled = true;

	Logger::Info("Croupier: Hooks installed.");
}

auto CroupierPlugin::UninstallHooks() -> void {
	if (!this->hooksInstalled) return;

	const ZMemberDelegate<CroupierPlugin, void(const SGameUpdateEvent&)> frameUpdateDelegate(this, &CroupierPlugin::OnFrameUpdate);
	const ZMemberDelegate<CroupierPlugin, void(const SGameUpdateEvent&)> frameUpdateDelegatePlay(this, &CroupierPlugin::OnFrameUpdate_PlayMode);
	Globals::GameLoopManager->UnregisterFrameUpdate(frameUpdateDelegate, 0, EUpdateMode::eUpdateAlways);
	Globals::GameLoopManager->UnregisterFrameUpdate(frameUpdateDelegatePlay, 0, EUpdateMode::eUpdatePlayMode);

	Hooks::ZLoadingScreenVideo_ActivateLoadingScreen->RemoveDetour(&CroupierPlugin::OnLoadingScreenActivated);
	Hooks::ZAchievementManagerSimple_OnEventReceived->RemoveDetour(&CroupierPlugin::OnEventReceived);
	Hooks::ZAchievementManagerSimple_OnEventSent->RemoveDetour(&CroupierPlugin::OnEventSent);
	Hooks::Http_WinHttpCallback->RemoveDetour(&CroupierPlugin::OnWinHttpCallback);
	Hooks::SignalOutputPin->RemoveDetour(&CroupierPlugin::OnPinOutput);

	this->hooksInstalled = false;

	Logger::Info("Croupier: Hooks uninstalled.");
}

auto CroupierPlugin::OnFrameUpdate(const SGameUpdateEvent& ev) -> void {
	this->ProcessClientMessages();
	this->ProcessLoadRemoval();
}

auto CroupierPlugin::OnFrameUpdate_PlayMode(const SGameUpdateEvent& ev) -> void {
	this->ProcessSpinState();
	this->ProcessPlayerState();
}

auto CroupierPlugin::ProcessPlayerState() const -> void {
	// Process player state flags for de-duping events for spammed pins
	if (!State::current.playerInInstinctSinceFrame && State::current.playerInInstinct)
		State::current.playerInInstinct = false;
	State::current.playerInInstinctSinceFrame = false;
	if (!State::current.playerStartingAgilitySinceFrame && State::current.playerStartingAgility)
		State::current.playerStartingAgility = false;
	State::current.playerStartingAgilitySinceFrame = false;
	if (!State::current.playerShootingSinceFrame && State::current.playerShooting)
		State::current.playerShooting = false;
	State::current.playerShootingSinceFrame = false;

	auto player = SDK()->GetLocalPlayer();
	if (!player) return;
	if (State::current.gameMode == GameMode::Roulette) return;

	const auto spatial = player.m_entityRef.QueryInterface<ZSpatialEntity>();
	State::current.playerMatrix = spatial->m_mTransform;

	// Process area entry for bingo
	auto area = State::current.getArea(State::current.playerMatrix.Trans);
	if (area && area != State::current.area) {
		this->SendCustomEvent("EnterArea"sv, ImbuedPlayerInfo({
			{"Area", area->ID},
		}));
	}

	State::current.area = area;

	// Process room changes for bingo
	auto roomId = ZRoomManagerCreator::GetRoomID(spatial->GetObjectToWorldMatrix().Pos);
	if (roomId != State::current.roomId && roomId != -1) {
		State::current.roomId = roomId;
		this->SendCustomEvent("EnterRoom"sv, ImbuedPlayerInfo({
			{"Room", roomId},
		}));
	}

	// Process player on steps detection for bingo
	if (Globals::HM5GridManager) {
		auto& gridManager = *reinterpret_cast<CroupierZHM5GridManager*>(Globals::HM5GridManager);
		auto mask = reinterpret_cast<ZPFAreaRef&>(gridManager.m_HitmanPFLocation.m_area).GetRegionMask();
		State::current.playerOnStairs = (static_cast<int>(mask) & static_cast<int>(ERegionMask::eRM_Stairs)) != 0;
	}

	/*auto const now = std::chrono::system_clock::now();
	if ((std::chrono::duration<double>(now - lastContainersCheckTime).count() > .1)) {
		lastContainersCheckTime = now;
		for (size_t i = 0; i < entitiesPutInContainer.size(); ++i) {
			auto& entity = entitiesPutInContainer[i];

			auto item = entity.first.QueryInterface<ZHM5Item>();
			auto container = item->m_rItemContainer;

			if (++entity.second >= 20 || container) {
				if (container) {
					SendCustomEvent("OnPutInContainer"sv, ImbuedPlayerInfo(ImbuedItemInfo(container.m_ref, ImbuedItemInfo(entity.first), "ContainerItem"), true));
				}
				else {
					SendCustomEvent("OnPutInContainer"sv, ImbuedPlayerInfo(ImbuedItemInfo(entity.first), true));
				}
				entitiesPutInContainer.erase(entitiesPutInContainer.begin() + i);
				--i;
			}
		}
	}*/
}

auto CroupierPlugin::ProcessSpinState() -> void {
	if (State::current.spinCompleted) return;
	//if (State::current.hasLoadedGame) return;

	for (int i = 0; i < *Globals::NextActorId; ++i) {
		auto& actorData = State::current.actorData[i];

		auto const& actorRef = Globals::ActorManager->m_aActors[i];
		actorData.actor = &actorRef;

		auto repoEntity = actorRef.m_entityRef.QueryInterface<ZRepositoryItemEntity>();
		if (repoEntity != nullptr && (!actorData.repoId || *actorData.repoId != repoEntity->m_sId)) {
			if (actorData.repoId && *actorData.repoId != repoEntity->m_sId)
				State::current.actorDataRepoIdMap.erase(*actorData.repoId);
			actorData.repoId = repoEntity->m_sId;
			State::current.actorDataRepoIdMap.emplace(*actorData.repoId, i);
		}

		if (!actorRef.m_pInterfaceRef) continue;

		auto& actor = *actorRef.m_pInterfaceRef;
		actorData.isTarget = actor.m_bContractTarget;
		actorData.isPacified = actor.IsPacified();
		actorData.isDead = !actor.IsPacified() && actor.IsDead();

		auto spatial = actorRef.m_entityRef.QueryInterface<ZSpatialEntity>();
		if (spatial) {
			auto matrix = spatial->GetObjectToWorldMatrix();
			actorData.transform = spatial->m_mTransform;
			actorData.roomId = ZRoomManagerCreator::GetRoomID(matrix.Pos);
		}

		auto character = actor.m_rCharacter;
		auto outfit = actor.m_rOutfit;
		auto characterTemplateAspect = character.m_entityRef.QueryInterface<ZCharacterTemplateAspect>();
		auto characterTemplateAspectRef = character.m_entityRef.QueryInterface<TEntityRef<ZCharacterTemplateAspect>>();

		if (outfit && outfit.m_pInterfaceRef) {
			auto outfitRef = outfit.m_pInterfaceRef;
			actorData.isFemale = outfitRef->m_bIsFemale;
			actorData.hasDisguise = outfitRef->m_bHeroDisguiseAvailable;
			actorData.disguiseRepoId = outfitRef->m_sId;
			actorData.actorType = outfitRef->m_eActorType;
			actorData.outfitType = outfitRef->m_eOutfitType;
		}

		if (actor.m_bIsBeingDragged && gameplay.playerIsDragging && !gameplay.sentPlayerDraggingEvent) {
			SendCustomEvent("DragBodyMove"sv, ImbuedActorInfo(actorRef, ImbuedPlayerInfo()));
			gameplay.sentPlayerDraggingEvent = true;
		}

		// Handle roulette target No KO confirmations
		if (!actorData.isTarget || !actorData.repoId) continue;

		auto targetId = GetTargetByRepoID(*actorData.repoId);
		auto const& conditions = State::current.spin.getConditions();
		if (conditions.empty()) continue;

		for (auto i = 0; i < conditions.size() && i < State::current.killValidations.size(); ++i) {
			auto& cond = conditions[i];
			auto& target = cond.target.get();
			if (targetId != target.getID()) continue;
			auto& kc = State::current.getKillConfirmation(i);
			if (!kc.isPacified) break;

			if (!actor.IsPacified() && !actor.IsDead())
				kc.isPacified = false;
		}
	}

	State::current.actorDataSize = *Globals::NextActorId;
}

auto CroupierPlugin::ProcessClientEvent(std::string_view name, const json& json) -> void {
	if (name == "Areas") {
		State::current.areas.clear();
		if (!json.is_array())
			return;

		for (auto const& areaJson : json) {
			if (!areaJson.is_object()) continue;

			auto const& id = areaJson["ID"];
			if (!id.is_string()) continue;

			auto const& fromJson = areaJson["From"];
			if (!fromJson.is_object()) continue;
			if (fromJson.size() != 3) continue;

			auto const& toJson = areaJson["To"];
			if (!toJson.is_object()) continue;
			if (toJson.size() != 3) continue;

			auto const& fromXJson = fromJson["X"];
			auto const& fromYJson = fromJson["Y"];
			auto const& fromZJson = fromJson["Z"];
			auto const& toXJson = toJson["X"];
			auto const& toYJson = toJson["Y"];
			auto const& toZJson = toJson["Z"];

			Area area;
			area.ID = id.get<std::string>();
			area.From.x = fromXJson.get<float32>();
			area.From.y = fromYJson.get<float32>();
			area.From.z = fromZJson.get<float32>();
			area.To.x = toXJson.get<float32>();
			area.To.y = toYJson.get<float32>();
			area.To.z = toZJson.get<float32>();

			State::current.areas.push_back(std::move(area));
		}
	}
}

auto CroupierPlugin::ProcessClientMessages() -> void {
	ClientMessage message;
	if (State::current.client.tryTakeMessage(message)) {
		switch (message.type) {
			case eClientMessage::Event: {
				auto json = json::parse(message.args);
				auto name = json.value("Name", "");
				if (name.empty()) return;
				ProcessClientEvent(name, json["Data"]);
				return;
			}
			case eClientMessage::SpinData:
				return ProcessSpinDataMessage(message);
			case eClientMessage::BingoData:
				return ProcessBingoDataMessage(message);
			case eClientMessage::Missions:
				return ProcessMissionsMessage(message);
			case eClientMessage::GameMode:
				return ProcessGameModeMessage(message);
			case eClientMessage::SpinLock:
				if (message.args.size() < 1) break;
				State::current.spinLocked = message.args[0] == '1';
				return;
			case eClientMessage::Streak:
				if (message.args.size() < 1) break;
				std::from_chars(message.args.c_str(), message.args.c_str() + message.args.size(), this->config.streakCurrent);
				return;
			case eClientMessage::Timer: {
				auto parts = split(message.args, ",", 2);
				if (parts.empty() || parts[0].empty()) return;
				auto timerStopped = 0;
				if (std::from_chars(parts[0].data(), parts[0].data() + parts[0].size(), timerStopped).ec != std::errc()) {
					if (timerStopped) State::current.isFinished = true;
					else {
						State::current.isPlaying = true;
						State::current.isFinished = false;
					}
				}
				if (parts.size() < 2) return;
				uint64_t timeElapsed = 0;
				auto res = std::from_chars(parts[1].data(), parts[1].data() + parts[1].size(), timeElapsed);
				if (res.ec == std::errc()) {
					auto now = std::chrono::steady_clock::now();
					State::current.timeStarted = now - std::chrono::milliseconds(timeElapsed);
					State::current.timeElapsed = std::chrono::duration_cast<std::chrono::seconds>(now - State::current.timeStarted);
				}
				return;
			}
		}
	}
}

auto CroupierPlugin::ProcessLoadRemoval() -> void {
	class ZRenderManager {
	public:
		virtual ~ZRenderManager() = default;
		virtual bool ZRenderManager_unk1() = 0;
		virtual bool ZRenderManager_unk2() = 0;
		virtual bool ZRenderManager_unk3() = 0;
		virtual bool ZRenderManager_unk4() = 0; //
		virtual bool ZRenderManager_unk5() = 0; // gracefully freezes the game??
		virtual bool IsLoadingScreenActive() = 0;

	public:
		PAD(0x14178);
		ZRenderDevice* m_pDevice; // 0x14180, look for ZRenderDevice constructor
		PAD(0xF8); // 0x14188
		ZRenderContext* m_pRenderContext; // 0x14280, look for "ZRenderManager::RenderThread                               " string, first thing being constructed and assigned
	};

	static_assert(offsetof(ZRenderManager, m_pDevice) == 0x14180);
	static_assert(offsetof(ZRenderManager, m_pRenderContext) == 0x14280);

	auto ptr = Globals::RenderManager;
	auto renderManager = reinterpret_cast<ZRenderManager*>(Globals::RenderManager);
	if (!renderManager) return;

	auto isLoadingScreenActive = renderManager->IsLoadingScreenActive();

	if ((isLoadingScreenActive || loadingScreenActivated) && !loadRemovalActive) {
		SendLoadStarted();
		loadRemovalActive = true;
	}

	if (isLoadingScreenActive)
		isLoadingScreenCheckHasBeenTrue = true;
	else if (isLoadingScreenCheckHasBeenTrue) {
		loadingScreenActivated = false;
		isLoadingScreenCheckHasBeenTrue = false;

		if (loadRemovalActive) {
			SendLoadFinished();
			loadRemovalActive = false;
		}
	}
}

auto CroupierPlugin::ProcessMissionsMessage(const ClientMessage& message) -> void {
	auto tokens = split(message.args, ",");
	Config::main.missionPool.clear();
	std::string buffer;

	for (auto const& token : tokens) {
		buffer = trim(token);
		auto mission = getMissionByCodename(buffer);
		if (mission != eMission::NONE)
			Config::main.missionPool.push_back(mission);
	}
}

auto CroupierPlugin::ProcessSpinDataMessage(const ClientMessage& message) -> void {
	auto spin = SpinParser::parse(message.args);
	if (!spin.has_value()) return;

	State::current.spin = std::move(*spin);
	State::current.isPlaying = false;
	this->currentSpinSaved = true;
	State::current.generator.setMission(State::current.spin.getMission());
	State::current.spinCompleted = false;
}

auto CroupierPlugin::ProcessBingoDataMessage(const ClientMessage& message) -> void {
	auto js = json::parse(message.args);
	if (!js.is_object()) return;

	auto mission = static_cast<eMission>(js.value("Mission", 0));
	auto it = js.find("Tiles");
	if (it == js.end()) return;
	if (!it->is_array()) return;

	auto card = BingoCard{};
	for (auto const& tilejs : *it) {
		if (!tilejs.is_object()) return;
		BingoTile tile;
		tile.text = tilejs.value("Text", "");
		tile.group = tilejs.value("Group", "");
		tile.tip = tilejs.value("Tip", "");
		tile.achieved = tilejs.value("Achieved", false);
		tile.failed = tilejs.value("Failed", false);
		tile.groupColour = tilejs.value("GroupColour", 0xFFFFFFFF);
		card.tiles.emplace_back(std::move(tile));
	}

	std::unique_lock lock(State::current.stateMutex);

	State::current.card = std::move(card);
	State::current.isPlaying = false;
	State::current.generator.setMission(Missions::get(mission));
	this->currentSpinSaved = true;
	State::current.spinCompleted = false;
}

auto CroupierPlugin::ProcessGameModeMessage(const ClientMessage& message) -> void {
	if (message.args.size() < 1) return;
	uint8 val;
	auto res = std::from_chars(message.args.c_str(), message.args.c_str() + message.args.size(), val);
	if (res.ec != std::errc()) return;
	switch (val) {
		case (uint8)GameMode::Bingo:
		case (uint8)GameMode::Hybrid:
		case (uint8)GameMode::Roulette:
			State::current.gameMode = static_cast<GameMode>(val);
			break;
	}
}

auto CroupierPlugin::OnDrawMenu() -> void {
	this->ui.DrawMenu();
}

auto CroupierPlugin::OnDrawUI(bool focused) -> void {
	this->ui.Draw(focused);
}

auto CroupierPlugin::SaveSpinHistory() -> void {
	if (!State::current.generator.getMission()) return;
	if (State::current.spin.getConditions().empty()) return;

	if (!this->currentSpinSaved) {
		SerializedSpin spin;

		for (const auto& cond : State::current.spin.getConditions()) {
			SerializedSpin::Condition condition;
			condition.targetName = Keyword::getForTarget(cond.target.get().getName());
			condition.disguise = cond.disguise.get().name;
			condition.killMethod = cond.killComplication != eKillComplication::None ? std::format("({}) ", Keyword::get(cond.killComplication)) : "";
			condition.killMethod += cond.killType != eKillType::Any ? std::format("{} ", Keyword::get(cond.killType)) : "";
			condition.killMethod += cond.killMethod.method != eKillMethod::NONE ? Keyword::get(cond.killMethod.method) : Keyword::get(cond.specificKillMethod.method);
			spin.conditions.push_back(std::move(condition));
		}

		this->config.spinHistory.push_back(std::move(spin));
		this->currentSpinSaved = true;
	}

	Config::Save();
}

auto CroupierPlugin::OnFinishMission() -> void {
	if (!State::current.generator.getMission()) return;
	if (State::current.spin.getConditions().empty()) return;
}

auto CroupierPlugin::SetDefaultMissionPool() -> void {
	this->config.missionPool = defaultMissionPool;
}

auto CroupierPlugin::PreviousSpin() -> void {
	if (State::current.spinHistory.empty()) return;

	State::current.spin = std::move(State::current.spinHistory.top());
	State::current.isPlaying = false;
	this->currentSpinSaved = true;
	State::current.generator.setMission(State::current.spin.getMission());
	State::current.spinHistory.pop();
	State::current.spinCompleted = false;

	State::current.playerStart();
}

auto CroupierPlugin::Random() -> void {
	if (State::current.client.isConnected()) {
		SendRandom();
		return;
	}

	if (this->config.missionPool.empty())
		this->SetDefaultMissionPool();
	if (this->config.missionPool.empty())
		return;

	auto mission = randomVectorElement(this->config.missionPool);
	auto currentMission = State::current.spin.getMission();

	if (currentMission && mission == currentMission->getMission())
		this->Respin(false);
	else
		State::OnMissionSelect(mission, false);
}

auto CroupierPlugin::Respin(bool isAuto) -> void {
	LogDebug("Respin()");
	if (!State::current.generator.getMission()) return;

	auto mission = State::current.generator.getMission()->getMission();
	LogDebug("Issuing respin for mission {}", static_cast<int>(mission));

	if (isAuto)
		SendAutoSpin(mission);
	else
		SendRespin(mission);

	State::current.generator.setRuleset(&State::current.rules);

	if (isAuto && State::current.spinLocked) return;
	if (State::current.client.isConnected()) return;

	try {
		if (!State::current.spin.getConditions().empty()) {
			State::current.spinHistory.emplace(std::move(State::current.spin));
		}

		std::unique_lock lock(State::current.stateMutex);

		State::current.spin = State::current.generator.spin(&State::current.spin);
		State::current.timeStarted = std::chrono::steady_clock::now();
		this->currentSpinSaved = false;
		State::current.spinCompleted = false;
	} catch (const std::runtime_error& ex) {
		Logger::Error("Croupier: {}", ex.what());
	}

	this->SaveSpinHistory();
}

auto lastThrownItem = ""s;

auto CroupierPlugin::GetOutfitByRepoId(std::string_view repoId) const -> const ZGlobalOutfitKit* {
	return this->GetOutfitByRepoId(ZRepositoryID{repoId});
}

auto CroupierPlugin::GetOutfitByRepoId(ZRepositoryID repoId) const -> const ZGlobalOutfitKit* {
	if (!Globals::ContentKitManager) return nullptr;
	auto& globalOutfitKitsRepo = Globals::ContentKitManager->m_repositoryGlobalOutfitKits;
	auto it = globalOutfitKitsRepo.find(repoId);
	if (it == globalOutfitKitsRepo.end() || !it->second.m_pInterfaceRef)
		return nullptr;
	return it->second.m_pInterfaceRef;
}

/*auto CroupierPlugin::GetItemContainer(ZEntityRef entity) -> TEntityRef<IItemContainer> {
	auto item = entity.QueryInterface<ZHM5Item>();
	if (!item) return {};
	return item->m_rItemContainer;
	for (const auto action : Globals::HM5ActionManager->m_Actions) {
		if (!action || (static_cast<uint64_t>(action->m_eActionType) & static_cast<uint64_t>(EActionType::AT_ITEMCONTAINER)) == 0)
			continue;
		auto container = action->m_Object.QueryInterface<ZItemStorageEntity>();
		if (container->m_rStorage) {
			//container->m_rStorage.m_pInterfaceRef->
		}
		//return action->m_Object.Ge;
		if (!item->m_pItemConfigDescriptor) continue;
		if (item->m_pItemConfigDescriptor->m_ItemID != ZRepositoryID(ev.RepositoryId))
			continue;
		//if (State::current.collectedItemInstances.contains(instanceId))
		//	continue;
		//State::current.collectedItemInstances.insert(instanceId);
		return ImbuedPlayerInfo({
			{"ItemRepositoryId", ev.RepositoryId},
			{"ItemInstanceId", item->GetType()->m_nEntityID},
			{"ItemType", ev.ItemType},
			{"ItemName", ev.ItemName},
		});
	}
}*/

auto CroupierPlugin::ImbueDisguiseEvent(const std::string& repoId) -> json {
	auto outfit = this->GetOutfitByRepoId(repoId);
	auto json = json::object({ {"RepositoryId", repoId} });
	ImbuePlayerInfo(json);
	if (outfit) {
		json.merge_patch({
			{"Title", outfit->m_sTitle},
			{"ActorType", outfit->m_eActorType},
			{"IsSuit", outfit->m_bIsHitmanSuit},
			{"OutfitType", outfit->m_eOutfitType},
		});
	}
	if (gameplay.disguiseChange.havePinData) {
		json.merge_patch({
			{"IsBundle" , gameplay.disguiseChange.wasFree},
		});
		gameplay.disguiseChange.havePinData = false;
		gameplay.disguiseChange.wasFree = false;
	}

	gameplay.disguiseChange = GameplayData::DisguiseChangeData{};
	return json;
}

auto CroupierPlugin::ImbueActorInfoWithReference(TEntityRef<ZActor> ref, json& j, bool asActor, bool referenceDataOnly) const -> void {
	if (!ref) return;

	const auto actor = ref.m_pInterfaceRef;
	const auto repoEntity = ref.m_entityRef.QueryInterface<ZRepositoryItemEntity>();

	if (repoEntity) {
		const auto& repoId = repoEntity->m_sId;
		j.merge_patch({
			{"ActorRepositoryId", repoId.ToString()},
		});
		if (!referenceDataOnly) ImbueActorInfoWithRepoID(repoId, j, asActor, true);
	}

	if (actor->m_rOutfit) {
		auto outfit = actor->m_rOutfit.m_pInterfaceRef;
		j.merge_patch({
			{"ActorIsAuthorityFigure", outfit->m_bAuthorityFigure},
			{"ActorOutfitAllowsWeapons", outfit->m_bWeaponsAllowed},
			{"ActorOutfitRepositoryId", outfit->m_sId.ToString()},
			{"ActorType", outfit->m_eActorType},
		});
	}

	j.merge_patch({
		{"ActorName", actor->m_sActorName},
		{"ActorWeaponIndex", actor->m_nWeaponIndex},
		{"ActorWeaponUnholstered", actor->m_bWeaponUnholstered},
	});
}

auto CroupierPlugin::ImbueActorInfoWithRepoID(ZRepositoryID repoId, json& j, bool asActor, bool repoDataOnly) const -> void {
	j.merge_patch({
		{"ActorRepositoryId", repoId.ToString()},
	});

	if (const auto actorData = State::current.getActorDataByRepoId(repoId)) {
		auto area = State::current.getArea(actorData->transform.Trans);
		j.merge_patch({
			{"ActorArea", area ? area->ID : ""},
			{"ActorHasDisguise", actorData->hasDisguise},
			{"ActorIsDead", actorData->isDead},
			{"ActorIsFemale", actorData->isFemale},
			{"ActorIsPacified", actorData->isPacified},
			{"ActorIsTarget", actorData->isTarget},
			{"ActorOutfitType", actorData->outfitType},
			{"ActorRoom", actorData->roomId},
			{"ActorPosition", {
				{"X", actorData->transform.Trans.x},
				{"Y", actorData->transform.Trans.y},
				{"Z", actorData->transform.Trans.z},
			}},
		});
		if (actorData->actor && !repoDataOnly)
			ImbueActorInfoWithReference(*actorData->actor, j, asActor, true);
	}
}

auto CroupierPlugin::ImbueActorInfo(TEntityRef<ZActor> ref, json& j, bool asActor) const -> void {
	ImbueActorInfoWithReference(ref, j, asActor);
}

auto CroupierPlugin::ImbueActorInfo(ZRepositoryID repoId, json& j, bool asActor) const -> void {
	ImbueActorInfoWithRepoID(repoId, j, asActor);
}

auto CroupierPlugin::ImbuePacifyEvent(const PacifyEventValue& ev) const -> std::optional<json> {
	const auto actorData = State::current.getActorDataByRepoId(ZRepositoryID(ev.RepositoryId));
	if (!actorData) return std::nullopt;
	auto const playerOutfitRepoId = ZRepositoryID(ev.OutfitRepositoryId);
	auto const actorOutfit = actorData->disguiseRepoId ? this->GetOutfitByRepoId(*actorData->disguiseRepoId) : nullptr;
	return ImbuedPlayerInfo({
		{"RepositoryId", ev.RepositoryId},
		{"Accident", ev.Accident},
		{"ActorName", ev.ActorName},
		{"ActorType", ev.ActorType},
		{"DamageEvents", ev.DamageEvents},
		{"ExplosionType", ev.ExplosionType},
		{"Explosive", ev.Explosive},
		{"IsHeadshot", ev.IsHeadshot},
		{"IsTarget", ev.IsTarget},
		{"OutfitIsHitmanSuit", ev.OutfitIsHitmanSuit},
		{"OutfitRepositoryId", ev.OutfitRepositoryId},
		{"KillClass", ev.KillClass},
		{"KillContext", ev.KillContext},
		{"KillItemCategory", ev.KillItemCategory},
		{"KillItemRepositoryId", ev.KillItemRepositoryId},
		{"KillMethodBroad", ev.KillMethodBroad},
		{"KillMethodStrict", ev.KillMethodStrict},
		{"KillType", ev.KillType},
		{"Projectile", ev.Projectile},
		{"SetPieceId", ev.SetPieceId},
		{"SetPieceType", ev.SetPieceType},
		{"Sniper", ev.Sniper},
		{"WeaponSilenced", ev.WeaponSilenced},
		{"RoomId", ev.RoomId},
		{"ActorHasDisguise", actorData->hasDisguise},
		{"ActorHasSameOutfit", actorData->disguiseRepoId && *actorData->disguiseRepoId == playerOutfitRepoId},
		{"ActorOutfitRepositoryId", actorData->disguiseRepoId ? toLowerCase(actorData->disguiseRepoId->ToString()) : ""},
		{"ActorOutfitType", actorData->outfitType},
		{"IsFemale", actorData->isFemale},
		{"ActorPosition", {
			{"X", actorData->transform.Trans.x},
			{"Y", actorData->transform.Trans.y},
			{"Z", actorData->transform.Trans.z},
		}},
	}, true);
}

auto CroupierPlugin::ImbuePlayerLocation(json& json, bool asHero) const -> void {
	const auto& trans = State::current.playerMatrix.Trans;
	json.merge_patch({
		{"IsIdle", State::current.playerMoveType == PlayerMoveType::Idle},
		{"IsCrouching", State::current.playerMoveType == PlayerMoveType::CrouchRunning || State::current.playerMoveType == PlayerMoveType::CrouchWalking},
		{"IsRunning", State::current.playerMoveType == PlayerMoveType::CrouchRunning || State::current.playerMoveType == PlayerMoveType::Running},
		{"IsWalking", State::current.playerMoveType == PlayerMoveType::CrouchWalking || State::current.playerMoveType == PlayerMoveType::Walking},
		{"IsTrespassing", State::current.isTrespassing},
		{asHero ? "HeroRoom" : "Room", State::current.roomId},
		{asHero ? "HeroArea" : "Area", State::current.area ? State::current.area->ID : ""},
		{asHero ? "HeroPosition" : "Position", {
			{"X", trans.x},
			{"Y", trans.y},
			{"Z", trans.z},
		}},
	});
}

auto CroupierPlugin::ImbuePlayerInfo(json& json, bool asHero) const -> void {
	const auto& trans = State::current.playerMatrix.Trans;
	ImbuePlayerLocation(json, asHero);
	auto disguiseChange = State::current.getLastDisguiseChange();
	json.merge_patch({
		{asHero ? "HeroOutfit" : "Outfit", disguiseChange ? disguiseChange->disguiseRepoId : ""},
		{asHero ? "HeroOutfitIsHitmanSuit" : "OutfitIsHitmanSuit", !disguiseChange},
	});
}

static auto weaponAnimSetToString(ECCWeaponAnimSet animsSet) -> std::string {
	switch (animsSet) {
		case ECCWeaponAnimSet::AS_AXE:
			return "AS_AXE"s;
		case ECCWeaponAnimSet::AS_BASH_1H:
			return "AS_BASH_1H"s;
		case ECCWeaponAnimSet::AS_BASH_2H:
			return "AS_BASH_2H"s;
		case ECCWeaponAnimSet::AS_SLIT_THROAT_1H:
			return "AS_SLIT_THROAT_1H"s;
		case ECCWeaponAnimSet::AS_SMASH_1H:
			return "AS_SMASH_1H"s;
		case ECCWeaponAnimSet::AS_STAB_1H:
			return "AS_STAB_1H"s;
		case ECCWeaponAnimSet::AS_STAB_2H:
			return "AS_STAB_2H"s;
		case ECCWeaponAnimSet::AS_STRANGLE:
			return "AS_STRANGLE"s;
		case ECCWeaponAnimSet::AS_STRANGLE_2H:
			return "AS_STRANGLE_2H"s;
		case ECCWeaponAnimSet::AS_SWING_1H:
			return "AS_SWING_1H"s;
		case ECCWeaponAnimSet::AS_SWING_2H:
			return "AS_SWING_2H"s;
		case ECCWeaponAnimSet::AS_SWORD_1H:
			return "AS_SWORD_1H"s;
	}
	return ""s;
}

auto CroupierPlugin::ImbueItemEvent(const ItemEventValue& ev, EActionType actionType) const -> std::optional<json> {
	for (const auto action : Globals::HM5ActionManager->m_Actions) {
		if (!action || (static_cast<uint64_t>(action->m_eActionType) & static_cast<uint64_t>(actionType)) == 0)
			continue;
		const ZHM5Item* item = action->m_Object.QueryInterface<ZHM5Item>();
		if (!item) continue;
		if (!item->m_pItemConfigDescriptor) continue;
		if (item->m_pItemConfigDescriptor->m_ItemID != ZRepositoryID(ev.RepositoryId))
			continue;
		//if (State::current.collectedItemInstances.contains(instanceId))
		//	continue;
		//State::current.collectedItemInstances.insert(instanceId);
		return ImbuedPlayerInfo({
			{"ItemRepositoryId", ev.RepositoryId},
			{"ItemInstanceId", item->GetType()->m_nEntityID},
			{"ItemType", ev.ItemType},
			{"ItemName", ev.ItemName},
		});
	}
	return std::nullopt;
}

auto CroupierPlugin::ImbueItemRepositoryInfo(json& j, ZRepositoryID repoId) -> void {
	std::string itemType;
	std::string itemSize;
	std::string commonName;
	std::vector<std::string> perks;
	std::vector<std::string> onlineTraits;

	if (repositoryResource.m_nResourceIndex.val == -1) {
		const auto s_ID = ResId<"[assembly:/repository/pro.repo].pc_repo">;
		Globals::ResourceManager->GetResourcePtr(repositoryResource, s_ID, 0);
	}
	if (repositoryResource.GetResourceInfo().status == RESOURCE_STATUS_VALID) {
		auto repositoryData = static_cast<THashMap<ZRepositoryID, ZDynamicObject, TDefaultHashMapPolicy<ZRepositoryID>>*>(repositoryResource.GetResourceData());
		if (repositoryData) {
			auto it = repositoryData->find(repoId);
			if (it != repositoryData->end()) {
				const auto entries = it->second.As<TArray<SDynamicObjectKeyValuePair>>();
				if (entries) {
					for (size_t i = 0; i < entries->size(); ++i) {
						auto const& entry = (*entries)[i];
						if (entry.sKey == "ItemType")
							itemType = *entry.value.As<ZString>();
						else if (entry.sKey == "Perks") {
							auto arr = entry.value.As<TArray<ZDynamicObject>>();
							if (!arr) continue;
							perks.reserve(arr->size());
							for (auto& obj : *arr) {
								auto str = obj.As<ZString>();
								if (!str) continue;
								perks.emplace_back(*str);
							}
						}
						else if (entry.sKey == "OnlineTraits") {
							auto arr = entry.value.As<TArray<ZDynamicObject>>();
							if (!arr) continue;
							onlineTraits.reserve(arr->size());
							for (auto& obj : *arr) {
								auto str = obj.As<ZString>();
								if (!str) continue;
								onlineTraits.emplace_back(*str);
							}
						}
						else if (entry.sKey == "CommonName") {
							commonName = *entry.value.As<ZString>();
						}
						else continue;
					}
				}
			}
		}
	}
	j.merge_patch({
		{"RepositoryItemSize", itemSize},
		{"RepositoryItemType", itemType},
		{"RepositoryCommonName", commonName},
		{"RepositoryPerks", perks},
		{"RepositoryOnlineTraits", onlineTraits},
	});
}

auto CroupierPlugin::ImbuePositionInfo(json& j, SVector3 pos, std::string prefix) -> void {
	auto area = State::current.getArea(pos);
	j.merge_patch({
		{prefix + "Area", area ? area->ID : ""},
		{prefix + "Room", ZRoomManagerCreator::GetRoomID({pos.x, pos.y, pos.z, 1.0})},
		{prefix + "Position", {
			{"X", pos.x},
			{"Y", pos.y},
			{"Z", pos.z},
		}},
	});
}

auto CroupierPlugin::ImbuedPositionInfo(SVector3 pos, std::string prefix, json&& j) -> json {
	ImbuePositionInfo(j, pos, prefix);
	return j;
}

auto CroupierPlugin::ImbuedSetepieceInfo(ZEntityRef entity, json&& j) -> json {
	ImbueSetpieceInfo(entity, j);
	return j;
}

auto CroupierPlugin::ImbueSetpieceInfo(ZEntityRef entity, json& j) -> bool {
	if (!entity || !entity->GetType()) return false;

	auto const spatial = QueryAnyParent<ZSpatialEntity>(entity);
	if (!spatial) return false;

	auto const setpiece = entity.GetLogicalParent();

	auto const& trans = spatial->m_mTransform.Trans;
	auto entityId = setpiece->GetType()->m_nEntityID;

	auto obj = ImbuedPositionInfo({ trans.x, trans.y, trans.z }, "", {
		{"EntityID", entityId},
	});
	auto sid = setpiece.GetProperty<ZRepositoryID>("m_sId");
	if (!sid.IsEmpty())
		obj.merge_patch({ {"RepositoryId", sid.Get().ToString()} });
	j.merge_patch(obj);
	return true;
}

auto CroupierPlugin::ImbuedSetpieceActivatorInfo(ZEntityRef entity, json&& j) -> json {
	ImbueSetpieceActivatorInfo(entity, j);
	return j;
}

auto CroupierPlugin::ImbueSetpieceActivatorInfo(ZEntityRef entity, json& j) -> bool {
	if (!entity || !entity->GetType()) return false;

	auto const spatial = QueryAnyParent<ZSpatialEntity>(entity);
	if (!spatial) return false;

	auto const& trans = spatial->GetObjectToWorldMatrix().Trans;
	auto const parentity = entity.GetLogicalParent();
	auto entityId = entity->GetType()->m_nEntityID;
	auto const isGenericActivator = entityId == 0x3BD21B06F863B910;

	auto setpieceEntity = ZEntityRef{};
	if (isGenericActivator) {
		if (parentity && parentity->GetType())
			entityId = parentity->GetType()->m_nEntityID;
		setpieceEntity = GetClosestEntityWithProperty<ZRepositoryID>(parentity, "m_sId");
		if (setpieceEntity && setpieceEntity->GetType())
			entityId = setpieceEntity->GetType()->m_nEntityID;
	}
	if (!setpieceEntity) setpieceEntity = entity;
	auto setpieceRepoIdPtr = GetValuePropertyFromTree<ZRepositoryID>(setpieceEntity, "m_sId");
	if (!setpieceRepoIdPtr) return false;
	auto initialStateOn = GetValuePropertyFromTree<bool>(setpieceEntity, "m_bInitialStateOn");
	if (!setpieceRepoIdPtr) return false;

	auto obj = ImbuedPositionInfo({ trans.x, trans.y, trans.z }, "", {
		{"RepositoryId", setpieceRepoIdPtr->ToString()},
		{"EntityID", entityId},
	});
	j.merge_patch(obj);
	if (initialStateOn)
		j.merge_patch({{"InitialStateOn", *initialStateOn}});
	return true;
}

auto CroupierPlugin::ImbueItemInfo(ZEntityRef entity, json& j, std::string prefix) -> void {
	auto item = QueryAnyParent<ZHM5Item>(entity);
	auto spawner = QueryAnyParent<ZItemSpawner>(entity);
	auto spatial = QueryAnyParent<ZSpatialEntity>(entity);

	if (!item) {
		if (!spawner || !spawner->m_rMainItemKey) return;
		auto& repoId = spawner->m_rMainItemKey.m_pInterfaceRef->m_RepositoryId;
		ImbuePositionInfo(j, spawner->GetObjectToWorldMatrix().ToMatrix43().Trans, prefix);
		ImbueItemRepositoryInfo(j, repoId);
		j.merge_patch({
			{prefix + "EntityID", spawner->GetType()->m_nEntityID},
			{prefix + "RepositoryId", repoId.ToString()},
			{prefix + "InstanceId", reinterpret_cast<uintptr_t>(spawner)},
		});
		return;
	}

	if (spatial) {
		ImbuePositionInfo(j, spatial->GetObjectToWorldMatrix().ToMatrix43().Trans, prefix);
	}

	auto ccWeapon = QueryAnyParent<ZHM5ItemCCWeapon>(entity);
	auto isFiberWire = ccWeapon && ccWeapon->m_bCountsAsFiberWire;
	
	if (const auto desc = item->m_pItemConfigDescriptor) {
		std::string itemType;
		std::string itemSize;
		std::vector<std::string> perks;
		std::vector<std::string> onlineTraits;

		ImbueItemRepositoryInfo(j, desc->m_ItemID);

		j.merge_patch({
			{prefix + "Name", desc->m_sTitle},
			{prefix + "InstanceId", reinterpret_cast<uintptr_t>(item)},
			{prefix + "RepositoryId", desc->m_ItemID.ToString()},
		});
	}

	j.merge_patch({
		{prefix + "EntityID", entity->GetType()->m_nEntityID},
	});

	// Back-compat because we didn't want to prefix these, but we probably should
	if (prefix == "Item")
		prefix = "";

	if (ccWeapon) {
		j.merge_patch({
			{prefix + "WeaponAnimFrontSide", weaponAnimSetToString(ccWeapon->m_eAnimSetFrontSide)},
			{prefix + "WeaponAnimBack", weaponAnimSetToString(ccWeapon->m_eAnimSetBack)},
		});
	}
	if (auto weapon = QueryAnyParent<ZHM5ItemWeapon>(entity)) {
		j.merge_patch({
			{prefix + "IsScopedWeapon", weapon->m_bScopedWeapon},
			{prefix + "WeaponAnimationCategory", weapon->m_eAnimationCategory},
			{prefix + "WeaponType", weapon->m_WeaponType},
		});
	}
	
	j.merge_patch({
		{prefix + "IsCloseCombatWeapon", ccWeapon != nullptr},
		{prefix + "IsFiberWire", isFiberWire},
		{prefix + "IsFirearm", QueryAnyParent<IFirearm>(entity) != nullptr},
		{prefix + "IsWeapon", QueryAnyParent<IItemWeapon>(entity) != nullptr},
	});
}

auto CroupierPlugin::ImbuedPlayerLocation(json&& j, bool asHero) const -> json {
	ImbuePlayerLocation(j, asHero);
	return j;
}

auto CroupierPlugin::ImbuedPlayerInfo(json&& j, bool asHero) const -> json {
	ImbuePlayerInfo(j, asHero);
	return j;
}

auto CroupierPlugin::ImbuedActorInfo(TEntityRef<ZActor> entity, json&& js, bool asActor) const -> json {
	ImbueActorInfo(entity, js, asActor);
	return js;
}

auto CroupierPlugin::ImbuedActorInfo(ZRepositoryID repoId, json&& js, bool asActor) const -> json {
	ImbueActorInfo(repoId, js, asActor);
	return js;
}

auto CroupierPlugin::ImbuedItemInfo(ZEntityRef entity, json&& js, std::string prefix) -> json {
	ImbueItemInfo(entity, js);
	return js;
}

auto CroupierPlugin::SendCustomEvent(std::string_view name, json eventValue) const -> void {
#ifndef _DEBUG
	if (!this->config.debug && !State::current.client.isConnected()) return;
#endif
	json js = {
		{"Name", name},
		{"Value", eventValue},
	};
	auto dump = js.dump();
	LogDebug("<--- {}", dump);
	State::current.client.sendRaw(dump);
}

auto CroupierPlugin::AddPinListener(ZHMPin pinId, std::function<PinListeners::HandlerFunc> func) -> void {
	auto& listeners = this->GetOrMakePinListeners(pinId);
	listeners.add(func);
}

auto CroupierPlugin::GetPinListeners(ZHMPin pinId) -> PinListeners* {
	auto it = this->pinListeners.find(pinId);
	if (it == this->pinListeners.end()) return nullptr;
	return it->second.get();
}

auto CroupierPlugin::GetOrMakePinListeners(ZHMPin pinId) -> PinListeners& {
	auto it = this->pinListeners.find(pinId);
	if (it == this->pinListeners.end())
		it = this->pinListeners.emplace(pinId, std::make_unique<PinListeners>()).first;
	return *it->second;
}

auto CroupierPlugin::SetupEvents() -> void {
	events.listen<Events::ContractStart>([this](const ServerEvent<Events::ContractStart>& ev) {
		State::current.playerStart();
		State::current.locationId = ev.Value.LocationId;
		State::current.loadout = ev.Value.Loadout;
		State::current.spinCompleted = false;

		SendKillValidationUpdate();
	});
	events.listen<Events::HeroSpawn_Location>([this](const ServerEvent<Events::HeroSpawn_Location>& ev) {
		SendMissionStart(State::current.locationId, ev.Value.RepositoryId, State::current.loadout);
	});
	events.listen<Events::IntroCutEnd>([this](const ServerEvent<Events::IntroCutEnd>& ev) {
		State::current.playerCutsceneEnd(ev.Timestamp);
	});
	events.listen<Events::ContractLoad>([this](auto& ev) {
		State::current.playerLoad();
		SendKillValidationUpdate();
	});
	events.listen<Events::ExitGate>([this](const ServerEvent<Events::ExitGate>& ev) {
		State::current.playerExit(ev.Timestamp);

		// Mark any unfulfilled kill methods as invalid (never killed a Berlin agent with correct requirements, destroyed heart instead of killing Soders or vice-versa, etc.)
		auto const& conds = State::current.spin.getConditions();
		for (auto& kv : State::current.killValidations) {
			if (kv.correctMethod == eKillValidationType::Incomplete)
				kv.correctMethod = eKillValidationType::Invalid;
		}

		SendKillValidationUpdate();
		SendMissionComplete();
	});
	events.listen<Events::ExitTango>([this](const ServerEvent<Events::ExitTango>& ev) {
		SendMissionOutroBegin();
	});
	events.listen<Events::FacilityExitEvent>([this](const ServerEvent<Events::FacilityExitEvent>& ev) {
		State::current.playerExit(ev.Timestamp);

		// Mark any unfulfilled kill methods as invalid (never killed a Berlin agent with correct requirements, destroyed heart instead of killing Soders or vice-versa, etc.)
		auto const& conds = State::current.spin.getConditions();
		for (auto& kv : State::current.killValidations) {
			if (kv.correctMethod == eKillValidationType::Incomplete)
				kv.correctMethod = eKillValidationType::Invalid;
		}

		SendKillValidationUpdate();
		SendMissionComplete();
	});
	events.listen<Events::ContractEnd>([this](const ServerEvent<Events::ContractEnd>& ev) {
		if (!State::current.isFinished) {
			State::current.playerExit(ev.Timestamp);

			// Mark any unfulfilled kill methods as invalid (never killed a Berlin agent with correct requirements, destroyed heart instead of killing Soders or vice-versa, etc.)
			auto const& conds = State::current.spin.getConditions();
			for (auto& kv : State::current.killValidations) {
				if (kv.correctMethod == eKillValidationType::Incomplete)
					kv.correctMethod = eKillValidationType::Invalid;
			}

			SendKillValidationUpdate();
			SendMissionComplete();
		}

		State::current.spinCompleted = true;
	});
	events.listen<Events::ContractFailed>([this](const ServerEvent<Events::ContractFailed>& ev) {
		SendMissionFailed();
		Logger::Info("Croupier: ContractFailed {}", ev.Value.value.dump());
	});
	events.listen<Events::StartingSuit>([this](const ServerEvent<Events::StartingSuit>& ev) {
		this->SendCustomEvent("StartingSuit"sv, ImbueDisguiseEvent(ev.Value.value));

		if (State::current.spinCompleted) return;
		State::current.disguiseChanges.emplace_back(ev.Value.value, ev.Timestamp);
	});
	events.listen<Events::Disguise>([this](const ServerEvent<Events::Disguise>& ev) {
		if (gameplay.disguiseChange.havePinData)
			this->SendCustomEvent("Disguise"sv, ImbueDisguiseEvent(ev.Value.value));
		else {
			gameplay.disguiseChange.haveEventData = true;
			gameplay.disguiseChange.eventData = ev.Value.value;
		}

		if (State::current.spinCompleted) return;
		State::current.disguiseChanges.emplace_back(ev.Value.value, ev.Timestamp);
	});
	events.listen<Events::FriskedSuccess>([this](const ServerEvent<Events::FriskedSuccess>& ev) {
		this->SendCustomEvent("FriskedSuccess"sv, ImbuedPlayerInfo());
	});
	events.listen<Events::Actorsick>([this](const ServerEvent<Events::Actorsick>& ev) {
		this->SendCustomEvent("Actorsick"sv, ImbuedActorInfo(ZRepositoryID(ev.Value.actor_R_ID), ImbuedPlayerInfo({
			{"ActorID", ev.Value.ActorId},
			//{"actor_R_ID", ev.Value.actor_R_ID},
			{"IsTarget", ev.Value.IsTarget},
			{"ItemRepositoryId", ev.Value.item_R_ID},
			{"SetpieceRepositoryId", ev.Value.setpiece_R_ID},
		}, true)));
	});
	events.listen<Events::Dart_Hit>([this](const ServerEvent<Events::Dart_Hit>& ev) {
		this->SendCustomEvent("DartHit"sv, ImbuedPlayerInfo({
			{"RepositoryId", ev.Value.RepositoryId},
			{"ActorType", ev.Value.ActorType},
			{"Blind", ev.Value.Blind},
			{"Sedative", ev.Value.Sedative},
			{"Sick", ev.Value.Sick},
			{"IsTarget", ev.Value.IsTarget},
		}, true));
	});
	events.listen<Events::ItemThrown>([this](const ServerEvent<Events::ItemThrown>& ev) {
		//auto imbued = this->ImbueItemEvent(ev.Value, EActionType::AT_ITEM_INTERACTION);
		//if (imbued) this->SendImbuedEvent(ev, *imbued);
		lastThrownItem = ev.Value.RepositoryId;
		this->SendCustomEvent("ItemThrown"sv, ImbuedPlayerInfo({
			{"ItemRepositoryId", ev.Value.RepositoryId},
			{"ItemInstanceId", ev.Value.InstanceId},
			{"ItemType", ev.Value.ItemType},
			{"ItemName", ev.Value.ItemName},
		}, true));
	});
	events.listen<Events::ItemStashed>([this](const ServerEvent<Events::ItemStashed>& ev) {
		this->SendCustomEvent("ItemStashed"sv, ImbuedPlayerInfo({
			{"ActorId", ev.Value.ActorId},
			{"ActorName", ev.Value.ActorName},
			{"ItemId", ev.Value.ItemId},
			{"ItemTypeId", ev.Value.ItemTypeId},
			{"RepositoryId", ev.Value.RepositoryId},
		}, true));
	});
	events.listen<Events::ItemPickedUp>([this](const ServerEvent<Events::ItemPickedUp>& ev) {
		//auto imbued = this->ImbueItemEvent(ev.Value, EActionType::AT_PICKUP);
		//if (imbued) this->SendCustomEvent("ItemPickedUp"sv, *imbued);
	});
	events.listen<Events::Trespassing>([this](const ServerEvent<Events::Trespassing>& ev) {
		State::current.isTrespassing = ev.Value.IsTrespassing;
		this->SendCustomEvent("Trespassing"sv, ImbuedPlayerInfo());
	});
	events.listen<Events::BodyFound>([this](const ServerEvent<Events::BodyFound>& ev) {
		this->SendCustomEvent("BodyFound"sv, ImbuedPlayerInfo({
			{"RepositoryId", ev.Value.DeadBody.RepositoryId},
			{"DeathContext", ev.Value.DeadBody.DeathContext},
			{"DeathType", ev.Value.DeadBody.DeathType},
			{"IsCrowdActor", ev.Value.DeadBody.IsCrowdActor},
		}, true));
	});
	events.listen<Events::BodyHidden>([this](const ServerEvent<Events::BodyHidden>& ev) {
		this->SendCustomEvent("BodyHidden"sv, ImbuedActorInfo({ev.Value.RepositoryId}, ImbuedPlayerInfo({
			{"ActorId", ev.Value.ActorId},
			{"ActorName", ev.Value.ActorName},
			{"RepositoryId", ev.Value.RepositoryId},
		}, true), true));
	});
	events.listen<Events::Door_Unlocked>([this](const ServerEvent<Events::Door_Unlocked>& ev) {
		this->SendCustomEvent("DoorUnlocked"sv, ImbuedPlayerInfo());
	});
	events.listen<Events::Pacify>([this](const ServerEvent<Events::Pacify>& ev) {
		auto data = this->ImbuePacifyEvent(ev.Value);
		if (data) this->SendCustomEvent("Pacify"sv, *data);

		if (!ev.Value.IsTarget) return;
		if (State::current.spinCompleted) return;

		auto const& conditions = State::current.spin.getConditions();
		if (conditions.empty()) return;

		auto targetId = GetTargetByRepoID(ZRepositoryID(ev.Value.RepositoryId));

		for (auto i = 0; i < conditions.size(); ++i) {
			auto& cond = conditions[i];
			auto& target = cond.target.get();

			if (targetId != target.getID() && target.getName() != ev.Value.ActorName)
				continue;

			// If this pacification is a throw and the last thrown item is an impact explosive, ignore
			// this as a pacification so lethal throws with impact explosives pass the 'live' condition.
			if (ev.Value.KillMethodBroad == "throw" && cond.killType == eKillType::Impact) {
				if (checkExplosiveKillType(lastThrownItem, eKillType::Impact)) return;
			}

			auto& kc = State::current.getKillConfirmation(i);
			kc.target = target.getID();
			kc.isPacified = true;
		}
	});
	events.listen<Events::C_Hungry_Hippo>([this](const ServerEvent<Events::C_Hungry_Hippo>& ev) {
		if (State::current.spinCompleted) return;
		auto const mission = State::current.spin.getMission();
		if (!mission) return;
		if (mission->getMission() != eMission::SANTAFORTUNA_THREEHEADEDSERPENT) return;

		auto const& conditions = State::current.spin.getConditions();
		if (conditions.empty()) return;

		for (auto i = 0; i < conditions.size(); ++i) {
			auto const& cond = conditions[i];
			if (cond.target.get().getID() != eTargetID::RicoDelgado) continue;
			if (cond.specificKillMethod.method != eMapKillMethod::Rico_FeedToHippo) return;
			
			auto const& target = cond.target.get();
			auto& kc = State::current.getKillConfirmation(i);
			kc.target = eTargetID::RicoDelgado;
			kc.correctMethod = eKillValidationType::Valid;
			SendKillValidationUpdate();
			break;
		}
	});
	events.listen<Events::TargetEscapeFoiled>([this](const ServerEvent<Events::TargetEscapeFoiled>& ev) {
		if (State::current.spinCompleted) return;
		auto const mission = State::current.spin.getMission();
		if (!mission) return;
		if (mission->getMission() != eMission::HOKKAIDO_SITUSINVERSUS) return;

		auto const& conditions = State::current.spin.getConditions();
		if (conditions.empty()) return;

		for (auto i = 0; i < conditions.size(); ++i) {
			auto const& cond = conditions[i];
			if (cond.target.get().getID() != eTargetID::YukiYamazaki) continue;
			if (cond.specificKillMethod.method != eMapKillMethod::Yuki_SabotageCableCar) return;
			
			auto const& target = cond.target.get();
			auto& kc = State::current.getKillConfirmation(i);
			kc.target = eTargetID::YukiYamazaki;
			kc.correctMethod = eKillValidationType::Valid;
			SendKillValidationUpdate();
			break;
		}
	});
	events.listen<Events::Kill>([this](const ServerEvent<Events::Kill>& ev) {
		auto data = this->ImbuePacifyEvent(ev.Value);
		if (data) this->SendCustomEvent("Kill"sv, *data);

		static auto isBerlinAgent = [](eTargetID id) -> bool {
			switch (id) {
			case eTargetID::Agent1:
			case eTargetID::Agent2:
			case eTargetID::Agent3:
			case eTargetID::Agent4:
			case eTargetID::Agent5:
			case eTargetID::Agent6:
			case eTargetID::Agent7:
			case eTargetID::Agent8:
			case eTargetID::Agent9:
			case eTargetID::Agent10:
			case eTargetID::Agent11:
			case eTargetID::AgentBanner:
			case eTargetID::AgentChamberlin:
			case eTargetID::AgentDavenport:
			case eTargetID::AgentGreen:
			case eTargetID::AgentLowenthal:
			case eTargetID::AgentMontgomery:
			case eTargetID::AgentPrice:
			case eTargetID::AgentRhodes:
			case eTargetID::AgentSwan:
			case eTargetID::AgentThames:
			case eTargetID::AgentTremaine:
				return true;
			}
			return false;
		};

		if (State::current.spinCompleted) return;

		State::current.killed.insert(ev.Value.RepositoryId);
		State::current.spottedNotKilled.erase(ev.Value.RepositoryId);

		if (!ev.Value.IsTarget) {
			if (ev.Value.KillContext != EDeathContext::eDC_NOT_HERO)
				State::current.voidSA();
			return;
		}

		auto const& conditions = State::current.spin.getConditions();
		if (conditions.empty()) return;

		bool validationUpdated = false;
		auto it = targetsByRepoId.find(ZRepositoryID(ev.Value.RepositoryId));
		auto targetId = it != end(targetsByRepoId) ? it->second : eTargetID::Unknown;

		for (auto i = 0; i < conditions.size(); ++i) {
			auto const& cond = conditions[i];
			auto const& target = cond.target.get();
			bool isApexPrey = isBerlinAgent(target.getID()) && isBerlinAgent(targetId);
			auto& kc = State::current.getKillConfirmation(i);

			if (isApexPrey) {
				if (kc.correctMethod != eKillValidationType::Incomplete)
					continue;
			}
			else if (targetId != target.getID() && target.getName() != ev.Value.ActorName)
				continue;

			auto disguiseRepoId = ev.Value.OutfitIsHitmanSuit ? ev.Value.OutfitRepositoryId : transformDisguiseVariantRepoId(ev.Value.OutfitRepositoryId);
			auto& reqDisguise = cond.disguise.get();
			auto correctDisguise = false;
			kc.target = target.getID();

			// Target already killed? Confusion. Turn an invalid kill valid, but don't invalidate previously validated kills.
			if (kc.correctMethod == eKillValidationType::Valid) {
				if (!kc.correctDisguise) {
					kc.correctDisguise = reqDisguise.any || (reqDisguise.suit ? ev.Value.OutfitIsHitmanSuit : reqDisguise.repoId == disguiseRepoId);
					validationUpdated = true;
				}
				break;
			}

			kc.correctDisguise = reqDisguise.any || (reqDisguise.suit ? ev.Value.OutfitIsHitmanSuit : reqDisguise.repoId == disguiseRepoId);

			if (!kc.correctDisguise && !reqDisguise.suit) {
				LogDebug("Kill - Invalid disguise '{}' (expected: '{}.').", disguiseRepoId, reqDisguise.repoId);
			}

			if (cond.killComplication == eKillComplication::Live && kc.isPacified) {
				kc.correctMethod = eKillValidationType::Invalid;

				LogDebug("Kill - Invalid kill, target was KO'd on death.", disguiseRepoId, reqDisguise.repoId);
			}
			else {
				if (cond.killMethod.method != eKillMethod::NONE)
					kc.correctMethod = ValidateKillMethod(target.getID(), ev, cond.killMethod.method, cond.killType);
				else if (cond.specificKillMethod.method != eMapKillMethod::NONE)
					kc.correctMethod = ValidateKillMethod(target.getID(), ev, cond.specificKillMethod.method, cond.killType);

				if (kc.correctMethod != eKillValidationType::Valid) {
					LogDebug("Kill - Invalid kill '{}' (type: {})", cond.killMethod.name, static_cast<int>(cond.killType));
					LogDebug("{}", ev.json.dump());
				}
			}

			if (isApexPrey) {
				// If we're in an unspecified target mode, replace invalidations with incompletes
				if (!kc.correctDisguise || kc.correctMethod == eKillValidationType::Invalid) {
					kc.correctMethod = eKillValidationType::Incomplete;
					continue;
				}

				// Fill in the info of the specific target killed
				kc.specificTarget = targetId;
			}

			validationUpdated = true;
		}

		if (validationUpdated) SendKillValidationUpdate();
	});
	events.listen<Events::Level_Setup_Events>([this](const ServerEvent<Events::Level_Setup_Events>& ev) {
		this->SendCustomEvent("Level_Setup_Events"sv, ImbuedPlayerInfo({
			{"Contract_Name_metricvalue", ev.Value.Contract_Name_metricvalue},
			{"Event_metricvalue", ev.Value.Event_metricvalue},
			{"Location_MetricValue", ev.Value.Location_MetricValue},
		}, true));

		auto const& conditions = State::current.spin.getConditions();
		auto mission = State::current.spin.getMission();

		if (State::current.spinCompleted) return;

		LevelSetupEvent data {};
		data.event = ev.Value.Event_metricvalue;
		data.timestamp = ev.Timestamp;
		State::current.levelSetupEvents.push_back(std::move(data));

		if (!mission || mission->getMission() != eMission::HOKKAIDO_SITUSINVERSUS) return;
		if (ev.Value.Contract_Name_metricvalue != "SnowCrane") return;

		bool validationUpdated = false;

		for (auto i = 0; i < conditions.size(); ++i) {
			auto& cond = conditions[i];
			if (cond.target.get().getID() != eTargetID::ErichSoders) continue;

			auto& kc = State::current.getKillConfirmation(i);
			auto& reqDisguise = cond.disguise.get();
			kc.target = cond.target.get().getID();

			validationUpdated = true;

			auto getSodersKillDelay = [](std::string_view kill) -> double {
				if (kill == "Body_Kill") return 4;
				if (kill == "Soder_Electrocuted") return 8;
				if (kill == "Poison_Kill") return 12;
				if (kill == "Spidermachine_Kill") return 13;
				return 0;
			};
			auto getSodersKillTriggerDisguiseChange = [this, getSodersKillDelay](std::string_view kill, double timestamp) -> const DisguiseChange* {
				auto const delay = getSodersKillDelay(kill);
				return State::current.getLastDisguiseChangeAtTimestamp(timestamp - delay);
			};

			auto const triggerDisguiseChange = getSodersKillTriggerDisguiseChange(ev.Value.Event_metricvalue, ev.Timestamp);

			// There should be at least one disguise. If in doubt, trust the player...
			if (!triggerDisguiseChange)
				kc.correctDisguise = true;
			// If we're not looking for a suit, just compare repo IDs
			else if (!reqDisguise.suit)
				kc.correctDisguise = toLowerCase(triggerDisguiseChange->disguiseRepoId) == reqDisguise.repoId;
			// If it is suit, just check the repo ID does not match any non-suit disguises in the level (player-unlocked suit IDs are vast)
			else {
				auto isNotInSuit = false;
				for (auto const& disguise : mission->getDisguises()) {
					if (disguise.suit) continue;
					if (disguise.repoId != triggerDisguiseChange->disguiseRepoId) continue;
					isNotInSuit = true;
					break;
				}

				kc.correctDisguise = !isNotInSuit;
			}

			if (!kc.correctDisguise && !reqDisguise.suit) {
				LogDebug("Kill - Invalid disguise '{}' (expected: '{}')", triggerDisguiseChange->disguiseRepoId, reqDisguise.repoId);
			}

			if (cond.specificKillMethod.method != eMapKillMethod::NONE) {
				if (ev.Value.Event_metricvalue == "Heart_Kill")
					kc.correctMethod = cond.specificKillMethod.method == eMapKillMethod::Soders_TrashHeart
						|| cond.specificKillMethod.method == eMapKillMethod::Soders_ShootHeart
						? eKillValidationType::Valid : eKillValidationType::Invalid;
				else if (ev.Value.Event_metricvalue == "Spidermachine_Kill")
					kc.correctMethod = cond.specificKillMethod.method == eMapKillMethod::Soders_RobotArms ? eKillValidationType::Valid : eKillValidationType::Invalid;
				else if (ev.Value.Event_metricvalue == "Soder_Electrocuted")
					kc.correctMethod = cond.specificKillMethod.method == eMapKillMethod::Soders_Electrocution ? eKillValidationType::Valid : eKillValidationType::Invalid;
				else if (ev.Value.Event_metricvalue == "Poison_Kill")
					kc.correctMethod = cond.specificKillMethod.method == eMapKillMethod::Soders_PoisonStemCells ? eKillValidationType::Valid : eKillValidationType::Invalid;
				else
					validationUpdated = false;
			}
			else if (cond.killMethod.method != eKillMethod::NONE) {
				if (ev.Value.Event_metricvalue == "Body_Kill")
					kc.correctMethod = cond.killMethod.isGun
						|| cond.killMethod.method == eKillMethod::Explosive
						|| cond.killMethod.method == eKillMethod::Explosion
						? eKillValidationType::Valid : eKillValidationType::Invalid;
				else if (ev.Value.Event_metricvalue == "Heart_Kill")
					kc.correctMethod = eKillValidationType::Invalid;
				else if (ev.Value.Event_metricvalue == "Spidermachine_Kill")
					kc.correctMethod = eKillValidationType::Invalid;
				else if (ev.Value.Event_metricvalue == "Soder_Electrocuted")
					kc.correctMethod = cond.killMethod.method == eKillMethod::Electrocution ? eKillValidationType::Valid : eKillValidationType::Invalid;
				else if (ev.Value.Event_metricvalue == "Poison_Kill")
					kc.correctMethod = cond.killMethod.method == eKillMethod::ConsumedPoison ? eKillValidationType::Valid : eKillValidationType::Invalid;
				else
					validationUpdated = false;
			}
			else validationUpdated = false;
		}

		if (validationUpdated) SendKillValidationUpdate();
	});
	events.listen<Events::Crocodile>([this](const ServerEvent<Events::Crocodile>& ev) {
		this->SendCustomEvent("Crocodile"sv, ImbuedPlayerInfo({
			{"RepositoryId", ev.Value.RepositoryId},
		}, true));
	});
	events.listen<Events::setpieces>([this](const ServerEvent<Events::setpieces>& ev) {
		this->SendCustomEvent("setpieces"sv, ImbuedPositionInfo(ev.Value.Position, "", ImbuedPlayerInfo({
			{"RepositoryId", ev.Value.RepositoryId},
			{"Name", ev.Value.name_metricvalue},
			{"Helper", ev.Value.setpieceHelper_metricvalue},
			{"Type", ev.Value.setpieceType_metricvalue},
			{"ToolUsed", ev.Value.toolUsed_metricvalue},
			{"ItemTriggered", ev.Value.Item_triggered_metricvalue},
		}, true)));

		if (State::current.spinCompleted) return;

		KillSetpieceEvent data{};
		data.id = ev.Value.RepositoryId;
		data.name = ev.Value.name_metricvalue;
		data.type = ev.Value.setpieceType_metricvalue;
		data.timestamp = ev.Timestamp;
		State::current.killSetpieceEvents.push_back(std::move(data));
	});

	// SA Tracking
	events.listen<Events::MurderedBodySeen>([this](const ServerEvent<Events::MurderedBodySeen>& ev) {
		if (ev.Value.IsWitnessTarget) return;
		State::current.voidSA();
	});
	events.listen<Events::Spotted>([this](const ServerEvent<Events::Spotted>& ev) {
		for (auto const& id : ev.Value.value) {
			if (!State::current.killed.contains(id))
				State::current.spottedNotKilled.insert(id);
		}
	});
	events.listen<Events::DrainPipe_climbed>([this](const ServerEvent<Events::DrainPipe_climbed>& ev) {
		this->SendCustomEvent("DrainPipeClimbed"sv, ImbuedPlayerInfo());
	});
	events.listen<Events::SecuritySystemRecorder>([this](const ServerEvent<Events::SecuritySystemRecorder>& ev) {
		this->SendCustomEvent("SecuritySystemRecorder"sv, ImbuedPlayerInfo({
			{"Camera", ev.Value.camera},
			{"Event", ev.Value.event},
			{"Recorder", ev.Value.recorder},
		}, true));
		switch (ev.Value.event) {
			case SecuritySystemRecorderEvent::Spotted:
				if (State::current.isCamsDestroyed) return;
				State::current.isCaughtOnCams = true;
				break;
			case SecuritySystemRecorderEvent::Destroyed:
				State::current.isCamsDestroyed = true;
				State::current.isCaughtOnCams = false;
				break;
			case SecuritySystemRecorderEvent::Erased:
				State::current.isCaughtOnCams = false;
				break;
		}
	});
	events.listen<Events::OpportunityEvents>([this](const ServerEvent<Events::OpportunityEvents>& ev) {
		if (State::current.gameMode == GameMode::Roulette) return;
		this->SendCustomEvent("OpportunityEvents"sv, ImbuedPlayerInfo({
			{"RepositoryId", ev.Value.RepositoryId},
			{"Event", ev.Value.Event},
		}));
	});
}

auto CroupierPlugin::ValidateKillMethod(eTargetID target, const ServerEvent<Events::Kill>& ev, eKillMethod method, eKillType type) -> eKillValidationType {
	auto const killType = ev.Value.KillType;
	auto const& killClass = ev.Value.KillClass;
	auto const& killMethodBroad = ev.Value.KillMethodBroad;
	auto const& killMethodStrict = ev.Value.KillMethodStrict;
	auto const killContext = ev.Value.KillContext;
	auto const haveKillItem = !ev.Value.KillItemRepositoryId.empty();
	auto const isKillClassUnknown = killClass == "unknown";
	auto const isSilencedWeapon = ev.Value.WeaponSilenced;
	auto const isAccident = ev.Value.Accident;
	auto const isExplosive = ev.Value.Explosive;
	auto const isSniper = ev.Value.Sniper;
	auto const isProjectile = ev.Value.Projectile;
	auto const haveKillMethod = !ev.Value.KillMethodBroad.empty() || !ev.Value.KillMethodStrict.empty();
	auto const haveDamageEvents = !ev.Value.DamageEvents.empty();

	if (target == eTargetID::SierraKnox) {
		// If expecting injected poison, determine whether the proxy medic opportunity was used
		if (method == eKillMethod::InjectedPoison
			&& killContext == EDeathContext::eDC_ACCIDENT
			&& killClass == "poison"
			&& killMethodStrict == "")
			// EKillType_ItemTakeOutFront (4)
			return eKillValidationType::Valid;

		// true for car kill
		// EKillType_ItemTakeOutFront (4)
		// KillClass == "unknown"
		// killContext == eDC_HIDDEN (2)
		// KillMethodBroad == ""
		// KillMethodStrict == ""
		auto const isContextKill = haveDamageEvents && ev.Value.DamageEvents[0] == "ContextKill";
		if (method == eKillMethod::Explosion
			&& isContextKill
			&& isKillClassUnknown
			&& killContext == EDeathContext::eDC_HIDDEN)
			return eKillValidationType::Valid;
	}

	if (type == eKillType::Silenced && !isSilencedWeapon)
		return eKillValidationType::Invalid;
	if (type == eKillType::Loud && isSilencedWeapon)
		return eKillValidationType::Invalid;

	// ev.Value.KillMethodBroad == "close_combat_pistol_elimination"
	switch (method) {
	case eKillMethod::NeckSnap:
		return killMethodBroad == "unarmed" ? eKillValidationType::Valid : eKillValidationType::Invalid;
	case eKillMethod::Pistol:
		return killMethodBroad == "pistol" || killMethodBroad == "close_combat_pistol_elimination" ? eKillValidationType::Valid : eKillValidationType::Invalid;
	case eKillMethod::PistolElimination:
		return killMethodBroad == "close_combat_pistol_elimination" ? eKillValidationType::Valid : eKillValidationType::Invalid;
	case eKillMethod::SMG:
		if (killMethodBroad == "melee_lethal" && ev.Value.KillItemCategory == "smg") // wtf?
			return eKillValidationType::Valid;
		return killMethodBroad == "smg" ? eKillValidationType::Valid : eKillValidationType::Invalid;
	case eKillMethod::SMGElimination:
		return killMethodBroad == "melee_lethal" && ev.Value.KillItemCategory == "smg" ? eKillValidationType::Valid : eKillValidationType::Invalid;
	case eKillMethod::Shotgun:
		return killMethodBroad == "shotgun" ? eKillValidationType::Valid : eKillValidationType::Invalid;
	case eKillMethod::AssaultRifle:
		return killMethodBroad == "assaultrifle" ? eKillValidationType::Valid : eKillValidationType::Invalid;
	case eKillMethod::Sniper:
		return killMethodBroad == "sniperrifle" ? eKillValidationType::Valid : eKillValidationType::Invalid;
	case eKillMethod::Explosive:
		if (type == eKillType::Any || type == eKillType::Loud || type == eKillType::Impact) {
			// Check for molotov burn kills...
			if (haveDamageEvents
				&& ev.Value.DamageEvents[0] == "Burn"
				&& !haveKillMethod
				&& !isAccident)
				return eKillValidationType::Valid;
		}
		if (type == eKillType::Any || type == eKillType::Loud || type == eKillType::Impact) {
			// Check for deadly lock-on throw kills...
			if (killMethodBroad == "throw"
				&& killClass == "melee"
				&& checkExplosiveKillType(ev.Value.KillItemRepositoryId, type))
				return eKillValidationType::Valid;
		}
		return killMethodBroad == "explosive"
			&& checkExplosiveKillType(ev.Value.KillItemRepositoryId, type)
			? eKillValidationType::Valid : eKillValidationType::Invalid;
	case eKillMethod::FiberWire:
		return killMethodBroad == "fiberwire" ? eKillValidationType::Valid : eKillValidationType::Invalid;
	case eKillMethod::InjectedPoison:
		// Validate medic proxy injected opportunity for Sierra Knox
		if (ev.Value.SetPieceId == "4337d53a-5966-493f-9caa-ca6ec01cb101")
			return eKillValidationType::Valid;
		// Validate ambiguous poisons that aren't "consumed"
		if (killClass == "poison" && killMethodStrict == "")
			// EKillType_ItemTakeOutFront (4)
			return eKillValidationType::Valid;
		return killMethodStrict == "injected_poison" ? eKillValidationType::Valid : eKillValidationType::Invalid;
	case eKillMethod::ConsumedPoison:
		// killItemCategory = poison
		// killItemRepoId = id of poison itemA
		// Validate ambiguous poisons that aren't "injected"
		if (killClass == "poison"
			&& killMethodStrict == "")
			// EKillType_ItemTakeOutFront (4)
			return eKillValidationType::Valid;
		return killMethodStrict == "consumed_poison" ? eKillValidationType::Valid : eKillValidationType::Invalid;
	case eKillMethod::Drowning:
		// ev.Value.KillMethodBroad == "accident"
		return killMethodStrict == "accident_drown" ? eKillValidationType::Valid : eKillValidationType::Invalid;
	case eKillMethod::Explosion:
		return killMethodStrict == "accident_explosion" ? eKillValidationType::Valid : eKillValidationType::Invalid;
	case eKillMethod::Fall:
		// If expecting fall kill and the cause of death is mysterious, we can assume it's correct based on some OOB kill indicators
		if (killContext == EDeathContext::eDC_MURDER
			&& killType == EKillType::EKillType_ItemTakeOutFront
			&& isKillClassUnknown
			&& !haveKillMethod
			&& !haveDamageEvents
			&& !haveKillItem)
			return eKillValidationType::Valid;

		return killMethodStrict == "accident_push" ? eKillValidationType::Valid : eKillValidationType::Invalid;
	case eKillMethod::FallingObject:
		// Special cases: sometimes (possibly for challenge reasons?) the game reports no specific kill method,
		// but does report a setpiece repository ID of the falling object. Check for one of these objects...
		if (killMethodStrict == "") {
			// Only one proven to present this issue, specifically on Wazir Kale (and the FO by the lead actor's puke spot)
			// SetPiece_Mumbai_Falling_Sign_Shop_Electric_B_05
			if (ev.Value.SetPieceId == "701a4dfc-fb62-4702-ac1d-a07188851642")
				return eKillValidationType::Valid;

			// But because of it we need to check every single one...
			// SetPiece_Mumbai_Falling_Sign_Shop_Electric_D_02
			if (ev.Value.SetPieceId == "52837b63-b731-45e5-b220-d6680ac5eb16")
				return eKillValidationType::Valid;
			// SetPiece_Mumbai_Falling_Sign_Shop_Electric_D_03
			if (ev.Value.SetPieceId == "d785c660-6b7a-4804-979b-34921b75c138")
				return eKillValidationType::Valid;
			// SetPiece_Mumbai_Falling_Sign_Shop_Electric_D_08
			if (ev.Value.SetPieceId == "4b19effc-09ae-476c-9124-c811a0f82d51")
				return eKillValidationType::Valid;
			// SetPiece_Mumbai_Falling_Sign_Shop_Electric_B_03
			if (ev.Value.SetPieceId == "9c94f9ed-6083-4c4e-94a3-067dce5db327")
				return eKillValidationType::Valid;
			// SetPiece_Mumbai_Falling_Sign_Shop_Electric_D
			if (ev.Value.SetPieceId == "3d937afc-e4c2-432f-b852-0daf0f73c855")
				return eKillValidationType::Valid;
			// SetPiece_Mumbai_Falling_Sign_Shop_Electric_D_01
			if (ev.Value.SetPieceId == "8e474ae0-699f-44b5-8343-d09eadc9a8af")
				return eKillValidationType::Valid;
			// SetPiece_Mumbai_Falling_Sign_Shop_Electric_D_03
			if (ev.Value.SetPieceId == "f97e7a1d-f188-4bb5-a46d-bc97505c667f")
				return eKillValidationType::Valid;
			// SetPiece_Mumbai_Falling_Sign_Shop_Electric_D_04
			if (ev.Value.SetPieceId == "82864825-624e-40a4-9b17-0d51a7aa663d")
				return eKillValidationType::Valid;
			// SetPiece_Mumbai_Falling_Sign_Shop_Electric_D_05
			if (ev.Value.SetPieceId == "205dfccf-c187-4867-890e-0a3f3856ed09")
				return eKillValidationType::Valid;
			// SetPiece_Mumbai_Falling_Sign_Shop_Electric_D_06
			if (ev.Value.SetPieceId == "ed5d28f9-70b7-4460-b0db-8d1e0f3970e4")
				return eKillValidationType::Valid;
			// SetPiece_Mumbai_Falling_Sign_Shop_Electric_B_04
			if (ev.Value.SetPieceId == "46d62cd5-6b7a-4ef1-b284-2e06391197d3")
				return eKillValidationType::Valid;
			// SetPiece_Mumbai_Falling_Sign_Shop_Electric_D_06
			if (ev.Value.SetPieceId == "379402c3-0f48-440a-bb0c-e6d70ae16e77")
				return eKillValidationType::Valid;
			// SetPiece_Mumbai_Falling_Sign_Shop_Electric_D_03
			if (ev.Value.SetPieceId == "1d5c45af-ef8a-45f2-aab2-262e337f2584")
				return eKillValidationType::Valid;
			// SetPiece_Mumbai_Falling_Sign_Shop_Electric_B_05
			if (ev.Value.SetPieceId == "27ad6d30-1587-4411-8507-17c19b311c9e")
				return eKillValidationType::Valid;
			// SetPiece_Mumbai_Falling_Sign_Shop_Electric_B_04
			if (ev.Value.SetPieceId == "3bf4a4c5-be0a-423a-b34b-fe29602ac499")
				return eKillValidationType::Valid;
			// SetPiece_Mumbai_Falling_Sign_Shop_Electric_B_05
			if (ev.Value.SetPieceId == "e237df91-9ea7-4c96-b711-d29d11b70a73")
				return eKillValidationType::Valid;
			// SetPiece_Mumbai_Falling_Sign_Shop_Electric_D_03
			if (ev.Value.SetPieceId == "d0eb2ff6-d95a-48b5-816c-394cadc7e3e5")
				return eKillValidationType::Valid;
			// SetPiece_Mumbai_Falling_Sign_Shop_Electric_D_04
			if (ev.Value.SetPieceId == "6ea6dc37-beb1-465c-aa67-706be152b137")
				return eKillValidationType::Valid;
			// SetPiece_Mumbai_Falling_Sign_Shop_Electric_D_04
			if (ev.Value.SetPieceId == "98be2403-5d97-4eea-840f-876adaa098c4")
				return eKillValidationType::Valid;
		}
		return killMethodStrict == "accident_suspended_object" ? eKillValidationType::Valid : eKillValidationType::Invalid;
	case eKillMethod::Fire:
		{
			// Validate incinerator fire kills (this may also pass for garden shredder kills)
			auto fireSetpieceEv = State::current.getSetpieceEventAtTimestamp(ev.Timestamp);
			if (killMethodStrict == ""
				&& killType == EKillType::EKillType_ItemTakeOutFront
				&& isKillClassUnknown
				&& !haveKillMethod
				&& !haveKillItem
				&& !ev.Value.DamageEvents.empty()
				&& std::find(ev.Value.DamageEvents.cbegin(), ev.Value.DamageEvents.cend(), "InCloset") != ev.Value.DamageEvents.cend()
				&& fireSetpieceEv
				&& isIncineratorSetpiece(fireSetpieceEv->id)
				&& fireSetpieceEv->name == "BodyFlushed")
				return eKillValidationType::Valid;
		}
		// Handle typical fire kills.
		return killMethodStrict == "accident_burn" ? eKillValidationType::Valid : eKillValidationType::Invalid;
	case eKillMethod::Electrocution:
		return killMethodStrict == "accident_electric" ? eKillValidationType::Valid : eKillValidationType::Invalid;
	}
	return eKillValidationType::Unknown;
}

auto CroupierPlugin::ValidateKillMethod(eTargetID target, const ServerEvent<Events::Kill>& ev, eMapKillMethod method, eKillType type) -> eKillValidationType {
	if (target == eTargetID::SilvioCaruso) {
		// {"Timestamp":173.116608,"Name":"Kill","ContractSessionId":"2516591008337813079-9ea716b6-6798-4687-ba22-3bb8d89cce9b","ContractId":"00000000-0000-0000-0000-000000000600","Value":{"RepositoryId":"0dfaea51-3c36-4722-9eff-f1e7ef139878","ActorId":2739847461.000000,"ActorName":"Silvio Caruso","ActorType":0.000000,"KillType":4.000000,"KillContext":3.000000,"KillClass":"unknown","Accident":true,"WeaponSilenced":false,"Explosive":false,"ExplosionType":0.000000,"Projectile":false,"Sniper":false,"IsHeadshot":false,"IsTarget":true,"ThroughWall":false,"BodyPartId":-1.000000,"TotalDamage":100000.000000,"IsMoving":false,"RoomId":182.000000,"ActorPosition":"-105.703, -175.75, -0.775244","HeroPosition":"-110.366, -119.28, 16.0837","DamageEvents":[],"PlayerId":4294967295.000000,"OutfitRepositoryId":"fd56a934-f402-4b52-bdca-8bbc737400ff","OutfitIsHitmanSuit":false,"EvergreenRarity":-1.000000,"KillMethodBroad":"","KillMethodStrict":"","IsReplicated":true,"History":[]},"UserId":"b1585b4d-36f0-48a0-8ffa-1b72f01759da","SessionId":"61e82efa0bcb4a3088825dd75e115f61-468215834","Origin":"gameclient","Id":"c5d04012-68a1-473a-8769-3a0c3b9da097"}
		if (method == eMapKillMethod::Silvio_SeaPlane) {
			// Best we can really do is just check Silvio ever entered the plane
			// and the kill was an accident (it is possible to kill him directly with explosive while he is in the plane).
			auto lse = State::current.getLevelSetupEventByEvent("Silvio_InPlane");
			return lse != nullptr && ev.Value.Accident
				? eKillValidationType::Valid
				: eKillValidationType::Invalid;
		}
		if (method == eMapKillMethod::Silvio_ShootThroughTelescope) {
			return ev.Value.SetPieceId == "a84ba351-285a-4f07-8758-2d7640401aad"
				? eKillValidationType::Valid
				: eKillValidationType::Invalid;
		}
	}
	if (target == eTargetID::JordanCross) {
		if (method == eMapKillMethod::Jordan_CakeSmother) {
			return ev.Value.SetPieceId == "be8452d0-3ce9-4f41-b1c2-a381d7e95e15"
				? eKillValidationType::Valid
				: eKillValidationType::Invalid;
		}
	}
	if (target == eTargetID::SeanRose) {
		if (method == eMapKillMethod::Sean_ExplosiveWatchBattery) {
			return ev.Value.SetPieceId == "66d7a0d3-7ee8-4065-9475-8765fca06faa"
				? eKillValidationType::Valid
				: eKillValidationType::Invalid;
		}
		if (method == eMapKillMethod::Sean_OverpoweredNitro) {
			return ev.Value.SetPieceId == "f22c3477-996d-4cfd-88cd-50301bdcb3fb"
				? eKillValidationType::Valid
				: eKillValidationType::Invalid;
		}
	}
	if (target == eTargetID::YukiYamazaki) {
		if (method == eMapKillMethod::Yuki_Sauna) {
			return ev.Value.SetPieceId == "9477e941-880c-4b05-932f-d431eaeb634e"
				? eKillValidationType::Valid
				: eKillValidationType::Invalid;
		}
		if (method == eMapKillMethod::Yuki_SabotageCableCar) {
			auto lse = State::current.getLevelSetupEventByEvent("Cablecar_Down");
			return lse != nullptr
				? eKillValidationType::Valid
				: eKillValidationType::Invalid;
		}

		// Yoga: 40237d59-f3c8-46a7-9760-49eac05315d6
	}
	if (target == eTargetID::RobertKnox) {
		// Push On Track: b6d26119-db90-4224-b50a-dcb04c3e159d
	}
	if (target == eTargetID::SierraKnox) {
		auto const haveDamageEvents = !ev.Value.DamageEvents.empty();
		auto const killContext = ev.Value.KillContext;
		auto const& killClass = ev.Value.KillClass;
		auto const& killMethodStrict = ev.Value.KillMethodStrict;
		auto const isKillClassUnknown = killClass == "unknown";
		// If expecting injected poison, determine whether the proxy medic opportunity was used
		if (method == eMapKillMethod::Sierra_PoisonIVDrip) {
			return ev.Value.SetPieceId == "4337d53a-5966-493f-9caa-ca6ec01cb101"
				? eKillValidationType::Valid
				: eKillValidationType::Invalid;
		}
		// true for car kill
		// EKillType_ItemTakeOutFront (4)
		// KillClass == "unknown"
		// killContext == eDC_HIDDEN (2)
		// KillMethodBroad == ""
		// KillMethodStrict == ""
		auto const isContextKill = haveDamageEvents && ev.Value.DamageEvents[0] == "ContextKill";
		if ((method == eMapKillMethod::Sierra_BombCar || method == eMapKillMethod::Sierra_ShootCar)
			&& isContextKill
			&& isKillClassUnknown
			&& killContext == EDeathContext::eDC_HIDDEN)
			return eKillValidationType::Valid;
	}
	if (target == eTargetID::RicoDelgado) {
		if (method == eMapKillMethod::Rico_FeedToHippo) {
			// Method should already be validated by a separate event for dumping into enclosure.
			// The following is specifically for the context kill push with cutscene.
			return ev.Value.SetPieceId == "41f35d49-c74a-4de2-8119-d11cfef0b408"
				? eKillValidationType::Valid
				: eKillValidationType::Invalid;
		}
	}
	if (target == eTargetID::JorgeFranco) {
		if (method == eMapKillMethod::Jorge_CocaineMachine) {
			// Context kill with cutscene.
			if (ev.Value.SetPieceId == "803b6461-0c4c-4f3d-9d6a-d9219a9d3136")
				return eKillValidationType::Valid;
			// For KO and dump, it should be sufficient to check the kill happened inside a container.
			auto const it = std::find(ev.Value.DamageEvents.cbegin(), ev.Value.DamageEvents.cend(), "InCloset");
			return it != ev.Value.DamageEvents.cend()
				? eKillValidationType::Valid
				: eKillValidationType::Invalid;
		}
		//if (method == eMapKillMethod::RarePlant) {
		//	return ev.Value.SetPieceId == "eff668ff-341b-4a9d-850f-14c3b05bb1f7"
		//		? eKillValidationType::Valid
		//		: eKillValidationType::Invalid;
		//}
	}
	if (target == eTargetID::VanyaShah) {
		if (method == eMapKillMethod::Vanya_SteamPool) {
			return ev.Value.SetPieceId == "36744d6c-77e9-429a-98d6-8cfc1b93454f"
				? eKillValidationType::Valid
				: eKillValidationType::Invalid;
		}
	}
	if (target == eTargetID::AndreaMartinez) {
		//if (method == eMapKillMethod::PiranhaFood) {
		//	return ev.Value.SetPieceId == "7e9e7387-8c6a-4782-bb2a-cfc7c3574895"
		//		? eKillValidationType::Valid
		//		: eKillValidationType::Invalid;
		//}
	}
	if (target == eTargetID::Janus) {
		if (method == eMapKillMethod::Janus_Sculpture) {
			return ev.Value.SetPieceId == "2258f06a-76d0-49a1-ba01-b34d894760bf"
				? eKillValidationType::Valid
				: eKillValidationType::Invalid;
		}
	}
	if (target == eTargetID::AthenaSavalas) {
		if (method == eMapKillMethod::Athena_Award) {
			// setpiece_raccoon_unique.template -> SetpieceHelpers_ContextKill_CustomSequence2
			return ev.Value.SetPieceId == "1a29d28c-be03-4149-b49c-b0c38d060772"
				? eKillValidationType::Valid
				: eKillValidationType::Invalid;
		}
	}
	if (target == eTargetID::StevenBradley) {
		if (method == eMapKillMethod::Steven_BombWaterScooter) {
			// 'Legit' water scooter kill
			// setpiece_stingray_unique.template -> Setpiece_Trap_WaterScooterRide
			if (ev.Value.SetPieceId == "0bd4c163-9674-403a-aa3d-a714be3d7a09")
				return eKillValidationType::Valid;
			// Accident explosion via water scooter
			// Technically this can validate even if Steven is killed in an unrelated accident explosion and the scooter
			// also gets blown up around the same time, but fuck it
			auto const setpiece = State::current.getSetpieceEventAtTimestamp(ev.Timestamp, 0.3);
			return setpiece != nullptr && setpiece->id == "2f4a7b8f-a5f1-4c59-8a0e-678b3c2ee32f"
				? ValidateKillMethod(target, ev, eKillMethod::Explosion, type)
				: eKillValidationType::Invalid;
		}
	}
	if (target == eTargetID::AlexaCarlisle) {
		if (method == eMapKillMethod::Alexa_PillowSmother) {
			return ev.Value.SetPieceId == "4b67e04a-c4b8-49b3-9ef1-e6ee070e2393"
				? eKillValidationType::Valid
				: eKillValidationType::Invalid;
		}
	}

	if (!ev.Value.KillItemRepositoryId.empty()) {
		if (type == eKillType::Thrown && ev.Value.KillMethodBroad != "throw") {
			LogDebug("Kill validation failed. Expected 'throw', got '{}'.", ev.Value.KillMethodBroad);
			LogDebug("{}", ev.json.dump());
			return eKillValidationType::Invalid;
		}
		if (type == eKillType::Melee && ev.Value.KillMethodBroad != "melee_lethal") {
			LogDebug("Kill validation failed. Expected 'melee_lethal', got '{}'.", ev.Value.KillMethodBroad);
			LogDebug("{}", ev.json.dump());
			return eKillValidationType::Invalid;
		}

		auto it = specificKillMethodsByRepoId.find(ev.Value.KillItemRepositoryId);
		if (it != end(specificKillMethodsByRepoId) && it->second == method) {
			return eKillValidationType::Valid;
		}

		if (it == end(specificKillMethodsByRepoId))
			LogDebug("Invalid kill '{}'. Repo ID unknown.", ev.Value.KillItemRepositoryId);
		else
			LogDebug("Invalid kill '{}'. Repo ID kill method mismatch (expected {}, got {}).", ev.Value.KillItemRepositoryId, static_cast<int>(method), static_cast<int>(it->second));
		LogDebug("{}", ev.json.dump());
	}
	return eKillValidationType::Invalid;
}

DEFINE_PLUGIN_DETOUR(CroupierPlugin, void*, OnLoadingScreenActivated, void* th, void* a1) {
	loadingScreenActivated = true;
	if (!loadRemovalActive) {
		SendLoadStarted();
		loadRemovalActive = true;
	}
	return HookResult<void*>(HookAction::Continue());
}

DEFINE_PLUGIN_DETOUR(CroupierPlugin, void, OnEventReceived, ZAchievementManagerSimple* th, const SOnlineEvent& event) {
	return HookResult<void>(HookAction::Continue());
}

DEFINE_PLUGIN_DETOUR(CroupierPlugin, void, OnClearScene, ZEntitySceneContext* th, bool p_FullyUnloadScene) {
	m_NavMesh = {};
	return HookResult<void>(HookAction::Continue());
}

// Wrapper for Functions::ZDynamicObject_ToString that attempts to prevent invalid JSON output.
static auto ZDynamicObjectToString(ZDynamicObject& obj) -> ZString {
	// Handle the main object structure so we can invoke ourselves for individual entries.
	if (obj.Is<TArray<SDynamicObjectKeyValuePair>>()) {
		auto arr = obj.As<TArray<SDynamicObjectKeyValuePair>>();
		auto first = true;
		auto str = std::ostringstream();
		str << "{";

		for (auto& entry : *arr) {
			if (!first) str << ",";
			first = false;

			auto objStr = ZDynamicObjectToString(entry.value);

			// Key should never contain quotes but we'll do quoted to be on the safe side + it's neater.
			str << std::quoted(entry.sKey.c_str()) << ":" << objStr.c_str();
		}

		str << "}";
		return ZString(str.str());
	}

	if (obj.Is<ZString>()) {
		// Remove null terminator from strings.
		auto res = obj.As<ZString>();
		auto resSV = std::string_view(res->c_str(), res->size());
		auto fixedStr = std::string(resSV.size(), '\0');
		std::remove_copy(resSV.cbegin(), resSV.cend(), fixedStr.begin(), '\n');

		// Quote the string.
		return (std::ostringstream() << std::quoted(fixedStr.c_str())).str();
	}

	if (obj.Is<SVector3>()) {
		auto res = obj.As<SVector3>();
		return (std::ostringstream() << "[" << res->x << "," << res->y << "," << res->z << "]").str();
	}

	// Use the game method for anything we don't need to handle.
	ZString res;
	Functions::ZDynamicObject_ToString->Call(const_cast<ZDynamicObject*>(&obj), res);
	return res;
}

static std::set<std::string> eventsNotToPrint = {
	// Map-specific Perma Shortcut Events
	"Bulldog_Ladder_A_Open",
	"Bulldog_Ladder_B_Open",
	"Dugong_Ladder_A_Down",
	"Dugong_Ladder_B_Down",
	"Edgy_Ladder_A_Down",
	"Gecko_Ladder_A_Down",
	"Gecko_Ladder_B_Down",
	"Gecko_Ladder_C_Down",
	"Rat_Ladder_A_Open",
	// Freelancer Objectives
	"Activate_BlindGuard",
	"Activate_BlindTarget",
	"Activate_Camera_Caught",
	"Activate_Camera_DestroyRecorder",
	"Activate_DartGun_Target",
	"Activate_DisguiseBlown",
	"Activate_Distract_Target",
	"Activate_DontTakeDamage",
	"Activate_EliminationPayout",
	"Activate_HideTargetBodies",
	"Activate_KillGuard_Sniper",
	"Activate_KillGuard_SubMachineGun",
	"Activate_KillMethod_Poison",
	"Activate_KillMethod_Sniper",
	"Activate_KillMethod_UnSilenced_Pistol",
	"Activate_LimitedDisguise",
	"Activate_No_Firearms",
	"Activate_No_Witnesses",
	"Activate_NoCombat",
	"Activate_NoMissedShots",
	"Activate_NoBodyFound",
	"Activate_NotSpotted",
	"Activate_PacifyGuard_Explosive",
	"Activate_PoisonGuard_Any",
	"Activate_PoisonGuard_Syringe",
	"Activate_PoisonTarget_Emetic",
	"Activate_PoisonTarget_Sedative",
	"Activate_SA",
	"Activate_SASO",
	"Activate_SilentTakedown_3",
	"Activate_Timed_SilientTakedown",
	"DrActivate_EliminationPayout",
	// Misc. Freelancer Events
	"AddAssassin_Event",
	"AddLookout_Event",
	"AddSuspectGlow",
	"CompleteEvergreenPrimaryObj",
	"Evergreen_EvaluateChallenge",
	"Evergreen_Mastery_Level",
	"Evergreen_Merces_Data",
	"Evergreen_MissionCompleted_Hot",
	"Evergreen_MissionPayout",
	"Evergreen_Payout_Data",
	"Evergreen_Safehouse_Stash_ItemChosen",
	"Evergreen_SecurityCameraDestroyed",
	"Evergreen_Stash_ItemChosen",
	"Evergreen_Suspect_Looks",
	"EvergreenExitTriggered",
	"EvergreenExitTriggeredOrWounded",
	"EvergreenMissionEnd",
	"GearSlotsTotal",
	"GearSlotsTutorialised",
	"GearSlotsUsed",
	"MildMissionCompleted_Africa_Event",
	"MildMissionCompleted_Asia_Event",
	"MildMissionCompleted_Event",
	"MissionCompleted_Event",
	"NoTargetsLeft",
	"NumberOfTargets",
	"PayoutObjective_Completed",
	"ScoringScreenEndState_CampaignCompletedBonusXP_Professional",
	"ScoringScreenEndState_CampaignCompletedBonusXP_Hard",
	"ScoringScreenEndState_MildCompleted",
	"SetPayout",
	"Setup_TargetName",
	"TravelDestination",
	"Leader_In_Meeting",
	"LeaderDeadEscaping_Event",
	"LeaderEscaping",
	"LeaderPacifiedEscaping_Event",
	"RemoveSuspectGlow",
	"SupplierVisited",
	"TargetPickedConfirm",
	// Freelancer Challenge Events
	"CollectorUpdate",
	"GunmasterComplete",
	"GunslingerUpdate",
	"LetsGoHuntingUpdate",
	"OneShotOneKillUpdate",
	"SprayAndPrayUpdate",
	"ThisIsMyRifleUpdate",
	"UpCloseAndPersonalUpdate",
	// Gameplay Events
	"AccidentBodyFound",
	//"Actorsick",
	"Agility_Start",
	"AllBodiesHidden",
	"AmbientChanged",
	"BlueEgg",
	"BodyBagged",
	"BodyFound",
	"BodyHidden",
	"Dart_Hit",
	"DeadBodySeen",
	//"Disguise",
	"Door_Unlocked",
	//"DrainPipe_climbed",
	"Drain_Pipe_Climbed",
	"EvidenceHidden",
	"ExitInventory",
	"FirstMissedShot",
	"FirstNonHeadshot",
	"HeroSpawn_Location",
	"HoldingIllegalWeapon",
#ifndef _DEBUG
	"Investigate_Curious",
#endif
	"ItemDropped",
#ifndef _DEBUG
	"ItemPickedUp",
#endif
	"ItemRemovedFromInventory",
	"ItemThrown",
	"MurderedBodySeen",
	"Noticed_Pacified",
	"NPC_Distracted",
	"ObjectiveCompleted",
	"OpportunityEvents",
	"OpportunityStageEvent",
	"PlayingPianoChanged",
	"SecuritySystemRecorder",
	"SituationContained",
	"StartingSuit",
	//"Trespassing",
	"Unnoticed_Pacified",
	"Unnoticed_Kill",
	"VirusDestroyed",
	"Witnesses",
	// Misc. Events
	"ChallengeCompleted",
	"ContractSessionMarker",
	"CpdSet",
	"Hero_Health",
	"LeaderboardUpdated",
	"Progression_XPGain",
	"SegmentClosing",
	"StartCpd",
};

DEFINE_PLUGIN_DETOUR(CroupierPlugin, void, OnEventSent, ZAchievementManagerSimple* th, uint32_t eventIndex, const ZDynamicObject& ev) {
	ZString eventData = ZDynamicObjectToString(const_cast<ZDynamicObject&>(ev));

	try {
		auto json = json::parse(eventData.c_str(), eventData.c_str() + eventData.size());
		auto const eventName = json.value("Name", "");
		auto const dontPrint = eventsNotToPrint.contains(eventName);

		if (!dontPrint)
			LogDebug("Croupier: event {}", eventData);

		this->events.handle(eventName, json);
		//if (!eventsNotToSend.contains(eventName))
			//State::current.client.sendRaw(eventData.c_str());
	}
	catch (const json::exception& ex) {
		Logger::Info("Error handling event: {}", eventData);
		Logger::Error("{}", eventData);
		Logger::Error("JSON exception: {}", ex.what());
	}

	return HookAction::Continue();
}

// Entity IDs that fire generic pins too frequently to healthily process and send as events
std::unordered_set<uint64_t> spamEntityIDs = {
	0x62B5D8255EF6D149, // Access_Helper_DoorLogic > SignalBranch
	0x683B506EE078582B, // MusicCore_Collector > DynamicMix_Rules > SignalFork_Void01
	0x14018BCD0D7FA2B3, // SetPrioLevel > CheckPriorityLevel > SignalBranch
	0x5B6BA06DE5EC47E, // SetPiece_Surveillance_Camera_A > SignalFork_Void
	0x683CCD34F71FC411, // Prop_Gadget_Camera_Runtime > ActivateTakePhotoPrompt_RightHand
	0x78C75547EC293E6E, // MusicCore_Rules > Lack_of_Activity > SignalFork_Void
	0x7E59C88E50D89C6C, // Global_Profile > Metrics > SignalFork_Void01
	0x9EB2067308F5B46D,
	0xBF207AE81912A58C, // VR
	0xD0F8367928F63692, // VR
	0x516133948EDCE41, // VR ("Get nearest hand")
	0xB03F64F5237856F5, // VR
	0x74C6A0767DC97147, // VR
	0x60DA0634E9480F5B, // VR
	0x26B9274FFD16753E, // VR
	0x8106964EBA968ACA,
	0xA4302EE275C8CA46, // FX_Logic_Fan > SignalFork_Void
	0xBA677C421CD3C543, // FX_Logic_Fan > SignalFork_Void01
	0xBB95A1B76368A952, // OpportunityEvents > EventName_Triggered > SignalFork_Void
	0xC1496F95D5B8A66D, // Poison_sick_metric01 > RepositoryItemEvent > Signal_Ordering
	0x51A07D2CED7C4C50, // VR
	0x3753F93A2653A23, // (VR) PSVR2_HMD_HeadsetVibration > SignalFork_Void_01
	0x3A12A4AAC7F3BF84, // SetpieceHelper_ConsumeItem > SignalFork_Void
	0xAB906F906DA3C114, // Act_MR_Sit_Smoke_ShishaPipe > SignalFork_Void
	0x56426EDA9C0F9909, // Act_FR_Sit_Smoke_ShishaPipe > SignalFork_Void
	0x4ADCE53A74A8DA0F, // Act_MR_Stand_Drink_Coffee_75cm > SignalFork_Void
	0xDE88C67EB34F84DB, // Act_MR_Stand_Drink_Coffee_100cm > SignalFork_Void02
	0x86276B9665DA1E7D, // Act_MR_Stand_Drink_Coffee_100cm > SignalFork_Void01
	0xCABD37AE3958B992, // Act_MR_Stand_Drink_Coffee_100cm > SignalFork_Void
	0x410233FA82E77F6, // Act_MR_Stand_Drink_Glass_Right_100cm > SignalFork_Void
	0xA7F0E038C82852F3, // ActHelper_Drink > SignalFork_Void
	0x2B4CBFFF142FF626, // ActHelper_Drink > GetRidOfCup
	0x57470EAE48E802C5, // FX_Logic_Fan > SignalFork_Void02
	0xBB48DB1CEC6D6463, // MusicCore_CustomPlugin_IntroPartA/MusicCore_CustomPlugin_IntroPartB > SignalFork_Void
	0xD2B5D4722EBC312D, // EventName_StageActive > SignalFork_Void
	0xE9CA2823EE7DE13B, // Opportunity Stage Events > SignalFork_Void
	0x38727C56CB2341B8, // FX_Logic_ShotActivate > SignalFork_Void
	0x7F60A2063D781546, // Chandelier_Chain_A > SignalFork_Void
	0x5D17A3B44240C85F, // LD_AreaDiscoveredMapTracker > Area Discovered
	0x43F4C2BB1B13DB7B, // ControllerSplashHintsVisibilityController > Show
	0x45FE9BCCC264A093, // AITensionEvent > SignalFork_Int
	0xD05DE6BA45F0EE34, // TensionUpdater_Ambient > SignalFork_Void
	0xA1094241852D96A7, // TensionUpdater_Ambient > SignalFork_Void
	0x9C37D5CEB5DED392, // GameTensionEmitter_Sound > SignalBranch
	0xFB052BC2FD70AFD3, // sound_caralarms > SignalFork_Void
	0x16A6E946B95892FA, // WwiseMusic_Core_v3 > SignalFork_Void
	0x7FF95D8031D40635, // SetPieceHelpers_ItemContainer > SignalFork_Void
	0x11B28987E39C169, // TV_Security_Screen_A_00 > SignalFork_Void01
	0x9743B7D1081F013B, // TV_Security_Screen_B_00 > SignalFork_Void01
	0xDE770DF49DD4C178, // TV_Security_Screen_B_00 > SignalFork_Void02
	0x3798A3E66E60EEB, // ActHelper_ItemInteract_Default > SignalFork_Void
	0x20ED9EA75E730AC1, // Completed - ActHelper_Generic_Dialog > ActorSpeak_SoundDef
	0x350134EBE01C884D, // Started - Effects_Videos > RenderVideoPlayer_Demonstration
	0x7BFB2A2D28DF1F5E, // Started - BusControl > IntroSeq
	0xD63AC16CD1C66F50, // ControllerSplashHintsVisibilityController > Hide
	0x5DE29D6104122E3B, // HeroSpawnSequenceDirector > SkipToEndThenStop
	0xBF1EB5AB1940BD3E, // ModalDialoguePopup > SignalFork_Void
	0x71D44BF63414B281, // ModalDialoguePopup > SignalFork_Void_01
	0x5FDEC67D7600D2DA, // GameEvents > SignalFork_Void
	0x7B173F0ECBFB2B63, // School_Swing_A_01 > SignalFork_Void
	0x3E8E90A19716F05E, // FX_E_MuzzleFlash_ChamberSmoke > SignalFork_Void
	0x5BADBA7291B6F2A2, // Play Inhale > ActorSpeak_SoundDef
	0x7034AACD8EC387BC, // Act_MR_Stand_CheckGunParts_100cm > SignalFork_Void
	0x384A412D45B9F80B, // Act_MR_Stand_CheckGunParts_100cm > SignalFork_Void01
	0xAB28943253E48BC3, // Act_MR_Stand_CheckGunParts_100cm > SignalFork_Void02
	0xCDF6914E9651412A, // Act_MR_Stand_CheckGunParts_100cm > SignalFork_Void03
};

auto CroupierPlugin::SetupPins() -> void {
	this->AddPinListener(static_cast<ZHMPin>(1940849038), [this](ZEntityRef entity, const ZObjectRef& data, ZHMPin pin) {
		auto setpiece = entity.GetLogicalParent();
		if (!setpiece) return;

		auto spatial = setpiece.QueryInterface<ZSpatialEntity>();
		if (!spatial) return;

		//entity.GetProperty<bool>("m_bIsIllegal");
		//entity.GetProperty<bool>("m_bIsSevere");
		//entity.GetProperty<bool>("m_bIsLargeScale");
		//auto promptDescriptionOff = entity.GetProperty<TResourcePtr>("Offm_sPromptDescriptionText");
		//auto promptDescriptionResourceOff = entity.GetProperty<TResourcePtr>("Offm_rPromptDescriptionTextResource");
		//auto promptDescriptionOn = entity.GetProperty<ZString>("ONm_sPromptDescriptionText");
		//if (!promptDescriptionOn.IsEmpty())
		//	obj.merge_patch({ {"PromptOnText", promptDescriptionOn.Get()}});
		//auto promptDescriptionResourceOn = entity.GetProperty<TResourcePtr>("Onm_rPromptDescriptionTextResource");
		//if (!promptDescriptionResourceOn.IsEmpty())
		//	obj.merge_patch({ {"PromptOnTextResource", promptDescriptionResourceOn.Get()}});
		SendCustomEvent("OnSabotageSetpiece"sv, ImbuedPlayerInfo(ImbuedSetepieceInfo(entity, {}), true));
	});

	this->AddPinListener(ZHMPin::OnHitInfo, [this](ZEntityRef entity, const ZObjectRef& data, ZHMPin pin) {
		// <SHitInfo> ZHM5ShotListenerEntity
		auto setpiece = entity.GetLogicalParent();
		if (!setpiece) return;

		auto const& info = data.As<SHitInfo>();
		if (!info->m_rHitEntity) return;

		SendCustomEvent("OnShot"sv, ImbuedPlayerInfo(ImbuedSetepieceInfo(info->m_rHitEntity), true));
	});

	this->AddPinListener(ZHMPin::OnHitByImpulseInfo, [this](ZEntityRef entity, const ZObjectRef& data, ZHMPin pin) {
		// <SHitInfo> ZMassImpulseListenerEntity
		auto setpiece = entity.GetLogicalParent();
		if (!setpiece) return;

		auto spatial = setpiece.QueryInterface<ZSpatialEntity>();
		if (!spatial) return;

		auto const& info = data.As<SHitInfo>();
		if (!info->m_rHitEntity) return;

		SendCustomEvent("OnHitByImpulse"sv, ImbuedPlayerInfo(ImbuedSetepieceInfo(info->m_rHitEntity), true));
	});

	this->AddPinListener(ZHMPin::ChangedDisguise, [this](ZEntityRef entity, const ZObjectRef& data, ZHMPin pin) {
		// ZActorOutfitListener
		if (!gameplay.disguiseChange.havePinData) {
			gameplay.disguiseChange.havePinData = true;
			gameplay.disguiseChange.wasFree = true;
		}
	});

	this->AddPinListener(ZHMPin::OutfitTaken, [this](ZEntityRef entity, const ZObjectRef& data, ZHMPin pin) {
		// ZActorOutfitListener
		gameplay.disguiseChange.havePinData = true;
		gameplay.disguiseChange.wasFree = false;
	});

	this->AddPinListener(ZHMPin::BundleDestroyed, [this](ZEntityRef entity, const ZObjectRef& data, ZHMPin pin) {
		// ZClothBundleSpawnEntity
		gameplay.disguiseChange.havePinData = true;
		gameplay.disguiseChange.wasFree = true;
		if (gameplay.disguiseChange.haveEventData)
			this->SendCustomEvent("Disguise"sv, ImbueDisguiseEvent(gameplay.disguiseChange.eventData));
	});

	this->AddPinListener(ZHMPin::HMMovementIndex, [this](ZEntityRef entity, const ZObjectRef& data, ZHMPin pin) {
		auto moveIdx = data.As<int32>();
		if (!moveIdx) return;
		auto moveType = static_cast<PlayerMoveType>(*moveIdx);
		if (State::current.playerMoveType != moveType) {
			State::current.playerMoveType = moveType;
			SendCustomEvent("Movement"sv, ImbuedPlayerInfo());
		}
	});

	this->AddPinListener(ZHMPin::OnAttachToHitman, [this](ZEntityRef entity, const ZObjectRef& data, ZHMPin pin) {
		// ZHM5Item, ZHM5ItemCCWeapon, ZEntity // accomodates coins etc.
		SendCustomEvent("OnAttachToHitman"sv, ImbuedPlayerInfo(ImbuedItemInfo(entity), true));
	});

	this->AddPinListener(ZHMPin::DoorBroken, [this](ZEntityRef entity, const ZObjectRef& data, ZHMPin pin) {
		SendCustomEvent("DoorBroken"sv, ImbuedPlayerInfo());
	});

	this->AddPinListener(ZHMPin::OnIsFullyInCrowd, [this](ZEntityRef entity, const ZObjectRef& data, ZHMPin pin) {
		SendCustomEvent("OnIsFullyInCrowd"sv, ImbuedPlayerInfo());
	});

	this->AddPinListener(ZHMPin::OnIsFullyInVegetation, [this](ZEntityRef entity, const ZObjectRef& data, ZHMPin pin) {
		SendCustomEvent("OnIsFullyInVegetation"sv, ImbuedPlayerInfo());
	});

	auto onTakeDamage = [this](ZEntityRef entity, const ZObjectRef& data, ZHMPin pin) {
		SendCustomEvent("OnTakeDamage"sv, ImbuedPlayerInfo());
	};
	this->AddPinListener(ZHMPin::TakeDamage, onTakeDamage);
	this->AddPinListener(ZHMPin::OnTakeDamage, onTakeDamage);

	this->AddPinListener(ZHMPin::OnPickup, [this](ZEntityRef entity, const ZObjectRef& data, ZHMPin pin) {
		// ZItemSpawner pickups
		SendCustomEvent("OnPickup"sv, ImbuedPlayerInfo(ImbuedItemInfo(entity), true));
	});

	this->AddPinListener(ZHMPin::Explode, [this](ZEntityRef entity, const ZObjectRef& data, ZHMPin pin) {
		// We should have Vehicle_Core.Explode which is a ZEntity, the Vehicle_Core should be a ZCompositeEntity
		if (!entity.m_pObj) return;
		auto parent = entity.GetLogicalParent();
		if (!parent || !parent.GetEntity()) return;
		auto owner = entity.GetLogicalParent();
		if (!owner || !owner.GetEntity() || !owner->GetType()) return;

		// Vehicle_Core should have a Car_Size_Int prop
		auto res = parent.GetProperty<int32>("Car_Size_Int");	
		if (res.IsEmpty()) return;

		auto json = json::object({
			{"CarSize", res.Get()},
			{"EntityID", owner->GetType()->m_nEntityID},
		});
		auto spatial = parent.QueryInterface<ZSpatialEntity>();
		if (spatial) {
			auto trans = spatial->GetObjectToWorldMatrix().Pos;
			auto area = State::current.getArea(trans);
			auto roomId = ZRoomManagerCreator::GetRoomID(trans);
			json.merge_patch({
				{"CarPosition", {
					{"X", trans.x},
					{"Y", trans.y},
					{"Z", trans.z},
				}},
				{"CarArea", area ? area->ID : ""},
				{"CarRoom", roomId},
			});
		}
		SendCustomEvent("CarExploded"sv, ImbuedPlayerInfo(std::move(json)));
	});

	this->AddPinListener(ZHMPin::OnItemDestroyed, [this](ZEntityRef entity, const ZObjectRef& data, ZHMPin pin) {
		auto itemSpawner = entity.QueryInterface<ZItemSpawner>();
		if (!itemSpawner) return;
		if (!itemSpawner->m_rMainItemKey) return;
		auto repoId = itemSpawner->m_rMainItemKey.m_pInterfaceRef->m_RepositoryId.ToString();
		auto pos = itemSpawner->GetObjectToWorldMatrix().Pos;
		auto area = State::current.getArea(pos);
		SendCustomEvent("ItemDestroyed"sv, ImbuedPositionInfo(itemSpawner->m_mTransform.Trans, "Item", ImbuedPlayerInfo({
			{"RepositoryId", repoId.c_str()},
		}, true)));
	});

	this->AddPinListener(ZHMPin::OnTurnOn, [this](ZEntityRef entity, const ZObjectRef& data, ZHMPin pin) {
		// ZEntity > ZCompositeEntity > ZCompositeEntity > ZCompositeEntity
		json obj;
		if (!ImbueSetpieceActivatorInfo(entity, obj)) return;
		SendCustomEvent("OnTurnOn"sv, ImbuedPlayerInfo(std::move(obj), true));
	});

	this->AddPinListener(ZHMPin::OnTurnOff, [this](ZEntityRef entity, const ZObjectRef& data, ZHMPin pin) {
		// ZEntity > ZCompositeEntity > ZCompositeEntity > ZCompositeEntity
		json obj;
		if (!ImbueSetpieceActivatorInfo(entity, obj)) return;
		SendCustomEvent("OnTurnOff"sv, ImbuedPlayerInfo(std::move(obj), true));
	});

	this->AddPinListener(static_cast<ZHMPin>(4101414679), [this](ZEntityRef entity, const ZObjectRef& data, ZHMPin pin) {
		//ZEntity > ZCompositeEntity > ZCompositeEntity - fired on e.g. fusebox destroyed
		auto setpiece = entity.GetLogicalParent();
		if (!setpiece) return;
		auto spatial = setpiece.QueryInterface<ZSpatialEntity>();
		if (!spatial) return;
		const auto& trans = spatial->m_mTransform.Trans;
		auto obj = ImbuedPositionInfo({ trans.x, trans.y, trans.z }, "", {
			{"EntityID", setpiece->GetType()->m_nEntityID},
		});
		auto initialStateOn = entity.GetProperty<bool>("m_bInitialStateOn");
		if (!initialStateOn.IsEmpty())
			obj.merge_patch({ {"InitialStateOn", initialStateOn.Get()} });
		SendCustomEvent("OnDestroy"sv, ImbuedPlayerInfo(std::move(obj), true));
	});

	this->AddPinListener(ZHMPin::OnEntered, [this](ZEntityRef entity, const ZObjectRef& data, ZHMPin pin) {
		// Entered HIPS
		auto spatial = entity.QueryInterface<ZSpatialEntity>();
		auto sid = entity.GetProperty<ZRepositoryID>("m_sId");
		if (sid.IsEmpty() || sid.Get().IsEmpty()) return;
		auto obj = ImbuedPlayerInfo({
			{"EntityID", entity->GetType()->m_nEntityID},
			{"RepositoryId", sid.Get().ToString()},
		});
		if (spatial) {
			const auto& trans = spatial->m_mTransform.Trans;
			obj.merge_patch(ImbuedPositionInfo({ trans.x, trans.y, trans.z }, ""));
		}
		SendCustomEvent("OnEntered"sv, obj);
	});

	this->AddPinListener(ZHMPin::OnLeaving, [this](ZEntityRef entity, const ZObjectRef& data, ZHMPin pin) {
		// Leaving HIPS
		auto spatial = entity.QueryInterface<ZSpatialEntity>();
		auto sid = entity.GetProperty<ZRepositoryID>("m_sId");
		if (sid.IsEmpty() || sid.Get().IsEmpty()) return;
		auto obj = ImbuedPlayerInfo({
			{"EntityID", entity->GetType()->m_nEntityID},
			{"RepositoryId", sid.Get().ToString()},
		});
		if (spatial) {
			const auto& trans = spatial->m_mTransform.Trans;
			obj.merge_patch(ImbuedPositionInfo({ trans.x, trans.y, trans.z }, ""));
		}
		SendCustomEvent("OnLeaving"sv, obj);
	});

	this->AddPinListener(ZHMPin::OnInitialFracture, [this](ZEntityRef entity, const ZObjectRef& data, ZHMPin pin) {
		auto type = entity->GetType();
		if (!type) return;
		auto spatial = entity.QueryInterface<ZSpatialEntity>();
		if (!spatial) return;
		const auto& trans = spatial->m_mTransform.Trans;
		SendCustomEvent("OnInitialFracture", ImbuedPositionInfo({trans.x, trans.y, trans.z}, "", ImbuedPlayerInfo({
			{"EntityID", entity->GetType()->m_nEntityID}
		}, true)));
	});

	this->AddPinListener(ZHMPin::OnDestroyed, [this](ZEntityRef entity, const ZObjectRef& data, ZHMPin pin) {
		auto type = entity->GetType();
		if (!type) return;
		auto spatial = entity.QueryInterface<ZSpatialEntity>();
		if (!spatial) {
			LogDebug("Destroyed with no spatial {}", entity->GetType()->m_nEntityID);
			return;
		}
		const auto& trans = spatial->m_mTransform.Trans;
		SendCustomEvent("OnDestroyed", ImbuedPositionInfo({ trans.x, trans.y, trans.z }, "", ImbuedPlayerInfo({
			{"EntityID", entity->GetType()->m_nEntityID}
		}, true)));
	});

	this->AddPinListener(ZHMPin::OnBroken, [this](ZEntityRef entity, const ZObjectRef& data, ZHMPin pin) {
		auto type = entity->GetType();
		if (!type) return;
		auto spatial = entity.QueryInterface<ZSpatialEntity>();
		if (!spatial) {
			LogDebug("Broken with no spatial {}", entity->GetType()->m_nEntityID);
			return;
		}
		const auto& trans = spatial->m_mTransform.Trans;
		SendCustomEvent("OnBroken", ImbuedPositionInfo({ trans.x, trans.y, trans.z }, "", ImbuedPlayerInfo({
			{"EntityID", entity->GetType()->m_nEntityID}
		}, true)));
	});

	this->AddPinListener(ZHMPin::ExplosionAtPos, [this](ZEntityRef entity, const ZObjectRef& data, ZHMPin pin) {
		auto pos = data.As<SVector3>();
		pos ? SendCustomEvent("Explosion"sv, ImbuedPositionInfo(*pos, "", ImbuedPlayerInfo({}, true)))
			: SendCustomEvent("Explosion"sv, ImbuedPlayerInfo({}, true));
	});

	this->AddPinListener(ZHMPin::ProjectileBodyShot, [this](ZEntityRef entity, const ZObjectRef& data, ZHMPin pin) {
		const auto& trans = State::current.playerMatrix.Trans;
		auto pos = float4{trans.x, trans.y, trans.z, 1.0};
		if (this->gameplay.playerBodyShotPos == pos) return;
		SendCustomEvent("ProjectileBodyShot"sv, ImbuedPlayerInfo());
		this->gameplay.playerBodyShotPos = pos;
	});

	this->AddPinListener(ZHMPin::InstinctActive, [this](ZEntityRef entity, const ZObjectRef& data, ZHMPin pin) {
		if (!State::current.playerInInstinct)
			SendCustomEvent("InstinctActive"sv, ImbuedPlayerInfo());
		State::current.playerInInstinct = true;
		State::current.playerInInstinctSinceFrame = true;
	});

	this->AddPinListener(ZHMPin::OnEquip, [this](ZEntityRef entity, const ZObjectRef& data, ZHMPin pin) {
		SendCustomEvent("OnEquip"sv, ImbuedPlayerInfo(ImbuedItemInfo(entity)));
	});

	this->AddPinListener(ZHMPin::DoorOpen, [this](ZEntityRef entity, const ZObjectRef& data, ZHMPin pin) {
		auto singleDoor = entity.QueryInterface<ZHM5SingleDoor2>();
		auto doubleDoor = entity.QueryInterface<ZHM5DoubleDoor2>();
		if (!singleDoor && !doubleDoor) return;
		SendCustomEvent("OpenDoor"sv, ImbuedPlayerInfo({
			{"Type", doubleDoor ? "Double" : "Single"}
		}));
	});

	this->AddPinListener(ZHMPin::AgilityEnter, [this](ZEntityRef entity, const ZObjectRef& data, ZHMPin pin) {
		auto window = entity.QueryInterface<ZGuideWindow>();
		auto ledge = entity.QueryInterface<ZGuideLedge>();

		SendCustomEvent("AgilityEnter", ImbuedPlayerInfo({
			{"Type", ledge ? "Ledge" : (window ? "Window" : "")},
			{"Hangable", ledge && ledge->m_bCanHang},
		}));
	});

	this->AddPinListener(ZHMPin::AgilityStart, [this](ZEntityRef entity, const ZObjectRef& data, ZHMPin pin) {
		auto window = entity.QueryInterface<ZGuideWindow>();
		auto ledge = entity.QueryInterface<ZGuideLedge>();
		auto ladder = entity.QueryInterface<ZGuideLadder>();

		if (window || ledge || !State::current.playerStartingAgility) {
			SendCustomEvent("AgilityStart", ImbuedPlayerInfo({
				{"Type", window ? "Window" : (ledge ? "Ledge" : "")},
				{"Hangable", ledge && ledge->m_bCanHang},
			}));
		}
		State::current.playerStartingAgility = true;
		State::current.playerStartingAgilitySinceFrame = true;
	});

	this->AddPinListener(ZHMPin::WeaponStartReload, [this](ZEntityRef entity, const ZObjectRef& data, ZHMPin pin) {
		auto weap = entity.QueryInterface<ZHM5ItemWeapon>();
		SendCustomEvent("OnWeaponReload"sv, ImbuedPlayerInfo(ImbuedItemInfo(entity)));
	});

	this->AddPinListener(ZHMPin::PlayerAllShots, [this](ZEntityRef entity, const ZObjectRef& data, ZHMPin pin) {
		if (!State::current.playerShooting) {
			auto weap = entity.QueryInterface<ZHM5ItemWeapon>();
			if (!weap) return;
			auto descriptor = weap->m_pItemConfigDescriptor;
			SendCustomEvent("PlayerShot"sv, ImbuedPlayerInfo(ImbuedItemInfo(entity)));
		}
		State::current.playerShooting = true;
		State::current.playerShootingSinceFrame = true;
	});

	auto onStopDragging = [this](ZEntityRef entity, const ZObjectRef& data, ZHMPin pin) {
		gameplay.playerIsDragging = false;
	};
	this->AddPinListener(ZHMPin::DraggingStop, onStopDragging);
	this->AddPinListener(ZHMPin::DraggingStopMoving, onStopDragging);

	this->AddPinListener(ZHMPin::DraggingStartMoving, [this](ZEntityRef entity, const ZObjectRef& data, ZHMPin pin) {
		if (!gameplay.playerIsDragging) {
			gameplay.playerIsDragging = true;
			gameplay.sentPlayerDraggingEvent = false;
		}
	});

	this->AddPinListener(ZHMPin::OnEvacuationStarted, [this](ZEntityRef entity, const ZObjectRef& data, ZHMPin pin) {
		auto vip = entity.QueryInterface<ZVIPControllerEntity>();
		if (!vip || !vip->m_rVIP) return;
		SendCustomEvent("OnEvacuationStarted"sv, ImbuedPlayerInfo(ImbuedActorInfo(vip->m_rVIP, {}, true), true));
	});

	auto onOut = [this](ZEntityRef entity, const ZObjectRef& data, int output) {
		auto entityID = entity->GetType()->m_nEntityID;
		if (spamEntityIDs.contains(entityID)) return;
		SendCustomEvent("OnOut", ImbuedPlayerInfo({
			{"Output", output},
			{"EntityID", entityID}
		}, true));
	};
	this->AddPinListener(ZHMPin::Out00, [onOut](ZEntityRef entity, const ZObjectRef& data, ZHMPin pin) { onOut(entity, data, 0); });
	this->AddPinListener(ZHMPin::Out01, [onOut](ZEntityRef entity, const ZObjectRef& data, ZHMPin pin) { onOut(entity, data, 1); });
	//this->AddPinListener(ZHMPin::Out02, [onOut](ZEntityRef entity, const ZObjectRef& data, ZHMPin pin) { onOut(entity, data, 2); });
	//this->AddPinListener(ZHMPin::Out03, [onOut](ZEntityRef entity, const ZObjectRef& data, ZHMPin pin) { onOut(entity, data, 3); });
	//this->AddPinListener(ZHMPin::Out04, [onOut](ZEntityRef entity, const ZObjectRef& data, ZHMPin pin) { onOut(entity, data, 4); });
	//this->AddPinListener(ZHMPin::Out05, [onOut](ZEntityRef entity, const ZObjectRef& data, ZHMPin pin) { onOut(entity, data, 5); });
	//this->AddPinListener(ZHMPin::Out06, [onOut](ZEntityRef entity, const ZObjectRef& data, ZHMPin pin) { onOut(entity, data, 6); });
	//this->AddPinListener(ZHMPin::Out07, [onOut](ZEntityRef entity, const ZObjectRef& data, ZHMPin pin) { onOut(entity, data, 7); });
	//this->AddPinListener(ZHMPin::Out08, [onOut](ZEntityRef entity, const ZObjectRef& data, ZHMPin pin) { onOut(entity, data, 8); });

	this->AddPinListener(ZHMPin::OnInterrupted, [this](ZEntityRef entity, const ZObjectRef& data, ZHMPin pin) {
		auto entityID = entity->GetType()->m_nEntityID;
		SendCustomEvent("OnInterrupted", ImbuedPlayerInfo({
			{"EntityID", entityID}
		}, true));
	});

	auto onInteraction = [this](ZEntityRef entity, const ZObjectRef& data, ZHMPin pin) {
		if (!entity->GetType()) return;

		auto entityID = entity->GetType()->m_nEntityID;
		if (spamEntityIDs.contains(entityID))
			return;

		auto inputAction = entity.GetProperty<EHM5GameInputFlag>("m_eInputAction");
		if (inputAction.IsEmpty()) {
			Logger::Debug("Entity without m_eInputAction property: {:#08x}", entityID);
			return;
		}

		auto actionType = entity.GetProperty<EActionType>("m_eActionType");
		auto objectRef = entity.GetProperty<ZEntityRef>("m_Object");
		auto object = !objectRef.IsEmpty() ? objectRef.Get() : nullptr;

		auto js = ImbuedPlayerInfo({
			{"EntityID", entityID},
			{"InputAction", inputAction.Get()},
			{"ActionType", static_cast<int32_t>(actionType.Get())},
		}, true);
		if (pin == ZHMPin::ItemUsed) {
			auto item = data.As<ZEntityRef>();
			if (item) {
				js.merge_patch({
					{"Item", ImbuedItemInfo(*item, {}, "")}
				});
			}
		}

		if (object && object->GetType()) {
			js.merge_patch({
				{"Object", ImbuedItemInfo(object, {}, "")}
			});
		}

		if (pin == ZHMPin::Started)
			SendCustomEvent("OnInteractionStarted", std::move(js));
		else if (pin == ZHMPin::Completed)
			SendCustomEvent("OnInteractionCompleted", std::move(js));
		else if (pin == ZHMPin::ItemUsed)
			SendCustomEvent("OnInteractionItemUsed", std::move(js));
	};
	this->AddPinListener(ZHMPin::Started, onInteraction);
	this->AddPinListener(ZHMPin::ItemUsed, onInteraction);
	this->AddPinListener(ZHMPin::Completed, onInteraction);
}

DEFINE_PLUGIN_DETOUR(CroupierPlugin, bool, OnPinOutput, ZEntityRef entity, uint32_t pinId, const ZObjectRef& data) {
	// ZHMPin::Discharge_Shot - On NPC Fire
	// ZHMPin::PlayerAllShots - On Player Fire (Twice)

	// Try: DoorBroken, NormalShot

	if (State::current.gameMode != GameMode::Roulette) {
		auto pin = static_cast<ZHMPin>(pinId);
		auto listeners = GetPinListeners(pin);
		if (listeners)
			listeners->handle(entity, data, pin);
	}

	/*switch (pin) {
		case ZHMPin::HitmanInVision:
			// "Never seen by targets", "Never seen by guards" etc?
			break;
		//case ZHMPin::HM_WeaponEquipped: // ZWeaponSoundSetupEntity (data: void), child of zhmitemweapon or whatever
		//case ZHMPin::OnRemovedFromContainer: // ZHM5ItemWeapon (data: void)
		//case ZHMPin::ThrowActivated: // ZThrowSoundController
		//case ZHMPin::ThrowImpact:
		//case ZHMPin::OnPutInContainer: {
		//	auto it = std::find_if(entitiesPutInContainer.begin(), entitiesPutInContainer.end(), [entity](const std::pair<ZEntityRef, int>& ent) { return ent.first == entity; });
		//	if (it == entitiesPutInContainer.end())
		//		entitiesPutInContainer.emplace_back(entity, 0);
		//	break;
		//}
		case ZHMPin::Activate: { // ZInteractionEventConsumer
			// m_nEvent property enum - triggered on certain interactions?
			break;
		}
		//case ZHMPin::OnHearExplosion:
		//case ZHMPin::ShotBegin:
		//case ZHMPin::Discharge_ShotSilenced:
		//case ZHMPin::SpawnPhysicsClip:
		//case ZHMPin::WeaponEquipIllegal:
		//	Logger::Debug("PIN: WeaponEquipIllegal {}", typeName);	// works!
		//	break;
		//case ZHMPin::WeaponEquipSuspicious:
		//	Logger::Debug("PIN: WeaponEquipSuspicious {}", typeName);
		//	break;
		//case ZHMPin::OnDropByHero: // ZHM5ItemWeapon
		//	break;
		//case ZHMPin::OnDrop: // ZHM5ItemWeapon, ZEntity
		//	break;
		//case ZHMPin::WeaponUnEquipped: // ZHM5ItemWeapon, ZEntity
		//	break;
		//case ZHMPin::WeaponPlayerUnEquipped: // ZHM5ItemWeapon, ZEntity
		//	break;
		//case ZHMPin::WeaponPlayerEquipped: // ZHM5ItemWeapon, ZEntity
		//	break;
		//case ZHMPin::Equipped: // ZHM5ItemCCWeapon, ZEntity
		//	break;
		//case ZHMPin::ThrowImpact: // ZHM5Item, ZEntity
		//	break;
		//case ZHMPin::OnThrown: // ZHM5Item, ZEntity
		//	break;
		
		// ONLY WORK WHILE TRESPASSING :(
		//case ZHMPin::IsCrouchWalkingSlowly:
		//	if (State::current.playerMoveType != PlayerMoveType::WalkingSlowly)
		//		SendCustomEvent("IsCrouchWalkingSlowly", {});
		//	State::current.playerMoveType = PlayerMoveType::WalkingSlowly;
		//	break;
		//case ZHMPin::IsCrouchWalking:
		//	if (State::current.playerMoveType != PlayerMoveType::CrouchWalkingSlowly)
		//		SendCustomEvent("IsCrouchWalking", {});
		//	State::current.playerMoveType = PlayerMoveType::CrouchWalkingSlowly;
		//	break;
		//case ZHMPin::IsCrouchRunning:
		//	if (State::current.playerMoveType != PlayerMoveType::CrouchRunning)
		//		SendCustomEvent("IsCrouchRunning", {});
		//	State::current.playerMoveType = PlayerMoveType::CrouchRunning;
		//	break;
		//case ZHMPin::IsRunning:
		//	if (State::current.playerMoveType != PlayerMoveType::Running)
		//		SendCustomEvent("IsRunning", {});
		//	State::current.playerMoveType = PlayerMoveType::Running;
		//	break;
		//case ZHMPin::IsWalking:
		//	if (State::current.playerMoveType != PlayerMoveType::Walking)
		//		SendCustomEvent("IsWalking", {});
		//	State::current.playerMoveType = PlayerMoveType::Walking;
		//	break;
		//case ZHMPin::IsWalkingSlowly:
		//	if (State::current.playerMoveType != PlayerMoveType::WalkingSlowly)
		//		SendCustomEvent("IsWalkingSlowly", {});
		//	State::current.playerMoveType = PlayerMoveType::WalkingSlowly;
		//	break;
	}*/
	return HookAction::Continue();
}

DEFINE_PLUGIN_DETOUR(CroupierPlugin, void, OnWinHttpCallback, void* dwContext, void* hInternet, void* param_3, int dwInternetStatus, void* param_5, int param_6) {
	static wchar_t buffer[200];
	DWORD size = sizeof(buffer);
	if (WinHttpQueryOption(hInternet, WINHTTP_OPTION_URL, buffer, &size)) {
		std::wstring wstr(buffer, size);
		auto url = narrow(wstr);
		std::string_view sv = url;

		//Logger::Info("WinHttpQueryOption result: {}", sv);

		auto isHttps = sv.starts_with("https://");
		if (isHttps || sv.starts_with("http://")) {
			auto urlNoProto = sv.substr((isHttps ? sizeof("https://") : sizeof("http://")) - 1);
			auto isLocal = urlNoProto.starts_with("127.0.0.1/") || urlNoProto.starts_with("localhost/");

			if (isLocal || urlNoProto.starts_with("hm3-service.hitman.io/")) {
				auto urlPath = isLocal ? urlNoProto.substr(sizeof("localhost/") - 1) : urlNoProto.substr(sizeof("hm3-service.hitman.io/") - 1);


				if (urlPath.starts_with("profiles/page/Planning?contractid=")) {
					auto rest = urlPath.substr(sizeof("profiles/page/Planning?contractid="));
					auto contractId = rest.substr(0, rest.find_first_of('&'));
					auto mission = getMissionByContractId(std::string(contractId));

					if (mission != eMission::NONE) {
						State::OnMissionSelect(mission);
						if (!State::current.isPlaying)
							State::current.playerStart();
					}
				}
			}
		}
	}
	//else Logger::Info("WinHttpQueryOption failed.");
	return HookAction::Continue();
}

DECLARE_ZHM_PLUGIN(CroupierPlugin);
