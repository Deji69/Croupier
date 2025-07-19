#include "UI.h"
#include "App.h"
#include "Config.h"
#include "State.h"
#include <Globals.h>
#include <IconsMaterialDesign.h>
#include <IPluginInterface.h>
#include <format>

using namespace Croupier;
using namespace std::string_literals;
using namespace std::string_view_literals;

auto UI::DrawMenu() -> void {
	if (ImGui::Button(ICON_MD_CASINO " CROUPIER"))
		this->showUI = !this->showUI;
}

auto UI::Draw(bool focused) -> void {
	this->DrawBingoDebugUI(focused);
	this->DrawSpinUI(focused);

	if (!focused) return;

	this->DrawEditSpinUI(focused);
	this->DrawCustomRulesetUI(focused);
	this->DrawEditMissionPoolUI(focused);

	if (!this->showUI) return;

	ImGui::PushFont(SDK()->GetImGuiBlackFont());
	ImGui::SetNextWindowContentSize(ImVec2(400, 0));

	if (ImGui::Begin(ICON_MD_SETTINGS " CROUPIER", &this->showUI, ImGuiWindowFlags_AlwaysAutoResize)) {
		auto connected = State::current.client.isConnected();
		ImGui::PushStyleColor(ImGuiCol_Text, connected ? IM_COL32(0, 255, 0, 255) : IM_COL32(255, 0, 0, 255));
		ImGui::TextUnformatted(connected ? "Connected" : "Disconnected");
		ImGui::PopStyleColor();

		ImGui::PushFont(SDK()->GetImGuiRegularFont());

#ifdef _DEBUG
		if (ImGui::Checkbox("Debug", &Configuration::main.debug))
			Configuration::main.Save();
#else
		if (Configuration::main.debug && ImGui::Checkbox("Debug", &Configuration::main.debug))
			Configuration::main.Save();
#endif

		if (ImGui::Checkbox("Overlay", &Configuration::main.spinOverlay))
			Configuration::main.Save();

		ImGui::SameLine(150.0);

		auto selectedOverlayDockName = "Undocked";

		switch (Configuration::main.overlayDockMode) {
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
			if (ImGui::Selectable("Undocked", Configuration::main.overlayDockMode == DockMode::None, 0)) {
				Configuration::main.overlayDockMode = DockMode::None;
				Configuration::main.Save();
			}
			if (Configuration::main.overlayDockMode == DockMode::None) ImGui::SetItemDefaultFocus();

			if (ImGui::Selectable("Top Left", Configuration::main.overlayDockMode == DockMode::TopLeft, 0)) {
				Configuration::main.overlayDockMode = DockMode::TopLeft;
				Configuration::main.Save();
			}
			if (Configuration::main.overlayDockMode == DockMode::TopLeft) ImGui::SetItemDefaultFocus();

			if (ImGui::Selectable("Top Right", Configuration::main.overlayDockMode == DockMode::TopRight, 0)) {
				Configuration::main.overlayDockMode = DockMode::TopRight;
				Configuration::main.Save();
			}
			if (Configuration::main.overlayDockMode == DockMode::TopRight) ImGui::SetItemDefaultFocus();

			if (ImGui::Selectable("Bottom Left", Configuration::main.overlayDockMode == DockMode::BottomLeft, 0)) {
				Configuration::main.overlayDockMode = DockMode::BottomLeft;
				Configuration::main.Save();
			}
			if (Configuration::main.overlayDockMode == DockMode::BottomLeft) ImGui::SetItemDefaultFocus();

			if (ImGui::Selectable("Bottom Right", Configuration::main.overlayDockMode == DockMode::BottomRight, 0)) {
				Configuration::main.overlayDockMode = DockMode::BottomRight;
				Configuration::main.Save();
			}
			if (Configuration::main.overlayDockMode == DockMode::BottomRight) ImGui::SetItemDefaultFocus();

			ImGui::EndCombo();
		}

		if (ImGui::Checkbox("Overlay Kill Confirmations", &Configuration::main.overlayKillConfirmations))
			Configuration::main.Save();

		if (!connected) {
			// Legacy timer
			if (ImGui::Checkbox("Timer", &Configuration::main.timer))
				Configuration::main.Save();

			if (Configuration::main.timer) {
				ImGui::SameLine();
				ImGui::SetCursorPosX(ImGui::GetCursorPosX() + 40.0);
				if (ImGui::Button("Reset"))
					State::current.timeStarted = std::chrono::steady_clock::now();
			}
		} else {
			// In-App Timer
			if (ImGui::Checkbox("Timer", &Configuration::main.timer))
				Configuration::main.Save();

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

		if (connected) {
			// Streak Tracking
			if (ImGui::Checkbox("Streak", &Configuration::main.streak))
				Configuration::main.Save();

			ImGui::SameLine(150.0);

			if (ImGui::Button("Reset##Streak")) {
				Configuration::main.streakCurrent = 0;
				SendResetStreak();
			}
		}

		if (!connected) {
			// Ruleset select
			auto const rulesetName = getRulesetName(State::current.ruleset);
			if (ImGui::BeginCombo("##Ruleset", rulesetName.value_or("").data(), ImGuiComboFlags_HeightLarge)) {
				for (auto& info : rulesets) {
					auto const selected = State::current.ruleset == info.ruleset;
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

		auto mission = State::current.spin.getMission();

		auto missionInfoIt = !mission ? missionInfos.end() : std::find_if(missionInfos.begin(), missionInfos.end(), [this](const MissionInfo& info) {
			return info.mission == State::current.spin.getMission()->getMission();
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

		if (connected) {
			if (ImGui::Button(ICON_MD_ARROW_BACK))
				SendPrev();
			if (ImGui::IsItemHovered(ImGuiHoveredFlags_AllowWhenDisabled)) ImGui::SetTooltip("Previous Spin");
			ImGui::SameLine();
			if (ImGui::Button(ICON_MD_ARROW_FORWARD))
				SendNext();
			if (ImGui::IsItemHovered(ImGuiHoveredFlags_AllowWhenDisabled)) ImGui::SetTooltip("Next Spin");
			ImGui::SameLine();
		}

		if (connected || State::current.spin.getMission()) {
			if (ImGui::Button(State::current.spinLocked ? ICON_MD_LOCK : ICON_MD_LOCK_OPEN)) {
				State::current.spinLocked = !State::current.spinLocked;
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

		if (!connected && !State::current.spinHistory.empty()) {
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

auto UI::DrawBingoDebugUI(bool focused) -> void {
	if (!Configuration::main.debug)
		return;

	ImGui::PushFont(SDK()->GetImGuiBlackFont());

	auto viewportSize = ImGui::GetMainViewport()->Size;
	auto flags = static_cast<ImGuiWindowFlags>(ImGuiWindowFlags_AlwaysAutoResize);

	if (Configuration::main.overlayDockMode != DockMode::None || !focused)
		flags |= ImGuiWindowFlags_NoTitleBar;

	ImGui::SetNextWindowPos({0, viewportSize.y - this->debugOverlaySize.y});

	if (ImGui::Begin(ICON_MD_DEVELOPER_MODE " CROUPIER - DEBUG", nullptr, flags)) {
		this->debugOverlaySize = ImGui::GetWindowSize();

		ImGui::PushFont(SDK()->GetImGuiBoldFont());

		auto const& pos = State::current.playerMatrix.Trans;

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
		roomText = std::format("Room: {}", State::current.roomId);
		ImGui::Text(roomText.c_str());

		auto str = std::format("{}, {}, {}", pos.x, pos.y, pos.z);
		ImGui::Text(str.c_str());

		if (ImGui::Button("Print")) {
			Logger::Info("{}, {}, {}", pos.x, pos.y, pos.z);
		}

		ImGui::PopFont();
	}

	ImGui::End();
	ImGui::PopFont();
}

auto UI::DrawSpinUI(bool focused) -> void {
	if (!Configuration::main.spinOverlay) return;

	ImGui::PushFont(SDK()->GetImGuiBlackFont());

	auto viewportSize = ImGui::GetMainViewport()->Size;
	auto flags = static_cast<ImGuiWindowFlags>(ImGuiWindowFlags_AlwaysAutoResize);

	if (Configuration::main.overlayDockMode != DockMode::None || !focused)
		flags |= ImGuiWindowFlags_NoTitleBar;

	switch (Configuration::main.overlayDockMode) {
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

	if (ImGui::Begin(ICON_MD_CASINO " CROUPIER - SPIN", &Configuration::main.spinOverlay, flags)) {
		this->overlaySize = ImGui::GetWindowSize();

		ImGui::PushFont(SDK()->GetImGuiBoldFont());

		auto elapsed = std::chrono::seconds::zero();
		auto& conds = State::current.spin.getConditions();

		elapsed = State::current.getTimeElapsed();
		for (auto i = 0; i < conds.size(); ++i) {
			auto& cond = conds[i];
			auto kc = State::current.getTargetKillValidation(cond.target.get().getID());
			auto validation = " - "s;

			if (Configuration::main.overlayKillConfirmations) {
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

		if (Configuration::main.timer) {
			if (!text.empty()) text += " - ";
			auto timeFormat = std::string();
			auto const includeHr = std::chrono::duration_cast<std::chrono::hours>(elapsed).count() >= 1;
			auto const time = includeHr ? std::format("{:%H:%M:%S}", elapsed) : std::format("{:%M:%S}", elapsed);
			text += time;
		}

		if (Configuration::main.streak) {
			if (!text.empty()) text += " - ";
			text += std::format("Streak: {}", Configuration::main.streakCurrent);
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

auto UI::DrawEditSpinUI(bool focused) -> void {
	if (!this->showManualModeUI) return;
	if (!State::current.generator.getMission()) {
		this->showManualModeUI = false;
		return;
	}

	ImGui::PushFont(SDK()->GetImGuiBlackFont());

	if (ImGui::Begin(ICON_MD_EDIT " CROUPIER - EDIT SPIN", &this->showManualModeUI, ImGuiWindowFlags_AlwaysAutoResize)) {
		ImGui::PushFont(SDK()->GetImGuiRegularFont());

		auto& mission = *State::current.generator.getMission();
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

			for (auto& cond : State::current.spin.getConditions()) {
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

			auto it = find(cbegin(Configuration::main.missionPool), cend(Configuration::main.missionPool), missionInfo.mission);
			auto enabled = it != cend(Configuration::main.missionPool);
			if (ImGui::Checkbox(missionInfo.name.data(), &enabled)) {
				auto it = remove(begin(Configuration::main.missionPool), end(Configuration::main.missionPool), missionInfo.mission);
				if (it != end(Configuration::main.missionPool)) Configuration::main.missionPool.erase(it, end(Configuration::main.missionPool));
				if (enabled) Configuration::main.missionPool.push_back(missionInfo.mission);
				SendMissions();
				Configuration::main.Save();
			}
		}

		ImGui::PopFont();
	}

	ImGui::End();
	ImGui::PopFont();
}


