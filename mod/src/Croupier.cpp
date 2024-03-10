#include "Croupier.h"
#include <Logging.h>
#include <IconsMaterialDesign.h>
#include <Globals.h>
#include <Glacier/SOnlineEvent.h>
#include <Glacier/ZActor.h>
#include <Glacier/ZGameLoopManager.h>
#include <Glacier/ZScene.h>
#include <Glacier/ZString.h>
#include <chrono>
#include <variant>
#include <winhttp.h>
#include "Events.h"
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

Croupier::Croupier() : sharedSpin(spin) {
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
		{"spin_overlay", [this, parseBool](std::string_view val) { this->config.spinOverlay = parseBool(val, this->config.spinOverlay); }},
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

	std::println(this->file, "timer {}", this->config.timer ? "true" : "false");
	std::println(this->file, "spin_overlay {}", this->config.spinOverlay ? "true" : "false");
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
	Globals::GameLoopManager->RegisterFrameUpdate(frameUpdateDelegate, 0, EUpdateMode::eUpdatePlayMode);

	Hooks::ZAchievementManagerSimple_OnEventReceived->AddDetour(this, &Croupier::OnEventReceived);
	Hooks::ZAchievementManagerSimple_OnEventSent->AddDetour(this, &Croupier::OnEventSent);
	Hooks::Http_WinHttpCallback->AddDetour(this, &Croupier::OnWinHttpCallback);

	this->hooksInstalled = true;
}

auto Croupier::UninstallHooks() -> void {
	if (!this->hooksInstalled) return;

	const ZMemberDelegate<Croupier, void(const SGameUpdateEvent&)> frameUpdateDelegate(this, &Croupier::OnFrameUpdate);
	Globals::GameLoopManager->UnregisterFrameUpdate(frameUpdateDelegate, 0, EUpdateMode::eUpdatePlayMode);

	Hooks::ZAchievementManagerSimple_OnEventReceived->RemoveDetour(&Croupier::OnEventReceived);
	Hooks::ZAchievementManagerSimple_OnEventSent->RemoveDetour(&Croupier::OnEventSent);
	Hooks::Http_WinHttpCallback->RemoveDetour(&Croupier::OnWinHttpCallback);

	this->hooksInstalled = false;
}

auto Croupier::OnFrameUpdate(const SGameUpdateEvent&) -> void {
	this->ProcessSpinState();
	this->ProcessClientMessages();
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
			auto& kc = this->sharedSpin.killValidations[i];
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

auto Croupier::SendKillValidationUpdate() -> void {
	auto data = ""s;
	for (const auto& cond : spin.getConditions()) {
		auto kc = this->sharedSpin.getTargetKillValidation(cond.target.get().getID());
		if (!data.empty()) data += ",";

		data += std::format("{}:{}:{}", static_cast<int>(cond.target.get().getID()), static_cast<int>(kc.correctMethod), kc.correctDisguise ? 1 : 0);
	}
	this->client->send(eClientMessage::KillValidation, { data });
}

auto Croupier::OnDrawMenu() -> void {
	// Toggle our message when the user presses our button.
	if (ImGui::Button(ICON_MD_CASINO " CROUPIER"))
		this->showUI = !this->showUI;
}

auto Croupier::OnDrawUI(bool focused) -> void {
	this->DrawSpinUI(focused);
	this->DrawEditSpinUI(focused);
	this->DrawCustomRulesetUI(focused);
	this->DrawEditMissionPoolUI(focused);

	if (!this->showUI) return;

	ImGui::PushFont(SDK()->GetImGuiBlackFont());
	ImGui::SetNextWindowContentSize(ImVec2(400, 0));

	if (ImGui::Begin(ICON_MD_SETTINGS " CROUPIER", &this->showUI)) {
		auto connected = this->client->isConnected();
		ImGui::PushStyleColor(ImGuiCol_Text, connected ? IM_COL32(0, 255, 0, 255) : IM_COL32(255, 0, 0, 255));
		ImGui::TextUnformatted(connected ? "Connected" : "Disconnected");
		ImGui::PopStyleColor();

		ImGui::PushFont(SDK()->GetImGuiRegularFont());

		if (ImGui::Checkbox("Timer", &this->config.timer)) {
			this->SaveConfiguration();
		}

		if (this->config.timer) {
			ImGui::SameLine();
			ImGui::SetCursorPosX(ImGui::GetCursorPosX() + 40.0);
			if (ImGui::Button("Reset")) {
				this->sharedSpin.timeStarted = std::chrono::steady_clock::now();
			}
		}

		if (ImGui::Checkbox("Overlay", &this->config.spinOverlay))
			this->SaveConfiguration();

		{
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
		ImGui::SetWindowFontScale(1.0);

		if (!connected && !this->spinHistory.empty()) {
			ImGui::SameLine();

			if (ImGui::Button("Previous"))
				this->PreviousSpin();
		}
		ImGui::PopFont();
	}

	ImGui::End();
	ImGui::PopFont();
}

auto Croupier::DrawSpinUI(bool focused) -> void {
	if (!this->config.spinOverlay) return;

	ImGui::PushFont(SDK()->GetImGuiBlackFont());

	if (ImGui::Begin(ICON_MD_CASINO " CROUPIER - SPIN", &this->config.spinOverlay, ImGuiWindowFlags_AlwaysAutoResize)) {
		ImGui::PushFont(SDK()->GetImGuiBoldFont());

		auto elapsed = std::chrono::seconds::zero();
		auto& conds = this->spin.getConditions();

		elapsed = this->sharedSpin.getTimeElapsed();
		for (auto i = 0; i < conds.size(); ++i) {
			auto& cond = conds[i];
			auto kc = this->sharedSpin.getTargetKillValidation(cond.target.get().getID());
			//auto str = std::format("{}: {} / {}", cond.target.get().getName(), cond.methodName, cond.disguise.get().name);
			auto validation = " - "s;
			if (kc.correctMethod == eKillValidationType::Unknown)
				validation += "Unknown, "s + (kc.correctDisguise ? "valid disguise" : "invalid disguise");
			else if (kc.correctMethod == eKillValidationType::Invalid)
				validation += kc.correctDisguise ? "Invalid, valid disguise" : "Invalid";
			else if (kc.correctMethod == eKillValidationType::Valid)
				validation += kc.correctDisguise ? "Done" : "Invalid disguise";
			else if (kc.correctMethod == eKillValidationType::Incomplete)
				validation = "";

			auto str = std::format("{}: {} / {}{}", cond.target.get().getName(), cond.methodName, cond.disguise.get().name, validation);
			ImGui::Text(str.c_str());
		}

		if (this->config.timer) {
			auto timeFormat = std::string();
			auto const includeHr = std::chrono::duration_cast<std::chrono::hours>(elapsed).count() >= 1;
			auto const time = includeHr ? std::format("{:%H:%M:%S}", elapsed) : std::format("{:%M:%S}", elapsed);
			auto windowWidth = ImGui::GetWindowSize().x;
			auto textWidth = ImGui::CalcTextSize(time.c_str()).x;

			ImGui::SetCursorPosX((windowWidth - textWidth) * 0.5f);
			ImGui::Text(time.c_str());
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
	auto currentMission = this->spin.getMission();
	if (currentMission && mission == currentMission->getMission() && !this->spinCompleted) return;
	this->sharedSpin.playerSelectMission();
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
	for (auto& cond : this->spin.getConditions())
	{
		if (!spinText.empty()) spinText += " || ";
		if (cond.killMethod.name.empty()) spinText += "***";
		spinText += std::format("{}: {} / {}", cond.target.get().getName(), cond.methodName, cond.disguise.get().name);
	}

	Logger::Info("Croupier: {}", spinText);
}

auto Croupier::SetupEvents() -> void {
	events.listen<Events::ContractStart>([this](auto& ev) {
		this->sharedSpin.hasLoadedGame = false;
		this->sharedSpin.resetKillValidations();

		if (!this->sharedSpin.isPlaying || this->sharedSpin.isFinished)
			this->sharedSpin.playerStart();

		this->SendKillValidationUpdate();
	});
	events.listen<Events::ContractLoad>([this](auto& ev) {
		this->sharedSpin.hasLoadedGame = true;
		this->sharedSpin.resetKillValidations();
		this->SendKillValidationUpdate();
	});
	events.listen<Events::ExitGate>([this](const ServerEvent<Events::ExitGate>& ev) {
		this->sharedSpin.playerExit();
	});
	events.listen<Events::ContractEnd>([this](const ServerEvent<Events::ContractEnd>& ev){
		this->spinCompleted = true;
		this->OnFinishMission();
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

			auto& kc = this->sharedSpin.killValidations[i];
			kc.target = target.getID();
			kc.isPacified = true;
		}
	});
	events.listen<Events::Kill>([this](const ServerEvent<Events::Kill>& ev) {
		if (!ev.Value.IsTarget) return;
		if (this->spinCompleted) return;

		auto const& conditions = this->sharedSpin.spin.getConditions();
		if (conditions.empty()) return;

		bool validationUpdated = false;
		auto it = targetsByRepoId.find(ev.Value.RepositoryId);
		auto targetId = it != end(targetsByRepoId) ? it->second : eTargetID::Unknown;

		for (auto i = 0; i < conditions.size(); ++i) {
			auto& cond = conditions[i];
			auto& target = cond.target.get();

			if (targetId != target.getID() && target.getName() != ev.Value.ActorName)
				continue;

			auto& kc = this->sharedSpin.killValidations[i];
			auto& reqDisguise = cond.disguise.get();
			kc.target = target.getID();

			// Target already killed? Confusion. Turn an invalid kill valid, but don't invalidate previously validated kills.
			if (kc.correctMethod == eKillValidationType::Valid) {
				if (!kc.correctDisguise)
					kc.correctDisguise = reqDisguise.suit ? ev.Value.OutfitIsHitmanSuit : reqDisguise.repoId == ev.Value.OutfitRepositoryId;
				break;
			}

			kc.correctDisguise = reqDisguise.suit ? ev.Value.OutfitIsHitmanSuit : reqDisguise.repoId == ev.Value.OutfitRepositoryId;

			if (cond.killComplication == eKillComplication::Live && kc.isPacified)
				kc.correctMethod = eKillValidationType::Invalid;
			else if (cond.killMethod.method != eKillMethod::NONE)
				kc.correctMethod = ValidateKillMethod(target.getID(), ev, cond.killMethod.method);
			else if (cond.specificKillMethod.method != eMapKillMethod::NONE)
				kc.correctMethod = ValidateKillMethod(target.getID(), ev, cond.specificKillMethod.method);

			validationUpdated = true;

			Logger::Debug("Killed '{}'", ev.Value.ActorName);
		}

		if (validationUpdated) this->SendKillValidationUpdate();
	});
	events.listen<Events::Level_Setup_Events>([this](const ServerEvent<Events::Level_Setup_Events>& ev) {
		auto const& conditions = this->sharedSpin.spin.getConditions();
		auto mission = this->sharedSpin.spin.getMission();
		if (!mission || mission->getMission() != eMission::HOKKAIDO_SITUSINVERSUS) return;
		if (ev.Value.Contract_Name_metricvalue != "SnowCrane") return;

		bool validationUpdated = false;

		for (auto i = 0; i < conditions.size(); ++i) {
			auto& cond = conditions[i];
			if (cond.target.get().getID() != eTargetID::ErichSoders) continue;

			auto& kc = this->sharedSpin.killValidations[i];
			auto& reqDisguise = cond.disguise.get();
			kc.target = cond.target.get().getID();

			validationUpdated = true;

			// Target already killed? Confusion. Turn an invalid kill valid, but don't invalidate previously validated kills.
			kc.correctDisguise = true;//reqDisguise.suit ? ev.Value.OutfitIsHitmanSuit : reqDisguise.repoId == ev.Value.OutfitRepositoryId;

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
					kc.correctMethod = cond.killMethod.isGun || cond.killMethod.method == eKillMethod::Explosive ? eKillValidationType::Valid : eKillValidationType::Invalid;
				else if (ev.Value.Event_metricvalue == "Heart_Kill")
					kc.correctMethod = eKillValidationType::Invalid;
				else if (ev.Value.Event_metricvalue == "Spidermachine_Kill")
					kc.correctMethod = eKillValidationType::Invalid;
				else if (ev.Value.Event_metricvalue == "Soder_Electrocuted")
					kc.correctMethod = eKillValidationType::Invalid;
				else if (ev.Value.Event_metricvalue == "Poison_Kill")
					kc.correctMethod = eKillValidationType::Invalid;
				else
					validationUpdated = false;
			}
			else validationUpdated = false;
		}

		if (validationUpdated) this->SendKillValidationUpdate();
	});
}

auto Croupier::ValidateKillMethod(eTargetID target, const ServerEvent<Events::Kill>& ev, eKillMethod method) -> eKillValidationType {
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
		// If expecting injected poison, we can determine whether the proxy medic opportunity was used
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

	// ev.Value.KillMethodBroad == "close_combat_pistol_elimination"
	switch (method) {
	case eKillMethod::NeckSnap:
		return killMethodBroad == "unarmed" ? eKillValidationType::Valid : eKillValidationType::Invalid;
	case eKillMethod::Pistol:
		return killMethodBroad == "pistol" || killMethodBroad == "close_combat_pistol_elimination" ? eKillValidationType::Valid : eKillValidationType::Invalid;
	case eKillMethod::SMG:
		return killMethodBroad == "smg" ? eKillValidationType::Valid : eKillValidationType::Invalid;
	case eKillMethod::PistolElimination:
		return killMethodBroad == "close_combat_pistol_elimination" ? eKillValidationType::Valid : eKillValidationType::Invalid;
	case eKillMethod::Shotgun:
		return killMethodBroad == "shotgun" ? eKillValidationType::Valid : eKillValidationType::Invalid;
	case eKillMethod::AssaultRifle:
		return killMethodBroad == "assaultrifle" ? eKillValidationType::Valid : eKillValidationType::Invalid;
	case eKillMethod::Explosive:
		return killMethodBroad == "explosive" ? eKillValidationType::Valid : eKillValidationType::Invalid;
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
			return method == eKillMethod::Fall ? eKillValidationType::Valid : eKillValidationType::Unknown;

		return killMethodStrict == "accident_push" ? eKillValidationType::Valid : eKillValidationType::Invalid;
	case eKillMethod::FallingObject:
		return killMethodStrict == "accident_suspended_object" ? eKillValidationType::Valid : eKillValidationType::Invalid;
	case eKillMethod::Fire:
		return killMethodStrict == "accident_burn" ? eKillValidationType::Valid : eKillValidationType::Invalid;
	case eKillMethod::Electrocution:
		return killMethodStrict == "accident_electric" ? eKillValidationType::Valid : eKillValidationType::Invalid;
	}
	return eKillValidationType::Unknown;
}

auto Croupier::ValidateKillMethod(eTargetID target, const ServerEvent<Events::Kill>& ev, eMapKillMethod method) -> eKillValidationType {
	if (!ev.Value.KillItemRepositoryId.empty()) {
		auto it = specificKillMethodsByRepoId.find(ev.Value.KillItemRepositoryId);
		if (it != end(specificKillMethodsByRepoId) && it->second == method)
			return eKillValidationType::Valid;
	}
	return eKillValidationType::Invalid;
}

DEFINE_PLUGIN_DETOUR(Croupier, void, OnEventReceived, ZAchievementManagerSimple* th, const SOnlineEvent& event) {
	Logger::Info("OnEventReceived: {}", event.sName);
	return HookResult<void>(HookAction::Continue());
}

DEFINE_PLUGIN_DETOUR(Croupier, void, OnEventSent, ZAchievementManagerSimple* th, uint32_t eventIndex, const ZDynamicObject& ev) {
	ZString eventData;
	Functions::ZDynamicObject_ToString->Call(const_cast<ZDynamicObject*>(&ev), &eventData);

	auto eventDataSV = std::string_view(eventData.c_str(), eventData.size());
	auto fixedEventDataStr = std::string(eventData.size(), '\0');
	std::remove_copy(eventDataSV.cbegin(), eventDataSV.cend(), fixedEventDataStr.begin(), '\n');

	try {
		auto json = nlohmann::json::parse(fixedEventDataStr.c_str(), fixedEventDataStr.c_str() + fixedEventDataStr.size());
		auto const eventName = json.value("Name", "");
		auto const timestamp = json.value("Timestamp", 0.0);

		if (!this->events.handle(eventName, json))
			Logger::Info("Unhandled Event Sent: {}", eventData);
	}
	catch (const nlohmann::json::exception& ex) {
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
		Logger::Info("WinHttpQueryOption URL: {}", narrow(wstr));
		auto url = narrow(wstr);
		std::string_view sv = url;

		if (sv.starts_with("https://hm3-service.hitman.io/profiles/page/Planning?contractid=")) {
			auto rest = sv.substr(sizeof("https://hm3-service.hitman.io/profiles/page/Planning?contractid="));
			auto contractId = rest.substr(0, rest.find_first_of('&'));
			auto mission = getMissionByContractId(std::string(contractId));

			if (mission != eMission::NONE) {
				this->OnMissionSelect(mission);
				if (!this->sharedSpin.isPlaying)
					this->sharedSpin.playerStart();
			}
		}
	}
	else Logger::Error("WinHttpQueryOption failed: {}", GetLastError());
	return HookResult<void>(HookAction::Continue());
}

DECLARE_ZHM_PLUGIN(Croupier);
