using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace Croupier {
	public class EditBingoComboBoxItem(BingoTile? tile = null, bool isSeparator = false) {
		public BingoTile? Tile { get; set; } = tile;

		public string Name { get; set; } = tile == null ? "" : (tile.Disabled ? $"(Disabled) {tile.Text}" : tile.Text);
		public bool IsSeparator { get; set; } = isSeparator;
	}

	public class EditTile(MissionID mission) : INotifyPropertyChanged {
		private int selectedIndex = 0;
		private readonly MissionID mission = mission;

		public ObservableCollection<EditBingoComboBoxItem> AvailableTiles {
			get {
				var items = new ObservableCollection<EditBingoComboBoxItem>();
				if (Bingo.Main.Tiles.Count > 0) {
					var list = Bingo.Main.Tiles.Where(t => !t.Disabled && (t.Missions.Count == 0 || t.Missions.Contains(mission)))
						.OrderBy(t => t.Name)
						.OrderBy(t => t.GroupText)
						.ToList();
					var group = "";
					foreach (var tile in list) {
						if (group != tile.GroupText) {
							items.Add(new EditBingoComboBoxItem(null, true) { Name = tile.GroupText });
							group = tile.GroupText;
						}
						items.Add(new EditBingoComboBoxItem(tile));
					}
				}
				return items;
			}
		}

		public EditBingoComboBoxItem Selected => AvailableTiles[selectedIndex];

		public int SelectedIndex {
			get => selectedIndex;
			set {
				if (value < 0 || value >= AvailableTiles.Count) return;
				selectedIndex = value;
				OnPropertyChanged(nameof(SelectedIndex));
			}
		}

		public event PropertyChangedEventHandler? PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName) {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}

	public class EditBingoViewModel : ViewModel {
		public List<EditTile> Tiles { get; set; } = [];
	}

	public partial class EditBingoWindow : Window {
		private readonly EditBingoViewModel viewModel = new();
		private readonly BingoGame game;

		public EditBingoWindow(BingoGame game) {
			this.game = game;
			DataContext = viewModel;
			InitializeComponent();
			if (game.Card != null)
				UpdateTiles(game.Card.Tiles);
			game.CardUpdated += OnCardUpdated;
		}

		private void UpdateTiles(ReadOnlyObservableCollection<BingoTile?> tiles) {
			viewModel.Tiles.Clear();
			for (var i = 0; i < tiles.Count; ++i) {
				var tile = tiles[i];
				var editTile = new EditTile(game.Mission);
				if (tile != null) editTile.SelectedIndex = editTile.AvailableTiles.ToList().FindIndex(t => t.Tile == tile?.Source);
				viewModel.Tiles.Add(editTile);
				var idx = i;
				editTile.PropertyChanged += (sender, e) => {
					if (editTile.Selected == null) return;
					game.Card?.SetTile(idx, editTile.Selected.Tile);
				};
			}
		}

		private void RefitWindow() {
			if (viewModel.Tiles.Count >= 4) MinHeight = 4 * 104.3 + 40;
			else if (viewModel.Tiles.Count >= 3) MinHeight = 3 * 104.3 + 40;
			else if (viewModel.Tiles.Count >= 2) MinHeight = 2 * 104.3 + 40;
			else if (viewModel.Tiles.Count >= 1) MinHeight = 1 * 104.3 + 40;
			MaxHeight = viewModel.Tiles.Count * 104.3 + 40;
		}

		private void OnSizeChange(object sender, SizeChangedEventArgs e) {
			if (e.WidthChanged) RefitWindow();
		}

		private void OnCardUpdated(object? sender, BingoCard? card) {
			viewModel.Tiles.Clear();
			if (card != null) UpdateTiles(card.Tiles);
		}
	}
}
