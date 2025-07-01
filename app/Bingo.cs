using Croupier.Exceptions;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Windows;
using System.Windows.Media;

namespace Croupier {
	public class BingoGroup(string id, string name) {
		public string ID { get; set; } = id;
		public string Name { get; set; } = name;
		public bool Hidden { get; set; } = false;
		public string? Tip { get; set; }
		public string? Color {
			get => color;
			set => color = value;
		}

		private string? color = null;

		public static BingoGroup FromJson(string id, JsonElement json) {
			if (!json.TryGetProperty("Name", out var nameProp))
				throw new BingoTileConfigException($"Missing property 'Name' for group with ID '{id}'.");
			if (nameProp.ValueKind != JsonValueKind.String)
				throw new BingoTileConfigException($"Invalid type of property 'Name' for group with ID '{id}', expected string.");

			var name = nameProp.GetString()!;
			var color = json.TryGetProperty("Color", out var colorProp) ? colorProp.GetString() : null;
			var hidden = json.TryGetProperty("Hidden", out var hiddenProp) && hiddenProp.GetBoolean();
			var tip = json.TryGetProperty("Tip", out var tipProp) ? tipProp.GetString() : null;
			if (color != null)
				_ = new BrushConverter().ConvertFromString(color) ?? throw new BingoTileConfigException($"Invalid Color property for group with ID '{id}'.");
			return new(id, name) {
				Color = color,
				Tip = tip,
				Hidden = hidden,
			};
		}
	}

	public class Bingo {
		public static readonly Bingo Main = new();

		public List<BingoTile> Tiles { get; } = [];
		public List<BingoGroup> Groups { get; } = [];

		private bool loaded = false;

		public void LoadConfiguration(bool reload = false) {
			if (loaded && !reload) return;
			Tiles.Clear();
			Groups.Clear();
			if (File.Exists("config/bingo/group.json"))
				LoadGroupsFromFile("config/bingo/group.json");
			foreach (var file in Directory.GetFiles("config/bingo", "*.json", SearchOption.AllDirectories)) {
				if (file.EndsWith("group.json")) continue;
				LoadTilesFromFile(file);
			}
			loaded = true;
		}

		public void LoadGroupsFromFile(string file) {
			try {
				var options = new JsonDocumentOptions {
					CommentHandling = JsonCommentHandling.Skip,
					AllowTrailingCommas = true,
				};
				using var json = JsonDocument.Parse(File.ReadAllText(file), options) ?? throw new BingoTileConfigException($"Failed to parse JSON file {file}.");
				LoadGroupsFromJson(json.RootElement);
			}
			catch (BingoConfigException e) {
				MessageBox.Show($"File: {file}\nException: {e.Message}", "Config Error - Croupier", MessageBoxButton.OK, MessageBoxImage.Exclamation);
			}
		}

		public void LoadTilesFromFile(string file) {
			try {
				var json = JsonNode.Parse(File.ReadAllText(file)) ?? throw new BingoTileConfigException($"Failed to parse JSON file {file}.");
				LoadTilesFromJson(json, file);
			}
			catch (BingoConfigException e) {
				MessageBox.Show($"File: {file}\nException: {e.Message}", "Config Error - Croupier", MessageBoxButton.OK, MessageBoxImage.Exclamation);
			}
		}

		public void LoadTilesFromJson(JsonNode json, string? file = null) {
			var jsonArray = json.AsArray();
			foreach (var item in jsonArray) {
				var obj = item?.AsObject() ?? throw new BingoTileConfigException($"Invalid bingo tile entry.");
				var tile = BingoTile.FromJson(obj, file);
				Tiles.Add(tile);
			}
		}

		public void LoadGroupsFromJson(JsonElement json) {
			using var jsonArray = json.EnumerateObject();
			foreach (var elem in jsonArray) {
				if (elem.Value.ValueKind != JsonValueKind.Object)
					throw new BingoTileConfigException($"Invalid bingo group entry ({elem.Name}).");
				var group = BingoGroup.FromJson(elem.Name, elem.Value);
				Groups.Add(group);
			}
		}
	}
}
