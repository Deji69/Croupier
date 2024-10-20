﻿using System;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Croupier {
	public enum MissionID {
		NONE = 0,
		ICAFACILITY_ARRIVAL,
		ICAFACILITY_GUIDED,
		ICAFACILITY_FREEFORM,
		ICAFACILITY_FINALTEST,
		PARIS_SHOWSTOPPER,
		PARIS_HOLIDAYHOARDERS,
		SAPIENZA_WORLDOFTOMORROW,
		SAPIENZA_THEICON,
		SAPIENZA_LANDSLIDE,
		SAPIENZA_THEAUTHOR,
		MARRAKESH_GILDEDCAGE,
		MARRAKESH_HOUSEBUILTONSAND,
		BANGKOK_CLUB27,
		BANGKOK_THESOURCE,
		COLORADO_FREEDOMFIGHTERS,
		//COLORADO_THEVECTOR,
		HOKKAIDO_SITUSINVERSUS,
		HOKKAIDO_SNOWFESTIVAL,
		HOKKAIDO_PATIENTZERO,
		HAWKESBAY_NIGHTCALL,
		MIAMI_FINISHLINE,
		MIAMI_ASILVERTONGUE,
		SANTAFORTUNA_THREEHEADEDSERPENT,
		SANTAFORTUNA_EMBRACEOFTHESERPENT,
		MUMBAI_CHASINGAGHOST,
		MUMBAI_ILLUSIONSOFGRANDEUR,
		WHITTLETON_ANOTHERLIFE,
		WHITTLETON_ABITTERPILL,
		ISLEOFSGAIL_THEARKSOCIETY,
		NEWYORK_GOLDENHANDSHAKE,
		HAVEN_THELASTRESORT,
		DUBAI_ONTOPOFTHEWORLD,
		DARTMOOR_DEATHINTHEFAMILY,
		BERLIN_APEXPREDATOR,
		CHONGQING_ENDOFANERA,
		MENDOZA_THEFAREWELL,
		CARPATHIAN_UNTOUCHABLE,
		AMBROSE_SHADOWSINTHEWATER,
	}

	static partial class MissionIDMethods {
		public static string GetKey(this MissionID missionID) {
			return missionID switch {
				MissionID.ICAFACILITY_FREEFORM => "POLARBEAR",
				MissionID.ICAFACILITY_FINALTEST => "GRADUATION",
				MissionID.PARIS_SHOWSTOPPER => "PEACOCK",
				MissionID.PARIS_HOLIDAYHOARDERS => "PARISNOEL",
				MissionID.SAPIENZA_WORLDOFTOMORROW => "OCTOPUS",
				MissionID.SAPIENZA_THEICON => "COPPERHEAD",
				MissionID.SAPIENZA_LANDSLIDE => "MAMBA",
				MissionID.SAPIENZA_THEAUTHOR => "LASTWORD",
				MissionID.MARRAKESH_GILDEDCAGE => "SPIDER",
				MissionID.MARRAKESH_HOUSEBUILTONSAND => "PYTHON",
				MissionID.BANGKOK_CLUB27 => "TIGER",
				MissionID.BANGKOK_THESOURCE => "TAMAGOZAKE",
				MissionID.COLORADO_FREEDOMFIGHTERS => "BULL",
				MissionID.HOKKAIDO_SITUSINVERSUS => "SNOWCRANE",
				MissionID.HOKKAIDO_SNOWFESTIVAL => "MAMUSHI",
				MissionID.HOKKAIDO_PATIENTZERO => "BRONX",
				MissionID.HAWKESBAY_NIGHTCALL => "SHEEP",
				MissionID.MIAMI_FINISHLINE => "FLAMINGO",
				MissionID.MIAMI_ASILVERTONGUE => "COTTONMOUTH",
				MissionID.SANTAFORTUNA_THREEHEADEDSERPENT => "HIPPO",
				MissionID.SANTAFORTUNA_EMBRACEOFTHESERPENT => "ANACONDA",
				MissionID.MUMBAI_CHASINGAGHOST => "MONGOOSE",
				MissionID.MUMBAI_ILLUSIONSOFGRANDEUR => "KINGCOBRA",
				MissionID.WHITTLETON_ANOTHERLIFE => "SKUNK",
				MissionID.WHITTLETON_ABITTERPILL => "GARTERSNAKE",
				MissionID.ISLEOFSGAIL_THEARKSOCIETY => "MAGPIE",
				MissionID.NEWYORK_GOLDENHANDSHAKE => "RACCOON",
				MissionID.HAVEN_THELASTRESORT => "STINGRAY",
				MissionID.DUBAI_ONTOPOFTHEWORLD => "GECKO",
				MissionID.DARTMOOR_DEATHINTHEFAMILY => "BULLDOG",
				MissionID.BERLIN_APEXPREDATOR => "FOX",
				MissionID.CHONGQING_ENDOFANERA => "RAT",
				MissionID.MENDOZA_THEFAREWELL => "LLAMA",
				MissionID.CARPATHIAN_UNTOUCHABLE => "WOLVERINE",
				MissionID.AMBROSE_SHADOWSINTHEWATER => "DUGONG",
				_ => "",
			};
		}

		public static MissionID FromKey(string key) {
			return CreateMissionNameTokenRegex().Replace(key.ToUpper(), "") switch {
				"POLARBEAR" => MissionID.ICAFACILITY_FREEFORM,
				"FREEFORM" => MissionID.ICAFACILITY_FREEFORM,
				"GRADUATION" => MissionID.ICAFACILITY_FINALTEST,
				"FINALTEST" => MissionID.ICAFACILITY_FINALTEST,
				"PEACOCK" => MissionID.PARIS_SHOWSTOPPER,
				"PARISNOEL" => MissionID.PARIS_HOLIDAYHOARDERS,
				"OCTOPUS" => MissionID.SAPIENZA_WORLDOFTOMORROW,
				"COPPERHEAD" => MissionID.SAPIENZA_THEICON,
				"MAMBA" => MissionID.SAPIENZA_LANDSLIDE,
				"LASTWORD" => MissionID.SAPIENZA_THEAUTHOR,
				"SPIDER" => MissionID.MARRAKESH_GILDEDCAGE,
				"PYTHON" => MissionID.MARRAKESH_HOUSEBUILTONSAND,
				"TIGER" => MissionID.BANGKOK_CLUB27,
				"TAMAGOZAKE" => MissionID.BANGKOK_THESOURCE,
				"BULL" => MissionID.COLORADO_FREEDOMFIGHTERS,
				"SNOWCRANE" => MissionID.HOKKAIDO_SITUSINVERSUS,
				"MAMUSHI" => MissionID.HOKKAIDO_SNOWFESTIVAL,
				"BRONX" => MissionID.HOKKAIDO_PATIENTZERO,
				"SHEEP" => MissionID.HAWKESBAY_NIGHTCALL,
				"FLAMINGO" => MissionID.MIAMI_FINISHLINE,
				"COTTONMOUTH" => MissionID.MIAMI_ASILVERTONGUE,
				"HIPPO" => MissionID.SANTAFORTUNA_THREEHEADEDSERPENT,
				"ANACONDA" => MissionID.SANTAFORTUNA_EMBRACEOFTHESERPENT,
				"MONGOOSE" => MissionID.MUMBAI_CHASINGAGHOST,
				"KINGCOBRA" => MissionID.MUMBAI_ILLUSIONSOFGRANDEUR,
				"SKUNK" => MissionID.WHITTLETON_ANOTHERLIFE,
				"GARTERSNAKE" => MissionID.WHITTLETON_ABITTERPILL,
				"MAGPIE" => MissionID.ISLEOFSGAIL_THEARKSOCIETY,
				"RACCOON" => MissionID.NEWYORK_GOLDENHANDSHAKE,
				"STINGRAY" => MissionID.HAVEN_THELASTRESORT,
				"GECKO" => MissionID.DUBAI_ONTOPOFTHEWORLD,
				"BULLDOG" => MissionID.DARTMOOR_DEATHINTHEFAMILY,
				"FOX" => MissionID.BERLIN_APEXPREDATOR,
				"RAT" => MissionID.CHONGQING_ENDOFANERA,
				"LLAMA" => MissionID.MENDOZA_THEFAREWELL,
				"WOLVERINE" => MissionID.CARPATHIAN_UNTOUCHABLE,
				"DUGONG" => MissionID.AMBROSE_SHADOWSINTHEWATER,
				_ => MissionID.NONE,
			};
		}

		public static MissionID FromName(string name) {
			var id = CreateMissionNameTokenRegex().Replace(name.ToLower(), "") switch {
				"arrival" => MissionID.ICAFACILITY_ARRIVAL,
				"guidedtraining" => MissionID.ICAFACILITY_GUIDED,
				"freeformtraining" => MissionID.ICAFACILITY_FREEFORM,
				"thefinaltest" => MissionID.ICAFACILITY_FINALTEST,
				"theshowstopper" => MissionID.PARIS_SHOWSTOPPER,
				"holidayhoarders" => MissionID.PARIS_HOLIDAYHOARDERS,
				"worldoftomorrow" => MissionID.SAPIENZA_WORLDOFTOMORROW,
				"theicon" => MissionID.SAPIENZA_THEICON,
				"landslide" => MissionID.SAPIENZA_LANDSLIDE,
				"theauthor" => MissionID.SAPIENZA_THEAUTHOR,
				"agildedcage" => MissionID.MARRAKESH_GILDEDCAGE,
				"ahousebuiltonsand" => MissionID.MARRAKESH_HOUSEBUILTONSAND,
				"club27" => MissionID.BANGKOK_CLUB27,
				"thesource" => MissionID.BANGKOK_THESOURCE,
				"freedomfighters" => MissionID.COLORADO_FREEDOMFIGHTERS,
				"situsinversus" => MissionID.HOKKAIDO_SITUSINVERSUS,
				"snowfestival" => MissionID.HOKKAIDO_SNOWFESTIVAL,
				"hokkaidosnowfestival" => MissionID.HOKKAIDO_SNOWFESTIVAL,
				"patientzero" => MissionID.HOKKAIDO_PATIENTZERO,
				"nightcall" => MissionID.HAWKESBAY_NIGHTCALL,
				"thefinishline" => MissionID.MIAMI_FINISHLINE,
				"asilvertongue" => MissionID.MIAMI_ASILVERTONGUE,
				"threeheadedserpent" => MissionID.SANTAFORTUNA_THREEHEADEDSERPENT,
				"embraceoftheserpent" => MissionID.SANTAFORTUNA_EMBRACEOFTHESERPENT,
				"chasingaghost" => MissionID.MUMBAI_CHASINGAGHOST,
				"illusionsofgrandeur" => MissionID.MUMBAI_ILLUSIONSOFGRANDEUR,
				"anotherlife" => MissionID.WHITTLETON_ANOTHERLIFE,
				"abitterpill" => MissionID.WHITTLETON_ABITTERPILL,
				"thearksociety" => MissionID.ISLEOFSGAIL_THEARKSOCIETY,
				"goldenhandshake" => MissionID.NEWYORK_GOLDENHANDSHAKE,
				"thelastresort" => MissionID.HAVEN_THELASTRESORT,
				"ontopoftheworld" => MissionID.DUBAI_ONTOPOFTHEWORLD,
				"deathinthefamily" => MissionID.DARTMOOR_DEATHINTHEFAMILY,
				"apexpredator" => MissionID.BERLIN_APEXPREDATOR,
				"endofanera" => MissionID.CHONGQING_ENDOFANERA,
				"thefarewell" => MissionID.MENDOZA_THEFAREWELL,
				"untouchable" => MissionID.CARPATHIAN_UNTOUCHABLE,
				"shadowsinthewater" => MissionID.AMBROSE_SHADOWSINTHEWATER,
				//"freelancer" => MissionID.FREELANCER,
				_ => MissionID.NONE,
			};
			return id != MissionID.NONE ? id : FromKey(name);
		}

		public static bool IsMajorMap(this MissionID missionID) {
			return missionID switch {
				MissionID.AMBROSE_SHADOWSINTHEWATER or
				MissionID.BANGKOK_CLUB27 or
				MissionID.BERLIN_APEXPREDATOR or
				MissionID.CHONGQING_ENDOFANERA or
				MissionID.COLORADO_FREEDOMFIGHTERS or
				MissionID.DARTMOOR_DEATHINTHEFAMILY or
				MissionID.DUBAI_ONTOPOFTHEWORLD or
				MissionID.HAVEN_THELASTRESORT or
				MissionID.HOKKAIDO_SITUSINVERSUS or
				MissionID.ISLEOFSGAIL_THEARKSOCIETY or
				MissionID.MARRAKESH_GILDEDCAGE or
				MissionID.MENDOZA_THEFAREWELL or
				MissionID.MIAMI_FINISHLINE or
				MissionID.MUMBAI_CHASINGAGHOST or
				MissionID.NEWYORK_GOLDENHANDSHAKE or
				MissionID.PARIS_SHOWSTOPPER or
				MissionID.SANTAFORTUNA_THREEHEADEDSERPENT or
				MissionID.SAPIENZA_WORLDOFTOMORROW or
				MissionID.WHITTLETON_ANOTHERLIFE => true,
				_ => false,
			};
		}

		public static MissionGroup GetGroup(this MissionID m) {
			return m switch {
				MissionID.ICAFACILITY_ARRIVAL or MissionID.ICAFACILITY_GUIDED or
				MissionID.ICAFACILITY_FREEFORM or MissionID.ICAFACILITY_FINALTEST => MissionGroup.Prologue,
				MissionID.PARIS_SHOWSTOPPER or MissionID.SAPIENZA_WORLDOFTOMORROW or
				MissionID.MARRAKESH_GILDEDCAGE or MissionID.BANGKOK_CLUB27 or
				MissionID.COLORADO_FREEDOMFIGHTERS or MissionID.HOKKAIDO_SITUSINVERSUS => MissionGroup.Season1,
				MissionID.PARIS_HOLIDAYHOARDERS or
				MissionID.SAPIENZA_THEICON or
				MissionID.SAPIENZA_LANDSLIDE or
				MissionID.MARRAKESH_HOUSEBUILTONSAND or
				MissionID.BANGKOK_THESOURCE or MissionID.SAPIENZA_THEAUTHOR or MissionID.HOKKAIDO_PATIENTZERO or
				MissionID.HOKKAIDO_SNOWFESTIVAL => MissionGroup.Season1Bonus,
				MissionID.HAWKESBAY_NIGHTCALL or
				MissionID.MIAMI_FINISHLINE or MissionID.SANTAFORTUNA_THREEHEADEDSERPENT or
				MissionID.MUMBAI_CHASINGAGHOST or MissionID.WHITTLETON_ANOTHERLIFE or
				MissionID.AMBROSE_SHADOWSINTHEWATER or MissionID.ISLEOFSGAIL_THEARKSOCIETY or
				MissionID.NEWYORK_GOLDENHANDSHAKE or MissionID.HAVEN_THELASTRESORT => MissionGroup.Season2,
				MissionID.MIAMI_ASILVERTONGUE or
				MissionID.SANTAFORTUNA_EMBRACEOFTHESERPENT or
				MissionID.MUMBAI_ILLUSIONSOFGRANDEUR or
				MissionID.WHITTLETON_ABITTERPILL => MissionGroup.SpecialAssignments,
				MissionID.DUBAI_ONTOPOFTHEWORLD or MissionID.DARTMOOR_DEATHINTHEFAMILY or
				MissionID.BERLIN_APEXPREDATOR or MissionID.CHONGQING_ENDOFANERA or
				MissionID.MENDOZA_THEFAREWELL or MissionID.CARPATHIAN_UNTOUCHABLE => MissionGroup.Season3,
				_ => MissionGroup.None,
			};
		}

		public static int GetGroupOrder(this MissionID m) {
			return m.GetGroup() switch {
				MissionGroup.None => 0,
				MissionGroup.Prologue => m switch {
					MissionID.ICAFACILITY_ARRIVAL => 1,
					MissionID.ICAFACILITY_GUIDED => 2,
					MissionID.ICAFACILITY_FREEFORM => 3,
					MissionID.ICAFACILITY_FINALTEST => 4,
					_ => 5,
				},
				MissionGroup.Season1 => 100 + m switch {
					MissionID.PARIS_SHOWSTOPPER => 1,
					MissionID.SAPIENZA_WORLDOFTOMORROW => 2,
					MissionID.MARRAKESH_GILDEDCAGE => 3,
					MissionID.BANGKOK_CLUB27 => 4,
					MissionID.COLORADO_FREEDOMFIGHTERS => 5,
					MissionID.HOKKAIDO_SITUSINVERSUS => 6,
					_ => 7,
				},
				MissionGroup.Season1Bonus => 200 + m switch {
					MissionID.PARIS_HOLIDAYHOARDERS => 1,
					MissionID.SAPIENZA_THEICON => 2,
					MissionID.MARRAKESH_HOUSEBUILTONSAND => 3,
					MissionID.SAPIENZA_LANDSLIDE => 4,
					MissionID.BANGKOK_THESOURCE => 5,
					MissionID.SAPIENZA_THEAUTHOR => 6,
					MissionID.HOKKAIDO_PATIENTZERO => 7,
					MissionID.HOKKAIDO_SNOWFESTIVAL => 8,
					_ => 9,
				},
				MissionGroup.Season2 => 300 + m switch {
					MissionID.HAWKESBAY_NIGHTCALL => 1,
					MissionID.MIAMI_FINISHLINE => 2,
					MissionID.SANTAFORTUNA_THREEHEADEDSERPENT => 3,
					MissionID.MUMBAI_CHASINGAGHOST => 4,
					MissionID.WHITTLETON_ANOTHERLIFE => 5,
					MissionID.AMBROSE_SHADOWSINTHEWATER => 6,
					MissionID.ISLEOFSGAIL_THEARKSOCIETY => 7,
					MissionID.NEWYORK_GOLDENHANDSHAKE => 8,
					MissionID.HAVEN_THELASTRESORT => 9,
					_ => 10,
				},
				MissionGroup.SpecialAssignments => 400 + m switch {
					MissionID.MIAMI_ASILVERTONGUE => 1,
					MissionID.SANTAFORTUNA_EMBRACEOFTHESERPENT => 2,
					MissionID.MUMBAI_ILLUSIONSOFGRANDEUR => 3,
					MissionID.WHITTLETON_ABITTERPILL => 4,
					_ => 5,
				},
				MissionGroup.Season3 => 500 + m switch {
					MissionID.DUBAI_ONTOPOFTHEWORLD => 1,
					MissionID.DARTMOOR_DEATHINTHEFAMILY => 2,
					MissionID.BERLIN_APEXPREDATOR => 3,
					MissionID.CHONGQING_ENDOFANERA => 4,
					MissionID.MENDOZA_THEFAREWELL => 5,
					MissionID.CARPATHIAN_UNTOUCHABLE => 6,
					_ => 7,
				},
				_ => 9999,
			};
		}

		[GeneratedRegex("[^a-zA-Z0-9]")]
		private static partial Regex CreateMissionNameTokenRegex();
	}
}