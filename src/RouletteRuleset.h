#pragma once
#include <Glacier/ZPrimitives.h>

enum class eRouletteRuleset {
	RR11,
	RR12,
	Default = RR12,
	Custom,
};

struct RouletteRuleset
{
	bool genericEliminations = false;
	bool meleeKillTypes = false;
	bool thrownKillTypes = false;
	bool liveComplications = false;
	bool liveComplicationsExcludeStandard = false;
	uint32 liveComplicationChance = 30;

	static inline auto compare(const RouletteRuleset& a, const RouletteRuleset& b) {
		return a.genericEliminations == b.genericEliminations
			&& a.meleeKillTypes == b.meleeKillTypes
			&& a.thrownKillTypes == b.thrownKillTypes
			&& a.liveComplications == b.liveComplications
			&& a.liveComplicationsExcludeStandard == b.liveComplicationsExcludeStandard
			&& a.liveComplicationChance == b.liveComplicationChance;
	}
};

inline auto getRulesetName(eRouletteRuleset ruleset) -> std::string_view {
	switch (ruleset) {
	case eRouletteRuleset::RR11: return "RR11";
	case eRouletteRuleset::RR12: return "RR12";
	case eRouletteRuleset::Custom: return "Custom";
	}
	return "";
}

inline auto makeRouletteRuleset(eRouletteRuleset ruleset = eRouletteRuleset::Default) {
	auto result = RouletteRuleset{};
	switch (ruleset) {
	case eRouletteRuleset::RR11:
		result.genericEliminations = true;
		result.meleeKillTypes = true;
		result.thrownKillTypes = true;
		break;
	case eRouletteRuleset::RR12:
		result.liveComplications = true;
		result.liveComplicationsExcludeStandard = true;
		result.liveComplicationChance = 20;
		break;
	}
	return result;
}
