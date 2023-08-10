#pragma once

enum class eRouletteRuleset {
	RR11,
	RR12,
	Default = RR12,
	Custom,
};

struct RouletteRuleset
{
	bool genericEliminations = false;
	bool thrownKillTypes = false;
	bool meleeKillTypes = false;
	bool liveComplications = false;
	bool liveComplicationsExcludeStandard = false;

	static inline auto compare(const RouletteRuleset& a, const RouletteRuleset& b) {
		return a.genericEliminations == b.genericEliminations
			&& a.liveComplications == b.liveComplications
			&& a.liveComplicationsExcludeStandard == b.liveComplicationsExcludeStandard
			&& a.meleeKillTypes == b.meleeKillTypes
			&& a.thrownKillTypes == b.thrownKillTypes;
	}
};

inline auto makeRouletteRuleset(eRouletteRuleset ruleset = eRouletteRuleset::Default) {
	auto result = RouletteRuleset{};
	switch (ruleset) {
	case eRouletteRuleset::RR11:
		result.genericEliminations = true;
		result.thrownKillTypes = true;
		result.meleeKillTypes = true;
		break;
	case eRouletteRuleset::RR12:
		result.liveComplications = true;
		result.liveComplicationsExcludeStandard = true;
		break;
	}
	return result;
}
