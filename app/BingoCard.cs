using Croupier.Exceptions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace Croupier {
	public enum BingoWinType {
		Horizonal,
		Vertical,
		Diagonal,
		Majority,
		Completion,
	}

	public struct BingoCardSize(int rows, int columns, bool remainder = false) {
		public int Rows = rows;
		public int Columns = columns;
		public bool HasRemainder = remainder;
	}

	public class BingoWinResult(BingoCard card, BingoWinType type, List<int> tiles) {
		public BingoCard Card { get; set; } = card;
		public BingoWinType Type { get; set; } = type;
		public List<int> TileIndexes { get; set; } = tiles;
		public List<BingoTile> Tiles => [..Card.Tiles.Where((t, i) => TileIndexes.Contains(i))];
	}

	public class BingoCard : INotifyPropertyChanged {
		public BingoTileType Mode = BingoTileType.Objective;
		public MissionID Mission = MissionID.NONE;
		public BingoCardSize Size => size;
		public ReadOnlyObservableCollection<BingoTile?> Tiles => new(tiles);

		private readonly ObservableCollection<BingoTile?> tiles = [];
		private BingoCardSize size = new(0, 0, false);

		public BingoCard(BingoTileType mode, MissionID mission = MissionID.NONE) {
			Mode = mode;
			Mission = mission;
			tiles.CollectionChanged += (sender, e) => {
				OnPropertyChanged(nameof(Tiles));
				OnPropertyChanged(nameof(Size));
			};
		}

		public void Add(BingoTile tile) {
			var liveTile = (BingoTile)tile.Clone();
			liveTile.Reset();
			tiles.Add(liveTile);
			size = GetCardSize();
		}

		public void SetTile(int index, BingoTile? tile) {
			if (index < 0 || index >= tiles.Count) throw new ArgumentOutOfRangeException($"Invalid index: {index}");
			if (tile == null) tiles[index] = null;
			else {
				var liveTile = (BingoTile)tile.Clone();
				liveTile.Reset();
				tiles[index] = liveTile;
			}
			OnPropertyChanged(nameof(Tiles));
		}

		public bool TryAdvance(GameEvents.EventValue ev) {
			return TestObjectives(ev) || TestComplications(ev);
		}
		 
		private bool TestObjectives(GameEvents.EventValue ev) {
			var advanced = false;
			foreach (var tile in Tiles) {
				if (tile == null) continue;
				if (tile.Type != BingoTileType.Objective) continue;
				if (tile.Complete) continue;
				if (!tile.Test(ev)) continue;
				tile.Advance();
				advanced = true;
			}
			return advanced;
		}

		private bool TestComplications(GameEvents.EventValue ev) {
			var advanced = false;
			foreach (var tile in Tiles) {
				if (tile == null) continue;
				if (tile.Type != BingoTileType.Complication) continue;
				if (tile.Failed) continue;
				if (!tile.Test(ev)) continue;
				tile.Advance();
				advanced = true;
			}
			return advanced;
		}

		public bool HasWon() {
			var numCompleted = Tiles.Count(t => t?.Complete ?? true);
			if (numCompleted == Tiles.Count)
				return true;

			for (var row = 0; row < Size.Rows; ++row) {
				if (TestRow(row)) return true;
			}
			for (var col = 0; col < Size.Columns; ++col) {
				if (TestColumn(col)) return true;
			}
			return TestDiagonal() || TestReverseDiagonal();
		}

		private static List<BingoTriggerEnterArea> GetEnterAreaTriggerChildren(IBingoTrigger trigger) {
			if (trigger is BingoTriggerEnterArea t) return [t];
			if (trigger is BingoTriggerComplication c)
				return c.Trigger != null ? GetEnterAreaTriggerChildren(c.Trigger) : [];
			if (trigger is BingoTriggerAND tand) {
				List<BingoTriggerEnterArea> res = [];
				foreach (var tandTrigger in tand.Triggers) {
					if (tandTrigger == null) continue;
					res.AddRange(GetEnterAreaTriggerChildren(tandTrigger));
				}
				return res;
			}
			if (trigger is BingoTriggerOR tor) {
				List<BingoTriggerEnterArea> res = [];
				foreach (var torTrigger in tor.Triggers) {
					if (torTrigger == null) continue;
					res.AddRange(GetEnterAreaTriggerChildren(torTrigger));
				}
				return res;
			}
			if (trigger is BingoTriggerNOT tnot) {
				if (tnot.Trigger != null)
					return GetEnterAreaTriggerChildren(tnot.Trigger);
			}
			return [];
		}

		public void Finish() {
			if (Mode != BingoTileType.Complication && Mode != BingoTileType.Mixed) return;
			for (var row = 0; row < Size.Rows; ++row) {
				if (TestRow(row))
					ScoreIndexes(GetRowIndexes(row));
			}
			for (var col = 0; col < Size.Columns; ++col) {
				if (TestColumn(col))
					ScoreIndexes(GetColumnIndexes(col));
			}
			if (TestDiagonal())
				ScoreIndexes(GetDiagonalIndexes());
			if (TestReverseDiagonal())
				ScoreIndexes(GetReverseDiagonalIndexes());
		}

		public int PositionToIndex(int col, int row) {
			if (col >= Size.Rows) throw new BingoException("Noot noot (1)");
			if (row >= Size.Columns) throw new BingoException("Noot noot (2)");
			return col + row * Size.Columns;
		}

		public bool TestPosition(int col, int row) {
			var idx = PositionToIndex(col, row);
			if (idx >= Tiles.Count) throw new BingoException("Noot noot (3)");
			return Tiles[idx]?.Complete ?? true;
		}

		public bool TestDiagonal() {
			if (!IsCardSquare()) return false;
			for (int col = 0, row = 0; row < Size.Rows && col < Size.Columns; ++col, ++row) {
				if (!TestPosition(col, row)) return false;
			}
			return true;
		}

		public bool TestReverseDiagonal() {
			if (!IsCardSquare()) return false;
			for (int col = Size.Columns - 1, row = Size.Rows - 1; row >= 0 && col >= 0; --col, --row) {
				if (!TestPosition(col, row)) return false;
			}
			return true;
		}

		public bool TestRow(int row) {
			for (var col = 0; col < Size.Columns; ++col) {
				if (!TestPosition(col, row)) return false;
			}
			return true;
		}

		public bool TestColumn(int col) {
			for (var row = 0; row < Size.Rows; ++row) {
				if (!TestPosition(col, row)) return false;
			}
			return true;
		}

		public List<int> GetRowIndexes(int row) {
			List<int> indexes = [];
			for (var col = 0; col < Size.Columns; ++col)
				indexes.Add(PositionToIndex(col, row));
			return indexes;
		}

		public List<int> GetColumnIndexes(int col) {
			List<int> indexes = [];
			for (var row = 0; row < Size.Columns; ++row)
				indexes.Add(PositionToIndex(col, row));
			return indexes;
		}

		public List<int> GetDiagonalIndexes() {
			if (!IsCardSquare()) return [];
			List<int> indexes = [];
			for (int col = 0, row = 0; col * row < Tiles.Count; ++row, ++col)
				indexes.Add(PositionToIndex(col, row));
			return indexes;
		}

		public List<int> GetReverseDiagonalIndexes() {
			if (!IsCardSquare()) return [];
			List<int> indexes = [];
			for (int col = Size.Columns - 1, row = Size.Rows - 1; col > 0 && row > 0; --row, --col)
				indexes.Add(PositionToIndex(col, row));
			return indexes;
		}

		private void ScoreIndexes(List<int> indexes) {
			foreach (var idx in indexes) {
				if (idx >= Tiles.Count) continue;
				Tiles[idx]?.Score();
			}
		}

		private bool IsCardSquare() {
			var sqrt = Math.Sqrt(Tiles.Count);
			return Math.Abs(Math.Ceiling(sqrt) - Math.Floor(sqrt)) < Double.Epsilon;
		}

		public void Reset() {
			foreach (var tile in Tiles)
				tile?.Reset();
		}

		private BingoCardSize GetCardSize() {
			if (Tiles.Count <= 5) return new(Tiles.Count, 1);
			if (Tiles.Count == 6) return new(3, 2);
			if (Tiles.Count == 8) return new(4, 2);
			if (Tiles.Count == 10) return new(5, 2);
			if (Tiles.Count == 12) return new(4, 3);
			if (Tiles.Count == 15) return new(5, 3);
			if (Tiles.Count == 20) return new(5, 4);
			if (Tiles.Count == 30) return new(6, 5);
			if (IsCardSquare()) {
				var sqrt = (int)(Math.Sqrt(Tiles.Count));
				return new(sqrt, sqrt);
			}
			if (Tiles.Count < 36) {
				var rem = Tiles.Count % 5 != 0;
				return new(5, (Tiles.Count / 5) + (rem ? 1 : 0), rem);
			}
			var rem2 = Tiles.Count % 6 != 0;
			return new(6, (Tiles.Count / 6) + (rem2 ? 1 : 0), rem2);
		}

		public override string ToString() {
			var missionName = Croupier.Mission.TryGet(Mission)?.Name;
			var str = missionName != null ? $"{missionName}: " : "";

			foreach (var cond in Tiles) {
				if (str.Length > 0) str += ", ";
				str += cond?.ToString();
			}

			return str;
		}

		public event PropertyChangedEventHandler? PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName) {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
