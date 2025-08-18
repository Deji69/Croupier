using Croupier.Exceptions;
using Croupier.GameEvents;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Text.Json;
using System.Windows;

namespace Croupier {
	public class BingoEvent {
		public required MissionID Mission { get; set; }
		public List<BingoTileEventValue> Tiles { get; set; } = [];
	}

	public class BingoTileEventValue {
		public required string Text { get; set; }
		public string Group { get; set; } = "";
		public UInt32? GroupColour { get; set; } = null;
		public bool Achieved { get; set; } = false;
		public bool Failed { get; set; } = false;
	}

	public class BingoGame : ViewModel {
		private readonly GameController controller;
		public event EventHandler<BingoCard?>? CardUpdated;

		private BingoCard? card = null;
		private bool enableSocketOperations = false;
		public BingoCard? Card => card;


		private BingoTileType bingoTileType = BingoTileType.Objective;
		public BingoTileType TileType {
			get => bingoTileType;
			set {
				var oldType = bingoTileType;
				SetProperty(ref bingoTileType, value);
				Config.Default.BingoTileType = value;
				if (Card != null && value != oldType)
					Draw();
			}
		}

		public MissionID Mission => card?.Mission ?? MissionID.NONE;

		private int cardSize = 25;
		public int CardSize {
			get => cardSize;
			set {
				SetProperty(ref cardSize, value);
				Config.Default.BingoCardSize = cardSize;
				if (Card != null && Card.Tiles.Count != cardSize)
					Draw();
			}
		}
		private bool enableGroupTileColours = true;
		public bool EnableGroupTileColours {
			get => enableGroupTileColours;
			set {
				SetProperty(ref enableGroupTileColours, value);
				Config.Default.EnableGroupTileColors = enableGroupTileColours;
				RefreshColors();
			}
		}

		public int NumTiles => card?.Tiles.Count ?? 0;

		public BingoGame(GameController controller) {
			this.controller = controller;
			CroupierSocketServer.MissionStart += (sender, start) => {
				card?.Reset();
				SendAreasToClient();
			};
			CroupierSocketServer.MissionComplete += (sender, arg) => card?.Finish();
			CroupierSocketServer.Event += OnEvent;
			CroupierSocketServer.Connected += (sender, arg) => {
				enableSocketOperations = true;
				SendAreasToClient();
			};
		}

		public void LoadConfig(Config cfg) {
			TileType = cfg.BingoTileType;
			EnableGroupTileColours = cfg.EnableGroupTileColors;
		}

		// Draw card for a new round of bingo.
		public void Draw() {
			try {
				// Make sure configuration is loaded.
				Roulette.Main.Load();
				Bingo.Main.LoadConfiguration();
				var generator = new BingoGenerator(TileType);
				card = generator.Generate(CardSize, controller.MissionID);
				card.PropertyChanged += Card_PropertyChanged;
				SendAreasToClient();
			} catch (BingoGeneratorException e) {
				MessageBox.Show(
					e.Message,
					"Bingo Generation Error - Croupier",
					MessageBoxButton.OK,
					MessageBoxImage.Exclamation
				);
			}

			CardUpdated?.Invoke(this, card);
		}

		private void Card_PropertyChanged(object? sender, PropertyChangedEventArgs e) {
			CardUpdated?.Invoke(this, card);
		}

		public void RefreshColors() {
			if (card == null) return;
			foreach (var tile in card.Tiles) {
				tile?.RefreshColor();
			}
		}

		private void SendAreasToClient() {
			if (!enableSocketOperations) return;
			if (card == null || card.Tiles.Count == 0) return;

			dynamic obj = new ExpandoObject();
			obj.Name = "Areas";
			var areas = new List<dynamic>();

			foreach (var area in Bingo.Main.Areas.Where(a => a.Missions.Count == 0 || a.Missions.Contains(Mission))) {
				dynamic data = new ExpandoObject();
				data.ID = area.ID;
				data.From = area.From;
				data.To = area.To;
				areas.Add(area);
			}

			obj.Data = areas.ToArray();
			CroupierSocketServer.Send(obj);
		}

		public void SendBingoDataToClient() {
			if (Card == null) return;
			var tiles = new List<BingoTileEventValue>();
			foreach (var tile in Card.Tiles) {
				if (tile == null) continue;
				uint r = (uint)(tile.GroupTextColor.Color.R);
				uint g = (uint)(tile.GroupTextColor.Color.G);
				uint b = (uint)(tile.GroupTextColor.Color.B);
				uint a = (uint)(tile.GroupTextColor.Color.A);
				tiles.Add(new() {
					Text = tile.Text,
					Group = tile.GroupTextVisibilityBool ? tile.GroupText : "",
					GroupColour = (a << 24) | (b << 16) | (g << 8) | r,
					Achieved = tile.Achieved,
					Failed = tile.Failed,
				});
			}

			CroupierSocketServer.Send("BingoData:" + JsonSerializer.Serialize(new BingoEvent() {
				Mission = GameController.Main.MissionID,
				Tiles = tiles,
			}));
		}

		private void OnEvent(object? sender, string evData) {
			try {
				if (card == null) return;
				var json = JsonDocument.Parse(evData);
				var ev = json.Deserialize<Event>(jsonGameEventSerializerOptions);
				if (ev != null) {
					var val = DeserializeEventValue(ev.Name, ev.Value is JsonElement value ? value : null);
					if (val != null) {
						if (card.TryAdvance(val))
							SendBingoDataToClient();
					}
				}
			}
			catch (JsonException e) {
				Debug.WriteLine(e);
			}
		}

		private PacifyEventValue? ImbuePacifyEvent(PacifyEventValue? value) {
			if (value?.OutfitRepositoryId != null) {
				var playerRepoId = value.OutfitRepositoryId.ToLower();
				var playerDisguise = Roulette.Main.GetDisguiseByRepoId(playerRepoId, Mission != MissionID.NONE ? Mission : null);
				value.OutfitIsUnique = playerDisguise?.Unique;
			}
			if (value?.ActorOutfitRepositoryId != null) {
				var victimRepoId = value.ActorOutfitRepositoryId.ToLower();
				var victimDisguise = Roulette.Main.GetDisguiseByRepoId(victimRepoId, Mission != MissionID.NONE ? Mission : null);
				value.ActorOutfitIsUnique = victimDisguise?.Unique;
			}
			return value;
		}

		private DisguiseEventValue? ImbueDisguiseEvent(DisguiseEventValue? value) {
			if (value == null) return value;
			var repoId = value.RepositoryId;
			var disguise = Roulette.Main.GetDisguiseByRepoId(repoId, Mission != MissionID.NONE ? Mission : null);
			value.IsUnique = disguise?.Unique;
			return value;
		}

		// TODO: Something better
		private EventValue? DeserializeEventValue(string name, JsonElement? jsonEl = null) {
			if (jsonEl is JsonElement json) {
				return name switch {
					"Actorsick" =>  json.Deserialize<ActorSickEventValue>(jsonGameEventSerializerOptions),
					"AgilityStart" => AgilityStartEventValue.Load(json),
					"BodyBagged" => json.Deserialize<BodyBaggedEventValue>(jsonGameEventSerializerOptions),
					"BodyFound" => json.Deserialize<BodyFoundEventValue>(jsonGameEventSerializerOptions),
					"BodyHidden" => json.Deserialize<BodyHiddenEventValue>(jsonGameEventSerializerOptions),
					"CarExploded" => json.Deserialize<CarExplodedEventValue>(jsonGameEventSerializerOptions),
					"Crocodile" => json.Deserialize<CrocodileEventValue>(jsonGameEventSerializerOptions),
					"DartHit" => json.Deserialize<DartHitEventValue>(jsonGameEventSerializerOptions),
					"Disguise" => ImbueDisguiseEvent(json.Deserialize<DisguiseEventValue>(jsonGameEventSerializerOptions)),
					"DoorBroken" => json.Deserialize<DoorBrokenEventValue>(jsonGameEventSerializerOptions),
					"DoorUnlocked" => json.Deserialize<DoorUnlockedEventValue>(jsonGameEventSerializerOptions),
					"DragBodyMove" => DragBodyMoveEventValue.Load(json),
					"DrainPipeClimbed" => json.Deserialize<DrainPipeClimbedEventValue>(jsonGameEventSerializerOptions),
					"EnterArea" => json.Deserialize<EnterAreaEventValue>(jsonGameEventSerializerOptions),
					"EnterRoom" => json.Deserialize<EnterRoomEventValue>(jsonGameEventSerializerOptions),
					"Explosion" => json.Deserialize<ExplosionEventValue>(jsonGameEventSerializerOptions),
					"FriskedSuccess" => json.Deserialize<FriskedSuccessEventValue>(jsonGameEventSerializerOptions),
					"InstinctActive" => json.Deserialize<InstinctActiveEventValue>(jsonGameEventSerializerOptions),
					"Investigate_Curious" => json.Deserialize<InvestigateCuriousEventValue>(jsonGameEventSerializerOptions),
					"ItemDestroyed" => json.Deserialize<ItemDestroyedEventValue>(jsonGameEventSerializerOptions),
					"ItemDropped" => json.Deserialize<ItemDroppedEventValue>(jsonGameEventSerializerOptions),
					"ItemPickedUp" => json.Deserialize<ItemPickedUpEventValue>(jsonGameEventSerializerOptions),
					"ItemRemovedFromInventory" => json.Deserialize<ItemRemovedFromInventoryEventValue>(jsonGameEventSerializerOptions),
					"ItemThrown" => json.Deserialize<ItemThrownEventValue>(jsonGameEventSerializerOptions),
					"Kill" => ImbuePacifyEvent(json.Deserialize<KillEventValue>(jsonGameEventSerializerOptions)),
					"Level_Setup_Events" => json.Deserialize<LevelSetupEventValue>(jsonGameEventSerializerOptions),
					"Movement" => json.Deserialize<MovementEventValue>(jsonGameEventSerializerOptions),
					"OnDestroy" => json.Deserialize<OnDestroyEventValue>(jsonGameEventSerializerOptions),
					"OnEvacuationStarted" => json.Deserialize<OnEvacuationStartedEventValue>(jsonGameEventSerializerOptions),
					"OnInitialFracture" => json.Deserialize<OnInitialFractureEventValue>(jsonGameEventSerializerOptions),
					"OnPickup" => OnPickupEventValue.Load(json),
					"OnTurnOn" => json.Deserialize<OnTurnOnEventValue>(jsonGameEventSerializerOptions),
					"OnTurnOff" => json.Deserialize<OnTurnOffEventValue>(jsonGameEventSerializerOptions),
					"OnWeaponReload" => OnWeaponReloadEventValue.Load(json),
					"OpenDoor" => json.Deserialize<OpenDoorEventValue>(jsonGameEventSerializerOptions),
					"OpportunityEvents" => json.Deserialize<OpportunityEventValue>(jsonGameEventSerializerOptions),
					"Pacify" => ImbuePacifyEvent(json.Deserialize<PacifyEventValue>(jsonGameEventSerializerOptions)),
					"PlayerShot" => PlayerShotEventValue.Load(json),
					"ProjectileBodyShot" => json.Deserialize<ProjectileBodyShotEventValue>(jsonGameEventSerializerOptions),
					"setpieces" => json.Deserialize<SetpiecesEventValue>(jsonGameEventSerializerOptions),
					"SecuritySystemRecorder" => json.Deserialize<SecuritySystemRecorderEventValue>(jsonGameEventSerializerOptions),
					"StartingSuit" => ImbueDisguiseEvent(json.Deserialize<StartingSuitEventValue>(jsonGameEventSerializerOptions)),
					"Trespassing" => json.Deserialize<TrespassingEventValue>(jsonGameEventSerializerOptions),
					null => null,
					_ => DeserializeEventValue(name),
				};
			}
			return name switch {
				"ItemStashed" => new ItemStashedEventValue(),
				"OnIsFullyInCrowd" => new OnIsFullyInCrowdEventValue(),
				"OnIsFullyInVegetation" => new OnIsFullyInVegetationEventValue(),
				"OnTakeDamage" => new OnTakeDamageEventValue(),
				"ProjectileBodyShot" => new ProjectileBodyShotEventValue(),
				_ => null,
			};
		}

		private static readonly JsonSerializerOptions jsonGameEventSerializerOptions = new() {
			AllowTrailingCommas = true,
			ReadCommentHandling = JsonCommentHandling.Skip,
		};
	}
}
