﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Croupier {
	public class StatisticsViewModel : ViewModel {
		public string Name { get; set; }
		public string Description { get; set; }
		public object Value { get; set; }
	}

	public class HistoryViewModel(SpinCompletionStats stats) : ViewModel, IEditableObject {
		public string Mission { get; set; }
		public string Spin { get; set; }
		public string IGT { get; set; }
		public string Entrance { get; set; }

		public string Comment {
			get => comment;
			set {
				if (value.Length > 1000)
					comment = value[0..1000];
				else
					comment = value;
			}
		}

		private readonly SpinCompletionStats stats = stats;
		private bool editing = false;
		private string comment = "";
		private string commentTemp = "";

		void IEditableObject.BeginEdit() {
			if (editing) return;
			commentTemp = Comment;
			editing = true;
		}

		void IEditableObject.CancelEdit() {
			if (!editing) return;
			Comment = commentTemp;
			editing = false;
		}

		void IEditableObject.EndEdit() {
			if (!editing) return;
			commentTemp = "";
			editing = false;

			stats.Comment = Comment;
			Config.Save();
		}
	}

	public class StatisticsWindowViewModel : ViewModel {
		public ObservableCollection<StatisticsViewModel> MainStats { get; set; } = [];
		public ObservableCollection<HistoryViewModel> History { get; set; } = [];
		public string Title {
			get {
				if (FilterMissionID == MissionID.NONE)
					return "Stats";
				return Mission.GetMissionName(FilterMissionID) + " Stats";
			}
		}

		public MissionID FilterMissionID {
			get => _filterMissionID;
			set {
				_filterMissionID = value;
				UpdateProperty(nameof(FilterMissionID));
				UpdateProperty(nameof(Title));
			}
		}

		private MissionID _filterMissionID = MissionID.NONE;

		public List<MissionComboBoxItem> Missions {
			get {
				var items = new List<MissionComboBoxItem>();
				var group = MissionGroup.None;
				items.Add(new() {
					ID = MissionID.NONE,
					Name = "(ALL)",
					Location = "",
					IsSeparator = false
				});
				foreach (var mission in Mission.All) {
					if (mission.Group != group) {
						items.Add(new() {
							Name = mission.Group.GetName(),
							IsSeparator = true,
						});
						group = mission.Group;
					}
					if (mission.ID == MissionID.NONE)
						continue;
					items.Add(new() {
						ID = mission.ID,
						Name = mission.Name,
						Location = mission.Location,
						IsSeparator = false
					});
				}
					
				return items;
			}
		}

		public StatisticsWindowViewModel() {
			Update();
			UpdateHistory();

			Config.OnSave += (object sender, int _) => {
				Update();
				UpdateHistory();
			};
		}

		public void UpdateHistory() {
			History.Clear();

			foreach (var spin in Config.Default.Stats.SpinStats) {
				if (FilterMissionID != MissionID.NONE && spin.Value.Mission != FilterMissionID)
					continue;
				foreach (var c in spin.Value.Completions) {
					History.Insert(0, new(c) {
						Entrance = Locations.GetEntranceCommonName(c.StartLocation),
						IGT = this.FormatIGT(c.IGT),
						Mission = Mission.GetMissionName(c.Mission),
						Spin = spin.Key,
						Comment = c.Comment ?? "",
					});
				}
			}
		}

		public void Update() {
			MainStats.Clear();
			if (FilterMissionID == MissionID.NONE)
				AddGlobalStats();
			else
				AddMissionStats(FilterMissionID);
		}

		public void AddGlobalStats() {
			MainStats.Add(new() {
				Name = "Spins",
				Description = "Total spins created.",
				Value = Config.Default.Stats.NumCustomSpins + Config.Default.Stats.NumRandomSpins,
			});
			MainStats.Add(new() {
				Name = "Random Spins",
				Description = "Spin randomly generated by Croupier.",
				Value = Config.Default.Stats.NumRandomSpins,
			});
			MainStats.Add(new() {
				Name = "Custom Spins",
				Description = "Spins edited/pasted/received and attempted.",
				Value = Config.Default.Stats.NumCustomSpins,
			});
			MainStats.Add(new() {
				Name = "Top Streak",
				Description = "Highest number of consecutive SA spin completions.",
				Value = Config.Default.Stats.TopStreak,
			});
			MainStats.Add(new() {
				Name = "Attempts",
				Description = "Number of mission attempts.",
				Value = Config.Default.Stats.NumAttempts,
			});
			MainStats.Add(new() {
				Name = "Spin Attempts",
				Description = "Unique spin attempts.",
				Value = Config.Default.Stats.NumUniqueAttempts,
			});
			MainStats.Add(new() {
				Name = "Spin Wins",
				Description = "Total SA spin completions.",
				Value = Config.Default.Stats.NumWins,
			});
			MainStats.Add(new() {
				Name = "Messy Spin Completions",
				Description = "You didn't get SA, but at least you killed the targets and escaped...",
				Value = Config.Default.Stats.NumNonSAWins,
			});
			MainStats.Add(new() {
				Name = "Unique Spin Wins",
				Description = "SA completions of unique spins.",
				Value = Config.Default.Stats.NumUniqueWins,
			});

			MainStats.Add(new() {
				Name = "Target Kills",
				Description = "Total number of times you've killed a target with the correct conditions.",
				Value = Config.Default.Stats.NumValidKills,
			});

			var averageBestIGT = FormatIGT(Config.Default.Stats.GetAverageBestIGT());

			MainStats.Add(new() {
				Name = "Avg IGT",
				Description = "The time averaged from your fastest completions of every spin.",
				Value = averageBestIGT,
			});

			var fastestIGT = "N/A";
			var fastestIGTMission = "N/A";
			var fastestIGTSpin = "N/A";

			var fastestIGTSpinStats = Config.Default.Stats.GetFastestIGTSpinStats();
			if (fastestIGTSpinStats != null) {
				fastestIGT = FormatIGT(fastestIGTSpinStats.GetFastestIGTCompletion().IGT);
				fastestIGTMission = Mission.GetMissionName(fastestIGTSpinStats.Mission);
				fastestIGTSpin = fastestIGTSpinStats.Spin;
			}

			var longestIGT = "N/A";
			var longestIGTMission = "N/A";
			var longestIGTSpin = "N/A";

			var longestIGTSpinStats = Config.Default.Stats.GetSlowestIGTSpinStats();
			if (longestIGTSpinStats != null) {
				longestIGT = FormatIGT(longestIGTSpinStats.GetFastestIGTCompletion().IGT);
				longestIGTMission = Mission.GetMissionName(longestIGTSpinStats.Mission);
				longestIGTSpin = longestIGTSpinStats.Spin;
			};

			MainStats.Add(new() {
				Name = "Fastest IGT",
				Description = "Fastest in-game time achieved on a spin.",
				Value = fastestIGT,
			});
			MainStats.Add(new() {
				Name = "Fastest IGT Mission",
				Description = "The mission you got your fastest in-game time on.",
				Value = fastestIGTMission,
			});
			MainStats.Add(new() {
				Name = "Fastest IGT Spin",
				Description = "The spin you've completed with the fastest in-game time.",
				Value = fastestIGTSpin,
			});

			MainStats.Add(new() {
				Name = "Longest IGT",
				Description = "Longest in-game time achieved on a spin.",
				Value = longestIGT,
			});
			MainStats.Add(new() {
				Name = "Longest IGT Mission",
				Description = "The mission you got your longest in-game time on.",
				Value = longestIGTMission,
			});
			MainStats.Add(new() {
				Name = "Longest IGT Spin",
				Description = "The spin you spent the most in-game time completing.",
				Value = longestIGTSpin,
			});

			var mostSpunMission = "N/A";
			var mostPlayedMission = "N/A";
			var mostWonMission = "N/A";

			var mostSpunMissionStats = Config.Default.Stats.GetMostSpunMissionStats();
			if (mostSpunMissionStats != null) {
				mostSpunMission = Mission.GetMissionName(mostSpunMissionStats.Mission) + $" ({mostSpunMissionStats.NumSpins})";
			}

			var mostPlayedMissionStats = Config.Default.Stats.GetMostPlayedMissionStats();
			if (mostPlayedMissionStats != null) {
				mostPlayedMission = Mission.GetMissionName(mostPlayedMissionStats.Mission) + $" ({mostPlayedMissionStats.NumAttempts})";
			}

			var mostWonMissionStats = Config.Default.Stats.GetMostWonMissionStats();
			if (mostWonMissionStats != null) {
				mostWonMission = Mission.GetMissionName(mostWonMissionStats.Mission) + $" ({mostWonMissionStats.NumWins})";
			}

			MainStats.Add(new() {
				Name = "Most Spun Mission",
				Description = "The mission appearing in the highest number of spins.",
				Value = mostSpunMission,
			});
			MainStats.Add(new() {
				Name = "Most Attempted Mission",
				Description = "The mission you have started/restarted the most.",
				Value = mostPlayedMission,
			});
			MainStats.Add(new() {
				Name = "Most Won Mission",
				Description = "The mission you've beaten the most spins on.",
				Value = mostWonMission,
			});

			var mostUsedEntrance = Config.Default.Stats.GetMostUsedEntrance();
			var mostUsedEntranceStr = "N/A";
			if (mostUsedEntrance != null)
				mostUsedEntranceStr = mostUsedEntrance.Disguise.Length > 0 ? mostUsedEntrance.Disguise : mostUsedEntrance.Name;

			MainStats.Add(new() {
				Name = "Top Starting Location",
				Description = "The starting location you have beaten the most spins from.",
				Value = mostUsedEntranceStr,
			});
		}


		public void AddMissionStats(MissionID mission) {
			var missionStats = Config.Default.Stats.GetMissionStats(mission);
			
			MainStats.Add(new() {
				Name = "Spins",
				Description = "Total spins created.",
				Value = missionStats.NumSpins,
			});
			MainStats.Add(new() {
				Name = "Attempts",
				Description = "Number of mission attempts.",
				Value = missionStats.NumAttempts,
			});
			MainStats.Add(new() {
				Name = "Spin Wins",
				Description = "Total SA spin completions.",
				Value = missionStats.NumWins,
			});

			var averageBestIGT = FormatIGT(Config.Default.Stats.GetAverageBestIGT(mission));

			MainStats.Add(new() {
				Name = "Avg IGT",
				Description = "The time averaged from your fastest completions of every spin.",
				Value = averageBestIGT,
			});

			var fastestIGT = "N/A";
			var fastestIGTSpin = "N/A";

			var fastestIGTSpinStats = Config.Default.Stats.GetFastestIGTSpinStats(mission);
			if (fastestIGTSpinStats != null) {
				fastestIGT = FormatIGT(fastestIGTSpinStats.GetFastestIGTCompletion().IGT);
				fastestIGTSpin = fastestIGTSpinStats.Spin;
			}

			var longestIGT = "N/A";
			var longestIGTSpin = "N/A";

			var longestIGTSpinStats = Config.Default.Stats.GetSlowestIGTSpinStats(mission);
			if (longestIGTSpinStats != null) {
				longestIGT = FormatIGT(longestIGTSpinStats.GetFastestIGTCompletion().IGT);
				longestIGTSpin = longestIGTSpinStats.Spin;
			};

			MainStats.Add(new() {
				Name = "Fastest IGT",
				Description = "Fastest in-game time achieved on a spin.",
				Value = fastestIGT,
			});
			MainStats.Add(new() {
				Name = "Fastest IGT Spin",
				Description = "The spin you've completed with the fastest in-game time.",
				Value = fastestIGTSpin,
			});

			MainStats.Add(new() {
				Name = "Longest IGT",
				Description = "Longest in-game time achieved on a spin.",
				Value = longestIGT,
			});
			MainStats.Add(new() {
				Name = "Longest IGT Spin",
				Description = "The spin you spent the most in-game time completing.",
				Value = longestIGTSpin,
			});
		}

		private string FormatIGT(double igt) {
			var ts = TimeSpan.FromSeconds(igt);
			var str = "";

			if (ts.TotalHours >= 1)
				str = ts.ToString(@"h\h\ m\m\ ss\s");
			else if (ts.TotalMinutes >= 1)
				str = ts.ToString(@"m\m\ ss\s");
			else
				str = ts.ToString(@"ss\s");

			var frac = ts.ToString("FFF").TrimEnd('0');

			return str + (frac.Length > 0 ? $" {frac}ms" : "");
		}
	}
	
	public partial class StatisticsWindow : Window {
		private readonly StatisticsWindowViewModel viewModel = new();

		public StatisticsWindow() {
			DataContext = viewModel;
			InitializeComponent();
		}

		public override void OnApplyTemplate() {
			base.OnApplyTemplate();
		}

		private void MissionFilterComboBox_Selected(object sender, SelectionChangedEventArgs e) {
			var items = e.AddedItems;
			if (items.Count == 0)
				return;
			var item = (MissionComboBoxItem)items[0];
			viewModel.FilterMissionID = item.ID;
			viewModel.Update();
			viewModel.UpdateHistory();
		}

		private void DataGridCell_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
			var cell = (DataGridCell)sender;
			var item = (HistoryViewModel)cell.DataContext;
		}
	}
}
