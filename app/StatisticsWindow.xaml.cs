﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
		public string RTA { get; set; }
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

		public bool MissionColumnEnabled {
			get => _missionColumnEnabled;
			set {
				_missionColumnEnabled = value;
				UpdateProperty(nameof(MissionColumnEnabled));
			}
		}

		public bool EntranceColumnEnabled {
			get => _entranceColumnEnabled;
			set {
				_entranceColumnEnabled = value;
				UpdateProperty(nameof(EntranceColumnEnabled));
			}
		}

		public bool IGTColumnEnabled {
			get => _igtColumnEnabled;
			set {
				_igtColumnEnabled = value;
				UpdateProperty(nameof(IGTColumnEnabled));
			}
		}

		public bool RTAColumnEnabled {
			get => _rtaColumnEnabled;
			set {
				_rtaColumnEnabled = value;
				UpdateProperty(nameof(RTAColumnEnabled));
			}
		}

		public bool CommentColumnEnabled {
			get => _commentColumnEnabled;
			set {
				_commentColumnEnabled = value;
				UpdateProperty(nameof(CommentColumnEnabled));
			}
		}

		public Visibility MissionColumnVisibility {
			get {
				return FilterMissionID != MissionID.NONE ? Visibility.Visible : Visibility.Hidden;
			}
		}

		public MissionID FilterMissionID {
			get => _filterMissionID;
			set {
				_filterMissionID = value;
				UpdateProperty(nameof(FilterMissionID));
				UpdateProperty(nameof(Title));
				UpdateProperty(nameof(MissionColumnVisibility));
			}
		}

		private MissionID _filterMissionID = MissionID.NONE;
		private bool _missionColumnEnabled = true;
		private bool _entranceColumnEnabled = true;
		private bool _igtColumnEnabled = true;
		private bool _rtaColumnEnabled = true;
		private bool _commentColumnEnabled = true;

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
						IGT = this.FormatSecondsTime(c.IGT),
						RTA = this.FormatSecondsTime(c.RTA),
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

			var averageBestIGT = FormatSecondsTime(Config.Default.Stats.GetAverageBestIGT());

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
				fastestIGT = FormatSecondsTime(fastestIGTSpinStats.GetFastestIGTCompletion().IGT);
				fastestIGTMission = Mission.GetMissionName(fastestIGTSpinStats.Mission);
				fastestIGTSpin = fastestIGTSpinStats.Spin;
			}

			var longestIGT = "N/A";
			var longestIGTMission = "N/A";
			var longestIGTSpin = "N/A";

			var longestIGTSpinStats = Config.Default.Stats.GetSlowestIGTSpinStats();
			if (longestIGTSpinStats != null) {
				longestIGT = FormatSecondsTime(longestIGTSpinStats.GetFastestIGTCompletion().IGT);
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

			var averageBestIGT = FormatSecondsTime(Config.Default.Stats.GetAverageBestIGT(mission));

			MainStats.Add(new() {
				Name = "Avg IGT",
				Description = "The time averaged from your fastest completions of every spin in in-game time.",
				Value = averageBestIGT,
			});

			var fastestIGT = "N/A";
			var fastestIGTSpin = "N/A";

			var fastestIGTSpinStats = Config.Default.Stats.GetFastestIGTSpinStats(mission);
			if (fastestIGTSpinStats != null) {
				fastestIGT = FormatSecondsTime(fastestIGTSpinStats.GetFastestIGTCompletion().IGT);
				fastestIGTSpin = fastestIGTSpinStats.Spin;
			}

			var longestIGT = "N/A";
			var longestIGTSpin = "N/A";

			var longestIGTSpinStats = Config.Default.Stats.GetSlowestIGTSpinStats(mission);
			if (longestIGTSpinStats != null) {
				longestIGT = FormatSecondsTime(longestIGTSpinStats.GetFastestIGTCompletion().IGT);
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

			var averageBestRTA = FormatSecondsTime(Config.Default.Stats.GetAverageBestIGT(mission));

			MainStats.Add(new() {
				Name = "Avg RTA",
				Description = "The time averaged from your fastest completions of every spin in real time.",
				Value = averageBestRTA,
			});

			var fastestRTA = "N/A";
			var fastestRTASpin = "N/A";

			var fastestRTASpinStats = Config.Default.Stats.GetFastestRTASpinStats(mission);
			if (fastestRTASpinStats != null) {
				fastestRTA = FormatSecondsTime(fastestRTASpinStats.GetFastestRTACompletion().RTA);
				fastestRTASpin = fastestRTASpinStats.Spin;
			}

			MainStats.Add(new() {
				Name = "Fastest RTA",
				Description = "Fastest real time achieved on a spin.",
				Value = fastestRTA,
			});
			MainStats.Add(new() {
				Name = "Fastest RTA Spin",
				Description = "The spin you've completed with the fastest real time.",
				Value = fastestRTASpin,
			});

			var longestRTA = "N/A";
			var longestRTASpin = "N/A";

			var longestRTASpinStats = Config.Default.Stats.GetSlowestRTASpinStats(mission);
			if (longestRTASpinStats != null) {
				longestRTA = FormatSecondsTime(longestRTASpinStats.GetFastestRTACompletion().RTA);
				longestRTASpin = longestRTASpinStats.Spin;
			}

			MainStats.Add(new() {
				Name = "Longest RTA",
				Description = "Longest real time taken on a spin.",
				Value = longestRTA,
			});
			MainStats.Add(new() {
				Name = "Longest RTA Spin",
				Description = "The spin you spent the most real time completing.",
				Value = longestRTASpin,
			});
		}

		private string FormatSecondsTime(double time, bool allowFrac = true) {
			return TimeFormatter.FormatSecondsTime(time, allowFrac);
		}
	}
	
	public partial class StatisticsWindow : Window {
		private readonly StatisticsWindowViewModel viewModel = new();

		public StatisticsWindow() {
			DataContext = viewModel;
			InitializeComponent();
			UpdateColumnVisibilities();
			viewModel.PropertyChanged += ViewModel_PropertyChanged;
		}

		private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e) {
			switch (e.PropertyName) {
				case nameof(StatisticsWindowViewModel.MissionColumnEnabled):
				case nameof(StatisticsWindowViewModel.EntranceColumnEnabled):
				case nameof(StatisticsWindowViewModel.IGTColumnEnabled):
				case nameof(StatisticsWindowViewModel.RTAColumnEnabled):
				case nameof(StatisticsWindowViewModel.CommentColumnEnabled):
					UpdateColumnVisibilities();
					break;
			};
		}

		private void UpdateColumnVisibilities() {
			ToggleHistoryTableColumnVisibility(0, viewModel.MissionColumnEnabled);
			ToggleHistoryTableColumnVisibility(2, viewModel.EntranceColumnEnabled);
			ToggleHistoryTableColumnVisibility(3, viewModel.IGTColumnEnabled);
			ToggleHistoryTableColumnVisibility(4, viewModel.RTAColumnEnabled);
			ToggleHistoryTableColumnVisibility(5, viewModel.CommentColumnEnabled);
		}

		private void ToggleHistoryTableColumnVisibility(int idx, bool enable) {
			HistoryTable.Columns[idx].Width = enable ? DataGridLength.Auto : 0;
			HistoryTable.Columns[idx].MaxWidth = enable ? double.PositiveInfinity : 0;
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

		private void HistoryTable_PreviewMouseWheel(object sender, MouseWheelEventArgs e) {
			//what we're doing here, is that we're invoking the "MouseWheel" event of the parent ScrollViewer.

			//first, we make the object with the event arguments (using the values from the current event)
			var args = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);

			//then we need to set the event that we're invoking.
			//the ScrollViewer control internally does the scrolling on MouseWheelEvent, so that's what we're going to use:
			args.RoutedEvent = ScrollViewer.MouseWheelEvent;

			//and finally, we raise the event on the parent ScrollViewer.
			WindowScrollViewer.RaiseEvent(args);
		}
	}
}
