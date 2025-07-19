#include "State.h"

using namespace Croupier;

State State::current;

std::function<void(bool)> Commands::Respin;
std::function<void()> Commands::Random;
std::function<void()> Commands::PreviousSpin;

auto State::OnRulesetSelect(eRouletteRuleset ruleset) -> void {
	State::current.ruleset = ruleset;
	if (ruleset != eRouletteRuleset::Custom)
		State::current.rules = makeRouletteRuleset(ruleset);
	try {
		State::current.generator.setRuleset(&State::current.rules);
	} catch (const RouletteGeneratorException& ex) {
		Logger::Error("Croupier: {}", ex.what());
	}
}

auto State::OnMissionSelect(eMission mission, bool isAuto) -> void {
	State::current.playerSelectMission();
	auto currentMission = State::current.spin.getMission();
	if (currentMission && mission == currentMission->getMission() && !State::current.spinCompleted) return;
	State::current.spinCompleted = false;

	try {
		State::current.generator.setMission(Missions::get(mission));
		Commands::Respin(isAuto);
	}
	catch (const RouletteGeneratorException& ex) {
		Logger::Error("Croupier: {}", ex.what());
	}
}

auto State::OnRulesetCustomised() -> void {
	if (RouletteRuleset::compare(State::current.rules, makeRouletteRuleset(eRouletteRuleset::RRWC2023)))
		State::OnRulesetSelect(eRouletteRuleset::RRWC2023);
	else if (RouletteRuleset::compare(State::current.rules, makeRouletteRuleset(eRouletteRuleset::RR12)))
		State::OnRulesetSelect(eRouletteRuleset::RR12);
	else if (RouletteRuleset::compare(State::current.rules, makeRouletteRuleset(eRouletteRuleset::RR11)))
		State::OnRulesetSelect(eRouletteRuleset::RR11);
	else if (RouletteRuleset::compare(State::current.rules, makeRouletteRuleset(eRouletteRuleset::Normal)))
		State::OnRulesetSelect(eRouletteRuleset::Normal);
	else
		State::OnRulesetSelect(eRouletteRuleset::Custom);
}
