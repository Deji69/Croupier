#include "Config.h"
#include "Roulette.h"
#include "SpinParser.h"
#include "State.h"
#include <Logging.h>
#include <filesystem>
#include <string>
#include <string_view>
#include <vector>

using namespace Croupier;

Config Config::main;

static auto parseDockMode(const ZString& val) {
	if (val == "topleft") return DockMode::TopLeft;
	else if (val == "topright") return DockMode::TopRight;
	else if (val == "bottomleft") return DockMode::BottomLeft;
	else if (val == "bottomright") return DockMode::BottomRight;
	return DockMode::None;
}

static auto dockModeToString(DockMode val) {
	if (val == DockMode::TopLeft) return "topleft";
	else if (val == DockMode::TopRight) return "topright";
	else if (val == DockMode::BottomLeft) return "bottomleft";
	else if (val == DockMode::BottomRight) return "bottomright";
	return "none";
}

Config::Config() {
	CHAR filename[MAX_PATH] = {};
	if (GetModuleFileName(NULL, filename, MAX_PATH) != 0)
		this->modulePath = std::filesystem::path(filename).parent_path();
}

auto Config::LoadConfig() -> void {
	this->debug = SDK()->GetPluginSettingBool(plugin, "general", "debug", this->debug);
	this->timer = SDK()->GetPluginSettingBool(plugin, "general", "timer", this->timer);
	this->streak = SDK()->GetPluginSettingBool(plugin, "general", "streak", this->streak);
	this->streakCurrent = SDK()->GetPluginSettingInt(plugin, "general", "streak_current", this->streakCurrent);
	this->spinOverlay = SDK()->GetPluginSettingBool(plugin, "general", "spin_overlay", this->spinOverlay);
	this->overlayDockMode = parseDockMode(SDK()->GetPluginSetting(plugin, "general", "spin_overlay_dock", std::string_view{dockModeToString(this->overlayDockMode)}));
	this->overlayKillConfirmations = SDK()->GetPluginSettingBool(plugin, "general", "spin_overlay_confirmations", this->overlayKillConfirmations);
	this->ruleset = getRulesetByName(SDK()->GetPluginSetting(plugin, "general", "ruleset", "")).value_or(this->ruleset);
	this->customRules.enableMedium = SDK()->GetPluginSettingBool(plugin, "general", "ruleset_medium", this->customRules.enableMedium);
	this->customRules.enableHard = SDK()->GetPluginSettingBool(plugin, "general", "ruleset_hard", this->customRules.enableHard);
	this->customRules.enableExtreme = SDK()->GetPluginSettingBool(plugin, "general", "ruleset_extreme", this->customRules.enableExtreme);
	this->customRules.enableBuggy = SDK()->GetPluginSettingBool(plugin, "general", "ruleset_buggy", this->customRules.enableBuggy);
	this->customRules.enableImpossible = SDK()->GetPluginSettingBool(plugin, "general", "ruleset_impossible", this->customRules.enableImpossible);
	this->customRules.genericEliminations = SDK()->GetPluginSettingBool(plugin, "general", "ruleset_generic_elims", this->customRules.genericEliminations);
	this->customRules.liveComplications = SDK()->GetPluginSettingBool(plugin, "general", "ruleset_live_complications", this->customRules.liveComplications);
	this->customRules.liveComplicationsExcludeStandard = SDK()->GetPluginSettingBool(plugin, "general", "ruleset_live_complications_exclude_standard", this->customRules.liveComplicationsExcludeStandard);
	this->customRules.liveComplicationChance = SDK()->GetPluginSettingInt(plugin, "general", "ruleset_live_complication_chance", this->customRules.liveComplicationChance);
	this->customRules.meleeKillTypes = SDK()->GetPluginSettingBool(plugin, "general", "ruleset_melee_kill_types", this->customRules.meleeKillTypes);
	this->customRules.thrownKillTypes = SDK()->GetPluginSettingBool(plugin, "general", "ruleset_thrown_kill_types", this->customRules.thrownKillTypes);

	auto missionPoolStr = SDK()->GetPluginSetting(plugin, "general", "mission_pool", "");
	const auto maps = split(missionPoolStr, ",");
	this->missionPool.clear();
	for (const auto& map : maps) {
		auto mission = getMissionByCodename(std::string(trim(map)));
		if (mission != eMission::NONE)
			this->missionPool.push_back(mission);
	}

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

	bool inHistorySection = false;

	auto parseHistorySection = [this](std::string_view line) {
		auto spin = SpinParser::parse(line);
		if (!spin) return;

		State::current.spinHistory.emplace(std::move(*spin));
	};

	for (const auto& sv : split(content, "\n")) {
		if (inHistorySection)
			parseHistorySection(sv);
		else if (trim(sv) == "[history]")
			inHistorySection = true;
	}
}

auto Config::SaveConfig() -> void {
	SDK()->SetPluginSettingBool(plugin, "general", "debug", this->debug);
	SDK()->SetPluginSettingBool(plugin, "general", "timer", this->timer);
	SDK()->SetPluginSettingBool(plugin, "general", "streak", this->streak);
	SDK()->SetPluginSettingInt(plugin, "general", "streak_current", this->streakCurrent);
	SDK()->SetPluginSettingBool(plugin, "general", "spin_overlay", this->spinOverlay);
	SDK()->SetPluginSetting(plugin, "general", "spin_overlay_dock", std::string_view{dockModeToString(this->overlayDockMode)});
	SDK()->SetPluginSettingBool(plugin, "general", "spin_overlay_confirmations", this->overlayKillConfirmations);
	SDK()->SetPluginSetting(plugin, "general", "ruleset", getRulesetName(this->ruleset).value_or(""));
	SDK()->SetPluginSettingBool(plugin, "general", "ruleset_medium", this->customRules.enableMedium);
	SDK()->SetPluginSettingBool(plugin, "general", "ruleset_hard", this->customRules.enableHard);
	SDK()->SetPluginSettingBool(plugin, "general", "ruleset_extreme", this->customRules.enableExtreme);
	SDK()->SetPluginSettingBool(plugin, "general", "ruleset_buggy", this->customRules.enableBuggy);
	SDK()->SetPluginSettingBool(plugin, "general", "ruleset_impossible", this->customRules.enableImpossible);
	SDK()->SetPluginSettingBool(plugin, "general", "ruleset_generic_elims", this->customRules.genericEliminations);
	SDK()->SetPluginSettingBool(plugin, "general", "ruleset_live_complications", this->customRules.liveComplications);
	SDK()->SetPluginSettingBool(plugin, "general", "ruleset_live_complications_exclude_standard", this->customRules.liveComplicationsExcludeStandard);
	SDK()->SetPluginSettingInt(plugin, "general", "ruleset_live_complication_chance", this->customRules.liveComplicationChance);
	SDK()->SetPluginSettingBool(plugin, "general", "ruleset_melee_kill_types", this->customRules.meleeKillTypes);
	SDK()->SetPluginSettingBool(plugin, "general", "ruleset_thrown_kill_types", this->customRules.thrownKillTypes);

	std::string mapPoolValue;
	for (const auto mission : this->missionPool) {
		auto codename = getMissionCodename(mission);
		if (!codename) continue;
		if (mapPoolValue.size()) mapPoolValue += ", ";
		mapPoolValue += codename.value();
	}

	SDK()->SetPluginSetting(plugin, "general", "mission_pool", mapPoolValue);

	const auto filepath = this->modulePath / "mods" / "Croupier" / "croupier.txt";

	this->file.open(filepath, std::ios::out | std::ios::trunc);

	if (this->file.is_open()) {
		std::println(this->file, "[history]");

		for (const auto& spin : this->spinHistory) {
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

	Logger::Info("Croupier: Config saved.");
}

auto Config::Save() -> void {
	if (main.plugin) main.SaveConfig();
}

auto Config::Load() -> void {
	if (main.plugin) main.LoadConfig();
}
