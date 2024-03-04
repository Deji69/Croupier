#pragma once
#include "Target.h"

enum class eKillValidationType
{
	Unknown = 0,
	Invalid = 1,
	Valid = 2,
	Incomplete = 3,
};

struct KillConfirmation
{
	eTargetID target = eTargetID::Unknown;
	bool correctDisguise = false;
	eKillValidationType correctMethod = eKillValidationType::Incomplete;

	KillConfirmation() = default;
	KillConfirmation(eTargetID target, eKillValidationType validation = eKillValidationType::Incomplete) : target(target), correctMethod(validation)
	{ }
};
