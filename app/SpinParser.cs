using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Croupier {
	public class ParseContext(List<string> tokens = null) {
		public List<string> tokens = tokens ?? [];
		public int conditions = 0;
		public int nextIndex = 0;
		public MissionID mission = MissionID.NONE;
		public string target = "";
		public string disguise = "";
		public KillMethod killMethod = null;
		public KillType killType = KillType.Any;
		public KillComplication killComplication = KillComplication.None;
		public StandardKillMethod standardKillMethod = StandardKillMethod.None;
		public FirearmKillMethod firearmKillMethod = FirearmKillMethod.None;
		public SpecificKillMethod specificKillMethod = SpecificKillMethod.None;

		public bool HaveDisguise {
			get {
				return disguise != null && disguise.Length > 0;
			}
		}
		public bool HaveTarget {
			get {
				return target != null && target.Length > 0;
			}
		}

		public bool HaveKillMethod {
			get {
				return standardKillMethod != StandardKillMethod.None
					|| firearmKillMethod != FirearmKillMethod.None
					|| specificKillMethod != SpecificKillMethod.None;
			}
		}

		public KillMethod CreateKillMethod() {
			KillMethodType? type = null;
			if (standardKillMethod != StandardKillMethod.None)
				type = KillMethodType.Standard;
			if (firearmKillMethod != FirearmKillMethod.None)
				type = KillMethodType.Firearm;
			if (specificKillMethod != SpecificKillMethod.None)
				type = KillMethodType.Specific;
			if (type == null)
				return null;
			return new KillMethod(type.Value) {
				Complication = killComplication,
				Standard = standardKillMethod,
				Firearm = firearmKillMethod,
				Specific = specificKillMethod,
				KillType = killType,
			};
		}
	}

	public class ConditionKeywords(HashSet<MissionID> missions = null, Dictionary<string, string> keywords = null) {
		public readonly HashSet<MissionID> missions = missions ?? [];
		public readonly Dictionary<string, string> keywords = keywords ?? [];
	}

	public class SpinParser {
		public static readonly Dictionary<string, string> TargetKeywords = new() {
			{"kr", "KR"},
			{"kalvinritter", "KR"},
			{"kal", "KR"},
			{"kalvin", "KR"},
			{"sparrow", "KR"},
			{"thesparrow", "KR"},
			{"ritter", "KR"},
			{"jk", "JK"},
			{"jasper", "JK"},
			{"jasperknight", "JK"},
			{"knight", "JK"},
			{"vn", "VN"},
			{"viktornovikov", "VN"},
			{"vic", "VN"}, {"victor", "VN"},
			{"vik", "VN"}, {"viktor", "VN"}, {"novikov", "VN"},
			{"dm", "DM"},
			{"daliamargolis", "DM"},
			{"dalia", "DM"}, {"dahlia", "DM"}, {"margolis", "DM"},
			{"hb", "HSB"}, {"hsb", "HSB"},
			{"harrysmokeybagnato", "HSB"},
			{"harrybagnato", "HSB"}, {"smokey", "HSB"}, {"bagnato", "HSB"},
			{"msg", "MSG"}, {"mg", "MSG"},
			{"marvslickgonif", "MSG"}, {"marvgonif", "MSG"}, {"slick", "MSG"},
			{"marv", "MSG"}, {"gonif", "MSG"},
			{"sc", "SC"},
			{"silviocaruso", "SC"},
			{"silv", "SC"}, {"silvio", "SC"}, {"caruso", "SC"},
			{"francescadesantis", "FDS"},
			{"fds", "FDS"},
			{"fran", "FDS"}, {"frann", "FDS"}, {"franny", "FDS"},
			{"francesca", "FDS"}, {"desantis", "FDS"}, {"santis", "FDS"},
			{"frannydesanny", "FDS"}, {"sanny", "FDS"}, {"desanny", "FDS"},
			{"dinobosco", "DB"},
			{"db", "DB"},
			{"dino", "DB"}, {"bosco", "DB"}, {"ironman", "DB"},
			{"marcoabiatti", "MA"},
			{"ma", "MA"},
			{"marco", "MA"}, {"abiatti", "MA"},
			{"cb", "CB"},
			{"craigblack", "CB"},
			{"craig", "CB"}, {"black", "CB"},
			{"ba", "BA"},
			{"brotherakram", "BA"},
			{"brother", "BA"}, {"akram", "BA"},
			{"chs", "CHS"}, {"cs", "CHS"},
			{"claushugostrandberg", "CHS"},
			{"hugo", "CHS"}, {"claus", "CHS"}, {"strandberg", "CHS"}, {"stranberg", "CHS"},
			{"claustrandberg", "CHS"}, {"clausstranberg", "CHS"}, {"claushugostranberg", "CHS"},
			{"rz", "RZ"}, {"grz", "RZ"},
			{"rezazaydan", "RZ"},
			{"reza", "RZ"}, {"rez", "RZ"}, {"zaydan", "RZ"}, {"general", "RZ"}, {"generalrezazaydan", "RZ"}, {"generalzaydan", "RZ"},
			{"ktk", "KTK"},
			{"kongtuokwang", "KTK"},
			{"kk", "KTK"}, {"kong", "KTK"}, {"tuo", "KTK"}, {"kwang", "KTK"},
			{"tuokwang", "KTK"}, {"kongkwang", "KTK"},
			{"mm", "MM"},
			{"matthieumendola", "MM"},
			{"mat", "MM"}, {"matt", "MM"}, {"matthieu", "MM"}, {"mendola", "MM"},
			{"jc", "JC"},
			{"jordancross", "JC"},
			{"jordan", "JC"}, {"cross", "JC"},
			{"km", "KM"},
			{"kenmorgan", "KM"},
			{"ken", "KM"}, {"morgan", "KM"}, {"thebrick", "KM"}, {"brick", "KM"}, {"kenthebrickmorgan", "KM"},
			{"on", "ON"},
			{"oybeknabazov", "ON"},
			{"oybek", "ON"}, {"nabazov", "ON"},
			{"sy", "SY"},
			{"sisteryulduz", "SY"},
			{"sister", "SY"}, {"yulduz", "SY"}, {"sis", "SY"},
			{"sr", "SR"},
			{"seanrose", "SR"},
			{"sean", "SR"}, {"rose", "SR"},
			{"pg", "PG"},
			{"penelopegraves", "PG"},
			{"penelope", "PG"}, {"graves", "PG"}, {"pen", "PG"}, {"penny", "PG"},
			{"eb", "EB"},
			{"ezraberg", "EB"},
			{"ezra", "EB"}, {"berg", "EB"},
			{"mp", "MP"},
			{"mayaparvati", "MP"},
			{"maya", "MP"}, {"parvati", "MP"},
			{"es", "ES"},
			{"erichsoders", "ES"},
			{"erich", "ES"}, {"ericsoders", "ES"}, {"eric", "ES"}, {"soders", "ES"},
			{"yy", "YY"},
			{"yukiyamazaki" , "YY"},
			{"yuki" , "YY"}, {"yamazaki" , "YY"},
			{"oc", "OC"},
			{"owencage" , "OC"},
			{"owen" , "OC"}, {"cage" , "OC"},
			{"kl", "KL"},
			{"klausliebleid", "KL"},
			{"klaus", "KL"}, {"liebleid", "KL"},
			{"df", "DF"},
			{"dmitrifedorov", "DF"},
			{"dmitri", "DF"}, {"fedorov", "DF"},
			{"ar", "AR"},
			{"almareynard", "AR"},
			{"alma", "AR"}, {"reynard", "AR"},
			{"sk", "SK"},
			{"sierraknox", "SK"},
			{"sierra", "SK"},
			{"rk", "RK"},
			{"robertknox", "RK"},
			{"robert", "RK"}, {"bob", "RK"}, {"bobby", "RK"}, {"bobbyknox", "RK"}, {"bobert", "RK"},
			{"aj", "AJ"},
			{"ajitajkrish", "AJ"},
			{"aak", "AJ"},
			{"ak", "AJ"},
			{"ajitkrish", "AJ"}, {"ajit", "AJ"}, {"krish", "AJ"},
			{"rd", "RD"},
			{"ricodelgado", "RD"},
			{"rico", "RD"}, {"delgado", "RD"},
			{"jf", "JF"},
			{"jorgefranco", "JF"},
			{"jorge", "JF"}, {"franco", "JF"},
			{"am", "AM"},
			{"andreamartinez", "AM"},
			{"andrea", "AM"}, {"martinez", "AM"},
			{"br", "BR"},
			{"blairreddington", "BR"},
			{"blair", "BR"}, {"reddington", "BR"},
			{"wk", "WK"},
			{"wazirkale", "WK"},
			{"wazir", "WK"}, {"kale", "WK"}, {"maelstrom", "WK"},
			{"malestrom", "WK"}, {"themaelstrom", "WK"}, {"themalestrom", "WK"},
			{"vs", "VS"},
			{"vanyashah", "VS"},
			{"vanya", "VS"}, {"shah", "VS"},
			{"dr", "DR"},
			{"dawoodrangan", "DR"},
			{"dawood", "DR"}, {"rangan", "DR"},
			{"bc", "BC"},
			{"basilcarnaby", "BC"},
			{"basil", "BC"}, {"carnaby", "BC"},
			{"j", "J"},
			{"janus", "J"},
			{"nc", "NC"},
			{"nolancassidy", "NC"},
			{"nolan", "NC"}, {"cassidy", "NC"},
			{"gv", "GV"},
			{"galenvholes", "GV"},
			{"galen", "GV"}, {"vholes", "GV"}, {"gale", "GV"},
			{"zw", "ZW"},
			{"zoewashington", "ZW"},
			{"zoe", "ZW"},
			{"sw", "SW"},
			{"sophiawashington", "SW"},
			{"sophia", "ZW"}, {"soph", "ZW"},
			{"as", "AS"},
			{"athenasavalas", "AS"},
			{"athena", "AS"}, {"savalas", "AS"},
			{"tw", "TW"},
			{"tysonwilliams", "TW"},
			{"tyson", "TW"}, {"williams", "TW"},
			{"sb", "SB"},
			{"stevenbradley", "SB"},
			{"steven", "SB"}, {"steve", "SB"}, {"bradley", "SB"},
			{"lv", "LV"},
			{"ljudmilavetrova", "LV"},
			{"ljudmila", "LV"}, {"vetrova", "LV"}, {"ljud", "LV"},
			{"ci", "CI"},
			{"carlingram", "CI"},
			{"carl", "CI"}, {"ingram", "CI"},
			{"ms", "MS"},
			{"marcusstuyvesant", "MS"},
			{"marcus", "MS"}, {"stuyvesant", "MS"},
			{"ac", "AC"},
			{"alexacarlisle", "AC"},
			{"alexa", "AC"}, {"carlisle", "AC"},
			{"h", "H"},
			{"hush", "H"},
			{"ir", "IR"},
			{"imogenroyce", "IR"},
			{"imogen", "IR"}, {"royce", "IR"},
			{"dy", "DY"},
			{"donarchibaldyates", "DY"},
			{"day", "DY"}, {"don", "DY"}, {"yates", "DY"}, {"donyates", "DY"}, {"archibald", "DY"},
			{"tv", "TV"},
			{"tamaravidal", "TV"},
			{"tamara", "TV"}, {"tam", "TV"}, {"vidal", "TV"},
			{"ae", "AE"},
			{"arthuredwards", "AE"},
			{"arthur", "AE"}, {"edwards", "AE"},
			{"ncr", "NCR"},
			{"noelcrest", "NCR"},
			{"noel", "NCR"}, {"crest", "NCR"},
			{"sv", "SV"},
			{"sinhiakkaventhan", "SV"},
			{"sav", "SV"},
			{"akk", "SV"}, {"akka", "SV"}, {"sinhi", "SV"}, {"venthan", "SV"},
			{"1", "1"}, {"one", "1"}, {"1st", "1"},
			{"2", "2"}, {"two", "2"}, {"2nd", "2"},
			{"3", "3"}, {"three", "3"}, {"3rd", "3"},
			{"4", "4"}, {"four", "4"}, {"4th", "4"},
			{"5", "5"}, {"five", "5"}, {"5th", "5"},
		};
		public static readonly Dictionary<string, string> Substitutions = new() {
			{"bg", "bodyguard"},
			{"bgd", "bodyguard"},
			{"bdgd", "bodyguard"},
			{"bdygd", "bodyguard"},
			{"bdygrd", "bodyguard"},
			{"bgrd", "bodyguard"},
			{"bdgrd", "bodyguard"},
			{"cocoa", "coca"},
			{"drvr", "driver"},
			{"mech", "mechanic"},
			{"shiek", "sheikh"},
			{"sheik", "sheikh"},
			{"shiekh", "sheikh"},
			{"shake", "sheikh"},
			{"vamp", "vampire"},
			{"dracula", "vampire"},
			{"hippy", "hippie"},
			{"bicyclist", "cyclist"},
			{"garden", "gardener"},
			{"gardner", "gardener"},
			{"guardener", "gardener"},
			{"housekpr", "housekeeper"},
			{"housekper", "housekeeper"},
			{"housekeper", "housekeeper"},
			{"housekeep", "housekeeper"},
			{"groundkeeper", "groundskeeper"},
			{"miltia", "militia"},
			{"kron", "kronstadt"},
			{"kronst", "kronstadt"},
			{"kronstad", "kronstadt"},
			{"kronstat", "kronstadt"},
			{"krondstat", "kronstadt"},
			{"krondstadt", "kronstadt"},
			{"bollywd", "bollywood"},
			{"bllywd", "bollywood"},
			{"blywd", "bollywood"},
			{"bollywod", "bollywood"},
			{"bolly", "bollywood"},
			{"servent", "servant"},
			{"armour", "armor"},
			{"famos", "famous"},
			{"maintenence", "maintenance"},
			{"maintainance", "maintenance"},
			{"maintainence", "maintenance"},
			{"fac", "facility"},
			{"facil", "facility"},
			{"analist", "analyst"},
			{"guacho", "gaucho"},
			{"somelier", "sommelier"},
			{"sacraficial", "sacrificial"},
			{"millitary", "military"},
			{"prison", "prisoner"},
		};
		public static readonly List<ConditionKeywords> DisguiseKeywords = [
			new([], new() { { "suit", "Suit" } }),
			new([MissionID.ICAFACILITY_GUIDED, MissionID.ICAFACILITY_FREEFORM], new(){
				{"bodyguard", "Bodyguard"},
				{"guard", "Bodyguard"},
				{"mechanic", "Mechanic"},
				{"terrynorfolk", "Terry Norfolk"},
				{"terry", "Terry Norfolk"},
				{"norfolk", "Terry Norfolk"},
				{"crew", "Yacht Crew"},
				{"yachtcrew", "Yacht Crew"},
				{"yachtsecurity", "Yacht Security"},
				{"security", "Yacht Security"},
			}),
			new([MissionID.ICAFACILITY_FINALTEST], new(){
				{"security", "Airfield Security"},
				{"guard", "Airfield Security"},
				{"mechanic", "Airplane Mechanic"},
				{"kgbofficer", "KGB Officer"},
				{"kgb", "KGB Officer"},
				{"officer", "KGB Officer"},
				{"soldier", "Soviet Soldier"},
				{"sovietsoldier", "Soviet Soldier"},
				{"soviet", "Soviet Soldier"},
			}),
			new([MissionID.PARIS_SHOWSTOPPER, MissionID.PARIS_HOLIDAYHOARDERS], new(){
				{"auction", "Auction Staff"},
				{"auctioneer", "Auction Staff"},
				{"auctionstaff", "Auction Staff"},
				{"chef", "Chef"},
				{"cook", "Chef"},
				{"bodyguard", "CICADA Bodyguard"},
				{"cicada", "CICADA Bodyguard"},
				{"cicadabodyguard", "CICADA Bodyguard"},
				{"helmut", "Helmut Kruger"},
				{"kruger", "Helmut Kruger"},
				{"helmutkruger", "Helmut Kruger"},
				{"palace", "Palace Staff"},
				{"staff", "Palace Staff"},
				{"waiter", "Palace Staff"},
				{"palacestaff", "Palace Staff"},
				{"security", "Security Guard"},
				{"guard", "Security Guard"},
				{"securityguard", "Security Guard"},
				{"sec", "Security Guard"},
				{"secguard", "Security Guard"},
				{"sheikh", "Sheikh Salman Al-Ghazali"},
				{"salman", "Sheikh Salman Al-Ghazali"},
				{"alghazali", "Sheikh Salman Al-Ghazali"},
				{"ghazali", "Sheikh Salman Al-Ghazali"},
				{"sheikhsalman", "Sheikh Salman Al-Ghazali"},
				{"sheikhsalmanal", "Sheikh Salman Al-Ghazali"},
				{"sheikhsalmanalghazali", "Sheikh Salman Al-Ghazali"},
				{"salmanalghazali", "Sheikh Salman Al-Ghazali"},
				{"stylist", "Stylist"},
				{"tech", "Tech Crew"},
				{"techcrew", "Tech Crew"},
				{"crew", "Tech Crew"},
				{"roadie", "Tech Crew"},
				{"stagecrew", "Tech Crew"},
				{"vampiremagician", "Vampire Magician"},
				{"vampire", "Vampire Magician"},
				{"magician", "Vampire Magician"},
			}),
			new([MissionID.PARIS_HOLIDAYHOARDERS], new(){
				{"santaclaus", "Santa"},
				{"santa", "Santa"},
				{"saintnick", "Santa"},
				{"hohoho", "Santa"},
			}),
			new([MissionID.SAPIENZA_WORLDOFTOMORROW], new(){
				{"biolab", "Biolab Security"},
				{"bioguard", "Biolab Security"},
				{"biosecurity", "Biolab Security"},
				{"biolabguard", "Biolab Security"},
				{"biolabsecurity", "Biolab Security"},
				{"labsecurity", "Biolab Security"},
				{"labguard", "Biolab Security"},
				{"caveguard", "Biolab Security"},
				{"cavesecurity", "Biolab Security"},
				{"bodyguard", "Bodyguard"},
				{"cicada", "Bodyguard"},
				{"bohemian", "Bohemian"},
				{"hippie", "Bohemian"},
				{"torres", "Bohemian"},
				{"piombo", "Bohemian"},
				{"torrespiombo", "Bohemian"},
				{"butler", "Butler"},
				{"chef", "Chef"},
				{"churchstaff", "Church Staff"},
				{"church", "Church Staff"},
				{"cyclist", "Cyclist"},
				{"delivery", "Delivery Man"},
				{"deliveryman", "Delivery Man"},
				{"deliveryguy", "Delivery Man"},
				{"lafayette", "Dr. Oscar Lafayette"},
				{"drlafayette", "Dr. Oscar Lafayette"},
				{"oscar", "Dr. Oscar Lafayette"},
				{"droscarlafayette", "Dr. Oscar Lafayette"},
				{"oscarlafayette", "Dr. Oscar Lafayette"},
				{"laf", "Dr. Oscar Lafayette"},
				{"laff", "Dr. Oscar Lafayette"},
				{"dr", "Dr. Oscar Lafayette"},
				{"gardener", "Gardener"},
				{"gplumber", "Green Plumber"},
				{"grnplumber", "Green Plumber"},
				{"luigi", "Green Plumber"},
				{"luigimario", "Green Plumber"},
				{"greenplumber", "Green Plumber"},
				{"hazmatsuit", "Hazmat Suit"},
				{"hazardsuit", "Hazmat Suit"},
				{"biosuit", "Hazmat Suit"},
				{"hazmat", "Hazmat Suit"},
				{"housekeeper", "Housekeeper"},
				{"kitchenstaff", "Kitchen Assistant"},
				{"kitchenassistant", "Kitchen Assistant"},
				{"kitchenass", "Kitchen Assistant"},
				{"kitchensass", "Kitchen Assistant"},
				{"kitchensassistant", "Kitchen Assistant"},
				{"rocco", "Kitchen Assistant"},
				{"labtechnician", "Lab Technician"},
				{"labtech", "Lab Technician"},
				{"lab", "Lab Technician"},
				{"technician", "Lab Technician"},
				{"scientist", "Lab Technician"},
				{"mansionsecurity", "Mansion Security"},
				{"housesecurity", "Security"},
				{"villasecurity", "Security"},
				{"houseguard", "Security"},
				{"mansionguard", "Security"},
				{"villaguard", "Security"},
				{"guard", "Security"},
				{"security", "Security"},
				{"mansionstaff", "Mansion Staff"},
				{"mansion", "Mansion Staff"},
				{"staff", "Mansion Staff"},
				{"plague", "Plague Doctor"},
				{"plaguedoctor", "Plague Doctor"},
				{"doctor", "Plague Doctor"},
				{"priest", "Priest"},
				{"padre", "Priest"},
				{"fadre", "Priest"},
				{"privatedetective", "Private Detective"},
				{"detective", "Private Detective"},
				{"pi", "Private Detective"},
				{"pd", "Private Detective"},
				{"privateinvestigator", "Private Detective"},
				{"investigator", "Private Detective"},
				{"robertovargas", "Roberto Vargas"},
				{"roberto", "Roberto Vargas"},
				{"vargas", "Roberto Vargas"},
				{"coach", "Roberto Vargas"},
				{"instructor", "Roberto Vargas"},
				{"golfcoach", "Roberto Vargas"},
				{"golfinstructor", "Roberto Vargas"},
				{"golf", "Roberto Vargas"},
				{"robert", "Roberto Vargas"},
				{"redplumber", "Red Plumber"},
				{"rplumber", "Red Plumber"},
				{"mariomario", "Red Plumber"},
				{"mario", "Red Plumber"},
				{"rdplumber", "Red Plumber"},
				{"storeclerk", "Store Clerk"},
				{"store", "Store Clerk"},
				{"clerk", "Store Clerk"},
				{"shop", "Store Clerk"},
				{"shopkeeper", "Store Clerk"},
				{"storekeeper", "Store Clerk"},
				{"streetperformer", "Street Performer"},
				{"jingles", "Street Performer"},
				{"clown", "Street Performer"},
				{"performer", "Street Performer"},
				{"jester", "Street Performer"},
				{"mime", "Street Performer"},
				{"juggler", "Street Performer"},
				{"waiter", "Waiter"},
			}),
			new([MissionID.SAPIENZA_THEAUTHOR], new(){
				{"bodyguard", "Bodyguard"},
				{"guard", "Bodyguard"},
				{"cicada", "Bodyguard"},
				{"bohemian", "Bohemian"},
				{"hippie", "Bohemian"},
				{"torres", "Bohemian"},
				{"piombo", "Bohemian"},
				{"torrespiombo", "Bohemian"},
				{"brotherakram", "Brother Akram"},
				{"brother", "Brother Akram"},
				{"akram", "Brother Akram"},
				{"chef", "Chef"},
				{"churchstaff", "Church Staff"},
				{"church", "Church Staff"},
				{"craigblack", "Craig Black"},
				{"craig", "Craig Black"},
				{"black", "Craig Black"},
				{"gardener", "Gardener"},
				{"gplumber", "Green Plumber"},
				{"grnplumber", "Green Plumber"},
				{"luigi", "Green Plumber"},
				{"luigimario", "Green Plumber"},
				{"greenplumber", "Green Plumber"},
				{"housekeeper", "Housekeeper"},
				{"kitchenstaff", "Kitchen Assistant"},
				{"kitchenassistant", "Kitchen Assistant"},
				{"kitchenass", "Kitchen Assistant"},
				{"kitchensass", "Kitchen Assistant"},
				{"kitchensassistant", "Kitchen Assistant"},
				{"rocco", "Kitchen Assistant"},
				{"redplumber", "Red Plumber"},
				{"rplumber", "Red Plumber"},
				{"mariomario", "Red Plumber"},
				{"mario", "Red Plumber"},
				{"rdplumber", "Red Plumber"},
				{"salvatorebravuomo", "Salvatore Bravuomo"},
				{"sal", "Salvatore Bravuomo"},
				{"salvatore", "Salvatore Bravuomo"},
				{"bravuomo", "Salvatore Bravuomo"},
				{"bravomo", "Salvatore Bravuomo"},
				{"bravumo", "Salvatore Bravuomo"},
				{"sfxcrew", "SFX Crew"},
				{"fxcrew", "SFX Crew"},
				{"sfx", "SFX Crew"},
				{"crew", "SFX Crew"},
				{"superfan", "Super Fan"},
				{"fan", "Super Fan"},
				{"waiter", "Waiter"},
			}),
			new([MissionID.SAPIENZA_LANDSLIDE], new(){
				{"bodyguard", "Bodyguard"},
				{"guard", "Bodyguard"},
				{"cicada", "Bodyguard"},
				{"bohemian", "Bohemian"},
				{"hippie", "Bohemian"},
				{"torres", "Bohemian"},
				{"piombo", "Bohemian"},
				{"torrespiombo", "Bohemian"},
				{"churchstaff", "Church Staff"},
				{"church", "Church Staff"},
				{"gardener", "Gardener"},
				{"gplumber", "Green Plumber"},
				{"grnplumber", "Green Plumber"},
				{"luigi", "Green Plumber"},
				{"luigimario", "Green Plumber"},
				{"greenplumber", "Green Plumber"},
				{"kitchenstaff", "Kitchen Assistant"},
				{"kitchenassistant", "Kitchen Assistant"},
				{"kitchenass", "Kitchen Assistant"},
				{"kitchensass", "Kitchen Assistant"},
				{"kitchensassistant", "Kitchen Assistant"},
				{"rocco", "Kitchen Assistant"},
				{"photographer", "Photographer"},
				{"photo", "Photographer"},
				{"priest", "Priest"},
				{"padre", "Priest"},
				{"fadre", "Priest"},
				{"rplumber", "Red Plumber"},
				{"mariomario", "Red Plumber"},
				{"mario", "Red Plumber"},
				{"rdplumber", "Red Plumber"},
				{"salvatorebravuomo", "Salvatore Bravuomo"},
				{"sal", "Salvatore Bravuomo"},
				{"salvatore", "Salvatore Bravuomo"},
				{"bravuomo", "Salvatore Bravuomo"},
				{"bravomo", "Salvatore Bravuomo"},
				{"bravumo", "Salvatore Bravuomo"},
				{"security", "Security"},
				{"stagecrew", "Stage Crew"},
				{"crew", "Stage Crew"},
				{"stage", "Stage Crew"},
				{"waiter", "Waiter"},
			}),
			new([MissionID.SAPIENZA_THEICON], new(){
				{"kitchenstaff", "Kitchen Assistant"},
				{"kitchenassistant", "Kitchen Assistant"},
				{"kitchenass", "Kitchen Assistant"},
				{"kitchensass", "Kitchen Assistant"},
				{"kitchensassistant", "Kitchen Assistant"},
				{"rocco", "Kitchen Assistant"},
				{"moviecrew", "Movie Crew"},
				{"filmcrew", "Movie Crew"},
				{"movie", "Movie Crew"},
				{"security", "Security"},
				{"guard", "Security"},
				{"sfxcrew", "SFX Crew"},
				{"sfx", "SFX Crew"},
			}),
			new([MissionID.MARRAKESH_GILDEDCAGE], new(){
				{"bodyguard", "Bodyguard"},
				{"camera", "Cameraman"},
				{"cameraman", "Cameraman"},
				{"photographer", "Cameraman"},
				{"consulateintern", "Consulate Intern"},
				{"intern", "Consulate Intern"},
				{"steve", "Consulate Intern"},
				{"hektor", "Consulate Intern"},
				{"lindberg", "Consulate Intern"},
				{"hektorlindberg", "Consulate Intern"},
				{"consulatejanitor", "Consulate Janitor"},
				{"janitor", "Consulate Janitor"},
				{"consulatesecurity", "Consulate Security"},
				{"security", "Consulate Security"},
				{"consulateguard", "Consulate Security"},
				{"elitesoldier", "Elite Soldier"},
				{"elite", "Elite Soldier"},
				{"foodvendor", "Food Vendor"},
				{"vendor", "Food Vendor"},
				{"fortuneteller", "Fortune Teller"},
				{"fortune", "Fortune Teller"},
				{"psychic", "Fortune Teller"},
				{"ft", "Fortune Teller"},
				{"handyman", "Handyman"},
				{"headmaster", "Headmaster"},
				{"teacher", "Headmaster"},
				{"localprintingcrew", "Local Printing Crew"},
				{"printingcrew", "Local Printing Crew"},
				{"printer", "Local Printing Crew"},
				{"printcrew", "Local Printing Crew"},
				{"printing", "Local Printing Crew"},
				{"masseur", "Masseur"},
				{"masseuse", "Masseur"},
				{"massage", "Masseur"},
				{"konnyengstrom", "Masseur"},
				{"konny", "Masseur"},
				{"engstrom", "Masseur"},
				{"militaryofficer", "Military Officer"},
				{"officer", "Military Officer"},
				{"redberet", "Military Officer"},
				{"redhat", "Military Officer"},
				{"militarysoldier", "Military Soldier"},
				{"soldier", "Military Soldier"},
				{"prisoner", "Prisoner"},
				{"shopkeeper", "Shopkeeper"},
				{"shopowner", "Shopkeeper"},
				{"storeowner", "Shopkeeper"},
				{"waiter", "Waiter"},
			}),
			new([MissionID.MARRAKESH_HOUSEBUILTONSAND], new(){
				{"bodyguard", "Bodyguard"},
				{"foodvendor", "Food Vendor"},
				{"vendor", "Food Vendor"},
				{"fortuneteller", "Fortune Teller"},
				{"fortune", "Fortune Teller"},
				{"psychic", "Fortune Teller"},
				{"ft", "Fortune Teller"},
				{"handyman", "Handyman"},
				{"militarysoldier", "Military Soldier"},
				{"soldier", "Military Soldier"},
				{"shopkeeper", "Shopkeeper"},
				{"shopowner", "Shopkeeper"},
				{"storeowner", "Shopkeeper"},
				{"waiter", "Waiter"},
			}),
			new([MissionID.BANGKOK_CLUB27], new(){
				{"abeldesilva", "Abel de Silva"},
				{"abel", "Abel de Silva"},
				{"desilva", "Abel de Silva"},
				{"silva", "Abel de Silva"},
				{"exterminator", "Exterminator"},
				{"bugman", "Exterminator"},
				{"groundskeeper", "Groundskeeper"},
				{"gardener", "Groundskeeper"},
				{"hotelsecurity", "Hotel Security"},
				{"security", "Hotel Security"},
				{"hotelguard", "Hotel Security"},
				{"hotelstaff", "Hotel Staff"},
				{"staff", "Hotel Staff"},
				{"jordancrossbodyguard", "Jordan Cross' Bodyguard"},
				{"jordanbodyguard", "Jordan Cross' Bodyguard"},
				{"jcbodyguard", "Jordan Cross' Bodyguard"},
				{"jcguard", "Jordan Cross' Bodyguard"},
				{"jordanguard", "Jordan Cross' Bodyguard"},
				{"jordancrossguard", "Jordan Cross' Bodyguard"},
				{"kitchenstaff", "Kitchen Staff"},
				{"kmbodyguard", "Morgan's Bodyguard"},
				{"kmguard", "Morgan's Bodyguard"},
				{"kmsbodyguard", "Morgan's Bodyguard"},
				{"kmsguard", "Morgan's Bodyguard"},
				{"morgansbodyguard", "Morgan's Bodyguard"},
				{"morgansguard", "Morgan's Bodyguard"},
				{"morganguard", "Morgan's Bodyguard"},
				{"morganbodyguard", "Morgan's Bodyguard"},
				{"kensbodyguard", "Morgan's Bodyguard"},
				{"kensguard", "Morgan's Bodyguard"},
				{"kenguard", "Morgan's Bodyguard"},
				{"kenbodyguard", "Morgan's Bodyguard"},
				{"otis", "Morgan's Bodyguard"},
				{"recordingcrew", "Recording Crew"},
				{"recording", "Recording Crew"},
				{"crew", "Recording Crew"},
				{"band", "Recording Crew"},
				{"stage", "Recording Crew"},
				{"roadie", "Recording Crew"},
				{"stalker", "Stalker"},
				{"stalk", "Stalker"},
				{"creep", "Stalker"},
				{"waiter", "Waiter"},
			}),
			new([MissionID.BANGKOK_THESOURCE], new(){
				{"cultbodyguard", "Cult Bodyguard"},
				{"cultguard", "Cult Bodyguard"},
				{"cultinitiate", "Cult Initiate"},
				{"initiate", "Cult Initiate"},
				{"gardener", "Groundskeeper"},
				{"groundskeeper", "Groundskeeper"},
				{"exterminator", "Exterminator"},
				{"bugman", "Exterminator"},
				{"hotelsecurity", "Hotel Security"},
				{"security", "Hotel Security"},
				{"hotelguard", "Hotel Security"},
				{"hotelstaff", "Hotel Staff"},
				{"staff", "Hotel Staff"},
				{"jordancrossbodyguard", "Jordan Cross' Bodyguard"},
				{"jordanbodyguard", "Jordan Cross' Bodyguard"},
				{"jcbodyguard", "Jordan Cross' Bodyguard"},
				{"jcguard", "Jordan Cross' Bodyguard"},
				{"jordanguard", "Jordan Cross' Bodyguard"},
				{"jordancrossguard", "Jordan Cross' Bodyguard"},
				{"kitchenstaff", "Kitchen Staff"},
				{"kitchen", "Kitchen Staff"},
				{"militiasoldier", "Militia Soldier"},
				{"militia", "Militia Soldier"},
				{"military", "Militia Soldier"},
				{"soldier", "Militia Soldier"},
				{"recordingcrew", "Recording Crew"},
				{"recording", "Recording Crew"},
				{"crew", "Recording Crew"},
				{"band", "Recording Crew"},
				{"stage", "Recording Crew"},
				{"roadie", "Recording Crew"},
				{"waiter", "Waiter"},
			}),
			new([MissionID.COLORADO_FREEDOMFIGHTERS], new(){
				{"explosivesspecialist", "Explosives Specialist"},
				{"explosivespecialist", "Explosives Specialist"},
				{"explosivespec", "Explosives Specialist"},
				{"explosives", "Explosives Specialist"},
				{"explspec", "Explosives Specialist"},
				{"expspec", "Explosives Specialist"},
				{"expexpert", "Explosives Specialist"},
				{"explexpert", "Explosives Specialist"},
				{"explosiveexpert", "Explosives Specialist"},
				{"explosivesexpert", "Explosives Specialist"},
				{"specialist", "Explosives Specialist"},
				{"hacker", "Hacker"},
				{"militiacook", "Militia Cook"},
				{"cook", "Militia Cook"},
				{"chef", "Militia Cook"},
				{"militiaelite", "Militia Elite"},
				{"elite", "Militia Elite"},
				{"militiasoldier", "Militia Soldier"},
				{"soldier", "Militia Soldier"},
				{"sold", "Militia Soldier"},
				{"militiaspecops", "Militia Spec Ops"},
				{"militiaspec", "Militia Spec Ops"},
				{"specops", "Militia Spec Ops"},
				{"specialops", "Militia Spec Ops"},
				{"spec", "Militia Spec Ops"},
				{"militiatechnician", "Militia Technician"},
				{"militiatech", "Militia Technician"},
				{"technician", "Militia Technician"},
				{"tech", "Militia Technician"},
				{"pointman", "Point Man"},
				{"burges", "Point Man"},
				{"point", "Point Man"},
				{"scarecrow", "Scarecrow"},
			}),
			new([MissionID.HOKKAIDO_SITUSINVERSUS, MissionID.HOKKAIDO_SNOWFESTIVAL], new(){
				{"baseballplayer", "Baseball Player"},
				{"baseball", "Baseball Player"},
				{"baseboi", "Baseball Player"},
				{"bodyguard", "Bodyguard"},
				{"chef", "Chef"},
				{"chiefsurgeon", "Chief Surgeon"},
				{"chief", "Chief Surgeon"},
				{"chiefy", "Chief Surgeon"},
				{"laurant", "Chief Surgeon"},
				{"doctor", "Doctor"},
				{"dr", "Doctor"},
				{"doc", "Doctor"},
				{"handyman", "Handyman"},
				{"gardener", "Handyman"},
				{"repairman", "Handyman"},
				{"helicopterpilot", "Helicopter Pilot"},
				{"helicopter", "Helicopter Pilot"},
				{"pilot", "Helicopter Pilot"},
				{"hospitaldirector", "Hospital Director"},
				{"director", "Hospital Director"},
				{"morguedoctor", "Morgue Doctor"},
				{"morgue", "Morgue Doctor"},
				{"morguedr", "Morgue Doctor"},
				{"motorcyclist", "Motorcyclist"},
				{"motorcycle", "Motorcyclist"},
				{"ninja", "Ninja"},
				{"patient", "Patient"},
				{"resortsecurity", "Resort Security"},
				{"security", "Resort Security"},
				{"resortguard", "Resort Security"},
				{"guard", "Resort Security"},
				{"resortstaff", "Resort Staff"},
				{"staff", "Resort Staff"},
				{"surgeon", "Surgeon"},
				{"vippatientamosdexter", "VIP Patient (Amos Dexter)"},
				{"vippatientamos", "VIP Patient (Amos Dexter)"},
				{"vippatientdexter", "VIP Patient (Amos Dexter)"},
				{"patientamosdexter", "VIP Patient (Amos Dexter)"},
				{"patientdexter", "VIP Patient (Amos Dexter)"},
				{"patientamos", "VIP Patient (Amos Dexter)"},
				{"amosdexter", "VIP Patient (Amos Dexter)"},
				{"amos", "VIP Patient (Amos Dexter)"},
				{"dexter", "VIP Patient (Amos Dexter)"},
				{"jasonportman", "VIP Patient (Jason Portman)"},
				{"vippatientjasonportman", "VIP Patient (Jason Portman)"},
				{"patientjasonportman", "VIP Patient (Jason Portman)"},
				{"patientjason", "VIP Patient (Jason Portman)"},
				{"patientportman", "VIP Patient (Jason Portman)"},
				{"vippatientjason", "VIP Patient (Jason Portman)"},
				{"vippatientportman", "VIP Patient (Jason Portman)"},
				{"jason", "VIP Patient (Jason Portman)"},
				{"portman", "VIP Patient (Jason Portman)"},
				{"yogainstructor", "Yoga Instructor"},
				{"yoga", "Yoga Instructor"},
			}),
			new([MissionID.HOKKAIDO_PATIENTZERO], new(){
				{"biosuit", "Bio Suit"},
				{"hazmatsuit", "Bio Suit"},
				{"biohazardsuit", "Bio Suit"},
				{"hazardsuit", "Bio Suit"},
				{"bodyguard", "Bodyguard"},
				{"chef", "Chef"},
				{"doctor", "Doctor"},
				{"dr", "Doctor"},
				{"doc", "Doctor"},
				{"handyman", "Handyman"},
				{"gardener", "Handyman"},
				{"repairman", "Handyman"},
				{"headresearcher", "Head Researcher"},
				{"researcher", "Head Researcher"},
				{"helicopterpilot", "Helicopter Pilot"},
				{"helicopter", "Helicopter Pilot"},
				{"pilot", "Helicopter Pilot"},
				{"hospitaldirector", "Hospital Director"},
				{"director", "Hospital Director"},
				{"morguedoctor", "Morgue Doctor"},
				{"morgue", "Morgue Doctor"},
				{"morguedr", "Morgue Doctor"},
				{"motorcyclist", "Motorcyclist"},
				{"motorcycle", "Motorcyclist"},
				{"patient", "Patient"},
				{"resortsecurity", "Resort Security"},
				{"security", "Resort Security"},
				{"resortguard", "Resort Security"},
				{"guard", "Resort Security"},
				{"resortstaff", "Resort Staff"},
				{"staff", "Resort Staff"},
				{"surgeon", "Surgeon"},
				{"amosdexter", "VIP Patient (Amos Dexter)"},
				{"amos", "VIP Patient (Amos Dexter)"},
				{"dexter", "VIP Patient (Amos Dexter)"},
				{"yogainstructor", "Yoga Instructor"},
				{"yoga", "Yoga Instructor"},
			}),
			new([MissionID.HAWKESBAY_NIGHTCALL], new(){
				{"bodyguard", "Bodyguard"},
				{"guard", "Bodyguard"},
			}),
			new([MissionID.MIAMI_FINISHLINE, MissionID.MIAMI_ASILVERTONGUE], new(){
				{"aeondriver", "Aeon Driver"},
				{"aeonmechanic", "Aeon Mechanic"},
				{"blueseeddriver", "Blue Seed Driver"},
				{"blueseed", "Blue Seed Driver"},
				{"crashedkronstadtdriver", "Crashed Kronstadt Driver"},
				{"crasheddriver", "Crashed Kronstadt Driver"},
				{"crashed", "Crashed Kronstadt Driver"},
				{"eventcrew", "Event Crew"},
				{"crew", "Event Crew"},
				{"eventstaff", "Event Crew"},
				{"eventsecurity", "Event Security"},
				{"security", "Event Security"},
				{"cop", "Event Security"},
				{"floridaman", "Florida Man"},
				{"flman", "Florida Man"},
				{"florida", "Florida Man"},
				{"foodvendor", "Food Vendor"},
				{"vendor", "Food Vendor"},
				{"journalist", "Journalist"},
				{"press", "Journalist"},
				{"paparazzi", "Journalist"},
				{"kitchenstaff", "Kitchen Staff"},
				{"kitchen", "Kitchen Staff"},
				{"kowoondriver", "Kowoon Driver"},
				{"kowoonmechanic", "Kowoon Mechanic"},
				{"kronstadtengineer", "Kronstadt Engineer"},
				{"engineer", "Kronstadt Engineer"},
				{"kronstadtmechanic", "Kronstadt Mechanic"},
				{"kronstadtresearcher", "Kronstadt Researcher"},
				{"researcher", "Kronstadt Researcher"},
				{"research", "Kronstadt Researcher"},
				{"kronstadtsecurity", "Kronstadt Security"},
				{"kronstadtguard", "Kronstadt Security"},
				{"bodyguard", "Kronstadt Security"},
				{"robertguard", "Kronstadt Security"},
				{"robertsecurity", "Kronstadt Security"},
				{"mascot", "Mascot"},
				{"medic", "Medic"},
				{"moseslee", "Moses Lee"},
				{"palerider", "Pale Rider"},
				{"pale", "Pale Rider"},
				{"thestig", "Pale Rider"},
				{"stig", "Pale Rider"},
				{"racecoordinator", "Race Coordinator"},
				{"racecoord", "Race Coordinator"},
				{"coordinator", "Race Coordinator"},
				{"coord", "Race Coordinator"},
				{"racemarshal", "Race Marshal"},
				{"racemarshall", "Race Marshal"},
				{"marshal", "Race Marshal"},
				{"marshall", "Race Marshal"},
				{"sheikh", "Sheik"},
				{"sotteraneomechanic", "Sotteraneo Mechanic"},
				{"sotteraneo", "Sotteraneo Mechanic"},
				{"streetmusician", "Street Musician"},
				{"musician", "Street Musician"},
				{"streetperformer", "Street Musician"},
				{"performer", "Street Musician"},
				{"drum", "Street Musician"},
				{"drummer", "Street Musician"},
				{"tedmendez", "Ted Mendez"},
				{"ted", "Ted Mendez"},
				{"mendez", "Ted Mendez"},
				{"military", "Ted Mendez"},
				{"thwackdriver", "Thwack Driver"},
				{"thwackmechanic", "Thwack Mechanic"},
				{"waiter", "Waiter"},
			}),
			new([MissionID.SANTAFORTUNA_THREEHEADEDSERPENT, MissionID.SANTAFORTUNA_EMBRACEOFTHESERPENT], new(){
				{"bandmember", "Band Member"},
				{"band", "Band Member"},
				{"drummer", "Band Member"},
				{"drum", "Band Member"},
				{"barman", "Barman"},
				{"bartender", "Barman"},
				{"bar", "Barman"},
				{"chef", "Chef"},
				{"cocafieldguard", "Coca Field Guard"},
				{"cocaguard", "Coca Field Guard"},
				{"fieldguard", "Coca Field Guard"},
				{"jorgeguard", "Coca Field Guard"},
				{"cocafieldworker", "Coca Field Worker"},
				{"cocaworker", "Coca Field Worker"},
				{"fieldworker", "Coca Field Worker"},
				{"constructionworker", "Construction Worker"},
				{"construction", "Construction Worker"},
				{"builder", "Construction Worker"},
				{"druglabworker", "Drug Lab Worker"},
				{"drugworker", "Drug Lab Worker"},
				{"labworker", "Drug Lab Worker"},
				{"druglab", "Drug Lab Worker"},
				{"lab", "Drug Lab Worker"},
				{"eliteguard", "Elite Guard"},
				{"elite", "Elite Guard"},
				{"gardener", "Gardener"},
				{"hippie", "Hippie"},
				{"bohemian", "Hippie"},
				{"hippowhisperer", "Hippo Whisperer"},
				{"hippo", "Hippo Whisperer"},
				{"whisperer", "Hippo Whisperer"},
				{"mansionguard", "Mansion Guard"},
				{"manguard", "Mansion Guard"},
				{"mansionsecurity", "Mansion Guard"},
				{"mansionstaff", "Mansion Staff"},
				{"staff", "Mansion Staff"},
				{"servant", "Mansion Staff"},
				{"shaman", "Shaman"},
				{"taita", "Shaman"},
				{"streetsoldier", "Street Soldier"},
				{"soldier", "Street Soldier"},
				{"streetguard", "Street Soldier"},
				{"submarinecrew", "Submarine Crew"},
				{"subcrew", "Submarine Crew"},
				{"submarineworker", "Submarine Crew"},
				{"submarine", "Submarine Crew"},
				{"submarineengineer", "Submarine Engineer"},
				{"subengineer", "Submarine Engineer"},
				{"engineer", "Submarine Engineer"},
				{"tattooartist", "Tattoo Artist (P-Power)"},
				{"tattooartistppower", "Tattoo Artist (P-Power)"},
				{"tattooartistp", "Tattoo Artist (P-Power)"},
				{"artistppower", "Tattoo Artist (P-Power)"},
				{"ppower", "Tattoo Artist (P-Power)"},
				{"paulpowers", "Tattoo Artist (P-Power)"},
				{"puhpower", "Tattoo Artist (P-Power)"},
				{"power", "Tattoo Artist (P-Power)"},
			}),
			new([MissionID.MUMBAI_CHASINGAGHOST, MissionID.MUMBAI_ILLUSIONSOFGRANDEUR], new(){
				{"barber", "Barber"},
				{"barbershop", "Barber"},
				{"bollywoodbodyguard", "Bollywood Bodyguard"},
				{"bollywoodguard", "Bollywood Bodyguard"},
				{"bollywoodcrew", "Bollywood Crew"},
				{"crew", "Bollywood Crew"},
				{"dancer", "Dancer"},
				{"dance", "Dancer"},
				{"elitethug", "Elite Thug"},
				{"elite", "Elite Thug"},
				{"foodvendor", "Food Vendor"},
				{"vendor", "Food Vendor"},
				{"holyman", "Holy Man"},
				{"holy", "Holy Man"},
				{"kashmirian", "Kashmirian"},
				{"assassin", "Kashmirian"},
				{"laundryforeman", "Laundry Foreman"},
				{"foreman", "Laundry Foreman"},
				{"laundryworker", "Laundry Worker"},
				{"leadactor", "Lead Actor"},
				{"actor", "Lead Actor"},
				{"gregoryarthur", "Lead Actor"},
				{"gregory", "Lead Actor"},
				{"localsecurity", "Local Security"},
				{"security", "Local Security"},
				{"metalworker", "Metal Worker"},
				{"metal", "Metal Worker"},
				{"forger", "Metal Worker"},
				{"painter", "Painter"},
				{"paint", "Painter"},
				{"artist", "Painter"},
				{"queensbodyguard", "Queen's Bodyguard"},
				{"queenbodyguard", "Queen's Bodyguard"},
				{"vanyabodyguard", "Queen's Bodyguard"},
				{"vanyasbodyguard", "Queen's Bodyguard"},
				{"queensguard", "Queen's Guard"},
				{"queenguard", "Queen's Guard"},
				{"vanyaguard", "Queen's Guard"},
				{"vanyasguard", "Queen's Guard"},
				{"tailor", "Tailor"},
				{"thug", "Thug"},
				{"vanyasservant", "Vanya's Servant"},
				{"vanyaservant", "Vanya's Servant"},
			}),
			new([MissionID.WHITTLETON_ANOTHERLIFE, MissionID.WHITTLETON_ABITTERPILL], new(){
				{"arkianrobes", "Arkian Robes"},
				{"arkrobes", "Arkian Robes"},
				{"arkiantux", "Arkian Robes"},
				{"arkian", "Arkian Robes"},
				{"tux", "Arkian Robes"},
				{"robes", "Arkian Robes"},
				{"bbqowner", "BBQ Owner"},
				{"bbq", "BBQ Owner"},
				{"richard", "BBQ Owner"},
				{"cassidybodyguard", "Cassidy Bodyguard"},
				{"cassidysbodyguard", "Cassidy Bodyguard"},
				{"nolanbodyguard", "Cassidy Bodyguard"},
				{"ncbodyguard", "Cassidy Bodyguard"},
				{"nolanguard", "Cassidy Bodyguard"},
				{"cassidyguard", "Cassidy Bodyguard"},
				{"constructionworker", "Construction Worker"},
				{"construction", "Construction Worker"},
				{"builder", "Construction Worker"},
				{"exterminator", "Exterminator"},
				{"bugman", "Exterminator"},
				{"pestcontrol", "Exterminator"},
				{"garbageman", "Garbage Man"},
				{"garbage", "Garbage Man"},
				{"bin", "Garbage Man"},
				{"gardener", "Gardener"},
				{"gunthermueller", "Gunther Mueller"},
				{"gunther", "Gunther Mueller"},
				{"mueller", "Gunther Mueller"},
				{"jamesbatty", "James Batty"},
				{"batty", "James Batty"},
				{"james", "James Batty"},
				{"janusbodyguard", "Janus' Bodyguard"},
				{"janusguard", "Janus' Bodyguard"},
				{"mailman", "Mailman"},
				{"mail", "Mailman"},
				{"deliveryman", "Mailman"},
				{"delivery", "Mailman"},
				{"nurse", "Nurse"},
				{"plumber", "Plumber"},
				{"policedeputy", "Police Deputy"},
				{"deputy", "Police Deputy"},
				{"police", "Police Deputy"},
				{"cop", "Police Deputy"},
				{"politician", "Politician"},
				{"politiciansassistant", "Politician's Assistant"},
				{"politicianassistant", "Politician's Assistant"},
				{"politiciansass", "Politician's Assistant"},
				{"politicianass", "Politician's Assistant"},
				{"politicassistant", "Politician's Assistant"},
				{"pa", "Politician's Assistant"},
				{"realestatebroker", "Real Estate Broker"},
				{"realestateagent", "Real Estate Broker"},
				{"estateagent", "Real Estate Broker"},
				{"realestate", "Real Estate Broker"},
				{"server", "Server"},
				{"sheriffmasterson", "Sheriff Masterson"},
				{"sheriff", "Sheriff Masterson"},
				{"masterson", "Sheriff Masterson"},
				{"spencergreen", "Spencer \"The Hammer\" Green"},
				{"thehammergreen", "Spencer \"The Hammer\" Green"},
				{"spencerthehammer", "Spencer \"The Hammer\" Green"},
				{"spencerhammergreen", "Spencer \"The Hammer\" Green"},
				{"thehammer", "Spencer \"The Hammer\" Green"},
				{"spencer", "Spencer \"The Hammer\" Green"},
				{"hammer", "Spencer \"The Hammer\" Green"},
				{"green", "Spencer \"The Hammer\" Green"},
			}),
			new([MissionID.ISLEOFSGAIL_THEARKSOCIETY], new(){
				{"architect", "Architect"},
				{"arkitect", "Architect"},
				{"arkmember", "Ark Member"},
				{"arkian", "Ark Member"},
				{"member", "Ark Member"},
				{"blakenathaniel", "Blake Nathaniel"},
				{"blake", "Blake Nathaniel"},
				{"nathaniel", "Blake Nathaniel"},
				{"burialrobes", "Burial Robes"},
				{"burialoutfit", "Burial Robes"},
				{"burial", "Burial Robes"},
				{"robes", "Burial Robes"},
				{"janus", "Burial Robes"},
				{"butler", "Butler"},
				{"castlestaff", "Castle Staff"},
				{"castle", "Castle Staff"},
				{"chef", "Chef"},
				{"custodian", "Custodian"},
				{"eliteguard", "Elite Guard"},
				{"elite", "Elite Guard"},
				{"entertainer", "Entertainer"},
				{"eventstaff", "Event Staff"},
				{"event", "Event Staff"},
				{"guard", "Guard"},
				{"initiate", "Initiate"},
				{"jebediahblock", "Jebediah Block"},
				{"jebediah", "Jebediah Block"},
				{"jebidiah", "Jebediah Block"},
				{"block", "Jebediah Block"},
				{"jeb", "Jebediah Block"},
				{"kightsarmor", "Knight's Armor"},
				{"kightarmor", "Knight's Armor"},
				{"kight", "Knight's Armor"},
				{"armor", "Knight's Armor"},
				{"masterofceremonies", "Master of Ceremonies"},
				{"master", "Master of Ceremonies"},
				{"ceremonies", "Master of Ceremonies"},
				{"moc", "Master of Ceremonies"},
				{"raider", "Raider"},
			}),
			new([MissionID.NEWYORK_GOLDENHANDSHAKE], new(){
				{"bankrobber", "Bank Robber"},
				{"robber", "Bank Robber"},
				{"burglar", "Bank Robber"},
				{"bankteller", "Bank Teller"},
				{"teller", "Bank Teller"},
				{"firedbanker", "Fired Banker"},
				{"fired", "Fired Banker"},
				{"highsecurityguard", "High Security Guard"},
				{"highsecurity", "High Security Guard"},
				{"highguard", "High Security Guard"},
				{"investmentbanker", "Investment Banker"},
				{"investment", "Investment Banker"},
				{"itworker", "IT Worker"},
				{"it", "IT Worker"},
				{"janitor", "Janitor"},
				{"jobapplicant", "Job Applicant"},
				{"applicant", "Job Applicant"},
				{"securityguard", "Security Guard"},
				{"security", "Security Guard"},
				{"guard", "Security Guard"},
			}),
			new([MissionID.HAVEN_THELASTRESORT], new(){
				{"boatcaptain", "Boat Captain"},
				{"boat", "Boat Captain"},
				{"captain", "Boat Captain"},
				{"bodyguard", "Bodyguard"},
				{"butler", "Butler"},
				{"chef", "Chef"},
				{"doctor", "Doctor"},
				{"gassuit", "Gas Suit"},
				{"gas", "Gas Suit"},
				{"boilersuit", "Gas Suit"},
				{"boiler", "Gas Suit"},
				{"lifeguard", "Life Guard"},
				{"life", "Life Guard"},
				{"masseur", "Masseur"},
				{"personaltrainer", "Personal Trainer"},
				{"gymtrainer", "Personal Trainer"},
				{"trainer", "Personal Trainer"},
				{"gym", "Personal Trainer"},
				{"coach", "Personal Trainer"},
				{"resortguard", "Resort Guard"},
				{"resortstaff", "Resort Staff"},
				{"snorkelinstructor", "Snorkel Instructor"},
				{"snorkel", "Snorkel Instructor"},
				{"cj", "Snorkel Instructor"},
				{"scuba", "Snorkel Instructor"},
				{"techcrew", "Tech Crew"},
				{"tech", "Tech Crew"},
				{"crew", "Tech Crew"},
				{"villaguard", "Villa Guard"},
				{"mansionguard", "Villa Guard"},
				{"villastaff", "Villa Staff"},
				{"mansionstaff", "Villa Staff"},
				{"waiter", "Waiter"},
			}),
			new([MissionID.DUBAI_ONTOPOFTHEWORLD], new(){
				{"artcrew", "Art Crew"},
				{"art", "Art Crew"},
				{"crew", "Art Crew"},
				{"eventsecurity", "Event Security"},
				{"security", "Event Security"},
				{"eventstaff", "Event Staff"},
				{"staff", "Event Staff"},
				{"famouschef", "Famous Chef"},
				{"chef", "Famous Chef"},
				{"famous", "Famous Chef"},
				{"helicopterpilot", "Helicopter Pilot"},
				{"helicopter", "Helicopter Pilot"},
				{"pilot", "Helicopter Pilot"},
				{"ingramsbodyguard", "Ingram's Bodyguard"},
				{"bodyguard", "Ingram's Bodyguard"},
				{"ingrambodyguard", "Ingram's Bodyguard"},
				{"carlbodyguard", "Ingram's Bodyguard"},
				{"carlguard", "Ingram's Bodyguard"},
				{"carlsguard", "Ingram's Bodyguard"},
				{"carlsbodyguard", "Ingram's Bodyguard"},
				{"cibodyguard", "Ingram's Bodyguard"},
				{"ciguard", "Ingram's Bodyguard"},
				{"kitchenstaff", "Kitchen Staff"},
				{"kitchen", "Kitchen Staff"},
				{"maintenancestaff", "Maintenance Staff"},
				{"maintenance", "Maintenance Staff"},
				{"penthouseguard", "Penthouse Guard"},
				{"penthouse", "Penthouse Guard"},
				{"penthousestaff", "Penthouse Staff"},
				{"skydivingsuit", "Skydiving Suit"},
				{"skydiving", "Skydiving Suit"},
				{"divingsuit", "Skydiving Suit"},
				{"diving", "Skydiving Suit"},
				{"theassassin", "The Assassin"},
				{"assassin", "The Assassin"},
			}),
			new([MissionID.DARTMOOR_DEATHINTHEFAMILY], new(){
				{"bodyguard", "Bodyguard"},
				{"gardener", "Gardener"},
				{"lawyer", "Lawyer"},
				{"mansionguard", "Mansion Guard"},
				{"guard", "Mansion Guard"},
				{"mansionstaff", "Mansion Staff"},
				{"staff", "Mansion Staff"},
				{"photographer", "Photographer"},
				{"photo", "Photographer"},
				{"cameraman", "Photographer"},
				{"camera", "Photographer"},
				{"privateinvestigator", "Private Investigator"},
				{"investigator", "Private Investigator"},
				{"privatedetective", "Private Investigator"},
				{"detective", "Private Investigator"},
				{"phinas", "Private Investigator"},
				{"pi", "Private Investigator"},
				{"pd", "Private Investigator"},
				{"undertaker", "Undertaker"},
				{"funeral", "Undertaker"},
			}),
			new([MissionID.BERLIN_APEXPREDATOR], new(){
				{"bartender", "Bartender"},
				{"bar", "Bartender"},
				{"biker", "Biker"},
				{"clubcrew", "Club Crew"},
				{"crew", "Club Crew"},
				{"clubsecurity", "Club Security"},
				{"security", "Club Security"},
				{"dealer", "Dealer"},
				{"drug", "Dealer"},
				{"deliveryguy", "Delivery Guy"},
				{"delivery", "Delivery Guy"},
				{"dj", "DJ"},
				{"diskjockey", "DJ"},
				{"floridaman", "Florida Man"},
				{"florida", "Florida Man"},
				{"flman", "Florida Man"},
				{"rolfhirschmuller", "Rolf Hirschmüller"},
				{"rolf", "Rolf Hirschmüller"},
				{"hirschmuller", "Rolf Hirschmüller"},
				{"technician", "Technician"},
				{"tech", "Technician"},
			}),
			new([MissionID.CHONGQING_ENDOFANERA], new(){
				{"blockguard", "Block Guard"},
				{"dumplingcook", "Dumpling Cook"},
				{"dumpling", "Dumpling Cook"},
				{"cook", "Dumpling Cook"},
				{"chef", "Dumpling Cook"},
				{"dumplincook", "Dumpling Cook"},
				{"facilityanalyst", "Facility Analyst"},
				{"analyst", "Facility Analyst"},
				{"facilityengineer", "Facility Engineer"},
				{"engineer", "Facility Engineer"},
				{"eliteguard", "Facility Guard"},
				{"facilityguard", "Facility Guard"},
				{"facilitysecurity", "Facility Security"},
				{"security", "Facility Security"},
				{"homelessperson", "Homeless Person"},
				{"homeless", "Homeless Person"},
				{"person", "Homeless Person"},
				{"hobo", "Homeless Person"},
				{"bum", "Homeless Person"},
				{"perfecttestsubject", "Perfect Test Subject"},
				{"perfecttest", "Perfect Test Subject"},
				{"testsubject", "Perfect Test Subject"},
				{"perfectsubject", "Perfect Test Subject"},
				{"perfect", "Perfect Test Subject"},
				{"test", "Perfect Test Subject"},
				{"subject", "Perfect Test Subject"},
				{"researcher", "Researcher"},
				{"research", "Researcher"},
				{"streetguard", "Street Guard"},
				{"street", "Street Guard"},
				{"guard", "Street Guard"},
				{"theboardmember", "The Board Member"},
				{"boardmember", "The Board Member"},
				{"board", "The Board Member"},
				{"member", "The Board Member"},
				{"mrpritchard", "The Board Member"},
				{"pritchard", "The Board Member"},
			}),
			new([MissionID.MENDOZA_THEFAREWELL], new(){
				{"asadochef", "Asado Chef"},
				{"asado", "Asado Chef"},
				{"chef", "Asado Chef"},
				{"bodyguard", "Bodyguard"},
				{"guard", "Bodyguard"},
				{"chiefwinemaker", "Chief Winemaker"},
				{"chief", "Chief Winemaker"},
				{"gabrielvargas", "Chief Winemaker"},
				{"vargas", "Chief Winemaker"},
				{"gabriel", "Chief Winemaker"},
				{"winemaker", "Chief Winemaker"},
				{"cw", "Chief Winemaker"},
				{"corvoblack", "Corvo Black"},
				{"corvo", "Corvo Black"},
				{"black", "Corvo Black"},
				{"gaucho", "Gaucho"},
				{"headofsecurity", "Head of Security"},
				{"hos", "Head of Security"},
				{"head", "Head of Security"},
				{"juancortazar", "Head of Security"},
				{"cortazar", "Head of Security"},
				{"lawyer", "Lawyer"},
				{"law", "Lawyer"},
				{"mercenary", "Mercenary"},
				{"merc", "Mercenary"},
				{"providenceherald", "Providence Herald"},
				{"providence", "Providence Herald"},
				{"herald", "Providence Herald"},
				{"sommelier", "Sommelier"},
				{"tangomusician", "Tango Musician"},
				{"tango", "Tango Musician"},
				{"musician", "Tango Musician"},
				{"waiter", "Waiter"},
				{"wineryworker", "Winery Worker"},
				{"winery", "Winery Worker"},
				{"worker", "Winery Worker"},
			}),
			new([MissionID.CARPATHIAN_UNTOUCHABLE], new(){
				{"officestaff", "Office Staff"},
				{"office", "Office Staff"},
				{"staff", "Office Staff"},
				{"providencecommando", "Providence Commando"},
				{"commando", "Providence Commando"},
				{"providencecommandoleader", "Providence Commando Leader"},
				{"commandoleader", "Providence Commando Leader"},
				{"providencedoctor", "Providence Doctor"},
				{"doctor", "Providence Doctor"},
				{"providenceeliteguard", "Providence Elite Guard"},
				{"eliteguard", "Providence Elite Guard"},
				{"elite", "Providence Elite Guard"},
				{"militiazone", "Providence Security Guard (Militia Zone)"},
				{"militia", "Providence Security Guard (Militia Zone)"},
				{"securityguardoffice", "Providence Security Guard (Office)"},
				{"guardoffice", "Providence Security Guard (Office)"},
				{"officeguard", "Providence Security Guard (Office)"},
			}),
			new([MissionID.AMBROSE_SHADOWSINTHEWATER], new(){
				{"cook", "Cook"},
				{"engineer", "Engineer"},
				{"hippie", "Hippie"},
				{"metalworker", "Metal Worker"},
				{"metal", "Metal Worker"},
				{"worker", "Metal Worker"},
				{"militiasoldier", "Militia Soldier"},
				{"militia", "Militia Soldier"},
				{"soldier", "Militia Soldier"},
				{"pirate", "Pirate"},
			}),
		];
		public static readonly Dictionary<string, string> MethodKeywords = new() {
			{"amputation", "Amputation Knife"},
			{"ampknife", "Amputation Knife"},
			{"amputationknife", "Amputation Knife"},
			{"antiquecurvedknife", "Antique Curved Knife"},
			{"antiquecurveknife", "Antique Curved Knife"},
			{"antiqueknife", "Antique Curved Knife"},
			{"curvedknife", "Antique Curved Knife"},
			{"curveknife", "Antique Curved Knife"},
			{"acknife", "Antique Curved Knife"},
			{"barberrazor", "Barber Razor"},
			{"razor", "Barber Razor"},
			{"battleaxe", "Battle Axe"},
			{"bataxe", "Battle Axe"},
			{"baxe", "Battle Axe"},
			{"beakstaff", "Beak Staff"},
			{"broadsword", "Broadsword"},
			{"burialknife", "Burial Knife"},
			{"burialdagger", "Burial Knife"},
			{"circumcision", "Circumcision Knife"},
			{"circumcisionknife", "Circumcision Knife"},
			{"circknife", "Circumcision Knife"},
			{"cleaver", "Cleaver"},
			{"meatcleaver", "Cleaver"},
			{"combatknife", "Combat Knife"},
			{"concealknife", "Concealable Knife"},
			{"concealableknife", "Concealable Knife"},
			{"fireaxe", "Fire Axe"},
			{"foldingknife", "Folding Knife"},
			{"foldknife", "Folding Knife"},
			{"gardenfork", "Garden Fork"},
			{"grapeknife", "Grape Knife"},
			{"hatchet", "Hatchet"},
			{"hobby", "Hobby Knife"},
			{"hobbyknife", "Hobby Knife"},
			{"boxcutter", "Hobby Knife"},
			{"holidayfireaxe", "Holiday Fire Axe"},
			{"festivefireaxe", "Holiday Fire Axe"},
			{"christmasfireaxe", "Holiday Fire Axe"},
			{"xmasfireaxe", "Holiday Fire Axe"},
			{"hook", "Hook"},
			{"icicle", "Icicle"},
			{"ice", "Icicle"},
			{"jarlspiratesaber", "Jarl's Pirate Saber"},
			{"piratesaber", "Jarl's Pirate Saber"},
			{"jarlssaber", "Jarl's Pirate Saber"},
			{"piratesword", "Jarl's Pirate Saber"},
			{"jarlspiratesword", "Jarl's Pirate Saber"},
			{"jarlssword", "Jarl's Pirate Saber"},
			{"katana", "Katana"},
			{"katanasword", "Katana"},
			{"kitchenknife", "Kitchen Knife"},
			{"kknife", "Kitchen Knife"},
			{"knife", "Kitchen Knife"},
			{"kukrimachete", "Kukri Machete"},
			{"kukri", "Kukri Machete"},
			{"letteropener", "Letter Opener"},
			{"letopener", "Letter Opener"},
			{"machete", "Machete"},
			{"meatfork", "Meat Fork"},
			{"fork", "Meat Fork"},
			{"oldaxe", "Old Axe"},
			{"ornatescimitar", "Ornate Scimitar"},
			{"ornate", "Ornate Scimitar"},
			{"scimitar", "Ornate Scimitar"},
			{"rustyscrewdriver", "Rusty Screwdriver"},
			{"rustedscrewdriver", "Rusty Screwdriver"},
			{"rustscrewdriver", "Rusty Screwdriver"},
			{"saber", "Saber"},
			{"sacknife", "Sacrificial Knife"},
			{"sacrificial", "Sacrificial Knife"},
			{"sacrificialknife", "Sacrificial Knife"},
			{"sappersaxe", "Sapper's Axe"},
			{"sappers", "Sapper's Axe"},
			{"sapperaxe", "Sapper's Axe"},
			{"sapaxe", "Sapper's Axe"},
			{"scalpel", "Scalpel"},
			{"scissors", "Scissors"},
			{"scissor", "Scissors"},
			{"scrapsword", "Scrap Sword"},
			{"scrap", "Scrap Sword"},
			{"screwdriver", "Screwdriver"},
			{"seashell", "Seashell"},
			{"shell", "Seashell"},
			{"shears", "Shears"},
			{"shear", "Shears"},
			{"shuriken", "Shuriken"},
			{"ninjastar", "Shuriken"},
			{"throwingstar", "Shuriken"},
			{"electrocution", "Electrocution"},
			{"electro", "Electrocution"},
			{"electrocute", "Electrocution"},
			{"elec", "Electrocution"},
			{"electricity", "Electrocution"},
			{"electric", "Electrocution"},
			{"zap", "Electrocution"},
			{"explosion", "Explosion"},
			{"stemcells", "Poison Stem Cells"},
			{"poisonstemcells", "Poison Stem Cells"},
			{"poison", "Poison Stem Cells"},
			{"poisoncells", "Poison Stem Cells"},
			{"robotarms", "Robot Arms"},
			{"arms", "Robot Arms"},
			{"robot", "Robot Arms"},
			{"kai", "Robot Arms"},
			{"shootheart", "Shoot Heart"},
			{"shoottheheart", "Shoot Heart"},
			{"heartshot", "Shoot Heart"},
			{"trashheart", "Trash Heart"},
			{"trashcan", "Trash Heart"},
			{"throwheart", "Trash Heart"},
			{"trash", "Trash Heart"},
			{"throwheartinthetrash", "Trash Heart"},
			{"throwtheheartinthetrash", "Trash Heart"},
			{"throwheartintrash", "Trash Heart"},
			{"starfish", "Starfish"},
			{"tanto", "Tanto"},
			{"unicornhorn", "Unicorn Horn"},
			{"unicorn", "Unicorn Horn"},
			{"vikingaxe", "Viking Axe"},
			{"viking", "Viking Axe"},
			{"xmasstar", "Xmas Star"},
			{"christmasstar", "Xmas Star"},
			{"festivestar", "Xmas Star"},
		};
		public static readonly Dictionary<string, KillComplication> ComplicationKeywords = new() {
			{"live", KillComplication.Live},
			{"nko", KillComplication.Live},
			{"noko", KillComplication.Live},
			{"nonko", KillComplication.Live},
			{"ntko", KillComplication.Live},
			{"notargetko", KillComplication.Live},
			{"notargetpacification", KillComplication.Live},
			{"nopacification", KillComplication.Live},
			{"nopacify", KillComplication.Live},
			{"notargetknockout", KillComplication.Live},
			{"noknockout", KillComplication.Live},
			{"notargetpacify", KillComplication.Live},
			{"donotko", KillComplication.Live},
			{"donotpacify", KillComplication.Live},
			{"nonpacify", KillComplication.Live},
			{"nonpacification", KillComplication.Live},
		};
		public static readonly Dictionary<string, KillType> KillTypeKeywords = new() {
			{"loud", KillType.Loud},
			{"ld", KillType.Loud},
			{"silenced", KillType.Silenced},
			{"silence", KillType.Silenced},
			{"sil", KillType.Silenced},
			{"silent", KillType.Silenced},
			{"melee", KillType.Melee},
			{"mel", KillType.Melee},
			{"thrown", KillType.Thrown},
			{"throw", KillType.Thrown},
			{"remote", KillType.Remote},
			{"impact", KillType.Impact},
			{"imp", KillType.Impact},
			{"rem", KillType.Remote},
			{"loudremote", KillType.LoudRemote},
			{"ldremote", KillType.LoudRemote},
			{"loudrem", KillType.LoudRemote},
			{"ldrem", KillType.LoudRemote},
		};
		public static readonly Dictionary<string, bool> IgnoreKeywords = new() {
			{"as", true},
			{"in", true},
			{"with", true},
		};
		
		public static bool CreateSpinFromParseContexts(List<ParseContext> contexts, out Spin spin) {
			spin = null;
			if (contexts.Count == 0) return false;
			var mission = new Mission(contexts.First().mission);

			spin = new Spin();

			foreach (var target in mission.Targets) {
				var context = mission.ID == MissionID.BERLIN_APEXPREDATOR
					? (spin.Conditions.Count < contexts.Count ? contexts[spin.Conditions.Count] : null)
					: contexts.Find(ctx => ctx.target == target.Name);
				if (context == null) return false;
				var disguise = Mission.GetDisguiseByName(context.mission, context.disguise)
				            ?? Mission.GetSuitDisguise(context.mission);
				if (disguise == null) return false;
				if (!context.HaveKillMethod) context.standardKillMethod = StandardKillMethod.NeckSnap;
				var killMethod = context.CreateKillMethod();
				if (killMethod == null) return false;
				spin.Conditions.Add(new() {
					Target = target,
					Disguise = disguise,
					Method = killMethod,
				});
			}

			return true;
		}

		public static bool Parse(string input, out Spin spin)
		{
			spin = null;
			List<ParseContext> contexts = [];
			List<string> tokens = [];
			contexts.Add(new(tokens));
			var detectedMission = MissionID.NONE;
			var processed = ProcessInput(input);

			while (true) {
				var context = contexts.Last<ParseContext>();
				var haveFullCondition = ParseCondition(processed, contexts.Last());
				var mission = context.mission;
				var nextIndex = context.nextIndex;

				if (haveFullCondition) {
					if (detectedMission == MissionID.NONE)
						detectedMission = mission;
					else if (mission != detectedMission)
						return false;

					contexts.Add(new(tokens) {
						nextIndex = nextIndex,
						mission = mission,
						conditions = contexts.Count,
					});
				}
				else if (contexts.Count > 1 && !context.HaveTarget && !context.HaveDisguise && !context.HaveKillMethod) {
					var prevContext = contexts[^2];
					if (prevContext.killComplication == KillComplication.None)
						prevContext.killComplication = context.killComplication;
					if (prevContext.killType == KillType.Any)
						prevContext.killType = context.killType;

					context.killComplication = KillComplication.None;
					context.killType = KillType.Any;
				}
				if (nextIndex >= tokens.Count)
					break;
			}

			return CreateSpinFromParseContexts(contexts, out spin);
		}

		public static bool ParseCondition(string processed, ParseContext context)
		{
			bool parseTargetToken(string token) {
				if (TargetKeywords.TryGetValue(token, out string targetKey)) {
					if (!context.HaveTarget) {
						if (Target.GetTargetFromKey(targetKey, out Target target)) {
							context.target = target.Name;
							context.mission = target.Mission;
						}
					}
					return true;
				}
				return false;
			}

			bool parseToken(string token) {
				if (ComplicationKeywords.TryGetValue(token, out var complication)) {
					context.killComplication = complication;
					return true;
				}
				if (KillTypeKeywords.TryGetValue(token, out var killType)) {
					context.killType = killType;
					return true;
				}
				if (!context.HaveKillMethod) {
					if (MethodKeywords.TryGetValue(token, out string methodName)) {
						if (KillMethod.ParseFirearmMethod(methodName, out var firearmMethod)) {
							context.firearmKillMethod = firearmMethod;
							return true;
						}
						if (KillMethod.ParseStandardMethod(methodName, out var standardMethod)) {
							context.standardKillMethod = standardMethod;
							return true;
						}
						if (KillMethod.ParseSpecificMethod(methodName, out var specificMethod)) {
							context.specificKillMethod = specificMethod;
							return true;
						}
					}
					else {
						if (KillMethod.ParseFirearmMethod(token, out var firearmMethod)) {
							context.firearmKillMethod = firearmMethod;
							return true;
						}
						if (KillMethod.ParseStandardMethod(token, out var standardMethod)) {
							context.standardKillMethod = standardMethod;
							return true;
						}
						if (KillMethod.ParseSpecificMethod(token, out var specificMethod)) {
							context.specificKillMethod = specificMethod;
							return true;
						}
					}
				}

				var alreadyHaveTarget = context.HaveTarget;
				if (!alreadyHaveTarget && parseTargetToken(token))
					return true;

				if (context.mission == MissionID.NONE)
					return false;

				foreach (var dkws in DisguiseKeywords) {
					if (dkws.missions.Count != 0 && !dkws.missions.Contains(context.mission))
						continue;
					if (dkws.keywords.TryGetValue(token, out var disguise)) {
						context.disguise = disguise;
						return true;
					}
				}

				return false;
			};

			if (context.tokens.Count == 0 && context.conditions == 0) {
				var token = "";

				foreach (var c in processed) {
					if (!char.IsWhiteSpace(c)) {
						if (char.IsLetterOrDigit(c))
							token += c;
						continue;
					}

					if (token.Length == 0)
						continue;

					if (Substitutions.TryGetValue(token, out string substitution))
						context.tokens.Add(substitution);
					else
						context.tokens.Add(token);

					if (c == '\n')
						context.tokens.Add("\n");

					token = "";
				}

				if (token.Length > 0)
					context.tokens.Add(token);
			}
			
			var i = context.nextIndex;

			if (context.mission == MissionID.NONE) {
				string token;

				// Sweep through all the tokens first to find a target, in order to determine the mission.
				while (i < context.tokens.Count) {
					var tokensLeft = context.tokens.Count - i;

					switch (tokensLeft) {
						default:
						case 3:
							token = context.tokens[i] + context.tokens[i + 1] + context.tokens[i + 2];
							if (parseTargetToken(token)) {
								context.tokens.RemoveRange(i, 3);
								break;
							}
							goto case 2;
						case 2:
							token = context.tokens[i] + context.tokens[i + 1];
							if (parseTargetToken(token)) {
								context.tokens.RemoveRange(i, 2);
								break;
							}
							goto case 1;
						case 1:
							if (parseTargetToken(context.tokens[i])) {
								context.tokens.RemoveAt(i);
								break;
							}
							++i;
							continue;
						case 0:
							break;
					}

					break;
				}

				// No target names found, presume this is Berlin
				if (context.mission == MissionID.NONE)
					context.mission = MissionID.BERLIN_APEXPREDATOR;
			}

			for (i = context.nextIndex; i < context.tokens.Count; ) {
				var tokensLeft = context.tokens.Count - i;

				if (context.tokens[i] == "\n") {
					++i;
					break;
				}

				switch (tokensLeft) {
					default:
					case 4:
						if (parseToken(context.tokens[i] + context.tokens[i + 1] + context.tokens[i + 2] + context.tokens[i + 3])) {
							context.tokens.RemoveRange(i, 4);
							break;
						}
						goto case 3;
					case 3:
						if (parseToken(context.tokens[i] + context.tokens[i + 1] + context.tokens[i + 2])) {
							context.tokens.RemoveRange(i, 3);
							break;
						}
						goto case 2;
					case 2:
						if (parseToken(context.tokens[i] + context.tokens[i + 1])) {
							context.tokens.RemoveRange(i, 2);
							break;
						}
						goto case 1;
					case 1:
						if (parseToken(context.tokens[i]) || IgnoreKeywords.ContainsKey(context.tokens[i])) {
							context.tokens.RemoveAt(i);
							break;
						}
						else ++i;
						break;
					case 0:
						break;
				}

				if (
					(context.HaveTarget || context.mission == MissionID.BERLIN_APEXPREDATOR)
					&& context.HaveDisguise
					&& context.HaveKillMethod
				) break;
			}

			context.nextIndex = i;
			return (context.HaveTarget || context.mission == MissionID.BERLIN_APEXPREDATOR) && context.HaveDisguise && context.HaveKillMethod;
		}

		private static string ProcessInput(string input)
		{
			return Strings.TokenCharacterWithSpacesRegex.Replace(input.RemoveDiacritics().ToLower(), " ");
		}
	}
}
