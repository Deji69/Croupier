#pragma once
#include "Roulette.h"
#include "RouletteRuleset.h"
#include <fstream>
#include <filesystem>
#include <string>
#include <vector>

namespace Croupier {
	// TODO: Move
	struct SerializedSpin {
		struct Condition {
			std::string targetName;
			std::string killMethod;
			std::string disguise;
		};

		std::vector<Condition> conditions;
	};

	enum class DockMode {
		None,
		TopLeft,
		TopRight,
		BottomLeft,
		BottomRight,
	};

	struct Configuration {
		static Configuration main;

		bool debug = false;
		bool timer = false;
		bool streak = false;
		int streakCurrent = 0;
		bool spinOverlay = false;
		bool overlayKillConfirmations = true;
		DockMode overlayDockMode = DockMode::None;
		RouletteRuleset customRules;
		eRouletteRuleset ruleset = eRouletteRuleset::Default;
		std::vector<eMission> missionPool;
		std::vector<SerializedSpin> spinHistory;
		std::fstream file;
		std::filesystem::path modulePath;

		Configuration();
		auto Load() -> void;
		auto Save() -> void;
	};
}
