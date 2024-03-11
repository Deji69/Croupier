#pragma once
#include "Target.h"
#include <string>
#include <string_view>

enum class eKillValidationType
{
	Unknown = 0,
	Invalid = 1,
	Valid = 2,
	Incomplete = 3,
};

struct DisguiseChange
{
	std::string disguiseRepoId;
	float timestamp;

	DisguiseChange(std::string_view repoId, double timestamp) : disguiseRepoId(toLowerCase(repoId)), timestamp(timestamp)
	{ }
};

struct KillConfirmation
{
	eTargetID target = eTargetID::Unknown;
	eTargetID specificTarget = eTargetID::Unknown;
	bool correctDisguise = false;
	eKillValidationType correctMethod = eKillValidationType::Incomplete;
	bool isPacified = false;

	KillConfirmation() = default;
	KillConfirmation(eTargetID target, eKillValidationType validation = eKillValidationType::Incomplete) : target(target), correctMethod(validation)
	{ }
};
