using Croupier.Exceptions;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Windows;

namespace Croupier {
	public abstract class BingoTileTrigger {
		public abstract bool Test(GameEvents.EventValue ev);
		public virtual void Advance(BingoTileState state) {
			state.Complete = true;
		}
		public virtual object[] GetFormatArgs(BingoTileState state) {
			return [];
		}

		public static BingoTileTrigger? FromJson(JsonNode json) {
			if (json["Setpiece"] != null) {
				var setpiece = json["Setpiece"]!.AsObject();
				return new BingoTileSetpieceTrigger() {
					RepositoryId = setpiece["RepositoryId"]?.GetValue<string>(),
					Name = setpiece["name_metricvalue"]?.GetValue<string>(),
					Type = setpiece["setpieceType_metricvalue"]?.GetValue<string>(),
					ToolUsed = setpiece["toolUsed_metricvalue"]?.GetValue<string>(),
				};
			}
			if (json["Collect"] != null) {
				var collect = json["Collect"]!.AsObject();
				StringCollection items = [];
				foreach (var node in collect["Items"]?.AsArray() ?? []) {
					if (node == null) continue;
					items.Add(node.GetValue<string>());
				}
				return new BingoTileCollectTrigger() {
					Items = items,
					Count = collect["Count"]?.GetValue<int>(),
					Max = collect["Max"]?.GetValue<int>(),
				};
			}
			if (json["Disguise"] != null) {
				StringCollection items = [];
				foreach (var node in json["Disguise"]?.AsArray() ?? []) {
					if (node == null) continue;
					items.Add(node.GetValue<string>());
				}
				return new BingoTileDisguiseTrigger() {
					Items = items
				};
			}
			return null;
		}
	}

	public class BingoTileSetpieceTrigger : BingoTileTrigger {
		public string? RepositoryId { get; set; }
		public string? Name { get; set; }
		public string? Type { get; set; }
		public string? ToolUsed { get; set; }

		public override bool Test(GameEvents.EventValue ev) {
			if (ev is GameEvents.SetpiecesEventValue v) {
				if (Name != null && v.name_metricvalue != Name)
					return false;
				if (Type != null && v.setpieceType_metricvalue != Type)
					return false;
				if (ToolUsed != null && v.toolUsed_metricvalue != ToolUsed)
					return false;
				if (RepositoryId != null && v.RepositoryId != RepositoryId)
					return false;
				return true;
			}
			return false;
		}
	}

	public class BingoTileCollectTrigger : BingoTileTrigger {
		public required StringCollection Items { get; set; }
		public int? Count { get; set; }
		public int? Max { get; set; }

		public override bool Test(GameEvents.EventValue ev) {
			if (ev is GameEvents.ItemPickedUpEventValue v) {
				if (Items.Contains(v.RepositoryId))
					return false;
				return true;
			}
			return false;
		}

		public override void Advance(BingoTileState state) {
			if (Count == null) state.Complete = true;
			else state.Complete = ++state.Counter >= Count;
		}

		public override object[] GetFormatArgs(BingoTileState state) {
			return [state.Counter, Count ?? 1];
		}
	}

	public class BingoTileDisguiseTrigger : BingoTileTrigger {
		public required StringCollection Items { get; set; }
		public int? Count { get; set; }
		public int? Max { get; set; }

		public override bool Test(GameEvents.EventValue ev) {
			if (ev is GameEvents.ItemPickedUpEventValue v) {
				if (Items.Contains(v.RepositoryId))
					return false;
				return true;
			}
			return false;
		}

		public override void Advance(BingoTileState state) {
			if (Count == null) state.Complete = true;
			else state.Complete = ++state.Counter >= Count;
		}

		public override object[] GetFormatArgs(BingoTileState state) {
			return [];
		}
	}

	public class BingoTileState {
		public bool Complete { get; set; } = false;
		public int Counter { get; set; } = 0;
	}

	public partial class BingoTile : INotifyPropertyChanged, ICloneable {
		public required string Name { get; set; }
		public string? Group { get; set; }
		public required List<MissionID> Missions { get; set; }
		public required StringCollection Tags { get; set; }
		public required BingoTileTrigger Trigger { get; set; }
		public string Text => ToString();
		public string GroupText => Group != null ? $"{Group}:" : "";
		public Visibility GroupTextVisibility => Group != null ? Visibility.Visible : Visibility.Collapsed;

		private BingoTileState state = new();

		public bool Complete {
			get => state.Complete;
			set {
				state.Complete = value;
				OnPropertyChanged(nameof(Complete));
			}
		}

		public void Reset() {
			state = new();
			OnPropertyChanged(nameof(Complete));
		}

		public void Advance() {
			Trigger.Advance(state);
			OnPropertyChanged(nameof(Text));
			OnPropertyChanged(nameof(Complete));
		}


		public override string ToString() {
			var args = Trigger.GetFormatArgs(this.state);
			var res = string.Format(Name, args);
			return res;
		}

		public event PropertyChangedEventHandler? PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName) {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public object Clone() {
			return MemberwiseClone();
		}

		public static BingoTile FromJson(JsonNode json, string? filename = null) {
			var name = (json["Name"]?.GetValue<string>()) ?? throw new BingoTileConfigException("Invalid 'Name' property.");
			var group = (json["Group"]?.GetValue<string>());
			var tags = (StringCollection)[];
			var missions = (List<MissionID>)[];

			foreach (var node in json["Tags"]?.AsArray() ?? []) {
				try {
					if (node == null) throw new BingoTileConfigException("Invalid entry in 'Tags' array.");
					var v = node.GetValue<string>();
					if (v == null || v.Length == 0) throw new BingoTileConfigException("Invalid entry in 'Tags' array.");
					tags.Add(v);
				}
				catch (BingoConfigException e) {
					MessageBox.Show($"{(filename != null ? $"File: {filename}\n" : "")}Exception: {e.Message}", "Bingo tiles JSON error", MessageBoxButton.OK, MessageBoxImage.Warning);
				}
			}

			foreach (var node in json["Missions"]?.AsArray() ?? []) {
				try {
					if (node == null) throw new BingoTileConfigException("Invalid entry in 'Missions' array.");
					var v = node.GetValue<string>();
					if (v == null || v.Length == 0) throw new BingoTileConfigException("Invalid entry in 'Missions' array.");
					var id = MissionIDMethods.FromName(v);
					if (id == MissionID.NONE) throw new BingoTileConfigException($"Unknown mission '{v}' in 'Missions' array.");
					missions.Add(id);
				}
				catch (BingoConfigException e) {
					MessageBox.Show($"{(filename != null ? $"File: {filename}\n" : "")}Exception: {e.Message}", "Bingo tiles JSON error", MessageBoxButton.OK, MessageBoxImage.Warning);
				}
			}

			try {
				return new() {
					Name = name,
					Group = group,
					Missions = missions,
					Tags = tags,
					Trigger = BingoTileTrigger.FromJson(json) ?? throw new BingoTileConfigException($"No trigger logic found in tile."),
				};
			}
			catch (Exception e) {
				throw new BingoTileConfigException($"Exception while loading bingo tile trigger logic (Name: '{name}')", e);
			}
		}

		[GeneratedRegex("/^(.+):\\s/i")]
		private static partial Regex ColonSeparatorRegexGenerator();
		private static Regex ColonSeparatorRegex = ColonSeparatorRegexGenerator();
	}
}
