#include <string>
#include "Roulette.h"
#include "RouletteMission.h"

using namespace std::string_literals;

const std::vector<eMission> defaultMissionPool = {
	eMission::PARIS_SHOWSTOPPER,
	eMission::SAPIENZA_WORLDOFTOMORROW,
	eMission::MARRAKESH_GILDEDCAGE,
	eMission::BANGKOK_CLUB27,
	eMission::COLORADO_FREEDOMFIGHTERS,
	eMission::HOKKAIDO_SITUSINVERSUS,
	eMission::MIAMI_FINISHLINE,
	eMission::SANTAFORTUNA_THREEHEADEDSERPENT,
	eMission::MUMBAI_CHASINGAGHOST,
	eMission::WHITTLETON_ANOTHERLIFE,
	eMission::ISLEOFSGAIL_THEARKSOCIETY,
	eMission::NEWYORK_GOLDENHANDSHAKE,
	eMission::HAVEN_THELASTRESORT,
	eMission::DUBAI_ONTOPOFTHEWORLD,
	eMission::DARTMOOR_DEATHINTHEFAMILY,
	eMission::CHONGQING_ENDOFANERA,
	eMission::BERLIN_APEXPREDATOR,
	eMission::MENDOZA_THEFAREWELL,
	eMission::AMBROSE_SHADOWSINTHEWATER,
};

const std::unordered_map<std::string, eMission> missionsByCodename = {
	{"FREEFORM", eMission::ICAFACILITY_FREEFORM},
	{"POLARBEAR", eMission::ICAFACILITY_FREEFORM},
	{"FINALTEST", eMission::ICAFACILITY_FINALTEST},
	{"GRADUATION", eMission::ICAFACILITY_FINALTEST},
	{"PEACOCK", eMission::PARIS_SHOWSTOPPER},
	{"PARISNOEL", eMission::PARIS_HOLIDAYHOARDERS},
	{"OCTOPUS", eMission::SAPIENZA_WORLDOFTOMORROW},
	{"COPPERHEAD", eMission::SAPIENZA_THEICON},
	{"MAMBA", eMission::SAPIENZA_LANDSLIDE},
	{"LASTWORD", eMission::SAPIENZA_THEAUTHOR},
	{"SPIDER", eMission::MARRAKESH_GILDEDCAGE},
	{"PYTHON", eMission::MARRAKESH_HOUSEBUILTONSAND},
	{"TIGER", eMission::BANGKOK_CLUB27},
	{"TAMAGOZAKE", eMission::BANGKOK_THESOURCE},
	{"BULL", eMission::COLORADO_FREEDOMFIGHTERS},
	{"SNOWCRANE", eMission::HOKKAIDO_SITUSINVERSUS},
	{"MAMUSHI", eMission::HOKKAIDO_SNOWFESTIVAL},
	{"BRONX", eMission::HOKKAIDO_PATIENTZERO},
	{"SHEEP", eMission::HAWKESBAY_NIGHTCALL},
	{"FLAMINGO", eMission::MIAMI_FINISHLINE},
	{"COTTONMOUTH", eMission::MIAMI_ASILVERTONGUE},
	{"HIPPO", eMission::SANTAFORTUNA_THREEHEADEDSERPENT},
	{"ANACONDA", eMission::SANTAFORTUNA_EMBRACEOFTHESERPENT},
	{"MONGOOSE", eMission::MUMBAI_CHASINGAGHOST},
	{"KINGCOBRA", eMission::MUMBAI_ILLUSIONSOFGRANDEUR},
	{"SKUNK", eMission::WHITTLETON_ANOTHERLIFE},
	{"GARTERSNAKE", eMission::WHITTLETON_ABITTERPILL},
	{"MAGPIE", eMission::ISLEOFSGAIL_THEARKSOCIETY},
	{"RACCOON", eMission::NEWYORK_GOLDENHANDSHAKE},
	{"STINGRAY", eMission::HAVEN_THELASTRESORT},
	{"GECKO", eMission::DUBAI_ONTOPOFTHEWORLD},
	{"BULLDOG", eMission::DARTMOOR_DEATHINTHEFAMILY},
	{"FOX", eMission::BERLIN_APEXPREDATOR},
	{"RAT", eMission::CHONGQING_ENDOFANERA},
	{"LLAMA", eMission::MENDOZA_THEFAREWELL},
	{"WOLVERINE", eMission::CARPATHIAN_UNTOUCHABLE},
	{"DUGONG", eMission::AMBROSE_SHADOWSINTHEWATER},
};

const std::unordered_map<std::string, eMission> missionsByContractId = {
	//{"436cbe4-164b-450f-ad2c-77dec88f53dd", eMission::ICAFACILITY_ARRIVAL},
	//{"d241b00-f585-4e3d-bc61-3095af1b96e2", eMission::ICAFACILITY_GUIDED},
	{"573932d-7a34-44f1-bcf4-ea8f79f75710", eMission::ICAFACILITY_FREEFORM},
	{"da5f2b1-8529-48bb-a596-717f75f5eacb", eMission::ICAFACILITY_FINALTEST},
	{"0000000-0000-0000-0000-000000000200", eMission::PARIS_SHOWSTOPPER},
	{"0000000-0000-0000-0000-000000000600", eMission::SAPIENZA_WORLDOFTOMORROW},
	{"0000000-0000-0000-0000-000000000400", eMission::MARRAKESH_GILDEDCAGE},
	{"b341d9f-58a4-411d-be57-0bc4ed85646b", eMission::BANGKOK_CLUB27},
	{"2bac555-bbb9-429d-a8ce-f1ffdf94211c", eMission::COLORADO_FREEDOMFIGHTERS},
	{"e81a82e-b409-41e9-9e3b-5f82e57f7a12", eMission::HOKKAIDO_SITUSINVERSUS},
	{"e45e91a-94ca-4d89-89fc-1b250e608e73", eMission::PARIS_HOLIDAYHOARDERS},
	{"0000000-0000-0000-0001-000000000006", eMission::SAPIENZA_THEICON},
	{"0000000-0000-0000-0001-000000000005", eMission::SAPIENZA_LANDSLIDE},
	{"e3f758a-2435-42de-93bd-d8f0b72c63a4", eMission::SAPIENZA_THEAUTHOR},
	{"ed93d8f-9535-425a-beb9-ef219e781e81", eMission::MARRAKESH_HOUSEBUILTONSAND},
	{"24b6964-a3bb-4457-b085-08f9a7dc7fb7", eMission::BANGKOK_THESOURCE},
	//{"da6205e-6ee8-4189-9cdb-4947cccd84f4", eMission::COLORADO_THEVECTOR},
	{"2befcec-7799-4987-9215-6a152cb6a320", eMission::HOKKAIDO_PATIENTZERO},
	{"414a084-a7b9-43ce-b6ca-590620acd87e", eMission::HOKKAIDO_SNOWFESTIVAL},
	{"65019e5-43a8-4a33-8a2a-84c750a5eeb3", eMission::HAWKESBAY_NIGHTCALL},
	{"1d015b4-be08-4e44-808e-ada0f387656f", eMission::MIAMI_FINISHLINE},
	{"22519be-ed2e-44df-9dac-18f739d44fd9", eMission::SANTAFORTUNA_THREEHEADEDSERPENT},
	{"fad48d7-3d0f-4c66-8605-6cbe9c3a46d7", eMission::MUMBAI_CHASINGAGHOST},
	{"2f55837-e26c-41bf-bc6e-fa97b7981fbc", eMission::WHITTLETON_ANOTHERLIFE},
	{"d225edf-40cd-4f20-a30f-b62a373801d3", eMission::ISLEOFSGAIL_THEARKSOCIETY},
	{"a03a97d-238c-48bd-bda0-e5f279569cce", eMission::NEWYORK_GOLDENHANDSHAKE},
	{"95261b5-e15b-4ca1-9bb7-001fb85c5aaa", eMission::HAVEN_THELASTRESORT},
	{"1ba328f-e3dd-4ef8-bb26-0363499fdd95", eMission::MIAMI_ASILVERTONGUE},
	{"79563a4-727a-4072-b354-c9fff4e8bff0", eMission::SANTAFORTUNA_EMBRACEOFTHESERPENT},
	{"8036782-de0a-4353-b522-0ab7a384bade", eMission::MUMBAI_ILLUSIONSOFGRANDEUR},
	{"b616e62-af0c-495b-82e3-b778e82b5912", eMission::WHITTLETON_ABITTERPILL},
	{"d85f2b0-80ca-49be-a2b7-d56f67faf252", eMission::DUBAI_ONTOPOFTHEWORLD},
	{"55984a8-fb0b-4673-8637-95cfe7d34e0f", eMission::DARTMOOR_DEATHINTHEFAMILY},
	{"bcd14b2-0786-4ceb-a2a4-e771f60d0125", eMission::BERLIN_APEXPREDATOR},
	{"d0cbb8c-2a80-442a-896b-fea00e98768c", eMission::CHONGQING_ENDOFANERA},
	{"42f850f-ca55-4fc9-9766-8c6a2b5c3129", eMission::MENDOZA_THEFAREWELL},
	{"3e19d55-64a6-4282-bb3c-d18c3f3e6e29", eMission::CARPATHIAN_UNTOUCHABLE},
	{"2aac100-dfc7-4f85-b9cd-528114436f6c", eMission::AMBROSE_SHADOWSINTHEWATER},
};

const std::vector<MissionInfo> missionInfos = {
	{eMission::NONE, "--------- PROLOGUE ---------", "PROLGUE", false},
	{eMission::ICAFACILITY_FREEFORM, "Prologue: Freeform Training", "Freeform Training", false},
	{eMission::ICAFACILITY_FINALTEST, "Prologue: The Final Test", "The Final Test", false},
	{eMission::NONE, "--------- SEASON 1 ---------", "SEASON 1"},
	{eMission::PARIS_SHOWSTOPPER, "Paris: The Showstopper", "The Showstopper"},
	{eMission::SAPIENZA_WORLDOFTOMORROW, "Sapienza: World of Tomorrow", "World of Tomorrow"},
	{eMission::MARRAKESH_GILDEDCAGE, "Marrakesh: A Gilded Cage", "A Gilded Cage"},
	{eMission::BANGKOK_CLUB27, "Bangkok: Club 27", "Club 27"},
	{eMission::COLORADO_FREEDOMFIGHTERS, "Colorado: Freedom Fighters", "Freedom Fighters"},
	{eMission::HOKKAIDO_SITUSINVERSUS, "Hokkaido: Situs Inversus", "Situs Inversus"},
	{eMission::NONE, "--- SEASON 1 BONUS ---", "SEASON 1 BONUS", false},
	{eMission::BANGKOK_THESOURCE, "Bangkok: The Source", "The Source", false},
	{eMission::SAPIENZA_THEAUTHOR, "Sapienza: The Author", "The Author", false},
	{eMission::HOKKAIDO_PATIENTZERO, "Hokkaido: Patient Zero", "Patient Zero", false},
	{eMission::PARIS_HOLIDAYHOARDERS, "Paris: Holiday Hoarders", "Holiday Hoarders", false},
	{eMission::SAPIENZA_THEICON, "Sapienza: The Icon", "The Icon", false},
	{eMission::SAPIENZA_LANDSLIDE, "Sapienza: Landslide", "Landslide", false},
	{eMission::MARRAKESH_HOUSEBUILTONSAND, "Marrakesh: A House Built On Sand", "A House Built On Sand", false},
	{eMission::HOKKAIDO_SNOWFESTIVAL, "Hokkaido: Snow Festival", "Snow Festival", false},
	{eMission::NONE, "--------- SEASON 2 ---------", "SEASON 2"},
	{eMission::HAWKESBAY_NIGHTCALL, "Hawke's Bay: Nightcall", "Nightcall", false},
	{eMission::MIAMI_FINISHLINE, "Miami: The Finish Line", "The Finish Line"},
	{eMission::SANTAFORTUNA_THREEHEADEDSERPENT, "Santa Fortuna: Three-Headed Serpent", "Three-Headed Serpent"},
	{eMission::MUMBAI_CHASINGAGHOST, "Mumbai: Chasing a Ghost", "Chasing a Ghost"},
	{eMission::WHITTLETON_ANOTHERLIFE, "Whittleton Creek: Another Life", "Another Life"},
	{eMission::ISLEOFSGAIL_THEARKSOCIETY, "Isle of Sgàil: The Ark Society", "The Ark Society"},
	{eMission::NEWYORK_GOLDENHANDSHAKE, "New York: Golden Handshake", "Golden Handshake"},
	{eMission::HAVEN_THELASTRESORT, "Haven: The Last Resort", "The Last Resort"},
	{eMission::NONE, "- SPECIAL ASSIGNMENTS -", "SPECIAL ASSIGNMENTS", false},
	{eMission::MIAMI_ASILVERTONGUE, "Miami: A Silver Tongue", "A Silver Tongue"},
	{eMission::SANTAFORTUNA_EMBRACEOFTHESERPENT, "Santa Fortuna: Embrace of the Serpent", "Embrace of the Serpent", false},
	{eMission::MUMBAI_ILLUSIONSOFGRANDEUR, "Mumbai: Illusions of Grandeur", "Illusions of Grandeur", false},
	{eMission::WHITTLETON_ABITTERPILL, "Whittleton Creek: A Bitter Pill", "A Bitter Pill", false},
	{eMission::NONE, "--------- SEASON 3 ---------", "SEASON 3"},
	{eMission::DUBAI_ONTOPOFTHEWORLD, "Dubai: On Top of the World", "On Top of the World"},
	{eMission::DARTMOOR_DEATHINTHEFAMILY, "Dartmoor: Death in the Family", "Death in the Family"},
	{eMission::BERLIN_APEXPREDATOR, "Berlin: Apex Predator", "Apex Predator"},
	{eMission::CHONGQING_ENDOFANERA, "Chongqing: End of an Era", "End of an Era"},
	{eMission::MENDOZA_THEFAREWELL, "Mendoza: The Farewell", "The Farewell"},
	{eMission::CARPATHIAN_UNTOUCHABLE, "Carpathian Mountains: Untouchable", "Untouchable", false},
	{eMission::AMBROSE_SHADOWSINTHEWATER, "Ambrose Island: Shadows in the Water", "Shadows in the Water"},
};

const std::unordered_map<eMission, std::vector<MapKillMethod>> missionMethods = {
	{eMission::PARIS_HOLIDAYHOARDERS, {
		eMapKillMethod::CircumcisionKnife,
		eMapKillMethod::HolidayFireAxe,
		eMapKillMethod::Katana,
		eMapKillMethod::Shuriken,
		eMapKillMethod::XmasStar,
		eMapKillMethod::BattleAxe,
		eMapKillMethod::Cleaver,
		eMapKillMethod::FireAxe,
		eMapKillMethod::Hatchet,
		eMapKillMethod::KitchenKnife,
		eMapKillMethod::LetterOpener,
		eMapKillMethod::Saber,
		eMapKillMethod::Scissors,
		eMapKillMethod::Screwdriver,
	}},
	{eMission::PARIS_SHOWSTOPPER, {
		eMapKillMethod::BattleAxe,
		eMapKillMethod::Cleaver,
		eMapKillMethod::FireAxe,
		eMapKillMethod::Hatchet,
		eMapKillMethod::KitchenKnife,
		eMapKillMethod::LetterOpener,
		eMapKillMethod::Saber,
		eMapKillMethod::Scissors,
		eMapKillMethod::Screwdriver,
	}},
	{eMission::SAPIENZA_WORLDOFTOMORROW, {
		eMapKillMethod::AmputationKnife,
		eMapKillMethod::BattleAxe,
		eMapKillMethod::CircumcisionKnife,
		eMapKillMethod::Cleaver,
		eMapKillMethod::CombatKnife,
		eMapKillMethod::FireAxe,
		eMapKillMethod::FoldingKnife,
		eMapKillMethod::Hatchet,
		eMapKillMethod::Katana,
		eMapKillMethod::KitchenKnife,
		eMapKillMethod::LetterOpener,
		eMapKillMethod::OldAxe,
		eMapKillMethod::Saber,
		eMapKillMethod::Scissors,
		eMapKillMethod::Screwdriver,
	}},
	{eMission::SAPIENZA_LANDSLIDE, {
		eMapKillMethod::Cleaver,
		eMapKillMethod::FireAxe,
		eMapKillMethod::FoldingKnife,
		eMapKillMethod::KitchenKnife,
		eMapKillMethod::LetterOpener,
		eMapKillMethod::OldAxe,
		eMapKillMethod::Saber,
		eMapKillMethod::Scissors,
		eMapKillMethod::Screwdriver,
	}},
	{eMission::SAPIENZA_THEICON, {
		eMapKillMethod::BattleAxe,
		eMapKillMethod::Cleaver,
		eMapKillMethod::FireAxe,
		eMapKillMethod::KitchenKnife,
		eMapKillMethod::Screwdriver,
	}},
	{eMission::SAPIENZA_THEAUTHOR, {
		eMapKillMethod::FireAxe,
		eMapKillMethod::KitchenKnife,
		eMapKillMethod::Scissors,
		eMapKillMethod::Screwdriver,
	}},
	{eMission::MARRAKESH_GILDEDCAGE, {
		eMapKillMethod::BattleAxe,
		eMapKillMethod::Cleaver,
		eMapKillMethod::FireAxe,
		eMapKillMethod::KitchenKnife,
		eMapKillMethod::LetterOpener,
		eMapKillMethod::Saber,
		eMapKillMethod::Scissors,
		eMapKillMethod::Screwdriver,
	}},
	{eMission::MARRAKESH_HOUSEBUILTONSAND, {
		eMapKillMethod::BattleAxe,
		eMapKillMethod::Cleaver,
		eMapKillMethod::FoldingKnife,
		eMapKillMethod::KitchenKnife,
		eMapKillMethod::Scissors,
		eMapKillMethod::Screwdriver,
	}},
	{eMission::BANGKOK_THESOURCE, {
		eMapKillMethod::AmputationKnife,
		eMapKillMethod::CircumcisionKnife,
		eMapKillMethod::Cleaver,
		eMapKillMethod::FireAxe,
		eMapKillMethod::Hatchet,
		eMapKillMethod::Katana,
		eMapKillMethod::KitchenKnife,
		eMapKillMethod::LetterOpener,
		eMapKillMethod::SappersAxe,
		eMapKillMethod::Screwdriver,
	}},
	{eMission::BANGKOK_CLUB27, {
		eMapKillMethod::Cleaver,
		eMapKillMethod::FireAxe,
		eMapKillMethod::Hatchet,
		eMapKillMethod::Katana,
		eMapKillMethod::KitchenKnife,
		eMapKillMethod::LetterOpener,
		eMapKillMethod::SappersAxe,
		eMapKillMethod::Screwdriver,
	}},
	{eMission::COLORADO_FREEDOMFIGHTERS, {
		eMapKillMethod::Cleaver,
		eMapKillMethod::KitchenKnife,
		eMapKillMethod::OldAxe,
		eMapKillMethod::Screwdriver,
	}},
	{eMission::HOKKAIDO_SNOWFESTIVAL, {
		eMapKillMethod::Icicle,
		eMapKillMethod::Cleaver,
		eMapKillMethod::FireAxe,
		eMapKillMethod::Katana,
		eMapKillMethod::KitchenKnife,
		eMapKillMethod::Scalpel,
		eMapKillMethod::Scissors,
		eMapKillMethod::Screwdriver,
	}},
	{eMission::HOKKAIDO_SITUSINVERSUS, {
		eMapKillMethod::Cleaver,
		eMapKillMethod::FireAxe,
		eMapKillMethod::Katana,
		eMapKillMethod::KitchenKnife,
		eMapKillMethod::Scalpel,
		eMapKillMethod::Scissors,
		eMapKillMethod::Screwdriver,
	}},
	{eMission::MIAMI_FINISHLINE, {
		eMapKillMethod::AmputationKnife,
		eMapKillMethod::BattleAxe,
		eMapKillMethod::Cleaver,
		eMapKillMethod::FireAxe,
		eMapKillMethod::KitchenKnife,
		eMapKillMethod::OldAxe,
		eMapKillMethod::Scissors,
		eMapKillMethod::Screwdriver,
		eMapKillMethod::Starfish,
	}},
	{eMission::MIAMI_ASILVERTONGUE, {
		eMapKillMethod::AmputationKnife,
		eMapKillMethod::BattleAxe,
		eMapKillMethod::Cleaver,
		eMapKillMethod::FireAxe,
		eMapKillMethod::KitchenKnife,
		eMapKillMethod::OldAxe,
		eMapKillMethod::Scissors,
		eMapKillMethod::Screwdriver,
		eMapKillMethod::Starfish,
	}},
	{eMission::SANTAFORTUNA_THREEHEADEDSERPENT, {
		eMapKillMethod::BarberRazor,
		eMapKillMethod::Cleaver,
		eMapKillMethod::FoldingKnife,
		eMapKillMethod::KitchenKnife,
		eMapKillMethod::LetterOpener,
		eMapKillMethod::Machete,
		eMapKillMethod::SacrificialKnife,
		eMapKillMethod::Scissors,
		eMapKillMethod::Screwdriver,
	}},
	{eMission::SANTAFORTUNA_EMBRACEOFTHESERPENT, {
		eMapKillMethod::BarberRazor,
		eMapKillMethod::Cleaver,
		eMapKillMethod::FoldingKnife,
		eMapKillMethod::KitchenKnife,
		eMapKillMethod::LetterOpener,
		eMapKillMethod::Machete,
		eMapKillMethod::SacrificialKnife,
		eMapKillMethod::Scissors,
		eMapKillMethod::Screwdriver,
	}},
	{eMission::MUMBAI_CHASINGAGHOST, {
		eMapKillMethod::AmputationKnife,
		eMapKillMethod::BarberRazor,
		eMapKillMethod::BeakStaff,
		eMapKillMethod::Cleaver,
		eMapKillMethod::FireAxe,
		eMapKillMethod::FoldingKnife,
		eMapKillMethod::KitchenKnife,
		eMapKillMethod::LetterOpener,
		eMapKillMethod::OldAxe,
		eMapKillMethod::Saber,
		eMapKillMethod::Scissors,
		eMapKillMethod::Screwdriver,
	}},
	{eMission::MUMBAI_ILLUSIONSOFGRANDEUR, {
		eMapKillMethod::AmputationKnife,
		eMapKillMethod::BarberRazor,
		eMapKillMethod::BeakStaff,
		eMapKillMethod::Cleaver,
		eMapKillMethod::FireAxe,
		eMapKillMethod::FoldingKnife,
		eMapKillMethod::KitchenKnife,
		eMapKillMethod::LetterOpener,
		eMapKillMethod::OldAxe,
		eMapKillMethod::Saber,
		eMapKillMethod::Scissors,
		eMapKillMethod::Screwdriver,
	}},
	{eMission::WHITTLETON_ANOTHERLIFE, {
		eMapKillMethod::BattleAxe,
		eMapKillMethod::BeakStaff,
		eMapKillMethod::FireAxe,
		eMapKillMethod::Hatchet,
		eMapKillMethod::KitchenKnife,
		eMapKillMethod::LetterOpener,
		eMapKillMethod::Scissors,
		eMapKillMethod::Screwdriver,
	}},
	{eMission::WHITTLETON_ABITTERPILL, {
		eMapKillMethod::BattleAxe,
		eMapKillMethod::BeakStaff,
		eMapKillMethod::FireAxe,
		eMapKillMethod::Hatchet,
		eMapKillMethod::KitchenKnife,
		eMapKillMethod::LetterOpener,
		eMapKillMethod::Scissors,
		eMapKillMethod::Screwdriver,
	}},
	{eMission::ISLEOFSGAIL_THEARKSOCIETY, {
		eMapKillMethod::BattleAxe,
		eMapKillMethod::Broadsword,
		eMapKillMethod::CircumcisionKnife,
		eMapKillMethod::Cleaver,
		eMapKillMethod::FireAxe,
		eMapKillMethod::Hatchet,
		eMapKillMethod::Katana,
		eMapKillMethod::KitchenKnife,
		eMapKillMethod::LetterOpener,
		eMapKillMethod::OldAxe,
		eMapKillMethod::Saber,
		eMapKillMethod::SacrificialKnife,
		eMapKillMethod::SappersAxe,
		eMapKillMethod::Scalpel,
		eMapKillMethod::Scissors,
		eMapKillMethod::Screwdriver,
		eMapKillMethod::Starfish,
		eMapKillMethod::VikingAxe,
	}},
	{eMission::HAVEN_THELASTRESORT, {
		eMapKillMethod::BarberRazor,
		eMapKillMethod::Cleaver,
		eMapKillMethod::FireAxe,
		eMapKillMethod::FoldingKnife,
		eMapKillMethod::Hatchet,
		eMapKillMethod::JarlsPirateSaber,
		eMapKillMethod::KitchenKnife,
		eMapKillMethod::LetterOpener,
		eMapKillMethod::Machete,
		eMapKillMethod::Scalpel,
		eMapKillMethod::Scissors,
		eMapKillMethod::Screwdriver,
		eMapKillMethod::Seashell,
		eMapKillMethod::Starfish,
	}},
	{eMission::DUBAI_ONTOPOFTHEWORLD, {
		eMapKillMethod::Cleaver,
		eMapKillMethod::FireAxe,
		eMapKillMethod::FoldingKnife,
		eMapKillMethod::KitchenKnife,
		eMapKillMethod::LetterOpener,
		eMapKillMethod::OrnateScimitar,
		eMapKillMethod::Scissors,
		eMapKillMethod::Screwdriver,
	}},
	{eMission::DARTMOOR_DEATHINTHEFAMILY, {
		eMapKillMethod::BarberRazor,
		eMapKillMethod::Cleaver,
		eMapKillMethod::FoldingKnife,
		eMapKillMethod::GardenFork,
		eMapKillMethod::Hatchet,
		eMapKillMethod::KitchenKnife,
		eMapKillMethod::LetterOpener,
		eMapKillMethod::Saber,
		eMapKillMethod::Scissors,
		eMapKillMethod::Screwdriver,
		eMapKillMethod::Shears,
		eMapKillMethod::UnicornHorn,
	}},
	{eMission::BERLIN_APEXPREDATOR, {
		eMapKillMethod::BattleAxe,
		eMapKillMethod::CombatKnife,
		eMapKillMethod::ConcealableKnife,
		eMapKillMethod::FireAxe,
		eMapKillMethod::GardenFork,
		eMapKillMethod::HobbyKnife,
		eMapKillMethod::KitchenKnife,
		eMapKillMethod::OldAxe,
		eMapKillMethod::ScrapSword,
		eMapKillMethod::Screwdriver,
	}},
	{eMission::CHONGQING_ENDOFANERA, {
		eMapKillMethod::Cleaver,
		eMapKillMethod::FireAxe,
		eMapKillMethod::GardenFork,
		eMapKillMethod::HobbyKnife,
		eMapKillMethod::KitchenKnife,
		eMapKillMethod::LetterOpener,
		eMapKillMethod::Scissors,
		eMapKillMethod::ScrapSword,
		eMapKillMethod::Screwdriver,
		eMapKillMethod::Tanto,
	}},
	{eMission::MENDOZA_THEFAREWELL, {
		eMapKillMethod::Broadsword,
		eMapKillMethod::CombatKnife,
		eMapKillMethod::FireAxe,
		eMapKillMethod::GardenFork,
		eMapKillMethod::GrapeKnife,
		eMapKillMethod::Hatchet,
		eMapKillMethod::HobbyKnife,
		eMapKillMethod::Icicle,
		eMapKillMethod::JarlsPirateSaber,
		eMapKillMethod::KitchenKnife,
		eMapKillMethod::Machete,
		eMapKillMethod::SappersAxe,
		eMapKillMethod::Screwdriver,
		eMapKillMethod::Shears,
		eMapKillMethod::OldAxe,
	}},
	{eMission::CARPATHIAN_UNTOUCHABLE, {
		eMapKillMethod::BarberRazor,
		eMapKillMethod::FireAxe,
		eMapKillMethod::Icicle,
		eMapKillMethod::RustyScrewdriver,
	}},
	{eMission::AMBROSE_SHADOWSINTHEWATER, {
		eMapKillMethod::BarberRazor,
		eMapKillMethod::Cleaver,
		eMapKillMethod::FoldingKnife,
		eMapKillMethod::GardenFork,
		eMapKillMethod::HobbyKnife,
		eMapKillMethod::Hook,
		eMapKillMethod::JarlsPirateSaber,
		eMapKillMethod::KitchenKnife,
		eMapKillMethod::KukriMachete,
		eMapKillMethod::LetterOpener,
		eMapKillMethod::MeatFork,
		eMapKillMethod::OldAxe,
		eMapKillMethod::Scissors,
		eMapKillMethod::ScrapSword,
		eMapKillMethod::Screwdriver,
		eMapKillMethod::Seashell,
		eMapKillMethod::Shears,
		eMapKillMethod::Starfish,
	}},
};
const std::unordered_map<eMission, std::vector<RouletteDisguise>> missionDisguises = {
	{eMission::ICAFACILITY_GUIDED, {
		RouletteDisguise{"Suit", "outfit_22725852-7989-463a-822a-5848b1b2c6cf_0.jpg", "22725852-7989-463a-822a-5848b1b2c6cf", true},
		RouletteDisguise{"Bodyguard", "outfit_cf170965-5582-48d7-8dd7-774ae0a144dd_0.jpg", "cf170965-5582-48d7-8dd7-774ae0a144dd"},
		RouletteDisguise{"Mechanic", "outfit_e222cc14-8d48-42de-9af6-1b745dbb3614_0.jpg", "e222cc14-8d48-42de-9af6-1b745dbb3614"},
		RouletteDisguise{"Terry Norfolk", "outfit_63d2164f-efa3-4a19-aaa0-279a0029dd74_0.jpg", "_63d2164f-efa3-4a19-aaa0-279a0029dd74"},
		RouletteDisguise{"Yacht Crew", "outfit_bbffa24b-fa46-4f9d-a73d-71de56ff3bfe_0.jpg", "bbffa24b-fa46-4f9d-a73d-71de56ff3bfe"},
		RouletteDisguise{"Yacht Security", "outfit_f7acaf86-205c-4ac4-98c7-2c418007299c_0.jpg", "f7acaf86-205c-4ac4-98c7-2c418007299c"},
	}},
	{eMission::ICAFACILITY_FREEFORM, {
		RouletteDisguise{"Suit", "outfit_22725852-7989-463a-822a-5848b1b2c6cf_0.jpg", "22725852-7989-463a-822a-5848b1b2c6cf", true},
		RouletteDisguise{"Bodyguard", "outfit_cf170965-5582-48d7-8dd7-774ae0a144dd_0.jpg", "cf170965-5582-48d7-8dd7-774ae0a144dd"},
		RouletteDisguise{"Mechanic", "outfit_e222cc14-8d48-42de-9af6-1b745dbb3614_0.jpg", "e222cc14-8d48-42de-9af6-1b745dbb3614"},
		RouletteDisguise{"Terry Norfolk", "outfit_63d2164f-efa3-4a19-aaa0-279a0029dd74_0.jpg", "63d2164f-efa3-4a19-aaa0-279a0029dd74"},
		RouletteDisguise{"Yacht Crew", "outfit_bbffa24b-fa46-4f9d-a73d-71de56ff3bfe_0.jpg", "bbffa24b-fa46-4f9d-a73d-71de56ff3bfe"},
		RouletteDisguise{"Yacht Security", "outfit_f7acaf86-205c-4ac4-98c7-2c418007299c_0.jpg", "f7acaf86-205c-4ac4-98c7-2c418007299c"},
	}},
	{eMission::ICAFACILITY_FINALTEST, {
		RouletteDisguise{"Suit", "outfit_22725852-7989-463a-822a-5848b1b2c6cf_0.jpg", "22725852-7989-463a-822a-5848b1b2c6cf", true},
		RouletteDisguise{"Airfield Security", "outfit_f7acaf86-205c-4ac4-98c7-2c418007299c_0.jpg", "f7acaf86-205c-4ac4-98c7-2c418007299c"},
		RouletteDisguise{"Airplane Mechanic", "outfit_8f6ea4f1-32a8-4e57-a39d-90a2c2ff2bb0_0.jpg", "8f6ea4f1-32a8-4e57-a39d-90a2c2ff2bb0"},
		RouletteDisguise{"KGB Officer", "outfit_a1bb1e004-7fdf-462b-96b3-074e3390c171_0.jpg", "a1bb1e004-7fdf-462b-96b3-074e3390c171"},
		RouletteDisguise{"Soviet Soldier", "outfit_5c419edc-203d-4736-8cd9-bed24e34171c_0.jpg", "5c419edc-203d-4736-8cd9-bed24e34171c"},
	}},
	{eMission::PARIS_HOLIDAYHOARDERS, {
		RouletteDisguise{"Suit", "outfit_4ea5c203-b7c4-489d-85a1-bf91272d6190_0.jpg", "4ea5c203-b7c4-489d-85a1-bf91272d6190", true},
		RouletteDisguise{"Auction Staff", "outfit_b5664bed-462a-417c-bc07-6d9d3f666e2d_0.jpg", "b5664bed-462a-417c-bc07-6d9d3f666e2d"},
		RouletteDisguise{"Chef", "outfit_26ae3261-2a1d-49b2-8cab-d626d0887836_0.jpg", "26ae3261-2a1d-49b2-8cab-d626d0887836"},
		RouletteDisguise{"CICADA Bodyguard", "outfit_d2c76544-3a12-43a8-abc3-c7ce51830c1e_0.jpg", "d2c76544-3a12-43a8-abc3-c7ce51830c1e"},
		RouletteDisguise{"Helmut Kruger", "outfit_642c20f9-bf41-41b4-b0bb-2491b5be938a_0.jpg", "642c20f9-bf41-41b4-b0bb-2491b5be938a"},
		RouletteDisguise{"Palace Staff", "outfit_2018db77-aa8a-4bf9-9afb-56bdaa161156_0.jpg", "2018db77-aa8a-4bf9-9afb-56bdaa161156"},
		RouletteDisguise{"Santa", "outfit_315400cd-90d8-43cc-8c22-62c0cb8969a5_0.jpg", "315400cd-90d8-43cc-8c22-62c0cb8969a5"},
		RouletteDisguise{"Security Guard", "outfit_992cc7b6-4ccf-4ae8-a467-e9b2aabaeeb5_0.jpg", "992cc7b6-4ccf-4ae8-a467-e9b2aabaeeb5"},
		RouletteDisguise{"Sheikh Salman Al-Ghazali", "outfit_a8ecd823-6e08-4cfe-a04d-816d387fcf0c_0.jpg", "a8ecd823-6e08-4cfe-a04d-816d387fcf0c"},
		RouletteDisguise{"Stylist", "outfit_96e32a7a-129a-4dd6-9b5b-3000a58f2a0f_0.jpg", "96e32a7a-129a-4dd6-9b5b-3000a58f2a0f"},
		RouletteDisguise{"Tech Crew", "outfit_69aac6db-461e-43af-89bc-2c27e50d430f_0.jpg", "69aac6db-461e-43af-89bc-2c27e50d430f"},
		RouletteDisguise{"Vampire Magician", "outfit_1fdc259e-b96a-47f2-bbd8-e86e78d6df70_0.jpg", "1fdc259e-b96a-47f2-bbd8-e86e78d6df70"},
	}},
	{eMission::PARIS_SHOWSTOPPER, {
		RouletteDisguise{"Suit", "outfit_4ea5c203-b7c4-489d-85a1-bf91272d6190_0.jpg", "4ea5c203-b7c4-489d-85a1-bf91272d6190", true},
		RouletteDisguise{"Auction Staff", "outfit_b5664bed-462a-417c-bc07-6d9d3f666e2d_0.jpg", "b5664bed-462a-417c-bc07-6d9d3f666e2d"},
		RouletteDisguise{"Chef", "outfit_26ae3261-2a1d-49b2-8cab-d626d0887836_0.jpg", "26ae3261-2a1d-49b2-8cab-d626d0887836"},
		RouletteDisguise{"CICADA Bodyguard", "outfit_d2c76544-3a12-43a8-abc3-c7ce51830c1e_0.jpg", "d2c76544-3a12-43a8-abc3-c7ce51830c1e"},
		RouletteDisguise{"Helmut Kruger", "outfit_642c20f9-bf41-41b4-b0bb-2491b5be938a_0.jpg", "642c20f9-bf41-41b4-b0bb-2491b5be938a"},
		RouletteDisguise{"Palace Staff", "outfit_2018db77-aa8a-4bf9-9afb-56bdaa161156_0.jpg", "2018db77-aa8a-4bf9-9afb-56bdaa161156"},
		RouletteDisguise{"Security Guard", "outfit_992cc7b6-4ccf-4ae8-a467-e9b2aabaeeb5_0.jpg", "992cc7b6-4ccf-4ae8-a467-e9b2aabaeeb5"},
		RouletteDisguise{"Sheikh Salman Al-Ghazali", "outfit_a8ecd823-6e08-4cfe-a04d-816d387fcf0c_0.jpg", "a8ecd823-6e08-4cfe-a04d-816d387fcf0c"},
		RouletteDisguise{"Stylist", "outfit_96e32a7a-129a-4dd6-9b5b-3000a58f2a0f_0.jpg", "96e32a7a-129a-4dd6-9b5b-3000a58f2a0f"},
		RouletteDisguise{"Tech Crew", "outfit_69aac6db-461e-43af-89bc-2c27e50d430f_0.jpg", "69aac6db-461e-43af-89bc-2c27e50d430f"},
		RouletteDisguise{"Vampire Magician", "outfit_1fdc259e-b96a-47f2-bbd8-e86e78d6df70_0.jpg", "1fdc259e-b96a-47f2-bbd8-e86e78d6df70"},
	}},
	{eMission::SAPIENZA_WORLDOFTOMORROW, {
		RouletteDisguise{"Suit", "outfit_75759271-e236-4b33-8dd5-7e502c958d05_0.jpg", "75759271-e236-4b33-8dd5-7e502c958d05", true},
		RouletteDisguise{"Biolab Security", "outfit_91b37d1f-21ba-42b5-81fa-f4b6ce2ba691_0.jpg", "91b37d1f-21ba-42b5-81fa-f4b6ce2ba691"},
		RouletteDisguise{"Bodyguard", "outfit_bf9629e0-f25c-4e71-9561-4a99a93a43e8_0.jpg", "bf9629e0-f25c-4e71-9561-4a99a93a43e8"},
		RouletteDisguise{"Bohemian", "outfit_61b8f96e-4986-4f0a-ab95-dcdc69f51580_0.jpg", "61b8f96e-4986-4f0a-ab95-dcdc69f51580"},
		RouletteDisguise{"Butler", "outfit_81eed11b-eaa3-4fd3-97ba-e1e89dcca57e_0.jpg", "81eed11b-eaa3-4fd3-97ba-e1e89dcca57e"},
		RouletteDisguise{"Chef", "outfit_4c6816d8-4ae7-4161-a971-970055e64b34_0.jpg", "4c6816d8-4ae7-4161-a971-970055e64b34"},
		RouletteDisguise{"Church Staff", "outfit_5440b347-026f-402c-9cd4-3b4e142804ce_0.jpg", "5440b347-026f-402c-9cd4-3b4e142804ce"},
		RouletteDisguise{"Cyclist", "outfit_b58672d6-c235-4c08-a856-ef7caee777dd_0.jpg", "b58672d6-c235-4c08-a856-ef7caee777dd"},
		RouletteDisguise{"Delivery Man", "outfit_0d35cd13-e2ca-4375-9a67-763d1b776b48_0.jpg", "0d35cd13-e2ca-4375-9a67-763d1b776b48"},
		RouletteDisguise{"Dr. Oscar Lafayette", "outfit_fbf69e85-da3c-423b-bef9-da1b64f35f6b_0.jpg", "fbf69e85-da3c-423b-bef9-da1b64f35f6b"},
		RouletteDisguise{"Gardener", "outfit_d788ff58-8a7a-4a85-acdd-c0e5693525f0_0.jpg", "d788ff58-8a7a-4a85-acdd-c0e5693525f0"},
		RouletteDisguise{"Green Plumber", "outfit_844680e8-ae40-4fec-92b7-69c7619feb82_0.jpg", "844680e8-ae40-4fec-92b7-69c7619feb82"},
		RouletteDisguise{"Hazmat Suit", "outfit_92c30801-1782-44c5-88a0-d2020d6d4ff5_0.jpg", "92c30801-1782-44c5-88a0-d2020d6d4ff5"},
		RouletteDisguise{"Housekeeper", "outfit_a6c81663-684d-4506-abc0-65b35c4d8b63_0.jpg", "a6c81663-684d-4506-abc0-65b35c4d8b63"},
		RouletteDisguise{"Kitchen Assistant", "outfit_10601c6b-1f65-44ed-92a1-bf843f023d3f_0.jpg", "10601c6b-1f65-44ed-92a1-bf843f023d3f"},
		RouletteDisguise{"Lab Technician", "outfit_2894c92d-b780-412f-a48f-5c5ddf0dafc8_0.jpg", "2894c92d-b780-412f-a48f-5c5ddf0dafc8"},
		RouletteDisguise{"Mansion Security", "outfit_fd56a934-f402-4b52-bdca-8bbc737400ff_0.jpg", "fd56a934-f402-4b52-bdca-8bbc737400ff"},
		RouletteDisguise{"Mansion Staff", "outfit_5edcef70-c4bb-4856-9124-de3d39fa814a_0.jpg", "5edcef70-c4bb-4856-9124-de3d39fa814a"},
		RouletteDisguise{"Plague Doctor", "outfit_2bbc0c72-fc99-465d-9dec-c276f68ab982_0.jpg", "2bbc0c72-fc99-465d-9dec-c276f68ab982"},
		RouletteDisguise{"Priest", "outfit_98888ced-60f9-4f83-a93b-bf0ef2963341_0.jpg", "98888ced-60f9-4f83-a93b-bf0ef2963341"},
		RouletteDisguise{"Private Detective", "outfit_02a9f8e4-3ed1-4c29-9f73-88e0cf2d7b5e_0.jpg", "02a9f8e4-3ed1-4c29-9f73-88e0cf2d7b5e"},
		RouletteDisguise{"Roberto Vargas", "outfit_691311a2-c215-4250-a318-fb25fd08e265_0.jpg", "691311a2-c215-4250-a318-fb25fd08e265"},
		RouletteDisguise{"Red Plumber", "outfit_37352a6b-eb58-4458-a5d6-522dd0508baa_0.jpg", "37352a6b-eb58-4458-a5d6-522dd0508baa"},
		RouletteDisguise{"Store Clerk", "outfit_063fa6ef-9d58-4c3e-a4fd-70ce51a2f862_0.jpg", "063fa6ef-9d58-4c3e-a4fd-70ce51a2f862"},
		RouletteDisguise{"Street Performer", "outfit_40bc08b2-1d39-4321-ab14-76f300e4ea3a_0.jpg", "40bc08b2-1d39-4321-ab14-76f300e4ea3a"},
		RouletteDisguise{"Waiter", "outfit_430e8743-df1a-4e88-955f-793bff2e3a6a_0.jpg", "430e8743-df1a-4e88-955f-793bff2e3a6a"},
	}},
	{eMission::SAPIENZA_THEAUTHOR, {
		RouletteDisguise{"Suit", "outfit_75759271-e236-4b33-8dd5-7e502c958d05_0.jpg", "75759271-e236-4b33-8dd5-7e502c958d05", true},
		RouletteDisguise{"Bodyguard", "outfit_bf9629e0-f25c-4e71-9561-4a99a93a43e8_0.jpg", "bf9629e0-f25c-4e71-9561-4a99a93a43e8"},
		RouletteDisguise{"Bohemian", "outfit_61b8f96e-4986-4f0a-ab95-dcdc69f51580_0.jpg", "61b8f96e-4986-4f0a-ab95-dcdc69f51580"},
		RouletteDisguise{"Brother Akram", "outfit_a6034d86-0fa4-46ce-8deb-90acf2d1e485_0.jpg", "a6034d86-0fa4-46ce-8deb-90acf2d1e485"},
		RouletteDisguise{"Chef", "outfit_4c6816d8-4ae7-4161-a971-970055e64b34_0.jpg", "4c6816d8-4ae7-4161-a971-970055e64b34"},
		RouletteDisguise{"Church Staff", "outfit_5440b347-026f-402c-9cd4-3b4e142804ce_0.jpg", "5440b347-026f-402c-9cd4-3b4e142804ce"},
		RouletteDisguise{"Craig Black", "outfit_e710b6a4-b6f1-40a3-9389-cf297fff8d86_0.jpg", "e710b6a4-b6f1-40a3-9389-cf297fff8d86"},
		RouletteDisguise{"Gardener", "outfit_d788ff58-8a7a-4a85-acdd-c0e5693525f0_0.jpg", "d788ff58-8a7a-4a85-acdd-c0e5693525f0"},
		RouletteDisguise{"Green Plumber", "outfit_844680e8-ae40-4fec-92b7-69c7619feb82_0.jpg", "844680e8-ae40-4fec-92b7-69c7619feb82"},
		RouletteDisguise{"Housekeeper", "outfit_a6c81663-684d-4506-abc0-65b35c4d8b63_0.jpg", "a6c81663-684d-4506-abc0-65b35c4d8b63"},
		RouletteDisguise{"Kitchen Assistant", "outfit_10601c6b-1f65-44ed-92a1-bf843f023d3f_0.jpg", "10601c6b-1f65-44ed-92a1-bf843f023d3f"},
		RouletteDisguise{"Red Plumber", "outfit_37352a6b-eb58-4458-a5d6-522dd0508baa_0.jpg", "37352a6b-eb58-4458-a5d6-522dd0508baa"},
		RouletteDisguise{"Salvatore Bravuomo", "outfit_7766b295-35f3-45a8-b73d-10b222ed18ef_0.jpg", "7766b295-35f3-45a8-b73d-10b222ed18ef"},
		RouletteDisguise{"SFX Crew", "outfit_2fd2437d-a5eb-4bfd-bd2d-a4f240a8f0ce_0.jpg", "2fd2437d-a5eb-4bfd-bd2d-a4f240a8f0ce"},
		RouletteDisguise{"Super Fan", "outfit_95c7f899-e20c-4182-a228-28bd3d8a4ff4_0.jpg", "95c7f899-e20c-4182-a228-28bd3d8a4ff4"},
		RouletteDisguise{"Waiter", "outfit_430e8743-df1a-4e88-955f-793bff2e3a6a_0.jpg", "430e8743-df1a-4e88-955f-793bff2e3a6a"},
	}},
	{eMission::SAPIENZA_LANDSLIDE, {
		RouletteDisguise{"Suit", "outfit_75759271-e236-4b33-8dd5-7e502c958d05_0.jpg", "75759271-e236-4b33-8dd5-7e502c958d05", true},
		RouletteDisguise{"Bodyguard", "outfit_bf9629e0-f25c-4e71-9561-4a99a93a43e8_0.jpg", "bf9629e0-f25c-4e71-9561-4a99a93a43e8"},
		RouletteDisguise{"Bohemian", "outfit_61b8f96e-4986-4f0a-ab95-dcdc69f51580_0.jpg", "61b8f96e-4986-4f0a-ab95-dcdc69f51580"},
		RouletteDisguise{"Church Staff", "outfit_5440b347-026f-402c-9cd4-3b4e142804ce_0.jpg", "5440b347-026f-402c-9cd4-3b4e142804ce"},
		RouletteDisguise{"Gardener", "outfit_d788ff58-8a7a-4a85-acdd-c0e5693525f0_0.jpg", "d788ff58-8a7a-4a85-acdd-c0e5693525f0"},
		RouletteDisguise{"Green Plumber", "outfit_844680e8-ae40-4fec-92b7-69c7619feb82_0.jpg", "844680e8-ae40-4fec-92b7-69c7619feb82"},
		RouletteDisguise{"Kitchen Assistant", "outfit_10601c6b-1f65-44ed-92a1-bf843f023d3f_0.jpg", "10601c6b-1f65-44ed-92a1-bf843f023d3f"},
		RouletteDisguise{"Photographer", "outfit_b110cb05-0a38-4d77-b199-16e15a98b111_0.jpg", "b110cb05-0a38-4d77-b199-16e15a98b111"},
		RouletteDisguise{"Priest", "outfit_98888ced-60f9-4f83-a93b-bf0ef2963341_0.jpg", "98888ced-60f9-4f83-a93b-bf0ef2963341"},
		RouletteDisguise{"Red Plumber", "outfit_37352a6b-eb58-4458-a5d6-522dd0508baa_0.jpg", "37352a6b-eb58-4458-a5d6-522dd0508baa"},
		RouletteDisguise{"Salvatore Bravuomo", "outfit_7766b295-35f3-45a8-b73d-10b222ed18ef_0.jpg", "7766b295-35f3-45a8-b73d-10b222ed18ef"},
		RouletteDisguise{"Security", "outfit_94a0d283-bea4-468d-ad8f-ec2735008511_0.jpg", "94a0d283-bea4-468d-ad8f-ec2735008511"},
		RouletteDisguise{"Stage Crew", "outfit_6f9f7786-2044-4394-98b4-f79da0341e7f_0.jpg", "6f9f7786-2044-4394-98b4-f79da0341e7f"},
		RouletteDisguise{"Waiter", "outfit_430e8743-df1a-4e88-955f-793bff2e3a6a_0.jpg", "430e8743-df1a-4e88-955f-793bff2e3a6a"},
	}},
	{eMission::SAPIENZA_THEICON, {
		RouletteDisguise{"Suit", "outfit_cad53cde-4d4a-41cb-9259-87b544f718ad_0.jpg", "cad53cde-4d4a-41cb-9259-87b544f718ad", true},
		RouletteDisguise{"Kitchen Assistant", "outfit_10601c6b-1f65-44ed-92a1-bf843f023d3f_0.jpg", "10601c6b-1f65-44ed-92a1-bf843f023d3f"},
		RouletteDisguise{"Movie Crew", "outfit_79a767f7-5473-4380-b778-e93028e4fc2f_0.jpg", "79a767f7-5473-4380-b778-e93028e4fc2f"},
		RouletteDisguise{"Security", "outfit_94a0d283-bea4-468d-ad8f-ec2735008511_0.jpg", "94a0d283-bea4-468d-ad8f-ec2735008511"},
		RouletteDisguise{"SFX Crew", "outfit_2fd2437d-a5eb-4bfd-bd2d-a4f240a8f0ce_0.jpg", "2fd2437d-a5eb-4bfd-bd2d-a4f240a8f0ce"},
	}},
	{eMission::MARRAKESH_GILDEDCAGE, {
		RouletteDisguise{"Suit", "outfit_cb105877-743d-4a3b-bdad-28a022630306_0.jpg", "cb105877-743d-4a3b-bdad-28a022630306", true},
		RouletteDisguise{"Bodyguard", "outfit_97536da8-64a5-4675-a108-08ff7be41c1f_0.jpg", "97536da8-64a5-4675-a108-08ff7be41c1f"},
		RouletteDisguise{"Cameraman", "outfit_e0fc86fb-a852-4652-bb5c-b591f7bfeb29_0.jpg", "e0fc86fb-a852-4652-bb5c-b591f7bfeb29"},
		RouletteDisguise{"Consulate Intern", "outfit_65f23b45-f5ab-4ede-82ec-46e4de38c0e9_0.jpg", "65f23b45-f5ab-4ede-82ec-46e4de38c0e9"},
		RouletteDisguise{"Consulate Janitor", "outfit_1b87edd3-fb1e-4adf-9463-efcb380cbd6b_0.jpg", "1b87edd3-fb1e-4adf-9463-efcb380cbd6b"},
		RouletteDisguise{"Consulate Security", "outfit_e0fc86fc-a852-4652-bb5c-b591f7bfeb29_0.jpg", "e0fc86fc-a852-4652-bb5c-b591f7bfeb29"},
		RouletteDisguise{"Elite Soldier", "outfit_e5bb3c6b-2fcd-4e36-a30d-03955cb05088_0.jpg", "e5bb3c6b-2fcd-4e36-a30d-03955cb05088"},
		RouletteDisguise{"Food Vendor", "outfit_98b1c1f6-2634-4c29-b9e6-fa8d7633100a_0.jpg", "98b1c1f6-2634-4c29-b9e6-fa8d7633100a"},
		RouletteDisguise{"Fortune Teller", "outfit_dc4762e0-e58e-4336-a3c1-40646087267a_0.jpg", "dc4762e0-e58e-4336-a3c1-40646087267a"},
		RouletteDisguise{"Handyman", "outfit_eb48ed16-195a-4de1-bae6-d7e7ec92046c_0.jpg", "eb48ed16-195a-4de1-bae6-d7e7ec92046c"},
		RouletteDisguise{"Headmaster", "outfit_6136fa6a-3f1f-4606-9b87-fde9538966dc_0.jpg", "6136fa6a-3f1f-4606-9b87-fde9538966dc"},
		RouletteDisguise{"Local Printing Crew", "outfit_ff6668af-dde5-48c3-ac9a-f47b860122d0_0.jpg", "ff6668af-dde5-48c3-ac9a-f47b860122d0"},
		RouletteDisguise{"Masseur", "outfit_138eda40-501a-48b8-affc-928321566a4e_0.jpg", "138eda40-501a-48b8-affc-928321566a4e"},
		RouletteDisguise{"Military Officer", "outfit_65d860f4-998e-4f92-a1d7-9f40c04a2474_0.jpg", "65d860f4-998e-4f92-a1d7-9f40c04a2474"},
		RouletteDisguise{"Military Soldier", "outfit_955bca9e-bc91-46da-a4df-3dfc787c8aff_0.jpg", "955bca9e-bc91-46da-a4df-3dfc787c8aff"},
		RouletteDisguise{"Prisoner", "outfit_fdb4aade-4d5f-47e2-896f-fc1addf64d52_0.jpg", "fdb4aade-4d5f-47e2-896f-fc1addf64d52"},
		RouletteDisguise{"Shopkeeper", "outfit_ecf1b752-0cd6-4283-a1a5-743fc0249525_0.jpg", "ecf1b752-0cd6-4283-a1a5-743fc0249525"},
		RouletteDisguise{"Waiter", "outfit_6348cc33-665f-4470-80b4-a0ad836df702_0.jpg", "6348cc33-665f-4470-80b4-a0ad836df702"},
	}},
	{eMission::MARRAKESH_HOUSEBUILTONSAND, {
		RouletteDisguise{"Suit", "outfit_cb105877-743d-4a3b-bdad-28a022630306_0.jpg", "cb105877-743d-4a3b-bdad-28a022630306", true},
		RouletteDisguise{"Bodyguard", "outfit_97536da8-64a5-4675-a108-08ff7be41c1f_0.jpg", "97536da8-64a5-4675-a108-08ff7be41c1f"},
		RouletteDisguise{"Food Vendor", "outfit_98b1c1f6-2634-4c29-b9e6-fa8d7633100a_0.jpg", "98b1c1f6-2634-4c29-b9e6-fa8d7633100a"},
		RouletteDisguise{"Fortune Teller", "outfit_dc4762e0-e58e-4336-a3c1-40646087267a_0.jpg", "dc4762e0-e58e-4336-a3c1-40646087267a"},
		RouletteDisguise{"Handyman", "outfit_eb48ed16-195a-4de1-bae6-d7e7ec92046c_0.jpg", "eb48ed16-195a-4de1-bae6-d7e7ec92046c"},
		RouletteDisguise{"Military Soldier", "outfit_955bca9e-bc91-46da-a4df-3dfc787c8aff_0.jpg", "955bca9e-bc91-46da-a4df-3dfc787c8aff"},
		RouletteDisguise{"Shopkeeper", "outfit_ecf1b752-0cd6-4283-a1a5-743fc0249525_0.jpg", "ecf1b752-0cd6-4283-a1a5-743fc0249525"},
		RouletteDisguise{"Waiter", "outfit_6348cc33-665f-4470-80b4-a0ad836df702_0.jpg", "6348cc33-665f-4470-80b4-a0ad836df702"},
	}},
	{eMission::BANGKOK_CLUB27, {
		RouletteDisguise{"Suit", "outfit_c85c6e9c-0aee-43b8-b6e3-d70e76f1890e_0.jpg", "c85c6e9c-0aee-43b8-b6e3-d70e76f1890e", true},
		RouletteDisguise{"Abel de Silva", "outfit_f17c737e-9947-4fff-a443-65b381839d00_0.jpg", "f17c737e-9947-4fff-a443-65b381839d00"},
		RouletteDisguise{"Exterminator", "outfit_bf0bcc10-a335-4714-9dd2-69e7e96704b2_0.jpg", "bf0bcc10-a335-4714-9dd2-69e7e96704b2"},
		RouletteDisguise{"Groundskeeper", "outfit_7f6da010-1a96-4783-83e0-48c55a3e7103_0.jpg", "7f6da010-1a96-4783-83e0-48c55a3e7103"},
		RouletteDisguise{"Hotel Security", "outfit_07f3479a-29fc-45e0-bb80-e49a41c0c410_0.jpg", "07f3479a-29fc-45e0-bb80-e49a41c0c410"},
		RouletteDisguise{"Hotel Staff", "outfit_c96f9796-0194-47c6-836c-102473cc6c3c_0.jpg", "c96f9796-0194-47c6-836c-102473cc6c3c"},
		RouletteDisguise{"Jordan Cross' Bodyguard", "outfit_d01c8adc-735c-44f0-9105-b28d85062def_0.jpg", "d01c8adc-735c-44f0-9105-b28d85062def"},
		RouletteDisguise{"Kitchen Staff", "outfit_85971c2e-34ae-423f-9653-bc32c5f3e4f7_0.jpg", "85971c2e-34ae-423f-9653-bc32c5f3e4f7"},
		RouletteDisguise{"Morgan's Bodyguard", "outfit_4c8941af-541c-4bb9-a8e3-8b8e61b0a789_0.jpg", "4c8941af-541c-4bb9-a8e3-8b8e61b0a789"},
		RouletteDisguise{"Recording Crew", "outfit_ef704a8e-88b7-430a-a217-09bbeea7074f_0.jpg", "ef704a8e-88b7-430a-a217-09bbeea7074f"},
		RouletteDisguise{"Stalker", "outfit_c5bf909f-66a5-4f19-9aee-aeb953172e45_0.jpg", "c5bf909f-66a5-4f19-9aee-aeb953172e45"},
		RouletteDisguise{"Waiter", "outfit_57669117-fbf3-4630-80e3-53e5420a8f30_0.jpg", "57669117-fbf3-4630-80e3-53e5420a8f30"},
	}},
	{eMission::BANGKOK_THESOURCE, {
		RouletteDisguise{"Suit", "outfit_c85c6e9c-0aee-43b8-b6e3-d70e76f1890e_0.jpg", "c85c6e9c-0aee-43b8-b6e3-d70e76f1890e", true},
		RouletteDisguise{"Cult Bodyguard", "outfit_78fcc1c0-5612-4284-924f-c20d9e322c96_0.jpg", "78fcc1c0-5612-4284-924f-c20d9e322c96"},
		RouletteDisguise{"Cult Initiate", "outfit_54c5dce7-cfe4-43f9-8cee-8204e38c608d_0.jpg", "54c5dce7-cfe4-43f9-8cee-8204e38c608d"},
		RouletteDisguise{"Exterminator", "outfit_bf0bcc10-a335-4714-9dd2-69e7e96704b2_0.jpg", "bf0bcc10-a335-4714-9dd2-69e7e96704b2"},
		RouletteDisguise{"Groundskeeper", "outfit_7f6da010-1a96-4783-83e0-48c55a3e7103_0.jpg", "7f6da010-1a96-4783-83e0-48c55a3e7103"},
		RouletteDisguise{"Hotel Security", "outfit_07f3479a-29fc-45e0-bb80-e49a41c0c410_0.jpg", "07f3479a-29fc-45e0-bb80-e49a41c0c410"},
		RouletteDisguise{"Hotel Staff", "outfit_c96f9796-0194-47c6-836c-102473cc6c3c_0.jpg", "c96f9796-0194-47c6-836c-102473cc6c3c"},
		RouletteDisguise{"Jordan Cross' Bodyguard", "outfit_d01c8adc-735c-44f0-9105-b28d85062def_0.jpg", "d01c8adc-735c-44f0-9105-b28d85062def"},
		RouletteDisguise{"Kitchen Staff", "outfit_85971c2e-34ae-423f-9653-bc32c5f3e4f7_0.jpg", "85971c2e-34ae-423f-9653-bc32c5f3e4f7"},
		RouletteDisguise{"Militia Soldier", "outfit_3dd1467a-72d2-4590-93d8-10807c9f1645_0.jpg", "3dd1467a-72d2-4590-93d8-10807c9f1645"},
		RouletteDisguise{"Recording Crew", "outfit_ef704a8e-88b7-430a-a217-09bbeea7074f_0.jpg", "ef704a8e-88b7-430a-a217-09bbeea7074f"},
		RouletteDisguise{"Waiter", "outfit_57669117-fbf3-4630-80e3-53e5420a8f30_0.jpg", "57669117-fbf3-4630-80e3-53e5420a8f30"},
	}},
	{eMission::COLORADO_FREEDOMFIGHTERS, {
		RouletteDisguise{"Suit", "outfit_dd9792ec-4a1d-4c29-a928-a556fc0b6692_0.jpg", "dd9792ec-4a1d-4c29-a928-a556fc0b6692", true},
		RouletteDisguise{"Explosives Specialist", "outfit_59c4f7db-f065-4fac-bc6c-5c7ac3758eed_0.jpg", "59c4f7db-f065-4fac-bc6c-5c7ac3758eed"},
		RouletteDisguise{"Hacker", "outfit_aab7f28d-84d9-47d1-be52-d142f5970086_0.jpg", "aab7f28d-84d9-47d1-be52-d142f5970086"},
		RouletteDisguise{"Militia Cook", "outfit_dac13714-a012-47cf-b76a-150cfc4cece5_0.jpg", "dac13714-a012-47cf-b76a-150cfc4cece5"},
		RouletteDisguise{"Militia Elite", "outfit_d878b5ee-90cd-4222-8503-1e9ae193d556_0.jpg", "d878b5ee-90cd-4222-8503-1e9ae193d556"},
		RouletteDisguise{"Militia Soldier", "outfit_3dd1467a-72d2-4590-93d8-10807c9f1645_0.jpg", "3dd1467a-72d2-4590-93d8-10807c9f1645"},
		RouletteDisguise{"Militia Spec Ops", "outfit_ed1a4bf9-0641-4e70-9af1-d6e68cbb84d6_0.jpg", "ed1a4bf9-0641-4e70-9af1-d6e68cbb84d6"},
		RouletteDisguise{"Militia Technician", "outfit_143c62fc-4bf6-474a-9542-1e81bf93a044_0.jpg", "143c62fc-4bf6-474a-9542-1e81bf93a044"},
		RouletteDisguise{"Point Man", "outfit_338021a8-0c61-4732-a991-559e25e49efe_0.jpg", "338021a8-0c61-4732-a991-559e25e49efe"},
		RouletteDisguise{"Scarecrow", "outfit_fd5d2b9d-dcef-4596-a98a-5266a148c40c_0.jpg", "fd5d2b9d-dcef-4596-a98a-5266a148c40c"},
	}},
	{eMission::HOKKAIDO_SITUSINVERSUS, {
		RouletteDisguise{"Suit", "outfit_1c3964e1-75c6-4adb-8cbb-ebd0a830b839_0.jpg", "1c3964e1-75c6-4adb-8cbb-ebd0a830b839", true},
		RouletteDisguise{"Baseball Player", "outfit_5946924c-958d-48f4-ada3-86beb58aa778_0.jpg", "5946924c-958d-48f4-ada3-86beb58aa778"},
		RouletteDisguise{"Bodyguard", "outfit_5270225d-797a-43f8-8435-078ae0d92249_0.jpg", "5270225d-797a-43f8-8435-078ae0d92249"},
		RouletteDisguise{"Chef", "outfit_d6bbbe57-8cc8-45ed-b1cb-d1f9477c4b61_0.jpg", "d6bbbe57-8cc8-45ed-b1cb-d1f9477c4b61"},
		RouletteDisguise{"Chief Surgeon", "outfit_b8deb948-a0a9-4dcb-9df4-1c2ecd29765f_0.jpg", "b8deb948-a0a9-4dcb-9df4-1c2ecd29765f"},
		RouletteDisguise{"Doctor", "outfit_a8191fb6-9a6d-4145-8baf-d786e6f392b7_0.jpg", "a8191fb6-9a6d-4145-8baf-d786e6f392b7"},
		RouletteDisguise{"Handyman", "outfit_d9e0fbe7-ff74-4030-bed6-5a33a01acead_0.jpg", "d9e0fbe7-ff74-4030-bed6-5a33a01acead"},
		RouletteDisguise{"Helicopter Pilot", "outfit_b8dbb7b6-fef9-4782-923f-ddebc573625e_0.jpg", "b8dbb7b6-fef9-4782-923f-ddebc573625e"},
		RouletteDisguise{"Hospital Director", "outfit_f6f53c39-17f9-48cf-9594-7a696b036d61_0.jpg", "f6f53c39-17f9-48cf-9594-7a696b036d61"},
		RouletteDisguise{"Morgue Doctor", "outfit_3d4424a3-23f9-4cfe-b225-2e06c17d780b_0.jpg", "3d4424a3-23f9-4cfe-b225-2e06c17d780b"},
		RouletteDisguise{"Motorcyclist", "outfit_8e01f48f-ef06-448c-9d22-5d58c4414968_0.jpg", "8e01f48f-ef06-448c-9d22-5d58c4414968"},
		RouletteDisguise{"Ninja", "outfit_06456d4d-da36-4008-bea5-c0b985a565f5_0.jpg", "06456d4d-da36-4008-bea5-c0b985a565f5"},
		RouletteDisguise{"Patient", "outfit_c98a6467-5dd9-4041-8bff-119445750d4d_0.jpg", "c98a6467-5dd9-4041-8bff-119445750d4d"},
		RouletteDisguise{"Resort Security", "outfit_25406dac-d206-48c7-a6df-dffb887c9227_0.jpg", "25406dac-d206-48c7-a6df-dffb887c9227"},
		RouletteDisguise{"Resort Staff", "outfit_52992428-8884-48db-9764-e486d17d4804_0.jpg", "52992428-8884-48db-9764-e486d17d4804"},
		RouletteDisguise{"Surgeon", "outfit_6a25f81d-cf2e-4e47-9b15-0f712a3f71d9_0.jpg", "6a25f81d-cf2e-4e47-9b15-0f712a3f71d9"},
		RouletteDisguise{"VIP Patient (Amos Dexter)", "outfit_427bac46-50b4-4470-9b0e-478efcd37793_0.jpg", "427bac46-50b4-4470-9b0e-478efcd37793"},
		RouletteDisguise{"VIP Patient (Jason Portman)", "outfit_b00380d9-3f84-4484-8bd6-39c0872da414_0.jpg", "b00380d9-3f84-4484-8bd6-39c0872da414"},
		RouletteDisguise{"Yoga Instructor", "outfit_f4ea7065-d32b-4a97-baf9-98072a9c8128_0.jpg", "f4ea7065-d32b-4a97-baf9-98072a9c8128"},
	}},
	{eMission::HOKKAIDO_SNOWFESTIVAL, {
		RouletteDisguise{"Suit", "outfit_1c3964e1-75c6-4adb-8cbb-ebd0a830b839_0.jpg", "1c3964e1-75c6-4adb-8cbb-ebd0a830b839", true},
		RouletteDisguise{"Baseball Player", "outfit_5946924c-958d-48f4-ada3-86beb58aa778_0.jpg", "5946924c-958d-48f4-ada3-86beb58aa778"},
		RouletteDisguise{"Bodyguard", "outfit_5270225d-797a-43f8-8435-078ae0d92249_0.jpg", "5270225d-797a-43f8-8435-078ae0d92249"},
		RouletteDisguise{"Chef", "outfit_d6bbbe57-8cc8-45ed-b1cb-d1f9477c4b61_0.jpg", "d6bbbe57-8cc8-45ed-b1cb-d1f9477c4b61"},
		RouletteDisguise{"Chief Surgeon", "outfit_b8deb948-a0a9-4dcb-9df4-1c2ecd29765f_0.jpg", "b8deb948-a0a9-4dcb-9df4-1c2ecd29765f"},
		RouletteDisguise{"Doctor", "outfit_a8191fb6-9a6d-4145-8baf-d786e6f392b7_0.jpg", "a8191fb6-9a6d-4145-8baf-d786e6f392b7"},
		RouletteDisguise{"Handyman", "outfit_d9e0fbe7-ff74-4030-bed6-5a33a01acead_0.jpg", "d9e0fbe7-ff74-4030-bed6-5a33a01acead"},
		RouletteDisguise{"Helicopter Pilot", "outfit_b8dbb7b6-fef9-4782-923f-ddebc573625e_0.jpg", "b8dbb7b6-fef9-4782-923f-ddebc573625e"},
		RouletteDisguise{"Hospital Director", "outfit_f6f53c39-17f9-48cf-9594-7a696b036d61_0.jpg", "f6f53c39-17f9-48cf-9594-7a696b036d61"},
		RouletteDisguise{"Morgue Doctor", "outfit_3d4424a3-23f9-4cfe-b225-2e06c17d780b_0.jpg", "3d4424a3-23f9-4cfe-b225-2e06c17d780b"},
		RouletteDisguise{"Motorcyclist", "outfit_8e01f48f-ef06-448c-9d22-5d58c4414968_0.jpg", "8e01f48f-ef06-448c-9d22-5d58c4414968"},
		RouletteDisguise{"Ninja", "outfit_06456d4d-da36-4008-bea5-c0b985a565f5_0.jpg", "06456d4d-da36-4008-bea5-c0b985a565f5"},
		RouletteDisguise{"Patient", "outfit_c98a6467-5dd9-4041-8bff-119445750d4d_0.jpg", "c98a6467-5dd9-4041-8bff-119445750d4d"},
		RouletteDisguise{"Resort Security", "outfit_25406dac-d206-48c7-a6df-dffb887c9227_0.jpg", "25406dac-d206-48c7-a6df-dffb887c9227"},
		RouletteDisguise{"Resort Staff", "outfit_52992428-8884-48db-9764-e486d17d4804_0.jpg", "52992428-8884-48db-9764-e486d17d4804"},
		RouletteDisguise{"Surgeon", "outfit_6a25f81d-cf2e-4e47-9b15-0f712a3f71d9_0.jpg", "6a25f81d-cf2e-4e47-9b15-0f712a3f71d9"},
		RouletteDisguise{"VIP Patient (Amos Dexter)", "outfit_427bac46-50b4-4470-9b0e-478efcd37793_0.jpg", "427bac46-50b4-4470-9b0e-478efcd37793"},
		RouletteDisguise{"VIP Patient (Jason Portman)", "outfit_b00380d9-3f84-4484-8bd6-39c0872da414_0.jpg", "b00380d9-3f84-4484-8bd6-39c0872da414"},
		RouletteDisguise{"Yoga Instructor", "outfit_f4ea7065-d32b-4a97-baf9-98072a9c8128_0.jpg", "f4ea7065-d32b-4a97-baf9-98072a9c8128"},
	}},
	{eMission::HOKKAIDO_PATIENTZERO, {
		RouletteDisguise{"Suit", "outfit_250112ba-e39d-473c-99cd-5fc429c5fff5_0.jpg", "250112ba-e39d-473c-99cd-5fc429c5fff5", true},
		RouletteDisguise{"Bio Suit", "outfit_e8ef431d-62b2-4d0a-a766-750c0bc6e39e_0.jpg", "e8ef431d-62b2-4d0a-a766-750c0bc6e39e"},
		RouletteDisguise{"Bodyguard", "outfit_5270225d-797a-43f8-8435-078ae0d92249_0.jpg", "5270225d-797a-43f8-8435-078ae0d92249"},
		RouletteDisguise{"Chef", "outfit_d6bbbe57-8cc8-45ed-b1cb-d1f9477c4b61_0.jpg", "d6bbbe57-8cc8-45ed-b1cb-d1f9477c4b61"},
		RouletteDisguise{"Doctor", "outfit_a8191fb6-9a6d-4145-8baf-d786e6f392b7_0.jpg", "a8191fb6-9a6d-4145-8baf-d786e6f392b7"},
		RouletteDisguise{"Handyman", "outfit_d9e0fbe7-ff74-4030-bed6-5a33a01acead_0.jpg", "d9e0fbe7-ff74-4030-bed6-5a33a01acead"},
		RouletteDisguise{"Head Researcher", "outfit_ff534fe6-065e-4062-a32c-8bdf223efd98_0.jpg", "ff534fe6-065e-4062-a32c-8bdf223efd98"},
		RouletteDisguise{"Helicopter Pilot", "outfit_b8dbb7b6-fef9-4782-923f-ddebc573625e_0.jpg", "b8dbb7b6-fef9-4782-923f-ddebc573625e"},
		RouletteDisguise{"Hospital Director", "outfit_f6f53c39-17f9-48cf-9594-7a696b036d61_0.jpg", "f6f53c39-17f9-48cf-9594-7a696b036d61"},
		RouletteDisguise{"Morgue Doctor", "outfit_3d4424a3-23f9-4cfe-b225-2e06c17d780b_0.jpg", "3d4424a3-23f9-4cfe-b225-2e06c17d780b"},
		RouletteDisguise{"Motorcyclist", "outfit_8e01f48f-ef06-448c-9d22-5d58c4414968_0.jpg", "8e01f48f-ef06-448c-9d22-5d58c4414968"},
		RouletteDisguise{"Patient", "outfit_c98a6467-5dd9-4041-8bff-119445750d4d_0.jpg", "c98a6467-5dd9-4041-8bff-119445750d4d"},
		RouletteDisguise{"Resort Security", "outfit_25406dac-d206-48c7-a6df-dffb887c9227_0.jpg", "25406dac-d206-48c7-a6df-dffb887c9227"},
		RouletteDisguise{"Resort Staff", "outfit_52992428-8884-48db-9764-e486d17d4804_0.jpg", "52992428-8884-48db-9764-e486d17d4804"},
		RouletteDisguise{"Surgeon", "outfit_6a25f81d-cf2e-4e47-9b15-0f712a3f71d9_0.jpg", "6a25f81d-cf2e-4e47-9b15-0f712a3f71d9"},
		RouletteDisguise{"VIP Patient (Amos Dexter)", "outfit_427bac46-50b4-4470-9b0e-478efcd37793_0.jpg", "427bac46-50b4-4470-9b0e-478efcd37793"},
		RouletteDisguise{"Yoga Instructor", "outfit_f4ea7065-d32b-4a97-baf9-98072a9c8128_0.jpg", "f4ea7065-d32b-4a97-baf9-98072a9c8128"},
	}},
	{eMission::HAWKESBAY_NIGHTCALL, {
		RouletteDisguise{"Suit", "outfit_08022e2c-4954-4b63-b632-3ac50d018292_0.jpg", "08022e2c-4954-4b63-b632-3ac50d018292", true},
		RouletteDisguise{"Bodyguard", "outfit_d4dd18b3-2dbe-4ad2-8bfa-db5fdb9a6568_0.jpg", "d4dd18b3-2dbe-4ad2-8bfa-db5fdb9a6568"},
	}},
	{eMission::MIAMI_FINISHLINE, {
		RouletteDisguise{"Suit", "outfit_1ad1ec9b-1e96-4fac-b0e6-8817a46da9db_0.jpg", "1ad1ec9b-1e96-4fac-b0e6-8817a46da9db", true},
		RouletteDisguise{"Aeon Driver", "outfit_6fca97a0-cb80-4bd7-9a8f-106fa20a5d04_0.jpg", "6fca97a0-cb80-4bd7-9a8f-106fa20a5d04"},
		RouletteDisguise{"Aeon Mechanic", "outfit_da951ccc-1d8b-4d84-b30a-72e74ac2a312_0.jpg", "da951ccc-1d8b-4d84-b30a-72e74ac2a312"},
		RouletteDisguise{"Blue Seed Driver", "outfit_46dedef3-bbcf-438c-af2b-c97dd853aac1_0.jpg", "46dedef3-bbcf-438c-af2b-c97dd853aac1"},
		RouletteDisguise{"Crashed Kronstadt Driver", "outfit_8980597c-1559-46a7-ba8d-bc5d95d5936a_0.jpg", "8980597c-1559-46a7-ba8d-bc5d95d5936a"},
		RouletteDisguise{"Event Crew", "outfit_d86e4379-4aad-42cf-a7cf-38d0fa7e727b_0.jpg", "d86e4379-4aad-42cf-a7cf-38d0fa7e727b"},
		RouletteDisguise{"Event Security", "outfit_dc5b1ccd-0997-4834-93a0-db7543e729cc_0.jpg", "dc5b1ccd-0997-4834-93a0-db7543e729cc"},
		RouletteDisguise{"Florida Man", "outfit_f8ef3523-2500-410c-98fb-b6926a832df4_0.jpg", "f8ef3523-2500-410c-98fb-b6926a832df4"},
		RouletteDisguise{"Food Vendor", "outfit_e9ed2969-146a-472d-8e87-39c77bd1757d_0.jpg", "e9ed2969-146a-472d-8e87-39c77bd1757d"},
		RouletteDisguise{"Journalist", "outfit_723e73f3-9fa4-40d8-bb11-b66184c9a795_0.jpg", "723e73f3-9fa4-40d8-bb11-b66184c9a795"},
		RouletteDisguise{"Kitchen Staff", "outfit_2a5a3dba-bafd-4a1f-8bbf-204668b32fe1_0.jpg", "2a5a3dba-bafd-4a1f-8bbf-204668b32fe1"},
		RouletteDisguise{"Kowoon Driver", "outfit_12181b2f-8c74-4761-8d78-3bedfbf9281d_0.jpg", "12181b2f-8c74-4761-8d78-3bedfbf9281d"},
		RouletteDisguise{"Kowoon Mechanic", "outfit_fc829fad-5afc-4236-8662-65ab8698ef44_0.jpg", "fc829fad-5afc-4236-8662-65ab8698ef44"},
		RouletteDisguise{"Kronstadt Engineer", "outfit_9bd53a5a-a152-488f-be20-7394b083d99a_0.jpg", "9bd53a5a-a152-488f-be20-7394b083d99a"},
		RouletteDisguise{"Kronstadt Mechanic", "outfit_085e639e-2cf4-4e9b-bd9b-f9fd5b899676_0.jpg", "085e639e-2cf4-4e9b-bd9b-f9fd5b899676"},
		RouletteDisguise{"Kronstadt Researcher", "outfit_ade47a03-a3ec-4d78-aefa-6057abceea28_0.jpg", "ade47a03-a3ec-4d78-aefa-6057abceea28"},
		RouletteDisguise{"Kronstadt Security", "outfit_d37ae121-69b4-4a9c-ab57-972063505e2f_0.jpg", "d37ae121-69b4-4a9c-ab57-972063505e2f"},
		RouletteDisguise{"Mascot", "outfit_124d145e-469e-485d-a628-ced82ddf1b75_0.jpg", "124d145e-469e-485d-a628-ced82ddf1b75"},
		RouletteDisguise{"Medic", "outfit_b838226d-5fbf-4b5d-8e5f-98e5c8ddc1f2_0.jpg", "b838226d-5fbf-4b5d-8e5f-98e5c8ddc1f2"},
		RouletteDisguise{"Moses Lee", "outfit_a45555d8-d68c-4cd7-8006-7d7f61b36c72_0.jpg", "a45555d8-d68c-4cd7-8006-7d7f61b36c72"},
		RouletteDisguise{"Pale Rider", "outfit_9df94442-ed1b-436c-942f-3195b1ef7e0e_0.jpg", "9df94442-ed1b-436c-942f-3195b1ef7e0e"},
		RouletteDisguise{"Race Coordinator", "outfit_be9dd1c4-af52-43db-b8f7-37c3c054c90f_0.jpg", "be9dd1c4-af52-43db-b8f7-37c3c054c90f"},
		RouletteDisguise{"Race Marshal", "outfit_a166a37e-a3f8-42d2-99d6-e0dd2cf5c090_0.jpg", "a166a37e-a3f8-42d2-99d6-e0dd2cf5c090"},
		RouletteDisguise{"Sheik", "outfit_a68b2030-d52f-4e52-907f-8657b867dd50_0.jpg", "a68b2030-d52f-4e52-907f-8657b867dd50"},
		RouletteDisguise{"Sotteraneo Mechanic", "outfit_981b7a3b-5548-4a6f-b568-f767784e6d91_0.jpg", "981b7a3b-5548-4a6f-b568-f767784e6d91"},
		RouletteDisguise{"Street Musician", "outfit_2018be7c-5f79-497e-a7e1-0e64f31e71f5_0.jpg", "2018be7c-5f79-497e-a7e1-0e64f31e71f5"},
		RouletteDisguise{"Ted Mendez", "outfit_d7d8d5e8-070c-4d6c-9a67-f1165a7bb29d_0.jpg", "d7d8d5e8-070c-4d6c-9a67-f1165a7bb29d"},
		RouletteDisguise{"Thwack Driver", "outfit_bac37ceb-6f72-406f-bfa3-e49413436525_0.jpg", "bac37ceb-6f72-406f-bfa3-e49413436525"},
		RouletteDisguise{"Thwack Mechanic", "outfit_4376850a-3a37-4ad3-a886-168d0e24aa20_0.jpg", "4376850a-3a37-4ad3-a886-168d0e24aa20"},
		RouletteDisguise{"Waiter", "outfit_c7bbd142-7873-4a91-98c8-76a6900bea60_0.jpg", "c7bbd142-7873-4a91-98c8-76a6900bea60"},
	}},
	{eMission::MIAMI_ASILVERTONGUE, {
		RouletteDisguise{"Suit", "outfit_1ad1ec9b-1e96-4fac-b0e6-8817a46da9db_0.jpg", "1ad1ec9b-1e96-4fac-b0e6-8817a46da9db", true},
		RouletteDisguise{"Aeon Driver", "outfit_6fca97a0-cb80-4bd7-9a8f-106fa20a5d04_0.jpg", "6fca97a0-cb80-4bd7-9a8f-106fa20a5d04"},
		RouletteDisguise{"Aeon Mechanic", "outfit_da951ccc-1d8b-4d84-b30a-72e74ac2a312_0.jpg", "da951ccc-1d8b-4d84-b30a-72e74ac2a312"},
		RouletteDisguise{"Blue Seed Driver", "outfit_46dedef3-bbcf-438c-af2b-c97dd853aac1_0.jpg", "46dedef3-bbcf-438c-af2b-c97dd853aac1"},
		RouletteDisguise{"Crashed Kronstadt Driver", "outfit_8980597c-1559-46a7-ba8d-bc5d95d5936a_0.jpg", "8980597c-1559-46a7-ba8d-bc5d95d5936a"},
		RouletteDisguise{"Event Crew", "outfit_d86e4379-4aad-42cf-a7cf-38d0fa7e727b_0.jpg", "d86e4379-4aad-42cf-a7cf-38d0fa7e727b"},
		RouletteDisguise{"Event Security", "outfit_dc5b1ccd-0997-4834-93a0-db7543e729cc_0.jpg", "dc5b1ccd-0997-4834-93a0-db7543e729cc"},
		RouletteDisguise{"Florida Man", "outfit_f8ef3523-2500-410c-98fb-b6926a832df4_0.jpg", "f8ef3523-2500-410c-98fb-b6926a832df4"},
		RouletteDisguise{"Food Vendor", "outfit_e9ed2969-146a-472d-8e87-39c77bd1757d_0.jpg", "e9ed2969-146a-472d-8e87-39c77bd1757d"},
		RouletteDisguise{"Journalist", "outfit_723e73f3-9fa4-40d8-bb11-b66184c9a795_0.jpg", "723e73f3-9fa4-40d8-bb11-b66184c9a795"},
		RouletteDisguise{"Kitchen Staff", "outfit_2a5a3dba-bafd-4a1f-8bbf-204668b32fe1_0.jpg", "2a5a3dba-bafd-4a1f-8bbf-204668b32fe1"},
		RouletteDisguise{"Kowoon Driver", "outfit_12181b2f-8c74-4761-8d78-3bedfbf9281d_0.jpg", "12181b2f-8c74-4761-8d78-3bedfbf9281d"},
		RouletteDisguise{"Kowoon Mechanic", "outfit_fc829fad-5afc-4236-8662-65ab8698ef44_0.jpg", "fc829fad-5afc-4236-8662-65ab8698ef44"},
		RouletteDisguise{"Kronstadt Engineer", "outfit_9bd53a5a-a152-488f-be20-7394b083d99a_0.jpg", "9bd53a5a-a152-488f-be20-7394b083d99a"},
		RouletteDisguise{"Kronstadt Mechanic", "outfit_085e639e-2cf4-4e9b-bd9b-f9fd5b899676_0.jpg", "085e639e-2cf4-4e9b-bd9b-f9fd5b899676"},
		RouletteDisguise{"Kronstadt Researcher", "outfit_ade47a03-a3ec-4d78-aefa-6057abceea28_0.jpg", "ade47a03-a3ec-4d78-aefa-6057abceea28"},
		RouletteDisguise{"Kronstadt Security", "outfit_d37ae121-69b4-4a9c-ab57-972063505e2f_0.jpg", "d37ae121-69b4-4a9c-ab57-972063505e2f"},
		RouletteDisguise{"Mascot", "outfit_5fc2ed75-ab22-4c61-af6c-bdd07b1a55a6_0.jpg", "5fc2ed75-ab22-4c61-af6c-bdd07b1a55a6"},
		RouletteDisguise{"Medic", "outfit_b838226d-5fbf-4b5d-8e5f-98e5c8ddc1f2_0.jpg", "b838226d-5fbf-4b5d-8e5f-98e5c8ddc1f2"},
		RouletteDisguise{"Moses Lee", "outfit_a45555d8-d68c-4cd7-8006-7d7f61b36c72_0.jpg", "a45555d8-d68c-4cd7-8006-7d7f61b36c72"},
		RouletteDisguise{"Pale Rider", "outfit_9df94442-ed1b-436c-942f-3195b1ef7e0e_0.jpg", "9df94442-ed1b-436c-942f-3195b1ef7e0e"},
		RouletteDisguise{"Race Coordinator", "outfit_be9dd1c4-af52-43db-b8f7-37c3c054c90f_0.jpg", "be9dd1c4-af52-43db-b8f7-37c3c054c90f"},
		RouletteDisguise{"Race Marshal", "outfit_a166a37e-a3f8-42d2-99d6-e0dd2cf5c090_0.jpg", "a166a37e-a3f8-42d2-99d6-e0dd2cf5c090"},
		RouletteDisguise{"Sheik", "outfit_a68b2030-d52f-4e52-907f-8657b867dd50_0.jpg", "a68b2030-d52f-4e52-907f-8657b867dd50"},
		RouletteDisguise{"Sotteraneo Mechanic", "outfit_981b7a3b-5548-4a6f-b568-f767784e6d91_0.jpg", "981b7a3b-5548-4a6f-b568-f767784e6d91"},
		RouletteDisguise{"Street Musician", "outfit_2018be7c-5f79-497e-a7e1-0e64f31e71f5_0.jpg", "2018be7c-5f79-497e-a7e1-0e64f31e71f5"},
		RouletteDisguise{"Ted Mendez", "outfit_d7d8d5e8-070c-4d6c-9a67-f1165a7bb29d_0.jpg", "d7d8d5e8-070c-4d6c-9a67-f1165a7bb29d"},
		RouletteDisguise{"Thwack Driver", "outfit_bac37ceb-6f72-406f-bfa3-e49413436525_0.jpg", "bac37ceb-6f72-406f-bfa3-e49413436525"},
		RouletteDisguise{"Thwack Mechanic", "outfit_4376850a-3a37-4ad3-a886-168d0e24aa20_0.jpg", "4376850a-3a37-4ad3-a886-168d0e24aa20"},
		RouletteDisguise{"Waiter", "outfit_c7bbd142-7873-4a91-98c8-76a6900bea60_0.jpg", "c7bbd142-7873-4a91-98c8-76a6900bea60"},
	}},
	{eMission::SANTAFORTUNA_THREEHEADEDSERPENT, {
		RouletteDisguise{"Suit", "outfit_ac71f90e-a67d-4898-b2f0-43b605332dc8_0.jpg", "ac71f90e-a67d-4898-b2f0-43b605332dc8", true},
		RouletteDisguise{"Band Member", "outfit_fc0491ac-8592-486d-9dc2-b39af13cf6e3_0.jpg", "fc0491ac-8592-486d-9dc2-b39af13cf6e3"},
		RouletteDisguise{"Barman", "outfit_cfacf46a-eb59-4a16-a221-a690defd05a3_0.jpg", "cfacf46a-eb59-4a16-a221-a690defd05a3"},
		RouletteDisguise{"Chef", "outfit_d0fe70cb-c30b-41a3-8d1c-5503e898f686_0.jpg", "d0fe70cb-c30b-41a3-8d1c-5503e898f686"},
		RouletteDisguise{"Coca Field Guard", "outfit_56a589d8-bf28-489f-a30c-2ecea87177f5_0.jpg", "56a589d8-bf28-489f-a30c-2ecea87177f5"},
		RouletteDisguise{"Coca Field Worker", "outfit_11f2849d-87c5-4806-a25e-1a9dad85981d_0.jpg", "11f2849d-87c5-4806-a25e-1a9dad85981d"},
		RouletteDisguise{"Construction Worker", "outfit_57342129-03a9-47a4-a509-cc0656e0a76a_0.jpg", "57342129-03a9-47a4-a509-cc0656e0a76a"},
		RouletteDisguise{"Drug Lab Worker", "outfit_a741cd97-135e-465e-89c3-4fa52a2bbf9d_0.jpg", "a741cd97-135e-465e-89c3-4fa52a2bbf9d"},
		RouletteDisguise{"Elite Guard", "outfit_3e3be8e1-1fe4-4b3a-959c-9e52f595b0c4_0.jpg", "3e3be8e1-1fe4-4b3a-959c-9e52f595b0c4"},
		RouletteDisguise{"Gardener", "outfit_886c3b26-b81f-4731-8080-524f2d6da5dd_0.jpg", "886c3b26-b81f-4731-8080-524f2d6da5dd"},
		RouletteDisguise{"Hippie", "outfit_177410f1-4fd7-4ef2-8ed7-2119bcba3661_0.jpg", "177410f1-4fd7-4ef2-8ed7-2119bcba3661"},
		RouletteDisguise{"Hippo Whisperer", "outfit_4a145036-e4cc-4798-a795-42bcee511524_0.jpg", "4a145036-e4cc-4798-a795-42bcee511524"},
		RouletteDisguise{"Mansion Guard", "outfit_f0d1dfab-ac73-4fe9-bbac-a5587fbc0f91_0.jpg", "f0d1dfab-ac73-4fe9-bbac-a5587fbc0f91"},
		RouletteDisguise{"Mansion Staff", "outfit_2dec1e42-0093-462a-83aa-c0f4d82ac224_0.jpg", "2dec1e42-0093-462a-83aa-c0f4d82ac224"},
		RouletteDisguise{"Shaman", "outfit_30005896-2b39-49c0-bb04-3475d4a12ae6_0.jpg", "30005896-2b39-49c0-bb04-3475d4a12ae6"},
		RouletteDisguise{"Street Soldier", "outfit_ab5a46a2-6e53-4b15-a48e-c336df1ef5ff_0.jpg", "ab5a46a2-6e53-4b15-a48e-c336df1ef5ff"},
		RouletteDisguise{"Submarine Crew", "outfit_f86848e7-ca8c-48e0-94d1-2d925e96a3e7_0.jpg", "f86848e7-ca8c-48e0-94d1-2d925e96a3e7"},
		RouletteDisguise{"Submarine Engineer", "outfit_dfaa8260-20af-4112-b1ca-88a98481127b_0.jpg", "dfaa8260-20af-4112-b1ca-88a98481127b"},
		RouletteDisguise{"Tattoo Artist (P-Power)", "outfit_bbc1dd7c-a0ef-41eb-9bef-a4c38fded3fa_0.jpg", "bbc1dd7c-a0ef-41eb-9bef-a4c38fded3fa"},
	}},
	{eMission::SANTAFORTUNA_EMBRACEOFTHESERPENT, {
		RouletteDisguise{"Suit", "outfit_ac71f90e-a67d-4898-b2f0-43b605332dc8_0.jpg", "ac71f90e-a67d-4898-b2f0-43b605332dc8", true},
		RouletteDisguise{"Band Member", "outfit_fc0491ac-8592-486d-9dc2-b39af13cf6e3_0.jpg", "fc0491ac-8592-486d-9dc2-b39af13cf6e3"},
		RouletteDisguise{"Barman", "outfit_cfacf46a-eb59-4a16-a221-a690defd05a3_0.jpg", "cfacf46a-eb59-4a16-a221-a690defd05a3"},
		RouletteDisguise{"Chef", "outfit_d0fe70cb-c30b-41a3-8d1c-5503e898f686_0.jpg", "d0fe70cb-c30b-41a3-8d1c-5503e898f686"},
		RouletteDisguise{"Coca Field Guard", "outfit_56a589d8-bf28-489f-a30c-2ecea87177f5_0.jpg", "56a589d8-bf28-489f-a30c-2ecea87177f5"},
		RouletteDisguise{"Coca Field Worker", "outfit_11f2849d-87c5-4806-a25e-1a9dad85981d_0.jpg", "11f2849d-87c5-4806-a25e-1a9dad85981d"},
		RouletteDisguise{"Construction Worker", "outfit_57342129-03a9-47a4-a509-cc0656e0a76a_0.jpg", "57342129-03a9-47a4-a509-cc0656e0a76a"},
		RouletteDisguise{"Drug Lab Worker", "outfit_a741cd97-135e-465e-89c3-4fa52a2bbf9d_0.jpg", "a741cd97-135e-465e-89c3-4fa52a2bbf9d"},
		RouletteDisguise{"Elite Guard", "outfit_3e3be8e1-1fe4-4b3a-959c-9e52f595b0c4_0.jpg", "3e3be8e1-1fe4-4b3a-959c-9e52f595b0c4"},
		RouletteDisguise{"Gardener", "outfit_886c3b26-b81f-4731-8080-524f2d6da5dd_0.jpg", "886c3b26-b81f-4731-8080-524f2d6da5dd"},
		RouletteDisguise{"Hippie", "outfit_177410f1-4fd7-4ef2-8ed7-2119bcba3661_0.jpg", "177410f1-4fd7-4ef2-8ed7-2119bcba3661"},
		RouletteDisguise{"Hippo Whisperer", "outfit_4a145036-e4cc-4798-a795-42bcee511524_0.jpg", "4a145036-e4cc-4798-a795-42bcee511524"},
		RouletteDisguise{"Mansion Guard", "outfit_f0d1dfab-ac73-4fe9-bbac-a5587fbc0f91_0.jpg", "f0d1dfab-ac73-4fe9-bbac-a5587fbc0f91"},
		RouletteDisguise{"Mansion Staff", "outfit_2dec1e42-0093-462a-83aa-c0f4d82ac224_0.jpg", "2dec1e42-0093-462a-83aa-c0f4d82ac224"},
		RouletteDisguise{"Shaman", "outfit_30005896-2b39-49c0-bb04-3475d4a12ae6_0.jpg", "30005896-2b39-49c0-bb04-3475d4a12ae6"},
		RouletteDisguise{"Street Soldier", "outfit_ab5a46a2-6e53-4b15-a48e-c336df1ef5ff_0.jpg", "ab5a46a2-6e53-4b15-a48e-c336df1ef5ff"},
		RouletteDisguise{"Submarine Crew", "outfit_f86848e7-ca8c-48e0-94d1-2d925e96a3e7_0.jpg", "f86848e7-ca8c-48e0-94d1-2d925e96a3e7"},
		RouletteDisguise{"Submarine Engineer", "outfit_dfaa8260-20af-4112-b1ca-88a98481127b_0.jpg", "dfaa8260-20af-4112-b1ca-88a98481127b"},
		RouletteDisguise{"Tattoo Artist (P-Power)", "outfit_135073d8-ef7c-4f4d-b30b-cbf65de613cb_0.jpg", "135073d8-ef7c-4f4d-b30b-cbf65de613cb"},
	}},
	{eMission::MUMBAI_CHASINGAGHOST, {
		RouletteDisguise{"Suit", "outfit_5a77f988-62b8-4414-bd8f-b47fd457d0bd_0.jpg", "5a77f988-62b8-4414-bd8f-b47fd457d0bd", true},
		RouletteDisguise{"Barber", "outfit_c4011c75-39ff-4bff-aff5-fe902ae4b83b_0.jpg", "c4011c75-39ff-4bff-aff5-fe902ae4b83b"},
		RouletteDisguise{"Bollywood Bodyguard", "outfit_06fb2890-e820-45f2-aef3-0cb7d0528ee1_0.jpg", "06fb2890-e820-45f2-aef3-0cb7d0528ee1"},
		RouletteDisguise{"Bollywood Crew", "outfit_6d3d59b4-571c-4dbb-9737-205fb34a1ffa_0.jpg", "6d3d59b4-571c-4dbb-9737-205fb34a1ffa"},
		RouletteDisguise{"Dancer", "outfit_88adef78-2a19-45fb-9c95-988e82c056f1_0.jpg", "88adef78-2a19-45fb-9c95-988e82c056f1"},
		RouletteDisguise{"Elite Thug", "outfit_e9e143e1-f5a6-40a5-af56-947cbf32e20a_0.jpg", "e9e143e1-f5a6-40a5-af56-947cbf32e20a"},
		RouletteDisguise{"Food Vendor", "outfit_e9dffefc-e896-46e4-b158-1b401b015764_0.jpg", "e9dffefc-e896-46e4-b158-1b401b015764"},
		RouletteDisguise{"Holy Man", "outfit_e4581e1a-a45a-4c42-ba25-3527bd75c0f7_0.jpg", "e4581e1a-a45a-4c42-ba25-3527bd75c0f7"},
		RouletteDisguise{"Kashmirian", "outfit_6f875d32-869e-437a-8935-368e0c2cc8bc_0.jpg", "6f875d32-869e-437a-8935-368e0c2cc8bc"},
		RouletteDisguise{"Laundry Foreman", "outfit_eeefa90a-6665-4eb1-8bc9-3e08c222abae_0.jpg", "eeefa90a-6665-4eb1-8bc9-3e08c222abae"},
		RouletteDisguise{"Laundry Worker", "outfit_c5c8e251-bb30-4e9e-b146-74ed96c7048f_0.jpg", "c5c8e251-bb30-4e9e-b146-74ed96c7048f"},
		RouletteDisguise{"Lead Actor", "outfit_446ace07-c9c6-49fc-b157-fa58e812fcef_0.jpg", "446ace07-c9c6-49fc-b157-fa58e812fcef"},
		RouletteDisguise{"Local Security", "outfit_d136699a-a244-4789-b332-9a3afc4e3f48_0.jpg", "d136699a-a244-4789-b332-9a3afc4e3f48"},
		RouletteDisguise{"Metal Worker", "outfit_48afc44d-cf8a-44ba-9436-663a6979c908_0.jpg", "48afc44d-cf8a-44ba-9436-663a6979c908"},
		RouletteDisguise{"Painter", "outfit_81f55bbc-a120-4757-a778-b73fd775d1a4_0.jpg", "81f55bbc-a120-4757-a778-b73fd775d1a4"},
		RouletteDisguise{"Queen's Bodyguard", "outfit_6edb224d-0970-4d1d-8740-5e86d1e7af59_0.jpg", "6edb224d-0970-4d1d-8740-5e86d1e7af59"},
		RouletteDisguise{"Queen's Guard", "outfit_b36075a1-b352-4e0f-9d84-84f2bdac6a86_0.jpg", "b36075a1-b352-4e0f-9d84-84f2bdac6a86"},
		RouletteDisguise{"Tailor", "outfit_b384ff35-9c38-4b08-ab0b-e333cfd7bc6a_0.jpg", "b384ff35-9c38-4b08-ab0b-e333cfd7bc6a"},
		RouletteDisguise{"Thug", "outfit_a2cef12c-77d6-4062-9596-cf9d1a47d1b5_0.jpg", "a2cef12c-77d6-4062-9596-cf9d1a47d1b5"},
		RouletteDisguise{"Vanya's Servant", "outfit_ae320bab-bb37-42a5-86a1-df283ada49c0_0.jpg", "ae320bab-bb37-42a5-86a1-df283ada49c0"},
	}},
	{eMission::MUMBAI_ILLUSIONSOFGRANDEUR, {
		RouletteDisguise{"Suit", "outfit_5a77f988-62b8-4414-bd8f-b47fd457d0bd_0.jpg", "5a77f988-62b8-4414-bd8f-b47fd457d0bd", true},
		RouletteDisguise{"Barber", "outfit_c4011c75-39ff-4bff-aff5-fe902ae4b83b_0.jpg", "c4011c75-39ff-4bff-aff5-fe902ae4b83b"},
		RouletteDisguise{"Bollywood Bodyguard", "outfit_06fb2890-e820-45f2-aef3-0cb7d0528ee1_0.jpg", "06fb2890-e820-45f2-aef3-0cb7d0528ee1"},
		RouletteDisguise{"Bollywood Crew", "outfit_6d3d59b4-571c-4dbb-9737-205fb34a1ffa_0.jpg", "6d3d59b4-571c-4dbb-9737-205fb34a1ffa"},
		RouletteDisguise{"Dancer", "outfit_88adef78-2a19-45fb-9c95-988e82c056f1_0.jpg", "88adef78-2a19-45fb-9c95-988e82c056f1"},
		RouletteDisguise{"Elite Thug", "outfit_e9e143e1-f5a6-40a5-af56-947cbf32e20a_0.jpg", "e9e143e1-f5a6-40a5-af56-947cbf32e20a"},
		RouletteDisguise{"Food Vendor", "outfit_e9dffefc-e896-46e4-b158-1b401b015764_0.jpg", "e9dffefc-e896-46e4-b158-1b401b015764"},
		RouletteDisguise{"Holy Man", "outfit_e4581e1a-a45a-4c42-ba25-3527bd75c0f7_0.jpg", "e4581e1a-a45a-4c42-ba25-3527bd75c0f7"},
		RouletteDisguise{"Kashmirian", "outfit_6f875d32-869e-437a-8935-368e0c2cc8bc_0.jpg", "6f875d32-869e-437a-8935-368e0c2cc8bc"},
		RouletteDisguise{"Laundry Foreman", "outfit_eeefa90a-6665-4eb1-8bc9-3e08c222abae_0.jpg", "eeefa90a-6665-4eb1-8bc9-3e08c222abae"},
		RouletteDisguise{"Laundry Worker", "outfit_c5c8e251-bb30-4e9e-b146-74ed96c7048f_0.jpg", "c5c8e251-bb30-4e9e-b146-74ed96c7048f"},
		RouletteDisguise{"Lead Actor", "outfit_446ace07-c9c6-49fc-b157-fa58e812fcef_0.jpg", "446ace07-c9c6-49fc-b157-fa58e812fcef"},
		RouletteDisguise{"Local Security", "outfit_d136699a-a244-4789-b332-9a3afc4e3f48_0.jpg", "d136699a-a244-4789-b332-9a3afc4e3f48"},
		RouletteDisguise{"Metal Worker", "outfit_48afc44d-cf8a-44ba-9436-663a6979c908_0.jpg", "48afc44d-cf8a-44ba-9436-663a6979c908"},
		RouletteDisguise{"Painter", "outfit_81f55bbc-a120-4757-a778-b73fd775d1a4_0.jpg", "81f55bbc-a120-4757-a778-b73fd775d1a4"},
		RouletteDisguise{"Queen's Bodyguard", "outfit_6edb224d-0970-4d1d-8740-5e86d1e7af59_0.jpg", "6edb224d-0970-4d1d-8740-5e86d1e7af59"},
		RouletteDisguise{"Queen's Guard", "outfit_b36075a1-b352-4e0f-9d84-84f2bdac6a86_0.jpg", "b36075a1-b352-4e0f-9d84-84f2bdac6a86"},
		RouletteDisguise{"Tailor", "outfit_b384ff35-9c38-4b08-ab0b-e333cfd7bc6a_0.jpg", "b384ff35-9c38-4b08-ab0b-e333cfd7bc6a"},
		RouletteDisguise{"Thug", "outfit_a2cef12c-77d6-4062-9596-cf9d1a47d1b5_0.jpg", "a2cef12c-77d6-4062-9596-cf9d1a47d1b5"},
		RouletteDisguise{"Vanya's Servant", "outfit_ae320bab-bb37-42a5-86a1-df283ada49c0_0.jpg", "ae320bab-bb37-42a5-86a1-df283ada49c0"},
	}},
	{eMission::WHITTLETON_ANOTHERLIFE, {
		RouletteDisguise{"Suit", "outfit_cdb31942-27b5-4ec3-9009-f19b34d27fd0_0.jpg", "cdb31942-27b5-4ec3-9009-f19b34d27fd0", true},
		RouletteDisguise{"Arkian Robes", "outfit_e75d76b7-25ec-4500-9585-0ce34cec1e1f_0.jpg", "e75d76b7-25ec-4500-9585-0ce34cec1e1f"},
		RouletteDisguise{"BBQ Owner", "outfit_44f30ddb-cad9-402b-a307-6076fae3aa74_0.jpg", "44f30ddb-cad9-402b-a307-6076fae3aa74"},
		RouletteDisguise{"Cassidy Bodyguard", "outfit_7edfb519-9d60-4cd9-b4f4-74dd64d622b9_0.jpg", "7edfb519-9d60-4cd9-b4f4-74dd64d622b9"},
		RouletteDisguise{"Construction Worker", "outfit_699ce756-8eef-4a6e-bc65-264d0e763fde_0.jpg", "699ce756-8eef-4a6e-bc65-264d0e763fde"},
		RouletteDisguise{"Exterminator", "outfit_b1739270-f9fc-4a24-a3e7-beb2deb235f2_0.jpg", "b1739270-f9fc-4a24-a3e7-beb2deb235f2"},
		RouletteDisguise{"Garbage Man", "outfit_4912d30a-80cb-41d8-8137-7b4727e76e4e_0.jpg", "4912d30a-80cb-41d8-8137-7b4727e76e4e"},
		RouletteDisguise{"Gardener", "outfit_78fc9e38-cade-42c3-958c-c7d8edf43713_0.jpg", "78fc9e38-cade-42c3-958c-c7d8edf43713"},
		RouletteDisguise{"Gunther Mueller", "outfit_98a9d41b-3a39-4fe3-ab6a-31d8b574f2ff_0.jpg", "98a9d41b-3a39-4fe3-ab6a-31d8b574f2ff"},
		RouletteDisguise{"James Batty", "outfit_13cbccd1-8a96-435b-84e8-107c0a29349d_0.jpg", "13cbccd1-8a96-435b-84e8-107c0a29349d"},
		RouletteDisguise{"Janus' Bodyguard", "outfit_078a5c70-737c-48b7-a190-b356438419b4_0.jpg", "078a5c70-737c-48b7-a190-b356438419b4"},
		RouletteDisguise{"Mailman", "outfit_89f20c16-4e13-4f89-a85b-44dd17698bc7_0.jpg", "89f20c16-4e13-4f89-a85b-44dd17698bc7"},
		RouletteDisguise{"Nurse", "outfit_4b416b2a-ac08-4379-8c53-46e46d8bcbf8_0.jpg", "4b416b2a-ac08-4379-8c53-46e46d8bcbf8"},
		RouletteDisguise{"Plumber", "outfit_e4c5735c-ea33-4d11-a72b-584902370cf3_0.jpg", "e4c5735c-ea33-4d11-a72b-584902370cf3"},
		RouletteDisguise{"Police Deputy", "outfit_b210be64-ea03-4983-aa0f-8d18882a23c7_0.jpg", "b210be64-ea03-4983-aa0f-8d18882a23c7"},
		RouletteDisguise{"Politician", "outfit_64e48347-cac5-434b-b25c-711ff78c46fd_0.jpg", "64e48347-cac5-434b-b25c-711ff78c46fd"},
		RouletteDisguise{"Politician's Assistant", "outfit_aee5458a-51b7-4ee2-996a-b71b3e149354_0.jpg", "aee5458a-51b7-4ee2-996a-b71b3e149354"},
		RouletteDisguise{"Real Estate Broker", "outfit_d47223f2-3fe4-46d1-a99a-09e0eb57aa7b_0.jpg", "d47223f2-3fe4-46d1-a99a-09e0eb57aa7b"},
		RouletteDisguise{"Server", "outfit_5d19c9f8-7df2-4113-b81d-b32d5e231717_0.jpg", "5d19c9f8-7df2-4113-b81d-b32d5e231717"},
		RouletteDisguise{"Sheriff Masterson", "outfit_874efc03-afda-48f9-b073-b2db0e93bc3f_0.jpg", "874efc03-afda-48f9-b073-b2db0e93bc3f"},
		RouletteDisguise{"Spencer \"The Hammer\" Green", "outfit_6d1a3100-5dc0-4a8a-b9fc-341c864e3841_0.jpg", "6d1a3100-5dc0-4a8a-b9fc-341c864e3841"},
	}},
	{eMission::WHITTLETON_ABITTERPILL, {
		RouletteDisguise{"Suit", "outfit_cdb31942-27b5-4ec3-9009-f19b34d27fd0_0.jpg", "cdb31942-27b5-4ec3-9009-f19b34d27fd0", true},
		RouletteDisguise{"Arkian Robes", "outfit_e75d76b7-25ec-4500-9585-0ce34cec1e1f_0.jpg", "e75d76b7-25ec-4500-9585-0ce34cec1e1f"},
		RouletteDisguise{"BBQ Owner", "outfit_44f30ddb-cad9-402b-a307-6076fae3aa74_0.jpg", "44f30ddb-cad9-402b-a307-6076fae3aa74"},
		RouletteDisguise{"Cassidy Bodyguard", "outfit_7edfb519-9d60-4cd9-b4f4-74dd64d622b9_0.jpg", "7edfb519-9d60-4cd9-b4f4-74dd64d622b9"},
		RouletteDisguise{"Construction Worker", "outfit_699ce756-8eef-4a6e-bc65-264d0e763fde_0.jpg", "699ce756-8eef-4a6e-bc65-264d0e763fde"},
		RouletteDisguise{"Exterminator", "outfit_b1739270-f9fc-4a24-a3e7-beb2deb235f2_0.jpg", "b1739270-f9fc-4a24-a3e7-beb2deb235f2"},
		RouletteDisguise{"Garbage Man", "outfit_4912d30a-80cb-41d8-8137-7b4727e76e4e_0.jpg", "4912d30a-80cb-41d8-8137-7b4727e76e4e"},
		RouletteDisguise{"Gardener", "outfit_78fc9e38-cade-42c3-958c-c7d8edf43713_0.jpg", "78fc9e38-cade-42c3-958c-c7d8edf43713"},
		RouletteDisguise{"Gunther Mueller", "outfit_98a9d41b-3a39-4fe3-ab6a-31d8b574f2ff_0.jpg", "98a9d41b-3a39-4fe3-ab6a-31d8b574f2ff"},
		RouletteDisguise{"James Batty", "outfit_13cbccd1-8a96-435b-84e8-107c0a29349d_0.jpg", "13cbccd1-8a96-435b-84e8-107c0a29349d"},
		RouletteDisguise{"Janus' Bodyguard", "outfit_078a5c70-737c-48b7-a190-b356438419b4_0.jpg", "078a5c70-737c-48b7-a190-b356438419b4"},
		RouletteDisguise{"Mailman", "outfit_89f20c16-4e13-4f89-a85b-44dd17698bc7_0.jpg", "89f20c16-4e13-4f89-a85b-44dd17698bc7"},
		RouletteDisguise{"Nurse", "outfit_4b416b2a-ac08-4379-8c53-46e46d8bcbf8_0.jpg", "4b416b2a-ac08-4379-8c53-46e46d8bcbf8"},
		RouletteDisguise{"Plumber", "outfit_e4c5735c-ea33-4d11-a72b-584902370cf3_0.jpg", "e4c5735c-ea33-4d11-a72b-584902370cf3"},
		RouletteDisguise{"Police Deputy", "outfit_b210be64-ea03-4983-aa0f-8d18882a23c7_0.jpg", "b210be64-ea03-4983-aa0f-8d18882a23c7"},
		RouletteDisguise{"Politician", "outfit_64e48347-cac5-434b-b25c-711ff78c46fd_0.jpg", "64e48347-cac5-434b-b25c-711ff78c46fd"},
		RouletteDisguise{"Politician's Assistant", "outfit_aee5458a-51b7-4ee2-996a-b71b3e149354_0.jpg", "aee5458a-51b7-4ee2-996a-b71b3e149354"},
		RouletteDisguise{"Real Estate Broker", "outfit_d47223f2-3fe4-46d1-a99a-09e0eb57aa7b_0.jpg", "d47223f2-3fe4-46d1-a99a-09e0eb57aa7b"},
		RouletteDisguise{"Server", "outfit_5d19c9f8-7df2-4113-b81d-b32d5e231717_0.jpg", "5d19c9f8-7df2-4113-b81d-b32d5e231717"},
		RouletteDisguise{"Sheriff Masterson", "outfit_874efc03-afda-48f9-b073-b2db0e93bc3f_0.jpg", "874efc03-afda-48f9-b073-b2db0e93bc3f"},
		RouletteDisguise{"Spencer \"The Hammer\" Green", "outfit_6d1a3100-5dc0-4a8a-b9fc-341c864e3841_0.jpg", "6d1a3100-5dc0-4a8a-b9fc-341c864e3841"},
	}},
	{eMission::ISLEOFSGAIL_THEARKSOCIETY, {
		RouletteDisguise{"Suit", "outfit_1e48fda8-4795-4ad4-a05d-0b9ca5d23f78_0.jpg", "1e48fda8-4795-4ad4-a05d-0b9ca5d23f78", true},
		RouletteDisguise{"Architect", "outfit_8d2b15f2-1d23-4b5e-b128-d2f47b53faf7_0.jpg", "8d2b15f2-1d23-4b5e-b128-d2f47b53faf7"},
		RouletteDisguise{"Ark Member", "outfit_b8b1d3c2-cf47-4a44-acc8-d8aa965ec8d8_0.jpg", "b8b1d3c2-cf47-4a44-acc8-d8aa965ec8d8"},
		RouletteDisguise{"Blake Nathaniel", "outfit_d40fe7e8-ec8d-429b-a86b-7844c0e4d1c7_0.jpg", "d40fe7e8-ec8d-429b-a86b-7844c0e4d1c7"},
		RouletteDisguise{"Burial Robes", "outfit_ae340f4d-6282-48d0-8e0d-c3dcb414bb4f_0.jpg", "ae340f4d-6282-48d0-8e0d-c3dcb414bb4f"},
		RouletteDisguise{"Butler", "outfit_e9a9b20d-93de-48b7-8840-73411bace252_0.jpg", "e9a9b20d-93de-48b7-8840-73411bace252"},
		RouletteDisguise{"Castle Staff", "outfit_415c3c97-3c45-43a8-b930-40bece444a55_0.jpg", "415c3c97-3c45-43a8-b930-40bece444a55"},
		RouletteDisguise{"Chef", "outfit_e4aeb186-bedd-41a1-b4c0-bb9c49bc7982_0.jpg", "e4aeb186-bedd-41a1-b4c0-bb9c49bc7982"},
		RouletteDisguise{"Custodian", "outfit_04d72492-1b6b-4e6b-8372-5e65dc209cc4_0.jpg", "04d72492-1b6b-4e6b-8372-5e65dc209cc4"},
		RouletteDisguise{"Elite Guard", "outfit_84c55eed-6891-40b3-9449-6881b53fabdd_0.jpg", "84c55eed-6891-40b3-9449-6881b53fabdd"},
		RouletteDisguise{"Entertainer", "outfit_f9a34b19-f9ff-44a9-b232-86b1b8fcdbb0_0.jpg", "f9a34b19-f9ff-44a9-b232-86b1b8fcdbb0"},
		RouletteDisguise{"Event Staff", "outfit_e3d61bbf-5b28-45cb-88bd-b386f5daa605_0.jpg", "e3d61bbf-5b28-45cb-88bd-b386f5daa605"},
		RouletteDisguise{"Guard", "outfit_6565bf3a-aa59-44f5-9b89-ef645f99d4fa_0.jpg", "6565bf3a-aa59-44f5-9b89-ef645f99d4fa"},
		RouletteDisguise{"Initiate", "outfit_daf223e8-0b22-405f-a3b9-40d2b9992c2f_0.jpg", "daf223e8-0b22-405f-a3b9-40d2b9992c2f"},
		RouletteDisguise{"Jebediah Block", "outfit_bef91840-e5aa-4a44-9f2e-30c732b1f7be_0.jpg", "bef91840-e5aa-4a44-9f2e-30c732b1f7be"},
		RouletteDisguise{"Knight's Armor", "outfit_fae73e92-2307-4163-8e9f-30401ca884bf_0.jpg", "fae73e92-2307-4163-8e9f-30401ca884bf"},
		RouletteDisguise{"Master of Ceremonies", "outfit_9db0a810-7549-4932-b0ab-9d6241afdc2c_0.jpg", "9db0a810-7549-4932-b0ab-9d6241afdc2c"},
		RouletteDisguise{"Raider", "outfit_58f91772-a202-49e4-a558-159f762d78e3_0.jpg", "58f91772-a202-49e4-a558-159f762d78e3"},
	}},
	{eMission::NEWYORK_GOLDENHANDSHAKE, {
		RouletteDisguise{"Suit", "outfit_84f2e067-70c3-4d79-aa90-53b46b727505_0.jpg", "84f2e067-70c3-4d79-aa90-53b46b727505", true},
		RouletteDisguise{"Bank Robber", "outfit_6b22a1db-861c-42fd-ae2d-a4a7bcda72ab_0.jpg", "6b22a1db-861c-42fd-ae2d-a4a7bcda72ab"},
		RouletteDisguise{"Bank Teller", "outfit_589d5082-92e5-4136-922d-786646fb781a_0.jpg", "589d5082-92e5-4136-922d-786646fb781a"},
		RouletteDisguise{"Fired Banker", "outfit_c105fd1e-a017-42e5-8a0c-2996363352eb_0.jpg", "c105fd1e-a017-42e5-8a0c-2996363352eb"},
		RouletteDisguise{"High Security Guard", "outfit_513c0da0-1cb0-4029-85c9-ad9e9522818d_0.jpg", "513c0da0-1cb0-4029-85c9-ad9e9522818d"},
		RouletteDisguise{"Investment Banker", "outfit_e2f6fbfb-0237-477d-b93f-2374b02f0354_0.jpg", "e2f6fbfb-0237-477d-b93f-2374b02f0354"},
		RouletteDisguise{"IT Worker", "outfit_88156045-87c6-4aff-9f99-f2fd40e0ab19_0.jpg", "88156045-87c6-4aff-9f99-f2fd40e0ab19"},
		RouletteDisguise{"Janitor", "outfit_f4e27f1a-3e30-42fe-aa80-dc368590886b_0.jpg", "f4e27f1a-3e30-42fe-aa80-dc368590886b"},
		RouletteDisguise{"Job Applicant", "outfit_d7939c60-087c-461e-9798-c0069cfec299_0.jpg", "d7939c60-087c-461e-9798-c0069cfec299"},
		RouletteDisguise{"Security Guard", "outfit_ee38c686-f447-4a0d-bc5f-3822550db095_0.jpg", "ee38c686-f447-4a0d-bc5f-3822550db095"},
	}},
	{eMission::HAVEN_THELASTRESORT, {
		RouletteDisguise{"Suit", "outfit_ea4230f3-03f7-46f1-a3f4-be2ff383b417_0.jpg", "ea4230f3-03f7-46f1-a3f4-be2ff383b417", true},
		RouletteDisguise{"Boat Captain", "outfit_2817afb5-6dff-4496-bf56-4cd59b9abc9b_0.jpg", "2817afb5-6dff-4496-bf56-4cd59b9abc9b"},
		RouletteDisguise{"Bodyguard", "outfit_95f2f02f-205b-422f-a315-875568f911da_0.jpg", "95f2f02f-205b-422f-a315-875568f911da"},
		RouletteDisguise{"Butler", "haven-butler.png", "fd4c537a-226f-448d-9635-941c6f09d388"},
		RouletteDisguise{"Chef", "outfit_cfc19dda-bff1-4bd1-9b0c-b1a799ee011f_0.jpg", "cfc19dda-bff1-4bd1-9b0c-b1a799ee011f"},
		RouletteDisguise{"Doctor", "outfit_f108122d-5b31-487a-857b-d5f1badf2220_0.jpg", "f108122d-5b31-487a-857b-d5f1badf2220"},
		RouletteDisguise{"Gas Suit", "outfit_cbcfe485-f706-46a1-a14a-316f6dedf398_0.jpg", "cbcfe485-f706-46a1-a14a-316f6dedf398"},
		RouletteDisguise{"Life Guard", "outfit_53415cf7-8d62-45b9-943f-d1a50c7c6024_0.jpg", "53415cf7-8d62-45b9-943f-d1a50c7c6024"},
		RouletteDisguise{"Masseur", "outfit_dec42c4a-3ff0-451f-80b0-a01e68310286_0.jpg", "dec42c4a-3ff0-451f-80b0-a01e68310286"},
		RouletteDisguise{"Personal Trainer", "outfit_49e70108-2c8d-4418-8e42-8f63d6ed43af_0.jpg", "49e70108-2c8d-4418-8e42-8f63d6ed43af"},
		RouletteDisguise{"Resort Guard", "outfit_d4c9507a-b297-46ce-8e9c-4ec479da22a4_0.jpg", "d4c9507a-b297-46ce-8e9c-4ec479da22a4"},
		RouletteDisguise{"Resort Staff", "outfit_e9fa4892-fa2a-40a1-a51c-78d8561034f3_0.jpg", "e9fa4892-fa2a-40a1-a51c-78d8561034f3"},
		RouletteDisguise{"Snorkel Instructor", "outfit_30164cfe-a26b-4a72-8bc2-5bc99c0283c1_0.jpg", "30164cfe-a26b-4a72-8bc2-5bc99c0283c1"},
		RouletteDisguise{"Tech Crew", "outfit_f6e37038-98c1-4e58-bd85-c895f5c19d56_0.jpg", "f6e37038-98c1-4e58-bd85-c895f5c19d56"},
		RouletteDisguise{"Villa Guard", "outfit_33e3a400-0bbc-4edd-b07f-056135329802_0.jpg", "33e3a400-0bbc-4edd-b07f-056135329802"},
		RouletteDisguise{"Villa Staff", "outfit_cda86b1b-63a4-4e3a-975e-d716685335a7_0.jpg", "cda86b1b-63a4-4e3a-975e-d716685335a7"},
		RouletteDisguise{"Waiter", "outfit_a260d9d6-a33c-499e-a6c5-698cfcc3de8f_0.jpg", "a260d9d6-a33c-499e-a6c5-698cfcc3de8f"},
	}},
	{eMission::DUBAI_ONTOPOFTHEWORLD, {
		RouletteDisguise{"Suit", "outfit_07ab08e1-013e-439d-a98b-3b7e8c9f13bc_0.jpg", "07ab08e1-013e-439d-a98b-3b7e8c9f13bc", true},
		RouletteDisguise{"Art Crew", "outfit_2c649c52-f85a-4b29-838a-31c2525cc862_0.jpg", "2c649c52-f85a-4b29-838a-31c2525cc862"},
		RouletteDisguise{"Event Security", "outfit_eb12cc2b-6dcf-4831-ba4e-ef8e53180e2f_0.jpg", "eb12cc2b-6dcf-4831-ba4e-ef8e53180e2f"},
		RouletteDisguise{"Event Staff", "outfit_77fb4c80-0b81-4672-be65-12c16c3ac7ac_0.jpg", "77fb4c80-0b81-4672-be65-12c16c3ac7ac"},
		RouletteDisguise{"Famous Chef", "outfit_6dcf16f6-6620-410f-b51c-179f75de938c_0.jpg", "6dcf16f6-6620-410f-b51c-179f75de938c"},
		RouletteDisguise{"Helicopter Pilot", "outfit_ea5b1cea-c305-4f60-9512-78b2e6cd5030_0.jpg", "ea5b1cea-c305-4f60-9512-78b2e6cd5030"},
		RouletteDisguise{"Ingram's Bodyguard", "outfit_bdbd806d-eb11-4167-bd2d-f5f015c3fe86_0.jpg", "bdbd806d-eb11-4167-bd2d-f5f015c3fe86"},
		RouletteDisguise{"Kitchen Staff", "outfit_eb15e523-713f-41ba-ad67-d33b02de43c6_0.jpg", "eb15e523-713f-41ba-ad67-d33b02de43c6"},
		RouletteDisguise{"Maintenance Staff", "outfit_e65f04b2-47a6-4d3d-b36c-9fb7fa08a00b_0.jpg", "e65f04b2-47a6-4d3d-b36c-9fb7fa08a00b"},
		RouletteDisguise{"Penthouse Guard", "outfit_f0a52fef-608a-4fa8-9fd6-bd5c15506188_0.jpg", "f0a52fef-608a-4fa8-9fd6-bd5c15506188"},
		RouletteDisguise{"Penthouse Staff", "outfit_a745ca17-3a7e-4c15-8219-6a5d6245ac7f_0.jpg", "a745ca17-3a7e-4c15-8219-6a5d6245ac7f"},
		RouletteDisguise{"Skydiving Suit", "outfit_c4146f27-81a9-42ef-b3c7-87a9d60b87fe_0.jpg", "c4146f27-81a9-42ef-b3c7-87a9d60b87fe"},
		RouletteDisguise{"The Assassin", "outfit_ef9dddc5-25c7-450f-afcb-ac1b8f9569c9_0.jpg", "ef9dddc5-25c7-450f-afcb-ac1b8f9569c9"},
	}},
	{eMission::DARTMOOR_DEATHINTHEFAMILY, {
		RouletteDisguise{"Suit", "outfit_a9de864e-ce00-4970-978a-4a9f8db73974_0.jpg", "a9de864e-ce00-4970-978a-4a9f8db73974", true},
		RouletteDisguise{"Bodyguard", "outfit_29389af2-4fe4-4f72-917a-d9747adc0f3d_0.jpg", "29389af2-4fe4-4f72-917a-d9747adc0f3d"},
		RouletteDisguise{"Gardener", "outfit_88246749-2118-2101-5575-991052571240_0.jpg", "88246749-2118-2101-5575-991052571240"},
		RouletteDisguise{"Lawyer", "outfit_ffb2e3a8-728b-4a54-95cb-55eaf616b422_0.jpg", "ffb2e3a8-728b-4a54-95cb-55eaf616b422"},
		RouletteDisguise{"Mansion Guard", "outfit_c3349736-91d1-48e3-bc62-fc16a7d8d6f1_0.jpg", "c3349736-91d1-48e3-bc62-fc16a7d8d6f1"},
		RouletteDisguise{"Mansion Staff", "outfit_4115e440-fdf8-44d2-a3ba-a1bb2285e542_0.jpg", "4115e440-fdf8-44d2-a3ba-a1bb2285e542"},
		RouletteDisguise{"Photographer", "outfit_7062bd6b-4926-4ab3-932c-de7d63c095b7_0.jpg", "7062bd6b-4926-4ab3-932c-de7d63c095b7"},
		RouletteDisguise{"Private Investigator", "outfit_12f5bdb5-7e71-4f48-9740-13d0211f48c6_0.jpg", "12f5bdb5-7e71-4f48-9740-13d0211f48c6"},
		RouletteDisguise{"Undertaker", "outfit_dc3c386d-52c2-4e17-855d-6c15e080ccf3_0.jpg", "dc3c386d-52c2-4e17-855d-6c15e080ccf3"},
	}},
	{eMission::BERLIN_APEXPREDATOR, {
		RouletteDisguise{"Suit", "outfit_19e3757f-01b5-4821-97c3-1a1045646531_0.jpg", "19e3757f-01b5-4821-97c3-1a1045646531", true},
		RouletteDisguise{"Bartender", "outfit_816cf012-ab64-48a0-b4cc-ff7470874007_0.jpg", "816cf012-ab64-48a0-b4cc-ff7470874007"},
		RouletteDisguise{"Biker", "outfit_95918f14-fa9f-4315-be95-bf4b9efe6ee6_0.jpg", "95918f14-fa9f-4315-be95-bf4b9efe6ee6"},
		RouletteDisguise{"Club Crew", "outfit_6e84215c-28b7-44b2-9d15-83e9be490965_0.jpg", "6e84215c-28b7-44b2-9d15-83e9be490965"},
		RouletteDisguise{"Club Security", "outfit_590629f7-19a3-4eb8-88a6-94e550cd1c07_0.jpg", "590629f7-19a3-4eb8-88a6-94e550cd1c07"},
		RouletteDisguise{"Dealer", "outfit_4c379903-4cf2-49cf-953f-db7b31d2987d_0.jpg", "4c379903-4cf2-49cf-953f-db7b31d2987d"},
		RouletteDisguise{"Delivery Guy", "outfit_2e5bdc9b-822d-4f5f-bc16-bd99729ef4f5_0.jpg", "2e5bdc9b-822d-4f5f-bc16-bd99729ef4f5"},
		RouletteDisguise{"DJ", "outfit_ac636da9-fd3a-4019-816a-6333e0c4298e_0.jpg", "ac636da9-fd3a-4019-816a-6333e0c4298e"},
		RouletteDisguise{"Florida Man", "outfit_0e931214-6ba9-4763-b7e1-32ca64dd864a_0.jpg", "0e931214-6ba9-4763-b7e1-32ca64dd864a"},
		RouletteDisguise{"Rolf Hirschmüller"s, "outfit_8e41db54-b097-4704-8a88-83043e6557f7_0.jpg", "8e41db54-b097-4704-8a88-83043e6557f7"},
		RouletteDisguise{"Technician", "outfit_f724d6b9-a45b-425f-84f1-c27dedd1fd07_0.jpg", "f724d6b9-a45b-425f-84f1-c27dedd1fd07"},
	}},
	{eMission::CHONGQING_ENDOFANERA, {
		RouletteDisguise{"Suit", "outfit_90ad022f-0789-413f-bf3d-603c1237c9b1_0.jpg", "90ad022f-0789-413f-bf3d-603c1237c9b1", true},
		RouletteDisguise{"Block Guard", "outfit_4dd90d10-ac5f-404f-9c20-4428653ca7ba_0.jpg", "4dd90d10-ac5f-404f-9c20-4428653ca7ba"},
		RouletteDisguise{"Dumpling Cook", "outfit_c5f6dd2a-3600-40be-9a82-bbf5d360c379_0.jpg", "c5f6dd2a-3600-40be-9a82-bbf5d360c379"},
		RouletteDisguise{"Facility Analyst", "outfit_9c07a86c-2d03-417b-b46f-cb433481080e_0.jpg", "9c07a86c-2d03-417b-b46f-cb433481080e"},
		RouletteDisguise{"Facility Engineer", "outfit_8fc343f2-6e9a-4e06-9342-705e5b171895_0.jpg", "8fc343f2-6e9a-4e06-9342-705e5b171895"},
		RouletteDisguise{"Facility Guard", "outfit_f5c73d58-a24f-4957-b80d-5efb6771ad9b_0.jpg", "f5c73d58-a24f-4957-b80d-5efb6771ad9b"},
		RouletteDisguise{"Facility Security", "outfit_b3515a1e-4a32-475c-bd61-4fdae243a7e5_0.jpg", "b3515a1e-4a32-475c-bd61-4fdae243a7e5"},
		RouletteDisguise{"Homeless Person", "outfit_ba4e595e-da3b-4902-8622-40889fc088db_0.jpg", "ba4e595e-da3b-4902-8622-40889fc088db"},
		RouletteDisguise{"Perfect Test Subject", "outfit_9cd5fbd7-903c-4ab7-afe8-01eb755ce9da_0.jpg", "9cd5fbd7-903c-4ab7-afe8-01eb755ce9da"},
		RouletteDisguise{"Researcher", "outfit_553bb399-2ee0-41bb-a76b-b7ec6d49f5a3_0.jpg", "553bb399-2ee0-41bb-a76b-b7ec6d49f5a3"},
		RouletteDisguise{"Street Guard", "outfit_86bdb741-6810-4610-8e21-799c93c71849_0.jpg", "86bdb741-6810-4610-8e21-799c93c71849"},
		RouletteDisguise{"The Board Member", "outfit_fd1d39d8-db06-47b3-8f4b-eb1febf5ccb7_0.jpg", "fd1d39d8-db06-47b3-8f4b-eb1febf5ccb7"},
	}},
	{eMission::MENDOZA_THEFAREWELL, {
		RouletteDisguise{"Suit", "outfit_6c129ec5-41cb-43f1-837d-ebff54f260c6_0.jpg", "6c129ec5-41cb-43f1-837d-ebff54f260c6", true},
		RouletteDisguise{"Asado Chef", "outfit_8d105591-dfbe-46aa-8520-f00f986b57e2_0.jpg", "8d105591-dfbe-46aa-8520-f00f986b57e2"},
		RouletteDisguise{"Bodyguard", "outfit_aa7dc754-702a-401b-8f84-e806e958869c_0.jpg", "aa7dc754-702a-401b-8f84-e806e958869c"},
		RouletteDisguise{"Chief Winemaker", "outfit_af56d687-ba1b-44c8-8061-fd4a4a1222a3_0.jpg", "af56d687-ba1b-44c8-8061-fd4a4a1222a3"},
		RouletteDisguise{"Corvo Black", "outfit_214b2143-3277-44cd-b20f-344747fc23d9_0.jpg", "214b2143-3277-44cd-b20f-344747fc23d9"},
		RouletteDisguise{"Gaucho", "outfit_e887e53a-6b02-455d-88be-284af6d88e94_0.jpg", "e887e53a-6b02-455d-88be-284af6d88e94"},
		RouletteDisguise{"Head of Security", "outfit_16df6808-97ac-4c3a-8d4b-7ddacfc8a7ea_0.jpg", "16df6808-97ac-4c3a-8d4b-7ddacfc8a7ea"},
		RouletteDisguise{"Lawyer", "outfit_521ed265-2115-4977-8db0-45404b067102_0.jpg", "521ed265-2115-4977-8db0-45404b067102"},
		RouletteDisguise{"Mercenary", "outfit_69d4d32b-0fc9-4fde-8817-fafd98c13365_0.jpg", "69d4d32b-0fc9-4fde-8817-fafd98c13365"},
		RouletteDisguise{"Providence Herald", "outfit_f5b24132-7a6b-4a3f-868d-193b8692a52b_0.jpg", "f5b24132-7a6b-4a3f-868d-193b8692a52b"},
		RouletteDisguise{"Sommelier", "outfit_7fed7c24-08b2-468b-8e49-22b5ad59b56b_0.jpg", "7fed7c24-08b2-468b-8e49-22b5ad59b56b"},
		RouletteDisguise{"Tango Musician", "outfit_6ab03e04-9e88-4237-a596-96e3135420ab_0.jpg", "6ab03e04-9e88-4237-a596-96e3135420ab"},
		RouletteDisguise{"Waiter", "outfit_cac0081e-9eb0-4fbf-ba23-70c2815f0874_0.jpg", "cac0081e-9eb0-4fbf-ba23-70c2815f0874"},
		RouletteDisguise{"Winery Worker", "outfit_bbdfca80-abef-4b43-953e-9a46c3eee2eb_0.jpg", "bbdfca80-abef-4b43-953e-9a46c3eee2eb"},
	}},
	{eMission::CARPATHIAN_UNTOUCHABLE, {
		RouletteDisguise{"Suit", "outfit_e1d1ffa6-deca-445a-8e8c-db74b0856cee_0.jpg", "e1d1ffa6-deca-445a-8e8c-db74b0856cee", true},
		RouletteDisguise{"Office Staff", "outfit_81fc37ca-e20b-495f-961f-d5be311a6e6d_0.jpg", "81fc37ca-e20b-495f-961f-d5be311a6e6d"},
		RouletteDisguise{"Providence Commando", "outfit_e77b5340-41d3-448a-84d3-a4f7f6426634_0.jpg", "e77b5340-41d3-448a-84d3-a4f7f6426634"},
		RouletteDisguise{"Providence Commando Leader", "outfit_36402728-1197-4a3c-a8ac-1fed399a2344_0.jpg", "36402728-1197-4a3c-a8ac-1fed399a2344"},
		RouletteDisguise{"Providence Doctor", "outfit_abe4b536-1f09-421e-916b-20af142c6adb_0.jpg", "abe4b536-1f09-421e-916b-20af142c6adb"},
		RouletteDisguise{"Providence Elite Guard", "outfit_68225457-66b3-457c-a6ec-065b001f5151_0.jpg", "68225457-66b3-457c-a6ec-065b001f5151"},
		RouletteDisguise{"Providence Security Guard (Militia Zone)", "outfit_c3239200-0f56-4b45-9be5-e514bdf59d26_0.jpg", "c3239200-0f56-4b45-9be5-e514bdf59d26"},
		RouletteDisguise{"Providence Security Guard (Office)", "outfit_653ad7d6-7d5d-4554-9551-7573be7205be_0.jpg", "653ad7d6-7d5d-4554-9551-7573be7205be"},
	}},
	{eMission::AMBROSE_SHADOWSINTHEWATER, {
		RouletteDisguise{"Suit", "outfit_8b74c103-ec0d-4e4e-8664-d06dfe869e8f_0.jpg", "8b74c103-ec0d-4e4e-8664-d06dfe869e8f", true},
		RouletteDisguise{"Cook", "outfit_d9d95b38-3708-4220-9838-597c078a1081_0.jpg", "d9d95b38-3708-4220-9838-597c078a1081"},
		RouletteDisguise{"Engineer", "outfit_c3af265c-7648-4ddb-a02b-ab605a053886_0.jpg", "c3af265c-7648-4ddb-a02b-ab605a053886"},
		RouletteDisguise{"Hippie", "outfit_9f95337d-0316-4cfc-9881-13080e2bc365_0.jpg", "9f95337d-0316-4cfc-9881-13080e2bc365"},
		RouletteDisguise{"Metal Worker", "outfit_f011f287-ea39-42a4-be1d-17ba5b783611_0.jpg", "f011f287-ea39-42a4-be1d-17ba5b783611"},
		RouletteDisguise{"Militia Soldier", "outfit_afdb3b2b-7e5d-4c9a-be6e-4ebc41879e02_0.jpg", "afdb3b2b-7e5d-4c9a-be6e-4ebc41879e02"},
		RouletteDisguise{"Pirate", "outfit_1cec2601-c1ed-474f-ac70-ff8614799fcc_0.jpg", "1cec2601-c1ed-474f-ac70-ff8614799fcc"},
	}},
};


const std::vector<RouletteMission> missions = {
	{eMission::ICAFACILITY_FREEFORM},
};

std::vector<RouletteMission> Missions::data;
Missions Missions::instance;

Missions::Missions() {
	// Check for loud eliminations or loud live kills.
	auto loudElimTest = [](const RouletteSpinCondition& cond) {
		return (cond.killMethod.isElimination || cond.killComplication == eKillComplication::Live) && cond.killType == eKillType::Loud;
	};

	for (auto& miss : missionInfos) {
		auto& mission = data.emplace_back(miss.mission);
		switch (mission.getMission()) {
			case eMission::ICAFACILITY_GUIDED:
			case eMission::ICAFACILITY_FREEFORM:
				{
					auto& kr = mission.addTarget(eTargetID::KalvinRitter, "Kalvin Ritter", "polarbear2_sparrow.jpg");
					kr.defineMethod(eKillMethod::Electrocution, { eMethodTag::Impossible });
					kr.defineMethod(eKillMethod::Fire, { eMethodTag::Impossible });
					kr.defineMethod(eKillMethod::InjectedPoison, { eMethodTag::Impossible });
					kr.defineMethod(eKillMethod::AssaultRifle, { eMethodTag::LoudOnly });
					kr.defineMethod(eKillMethod::Shotgun, { eMethodTag::LoudOnly });
					kr.defineMethod(eKillMethod::SMG, { eMethodTag::Impossible });
					kr.defineMethod(eKillMethod::SMGElimination, { eMethodTag::Impossible });
					kr.defineMethod(eKillMethod::Sniper, { eMethodTag::Impossible });
					break;
				}
			case eMission::ICAFACILITY_FINALTEST:
				{
					auto& jk = mission.addTarget(eTargetID::JasperKnight, "Jasper Knight", "polarbear5.jpg");
					jk.defineMethod(eKillMethod::Electrocution, { eMethodTag::Impossible });
					jk.defineMethod(eKillMethod::Fire, { eMethodTag::Impossible });
					jk.defineMethod(eKillMethod::InjectedPoison, { eMethodTag::Impossible });
					jk.defineMethod(eKillMethod::AssaultRifle, { eMethodTag::LoudOnly });
					jk.defineMethod(eKillMethod::Shotgun, { eMethodTag::LoudOnly });
					jk.defineMethod(eKillMethod::SMG, { eMethodTag::Impossible });
					jk.defineMethod(eKillMethod::SMGElimination, { eMethodTag::Impossible });
					jk.defineMethod(eKillMethod::Sniper, { eMethodTag::Impossible });
					break;
				}
			case eMission::PARIS_SHOWSTOPPER:
				{
					auto& vn = mission.addTarget(eTargetID::ViktorNovikov, "Viktor Novikov", "showstopper_viktor_novikov.jpg");
					vn.defineMethod(eKillMethod::Fire, { eMethodTag::BannedInRR, eMethodTag::Extreme, eMethodTag::DuplicateOnlySameDisguise });

					auto& dm = mission.addTarget(eTargetID::DaliaMargolis, "Dalia Margolis", "showstopper_dahlia_margolis.jpg");
					dm.defineMethod(eKillMethod::Fire, { eMethodTag::BannedInRR, eMethodTag::Extreme, eMethodTag::DuplicateOnlySameDisguise });
					break;
				}
			case eMission::PARIS_HOLIDAYHOARDERS:
				{
					auto& hb = mission.addTarget(eTargetID::HarrySmokeyBagnato, "Harry \"Smokey\" Bagnato", "noel_harry_bagnato.jpg");
					hb.defineMethod(eKillMethod::Fire, { eMethodTag::BannedInRR, eMethodTag::Extreme, eMethodTag::DuplicateOnlySameDisguise });
					auto& mg = mission.addTarget(eTargetID::MarvSlickGonif, "Marv \"Slick\" Gonif", "noel_marv_gonif.jpg");
					mg.defineMethod(eKillMethod::Fire, { eMethodTag::BannedInRR, eMethodTag::Extreme, eMethodTag::DuplicateOnlySameDisguise });
					break;
				}
			case eMission::SAPIENZA_WORLDOFTOMORROW:
				{
					auto& sc = mission.addTarget(eTargetID::SilvioCaruso, "Silvio Caruso", "world_of_tomorrow_silvio_caruso.jpg");
					sc.defineMethod(eKillMethod::ConsumedPoison, { eMethodTag::BannedInRR, eMethodTag::Buggy });
					sc.defineMethod(eKillMethod::Fire, { eMethodTag::BannedInRR, eMethodTag::Impossible });

					auto& fds = mission.addTarget(eTargetID::FrancescaDeSantis, "Francesca De Santis", "world_of_tomorrow_francesca_de_santis.jpg");
					fds.defineMethod(eKillMethod::Fire, { eMethodTag::BannedInRR, eMethodTag::Impossible });
					break;
				}
			case eMission::SAPIENZA_THEICON:
				{
					auto& db = mission.addTarget(eTargetID::DinoBosco, "Dino Bosco", "copperhead_roman_strauss_levine.jpg");
					break;
				}
			case eMission::SAPIENZA_LANDSLIDE:
				{
					auto& ma = mission.addTarget(eTargetID::MarcoAbiatti, "Marco Abiatti", "mamba_marco_abiatti.jpg");
					ma.defineMethod(eKillMethod::Fire, { eMethodTag::BannedInRR, eMethodTag::Impossible });
					break;
				}
			case eMission::SAPIENZA_THEAUTHOR:
				{
					auto& cb = mission.addTarget(eTargetID::CraigBlack, "Craig Black", "ws_ebola_craig_black.jpg");
					cb.defineMethod(eKillMethod::Fire, { eMethodTag::BannedInRR, eMethodTag::Impossible });

					auto& ba = mission.addTarget(eTargetID::BrotherAkram, "Brother Akram", "ws_ebola_brother_akram.jpg");
					ba.defineMethod(eKillMethod::Fire, { eMethodTag::BannedInRR, eMethodTag::Impossible });
					break;
				}
			case eMission::MARRAKESH_GILDEDCAGE:
				{
					auto& rz = mission.addTarget(eTargetID::RezaZaydan, "Reza Zaydan", "tobigforjail_general_zaydan.jpg");
					rz.defineMethod(eKillMethod::Drowning, { eMethodTag::BannedInRR, eMethodTag::Extreme });
					rz.defineMethod(eKillMethod::Electrocution, { eMethodTag::BannedInRR, eMethodTag::Extreme });

					auto& chs = mission.addTarget(eTargetID::ClausHugoStrandberg, "Claus Hugo Strandberg", "tobigforjail_claus_hugo_stranberg.jpg");
					chs.defineMethod(eKillMethod::FallingObject, { eMethodTag::BannedInRR, eMethodTag::Extreme });
					auto& prisoner = mission.getDisguiseByNameAssert("Prisoner");

					chs.addRule([&prisoner](const RouletteSpinCondition& cond) {
						if (&cond.disguise.get() != &prisoner) return false;
						return !cond.killMethod.isRemote;
					}, { eMethodTag::BannedInRR, eMethodTag::Hard });
					break;
				}
			case eMission::MARRAKESH_HOUSEBUILTONSAND:
				{
					auto& ktk = mission.addTarget(eTargetID::KongTuoKwang, "Kong Tuo-Kwang", "python_kong_tou_kwang_briefing.jpg");
					auto& mm = mission.addTarget(eTargetID::MatthieuMendola, "Matthieu Mendola", "python_matthieu_mendola_briefing.jpg");
					break;
				}
			case eMission::BANGKOK_CLUB27:
				{
					auto& stalker = mission.getDisguiseByNameAssert("Stalker");
					auto stalkerRemoteTest = [&stalker](const RouletteSpinCondition& cond) {
						if (&cond.disguise.get() != &stalker) return false;
						return !cond.killMethod.isRemote;
					};

					auto& jc = mission.addTarget(eTargetID::JordanCross, "Jordan Cross", "club27_jordan_cross.jpg");
					jc.defineMethod(eKillMethod::Fire, { eMethodTag::BannedInRR, eMethodTag::Extreme });
					jc.addRule(stalkerRemoteTest, { eMethodTag::BannedInRR, eMethodTag::Hard });

					auto& km = mission.addTarget(eTargetID::KenMorgan, "Ken Morgan", "club27_ken_morgan.jpg");
					km.defineMethod(eKillMethod::Fire, { eMethodTag::BannedInRR, eMethodTag::Extreme });
					km.addRule(stalkerRemoteTest, { eMethodTag::BannedInRR, eMethodTag::Extreme });
					break;
				}
			case eMission::BANGKOK_THESOURCE:
				{
					auto& on = mission.addTarget(eTargetID::OybekNabazov, "Oybek Nabazov", "ws_zika_oybek_nabazov.jpg");
					on.defineMethod(eKillMethod::FallingObject, { eMethodTag::BannedInRR, eMethodTag::Extreme });
					auto& sy = mission.addTarget(eTargetID::SisterYulduz, "Sister Yulduz", "ws_zika_sister_yulduz.jpg");
					sy.defineMethod(eKillMethod::FallingObject, { eMethodTag::BannedInRR, eMethodTag::Extreme });
					break;
				}
			case eMission::COLORADO_FREEDOMFIGHTERS:
				{
					auto& sr = mission.addTarget(eTargetID::SeanRose, "Sean Rose", "freedom_fighters_sean_rose.jpg");
					sr.defineMethod(eKillMethod::Drowning, { eMethodTag::BannedInRR, eMethodTag::Extreme });
					sr.defineMethod(eKillMethod::ConsumedPoison, { eMethodTag::BannedInRR, eMethodTag::Impossible });
					sr.addRule(loudElimTest, { eMethodTag::BannedInRR, eMethodTag::Hard });

					auto& pg = mission.addTarget(eTargetID::PenelopeGraves, "Penelope Graves", "freedom_fighters_penelope_graves.jpg");
					pg.defineMethod(eKillMethod::Drowning, { eMethodTag::BannedInRR, eMethodTag::Extreme });
					pg.defineMethod(eKillMethod::Fire, { eMethodTag::BannedInRR, eMethodTag::Hard });
					pg.addRule(loudElimTest, { eMethodTag::BannedInRR, eMethodTag::Hard });

					auto& eb = mission.addTarget(eTargetID::EzraBerg, "Ezra Berg", "freedom_fighters_ezra_berg.jpg");
					eb.defineMethod(eKillMethod::ConsumedPoison, { eMethodTag::BannedInRR, eMethodTag::Impossible });
					eb.defineMethod(eKillMethod::Drowning, { eMethodTag::BannedInRR, eMethodTag::Extreme });
					eb.defineMethod(eKillMethod::Electrocution, { eMethodTag::BannedInRR, eMethodTag::Extreme });

					auto& mp = mission.addTarget(eTargetID::MayaParvati, "Maya Parvati", "freedom_fighters_maya_parvati.jpg");
					mp.defineMethod(eKillMethod::Drowning, { eMethodTag::BannedInRR, eMethodTag::Extreme });
					break;
				}
			case eMission::HOKKAIDO_SITUSINVERSUS:
				{
					auto& es = mission.addTarget(eTargetID::ErichSoders, "Erich Soders", "snowcrane_erich_soders_briefing.jpg", eTargetType::Soders);

					auto& yy = mission.addTarget(eTargetID::YukiYamazaki, "Yuki Yamazaki", "snowcrane_yuki_yamazaki_briefing.jpg");
					yy.defineMethod(eKillMethod::Fire, { eMethodTag::BannedInRR });
					break;
				}
			case eMission::HOKKAIDO_PATIENTZERO:
				{
					auto& oc = mission.addTarget(eTargetID::OwenCage, "Owen Cage", "ws_flu_owen_cage.jpg");
					oc.defineMethod(eKillMethod::Drowning, { eMethodTag::BannedInRR, eMethodTag::Extreme });
					auto& kl = mission.addTarget(eTargetID::KlausLiebleid, "Klaus Liebleid", "ws_flu_klaus_leiblied.jpg");
					break;
				}
			case eMission::HOKKAIDO_SNOWFESTIVAL:
				{
					auto& df = mission.addTarget(eTargetID::DmitriFedorov, "Dmitri Fedorov", "mamushi_dimitri-fedorov.jpg");
					break;
				}
			case eMission::HAWKESBAY_NIGHTCALL:
				{
					auto& ar = mission.addTarget(eTargetID::AlmaReynard, "Alma Reynard", "sheep_alma_reynard.jpg");
					ar.defineMethod(eKillMethod::Fire, { eMethodTag::Impossible });
					ar.defineMethod(eKillMethod::FallingObject, { eMethodTag::Impossible });
					break;
				}
			case eMission::MIAMI_FINISHLINE:
				{
					auto& sk = mission.addTarget(eTargetID::SierraKnox, "Sierra Knox", "flamingo_sierra_knox.jpg");

					auto& rk = mission.addTarget(eTargetID::RobertKnox, "Robert Knox", "flamingo_robert_knox.jpg");
					rk.defineMethod(eKillMethod::Fire, { eMethodTag::BannedInRR, eMethodTag::Impossible });
					break;
				}
			case eMission::MIAMI_ASILVERTONGUE:
				{
					auto& aj = mission.addTarget(eTargetID::AjitKrish, "Ajit \"AJ\" Krish", "cottonmouth_ajit_krish.jpg");
					break;
				}
			case eMission::SANTAFORTUNA_THREEHEADEDSERPENT:
				{
					auto& rd = mission.addTarget(eTargetID::RicoDelgado, "Rico Delgado", "hippo_rico_delgado.jpg");
					rd.defineMethod(eKillMethod::Fire, { eMethodTag::BannedInRR, eMethodTag::Extreme });
					rd.defineMethod(eKillMethod::ConsumedPoison, { eMethodTag::BannedInRR, eMethodTag::Extreme });

					auto& jf = mission.addTarget(eTargetID::JorgeFranco, "Jorge Franco", "hippo_jorge_franco.jpg");
					jf.defineMethod(eKillMethod::FallingObject, { eMethodTag::BannedInRR, eMethodTag::Impossible });

					auto& am = mission.addTarget(eTargetID::AndreaMartinez, "Andrea Martinez", "hippo_andrea_martinez.jpg");
					am.defineMethod(eKillMethod::Fire, { eMethodTag::BannedInRR, eMethodTag::Extreme });
					break;
				}
			case eMission::SANTAFORTUNA_EMBRACEOFTHESERPENT:
				{
					auto& br = mission.addTarget(eTargetID::BlairReddington, "Blair Reddington", "anaconda_blair_reddington_face.jpg");
					break;
				}
			case eMission::MUMBAI_CHASINGAGHOST:
				{
					auto& wk = mission.addTarget(eTargetID::WazirKale, "Wazir Kale", "mongoose_wazir_kale_identified.jpg");
					wk.defineMethod(eKillMethod::Fire, { eMethodTag::BannedInRR, eMethodTag::Extreme });
					wk.defineMethod(eKillMethod::Drowning, { eMethodTag::BannedInRR, eMethodTag::Extreme });
					wk.addRule(loudElimTest, { eMethodTag::BannedInRR, eMethodTag::Extreme });

					auto& vs = mission.addTarget(eTargetID::VanyaShah, "Vanya Shah", "mongoose_vanya_shah.jpg");
					vs.defineMethod(eKillMethod::Drowning, { eMethodTag::BannedInRR, eMethodTag::Extreme });
					vs.defineMethod(eKillMethod::ConsumedPoison, { eMethodTag::BannedInRR, eMethodTag::Extreme });
					vs.addRule(loudElimTest, { eMethodTag::BannedInRR, eMethodTag::Extreme });

					auto& dr = mission.addTarget(eTargetID::DawoodRangan, "Dawood Rangan", "mongoose_dawood_rangan.jpg");
					dr.defineMethod(eKillMethod::Fire, { eMethodTag::BannedInRR, eMethodTag::Extreme });
					dr.defineMethod(eKillMethod::ConsumedPoison, { eMethodTag::BannedInRR, eMethodTag::Hard });
					dr.addRule(loudElimTest, { eMethodTag::BannedInRR, eMethodTag::Hard });
					break;
				}
			case eMission::MUMBAI_ILLUSIONSOFGRANDEUR:
				{
					auto& bc = mission.addTarget(eTargetID::BasilCarnaby, "Basil Carnaby", "kingcobra_basil_carnaby_face.jpg");
					break;
				}
			case eMission::WHITTLETON_ANOTHERLIFE:
				{
					auto& j = mission.addTarget(eTargetID::Janus, "Janus", "skunk_janus.jpg");
					j.defineMethod(eKillMethod::Fire, { eMethodTag::BannedInRR, eMethodTag::Impossible });
					j.defineMethod(eKillMethod::FallingObject, { eMethodTag::BannedInRR, eMethodTag::Impossible });
					j.defineMethod(eMapKillMethod::BattleAxe, { eMethodTag::BannedInRR, eMethodTag::Hard });
					j.defineMethod(eMapKillMethod::BeakStaff, { eMethodTag::BannedInRR, eMethodTag::Extreme });

					auto& nc = mission.addTarget(eTargetID::NolanCassidy, "Nolan Cassidy", "skunk_nolan_cassidy.jpg");
					nc.defineMethod(eKillMethod::Fire, { eMethodTag::BannedInRR, eMethodTag::Impossible });
					nc.defineMethod(eMapKillMethod::BattleAxe, { eMethodTag::BannedInRR, eMethodTag::Hard });
					nc.defineMethod(eMapKillMethod::BeakStaff, { eMethodTag::BannedInRR, eMethodTag::Extreme });
					break;
				}
			case eMission::WHITTLETON_ABITTERPILL:
				{
					auto& gv = mission.addTarget(eTargetID::GalenVholes, "Galen Vholes", "gartersnake_ghalen_vholes.jpg");
					break;
				}
			case eMission::ISLEOFSGAIL_THEARKSOCIETY:
				{
					auto& knightsArmor = mission.getDisguiseByNameAssert("Knight's Armor");
					auto knightsArmorTrapTest = [&knightsArmor](const RouletteSpinCondition& cond){
						return &cond.disguise.get() == &knightsArmor && !cond.killMethod.isRemote;
					};

					auto& zw = mission.addTarget(eTargetID::ZoeWashington, "Zoe Washington", "magpie_zoe_washington.jpg");
					zw.defineMethod(eKillMethod::Drowning, { eMethodTag::BannedInRR, eMethodTag::Hard });
					zw.defineMethod(eKillMethod::ConsumedPoison, { eMethodTag::BannedInRR, eMethodTag::Impossible });
					zw.addRule(knightsArmorTrapTest, { eMethodTag::BannedInRR, eMethodTag::Extreme });

					auto& sw = mission.addTarget(eTargetID::SophiaWashington, "Sophia Washington", "magpie_serena_washington.jpg");
					sw.defineMethod(eKillMethod::Drowning, { eMethodTag::BannedInRR, eMethodTag::Hard });
					sw.defineMethod(eKillMethod::ConsumedPoison, { eMethodTag::BannedInRR, eMethodTag::Extreme });
					sw.defineMethod(eKillMethod::Fire, { eMethodTag::BannedInRR, eMethodTag::Hard });
					sw.addRule(knightsArmorTrapTest, { eMethodTag::BannedInRR, eMethodTag::Extreme });
					break;
				}
			case eMission::NEWYORK_GOLDENHANDSHAKE:
				{
					auto& as = mission.addTarget(eTargetID::AthenaSavalas, "Athena Savalas", "racoon_athena_savalas.jpg");
					as.defineMethod(eKillMethod::Fire, { eMethodTag::BannedInRR, eMethodTag::Impossible });
					break;
				}
			case eMission::HAVEN_THELASTRESORT:
				{
					auto& tw = mission.addTarget(eTargetID::TysonWilliams, "Tyson Williams", "stingray_tyson_williams.jpg");
					tw.defineMethod(eKillMethod::ConsumedPoison, { eMethodTag::BannedInRR, eMethodTag::Impossible });
					tw.defineMethod(eKillMethod::Fire, { eMethodTag::BannedInRR, eMethodTag::Impossible });

					auto& sb = mission.addTarget(eTargetID::StevenBradley, "Steven Bradley", "stingray_steven_bradley.jpg");
					sb.defineMethod(eKillMethod::ConsumedPoison, { eMethodTag::BannedInRR, eMethodTag::Impossible });
					sb.defineMethod(eKillMethod::Fire, { eMethodTag::BannedInRR, eMethodTag::Impossible });

					auto& lv = mission.addTarget(eTargetID::LjudmilaVetrova, "Ljudmila Vetrova", "stingray_ljudmila_vetrova.jpg");
					lv.defineMethod(eKillMethod::FallingObject, { eMethodTag::BannedInRR, eMethodTag::Impossible });
					lv.defineMethod(eKillMethod::Fire, { eMethodTag::BannedInRR, eMethodTag::Impossible });
					break;
				}
			case eMission::DUBAI_ONTOPOFTHEWORLD:
				{
					auto& ci = mission.addTarget(eTargetID::CarlIngram, "Carl Ingram", "golden_carl_ingram.jpg");
					ci.defineMethod(eKillMethod::ConsumedPoison, { eMethodTag::BannedInRR, eMethodTag::Buggy });
					ci.defineMethod(eKillMethod::Fire, { eMethodTag::BannedInRR, eMethodTag::Extreme });

					auto& ms = mission.addTarget(eTargetID::MarcusStuyvesant, "Marcus Stuyvesant", "golden_marcus_stuyvesant.jpg");
					ms.defineMethod(eKillMethod::ConsumedPoison, { eMethodTag::BannedInRR, eMethodTag::Extreme });
					ms.defineMethod(eKillMethod::Fire, { eMethodTag::BannedInRR, eMethodTag::Extreme });
					auto& skydivingSuit = mission.getDisguiseByNameAssert("Skydiving Suit");
					ms.addRule([&skydivingSuit](const RouletteSpinCondition& cond) {
						return &cond.disguise.get() == &skydivingSuit && cond.killMethod.method == eKillMethod::Drowning;
					}, { eMethodTag::BannedInRR, eMethodTag::Extreme });
					break;
				}
			case eMission::DARTMOOR_DEATHINTHEFAMILY:
				{
					auto& ac = mission.addTarget(eTargetID::AlexaCarlisle, "Alexa Carlisle", "ancestral_alexa_carlisle.jpg");
					ac.defineMethod(eKillMethod::Fire, { eMethodTag::BannedInRR, eMethodTag::Impossible });
					break;
				}
			case eMission::BERLIN_APEXPREDATOR:
				{
					auto constexpr image = "fox_pickup_earpiece.jpg";
					auto& a1 = mission.addTarget(eTargetID::Agent1, "ICA Agent #1", image);
					auto& a2 = mission.addTarget(eTargetID::Agent2, "ICA Agent #2", image);
					auto& a3 = mission.addTarget(eTargetID::Agent3, "ICA Agent #3", image);
					auto& a4 = mission.addTarget(eTargetID::Agent4, "ICA Agent #4", image);
					auto& a5 = mission.addTarget(eTargetID::Agent5, "ICA Agent #5", image);
					break;
				}
			case eMission::CHONGQING_ENDOFANERA:
				{
					auto& h = mission.addTarget(eTargetID::Hush, "Hush", "wet_hush.jpg");
					h.defineMethod(eKillMethod::ConsumedPoison, { eMethodTag::BannedInRR, eMethodTag::Impossible });
					h.defineMethod(eKillMethod::Fire, { eMethodTag::BannedInRR, eMethodTag::Extreme });
					h.addRule(loudElimTest, { eMethodTag::BannedInRR, eMethodTag::Extreme });

					auto& ir = mission.addTarget(eTargetID::ImogenRoyce, "Imogen Royce", "wet_imogen_royce.jpg");
					ir.defineMethod(eKillMethod::Fire, { eMethodTag::BannedInRR, eMethodTag::Hard });
					ir.defineMethod(eKillMethod::ConsumedPoison, { eMethodTag::BannedInRR, eMethodTag::Hard });
					ir.defineMethod(eKillMethod::FallingObject, { eMethodTag::BannedInRR, eMethodTag::Extreme });
					break;
				}
			case eMission::MENDOZA_THEFAREWELL:
				{
					auto& day = mission.addTarget(eTargetID::DonArchibaldYates, "Don Archibald Yates", "elegant_yates.jpg");
					day.defineMethod(eKillMethod::ConsumedPoison, { eMethodTag::BannedInRR, eMethodTag::Hard });
					day.defineMethod(eKillMethod::Fire, { eMethodTag::BannedInRR, eMethodTag::Extreme });

					auto& tv = mission.addTarget(eTargetID::TamaraVidal, "Tamara Vidal", "elegant_vidal.jpg");
					tv.defineMethod(eKillMethod::ConsumedPoison, { eMethodTag::BannedInRR, eMethodTag::Hard });
					tv.defineMethod(eKillMethod::FallingObject, { eMethodTag::BannedInRR, eMethodTag::Hard });
					tv.defineMethod(eKillMethod::Fire, { eMethodTag::BannedInRR, eMethodTag::Extreme });
					break;
				}
			case eMission::CARPATHIAN_UNTOUCHABLE:
				{
					auto& ae = mission.addTarget(eTargetID::ArthurEdwards, "Arthur Edwards", "trapped_arthur_edwards.jpg");
					ae.defineMethod(eKillMethod::Drowning, { eMethodTag::Impossible });
					ae.defineMethod(eKillMethod::ConsumedPoison, { eMethodTag::Impossible });
					ae.defineMethod(eKillMethod::Electrocution, { eMethodTag::Impossible });
					ae.defineMethod(eKillMethod::Explosion, { eMethodTag::Impossible });
					ae.defineMethod(eKillMethod::FallingObject, { eMethodTag::Impossible });
					ae.defineMethod(eKillMethod::Fire, { eMethodTag::Impossible });
					ae.defineMethod(eKillMethod::SMGElimination, { eMethodTag::Impossible });
					ae.defineMethod(eKillMethod::Sniper, { eMethodTag::Impossible });
					break;
				}
			case eMission::AMBROSE_SHADOWSINTHEWATER:
				{
					auto& nc = mission.addTarget(eTargetID::NoelCrest, "Noel Crest", "rocky_noel_crest.jpg");
					auto& sav = mission.addTarget(eTargetID::SinhiAkkaVenthan, "Sinhi \"Akka\" Venthan", "rocky_sinhi_akka_venthan.jpg");
					break;
				}
		}
	}
}

auto Missions::get(eMission id) -> const RouletteMission* {
	if (id == eMission::NONE) return nullptr;
	auto it = find_if(begin(data), end(data), [id](const RouletteMission& mission) {
		return mission.getMission() == id;
	});
	if (it == end(data)) return nullptr;
	return &*it;
}

auto RouletteMission::getTargetByName(std::string_view name) const -> const RouletteTarget* {
	auto it = find_if(cbegin(this->targets), cend(this->targets), [name](const RouletteTarget& target) {
		return target.getName() == name;
	});
	return it != cend(this->targets) ? &*it : nullptr;
}
