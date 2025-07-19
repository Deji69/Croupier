#pragma once
#include "Config.h"
#include "CroupierClient.h"
#include <imgui.h>
#include <Glacier/ZMath.h>
#include <Glacier/ZPrimitives.h>

namespace Croupier
{
	class UI {
	public:
		auto Draw(bool focused) -> void;
		auto DrawMenu() -> void;
		auto DrawEditSpinUI(bool focused) -> void;
		auto DrawCustomRulesetUI(bool focused) -> void;
		auto DrawEditMissionPoolUI(bool focused) -> void;
		auto DrawSpinUI(bool focused) -> void;
		auto DrawBingoDebugUI(bool focused) -> void;

	private:
		ImVec2 overlaySize = {};
		ImVec2 debugOverlaySize = {};
		bool showUI = false;
		bool showManualModeUI = false;
		bool showEditMissionPoolUI = false;
		bool showCustomRulesetUI = false;
		bool showEditHotkeysUI = false;
		bool showDebugUI = false;
	};
}
