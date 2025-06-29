using Croupier.Exceptions;
using System;
using System.Collections.Generic;

namespace Croupier {
	public class BingoGenerator(BingoTileType mode = BingoTileType.Objective) {
		private static readonly Random random = new();
		private readonly BingoTileType Mode = mode;

		public BingoCard Generate(int tiles = 25, MissionID missionId = MissionID.NONE) {
			if (missionId == MissionID.NONE)
				missionId = Mission.GetRandomMissionID();
			var mission = Mission.Get(missionId);
			BingoCard card = new(Mode, mission.ID);
			var availableTiles = GetTiles(missionId);

			if (availableTiles.Count < tiles)
				throw new BingoGeneratorException($"Insufficient {mission.Name} tiles available for {tiles}-tile board.");

			for (int i = 0; i < tiles; ++i) {
				var tile = availableTiles[random.Next(availableTiles.Count)];
				var liveTile = (BingoTile)tile.Clone();
				liveTile.Reset();
				availableTiles.Remove(tile);
				card.Add(liveTile);
			}

			return card;
		}

		private List<BingoTile> GetTiles(MissionID missionID) {
			return Bingo.Main.Tiles.FindAll(t => {
				if (t.Disabled)
					return false;
				if (t.Missions.Count != 0 && !t.Missions.Contains(missionID))
					return false;
				return t.Type == Mode;
			});
		}
	}
}
