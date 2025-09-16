using Croupier.Exceptions;
using System;

namespace Croupier {
	public class BingoGenerator(BingoTileType mode = BingoTileType.Objective) {
		private static readonly Random random = new();
		private readonly BingoTileType Mode = mode;

		public BingoCard Generate(int tiles = 25, MissionID missionId = MissionID.NONE) {
			if (missionId == MissionID.NONE)
				missionId = Mission.GetRandomMissionID();
			var mission = Mission.Get(missionId);
			BingoCard card = new(mission.ID);
			var availableTiles = Bingo.Main.GetTilesForMission(mission.ID, Mode);

			if (availableTiles.Count < tiles)
				throw new BingoGeneratorException($"Insufficient {mission.Name} tiles available for {tiles}-tile board.");

			for (int i = 0; i < tiles; ++i) {
				var tile = availableTiles[random.Next(availableTiles.Count)];
				availableTiles.Remove(tile);
				card.Add(tile);
			}

			return card;
		}
	}
}
