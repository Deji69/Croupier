#pragma once
#include "Roulette.h"
#include "RouletteRuleset.h"
#include <IPluginInterface.h>
#include <fstream>
#include <filesystem>
#include <string>
#include <vector>

class IPluginInterface;

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

	struct Config {
		static Config main;

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
		IPluginInterface* plugin = nullptr;

		Config();

		static auto Load() -> void;
		static auto Save() -> void;

	private:
		auto LoadConfig() -> void;
		auto SaveConfig() -> void;
	};
}
