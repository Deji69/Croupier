using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Croupier {
	public class MissionStart {
		public string Location { get; set; } = "";
		public string[] Loadout { get; set; } = [];
	}

	public class MissionCompletion {
		public double IGT { get; set; } = 0;
		public bool SA { get; set; } = false;
		public bool KillsValidated { get; set; } = false;
	}

	public class MissionStats(MissionID mission) {
		public MissionID Mission { get; set; } = mission;
		public int NumSpins { get; set; } = 0;
		public int NumAttempts { get; set; } = 0;
		public int NumWins { get; set; } = 0;
	}

	public class SpinCompletionStats {
		public MissionID Mission = MissionID.NONE;
		public double IGT = 0;
		public double RTA = 0;
		public int? DailyID = null;
		public string StartLocation = "";
		public List<string> Loadout = [];
		public string Comment = "";
	}

	public class SpinStats(MissionID mission, string spin) {
		public MissionID Mission = mission;
		public string Spin { get; set; } = spin;
		public int Attempts { get; set; } = 0;
		public bool IsCustom { get; set; } = false;
		public int DailyID { get; set; } = 0;
		public List<SpinCompletionStats> Completions { get; set; } = [];

		public SpinCompletionStats? GetFastestIGTCompletion() {
			SpinCompletionStats? best = null;
			foreach (var c in Completions) {
				if (c.IGT > 0 && (best == null || c.IGT < best.IGT))
					best = c;
			}
			return best;
		}

		public SpinCompletionStats? GetFastestRTACompletion() {
			SpinCompletionStats? best = null;
			foreach (var c in Completions) {
				if (c.RTA > 0 && (best == null || c.RTA < best.RTA))
					best = c;
			}
			return best;
		}
	}

	public class Stats {
		public Dictionary<MissionID, MissionStats> MissionStats { get; set; } = [];
		public Dictionary<string, SpinStats> SpinStats { get; set; } = [];
		public int NumRandomSpins { get; set; } = 0;
		public int NumDailySpins { get; set; } = 0;
		public int NumCustomSpins { get; set; } = 0;
		public int NumWins { get; set; } = 0;
		public int NumNonSAWins { get; set; } = 0;
		public int NumUniqueWins { get; set; } = 0;
		public int NumAttempts { get; set; } = 0;
		public int NumUniqueAttempts { get; set; } = 0;
		public int NumValidKills { get; set; } = 0;
		public int TopStreak { get; set; } = 0;

		public MissionStats? GetMostSpunMissionStats() {
			MissionStats? best = null;
			foreach (var item in MissionStats) {
				if (best == null || item.Value.NumSpins > best.NumSpins)
					best = item.Value;
			}
			return best?.NumSpins > 0 ? best : null;
		}

		public MissionStats? GetMostPlayedMissionStats() {
			MissionStats? best = null;
			foreach (var item in MissionStats) {
				if (best == null || item.Value.NumAttempts > best.NumAttempts)
					best = item.Value;
			}
			return best?.NumAttempts > 0 ? best : null;
		}

		public MissionStats? GetMostWonMissionStats() {
			MissionStats? best = null;
			foreach (var item in MissionStats) {
				if (best == null || item.Value.NumWins > best.NumWins)
					best = item.Value;
			}
			return best?.NumWins > 0 ? best : null;
		}

		public Entrance? GetMostUsedEntrance() {
			Dictionary<string, int> scoreBook = [];
			foreach (var item in SpinStats) {
				foreach (var completion in item.Value.Completions) {
					if (scoreBook.TryGetValue(completion.StartLocation, out var _))
						++scoreBook[completion.StartLocation];
					else
						scoreBook[completion.StartLocation] = 0;
				}
			}

			if (scoreBook.Count == 0)
				return null;

			var key = scoreBook.MaxBy(s => s.Value).Key;
			return Locations.Entrances.Find(e => e.ID == key);
		}

		public SpinStats? GetFastestIGTSpinStats(MissionID mission = MissionID.NONE) {
			SpinStats? bestStats = null;
			SpinCompletionStats? bestCompletionStats = null;
			foreach (var item in SpinStats.Where(s => s.Value.Completions.Count > 0 && (mission == MissionID.NONE || s.Value.Mission == mission))) {
				var completion = item.Value.GetFastestIGTCompletion();
				if (completion == null)
					continue;
				if (completion.IGT > 0 && (bestStats == null || completion.IGT < bestCompletionStats?.IGT)) {
					bestStats = item.Value;
					bestCompletionStats = completion;
				}
			}
			return bestStats;
		}

		public SpinStats? GetFastestRTASpinStats(MissionID mission = MissionID.NONE) {
			SpinStats? bestStats = null;
			SpinCompletionStats? bestCompletionStats = null;
			foreach (var item in SpinStats.Where(s => s.Value.Completions.Count > 0 && (mission == MissionID.NONE || s.Value.Mission == mission))) {
				var completion = item.Value.GetFastestRTACompletion();
				if (completion == null)
					continue;
				if (completion.RTA > 0 && (bestStats == null || completion.RTA < bestCompletionStats?.RTA)) {
					bestStats = item.Value;
					bestCompletionStats = completion;
				}
			}
			return bestStats;
		}

		public SpinStats? GetSlowestIGTSpinStats(MissionID mission = MissionID.NONE) {
			SpinStats? bestStats = null;
			SpinCompletionStats? bestCompletionStats = null;
			foreach (var item in SpinStats.Where(s => s.Value.Completions.Count > 0 && (mission == MissionID.NONE || s.Value.Mission == mission))) {
				var completion = item.Value.GetFastestIGTCompletion();
				if (completion == null)
					continue;
				if (completion.IGT > 0 && (bestStats == null || completion.IGT > bestCompletionStats?.IGT)) {
					bestStats = item.Value;
					bestCompletionStats = completion;
				}
			}
			return bestStats;
		}

		public SpinStats? GetSlowestRTASpinStats(MissionID mission = MissionID.NONE) {
			SpinStats? bestStats = null;
			SpinCompletionStats? bestCompletionStats = null;
			foreach (var item in SpinStats.Where(s => s.Value.Completions.Count > 0 && (mission == MissionID.NONE || s.Value.Mission == mission))) {
				var completion = item.Value.GetFastestRTACompletion();
				if (completion == null)
					continue;
				if (completion.RTA > 0 && (bestStats == null || completion.RTA > bestCompletionStats?.RTA)) {
					bestStats = item.Value;
					bestCompletionStats = completion;
				}
			}
			return bestStats;
		}

		public double GetAverageBestIGT(MissionID mission = MissionID.NONE) {
			List<double> times = [];
			foreach (var item in SpinStats.Where(s => s.Value.Completions.Count > 0 && (mission == MissionID.NONE || s.Value.Mission == mission))) {
				foreach (var c in item.Value.Completions)
					times.Add(c.IGT);
			}
			return times.Count > 0 ? times.Average() : 0;
		}

		public double GetAverageBestRTA(MissionID mission = MissionID.NONE) {
			List<double> times = [];
			foreach (var item in SpinStats.Where(s => s.Value.Completions.Count > 0 && (mission == MissionID.NONE || s.Value.Mission == mission))) {
				foreach (var c in item.Value.Completions.Where(c => c.RTA > 0))
					times.Add(c.RTA);
			}
			return times.Count > 0 ? times.Average() : 0;
		}

		public SpinStats GetSpinStats(Spin spin) {
			var spinStr = spin.ToString();
			if (SpinStats.TryGetValue(spinStr, out var spinStats))
				return spinStats;
			var res = new SpinStats(spin.Mission, spinStr);
			SpinStats.Add(spinStr, res);
			return res;
		}

		public MissionStats GetMissionStats(MissionID mission) {
			if (MissionStats.TryGetValue(mission, out var stats))
				return stats;
			var res = new MissionStats(mission);
			MissionStats.Add(mission, res);
			return res;
		}
	}
}
