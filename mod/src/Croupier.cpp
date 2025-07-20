#include "Croupier.h"
#include <chrono>
#include <format>
#include <variant>
#include <winhttp.h>
#include <Logging.h>
#include <IconsMaterialDesign.h>
#include <Globals.h>
#include <Glacier/SOnlineEvent.h>
#include <Glacier/ZAction.h>
#include <Glacier/ZActor.h>
#include <Glacier/ZGameLoopManager.h>
#include <Glacier/ZInputActionManager.h>
#include <Glacier/ZItem.h>
#include <Glacier/ZScene.h>
#include <Glacier/ZString.h>
#include <Glacier/ZHitman5.h>
#include <Glacier/ZModule.h>
#include <Glacier/ZOutfit.h>
#include <Glacier/ZGameMode.h>
#include <Glacier/ZKnowledge.h>
#include <Glacier/ZResource.h>
#include <Glacier/ZPlayerRegistry.h>
#include <Glacier/ZContentKitManager.h>
#include <Glacier/ZHM5BaseCharacter.h>
#include <Glacier/Pins.h>
#include "App.h"
#include "Debug.h"
#include "Events.h"
#include "KillConfirmation.h"
#include "KillMethod.h"
#include "SpinParser.h"
#include "json.hpp"
#include "util.h"
#include "ZHMUtils.h"

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
}

CroupierPlugin::~CroupierPlugin() {
	State::current.client.stop();
	this->config.Save();
	this->UninstallHooks();
}

auto CroupierPlugin::OnEngineInitialized() -> void {
	Logger::Info("Croupier has been initialized!");

	this->SetupEvents();

	State::current.client.start();

	this->InstallHooks();
	this->config.Load();

	if (this->config.missionPool.empty())
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

	Logger::Info("Croupier - Hooks installed.");
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

	Logger::Info("Croupier - Hooks uninstalled.");
}

auto CroupierPlugin::OnFrameUpdate(const SGameUpdateEvent& ev) -> void {
	this->ProcessClientMessages();
	this->ProcessLoadRemoval();
}

auto CroupierPlugin::OnFrameUpdate_PlayMode(const SGameUpdateEvent& ev) -> void {
	this->ProcessSpinState();
	this->ProcessPlayerState();
}

auto CroupierPlugin::ProcessPlayerState() -> void {
	if (!this->state.playerInInstinctSinceFrame && this->state.playerInInstinct)
		this->state.playerInInstinct = false;
	this->state.playerInInstinctSinceFrame = false;

	auto player = SDK()->GetLocalPlayer();
	if (!player) return;

	const auto spatial = player.m_ref.QueryInterface<ZSpatialEntity>();
	this->state.playerMatrix = spatial->m_mTransform;

	// Process area entry for bingo
	auto area = this->state.getArea(this->state.playerMatrix.Trans);
	if (area && area != this->state.area) {
		this->SendCustomEvent("EnterArea"sv, ImbuedPlayerLocation({
			{"Area", area->ID},
		}));
	}

	this->state.area = area;

	// Process room changes for bingo
	auto roomId = ZRoomManagerCreator::GetRoomID(spatial->GetWorldMatrix().Pos);
	if (roomId != this->state.roomId && roomId != -1) {
		this->state.roomId = roomId;
		this->SendCustomEvent("EnterRoom"sv, ImbuedPlayerLocation({
			{"Room", roomId},
		}));
	}
}

auto CroupierPlugin::ProcessSpinState() -> void {
	if (this->state.spinCompleted) return;
	//if (this->state.hasLoadedGame) return;

	for (int i = 0; i < *Globals::NextActorId; ++i) {
		auto& actorData = this->state.actorData[i];

		auto const& actorRef = Globals::ActorManager->m_aActiveActors[i];
		actorData.actor = &actorRef;

		auto repoEntity = actorRef.m_ref.QueryInterface<ZRepositoryItemEntity>();
		if (repoEntity != nullptr && (!actorData.repoId || *actorData.repoId != repoEntity->m_sId)) {
			if (actorData.repoId && *actorData.repoId != repoEntity->m_sId)
				this->state.actorDataRepoIdMap.erase(*actorData.repoId);
			actorData.repoId = repoEntity->m_sId;
			this->state.actorDataRepoIdMap.emplace(*actorData.repoId, i);
		}

		if (!actorRef.m_pInterfaceRef) continue;

		auto& actor = *actorRef.m_pInterfaceRef;
		actorData.isTarget = actor.m_bUnk16; // m_bUnk16 = is target (and still alive)
		actorData.isPacified = actor.IsPacified() && !actor.IsDead();
		actorData.isDead = actor.IsDead();

		auto spatial = actorRef.m_ref.QueryInterface<ZSpatialEntity>();
		actorData.transform = spatial->m_mTransform;

		auto character = actor.m_rCharacter;
		auto outfit = actor.m_rOutfit;
		auto characterTemplateAspect = character.m_ref.QueryInterface<ZCharacterTemplateAspect>();
		auto characterTemplateAspectRef = character.m_ref.QueryInterface<TEntityRef<ZCharacterTemplateAspect>>();

		if (outfit && outfit.m_pInterfaceRef) {
			auto outfitRef = outfit.m_pInterfaceRef;
			actorData.isFemale = outfitRef->m_bIsFemale;
			actorData.hasDisguise = outfitRef->m_bHeroDisguiseAvailable;
			actorData.disguiseRepoId = outfitRef->m_sId;
			actorData.actorType = outfitRef->m_eActorType;
			actorData.outfitType = outfitRef->m_eOutfitType;
		}

		if (actor.m_bIsBeingDragged && gameplay.playerIsDragging && !gameplay.sentPlayerDraggingEvent) {
			SendCustomEvent("DragBodyMove"sv, ImbuedActorInfo(actorRef, ImbuedPlayerLocation()));
			gameplay.sentPlayerDraggingEvent = true;
		}

		// Handle roulette target No KO confirmations
		if (!actorData.isTarget || !actorData.repoId) continue;

		auto targetId = GetTargetByRepoID(*actorData.repoId);
		auto const& conditions = this->state.spin.getConditions();
		if (conditions.empty()) continue;

		for (auto i = 0; i < conditions.size() && i < this->state.killValidations.size(); ++i) {
			auto& cond = conditions[i];
			auto& target = cond.target.get();
			if (targetId != target.getID()) continue;
			auto& kc = this->state.getKillConfirmation(i);
			if (!kc.isPacified) break;

			if (!actor.IsPacified() && !actor.IsDead())
				kc.isPacified = false;
		}
	}

	this->state.actorDataSize = *Globals::NextActorId;
}

auto CroupierPlugin::ProcessClientEvent(std::string_view name, const json& json) -> void {
	if (name == "Areas") {
		this->state.areas.clear();
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

			this->state.areas.push_back(std::move(area));
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
			case eClientMessage::Missions:
				return ProcessMissionsMessage(message);
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
					if (timerStopped) this->state.isFinished = true;
					else {
						this->state.isPlaying = true;
						this->state.isFinished = false;
					}
				}
				if (parts.size() < 2) return;
				uint64_t timeElapsed = 0;
				auto res = std::from_chars(parts[1].data(), parts[1].data() + parts[1].size(), timeElapsed);
				if (res.ec == std::errc()) {
					auto now = std::chrono::steady_clock::now();
					this->state.timeStarted = now - std::chrono::milliseconds(timeElapsed);
					this->state.timeElapsed = std::chrono::duration_cast<std::chrono::seconds>(now - this->state.timeStarted);
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
		ZRenderContext* m_pRenderContext; // 0x14280, look for "ZRenderManager::RenderThread" string, first thing being constructed and assigned
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
	Configuration::main.missionPool.clear();
	std::string buffer;

	for (auto const& token : tokens) {
		buffer = trim(token);
		auto mission = getMissionByCodename(buffer);
		if (mission != eMission::NONE)
			Configuration::main.missionPool.push_back(mission);
	}
}

auto CroupierPlugin::ProcessSpinDataMessage(const ClientMessage& message) -> void {
	auto spin = SpinParser::parse(message.args);
	if (!spin.has_value()) return;

	State::current.spin = std::move(*spin);
	this->state.isPlaying = false;
	this->currentSpinSaved = true;
	State::current.generator.setMission(State::current.spin.getMission());
	this->state.spinCompleted = false;
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

	this->config.Save();
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
	this->state.isPlaying = false;
	this->currentSpinSaved = true;
	State::current.generator.setMission(State::current.spin.getMission());
	State::current.spinHistory.pop();
	this->state.spinCompleted = false;

	this->state.playerStart();
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
	if (!State::current.generator.getMission()) return;

	if (isAuto)
		SendAutoSpin(State::current.generator.getMission()->getMission());
	else
		SendRespin(State::current.generator.getMission()->getMission());

	State::current.generator.setRuleset(&State::current.rules);

	if (isAuto && State::current.spinLocked) return;
	if (State::current.client.isConnected()) return;

	try {
		if (!State::current.spin.getConditions().empty()) {
			State::current.spinHistory.emplace(std::move(State::current.spin));
		}

		State::current.spin = State::current.generator.spin(&State::current.spin);
		this->state.timeStarted = std::chrono::steady_clock::now();
		this->currentSpinSaved = false;
		this->state.spinCompleted = false;
	} catch (const std::runtime_error& ex) {
		Logger::Error("Croupier: {}", ex.what());
	}

	this->SaveSpinHistory();
}

auto lastThrownItem = ""s;

auto CroupierPlugin::GetOutfitByRepoId(std::string_view repoId) -> const ZGlobalOutfitKit* {
	return this->GetOutfitByRepoId(ZRepositoryID{repoId});
}

auto CroupierPlugin::GetOutfitByRepoId(ZRepositoryID repoId) -> const ZGlobalOutfitKit* {
	if (!Globals::ContentKitManager) return nullptr;
	auto& globalOutfitKitsRepo = Globals::ContentKitManager->m_repositoryGlobalOutfitKits;
	auto it = globalOutfitKitsRepo.find(repoId);
	if (it == globalOutfitKitsRepo.end() || !it->second.m_pInterfaceRef)
		return nullptr;
	return it->second.m_pInterfaceRef;
}

auto CroupierPlugin::ImbueDisguiseEvent(const std::string& repoId) -> json {
	auto outfit = this->GetOutfitByRepoId(repoId);
	auto json = json::object({ {"RepositoryId", repoId} });
	ImbuePlayerLocation(json);
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

auto CroupierPlugin::ImbueActorInfo(TEntityRef<ZActor> ref, json& j, bool asActor) const -> void {
	if (!ref) return;

	const auto actor = ref.m_pInterfaceRef;
	const auto repoEntity = ref.m_ref.QueryInterface<ZRepositoryItemEntity>();

	if (repoEntity) {
		j.merge_patch({
			{"ActorRepositoryId", repoEntity->m_sId.ToString()},
		});

		if (const auto actorData = this->state.getActorDataByRepoId(repoEntity->m_sId)) {
			auto area = this->state.getArea(actorData->transform.Trans);
			j.merge_patch({
				{"ActorArea", area ? area->ID : ""},
				{"ActorHasDisguise", actorData->hasDisguise},
				{"ActorIsDead", actorData->isDead},
				{"ActorIsFemale", actorData->isDead},
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
		}
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

auto CroupierPlugin::ImbuePacifyEvent(const PacifyEventValue& ev) -> std::optional<json> {
	const auto actorData = this->state.getActorDataByRepoId(ZRepositoryID(ev.RepositoryId));
	if (!actorData) return std::nullopt;
	auto const playerOutfitRepoId = ZRepositoryID(ev.OutfitRepositoryId);
	auto const actorOutfit = actorData->disguiseRepoId ? this->GetOutfitByRepoId(*actorData->disguiseRepoId) : nullptr;
	return ImbuedPlayerLocation({
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
	const auto& trans = this->state.playerMatrix.Trans;
	json.merge_patch({
		{"IsIdle", this->state.playerMoveType == PlayerMoveType::Idle},
		{"IsCrouching", this->state.playerMoveType == PlayerMoveType::CrouchRunning || this->state.playerMoveType == PlayerMoveType::CrouchWalking},
		{"IsRunning", this->state.playerMoveType == PlayerMoveType::CrouchRunning || this->state.playerMoveType == PlayerMoveType::Running},
		{"IsWalking", this->state.playerMoveType == PlayerMoveType::CrouchWalking || this->state.playerMoveType == PlayerMoveType::Walking},
		{"IsTrespassing", this->state.isTrespassing},
		{asHero ? "HeroRoom" : "Room", this->state.roomId},
		{asHero ? "HeroArea" : "Area", this->state.area ? this->state.area->ID : ""},
		{asHero ? "HeroPosition" : "Position", {
			{"X", trans.x},
			{"Y", trans.y},
			{"Z", trans.z},
		}},
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

auto CroupierPlugin::ImbueItemEvent(const ItemEventValue& ev, EActionType actionType) -> std::optional<json> {
	for (const auto action : Globals::HM5ActionManager->m_Actions) {
		if (!action || action->m_eActionType != actionType)
			continue;
		const ZHM5Item* item = action->m_Object.QueryInterface<ZHM5Item>();
		if (!item) continue;
		if (!item->m_pItemConfigDescriptor) continue;
		if (item->m_pItemConfigDescriptor->m_RepositoryId != ZRepositoryID(ev.RepositoryId))
			continue;
		auto const instanceId = reinterpret_cast<uintptr_t>(item);
		if (this->state.collectedItemInstances.contains(instanceId))
			continue;
		this->state.collectedItemInstances.insert(instanceId);
		return ImbuedPlayerLocation({
			{"RepositoryId", ev.RepositoryId},
			{"InstanceId", std::format("{}", instanceId)},
			{"ItemType", ev.ItemType},
			{"ItemName", ev.ItemName},
		});
	}
	return std::nullopt;
}

auto CroupierPlugin::ImbueItemInfo(ZEntityRef entity, json& j) -> void {
	auto item = QueryAnyParent<ZHM5Item>(entity);
	if (!item) return;
	auto ccWeapon = QueryAnyParent<ZHM5ItemCCWeapon>(entity);
	auto isFiberWire = ccWeapon && ccWeapon->m_bCountsAsFiberWire;
	std::string itemType;
	std::string itemSize;
	
	if (const auto desc = item->m_pItemConfigDescriptor) {
		if (repositoryResource.m_nResourceIndex == -1) {
			const auto s_ID = ResId<"[assembly:/repository/pro.repo].pc_repo">;
			Globals::ResourceManager->GetResourcePtr(repositoryResource, s_ID, 0);
		}
		if (repositoryResource.GetResourceInfo().status == RESOURCE_STATUS_VALID) {
			auto repositoryData = static_cast<THashMap<ZRepositoryID, ZDynamicObject, TDefaultHashMapPolicy<ZRepositoryID>>*>(repositoryResource.GetResourceData());
			if (repositoryData) {
				auto it = repositoryData->find(desc->m_RepositoryId);
				if (it != repositoryData->end()) {
					const auto entries = it->second.As<TArray<SDynamicObjectKeyValuePair>>();
					if (entries) {
						for (size_t i = 0; i < entries->size(); ++i) {
							auto const& entry = (*entries)[i];
							if (entry.sKey != "ItemType") continue;
							itemType = *entry.value.As<ZString>();
							break;
						}
					}
				}
			}
		}

		j.merge_patch({
			{"ItemName", desc->m_sTitle},
			{"ItemInstanceId", reinterpret_cast<uintptr_t>(item)},
			{"ItemRepositoryId", desc->m_RepositoryId.ToString()},
		});
	}

	if (ccWeapon) {
		j.merge_patch({
			{"WeaponAnimFrontSide", weaponAnimSetToString(ccWeapon->m_eAnimSetFrontSide)},
			{"WeaponAnimBack", weaponAnimSetToString(ccWeapon->m_eAnimSetBack)},
		});
	}
	if (auto weapon = QueryAnyParent<ZHM5ItemWeapon>(entity)) {
		j.merge_patch({
			{"IsScopedWeapon", weapon->m_bScopedWeapon},
			{"WeaponAnimationCategory", weapon->m_eAnimationCategory},
			{"WeaponType", weapon->m_WeaponType},
		});
	}
	
	j.merge_patch({
		{"IsCloseCombatWeapon", ccWeapon != nullptr},
		{"IsFiberWire", isFiberWire},
		{"IsFirearm", QueryAnyParent<IFirearm>(entity) != nullptr},
		{"RepositoryItemType", itemType},
		{"RepositoryItemSize", itemSize},
	});
}

auto CroupierPlugin::ImbuedPlayerLocation(json&& j, bool asHero) const -> json {
	ImbuePlayerLocation(j, asHero);
	return j;
}

auto CroupierPlugin::ImbuedActorInfo(TEntityRef<ZActor> entity, json&& js, bool asActor) const -> json {
	ImbueActorInfo(entity, js, asActor);
	return js;
}

auto CroupierPlugin::ImbuedItemInfo(ZEntityRef entity, json&& js) -> json {
	ImbueItemInfo(entity, js);
	return js;
}

auto CroupierPlugin::SendCustomEvent(std::string_view name, json eventValue) -> void {
#ifndef _DEBUG
	if (!this->config.debug && !State::current.client.isConnected()) return;
#endif
	json js = {
		{"Name", name},
		{"Value", eventValue},
	};
	auto dump = js.dump();
	Logger::Debug("<--- {}", dump);
	State::current.client.sendRaw(dump);
}

auto CroupierPlugin::SetupEvents() -> void {
	events.listen<Events::ContractStart>([this](const ServerEvent<Events::ContractStart>& ev) {
		this->state.playerStart();
		this->state.locationId = ev.Value.LocationId;
		this->state.loadout = ev.Value.Loadout;
		this->state.spinCompleted = false;

		SendKillValidationUpdate();
	});
	events.listen<Events::HeroSpawn_Location>([this](const ServerEvent<Events::HeroSpawn_Location>& ev) {
		SendMissionStart(this->state.locationId, ev.Value.RepositoryId, this->state.loadout);
	});
	events.listen<Events::IntroCutEnd>([this](const ServerEvent<Events::IntroCutEnd>& ev) {
		this->state.playerCutsceneEnd(ev.Timestamp);
	});
	events.listen<Events::ContractLoad>([this](auto& ev) {
		this->state.playerLoad();
		SendKillValidationUpdate();
	});
	events.listen<Events::ExitGate>([this](const ServerEvent<Events::ExitGate>& ev) {
		this->state.playerExit(ev.Timestamp);

		// Mark any unfulfilled kill methods as invalid (never killed a Berlin agent with correct requirements, destroyed heart instead of killing Soders or vice-versa, etc.)
		auto const& conds = State::current.spin.getConditions();
		for (auto& kv : this->state.killValidations) {
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
		this->state.playerExit(ev.Timestamp);

		// Mark any unfulfilled kill methods as invalid (never killed a Berlin agent with correct requirements, destroyed heart instead of killing Soders or vice-versa, etc.)
		auto const& conds = State::current.spin.getConditions();
		for (auto& kv : this->state.killValidations) {
			if (kv.correctMethod == eKillValidationType::Incomplete)
				kv.correctMethod = eKillValidationType::Invalid;
		}

		SendKillValidationUpdate();
		SendMissionComplete();
	});
	events.listen<Events::ContractEnd>([this](const ServerEvent<Events::ContractEnd>& ev) {
		if (!this->state.isFinished) {
			this->state.playerExit(ev.Timestamp);

			// Mark any unfulfilled kill methods as invalid (never killed a Berlin agent with correct requirements, destroyed heart instead of killing Soders or vice-versa, etc.)
			auto const& conds = State::current.spin.getConditions();
			for (auto& kv : this->state.killValidations) {
				if (kv.correctMethod == eKillValidationType::Incomplete)
					kv.correctMethod = eKillValidationType::Invalid;
			}

			SendKillValidationUpdate();
			SendMissionComplete();
		}

		this->state.spinCompleted = true;
	});
	events.listen<Events::ContractFailed>([this](const ServerEvent<Events::ContractFailed>& ev) {
		SendMissionFailed();
		Logger::Info("Croupier: ContractFailed {}", ev.Value.value.dump());
	});
	events.listen<Events::StartingSuit>([this](const ServerEvent<Events::StartingSuit>& ev) {
		this->SendCustomEvent("StartingSuit"sv, ImbueDisguiseEvent(ev.Value.value));

		if (this->state.spinCompleted) return;
		this->state.disguiseChanges.emplace_back(ev.Value.value, ev.Timestamp);
	});
	events.listen<Events::Disguise>([this](const ServerEvent<Events::Disguise>& ev) {
		if (gameplay.disguiseChange.havePinData)
			this->SendCustomEvent("Disguise"sv, ImbueDisguiseEvent(ev.Value.value));
		else {
			gameplay.disguiseChange.haveEventData = true;
			gameplay.disguiseChange.eventData = ev.Value.value;
		}

		if (this->state.spinCompleted) return;
		this->state.disguiseChanges.emplace_back(ev.Value.value, ev.Timestamp);
	});
	events.listen<Events::FriskedSuccess>([this](const ServerEvent<Events::FriskedSuccess>& ev) {
		this->SendCustomEvent("FriskedSuccess"sv, ImbuedPlayerLocation());
	});
	events.listen<Events::Dart_Hit>([this](const ServerEvent<Events::Dart_Hit>& ev) {
		this->SendCustomEvent("DartHit"sv, ImbuedPlayerLocation({
			{"RepositoryId", ev.Value.RepositoryId},
			{"ActorType", ev.Value.ActorType},
			{"Blind", ev.Value.Blind},
			{"Sedative", ev.Value.Sedative},
			{"Sick", ev.Value.Sick},
			{"IsTarget", ev.Value.IsTarget},
		}));
	});
	events.listen<Events::ItemThrown>([this](const ServerEvent<Events::ItemThrown>& ev) {
		//auto imbued = this->ImbueItemEvent(ev.Value, EActionType::AT_ITEM_INTERACTION);
		//if (imbued) this->SendImbuedEvent(ev, *imbued);
		lastThrownItem = ev.Value.RepositoryId;
		this->SendCustomEvent("ItemThrown"sv, ImbuedPlayerLocation({
			{"RepositoryId", ev.Value.RepositoryId},
			{"InstanceId", ev.Value.InstanceId},
			{"ItemType", ev.Value.ItemType},
			{"ItemName", ev.Value.ItemName},
		}));
	});
	events.listen<Events::ItemPickedUp>([this](const ServerEvent<Events::ItemPickedUp>& ev) {
		auto imbued = this->ImbueItemEvent(ev.Value, EActionType::AT_PICKUP);
		if (imbued) this->SendCustomEvent("ItemPickedUp"sv, *imbued);
	});
	events.listen<Events::Trespassing>([this](const ServerEvent<Events::Trespassing>& ev) {
		this->state.isTrespassing = ev.Value.IsTrespassing;
		this->SendCustomEvent("Trespassing"sv, ImbuedPlayerLocation());
	});
	events.listen<Events::BodyFound>([this](const ServerEvent<Events::BodyFound>& ev) {
		this->SendCustomEvent("BodyFound"sv, json{
			{"RepositoryId", ev.Value.DeadBody.RepositoryId},
			{"DeathContext", ev.Value.DeadBody.DeathContext},
			{"DeathType", ev.Value.DeadBody.DeathType},
			{"IsCrowdActor", ev.Value.DeadBody.IsCrowdActor},
		});
	});
	events.listen<Events::Door_Unlocked>([this](const ServerEvent<Events::Door_Unlocked>& ev) {
		this->SendCustomEvent("DoorUnlocked"sv, ImbuedPlayerLocation());
	});
	events.listen<Events::Pacify>([this](const ServerEvent<Events::Pacify>& ev) {
		auto data = this->ImbuePacifyEvent(ev.Value);
		if (data) this->SendCustomEvent("Pacify"sv, *data);

		if (!ev.Value.IsTarget) return;
		if (this->state.spinCompleted) return;

		auto const& conditions = this->state.spin.getConditions();
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

			auto& kc = this->state.getKillConfirmation(i);
			kc.target = target.getID();
			kc.isPacified = true;
		}
	});
	events.listen<Events::C_Hungry_Hippo>([this](const ServerEvent<Events::C_Hungry_Hippo>& ev) {
		if (this->state.spinCompleted) return;
		auto const mission = this->state.spin.getMission();
		if (!mission) return;
		if (mission->getMission() != eMission::SANTAFORTUNA_THREEHEADEDSERPENT) return;

		auto const& conditions = this->state.spin.getConditions();
		if (conditions.empty()) return;

		for (auto i = 0; i < conditions.size(); ++i) {
			auto const& cond = conditions[i];
			if (cond.target.get().getID() != eTargetID::RicoDelgado) continue;
			if (cond.specificKillMethod.method != eMapKillMethod::Rico_FeedToHippo) return;
			
			auto const& target = cond.target.get();
			auto& kc = this->state.getKillConfirmation(i);
			kc.target = eTargetID::RicoDelgado;
			kc.correctMethod = eKillValidationType::Valid;
			SendKillValidationUpdate();
			break;
		}
	});
	events.listen<Events::TargetEscapeFoiled>([this](const ServerEvent<Events::TargetEscapeFoiled>& ev) {
		if (this->state.spinCompleted) return;
		auto const mission = this->state.spin.getMission();
		if (!mission) return;
		if (mission->getMission() != eMission::HOKKAIDO_SITUSINVERSUS) return;

		auto const& conditions = this->state.spin.getConditions();
		if (conditions.empty()) return;

		for (auto i = 0; i < conditions.size(); ++i) {
			auto const& cond = conditions[i];
			if (cond.target.get().getID() != eTargetID::YukiYamazaki) continue;
			if (cond.specificKillMethod.method != eMapKillMethod::Yuki_SabotageCableCar) return;
			
			auto const& target = cond.target.get();
			auto& kc = this->state.getKillConfirmation(i);
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

		if (this->state.spinCompleted) return;

		this->state.killed.insert(ev.Value.RepositoryId);
		this->state.spottedNotKilled.erase(ev.Value.RepositoryId);

		if (!ev.Value.IsTarget) {
			if (ev.Value.KillContext != EDeathContext::eDC_NOT_HERO)
				this->state.voidSA();
			return;
		}

		auto const& conditions = this->state.spin.getConditions();
		if (conditions.empty()) return;

		bool validationUpdated = false;
		auto it = targetsByRepoId.find(ZRepositoryID(ev.Value.RepositoryId));
		auto targetId = it != end(targetsByRepoId) ? it->second : eTargetID::Unknown;

		for (auto i = 0; i < conditions.size(); ++i) {
			auto const& cond = conditions[i];
			auto const& target = cond.target.get();
			bool isApexPrey = isBerlinAgent(target.getID()) && isBerlinAgent(targetId);
			auto& kc = this->state.getKillConfirmation(i);

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
				Logger::Info("Invalid disguise '{}' (expected: '{}')", disguiseRepoId, reqDisguise.repoId);
			}

			if (cond.killComplication == eKillComplication::Live && kc.isPacified) {
				kc.correctMethod = eKillValidationType::Invalid;

				Logger::Info("Invalid kill, target was KO'd on death");
			}
			else if (cond.killMethod.method != eKillMethod::NONE) {
				kc.correctMethod = ValidateKillMethod(target.getID(), ev, cond.killMethod.method, cond.killType);
				if (kc.correctMethod != eKillValidationType::Valid) {
					Logger::Info("Invalid kill '{}' (type: {})", cond.killMethod.name, static_cast<int>(cond.killType));
					Logger::Info("{}", ev.json.dump());
				}
			}
			else if (cond.specificKillMethod.method != eMapKillMethod::NONE)
				kc.correctMethod = ValidateKillMethod(target.getID(), ev, cond.specificKillMethod.method, cond.killType);

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
		auto const& conditions = this->state.spin.getConditions();
		auto mission = this->state.spin.getMission();


		if (this->state.spinCompleted) return;

		LevelSetupEvent data {};
		//data.contractName = ev.Value.Contract_Name_metricvalue;
		//data.location = ev.Value.Location_MetricValue;
		data.event = ev.Value.Event_metricvalue;
		data.timestamp = ev.Timestamp;
		this->state.levelSetupEvents.push_back(std::move(data));

		if (!mission || mission->getMission() != eMission::HOKKAIDO_SITUSINVERSUS) return;
		if (ev.Value.Contract_Name_metricvalue != "SnowCrane") return;

		bool validationUpdated = false;

		for (auto i = 0; i < conditions.size(); ++i) {
			auto& cond = conditions[i];
			if (cond.target.get().getID() != eTargetID::ErichSoders) continue;

			auto& kc = this->state.getKillConfirmation(i);
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
				return this->state.getLastDisguiseChangeAtTimestamp(timestamp - delay);
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
				Logger::Info("Invalid disguise '{}' (expected: '{}')", triggerDisguiseChange->disguiseRepoId, reqDisguise.repoId);
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
	events.listen<Events::setpieces>([this](const ServerEvent<Events::setpieces>& ev) {
		if (this->state.spinCompleted) return;

		KillSetpieceEvent data{};
		data.id = ev.Value.RepositoryId;
		data.name = ev.Value.name_metricvalue;
		data.type = ev.Value.setpieceType_metricvalue;
		data.timestamp = ev.Timestamp;
		this->state.killSetpieceEvents.push_back(std::move(data));
	});

	// SA Tracking
	events.listen<Events::MurderedBodySeen>([this](const ServerEvent<Events::MurderedBodySeen>& ev) {
		if (ev.Value.IsWitnessTarget) return;
		this->state.voidSA();
	});
	events.listen<Events::Spotted>([this](const ServerEvent<Events::Spotted>& ev) {
		for (auto const& id : ev.Value.value) {
			if (!this->state.killed.contains(id))
				this->state.spottedNotKilled.insert(id);
		}
	});
	events.listen<Events::DrainPipe_climbed>([this](const ServerEvent<Events::DrainPipe_climbed>& ev) {
		this->SendCustomEvent("DrainPipeClimbed"sv, ImbuedPlayerLocation());
	});
	events.listen<Events::SecuritySystemRecorder>([this](const ServerEvent<Events::SecuritySystemRecorder>& ev) {
		switch (ev.Value.event) {
			case SecuritySystemRecorderEvent::Spotted:
				if (this->state.isCamsDestroyed) return;
				this->state.isCaughtOnCams = true;
				break;
			case SecuritySystemRecorderEvent::Destroyed:
				this->state.isCamsDestroyed = true;
				this->state.isCaughtOnCams = false;
				break;
			case SecuritySystemRecorderEvent::Erased:
				this->state.isCaughtOnCams = false;
				break;
		}
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
		// Validate ambiguous poisons that aren't "consumed" - includes medic proxy injected opportunity for Sierra Knox
		if (killClass == "poison"
			&& killMethodStrict == "")
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
			auto fireSetpieceEv = this->state.getSetpieceEventAtTimestamp(ev.Timestamp);
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
			auto lse = this->state.getLevelSetupEventByEvent("Silvio_InPlane");
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
	}
	if (target == eTargetID::YukiYamazaki) {
		if (method == eMapKillMethod::Yuki_Sauna) {
			return ev.Value.SetPieceId == "9477e941-880c-4b05-932f-d431eaeb634e"
				? eKillValidationType::Valid
				: eKillValidationType::Invalid;
		}
		if (method == eMapKillMethod::Yuki_SabotageCableCar) {
			auto lse = this->state.getLevelSetupEventByEvent("Cablecar_Down");
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
			return killContext == EDeathContext::eDC_ACCIDENT
				&& killClass == "poison"
				&& killMethodStrict == ""
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
			auto const setpiece = this->state.getSetpieceEventAtTimestamp(ev.Timestamp, 0.3);
			return setpiece != nullptr && setpiece->id == "2f4a7b8f-a5f1-4c59-8a0e-678b3c2ee32f"
				? ValidateKillMethod(target, ev, eKillMethod::Explosion, type)
				: eKillValidationType::Invalid;
		}
	}

	if (!ev.Value.KillItemRepositoryId.empty()) {
		if (type == eKillType::Thrown && ev.Value.KillMethodBroad != "throw") {
			Logger::Info("Kill validation failed. Expected 'throw', got '{}'.", ev.Value.KillMethodBroad);
			Logger::Info("{}", ev.json.dump());
			return eKillValidationType::Invalid;
		}
		if (type == eKillType::Melee && ev.Value.KillMethodBroad != "melee_lethal") {
			Logger::Info("Kill validation failed. Expected 'melee_lethal', got '{}'.", ev.Value.KillMethodBroad);
			Logger::Info("{}", ev.json.dump());
			return eKillValidationType::Invalid;
		}

		auto it = specificKillMethodsByRepoId.find(ev.Value.KillItemRepositoryId);
		if (it != end(specificKillMethodsByRepoId) && it->second == method) {
			return eKillValidationType::Valid;
		}

		if (it == end(specificKillMethodsByRepoId))
			Logger::Info("Invalid kill '{}'. Repo ID unknown.", ev.Value.KillItemRepositoryId);
		else
			Logger::Info("Invalid kill '{}'. Repo ID kill method mismatch (expected {}, got {}).", ev.Value.KillItemRepositoryId, static_cast<int>(method), static_cast<int>(it->second));
		Logger::Info("{}", ev.json.dump());
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
		return (std::ostringstream() << std::quoted(std::to_string(res->x) + "," + std::to_string(res->y) + "," + std::to_string(res->z))).str();
	}

	// Use the game method for anything we don't need to handle.
	ZString res;
	Functions::ZDynamicObject_ToString->Call(const_cast<ZDynamicObject*>(&obj), &res);
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

static std::set<std::string> eventsNotToSend = {
	"ContractStart",
	"ContractLoad",
	"ContractFailed",
	"ContractEnd",
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
	"ShotsHit",
	// Misc. Events
	"ChallengeCompleted",
	"ContractSessionMarker",
	"CpdSet",
	"Hero_Health",
	"LeaderboardUpdated",
	"Progression_XPGain",
	"SegmentClosing",
	"StartCpd",

	// Gameplay Events
	"AccidentBodyFound",
	//"Agility_Start",
	"AllBodiesHidden",
	"AmbientChanged",
	"DeadBodySeen",
	"ExitInventory",
	"FirstMissedShot",
	"FirstNonHeadshot",
	"HeroSpawn_Location",
	"HoldingIllegalWeapon", // ?
	//"ItemRemovedFromInventory",
	"MurderedBodySeen",
	"Noticed_Pacified",
	"ObjectiveCompleted",
	"PlayingPianoChanged",
	"SituationContained",
	"Unnoticed_Pacified",
	"Unnoticed_Kill",
	"VirusDestroyed",
	"Witnesses",

	"OpportunityStageEvent",
	"IntroCutEnd",
	"OpportunityEvents",
	"Spotted",
	"DisguiseBlown",
	"47_FoundTrespassing",
	"ShotsFired",

	// Imbued
	"BodyFound",
	"Dart_Hit",
	"Disguise",
	"Door_Unlocked",
	"Drain_Pipe_Climbed",
	"FriskedSuccess",
	"ItemThrown",
	"Kill",
	"Pacify",
	"StartingSuit",
	"Trespassing",
};

DEFINE_PLUGIN_DETOUR(CroupierPlugin, void, OnEventSent, ZAchievementManagerSimple* th, uint32_t eventIndex, const ZDynamicObject& ev) {
	ZString eventData = ZDynamicObjectToString(const_cast<ZDynamicObject&>(ev));

	try {
		auto json = json::parse(eventData.c_str(), eventData.c_str() + eventData.size());
		auto const eventName = json.value("Name", "");
		auto const dontPrint = eventsNotToPrint.contains(eventName);

		if (!dontPrint)
			Logger::Info("Croupier: event {}", eventData);

		this->events.handle(eventName, json);
		if (!eventsNotToSend.contains(eventName))
			State::current.client.sendRaw(eventData.c_str());
	}
	catch (const json::exception& ex) {
		Logger::Info("Error handling event: {}", eventData);
		Logger::Error("{}", eventData);
		Logger::Error("JSON exception: {}", ex.what());
	}

	return HookAction::Continue();
}

DEFINE_PLUGIN_DETOUR(CroupierPlugin, bool, OnPinOutput, ZEntityRef entity, uint32_t pinId, const ZObjectRef& data) {
	// ZHMPin::Discharge_Shot - On NPC Fire
	// ZHMPin::PlayerAllShots - On Player Fire (Twice)

	// Try: DoorBroken, NormalShot
	// ZHMPin::OnIsFullyInCrowd - Test, should work
	// ZHMPin::OnIsFullyInVegetation

	// OnItemDestroyed???

	switch (static_cast<ZHMPin>(pinId)) {
#if _DEBUG
		default:
			break;
#endif
		case ZHMPin::HitmanInVision:
			// "Never seen by targets", "Never seen by guards" etc?
			break;
		//case ZHMPin::HM_WeaponEquipped: // ZWeaponSoundSetupEntity (data: void), child of zhmitemweapon or whatever
		//case ZHMPin::OnRemovedFromContainer: // ZHM5ItemWeapon (data: void)
		//case ZHMPin::ThrowActivated: // ZThrowSoundController
		//case ZHMPin::ThrowImpact:
		case ZHMPin::OnEvacuationStarted: {
			auto vip = entity.QueryInterface<ZVIPControllerEntity>();
			if (!vip || !vip->m_rVIP) break;
			SendCustomEvent("OnEvacuationStarted"sv, ImbuedPlayerLocation(ImbuedActorInfo(vip->m_rVIP, {}, true), true));
			break;
		}
		case ZHMPin::DraggingStartMoving: {
			if (!gameplay.playerIsDragging) {
				gameplay.playerIsDragging = true;
				gameplay.sentPlayerDraggingEvent = false;
			}
			break;
		}
		case ZHMPin::DraggingStop:
		case ZHMPin::DraggingStopMoving:
			gameplay.playerIsDragging = false;
			break;
		//case ZHMPin::OnHearExplosion:
		//case ZHMPin::ShotBegin:
		//case ZHMPin::Discharge_ShotSilenced:
		case ZHMPin::PlayerAllShots: {
			auto weap = entity.QueryInterface<ZHM5ItemWeapon>();
			if (!weap) break;
			auto descriptor = weap->m_pItemConfigDescriptor;
			SendCustomEvent("PlayerShot"sv, ImbuedPlayerLocation(ImbuedItemInfo(entity)));
			break;
		}
		//case ZHMPin::SpawnPhysicsClip:
		case ZHMPin::WeaponStartReload: {
			auto weap = entity.QueryInterface<ZHM5ItemWeapon>();
			SendCustomEvent("OnWeaponReload"sv, ImbuedPlayerLocation(ImbuedItemInfo(entity)));
			break;
		}
		case ZHMPin::DoorOpen: {
			auto singleDoor = entity.QueryInterface<ZHM5SingleDoor2>();
			auto doubleDoor = entity.QueryInterface<ZHM5DoubleDoor2>();
			if (!singleDoor && !doubleDoor) break;
			SendCustomEvent("OpenDoor"sv, ImbuedPlayerLocation({
				{"Type", doubleDoor ? "Double" : "Single"}
			}));
			break;
		}
		//case ZHMPin::WeaponEquipIllegal:
		//	Logger::Debug("PIN: WeaponEquipIllegal {}", typeName);	// works!
		//	break;
		//case ZHMPin::WeaponEquipSuspicious:
		//	Logger::Debug("PIN: WeaponEquipSuspicious {}", typeName);
		//	break;
		case ZHMPin::InstinctActive:
			if (!this->state.playerInInstinct)
				SendCustomEvent("InstinctActive"sv, ImbuedPlayerLocation());
			this->state.playerInInstinct = true;
			this->state.playerInInstinctSinceFrame = true;
			break;
		case ZHMPin::ProjectileBodyShot: {
			const auto& trans = this->state.playerMatrix.Trans;
			auto pos = float4{trans.x, trans.y, trans.z, 1.0};
			if (this->gameplay.playerBodyShotPos != pos) {
			SendCustomEvent("ProjectileBodyShot"sv, ImbuedPlayerLocation());
				this->gameplay.playerBodyShotPos = pos;
			}
			break;
		}
		case ZHMPin::ExplosionAtPos: {
			auto pos = data.As<SVector3>();
			if (pos) {
				auto area = this->state.getArea(*pos);
				SendCustomEvent("Explosion"sv, ImbuedPlayerLocation({
					{"Position", {
						{"X", pos->x},
						{"Y", pos->y},
						{"Z", pos->z},
					}},
					{"Area", area ? area->ID : ""},
					{"Room", ZRoomManagerCreator::GetRoomID({pos->x, pos->y, pos->z, 1.0})},
				}, true));
			}
			else SendCustomEvent("Explosion"sv, ImbuedPlayerLocation({}, true));
			break;
		}
		case ZHMPin::OnDestroyed:
			Logger::Info("Destroyed!");
			break;
		case static_cast<ZHMPin>(4101414679): { //ZEntity > ZCompositeEntity > ZCompositeEntity - fired on e.g. fusebox destroyed
			auto initialStateOn = entity.GetProperty<bool>("m_bInitialStateOn");
			auto setpiece = entity.GetLogicalParent();
			if (!setpiece) break;
			auto spatial = setpiece.QueryInterface<ZSpatialEntity>();
			if (!spatial) break;
			const auto& trans = spatial->GetWorldMatrix().Trans;
			auto roomId = ZRoomManagerCreator::GetRoomID({ trans.x, trans.y, trans.z, 1.0 });
			auto area = this->state.getArea({ trans.x, trans.y, trans.z });
			auto obj = json {
				{"EntityID", setpiece->GetType()->m_nEntityId},
				{"Position", {
					{"X", trans.x},
					{"Y", trans.y},
					{"Z", trans.z},
				}},
				{"Room", roomId},
				{"Area", area ? area->ID : ""},
			};
			if (!initialStateOn.IsEmpty())
				obj.merge_patch({ {"InitialStateOn", initialStateOn.Get()} });
			SendCustomEvent("OnDestroy"sv, ImbuedPlayerLocation(std::move(obj), true));
			break;
		}
		case ZHMPin::OnTurnOn: {// ZEntity > ZCompositeEntity > ZCompositeEntity > ZCompositeEntity
			auto setpieceRepoIdPtr = GetValueProperty<ZRepositoryID>(entity, "m_sId");
			if (!setpieceRepoIdPtr) break;
			auto setpieceHelpersDistractionTargetedResetable = entity.GetLogicalParent();
			if (!setpieceHelpersDistractionTargetedResetable) break;
			auto initialStateOn = setpieceHelpersDistractionTargetedResetable.GetProperty<bool>("m_bInitialStateOn");
			auto setpiece = setpieceHelpersDistractionTargetedResetable.GetLogicalParent();
			if (!setpiece) break;
			auto spatial = setpiece.QueryInterface<ZSpatialEntity>();
			if (!spatial) break;
			const auto& trans = spatial->GetWorldMatrix().Trans;
			auto roomId = ZRoomManagerCreator::GetRoomID({ trans.x, trans.y, trans.z, 1.0 });
			auto area = this->state.getArea({ trans.x, trans.y, trans.z });
			auto obj = json {
				{"RepositoryId", setpieceRepoIdPtr->ToString()},
				{"EntityID", setpiece->GetType()->m_nEntityId},
				{"Position", {
					{"X", trans.x},
					{"Y", trans.y},
					{"Z", trans.z},
				}},
				{"Room", roomId},
				{"Area", area ? area->ID : ""},
			};
			if (!initialStateOn.IsEmpty())
				obj.merge_patch({ {"InitialStateOn", initialStateOn.Get()} });
			SendCustomEvent("OnTurnOn"sv, ImbuedPlayerLocation(std::move(obj), true));
			break;
		}
		case ZHMPin::OnTurnOff: {// ZEntity > ZCompositeEntity > ZCompositeEntity > ZCompositeEntity
			auto setpieceRepoIdPtr = GetValueProperty<ZRepositoryID>(entity, "m_sId");
			if (!setpieceRepoIdPtr) break;
			auto setpieceHelpersDistractionTargetedResetable = entity.GetLogicalParent();
			if (!setpieceHelpersDistractionTargetedResetable) break;
			auto initialStateOn = setpieceHelpersDistractionTargetedResetable.GetProperty<bool>("m_bInitialStateOn");
			auto setpiece = setpieceHelpersDistractionTargetedResetable.GetLogicalParent();
			if (!setpiece) break;
			auto spatial = setpiece.QueryInterface<ZSpatialEntity>();
			if (!spatial) break;
			const auto& trans = spatial->GetWorldMatrix().Trans;
			auto roomId = ZRoomManagerCreator::GetRoomID({ trans.x, trans.y, trans.z, 1.0 });
			auto area = this->state.getArea({ trans.x, trans.y, trans.z });
			auto obj = json {
				{"RepositoryId", setpieceRepoIdPtr->ToString()},
				{"EntityID", setpiece->GetType()->m_nEntityId},
				{"Position", {
					{"X", trans.x},
					{"Y", trans.y},
					{"Z", trans.z},
				}},
				{"Room", roomId},
				{"Area", area ? area->ID : ""},
			};
			if (!initialStateOn.IsEmpty())
				obj.merge_patch({ {"InitialStateOn", initialStateOn.Get()} });
			SendCustomEvent("OnTurnOff"sv, ImbuedPlayerLocation(std::move(obj), true));
			break;
		}
		case ZHMPin::OnItemDestroyed: {
			auto itemSpawner = entity.QueryInterface<ZItemSpawner>();
			if (!itemSpawner) break;
			if (!itemSpawner->m_rMainItemKey) break;
			auto repoId = itemSpawner->m_rMainItemKey.m_pInterfaceRef->m_RepositoryId.ToString();
			auto pos = itemSpawner->GetWorldMatrix().Pos;
			auto area = this->state.getArea(itemSpawner->m_mTransform.Trans);
			SendCustomEvent("ItemDestroyed"sv, ImbuedPlayerLocation({
				{"RepositoryId", repoId.c_str()},
				{"Area", area ? area->ID : ""},
				{"Room", ZRoomManagerCreator::GetRoomID(pos)},
				{"Position", {
					{"X", itemSpawner->m_mTransform.Trans.x},
					{"Y", itemSpawner->m_mTransform.Trans.y},
					{"Z", itemSpawner->m_mTransform.Trans.z},
				}},
			}, true));
			break;
		}
		case static_cast<ZHMPin>(-1680993007): { // Explode
			// We should have Vehicle_Core.Explode which is a ZEntity, the Vehicle_Core should be a ZCompositeEntity
			if (!entity.m_pEntity) break;
			auto parent = entity.GetLogicalParent();
			if (!parent || !parent.GetEntity()) break;

			// Vehicle_Core should have a Car_Size_Int prop
			auto res = parent.GetProperty<int32>("Car_Size_Int");
			if (res.IsEmpty()) break;

			auto json = json::object({ {"CarSize", res.Get()} });
			auto spatial = parent.QueryInterface<ZSpatialEntity>();
			if (spatial) {
				auto trans = spatial->m_mTransform.Trans;
				auto area = this->state.getArea(trans);
				auto roomId = ZRoomManagerCreator::GetRoomID(spatial->GetWorldMatrix().Pos);
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
			SendCustomEvent("CarExploded"sv, ImbuedPlayerLocation(std::move(json)));
			break;
		}
		case ZHMPin::OnPickup: {// ZItemSpawner pickups
			SendCustomEvent("OnPickup"sv, ImbuedPlayerLocation(ImbuedItemInfo(entity)));
			break;
		}
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
		case ZHMPin::OnAttachToHitman: { // ZHM5Item, ZHM5ItemCCWeapon, ZEntity // accomodates coins etc.
			SendCustomEvent("OnAttachToHitman"sv, ImbuedPlayerLocation(ImbuedItemInfo(entity)));
			break;
		}
		case ZHMPin::DoorBroken:
			SendCustomEvent("DoorBroken"sv, ImbuedPlayerLocation());
			break;
		case ZHMPin::OnIsFullyInCrowd:
			SendCustomEvent("OnIsFullyInCrowd"sv, ImbuedPlayerLocation());
			break;
		case ZHMPin::OnIsFullyInVegetation:
			SendCustomEvent("OnIsFullyInVegetation"sv, ImbuedPlayerLocation());
			break;
		case ZHMPin::OnTakeDamage:
		case ZHMPin::TakeDamage:
			SendCustomEvent("OnTakeDamage"sv, ImbuedPlayerLocation());
			break;
		case ZHMPin::HMMovementIndex: {
			auto moveIdx = data.As<int32>();
			if (!moveIdx) break;
			auto moveType = static_cast<PlayerMoveType>(*moveIdx);
			if (this->state.playerMoveType != moveType) {
				this->state.playerMoveType = moveType;
				SendCustomEvent("Movement"sv, ImbuedPlayerLocation());
			}
			break;
		}
		case ZHMPin::BundleDestroyed:
			// ZClothBundleSpawnEntity
			gameplay.disguiseChange.havePinData = true;
			gameplay.disguiseChange.wasFree = true;
			if (gameplay.disguiseChange.haveEventData)
				this->SendCustomEvent("Disguise"sv, ImbueDisguiseEvent(gameplay.disguiseChange.eventData));
			break;
		case ZHMPin::OnOutfitTaken: {
			// ZActorOutfitListener
			gameplay.disguiseChange.havePinData = true;
			gameplay.disguiseChange.wasFree = false;
			break;
		}
		// ONLY WORK WHILE TRESPASSING :(
		//case ZHMPin::IsCrouchWalkingSlowly:
		//	if (this->state.playerMoveType != PlayerMoveType::WalkingSlowly)
		//		SendCustomEvent("IsCrouchWalkingSlowly", {});
		//	this->state.playerMoveType = PlayerMoveType::WalkingSlowly;
		//	break;
		//case ZHMPin::IsCrouchWalking:
		//	if (this->state.playerMoveType != PlayerMoveType::CrouchWalkingSlowly)
		//		SendCustomEvent("IsCrouchWalking", {});
		//	this->state.playerMoveType = PlayerMoveType::CrouchWalkingSlowly;
		//	break;
		//case ZHMPin::IsCrouchRunning:
		//	if (this->state.playerMoveType != PlayerMoveType::CrouchRunning)
		//		SendCustomEvent("IsCrouchRunning", {});
		//	this->state.playerMoveType = PlayerMoveType::CrouchRunning;
		//	break;
		//case ZHMPin::IsRunning:
		//	if (this->state.playerMoveType != PlayerMoveType::Running)
		//		SendCustomEvent("IsRunning", {});
		//	this->state.playerMoveType = PlayerMoveType::Running;
		//	break;
		//case ZHMPin::IsWalking:
		//	if (this->state.playerMoveType != PlayerMoveType::Walking)
		//		SendCustomEvent("IsWalking", {});
		//	this->state.playerMoveType = PlayerMoveType::Walking;
		//	break;
		//case ZHMPin::IsWalkingSlowly:
		//	if (this->state.playerMoveType != PlayerMoveType::WalkingSlowly)
		//		SendCustomEvent("IsWalkingSlowly", {});
		//	this->state.playerMoveType = PlayerMoveType::WalkingSlowly;
		//	break;
	}
	return HookAction::Continue();
}

DEFINE_PLUGIN_DETOUR(CroupierPlugin, void, OnWinHttpCallback, void* dwContext, void* hInternet, void* param_3, int dwInternetStatus, void* param_5, int param_6) {
	static wchar_t buffer[200];
	DWORD size = sizeof(buffer);
	if (WinHttpQueryOption(hInternet, WINHTTP_OPTION_URL, buffer, &size)) {
		std::wstring wstr(buffer, size);
		auto url = narrow(wstr);
		std::string_view sv = url;

		auto isHttps = sv.starts_with("https://");
		if (isHttps || sv.starts_with("http://")) {
			auto urlNoProto = sv.substr((isHttps ? sizeof("https://") : sizeof("http://")) - 1);

			auto isLocal = urlNoProto.starts_with("127.0.0.1/") || urlNoProto.starts_with("localhost/");

			if (isLocal || urlNoProto.starts_with("hm3-service.hitman.io/")) {
				auto urlPath = isLocal ? urlNoProto.substr(sizeof("localhost/" /* == sizeof("127.0.0.1/") */) - 1) : urlNoProto.substr(sizeof("hm3-service.hitman.io/") - 1);

				if (urlPath.starts_with("profiles/page/Planning?contractid=")) {
					auto rest = urlPath.substr(sizeof("profiles/page/Planning?contractid="));
					auto contractId = rest.substr(0, rest.find_first_of('&'));
					auto mission = getMissionByContractId(std::string(contractId));

					if (mission != eMission::NONE) {
						State::OnMissionSelect(mission);
						if (!this->state.isPlaying)
							this->state.playerStart();
					}
				}
			}
		}
	}
	return HookAction::Continue();
}

DECLARE_ZHM_PLUGIN(CroupierPlugin);
