using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Croupier {
	public class Location(string id, string name, MissionID mission) {
		public string ID { get; set; } = id;
		public string Name { get; set; } = name;
		public MissionID Mission { get; set; } = mission;
	}

	public class Entrance(string id, string name, string disguise = "", List<MissionID> missions = null) {
		public string ID { get; set; } = id;
		public string Name { get; set; } = name;
		public string Disguise { get; set; } = disguise;
		public List<MissionID> Missions { get; set; } = missions ?? [];
	}

	public class Locations {
		public static readonly List<Entrance> Entrances = [
			new("3bdb62b5-15ef-4eb6-94b1-7f2e0cdf1d31", "Attic", "", [MissionID.PARIS_SHOWSTOPPER]),
			new("943b8f1f-10c4-4dbf-a67e-25ed343290f5", "IAGO Auction", "", [MissionID.PARIS_SHOWSTOPPER]),
			new("461d35f5-55f5-4b07-884c-1fc1af3d1dc0", "Palace Garden", "", [MissionID.PARIS_SHOWSTOPPER]),
			new("67aec874-287f-4c82-9e01-3dbd7683faac", "Pile-Driver Barge", "", [MissionID.PARIS_SHOWSTOPPER]),
			new("74062d4a-3386-44b8-83e0-1d0348e5976f", "Palace Garden", "", [MissionID.PARIS_SHOWSTOPPER]),
			new("be610298-cbe8-47ce-9e90-951d2aae6f39", "AV Center", "Tech Crew", [MissionID.PARIS_SHOWSTOPPER]),
			new("04834714-406b-444e-b0d8-2f1054c1f8b5", "Dressing Area", "Stylist", [MissionID.PARIS_SHOWSTOPPER]),
			new("aa4cdd0b-8fcf-4160-a049-c15a19c82af1", "Kitchen", "Palace Staff", [MissionID.PARIS_SHOWSTOPPER]),
			new("c6765d3e-4031-4778-a5c5-a6d875131ea4", "Locker Room", "Chef", [MissionID.PARIS_SHOWSTOPPER]),

			new("a73d92c4-cd4b-4bd2-9485-943aa613a79c", "Main Square", "", [MissionID.SAPIENZA_WORLDOFTOMORROW]),
			new("4970aae5-e4c1-4d5c-a66a-0fc06616211b", "Main Square Tower", "", [MissionID.SAPIENZA_WORLDOFTOMORROW]),
			new("c113315d-ffcc-4878-9e70-bb35aadf0211", "ICA Safe House", "", [MissionID.SAPIENZA_WORLDOFTOMORROW]),
			new("1c386bb9-aead-42c0-b604-ad560056ddf4", "Church Morgue", "", [MissionID.SAPIENZA_WORLDOFTOMORROW]),
			new("a72bd197-f996-4e3b-b494-11f62296a2b3", "Sapienza Ruins", "", [MissionID.SAPIENZA_WORLDOFTOMORROW]),
			new("249659f3-8989-4ffc-b727-4902e954605f", "Mansion Garden", "Gardener", [MissionID.SAPIENZA_WORLDOFTOMORROW]),
			new("1021798e-cec1-4b43-ba33-69b7d53da867", "Mansion Kitchen", "Kitchen Assistant", [MissionID.SAPIENZA_WORLDOFTOMORROW]),
			new("495222ec-e8c9-416a-a41f-4bdfc3e6b80e", "Field Lab", "Lab Technician", [MissionID.SAPIENZA_WORLDOFTOMORROW]),
			new("8fd339e1-ea06-4aee-bd65-0c89b34e4e7e", "Security Staff", "Mansion Security", [MissionID.SAPIENZA_WORLDOFTOMORROW]),
			new("237e7963-574a-48c8-8d8a-e415b30f5643", "Portico", "", [MissionID.SAPIENZA_WORLDOFTOMORROW]),
			new("a28dd5e3-1f1d-408d-9387-945641c32218", "Portico", "", [MissionID.SAPIENZA_WORLDOFTOMORROW]),
			new("c200b5c6-2c14-4602-b22d-3a67b9fb3e3b", "Via Valle del Sole 9", "", [MissionID.SAPIENZA_WORLDOFTOMORROW]),
			new("c27f76ca-ed71-4b2d-9b9d-693875df830c", "Harbor", "", [MissionID.SAPIENZA_WORLDOFTOMORROW]),
			new("da717656-934e-4783-8c36-65bf486cfdfa", "City Gates", "", [MissionID.SAPIENZA_WORLDOFTOMORROW]),

			new("6d84301a-5780-41db-8c6d-479b471c03bb", "Bazaar Entrance", "", [MissionID.MARRAKESH_GILDEDCAGE]),
			new("035838e6-a288-4d1a-a1ae-fba484a7c545", "Consulate Parking Garage", "", [MissionID.MARRAKESH_GILDEDCAGE]),
			new("80b8188c-1783-413a-aff6-516c0ab27ffc", "Lamp Store Rooftop", "", [MissionID.MARRAKESH_GILDEDCAGE]),
			new("f8aeef13-b522-4351-bc80-810159e629af", "School Alley", "", [MissionID.MARRAKESH_GILDEDCAGE]),
			new("70f9c93c-57fa-4d61-9cb8-6fcaa0651b0e", "Consulate", "Consulate Janitor", [MissionID.MARRAKESH_GILDEDCAGE]),
			new("52598840-4d4d-46de-9446-5f9c64760105", "Courtyard Club", "Waiter", [MissionID.MARRAKESH_GILDEDCAGE]),
			new("565614ab-213e-4277-8429-950267941d5e", "Snail Stand", "Food Vendor", [MissionID.MARRAKESH_GILDEDCAGE]),
			new("de8c6844-9d28-4cfc-a165-baa3373ae811", "West Bazaar Rooftop", "Handyman", [MissionID.MARRAKESH_GILDEDCAGE]),
			new("66ff5ba4-8ebc-4526-8e05-cdb8a055a4c8", "Zaydan's Compound", "Elite Soldier", [MissionID.MARRAKESH_GILDEDCAGE]),
			new("94755a4f-3b10-4c17-b8ac-ea973a8b8157", "Well", "", [MissionID.MARRAKESH_GILDEDCAGE]),
			new("a20e45b9-172c-49e6-84a2-04da840bf90f", "Consulate Plaza", "", [MissionID.MARRAKESH_GILDEDCAGE]),

			new("9ddbd515-2519-4c16-98aa-0f87af5d8ef5", "Riverside Landing", "", [MissionID.BANGKOK_CLUB27]),
			new("213d36c0-a6af-4533-90da-2c815f6ec927", "Hotel Front Terrace", "", [MissionID.BANGKOK_CLUB27]),
			new("57e5073f-b4e6-4717-a8db-cbc94cb51087", "47's Suite", "", [MissionID.BANGKOK_CLUB27]),
			new("3aa4388f-6f87-44a4-b3dc-ac015bf65264", "2nd Floor Hallway", "Recording Crew", [MissionID.BANGKOK_CLUB27]),
			new("c2302bad-8e77-4d72-88c2-36ac1ad8c1b4", "Linen Room", "Hotel Staff", [MissionID.BANGKOK_CLUB27]),
			new("3d59f7ff-11b5-4498-925d-d3a2ea148a4e", "Himmapan Bar", "Waiter", [MissionID.BANGKOK_CLUB27]),
			new("83f4f967-71bb-48a1-bc53-1e472a9a1b3e", "Security Hut", "Hotel Security", [MissionID.BANGKOK_CLUB27]),
			new("b33ae290-5ea1-4229-b41f-680a76d40320", "Side Garden", "Groundskeeper", [MissionID.BANGKOK_CLUB27]),
			new("61f8efe4-7e9e-4add-b69e-bf40213209c2", "Restaurant Kitchen", "Kitchen Staff", [MissionID.BANGKOK_CLUB27]),

			new("f8ff0b17-b9f5-41ce-b7b1-db8bf6c47f4a", "Southern Farm Perimeter", "", [MissionID.COLORADO_FREEDOMFIGHTERS]),
			new("1d3a5b4a-5573-4e2d-8a98-d1c881cbd13e", "West Bridge", "", [MissionID.COLORADO_FREEDOMFIGHTERS]),
			new("370b4106-96f2-4a04-9bc6-029b86b301aa", "Old Orchard", "", [MissionID.COLORADO_FREEDOMFIGHTERS]),
			new("53ce9bb6-ef1c-4f17-8951-5b501fb941d4", "Water Tower", "", [MissionID.COLORADO_FREEDOMFIGHTERS]),
			new("58921a5a-9b8e-445d-842b-7c7cf7f124c3", "Garage", "Militia Technician", [MissionID.COLORADO_FREEDOMFIGHTERS]),
			new("6c943798-8ca7-42cc-861a-becdb32017da", "Garage", "Militia Technician", [MissionID.COLORADO_FREEDOMFIGHTERS]),
			new("bd0cc571-e4a3-41f1-9b9b-608cdecc09c1", "Greenhouse", "Militia Soldier", [MissionID.COLORADO_FREEDOMFIGHTERS]),
			new("e5c0d1a4-72b0-4cc0-857d-8bb155ea09f4", "Demolition Range", "Explosives Specialist", [MissionID.COLORADO_FREEDOMFIGHTERS]),
			new("0cd2b917-eb26-4f19-90c2-a5e16eb18cef", "Farm House", "Hacker", [MissionID.COLORADO_FREEDOMFIGHTERS]),

			new("a786fc74-b379-41f4-a4ac-ce970ee88b2c", "Tobias Rieper's Suite", "", [MissionID.HOKKAIDO_SITUSINVERSUS]),
			new("85ef518f-0e94-4e5e-9fd2-66fb84d2d0bb", "Spa", "", [MissionID.HOKKAIDO_SITUSINVERSUS]),
			new("bdc80464-31dd-440d-ad79-2767b923a0a4", "Morgue", "", [MissionID.HOKKAIDO_SITUSINVERSUS]),
			new("aca2b971-d13e-4291-9a4d-c48605ad5566", "Restaurant", "", [MissionID.HOKKAIDO_SITUSINVERSUS]),
			new("be6045fd-26ab-4a3b-a6ce-b1ccdf405d09", "Restaurant", "", [MissionID.HOKKAIDO_SITUSINVERSUS]),
			new("0e7dd303-c9bb-42cc-aca0-70499931d098", "Mountain Path", "Ninja", [MissionID.HOKKAIDO_SITUSINVERSUS]),
			new("2fc483c8-500c-4475-ba5d-e2cdd6ccc64c", "Operating Theater", "Surgeon", [MissionID.HOKKAIDO_SITUSINVERSUS]),
			new("74b1ad25-06cd-41a2-9cf5-9dd5dac7345d", "Kitchen", "Chef", [MissionID.HOKKAIDO_SITUSINVERSUS]),
			new("fedf6cd3-d076-4037-b7d8-1449726b4c0a", "Staff Quarters", "Resort Staff", [MissionID.HOKKAIDO_SITUSINVERSUS]),
			new("df752dfa-623d-4750-83a6-8b4aba1d8e08", "Garden", "Handyman", [MissionID.HOKKAIDO_SITUSINVERSUS]),
			new("82461a54-f864-4041-972d-33ea82d444f7", "Bar", "Cowboy Suit", [MissionID.HOKKAIDO_SITUSINVERSUS]),
			new("faf13436-43ec-4d49-9d6f-decf29011642", "Helipad", "", [MissionID.HOKKAIDO_PATIENTZERO]),

			new("0f13f00b-abb4-4cdd-bd9d-748514b941d5", "Boat", "", [MissionID.HAWKESBAY_NIGHTCALL]),
			new("3764f26c-fe75-4ea1-b28a-3363d350a915", "Hut", "", [MissionID.HAWKESBAY_NIGHTCALL]),
			new("d08b7d6d-9b1f-4131-bb44-4ebd2cdb1cec", "Hut", "", [MissionID.HAWKESBAY_NIGHTCALL]),
			new("40e5cc64-9aba-4f1a-9d37-a2e558e118b6", "Office", "", [MissionID.HAWKESBAY_NIGHTCALL]),
			new("78477425-7330-4092-ad71-6548e07ccc93", "Beach", "", [MissionID.HAWKESBAY_NIGHTCALL]),
			new("0f79ca01-ef05-47d4-9efc-936da5b6cafb", "Bed", "The Jack-O-Lantern Suit", [MissionID.HAWKESBAY_NIGHTCALL]),

			new("27cc83fa-5fb3-4763-85a4-c584efc90e5e", "Event Entrance", "", [MissionID.MIAMI_FINISHLINE]),
			new("f1de2373-eebe-4448-808a-8c2764b2c931", "Dolphin Fountain", "", [MissionID.MIAMI_FINISHLINE]),
			new("360a61e0-1471-4c14-84ca-f4729c1198d4", "Dolphin Fountain", "", [MissionID.MIAMI_FINISHLINE]),
			new("3e84bc0c-c1a9-4bec-90bf-cfbdb9818ec7", "Marina", "", [MissionID.MIAMI_FINISHLINE]),
			new("fc234ee7-51a5-4915-a6a9-23b1bcd7ef57", "Overpass", "Race Marshal", [MissionID.MIAMI_FINISHLINE]),
			new("5004306e-0ee5-458d-87cc-2d71922fff3d", "Food Stand", "Food Vendor", [MissionID.MIAMI_FINISHLINE]),
			new("35a2bcb1-ecfc-4a6b-95f6-68d275408055", "Stands", "Mascot", [MissionID.MIAMI_FINISHLINE]),
			new("d8952ad4-57ae-42e1-8fec-dd87ade02328", "Drivers' Lounge", "Waiter", [MissionID.MIAMI_FINISHLINE]),
			new("7af4fe74-90a3-48f1-abb1-e6688554cb25", "Podium", "Event Crew", [MissionID.MIAMI_FINISHLINE]),
			new("35010bb4-b766-4958-9922-92183e90445d", "Medical Area", "Medic", [MissionID.MIAMI_FINISHLINE]),
			new("b42bb297-a449-4caf-8fbf-b6b7088c82dc", "Kronstadt Bayside Center", "Kronstadt Engineer", [MissionID.MIAMI_FINISHLINE]),
			new("9b806cc8-f363-442b-8747-29690bc2e27b", "Kowoon Pit", "Kowoon Mechanic", [MissionID.MIAMI_FINISHLINE]),

			new("a96c518b-07d7-4b31-bef4-147c9e0ec93a", "Village Bus Stop", "", [MissionID.SANTAFORTUNA_THREEHEADEDSERPENT]),
			new("0c848791-ae6e-4442-8bd9-9717e95d2a56", "Village Hostel", "", [MissionID.SANTAFORTUNA_THREEHEADEDSERPENT]),
			new("37d5b96b-2c27-4d1b-a7e4-2fa290e6d2d6", "Village Bar", "", [MissionID.SANTAFORTUNA_THREEHEADEDSERPENT]),
			new("efae3d00-e6cb-421e-936d-589afd6be8ec", "Shaman's Hut", "", [MissionID.SANTAFORTUNA_THREEHEADEDSERPENT]),
			new("ac337d4f-4094-4b27-b26b-cea8dde12480", "Shaman's Hut", "", [MissionID.SANTAFORTUNA_THREEHEADEDSERPENT]),
			new("6efe7a67-4be5-4c9f-b4c5-b432c1acad70", "Shaman's Hut", "", [MissionID.SANTAFORTUNA_THREEHEADEDSERPENT]),
			new("35007eed-f88b-4b43-a2a3-89c714fea3b5", "Shaman's Hut", "", [MissionID.SANTAFORTUNA_THREEHEADEDSERPENT]),
			new("1550a752-bec3-44eb-ae57-2480d6081e3c", "Shaman's Hut", "", [MissionID.SANTAFORTUNA_THREEHEADEDSERPENT]),
			new("2c2a5173-db6c-40d8-9520-67b895aac2cc", "Construction Site", "Construction Worker", [MissionID.SANTAFORTUNA_THREEHEADEDSERPENT]),
			new("d7013244-6f19-4895-bad8-3d75ceedd6fa", "Coca Fields", "Coca Field Worker", [MissionID.SANTAFORTUNA_THREEHEADEDSERPENT]),
			new("494b9470-402f-4902-8770-46a470097ad7", "Coca Fields", "Coca Field Worker", [MissionID.SANTAFORTUNA_THREEHEADEDSERPENT]),
			new("4b344092-e910-4c2f-80c8-9f2501e411bb", "Submarine Cave", "Submarine Crew", [MissionID.SANTAFORTUNA_THREEHEADEDSERPENT]),
			new("18bcd289-eada-499d-821b-64d232da4cda", "Mansion Basement", "", [MissionID.SANTAFORTUNA_THREEHEADEDSERPENT]),
			new("783a0cba-fdc5-44e3-85a3-e16e14d3cd04", "Steel Bridge", "", [MissionID.SANTAFORTUNA_EMBRACEOFTHESERPENT]),
			new("bd73b983-9f48-4d2d-9340-5368603edb99", "Hostage Room", "", [MissionID.SANTAFORTUNA_THREEHEADEDSERPENT]),

			new("a95703cb-091b-4d05-a2ee-3a70ade13590", "Main street", "", [MissionID.MUMBAI_CHASINGAGHOST]),
			new("f4659b56-65c3-4f4f-8f20-11d09af2f2d2", "Train", "", [MissionID.MUMBAI_CHASINGAGHOST]),
			new("51145d77-882e-4be0-a471-73704497aa8b", "Boat", "", [MissionID.MUMBAI_CHASINGAGHOST]),
			new("774b7dec-ce57-4318-9dbd-f6d7fa39ef1d", "Skywalk", "", [MissionID.MUMBAI_CHASINGAGHOST]),
			new("5e6a6e2c-ee2c-40a7-9de5-d7fb8fbd8dcd", "Chawls", "", [MissionID.MUMBAI_CHASINGAGHOST]),
			new("37af536b-3f36-49ed-986a-9d55f4481ed4", "Taxi", "", [MissionID.MUMBAI_CHASINGAGHOST]),
			new("8b1c3ba0-9c91-4b3e-96f9-7d3c8632b7c2", "Laundry", "Laundry Worker", [MissionID.MUMBAI_CHASINGAGHOST]),
			new("46b5b571-cb26-46d8-a6cb-b14b4967ce5d", "Laundry", "Laundry Worker", [MissionID.MUMBAI_CHASINGAGHOST]),
			new("86a3857f-dad7-45ac-818b-d933e5c8dfd9", "Barge", "Local Security", [MissionID.MUMBAI_CHASINGAGHOST]),
			new("f1bb66b5-b446-4a00-a951-af7310e28566", "Slums", "Food Vendor", [MissionID.MUMBAI_CHASINGAGHOST]),
			new("724b24ef-e87f-4060-b7ed-5c315fb0f29f", "Metal Forge", "Metal Worker", [MissionID.MUMBAI_CHASINGAGHOST]),
			new("cc3ebf7d-0fcf-40bc-9b55-1e2426e66edd", "Photoshoot", "Dancer", [MissionID.MUMBAI_CHASINGAGHOST]),
			new("0e33bbeb-2c77-419a-be0c-ce9c3627d543", "Hill", "Thug", [MissionID.MUMBAI_CHASINGAGHOST]),
			new("c7596e36-43ae-4e03-aa66-84f6a88f0cd3", "Train yard", "Vanya's Servant", [MissionID.MUMBAI_CHASINGAGHOST]),
			new("37dda555-cdf2-4444-8de2-4659d371be4c", "Outside Chawl", "", [MissionID.MUMBAI_ILLUSIONSOFGRANDEUR]),

			new("9034bf0a-6efa-4dec-bb99-5b342804af86", "Whittleton Creek", "", [MissionID.WHITTLETON_ANOTHERLIFE]),
			new("e0dcccd6-7e08-48ba-a1f0-aec0f83fedb5", "Construction Area", "Construction Worker", [MissionID.WHITTLETON_ANOTHERLIFE]),
			new("5877bd62-5609-4211-9fc5-ee7b5ce5984a", "Fumigation", "Exterminator", [MissionID.WHITTLETON_ANOTHERLIFE]),
			new("a03e5494-9861-475f-a116-2fa937dcb34c", "Garbage Removal", "Garbage Man", [MissionID.WHITTLETON_ANOTHERLIFE]),
			new("e8d12fac-221f-4b1d-9cad-fec2bd161c04", "Garbage Removal", "Garbage Man", [MissionID.WHITTLETON_ANOTHERLIFE]),
			new("3bb173da-7c8d-48fd-b2de-d602ff739ec5", "Garbage Removal", "Garbage Man", [MissionID.WHITTLETON_ANOTHERLIFE]),
			new("896cf183-d974-4d45-a819-2f04466d95e2", "Suburb Sign", "Gardener", [MissionID.WHITTLETON_ANOTHERLIFE]),
			new("03adcb04-2c83-4024-b472-5ae71ced41f9", "BBQ Party", "Server", [MissionID.WHITTLETON_ANOTHERLIFE]),
			new("0363dbda-6bf3-4b4d-aa27-499d008dbecc", "Helicopter Drop Off", "", [MissionID.WHITTLETON_ANOTHERLIFE]),

			new("42bfbe45-85c8-4555-b1fb-256b7006ee06", "Harbor", "", [MissionID.ISLEOFSGAIL_THEARKSOCIETY]),
			new("9c35b39c-53ca-4d5c-8ec4-90d1fc5a4bb0", "Chapel", "", [MissionID.ISLEOFSGAIL_THEARKSOCIETY]),
			new("5e30a7b9-7e06-464e-b2eb-1aa23635f500", "Keep", "", [MissionID.ISLEOFSGAIL_THEARKSOCIETY]),
			new("f6a11cec-cd25-4a4d-80cd-e9939cc081c1", "Reception Area", "Event Staff", [MissionID.ISLEOFSGAIL_THEARKSOCIETY]),
			new("95af0d95-bc0d-402e-acb3-ea918a791300", "Kitchens", "Chef", [MissionID.ISLEOFSGAIL_THEARKSOCIETY]),
			new("0df05e9f-78ef-4ce7-b627-b9d1ad55f607", "Warehouse", "Custodian", [MissionID.ISLEOFSGAIL_THEARKSOCIETY]),
			new("5272ff2c-98d1-4ae5-a406-d98aed51578b", "Gallery", "Ark Member", [MissionID.ISLEOFSGAIL_THEARKSOCIETY]),
			new("3e8a184c-8af6-453d-8a87-de59b51c58a3", "Architects' Lounge", "Architect", [MissionID.ISLEOFSGAIL_THEARKSOCIETY]),
			new("82a5698f-6cd6-4f68-a0a8-3bf1288eda48", "Loading Bay", "", [MissionID.ISLEOFSGAIL_THEARKSOCIETY]),
			new("1ba4f09b-ae87-4fee-aafb-09a09246e636", "Penthouse", "", [MissionID.ISLEOFSGAIL_THEARKSOCIETY]),
			new("aaa819f7-908e-4bc9-99e4-ebcbc29524a2", "Penthouse Balcony", "Elite Guard", [MissionID.ISLEOFSGAIL_THEARKSOCIETY]),
			new("c8cc6525-fc26-41af-99f2-fb5d2e7fae29", "Dungeon Cell", "Tuxedo and Mask", [MissionID.ISLEOFSGAIL_THEARKSOCIETY]),

			new("e0d0e391-a816-48c0-ad62-1fb944e108f4", "Bank Entrance", "", [MissionID.NEWYORK_GOLDENHANDSHAKE]),
			new("087cc56d-6317-4ce1-8f6a-bc2798ee4f3a", "Bank Entrance", "", [MissionID.NEWYORK_GOLDENHANDSHAKE]),
			new("735d751e-7c3d-4587-819b-d2d3c3a506f2", "First Floor Mezzanine", "Janitor", [MissionID.NEWYORK_GOLDENHANDSHAKE]),
			new("8d92827f-3231-4933-b9cd-130e0e9806f5", "Audit Hall", "Bank Teller", [MissionID.NEWYORK_GOLDENHANDSHAKE]),
			new("6dffa391-45c7-4254-8be5-96908d959023", "Deposit Box Room", "Security Guard", [MissionID.NEWYORK_GOLDENHANDSHAKE]),
			new("88937389-1e8f-46b7-9fa1-c39ca0aa4349", "Investment Floor", "Investment Banker", [MissionID.NEWYORK_GOLDENHANDSHAKE]),
			new("bc264812-2baf-46dc-91a6-4153b73336f6", "Garage", "Bank Robber", [MissionID.NEWYORK_GOLDENHANDSHAKE]),
			new("8df094e1-e4c9-4673-9341-f18594b31fa7", "Garage", "Bank Robber", [MissionID.NEWYORK_GOLDENHANDSHAKE]),

			new("6fbe6552-8e48-4971-824a-62cac516638e", "Resort Pier", "", [MissionID.HAVEN_THELASTRESORT]),
			new("7c0ea7df-ea2b-4b10-8398-5f2667dd7c40", "Resort Pool Area", "", [MissionID.HAVEN_THELASTRESORT]),
			new("59224315-c825-42ce-a121-ee0d09558431", "Shark Hut", "", [MissionID.HAVEN_THELASTRESORT]),
			new("272a52f4-e93e-4d73-8d39-612594bd1013", "Resort Gym", "Personal Trainer", [MissionID.HAVEN_THELASTRESORT]),
			new("0cf9b2b4-e440-40d8-a466-a2e374b9f6af", "Restaurant's Kitchen", "Waiter", [MissionID.HAVEN_THELASTRESORT]),
			new("544329a2-d961-461d-a254-a1712ecca3bb", "Private Villa Pier", "Villa Staff", [MissionID.HAVEN_THELASTRESORT]),

			new("29393ccc-6b84-4f5a-b857-cc8b5fc89860", "Atrium Lobby", "", [MissionID.DUBAI_ONTOPOFTHEWORLD]),
			new("34c8a195-8433-4813-aa3d-23a818a9f39a", "Atrium Lobby", "", [MissionID.DUBAI_ONTOPOFTHEWORLD]),
			new("92e26e2b-f293-42aa-8f07-877a65df7e89", "Atrium Lobby", "", [MissionID.DUBAI_ONTOPOFTHEWORLD]),
			new("a4e53696-94ca-4eeb-8e82-e235c212897f", "Atrium Lobby", "", [MissionID.DUBAI_ONTOPOFTHEWORLD]),
			new("d941abfd-3f70-49e3-a3da-bc36603ed9b4", "Atrium Lobby", "", [MissionID.DUBAI_ONTOPOFTHEWORLD]),
			new("f804c15b-b704-4f34-bf47-5d9da1fdfdfc", "Atrium Lobby", "", [MissionID.DUBAI_ONTOPOFTHEWORLD]),
			new("703b0196-a56e-41c3-86ce-a8aa77777472", "Atrium Lobby", "", [MissionID.DUBAI_ONTOPOFTHEWORLD]),
			new("43bed210-4415-40ee-8222-2cefaf4dda87", "Burj Al-Ghazali Exterior", "Skydiving Suit", [MissionID.DUBAI_ONTOPOFTHEWORLD]),
			new("bc531204-8e82-4550-b2a7-829b047dc6cc", "Burj Al-Ghazali Exterior", "Skydiving Suit", [MissionID.DUBAI_ONTOPOFTHEWORLD]),
			new("fae63964-9978-4cea-a927-198b4095036c", "Meeting Room", "Event Staff", [MissionID.DUBAI_ONTOPOFTHEWORLD]),
			new("fb1a7128-8a90-40fe-94d1-bff2e35ef0f5", "Art Installation", "Art Crew", [MissionID.DUBAI_ONTOPOFTHEWORLD]),
			new("9e981460-9fd0-4703-8369-baa503a5b366", "Guard Room", "Event Security", [MissionID.DUBAI_ONTOPOFTHEWORLD]),
			new("e690549c-d035-46fe-b175-2b73e1c048d3", "Guard Room", "Event Security", [MissionID.DUBAI_ONTOPOFTHEWORLD]),
			new("72a17d68-42b4-4ecd-8582-5d97bd93851b", "Penthouse", "Penthouse Staff", [MissionID.DUBAI_ONTOPOFTHEWORLD]),

			new("0aec817c-738e-4381-bbd4-a13b5d1c6730", "Main Road", "", [MissionID.DARTMOOR_DEATHINTHEFAMILY]),
			new("7e17b82c-56a6-4c6c-9666-b4ae2a8c0ac3", "Main Road", "", [MissionID.DARTMOOR_DEATHINTHEFAMILY]),
			new("b38a7610-5f7f-4fcb-ad09-c554c3544367", "Main Road", "", [MissionID.DARTMOOR_DEATHINTHEFAMILY]),
			new("ddc2d081-0a07-498f-babf-3016aabf5118", "Main Road", "", [MissionID.DARTMOOR_DEATHINTHEFAMILY]),
			new("9c111405-637e-46b7-a051-279039c533ab", "Main Road", "", [MissionID.DARTMOOR_DEATHINTHEFAMILY]),
			new("3812ea57-e29c-42cb-b93a-ea2d3e6a6806", "Behind Mansion", "", [MissionID.DARTMOOR_DEATHINTHEFAMILY]),
			new("5a60147f-9c85-4998-b70f-1441ee7ba9b1", "Behind Mansion", "", [MissionID.DARTMOOR_DEATHINTHEFAMILY]),
			new("8cb4bebf-6a89-46b4-a1c7-0e0ee040e16d", "Behind Mansion", "", [MissionID.DARTMOOR_DEATHINTHEFAMILY]),
			new("d4ec0c94-02b2-4aae-be9e-cbb5cc7d7e4a", "Garden", "Gardener", [MissionID.DARTMOOR_DEATHINTHEFAMILY]),
			new("5314c86b-7173-412b-a0b5-b0d9af2da722", "Staff Room", "Mansion Guard", [MissionID.DARTMOOR_DEATHINTHEFAMILY]),
			new("feaf524e-f54d-4815-bb2b-afb8f1a39141", "Library", "Mansion Staff", [MissionID.DARTMOOR_DEATHINTHEFAMILY]),
			new("0809cabd-20f1-4fd4-b4fc-9cdc86bdf7cf", "Zachary's Bedroom", "Private Investigator", [MissionID.DARTMOOR_DEATHINTHEFAMILY]),

			new("2f255e35-5d23-4b8a-9c5a-885a8c79e18a", "Bus Stop", "", [MissionID.BERLIN_APEXPREDATOR]),
			new("3ef16dfa-b97b-476f-85d0-bf7ac6ee4f6b", "Bus Stop", "", [MissionID.BERLIN_APEXPREDATOR]),
			new("7bb625a4-bbfe-485f-b970-0e19048831c6", "Bus Stop", "", [MissionID.BERLIN_APEXPREDATOR]),
			new("88f47eb3-d914-46d9-99aa-6fb848964c7c", "Bus Stop", "", [MissionID.BERLIN_APEXPREDATOR]),
			new("b1233f2e-83ce-4124-b139-9a7cbaf09e83", "Bus Stop", "", [MissionID.BERLIN_APEXPREDATOR]),
			new("b4c9da30-44f2-447f-b64f-a8fbaa59b35d", "Bus Stop", "", [MissionID.BERLIN_APEXPREDATOR]),
			new("000fc136-f098-43a2-b204-26c038a55490", "Club Entrance", "", [MissionID.BERLIN_APEXPREDATOR]),
			new("0523519e-608e-49fd-bdda-3edc75fd1f77", "Club Entrance", "", [MissionID.BERLIN_APEXPREDATOR]),
			new("22e52ffa-3c35-4cb4-94a7-2d493e14beb2", "Club Entrance", "", [MissionID.BERLIN_APEXPREDATOR]),
			new("bb0fbcfc-4a3b-4e00-a8f9-f399817bac83", "Club Entrance", "", [MissionID.BERLIN_APEXPREDATOR]),
			new("c704455f-f065-4665-b1b5-b66dfcfd4f00", "Club Entrance", "", [MissionID.BERLIN_APEXPREDATOR]),
			new("c82062a2-eb04-48f8-ac39-ddf85bff338b", "Club Entrance", "", [MissionID.BERLIN_APEXPREDATOR]),
			new("71de2f76-3f3e-43a0-b508-4992d32a344e", "Radio Tower", "", [MissionID.BERLIN_APEXPREDATOR]),
			new("694656e1-badf-4c20-9ff0-2a052c32aac8", "Projection Bar", "Bartender", [MissionID.BERLIN_APEXPREDATOR]),
			new("700603be-2925-4ec4-ae47-572068b801af", "Projection Bar", "Bartender", [MissionID.BERLIN_APEXPREDATOR]),
			new("b37a535e-ae99-4266-998e-609eee4c9626", "Projection Bar", "Bartender", [MissionID.BERLIN_APEXPREDATOR]),
			new("f223f1f2-f009-4aeb-a6d5-cf0a38dc7262", "Projection Bar", "Bartender", [MissionID.BERLIN_APEXPREDATOR]),
			new("1edf1fe3-687f-4588-a3b2-06586b948146", "Chill Out", "Club Crew", [MissionID.BERLIN_APEXPREDATOR]),
			new("1e9c74d3-f974-46a8-8358-8a1a06763774", "Chill Out", "Club Crew", [MissionID.BERLIN_APEXPREDATOR]),
			new("4e648bc5-9de1-4176-bd9d-ff70f9ad6a92", "DJ Booth", "DJ	", [MissionID.BERLIN_APEXPREDATOR]),
			new("e7ddcb47-1c3f-444c-8300-40baaf0c463c", "DJ Booth", "DJ	", [MissionID.BERLIN_APEXPREDATOR]),
			new("8ec50af5-e6b4-47d0-873b-0361cee52a05", "Biker Hangout", "Biker", [MissionID.BERLIN_APEXPREDATOR]),

			new("3f31c72e-fb07-4c3d-afa6-3c434abdc1b8", "Train Station", "", [MissionID.CHONGQING_ENDOFANERA]),
			new("f9c65d0b-e5ce-454e-8f20-08a8083a9b9d", "Train Station", "", [MissionID.CHONGQING_ENDOFANERA]),
			new("d497ce5a-011e-4b40-9d69-341cc8141120", "Train Station", "", [MissionID.CHONGQING_ENDOFANERA]),
			new("81370220-1fd2-4c59-9e82-d35f9b975c95", "River-side Walkway", "", [MissionID.CHONGQING_ENDOFANERA]),
			new("c235f578-f55b-4c9d-b568-db6cad909c8c", "River-side Walkway", "", [MissionID.CHONGQING_ENDOFANERA]),
			new("f3944deb-0747-4d67-b26a-99af5f979705", "Balcony", "", [MissionID.CHONGQING_ENDOFANERA]),
			new("4752ebbb-02c0-48b0-8e05-3ec2a920dedd", "Restaurant Kitchen", "Dumpling Cook", [MissionID.CHONGQING_ENDOFANERA]),
			new("7371c3ea-cfb9-46b8-a699-93ab1cdd07af", "Facility Rooftop", "Street Guard", [MissionID.CHONGQING_ENDOFANERA]),
			new("75827342-5601-4656-ac00-55aff44b48f4", "The Block", "Block Guard", [MissionID.CHONGQING_ENDOFANERA]),
			new("3fd57506-80d6-4a63-88ed-206b15686758", "Facility Locker Room", "Facility Analyst", [MissionID.CHONGQING_ENDOFANERA]),
			
			new("209589c8-e758-4632-a214-9205531032a3", "Winery Viewpoint", "", [MissionID.MENDOZA_THEFAREWELL]),
			new("b093bc4d-7f5e-47b9-bbbc-981ef745ec0f", "Winery Viewpoint", "", [MissionID.MENDOZA_THEFAREWELL]),
			new("f45b0af4-e50d-477e-bb92-83136b04506f", "Winery Viewpoint", "", [MissionID.MENDOZA_THEFAREWELL]),
			new("930f79a4-48f5-4521-ae7b-78104d11fe30", "Winery Viewpoint", "", [MissionID.MENDOZA_THEFAREWELL]),
			new("0bafac7b-a4fb-49c7-8ba5-0e2acd7153b5", "Parking Lot", "", [MissionID.MENDOZA_THEFAREWELL]),
			new("cef7b74a-bd2c-4ee9-94cd-4af5b4613556", "Parking Lot", "", [MissionID.MENDOZA_THEFAREWELL]),
			new("3d4de881-48fb-4bb8-8121-e4fb2dcd182d", "Parking Lot", "", [MissionID.MENDOZA_THEFAREWELL]),
			new("939ead3c-0602-4144-a8e6-740719d1950f", "Shrine", "", [MissionID.MENDOZA_THEFAREWELL]),
			new("cb100c2e-c741-4c27-988c-8dce814f4e9f", "Vineyard", "Winery Worker", [MissionID.MENDOZA_THEFAREWELL]),
			new("bfb5f09f-a8c9-4d4e-9787-f7169c09d428", "Tasting Room", "Waiter", [MissionID.MENDOZA_THEFAREWELL]),
			new("64ebf985-fcd6-45a7-8126-81a96ca950c6", "Sniper Spot", "Gaucho", [MissionID.MENDOZA_THEFAREWELL]),
			new("29b96e56-a29f-4a4d-85b3-24e5be9f35a1", "Dining Area", "Asado Chef", [MissionID.MENDOZA_THEFAREWELL]),
			
			new("0cb7c72a-305e-473e-8480-03e7f70ae340", "Reflection", "", [MissionID.CARPATHIAN_UNTOUCHABLE]),
			new("4fe8ecc9-7dae-4882-b596-bcccc53725e5", "Reflection", "", [MissionID.CARPATHIAN_UNTOUCHABLE]),
			new("756e9e78-e48e-4a0a-8661-356d07446a0e", "Reflection", "", [MissionID.CARPATHIAN_UNTOUCHABLE]),
			new("ee4b0060-b322-4bb0-99b5-942e8a20f278", "Reflection", "", [MissionID.CARPATHIAN_UNTOUCHABLE]),
			new("52464982-e7de-4f69-b60d-e8de2a8b1621", "Outdoors", "", [MissionID.CARPATHIAN_UNTOUCHABLE]),
			new("f21b1a1a-bec9-4a06-9a4a-d615e9721e3a", "Laboratory", "", [MissionID.CARPATHIAN_UNTOUCHABLE]),
			
			new("72f49b21-c240-4cf9-9b6b-8639f762f2d7", "Western Beach", "", [MissionID.AMBROSE_SHADOWSINTHEWATER]),
			new("cd368a51-7a36-4f98-8d1b-fa8a34c3092a", "Stilt Village", "", [MissionID.AMBROSE_SHADOWSINTHEWATER]),
			new("af29c8d3-45cd-4b36-b7ed-5acc9398556d", "Central Social Hub", "", [MissionID.AMBROSE_SHADOWSINTHEWATER]),
			new("ba619c92-9c55-4009-aa03-bf207c370548", "Shrine", "", [MissionID.AMBROSE_SHADOWSINTHEWATER]),
			new("a0b501e9-e693-4a67-9e95-8887855cc5c2", "Pirate Camp", "Metal Worker", [MissionID.AMBROSE_SHADOWSINTHEWATER]),
			new("8b444765-d082-4c34-932f-796084a5a819", "Militia Camp", "Cook", [MissionID.AMBROSE_SHADOWSINTHEWATER]),
		];

		public static string GetEntranceName(string id) {
			var entrance = Entrances.Find(e => e.ID == id);
			if (entrance != null)
				return entrance.Name;
			return "";
		}

		public static string GetEntranceCommonName(string id) {
			var entrance = Entrances.Find(e => e.ID == id);
			if (entrance != null)
				return entrance.Disguise.Length > 0 ? entrance.Disguise : entrance.Name;
			return "";
		}
	}
}
