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
		public string? Color {
			get => color;
			set => color = value;
		}

		private string? color = null;

		public static BingoGroup FromJson(string id, JsonNode json, string? filename = null) {
			var name = (json["Name"]?.GetValue<string>()) ?? throw new BingoTileConfigException($"Invalid 'Name' property for group with ID '{id}'.");
			var color = json["Color"]?.GetValue<string>();
			if (color != null) {
				_ = new BrushConverter().ConvertFromString(color) ?? throw new BingoTileConfigException($"Invalid Color property for group with ID '{id}'.");
			}
			return new(id, name) {
				Color = color,
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
				var json = JsonNode.Parse(File.ReadAllText(file)) ?? throw new BingoTileConfigException($"Failed to parse JSON file {file}.");
				LoadGroupsFromJson(json);
			}
			catch (BingoConfigException e) {
				MessageBox.Show($"File: {file}\nException: {e.Message}", "Config Error - Croupier", MessageBoxButton.OK, MessageBoxImage.Exclamation);
			}
		}

		public void LoadTilesFromFile(string file) {
			try {
				var json = JsonNode.Parse(File.ReadAllText(file)) ?? throw new BingoTileConfigException($"Failed to parse JSON file {file}.");
				LoadTilesFromJson(json);
			}
			catch (BingoConfigException e) {
				MessageBox.Show($"File: {file}\nException: {e.Message}", "Config Error - Croupier", MessageBoxButton.OK, MessageBoxImage.Exclamation);
			}
		}

		public void LoadTilesFromJson(JsonNode json) {
			var jsonArray = json.AsArray();
			foreach (var item in jsonArray) {
				var obj = item?.AsObject() ?? throw new BingoTileConfigException($"Invalid bingo tile entry.");
				var tile = BingoTile.FromJson(obj);
				Tiles.Add(tile);
			}
		}

		public void LoadGroupsFromJson(JsonNode json) {
			var jsonArray = json.AsObject();
			foreach (var (k, item) in jsonArray) {
				if (k == null) throw new BingoTileConfigException($"Invalid bingo group entry.");
				var obj = item?.AsObject() ?? throw new BingoTileConfigException($"Invalid bingo group entry ({k}).");
				var group = BingoGroup.FromJson(k, obj);
				Groups.Add(group);
			}
		}
	}
}
