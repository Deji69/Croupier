using Croupier.Exceptions;
using Croupier.GameEvents;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;

namespace Croupier {
	public enum BingoTileType {
		Objective,
		Complication,
		Mixed,
	}

	public class BingoGroup(string name) {
		public string Name { get; set; } = name;
		public bool Hidden { get; set; } = false;
		public BingoTileType Type { get; set; } = BingoTileType.Mixed;
		public string? Tip { get; set; }
		public string? Color {
			get => color;
			set => color = value;
		}

		private string? color = null;

		public static BingoGroup FromJson(JsonElement json) {
			if (!json.TryGetProperty("Name", out var nameProp))
				throw new BingoTileConfigException($"Missing property 'Name' for group.");
			if (nameProp.ValueKind != JsonValueKind.String)
				throw new BingoTileConfigException($"Invalid type {nameProp.ValueKind} of property 'Name' for group, expected string.");

			var name = nameProp.GetString()!;
			var color = json.TryGetProperty("Color", out var colorProp) ? colorProp.GetString() : null;
			var hidden = json.TryGetProperty("Hidden", out var hiddenProp) && hiddenProp.GetBoolean();
			var tip = json.TryGetProperty("Tip", out var tipProp) ? tipProp.GetString() : null;
			if (color != null)
				_ = new BrushConverter().ConvertFromString(color) ?? throw new BingoTileConfigException($"Invalid 'Color' property for group '{name}'.");
			var type = json.TryGetProperty("Type", out var typeProp) ? typeProp.GetString() : null;

			return new(name) {
				Color = color,
				Tip = tip,
				Hidden = hidden,
				Type = type switch {
					"Objective" => BingoTileType.Objective,
					"Complication" => BingoTileType.Complication,
					_ => BingoTileType.Mixed,
				},
			};
		}
	}

	public class BingoArea {
		public required string ID { get; set; }
		public List<MissionID> Missions { get; set; } = [];
		public SVector3? From { get; set; } = null;
		public SVector3? To { get; set; } = null;

		public static BingoArea FromJson(JsonElement json) {
			if (!json.TryGetProperty("ID", out var idProp))
				throw new BingoTileConfigException($"Missing property 'ID'.");
			if (idProp.ValueKind != JsonValueKind.String)
				throw new BingoTileConfigException($"Invalid type of property 'ID', expected string but got {idProp.ValueKind}.");

			try {
				var area = new BingoArea() {
					ID = idProp.GetString()!,
				};

				if (json.TryGetProperty("Missions", out var missionsProp)) {
					if (missionsProp.ValueKind != JsonValueKind.Array)
						throw new BingoTileConfigException($"Invalid property 'Missions', expected array but got {missionsProp.ValueKind}.");
					foreach (var node in missionsProp.EnumerateArray()) {
						if (node.ValueKind != JsonValueKind.String)
							throw new BingoTileConfigException($"Invalid element in 'Missions', expected string but got {node.ValueKind}.");
						var v = node.GetString();
						var mid = MissionIDMethods.FromName(v ?? "");
						if (mid == MissionID.NONE) throw new BingoTileConfigException($"Unknown mission '{v}' in 'Missions' array.");
						area.Missions.Add(mid);
					}
				}

				if (json.TryGetProperty("From", out var fromProp))
					area.From = LoadSVector3(fromProp, "From");
				if (json.TryGetProperty("To", out var toProp))
					area.To = LoadSVector3(toProp, "To");
				return area;
			} catch (Exception e) {
				throw new BingoConfigException($"Exception while loading bingo area '{idProp.GetString()}'.", e);
			}
		}

		private static SVector3 LoadSVector3(JsonElement json, string prop) {
			if (json.ValueKind != JsonValueKind.Array)
				throw new BingoConfigException($"Invalid property '{prop}', expected array but got {json.ValueKind}.");
			if (json.GetArrayLength() != 3)
				throw new BingoConfigException($"Invalid property '{prop}', expected array of length 3 but the length is {json.GetArrayLength()}.");
			if (json[0].ValueKind != JsonValueKind.Number || json[1].ValueKind != JsonValueKind.Number || json[2].ValueKind != JsonValueKind.Number)
				throw new BingoConfigException($"Invalid property '{prop}', all values must be numbers.");
			return new() {
				X = json[0].GetDouble(),
				Y = json[1].GetDouble(),
				Z = json[2].GetDouble(),
			};
		}
	}

	public class Bingo {
		private struct ConfigSection {
			public string Filename;
			public JsonElement Element;
		}
		private struct ConfigSections {
			public List<ConfigSection> areas = [];
			public List<ConfigSection> groups = [];
			public List<ConfigSection> tiles = [];

			public ConfigSections() { }
		}

		public static readonly Bingo Main = new();

		public List<BingoTile> Tiles { get; } = [];
		public List<BingoGroup> Groups { get; } = [];
		public List<BingoArea> Areas { get; } = [];

		private bool loaded = false;

		public void LoadConfiguration(bool reload = false) {
			if (loaded && !reload) return;
			
			Areas.Clear();
			Groups.Clear();
			Tiles.Clear();

			var jsonDocuments = new List<JsonDocument>();
			var sections = new ConfigSections();

			foreach (var file in Directory.GetFiles("config/bingo", "*.json", SearchOption.AllDirectories)) {
				try {
					var options = new JsonDocumentOptions {
						CommentHandling = JsonCommentHandling.Skip,
						AllowTrailingCommas = true,
					};
					var json = JsonDocument.Parse(File.ReadAllText(file), options) ?? throw new BingoConfigException("Failed to parse JSON.");
					jsonDocuments.Add(json);
					if (json.RootElement.ValueKind == JsonValueKind.Array)
						sections.tiles.Add(new(){ Filename = file, Element = json.RootElement });
					else if (json.RootElement.ValueKind == JsonValueKind.Object) {
						var subSections = LoadJsonObject(json.RootElement, file);
						sections.areas.AddRange(subSections.areas);
						sections.groups.AddRange(subSections.groups);
						sections.tiles.AddRange(subSections.tiles);
					}
					else
						throw new BingoConfigException("Expected array or object as JSON root node.");
				}
				catch (Exception e) {
					MessageBox.Show($"File: {file}\nException: {e.Message}", "Config Error - Croupier", MessageBoxButton.OK, MessageBoxImage.Exclamation);
				}
			}

			// Load the config sections in a specific order.
			foreach (var cfg in sections.areas)
				LoadAreasFromJson(cfg.Element);
			foreach (var cfg in sections.groups)
				LoadGroupsFromJson(cfg.Element);
			foreach (var cfg in sections.tiles)
				LoadTilesFromJson(cfg.Element);

			foreach (var doc in jsonDocuments)
				doc.Dispose();

			loaded = true;
		}

		private static ConfigSections LoadJsonObject(JsonElement json, string filename) {
			var sections = new ConfigSections();
			foreach (var elem in json.EnumerateObject()) {
				try {
					switch (elem.Name) {
						case "Areas":
							sections.areas.Add(new() { Element = elem.Value, Filename = filename });
							break;
						case "Groups":
							sections.groups.Add(new() { Element = elem.Value, Filename = filename });
							break;
						case "Tiles":
							sections.tiles.Add(new() { Element = elem.Value, Filename = filename });
							break;
					}
				} catch (Exception e) {
					throw new BingoConfigException($"Exception while loading '{elem.Name}' in JSON.\n{e.Message}", e);
				}
			}
			return sections;
		}

		private void LoadAreasFromJson(JsonElement json) {
			if (json.ValueKind != JsonValueKind.Array)
				throw new BingoConfigException($"Expected array but got {json.ValueKind}.");
			foreach (var elem in json.EnumerateArray()) {
				if (elem.ValueKind != JsonValueKind.Object)
					throw new BingoConfigException($"Invalid array element, expected object but got {elem.ValueKind}.");
				Areas.Add(BingoArea.FromJson(elem));
			}
		}

		private void LoadTilesFromJson(JsonElement json) {
			if (json.ValueKind != JsonValueKind.Array)
				throw new BingoConfigException("Expected array.");

			foreach (var item in json.EnumerateArray()) {
				if (item.ValueKind != JsonValueKind.Object)
					throw new BingoTileConfigException($"Expected object for bingo tile array entry, found {item.ValueKind}.");
				
				var tile = BingoTile.FromJson(item);
				Tiles.Add(tile);
			}
		}

		private void LoadGroupsFromJson(JsonElement json) {
			if (json.ValueKind != JsonValueKind.Array)
				throw new BingoConfigException("Expected array.");

			foreach (var elem in json.EnumerateArray()) {
				if (elem.ValueKind != JsonValueKind.Object)
					throw new BingoTileConfigException($"Invalid bingo group entry ({elem.ValueKind}).");

				var group = BingoGroup.FromJson(elem);
				Groups.Add(group);
			}
		}
	}
}
