#pragma once
#include "Config.h"
#include "CroupierClient.h"
#include "State.h"
#include <imgui.h>
#include <Glacier/ZMath.h>
#include <Glacier/ZPrimitives.h>

namespace Croupier
{
	struct UIState {
		RouletteSpin spin;
		BingoCard card;
		const RouletteMission* mission;
		GameMode gameMode = GameMode::Hybrid;
		SVector3 playerPos;
		int16_t playerRoomId = 0;
		bool isClientConnected = false;
		bool debugUI = false;
		bool spinLocked = false;
		bool haveSpinHistory = false;
		bool shouldDrawStatus = false;
		bool shouldDrawTimer = false;
		bool shouldDrawStreak = false;
		bool overlayEnabled = false;
		bool killConfirmationsEnabled = false;
		int currentStreak = 0;
		eRouletteRuleset ruleset = eRouletteRuleset::Default;
		std::chrono::steady_clock::time_point timeStarted;
		std::chrono::seconds timeElapsed;

		auto Update(const State& state) -> void {
			std::shared_lock lock(state.stateMutex, std::defer_lock_t{});
			if (!lock.try_lock()) return;
			spin = std::move(RouletteSpin{state.spin});
			card = std::move(BingoCard{state.card});
			gameMode = state.gameMode;
			mission = state.generator.getMission();
			playerPos = state.playerMatrix.Trans;
			playerRoomId = state.roomId;
			isClientConnected = state.client.isConnected();
			haveSpinHistory = !state.spinHistory.empty();
			spinLocked = state.spinLocked;
			ruleset = state.ruleset;
			timeStarted = state.timeStarted;
			timeElapsed = state.getTimeElapsed();
			debugUI = Config::main.debug;
			shouldDrawTimer = Config::main.timer;
			shouldDrawStreak = Config::main.streak;
			shouldDrawStatus = Config::main.timer || Config::main.streak;
			overlayEnabled = Config::main.spinOverlay;
			killConfirmationsEnabled = Config::main.overlayKillConfirmations;
			currentStreak = Config::main.streakCurrent;
		}
	};

	class UI {
	public:
		auto Draw(bool focused) -> void;
		auto DrawMenu() -> void;
		auto DrawEditSpinUI(bool focused) -> void;
		auto DrawCustomRulesetUI(bool focused) -> void;
		auto DrawEditMissionPoolUI(bool focused) -> void;
		auto DrawOverlayUI(bool focused) -> void;
		auto DrawSpinUI(bool focused) -> void;
		auto DrawBingoUI(bool focused) -> void;
		auto DrawBingoDebugUI(bool focused) -> void;
		auto DrawOverlayUIGameStatus() -> void;

	private:
		auto ShouldDrawStatus() -> bool;

	private:
		ImVec2 overlaySize = {};
		ImVec2 debugOverlaySize = {};
		UIState state;
		bool showUI = false;
		bool showManualModeUI = false;
		bool showEditMissionPoolUI = false;
		bool showCustomRulesetUI = false;
		bool showEditHotkeysUI = false;
		bool showDebugUI = false;
	};
}
