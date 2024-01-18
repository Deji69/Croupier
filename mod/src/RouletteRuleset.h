#pragma once
#include <optional>
#include <string_view>
#include <vector>

enum class eRouletteRuleset {
	RR11,
	RR12,
	RRWC2023,
	Default = RRWC2023,
	Normal,
	Custom,
};

struct RulesetInfo
{
	eRouletteRuleset ruleset;
	std::string_view name;

	RulesetInfo(eRouletteRuleset ruleset, std::string_view name) :
		ruleset(ruleset), name(name)
	{
	}
};

inline std::vector<RulesetInfo> rulesets = {
	{eRouletteRuleset::RR12, "RR12"},
	{eRouletteRuleset::RR11, "RR11"},
	{eRouletteRuleset::RRWC2023, "RRWC 2023"},
	{eRouletteRuleset::Normal, "Normal"},
	{eRouletteRuleset::Custom, "Custom"},
};

struct RouletteRuleset
{
	bool genericEliminations = false;
	bool meleeKillTypes = false;
	bool thrownKillTypes = false;
	bool liveComplications = false;
	bool liveComplicationsExcludeStandard = false;
	bool enableMedium = false;
	bool enableHard = false;
	bool enableExtreme = false;
	bool enableBuggy = false;
	bool enableImpossible = false;
	//
	bool allowDuplicateDisguise = false;
	bool allowDuplicateMethod = false;
	
	int liveComplicationChance = 30;

	static inline auto compare(const RouletteRuleset& a, const RouletteRuleset& b) {
		return a.genericEliminations == b.genericEliminations
			&& a.meleeKillTypes == b.meleeKillTypes
			&& a.thrownKillTypes == b.thrownKillTypes
			&& a.liveComplications == b.liveComplications
			&& a.liveComplicationsExcludeStandard == b.liveComplicationsExcludeStandard
			&& a.liveComplicationChance == b.liveComplicationChance
			&& a.enableMedium == b.enableMedium
			&& a.enableHard == b.enableHard
			&& a.enableExtreme == b.enableExtreme
			&& a.enableBuggy == b.enableBuggy
			&& a.enableImpossible == b.enableImpossible;
	}
};

inline auto getRulesetByName(std::string_view name) {
	auto it = find_if(begin(rulesets), end(rulesets), [name](const RulesetInfo& info) {
		return info.name == name;
	});
	return it != end(rulesets) ? std::make_optional(it->ruleset) : std::nullopt;
}

inline auto getRulesetName(eRouletteRuleset ruleset) {
	auto it = find_if(begin(rulesets), end(rulesets), [ruleset](const RulesetInfo& info) {
		return info.ruleset == ruleset;
	});
	return it != end(rulesets) ? std::make_optional(it->name) : std::nullopt;
}

inline auto makeRouletteRuleset(eRouletteRuleset ruleset = eRouletteRuleset::Default) -> RouletteRuleset {
	auto result = RouletteRuleset{};
	switch (ruleset) {
	case eRouletteRuleset::RR11:
		result.genericEliminations = true;
		result.meleeKillTypes = true;
		result.thrownKillTypes = true;
		break;
	case eRouletteRuleset::RR12:
		result = makeRouletteRuleset(eRouletteRuleset::RR11);
		result.genericEliminations = false;
		result.meleeKillTypes = false;
		result.thrownKillTypes = false;
		result.liveComplications = true;
		result.liveComplicationsExcludeStandard = true;
		result.liveComplicationChance = 20;
		break;
	case eRouletteRuleset::RRWC2023:
		result = makeRouletteRuleset(eRouletteRuleset::RR12);
		result.liveComplicationChance = 25;
		break;
	case eRouletteRuleset::Normal:
		result = makeRouletteRuleset(eRouletteRuleset::RRWC2023);
		result.enableMedium = true;
		break;
	}
	return result;
}
