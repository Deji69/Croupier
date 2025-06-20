using Croupier.Exceptions;
using System;
using System.Collections.Generic;

namespace Croupier {
	public class BingoGenerator {
		private static readonly Random random = new();

		public static BingoCard Generate(int tiles = 25, MissionID missionId = MissionID.NONE) {
			if (missionId == MissionID.NONE)
				missionId = Mission.GetRandomMissionID();
			var mission = Mission.Get(missionId);
			BingoCard card = new(mission.ID);
			var availableTiles = Bingo.Main.Tiles.FindAll(t => t.Missions.Contains(mission.ID));

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
	}
}
