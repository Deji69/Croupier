#pragma once
#include "util.h"
#include <map>
#include <set>
#include <string>

enum class eMethodType {
	Map,
	Gun,
	Standard,
};

enum class eKillMethod {
	NONE,
	Pistol,
	SMG,
	Sniper,
	Shotgun,
	AssaultRifle,
	Elimination,
	PistolElimination,
	SMGElimination,
	Explosive,
	Drowning,
	FallingObject,
	Fall,
	Fire,
	Electrocution,
	Explosion,
	ConsumedPoison,
	InjectedPoison,
	FiberWire,
	NeckSnap,
};

enum class eKillType {
	Any,
	Silenced,
	Loud,
	Melee,
	Thrown,
	Remote,
	Impact,
	LoudRemote,
};

enum class eKillComplication {
	None,
	Live,
};

enum class eMapKillMethod {
	NONE,
	AmputationKnife,
	AntiqueCurvedKnife,
	BarberRazor,
	BattleAxe,
	BeakStaff,
	Broadsword,
	BurialKnife,
	CircumcisionKnife,
	CombatKnife,
	ConcealableKnife,
	Cleaver,
	FireAxe,
	FoldingKnife,
	GardenFork,
	GrapeKnife,
	Hatchet,
	HobbyKnife,
	Hook,
	Icicle,
	JarlsPirateSaber,
	Katana,
	KitchenKnife,
	KukriMachete,
	LetterOpener,
	Machete,
	MeatFork,
	OldAxe,
	OrnateScimitar,
	RustyScrewdriver,
	Saber,
	SacrificialKnife,
	SappersAxe,
	Scalpel,
	Scissors,
	ScrapSword,
	Screwdriver,
	Seashell,
	Shears,
	Shuriken,
	Starfish,
	Tanto,
	UnicornHorn,
	VikingAxe,

	HolidayFireAxe,
	XmasStar,

	Soders_Electrocution,
	Soders_Explosion,
	Soders_PoisonStemCells,
	Soders_RobotArms,
	Soders_ShootHeart,
	Soders_TrashHeart,
	Yuki_SabotageCableCar,
};

inline static const std::set<std::string, InsensitiveCompareLexicographic> nonLoudExplosives = {
	"fc715a9a-3bf1-4768-bd67-0def61b92551", // Remote Breaching Charge
	"9d5daae3-10c8-4f03-a85d-9bd92861a672", // Breaching Charge Mk II
	"293af6cc-dd8d-4641-b650-14cdfd00f1de" // Breaching Charge Mk III
};

inline static const std::set<std::string, InsensitiveCompareLexicographic> impactExplosives = {
	"8b7c3ec6-c072-4a21-a323-0f8751028052", // Explosive Baseball
	"485f8902-b7e3-4916-8b90-ea7cebb305de", // Explosive Golf Ball 1
	"c95c55aa-34e5-42bd-bf27-32be3978b269", // Explosive Golf Ball 2
	"2a493cf9-7cb1-4aad-b892-17abf8b329f4", // ICA Impact Explosive
	"c82fefa7-febe-46c8-90ec-c945fbef0cb4", // Kronstadt Octane Booster
	"a83349bf-3d9c-43ec-92ee-c8c98cbeabc1", // Molotov Cocktail
	"af8a7b6c-692c-4a76-b9bc-2b91ce32bcbc", // Nitroglycerin
};

inline static auto checkExplosiveKillType(std::string repoId, eKillType kt) -> bool {
	switch (kt) {
	case eKillType::Any:
		return true;
	case eKillType::Loud:
		return !nonLoudExplosives.contains(repoId);
	case eKillType::Remote:
		return !impactExplosives.contains(repoId);
	case eKillType::Impact:
		if (repoId.empty()) return true;
		return impactExplosives.contains(repoId);
	case eKillType::LoudRemote:
		return !nonLoudExplosives.contains(repoId) && !impactExplosives.contains(repoId);
	}
	return false;
}

inline static const std::unordered_map<std::string, eMapKillMethod> specificKillMethodsByRepoId = {
	{"62c2ac2e-329e-4648-822a-e45a29a93cd0", eMapKillMethod::AmputationKnife},
	{"5c211971-235a-4856-9eea-fe890940f63a", eMapKillMethod::AntiqueCurvedKnife},
	{"5ce2f842-e091-4ead-a51c-1cc406309c8d", eMapKillMethod::BarberRazor},
	{"58dceb1c-d7db-41dc-9750-55e3ab87fdf0", eMapKillMethod::BattleAxe},
	{"b153112f-9cd1-4a49-a9c6-ba1a34f443ab", eMapKillMethod::BeakStaff},
	{"d938aa5c-72cf-4907-8bf5-522a67a11ae5", eMapKillMethod::Broadsword},
	{"6d4c88f3-9a09-453c-9a6e-a081f1136bf3", eMapKillMethod::BurialKnife},
	{"e312a416-5b56-4cb5-8994-1d4bc82fbb84", eMapKillMethod::CircumcisionKnife},
	{"2c037ef5-a01b-4532-8216-1d535193a837", eMapKillMethod::CombatKnife},
	{"e30a5b15-ce4d-41d5-a2a5-08dec9c4fe79", eMapKillMethod::ConcealableKnife},
	{"1bbf0ed5-0515-4599-a4c9-454ce59cff44", eMapKillMethod::Cleaver},
	{"a8bc4325-718e-45ba-b0e4-000729c70ce4", eMapKillMethod::FireAxe},
	{"a2c56798-026f-4d0b-9480-de0d2525a119", eMapKillMethod::FoldingKnife},
	{"1a105af8-fd30-447f-8b2c-f908f702e81c", eMapKillMethod::GardenFork},
	{"2b1bd2af-554e-4ea7-a717-3f6d0eb0215f", eMapKillMethod::GrapeKnife},
	{"3a8207bb-84f5-438f-8f30-5c83aef2af80", eMapKillMethod::Hatchet},
	{"9e728dc1-3344-4615-be7a-1bcbdd7ad4aa", eMapKillMethod::HobbyKnife},
	{"20f7ee3f-b261-4baa-8965-9b0b9061f27f", eMapKillMethod::Hook},
	{"b1b40b14-eded-404f-b933-c4da15e85644", eMapKillMethod::Icicle},
	{"fba6e133-78d1-4af1-8450-1ff30466c553", eMapKillMethod::JarlsPirateSaber},
	{"5631dace-7f4a-4df8-8e97-b47373b815ff", eMapKillMethod::Katana},
	{"e17172cc-bf70-4df6-9828-d9856b1a24fd", eMapKillMethod::KitchenKnife},
	{"c61fea13-aaf0-4173-8fd0-9c34b43638ae", eMapKillMethod::KukriMachete},
	{"f1f89faf-a441-4492-b442-9a923b5ecfd1", eMapKillMethod::LetterOpener},
	{"3e3819ca-4d19-4e0a-a238-4bd16c730e61", eMapKillMethod::Machete},
	{"66024572-7838-42d3-8c7b-c651e259438e", eMapKillMethod::MeatFork},
	{"369c68f7-cbef-4e45-83c7-8acd0dc2d237", eMapKillMethod::OldAxe},
	{"b4d4ed1a-0687-48a9-a731-0e3b99494eb6", eMapKillMethod::OrnateScimitar},
	{"ecf022db-ecfd-48c0-97b5-2258e4e89a65", eMapKillMethod::RustyScrewdriver},
	{"94f52181-b9ec-4363-baef-d53b4e424b74", eMapKillMethod::Saber},
	{"b2321154-4520-4911-9d94-9256b85e0983", eMapKillMethod::SacrificialKnife},
	{"d2a7fa04-2cac-45d8-b696-47c566bb95ff", eMapKillMethod::SappersAxe},
	{"5d8ca32a-fe4c-4597-b074-51e36c3de898", eMapKillMethod::Scalpel},
	{"6ecf1f15-453c-4783-9c70-8777c83934d7", eMapKillMethod::Scissors},
	{"d73251b4-4860-4b5b-8376-7c9cf2a054a2", eMapKillMethod::ScrapSword},
	{"12cb6b51-a6dd-4bf5-9653-0ab727820cac", eMapKillMethod::Screwdriver},
	{"7bc45270-83fe-4cf6-ad10-7d1b0cf3a3fd", eMapKillMethod::Seashell},
	{"42c7bb52-a71b-489c-8a74-7db0c09ba313", eMapKillMethod::Shears},
	{"e55eb9a4-e79c-43c7-970b-79e94e7683b7", eMapKillMethod::Shuriken},
	{"cad726d7-331d-4601-9723-6b8a17e5f91b", eMapKillMethod::Starfish},
	{"9488fa1e-10e1-49c9-bb24-6635d2e5bd49", eMapKillMethod::Tanto},
	{"58769c58-3e70-4746-be8e-4c7114f8c2bb", eMapKillMethod::UnicornHorn},
	{"9a7711c7-ede9-4230-853e-ab94c65fc0c9", eMapKillMethod::VikingAxe},
	{"2add9602-cda7-43fd-9758-6269c8fbb233", eMapKillMethod::HolidayFireAxe},
	{"1a852006-e632-401f-aedc-d0cf76521b1f", eMapKillMethod::XmasStar},
};
