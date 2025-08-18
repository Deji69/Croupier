#include "UI.h"
#include "App.h"
#include "Config.h"
#include "State.h"
#include <Globals.h>
#include <IconsMaterialDesign.h>
#include <IPluginInterface.h>
#include <format>
#include <imgui.h>

using namespace Croupier;
using namespace std::string_literals;
using namespace std::string_view_literals;

constexpr int ypadding = 3; // vertical spacing of text in pixels

static auto DrawCenteredText(ImVec2 position, float width, std::string_view text) -> void {
	ImVec2 renderPos;
	ImVec2 linesize = ImGui::CalcTextSize(text.data());
	auto spaceSize = ImGui::CalcTextSize(" ");
	position.x = position.x + ((width / 2) - ((linesize.x + spaceSize.x) / 2));
	//ImGui::GetIO().FontDefault->RenderText(dl, 0, renderPos, ImGui::GetColorU32(ImGuiCol_Text), {-1000, -1000, 1000, 1000}, text.data(), text.data() + text.size());
	ImGui::SetCursorPos(position);
	ImGui::SetNextItemWidth(width);
	ImGui::TextUnformatted(text.data(), text.data() + text.size());
}

static auto TextCenteredCalcLines(const std::vector<std::string_view>& words, ImVec2 position, ImVec2 bound) -> int {
	auto spaceSize = ImGui::CalcTextSize(" ");
	auto charSize = ImGui::CalcTextSize("x");

	int linecnt = 0; // 0 is first line
	float linex = 0; // space already used on line
	std::string lineText = "";

	for (size_t i = 0; i < words.size(); ++i) {
		auto& word = words[i];
		auto width = ImGui::CalcTextSize(word.data(), word.data() + word.size()).x;
		auto pos = ImVec2{position.x, position.y + (spaceSize.y + ypadding) * linecnt};

		if ((linex + width) > bound.x) {
			lineText.clear();
			linex = 0;
			linecnt++;
			--i;
			continue;
		}

		if (i == words.size() - 1) {
			lineText += word.data();
			linex += width;
			break;
		}

		auto needSpace = !lineText.empty();
		if (needSpace) lineText += " "s;
		lineText += word;
		linex += width + (needSpace ? spaceSize.x : 0);
	}
	return linecnt;
}

static auto TextCentered(std::string_view text, ImVec2 position, ImVec2 bound) -> void {
	auto words = split(text, " ");

	auto spaceSize = ImGui::CalcTextSize(" ");
	auto charSize = ImGui::CalcTextSize("x");
	auto lines = TextCenteredCalcLines(words, position, bound);

	position.y += (bound.y / 2) - ((spaceSize.y + ypadding) * (lines + 1)) * 0.5;

	int linecnt = 0; // 0 is first line
	float linex = 0; // space already used on line
	std::string lineText = "";

	for (size_t i = 0; i < words.size(); ++i) {
		auto& word = words[i];
		auto width = ImGui::CalcTextSize(word.data(), word.data() + word.size()).x;
		auto pos = ImVec2{position.x, position.y + (spaceSize.y + ypadding) * linecnt};

		if ((linex + width) > bound.x) {
			DrawCenteredText(pos, bound.x, lineText);
			lineText.clear();
			linex = 0;
			linecnt++;
			--i;
			continue;
		}

		if (i == words.size() - 1) {
			if (!lineText.empty()) lineText += " ";
			lineText += word.data();
			DrawCenteredText(pos, bound.x, lineText);
			break;
		}

		auto needSpace = !lineText.empty();
		if (needSpace) lineText += " "s;
		lineText += word;
		linex += width + (needSpace ? spaceSize.x : 0);
	}
}

auto UI::DrawMenu() -> void {
	if (ImGui::Button(ICON_MD_CASINO " CROUPIER"))
		this->showUI = !this->showUI;
}

auto UI::Draw(bool focused) -> void {
	statusDrawn = false;
	this->state.Update(State::current);

	this->DrawBingoDebugUI(focused);
	this->DrawOverlayUI(focused);

	if (!focused) return;

	this->DrawEditSpinUI(focused);
	this->DrawCustomRulesetUI(focused);
	this->DrawEditMissionPoolUI(focused);

	if (!this->showUI) return;

	ImGui::PushFont(SDK()->GetImGuiBlackFont());
	ImGui::SetNextWindowContentSize(ImVec2(400, 0));

	if (ImGui::Begin(ICON_MD_SETTINGS " CROUPIER", &this->showUI, ImGuiWindowFlags_AlwaysAutoResize)) {
		ImGui::PushStyleColor(ImGuiCol_Text, state.isClientConnected ? IM_COL32(0, 255, 0, 255) : IM_COL32(255, 0, 0, 255));
		ImGui::TextUnformatted(state.isClientConnected ? "Connected" : "Disconnected");
		ImGui::PopStyleColor();

		ImGui::PushFont(SDK()->GetImGuiRegularFont());

#ifdef _DEBUG
		if (ImGui::Checkbox("Debug", &Config::main.debug))
			Config::Save();
#else
		if (Config::main.debug && ImGui::Checkbox("Debug", &Config::main.debug))
			Config::Save();
#endif

		if (ImGui::Checkbox("Overlay", &Config::main.spinOverlay))
			Config::Save();

		ImGui::SameLine(150.0);

		auto selectedOverlayDockName = "Undocked";

		switch (Config::main.overlayDockMode) {
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
			if (ImGui::Selectable("Undocked", Config::main.overlayDockMode == DockMode::None, 0)) {
				Config::main.overlayDockMode = DockMode::None;
				Config::Save();
			}
			if (Config::main.overlayDockMode == DockMode::None) ImGui::SetItemDefaultFocus();

			if (ImGui::Selectable("Top Left", Config::main.overlayDockMode == DockMode::TopLeft, 0)) {
				Config::main.overlayDockMode = DockMode::TopLeft;
				Config::Save();
			}
			if (Config::main.overlayDockMode == DockMode::TopLeft) ImGui::SetItemDefaultFocus();

			if (ImGui::Selectable("Top Right", Config::main.overlayDockMode == DockMode::TopRight, 0)) {
				Config::main.overlayDockMode = DockMode::TopRight;
				Config::Save();
			}
			if (Config::main.overlayDockMode == DockMode::TopRight) ImGui::SetItemDefaultFocus();

			if (ImGui::Selectable("Bottom Left", Config::main.overlayDockMode == DockMode::BottomLeft, 0)) {
				Config::main.overlayDockMode = DockMode::BottomLeft;
				Config::Save();
			}
			if (Config::main.overlayDockMode == DockMode::BottomLeft) ImGui::SetItemDefaultFocus();

			if (ImGui::Selectable("Bottom Right", Config::main.overlayDockMode == DockMode::BottomRight, 0)) {
				Config::main.overlayDockMode = DockMode::BottomRight;
				Config::Save();
			}
			if (Config::main.overlayDockMode == DockMode::BottomRight) ImGui::SetItemDefaultFocus();

			ImGui::EndCombo();
		}

		if (ImGui::Checkbox("Overlay Kill Confirmations", &Config::main.overlayKillConfirmations))
			Config::Save();

		if (!state.isClientConnected) {
			// Legacy timer
			if (ImGui::Checkbox("Timer", &Config::main.timer))
				Config::Save();

			if (Config::main.timer) {
				ImGui::SameLine();
				ImGui::SetCursorPosX(ImGui::GetCursorPosX() + 40.0);
				if (ImGui::Button("Reset"))
					State::current.timeStarted = std::chrono::steady_clock::now();
			}
		} else {
			// In-App Timer
			if (ImGui::Checkbox("Timer", &Config::main.timer))
				Config::Save();

			ImGui::SameLine(150.0);

			if (ImGui::Button("Reset")) {
				State::current.timeStarted = std::chrono::steady_clock::now();
				SendResetTimer();
			}

			ImGui::SameLine();
			if (ImGui::Button("Start")) {
				State::current.timeStarted = std::chrono::steady_clock::now();
				SendPauseTimer(false);
			}

			ImGui::SameLine();
			if (ImGui::Button("Stop")) {
				State::current.timeStarted = std::chrono::steady_clock::now();
				SendPauseTimer(true);
			}

			ImGui::SameLine();
			if (ImGui::Button("Split")) {
				State::current.timeStarted = std::chrono::steady_clock::now();
				SendSplitTimer();
			}
		}

		if (state.isClientConnected) {
			// Streak Tracking
			if (ImGui::Checkbox("Streak", &Config::main.streak))
				Config::Save();

			ImGui::SameLine(150.0);

			if (ImGui::Button("Reset##Streak")) {
				Config::main.streakCurrent = 0;
				SendResetStreak();
			}
		}

		if (!state.isClientConnected) {
			// Ruleset select
			auto const rulesetName = getRulesetName(state.ruleset);
			if (ImGui::BeginCombo("##Ruleset", rulesetName.value_or("").data(), ImGuiComboFlags_HeightLarge)) {
				for (auto& info : rulesets) {
					auto const selected = state.ruleset == info.ruleset;
					auto flags = info.ruleset == eRouletteRuleset::Custom ? ImGuiSelectableFlags_Disabled : 0;

					if (ImGui::Selectable(info.name.data(), selected, flags) && info.ruleset != eRouletteRuleset::Custom)
						State::OnRulesetSelect(info.ruleset);

					// Set the initial focus when opening the combo (scrolling + keyboard navigation focus)
					if (selected) ImGui::SetItemDefaultFocus();
				}
				ImGui::EndCombo();
			}

			ImGui::SameLine();

			if (ImGui::Button("Customise"))
				this->showCustomRulesetUI = !this->showCustomRulesetUI;
		}
		
		auto mission = state.mission;
		auto missionInfoIt = !mission ? missionInfos.end() : std::find_if(missionInfos.begin(), missionInfos.end(), [this](const MissionInfo& info) {
			return info.mission == state.mission->getMission();
		});
		auto const currentIdx = missionInfoIt != missionInfos.end() ? std::distance(missionInfos.begin(), missionInfoIt) : 0;
		auto const& currentMissionInfo = missionInfos[currentIdx];
		if (ImGui::BeginCombo("##Mission", currentMissionInfo.name.data(), ImGuiComboFlags_HeightLarge)) {
			for (auto& missionInfo : missionInfos) {
				auto const selected = missionInfo.mission != eMission::NONE && mission && mission->getMission() == missionInfo.mission;
				auto imGuiFlags = missionInfo.mission == eMission::NONE ? ImGuiSelectableFlags_Disabled : 0;
				if (ImGui::Selectable(missionInfo.name.data(), selected, imGuiFlags) && missionInfo.mission != eMission::NONE)
					State::OnMissionSelect(missionInfo.mission, false);

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

		if (state.isClientConnected) {
			if (ImGui::Button(ICON_MD_ARROW_BACK))
				SendPrev();
			if (ImGui::IsItemHovered(ImGuiHoveredFlags_AllowWhenDisabled)) ImGui::SetTooltip("Previous Spin");
			ImGui::SameLine();
			if (ImGui::Button(ICON_MD_ARROW_FORWARD))
				SendNext();
			if (ImGui::IsItemHovered(ImGuiHoveredFlags_AllowWhenDisabled)) ImGui::SetTooltip("Next Spin");
			ImGui::SameLine();
		}

		if (state.isClientConnected || state.spin.getMission()) {
			if (ImGui::Button(state.spinLocked ? ICON_MD_LOCK : ICON_MD_LOCK_OPEN)) {
				State::current.spinLocked = !state.spinLocked;
				SendToggleSpinLock();
			}
			if (ImGui::IsItemHovered(ImGuiHoveredFlags_AllowWhenDisabled)) ImGui::SetTooltip("Toggle Spin Lock");
			ImGui::SameLine();

			if (ImGui::Button(ICON_MD_EDIT))
				this->showManualModeUI = !this->showManualModeUI;
			if (ImGui::IsItemHovered(ImGuiHoveredFlags_AllowWhenDisabled)) ImGui::SetTooltip("Edit Spin");
			ImGui::SameLine();
		}

		if (ImGui::Button(ICON_MD_SHUFFLE))
			Commands::Random();
		if (ImGui::IsItemHovered(ImGuiHoveredFlags_AllowWhenDisabled)) ImGui::SetTooltip("Random Map/Spin");

		ImGui::SameLine();
		if (ImGui::Button(ICON_MD_REFRESH))
			Commands::Respin(false);
		if (ImGui::IsItemHovered(ImGuiHoveredFlags_AllowWhenDisabled)) ImGui::SetTooltip("Respin Current Map");

		ImGui::PopFont();

		if (!state.isClientConnected && state.haveSpinHistory) {
			ImGui::SameLine();

			if (ImGui::Button(ICON_MD_ARROW_LEFT))
				Commands::PreviousSpin();
		}

		ImGui::SetWindowFontScale(1.0);
		ImGui::PopFont();
	}

	ImGui::End();
	ImGui::PopFont();
}

auto UI::DrawOverlayUI(bool focused) -> void {
	if (!Config::main.spinOverlay)
		return;
	
	ImGui::PushFont(SDK()->GetImGuiBlackFont());

	auto viewportSize = ImGui::GetMainViewport()->Size;
	auto flags = static_cast<ImGuiWindowFlags>(ImGuiWindowFlags_AlwaysAutoResize);
	auto dims = GetBingoDimensions(state.card.tiles.size());

	if (Config::main.overlayDockMode != DockMode::None || !focused)
		flags |= ImGuiWindowFlags_NoTitleBar;

	switch (Config::main.overlayDockMode) {
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

	auto const haveRoulette = (state.gameMode == GameMode::Roulette || state.gameMode == GameMode::Hybrid) && !state.spin.getConditions().empty();
	auto const haveBingo = (state.gameMode == GameMode::Bingo || state.gameMode == GameMode::Hybrid) && !state.card.tiles.empty();
	auto const title = haveRoulette && haveBingo
		? ICON_MD_CASINO " CROUPIER - BINGO & SPIN"
		: (haveRoulette ? ICON_MD_CASINO " CROUPIER - SPIN" : ICON_MD_CASINO " CROUPIER - BINGO");

	if (haveBingo) {
		ImGui::PushStyleVar(ImGuiStyleVar_WindowPadding, {0, 0});
		ImGui::PushStyleVar(ImGuiStyleVar_ItemSpacing, {1, 1});
		ImGui::PushStyleColor(ImGuiCol_WindowBg, ImColor{0, 0, 0}.Value);
	}

	if (ImGui::Begin(title, &Config::main.spinOverlay, flags)) {
		this->overlaySize = ImGui::GetWindowSize();
		if (haveRoulette) {
			if (haveBingo) {
				ImGui::PushStyleVar(ImGuiStyleVar_WindowPadding, {4, 4});
				ImGui::PushStyleColor(ImGuiCol_ChildBg, ImColor{23, 28, 32}.Value);
				ImGui::BeginChild("roulette", {0, 0}, ImGuiChildFlags_AlwaysUseWindowPadding | ImGuiChildFlags_AutoResizeY, ImGuiWindowFlags_NoScrollbar);
			}
			this->DrawSpinUI(focused);
			if (haveBingo) {
				ImGui::EndChild();
				ImGui::PopStyleVar();
				ImGui::PopStyleColor();
			}
		}
		else {
			if (haveBingo) {
				ImGui::PushStyleVar(ImGuiStyleVar_WindowPadding, {4, 4});
				ImGui::PushStyleColor(ImGuiCol_ChildBg, ImColor{23, 28, 32}.Value);
				ImGui::BeginChild("roulette", {0, 0}, ImGuiChildFlags_AlwaysUseWindowPadding | ImGuiChildFlags_AutoResizeY, ImGuiWindowFlags_NoScrollbar);
			}
			this->DrawOverlayUIGameStatus();
			if (haveBingo) {
				ImGui::EndChild();
				ImGui::PopStyleVar();
				ImGui::PopStyleColor();
			}
		}
		if (haveBingo)
			this->DrawBingoUI(focused);
	}

	ImGui::End();

	if (haveBingo) {
		ImGui::PopStyleVar();
		ImGui::PopStyleVar();
		ImGui::PopStyleColor();
	}

	ImGui::PopFont();
}

auto UI::DrawBingoUI(bool focused) -> void {
	ImGui::PushFont(SDK()->GetImGuiMediumFont());

	auto dims = GetBingoDimensions(state.card.tiles.size());

	std::string str;
	str.reserve(8);

	for (size_t y = 0; y < dims.rows; ++y) {
		for (size_t x = 0; x < dims.columns; ++x) {
			auto const idx = y * dims.rows + x;
			if (idx < 0 || idx >= state.card.tiles.size()) break;
			auto const& tile = state.card.tiles[idx];
			if (x != 0) ImGui::SameLine();
			str.clear();
			std::format_to(std::back_inserter(str), "grid{}x{}", x, y);
			if (tile.achieved)
				ImGui::PushStyleColor(ImGuiCol_ChildBg, ImColor{17, 39, 18}.Value);
			else if (tile.failed)
				ImGui::PushStyleColor(ImGuiCol_ChildBg, ImColor{57, 19, 19}.Value);
			else
				ImGui::PushStyleColor(ImGuiCol_ChildBg, ImColor{23, 28, 32}.Value);
			ImGui::BeginChild(str.c_str(), {120, 90}, ImGuiChildFlags_Border, ImGuiWindowFlags_NoScrollbar);
			if (!tile.group.empty()) {
				ImGui::PushStyleColor(ImGuiCol_Text, tile.groupColour);
				TextCentered(trim(tile.group), { 7, 2 }, { 110, 18 });
				ImGui::PopStyleColor();
			}
			TextCentered(trim(tile.text), { 7, tile.group.empty() ? 2.0f : 20.0f }, { 110, tile.group.empty() ? 90.0f : 70.0f });
			ImGui::EndChild();
			ImGui::PopStyleColor();
		}
	}

	ImGui::PopFont();
}

auto UI::DrawSpinUI(bool focused) -> void {
	ImGui::PushFont(SDK()->GetImGuiBoldFont());

	auto& conds = state.spin.getConditions();

	for (auto& cond : conds) {
		auto kc = State::current.getTargetKillValidation(cond.target.get().getID());
		auto validation = " - "s;

		if (Config::main.overlayKillConfirmations) {
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

	DrawOverlayUIGameStatus();

	ImGui::PopFont();
}

auto UI::DrawOverlayUIGameStatus() -> void {
	if (statusDrawn) return;
	statusDrawn = true;

	auto text = std::string();

	if (Config::main.timer) {
		if (!text.empty()) text += " - ";
		auto timeFormat = std::string();
		auto const includeHr = std::chrono::duration_cast<std::chrono::hours>(state.timeElapsed).count() >= 1;
		auto const time = includeHr ? std::format("{:%H:%M:%S}", state.timeElapsed) : std::format("{:%M:%S}", state.timeElapsed);
		text += time;
	}

	if (Config::main.streak) {
		if (!text.empty()) text += " - ";
		text += std::format("Streak: {}", Config::main.streakCurrent);
	}

	if (!text.empty()) {
		auto windowWidth = ImGui::GetWindowSize().x;
		auto textWidth = ImGui::CalcTextSize(text.c_str()).x;

		ImGui::SetCursorPosX((windowWidth - textWidth) * 0.5f);
		ImGui::Text(text.c_str());
	}
}

auto UI::DrawBingoDebugUI(bool focused) -> void {
	if (!Config::main.debug)
		return;

	ImGui::PushFont(SDK()->GetImGuiBlackFont());

	auto viewportSize = ImGui::GetMainViewport()->Size;
	auto flags = static_cast<ImGuiWindowFlags>(ImGuiWindowFlags_AlwaysAutoResize);

	if (Config::main.overlayDockMode != DockMode::None || !focused)
		flags |= ImGuiWindowFlags_NoTitleBar;

	ImGui::SetNextWindowPos({0, viewportSize.y - this->debugOverlaySize.y});

	if (ImGui::Begin(ICON_MD_DEVELOPER_MODE " CROUPIER - DEBUG", nullptr, flags)) {
		this->debugOverlaySize = ImGui::GetWindowSize();

		ImGui::PushFont(SDK()->GetImGuiBoldFont());

		/*if (checkInRoom(pos, SVector3(-193.116, 13.079, -1.966), SVector3(-217.883, 16.94, 2.422)))
			this->state.room = "Catwalk";
		else if (checkInRoom(pos, SVector3(-127.372, -13.625, 13.328), SVector3(-220.491, 52.328, 19.64)))
			this->state.room = "Attic";
		else if (checkInRoom(pos, SVector3(-231.592, 5.265, 13.348), SVector3(-256.671, 24.302, 19.812)))
			this->state.room = "Auction";
		else if (checkInRoom(pos, SVector3(-182.083, 0.243, 5.3), SVector3(-229.967, 29.639, 10.153)))
			this->state.room = "A/V Center";
		else if (checkInRoom(pos, SVector3(-185.791, 29.832, -0.611), SVector3(-218.352, 51.745, 2.305)))
			this->state.room = "Bar";
		else if (checkInRoom(pos, SVector3(-185.942, -21.0, -1.933), SVector3(-225.432, -0.409, 5.0)))
			this->state.room = "Dressing Room";
		else if (checkInRoom(pos, SVector3(-146.75, 4.742, -3.594), SVector3(-181.564, 29.943, 12.532)))
			this->state.room = "Entrance Hall";
		else if (checkInRoom(pos, SVector3(-279.825, -2.929, -4.935), SVector3(-328.375, 32.669, 0.215)))
			this->state.room = "Helipad";
		else if (checkInRoom(pos, SVector3(-225.788, 30.123, -3.0), SVector3(-237.235, 47.972, 3.474)))
			this->state.room = "Kitchen";
		else if (checkInRoom(pos, SVector3(-242.748, -1.236, 7.0), SVector3(-256.497, 31.882, 12.485)))
			this->state.room = "Library";
		else if (checkInRoom(pos, SVector3(-180.98, 6.378, -5.539), SVector3(-190.999, 29.67, -4.28)))
			this->state.room = "Locker Room";
		else if (checkInRoom(pos, SVector3(-222.572, 5.101, 14.417), SVector3(-231.112, 24.53, 18.962)))
			this->state.room = "Voltaire Suite";
		else if (checkInRoom(pos, SVector3(-77.039, -62.16, -3.932), SVector3(-360.996, 90.328, 2.034)))
			this->state.room = "Ground Floor";
		else if (checkInRoom(pos, SVector3(-127.372, -13.625, 13.328), SVector3(-261.536, 52.669, 20.305)))
			this->state.room = "Top Floor";
		else if (checkInRoom(pos, SVector3(-174.261, -14.547, -7.168), SVector3(-247.184, 48.18, -4.099)))
			this->state.room = "Basement";*/

		//ImGui::Text(this->state.room.c_str());
		static std::string roomText;
		roomText = std::format("Room: {}", state.playerRoomId);
		ImGui::Text(roomText.c_str());

		auto str = std::format("{}, {}, {}", state.playerPos.x, state.playerPos.y, state.playerPos.z);
		ImGui::Text(str.c_str());

		if (ImGui::Button("Print")) {
			Logger::Info("{}, {}, {}", state.playerPos.x, state.playerPos.y, state.playerPos.z);
		}

		ImGui::PopFont();
	}

	ImGui::End();
	ImGui::PopFont();
}

auto UI::DrawEditSpinUI(bool focused) -> void {
	if (!this->showManualModeUI) return;
	if (!state.mission) {
		this->showManualModeUI = false;
		return;
	}

	ImGui::PushFont(SDK()->GetImGuiBlackFont());

	if (ImGui::Begin(ICON_MD_EDIT " CROUPIER - EDIT SPIN", &this->showManualModeUI, ImGuiWindowFlags_AlwaysAutoResize)) {
		ImGui::PushFont(SDK()->GetImGuiRegularFont());

		auto& mission = *state.mission;
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

			for (auto& cond : state.spin.getConditions()) {
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
					for (auto const method : State::current.generator.sodersKills) {
						auto const name = getSpecificKillMethodName(method);
						auto const selected = method == currentMapMethod;

						if (ImGui::Selectable(name.data(), selected)) {
							State::current.spin.setTargetMapMethod(target, method);
							SendSpinData();
						}

						if (selected) ImGui::SetItemDefaultFocus();
					}
				} else {
					for (auto const method : State::current.generator.standardKillMethods) {
						auto const name = getKillMethodName(method);
						auto const selected = method == currentMethod;

						if (ImGui::Selectable(name.data(), selected)) {
							State::current.spin.setTargetStandardMethod(target, method);
							SendSpinData();
						}

						if (selected) ImGui::SetItemDefaultFocus();
					}
				}

				ImGui::Selectable("------ FIREARMS ------", false, ImGuiSelectableFlags_Disabled);

				for (auto const method : State::current.generator.firearmKillMethods) {
					auto name = getKillMethodName(method);
					auto const selected = method == currentMethod;

					if (isSoders && isKillMethodElimination(method)) continue;

					if (ImGui::Selectable(name.data(), selected)) {
						if (selected) ImGui::SetItemDefaultFocus();
						State::current.spin.setTargetStandardMethod(target, method);
						SendSpinData();
					}

					if (selected) ImGui::SetItemDefaultFocus();
				}

				if (!isSoders) {
					ImGui::Selectable("------ MAP METHODS ------", false, ImGuiSelectableFlags_Disabled);

					for (auto const& method : State::current.generator.getMission()->getMapKillMethods()) {
						auto const selected = method.method == currentMapMethod;

						if (ImGui::Selectable(method.name.data(), selected)) {
							State::current.spin.setTargetMapMethod(target, method.method);
							SendSpinData();
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
						? State::current.generator.meleeKillTypes
						: (methodType == eMethodType::Gun ? State::current.generator.gunKillTypes : State::current.generator.explosiveKillTypes);

					for (auto type : killTypes) {
						auto const selected = currentKillType == type;
						auto const name = getKillTypeName(type);
						if (ImGui::Selectable(name.empty() ? "(Any)" : name.data(), selected)) {
							State::current.spin.setTargetMethodType(target, type);
							SendSpinData();
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
						State::current.spin.setTargetDisguise(target, disguise);
						SendSpinData();
					}

					if (selected) ImGui::SetItemDefaultFocus();
				}

				ImGui::EndCombo();
			}

			ImGui::SameLine();

			bool liveKill = currentKillComplication == eKillComplication::Live;
			if (ImGui::Checkbox(("Live (No KO)##"s + target.getName()).c_str(), &liveKill)) {
				State::current.spin.setTargetComplication(target, liveKill ? eKillComplication::Live : eKillComplication::None);
				SendSpinData();
			}
		}

		ImGui::PopFont();
	}

	ImGui::End();
	ImGui::PopFont();
}

auto UI::DrawCustomRulesetUI(bool focused) -> void {
	if (!this->showCustomRulesetUI) return;

	ImGui::PushFont(SDK()->GetImGuiBlackFont());

	if (ImGui::Begin(ICON_MD_EDIT " CROUPIER - CONFIGURE RULESET", &this->showCustomRulesetUI, ImGuiWindowFlags_AlwaysAutoResize)) {
		ImGui::PushFont(SDK()->GetImGuiRegularFont());

		ImGui::PushFont(SDK()->GetImGuiBoldFont());
		ImGui::TextUnformatted("Toggle conditions of particular difficulty levels, regardless of RR ban status.");
		ImGui::PopFont();

		ImGui::TextUnformatted("'Medium' conditions require decent map knowledge but aren't too hard to pull off (Yuki fire).");

		if (ImGui::Checkbox("Enable medium conditions", &State::current.rules.enableMedium))
			State::OnRulesetCustomised();

		ImGui::TextUnformatted("'Hard' conditions require good routing, practice and some tricks (Claus prisoner, Bangkok stalker).");

		if (ImGui::Checkbox("Enable hard conditions", &State::current.rules.enableHard))
			State::OnRulesetCustomised();

		ImGui::TextUnformatted("'Extreme' conditions may require glitches, huge time investment or advanced tricks (Paris fire, WC Beak Staff etc.).");

		if (ImGui::Checkbox("Enable extreme conditions", &State::current.rules.enableExtreme))
			State::OnRulesetCustomised();

		ImGui::TextUnformatted("'Buggy' conditions have been found to encounter game bugs sometimes.");

		if (ImGui::Checkbox("Enable buggy conditions", &State::current.rules.enableBuggy))
			State::OnRulesetCustomised();

		ImGui::TextUnformatted("'Impossible' conditions are unfeasible due to objects not existing near target routes (Sapienza fire, etc.).");

		if (ImGui::Checkbox("Enable 'impossible' conditions", &State::current.rules.enableImpossible))
			State::OnRulesetCustomised();

		ImGui::PushFont(SDK()->GetImGuiBoldFont());
		ImGui::TextUnformatted("Toggle certain condition types from being in spins.");
		ImGui::PopFont();

		if (ImGui::Checkbox("Generic eliminations", &State::current.rules.genericEliminations))
			State::OnRulesetCustomised();

		if (ImGui::Checkbox("Live complications", &State::current.rules.liveComplications))
			State::OnRulesetCustomised();
		ImGui::SameLine();
		if (ImGui::Checkbox("Exclude 'Standard' kills", &State::current.rules.liveComplicationsExcludeStandard))
			State::OnRulesetCustomised();

		if (State::current.rules.liveComplications) {
			if (ImGui::SliderInt("Live complication chance", &State::current.rules.liveComplicationChance, 0, 100, "%d%%", ImGuiSliderFlags_AlwaysClamp))
				State::OnRulesetCustomised();
		}

		if (ImGui::Checkbox("'Melee' kill types", &State::current.rules.meleeKillTypes))
			State::OnRulesetCustomised();
		ImGui::SameLine();
		if (ImGui::Checkbox("'Thrown' kill types", &State::current.rules.thrownKillTypes))
			State::OnRulesetCustomised();

		ImGui::PopFont();
	}

	ImGui::End();
	ImGui::PopFont();
}

auto UI::DrawEditMissionPoolUI(bool focused) -> void {
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

			auto it = find(cbegin(Config::main.missionPool), cend(Config::main.missionPool), missionInfo.mission);
			auto enabled = it != cend(Config::main.missionPool);
			if (ImGui::Checkbox(missionInfo.name.data(), &enabled)) {
				auto it = remove(begin(Config::main.missionPool), end(Config::main.missionPool), missionInfo.mission);
				if (it != end(Config::main.missionPool)) Config::main.missionPool.erase(it, end(Config::main.missionPool));
				if (enabled) Config::main.missionPool.push_back(missionInfo.mission);
				SendMissions();
				Config::Save();
			}
		}

		ImGui::PopFont();
	}

	ImGui::End();
	ImGui::PopFont();
}


