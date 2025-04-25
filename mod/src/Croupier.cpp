#include "Croupier.h"
#include <Logging.h>
#include <IconsMaterialDesign.h>
#include <Globals.h>
#include <Glacier/SOnlineEvent.h>
#include <Glacier/ZActor.h>
#include <Glacier/ZGameLoopManager.h>
#include <Glacier/ZInputActionManager.h>
#include <Glacier/ZScene.h>
#include <Glacier/ZString.h>
#include <chrono>
#include <variant>
#include <winhttp.h>
#include "Events.h"
#include "InputUtil.h"
#include "KillConfirmation.h"
#include "KillMethod.h"
#include "SpinParser.h"
#include "json.hpp"
#include "util.h"

#pragma comment(lib, "Winhttp.lib")

using namespace std::string_literals;
using namespace std::string_view_literals;

std::random_device rd;
std::mt19937 gen(rd());

template<typename T>
static auto randomVectorElement(const std::vector<T>& vec) -> const T&
{
	std::uniform_int_distribution<> dist(0, vec.size() - 1);
	return vec[dist(gen)];
}

Croupier::Croupier() : sharedSpin(spin), respinAction("Respin"), shuffleAction("Shuffle") {
	this->SetupEvents();
	this->rules = makeRouletteRuleset(this->ruleset);

	CHAR filename[MAX_PATH] = {};
	if (GetModuleFileName(NULL, filename, MAX_PATH) != 0)
		this->modulePath = std::filesystem::path(filename).parent_path();
}

Croupier::~Croupier() {
	this->SaveConfiguration();
	this->UninstallHooks();
}

auto Croupier::LoadConfiguration() -> void {
	/*auto binds = std::format("CroupierInput={"
			"Respin=tap("
		"};");
	auto respinKey1 = GetSetting("general", "respin_hotkey1", "");
	auto respinKey2 = GetSetting("general", "respin_hotkey2", "");
	auto shuffleKey1 = GetSetting("general", "shuffle_hotkey1", "");
	auto shuffleKey2 = GetSetting("general", "shuffle_hotkey2", "");
	respinKeyBind.key1 = respinKey1.size() ? KeyBind(respinKey1.c_str()) : KeyBind();
	respinKeyBind.key2 = respinKey2.size() ? KeyBind(respinKey2.c_str()) : KeyBind();
	shuffleKeyBind.key1 = shuffleKey1.size() ? KeyBind(shuffleKey1.c_str()) : KeyBind();
	shuffleKeyBind.key2 = shuffleKey2.size() ? KeyBind(shuffleKey2.c_str()) : KeyBind();
	if (!ZInputActionManager::AddBindings(binds.c_str()))
		Logger::Error("Failed to add input bindings for Croupier.");*/

	if (this->file.is_open()) return;

	const auto filepath = this->modulePath / "mods" / "Croupier" / "croupier.txt";
	this->file.open(filepath, std::ios::in);

	std::string content;

	if (this->file.is_open()) {
		std::stringstream ss;
		ss << this->file.rdbuf();
		content = ss.str();
		this->file.close();
	}
	else {
		this->file.clear();
		this->file.open(filepath, std::ios::out);
		this->file.close();
	}

	auto parseInt = [](std::string_view sv, int64 defaultVal = 0) {
		int64 v;
		auto res = std::from_chars(sv.data(), sv.data() + sv.size(), v);
		if (res.ec == std::errc())
			return v;
		return defaultVal;
	};
	auto parseBool = [parseInt](std::string_view sv, bool defaultVal = false) {
		bool v = defaultVal;
		if (sv == "true")
			v = true;
		else if (sv == "false")
			v = false;
		else
			v = parseInt(sv, defaultVal ? 1 : 0) ? true : false;
		return v;
	};

	bool inHistorySection = false;
	auto cmds = std::map<std::string, std::function<void (std::string_view val)>> {
		{"timer", [this, parseBool](std::string_view val) { this->config.timer = parseBool(val, this->config.timer); }},
		{"streak", [this, parseBool](std::string_view val) { this->config.streak = parseBool(val, this->config.streak); }},
		{"streak_current", [this, parseInt](std::string_view val) { this->config.streakCurrent = parseInt(val, this->config.streakCurrent); }},
		{"spin_overlay", [this, parseBool](std::string_view val) { this->config.spinOverlay = parseBool(val, this->config.spinOverlay); }},
		{"spin_overlay_dock", [this](std::string_view val) {
			if (val == "topleft") this->config.overlayDockMode = DockMode::TopLeft;
			else if (val == "topright") this->config.overlayDockMode = DockMode::TopRight;
			else if (val == "bottomleft") this->config.overlayDockMode = DockMode::BottomLeft;
			else if (val == "bottomright") this->config.overlayDockMode = DockMode::BottomRight;
			else this->config.overlayDockMode = DockMode::None;
		}},
		{"spin_overlay_confirmations", [this, parseBool](std::string_view val) { this->config.overlayKillConfirmations = parseBool(val, this->config.overlayKillConfirmations); }},
		{"ruleset", [this](std::string_view val) { this->config.ruleset = getRulesetByName(val).value_or(this->config.ruleset); }},
		{"ruleset_medium", [this, parseBool](std::string_view val) { this->config.customRules.enableMedium = parseBool(val, this->config.customRules.enableMedium); }},
		{"ruleset_hard", [this, parseBool](std::string_view val) { this->config.customRules.enableHard = parseBool(val, this->config.customRules.enableHard); }},
		{"ruleset_extreme", [this, parseBool](std::string_view val) { this->config.customRules.enableExtreme = parseBool(val, this->config.customRules.enableExtreme); }},
		{"ruleset_buggy", [this, parseBool](std::string_view val) { this->config.customRules.enableBuggy = parseBool(val, this->config.customRules.enableBuggy); }},
		{"ruleset_impossible", [this, parseBool](std::string_view val) {
			this->config.customRules.enableImpossible = parseBool(val, this->config.customRules.enableImpossible);
		}},
		{"ruleset_generic_elims", [this, parseBool](std::string_view val) {
			this->config.customRules.genericEliminations = parseBool(val, this->config.customRules.genericEliminations);
		}},
		{"ruleset_live_complications", [this, parseBool](std::string_view val) {
			this->config.customRules.liveComplications = parseBool(val, this->config.customRules.liveComplications);
		}},
		{"ruleset_live_complications_exclude_standard", [this, parseBool](std::string_view val) {
			this->config.customRules.liveComplicationsExcludeStandard = parseBool(val, this->config.customRules.liveComplicationsExcludeStandard);
		}},
		{"ruleset_live_complication_chance", [this, parseInt](std::string_view val) {
			this->config.customRules.liveComplicationChance = parseInt(val, this->config.customRules.liveComplicationChance);
		}},
		{"ruleset_melee_kill_types", [this, parseBool](std::string_view val) {
			this->config.customRules.meleeKillTypes = parseBool(val, this->config.customRules.meleeKillTypes);
		}},
		{"ruleset_thrown_kill_types", [this, parseBool](std::string_view val) {
			this->config.customRules.thrownKillTypes = parseBool(val, this->config.customRules.thrownKillTypes);
		}},
		{"mission_pool", [this](std::string_view val) {
			const auto maps = split(val, ",");
			this->config.missionPool.clear();
			for (const auto& map : maps) {
				auto mission = getMissionByCodename(std::string(trim(map)));
				if (mission != eMission::NONE)
					this->config.missionPool.push_back(mission);
			}
		}},
	};

	auto parseHistorySection = [this](std::string_view line) {
		auto spin = SpinParser::parse(line);
		if (!spin) return;

		this->spinHistory.emplace(std::move(*spin));
	};

	for (const auto& sv : split(content, "\n")) {
		if (inHistorySection)
			parseHistorySection(sv);
		else if (trim(sv) == "[history]")
			inHistorySection = true;
		else {
			auto tokens = split(sv, " ", 2);
			if (tokens.size() < 2) continue;

			auto const it = cmds.find(std::string(tokens[0]));
			if (it != cend(cmds)) it->second(trim(tokens[1]));
		}
	}
}

auto Croupier::SaveConfiguration() -> void {
	std::string content;
	const auto filepath = this->modulePath / "mods" / "Croupier" / "croupier.txt";

	this->file.open(filepath, std::ios::out | std::ios::trunc);

	auto spinOverlayDock = "none";
	switch (this->config.overlayDockMode) {
	case DockMode::TopLeft:
		spinOverlayDock = "topleft";
		break;
	case DockMode::TopRight:
		spinOverlayDock = "topright";
		break;
	case DockMode::BottomLeft:
		spinOverlayDock = "bottomleft";
		break;
	case DockMode::BottomRight:
		spinOverlayDock = "bottomright";
		break;
	}

	std::println(this->file, "timer {}", this->config.timer ? "true" : "false");
	std::println(this->file, "streak {}", this->config.streak ? "true" : "false");
	std::println(this->file, "streak_current {}", this->config.streakCurrent);
	std::println(this->file, "spin_overlay {}", this->config.spinOverlay ? "true" : "false");
	std::println(this->file, "spin_overlay_dock {}", spinOverlayDock);
	std::println(this->file, "spin_overlay_confirmations {}", this->config.overlayKillConfirmations ? "true" : "false");
	const auto rulesetName = getRulesetName(this->config.ruleset);
	if (rulesetName) std::println(this->file, "ruleset {}", rulesetName.value());
	std::println(this->file, "ruleset_medium {}", this->config.customRules.enableMedium ? "true" : "false");
	std::println(this->file, "ruleset_hard {}", this->config.customRules.enableHard ? "true" : "false");
	std::println(this->file, "ruleset_extreme {}", this->config.customRules.enableExtreme ? "true" : "false");
	std::println(this->file, "ruleset_impossible {}", this->config.customRules.enableImpossible ? "true" : "false");
	std::println(this->file, "ruleset_buggy {}", this->config.customRules.enableBuggy ? "true" : "false");
	std::println(this->file, "ruleset_generic_elims {}", this->config.customRules.genericEliminations ? "true" : "false");
	std::println(this->file, "ruleset_live_complications {}", this->config.customRules.liveComplications ? "true" : "false");
	std::println(this->file, "ruleset_live_complications_exclude_standard {}", this->config.customRules.liveComplicationsExcludeStandard ? "true" : "false");
	std::println(this->file, "ruleset_live_complication_chance {}", this->config.customRules.liveComplicationChance);
	std::println(this->file, "ruleset_melee_kill_types {}", this->config.customRules.meleeKillTypes ? "true" : "false");
	std::println(this->file, "ruleset_thrown_kill_types {}", this->config.customRules.thrownKillTypes ? "true" : "false");

	std::string mapPoolValue;
	for (const auto mission : this->config.missionPool) {
		auto codename = getMissionCodename(mission);
		if (!codename) continue;
		if (mapPoolValue.size()) mapPoolValue += ", ";
		mapPoolValue += codename.value();
	}
	std::println(this->file, "mission_pool {}", mapPoolValue);

	std::println(this->file, "");
	std::println(this->file, "[history]");

	for (const auto& spin : this->config.spinHistory) {
		auto n = 0;

		for (const auto& cond : spin.conditions) {
			if (n++) std::print(this->file, ", ");
			std::print(this->file, "{}: {} / {}", cond.targetName, cond.killMethod, cond.disguise);
		}

		std::println(this->file, "");
	}

	this->file.flush();
	this->file.close();
}

auto Croupier::ParseSpin(std::string_view sv) -> std::optional<RouletteSpin> {
	auto targetsSeparated = split(sv, ",");
	auto lastTargetMission = eMission::NONE;
	RouletteSpin spin;

	for (const auto& targetCondText : targetsSeparated) {
		auto condTextSeparatedTarget = split(targetCondText, ":");
		if (condTextSeparatedTarget.size() < 2) return std::nullopt;

		auto separatedConds = split(condTextSeparatedTarget[1], "/");
		if (condTextSeparatedTarget.size() < 2) return std::nullopt;

		auto targetName = std::string(Keyword::targetKeyToName(toUpperCase(trim(condTextSeparatedTarget[0]))));
		if (targetName.empty()) return std::nullopt;

		auto mission = getMissionForTarget(targetName);
		if (mission == eMission::NONE) return std::nullopt;

		auto missionGen = Missions::get(mission);
		if (!missionGen) return std::nullopt;

		if (lastTargetMission == eMission::NONE) spin = RouletteSpin(missionGen);
		else if (mission != lastTargetMission) return std::nullopt;

		lastTargetMission = mission;

		auto condsFirstToken = trim(separatedConds[0]);

		auto complication = eKillComplication::None;
		auto killType = eKillType::Any;
		auto killMethod = eKillMethod::NONE;
		auto mapKillMethod = eMapKillMethod::NONE;

		if (condsFirstToken.starts_with("(")) {
			auto idx = condsFirstToken.find(")");
			if (idx == condsFirstToken.npos) return std::nullopt;
			auto complicationText = std::string(condsFirstToken.substr(1, idx - 1));
			auto it = Keyword::getMap().find(toLowerCase(complicationText));
			if (it != end(Keyword::getMap()) && std::holds_alternative<eKillComplication>(it->second))
				complication = std::get<eKillComplication>(it->second);
			condsFirstToken = trim(condsFirstToken.substr(idx + 1));
		}

		if (condsFirstToken.empty()) return std::nullopt;

		auto methodTokens = split(condsFirstToken, " ");
		for (const auto& methodToken : methodTokens) {
			auto it = Keyword::getMap().find(toLowerCase(methodToken));
			if (it == end(Keyword::getMap())) {
				Logger::Error("SPIN PARSE: Unknown keyword '{}'", methodToken);
				continue;
			}
			if (!std::visit(overloaded {
				[&killType](eKillType kt) { killType = kt; return true; },
				[&killMethod](eKillMethod km) { killMethod = km; return true; },
				[&mapKillMethod](eMapKillMethod mkm) { mapKillMethod = mkm; return true; },
				[](eKillComplication kc) { return false; },
			}, it->second)) continue;
		}

		if (killMethod == eKillMethod::NONE && mapKillMethod == eMapKillMethod::NONE)
			continue;

		auto disguiseName = trim(separatedConds[1]);

		auto target = missionGen->getTargetByName(targetName);
		if (!target) return std::nullopt;

		auto disguise = missionGen->getDisguiseByName(disguiseName);
		if (!disguise) {
			Logger::Error("SPIN PARSE: Unknown disguise '{}'", disguiseName);
			return std::nullopt;
		}

		spin.add(RouletteSpinCondition(*target, *disguise, killMethod, mapKillMethod, killType, complication));
	}

	auto mission = Missions::get(lastTargetMission);

	if (spin.getConditions().size() < mission->getTargets().size())
		return std::nullopt;

	return spin;
}

auto Croupier::OnEngineInitialized() -> void {
	Logger::Info("Croupier has been initialized!");

	client = std::make_unique<CroupierClient>();
	client->start();
	this->InstallHooks();
	this->LoadConfiguration();

	if (this->config.missionPool.empty())
		this->SetDefaultMissionPool();

	this->PreviousSpin();
}

auto Croupier::InstallHooks() -> void {
	if (this->hooksInstalled) return;

	const ZMemberDelegate<Croupier, void(const SGameUpdateEvent&)> frameUpdateDelegate(this, &Croupier::OnFrameUpdate);
	Globals::GameLoopManager->RegisterFrameUpdate(frameUpdateDelegate, 0, EUpdateMode::eUpdateAlways);

	Hooks::ZLoadingScreenVideo_ActivateLoadingScreen->AddDetour(this, &Croupier::OnLoadingScreenActivated);
	Hooks::ZAchievementManagerSimple_OnEventReceived->AddDetour(this, &Croupier::OnEventReceived);
	Hooks::ZAchievementManagerSimple_OnEventSent->AddDetour(this, &Croupier::OnEventSent);
	Hooks::Http_WinHttpCallback->AddDetour(this, &Croupier::OnWinHttpCallback);

	this->hooksInstalled = true;
}

auto Croupier::UninstallHooks() -> void {
	if (!this->hooksInstalled) return;

	const ZMemberDelegate<Croupier, void(const SGameUpdateEvent&)> frameUpdateDelegate(this, &Croupier::OnFrameUpdate);
	Globals::GameLoopManager->UnregisterFrameUpdate(frameUpdateDelegate, 0, EUpdateMode::eUpdateAlways);

	Hooks::ZLoadingScreenVideo_ActivateLoadingScreen->RemoveDetour(&Croupier::OnLoadingScreenActivated);
	Hooks::ZAchievementManagerSimple_OnEventReceived->RemoveDetour(&Croupier::OnEventReceived);
	Hooks::ZAchievementManagerSimple_OnEventSent->RemoveDetour(&Croupier::OnEventSent);
	Hooks::Http_WinHttpCallback->RemoveDetour(&Croupier::OnWinHttpCallback);

	this->hooksInstalled = false;
}

auto Croupier::OnFrameUpdate(const SGameUpdateEvent& ev) -> void {
	this->ProcessSpinState();
	this->ProcessClientMessages();
	this->ProcessLoadRemoval();
	this->ProcessHotkeys();
}

auto Croupier::ProcessHotkeys() -> void {
	/*if (Functions::ZInputAction_Digital->Call(&respinAction, -1))
		Respin(false);
	if (Functions::ZInputAction_Digital->Call(&shuffleAction, -1))
		Random();

	auto respinKey1Pressed = !respinKeyBind.key1.isSet() || respinKeyBind.key1.isDown();
	auto respinKey2Pressed = !respinKeyBind.key2.isSet() || respinKeyBind.key2.isDown();
	auto shuffleKey1Pressed = !shuffleKeyBind.key1.isSet() || shuffleKeyBind.key1.isDown();
	auto shuffleKey2Pressed = !shuffleKeyBind.key2.isSet() || shuffleKeyBind.key2.isDown();
	auto respinKeybindIsPressed = (respinKeyBind.key1.isSet() || respinKeyBind.key2.isSet()) && respinKey1Pressed && respinKey2Pressed;
	auto shuffleKeybindIsPressed = (shuffleKeyBind.key1.isSet() || shuffleKeyBind.key2.isSet())
		&& (!shuffleKeyBind.key1.isSet() || shuffleKeyBind.key1.isDown())
		&& (!shuffleKeyBind.key2.isSet() || shuffleKeyBind.key2.isDown());

	if (respinKeybindIsPressed && !respinKeybindWasPressed)
		Respin();
	else if (shuffleKeybindIsPressed && !shuffleKeybindWasPressed)
		Random();

	respinKeybindWasPressed = respinKeybindIsPressed;
	shuffleKeybindWasPressed = shuffleKeybindIsPressed;*/
}

auto Croupier::ProcessSpinState() -> void {
	if (this->spinCompleted) return;
	if (this->sharedSpin.hasLoadedGame) return;
	if (this->sharedSpin.spin.getConditions().empty()) return;

	for (int i = 0; i < *Globals::NextActorId; ++i) {
		auto const& actorRef = Globals::ActorManager->m_aActiveActors[i]; 
		if (!actorRef.m_pInterfaceRef) continue;

		auto& actor = *actorRef.m_pInterfaceRef;

		auto repoEntity = actorRef.m_ref.QueryInterface<ZRepositoryItemEntity>();
		auto repoId = std::string{repoEntity->m_sId.ToString()};

		if (!actor.m_bUnk16) continue; // m_bUnk16 = is target (and still alive)
		
		auto targetId = GetTargetByRepoID(repoId);
		auto const& conditions = this->sharedSpin.spin.getConditions();

		for (auto i = 0; i < conditions.size() && i < this->sharedSpin.killValidations.size(); ++i) {
			auto& cond = conditions[i];
			auto& target = cond.target.get();
			if (targetId != target.getID()) continue;
			auto& kc = this->sharedSpin.getKillConfirmation(i);
			if (!kc.isPacified) break;

			auto isPacified = actor.IsPacified();
			auto isDead = actor.IsDead();

			if (!actor.IsPacified() && !actor.IsDead())
				kc.isPacified = false;
		}
	}
}

auto Croupier::ProcessClientMessages() -> void {
	ClientMessage message;
	if (this->client->tryTakeMessage(message)) {
		switch (message.type) {
			case eClientMessage::SpinData:
				return ProcessSpinDataMessage(message);
			case eClientMessage::Missions:
				return ProcessMissionsMessage(message);
			case eClientMessage::SpinLock:
				if (message.args.size() < 1) break;
				this->spinLocked = message.args[0] == '1';
				return;
			case eClientMessage::Streak:
				if (message.args.size() < 1) break;
				std::from_chars(message.args.c_str(), message.args.c_str() + message.args.size(), this->config.streakCurrent);
				return;
			case eClientMessage::Timer:
				{
					auto parts = split(message.args, ",", 2);
					if (parts.empty() || parts[0].empty()) return;
					auto timerStopped = 0;
					if (std::from_chars(parts[0].data(), parts[0].data() + parts[0].size(), timerStopped).ec != std::errc()) {
						if (timerStopped) this->sharedSpin.isFinished = true;
						else {
							this->sharedSpin.isPlaying = true;
							this->sharedSpin.isFinished = false;
						}
					}
					if (parts.size() < 2) return;
					uint64_t timeElapsed = 0;
					auto res = std::from_chars(parts[1].data(), parts[1].data() + parts[1].size(), timeElapsed);
					if (res.ec == std::errc()) {
						auto now = std::chrono::steady_clock::now();
						this->sharedSpin.timeStarted = now - std::chrono::milliseconds(timeElapsed);
						this->sharedSpin.timeElapsed = std::chrono::duration_cast<std::chrono::seconds>(now - this->sharedSpin.timeStarted);
					}
				}
				return;
		}
	}
}

auto Croupier::ProcessLoadRemoval() -> void {
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

auto Croupier::ProcessMissionsMessage(const ClientMessage& message) -> void {
	auto tokens = split(message.args, ",");
	this->config.missionPool.clear();
	std::string buffer;

	for (auto const& token : tokens) {
		buffer = trim(token);
		auto mission = getMissionByCodename(buffer);
		if (mission != eMission::NONE)
			this->config.missionPool.push_back(mission);
	}
}

auto Croupier::ProcessSpinDataMessage(const ClientMessage& message) -> void {
	auto spin = SpinParser::parse(message.args);
	if (!spin.has_value()) return;

	this->spin = std::move(*spin);
	this->sharedSpin.isPlaying = false;
	this->currentSpinSaved = true;
	this->generator.setMission(this->spin.getMission());
	this->spinCompleted = false;
}

auto Croupier::SendAutoSpin(eMission mission) -> void {
	this->client->send(eClientMessage::AutoSpin, {getMissionCodename(mission).value_or("").data()});
}

auto Croupier::SendRespin(eMission mission) -> void {
	this->client->send(eClientMessage::Respin, {getMissionCodename(mission).value_or("").data()});
}

auto Croupier::SendMissions() -> void {
	std::string buffer;

	for (auto const id : this->config.missionPool) {
		auto codename = getMissionCodename(id);
		if (!codename.has_value()) continue;
		if (!buffer.empty()) buffer += ",";
		buffer += *codename;
	}

	this->client->send(eClientMessage::Missions, {buffer});
}

auto Croupier::SendToggleSpinLock() -> void {
	this->client->send(eClientMessage::ToggleSpinLock);
}

auto Croupier::SendToggleTimer(bool enable) -> void {
	this->client->send(eClientMessage::ToggleTimer, {enable ? "1" : "0"});
}

auto Croupier::SendLoadStarted() -> void {
	this->client->send(eClientMessage::LoadStarted);
}

auto Croupier::SendLoadFinished() -> void {
	this->client->send(eClientMessage::LoadFinished);
}

auto Croupier::SendResetStreak() -> void {
	this->client->send(eClientMessage::ResetStreak);
}

auto Croupier::SendResetTimer() -> void {
	this->client->send(eClientMessage::ResetTimer);
}

auto Croupier::SendStartTimer() -> void {
	this->client->send(eClientMessage::StartTimer);
}

auto Croupier::SendPauseTimer(bool pause) -> void {
	this->client->send(eClientMessage::PauseTimer, {pause ? "1" : "0"});
}

auto Croupier::SendSplitTimer() -> void {
	this->client->send(eClientMessage::SplitTimer);
}

auto Croupier::SendSpinData() -> void {
	std::string data;
	std::string prefixes;
	for (const auto& cond : spin.getConditions()) {
		if (!data.empty()) data += ", ";
		if (cond.killComplication == eKillComplication::Live)
			prefixes += "(Live) ";

		switch (cond.killType) {
			case eKillType::Silenced:
				prefixes += "Sil ";
				break;
			case eKillType::Loud:
				prefixes += "Ld ";
				break;
			case eKillType::LoudRemote:
				prefixes += "Ld Remote ";
				break;
			case eKillType::Remote:
				prefixes += "Remote ";
				break;
			case eKillType::Impact:
				prefixes += "Impact ";
				break;
			case eKillType::Melee:
				prefixes += "Melee ";
				break;
			case eKillType::Thrown:
				prefixes += "Thrown ";
				break;
		}

		data += std::format("{}: {}{} / {}", Keyword::getForTarget(cond.target.get().getName()), prefixes, cond.killMethod.name, cond.disguise.get().name);
		prefixes.clear();
	}
	this->client->send(eClientMessage::SpinData, { data });
}

auto Croupier::SendNext() -> void {
	this->client->send(eClientMessage::Next);
}

auto Croupier::SendPrev() -> void {
	this->client->send(eClientMessage::Prev);
}

auto Croupier::SendRandom() -> void {
	this->client->send(eClientMessage::Random);
}

auto Croupier::SendMissionFailed() -> void {
	this->client->send(eClientMessage::MissionFailed);
}

auto Croupier::SendMissionComplete() -> void {
	this->client->send(eClientMessage::MissionComplete, {this->sharedSpin.isSA ? "1" : "0", std::to_string(this->sharedSpin.exitIGT)});
}

auto Croupier::SendMissionOutroBegin() -> void {
	this->client->send(eClientMessage::MissionOutroBegin);
}

auto Croupier::SendMissionStart(const std::string& locationId, const std::string& entranceId, const std::vector<LoadoutItemEventValue>& loadout) -> void {
	std::string loadoutStr = "";
	for (const auto& item : loadout) {
		if (!loadoutStr.empty()) loadoutStr += ",";
		loadoutStr += std::format("\"{}\"", item.RepositoryId);
	}
	this->client->send(eClientMessage::MissionStart, {entranceId, "[" + loadoutStr + "]"});
}

auto Croupier::SendKillValidationUpdate() -> void {
	auto data = ""s;
	for (const auto& cond : spin.getConditions()) {
		auto kc = this->sharedSpin.getTargetKillValidation(cond.target.get().getID());
		if (!data.empty()) data += ",";

		data += std::format(
			"{}:{}:{}:{}",
			Keyword::getForTarget(cond.target.get().getName()),
			static_cast<int>(kc.correctMethod),
			kc.correctDisguise ? 1 : 0,
			Keyword::getForTarget(kc.specificTarget)
		);
	}
	this->client->send(eClientMessage::KillValidation, { data });
}

auto Croupier::OnDrawMenu() -> void {
	// Toggle our message when the user presses our button.
	if (ImGui::Button(ICON_MD_CASINO " CROUPIER"))
		this->showUI = !this->showUI;
}

static bool ImGuiHotkey(const char* label, KeyBindAssign& bind, float samelineOffset = 0.0f, const ImVec2& size = { 100.0f, 0.0f }) noexcept {
	ImGui::TextUnformatted(label);
	ImGui::SameLine(samelineOffset);

	if (bind.assigning1) {
		ImGui::PushStyleColor(ImGuiCol_Button, ImGui::GetColorU32(ImGuiCol_ButtonActive));
		ImGui::PushStyleColor(ImGuiCol_ButtonHovered, ImGui::GetColorU32(ImGuiCol_ButtonActive));
		ImGui::Button("...", size);
		ImGui::PopStyleColor();
		ImGui::PopStyleColor();

		if (!ImGui::IsItemHovered() && ImGui::GetIO().MouseClicked[0]) {
			bind.assigning1 = false;
			return false;
		}
		if (bind.key1.setToPressedKey()) {
			bind.assigning1 = false;
			return true;
		}
	}
	else if (ImGui::Button(bind.key1.toString(), size) && !bind.assigning2)
		bind.assigning1 = true;

	ImGui::SameLine();

	if (bind.assigning2) {
		ImGui::PushStyleColor(ImGuiCol_Button, ImGui::GetColorU32(ImGuiCol_ButtonActive));
		ImGui::PushStyleColor(ImGuiCol_ButtonHovered, ImGui::GetColorU32(ImGuiCol_ButtonActive));
		ImGui::Button("...", size);
		ImGui::PopStyleColor();
		ImGui::PopStyleColor();

		if (!ImGui::IsItemHovered() && ImGui::GetIO().MouseClicked[0]) {
			bind.assigning2 = false;
			return false;
		}
		if (bind.key2.setToPressedKey()) {
			bind.assigning2 = false;
			return true;
		}
	}
	else if (ImGui::Button(bind.key2.toString(), size) && !bind.assigning1)
		bind.assigning2 = true;
	return false;
}

auto Croupier::OnDrawUI(bool focused) -> void {
	this->DrawSpinUI(focused);

	if (!focused) return;

	this->DrawEditSpinUI(focused);
	this->DrawCustomRulesetUI(focused);
	this->DrawEditMissionPoolUI(focused);
	this->DrawEditHotkeysUI(focused);

	if (!this->showUI) return;

	ImGui::PushFont(SDK()->GetImGuiBlackFont());
	ImGui::SetNextWindowContentSize(ImVec2(400, 0));

	if (ImGui::Begin(ICON_MD_SETTINGS " CROUPIER", &this->showUI, ImGuiWindowFlags_AlwaysAutoResize)) {
		auto connected = this->client->isConnected();
		ImGui::PushStyleColor(ImGuiCol_Text, connected ? IM_COL32(0, 255, 0, 255) : IM_COL32(255, 0, 0, 255));
		ImGui::TextUnformatted(connected ? "Connected" : "Disconnected");
		ImGui::PopStyleColor();

		ImGui::PushFont(SDK()->GetImGuiRegularFont());

		if (ImGui::Checkbox("Overlay", &this->config.spinOverlay))
			this->SaveConfiguration();

		ImGui::SameLine(150.0);

		auto selectedOverlayDockName = "Undocked";

		switch (this->config.overlayDockMode) {
			case DockMode::TopLeft:
				selectedOverlayDockName = "Top Left";
				break;
			case DockMode::TopRight:
				selectedOverlayDockName = "Top Right";
				break;
			case DockMode::BottomLeft:
				selectedOverlayDockName = "Bottom Left";
				break;
			case DockMode::BottomRight:
				selectedOverlayDockName = "Bottom Right";
				break;
		}

		if (ImGui::BeginCombo("##OverlayDock", selectedOverlayDockName, ImGuiComboFlags_HeightLarge)) {
			if (ImGui::Selectable("Undocked", this->config.overlayDockMode == DockMode::None, 0)) {
				this->config.overlayDockMode = DockMode::None;
				this->SaveConfiguration();
			}
			if (this->config.overlayDockMode == DockMode::None) ImGui::SetItemDefaultFocus();

			if (ImGui::Selectable("Top Left", this->config.overlayDockMode == DockMode::TopLeft, 0)) {
				this->config.overlayDockMode = DockMode::TopLeft;
				this->SaveConfiguration();
			}
			if (this->config.overlayDockMode == DockMode::TopLeft) ImGui::SetItemDefaultFocus();

			if (ImGui::Selectable("Top Right", this->config.overlayDockMode == DockMode::TopRight, 0)) {
				this->config.overlayDockMode = DockMode::TopRight;
				this->SaveConfiguration();
			}
			if (this->config.overlayDockMode == DockMode::TopRight) ImGui::SetItemDefaultFocus();

			if (ImGui::Selectable("Bottom Left", this->config.overlayDockMode == DockMode::BottomLeft, 0)) {
				this->config.overlayDockMode = DockMode::BottomLeft;
				this->SaveConfiguration();
			}
			if (this->config.overlayDockMode == DockMode::BottomLeft) ImGui::SetItemDefaultFocus();

			if (ImGui::Selectable("Bottom Right", this->config.overlayDockMode == DockMode::BottomRight, 0)) {
				this->config.overlayDockMode = DockMode::BottomRight;
				this->SaveConfiguration();
			}
			if (this->config.overlayDockMode == DockMode::BottomRight) ImGui::SetItemDefaultFocus();

			ImGui::EndCombo();
		}

		if (ImGui::Checkbox("Overlay Kill Confirmations", &this->config.overlayKillConfirmations))
			this->SaveConfiguration();

		if (!connected) {
			// Legacy timer
			if (ImGui::Checkbox("Timer", &this->config.timer))
				this->SaveConfiguration();

			if (this->config.timer) {
				ImGui::SameLine();
				ImGui::SetCursorPosX(ImGui::GetCursorPosX() + 40.0);
				if (ImGui::Button("Reset"))
					this->sharedSpin.timeStarted = std::chrono::steady_clock::now();
			}
		} else {
			// In-App Timer
			if (ImGui::Checkbox("Timer", &this->config.timer))
				this->SaveConfiguration();

			ImGui::SameLine(150.0);

			if (ImGui::Button("Reset")) {
				this->sharedSpin.timeStarted = std::chrono::steady_clock::now();
				this->SendResetTimer();
			}

			ImGui::SameLine();
			if (ImGui::Button("Start")) {
				this->sharedSpin.timeStarted = std::chrono::steady_clock::now();
				this->SendPauseTimer(false);
			}

			ImGui::SameLine();
			if (ImGui::Button("Stop")) {
				this->sharedSpin.timeStarted = std::chrono::steady_clock::now();
				this->SendPauseTimer(true);
			}

			ImGui::SameLine();
			if (ImGui::Button("Split")) {
				this->sharedSpin.timeStarted = std::chrono::steady_clock::now();
				this->SendSplitTimer();
			}
		}

		if (connected) {
			// Streak Tracking
			if (ImGui::Checkbox("Streak", &this->config.streak))
				this->SaveConfiguration();

			ImGui::SameLine(150.0);

			if (ImGui::Button("Reset##Streak")) {
				this->config.streakCurrent = 0;
				this->SendResetStreak();
			}
		}

		/*if (connected) {
			ImGui::SameLine();
			ImGui::SetCursorPosX(165.0);
			if (ImGui::Button("Hotkeys"))
				this->showEditHotkeysUI = !this->showEditHotkeysUI;
		}*/

		if (!connected) {
			// Ruleset select
			auto const rulesetName = getRulesetName(this->ruleset);
			if (ImGui::BeginCombo("##Ruleset", rulesetName.value_or("").data(), ImGuiComboFlags_HeightLarge)) {
				for (auto& info : rulesets) {
					auto const selected = this->ruleset == info.ruleset;
					auto flags = info.ruleset == eRouletteRuleset::Custom ? ImGuiSelectableFlags_Disabled : 0;

					if (ImGui::Selectable(info.name.data(), selected, flags) && info.ruleset != eRouletteRuleset::Custom)
						this->OnRulesetSelect(info.ruleset);

					// Set the initial focus when opening the combo (scrolling + keyboard navigation focus)
					if (selected) ImGui::SetItemDefaultFocus();
				}
				ImGui::EndCombo();
			}

			ImGui::SameLine();

			if (ImGui::Button("Customise"))
				this->showCustomRulesetUI = !this->showCustomRulesetUI;
		}

		auto mission = this->spin.getMission();

		auto missionInfoIt = !mission ? missionInfos.end() : std::find_if(missionInfos.begin(), missionInfos.end(), [this](const MissionInfo& info) {
			return info.mission == this->spin.getMission()->getMission();
		});
		auto const currentIdx = missionInfoIt != missionInfos.end() ? std::distance(missionInfos.begin(), missionInfoIt) : 0;
		auto const& currentMissionInfo = missionInfos[currentIdx];
		if (ImGui::BeginCombo("##Mission", currentMissionInfo.name.data(), ImGuiComboFlags_HeightLarge)) {
			for (auto& missionInfo : missionInfos) {
				auto const selected = missionInfo.mission != eMission::NONE && mission && mission->getMission() == missionInfo.mission;
				auto imGuiFlags = missionInfo.mission == eMission::NONE ? ImGuiSelectableFlags_Disabled : 0;
				if (ImGui::Selectable(missionInfo.name.data(), selected, imGuiFlags) && missionInfo.mission != eMission::NONE)
					this->OnMissionSelect(missionInfo.mission, false);

				// Set the initial focus when opening the combo (scrolling + keyboard navigation focus)
				if (selected) ImGui::SetItemDefaultFocus();
			}
			ImGui::EndCombo();
		}

		ImGui::SameLine();
		if (ImGui::Button("Edit Pool"))
			this->showEditMissionPoolUI = !this->showEditMissionPoolUI;

		ImGui::SetWindowFontScale(1.5);
		ImGui::PushFont(SDK()->GetImGuiBlackFont());

		if (connected) {
			if (ImGui::Button(ICON_MD_ARROW_BACK))
				this->SendPrev();
			if (ImGui::IsItemHovered(ImGuiHoveredFlags_AllowWhenDisabled)) ImGui::SetTooltip("Previous Spin");
			ImGui::SameLine();
			if (ImGui::Button(ICON_MD_ARROW_FORWARD))
				this->SendNext();
			if (ImGui::IsItemHovered(ImGuiHoveredFlags_AllowWhenDisabled)) ImGui::SetTooltip("Next Spin");
			ImGui::SameLine();
		}

		if (connected || this->spin.getMission()) {
			if (ImGui::Button(this->spinLocked ? ICON_MD_LOCK : ICON_MD_LOCK_OPEN)) {
				this->spinLocked = !this->spinLocked;
				this->SendToggleSpinLock();
			}
			if (ImGui::IsItemHovered(ImGuiHoveredFlags_AllowWhenDisabled)) ImGui::SetTooltip("Toggle Spin Lock");
			ImGui::SameLine();

			if (ImGui::Button(ICON_MD_EDIT))
				this->showManualModeUI = !this->showManualModeUI;
			if (ImGui::IsItemHovered(ImGuiHoveredFlags_AllowWhenDisabled)) ImGui::SetTooltip("Edit Spin");
			ImGui::SameLine();
		}

		if (ImGui::Button(ICON_MD_SHUFFLE))
			this->Random();
		if (ImGui::IsItemHovered(ImGuiHoveredFlags_AllowWhenDisabled)) ImGui::SetTooltip("Random Map/Spin");

		ImGui::SameLine();
		if (ImGui::Button(ICON_MD_REFRESH))
			this->Respin(false);
		if (ImGui::IsItemHovered(ImGuiHoveredFlags_AllowWhenDisabled)) ImGui::SetTooltip("Respin Current Map");

		ImGui::PopFont();

		if (!connected && !this->spinHistory.empty()) {
			ImGui::SameLine();

			if (ImGui::Button(ICON_MD_ARROW_LEFT))
				this->PreviousSpin();
		}

		ImGui::SetWindowFontScale(1.0);
		ImGui::PopFont();
	}

	ImGui::End();
	ImGui::PopFont();
}

auto Croupier::DrawEditHotkeysUI(bool focused) -> void {
	if (!showEditHotkeysUI) return;

	ImGui::PushFont(SDK()->GetImGuiBlackFont());

	if (ImGui::Begin(ICON_MD_CASINO " CROUPIER - HOTKEYS", &showEditHotkeysUI, ImGuiWindowFlags_AlwaysAutoResize)) {
		ImGui::PushFont(SDK()->GetImGuiBoldFont());

		if (ImGuiHotkey("Respin Hotkey", respinKeyBind)) {
			SetSetting("general", "respin_hotkey1", respinKeyBind.key1.toString());
			SetSetting("general", "respin_hotkey2", respinKeyBind.key2.toString());
		}
		if (ImGuiHotkey("Shuffle Hotkey", shuffleKeyBind)) {
			SetSetting("general", "shuffle_hotkey1", shuffleKeyBind.key1.toString());
			SetSetting("general", "shuffle_hotkey2", shuffleKeyBind.key2.toString());
		}

		ImGui::PopFont();
	}

	ImGui::End();
	ImGui::PopFont();
}

auto Croupier::DrawSpinUI(bool focused) -> void {
	if (!this->config.spinOverlay) return;

	ImGui::PushFont(SDK()->GetImGuiBlackFont());

	auto viewportSize = ImGui::GetMainViewport()->Size;
	auto flags = static_cast<ImGuiWindowFlags>(ImGuiWindowFlags_AlwaysAutoResize);

	if (this->config.overlayDockMode != DockMode::None || !focused)
		flags |= ImGuiWindowFlags_NoTitleBar;

	switch (this->config.overlayDockMode) {
		case DockMode::TopLeft:
			ImGui::SetNextWindowPos({0, 0});
			break;
		case DockMode::TopRight:
			ImGui::SetNextWindowPos({viewportSize.x - this->overlaySize.x, 0});
			break;
		case DockMode::BottomLeft:
			ImGui::SetNextWindowPos({0, viewportSize.y - this->overlaySize.y});
			break;
		case DockMode::BottomRight:
			ImGui::SetNextWindowPos({viewportSize.x - this->overlaySize.x, viewportSize.y - this->overlaySize.y});
			break;
	}

	if (ImGui::Begin(ICON_MD_CASINO " CROUPIER - SPIN", &this->config.spinOverlay, flags)) {
		this->overlaySize = ImGui::GetWindowSize();

		ImGui::PushFont(SDK()->GetImGuiBoldFont());

		auto elapsed = std::chrono::seconds::zero();
		auto& conds = this->spin.getConditions();

		elapsed = this->sharedSpin.getTimeElapsed();
		for (auto i = 0; i < conds.size(); ++i) {
			auto& cond = conds[i];
			auto kc = this->sharedSpin.getTargetKillValidation(cond.target.get().getID());
			auto validation = " - "s;

			if (this->config.overlayKillConfirmations) {
				if (kc.correctMethod == eKillValidationType::Unknown)
					validation += "Unknown"s;
				else if (kc.correctMethod == eKillValidationType::Invalid)
					validation += kc.correctDisguise ? "Wrong method" : "Wrong";
				else if (kc.correctMethod == eKillValidationType::Valid)
					validation += kc.correctDisguise ? "Done" : "Wrong disguise";
				else if (kc.correctMethod == eKillValidationType::Incomplete)
					validation = "";
			}
			else validation = "";

			auto str = std::format("{}: {} / {}{}", cond.target.get().getName(), cond.methodName, cond.disguise.get().name, validation);
			ImGui::Text(str.c_str());
		}

		auto text = std::string();

		if (this->config.timer) {
			if (!text.empty()) text += " - ";
			auto timeFormat = std::string();
			auto const includeHr = std::chrono::duration_cast<std::chrono::hours>(elapsed).count() >= 1;
			auto const time = includeHr ? std::format("{:%H:%M:%S}", elapsed) : std::format("{:%M:%S}", elapsed);
			text += time;
		}

		if (this->config.streak) {
			if (!text.empty()) text += " - ";
			text += std::format("Streak: {}", this->config.streakCurrent);
		}

		if (!text.empty()) {
			auto windowWidth = ImGui::GetWindowSize().x;
			auto textWidth = ImGui::CalcTextSize(text.c_str()).x;

			ImGui::SetCursorPosX((windowWidth - textWidth) * 0.5f);
			ImGui::Text(text.c_str());
		}

		ImGui::PopFont();
	}

	ImGui::End();
	ImGui::PopFont();
}

auto Croupier::DrawEditSpinUI(bool focused) -> void {
	if (!this->showManualModeUI) return;
	if (!this->generator.getMission()) {
		this->showManualModeUI = false;
		return;
	}

	ImGui::PushFont(SDK()->GetImGuiBlackFont());

	if (ImGui::Begin(ICON_MD_EDIT " CROUPIER - EDIT SPIN", &this->showManualModeUI, ImGuiWindowFlags_AlwaysAutoResize)) {
		ImGui::PushFont(SDK()->GetImGuiRegularFont());

		auto& mission = *this->generator.getMission();
		auto& targets = mission.getTargets();
		auto& disguises = mission.getDisguises();

		for (auto& target : targets) {
			auto currentMethod = eKillMethod::NONE;
			auto currentMapMethod = eMapKillMethod::NONE;
			auto currentKillType = eKillType::Any;
			auto currentKillMethodIsGun = false;
			auto currentKillComplication = eKillComplication::None;
			auto const isSoders = target.getType() == eTargetType::Soders;
			RouletteSpinCondition* currentCondition = nullptr;
			const RouletteDisguise* currentDisguise = nullptr;

			for (auto& cond : this->spin.getConditions()) {
				if (cond.target.get().getName() != target.getName()) continue;
				currentMethod = cond.killMethod.method;
				currentMapMethod = cond.specificKillMethod.method;
				currentKillType = cond.killType;
				currentKillMethodIsGun = cond.killMethod.isGun;
				currentKillComplication = cond.killComplication;
				currentDisguise = &cond.disguise.get();
				currentCondition = &cond;
				break;
			}

			auto const methodType = currentMapMethod != eMapKillMethod::NONE
				? eMethodType::Map
				: (currentKillMethodIsGun ? eMethodType::Gun : eMethodType::Standard);
			auto const methodName = currentMethod != eKillMethod::NONE
				? getKillMethodName(currentMethod)
				: getSpecificKillMethodName(currentMapMethod);

			ImGui::PushFont(SDK()->GetImGuiBoldFont());
			ImGui::Text(target.getName().c_str());
			ImGui::PopFont();

			if (ImGui::BeginCombo(("Method##"s + target.getName()).c_str(), methodName.data(), ImGuiComboFlags_HeightLarge)) {
				ImGui::Selectable("------ STANDARD ------", false, ImGuiSelectableFlags_Disabled);

				if (isSoders) {
					for (auto const method : this->generator.sodersKills) {
						auto const name = getSpecificKillMethodName(method);
						auto const selected = method == currentMapMethod;

						if (ImGui::Selectable(name.data(), selected)) {
							this->spin.setTargetMapMethod(target, method);
							this->SendSpinData();
						}

						if (selected) ImGui::SetItemDefaultFocus();
					}
				} else {
					for (auto const method : this->generator.standardKillMethods) {
						auto const name = getKillMethodName(method);
						auto const selected = method == currentMethod;

						if (ImGui::Selectable(name.data(), selected)) {
							this->spin.setTargetStandardMethod(target, method);
							this->SendSpinData();
						}

						if (selected) ImGui::SetItemDefaultFocus();
					}
				}

				ImGui::Selectable("------ FIREARMS ------", false, ImGuiSelectableFlags_Disabled);

				for (auto const method : this->generator.firearmKillMethods) {
					auto name = getKillMethodName(method);
					auto const selected = method == currentMethod;

					if (isSoders && isKillMethodElimination(method)) continue;

					if (ImGui::Selectable(name.data(), selected)) {
						if (selected) ImGui::SetItemDefaultFocus();
						this->spin.setTargetStandardMethod(target, method);
						this->SendSpinData();
					}

					if (selected) ImGui::SetItemDefaultFocus();
				}

				if (!isSoders) {
					ImGui::Selectable("------ MAP METHODS ------", false, ImGuiSelectableFlags_Disabled);

					for (auto const& method : this->generator.getMission()->getMapKillMethods()) {
						auto const selected = method.method == currentMapMethod;

						if (ImGui::Selectable(method.name.data(), selected)) {
							this->spin.setTargetMapMethod(target, method.method);
							this->SendSpinData();
						}

						if (selected) ImGui::SetItemDefaultFocus();
					}
				}

				ImGui::EndCombo();
			}

			ImGui::SameLine();
			auto const hasKillTypes = methodType != eMethodType::Standard || currentMethod == eKillMethod::Explosive;
			auto const killTypeLabel = hasKillTypes ? getKillTypeName(currentKillType) : "-------"sv;
			if (ImGui::BeginCombo(("Type##"s + target.getName()).c_str(), killTypeLabel.empty() ? "(Any)" : killTypeLabel.data(), ImGuiComboFlags_HeightLarge)) {
				if (hasKillTypes) {
					auto const& killTypes = methodType == eMethodType::Map
						? this->generator.meleeKillTypes
						: (methodType == eMethodType::Gun ? this->generator.gunKillTypes : this->generator.explosiveKillTypes);

					for (auto type : killTypes) {
						auto const selected = currentKillType == type;
						auto const name = getKillTypeName(type);
						if (ImGui::Selectable(name.empty() ? "(Any)" : name.data(), selected)) {
							this->spin.setTargetMethodType(target, type);
							this->SendSpinData();
						}

						if (selected) ImGui::SetItemDefaultFocus();
					}
				}

				ImGui::EndCombo();
			}

			if (ImGui::BeginCombo(("Disguise##"s + target.getName()).c_str(), currentDisguise ? currentDisguise->name.c_str() : nullptr, ImGuiComboFlags_HeightLarge)) {
				for (auto const& disguise : disguises) {
					auto const selected = currentDisguise == &disguise;
					if (ImGui::Selectable(disguise.name.c_str(), selected)) {
						this->spin.setTargetDisguise(target, disguise);
						this->SendSpinData();
					}

					if (selected) ImGui::SetItemDefaultFocus();
				}

				ImGui::EndCombo();
			}

			ImGui::SameLine();

			bool liveKill = currentKillComplication == eKillComplication::Live;
			if (ImGui::Checkbox(("Live (No KO)##"s + target.getName()).c_str(), &liveKill)) {
				this->spin.setTargetComplication(target, liveKill ? eKillComplication::Live : eKillComplication::None);
				this->SendSpinData();
			}
		}

		ImGui::PopFont();
	}

	ImGui::End();
	ImGui::PopFont();
}

auto Croupier::DrawCustomRulesetUI(bool focused) -> void {
	if (!this->showCustomRulesetUI) return;

	ImGui::PushFont(SDK()->GetImGuiBlackFont());

	if (ImGui::Begin(ICON_MD_EDIT " CROUPIER - CONFIGURE RULESET", &this->showCustomRulesetUI, ImGuiWindowFlags_AlwaysAutoResize)) {
		ImGui::PushFont(SDK()->GetImGuiRegularFont());

		ImGui::PushFont(SDK()->GetImGuiBoldFont());
		ImGui::TextUnformatted("Toggle conditions of particular difficulty levels, regardless of RR ban status.");
		ImGui::PopFont();

		ImGui::TextUnformatted("'Medium' conditions require decent map knowledge but aren't too hard to pull off (Yuki fire).");

		if (ImGui::Checkbox("Enable medium conditions", &this->rules.enableMedium))
			this->OnRulesetCustomised();

		ImGui::TextUnformatted("'Hard' conditions require good routing, practice and some tricks (Claus prisoner, Bangkok stalker).");

		if (ImGui::Checkbox("Enable hard conditions", &this->rules.enableHard))
			this->OnRulesetCustomised();

		ImGui::TextUnformatted("'Extreme' conditions may require glitches, huge time investment or advanced tricks (Paris fire, WC Beak Staff etc.).");

		if (ImGui::Checkbox("Enable extreme conditions", &this->rules.enableExtreme))
			this->OnRulesetCustomised();

		ImGui::TextUnformatted("'Buggy' conditions have been found to encounter game bugs sometimes.");

		if (ImGui::Checkbox("Enable buggy conditions", &this->rules.enableBuggy))
			this->OnRulesetCustomised();

		ImGui::TextUnformatted("'Impossible' conditions are unfeasible due to objects not existing near target routes (Sapienza fire, etc.).");

		if (ImGui::Checkbox("Enable 'impossible' conditions", &this->rules.enableImpossible))
			this->OnRulesetCustomised();

		ImGui::PushFont(SDK()->GetImGuiBoldFont());
		ImGui::TextUnformatted("Toggle certain condition types from being in spins.");
		ImGui::PopFont();

		if (ImGui::Checkbox("Generic eliminations", &this->rules.genericEliminations))
			this->OnRulesetCustomised();

		if (ImGui::Checkbox("Live complications", &this->rules.liveComplications))
			this->OnRulesetCustomised();
		ImGui::SameLine();
		if (ImGui::Checkbox("Exclude 'Standard' kills", &this->rules.liveComplicationsExcludeStandard))
			this->OnRulesetCustomised();

		if (this->rules.liveComplications) {
			if (ImGui::SliderInt("Live complication chance", &this->rules.liveComplicationChance, 0, 100, "%d%%", ImGuiSliderFlags_AlwaysClamp))
				this->OnRulesetCustomised();
		}

		if (ImGui::Checkbox("'Melee' kill types", &this->rules.meleeKillTypes))
			this->OnRulesetCustomised();
		ImGui::SameLine();
		if (ImGui::Checkbox("'Thrown' kill types", &this->rules.thrownKillTypes))
			this->OnRulesetCustomised();

		ImGui::PopFont();
	}

	ImGui::End();
	ImGui::PopFont();
}

auto Croupier::DrawEditMissionPoolUI(bool focused) -> void {
	if (!this->showEditMissionPoolUI) return;

	ImGui::PushFont(SDK()->GetImGuiBlackFont());

	if (ImGui::Begin(ICON_MD_EDIT " CROUPIER - EDIT MISSION POOL", &this->showEditMissionPoolUI, ImGuiWindowFlags_AlwaysAutoResize)) {
		ImGui::PushFont(SDK()->GetImGuiRegularFont());

		ImGui::TextUnformatted("Select which missions will be in the pool for randomisation.");

		auto i = 0;

		for (auto& missionInfo : missionInfos) {
			if (missionInfo.mission == eMission::NONE) {
				i = 0;
				ImGui::NewLine();
				ImGui::PushFont(SDK()->GetImGuiBoldFont());
				ImGui::TextUnformatted(missionInfo.simpleName.data());
				ImGui::PopFont();
				continue;
			}

			if (++i % 3 != 1)
				ImGui::SameLine(320 * ((i - 1) % 3));

			auto it = find(cbegin(this->config.missionPool), cend(this->config.missionPool), missionInfo.mission);
			auto enabled = it != cend(this->config.missionPool);
			if (ImGui::Checkbox(missionInfo.name.data(), &enabled)) {
				auto it = remove(begin(this->config.missionPool), end(this->config.missionPool), missionInfo.mission);
				if (it != end(this->config.missionPool)) this->config.missionPool.erase(it, end(this->config.missionPool));
				if (enabled) this->config.missionPool.push_back(missionInfo.mission);
				SendMissions();
				SaveConfiguration();
			}
		}

		ImGui::PopFont();
	}

	ImGui::End();
	ImGui::PopFont();
}

auto Croupier::OnRulesetCustomised() -> void {
	if (RouletteRuleset::compare(this->rules, makeRouletteRuleset(eRouletteRuleset::RRWC2023)))
		this->OnRulesetSelect(eRouletteRuleset::RRWC2023);
	else if (RouletteRuleset::compare(this->rules, makeRouletteRuleset(eRouletteRuleset::RR12)))
		this->OnRulesetSelect(eRouletteRuleset::RR12);
	else if (RouletteRuleset::compare(this->rules, makeRouletteRuleset(eRouletteRuleset::RR11)))
		this->OnRulesetSelect(eRouletteRuleset::RR11);
	else if (RouletteRuleset::compare(this->rules, makeRouletteRuleset(eRouletteRuleset::Normal)))
		this->OnRulesetSelect(eRouletteRuleset::Normal);
	else
		this->OnRulesetSelect(eRouletteRuleset::Custom);
}

auto Croupier::OnRulesetSelect(eRouletteRuleset ruleset) -> void {
	this->ruleset = ruleset;
	if (ruleset != eRouletteRuleset::Custom)
		this->rules = makeRouletteRuleset(ruleset);
	try {
		this->generator.setRuleset(&this->rules);
	} catch (const RouletteGeneratorException& ex) {
		Logger::Error("Croupier: {}", ex.what());
	}
}

auto Croupier::OnMissionSelect(eMission mission, bool isAuto) -> void {
	this->sharedSpin.playerSelectMission();
	auto currentMission = this->spin.getMission();
	if (currentMission && mission == currentMission->getMission() && !this->spinCompleted) return;
	this->spinCompleted = false;

	try {
		this->generator.setMission(Missions::get(mission));
		this->Respin(isAuto);
	} catch (const RouletteGeneratorException& ex) {
		Logger::Error("Croupier: {}", ex.what());
	}
}

auto Croupier::SaveSpinHistory() -> void {
	if (!this->generator.getMission()) return;
	if (this->spin.getConditions().empty()) return;

	if (!this->currentSpinSaved) {
		SerializedSpin spin;

		for (const auto& cond : this->spin.getConditions()) {
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

	this->SaveConfiguration();
}

auto Croupier::OnFinishMission() -> void {
	if (!this->generator.getMission()) return;
	if (this->spin.getConditions().empty()) return;
}

auto Croupier::SetDefaultMissionPool() -> void {
	this->config.missionPool = defaultMissionPool;
}

auto Croupier::PreviousSpin() -> void {
	if (this->spinHistory.empty()) return;

	this->spin = std::move(this->spinHistory.top());
	this->sharedSpin.isPlaying = false;
	this->currentSpinSaved = true;
	this->generator.setMission(this->spin.getMission());
	this->spinHistory.pop();
	this->spinCompleted = false;

	this->sharedSpin.playerStart();
	this->LogSpin();
}

auto Croupier::Random() -> void {
	if (this->client->isConnected()) {
		this->SendRandom();
		return;
	}

	if (this->config.missionPool.empty())
		this->SetDefaultMissionPool();
	if (this->config.missionPool.empty())
		return;

	auto mission = randomVectorElement(this->config.missionPool);
	auto currentMission = this->spin.getMission();

	if (currentMission && mission == currentMission->getMission())
		this->Respin(false);
	else
		this->OnMissionSelect(mission, false);
}

auto Croupier::Respin(bool isAuto) -> void {
	if (!this->generator.getMission()) return;

	if (isAuto)
		SendAutoSpin(this->generator.getMission()->getMission());
	else
		SendRespin(this->generator.getMission()->getMission());

	this->generator.setRuleset(&this->rules);

	if (isAuto && this->spinLocked) return;
	if (this->client->isConnected()) return;

	try {
		if (!this->spin.getConditions().empty()) {
			this->spinHistory.emplace(std::move(this->spin));
		}

		this->spin = this->generator.spin(&this->spin);
		this->sharedSpin.timeStarted = std::chrono::steady_clock::now();
		this->currentSpinSaved = false;
		this->spinCompleted = false;
	} catch (const std::runtime_error& ex) {
		Logger::Error("Croupier: {}", ex.what());
	}

	this->LogSpin();
	this->SaveSpinHistory();
}

auto Croupier::LogSpin() -> void {
	std::string spinText;
	for (auto& cond : this->spin.getConditions()) {
		if (!spinText.empty()) spinText += " || ";
		if (cond.killMethod.name.empty()) spinText += "***";
		spinText += std::format("{}: {} / {}", cond.target.get().getName(), cond.methodName, cond.disguise.get().name);
	}

	Logger::Info("Croupier: {}", spinText);
}

auto lastThrownItem = ""s;

auto Croupier::SetupEvents() -> void {
	events.listen<Events::ContractStart>([this](const ServerEvent<Events::ContractStart>& ev) {
		this->sharedSpin.playerStart();
		this->sharedSpin.locationId = ev.Value.LocationId;
		this->sharedSpin.loadout = ev.Value.Loadout;

		this->SendKillValidationUpdate();
	});
	events.listen<Events::HeroSpawn_Location>([this](const ServerEvent<Events::HeroSpawn_Location>& ev) {
		this->SendMissionStart(this->sharedSpin.locationId, ev.Value.RepositoryId, this->sharedSpin.loadout);
	});
	events.listen<Events::IntroCutEnd>([this](const ServerEvent<Events::IntroCutEnd>& ev) {
		this->sharedSpin.playerCutsceneEnd(ev.Timestamp);
	});
	events.listen<Events::ContractLoad>([this](auto& ev) {
		this->sharedSpin.playerLoad();
		this->SendKillValidationUpdate();
	});
	events.listen<Events::ExitGate>([this](const ServerEvent<Events::ExitGate>& ev) {
		this->sharedSpin.playerExit(ev.Timestamp);

		// Mark any unfulfilled kill methods as invalid (never killed a Berlin agent with correct requirements, destroyed heart instead of killing Soders or vice-versa, etc.)
		auto const& conds = spin.getConditions();
		for (auto& kv : this->sharedSpin.killValidations) {
			if (kv.correctMethod == eKillValidationType::Incomplete)
				kv.correctMethod = eKillValidationType::Invalid;
		}

		this->SendKillValidationUpdate();
		this->SendMissionComplete();
	});
	events.listen<Events::ExitTango>([this](const ServerEvent<Events::ExitTango>& ev) {
		this->SendMissionOutroBegin();
	});
	events.listen<Events::FacilityExitEvent>([this](const ServerEvent<Events::FacilityExitEvent>& ev) {
		this->sharedSpin.playerExit(ev.Timestamp);

		// Mark any unfulfilled kill methods as invalid (never killed a Berlin agent with correct requirements, destroyed heart instead of killing Soders or vice-versa, etc.)
		auto const& conds = spin.getConditions();
		for (auto& kv : this->sharedSpin.killValidations)
		{
			if (kv.correctMethod == eKillValidationType::Incomplete)
				kv.correctMethod = eKillValidationType::Invalid;
		}

		this->SendKillValidationUpdate();
		this->SendMissionComplete();
	});
	events.listen<Events::ContractEnd>([this](const ServerEvent<Events::ContractEnd>& ev) {
		if (!this->sharedSpin.isFinished) {
			this->sharedSpin.playerExit(ev.Timestamp);

			// Mark any unfulfilled kill methods as invalid (never killed a Berlin agent with correct requirements, destroyed heart instead of killing Soders or vice-versa, etc.)
			auto const& conds = spin.getConditions();
			for (auto& kv : this->sharedSpin.killValidations) {
				if (kv.correctMethod == eKillValidationType::Incomplete)
					kv.correctMethod = eKillValidationType::Invalid;
			}

			this->SendKillValidationUpdate();
			this->SendMissionComplete();
		}

		this->spinCompleted = true;
	});
	events.listen<Events::ContractFailed>([this](const ServerEvent<Events::ContractFailed>& ev) {
		this->SendMissionFailed();
		Logger::Info("Croupier: ContractFailed {}", ev.Value.value.dump());
	});
	events.listen<Events::StartingSuit>([this](const ServerEvent<Events::StartingSuit>& ev) {
		if (this->spinCompleted) return;
		this->sharedSpin.disguiseChanges.emplace_back(ev.Value.value, ev.Timestamp);
	});
	events.listen<Events::Disguise>([this](const ServerEvent<Events::Disguise>& ev) {
		if (this->spinCompleted) return;
		this->sharedSpin.disguiseChanges.emplace_back(ev.Value.value, ev.Timestamp);
	});
	events.listen<Events::ItemThrown>([this](const ServerEvent<Events::ItemThrown>& ev) {
		lastThrownItem = ev.Value.RepositoryId;
	});
	events.listen<Events::Pacify>([this](const ServerEvent<Events::Pacify>& ev) {
		if (!ev.Value.IsTarget) return;
		if (this->spinCompleted) return;

		auto const& conditions = this->sharedSpin.spin.getConditions();
		if (conditions.empty()) return;

		auto targetId = GetTargetByRepoID(ev.Value.RepositoryId);

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

			auto& kc = this->sharedSpin.getKillConfirmation(i);
			kc.target = target.getID();
			kc.isPacified = true;
		}
	});
	events.listen<Events::C_Hungry_Hippo>([this](const ServerEvent<Events::C_Hungry_Hippo>& ev) {
		if (this->spinCompleted) return;
		auto const mission = this->sharedSpin.spin.getMission();
		if (!mission) return;
		if (mission->getMission() != eMission::SANTAFORTUNA_THREEHEADEDSERPENT) return;

		auto const& conditions = this->sharedSpin.spin.getConditions();
		if (conditions.empty()) return;

		for (auto i = 0; i < conditions.size(); ++i) {
			auto const& cond = conditions[i];
			if (cond.target.get().getID() != eTargetID::RicoDelgado) continue;
			if (cond.specificKillMethod.method != eMapKillMethod::Rico_FeedToHippo) return;
			
			auto const& target = cond.target.get();
			auto& kc = this->sharedSpin.getKillConfirmation(i);
			kc.target = eTargetID::RicoDelgado;
			kc.correctMethod = eKillValidationType::Valid;
			this->SendKillValidationUpdate();
			break;
		}
	});
	events.listen<Events::TargetEscapeFoiled>([this](const ServerEvent<Events::TargetEscapeFoiled>& ev) {
		if (this->spinCompleted) return;
		auto const mission = this->sharedSpin.spin.getMission();
		if (!mission) return;
		if (mission->getMission() != eMission::HOKKAIDO_SITUSINVERSUS) return;

		auto const& conditions = this->sharedSpin.spin.getConditions();
		if (conditions.empty()) return;

		for (auto i = 0; i < conditions.size(); ++i) {
			auto const& cond = conditions[i];
			if (cond.target.get().getID() != eTargetID::YukiYamazaki) continue;
			if (cond.specificKillMethod.method != eMapKillMethod::Yuki_SabotageCableCar) return;
			
			auto const& target = cond.target.get();
			auto& kc = this->sharedSpin.getKillConfirmation(i);
			kc.target = eTargetID::YukiYamazaki;
			kc.correctMethod = eKillValidationType::Valid;
			this->SendKillValidationUpdate();
			break;
		}
	});
	events.listen<Events::Kill>([this](const ServerEvent<Events::Kill>& ev) {
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

		if (this->spinCompleted) return;

		this->sharedSpin.killed.insert(ev.Value.RepositoryId);
		this->sharedSpin.spottedNotKilled.erase(ev.Value.RepositoryId);

		if (!ev.Value.IsTarget) {
			if (ev.Value.KillContext != EDeathContext::eDC_NOT_HERO)
				this->sharedSpin.voidSA();
			return;
		}

		auto const& conditions = this->sharedSpin.spin.getConditions();
		if (conditions.empty()) return;

		bool validationUpdated = false;
		auto it = targetsByRepoId.find(ev.Value.RepositoryId);
		auto targetId = it != end(targetsByRepoId) ? it->second : eTargetID::Unknown;

		for (auto i = 0; i < conditions.size(); ++i) {
			auto const& cond = conditions[i];
			auto const& target = cond.target.get();
			bool isApexPrey = isBerlinAgent(target.getID()) && isBerlinAgent(targetId);
			auto& kc = this->sharedSpin.getKillConfirmation(i);

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

			Logger::Debug("Killed '{}'", ev.Value.ActorName);
		}

		if (validationUpdated) this->SendKillValidationUpdate();
	});
	events.listen<Events::Level_Setup_Events>([this](const ServerEvent<Events::Level_Setup_Events>& ev) {
		auto const& conditions = this->sharedSpin.spin.getConditions();
		auto mission = this->sharedSpin.spin.getMission();


		if (this->spinCompleted) return;

		LevelSetupEvent data {};
		//data.contractName = ev.Value.Contract_Name_metricvalue;
		//data.location = ev.Value.Location_MetricValue;
		data.event = ev.Value.Event_metricvalue;
		data.timestamp = ev.Timestamp;
		this->sharedSpin.levelSetupEvents.push_back(std::move(data));

		if (!mission || mission->getMission() != eMission::HOKKAIDO_SITUSINVERSUS) return;
		if (ev.Value.Contract_Name_metricvalue != "SnowCrane") return;

		bool validationUpdated = false;

		for (auto i = 0; i < conditions.size(); ++i) {
			auto& cond = conditions[i];
			if (cond.target.get().getID() != eTargetID::ErichSoders) continue;

			auto& kc = this->sharedSpin.getKillConfirmation(i);
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
				return this->sharedSpin.getLastDisguiseChangeAtTimestamp(timestamp - delay);
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

		if (validationUpdated) this->SendKillValidationUpdate();
	});
	events.listen<Events::setpieces>([this](const ServerEvent<Events::setpieces>& ev) {
		if (this->spinCompleted) return;

		KillSetpieceEvent data{};
		data.id = ev.Value.RepositoryId;
		data.name = ev.Value.name_metricvalue;
		data.type = ev.Value.setpieceType_metricvalue;
		data.timestamp = ev.Timestamp;
		this->sharedSpin.killSetpieceEvents.push_back(std::move(data));
	});

	// SA Tracking
	events.listen<Events::MurderedBodySeen>([this](const ServerEvent<Events::MurderedBodySeen>& ev) {
		if (ev.Value.IsWitnessTarget) return;
		this->sharedSpin.voidSA();
	});
	events.listen<Events::Spotted>([this](const ServerEvent<Events::Spotted>& ev) {
		for (auto const& id : ev.Value.value) {
			if (!this->sharedSpin.killed.contains(id))
				this->sharedSpin.spottedNotKilled.insert(id);
		}
	});
	events.listen<Events::SecuritySystemRecorder>([this](const ServerEvent<Events::SecuritySystemRecorder>& ev) {
		switch (ev.Value.event) {
			case SecuritySystemRecorderEvent::Spotted:
				if (this->sharedSpin.isCamsDestroyed) return;
				this->sharedSpin.isCaughtOnCams = true;
				break;
			case SecuritySystemRecorderEvent::Destroyed:
				this->sharedSpin.isCamsDestroyed = true;
				this->sharedSpin.isCaughtOnCams = false;
				break;
			case SecuritySystemRecorderEvent::Erased:
				this->sharedSpin.isCaughtOnCams = false;
				break;
		}
	});
}

auto Croupier::ValidateKillMethod(eTargetID target, const ServerEvent<Events::Kill>& ev, eKillMethod method, eKillType type) -> eKillValidationType {
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
		if (type == eKillType::Impact) {
			// Check for molotov burn kills...
			if (haveDamageEvents
				&& ev.Value.DamageEvents[0] == "Burn"
				&& !haveKillMethod
				&& !isAccident)
				return eKillValidationType::Valid;
		}
		if (type == eKillType::Loud || type == eKillType::Impact) {
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
		// killItemRepoId = id of poison item
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
			// Only one proven to present this issue (the FO by the lead actor's puke spot)
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
			auto fireSetpieceEv = this->sharedSpin.getSetpieceEventAtTimestamp(ev.Timestamp);
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

auto Croupier::ValidateKillMethod(eTargetID target, const ServerEvent<Events::Kill>& ev, eMapKillMethod method, eKillType type) -> eKillValidationType {
	if (target == eTargetID::SilvioCaruso) {
		// {"Timestamp":173.116608,"Name":"Kill","ContractSessionId":"2516591008337813079-9ea716b6-6798-4687-ba22-3bb8d89cce9b","ContractId":"00000000-0000-0000-0000-000000000600","Value":{"RepositoryId":"0dfaea51-3c36-4722-9eff-f1e7ef139878","ActorId":2739847461.000000,"ActorName":"Silvio Caruso","ActorType":0.000000,"KillType":4.000000,"KillContext":3.000000,"KillClass":"unknown","Accident":true,"WeaponSilenced":false,"Explosive":false,"ExplosionType":0.000000,"Projectile":false,"Sniper":false,"IsHeadshot":false,"IsTarget":true,"ThroughWall":false,"BodyPartId":-1.000000,"TotalDamage":100000.000000,"IsMoving":false,"RoomId":182.000000,"ActorPosition":"-105.703, -175.75, -0.775244","HeroPosition":"-110.366, -119.28, 16.0837","DamageEvents":[],"PlayerId":4294967295.000000,"OutfitRepositoryId":"fd56a934-f402-4b52-bdca-8bbc737400ff","OutfitIsHitmanSuit":false,"EvergreenRarity":-1.000000,"KillMethodBroad":"","KillMethodStrict":"","IsReplicated":true,"History":[]},"UserId":"b1585b4d-36f0-48a0-8ffa-1b72f01759da","SessionId":"61e82efa0bcb4a3088825dd75e115f61-468215834","Origin":"gameclient","Id":"c5d04012-68a1-473a-8769-3a0c3b9da097"}
		if (method == eMapKillMethod::Silvio_SeaPlane) {
			// Best we can really do is just check Silvio ever entered the plane
			// and the kill was an accident (it is possible to kill him directly with explosive while he is in the plane).
			auto lse = this->sharedSpin.getLevelSetupEventByEvent("Silvio_InPlane");
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
			auto lse = this->sharedSpin.getLevelSetupEventByEvent("Cablecar_Down");
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
		auto const isKillClassUnknown = killClass == "unknown";
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
			auto const setpiece = this->sharedSpin.getSetpieceEventAtTimestamp(ev.Timestamp, 0.3);
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

DEFINE_PLUGIN_DETOUR(Croupier, void*, OnLoadingScreenActivated, void* th, void* a1) {
	loadingScreenActivated = true;
	if (!loadRemovalActive) {
		SendLoadStarted();
		loadRemovalActive = true;
	}
	return HookResult<void*>(HookAction::Continue());
}

DEFINE_PLUGIN_DETOUR(Croupier, void, OnEventReceived, ZAchievementManagerSimple* th, const SOnlineEvent& event) {
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
	"Actorsick",
	"Agility_Start",
	"AllBodiesHidden",
	"AmbientChanged",
	"BlueEgg",
	"BodyBagged",
	"BodyFound",
	"BodyHidden",
	"Dart_Hit",
	"DeadBodySeen",
	"Disguise",
	"Door_Unlocked",
	"DrainPipe_climbed",
	"Drain_Pipe_Climbed",
	"EvidenceHidden",
	"ExitInventory",
	"FirstMissedShot",
	"FirstNonHeadshot",
	"HeroSpawn_Location",
	"HoldingIllegalWeapon",
	"Investigate_Curious",
	"ItemDropped",
	"ItemPickedUp",
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
	"Trespassing",
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

DEFINE_PLUGIN_DETOUR(Croupier, void, OnEventSent, ZAchievementManagerSimple* th, uint32_t eventIndex, const ZDynamicObject& ev) {
	ZString eventData = ZDynamicObjectToString(const_cast<ZDynamicObject&>(ev));

	try {
		auto json = nlohmann::json::parse(eventData.c_str(), eventData.c_str() + eventData.size());
		auto const eventName = json.value("Name", "");
		auto const dontPrint = eventsNotToPrint.contains(eventName);

		if (!dontPrint)
			Logger::Info("Croupier: event {}", eventData);

		this->events.handle(eventName, json);
	}
	catch (const nlohmann::json::exception& ex) {
		Logger::Info("Error handling event: {}", eventData);
		Logger::Error("{}", eventData);
		Logger::Error("JSON exception: {}", ex.what());
	}

	return HookResult<void>(HookAction::Continue());
}

DEFINE_PLUGIN_DETOUR(Croupier, void, OnWinHttpCallback, void* dwContext, void* hInternet, void* param_3, int dwInternetStatus, void* param_5, int param_6) {
	static wchar_t buffer[200];
	DWORD size = sizeof(buffer);
	if (WinHttpQueryOption(hInternet, WINHTTP_OPTION_URL, buffer, &size)) {
		std::wstring wstr(buffer, size);
		//Logger::Info("WinHttpQueryOption URL: {}", narrow(wstr));
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
						this->OnMissionSelect(mission);
						if (!this->sharedSpin.isPlaying)
							this->sharedSpin.playerStart();
					}
				}
			}
		}
	}
	//else Logger::Error("WinHttpQueryOption failed: {}", GetLastError());
	return HookResult<void>(HookAction::Continue());
}

DECLARE_ZHM_PLUGIN(Croupier);
