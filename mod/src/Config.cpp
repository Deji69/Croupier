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

Configuration Configuration::main;

Configuration::Configuration() {
	CHAR filename[MAX_PATH] = {};
	if (GetModuleFileName(NULL, filename, MAX_PATH) != 0)
		this->modulePath = std::filesystem::path(filename).parent_path();
}

auto Configuration::Load() -> void {
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
		{"debug", [this, parseBool](std::string_view val) { this->debug = parseBool(val, this->debug); }},
		{"timer", [this, parseBool](std::string_view val) { this->timer = parseBool(val, this->timer); }},
		{"streak", [this, parseBool](std::string_view val) { this->streak = parseBool(val, this->streak); }},
		{"streak_current", [this, parseInt](std::string_view val) { this->streakCurrent = parseInt(val, this->streakCurrent); }},
		{"spin_overlay", [this, parseBool](std::string_view val) { this->spinOverlay = parseBool(val, this->spinOverlay); }},
		{"spin_overlay_dock", [this](std::string_view val) {
			if (val == "topleft") this->overlayDockMode = DockMode::TopLeft;
			else if (val == "topright") this->overlayDockMode = DockMode::TopRight;
			else if (val == "bottomleft") this->overlayDockMode = DockMode::BottomLeft;
			else if (val == "bottomright") this->overlayDockMode = DockMode::BottomRight;
			else this->overlayDockMode = DockMode::None;
		}},
		{"spin_overlay_confirmations", [this, parseBool](std::string_view val) { this->overlayKillConfirmations = parseBool(val, this->overlayKillConfirmations); }},
		{"ruleset", [this](std::string_view val) { this->ruleset = getRulesetByName(val).value_or(this->ruleset); }},
		{"ruleset_medium", [this, parseBool](std::string_view val) { this->customRules.enableMedium = parseBool(val, this->customRules.enableMedium); }},
		{"ruleset_hard", [this, parseBool](std::string_view val) { this->customRules.enableHard = parseBool(val, this->customRules.enableHard); }},
		{"ruleset_extreme", [this, parseBool](std::string_view val) { this->customRules.enableExtreme = parseBool(val, this->customRules.enableExtreme); }},
		{"ruleset_buggy", [this, parseBool](std::string_view val) { this->customRules.enableBuggy = parseBool(val, this->customRules.enableBuggy); }},
		{"ruleset_impossible", [this, parseBool](std::string_view val) {
			this->customRules.enableImpossible = parseBool(val, this->customRules.enableImpossible);
		}},
		{"ruleset_generic_elims", [this, parseBool](std::string_view val) {
			this->customRules.genericEliminations = parseBool(val, this->customRules.genericEliminations);
		}},
		{"ruleset_live_complications", [this, parseBool](std::string_view val) {
			this->customRules.liveComplications = parseBool(val, this->customRules.liveComplications);
		}},
		{"ruleset_live_complications_exclude_standard", [this, parseBool](std::string_view val) {
			this->customRules.liveComplicationsExcludeStandard = parseBool(val, this->customRules.liveComplicationsExcludeStandard);
		}},
		{"ruleset_live_complication_chance", [this, parseInt](std::string_view val) {
			this->customRules.liveComplicationChance = parseInt(val, this->customRules.liveComplicationChance);
		}},
		{"ruleset_melee_kill_types", [this, parseBool](std::string_view val) {
			this->customRules.meleeKillTypes = parseBool(val, this->customRules.meleeKillTypes);
		}},
		{"ruleset_thrown_kill_types", [this, parseBool](std::string_view val) {
			this->customRules.thrownKillTypes = parseBool(val, this->customRules.thrownKillTypes);
		}},
		{"mission_pool", [this](std::string_view val) {
			const auto maps = split(val, ",");
			this->missionPool.clear();
			for (const auto& map : maps) {
				auto mission = getMissionByCodename(std::string(trim(map)));
				if (mission != eMission::NONE)
					this->missionPool.push_back(mission);
			}
		}},
	};

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
		else {
			auto tokens = split(sv, " ", 2);
			if (tokens.size() < 2) continue;

			auto const it = cmds.find(std::string(tokens[0]));
			if (it != cend(cmds)) it->second(trim(tokens[1]));
		}
	}
}

auto Configuration::Save() -> void {
	std::string content;
	const auto filepath = this->modulePath / "mods" / "Croupier" / "croupier.txt";

	this->file.open(filepath, std::ios::out | std::ios::trunc);

	auto spinOverlayDock = "none";
	switch (this->overlayDockMode) {
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

	std::println(this->file, "debug {}", this->debug ? "true" : "false");
	std::println(this->file, "timer {}", this->timer ? "true" : "false");
	std::println(this->file, "streak {}", this->streak ? "true" : "false");
	std::println(this->file, "streak_current {}", this->streakCurrent);
	std::println(this->file, "spin_overlay {}", this->spinOverlay ? "true" : "false");
	std::println(this->file, "spin_overlay_dock {}", spinOverlayDock);
	std::println(this->file, "spin_overlay_confirmations {}", this->overlayKillConfirmations ? "true" : "false");
	const auto rulesetName = getRulesetName(this->ruleset);
	if (rulesetName) std::println(this->file, "ruleset {}", rulesetName.value());
	std::println(this->file, "ruleset_medium {}", this->customRules.enableMedium ? "true" : "false");
	std::println(this->file, "ruleset_hard {}", this->customRules.enableHard ? "true" : "false");
	std::println(this->file, "ruleset_extreme {}", this->customRules.enableExtreme ? "true" : "false");
	std::println(this->file, "ruleset_impossible {}", this->customRules.enableImpossible ? "true" : "false");
	std::println(this->file, "ruleset_buggy {}", this->customRules.enableBuggy ? "true" : "false");
	std::println(this->file, "ruleset_generic_elims {}", this->customRules.genericEliminations ? "true" : "false");
	std::println(this->file, "ruleset_live_complications {}", this->customRules.liveComplications ? "true" : "false");
	std::println(this->file, "ruleset_live_complications_exclude_standard {}", this->customRules.liveComplicationsExcludeStandard ? "true" : "false");
	std::println(this->file, "ruleset_live_complication_chance {}", this->customRules.liveComplicationChance);
	std::println(this->file, "ruleset_melee_kill_types {}", this->customRules.meleeKillTypes ? "true" : "false");
	std::println(this->file, "ruleset_thrown_kill_types {}", this->customRules.thrownKillTypes ? "true" : "false");

	std::string mapPoolValue;
	for (const auto mission : this->missionPool) {
		auto codename = getMissionCodename(mission);
		if (!codename) continue;
		if (mapPoolValue.size()) mapPoolValue += ", ";
		mapPoolValue += codename.value();
	}
	std::println(this->file, "mission_pool {}", mapPoolValue);

	std::println(this->file, "");
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

	Logger::Info("Croupier - Config saved.");
}
