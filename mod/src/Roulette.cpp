#include "Roulette.h"
#include "Target.h"
#include "util.h"
#include <string>
#include <string_view>
#include <variant>
#include <unordered_map>

using namespace std::literals::string_literals;

std::random_device RouletteSpinGenerator::rd;
std::mt19937 RouletteSpinGenerator::gen(rd());
std::set<eMethodTag> RouletteTarget::emptyMethodTags;
std::unordered_map<std::string, Keyword::Variant> Keyword::keywordMap;
std::unordered_map<std::string, std::string> Keyword::targetKeyMap = {
	{"Kalvin Ritter", "KR"},
	{"Jasper Knight", "JK"},
	{"Viktor Novikov", "VN"},
	{"Dalia Margolis", "DM"},
	{"Harry \"Smokey\" Bagnato", "HSB"},
	{"Marv \"Slick\" Gonif", "MSG"},
	{"Silvio Caruso", "SC"},
	{"Francesca De Santis", "FDS"},
	{"Dino Bosco", "DB"},
	{"Marco Abiatti", "MA"},
	{"Craig Black", "CB"},
	{"Brother Akram", "BA"},
	{"Reza Zaydan", "RZ"},
	{"Claus Hugo Strandberg", "CHS"},
	{"Kong Tuo-Kwang", "KTK"},
	{"Matthieu Mendola", "MM"},
	{"Jordan Cross", "JC"},
	{"Ken Morgan", "KM"},
	{"Oybek Nabazov", "ON"},
	{"Sister Yulduz", "SY"},
	{"Sean Rose", "SR"},
	{"Penelope Graves", "PG"},
	{"Ezra Berg", "EB"},
	{"Maya Parvati", "MP"},
	{"Erich Soders", "ES"},
	{"Yuki Yamazaki", "YY"},
	{"Owen Cage", "OC"},
	{"Klaus Liebleid", "KL"},
	{"Alma Reynard", "AR"},
	{"Sierra Knox", "SK"},
	{"Robert Knox", "RK"},
	{"Ajit \"AJ\" Krish", "AJ"},
	{"Rico Delgado", "RD"},
	{"Jorge Franco", "JF"},
	{"Andrea Martinez", "AM"},
	{"Blair Reddington", "BR"},
	{"Wazir Kale", "WK"},
	{"Vanya Shah", "VS"},
	{"Dawood Rangan", "DR"},
	{"Basil Carnaby", "BC"},
	{"Janus", "J"},
	{"Nolan Cassidy", "NC"},
	{"Galen Vholes", "GV"},
	{"Zoe Washington", "ZW"},
	{"Sophia Washington", "SW"},
	{"Athena Savalas", "AS"},
	{"Tyson Williams", "TW"},
	{"Steven Bradley", "SB"},
	{"Ljudmila Vetrova", "LV"},
	{"Carl Ingram", "CI"},
	{"Marcus Stuyvesant", "MS"},
	{"Alexa Carlisle", "AC"},
	{"ICA Agent #1", "1"},
	{"ICA Agent #2", "2"},
	{"ICA Agent #3", "3"},
	{"ICA Agent #4", "4"},
	{"ICA Agent #5", "5"},
	{"Hush", "H"},
	{"Imogen Royce", "IR"},
	{"Don Archibald Yates", "DY"},
	{"Tamara Vidal", "TV"},
	{"Arthur Edwards", "AE"},
	{"Noel Crest", "NCR"},
	{"Sinhi \"Akka\" Venthan", "SV"},
};
std::unordered_map<std::string, std::string> Keyword::keyTargetMap = {
	{"KR", "Kalvin Ritter"},
	{"JK", "Jasper Knight"},
	{"VN", "Viktor Novikov"},
	{"DM", "Dalia Margolis"},
	{"HSB", "Harry \"Smokey\" Bagnato"},
	{"MSG", "Marv \"Slick\" Gonif"},
	{"SC", "Silvio Caruso"},
	{"FDS", "Francesca De Santis"},
	{"DB", "Dino Bosco"},
	{"MA", "Marco Abiatti"},
	{"CB", "Craig Black"},
	{"BA", "Brother Akram"},
	{"RZ", "Reza Zaydan"},
	{"CHS", "Claus Hugo Strandberg"},
	{"JC", "Jordan Cross"},
	{"KM", "Ken Morgan"},
	{"SR", "Sean Rose"},
	{"ON", "Oybek Nabazov"},
	{"SY", "Sister Yulduz"},
	{"PG", "Penelope Graves"},
	{"EB", "Ezra Berg"},
	{"MP", "Maya Parvati"},
	{"ES", "Erich Soders"},
	{"YY", "Yuki Yamazaki"},
	{"OC", "Owen Cage"},
	{"KL", "Klaus Liebleid"},
	{"AR", "Alma Reynard"},
	{"SK", "Sierra Knox"},
	{"AJ", "Ajit \"AJ\" Krish"},
	{"RK", "Robert Knox"},
	{"RD", "Rico Delgado"},
	{"JF", "Jorge Franco"},
	{"AM", "Andrea Martinez"},
	{"BR", "Blair Reddington"},
	{"WK", "Wazir Kale"},
	{"VS", "Vanya Shah"},
	{"DR", "Dawood Rangan"},
	{"BC", "Basil Carnaby"},
	{"J", "Janus"},
	{"NC", "Nolan Cassidy"},
	{"GV", "Galen Vholes"},
	{"ZW", "Zoe Washington"},
	{"SW", "Sophia Washington"},
	{"AS", "Athena Savalas"},
	{"TW", "Tyson Williams"},
	{"SB", "Steven Bradley"},
	{"LV", "Ljudmila Vetrova"},
	{"CI", "Carl Ingram"},
	{"MS", "Marcus Stuyvesant"},
	{"AC", "Alexa Carlisle"},
	{"1", "ICA Agent #1"},
	{"2", "ICA Agent #2"},
	{"3", "ICA Agent #3"},
	{"4", "ICA Agent #4"},
	{"5", "ICA Agent #5"},
	{"H", "Hush"},
	{"IR", "Imogen Royce"},
	{"DY", "Don Archibald Yates"},
	{"TV", "Tamara Vidal"},
	{"AE", "Arthur Edwards"},
	{"NCR", "Noel Crest"},
	{"SV", "Sinhi \"Akka\" Venthan"},
};
std::unordered_map<std::string, eMission> targetMissionMap = {
	{"Kalvin Ritter", eMission::ICAFACILITY_FREEFORM},
	{"Jasper Knight", eMission::ICAFACILITY_FINALTEST},
	{"Viktor Novikov", eMission::PARIS_SHOWSTOPPER},
	{"Dalia Margolis", eMission::PARIS_SHOWSTOPPER},
	{"Harry \"Smokey\" Bagnato", eMission::PARIS_HOLIDAYHOARDERS},
	{"Marv \"Slick\" Gonif", eMission::PARIS_HOLIDAYHOARDERS},
	{"Silvio Caruso", eMission::SAPIENZA_WORLDOFTOMORROW},
	{"Francesca De Santis", eMission::SAPIENZA_WORLDOFTOMORROW},
	{"Craig Black", eMission::SAPIENZA_THEAUTHOR},
	{"Brother Akram", eMission::SAPIENZA_THEAUTHOR},
	{"Dino Bosco", eMission::SAPIENZA_THEICON},
	{"Marco Abiatti", eMission::SAPIENZA_LANDSLIDE},
	{"Reza Zaydan", eMission::MARRAKESH_GILDEDCAGE},
	{"Claus Hugo Strandberg", eMission::MARRAKESH_GILDEDCAGE},
	{"Jordan Cross", eMission::BANGKOK_CLUB27},
	{"Ken Morgan", eMission::BANGKOK_CLUB27},
	{"Oybek Nabazov", eMission::BANGKOK_THESOURCE},
	{"Sister Yulduz", eMission::BANGKOK_THESOURCE},
	{"Sean Rose", eMission::COLORADO_FREEDOMFIGHTERS},
	{"Penelope Graves", eMission::COLORADO_FREEDOMFIGHTERS},
	{"Ezra Berg", eMission::COLORADO_FREEDOMFIGHTERS},
	{"Maya Parvati", eMission::COLORADO_FREEDOMFIGHTERS},
	{"Erich Soders", eMission::HOKKAIDO_SITUSINVERSUS},
	{"Yuki Yamazaki", eMission::HOKKAIDO_SITUSINVERSUS},
	{"Owen Cage", eMission::HOKKAIDO_PATIENTZERO},
	{"Klaus Liebleid", eMission::HOKKAIDO_PATIENTZERO},
	{"Alma Reynard", eMission::HAWKESBAY_NIGHTCALL},
	{"Sierra Knox", eMission::MIAMI_FINISHLINE},
	{"Robert Knox", eMission::MIAMI_FINISHLINE},
	{"Ajit \"AJ\" Krish", eMission::MIAMI_ASILVERTONGUE},
	{"Rico Delgado", eMission::SANTAFORTUNA_THREEHEADEDSERPENT},
	{"Jorge Franco", eMission::SANTAFORTUNA_THREEHEADEDSERPENT},
	{"Andrea Martinez", eMission::SANTAFORTUNA_THREEHEADEDSERPENT},
	{"Blair Reddington", eMission::SANTAFORTUNA_EMBRACEOFTHESERPENT},
	{"Wazir Kale", eMission::MUMBAI_CHASINGAGHOST},
	{"Vanya Shah", eMission::MUMBAI_CHASINGAGHOST},
	{"Dawood Rangan", eMission::MUMBAI_CHASINGAGHOST},
	{"Basil Carnaby", eMission::MUMBAI_ILLUSIONSOFGRANDEUR},
	{"Janus", eMission::WHITTLETON_ANOTHERLIFE},
	{"Nolan Cassidy", eMission::WHITTLETON_ANOTHERLIFE},
	{"Galen Vholes", eMission::WHITTLETON_ABITTERPILL},
	{"Zoe Washington", eMission::ISLEOFSGAIL_THEARKSOCIETY},
	{"Sophia Washington", eMission::ISLEOFSGAIL_THEARKSOCIETY},
	{"Athena Savalas", eMission::NEWYORK_GOLDENHANDSHAKE},
	{"Tyson Williams", eMission::HAVEN_THELASTRESORT},
	{"Steven Bradley", eMission::HAVEN_THELASTRESORT},
	{"Ljudmila Vetrova", eMission::HAVEN_THELASTRESORT},
	{"Carl Ingram", eMission::DUBAI_ONTOPOFTHEWORLD},
	{"Marcus Stuyvesant", eMission::DUBAI_ONTOPOFTHEWORLD},
	{"Alexa Carlisle", eMission::DARTMOOR_DEATHINTHEFAMILY},
	{"ICA Agent #1", eMission::BERLIN_APEXPREDATOR},
	{"ICA Agent #2", eMission::BERLIN_APEXPREDATOR},
	{"ICA Agent #3", eMission::BERLIN_APEXPREDATOR},
	{"ICA Agent #4", eMission::BERLIN_APEXPREDATOR},
	{"ICA Agent #5", eMission::BERLIN_APEXPREDATOR},
	{"Hush", eMission::CHONGQING_ENDOFANERA},
	{"Imogen Royce", eMission::CHONGQING_ENDOFANERA},
	{"Don Archibald Yates", eMission::MENDOZA_THEFAREWELL},
	{"Tamara Vidal", eMission::MENDOZA_THEFAREWELL},
	{"Arthur Edwards", eMission::CARPATHIAN_UNTOUCHABLE},
	{"Noel Crest", eMission::AMBROSE_SHADOWSINTHEWATER},
	{"Sinhi \"Akka\" Venthan", eMission::AMBROSE_SHADOWSINTHEWATER},
};
std::map<std::string, eTargetID, InsensitiveCompareLexicographic> targetsByRepoId = {
	{"3d25ee6c-61fa-4ba5-8f19-fedd905fd8fb", eTargetID::KalvinRitter},
	{"579f2544-1970-4865-afa3-ad4566e5f98d", eTargetID::JasperKnight},
	{"052434e7-f451-462f-a9d7-13657cb047c0", eTargetID::ViktorNovikov},
	{"edad702b-5b37-4dc1-a47c-36a1588f1d3f", eTargetID::DaliaMargolis},
	{"21017f2e-3090-4b41-91b9-1ec4ddd358f9", eTargetID::HarrySmokeyBagnato},
	{"a713a8a2-e203-4032-9c4a-1f8eab4c3efa", eTargetID::MarvSlickGonif},
	{"0dfaea51-3c36-4722-9eff-f1e7ef139878", eTargetID::SilvioCaruso},
	{"94ab740b-b30f-4086-9aea-5c9c0de28456", eTargetID::FrancescaDeSantis},
	{"d2c71b7c-8c85-479e-9c14-754f1b61f2e4", eTargetID::DinoBosco}, // 9c81b545-a234-46c5-9296-54f9f2a0bb6d
	{"91071202-32b6-488f-aee7-09d8d847a6aa", eTargetID::MarcoAbiatti},
	{"ab48a89d-e8a7-4df4-ae72-f0fccaa65e7f", eTargetID::CraigBlack}, // 0b22a7cd-edd7-483d-8406-0a8706ae8047, ab48a89d-e8a7-4df4-ae72-f0fccaa65e7f, fb1ac120-0cea-4477-8344-8b4ffa5c3b01 (ET)
	{"07c64264-9fa9-4c4a-a933-802011bee9e6", eTargetID::BrotherAkram},
	{"ca31c88f-d15e-407b-8407-231f1b068402", eTargetID::RezaZaydan},
	{"b38b0b62-8071-4761-b2a5-2f635cd8da1b", eTargetID::ClausHugoStrandberg},
	{"23415c1d-6ee8-41a8-9975-523f3eede581", eTargetID::KongTuoKwang},
	{"4d0953f0-7f19-4130-abae-610a8f5dddca", eTargetID::MatthieuMendola},
	{"ee3f55b8-12f8-4245-8ef2-3022b4f6f120", eTargetID::JordanCross},
	{"f65fff84-6cad-4a11-9a0a-b89430c03397", eTargetID::KenMorgan},
	{"010e451b-99de-4a73-a6df-520158636654", eTargetID::OybekNabazov},
	{"893ecd0c-9a13-42a6-b3f5-72c6059d4b16", eTargetID::SisterYulduz},
	{"5b54d9fb-fa85-4302-a8d5-c5c5e97344c4", eTargetID::SeanRose},
	{"963c2774-cb9a-4b0c-ab69-210b2405383b", eTargetID::PenelopeGraves},
	{"1a8a827f-932e-49c0-a1b3-e3201795ae19", eTargetID::EzraBerg},
	{"d94f3e83-36e3-453c-8d4b-28c93229826a", eTargetID::MayaParvati},
	{"5651198f-9ef7-4f3c-908b-a570f1cd64e2", eTargetID::ErichSoders},
	{"9bebb40a-3746-4ba2-8bfc-a1fcabaec72c", eTargetID::YukiYamazaki},
	{"3798fcf3-4e33-40db-a8a2-c160a3ec55bf", eTargetID::OwenCage},
	{"01822ba2-7b3b-4bbd-a9f3-cf8006ba945a", eTargetID::KlausLiebleid},
	{"5087ab4a-586c-4967-967c-487cc2d17c92", eTargetID::DmitriFedorov},
	{"9e85c891-9eb7-4f3c-8115-896bf54d979b", eTargetID::AlmaReynard},
	{"c0ab162c-1502-40d5-801f-c5471289d6b7", eTargetID::SierraKnox}, // af77ead8-72d4-461c-adb8-dd55146d029f car? 
	{"ee454990-0c4b-49e5-9572-a67887325283", eTargetID::RobertKnox},
	{"466b652d-cea0-47eb-823a-2aba9f67b5ad", eTargetID::AjitKrish},
	{"00df867e-f27f-4904-8bc7-9504443ccb5a", eTargetID::RicoDelgado},
	{"b87b242e-4ef4-42d8-94ed-17cbfc9009bf", eTargetID::JorgeFranco},
	{"db21a429-add2-46fa-8176-540f846d89e0", eTargetID::AndreaMartinez},
	{"1e8bbf5a-473e-4b00-b58a-4706f13e497b", eTargetID::BlairReddington},
	{"c7c9e213-16f9-4215-bf07-dd8f801ce3e0", eTargetID::WazirKale},
	{"076f23cc-09d8-423f-b890-74020f53b1d6", eTargetID::VanyaShah},
	{"78f98c70-b7be-4578-9b6a-1c96a3e1ff1a", eTargetID::DawoodRangan},
	{"07ccfa5d-6dd3-4d2b-9f79-75b300524857", eTargetID::BasilCarnaby},
	{"672a7a52-a08a-45cd-a061-ced6a7b8d8c4", eTargetID::Janus},
	{"b8f0bf6c-4826-4de2-a785-2d139967e09c", eTargetID::NolanCassidy},
	{"3cf8ef68-e6f3-466a-9299-de50e9f0a9bb", eTargetID::GalenVholes},
	{"7eb39f2d-1030-44d2-be82-6df608085ec0", eTargetID::ZoeWashington},
	{"67f39ab8-c25f-48c3-84be-0ec495a553ec", eTargetID::SophiaWashington},
	{"ad93e268-3d6e-4aba-bec0-607cb5451ac7", eTargetID::AthenaSavalas},
	{"7504b78e-e766-42fe-930c-c5640f5f507b", eTargetID::TysonWilliams},
	{"0afcc59e-6d6e-433f-8404-7699df872c9d", eTargetID::StevenBradley},
	{"5bc06fb1-bfb3-48ef-94ae-6f18c16c1eee", eTargetID::LjudmilaVetrova},
	{"bd0689d6-07b4-4757-b8ee-cac19f1c9e16", eTargetID::CarlIngram},
	{"9571d196-8d67-4d94-8dad-6e2d970d7a91", eTargetID::MarcusStuyvesant},
	{"080efb03-a66a-401e-b6df-4eac496e9e2d", eTargetID::AlexaCarlisle},
	{"967abcf9-2672-4e81-8fef-211aaa366747", eTargetID::Hush},
	{"a7fd7a4f-2bee-4787-bc60-90f9dd64233b", eTargetID::ImogenRoyce},
	{"57907f04-329e-4faf-b753-7e95d5c2e085", eTargetID::DonArchibaldYates},
	{"651ceb9a-117f-4f8d-89dd-9b6bd2a38b5a", eTargetID::TamaraVidal},
	{"f133d999-a93b-450a-b236-a7ed404a38e2", eTargetID::ArthurEdwards},
	{"4f80ca40-a8f3-4a5e-9add-4c74d3bb5bcc", eTargetID::NoelCrest},
	{"666521d2-1efe-4d0a-b7f7-2afeb08262b6", eTargetID::SinhiAkkaVenthan},

	{"05966483-af64-47a6-9e3b-e639fe23206d", eTargetID::AgentChamberlin},
	{"107c35d7-7300-417c-832a-4f36cd3071b9", eTargetID::AgentThames},
	{"28cb7e91-bf9c-46ee-a371-1bd1448f1994", eTargetID::AgentThames},
	{"1305c2e4-6394-4cfa-b873-22adbd0c9702", eTargetID::AgentChamberlin},
	{"252428ca-3f8e-4477-b2b9-58f18cff3e44", eTargetID::AgentTremaine},
	{"2ab07903-e958-4af6-b01c-b62058745ce1", eTargetID::AgentLowenthal},
	{"5fd9b187-0745-492a-88a0-afcc51f90c46", eTargetID::AgentMontgomery},
	{"633398ac-c4b4-4441-852d-ae6460172025", eTargetID::AgentGreen},
	{"88fd409e-6570-4a42-94f7-e5d362a8d960", eTargetID::AgentThames},
	{"8b29da09-461f-44d7-9042-d4fde829b9f2", eTargetID::AgentMontgomery},
	{"8b7c3ec6-c072-4a21-a323-0f8751028052", eTargetID::AgentGreen},
	{"922deccd-7fb4-45d9-ae3d-2cf11915c403", eTargetID::AgentSwan},
	{"abd1c0e7-e406-43bd-9185-419029c5bf3d", eTargetID::AgentBanner},
	{"b8e7e65b-587e-471b-894d-282cda6614d4", eTargetID::AgentDavenport},
	{"daf89737-dd98-4850-a218-2435e99a378c", eTargetID::AgentSwan},
	{"eb024a5e-9580-49dc-a519-bb92c886f3b1", eTargetID::AgentRhodes},
	{"f83376a4-6e56-4f2a-8122-151b272108fd", eTargetID::AgentPrice},
};

std::vector<Keyword> Keyword::keywords = {
	// Kill Types
	{ "Ld", eKillType::Loud },
	{ "Sil", eKillType::Silenced },
	{ "Loud", eKillType::Loud, "Ld"},
	{ "Melee", eKillType::Melee },
	{ "Mel", eKillType::Melee, "Melee"},
	{ "Remote", eKillType::Remote },
	{ "Rem", eKillType::Remote, "Remote"},
	{ "Impact", eKillType::Impact},
	{ "Imp", eKillType::Impact, "Impact"},
	{ "LoudRemote", eKillType::LoudRemote},
	{ "Silenced", eKillType::Silenced, "Sil" },
	{ "Silence", eKillType::Silenced, "Sil" },
	{ "Silent", eKillType::Silenced, "Sil" },
	{ "Thrown", eKillType::Thrown },
	{ "Throw", eKillType::Thrown, "Thrown"},
	// Kill Complications
	{ "Live", eKillComplication::Live },
	{ "nko", eKillComplication::Live, "Live" },
	{ "noko", eKillComplication::Live, "Live" },
	{ "nonko", eKillComplication::Live, "Live" },
	{ "ntko", eKillComplication::Live, "Live" },
	{ "notargetko", eKillComplication::Live, "Live" },
	{ "notargetpacification", eKillComplication::Live, "Live" },
	{ "nopacification", eKillComplication::Live, "Live" },
	{ "nopacify", eKillComplication::Live, "Live" },
	{ "notargetknockout", eKillComplication::Live, "Live" },
	{ "noknockout", eKillComplication::Live, "Live" },
	{ "dontknockout", eKillComplication::Live, "Live" },
	{ "notargetpacify", eKillComplication::Live, "Live" },
	{ "donotko", eKillComplication::Live, "Live" },
	{ "dontko", eKillComplication::Live, "Live" },
	{ "donotpacify", eKillComplication::Live, "Live" },
	{ "dontpacify", eKillComplication::Live, "Live" },
	{ "nonpacify", eKillComplication::Live, "Live" },
	{ "nonpacification", eKillComplication::Live, "Live" },
	// Kill Method
	{ "Pistol", eKillMethod::Pistol },
	{ "SMG", eKillMethod::SMG },
	{ "Sniper", eKillMethod::Sniper },
	{ "Snipe", eKillMethod::Sniper, "Sniper" },
	{ "SniperRifle", eKillMethod::Sniper, "Sniper" },
	{ "Shotgun", eKillMethod::Shotgun },
	{ "AR", eKillMethod::AssaultRifle },
	{ "AssaultRifle", eKillMethod::AssaultRifle, "AR" },
	{ "Elim", eKillMethod::Elimination },
	{ "Elimination", eKillMethod::Elimination, "Elim"},
	{ "PistolElim", eKillMethod::PistolElimination },
	{ "PistolElimination", eKillMethod::PistolElimination, "PistolElim" },
	{ "SMGElim", eKillMethod::SMGElimination },
	{ "SMGElimination", eKillMethod::SMGElimination, "SMGElim" },
	{ "Explosive", eKillMethod::Explosive },
	{ "ExplosiveWeapon", eKillMethod::Explosive, "Explosive" },
	{ "Bomb", eKillMethod::Explosive, "Explosive" },
	{ "Drown", eKillMethod::Drowning },
	{ "Drowning", eKillMethod::Drowning, "Drown" },
	{ "FallingObject", eKillMethod::FallingObject },
	{ "FObj", eKillMethod::FallingObject, "FallingObject"},
	{ "FallingObj", eKillMethod::FallingObject, "FallingObject"},
	{ "Fall", eKillMethod::Fall },
	{ "Fire", eKillMethod::Fire },
	{ "Electro", eKillMethod::Electrocution },
	{ "Electrocution", eKillMethod::Electrocution, "Electro"},
	{ "Explosion", eKillMethod::Explosion },
	{ "AccidentExplosion", eKillMethod::Explosion, "Explosion"},
	{ "ExplosionAccident", eKillMethod::Explosion, "Explosion"},
	{ "AccExplosion", eKillMethod::Explosion, "Explosion"},
	{ "ExplosionAcc", eKillMethod::Explosion, "Explosion"},
	{ "Poison", eKillMethod::ConsumedPoison },
	{ "ConsumedPoison", eKillMethod::ConsumedPoison, "Poison"},
	{ "Consumed", eKillMethod::ConsumedPoison, "Poison"},
	{ "Injected", eKillMethod::InjectedPoison },
	{ "InjectedPoison", eKillMethod::InjectedPoison, "Injected"},
	{ "Inject", eKillMethod::InjectedPoison, "Injected"},
	{ "Injection", eKillMethod::InjectedPoison, "Injected"},
	{ "Syringe", eKillMethod::InjectedPoison, "Injected"},
	{ "FiberWire", eKillMethod::FiberWire },
	{ "FW", eKillMethod::FiberWire, "FiberWire"},
	{ "NeckSnap", eKillMethod::NeckSnap },
	{ "Unarmed", eKillMethod::NeckSnap, "NeckSnap" },
	// Map Kill Method
	{ "AmputationKnife", eMapKillMethod::AmputationKnife },
	{ "AmpKnife", eMapKillMethod::AmputationKnife, "AmputationKnife"},
	{ "Amputation", eMapKillMethod::AmputationKnife, "AmputationKnife"},
	{ "AntiqueCurvedKnife", eMapKillMethod::AntiqueCurvedKnife },
	{ "ACKnife", eMapKillMethod::AntiqueCurvedKnife, "AntiqueCurvedKnife"},
	{ "AntiqueKnife", eMapKillMethod::AntiqueCurvedKnife, "AntiqueCurvedKnife"},
	{ "BarberRazor", eMapKillMethod::BarberRazor },
	{ "Razor", eMapKillMethod::BarberRazor, "BarberRazor" },
	{ "BattleAxe", eMapKillMethod::BattleAxe },
	{ "BAxe", eMapKillMethod::BattleAxe, "BattleAxe" },
	{ "BatAxe", eMapKillMethod::BattleAxe, "BattleAxe" },
	{ "BeakStaff", eMapKillMethod::BeakStaff },
	{ "Broadsword", eMapKillMethod::Broadsword },
	{ "BurialKnife", eMapKillMethod::BurialKnife },
	{ "BurialDagger", eMapKillMethod::BurialKnife, "BurialKnife" },
	{ "CircumcisionKnife", eMapKillMethod::CircumcisionKnife },
	{ "CircKnife", eMapKillMethod::CircumcisionKnife, "CircumcisionKnife" },
	{ "CombatKnife", eMapKillMethod::CombatKnife },
	{ "ConcealableKnife", eMapKillMethod::ConcealableKnife },
	{ "Cleaver", eMapKillMethod::Cleaver },
	{ "FireAxe", eMapKillMethod::FireAxe },
	{ "FoldingKnife", eMapKillMethod::FoldingKnife },
	{ "GardenFork", eMapKillMethod::GardenFork },
	{ "GrapeKnife", eMapKillMethod::GrapeKnife },
	{ "Hatchet", eMapKillMethod::Hatchet },
	{ "HobbyKnife", eMapKillMethod::HobbyKnife },
	{ "Hook", eMapKillMethod::Hook },
	{ "Icicle", eMapKillMethod::Icicle },
	{ "JarlsPirateSaber", eMapKillMethod::JarlsPirateSaber },
	{ "PirateSaber", eMapKillMethod::JarlsPirateSaber, "JarlsPirateSaber"},
	{ "JarlsSaber", eMapKillMethod::JarlsPirateSaber, "JarlsPirateSaber"},
	{ "Katana", eMapKillMethod::Katana },
	{ "KitchenKnife", eMapKillMethod::KitchenKnife },
	{ "KKnife", eMapKillMethod::KitchenKnife, "KitchenKnife" },
	{ "KukriMachete", eMapKillMethod::KukriMachete },
	{ "Kukri", eMapKillMethod::KukriMachete, "KukriMachete" },
	{ "LetterOpener", eMapKillMethod::LetterOpener },
	{ "LetOpener", eMapKillMethod::LetterOpener, "LetterOpener" },
	{ "Machete", eMapKillMethod::Machete },
	{ "MeatFork", eMapKillMethod::MeatFork },
	{ "OldAxe", eMapKillMethod::OldAxe },
	{ "OrnateScimitar", eMapKillMethod::OrnateScimitar },
	{ "Scimitar", eMapKillMethod::OrnateScimitar, "OrnateScimitar" },
	{ "RustyScrewdriver", eMapKillMethod::RustyScrewdriver },
	{ "Saber", eMapKillMethod::Saber },
	{ "SacrificialKnife", eMapKillMethod::SacrificialKnife },
	{ "SacKnife", eMapKillMethod::SacrificialKnife, "SacrificialKnife" },
	{ "SappersAxe", eMapKillMethod::SappersAxe },
	{ "Sappers", eMapKillMethod::SappersAxe, "SappersAxe" },
	{ "SapAxe", eMapKillMethod::SappersAxe, "SappersAxe" },
	{ "Scalpel", eMapKillMethod::Scalpel },
	{ "Scissors", eMapKillMethod::Scissors },
	{ "ScrapSword", eMapKillMethod::ScrapSword },
	{ "Screwdriver", eMapKillMethod::Screwdriver },
	{ "Seashell", eMapKillMethod::Seashell },
	{ "Shears", eMapKillMethod::Shears },
	{ "Shuriken", eMapKillMethod::Shuriken },
	{ "Starfish", eMapKillMethod::Starfish },
	{ "Tanto", eMapKillMethod::Tanto },
	{ "UnicornHorn", eMapKillMethod::UnicornHorn },
	{ "VikingAxe", eMapKillMethod::VikingAxe },
	{ "HolidayFireAxe", eMapKillMethod::HolidayFireAxe },
	{ "XmasStar", eMapKillMethod::XmasStar },
	// Map Kill Method - Soders
	{ "PoisonStemCells", eMapKillMethod::Soders_PoisonStemCells, "Poison" },
	{ "StemCells", eMapKillMethod::Soders_PoisonStemCells, "Poison" },
	{ "RobotArms", eMapKillMethod::Soders_RobotArms },
	{ "ShootHeart", eMapKillMethod::Soders_ShootHeart },
	{ "TrashHeart", eMapKillMethod::Soders_TrashHeart },
	{ "ThrowHeart", eMapKillMethod::Soders_TrashHeart, "TrashHeart" },
	// Map Kill Method - Sierra Knox
	{ "ShootTheCar", eMapKillMethod::Sierra_ShootCar },
	{ "ShootCar", eMapKillMethod::Sierra_ShootCar, "ShootTheCar" },
	{ "BombTheCar", eMapKillMethod::Sierra_BombCar },
	{ "BombCar", eMapKillMethod::Sierra_BombCar, "BombTheCar" },
};

const std::vector<eKillMethod> RouletteSpinGenerator::standardKillMethods = {
	eKillMethod::ConsumedPoison,
	eKillMethod::Drowning,
	eKillMethod::Electrocution,
	eKillMethod::Explosion,
	eKillMethod::Explosive,
	eKillMethod::Fall,
	eKillMethod::FallingObject,
	eKillMethod::FiberWire,
	eKillMethod::Fire,
	eKillMethod::InjectedPoison,
	eKillMethod::NeckSnap,
};

const std::vector<eKillType> RouletteSpinGenerator::gunKillTypes {
	eKillType::Any,
	eKillType::Loud,
	eKillType::Silenced,
};

const std::vector<eKillType> RouletteSpinGenerator::ogExplosiveKillTypes {
	eKillType::Any,
	eKillType::Loud,
};

const std::vector<eKillType> RouletteSpinGenerator::explosiveKillTypes {
	eKillType::Any,
	eKillType::Loud,
	eKillType::Impact,
	eKillType::Remote,
	eKillType::LoudRemote,
};

const std::vector<eMapKillMethod> RouletteSpinGenerator::sodersKills {
	eMapKillMethod::Soders_Electrocution,
	eMapKillMethod::Soders_Explosion,
	eMapKillMethod::Soders_PoisonStemCells,
	eMapKillMethod::Soders_RobotArms,
	eMapKillMethod::Soders_ShootHeart,
	eMapKillMethod::Soders_TrashHeart,
};

KillMethod::KillMethod(eKillMethod method) : method(method),
	name(getKillMethodName(method)),
	image(getKillMethodImage(method)),
	isGun(isKillMethodGun(method)),
	isLarge(isKillMethodLarge(method)),
	isRemote(isKillMethodRemote(method)),
	isElimination(isKillMethodElimination(method))
{ }

MapKillMethod::MapKillMethod(eMapKillMethod method) : method(method),
	name(getSpecificKillMethodName(method)),
	image(getSpecificKillMethodImage(method)),
	isMelee(isSpecificKillMethodMelee(method))
{ }

auto isMethodTagHigherDifficulty(eMethodTag a, eMethodTag b) -> bool {
	switch (a) {
	case eMethodTag::BannedInRR:
		return false;
	case eMethodTag::Hard:
		return b == eMethodTag::BannedInRR;
	case eMethodTag::Extreme:
		return b == eMethodTag::BannedInRR || b == eMethodTag::Hard;
	case eMethodTag::Impossible:
		return b == eMethodTag::BannedInRR || b == eMethodTag::Hard || b == eMethodTag::Extreme;
	}
	return false;
}

auto isKillMethodGun(eKillMethod method) -> bool {
	switch (method) {
	case eKillMethod::Pistol:
	case eKillMethod::SMG:
	case eKillMethod::AssaultRifle:
	case eKillMethod::Shotgun:
	case eKillMethod::Sniper:
	case eKillMethod::Elimination:
	case eKillMethod::PistolElimination:
	case eKillMethod::SMGElimination:
		return true;
	}
	return false;
}

auto isKillMethodElimination(eKillMethod method) -> bool {
	switch (method) {
	case eKillMethod::Elimination:
	case eKillMethod::PistolElimination:
	case eKillMethod::SMGElimination:
		return true;
	}
	return false;
}

auto isKillMethodLarge(eKillMethod method) -> bool {
	switch (method) {
	case eKillMethod::AssaultRifle:
	case eKillMethod::Shotgun:
	case eKillMethod::Sniper:
		return true;
	}
	return false;
}

auto isKillMethodRemote(eKillMethod method) -> bool {
	switch (method) {
	case eKillMethod::ConsumedPoison:
	case eKillMethod::Electrocution:
	case eKillMethod::Explosive:
	case eKillMethod::Explosion:
	case eKillMethod::Fire:
		return true;
	}
	return false;
}

auto isSpecificKillMethodMelee(eMapKillMethod method) -> bool {
	switch (method) {
	case eMapKillMethod::Soders_Electrocution:
	case eMapKillMethod::Soders_Explosion:
	case eMapKillMethod::Soders_PoisonStemCells:
	case eMapKillMethod::Soders_RobotArms:
	case eMapKillMethod::Soders_ShootHeart:
	case eMapKillMethod::Soders_TrashHeart:
	case eMapKillMethod::Yuki_SabotageCableCar:
	case eMapKillMethod::Sierra_BombCar:
	case eMapKillMethod::Sierra_ShootCar:
		return false;
	}
	return true;
}

auto isKillMethodLivePrefixable(eKillMethod method) -> bool {
	switch (method) {
	case eKillMethod::NONE:
	// Exclude anything already live.
	case eKillMethod::ConsumedPoison:
	case eKillMethod::Drowning:
	case eKillMethod::Elimination:
	case eKillMethod::FiberWire:
	case eKillMethod::InjectedPoison:
	case eKillMethod::PistolElimination:
	case eKillMethod::SMGElimination:
		return false;
	}
	return true;
}

auto isSpecificKillMethodLivePrefixable(eMapKillMethod method) -> bool {
	return isSpecificKillMethodMelee(method);
}

auto getKillTypeName(eKillType type) -> std::string_view {
	switch (type) {
	case eKillType::Loud: return "Loud";
	case eKillType::LoudRemote: return "Loud Remote";
	case eKillType::Remote: return "Remote";
	case eKillType::Impact: return "Impact";
	case eKillType::Silenced: return "Silenced";
	case eKillType::Melee: return "Melee";
	case eKillType::Thrown: return "Thrown";
	}
	return "";
}

auto getKillComplicationName(eKillComplication complication) -> std::string_view {
	switch (complication) {
	case eKillComplication::Live: return "(Live)";
	}
	return "";
}

auto getKillMethodName(eKillMethod method) -> std::string_view {
	switch (method) {
	case eKillMethod::Pistol: return "Pistol";
	case eKillMethod::SMG: return "SMG";
	case eKillMethod::Shotgun: return "Shotgun";
	case eKillMethod::AssaultRifle: return "Assault Rifle";
	case eKillMethod::Sniper: return "Sniper";
	case eKillMethod::Elimination: return "Elimination";
	case eKillMethod::PistolElimination: return "Pistol Elimination";
	case eKillMethod::SMGElimination: return "SMG Elimination";
	case eKillMethod::Explosive: return "Explosive";
	case eKillMethod::NeckSnap: return "Neck Snap";
	case eKillMethod::InjectedPoison: return "Injected Poison";
	case eKillMethod::FiberWire: return "Fiber Wire";
	case eKillMethod::Fall: return "Fall";
	case eKillMethod::FallingObject: return "Falling Object";
	case eKillMethod::Drowning: return "Drowning";
	case eKillMethod::Fire: return "Fire";
	case eKillMethod::Electrocution: return "Electrocution";
	case eKillMethod::Explosion: return "Explosion (Accident)";
	case eKillMethod::ConsumedPoison: return "Consumed Poison";
	}
	return "";
}

auto getKillMethodImage(eKillMethod method) -> std::string_view {
	switch (method) {
	case eKillMethod::Pistol: return "condition_killmethod_pistol.jpg";
	case eKillMethod::SMG: return "condition_killmethod_smg.jpg";
	case eKillMethod::Shotgun: return "condition_killmethod_shotgun.jpg";
	case eKillMethod::AssaultRifle: return "condition_killmethod_assaultrifle.jpg";
	case eKillMethod::Sniper: return "condition_killmethod_sniperrifle.jpg";
	case eKillMethod::Elimination: return "condition_killmethod_close_combat_pistol_elimination.jpg";
	case eKillMethod::PistolElimination: return "condition_killmethod_close_combat_pistol_elimination.jpg";
	case eKillMethod::SMGElimination: return "condition_killmethod_close_combat_smg_elimination.jpg";
	case eKillMethod::Explosive: return "condition_killmethod_explosive.jpg";
	case eKillMethod::NeckSnap: return "condition_killmethod_unarmed.jpg";
	case eKillMethod::InjectedPoison: return "condition_killmethod_injected_poison.jpg";
	case eKillMethod::FiberWire: return "condition_killmethod_fiberwire.jpg";
	case eKillMethod::Fall: return "condition_killmethod_accident_push.jpg";
	case eKillMethod::FallingObject: return "condition_killmethod_accident_suspended_object.jpg";
	case eKillMethod::Drowning: return "condition_killmethod_accident_drown.jpg";
	case eKillMethod::Fire: return "condition_killmethod_accident_burn.jpg";
	case eKillMethod::Electrocution: return "condition_killmethod_accident_electric.jpg";
	case eKillMethod::Explosion: return "condition_killmethod_accident_explosion.jpg";
	case eKillMethod::ConsumedPoison: return "condition_killmethod_consumed_poison.jpg";
	}
	return "";
}

auto getSpecificKillMethodNameAndImage(eMapKillMethod method) -> std::pair<std::string_view, std::string_view> {
	switch (method) {
	case eMapKillMethod::AmputationKnife: return {"Amputation Knife", "item_perspective_62c2ac2e-329e-4648-822a-e45a29a93cd0_0.jpg"};
	case eMapKillMethod::AntiqueCurvedKnife: return {"Antique Curved Knife", "item_perspective_5c211971-235a-4856-9eea-fe890940f63a_0.jpg"};
	case eMapKillMethod::BarberRazor: return {"Barber Razor", "item_perspective_5ce2f842-e091-4ead-a51c-1cc406309c8d_0.jpg"};
	case eMapKillMethod::BattleAxe: return {"Battle Axe", "item_perspective_58dceb1c-d7db-41dc-9750-55e3ab87fdf0_0.jpg"};
	case eMapKillMethod::BeakStaff: return {"Beak Staff", "item_perspective_b153112f-9cd1-4a49-a9c6-ba1a34f443ab_0.jpg"};
	case eMapKillMethod::Broadsword: return {"Broadsword", "item_perspective_12200bd8-9605-4111-8b26-4e73cb07d816_0.jpg"};
	case eMapKillMethod::BurialKnife: return {"Burial Knife", "item_perspective_6d4c88f3-9a09-453c-9a6e-a081f1136bf3_0.jpg"};
	case eMapKillMethod::CircumcisionKnife: return {"Circumcision Knife", "item_perspective_e312a416-5b56-4cb5-8994-1d4bc82fbb84_0.jpg"};
	case eMapKillMethod::Cleaver: return {"Cleaver", "item_perspective_1bbf0ed5-0515-4599-a4c9-454ce59cff44_0.jpg"};
	case eMapKillMethod::CombatKnife: return {"Combat Knife", "item_perspective_2c037ef5-a01b-4532-8216-1d535193a837_0.jpg"};
	case eMapKillMethod::ConcealableKnife: return {"Concealable Knife", "item_perspective_e30a5b15-ce4d-41d5-a2a5-08dec9c4fe79_0.jpg"};
	case eMapKillMethod::FireAxe: return {"Fire Axe", "item_perspective_a8bc4325-718e-45ba-b0e4-000729c70ce4_0.jpg"};
	case eMapKillMethod::FoldingKnife: return {"Folding Knife", "item_perspective_a2c56798-026f-4d0b-9480-de0d2525a119_0.jpg"};
	case eMapKillMethod::GardenFork: return {"Garden Fork", "item_perspective_1a105af8-fd30-447f-8b2c-f908f702e81c_0.jpg"};
	case eMapKillMethod::GrapeKnife: return {"Grape Knife", "item_perspective_2b1bd2af-554e-4ea7-a717-3f6d0eb0215f_0.jpg"};
	case eMapKillMethod::Hatchet: return {"Hatchet", "item_perspective_3a8207bb-84f5-438f-8f30-5c83aef2af80_0.jpg"};
	case eMapKillMethod::HobbyKnife: return {"Hobby Knife", "item_perspective_9e728dc1-3344-4615-be7a-1bcbdd7ad4aa_0.jpg"};
	case eMapKillMethod::Hook: return {"Hook", "item_perspective_58a036dc-79d4-4d64-8bf5-3faafa3cfead_0.jpg"};
	case eMapKillMethod::Icicle: return {"Icicle", "item_perspective_d689f87e-c3b1-4018-8e78-2f0025cde2a9_0.jpg"};
	case eMapKillMethod::JarlsPirateSaber: return {"Jarl's Pirate Saber", "item_perspective_fba6e133-78d1-4af1-8450-1ff30466c553_0.jpg"};
	case eMapKillMethod::Katana: return {"Katana", "item_perspective_5631dace-7f4a-4df8-8e97-b47373b815ff_0.jpg"};
	case eMapKillMethod::KitchenKnife: return {"Kitchen Knife", "item_perspective_e17172cc-bf70-4df6-9828-d9856b1a24fd_0.jpg"};
	case eMapKillMethod::KukriMachete: return {"Kukri Machete", "item_perspective_5db9cefd-391e-4c35-a4c4-bb672ac9b996_0.jpg"};
	case eMapKillMethod::LetterOpener: return {"Letter Opener", "item_perspective_f1f89faf-a441-4492-b442-9a923b5ecfd1_0.jpg"};
	case eMapKillMethod::Machete: return {"Machete", "item_perspective_3e3819ca-4d19-4e0a-a238-4bd16c730e61_0.jpg"};
	case eMapKillMethod::MeatFork: return {"Meat Fork", "item_perspective_66024572-7838-42d3-8c7b-c651e259438e_0.jpg"};
	case eMapKillMethod::OldAxe: return {"Old Axe", "item_perspective_369c68f7-cbef-4e45-83c7-8acd0dc2d237_0.jpg"};
	case eMapKillMethod::OrnateScimitar: return {"Ornate Scimitar", "item_perspective_b4d4ed1a-0687-48a9-a731-0e3b99494eb6_0.jpg"};
	case eMapKillMethod::RustyScrewdriver: return {"Rusty Screwdriver", "item_perspective_ecf022db-ecfd-48c0-97b5-2258e4e89a65_0.jpg"};
	case eMapKillMethod::Saber: return {"Saber", "item_perspective_94f52181-b9ec-4363-baef-d53b4e424b74_0.jpg"};
	case eMapKillMethod::SacrificialKnife: return {"Sacrificial Knife", "item_perspective_b2321154-4520-4911-9d94-9256b85e0983_0.jpg"};
	case eMapKillMethod::SappersAxe: return {"Sapper's Axe", "item_perspective_d2a7fa04-2cac-45d8-b696-47c566bb95ff_0.jpg"};
	case eMapKillMethod::Seashell: return {"Seashell", "item_perspective_7bc45270-83fe-4cf6-ad10-7d1b0cf3a3fd_0.jpg"};
	case eMapKillMethod::Scalpel: return {"Scalpel", "item_perspective_5d8ca32a-fe4c-4597-b074-51e36c3de898_0.jpg"};
	case eMapKillMethod::Scissors: return {"Scissors", "item_perspective_6ecf1f15-453c-4783-9c70-8777c83934d7_0.jpg"};
	case eMapKillMethod::ScrapSword: return {"Scrap Sword", "item_perspective_d73251b4-4860-4b5b-8376-7c9cf2a054a2_0.jpg"};
	case eMapKillMethod::Screwdriver: return {"Screwdriver", "item_perspective_12cb6b51-a6dd-4bf5-9653-0ab727820cac_0.jpg"};
	case eMapKillMethod::Shears: return {"Shears", "item_perspective_42c7bb52-a71b-489c-8a74-7db0c09ba313_0.jpg"};
	case eMapKillMethod::Shuriken: return {"Shuriken", "item_perspective_e55eb9a4-e79c-43c7-970b-79e94e7683b7_0.jpg"};
	case eMapKillMethod::Starfish: return {"Starfish", "item_perspective_cad726d7-331d-4601-9723-6b8a17e5f91b_0.jpg"};
	case eMapKillMethod::Tanto: return {"Tanto", "item_perspective_9488fa1e-10e1-49c9-bb24-6635d2e5bd49_0.jpg"};
	case eMapKillMethod::UnicornHorn: return {"Unicorn Horn", "item_perspective_58769c58-3e70-4746-be8e-4c7114f8c2bb_0.jpg"};
	case eMapKillMethod::VikingAxe: return {"Viking Axe", "item_perspective_9a7711c7-ede9-4230-853e-ab94c65fc0c9_0.jpg"};
	case eMapKillMethod::HolidayFireAxe: return {"Holiday Fire Axe", "item_perspective_2add9602-cda7-43fd-9758-6269c8fbb233_0.jpg"};
	case eMapKillMethod::XmasStar: return { "Xmas Star", "item_perspective_1a852006-e632-401f-aedc-d0cf76521b1f_0.jpg" };
	case eMapKillMethod::Soders_Electrocution: return {"Electrocution", "snowcrane_sign_soders_electrocute.jpg"};
	case eMapKillMethod::Soders_Explosion: return {"Explosion", "condition_killmethod_accident_explosion.jpg"};
	case eMapKillMethod::Soders_PoisonStemCells: return {"Poison Stem Cells", "snowcrane_soders_poison.jpg"};
	case eMapKillMethod::Soders_RobotArms: return {"Robot Arms", "snowcrane_soders_spidermachine.jpg"};
	case eMapKillMethod::Soders_ShootHeart: return {"Shoot Heart", "snowcrane_sign_soders_heart.jpg"};
	case eMapKillMethod::Soders_TrashHeart: return {"Trash Heart", "snowcrane_throw_away_heart.jpg"};
	case eMapKillMethod::Sierra_BombCar: return {"Bomb the Car", "story_pitstop_flamingo.jpg"};
	case eMapKillMethod::Sierra_ShootCar: return {"Shoot the Car", "helping_hand.jpg"};
	}
	return {"", ""};
}

auto getSpecificKillMethodName(eMapKillMethod method) -> std::string_view {
	return getSpecificKillMethodNameAndImage(method).first;
}

auto getSpecificKillMethodImage(eMapKillMethod method) -> std::string_view {
	return getSpecificKillMethodNameAndImage(method).second;
}

auto getMissionForTarget(const std::string& name) -> eMission {
	auto it = targetMissionMap.find(name);
	if (it != end(targetMissionMap)) return it->second;
	return eMission::NONE;
}
