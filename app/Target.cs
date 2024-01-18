using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Croupier.UI
{
	public enum TargetType
	{
		Normal,
		Soders,
	}

	public class TargetKillMethodTags
	{
		public StandardKillMethod? standard;
		public FirearmKillMethod? firearm;
		public SpecificKillMethod? specific;
		public List<MethodTag> tags = [];

		public TargetKillMethodTags(StandardKillMethod? standard, MethodTag[] tags)
		{
			this.standard = standard;
			this.tags = [..tags];
		}

		public TargetKillMethodTags(FirearmKillMethod? firearm, MethodTag[] tags)
		{
			this.firearm = firearm;
			this.tags = [..tags];
		}

		public TargetKillMethodTags(SpecificKillMethod? specific, MethodTag[] tags)
		{
			this.specific = specific;
			this.tags = [..tags];
		}
	}

	public class MethodRule(Func<Disguise, KillMethod, bool> func, MethodTag[] tags)
	{
		public readonly Func<Disguise, KillMethod, bool> Func = func;
		public readonly MethodTag[] Tags = tags;
	}

	public class Target {
		static private readonly Func<Disguise, KillMethod, bool> stalkerRemoteTest = (Disguise disguise, KillMethod method) => {
			if (disguise.Name == "Stalker") return false;
			return !method.IsRemote;
		};
		static private readonly Func<Disguise, KillMethod, bool> loudLiveTest = (Disguise disguise, KillMethod method) => {
			return method.IsLiveFirearm && method.IsLoudWeapon;
		};
		static private readonly Func<Disguise, KillMethod, bool> knightsArmourTrapTest = (Disguise disguise, KillMethod method) => {
			return disguise.Name == "Knight's Armor" && !method.IsRemote;
		};
		static public readonly Dictionary<string, Target> Targets = new() {
			{"KR", new Target() {
				Name = "Kalvin Ritter",
				Image = "polarbear2_sparrow.jpg",
				Mission = MissionID.ICAFACILITY_FREEFORM,
				MethodTags = [
					new(StandardKillMethod.Electrocution, [MethodTag.Impossible]),
					new(StandardKillMethod.Fire, [MethodTag.Impossible]),
					new(StandardKillMethod.InjectedPoison, [MethodTag.Impossible]),
					new(FirearmKillMethod.AssaultRifle, [MethodTag.LoudOnly]),
					new(FirearmKillMethod.Shotgun, [MethodTag.LoudOnly]),
					new(FirearmKillMethod.SMG, [MethodTag.Impossible]),
					new(FirearmKillMethod.Sniper, [MethodTag.Impossible]),
				],
			}},
			{"JK", new Target() {
				Name = "Jasper Knight",
				Image = "polarbear5.jpg",
				Mission = MissionID.ICAFACILITY_FINALTEST,
				MethodTags = [
					new(StandardKillMethod.Electrocution, [MethodTag.Impossible]),
					new(StandardKillMethod.Fire, [MethodTag.Impossible]),
					new(StandardKillMethod.InjectedPoison, [MethodTag.Impossible]),
					new(FirearmKillMethod.AssaultRifle, [MethodTag.LoudOnly]),
					new(FirearmKillMethod.Shotgun, [MethodTag.LoudOnly]),
					new(FirearmKillMethod.SMG, [MethodTag.Impossible]),
					new(FirearmKillMethod.Sniper, [MethodTag.Impossible]),
				],
			}},
			{"VN", new Target() {
				Name = "Viktor Novikov",
				Image = "showstopper_viktor_novikov.jpg",
				Mission = MissionID.PARIS_SHOWSTOPPER,
				MethodTags = [
					new(StandardKillMethod.Fire, [MethodTag.BannedInRR, MethodTag.Extreme, MethodTag.DuplicateOnlySameDisguise]),
				],
			}},
			{"DM", new Target() {
				Name = "Dalia Margolis",
				Image = "showstopper_dahlia_margolis.jpg",
				Mission = MissionID.PARIS_SHOWSTOPPER,
				MethodTags = [
					new(StandardKillMethod.Fire, [MethodTag.BannedInRR, MethodTag.Extreme, MethodTag.DuplicateOnlySameDisguise]),
				],
			}},
			{"HSB", new Target() {
				Name = "Harry \"Smokey\" Bagnato",
				Image = "noel_harry_bagnato.jpg",
				Mission = MissionID.PARIS_HOLIDAYHOARDERS,
				MethodTags = [
					new(StandardKillMethod.Fire, [MethodTag.BannedInRR, MethodTag.Extreme, MethodTag.DuplicateOnlySameDisguise]),
				],
			}},
			{"MSG", new Target() {
				Name = "Marv \"Slick\" Gonif",
				Image = "noel_marv_gonif.jpg",
				Mission = MissionID.PARIS_HOLIDAYHOARDERS,
				MethodTags = [
					new(StandardKillMethod.Fire, [MethodTag.BannedInRR, MethodTag.Extreme, MethodTag.DuplicateOnlySameDisguise]),
				],
			}},
			{"SC", new Target() {
				Name = "Silvio Caruso",
				Image = "world_of_tomorrow_silvio_caruso.jpg",
				Mission = MissionID.SAPIENZA_WORLDOFTOMORROW,
				MethodTags = [
					new(StandardKillMethod.ConsumedPoison, [MethodTag.BannedInRR, MethodTag.Buggy]),
					new(StandardKillMethod.Fire, [MethodTag.BannedInRR, MethodTag.Impossible]),
				],
			}},
			{"FDS", new Target() {
				Name = "Francesca De Santis",
				Image = "world_of_tomorrow_francesca_de_santis.jpg",
				Mission = MissionID.SAPIENZA_WORLDOFTOMORROW,
				MethodTags = [
					new(StandardKillMethod.Fire, [MethodTag.BannedInRR, MethodTag.Impossible]),
				],
			}},
			{"DB", new Target() {
				Name = "Dino Bosco",
				Image = "copperhead_roman_strauss_levine.jpg",
				Mission = MissionID.SAPIENZA_THEICON,
			}},
			{"MA", new Target() {
				Name = "Marco Abiatti",
				Image = "mamba_marco_abiatti.jpg",
				Mission = MissionID.SAPIENZA_LANDSLIDE,
				MethodTags = [
					new(StandardKillMethod.Fire, [MethodTag.BannedInRR, MethodTag.Impossible]),
				],
			}},
			{"CB", new Target() {
				Name = "Craig Black",
				Image = "ws_ebola_craig_black.jpg",
				Mission = MissionID.SAPIENZA_THEAUTHOR,
				MethodTags = [
					new(StandardKillMethod.Fire, [MethodTag.BannedInRR, MethodTag.Impossible]),
				],
			}},
			{"BA", new Target() {
				Name = "Brother Akram",
				Image = "ws_ebola_brother_akram.jpg",
				Mission = MissionID.SAPIENZA_THEAUTHOR,
				MethodTags = [
					new(StandardKillMethod.Fire, [MethodTag.BannedInRR, MethodTag.Impossible]),
				],
			}},
			{"CHS", new Target() {
				Name = "Claus Hugo Strandberg",
				Image = "tobigforjail_claus_hugo_stranberg.jpg",
				Mission = MissionID.MARRAKESH_GILDEDCAGE,
				MethodTags = [
					new(StandardKillMethod.FallingObject, [MethodTag.BannedInRR, MethodTag.Extreme]),
				],
				Rules = {
					new((Disguise disguise, KillMethod method) => {
						if (disguise.Name != "Prisoner") return false;
						return !method.IsRemote;
					}, [MethodTag.BannedInRR, MethodTag.Hard]),
				},
			}},
			{"RZ", new Target() {
				Name = "Reza Zaydan",
				Image = "tobigforjail_general_zaydan.jpg",
				Mission = MissionID.MARRAKESH_GILDEDCAGE,
				MethodTags = [
					new(StandardKillMethod.Drowning, [MethodTag.BannedInRR, MethodTag.Extreme]),
					new(StandardKillMethod.Electrocution, [MethodTag.BannedInRR, MethodTag.Extreme]),
				],
			}},
			{"KTK", new Target() {
				Name = "Kong Tuo-Kwang",
				Image = "python_kong_tou_kwang_briefing.jpg",
				Mission = MissionID.MARRAKESH_HOUSEBUILTONSAND,
			}},
			{"MM", new Target() {
				Name = "Matthieu Mendola",
				Image = "python_matthieu_mendola_briefing.jpg",
				Mission = MissionID.MARRAKESH_HOUSEBUILTONSAND,
			}},
			{"JC", new Target() {
				Name = "Jordan Cross",
				Image = "club27_jordan_cross.jpg",
				Mission = MissionID.BANGKOK_CLUB27,
				MethodTags = [
					new(StandardKillMethod.Fire, [MethodTag.BannedInRR, MethodTag.Extreme]),
				],
				Rules = {
					new(stalkerRemoteTest, [MethodTag.BannedInRR, MethodTag.Hard])
				},
			}},
			{"KM", new Target() {
				Name = "Ken Morgan",
				Image = "club27_ken_morgan.jpg",
				Mission = MissionID.BANGKOK_CLUB27,
				MethodTags = [
					new(StandardKillMethod.Fire, [MethodTag.BannedInRR, MethodTag.Extreme]),
				],
				Rules = [
					new(stalkerRemoteTest, [MethodTag.BannedInRR, MethodTag.Hard])
				],
			}},
			{"ON", new Target() {
				Name = "Oybek Nabazov",
				Image = "ws_zika_oybek_nabazov.jpg",
				Mission = MissionID.BANGKOK_THESOURCE,
				MethodTags = [
					new(StandardKillMethod.FallingObject, [MethodTag.BannedInRR, MethodTag.Extreme]),
				],
			}},
			{"SY", new Target() {
				Name = "Sister Yulduz",
				Image = "ws_zika_sister_yulduz.jpg",
				Mission = MissionID.BANGKOK_THESOURCE,
				MethodTags = [
					new(StandardKillMethod.FallingObject, [MethodTag.BannedInRR, MethodTag.Extreme]),
				],
			}},
			{"SR", new Target() {
				Name = "Sean Rose",
				Image = "freedom_fighters_sean_rose.jpg",
				Mission = MissionID.COLORADO_FREEDOMFIGHTERS,
				MethodTags = [
					new(StandardKillMethod.Drowning, [ MethodTag.BannedInRR, MethodTag.Extreme ]),
					new(StandardKillMethod.ConsumedPoison, [ MethodTag.BannedInRR, MethodTag.Impossible ]),
				],
				Rules = [
					new(loudLiveTest, [ MethodTag.BannedInRR, MethodTag.Hard ])
				]
			}},
			{"PG", new Target() {
				Name = "Penelope Graves",
				Image = "freedom_fighters_penelope_graves.jpg",
				Mission = MissionID.COLORADO_FREEDOMFIGHTERS,
				MethodTags = [
					new(StandardKillMethod.Drowning, [MethodTag.BannedInRR, MethodTag.Extreme]),
					new(StandardKillMethod.Fire, [MethodTag.BannedInRR, MethodTag.Hard]),
				],
				Rules = [
					new(loudLiveTest, [MethodTag.BannedInRR, MethodTag.Hard])
				],
			}},
			{"EB", new Target() {
				Name = "Ezra Berg",
				Image = "freedom_fighters_ezra_berg.jpg",
				Mission = MissionID.COLORADO_FREEDOMFIGHTERS,
				MethodTags = [
					new(StandardKillMethod.ConsumedPoison, [MethodTag.BannedInRR, MethodTag.Impossible]),
					new(StandardKillMethod.Drowning, [MethodTag.BannedInRR, MethodTag.Extreme]),
					new(StandardKillMethod.Electrocution, [MethodTag.BannedInRR, MethodTag.Extreme]),
				],
			}},
			{"MP", new Target() {
				Name = "Maya Parvati",
				Image = "freedom_fighters_maya_parvati.jpg",
				Mission = MissionID.COLORADO_FREEDOMFIGHTERS,
				MethodTags = [
					new(StandardKillMethod.Drowning, [MethodTag.BannedInRR, MethodTag.Extreme]),
				],
			}},
			{"ES", new Target() {
				Name = "Erich Soders",
				Image = "snowcrane_erich_soders_briefing.jpg",
				Mission = MissionID.HOKKAIDO_SITUSINVERSUS,
				Type = TargetType.Soders,
			}},
			{"YY", new Target() {
				Name = "Yuki Yamazaki",
				Image = "snowcrane_yuki_yamazaki_briefing.jpg",
				Mission = MissionID.HOKKAIDO_SITUSINVERSUS,
				MethodTags = [
					new(StandardKillMethod.Fire, [MethodTag.BannedInRR]),
				],
			}},
			{"OC", new Target() {
				Name = "Owen Cage",
				Image = "ws_flu_owen_cage.jpg",
				Mission = MissionID.HOKKAIDO_PATIENTZERO,
				MethodTags = [
					new(StandardKillMethod.Drowning, [MethodTag.BannedInRR, MethodTag.Extreme]),
				],
			}},
			{"KL", new Target() {
				Name = "Klaus Liebleid",
				Image = "ws_flu_klaus_leiblied.jpg",
				Mission = MissionID.HOKKAIDO_PATIENTZERO,
			}},
			{"DF", new Target() {
				Name = "Dmitri Fedorov",
				Image = "mamushi_dimitri-fedorov.jpg",
				Mission = MissionID.HOKKAIDO_SNOWFESTIVAL,
			}},
			{"AR", new Target() {
				Name = "Alma Reynard",
				Image = "sheep_alma_reynard.jpg",
				Mission = MissionID.HAWKESBAY_NIGHTCALL,
				MethodTags = [
					new(StandardKillMethod.Fire, [MethodTag.BannedInRR, MethodTag.Impossible]),
					new(StandardKillMethod.FallingObject, [MethodTag.BannedInRR, MethodTag.Impossible]),
				],
			}},
			{"SK", new Target() {
				Name = "Sierra Knox",
				Image = "flamingo_sierra_knox.jpg",
				Mission = MissionID.MIAMI_FINISHLINE,
			}},
			{"RK", new Target() {
				Name = "Robert Knox",
				Image = "flamingo_robert_knox.jpg",
				Mission = MissionID.MIAMI_FINISHLINE,
				MethodTags = [
					new(StandardKillMethod.Fire, [MethodTag.BannedInRR, MethodTag.Extreme]),
				],
			}},
			{"AJ", new Target() {
				Name = "Ajit \"AJ\" Krish",
				Image = "cottonmouth_ajit_krish.jpg",
				Mission = MissionID.MIAMI_ASILVERTONGUE,
			}},
			{"RD", new Target() {
				Name = "Rico Delgado",
				Image = "hippo_rico_delgado.jpg",
				Mission = MissionID.SANTAFORTUNA_THREEHEADEDSERPENT,
				MethodTags = [
					new(StandardKillMethod.Fire, [MethodTag.BannedInRR, MethodTag.Extreme]),
					new(StandardKillMethod.ConsumedPoison, [MethodTag.BannedInRR, MethodTag.Extreme]),
				],
			}},
			{"JF", new Target() {
				Name = "Jorge Franco",
				Image = "hippo_jorge_franco.jpg",
				Mission = MissionID.SANTAFORTUNA_THREEHEADEDSERPENT,
				MethodTags = [
					new(StandardKillMethod.FallingObject, [MethodTag.BannedInRR, MethodTag.Impossible]),
				],
			}},
			{"AM", new Target() {
				Name = "Andrea Martinez",
				Image = "hippo_andrea_martinez.jpg",
				Mission = MissionID.SANTAFORTUNA_THREEHEADEDSERPENT,
				MethodTags = [
					new(StandardKillMethod.Fire, [MethodTag.BannedInRR, MethodTag.Extreme]),
				],
			}},
			{"BR", new Target() {
				Name = "Blair Reddington",
				Image = "anaconda_blair_reddington_face.jpg",
				Mission = MissionID.SANTAFORTUNA_EMBRACEOFTHESERPENT,
			}},
			{"WK", new Target() {
				Name = "Wazir Kale",
				Image = "mongoose_wazir_kale_identified.jpg",
				Mission = MissionID.MUMBAI_CHASINGAGHOST,
				MethodTags = [
					new(StandardKillMethod.Fire, [MethodTag.BannedInRR, MethodTag.Extreme]),
					new(StandardKillMethod.Drowning, [MethodTag.BannedInRR, MethodTag.Extreme]),
				],
				Rules = [
					new(loudLiveTest, [MethodTag.BannedInRR, MethodTag.Extreme]),
				],
			}},
			{"VS", new Target() {
				Name = "Vanya Shah",
				Image = "mongoose_vanya_shah.jpg",
				Mission = MissionID.MUMBAI_CHASINGAGHOST,
				MethodTags = [
					new(StandardKillMethod.Drowning, [MethodTag.BannedInRR, MethodTag.Extreme]),
					new(StandardKillMethod.ConsumedPoison, [MethodTag.BannedInRR, MethodTag.Extreme]),
				],
				Rules = [
					new(loudLiveTest, [MethodTag.BannedInRR, MethodTag.Extreme]),
				],
			}},
			{"DR", new Target() {
				Name = "Dawood Rangan",
				Image = "mongoose_dawood_rangan.jpg",
				Mission = MissionID.MUMBAI_CHASINGAGHOST,
				MethodTags = [
					new(StandardKillMethod.Fire, [MethodTag.BannedInRR, MethodTag.Extreme]),
					new(StandardKillMethod.ConsumedPoison, [MethodTag.BannedInRR, MethodTag.Extreme]),
				],
				Rules = {
					new MethodRule(loudLiveTest, [MethodTag.BannedInRR, MethodTag.Hard]),
				},
			}},
			{"BC", new Target() {
				Name = "Basil Carnaby",
				Image = "kingcobra_basil_carnaby_face.jpg",
				Mission = MissionID.MUMBAI_ILLUSIONSOFGRANDEUR,
			}},
			{"J", new Target() {
				Name = "Janus",
				Image = "skunk_janus.jpg",
				Mission = MissionID.WHITTLETON_ANOTHERLIFE,
				MethodTags = [
					new(StandardKillMethod.Fire, [MethodTag.BannedInRR, MethodTag.Impossible]),
					new(StandardKillMethod.FallingObject, [MethodTag.BannedInRR, MethodTag.Impossible]),
					new(SpecificKillMethod.BattleAxe, [MethodTag.BannedInRR, MethodTag.Extreme]),
					new(SpecificKillMethod.BeakStaff, [MethodTag.BannedInRR, MethodTag.Extreme]),
				],
			}},
			{"NC", new Target() {
				Name = "Nolan Cassidy",
				Image = "skunk_nolan_cassidy.jpg",
				Mission = MissionID.WHITTLETON_ANOTHERLIFE,
				MethodTags = [
					new(StandardKillMethod.Fire, [MethodTag.BannedInRR, MethodTag.Impossible]),
					new(SpecificKillMethod.BattleAxe, [MethodTag.BannedInRR, MethodTag.Extreme]),
					new(SpecificKillMethod.BeakStaff, [MethodTag.BannedInRR, MethodTag.Extreme]),
				],
			}},
			{"GV", new Target() {
				Name = "Galen Vholes",
				Image = "gartersnake_ghalen_vholes.jpg",
				Mission = MissionID.WHITTLETON_ABITTERPILL,
			}},
			{"ZW", new Target() {
				Name = "Zoe Washington",
				Image = "magpie_zoe_washington.jpg",
				Mission = MissionID.ISLEOFSGAIL_THEARKSOCIETY,
				MethodTags = [
					new(StandardKillMethod.ConsumedPoison, [MethodTag.BannedInRR, MethodTag.Impossible]),
					new(StandardKillMethod.Drowning, [MethodTag.BannedInRR, MethodTag.Hard]),
				],
				Rules = {
					new(knightsArmourTrapTest, [MethodTag.BannedInRR, MethodTag.Extreme])
				},
			}},
			{"SW", new Target() {
				Name = "Sophia Washington",
				Image = "magpie_serena_washington.jpg",
				Mission = MissionID.ISLEOFSGAIL_THEARKSOCIETY,
				MethodTags = [
					new(StandardKillMethod.ConsumedPoison, [MethodTag.BannedInRR, MethodTag.Extreme]),
					new(StandardKillMethod.Drowning, [MethodTag.BannedInRR, MethodTag.Hard]),
					new(StandardKillMethod.Fire, [MethodTag.BannedInRR, MethodTag.Hard]),
				],
				Rules = [
					new(knightsArmourTrapTest, [MethodTag.BannedInRR, MethodTag.Extreme])
				],
			}},
			{"AS", new Target() {
				Name = "Athena Savalas",
				Image = "racoon_athena_savalas.jpg",
				Mission = MissionID.NEWYORK_GOLDENHANDSHAKE,
				MethodTags = [
					new(StandardKillMethod.Fire, [MethodTag.BannedInRR, MethodTag.Impossible]),
				],
			}},
			{"TW", new Target() {
				Name = "Tyson Williams",
				Image = "stingray_tyson_williams.jpg",
				Mission = MissionID.HAVEN_THELASTRESORT,
				MethodTags = [
					new(StandardKillMethod.ConsumedPoison, [MethodTag.BannedInRR, MethodTag.Extreme]),
					new(StandardKillMethod.Fire, [MethodTag.BannedInRR, MethodTag.Impossible]),
				],
			}},
			{"SB", new Target() {
				Name = "Steven Bradley",
				Image = "stingray_steven_bradley.jpg",
				Mission = MissionID.HAVEN_THELASTRESORT,
				MethodTags = [
					new(StandardKillMethod.ConsumedPoison, [MethodTag.BannedInRR, MethodTag.Impossible]),
					new(StandardKillMethod.Fire, [MethodTag.BannedInRR, MethodTag.Impossible]),
				],
			}},
			{"LV", new Target() {
				Name = "Ljudmila Vetrova",
				Image = "stingray_ljudmila_vetrova.jpg",
				Mission = MissionID.HAVEN_THELASTRESORT,
				MethodTags = [
					new(StandardKillMethod.FallingObject, [MethodTag.BannedInRR, MethodTag.Hard]),
					new(StandardKillMethod.Fire, [MethodTag.BannedInRR, MethodTag.Impossible]),
				],
			}},
			{"CI", new Target() {
				Name = "Carl Ingram",
				Image = "golden_carl_ingram.jpg",
				Mission = MissionID.DUBAI_ONTOPOFTHEWORLD,
				MethodTags = [
					new(StandardKillMethod.ConsumedPoison, [MethodTag.BannedInRR, MethodTag.Buggy]),
					new(StandardKillMethod.Fire, [MethodTag.BannedInRR, MethodTag.Extreme]),
				],
			}},
			{"MS", new Target() {
				Name = "Marcus Stuyvesant",
				Image = "golden_marcus_stuyvesant.jpg",
				Mission = MissionID.DUBAI_ONTOPOFTHEWORLD,
				MethodTags = [
					new(StandardKillMethod.ConsumedPoison, [MethodTag.BannedInRR, MethodTag.Extreme]),
					new(StandardKillMethod.Fire, [MethodTag.BannedInRR, MethodTag.Extreme]),
				],
			}},
			{"AC", new Target() {
				Name = "Alexa Carlisle",
				Image = "ancestral_alexa_carlisle.jpg",
				Mission = MissionID.DARTMOOR_DEATHINTHEFAMILY,
				MethodTags = [
					new(StandardKillMethod.Fire, [MethodTag.BannedInRR, MethodTag.Impossible]),
				],
			}},
			{"1", new Target() {
				Name = "ICA Agent #1",
				Image = "fox_pickup_earpiece.jpg",
				Mission = MissionID.BERLIN_APEXPREDATOR,
			}},
			{"2", new Target() {
				Name = "ICA Agent #2",
				Image = "fox_pickup_earpiece.jpg",
				Mission = MissionID.BERLIN_APEXPREDATOR,
			}},
			{"3", new Target() {
				Name = "ICA Agent #3",
				Image = "fox_pickup_earpiece.jpg",
				Mission = MissionID.BERLIN_APEXPREDATOR,
			}},
			{"4", new Target() {
				Name = "ICA Agent #4",
				Image = "fox_pickup_earpiece.jpg",
				Mission = MissionID.BERLIN_APEXPREDATOR,
			}},
			{"5", new Target() {
				Name = "ICA Agent #5",
				Image = "fox_pickup_earpiece.jpg",
				Mission = MissionID.BERLIN_APEXPREDATOR,
			}},
			{"H", new Target() {
				Name = "Hush",
				Image = "wet_hush.jpg",
				Mission = MissionID.CHONGQING_ENDOFANERA,
				MethodTags = [
					new(StandardKillMethod.ConsumedPoison, [MethodTag.BannedInRR, MethodTag.Extreme]),
					new(StandardKillMethod.Fire, [MethodTag.BannedInRR, MethodTag.Extreme]),
				],
				Rules = [
					new(loudLiveTest, [MethodTag.BannedInRR, MethodTag.Hard]),
				],
			}},
			{"IR", new Target() {
				Name = "Imogen Royce",
				Image = "wet_imogen_royce.jpg",
				Mission = MissionID.CHONGQING_ENDOFANERA,
				MethodTags = [
					new(StandardKillMethod.ConsumedPoison, [MethodTag.BannedInRR, MethodTag.Hard]),
					new(StandardKillMethod.FallingObject, [MethodTag.BannedInRR, MethodTag.Extreme]),
					new(StandardKillMethod.Fire, [MethodTag.BannedInRR, MethodTag.Hard]),
				],
			}},
			{"DY", new Target() {
				Name = "Don Archibald Yates",
				Image = "elegant_yates.jpg",
				Mission = MissionID.MENDOZA_THEFAREWELL,
				MethodTags = [
					new(StandardKillMethod.ConsumedPoison, [MethodTag.BannedInRR, MethodTag.Hard]),
					new(StandardKillMethod.Fire, [MethodTag.BannedInRR, MethodTag.Extreme]),
				],
			}},
			{"TV", new Target() {
				Name = "Tamara Vidal",
				Image = "elegant_vidal.jpg",
				Mission = MissionID.MENDOZA_THEFAREWELL,
				MethodTags = [
					new(StandardKillMethod.ConsumedPoison, [MethodTag.BannedInRR, MethodTag.Hard]),
					new(StandardKillMethod.FallingObject, [MethodTag.BannedInRR, MethodTag.Hard]),
					new(StandardKillMethod.Fire, [MethodTag.BannedInRR, MethodTag.Extreme]),
				],
			}},
			{"AE", new Target() {
				Name = "Arthur Edwards",
				Image = "trapped_arthur_edwards.jpg",
				Mission = MissionID.CARPATHIAN_UNTOUCHABLE,
				MethodTags = [
					new(StandardKillMethod.Drowning, [MethodTag.BannedInRR, MethodTag.Impossible]),
					new(StandardKillMethod.ConsumedPoison, [MethodTag.BannedInRR, MethodTag.Impossible]),
					new(StandardKillMethod.Electrocution, [MethodTag.BannedInRR, MethodTag.Impossible]),
					new(StandardKillMethod.Explosion, [MethodTag.BannedInRR, MethodTag.Impossible]),
					new(StandardKillMethod.FallingObject, [MethodTag.BannedInRR, MethodTag.Impossible]),
					new(StandardKillMethod.Fire, [MethodTag.BannedInRR, MethodTag.Impossible]),
					new(FirearmKillMethod.Sniper, [MethodTag.BannedInRR, MethodTag.Impossible]),
				],
			}},
			{"NCR", new Target() {
				Name = "Noel Crest",
				Image = "rocky_noel_crest.jpg",
				Mission = MissionID.AMBROSE_SHADOWSINTHEWATER,
			}},
			{"SV", new Target() {
				Name = "Sinhi \"Akka\" Venthan",
				Image = "rocky_sinhi_akka_venthan.jpg",
				Mission = MissionID.AMBROSE_SHADOWSINTHEWATER,
			}},
		};
		static private readonly Dictionary<string, string> targetAliasDict = new() {
			{"kalvinritter", "KR"},
			{"kal", "KR"},
			{"kalvin", "KR"},
			{"ritter", "KR"},
			{"jasper", "JK"},
			{"jasperknight", "JK"},
			{"knight", "JK"},
			{"viktornovikov", "VN"},
			{"vic", "VN"}, {"victor", "VN"},
			{"vik", "VN"}, {"viktor", "VN"}, {"novikov", "VN"},
			{"daliamargolis", "DM"},
			{"dalia", "DM"}, {"dahlia", "DM"}, {"margolis", "DM"},
			{"harrysmokeybagnato", "HSB"},
			{"harrybagnato", "HSB"}, {"smokey", "HSB"}, {"bagnato", "HSB"}, {"hb", "HSB"},
			{"marvslickgonif", "MSG"}, {"marvgonif", "MSG"}, {"slick", "MSG"},
			{"marv", "MSG"}, {"gonif", "MSG"}, {"mg", "MSG"},
			{"silviocaruso", "SC"},
			{"silv", "SC"}, {"silvio", "SC"}, {"caruso", "SC"},
			{"francescadesantis", "FDS"},
			{"fran", "FDS"}, {"frann", "FDS"}, {"franny", "FDS"},
			{"francesca", "FDS"}, {"desantis", "FDS"}, {"santis", "FDS"},
			{"frannydesanny", "FDS"}, {"sanny", "FDS"}, {"desanny", "FDS"},
			{"dinobosco", "DB"},
			{"dino", "DB"}, {"bosco", "DB"}, {"ironman", "DB"},
			{"marcoabiatti", "MA"},
			{"marco", "MA"}, {"abiatti", "MA"},
			{"craigblack", "CB"},
			{"craig", "CB"}, {"black", "CB"},
			{"brotherakram", "BA"},
			{"brother", "BA"}, {"akram", "BA"},
			{"claushugostrandberg", "CHS"},
			{"hugo", "CHS"}, {"claus", "CHS"}, {"strandberg", "CHS"}, {"stranberg", "CHS"},
			{"claustrandberg", "CHS"}, {"clausstranberg", "CHS"}, {"claushugostranberg", "CHS"},
			{"rezazaydan", "RZ"},
			{"reza", "RZ"}, {"rez", "RZ"}, {"zaydan", "RZ"}, {"general", "RZ"}, {"generalrezazaydan", "RZ"}, {"generalzaydan", "RZ"},
			{"kongtuokwang", "KTK"},
			{"kk", "KTK"}, {"kong", "KTK"}, {"tuo", "KTK"}, {"kwang", "KTK"},
			{"tuokwang", "KTK"}, {"kongkwang", "KTK"},
			{"matthieumendola", "MM"},
			{"mat", "MM"}, {"matt", "MM"}, {"matthieu", "MM"}, {"mendola", "MM"},
			{"jordancross", "JC"},
			{"jordan", "JC"}, {"cross", "JC"},
			{"kenmorgan", "KM"},
			{"ken", "KM"}, {"morgan", "KM"}, {"thebrick", "KM"}, {"brick", "KM"}, {"kenthebrickmorgan", "KM"},
			{"oybeknabazov", "ON"},
			{"oybek", "ON"}, {"nabazov", "ON"},
			{"sisteryulduz", "SY"},
			{"sister", "SY"}, {"yulduz", "SY"}, {"sis", "SY"},
			{"seanrose", "SR"},
			{"sean", "SR"}, {"rose", "SR"},
			{"penelopegraves", "PG"},
			{"penelope", "PG"}, {"graves", "PG"}, {"pen", "PG"}, {"penny", "PG"},
			{"ezraberg", "EB"},
			{"ezra", "EB"}, {"berg", "EB"},
			{"mayaparvati", "MP"},
			{"maya", "MP"}, {"parvati", "MP"},
			{"erichsoders", "ES"},
			{"erich", "ES"}, {"ericsoders", "ES"}, {"eric", "ES"}, {"soders", "ES"},
			{"yukiyamazaki" , "YY"},
			{"yuki" , "YY"}, {"yamazaki" , "YY"},
			{"owencage" , "OC"},
			{"owen" , "OC"}, {"cage" , "OC"},
			{"klausliebleid", "KL"},
			{"klaus", "KL"}, {"liebleid", "KL"},
			{"dmitrifedorov", "DF"},
			{"dmitri", "DF"}, {"fedorov", "DF"},
			{"almareynard", "AR"},
			{"alma", "AR"}, {"reynard", "AR"},
			{"sierraknox", "SK"},
			{"sierra", "SK"},
			{"robertknox", "RK"},
			{"robert", "RK"}, {"bob", "RK"}, {"bobby", "RK"}, {"bobbyknox", "RK"}, {"bobert", "RK"},
			{"ajitajkrish", "AJ"},
			{"aak", "AJ"},
			{"ak", "AJ"},
			{"ajitkrish", "AJ"}, {"ajit", "AJ"}, {"krish", "AJ"},
			{"ricodelgado", "RD"},
			{"rico", "RD"}, {"delgado", "RD"},
			{"jorgefranco", "JF"},
			{"jorge", "JF"}, {"franco", "JF"},
			{"andreamartinez", "AM"},
			{"andrea", "AM"}, {"martinez", "AM"},
			{"blairreddington", "BR"},
			{"blair", "BR"}, {"reddington", "BR"},
			{"wazirkale", "WK"},
			{"wazir", "WK"}, {"kale", "WK"}, {"maelstrom", "WK"},
			{"malestrom", "WK"}, {"themaelstrom", "WK"}, {"themalestrom", "WK"},
			{"vanyashah", "VS"},
			{"vanya", "VS"}, {"shah", "VS"},
			{"dawoodrangan", "DR"},
			{"dawood", "DR"}, {"rangan", "DR"},
			{"basilcarnaby", "BC"},
			{"basil", "BC"}, {"carnaby", "BC"},
			{"janus", "J"},
			{"nolancassidy", "NC"},
			{"nolan", "NC"}, {"cassidy", "NC"},
			{"galenvholes", "GV"},
			{"galen", "GV"}, {"vholes", "GV"}, {"gale", "GV"},
			{"zoewashington", "ZW"},
			{"zoe", "ZW"},
			{"sophiawashington", "SW"},
			{"sophia", "ZW"}, {"soph", "ZW"},
			{"athenasavalas", "AS"},
			{"athena", "AS"}, {"savalas", "AS"},
			{"tysonwilliams", "TW"},
			{"tyson", "TW"}, {"williams", "TW"},
			{"stevenbradley", "SB"},
			{"steven", "SB"}, {"steve", "SB"}, {"bradley", "SB"},
			{"ljudmilavetrova", "LV"},
			{"ljudmila", "LV"}, {"vetrova", "LV"}, {"ljud", "LV"},
			{"carlingram", "CI"},
			{"carl", "CI"}, {"ingram", "CI"},
			{"marcusstuyvesant", "MS"},
			{"marcus", "MS"}, {"stuyvesant", "MS"},
			{"alexacarlisle", "AC"},
			{"alexa", "AC"}, {"carlisle", "AC"},
			{"hush", "H"},
			{"imogenroyce", "IR"},
			{"imogen", "IR"}, {"royce", "IR"},
			{"donarchibaldyates", "DY"},
			{"day", "DY"}, {"don", "DY"}, {"yates", "DY"}, {"donyates", "DY"}, {"archibald", "DY"},
			{"tamaravidal", "TV"},
			{"tamara", "TV"}, {"tam", "TV"}, {"vidal", "vidal"},
			{"arthuredwards", "AE"},
			{"arthur", "AE"}, {"edwards", "AE"},
			{"noelcrest", "NCR"},
			{"noel", "NCR"}, {"crest", "NCR"},
			{"sinhiakkaventhan", "SV"},
			{"akka", "SV"}, {"sinhi", "SV"}, {"venthan", "SV"},
		};

		public static string GetTargetKey(string targetName) {
			foreach (var t in Targets) {
				if (t.Value.Name == targetName) return t.Key;
			}
			return targetName;
		}

		public static string GetTargetFromName(string targetName) {
			if (targetAliasDict.TryGetValue(Strings.TokenCharacterRegex.Replace(targetName, "").RemoveDiacritics().ToLower(), out var key))
				return key;
			return targetName;
		}

		public static bool GetTargetFromKey(string key, out Target target) {
			var keyFromName = GetTargetFromName(key);
			if (keyFromName != null)
				key = keyFromName;
			return Targets.TryGetValue(key.ToUpper(), out target);
		}

		public string Name { get; set; }
		public MissionID Mission { get; set; }
		public List<TargetKillMethodTags> MethodTags { get; set; }
		public List<MethodRule> Rules { get; set; } = [];
		public string Image { get; set; }
		public TargetType Type { get; set; } = TargetType.Normal;
		public Uri ImageUri {
			get {
				return new Uri(Path.Combine(Environment.CurrentDirectory, "actors", this.Image));
			}
		}

		public List<MethodTag> TestRules(Disguise disguise, KillMethod method) {
			var broken = new List<MethodTag>();
			Rules.ForEach(rule => {
				if (rule.Func(disguise, method))
					broken.AddRange(rule.Tags);
			});
			return broken;
		}

		public List<MethodTag> GetMethodTags(KillMethod method) {
			var methodTagsList = MethodTags?.Find((TargetKillMethodTags methodTags) => {
				return method.Type switch {
					KillMethodType.Standard => method.Standard == methodTags.standard,
					KillMethodType.Specific => method.Specific == methodTags.specific,
					KillMethodType.Firearm => method.Firearm == methodTags.firearm,
					_ => false,
				};
			});
			return methodTagsList?.tags ?? [];
		}
	}
}
