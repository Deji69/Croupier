using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Croupier
{
	public enum TargetType {
		Normal,
		Soders,
	}

	public enum TargetNameFormat {
		Initials,
		Full,
		Short,
	}

	public enum TargetID {
		Unknown,
		Target1,
		Target2,
		Target3,
		Target4,
		Target5,
		Target6,
		Target7,
		Target8,
		Target9,
		Target10,
		KalvinRitter,
		JasperKnight,
		ViktorNovikov,
		DaliaMargolis,
		HarrySmokeyBagnato,
		MarvSlickGonif,
		SilvioCaruso,
		FrancescaDeSantis,
		DinoBosco,
		MarcoAbiatti,
		CraigBlack,
		BrotherAkram,
		RezaZaydan,
		ClausHugoStrandberg,
		KongTuoKwang,
		MatthieuMendola,
		JordanCross,
		KenMorgan,
		OybekNabazov,
		SisterYulduz,
		SeanRose,
		PenelopeGraves,
		EzraBerg,
		MayaParvati,
		ErichSoders,
		YukiYamazaki,
		OwenCage,
		KlausLiebleid,
		DmitriFedorov,
		AlmaReynard,
		SierraKnox,
		RobertKnox,
		AjitKrish,
		RicoDelgado,
		JorgeFranco,
		AndreaMartinez,
		BlairReddington,
		WazirKale,
		VanyaShah,
		DawoodRangan,
		BasilCarnaby,
		Janus,
		NolanCassidy,
		GalenVholes,
		ZoeWashington,
		SophiaWashington,
		AthenaSavalas,
		TysonWilliams,
		StevenBradley,
		LjudmilaVetrova,
		CarlIngram,
		MarcusStuyvesant,
		AlexaCarlisle,
		Agent1,
		Agent2,
		Agent3,
		Agent4,
		Agent5,
		Agent6,
		Agent7,
		Agent8,
		Agent9,
		Agent10,
		Agent11,
		AgentMontgomery,
		AgentChamberlin,
		AgentDavenport,
		AgentLowenthal,
		AgentThames,
		AgentGreen,
		AgentTremaine,
		AgentBanner,
		AgentSwan,
		AgentRhodes,
		AgentPrice,
		Hush,
		ImogenRoyce,
		DonArchibaldYates,
		TamaraVidal,
		DianaBurnwood,
		ArthurEdwards,
		NoelCrest,
		SinhiAkkaVenthan,
	}

	public static partial class TargetNameFormatMethods {
		public static string ToString(this TargetNameFormat id)
		{
			return id switch {
				TargetNameFormat.Full => "Full",
				TargetNameFormat.Short => "Short",
				_ => "Initials",
			};
		}

		public static TargetNameFormat FromString(string id)
		{
			return id switch {
				"Full" => TargetNameFormat.Full,
				"Short" => TargetNameFormat.Short,
				_ => TargetNameFormat.Initials,
			};
		}
	}

	public class TargetKillMethodTags {
		public StandardKillMethod? standard;
		public FirearmKillMethod? firearm;
		public SpecificKillMethod? specific;
		public List<string> tags = [];

		public TargetKillMethodTags(StandardKillMethod? standard, List<string> tags)
		{
			this.standard = standard;
			this.tags = tags;
		}

		public TargetKillMethodTags(FirearmKillMethod? firearm, List<string> tags)
		{
			this.firearm = firearm;
			this.tags = tags;
		}

		public TargetKillMethodTags(SpecificKillMethod? specific, List<string> tags)
		{
			this.specific = specific;
			this.tags = tags;
		}

		public bool MatchesKillMethod(KillMethod method) {
			return standard == method.Standard
				&& specific == method.Specific
				&& firearm == method.Firearm;
		}
	}

	public class MethodRule(Func<Disguise, KillMethod, Mission, bool> func, List<string> tags)
	{
		public readonly Func<Disguise, KillMethod, Mission, bool> Func = func;
		public readonly List<string> Tags = tags;
	}

	public class TargetInfo(string name, string shortName, string initials, string image, MissionID mission) {
		public string Name = name;
		public string ShortName = shortName;
		public string Initials = initials;
		public string Image = image;
		public MissionID Mission = mission;
	}

	public class Target {
		static public readonly Dictionary<TargetID, TargetInfo> targetInfos = new() {
			{ TargetID.KalvinRitter, new("Kalvin Ritter", "Kalvin", "KR", "polarbear2_sparrow.jpg", MissionID.ICAFACILITY_FREEFORM) },
			{ TargetID.JasperKnight, new("Jasper Knight", "Jasper", "JK", "polarbear5.jpg", MissionID.ICAFACILITY_FINALTEST) },
			{ TargetID.ViktorNovikov, new("Viktor Novikov", "Viktor", "VN", "showstopper_viktor_novikov.jpg", MissionID.PARIS_SHOWSTOPPER) },
			{ TargetID.DaliaMargolis, new("Dalia Margolis", "Dalia", "DM", "showstopper_dahlia_margolis.jpg", MissionID.PARIS_SHOWSTOPPER) },
			{ TargetID.HarrySmokeyBagnato, new("Harry \"Smokey\" Bagnato", "Smokey", "HSB", "noel_harry_bagnato.jpg", MissionID.PARIS_HOLIDAYHOARDERS) },
			{ TargetID.MarvSlickGonif, new("Marv \"Slick\" Gonif", "Slick", "MSG", "noel_marv_gonif.jpg", MissionID.PARIS_HOLIDAYHOARDERS) },
			{ TargetID.SilvioCaruso, new("Silvio Caruso", "Silvio", "SC", "world_of_tomorrow_silvio_caruso.jpg", MissionID.SAPIENZA_WORLDOFTOMORROW) },
			{ TargetID.FrancescaDeSantis, new("Francesca De Santis", "Francesca", "FDS", "world_of_tomorrow_francesca_de_santis.jpg", MissionID.SAPIENZA_WORLDOFTOMORROW) },
			{ TargetID.DinoBosco, new("Dino Bosco", "Dino", "FDS", "copperhead_roman_strauss_levine.jpg", MissionID.SAPIENZA_THEICON) },
			{ TargetID.MarcoAbiatti, new("Marco Abiatti", "Marco", "MA", "mamba_marco_abiatti.jpg", MissionID.SAPIENZA_LANDSLIDE) },
			{ TargetID.CraigBlack, new("Craig Black", "Craig", "CB", "ws_ebola_craig_black.jpg", MissionID.SAPIENZA_THEAUTHOR) },
			{ TargetID.BrotherAkram, new("Brother Akram", "Akram", "BA", "ws_ebola_brother_akram.jpg", MissionID.SAPIENZA_THEAUTHOR) },
			{ TargetID.RezaZaydan, new("Reza Zaydan", "Reza", "RZ", "tobigforjail_general_zaydan.jpg", MissionID.MARRAKESH_GILDEDCAGE) },
			{ TargetID.ClausHugoStrandberg, new("Claus Hugo Strandberg", "Claus", "CHS", "tobigforjail_claus_hugo_stranberg.jpg", MissionID.MARRAKESH_GILDEDCAGE) },
			{ TargetID.KongTuoKwang, new("Kong Tuo-Kwang", "Kong", "KTK", "python_kong_tou_kwang_briefing.jpg", MissionID.MARRAKESH_HOUSEBUILTONSAND) },
			{ TargetID.MatthieuMendola, new("Matthieu Mendola", "Matthieu", "MM", "python_matthieu_mendola_briefing.jpg", MissionID.MARRAKESH_HOUSEBUILTONSAND) },
			{ TargetID.JordanCross, new("Jordan Cross", "Jordan", "JC", "club27_jordan_cross.jpg", MissionID.BANGKOK_CLUB27) },
			{ TargetID.KenMorgan, new("Ken Morgan", "Ken", "KM", "club27_ken_morgan.jpg", MissionID.BANGKOK_CLUB27) },
			{ TargetID.OybekNabazov, new("Oybek Nabazov", "Nabazov", "ON", "ws_zika_oybek_nabazov.jpg", MissionID.BANGKOK_THESOURCE) },
			{ TargetID.SisterYulduz, new("Sister Yulduz", "Yulduz", "SY", "ws_zika_sister_yulduz.jpg", MissionID.BANGKOK_THESOURCE) },
			{ TargetID.SeanRose, new("Sean Rose", "Sean", "SR", "freedom_fighters_sean_rose.jpg", MissionID.COLORADO_FREEDOMFIGHTERS) },
			{ TargetID.PenelopeGraves, new("Penelope Graves", "Penelope", "PG", "freedom_fighters_penelope_graves.jpg", MissionID.COLORADO_FREEDOMFIGHTERS) },
			{ TargetID.EzraBerg, new("Ezra Berg", "Ezra", "EB", "freedom_fighters_ezra_berg.jpg", MissionID.COLORADO_FREEDOMFIGHTERS) },
			{ TargetID.MayaParvati, new("Maya Parvati", "Maya", "MP", "freedom_fighters_maya_parvati.jpg", MissionID.COLORADO_FREEDOMFIGHTERS) },
			{ TargetID.ErichSoders, new("Erich Soders", "Soders", "ES", "snowcrane_erich_soders_briefing.jpg", MissionID.HOKKAIDO_SITUSINVERSUS) },
			{ TargetID.YukiYamazaki, new("Yuki Yamazaki", "Yuki", "YY", "snowcrane_yuki_yamazaki_briefing.jpg", MissionID.HOKKAIDO_SITUSINVERSUS) },
			{ TargetID.OwenCage, new("Owen Cage", "Owen", "OC", "ws_flu_owen_cage.jpg", MissionID.HOKKAIDO_PATIENTZERO) },
			{ TargetID.KlausLiebleid, new("Klaus Liebleid", "Klaus", "KL", "ws_flu_klaus_leiblied.jpg", MissionID.HOKKAIDO_PATIENTZERO) },
			{ TargetID.DmitriFedorov, new("Dmitri Fedorov", "Dmitri", "DF", "mamushi_dimitri-fedorov.jpg", MissionID.HOKKAIDO_SNOWFESTIVAL) },
			{ TargetID.AlmaReynard, new("Alma Reynard", "Alma", "AR", "sheep_alma_reynard.jpg", MissionID.HAWKESBAY_NIGHTCALL) },
			{ TargetID.SierraKnox, new("Sierra Knox", "Sierra", "SK", "flamingo_sierra_knox.jpg", MissionID.MIAMI_FINISHLINE) },
			{ TargetID.RobertKnox, new("Robert Knox", "Robert", "RK", "flamingo_robert_knox.jpg", MissionID.MIAMI_FINISHLINE) },
			{ TargetID.AjitKrish, new("Ajit \"AJ\" Krish", "AJ", "AJ", "cottonmouth_ajit_krish.jpg", MissionID.MIAMI_ASILVERTONGUE) },
			{ TargetID.RicoDelgado, new("Rico Delgado", "Rico", "RD", "hippo_rico_delgado.jpg", MissionID.SANTAFORTUNA_THREEHEADEDSERPENT) },
			{ TargetID.JorgeFranco, new("Jorge Franco", "Jorge", "JF", "hippo_jorge_franco.jpg", MissionID.SANTAFORTUNA_THREEHEADEDSERPENT) },
			{ TargetID.AndreaMartinez, new("Andrea Martinez", "Andrea", "AM", "hippo_andrea_martinez.jpg", MissionID.SANTAFORTUNA_THREEHEADEDSERPENT) },
			{ TargetID.BlairReddington, new("Blair Reddington", "Blair", "BR", "anaconda_blair_reddington_face.jpg", MissionID.SANTAFORTUNA_EMBRACEOFTHESERPENT) },
			{ TargetID.WazirKale, new("Wazir Kale", "Wazir", "WK", "mongoose_wazir_kale_identified.jpg", MissionID.MUMBAI_CHASINGAGHOST) },
			{ TargetID.VanyaShah, new("Vanya Shah", "Vanya", "VS", "mongoose_vanya_shah.jpg", MissionID.MUMBAI_CHASINGAGHOST) },
			{ TargetID.DawoodRangan, new("Dawood Rangan", "Dawood", "DR", "mongoose_dawood_rangan.jpg", MissionID.MUMBAI_CHASINGAGHOST) },
			{ TargetID.BasilCarnaby, new("Basil Carnaby", "Basil", "BC", "kingcobra_basil_carnaby_face.jpg", MissionID.MUMBAI_ILLUSIONSOFGRANDEUR) },
			{ TargetID.Janus, new("Janus", "Janus", "J", "skunk_janus.jpg", MissionID.WHITTLETON_ANOTHERLIFE) },
			{ TargetID.NolanCassidy, new("Nolan Cassidy", "Nolan", "NC", "skunk_nolan_cassidy.jpg", MissionID.WHITTLETON_ANOTHERLIFE) },
			{ TargetID.GalenVholes, new("Galen Vholes", "Galen", "GV", "gartersnake_ghalen_vholes.jpg", MissionID.WHITTLETON_ABITTERPILL) },
			{ TargetID.ZoeWashington, new("Zoe Washington", "Zoe", "ZW", "magpie_zoe_washington.jpg", MissionID.ISLEOFSGAIL_THEARKSOCIETY) },
			{ TargetID.SophiaWashington, new("Sophia Washington", "Sophia", "sW", "magpie_serena_washington.jpg", MissionID.ISLEOFSGAIL_THEARKSOCIETY) },
			{ TargetID.AthenaSavalas, new("Athena Savalas", "Athena", "AS", "racoon_athena_savalas.jpg", MissionID.NEWYORK_GOLDENHANDSHAKE) },
			{ TargetID.TysonWilliams, new("Tyson Williams", "Tyson", "TW", "stingray_tyson_williams.jpg", MissionID.HAVEN_THELASTRESORT) },
			{ TargetID.StevenBradley, new("Steven Bradley", "Steven", "SB", "stingray_steven_bradley.jpg", MissionID.HAVEN_THELASTRESORT) },
			{ TargetID.LjudmilaVetrova, new("Ljudmila Vetrova", "Ljudmila", "LV", "stingray_ljudmila_vetrova.jpg", MissionID.HAVEN_THELASTRESORT) },
			{ TargetID.CarlIngram, new("Carl Ingram", "Carl", "CI", "golden_carl_ingram.jpg", MissionID.DUBAI_ONTOPOFTHEWORLD) },
			{ TargetID.MarcusStuyvesant, new("Marcus Stuyvesant", "Marcus", "MS", "golden_marcus_stuyvesant.jpg", MissionID.DUBAI_ONTOPOFTHEWORLD) },
			{ TargetID.AlexaCarlisle, new("Alexa Carlisle", "Alexa", "AC", "ancestral_alexa_carlisle.jpg", MissionID.DARTMOOR_DEATHINTHEFAMILY) },
			{ TargetID.Agent1, new("ICA Agent #1", "1", "1", "fox_pickup_earpiece.jpg", MissionID.BERLIN_APEXPREDATOR) },
			{ TargetID.Agent2, new("ICA Agent #2", "2", "2", "fox_pickup_earpiece.jpg", MissionID.BERLIN_APEXPREDATOR) },
			{ TargetID.Agent3, new("ICA Agent #3", "3", "3", "fox_pickup_earpiece.jpg", MissionID.BERLIN_APEXPREDATOR) },
			{ TargetID.Agent4, new("ICA Agent #4", "4", "4", "fox_pickup_earpiece.jpg", MissionID.BERLIN_APEXPREDATOR) },
			{ TargetID.Agent5, new("ICA Agent #5", "5", "5", "fox_pickup_earpiece.jpg", MissionID.BERLIN_APEXPREDATOR) },
			{ TargetID.Agent6, new("ICA Agent #6", "6", "6", "fox_pickup_earpiece.jpg", MissionID.BERLIN_APEXPREDATOR) },
			{ TargetID.Agent7, new("ICA Agent #7", "7", "7", "fox_pickup_earpiece.jpg", MissionID.BERLIN_APEXPREDATOR) },
			{ TargetID.Agent8, new("ICA Agent #8", "8", "8", "fox_pickup_earpiece.jpg", MissionID.BERLIN_APEXPREDATOR) },
			{ TargetID.Agent9, new("ICA Agent #9", "9", "9", "fox_pickup_earpiece.jpg", MissionID.BERLIN_APEXPREDATOR) },
			{ TargetID.Agent10, new("ICA Agent #10", "10", "10", "fox_pickup_earpiece.jpg", MissionID.BERLIN_APEXPREDATOR) },
			{ TargetID.Agent11, new("ICA Agent #11", "11", "11", "fox_pickup_earpiece.jpg", MissionID.BERLIN_APEXPREDATOR) },
			{ TargetID.Hush, new("Hush", "Hush", "H", "wet_hush.jpg", MissionID.CHONGQING_ENDOFANERA) },
			{ TargetID.ImogenRoyce, new("Imogen Royce", "Imogen", "IR", "wet_imogen_royce.jpg", MissionID.CHONGQING_ENDOFANERA) },
			{ TargetID.DonArchibaldYates, new("Don Archibald Yates", "Don", "DY", "elegant_yates.jpg", MissionID.MENDOZA_THEFAREWELL) },
			{ TargetID.TamaraVidal, new("Tamara Vidal", "Tamara", "TV", "elegant_vidal.jpg", MissionID.MENDOZA_THEFAREWELL) },
			{ TargetID.ArthurEdwards, new("Arthur Edwards", "Arthur", "AE", "trapped_arthur_edwards.jpg", MissionID.CARPATHIAN_UNTOUCHABLE) },
			{ TargetID.NoelCrest, new("Noel Crest", "Noel", "NC", "rocky_noel_crest.jpg", MissionID.AMBROSE_SHADOWSINTHEWATER) },
			{ TargetID.SinhiAkkaVenthan, new("Sinhi \"Akka\" Venthan", "Akka", "SV", "rocky_sinhi_akka_venthan.jpg", MissionID.AMBROSE_SHADOWSINTHEWATER) },
			{ TargetID.Target1, new("Target #1", "1", "1st", "default_target_briefing.dds", MissionID.NONE) },
			{ TargetID.Target2, new("Target #2", "2", "2nd", "default_target_briefing.dds", MissionID.NONE) },
			{ TargetID.Target3, new("Target #3", "3", "3rd", "default_target_briefing.dds", MissionID.NONE) },
			{ TargetID.Target4, new("Target #4", "4", "4th", "default_target_briefing.dds", MissionID.NONE) },
			{ TargetID.Target5, new("Target #5", "5", "5th", "default_target_briefing.dds", MissionID.NONE) },
			{ TargetID.Target6, new("Target #6", "6", "6th", "default_target_briefing.dds", MissionID.NONE) },
			{ TargetID.Target7, new("Target #7", "7", "7th", "default_target_briefing.dds", MissionID.NONE) },
			{ TargetID.Target8, new("Target #8", "8", "8th", "default_target_briefing.dds", MissionID.NONE) },
			{ TargetID.Target9, new("Target #9", "9", "9th", "default_target_briefing.dds", MissionID.NONE) },
			{ TargetID.Target10, new("Target #10", "10", "10th", "default_target_briefing.dds", MissionID.NONE) },
			{ TargetID.AgentMontgomery, new("Agent Montgomery", "Montgomery", "MON", "AgentMontgomery.jpg", MissionID.BERLIN_APEXPREDATOR) },
			{ TargetID.AgentChamberlin, new("Agent Chamberlin", "Chamberlin", "CHA", "AgentChamberlin.jpg", MissionID.BERLIN_APEXPREDATOR) },
			{ TargetID.AgentDavenport, new("Agent Davenport", "Davenport", "DAV", "AgentDavenport.jpg", MissionID.BERLIN_APEXPREDATOR) },
			{ TargetID.AgentLowenthal, new("Agent Lowenthal", "Lowenthal", "LOW", "AgentLowenthal.jpg", MissionID.BERLIN_APEXPREDATOR) },
			{ TargetID.AgentThames, new("Agent Thames", "Thames", "THA", "AgentThames.jpg", MissionID.BERLIN_APEXPREDATOR) },
			{ TargetID.AgentTremaine, new("Agent Tremaine", "Tremaine", "TRE", "AgentTremaine.jpg", MissionID.BERLIN_APEXPREDATOR) },
			{ TargetID.AgentBanner, new("Agent Banner", "Banner", "BAN", "AgentBanner.jpg", MissionID.BERLIN_APEXPREDATOR) },
			{ TargetID.AgentSwan, new("Agent Swan", "Swan", "SWA", "AgentSwan.jpg", MissionID.BERLIN_APEXPREDATOR) },
			{ TargetID.AgentRhodes, new("Agent Rhodes", "Rhodes", "RHO", "AgentRhodes.jpg", MissionID.BERLIN_APEXPREDATOR) },
			{ TargetID.AgentGreen, new("Agent Green", "Green", "GRE", "AgentGreen.jpg", MissionID.BERLIN_APEXPREDATOR) },
			{ TargetID.AgentPrice, new("Agent Price", "Price", "PRI", "AgentPrice.jpg", MissionID.BERLIN_APEXPREDATOR) },

			{ TargetID.Unknown, new("Unknown", "Unknown", "?", "mongoose_unknown_man.jpg", MissionID.NONE) },
		};
		static public readonly Dictionary<string, Target> Targets = new() {
			{"KR", new Target(TargetID.KalvinRitter)},
			{"JK", new Target(TargetID.JasperKnight)},
			{"VN", new Target(TargetID.ViktorNovikov)},
			{"DM", new Target(TargetID.DaliaMargolis)},
			{"HSB", new Target(TargetID.HarrySmokeyBagnato)},
			{"MSG", new Target(TargetID.MarvSlickGonif)},
			{"SC", new Target(TargetID.SilvioCaruso)},
			{"FDS", new Target(TargetID.FrancescaDeSantis)},
			{"DB", new Target(TargetID.DinoBosco)},
			{"MA", new Target(TargetID.MarcoAbiatti)},
			{"CB", new Target(TargetID.CraigBlack)},
			{"BA", new Target(TargetID.BrotherAkram)},
			{"RZ", new Target(TargetID.RezaZaydan)},
			{"CHS", new Target(TargetID.ClausHugoStrandberg)},
			{"KTK", new Target(TargetID.KongTuoKwang)},
			{"MM", new Target(TargetID.MatthieuMendola)},
			{"JC", new Target(TargetID.JordanCross)},
			{"KM", new Target(TargetID.KenMorgan)},
			{"ON", new Target(TargetID.OybekNabazov)},
			{"SY", new Target(TargetID.SisterYulduz)},
			{"SR", new Target(TargetID.SeanRose)},
			{"PG", new Target(TargetID.PenelopeGraves)},
			{"EB", new Target(TargetID.EzraBerg)},
			{"MP", new Target(TargetID.MayaParvati)},
			{"ES", new Target(TargetID.ErichSoders)},
			{"YY", new Target(TargetID.YukiYamazaki)},
			{"OC", new Target(TargetID.OwenCage)},
			{"KL", new Target(TargetID.KlausLiebleid)},
			{"DF", new Target(TargetID.DmitriFedorov)},
			{"AR", new Target(TargetID.AlmaReynard)},
			{"SK", new Target(TargetID.SierraKnox)},
			{"RK", new Target(TargetID.RobertKnox)},
			{"AJ", new Target(TargetID.AjitKrish) {
				Name = "Ajit \"AJ\" Krish",
				ShortName = "AJ",
				Image = "cottonmouth_ajit_krish.jpg",
				Mission = MissionID.MIAMI_ASILVERTONGUE,
			}},
			{"RD", new Target(TargetID.RicoDelgado)},
			{"JF", new Target(TargetID.JorgeFranco)},
			{"AM", new Target(TargetID.AndreaMartinez)},
			{"BR", new Target(TargetID.BlairReddington)},
			{"WK", new Target(TargetID.WazirKale)},
			{"VS", new Target(TargetID.VanyaShah)},
			{"DR", new Target(TargetID.DawoodRangan)},
			{"BC", new Target(TargetID.BasilCarnaby)},
			{"J", new Target(TargetID.Janus)},
			{"NC", new Target(TargetID.NolanCassidy)},
			{"GV", new Target(TargetID.GalenVholes)},
			{"ZW", new Target(TargetID.ZoeWashington)},
			{"SW", new Target(TargetID.SophiaWashington)},
			{"AS", new Target(TargetID.AthenaSavalas) {
				Name = "Athena Savalas",
				ShortName = "Athena",
				Image = "racoon_athena_savalas.jpg",
				Mission = MissionID.NEWYORK_GOLDENHANDSHAKE,
			}},
			{"TW", new Target(TargetID.TysonWilliams)},
			{"SB", new Target(TargetID.StevenBradley)},
			{"LV", new Target(TargetID.LjudmilaVetrova)},
			{"CI", new Target(TargetID.CarlIngram)},
			{"MS", new Target(TargetID.MarcusStuyvesant)},
			{"AC", new Target(TargetID.AlexaCarlisle)},
			{"1", new Target(TargetID.Agent1)},
			{"2", new Target(TargetID.Agent2)},
			{"3", new Target(TargetID.Agent3)},
			{"4", new Target(TargetID.Agent4)},
			{"5", new Target(TargetID.Agent5)},
			{"6", new Target(TargetID.Agent6)},
			{"7", new Target(TargetID.Agent7)},
			{"8", new Target(TargetID.Agent8)},
			{"9", new Target(TargetID.Agent9)},
			{"10", new Target(TargetID.Agent10)},
			{"11", new Target(TargetID.Agent11)},
			{"H", new Target(TargetID.Hush)},
			{"IR", new Target(TargetID.ImogenRoyce)},
			{"DY", new Target(TargetID.DonArchibaldYates)},
			{"TV", new Target(TargetID.TamaraVidal)},
			{"AE", new Target(TargetID.ArthurEdwards)},
			{"NCR", new Target(TargetID.NoelCrest)},
			{"SV", new Target(TargetID.SinhiAkkaVenthan)},
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
			{"rezazaydan", "RZ"},
			{"reza", "RZ"}, {"rez", "RZ"}, {"zaydan", "RZ"}, {"general", "RZ"}, {"generalrezazaydan", "RZ"}, {"generalzaydan", "RZ"},
			{"claushugostrandberg", "CHS"},
			{"hugo", "CHS"}, {"claus", "CHS"}, {"strandberg", "CHS"}, {"stranberg", "CHS"},
			{"claustrandberg", "CHS"}, {"clausstranberg", "CHS"}, {"claushugostranberg", "CHS"},
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
			{"robert", "RK"}, {"rob", "RK"}, {"bob", "RK"}, {"bobby", "RK"}, {"bobbyknox", "RK"}, {"bobert", "RK"},
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
			{"arthur", "AE"}, {"edwards", "AE"}, {"theconstant", "AE"}, {"constant", "AE"},
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

		public Target() { }
		public Target(TargetID id)
		{
			var info = targetInfos.GetValueOrDefault(id);
			_id = id;
			Name = info.Name;
			ShortName = info.ShortName;
			Image = info.Image;
			Mission = info.Mission;
		}

		private readonly TargetID _id = TargetID.Unknown;

		public TargetID ID { get => _id; }
		public string Name { get; set; }
		public string ShortName { get; set; }
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
	}
}
