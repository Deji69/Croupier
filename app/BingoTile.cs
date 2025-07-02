using Croupier.Exceptions;
using Croupier.GameEvents;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;

namespace Croupier {
	public enum BingoTileType {
		Objective,
		Complication,
		Mixed,
	}

	public class BingoTileState {
		public bool Complete { get; set; } = false;
		public int Counter { get; set; } = 0;
		public object? Data { get; set; } = null;
		public object? History { get; set; } = null;
	}

	public partial class BingoTile : INotifyPropertyChanged, ICloneable {
		public required string Name { get; set; }
		public string? NameSingular { get; set; } = null;
		public BingoTileType Type { get; set; } = BingoTileType.Objective;
		public bool Disabled { get; set; } = false;
		public BingoGroup? Group {
			get => group;
			set {
				group = value;
				if (group?.Color != null) {
					groupTextBrush = (SolidColorBrush?)new BrushConverter().ConvertFromString(group.Color) ?? throw new CroupierException("Could not create group text colour brush.");
					OnPropertyChanged(nameof(GroupTextColor));
				}
				OnPropertyChanged(nameof(GroupTextVisibility));
			}
		}
		public string? GroupName => Group?.Name;
		public required List<MissionID> Missions { get; set; }
		public required StringCollection Tags { get; set; }
		public required BingoTrigger Trigger { get; set; }
		public string Text => ToString();
		public string? Tip {
			get {
				var fmt = tip ?? Group?.Tip;
				if (fmt == null) return null;
				var args = Trigger.GetFormatArgs(state);
				var res = string.Format(fmt, args);
				return res;
			}
			set {
				tip = value;
				OnPropertyChanged(nameof(Tip));
			}
		}
		public string GroupText => Group != null ? $"{Group.Name}" : "";
		public Visibility GroupTextVisibility => Group != null && !Group.Hidden ? Visibility.Visible : Visibility.Collapsed;
		public SolidColorBrush GroupTextColor => Config.Default.EnableGroupTileColors ? groupTextBrush : defaultBrush;

		private BingoGroup? group = null;
		private BingoTileState state = new();
		private SolidColorBrush groupTextBrush = new(new() { R = 200, G = 200, B = 200, A = 255 });
		private readonly SolidColorBrush defaultBrush = new(new() { R = 200, G = 200, B = 200, A = 255 });
		private bool isScored = false;
		private string? tip = null;

		public bool Complete {
			get => state.Complete;
			set {
				state.Complete = value;
				OnPropertyChanged(nameof(Complete));
				OnPropertyChanged(nameof(Failed));
			}
		}

		public bool Achieved => (Type != BingoTileType.Complication || isScored) && state.Complete;
		public bool Failed => Type == BingoTileType.Complication && !state.Complete;

		public void Reset() {
			state = new() {
				Complete = Type != BingoTileType.Objective
			};
			isScored = false;
			OnPropertyChanged(nameof(Complete));
			OnPropertyChanged(nameof(Achieved));
			OnPropertyChanged(nameof(Failed));
		}

		public bool Test(EventValue ev) {
			return Trigger.Test(ev, state);
		}

		public void Advance() {
			Trigger.Advance(state);
			OnPropertyChanged(nameof(Text));
			OnPropertyChanged(nameof(Complete));
			OnPropertyChanged(nameof(Achieved));
			OnPropertyChanged(nameof(Failed));
		}

		public void Score() {
			isScored = true;
			OnPropertyChanged(nameof(Achieved));
		}

		public void RefreshColor() {
			OnPropertyChanged(nameof(GroupTextColor));
		}

		public override string ToString() {
			var args = Trigger.GetFormatArgs(state);
			var useSingular = (Trigger.Count ?? 1) - (state.Complete ? 0 : state.Counter) == 1;
			var res = string.Format(useSingular ? NameSingular ?? Name : Name, args);
			return res;
		}

		public event PropertyChangedEventHandler? PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName) {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public object Clone() {
			return MemberwiseClone();
		}

		public static BingoTile FromJson(JsonElement json) {
			if (!json.TryGetProperty("Name", out var nameProp))
				throw new BingoTileConfigException("Missing required property 'Name' for bingo tile.");
			if (nameProp.ValueKind != JsonValueKind.String)
				throw new BingoTileConfigException($"Invalid property 'Name' for bingo tile, expected string but got {nameProp.ValueKind}.");

			var name = nameProp.GetString()!;

			try {
				var nameSingular = json.TryGetProperty("Name", out var nameSingularProp) ? nameSingularProp.GetString() : null;
				var disabled = json.TryGetProperty("Disabled", out var disabledProp) ? disabledProp.GetBoolean() : false;
				var tip = json.TryGetProperty("Tip", out var tipProp) ? tipProp.GetString() : null;
				var groupName = json.TryGetProperty("Group", out var groupProp) ? groupProp.GetString() : null;
				var typeName = json.TryGetProperty("Type", out var typeProp) ? typeProp.GetString() : null;
			
				var group = Bingo.Main.Groups.Find(g => g.ID == groupName);
				var type = typeName != null ? Enum.Parse<BingoTileType>(typeName) : BingoTileType.Objective;

				var tags = (StringCollection)[];
				var missions = (List<MissionID>)[];

				if (json.TryGetProperty("Tags", out var tagsProp)) {
					if (tagsProp.ValueKind != JsonValueKind.Array)
						throw new BingoTileConfigException($"Invalid property 'Tags', expected array but got {tagsProp.ValueKind}.");
					foreach (var node in tagsProp.EnumerateArray()) {
						if (node.ValueKind != JsonValueKind.String)
							throw new BingoTileConfigException($"Invalid element in 'Tags', expected string but got {node.ValueKind}.");
						var v = node.GetString();
						if (v == null || v.Length == 0) throw new BingoTileConfigException("Empty or invalid tag in 'Tags' array.");
						tags.Add(v);
					}
				}

				if (json.TryGetProperty("Missions", out var missionsProp)) {
					if (missionsProp.ValueKind != JsonValueKind.Array)
						throw new BingoTileConfigException($"Invalid property 'Missions', expected array but got {missionsProp.ValueKind}.");
					foreach (var node in missionsProp.EnumerateArray()) {
						if (node.ValueKind != JsonValueKind.String)
							throw new BingoTileConfigException($"Invalid element in 'Missions', expected string but got {node.ValueKind}.");
						var v = node.GetString();
						var id = MissionIDMethods.FromName(v ?? "");
						if (id == MissionID.NONE) throw new BingoTileConfigException($"Unknown mission '{v}' in 'Missions' array.");
						missions.Add(id);
					}
				}

				return new() {
					Name = name,
					NameSingular = nameSingular,
					Disabled = disabled,
					Type = type,
					Tip = tip,
					Group = group,
					Missions = missions,
					Tags = tags,
					Trigger = BingoTrigger.FromJson(type, json) ?? throw new BingoTileConfigException($"No trigger logic found in tile."),
				};
			}
			catch (Exception e) {
				throw new BingoTileConfigException($"Exception while loading bingo tile trigger logic (Name: '{name}').\n{e.Message}", e);
			}
		}

		[GeneratedRegex("/^(.+):\\s/i")]
		private static partial Regex ColonSeparatorRegexGenerator();
		private static Regex ColonSeparatorRegex = ColonSeparatorRegexGenerator();
	}
}
