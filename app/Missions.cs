using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Croupier
{
	public enum MissionGroup {
		None,
		Prologue,
		Season1,
		Season1Bonus,
		Season2,
		SpecialAssignments,
		Season3,
	}

	static class MissionGroupMethods {
		public static string GetName(this MissionGroup group) {
			return group switch {
				MissionGroup.Prologue => "Prologue",
				MissionGroup.Season1 => "Season 1",
				MissionGroup.Season1Bonus => "Season 1 Bonus",
				MissionGroup.Season2 => "Season 2",
				MissionGroup.SpecialAssignments => "Special Assignments",
				MissionGroup.Season3 => "Season 3",
				_ => "",
			};
		}
	}

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

	static class MissionIDMethods {
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
			return key.ToUpper() switch {
				"POLARBEAR" => MissionID.ICAFACILITY_FREEFORM,
				"GRADUATION" => MissionID.ICAFACILITY_FINALTEST,
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
	}

	public enum MethodTag {
		BannedInRR,
		Impossible,
		Hard,
		Extreme,
		Buggy,

		DuplicateOnlySameDisguise,
		LoudOnly,

		IsGun,
		IsElimination,
		IsLarge,
		IsRemote,
	}

	public partial class Mission(MissionID mission) {
		private static readonly Regex TokenCharacterRegex = CreateTokenCharacterRegex();

		public readonly List<Target> Targets = MakeTargetList(mission);

		public MissionID ID { get; set; } = mission;
		public string Name {
			get {
				return ID switch {
					MissionID.ICAFACILITY_ARRIVAL => "Arrival",
					MissionID.ICAFACILITY_GUIDED => "Guided Training",
					MissionID.ICAFACILITY_FREEFORM => "Freeform Training",
					MissionID.ICAFACILITY_FINALTEST => "The Final Test",
					MissionID.PARIS_SHOWSTOPPER => "The Showstopper",
					MissionID.SAPIENZA_WORLDOFTOMORROW => "World of Tomorrow",
					MissionID.MARRAKESH_GILDEDCAGE => "A Gilded Cage",
					MissionID.BANGKOK_CLUB27 => "Club 27",
					MissionID.COLORADO_FREEDOMFIGHTERS => "Freedom Fighters",
					MissionID.HOKKAIDO_SITUSINVERSUS => "Situs Inversus",
					MissionID.BANGKOK_THESOURCE => "The Source",
					MissionID.SAPIENZA_THEAUTHOR => "The Author",
					MissionID.HOKKAIDO_PATIENTZERO => "Patient Zero",
					MissionID.PARIS_HOLIDAYHOARDERS => "Holiday Hoarders",
					MissionID.SAPIENZA_THEICON => "The Icon",
					MissionID.SAPIENZA_LANDSLIDE => "Landslide",
					MissionID.MARRAKESH_HOUSEBUILTONSAND => "A House Built On Sand",
					MissionID.HOKKAIDO_SNOWFESTIVAL => "Snow Festival",
					MissionID.HAWKESBAY_NIGHTCALL => "Nightcall",
					MissionID.MIAMI_FINISHLINE => "The Finish Line",
					MissionID.SANTAFORTUNA_THREEHEADEDSERPENT => "Three-Headed Serpent",
					MissionID.MUMBAI_CHASINGAGHOST => "Chasing a Ghost",
					MissionID.WHITTLETON_ANOTHERLIFE => "Another Life",
					MissionID.ISLEOFSGAIL_THEARKSOCIETY => "The Ark Society",
					MissionID.NEWYORK_GOLDENHANDSHAKE => "Golden Handshake",
					MissionID.HAVEN_THELASTRESORT => "The Last Resort",
					MissionID.MIAMI_ASILVERTONGUE => "A Silver Tongue",
					MissionID.SANTAFORTUNA_EMBRACEOFTHESERPENT => "Embrace of the Serpent",
					MissionID.MUMBAI_ILLUSIONSOFGRANDEUR => "Illusions of Grandeur",
					MissionID.WHITTLETON_ABITTERPILL => "A Bitter Pill",
					MissionID.DUBAI_ONTOPOFTHEWORLD => "On Top of the World",
					MissionID.DARTMOOR_DEATHINTHEFAMILY => "Death in the Family",
					MissionID.BERLIN_APEXPREDATOR => "Apex Predator",
					MissionID.CHONGQING_ENDOFANERA => "End of an Era",
					MissionID.MENDOZA_THEFAREWELL => "The Farewell",
					MissionID.CARPATHIAN_UNTOUCHABLE => "Untouchable",
					MissionID.AMBROSE_SHADOWSINTHEWATER => "Shadows in the Water",
					_ => "",
				};
			}
		}
		public string Location {
			get {
				return ID switch {
					MissionID.ICAFACILITY_ARRIVAL
					or MissionID.ICAFACILITY_GUIDED
					or MissionID.ICAFACILITY_FREEFORM
					or MissionID.ICAFACILITY_FINALTEST => "Prologue",
					MissionID.PARIS_SHOWSTOPPER
					or MissionID.PARIS_HOLIDAYHOARDERS => "Paris",
					MissionID.SAPIENZA_WORLDOFTOMORROW
					or MissionID.SAPIENZA_THEAUTHOR
					or MissionID.SAPIENZA_THEICON
					or MissionID.SAPIENZA_LANDSLIDE => "Sapienza",
					MissionID.MARRAKESH_GILDEDCAGE
					or MissionID.MARRAKESH_HOUSEBUILTONSAND => "Marrakesh",
					MissionID.BANGKOK_CLUB27
					or MissionID.BANGKOK_THESOURCE => "Bangkok",
					MissionID.COLORADO_FREEDOMFIGHTERS => "Colorado",
					MissionID.HOKKAIDO_SITUSINVERSUS
					or MissionID.HOKKAIDO_PATIENTZERO
					or MissionID.HOKKAIDO_SNOWFESTIVAL => "Hokkaido",
					MissionID.HAWKESBAY_NIGHTCALL => "Hawke's Bay",
					MissionID.MIAMI_FINISHLINE
					or MissionID.MIAMI_ASILVERTONGUE => "Miami",
					MissionID.SANTAFORTUNA_THREEHEADEDSERPENT
					or MissionID.SANTAFORTUNA_EMBRACEOFTHESERPENT => "Santa Fortuna",
					MissionID.MUMBAI_CHASINGAGHOST
					or MissionID.MUMBAI_ILLUSIONSOFGRANDEUR => "Mumbai",
					MissionID.WHITTLETON_ANOTHERLIFE
					or MissionID.WHITTLETON_ABITTERPILL => "Whittleton Creek",
					MissionID.ISLEOFSGAIL_THEARKSOCIETY => "Isle of Sgàil",
					MissionID.NEWYORK_GOLDENHANDSHAKE => "New York",
					MissionID.HAVEN_THELASTRESORT => "Haven",
					MissionID.DUBAI_ONTOPOFTHEWORLD => "Dubai",
					MissionID.DARTMOOR_DEATHINTHEFAMILY => "Dartmoor",
					MissionID.BERLIN_APEXPREDATOR => "Berlin",
					MissionID.CHONGQING_ENDOFANERA => "Chongqing",
					MissionID.MENDOZA_THEFAREWELL => "Mendoza",
					MissionID.CARPATHIAN_UNTOUCHABLE => "Carpathian Mountains",
					MissionID.AMBROSE_SHADOWSINTHEWATER => "Ambrose Island",
					_ => "",
				};
			}
		}
		public string Image {
			get {
				return ID switch {
					MissionID.ICAFACILITY_ARRIVAL => "polarbear-arrival.jpg",
					MissionID.ICAFACILITY_GUIDED or MissionID.ICAFACILITY_FREEFORM => "ica-facility-freeform-training.jpg",
					MissionID.ICAFACILITY_FINALTEST => "ica-facility-the-final-test.jpg",
					MissionID.PARIS_SHOWSTOPPER => "paris-the-showstopper.jpg",
					MissionID.PARIS_HOLIDAYHOARDERS => "paris-holiday-hoarders.jpg",
					MissionID.SAPIENZA_WORLDOFTOMORROW => "sapienza-world-of-tomorrow.jpg",
					MissionID.SAPIENZA_LANDSLIDE => "sapienza-landslide.jpg",
					MissionID.SAPIENZA_THEICON => "sapienza-the-icon.jpg",
					MissionID.SAPIENZA_THEAUTHOR => "whitespider-ebola.jpg",
					MissionID.MARRAKESH_GILDEDCAGE => "marrakesh-a-gilded-cage.jpg",
					MissionID.MARRAKESH_HOUSEBUILTONSAND => "marrakesh-a-house-built-on-sand.jpg",
					MissionID.BANGKOK_CLUB27 => "bangkok-club27.jpg",
					MissionID.BANGKOK_THESOURCE => "whitespider-zika.jpg",
					MissionID.COLORADO_FREEDOMFIGHTERS => "colorado-freedom-fighters.jpg",
					MissionID.HOKKAIDO_SITUSINVERSUS => "hokkaido-situs-inversus.jpg",
					MissionID.HOKKAIDO_SNOWFESTIVAL => "hokkaido-snow-festival.jpg",
					MissionID.HOKKAIDO_PATIENTZERO => "whitespider-flu.jpg",
					MissionID.HAWKESBAY_NIGHTCALL => "hawkes-bay-nightcall.jpg",
					MissionID.MIAMI_FINISHLINE => "miami-the-finish-line.jpg",
					MissionID.MIAMI_ASILVERTONGUE => "cottonmouth.jpg",
					MissionID.SANTAFORTUNA_THREEHEADEDSERPENT => "santa-fortuna-three-headed-serpent.jpg",
					MissionID.SANTAFORTUNA_EMBRACEOFTHESERPENT => "anaconda.jpg",
					MissionID.MUMBAI_CHASINGAGHOST => "mumbai-chasing-a-ghost.jpg",
					MissionID.MUMBAI_ILLUSIONSOFGRANDEUR => "mumbai-illusions-of-grandeur.jpg",
					MissionID.WHITTLETON_ANOTHERLIFE => "whittleton-creek-another-life.jpg",
					MissionID.WHITTLETON_ABITTERPILL => "whittleton-creek-a-bitter-pill.jpg",
					MissionID.ISLEOFSGAIL_THEARKSOCIETY => "isle-of-sgail-the-ark-society.jpg",
					MissionID.NEWYORK_GOLDENHANDSHAKE => "new-york-golden-handshake.jpg",
					MissionID.HAVEN_THELASTRESORT => "haven-the-last-resort.jpg",
					MissionID.DUBAI_ONTOPOFTHEWORLD => "dubai-on-top-of-the-world.jpg",
					MissionID.DARTMOOR_DEATHINTHEFAMILY => "dartmoor-death-in-the-family.jpg",
					MissionID.BERLIN_APEXPREDATOR => "berlin-apex-predator.jpg",
					MissionID.AMBROSE_SHADOWSINTHEWATER => "ambrose-island-shadows-on-the-water.jpg",
					MissionID.CHONGQING_ENDOFANERA => "chongqing-end-of-an-era.jpg",
					MissionID.MENDOZA_THEFAREWELL => "mendoza-the-farewell.jpg",
					MissionID.CARPATHIAN_UNTOUCHABLE => "carpathian-mountains-untouchable.jpg",
					_ => "",
				};
			}
		}
		public bool IsMainMap {
			get {
				return ID switch {
					MissionID.PARIS_SHOWSTOPPER
					or MissionID.SAPIENZA_WORLDOFTOMORROW
					or MissionID.MARRAKESH_GILDEDCAGE
					or MissionID.BANGKOK_CLUB27
					or MissionID.COLORADO_FREEDOMFIGHTERS
					or MissionID.HOKKAIDO_SITUSINVERSUS
					or MissionID.MIAMI_FINISHLINE
					or MissionID.SANTAFORTUNA_THREEHEADEDSERPENT
					or MissionID.MUMBAI_CHASINGAGHOST
					or MissionID.WHITTLETON_ANOTHERLIFE
					or MissionID.ISLEOFSGAIL_THEARKSOCIETY
					or MissionID.NEWYORK_GOLDENHANDSHAKE
					or MissionID.HAVEN_THELASTRESORT
					or MissionID.DUBAI_ONTOPOFTHEWORLD
					or MissionID.DARTMOOR_DEATHINTHEFAMILY
					or MissionID.BERLIN_APEXPREDATOR
					or MissionID.CHONGQING_ENDOFANERA
					or MissionID.MENDOZA_THEFAREWELL
					or MissionID.AMBROSE_SHADOWSINTHEWATER => true,
					_ => false,
				};
			}
		}
		public MissionGroup Group {
			get {
				switch (ID) {
					case MissionID.ICAFACILITY_ARRIVAL:
					case MissionID.ICAFACILITY_GUIDED:
					case MissionID.ICAFACILITY_FREEFORM:
					case MissionID.ICAFACILITY_FINALTEST:
						return MissionGroup.Prologue;
					case MissionID.PARIS_SHOWSTOPPER:
					case MissionID.SAPIENZA_WORLDOFTOMORROW:
					case MissionID.MARRAKESH_GILDEDCAGE:
					case MissionID.BANGKOK_CLUB27:
					case MissionID.COLORADO_FREEDOMFIGHTERS:
					case MissionID.HOKKAIDO_SITUSINVERSUS:
						return MissionGroup.Season1;
					case MissionID.BANGKOK_THESOURCE:
					case MissionID.SAPIENZA_THEAUTHOR:
					case MissionID.HOKKAIDO_PATIENTZERO:
					case MissionID.PARIS_HOLIDAYHOARDERS:
					case MissionID.SAPIENZA_THEICON:
					case MissionID.SAPIENZA_LANDSLIDE:
					case MissionID.MARRAKESH_HOUSEBUILTONSAND:
					case MissionID.HOKKAIDO_SNOWFESTIVAL:
						return MissionGroup.Season1Bonus;
					case MissionID.HAWKESBAY_NIGHTCALL:
					case MissionID.MIAMI_FINISHLINE:
					case MissionID.SANTAFORTUNA_THREEHEADEDSERPENT:
					case MissionID.MUMBAI_CHASINGAGHOST:
					case MissionID.WHITTLETON_ANOTHERLIFE:
					case MissionID.ISLEOFSGAIL_THEARKSOCIETY:
					case MissionID.NEWYORK_GOLDENHANDSHAKE:
					case MissionID.HAVEN_THELASTRESORT:
						return MissionGroup.Season2;
					case MissionID.MIAMI_ASILVERTONGUE:
					case MissionID.SANTAFORTUNA_EMBRACEOFTHESERPENT:
					case MissionID.MUMBAI_ILLUSIONSOFGRANDEUR:
					case MissionID.WHITTLETON_ABITTERPILL:
						return MissionGroup.SpecialAssignments;
					case MissionID.DUBAI_ONTOPOFTHEWORLD:
					case MissionID.DARTMOOR_DEATHINTHEFAMILY:
					case MissionID.BERLIN_APEXPREDATOR:
					case MissionID.CHONGQING_ENDOFANERA:
					case MissionID.MENDOZA_THEFAREWELL:
					case MissionID.CARPATHIAN_UNTOUCHABLE:
					case MissionID.AMBROSE_SHADOWSINTHEWATER:
						return MissionGroup.Season3;
				}
				return MissionGroup.None;
			}
		}
		public Uri ImagePath {
			get {
				return new Uri(Path.Combine(Environment.CurrentDirectory, "missions", this.Image));
			}
		}

		public List<Disguise> Disguises {
			get {
				return GetDisguises(ID);
			}
		}

		public List<SpecificKillMethod> Methods {
			get {
				List<SpecificKillMethod> methods = [];
				switch (ID) {
					case MissionID.ICAFACILITY_GUIDED:
					case MissionID.ICAFACILITY_FREEFORM:
						break;
					case MissionID.ICAFACILITY_FINALTEST:
						break;
					case MissionID.PARIS_HOLIDAYHOARDERS:
						methods.Add(SpecificKillMethod.CircumcisionKnife);
						methods.Add(SpecificKillMethod.HolidayFireAxe);
						methods.Add(SpecificKillMethod.Katana);
						methods.Add(SpecificKillMethod.Shuriken);
						methods.Add(SpecificKillMethod.XmasStar);
						goto case MissionID.PARIS_SHOWSTOPPER;
					case MissionID.PARIS_SHOWSTOPPER:
						methods.Add(SpecificKillMethod.BattleAxe);
						methods.Add(SpecificKillMethod.Cleaver);
						methods.Add(SpecificKillMethod.FireAxe);
						methods.Add(SpecificKillMethod.Hatchet);
						methods.Add(SpecificKillMethod.KitchenKnife);
						methods.Add(SpecificKillMethod.LetterOpener);
						methods.Add(SpecificKillMethod.Saber);
						methods.Add(SpecificKillMethod.Scissors);
						methods.Add(SpecificKillMethod.Screwdriver);
						break;
					case MissionID.SAPIENZA_WORLDOFTOMORROW:
						methods.Add(SpecificKillMethod.AmputationKnife);
						methods.Add(SpecificKillMethod.BattleAxe);
						methods.Add(SpecificKillMethod.CircumcisionKnife);
						methods.Add(SpecificKillMethod.Cleaver);
						methods.Add(SpecificKillMethod.CombatKnife);
						methods.Add(SpecificKillMethod.FireAxe);
						methods.Add(SpecificKillMethod.FoldingKnife);
						methods.Add(SpecificKillMethod.Hatchet);
						methods.Add(SpecificKillMethod.Katana);
						methods.Add(SpecificKillMethod.KitchenKnife);
						methods.Add(SpecificKillMethod.LetterOpener);
						methods.Add(SpecificKillMethod.OldAxe);
						methods.Add(SpecificKillMethod.Saber);
						methods.Add(SpecificKillMethod.Scissors);
						methods.Add(SpecificKillMethod.Screwdriver);
						break;
					case MissionID.SAPIENZA_LANDSLIDE:
						methods.Add(SpecificKillMethod.Cleaver);
						methods.Add(SpecificKillMethod.FireAxe);
						methods.Add(SpecificKillMethod.FoldingKnife);
						methods.Add(SpecificKillMethod.KitchenKnife);
						methods.Add(SpecificKillMethod.LetterOpener);
						methods.Add(SpecificKillMethod.OldAxe);
						methods.Add(SpecificKillMethod.Saber);
						methods.Add(SpecificKillMethod.Scissors);
						methods.Add(SpecificKillMethod.Screwdriver);
						break;
					case MissionID.SAPIENZA_THEICON:
						methods.Add(SpecificKillMethod.BattleAxe);
						methods.Add(SpecificKillMethod.Cleaver);
						methods.Add(SpecificKillMethod.FireAxe);
						methods.Add(SpecificKillMethod.KitchenKnife);
						methods.Add(SpecificKillMethod.Screwdriver);
						break;
					case MissionID.SAPIENZA_THEAUTHOR:
						methods.Add(SpecificKillMethod.FireAxe);
						methods.Add(SpecificKillMethod.KitchenKnife);
						methods.Add(SpecificKillMethod.Scissors);
						methods.Add(SpecificKillMethod.Screwdriver);
						break;
					case MissionID.MARRAKESH_GILDEDCAGE:
						methods.Add(SpecificKillMethod.BattleAxe);
						methods.Add(SpecificKillMethod.Cleaver);
						methods.Add(SpecificKillMethod.FireAxe);
						methods.Add(SpecificKillMethod.KitchenKnife);
						methods.Add(SpecificKillMethod.LetterOpener);
						methods.Add(SpecificKillMethod.Saber);
						methods.Add(SpecificKillMethod.Scissors);
						methods.Add(SpecificKillMethod.Screwdriver);
						break;
					case MissionID.MARRAKESH_HOUSEBUILTONSAND:
						methods.Add(SpecificKillMethod.BattleAxe);
						methods.Add(SpecificKillMethod.Cleaver);
						methods.Add(SpecificKillMethod.FoldingKnife);
						methods.Add(SpecificKillMethod.KitchenKnife);
						methods.Add(SpecificKillMethod.Scissors);
						methods.Add(SpecificKillMethod.Screwdriver);
						break;
					case MissionID.BANGKOK_THESOURCE:
						methods.Add(SpecificKillMethod.AmputationKnife);
						methods.Add(SpecificKillMethod.CircumcisionKnife);
						goto case MissionID.BANGKOK_CLUB27;
					case MissionID.BANGKOK_CLUB27:
						methods.Add(SpecificKillMethod.Cleaver);
						methods.Add(SpecificKillMethod.FireAxe);
						methods.Add(SpecificKillMethod.Hatchet);
						methods.Add(SpecificKillMethod.Katana);
						methods.Add(SpecificKillMethod.KitchenKnife);
						methods.Add(SpecificKillMethod.LetterOpener);
						methods.Add(SpecificKillMethod.SappersAxe);
						methods.Add(SpecificKillMethod.Screwdriver);
						break;
					case MissionID.COLORADO_FREEDOMFIGHTERS:
						methods.Add(SpecificKillMethod.Cleaver);
						methods.Add(SpecificKillMethod.KitchenKnife);
						methods.Add(SpecificKillMethod.OldAxe);
						methods.Add(SpecificKillMethod.Screwdriver);
						break;
					case MissionID.HOKKAIDO_SNOWFESTIVAL:
						methods.Add(SpecificKillMethod.Icicle);
						goto case MissionID.HOKKAIDO_PATIENTZERO;
					case MissionID.HOKKAIDO_PATIENTZERO:
					case MissionID.HOKKAIDO_SITUSINVERSUS:
						methods.Add(SpecificKillMethod.Cleaver);
						methods.Add(SpecificKillMethod.FireAxe);
						methods.Add(SpecificKillMethod.Katana);
						methods.Add(SpecificKillMethod.KitchenKnife);
						methods.Add(SpecificKillMethod.Scalpel);
						methods.Add(SpecificKillMethod.Scissors);
						methods.Add(SpecificKillMethod.Screwdriver);
						break;
					case MissionID.MIAMI_FINISHLINE:
					case MissionID.MIAMI_ASILVERTONGUE:
						methods.Add(SpecificKillMethod.AmputationKnife);
						methods.Add(SpecificKillMethod.BattleAxe);
						methods.Add(SpecificKillMethod.Cleaver);
						methods.Add(SpecificKillMethod.FireAxe);
						methods.Add(SpecificKillMethod.KitchenKnife);
						methods.Add(SpecificKillMethod.OldAxe);
						methods.Add(SpecificKillMethod.Scissors);
						methods.Add(SpecificKillMethod.Screwdriver);
						methods.Add(SpecificKillMethod.Starfish);
						break;
					case MissionID.SANTAFORTUNA_THREEHEADEDSERPENT:
					case MissionID.SANTAFORTUNA_EMBRACEOFTHESERPENT:
						methods.Add(SpecificKillMethod.BarberRazor);
						methods.Add(SpecificKillMethod.Cleaver);
						methods.Add(SpecificKillMethod.FoldingKnife);
						methods.Add(SpecificKillMethod.KitchenKnife);
						methods.Add(SpecificKillMethod.LetterOpener);
						methods.Add(SpecificKillMethod.Machete);
						methods.Add(SpecificKillMethod.SacrificialKnife);
						methods.Add(SpecificKillMethod.Scissors);
						methods.Add(SpecificKillMethod.Screwdriver);
						break;
					case MissionID.MUMBAI_CHASINGAGHOST:
					case MissionID.MUMBAI_ILLUSIONSOFGRANDEUR:
						methods.Add(SpecificKillMethod.AmputationKnife);
						methods.Add(SpecificKillMethod.BarberRazor);
						methods.Add(SpecificKillMethod.BeakStaff);
						methods.Add(SpecificKillMethod.Cleaver);
						methods.Add(SpecificKillMethod.FireAxe);
						methods.Add(SpecificKillMethod.FoldingKnife);
						methods.Add(SpecificKillMethod.KitchenKnife);
						methods.Add(SpecificKillMethod.LetterOpener);
						methods.Add(SpecificKillMethod.OldAxe);
						methods.Add(SpecificKillMethod.Saber);
						methods.Add(SpecificKillMethod.Scissors);
						methods.Add(SpecificKillMethod.Screwdriver);
						break;
					case MissionID.WHITTLETON_ABITTERPILL:
					case MissionID.WHITTLETON_ANOTHERLIFE:
						methods.Add(SpecificKillMethod.BattleAxe);
						methods.Add(SpecificKillMethod.BeakStaff);
						methods.Add(SpecificKillMethod.FireAxe);
						methods.Add(SpecificKillMethod.Hatchet);
						methods.Add(SpecificKillMethod.KitchenKnife);
						methods.Add(SpecificKillMethod.LetterOpener);
						methods.Add(SpecificKillMethod.Scissors);
						methods.Add(SpecificKillMethod.Screwdriver);
						break;
					case MissionID.ISLEOFSGAIL_THEARKSOCIETY:
						methods.Add(SpecificKillMethod.BattleAxe);
						methods.Add(SpecificKillMethod.Broadsword);
						methods.Add(SpecificKillMethod.CircumcisionKnife);
						methods.Add(SpecificKillMethod.Cleaver);
						methods.Add(SpecificKillMethod.FireAxe);
						methods.Add(SpecificKillMethod.Hatchet);
						methods.Add(SpecificKillMethod.Katana);
						methods.Add(SpecificKillMethod.KitchenKnife);
						methods.Add(SpecificKillMethod.LetterOpener);
						methods.Add(SpecificKillMethod.OldAxe);
						methods.Add(SpecificKillMethod.Saber);
						methods.Add(SpecificKillMethod.SacrificialKnife);
						methods.Add(SpecificKillMethod.SappersAxe);
						methods.Add(SpecificKillMethod.Scalpel);
						methods.Add(SpecificKillMethod.Scissors);
						methods.Add(SpecificKillMethod.Screwdriver);
						methods.Add(SpecificKillMethod.Starfish);
						methods.Add(SpecificKillMethod.VikingAxe);
						break;
					case MissionID.HAVEN_THELASTRESORT:
						methods.Add(SpecificKillMethod.BarberRazor);
						methods.Add(SpecificKillMethod.Cleaver);
						methods.Add(SpecificKillMethod.FireAxe);
						methods.Add(SpecificKillMethod.FoldingKnife);
						methods.Add(SpecificKillMethod.Hatchet);
						methods.Add(SpecificKillMethod.JarlsPirateSaber);
						methods.Add(SpecificKillMethod.KitchenKnife);
						methods.Add(SpecificKillMethod.LetterOpener);
						methods.Add(SpecificKillMethod.Machete);
						methods.Add(SpecificKillMethod.Scalpel);
						methods.Add(SpecificKillMethod.Scissors);
						methods.Add(SpecificKillMethod.Screwdriver);
						methods.Add(SpecificKillMethod.Seashell);
						methods.Add(SpecificKillMethod.Starfish);
						break;
					case MissionID.DUBAI_ONTOPOFTHEWORLD:
						methods.Add(SpecificKillMethod.Cleaver);
						methods.Add(SpecificKillMethod.FireAxe);
						methods.Add(SpecificKillMethod.FoldingKnife);
						methods.Add(SpecificKillMethod.KitchenKnife);
						methods.Add(SpecificKillMethod.LetterOpener);
						methods.Add(SpecificKillMethod.OrnateScimitar);
						methods.Add(SpecificKillMethod.Scissors);
						methods.Add(SpecificKillMethod.Screwdriver);
						break;
					case MissionID.DARTMOOR_DEATHINTHEFAMILY:
						methods.Add(SpecificKillMethod.BarberRazor);
						methods.Add(SpecificKillMethod.Cleaver);
						methods.Add(SpecificKillMethod.FoldingKnife);
						methods.Add(SpecificKillMethod.GardenFork);
						methods.Add(SpecificKillMethod.Hatchet);
						methods.Add(SpecificKillMethod.KitchenKnife);
						methods.Add(SpecificKillMethod.LetterOpener);
						methods.Add(SpecificKillMethod.Saber);
						methods.Add(SpecificKillMethod.Scissors);
						methods.Add(SpecificKillMethod.Screwdriver);
						methods.Add(SpecificKillMethod.Shears);
						methods.Add(SpecificKillMethod.UnicornHorn);
						break;
					case MissionID.BERLIN_APEXPREDATOR:
						methods.Add(SpecificKillMethod.BattleAxe);
						methods.Add(SpecificKillMethod.CombatKnife);
						methods.Add(SpecificKillMethod.ConcealableKnife);
						methods.Add(SpecificKillMethod.FireAxe);
						methods.Add(SpecificKillMethod.GardenFork);
						methods.Add(SpecificKillMethod.HobbyKnife);
						methods.Add(SpecificKillMethod.KitchenKnife);
						methods.Add(SpecificKillMethod.OldAxe);
						methods.Add(SpecificKillMethod.ScrapSword);
						methods.Add(SpecificKillMethod.Screwdriver);
						break;
					case MissionID.CHONGQING_ENDOFANERA:
						methods.Add(SpecificKillMethod.Cleaver);
						methods.Add(SpecificKillMethod.FireAxe);
						methods.Add(SpecificKillMethod.GardenFork);
						methods.Add(SpecificKillMethod.HobbyKnife);
						methods.Add(SpecificKillMethod.KitchenKnife);
						methods.Add(SpecificKillMethod.LetterOpener);
						methods.Add(SpecificKillMethod.Scissors);
						methods.Add(SpecificKillMethod.ScrapSword);
						methods.Add(SpecificKillMethod.Screwdriver);
						methods.Add(SpecificKillMethod.Tanto);
						break;
					case MissionID.MENDOZA_THEFAREWELL:
						methods.Add(SpecificKillMethod.Broadsword);
						methods.Add(SpecificKillMethod.CombatKnife);
						methods.Add(SpecificKillMethod.FireAxe);
						methods.Add(SpecificKillMethod.GardenFork);
						methods.Add(SpecificKillMethod.GrapeKnife);
						methods.Add(SpecificKillMethod.Hatchet);
						methods.Add(SpecificKillMethod.HobbyKnife);
						methods.Add(SpecificKillMethod.Icicle);
						methods.Add(SpecificKillMethod.JarlsPirateSaber);
						methods.Add(SpecificKillMethod.KitchenKnife);
						methods.Add(SpecificKillMethod.Machete);
						methods.Add(SpecificKillMethod.SappersAxe);
						methods.Add(SpecificKillMethod.Screwdriver);
						methods.Add(SpecificKillMethod.Shears);
						methods.Add(SpecificKillMethod.OldAxe);
						break;
					case MissionID.CARPATHIAN_UNTOUCHABLE:
						methods.Add(SpecificKillMethod.BarberRazor);
						methods.Add(SpecificKillMethod.FireAxe);
						methods.Add(SpecificKillMethod.Icicle);
						methods.Add(SpecificKillMethod.RustyScrewdriver);
						break;
					case MissionID.AMBROSE_SHADOWSINTHEWATER:
						methods.Add(SpecificKillMethod.BarberRazor);
						methods.Add(SpecificKillMethod.Cleaver);
						methods.Add(SpecificKillMethod.FoldingKnife);
						methods.Add(SpecificKillMethod.GardenFork);
						methods.Add(SpecificKillMethod.HobbyKnife);
						methods.Add(SpecificKillMethod.Hook);
						methods.Add(SpecificKillMethod.JarlsPirateSaber);
						methods.Add(SpecificKillMethod.KitchenKnife);
						methods.Add(SpecificKillMethod.KukriMachete);
						methods.Add(SpecificKillMethod.LetterOpener);
						methods.Add(SpecificKillMethod.MeatFork);
						methods.Add(SpecificKillMethod.OldAxe);
						methods.Add(SpecificKillMethod.Scissors);
						methods.Add(SpecificKillMethod.ScrapSword);
						methods.Add(SpecificKillMethod.Screwdriver);
						methods.Add(SpecificKillMethod.Seashell);
						methods.Add(SpecificKillMethod.Shears);
						methods.Add(SpecificKillMethod.Starfish);
						break;
				}
				return methods;
			}
		}

		private readonly static Dictionary<string, MissionID> missionDict = new() {
			{"arrival", MissionID.ICAFACILITY_ARRIVAL},
			{"guided", MissionID.ICAFACILITY_GUIDED},
			{"freeform", MissionID.ICAFACILITY_FREEFORM},
			{"polarbear", MissionID.ICAFACILITY_FINALTEST},
			{"finaltest", MissionID.ICAFACILITY_FINALTEST},
			{"peacock", MissionID.PARIS_SHOWSTOPPER},
			{"paris", MissionID.PARIS_SHOWSTOPPER},
			{"showstopper", MissionID.PARIS_SHOWSTOPPER},
			{"theshowstopper", MissionID.PARIS_SHOWSTOPPER},
			{"parisnoel", MissionID.PARIS_HOLIDAYHOARDERS},
			{"holidayhoarders", MissionID.PARIS_HOLIDAYHOARDERS},
			{"octopus", MissionID.SAPIENZA_WORLDOFTOMORROW},
			{"sapienza", MissionID.SAPIENZA_WORLDOFTOMORROW},
			{"worldoftomorrow", MissionID.SAPIENZA_WORLDOFTOMORROW},
			{"copperhead", MissionID.SAPIENZA_THEICON},
			{"theicon", MissionID.SAPIENZA_THEICON},
			{"icon", MissionID.SAPIENZA_THEICON},
			{"mamba", MissionID.SAPIENZA_LANDSLIDE},
			{"landslide", MissionID.SAPIENZA_LANDSLIDE},
			{"lastword", MissionID.SAPIENZA_THEAUTHOR},
			{"theauthor", MissionID.SAPIENZA_THEAUTHOR},
			{"author", MissionID.SAPIENZA_THEAUTHOR},
			{"spider", MissionID.MARRAKESH_GILDEDCAGE},
			{"marrakesh", MissionID.MARRAKESH_GILDEDCAGE},
			{"agildedcage", MissionID.MARRAKESH_GILDEDCAGE},
			{"gildedcage", MissionID.MARRAKESH_GILDEDCAGE},
			{"python", MissionID.MARRAKESH_HOUSEBUILTONSAND},
			{"ahousebuiltonsand", MissionID.MARRAKESH_HOUSEBUILTONSAND},
			{"housebuiltonsand", MissionID.MARRAKESH_HOUSEBUILTONSAND},
			{"ahbos", MissionID.MARRAKESH_HOUSEBUILTONSAND},
			{"tiger", MissionID.BANGKOK_CLUB27},
			{"club27", MissionID.BANGKOK_CLUB27},
			{"bangkok", MissionID.BANGKOK_CLUB27},
			{"tamagozake", MissionID.BANGKOK_THESOURCE},
			{"thesource", MissionID.BANGKOK_THESOURCE},
			{"source", MissionID.BANGKOK_THESOURCE},
			{"bull", MissionID.COLORADO_FREEDOMFIGHTERS},
			{"colorado", MissionID.COLORADO_FREEDOMFIGHTERS},
			{"freedomfighters", MissionID.COLORADO_FREEDOMFIGHTERS},
			{"snowcrane", MissionID.HOKKAIDO_SITUSINVERSUS},
			{"hokkaido", MissionID.HOKKAIDO_SITUSINVERSUS},
			{"situsinversus", MissionID.HOKKAIDO_SITUSINVERSUS},
			{"mamushi", MissionID.HOKKAIDO_SNOWFESTIVAL},
			{"snowfestival", MissionID.HOKKAIDO_SNOWFESTIVAL},
			{"hokkaidosnowfestival", MissionID.HOKKAIDO_SNOWFESTIVAL},
			{"bronx", MissionID.HOKKAIDO_PATIENTZERO},
			{"patientzero", MissionID.HOKKAIDO_PATIENTZERO},
			{"patient0", MissionID.HOKKAIDO_PATIENTZERO},
			{"pz", MissionID.HOKKAIDO_PATIENTZERO},
			{"sheep", MissionID.HAWKESBAY_NIGHTCALL},
			{"hawkesbay", MissionID.HAWKESBAY_NIGHTCALL},
			{"nightcall", MissionID.HAWKESBAY_NIGHTCALL},
			{"flamingo", MissionID.MIAMI_FINISHLINE},
			{"miami", MissionID.MIAMI_FINISHLINE},
			{"thefinishline", MissionID.MIAMI_FINISHLINE},
			{"finishline", MissionID.MIAMI_FINISHLINE},
			{"cottonmouth", MissionID.MIAMI_ASILVERTONGUE},
			{"asilvertongue", MissionID.MIAMI_ASILVERTONGUE},
			{"silvertongue", MissionID.MIAMI_ASILVERTONGUE},
			{"hippo", MissionID.SANTAFORTUNA_THREEHEADEDSERPENT},
			{"santafortuna", MissionID.SANTAFORTUNA_THREEHEADEDSERPENT},
			{"sf", MissionID.SANTAFORTUNA_THREEHEADEDSERPENT},
			{"threeheadedserpent", MissionID.SANTAFORTUNA_THREEHEADEDSERPENT},
			{"3headedserpent", MissionID.SANTAFORTUNA_THREEHEADEDSERPENT},
			{"anaconda", MissionID.SANTAFORTUNA_EMBRACEOFTHESERPENT},
			{"embraceoftheserpent", MissionID.SANTAFORTUNA_EMBRACEOFTHESERPENT},
			{"embracetheserpent", MissionID.SANTAFORTUNA_EMBRACEOFTHESERPENT},
			{"embraceofserpent", MissionID.SANTAFORTUNA_EMBRACEOFTHESERPENT},
			{"embraceserpent", MissionID.SANTAFORTUNA_EMBRACEOFTHESERPENT},
			{"embrace", MissionID.SANTAFORTUNA_EMBRACEOFTHESERPENT},
			{"mongoose", MissionID.MUMBAI_CHASINGAGHOST},
			{"mumbai", MissionID.MUMBAI_CHASINGAGHOST},
			{"chasingaghost", MissionID.MUMBAI_CHASINGAGHOST},
			{"kingcobra", MissionID.MUMBAI_ILLUSIONSOFGRANDEUR},
			{"illusionsofgrandeur", MissionID.MUMBAI_ILLUSIONSOFGRANDEUR},
			{"illusions", MissionID.MUMBAI_ILLUSIONSOFGRANDEUR},
			{"grandeur", MissionID.MUMBAI_ILLUSIONSOFGRANDEUR},
			{"skunk", MissionID.WHITTLETON_ANOTHERLIFE},
			{"anotherlife", MissionID.WHITTLETON_ANOTHERLIFE},
			{"anewlife", MissionID.WHITTLETON_ANOTHERLIFE},
			{"wc", MissionID.WHITTLETON_ANOTHERLIFE},
			{"whittleton", MissionID.WHITTLETON_ANOTHERLIFE},
			{"whittletoncreek", MissionID.WHITTLETON_ANOTHERLIFE},
			{"gartersnake", MissionID.WHITTLETON_ABITTERPILL},
			{"abitterpill", MissionID.WHITTLETON_ABITTERPILL},
			{"bitterpill", MissionID.WHITTLETON_ABITTERPILL},
			{"magpie", MissionID.ISLEOFSGAIL_THEARKSOCIETY},
			{"isleofsgail", MissionID.ISLEOFSGAIL_THEARKSOCIETY},
			{"sgail", MissionID.ISLEOFSGAIL_THEARKSOCIETY},
			{"thearksociety", MissionID.ISLEOFSGAIL_THEARKSOCIETY},
			{"arksociety", MissionID.ISLEOFSGAIL_THEARKSOCIETY},
			{"raccoon", MissionID.NEWYORK_GOLDENHANDSHAKE},
			{"newyork", MissionID.NEWYORK_GOLDENHANDSHAKE},
			{"ny", MissionID.NEWYORK_GOLDENHANDSHAKE},
			{"nyc", MissionID.NEWYORK_GOLDENHANDSHAKE},
			{"goldenhandshake", MissionID.NEWYORK_GOLDENHANDSHAKE},
			{"thegoldenhandshake", MissionID.NEWYORK_GOLDENHANDSHAKE},
			{"agoldenhandshake", MissionID.NEWYORK_GOLDENHANDSHAKE},
			{"stingray", MissionID.HAVEN_THELASTRESORT},
			{"haven", MissionID.HAVEN_THELASTRESORT},
			{"thelastresort", MissionID.HAVEN_THELASTRESORT},
			{"lastresort", MissionID.HAVEN_THELASTRESORT},
			{"gecko", MissionID.DUBAI_ONTOPOFTHEWORLD},
			{"dubai", MissionID.DUBAI_ONTOPOFTHEWORLD},
			{"ontopoftheworld", MissionID.DUBAI_ONTOPOFTHEWORLD},
			{"bulldog", MissionID.DARTMOOR_DEATHINTHEFAMILY},
			{"dartmoor", MissionID.DARTMOOR_DEATHINTHEFAMILY},
			{"deathinthefamily", MissionID.DARTMOOR_DEATHINTHEFAMILY},
			{"fox", MissionID.BERLIN_APEXPREDATOR},
			{"berlin", MissionID.BERLIN_APEXPREDATOR},
			{"bearlean", MissionID.BERLIN_APEXPREDATOR},
			{"apexpredator", MissionID.BERLIN_APEXPREDATOR},
			{"rat", MissionID.CHONGQING_ENDOFANERA},
			{"chongqing", MissionID.CHONGQING_ENDOFANERA},
			{"china", MissionID.CHONGQING_ENDOFANERA},
			{"endofanera", MissionID.CHONGQING_ENDOFANERA},
			{"llama", MissionID.MENDOZA_THEFAREWELL},
			{"mendoza", MissionID.MENDOZA_THEFAREWELL},
			{"doza", MissionID.MENDOZA_THEFAREWELL},
			{"dozer", MissionID.MENDOZA_THEFAREWELL},
			{"thefarewell", MissionID.MENDOZA_THEFAREWELL},
			{"farewell", MissionID.MENDOZA_THEFAREWELL},
			{"wolverine", MissionID.CARPATHIAN_UNTOUCHABLE},
			{"train", MissionID.CARPATHIAN_UNTOUCHABLE},
			{"carpathian", MissionID.CARPATHIAN_UNTOUCHABLE},
			{"carpathianmountains", MissionID.CARPATHIAN_UNTOUCHABLE},
			{"untouchable", MissionID.CARPATHIAN_UNTOUCHABLE},
			{"dugong", MissionID.AMBROSE_SHADOWSINTHEWATER},
			{"ambrose", MissionID.AMBROSE_SHADOWSINTHEWATER},
			{"ambroseisland", MissionID.AMBROSE_SHADOWSINTHEWATER},
			{"shadowsinthewater", MissionID.AMBROSE_SHADOWSINTHEWATER},
			{"shadowsonthewater", MissionID.AMBROSE_SHADOWSINTHEWATER},
		};

		public static bool GetMissionFromString(string name, out MissionID mission)
		{
			return missionDict.TryGetValue(TokenCharacterRegex.Replace(name.RemoveDiacritics(), "").ToLower(), out mission);
		}

		public static bool GetMissionCodename(MissionID id, out string codename)
		{
			codename = id switch {
				MissionID.NONE => "",
				// * POLARBEAR with module codenames used for these
				MissionID.ICAFACILITY_ARRIVAL => "ARRIVAL", // *
				MissionID.ICAFACILITY_GUIDED => "GUIDED", // *
				MissionID.ICAFACILITY_FREEFORM => "FREEFORM", // *
				MissionID.ICAFACILITY_FINALTEST => "FINALTEST", // *
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
			return id == MissionID.NONE || codename.Length > 0;
		}

		public static List<Disguise> GetDisguises(MissionID mission)
		{
			List<Disguise> disguises = [];
			switch (mission) {
				case MissionID.ICAFACILITY_GUIDED:
				case MissionID.ICAFACILITY_FREEFORM:
					return [
						new("Suit", "outfit_22725852-7989-463a-822a-5848b1b2c6cf_0.jpg", true),
						new("Bodyguard", "outfit_cf170965-5582-48d7-8dd7-774ae0a144dd_0.jpg"),
						new("Mechanic", "outfit_e222cc14-8d48-42de-9af6-1b745dbb3614_0.jpg"),
						new("Terry Norfolk", "outfit_63d2164f-efa3-4a19-aaa0-279a0029dd74_0.jpg"),
						new("Yacht Crew", "outfit_bbffa24b-fa46-4f9d-a73d-71de56ff3bfe_0.jpg"),
						new("Yacht Security", "outfit_f7acaf86-205c-4ac4-98c7-2c418007299c_0.jpg"),
					];
				case MissionID.ICAFACILITY_FINALTEST:
					return [
						new("Suit", "outfit_22725852-7989-463a-822a-5848b1b2c6cf_0.jpg", true),
						new("Airfield Security", "outfit_f7acaf86-205c-4ac4-98c7-2c418007299c_0.jpg"),
						new("Airplane Mechanic", "outfit_8f6ea4f1-32a8-4e57-a39d-90a2c2ff2bb0_0.jpg"),
						new("KGB Officer", "outfit_abb1e004-7fdf-462b-96b3-074e3390c171_0.jpg"),
						new("Soviet Soldier", "outfit_5c419edc-203d-4736-8cd9-bed24e34171c_0.jpg"),
					];
				case MissionID.PARIS_SHOWSTOPPER:
				case MissionID.PARIS_HOLIDAYHOARDERS:
					disguises.Add(new Disguise("Suit", "outfit_4ea5c203-b7c4-489d-85a1-bf91272d6190_0.jpg", true));
					disguises.Add(new Disguise("Auction Staff", "outfit_b5664bed-462a-417c-bc07-6d9d3f666e2d_0.jpg"));
					disguises.Add(new Disguise("Chef", "outfit_26ae3261-2a1d-49b2-8cab-d626d0887836_0.jpg"));
					disguises.Add(new Disguise("CICADA Bodyguard", "outfit_d2c76544-3a12-43a8-abc3-c7ce51830c1e_0.jpg"));
					disguises.Add(new Disguise("Helmut Kruger", "outfit_642c20f9-bf41-41b4-b0bb-2491b5be938a_0.jpg"));
					disguises.Add(new Disguise("Palace Staff", "outfit_2018db77-aa8a-4bf9-9afb-56bdaa161156_0.jpg"));

					if (mission == MissionID.PARIS_HOLIDAYHOARDERS)
						disguises.Add(new Disguise("Santa", "outfit_315400cd-90d8-43cc-8c22-62c0cb8969a5_0.jpg"));

					disguises.Add(new Disguise("Security Guard", "outfit_992cc7b6-4ccf-4ae8-a467-e9b2aabaeeb5_0.jpg"));
					disguises.Add(new Disguise("Sheikh Salman Al-Ghazali", "outfit_a8ecd823-6e08-4cfe-a04d-816d387fcf0c_0.jpg"));
					disguises.Add(new Disguise("Stylist", "outfit_96e32a7a-129a-4dd6-9b5b-3000a58f2a0f_0.jpg"));
					disguises.Add(new Disguise("Tech Crew", "outfit_69aac6db-461e-43af-89bc-2c27e50d430f_0.jpg"));
					disguises.Add(new Disguise("Vampire Magician", "outfit_1fdc259e-b96a-47f2-bbd8-e86e78d6df70_0.jpg"));
					break;
				case MissionID.SAPIENZA_WORLDOFTOMORROW:
					return [
						new("Suit", "outfit_75759271-e236-4b33-8dd5-7e502c958d05_0.jpg", true),
						new("Biolab Security", "outfit_91b37d1f-21ba-42b5-81fa-f4b6ce2ba691_0.jpg"),
						new("Bodyguard", "outfit_bf9629e0-f25c-4e71-9561-4a99a93a43e8_0.jpg"),
						new("Bohemian", "outfit_61b8f96e-4986-4f0a-ab95-dcdc69f51580_0.jpg"),
						new("Butler", "outfit_81eed11b-eaa3-4fd3-97ba-e1e89dcca57e_0.jpg"),
						new("Chef", "outfit_4c6816d8-4ae7-4161-a971-970055e64b34_0.jpg"),
						new("Church Staff", "outfit_5440b347-026f-402c-9cd4-3b4e142804ce_0.jpg"),
						new("Cyclist", "outfit_b58672d6-c235-4c08-a856-ef7caee777dd_0.jpg"),
						new("Delivery Man", "outfit_0d35cd13-e2ca-4375-9a67-763d1b776b48_0.jpg"),
						new("Dr. Oscar Lafayette", "outfit_fbf69e85-da3c-423b-bef9-da1b64f35f6b_0.jpg"),
						new("Gardener", "outfit_d788ff58-8a7a-4a85-acdd-c0e5693525f0_0.jpg"),
						new("Green Plumber", "outfit_844680e8-ae40-4fec-92b7-69c7619feb82_0.jpg"),
						new("Hazmat Suit", "outfit_98e839aa-7bee-46d7-9963-6190cd310a37_0.jpg"),
						new("Housekeeper", "outfit_a6c81663-684d-4506-abc0-65b35c4d8b63_0.jpg"),
						new("Kitchen Assistant", "outfit_10601c6b-1f65-44ed-92a1-bf843f023d3f_0.jpg"),
						new("Lab Technician", "outfit_2894c92d-b780-412f-a48f-5c5ddf0dafc8_0.jpg"),
						new("Mansion Security", "outfit_fd56a934-f402-4b52-bdca-8bbc737400ff_0.jpg"),
						new("Mansion Staff", "outfit_5edcef70-c4bb-4856-9124-de3d39fa814a_0.jpg"),
						new("Plague Doctor", "outfit_2bbc0c72-fc99-465d-9dec-c276f68ab982_0.jpg"),
						new("Priest", "outfit_98888ced-60f9-4f83-a93b-bf0ef2963341_0.jpg"),
						new("Private Detective", "outfit_02a9f8e4-3ed1-4c29-9f73-88e0cf2d7b5e_0.jpg"),
						new("Roberto Vargas", "outfit_691311a2-c215-4250-a318-fb25fd08e265_0.jpg"),
						new("Red Plumber", "outfit_37352a6b-eb58-4458-a5d6-522dd0508baa_0.jpg"),
						new("Store Clerk", "outfit_063fa6ef-9d58-4c3e-a4fd-70ce51a2f862_0.jpg"),
						new("Street Performer", "outfit_40bc08b2-1d39-4321-ab14-76f300e4ea3a_0.jpg"),
						new("Waiter", "outfit_430e8743-df1a-4e88-955f-793bff2e3a6a_0.jpg"),
					];
				case MissionID.SAPIENZA_THEAUTHOR:
					return [
						new("Suit", "outfit_75759271-e236-4b33-8dd5-7e502c958d05_0.jpg", true),
						new("Bodyguard", "outfit_bf9629e0-f25c-4e71-9561-4a99a93a43e8_0.jpg"),
						new("Bohemian", "outfit_61b8f96e-4986-4f0a-ab95-dcdc69f51580_0.jpg"),
						new("Brother Akram", "outfit_a6034d86-0fa4-46ce-8deb-90acf2d1e485_0.jpg"),
						new("Chef", "outfit_4c6816d8-4ae7-4161-a971-970055e64b34_0.jpg"),
						new("Church Staff", "outfit_5440b347-026f-402c-9cd4-3b4e142804ce_0.jpg"),
						new("Craig Black", "outfit_e710b6a4-b6f1-40a3-9389-cf297fff8d86_0.jpg"),
						new("Gardener", "outfit_d788ff58-8a7a-4a85-acdd-c0e5693525f0_0.jpg"),
						new("Green Plumber", "outfit_844680e8-ae40-4fec-92b7-69c7619feb82_0.jpg"),
						new("Housekeeper", "outfit_a6c81663-684d-4506-abc0-65b35c4d8b63_0.jpg"),
						new("Kitchen Assistant", "outfit_10601c6b-1f65-44ed-92a1-bf843f023d3f_0.jpg"),
						new("Red Plumber", "outfit_37352a6b-eb58-4458-a5d6-522dd0508baa_0.jpg"),
						new("Salvatore Bravuomo", "outfit_7766b295-35f3-45a8-b73d-10b222ed18ef_0.jpg"),
						new("SFX Crew", "outfit_2fd2437d-a5eb-4bfd-bd2d-a4f240a8f0ce_0.jpg"),
						new("Super Fan", "outfit_95c7f899-e20c-4182-a228-28bd3d8a4ff4_0.jpg"),
						new("Waiter", "outfit_430e8743-df1a-4e88-955f-793bff2e3a6a_0.jpg"),
					];
				case MissionID.SAPIENZA_LANDSLIDE:
					return [
						new("Suit", "outfit_75759271-e236-4b33-8dd5-7e502c958d05_0.jpg", true),
						new("Bodyguard", "outfit_bf9629e0-f25c-4e71-9561-4a99a93a43e8_0.jpg"),
						new("Bohemian", "outfit_61b8f96e-4986-4f0a-ab95-dcdc69f51580_0.jpg"),
						new("Church Staff", "outfit_5440b347-026f-402c-9cd4-3b4e142804ce_0.jpg"),
						new("Gardener", "outfit_d788ff58-8a7a-4a85-acdd-c0e5693525f0_0.jpg"),
						new("Green Plumber", "outfit_844680e8-ae40-4fec-92b7-69c7619feb82_0.jpg"),
						new("Kitchen Assistant", "outfit_10601c6b-1f65-44ed-92a1-bf843f023d3f_0.jpg"),
						new("Photographer", "outfit_b110cb05-0a38-4d77-b199-16e15a98b111_0.jpg"),
						new("Priest", "outfit_98888ced-60f9-4f83-a93b-bf0ef2963341_0.jpg"),
						new("Red Plumber", "outfit_37352a6b-eb58-4458-a5d6-522dd0508baa_0.jpg"),
						new("Salvatore Bravuomo", "outfit_7766b295-35f3-45a8-b73d-10b222ed18ef_0.jpg"),
						new("Security", "outfit_94a0d283-bea4-468d-ad8f-ec2735008511_0.jpg"),
						new("Stage Crew", "outfit_6f9f7786-2044-4394-98b4-f79da0341e7f_0.jpg"),
						new("Waiter", "outfit_430e8743-df1a-4e88-955f-793bff2e3a6a_0.jpg"),
					];
				case MissionID.SAPIENZA_THEICON:
					return [
						new("Suit", "outfit_cad53cde-4d4a-41cb-9259-87b544f718ad_0.jpg", true),
						new("Kitchen Assistant", "outfit_10601c6b-1f65-44ed-92a1-bf843f023d3f_0.jpg"),
						new("Movie Crew", "outfit_79a767f7-5473-4380-b778-e93028e4fc2f_0.jpg"),
						new("Security", "outfit_94a0d283-bea4-468d-ad8f-ec2735008511_0.jpg"),
						new("SFX Crew", "outfit_2fd2437d-a5eb-4bfd-bd2d-a4f240a8f0ce_0.jpg"),
					];
				case MissionID.MARRAKESH_GILDEDCAGE:
					return [
						new("Suit", "outfit_cb105877-743d-4a3b-bdad-28a022630306_0.jpg", true),
						new("Bodyguard", "outfit_97536da8-64a5-4675-a108-08ff7be41c1f_0.jpg"),
						new("Cameraman", "outfit_e0fc86fb-a852-4652-bb5c-b591f7bfeb29_0.jpg"),
						new("Consulate Intern", "outfit_65f23b45-f5ab-4ede-82ec-46e4de38c0e9_0.jpg"),
						new("Consulate Janitor", "outfit_1b87edd3-fb1e-4adf-9463-efcb380cbd6b_0.jpg"),
						new("Consulate Security", "outfit_e0fc86fc-a852-4652-bb5c-b591f7bfeb29_0.jpg"),
						new("Elite Soldier", "outfit_e5bb3c6b-2fcd-4e36-a30d-03955cb05088_0.jpg"),
						new("Food Vendor", "outfit_98b1c1f6-2634-4c29-b9e6-fa8d7633100a_0.jpg"),
						new("Fortune Teller", "outfit_dc4762e0-e58e-4336-a3c1-40646087267a_0.jpg"),
						new("Handyman", "outfit_eb48ed16-195a-4de1-bae6-d7e7ec92046c_0.jpg"),
						new("Headmaster", "outfit_6136fa6a-3f1f-4606-9b87-fde9538966dc_0.jpg"),
						new("Local Printing Crew", "outfit_ff6668af-dde5-48c3-ac9a-f47b860122d0_0.jpg"),
						new("Masseur", "outfit_138eda40-501a-48b8-affc-928321566a4e_0.jpg"),
						new("Military Officer", "outfit_65d860f4-998e-4f92-a1d7-9f40c04a2474_0.jpg"),
						new("Military Soldier", "outfit_955bca9e-bc91-46da-a4df-3dfc787c8aff_0.jpg"),
						new("Prisoner", "outfit_fdb4aade-4d5f-47e2-896f-fc1addf64d52_0.jpg"),
						new("Shopkeeper", "outfit_ecf1b752-0cd6-4283-a1a5-743fc0249525_0.jpg"),
						new("Waiter", "outfit_6348cc33-665f-4470-80b4-a0ad836df702_0.jpg"),
					];
				case MissionID.MARRAKESH_HOUSEBUILTONSAND:
					return [
						new("Suit", "outfit_cb105877-743d-4a3b-bdad-28a022630306_0.jpg", true),
						new("Bodyguard", "outfit_97536da8-64a5-4675-a108-08ff7be41c1f_0.jpg"),
						new("Food Vendor", "outfit_98b1c1f6-2634-4c29-b9e6-fa8d7633100a_0.jpg"),
						new("Fortune Teller", "outfit_dc4762e0-e58e-4336-a3c1-40646087267a_0.jpg"),
						new("Handyman", "outfit_eb48ed16-195a-4de1-bae6-d7e7ec92046c_0.jpg"),
						new("Military Soldier", "outfit_955bca9e-bc91-46da-a4df-3dfc787c8aff_0.jpg"),
						new("Shopkeeper", "outfit_ecf1b752-0cd6-4283-a1a5-743fc0249525_0.jpg"),
						new("Waiter", "outfit_6348cc33-665f-4470-80b4-a0ad836df702_0.jpg"),
					];
				case MissionID.BANGKOK_CLUB27:
					return [
						new("Suit", "outfit_c85c6e9c-0aee-43b8-b6e3-d70e76f1890e_0.jpg", true),
						new("Abel de Silva", "outfit_f17c737e-9947-4fff-a443-65b381839d00_0.jpg"),
						new("Exterminator", "outfit_bf0bcc10-a335-4714-9dd2-69e7e96704b2_0.jpg"),
						new("Groundskeeper", "outfit_7f6da010-1a96-4783-83e0-48c55a3e7103_0.jpg"),
						new("Hotel Security", "outfit_07f3479a-29fc-45e0-bb80-e49a41c0c410_0.jpg"),
						new("Hotel Staff", "outfit_c96f9796-0194-47c6-836c-102473cc6c3c_0.jpg"),
						new("Jordan Cross' Bodyguard", "outfit_d01c8adc-735c-44f0-9105-b28d85062def_0.jpg"),
						new("Kitchen Staff", "outfit_85971c2e-34ae-423f-9653-bc32c5f3e4f7_0.jpg"),
						new("Morgan's Bodyguard", "outfit_4c8941af-541c-4bb9-a8e3-8b8e61b0a789_0.jpg"),
						new("Recording Crew", "outfit_ef704a8e-88b7-430a-a217-09bbeea7074f_0.jpg"),
						new("Stalker", "outfit_c5bf909f-66a5-4f19-9aee-aeb953172e45_0.jpg"),
						new("Waiter", "outfit_57669117-fbf3-4630-80e3-53e5420a8f30_0.jpg"),
					];
				case MissionID.BANGKOK_THESOURCE:
					return [
						new("Suit", "outfit_c85c6e9c-0aee-43b8-b6e3-d70e76f1890e_0.jpg", true),
						new("Cult Bodyguard", "outfit_78fcc1c0-5612-4284-924f-c20d9e322c96_0.jpg"),
						new("Cult Initiate", "outfit_54c5dce7-cfe4-43f9-8cee-8204e38c608d_0.jpg"),
						new("Exterminator", "outfit_bf0bcc10-a335-4714-9dd2-69e7e96704b2_0.jpg"),
						new("Groundskeeper", "outfit_7f6da010-1a96-4783-83e0-48c55a3e7103_0.jpg"),
						new("Hotel Security", "outfit_07f3479a-29fc-45e0-bb80-e49a41c0c410_0.jpg"),
						new("Hotel Staff", "outfit_c96f9796-0194-47c6-836c-102473cc6c3c_0.jpg"),
						new("Jordan Cross' Bodyguard", "outfit_d01c8adc-735c-44f0-9105-b28d85062def_0.jpg"),
						new("Kitchen Staff", "outfit_85971c2e-34ae-423f-9653-bc32c5f3e4f7_0.jpg"),
						new("Militia Soldier", "outfit_3dd1467a-72d2-4590-93d8-10807c9f1645_0.jpg"),
						new("Recording Crew", "outfit_ef704a8e-88b7-430a-a217-09bbeea7074f_0.jpg"),
						new("Waiter", "outfit_57669117-fbf3-4630-80e3-53e5420a8f30_0.jpg"),
					];
				case MissionID.COLORADO_FREEDOMFIGHTERS:
					return [
						new("Suit", "outfit_dd9792ec-4a1d-4c29-a928-a556fc0b6692_0.jpg", true),
						new("Explosives Specialist", "outfit_59c4f7db-f065-4fac-bc6c-5c7ac3758eed_0.jpg"),
						new("Hacker", "outfit_aab7f28d-84d9-47d1-be52-d142f5970086_0.jpg"),
						new("Militia Cook", "outfit_dac13714-a012-47cf-b76a-150cfc4cece5_0.jpg"),
						new("Militia Elite", "outfit_d878b5ee-90cd-4222-8503-1e9ae193d556_0.jpg"),
						new("Militia Soldier", "outfit_3dd1467a-72d2-4590-93d8-10807c9f1645_0.jpg"),
						new("Militia Spec Ops", "outfit_ed1a4bf9-0641-4e70-9af1-d6e68cbb84d6_0.jpg"),
						new("Militia Technician", "outfit_143c62fc-4bf6-474a-9542-1e81bf93a044_0.jpg"),
						new("Point Man", "outfit_338021a8-0c61-4732-a991-559e25e49efe_0.jpg"),
						new("Scarecrow", "outfit_fd5d2b9d-dcef-4596-a98a-5266a148c40c_0.jpg"),
					];
				case MissionID.HOKKAIDO_SITUSINVERSUS:
				case MissionID.HOKKAIDO_SNOWFESTIVAL:
					return [
						new("Suit", "outfit_1c3964e1-75c6-4adb-8cbb-ebd0a830b839_0.jpg", true),
						new("Baseball Player", "outfit_5946924c-958d-48f4-ada3-86beb58aa778_0.jpg"),
						new("Bodyguard", "outfit_5270225d-797a-43f8-8435-078ae0d92249_0.jpg"),
						new("Chef", "outfit_d6bbbe57-8cc8-45ed-b1cb-d1f9477c4b61_0.jpg"),
						new("Chief Surgeon", "outfit_b8deb948-a0a9-4dcb-9df4-1c2ecd29765f_0.jpg"),
						new("Doctor", "outfit_a8191fb6-9a6d-4145-8baf-d786e6f392b7_0.jpg"),
						new("Handyman", "outfit_d9e0fbe7-ff74-4030-bed6-5a33a01acead_0.jpg"),
						new("Helicopter Pilot", "outfit_b8dbb7b6-fef9-4782-923f-ddebc573625e_0.jpg"),
						new("Hospital Director", "outfit_f6f53c39-17f9-48cf-9594-7a696b036d61_0.jpg"),
						new("Morgue Doctor", "outfit_3d4424a3-23f9-4cfe-b225-2e06c17d780b_0.jpg"),
						new("Motorcyclist", "outfit_8e01f48f-ef06-448c-9d22-5d58c4414968_0.jpg"),
						new("Ninja", "outfit_06456d4d-da36-4008-bea5-c0b985a565f5_0.jpg"),
						new("Patient", "outfit_c98a6467-5dd9-4041-8bff-119445750d4d_0.jpg"),
						new("Resort Security", "outfit_25406dac-d206-48c7-a6df-dffb887c9227_0.jpg"),
						new("Resort Staff", "outfit_52992428-8884-48db-9764-e486d17d4804_0.jpg"),
						new("Surgeon", "outfit_6a25f81d-cf2e-4e47-9b15-0f712a3f71d9_0.jpg"),
						new("VIP Patient (Amos Dexter)", "outfit_427bac46-50b4-4470-9b0e-478efcd37793_0.jpg"),
						new("VIP Patient (Jason Portman)", "outfit_b00380d9-3f84-4484-8bd6-39c0872da414_0.jpg"),
						new("Yoga Instructor", "outfit_f4ea7065-d32b-4a97-baf9-98072a9c8128_0.jpg"),
					];
				case MissionID.HOKKAIDO_PATIENTZERO:
					return [
						new("Suit", "outfit_250112ba-e39d-473c-99cd-5fc429c5fff5_0.jpg", true),
						new("Bio Suit", "outfit_e8ef431d-62b2-4d0a-a766-750c0bc6e39e_0.jpg"),
						new("Bodyguard", "outfit_5270225d-797a-43f8-8435-078ae0d92249_0.jpg"),
						new("Chef", "outfit_d6bbbe57-8cc8-45ed-b1cb-d1f9477c4b61_0.jpg"),
						new("Doctor", "outfit_a8191fb6-9a6d-4145-8baf-d786e6f392b7_0.jpg"),
						new("Handyman", "outfit_d9e0fbe7-ff74-4030-bed6-5a33a01acead_0.jpg"),
						new("Head Researcher", "outfit_ff534fe6-065e-4062-a32c-8bdf223efd98_0.jpg"),
						new("Helicopter Pilot", "outfit_b8dbb7b6-fef9-4782-923f-ddebc573625e_0.jpg"),
						new("Hospital Director", "outfit_f6f53c39-17f9-48cf-9594-7a696b036d61_0.jpg"),
						new("Morgue Doctor", "outfit_3d4424a3-23f9-4cfe-b225-2e06c17d780b_0.jpg"),
						new("Motorcyclist", "outfit_8e01f48f-ef06-448c-9d22-5d58c4414968_0.jpg"),
						new("Patient", "outfit_c98a6467-5dd9-4041-8bff-119445750d4d_0.jpg"),
						new("Resort Security", "outfit_25406dac-d206-48c7-a6df-dffb887c9227_0.jpg"),
						new("Resort Staff", "outfit_52992428-8884-48db-9764-e486d17d4804_0.jpg"),
						new("Surgeon", "outfit_6a25f81d-cf2e-4e47-9b15-0f712a3f71d9_0.jpg"),
						new("VIP Patient (Amos Dexter)", "outfit_427bac46-50b4-4470-9b0e-478efcd37793_0.jpg"),
						new("Yoga Instructor", "outfit_f4ea7065-d32b-4a97-baf9-98072a9c8128_0.jpg"),
					];
				case MissionID.HAWKESBAY_NIGHTCALL:
					return [
						new("Suit", "outfit_08022e2c-4954-4b63-b632-3ac50d018292_0.jpg", true),
						new("Bodyguard", "outfit_d4dd18b3-2dbe-4ad2-8bfa-db5fdb9a6568_0.jpg"),
					];
				case MissionID.MIAMI_FINISHLINE:
				case MissionID.MIAMI_ASILVERTONGUE:
					disguises.Add(new("Suit", "outfit_1ad1ec9b-1e96-4fac-b0e6-8817a46da9db_0.jpg", true));
					disguises.Add(new("Aeon Driver", "outfit_6fca97a0-cb80-4bd7-9a8f-106fa20a5d04_0.jpg"));
					disguises.Add(new("Aeon Mechanic", "outfit_da951ccc-1d8b-4d84-b30a-72e74ac2a312_0.jpg"));
					disguises.Add(new("Blue Seed Driver", "outfit_46dedef3-bbcf-438c-af2b-c97dd853aac1_0.jpg"));
					disguises.Add(new("Crashed Kronstadt Driver", "outfit_8980597c-1559-46a7-ba8d-bc5d95d5936a_0.jpg"));
					disguises.Add(new("Event Crew", "outfit_d86e4379-4aad-42cf-a7cf-38d0fa7e727b_0.jpg"));
					disguises.Add(new("Event Security", "outfit_dc5b1ccd-0997-4834-93a0-db7543e729cc_0.jpg"));
					disguises.Add(new("Florida Man", "outfit_f8ef3523-2500-410c-98fb-b6926a832df4_0.jpg"));
					disguises.Add(new("Food Vendor", "outfit_e9ed2969-146a-472d-8e87-39c77bd1757d_0.jpg"));
					disguises.Add(new("Journalist", "outfit_723e73f3-9fa4-40d8-bb11-b66184c9a795_0.jpg"));
					disguises.Add(new("Kitchen Staff", "outfit_2a5a3dba-bafd-4a1f-8bbf-204668b32fe1_0.jpg"));
					disguises.Add(new("Kowoon Driver", "outfit_12181b2f-8c74-4761-8d78-3bedfbf9281d_0.jpg"));
					disguises.Add(new("Kowoon Mechanic", "outfit_fc829fad-5afc-4236-8662-65ab8698ef44_0.jpg"));
					disguises.Add(new("Kronstadt Engineer", "outfit_9bd53a5a-a152-488f-be20-7394b083d99a_0.jpg"));
					disguises.Add(new("Kronstadt Mechanic", "outfit_085e639e-2cf4-4e9b-bd9b-f9fd5b899676_0.jpg"));
					disguises.Add(new("Kronstadt Researcher", "outfit_ade47a03-a3ec-4d78-aefa-6057abceea28_0.jpg"));
					disguises.Add(new("Kronstadt Security", "outfit_d37ae121-69b4-4a9c-ab57-972063505e2f_0.jpg"));
					if (mission == MissionID.MIAMI_ASILVERTONGUE)
						disguises.Add(new("Mascot", "outfit_5fc2ed75-ab22-4c61-af6c-bdd07b1a55a6_0.jpg"));
					else
						disguises.Add(new("Mascot", "outfit_124d145e-469e-485d-a628-ced82ddf1b75_0.jpg"));
					disguises.Add(new("Medic", "outfit_b838226d-5fbf-4b5d-8e5f-98e5c8ddc1f2_0.jpg"));
					disguises.Add(new("Moses Lee", "outfit_a45555d8-d68c-4cd7-8006-7d7f61b36c72_0.jpg"));
					disguises.Add(new("Pale Rider", "outfit_9df94442-ed1b-436c-942f-3195b1ef7e0e_0.jpg"));
					disguises.Add(new("Race Coordinator", "outfit_be9dd1c4-af52-43db-b8f7-37c3c054c90f_0.jpg"));
					disguises.Add(new("Race Marshal", "outfit_a166a37e-a3f8-42d2-99d6-e0dd2cf5c090_0.jpg"));
					disguises.Add(new("Sheik", "outfit_a68b2030-d52f-4e52-907f-8657b867dd50_0.jpg"));
					disguises.Add(new("Sotteraneo Mechanic", "outfit_981b7a3b-5548-4a6f-b568-f767784e6d91_0.jpg"));
					disguises.Add(new("Street Musician", "outfit_2018be7c-5f79-497e-a7e1-0e64f31e71f5_0.jpg"));
					disguises.Add(new("Ted Mendez", "outfit_d7d8d5e8-070c-4d6c-9a67-f1165a7bb29d_0.jpg"));
					disguises.Add(new("Thwack Driver", "outfit_bac37ceb-6f72-406f-bfa3-e49413436525_0.jpg"));
					disguises.Add(new("Thwack Mechanic", "outfit_4376850a-3a37-4ad3-a886-168d0e24aa20_0.jpg"));
					disguises.Add(new("Waiter", "outfit_c7bbd142-7873-4a91-98c8-76a6900bea60_0.jpg"));
					break;
				case MissionID.SANTAFORTUNA_THREEHEADEDSERPENT:
				case MissionID.SANTAFORTUNA_EMBRACEOFTHESERPENT:
					disguises.Add(new("Suit", "outfit_ac71f90e-a67d-4898-b2f0-43b605332dc8_0.jpg", true));
					disguises.Add(new("Band Member", "outfit_fc0491ac-8592-486d-9dc2-b39af13cf6e3_0.jpg"));
					disguises.Add(new("Barman", "outfit_cfacf46a-eb59-4a16-a221-a690defd05a3_0.jpg"));
					disguises.Add(new("Chef", "outfit_d0fe70cb-c30b-41a3-8d1c-5503e898f686_0.jpg"));
					disguises.Add(new("Coca Field Guard", "outfit_56a589d8-bf28-489f-a30c-2ecea87177f5_0.jpg"));
					disguises.Add(new("Coca Field Worker", "outfit_11f2849d-87c5-4806-a25e-1a9dad85981d_0.jpg"));
					disguises.Add(new("Construction Worker", "outfit_57342129-03a9-47a4-a509-cc0656e0a76a_0.jpg"));
					disguises.Add(new("Drug Lab Worker", "outfit_a741cd97-135e-465e-89c3-4fa52a2bbf9d_0.jpg"));
					disguises.Add(new("Elite Guard", "outfit_3e3be8e1-1fe4-4b3a-959c-9e52f595b0c4_0.jpg"));
					disguises.Add(new("Gardener", "outfit_886c3b26-b81f-4731-8080-524f2d6da5dd_0.jpg"));
					disguises.Add(new("Hippie", "outfit_177410f1-4fd7-4ef2-8ed7-2119bcba3661_0.jpg"));
					disguises.Add(new("Hippo Whisperer", "outfit_4a145036-e4cc-4798-a795-42bcee511524_0.jpg"));
					disguises.Add(new("Mansion Guard", "outfit_f0d1dfab-ac73-4fe9-bbac-a5587fbc0f91_0.jpg"));
					disguises.Add(new("Mansion Staff", "outfit_2dec1e42-0093-462a-83aa-c0f4d82ac224_0.jpg"));
					disguises.Add(new("Shaman", "outfit_30005896-2b39-49c0-bb04-3475d4a12ae6_0.jpg"));
					disguises.Add(new("Street Soldier", "outfit_ab5a46a2-6e53-4b15-a48e-c336df1ef5ff_0.jpg"));
					disguises.Add(new("Submarine Crew", "outfit_f86848e7-ca8c-48e0-94d1-2d925e96a3e7_0.jpg"));
					disguises.Add(new("Submarine Engineer", "outfit_dfaa8260-20af-4112-b1ca-88a98481127b_0.jpg"));
					disguises.Add(new("Tattoo Artist (P-Power)", "outfit_135073d8-ef7c-4f4d-b30b-cbf65de613cb_0.jpg"));
					break;
				case MissionID.MUMBAI_CHASINGAGHOST:
				case MissionID.MUMBAI_ILLUSIONSOFGRANDEUR:
					disguises.Add(new("Suit", "outfit_5a77f988-62b8-4414-bd8f-b47fd457d0bd_0.jpg", true));
					disguises.Add(new("Barber", "outfit_c4011c75-39ff-4bff-aff5-fe902ae4b83b_0.jpg"));
					disguises.Add(new("Bollywood Bodyguard", "outfit_06fb2890-e820-45f2-aef3-0cb7d0528ee1_0.jpg"));
					disguises.Add(new("Bollywood Crew", "outfit_6d3d59b4-571c-4dbb-9737-205fb34a1ffa_0.jpg"));
					disguises.Add(new("Dancer", "outfit_88adef78-2a19-45fb-9c95-988e82c056f1_0.jpg"));
					disguises.Add(new("Elite Thug", "outfit_e9e143e1-f5a6-40a5-af56-947cbf32e20a_0.jpg"));
					disguises.Add(new("Food Vendor", "outfit_e9dffefc-e896-46e4-b158-1b401b015764_0.jpg"));
					disguises.Add(new("Holy Man", "outfit_e4581e1a-a45a-4c42-ba25-3527bd75c0f7_0.jpg"));
					disguises.Add(new("Kashmirian", "outfit_6f875d32-869e-437a-8935-368e0c2cc8bc_0.jpg"));
					disguises.Add(new("Laundry Foreman", "outfit_eeefa90a-6665-4eb1-8bc9-3e08c222abae_0.jpg"));
					disguises.Add(new("Laundry Worker", "outfit_c5c8e251-bb30-4e9e-b146-74ed96c7048f_0.jpg"));
					disguises.Add(new("Lead Actor", "outfit_446ace07-c9c6-49fc-b157-fa58e812fcef_0.jpg"));
					disguises.Add(new("Local Security", "outfit_d136699a-a244-4789-b332-9a3afc4e3f48_0.jpg"));
					disguises.Add(new("Metal Worker", "outfit_48afc44d-cf8a-44ba-9436-663a6979c908_0.jpg"));
					disguises.Add(new("Painter", "outfit_81f55bbc-a120-4757-a778-b73fd775d1a4_0.jpg"));
					disguises.Add(new("Queen's Bodyguard", "outfit_6edb224d-0970-4d1d-8740-5e86d1e7af59_0.jpg"));
					disguises.Add(new("Queen's Guard", "outfit_b36075a1-b352-4e0f-9d84-84f2bdac6a86_0.jpg"));
					disguises.Add(new("Tailor", "outfit_b384ff35-9c38-4b08-ab0b-e333cfd7bc6a_0.jpg"));
					disguises.Add(new("Thug", "outfit_a2cef12c-77d6-4062-9596-cf9d1a47d1b5_0.jpg"));
					disguises.Add(new("Vanya's Servant", "outfit_ae320bab-bb37-42a5-86a1-df283ada49c0_0.jpg"));
					break;
				case MissionID.WHITTLETON_ANOTHERLIFE:
				case MissionID.WHITTLETON_ABITTERPILL:
					disguises.Add(new("Suit", "outfit_cdb31942-27b5-4ec3-9009-f19b34d27fd0_0.jpg", true));
					disguises.Add(new("Arkian Robes", "outfit_e75d76b7-25ec-4500-9585-0ce34cec1e1f_0.jpg"));
					disguises.Add(new("BBQ Owner", "outfit_44f30ddb-cad9-402b-a307-6076fae3aa74_0.jpg"));
					disguises.Add(new("Cassidy Bodyguard", "outfit_7edfb519-9d60-4cd9-b4f4-74dd64d622b9_0.jpg"));
					disguises.Add(new("Construction Worker", "outfit_699ce756-8eef-4a6e-bc65-264d0e763fde_0.jpg"));
					disguises.Add(new("Exterminator", "outfit_b1739270-f9fc-4a24-a3e7-beb2deb235f2_0.jpg"));
					disguises.Add(new("Garbage Man", "outfit_4912d30a-80cb-41d8-8137-7b4727e76e4e_0.jpg"));
					disguises.Add(new("Gardener", "outfit_78fc9e38-cade-42c3-958c-c7d8edf43713_0.jpg"));
					disguises.Add(new("Gunther Mueller", "outfit_98a9d41b-3a39-4fe3-ab6a-31d8b574f2ff_0.jpg"));
					disguises.Add(new("James Batty", "outfit_13cbccd1-8a96-435b-84e8-107c0a29349d_0.jpg"));
					disguises.Add(new("Janus' Bodyguard", "outfit_078a5c70-737c-48b7-a190-b356438419b4_0.jpg"));
					disguises.Add(new("Mailman", "outfit_89f20c16-4e13-4f89-a85b-44dd17698bc7_0.jpg"));
					disguises.Add(new("Nurse", "outfit_4b416b2a-ac08-4379-8c53-46e46d8bcbf8_0.jpg"));
					disguises.Add(new("Plumber", "outfit_e4c5735c-ea33-4d11-a72b-584902370cf3_0.jpg"));
					disguises.Add(new("Police Deputy", "outfit_b210be64-ea03-4983-aa0f-8d18882a23c7_0.jpg"));
					disguises.Add(new("Politician", "outfit_64e48347-cac5-434b-b25c-711ff78c46fd_0.jpg"));
					disguises.Add(new("Politician's Assistant", "outfit_aee5458a-51b7-4ee2-996a-b71b3e149354_0.jpg"));
					disguises.Add(new("Real Estate Broker", "outfit_d47223f2-3fe4-46d1-a99a-09e0eb57aa7b_0.jpg"));
					disguises.Add(new("Server", "outfit_5d19c9f8-7df2-4113-b81d-b32d5e231717_0.jpg"));
					disguises.Add(new("Sheriff Masterson", "outfit_874efc03-afda-48f9-b073-b2db0e93bc3f_0.jpg"));
					disguises.Add(new("Spencer \"The Hammer\" Green", "outfit_6d1a3100-5dc0-4a8a-b9fc-341c864e3841_0.jpg"));
					break;
				case MissionID.ISLEOFSGAIL_THEARKSOCIETY:
					disguises.Add(new("Suit", "outfit_1e48fda8-4795-4ad4-a05d-0b9ca5d23f78_0.jpg", true));
					disguises.Add(new("Architect", "outfit_8d2b15f2-1d23-4b5e-b128-d2f47b53faf7_0.jpg"));
					disguises.Add(new("Ark Member", "outfit_b8b1d3c2-cf47-4a44-acc8-d8aa965ec8d8_0.jpg"));
					disguises.Add(new("Blake Nathaniel", "outfit_d40fe7e8-ec8d-429b-a86b-7844c0e4d1c7_0.jpg"));
					disguises.Add(new("Burial Robes", "outfit_ae340f4d-6282-48d0-8e0d-c3dcb414bb4f_0.jpg"));
					disguises.Add(new("Butler", "outfit_e9a9b20d-93de-48b7-8840-73411bace252_0.jpg"));
					disguises.Add(new("Castle Staff", "outfit_415c3c97-3c45-43a8-b930-40bece444a55_0.jpg"));
					disguises.Add(new("Chef", "outfit_e4aeb186-bedd-41a1-b4c0-bb9c49bc7982_0.jpg"));
					disguises.Add(new("Custodian", "outfit_04d72492-1b6b-4e6b-8372-5e65dc209cc4_0.jpg"));
					disguises.Add(new("Elite Guard", "outfit_84c55eed-6891-40b3-9449-6881b53fabdd_0.jpg"));
					disguises.Add(new("Entertainer", "outfit_f9a34b19-f9ff-44a9-b232-86b1b8fcdbb0_0.jpg"));
					disguises.Add(new("Event Staff", "outfit_e3d61bbf-5b28-45cb-88bd-b386f5daa605_0.jpg"));
					disguises.Add(new("Guard", "outfit_6565bf3a-aa59-44f5-9b89-ef645f99d4fa_0.jpg"));
					disguises.Add(new("Initiate", "outfit_daf223e8-0b22-405f-a3b9-40d2b9992c2f_0.jpg"));
					disguises.Add(new("Jebediah Block", "outfit_bef91840-e5aa-4a44-9f2e-30c732b1f7be_0.jpg"));
					disguises.Add(new("Knight's Armor", "outfit_fae73e92-2307-4163-8e9f-30401ca884bf_0.jpg"));
					disguises.Add(new("Master of Ceremonies", "outfit_9db0a810-7549-4932-b0ab-9d6241afdc2c_0.jpg"));
					disguises.Add(new("Raider", "outfit_58f91772-a202-49e4-a558-159f762d78e3_0.jpg"));
					break;
				case MissionID.NEWYORK_GOLDENHANDSHAKE:
					disguises.Add(new("Suit", "outfit_84f2e067-70c3-4d79-aa90-53b46b727505_0.jpg", true));
					disguises.Add(new("Bank Robber", "outfit_6b22a1db-861c-42fd-ae2d-a4a7bcda72ab_0.jpg"));
					disguises.Add(new("Bank Teller", "outfit_d3c1c97f-84d8-4e68-8d72-e2ce7564aaba_0.jpg"));
					disguises.Add(new("Fired Banker", "outfit_c105fd1e-a017-42e5-8a0c-2996363352eb_0.jpg"));
					disguises.Add(new("High Security Guard", "outfit_513c0da0-1cb0-4029-85c9-ad9e9522818d_0.jpg"));
					disguises.Add(new("Investment Banker", "outfit_e2f6fbfb-0237-477d-b93f-2374b02f0354_0.jpg"));
					disguises.Add(new("IT Worker", "outfit_88156045-87c6-4aff-9f99-f2fd40e0ab19_0.jpg"));
					disguises.Add(new("Janitor", "outfit_f4e27f1a-3e30-42fe-aa80-dc368590886b_0.jpg"));
					disguises.Add(new("Job Applicant", "outfit_d7939c60-087c-461e-9798-c0069cfec299_0.jpg"));
					disguises.Add(new("Security Guard", "outfit_ee38c686-f447-4a0d-bc5f-3822550db095_0.jpg"));
					break;
				case MissionID.HAVEN_THELASTRESORT:
					disguises.Add(new("Suit", "outfit_ea4230f3-03f7-46f1-a3f4-be2ff383b417_0.jpg", true));
					disguises.Add(new("Boat Captain", "outfit_2817afb5-6dff-4496-bf56-4cd59b9abc9b_0.jpg"));
					disguises.Add(new("Bodyguard", "outfit_95f2f02f-205b-422f-a315-875568f911da_0.jpg"));
					disguises.Add(new("Butler", "haven-butler.png"));
					disguises.Add(new("Chef", "outfit_cfc19dda-bff1-4bd1-9b0c-b1a799ee011f_0.jpg"));
					disguises.Add(new("Doctor", "outfit_f108122d-5b31-487a-857b-d5f1badf2220_0.jpg"));
					disguises.Add(new("Gas Suit", "outfit_cbcfe485-f706-46a1-a14a-316f6dedf398_0.jpg"));
					disguises.Add(new("Life Guard", "outfit_53415cf7-8d62-45b9-943f-d1a50c7c6024_0.jpg"));
					disguises.Add(new("Masseur", "outfit_dec42c4a-3ff0-451f-80b0-a01e68310286_0.jpg"));
					disguises.Add(new("Personal Trainer", "outfit_49e70108-2c8d-4418-8e42-8f63d6ed43af_0.jpg"));
					disguises.Add(new("Resort Guard", "outfit_d4c9507a-b297-46ce-8e9c-4ec479da22a4_0.jpg"));
					disguises.Add(new("Resort Staff", "outfit_e9fa4892-fa2a-40a1-a51c-78d8561034f3_0.jpg"));
					disguises.Add(new("Snorkel Instructor", "outfit_30164cfe-a26b-4a72-8bc2-5bc99c0283c1_0.jpg"));
					disguises.Add(new("Tech Crew", "outfit_f6e37038-98c1-4e58-bd85-c895f5c19d56_0.jpg"));
					disguises.Add(new("Villa Guard", "outfit_33e3a400-0bbc-4edd-b07f-056135329802_0.jpg"));
					disguises.Add(new("Villa Staff", "outfit_cda86b1b-63a4-4e3a-975e-d716685335a7_0.jpg"));
					disguises.Add(new("Waiter", "outfit_a260d9d6-a33c-499e-a6c5-698cfcc3de8f_0.jpg"));
					break;
				case MissionID.DUBAI_ONTOPOFTHEWORLD:
					disguises.Add(new("Suit", "outfit_07ab08e1-013e-439d-a98b-3b7e8c9f13bc_0.jpg", true));
					disguises.Add(new("Art Crew", "outfit_2c649c52-f85a-4b29-838a-31c2525cc862_0.jpg"));
					disguises.Add(new("Event Security", "outfit_eb12cc2b-6dcf-4831-ba4e-ef8e53180e2f_0.jpg"));
					disguises.Add(new("Event Staff", "outfit_77fb4c80-0b81-4672-be65-12c16c3ac7ac_0.jpg"));
					disguises.Add(new("Famous Chef", "outfit_6dcf16f6-6620-410f-b51c-179f75de938c_0.jpg"));
					disguises.Add(new("Helicopter Pilot", "outfit_ea5b1cea-c305-4f60-9512-78b2e6cd5030_0.jpg"));
					disguises.Add(new("Ingram's Bodyguard", "outfit_bdbd806d-eb11-4167-bd2d-f5f015c3fe86_0.jpg"));
					disguises.Add(new("Kitchen Staff", "outfit_eb15e523-713f-41ba-ad67-d33b02de43c6_0.jpg"));
					disguises.Add(new("Maintenance Staff", "outfit_e65f04b2-47a6-4d3d-b36c-9fb7fa08a00b_0.jpg"));
					disguises.Add(new("Penthouse Guard", "outfit_f0a52fef-608a-4fa8-9fd6-bd5c15506188_0.jpg"));
					disguises.Add(new("Penthouse Staff", "outfit_a745ca17-3a7e-4c15-8219-6a5d6245ac7f_0.jpg"));
					disguises.Add(new("Skydiving Suit", "outfit_c4146f27-81a9-42ef-b3c7-87a9d60b87fe_0.jpg"));
					disguises.Add(new("The Assassin", "outfit_ef9dddc5-25c7-450f-afcb-ac1b8f9569c9_0.jpg"));
					break;
				case MissionID.DARTMOOR_DEATHINTHEFAMILY:
					disguises.Add(new("Suit", "outfit_a9de864e-ce00-4970-978a-4a9f8db73974_0.jpg", true));
					disguises.Add(new("Bodyguard", "outfit_29389af2-4fe4-4f72-917a-d9747adc0f3d_0.jpg"));
					disguises.Add(new("Gardener", "outfit_88246749-2118-2101-5575-991052571240_0.jpg"));
					disguises.Add(new("Lawyer", "outfit_ffb2e3a8-728b-4a54-95cb-55eaf616b422_0.jpg"));
					disguises.Add(new("Mansion Guard", "outfit_c3349736-91d1-48e3-bc62-fc16a7d8d6f1_0.jpg"));
					disguises.Add(new("Mansion Staff", "outfit_4115e440-fdf8-44d2-a3ba-a1bb2285e542_0.jpg"));
					disguises.Add(new("Photographer", "outfit_7062bd6b-4926-4ab3-932c-de7d63c095b7_0.jpg"));
					disguises.Add(new("Private Investigator", "outfit_12f5bdb5-7e71-4f48-9740-13d0211f48c6_0.jpg"));
					disguises.Add(new("Undertaker", "outfit_dc3c386d-52c2-4e17-855d-6c15e080ccf3_0.jpg"));
					break;
				case MissionID.BERLIN_APEXPREDATOR:
					disguises.Add(new("Suit", "outfit_19e3757f-01b5-4821-97c3-1a1045646531_0.jpg", true));
					disguises.Add(new("Bartender", "outfit_816cf012-ab64-48a0-b4cc-ff7470874007_0.jpg"));
					disguises.Add(new("Biker", "outfit_95918f14-fa9f-4315-be95-bf4b9efe6ee6_0.jpg"));
					disguises.Add(new("Club Crew", "outfit_6e84215c-28b7-44b2-9d15-83e9be490965_0.jpg"));
					disguises.Add(new("Club Security", "outfit_590629f7-19a3-4eb8-88a6-94e550cd1c07_0.jpg"));
					disguises.Add(new("Dealer", "outfit_4c379903-4cf2-49cf-953f-db7b31d2987d_0.jpg"));
					disguises.Add(new("Delivery Guy", "outfit_2e5bdc9b-822d-4f5f-bc16-bd99729ef4f5_0.jpg"));
					disguises.Add(new("DJ", "outfit_ac636da9-fd3a-4019-816a-6333e0c4298e_0.jpg"));
					disguises.Add(new("Florida Man", "outfit_0e931214-6ba9-4763-b7e1-32ca64dd864a_0.jpg"));
					disguises.Add(new("Rolf Hirschmüller", "outfit_8e41db54-b097-4704-8a88-83043e6557f7_0.jpg"));
					disguises.Add(new("Technician", "outfit_f724d6b9-a45b-425f-84f1-c27dedd1fd07_0.jpg"));
					break;
				case MissionID.CHONGQING_ENDOFANERA:
					disguises.Add(new("Suit", "outfit_90ad022f-0789-413f-bf3d-603c1237c9b1_0.jpg", true));
					disguises.Add(new("Block Guard", "outfit_4dd90d10-ac5f-404f-9c20-4428653ca7ba_0.jpg"));
					disguises.Add(new("Dumpling Cook", "outfit_c5f6dd2a-3600-40be-9a82-bbf5d360c379_0.jpg"));
					disguises.Add(new("Facility Analyst", "outfit_9c07a86c-2d03-417b-b46f-cb433481080e_0.jpg"));
					disguises.Add(new("Facility Engineer", "outfit_8fc343f2-6e9a-4e06-9342-705e5b171895_0.jpg"));
					disguises.Add(new("Facility Guard", "outfit_f5c73d58-a24f-4957-b80d-5efb6771ad9b_0.jpg"));
					disguises.Add(new("Facility Security", "outfit_b3515a1e-4a32-475c-bd61-4fdae243a7e5_0.jpg"));
					disguises.Add(new("Homeless Person", "outfit_ba4e595e-da3b-4902-8622-40889fc088db_0.jpg"));
					disguises.Add(new("Perfect Test Subject", "outfit_9cd5fbd7-903c-4ab7-afe8-01eb755ce9da_0.jpg"));
					disguises.Add(new("Researcher", "outfit_553bb399-2ee0-41bb-a76b-b7ec6d49f5a3_0.jpg"));
					disguises.Add(new("Street Guard", "outfit_86bdb741-6810-4610-8e21-799c93c71849_0.jpg"));
					disguises.Add(new("The Board Member", "outfit_fd1d39d8-db06-47b3-8f4b-eb1febf5ccb7_0.jpg"));
					break;
				case MissionID.MENDOZA_THEFAREWELL:
					disguises.Add(new("Suit", "outfit_6c129ec5-41cb-43f1-837d-ebff54f260c6_0.jpg", true));
					disguises.Add(new("Asado Chef", "outfit_8d105591-dfbe-46aa-8520-f00f986b57e2_0.jpg"));
					disguises.Add(new("Bodyguard", "outfit_aa7dc754-702a-401b-8f84-e806e958869c_0.jpg"));
					disguises.Add(new("Chief Winemaker", "outfit_af56d687-ba1b-44c8-8061-fd4a4a1222a3_0.jpg"));
					disguises.Add(new("Corvo Black", "outfit_214b2143-3277-44cd-b20f-344747fc23d9_0.jpg"));
					disguises.Add(new("Gaucho", "outfit_e887e53a-6b02-455d-88be-284af6d88e94_0.jpg"));
					disguises.Add(new("Head of Security", "outfit_16df6808-97ac-4c3a-8d4b-7ddacfc8a7ea_0.jpg"));
					disguises.Add(new("Lawyer", "outfit_521ed265-2115-4977-8db0-45404b067102_0.jpg"));
					disguises.Add(new("Mercenary", "outfit_69d4d32b-0fc9-4fde-8817-fafd98c13365_0.jpg"));
					disguises.Add(new("Providence Herald", "outfit_f5b24132-7a6b-4a3f-868d-193b8692a52b_0.jpg"));
					disguises.Add(new("Sommelier", "outfit_7fed7c24-08b2-468b-8e49-22b5ad59b56b_0.jpg"));
					disguises.Add(new("Tango Musician", "outfit_6ab03e04-9e88-4237-a596-96e3135420ab_0.jpg"));
					disguises.Add(new("Waiter", "outfit_cac0081e-9eb0-4fbf-ba23-70c2815f0874_0.jpg"));
					disguises.Add(new("Winery Worker", "outfit_bbdfca80-abef-4b43-953e-9a46c3eee2eb_0.jpg"));
					break;
				case MissionID.CARPATHIAN_UNTOUCHABLE:
					disguises.Add(new("Suit", "outfit_e1d1ffa6-deca-445a-8e8c-db74b0856cee_0.jpg", true));
					disguises.Add(new("Office Staff", "outfit_81fc37ca-e20b-495f-961f-d5be311a6e6d_0.jpg"));
					disguises.Add(new("Providence Commando", "outfit_e77b5340-41d3-448a-84d3-a4f7f6426634_0.jpg"));
					disguises.Add(new("Providence Commando Leader", "outfit_36402728-1197-4a3c-a8ac-1fed399a2344_0.jpg"));
					disguises.Add(new("Providence Doctor", "outfit_abe4b536-1f09-421e-916b-20af142c6adb_0.jpg"));
					disguises.Add(new("Providence Elite Guard", "outfit_68225457-66b3-457c-a6ec-065b001f5151_0.jpg"));
					disguises.Add(new("Providence Security Guard (Militia Zone)", "outfit_c3239200-0f56-4b45-9be5-e514bdf59d26_0.jpg"));
					disguises.Add(new("Providence Security Guard (Office)", "outfit_653ad7d6-7d5d-4554-9551-7573be7205be_0.jpg"));
					break;
				case MissionID.AMBROSE_SHADOWSINTHEWATER:
					disguises.Add(new("Suit", "outfit_8b74c103-ec0d-4e4e-8664-d06dfe869e8f_0.jpg", true));
					disguises.Add(new("Cook", "outfit_d9d95b38-3708-4220-9838-597c078a1081_0.jpg"));
					disguises.Add(new("Engineer", "outfit_c3af265c-7648-4ddb-a02b-ab605a053886_0.jpg"));
					disguises.Add(new("Hippie", "outfit_9f95337d-0316-4cfc-9881-13080e2bc365_0.jpg"));
					disguises.Add(new("Metal Worker", "outfit_f011f287-ea39-42a4-be1d-17ba5b783611_0.jpg"));
					disguises.Add(new("Militia Soldier", "outfit_afdb3b2b-7e5d-4c9a-be6e-4ebc41879e02_0.jpg"));
					disguises.Add(new("Pirate", "outfit_1cec2601-c1ed-474f-ac70-ff8614799fcc_0.jpg"));
					break;
			}
			return disguises;
		}
		public static Disguise GetDisguiseByName(MissionID mission, string name)
		{
			var disguises = GetDisguises(mission);
			return disguises.Find(d => d.Name == name);
		}

		public static Dictionary<string, Disguise> GetDisguiseDictionary(MissionID mission)
		{
			var disguises = GetDisguises(mission);
			var dict = new Dictionary<string, Disguise>();
			foreach (var item in disguises) {
				dict.Add(TokenCharacterRegex.Replace(item.Name, "").ToLower(), item);
			}
			return dict;
		}

		public static Disguise GetSuitDisguise(MissionID mission)
		{
			var disguises = GetDisguises(mission);
			foreach (var item in disguises) {
				if (item.Suit) return item;
			}
			return null;
		}

		public static List<Target> MakeTargetList(MissionID mission) {
			return mission switch {
				MissionID.ICAFACILITY_GUIDED or MissionID.ICAFACILITY_FREEFORM => [Target.Targets["KR"]],
				MissionID.ICAFACILITY_FINALTEST => [Target.Targets["JK"]],
				MissionID.PARIS_SHOWSTOPPER => [Target.Targets["VN"], Target.Targets["DM"]],
				MissionID.PARIS_HOLIDAYHOARDERS => [Target.Targets["HSB"], Target.Targets["MSG"]],
				MissionID.SAPIENZA_WORLDOFTOMORROW => [Target.Targets["SC"], Target.Targets["FDS"]],
				MissionID.SAPIENZA_THEICON => [Target.Targets["DB"]],
				MissionID.SAPIENZA_LANDSLIDE => [Target.Targets["MA"]],
				MissionID.SAPIENZA_THEAUTHOR => [Target.Targets["CB"], Target.Targets["BA"]],
				MissionID.MARRAKESH_GILDEDCAGE => [Target.Targets["CHS"], Target.Targets["RZ"]],
				MissionID.MARRAKESH_HOUSEBUILTONSAND => [Target.Targets["KTK"], Target.Targets["MM"]],
				MissionID.BANGKOK_CLUB27 => [Target.Targets["JC"], Target.Targets["KM"]],
				MissionID.BANGKOK_THESOURCE => [Target.Targets["ON"], Target.Targets["SY"]],
				MissionID.COLORADO_FREEDOMFIGHTERS => [Target.Targets["SR"], Target.Targets["PG"], Target.Targets["EB"], Target.Targets["MP"]],
				MissionID.HOKKAIDO_SITUSINVERSUS => [Target.Targets["ES"], Target.Targets["YY"]],
				MissionID.HOKKAIDO_PATIENTZERO => [Target.Targets["OC"], Target.Targets["KL"]],
				MissionID.HOKKAIDO_SNOWFESTIVAL => [Target.Targets["DF"]],
				MissionID.HAWKESBAY_NIGHTCALL => [Target.Targets["AR"]],
				MissionID.MIAMI_FINISHLINE => [Target.Targets["SK"], Target.Targets["RK"]],
				MissionID.MIAMI_ASILVERTONGUE => [Target.Targets["AJ"]],
				MissionID.SANTAFORTUNA_THREEHEADEDSERPENT => [Target.Targets["RD"], Target.Targets["JF"], Target.Targets["AM"]],
				MissionID.SANTAFORTUNA_EMBRACEOFTHESERPENT => [Target.Targets["BR"]],
				MissionID.MUMBAI_CHASINGAGHOST => [Target.Targets["WK"], Target.Targets["VS"], Target.Targets["DR"]],
				MissionID.MUMBAI_ILLUSIONSOFGRANDEUR => [Target.Targets["BC"]],
				MissionID.WHITTLETON_ANOTHERLIFE => [Target.Targets["J"], Target.Targets["NC"]],
				MissionID.WHITTLETON_ABITTERPILL => [Target.Targets["GV"]],
				MissionID.ISLEOFSGAIL_THEARKSOCIETY => [Target.Targets["ZW"], Target.Targets["SW"]],
				MissionID.NEWYORK_GOLDENHANDSHAKE => [Target.Targets["AS"]],
				MissionID.HAVEN_THELASTRESORT => [Target.Targets["TW"], Target.Targets["SB"], Target.Targets["LV"]],
				MissionID.DUBAI_ONTOPOFTHEWORLD => [Target.Targets["CI"], Target.Targets["MS"]],
				MissionID.DARTMOOR_DEATHINTHEFAMILY => [Target.Targets["AC"]],
				MissionID.BERLIN_APEXPREDATOR => [Target.Targets["1"], Target.Targets["2"], Target.Targets["3"], Target.Targets["4"], Target.Targets["5"]],
				MissionID.CHONGQING_ENDOFANERA => [Target.Targets["H"], Target.Targets["IR"]],
				MissionID.MENDOZA_THEFAREWELL => [Target.Targets["DY"], Target.Targets["TV"]],
				MissionID.CARPATHIAN_UNTOUCHABLE => [Target.Targets["AE"]],
				MissionID.AMBROSE_SHADOWSINTHEWATER => [Target.Targets["NCR"], Target.Targets["SV"]],
				_ => [],
			};
		}

		[GeneratedRegex("[^a-zA-Z0-9]")]
		private static partial Regex CreateTokenCharacterRegex();
	}
}
