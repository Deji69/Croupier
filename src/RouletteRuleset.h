#pragma once

enum class eRouletteRuleset {
	RR11,
	RR12,
	RRWC2023,
	Default = RRWC2023,
	Custom,
};

struct RouletteRuleset
{
	bool genericEliminations = false;
	bool meleeKillTypes = false;
	bool thrownKillTypes = false;
	bool liveComplications = false;
	bool liveComplicationsExcludeStandard = false;
	int liveComplicationChance = 30;

	static inline auto compare(const RouletteRuleset& a, const RouletteRuleset& b) {
		return a.genericEliminations == b.genericEliminations
			&& a.meleeKillTypes == b.meleeKillTypes
			&& a.thrownKillTypes == b.thrownKillTypes
			&& a.liveComplications == b.liveComplications
			&& a.liveComplicationsExcludeStandard == b.liveComplicationsExcludeStandard
			&& a.liveComplicationChance == b.liveComplicationChance;
	}
};

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
	}
	return result;
}
