using Croupier.Exceptions;
using Croupier.GameEvents;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
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

		public static BingoTile FromJson(JsonObject json, string? filename = null) {
			var name = (json["Name"]?.GetValue<string>()) ?? throw new BingoTileConfigException("Invalid 'Name' property.");
			var nameSingular = json["NameSingular"]?.GetValue<string>();
			var disabled = json["Disabled"]?.GetValue<bool>() ?? false;
			var tip = json["Tip"]?.GetValue<string>();
			var groupName = json["Group"]?.GetValue<string>();
			var tags = (StringCollection)[];
			var missions = (List<MissionID>)[];
			var group = Bingo.Main.Groups.Find(g => g.ID == groupName);
			var typeName = json["Type"]?.GetValue<string>();

			var type = typeName != null ? Enum.Parse<BingoTileType>(typeName) : BingoTileType.Objective;

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
