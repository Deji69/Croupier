using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace Croupier
{
	public class TargetNameFormatEntry(TargetNameFormat id, string name) : INotifyPropertyChanged {
		public TargetNameFormat ID { get; set; } = id;
		public int Index { get { return (int)ID; } }
		public string Name { get; set; } = name;
		public bool IsSelected {
			get {
				return TargetNameFormatMethods.FromString(Config.Default.TargetNameFormat) == ID;
			}
			set {
				Config.Default.TargetNameFormat = value.ToString();
				OnPropertyChanged(nameof(IsSelected));
			}
		}

		public void Refresh() {
			OnPropertyChanged(nameof(IsSelected));
			OnPropertyChanged(nameof(Name));
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName) {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}

	public class SpinHistoryEntry(string name, int index) : INotifyPropertyChanged
	{
		private string _name = name;
		private bool isSelected = false;
		private int _index = index;

		public string Name {
			get { return _name; }
			set {
				_name = value;
				OnPropertyChanged(nameof(Name));
			}
		}

		public int Index {
			get { return _index; }
			set {
				_index = value;
				OnPropertyChanged(nameof(Index));
			}
		}
		public bool IsSelected {
			get { return isSelected; }
			set {
				isSelected = value;
				OnPropertyChanged(nameof(IsSelected));
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName) {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}

	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window, INotifyPropertyChanged {

		private const int MAX_HISTORY_ENTRIES = 30;
		public event PropertyChangedEventHandler PropertyChanged;

		public static readonly RoutedUICommand CheckUpdateCommand = new("Check Update", "CheckUpdate", typeof(MainWindow));
		public static readonly RoutedUICommand CheckDailySpinsCommand = new("Check Daily Spins", "CheckDailySpins", typeof(MainWindow));
		public static readonly RoutedUICommand DailySpin1Command = new("Daily Spin 1", "DailySpin1", typeof(MainWindow));
		public static readonly RoutedUICommand DailySpin2Command = new("Daily Spin 2", "DailySpin2", typeof(MainWindow));
		public static readonly RoutedUICommand DailySpin3Command = new("Daily Spin 3", "DailySpin3", typeof(MainWindow));
		public static readonly RoutedUICommand CopySpinCommand = new("Copy Spin", "CopySpin", typeof(MainWindow), [
			new KeyGesture(Key.C, ModifierKeys.Control),
		]);
		public static readonly RoutedUICommand PasteSpinCommand = new("Paste Spin", "PasteSpin", typeof(MainWindow), [
			new KeyGesture(Key.V, ModifierKeys.Control),
		]);
		public static readonly RoutedUICommand EditMapPoolCommand = new("Edit Map Pool", "EditMapPool", typeof(MainWindow), [
			new KeyGesture(Key.M, ModifierKeys.Alt),
		]);
		public static readonly RoutedUICommand ShowHitmapsWindowCommand = new("Show Hitmaps Window", "ShowHitmapsWindow", typeof(MainWindow), [
			new KeyGesture(Key.H, ModifierKeys.Alt),
		]);
		public static readonly RoutedUICommand ShowLiveSplitWindowCommand = new("Show LiveSplit Window", "ShowLiveSplitWindow", typeof(MainWindow), [
			new KeyGesture(Key.L, ModifierKeys.Alt),
		]);
		public static readonly RoutedUICommand DebugWindowCommand = new("Debug Window", "DebugWindow", typeof(MainWindow), [
			new KeyGesture(Key.D, ModifierKeys.Alt),
		]);
		public static readonly RoutedUICommand EditRulesetsCommand = new("Edit Rulesets", "EditRulesets", typeof(MainWindow), [
			new KeyGesture(Key.R, ModifierKeys.Alt),
		]);
		public static readonly RoutedUICommand ShowStatisticsWindowCommand = new("Show Statistics Window", "ShowStatisticsWindow", typeof(MainWindow), [
			new KeyGesture(Key.S, ModifierKeys.Alt),
		]);
		public static readonly RoutedUICommand StreakSettingsCommand = new("Streak Settings", "StreakSettings", typeof(MainWindow), [
			new KeyGesture(Key.T, ModifierKeys.Alt),
		]);
		public static readonly RoutedUICommand ResetStreakCommand = new("Reset Streak", "ResetStreak", typeof(MainWindow), [
			new KeyGesture(Key.S, ModifierKeys.Control),
		]);
		public static readonly RoutedUICommand TimerSettingsCommand = new("Timer Settings", "TimerSettings", typeof(MainWindow), [
			new KeyGesture(Key.T, ModifierKeys.Alt),
		]);
		public static readonly RoutedUICommand EditSpinCommand = new("Edit Spin", "EditSpin", typeof(MainWindow), [
			new KeyGesture(Key.E, ModifierKeys.Alt),
		]);
		public static readonly RoutedUICommand PrevSpinCommand = new("Previous Spin", "PrevSpin", typeof(MainWindow), [
			new KeyGesture(Key.Left, ModifierKeys.Alt),
		]);
		public static readonly RoutedUICommand NextSpinCommand = new("Next Spin", "NextSpin", typeof(MainWindow), [
			new KeyGesture(Key.Right, ModifierKeys.Alt),
		]);
		public static readonly RoutedUICommand ShuffleCommand = new("Shuffle", "Shuffle", typeof(MainWindow), [
			new KeyGesture(Key.Space, ModifierKeys.Control),
		]);
		public static readonly RoutedUICommand RespinCommand = new("Respin", "Respin", typeof(MainWindow), [
			new KeyGesture(Key.R, ModifierKeys.Control),
		]);
		public static readonly RoutedUICommand ResetTimerCommand = new("Reset Timer", "ResetTimer", typeof(MainWindow), [
			new KeyGesture(Key.T, ModifierKeys.Control),
		]);
		public static readonly RoutedUICommand StartTimerCommand = new("Start Timer", "StartTimer", typeof(MainWindow));
		public static readonly RoutedUICommand StopTimerCommand = new("Stop Timer", "StopTimer", typeof(MainWindow));

		private static readonly Random random = new();
		private static readonly List<Spin> spinHistory = [];
		private static int spinHistoryIndex = 1;

		private DebugWindow DebugWindowInst;
		private EditMapPoolWindow EditMapPoolWindowInst;
		private EditRulesetWindow EditRulesetWindowInst;
		private TimerSettingsWindow TimerSettingsWindowInst;
		private StatisticsWindow StatisticsWindowInst;
		private EditSpinWindow EditSpinWindowInst;
		private HitmapsWindow HitmapsWindowInst;
		private LiveSplitWindow LiveSplitWindowInst;
		private TargetNameFormat _targetNameFormat = TargetNameFormat.Initials;
		private DailySpinData dailySpin1 = null;
		private DailySpinData dailySpin2 = null;
		private DailySpinData dailySpin3 = null;
		private double _spinFontSize = 16;
		private bool _rightToLeft = false;
		private bool _topmostEnabled = false;
		private bool _verticalDisplay = false;
		private bool _spinLock = false;
		private bool _editMode = false;
		private bool _staticSize = false;
		private bool _staticSizeLHS = false;
		private bool _killValidations = false;
		private bool _showStreak = false;
		private bool _showStreakPB = false;
		private bool _showTimer = false;
		private bool _timerMultiSpin = false;
		private bool _timerFractions = true;

		public TargetNameFormat TargetNameFormat {
			get { return _targetNameFormat; }
			set {
				_targetNameFormat = value;
				Config.Default.TargetNameFormat = value.ToString();
				Config.Save();
				OnPropertyChanged(nameof(TargetNameFormat));
				SyncHistoryEntries();
				foreach (var entry in TargetNameFormatEntries)
					entry.Refresh();
			}
		}
		public bool AutoUpdateCheck {
			get {
				return Config.Default.CheckUpdate;
			}
			set {
				Config.Default.CheckUpdate = value;
				Config.Save();
				OnPropertyChanged(nameof(AutoUpdateCheck));
			}
		}

		public double SpinFontSize {
			get { return _spinFontSize; }
			private set {
				if (_spinFontSize != value) {
					_spinFontSize = value;
					OnPropertyChanged(nameof(SpinFontSize));
				}
			}
		}

		public bool ShuffleButtonEnabled {
			get {
				return missionPool.Count > 0;
			}
		}

		public bool TopmostEnabled {
			get {
				return _topmostEnabled;
			}
			set {
				_topmostEnabled = value;
				Topmost = value;
				Config.Default.AlwaysOnTop = value;
				Config.Save();
				OnPropertyChanged(nameof(TopmostEnabled));
			}
		}
		public bool VerticalDisplay {
			get { return _verticalDisplay; }
			set {
				_verticalDisplay = value;
				Config.Default.VerticalDisplay = value;
				Config.Save();
				OnPropertyChanged(nameof(VerticalDisplay));
			}
		}
		public bool SpinLock {
			get { return _spinLock; }
			set {
				if (value == _spinLock) return;
				_spinLock = value;
				OnPropertyChanged(nameof(SpinLock));
				SendSpinLockToClient();
			}
		}
		public bool HorizontalSpinDisplay { get; set; } = false;
		public bool RightToLeft {
			get => _rightToLeft;
			set {
				if (value == _rightToLeft) return;
				_rightToLeft = value;
				if (value != Config.Default.RightToLeft) {
					Config.Default.RightToLeft = value;
					Config.Save();
				}
				OnPropertyChanged(nameof(RightToLeft));
				OnPropertyChanged(nameof(RightToLeftFlowDir));
				OnPropertyChanged(nameof(ContentGridFlowDir));
				OnPropertyChanged(nameof(SpinTextAlignment));
				OnPropertyChanged(nameof(StatusAlignLeft));
			}
		}
		public bool StaticSize {
			get => _staticSize;
			set {
				if (value == _staticSize) return;
				_staticSize = value;

				if (value != Config.Default.StaticSize) {
					Config.Default.StaticSize = value;
					Config.Save();
				}

				OnPropertyChanged(nameof(ContentGridAlign));
				OnPropertyChanged(nameof(StaticSize));
				OnPropertyChanged(nameof(SpinAlignHorz));
				OnPropertyChanged(nameof(SpinGridWidth));
				OnPropertyChanged(nameof(SpinGridHeight));
				OnPropertyChanged(nameof(ContentGridFlowDir));
				OnPropertyChanged(nameof(RightToLeftFlowDir));
				OnPropertyChanged(nameof(StatusAlignLeft));
			}
		}
		public bool StaticSizeLHS {
			get => _staticSizeLHS;
			set {
				if (value == _staticSizeLHS) return;
				_staticSizeLHS = value;
				if (value != Config.Default.StaticSizeLHS) {
					Config.Default.StaticSizeLHS = value;
					Config.Save();
				}
				OnPropertyChanged(nameof(ContentGridAlign));
				OnPropertyChanged(nameof(StaticSize));
				OnPropertyChanged(nameof(SpinAlignHorz));
				OnPropertyChanged(nameof(SpinGridWidth));
				OnPropertyChanged(nameof(SpinGridHeight));
				OnPropertyChanged(nameof(ContentGridFlowDir));
				OnPropertyChanged(nameof(RightToLeftFlowDir));
				OnPropertyChanged(nameof(StatusAlignLeft));
			}
		}
		public bool KillValidations {
			get => _killValidations;
			set {
				if (value == _killValidations) return;
				_killValidations = value;
				if (value != Config.Default.KillValidations) {
					Config.Default.KillValidations = value;
					Config.Save();
				}

				OnPropertyChanged(nameof(KillValidations));

				foreach (var cond in conditions)
					cond.ForceUpdate();
			}
		}
		public bool ShowSpinLabels {
			get => !_editMode;
		}
		public bool EditMode {
			get => _editMode;
			set {
				if (value == _editMode) return;
				_editMode = value;
				OnPropertyChanged(nameof(EditMode));
				OnPropertyChanged(nameof(ShowSpinLabels));
			}
		}
		public bool ShowStatusBar {
			get => ShowTimer || ShowStreak;
		}
		public bool ShowTimer {
			get => _showTimer;
			set {
				_showTimer = value;
				if (value != Config.Default.Timer) {
					Config.Default.Timer = value;
					Config.Save();
				}
				OnPropertyChanged(nameof(ShowStatusBar));
				OnPropertyChanged(nameof(ShowTimer));
				RefitWindow();
			}
		}
		public bool ShowStreak {
			get => _showStreak;
			set {
				_showStreak = value;
				if (value != Config.Default.Streak) {
					Config.Default.Streak = value;
					Config.Save();
				}
				OnPropertyChanged(nameof(ShowStatusBar));
				OnPropertyChanged(nameof(ShowStreak));
				RefitWindow();
			}
		}
		public bool ShowStreakPB {
			get => _showStreakPB;
			set {
				_showStreakPB = value;
				if (value != Config.Default.ShowStreakPB) {
					Config.Default.ShowStreakPB = value;
					Config.Save();
				}
				OnPropertyChanged(nameof(ShowStreakPB));
				UpdateStreakStatus();
			}
		}
		public bool TimerMultiSpin {
			get => _timerMultiSpin;
			set {
				_timerMultiSpin = value;
				if (value != Config.Default.TimerMultiSpin) {
					Config.Default.TimerMultiSpin = value;
					Config.Save();
				}
				OnPropertyChanged(nameof(TimerMultiSpin));
			}
		}
		public bool TimerFractions {
			get => _timerFractions;
			set {
				_timerFractions = value;
				if (value != Config.Default.TimerFractions) {
					Config.Default.TimerFractions = value;
					Config.Save();
				}
				OnPropertyChanged(nameof(TimerFractions));
			}
		}
		public FlowDirection RightToLeftFlowDir {
			get {
				if (ContentGridFlowDir == FlowDirection.RightToLeft)
					return RightToLeft ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
				return RightToLeft ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
			}
		}
		public HorizontalAlignment ContentGridAlign {
			get {
				if (!StaticSize)
					return HorizontalAlignment.Stretch;
				return StaticSizeLHS ? HorizontalAlignment.Left : HorizontalAlignment.Right;
			}
		}
		public TextAlignment SpinTextAlignment {
			get {
				return RightToLeft ? TextAlignment.Right : TextAlignment.Left;
			}
		}
		public FlowDirection ContentGridFlowDir {
			get {
				if (!StaticSize)
					return FlowDirection.LeftToRight;
				return StaticSizeLHS ? FlowDirection.LeftToRight : FlowDirection.RightToLeft;
			}
		}
		public HorizontalAlignment SpinAlignHorz {
			get {
				if (!StaticSize) return HorizontalAlignment.Stretch;
				return StaticSizeLHS ? HorizontalAlignment.Left : HorizontalAlignment.Right;
			}
		}
		public bool StatusAlignLeft {
			get {
				return StaticSize ? StaticSizeLHS : !RightToLeft;
			}
		}
		public double SpinGridWidth {
			get {
				if (!StaticSize) return double.NaN;
				return (VerticalDisplay ? Width : Width / 2) - 6;
			}
		}
		public double SpinGridHeight {
			get {
				if (!StaticSize)
					return double.NaN;
				return SpinGridWidth * 0.33;
			}
		}

		public string DailySpin1Label {
			get {
				if (dailySpin1 == null) return "Spin #1";
				if (!SpinParser.Parse(dailySpin1.spin, out var spin)) return "Spin #1";
				var stats = Config.Default.Stats.GetSpinStats(spin);
				var completion = stats.GetFastestIGTCompletion();
				var location = new Mission(spin.Mission).Location;
				if (completion == null) return $"Spin #{dailySpin1.id}: {location}";
				return $"Spin #{dailySpin1.id}: {location} ({TimeFormatter.FormatSecondsTime(completion.IGT)})";
			}
		}

		public string DailySpin1Tooltip {
			get {
				if (dailySpin1 == null) return "";
				return dailySpin1.spin;
			}
		}

		public bool DailySpin1Completed {
			get {
				if (dailySpin1 == null) return false;
				if (!SpinParser.Parse(dailySpin1.spin, out var spin)) return false;
				var stats = Config.Default.Stats.GetSpinStats(spin);
				return stats.Completions.Count > 0;
			}
		}

		public string DailySpin2Label {
			get {
				if (dailySpin2 == null) return "Spin #2";
				if (!SpinParser.Parse(dailySpin2.spin, out var spin)) return "Spin #2";
				var stats = Config.Default.Stats.GetSpinStats(spin);
				var completion = stats.GetFastestIGTCompletion();
				var location = new Mission(spin.Mission).Location;
				if (completion == null) return $"Spin #{dailySpin2.id}: {location}";
				return $"Spin #{dailySpin2.id}: {location} ({TimeFormatter.FormatSecondsTime(completion.IGT)})";
			}
		}

		public string DailySpin2Tooltip {
			get {
				if (dailySpin2 == null) return "";
				return dailySpin2.spin;
			}
		}

		public bool DailySpin2Completed {
			get {
				if (dailySpin2 == null) return false;
				if (!SpinParser.Parse(dailySpin2.spin, out var spin)) return false;
				var stats = Config.Default.Stats.GetSpinStats(spin);
				return stats.Completions.Count > 0;
			}
		}

		public string DailySpin3Label {
			get {
				if (dailySpin3 == null) return "Spin #3";
				if (!SpinParser.Parse(dailySpin3.spin, out var spin)) return "Spin #3";
				var stats = Config.Default.Stats.GetSpinStats(spin);
				var completion = stats.GetFastestIGTCompletion();
				var location = new Mission(spin.Mission).Location;
				if (completion == null) return $"Spin #{dailySpin3.id}: {location}";
				return $"Spin #{dailySpin3.id}: {location} ({TimeFormatter.FormatSecondsTime(completion.IGT)})";
			}
		}

		public string DailySpin3Tooltip {
			get {
				if (dailySpin3 == null) return "";
				return dailySpin3.spin;
			}
		}

		public bool DailySpin3Completed {
			get {
				if (dailySpin3 == null) return false;
				if (!SpinParser.Parse(dailySpin3.spin, out var spin)) return false;
				var stats = Config.Default.Stats.GetSpinStats(spin);
				return stats.Completions.Count > 0;
			}
		}

		public Spin CurrentSpin {
			get => spin;
		}

		public ObservableCollection<SpinHistoryEntry> HistoryEntries = [];
		public ObservableCollection<SpinHistoryEntry> BookmarkEntries = [
			new("<Add Current>", 0),
			new("<Remove Current>", 1),
		];
		public ObservableCollection<TargetNameFormatEntry> TargetNameFormatEntries = [
			new(TargetNameFormat.Initials, "Initials"),
			new(TargetNameFormat.Full, "Full Name"),
			new(TargetNameFormat.Short, "Short Name"),
		];

		private readonly List<SpinCondition> conditions = [];
		private readonly List<Mission> missions = [
			new Mission(MissionID.ICAFACILITY_FREEFORM),
			new Mission(MissionID.ICAFACILITY_FINALTEST),
			new Mission(MissionID.PARIS_SHOWSTOPPER),
			new Mission(MissionID.SAPIENZA_WORLDOFTOMORROW),
			new Mission(MissionID.MARRAKESH_GILDEDCAGE),
			new Mission(MissionID.BANGKOK_CLUB27),
			new Mission(MissionID.COLORADO_FREEDOMFIGHTERS),
			new Mission(MissionID.HOKKAIDO_SITUSINVERSUS),
			new Mission(MissionID.BANGKOK_THESOURCE),
			new Mission(MissionID.SAPIENZA_THEAUTHOR),
			new Mission(MissionID.HOKKAIDO_PATIENTZERO),
			new Mission(MissionID.PARIS_HOLIDAYHOARDERS),
			new Mission(MissionID.SAPIENZA_THEICON),
			new Mission(MissionID.SAPIENZA_LANDSLIDE),
			new Mission(MissionID.MARRAKESH_HOUSEBUILTONSAND),
			new Mission(MissionID.HOKKAIDO_SNOWFESTIVAL),
			new Mission(MissionID.HAWKESBAY_NIGHTCALL),
			new Mission(MissionID.MIAMI_FINISHLINE),
			new Mission(MissionID.SANTAFORTUNA_THREEHEADEDSERPENT),
			new Mission(MissionID.MUMBAI_CHASINGAGHOST),
			new Mission(MissionID.WHITTLETON_ANOTHERLIFE),
			new Mission(MissionID.ISLEOFSGAIL_THEARKSOCIETY),
			new Mission(MissionID.NEWYORK_GOLDENHANDSHAKE),
			new Mission(MissionID.HAVEN_THELASTRESORT),
			new Mission(MissionID.MIAMI_ASILVERTONGUE),
			new Mission(MissionID.SANTAFORTUNA_EMBRACEOFTHESERPENT),
			new Mission(MissionID.MUMBAI_ILLUSIONSOFGRANDEUR),
			new Mission(MissionID.WHITTLETON_ABITTERPILL),
			new Mission(MissionID.DUBAI_ONTOPOFTHEWORLD),
			new Mission(MissionID.DARTMOOR_DEATHINTHEFAMILY),
			new Mission(MissionID.BERLIN_APEXPREDATOR),
			new Mission(MissionID.CHONGQING_ENDOFANERA),
			new Mission(MissionID.MENDOZA_THEFAREWELL),
			new Mission(MissionID.CARPATHIAN_UNTOUCHABLE),
			new Mission(MissionID.AMBROSE_SHADOWSINTHEWATER),
		];
		private readonly ObservableCollection<Ruleset> rulesets = [];

		private readonly List<MissionID> missionPool = [];
		private Mission currentMission = null;
		private Entrance usedEntrance = null;
		private string[] loadout = [];
		private bool disableClientUpdate = false;
		private bool timerStopped = true;
		private bool timerManuallyStopped = false;
		private DateTime timerStart = DateTime.Now;
		private TimeSpan? timeElapsed = null;
		private int streak = 0;
		private Spin spin = null;
		private bool spinCompleted = false;
		private bool hasRestartedSinceSpin = false;
		private List<TargetID> trackedValidKills = [];
		private DateTime? autoSpinSchedule = null;
		private MissionID autoSpinMission = MissionID.NONE;
		private readonly LiveSplitClient liveSplit;
		private readonly DispatcherTimer timer = null;

		private ObservableCollection<MissionComboBoxItem> MissionListItems {
			get {
				var items = new ObservableCollection<MissionComboBoxItem>();
				var group = MissionGroup.None;
				missions.ForEach(mission => {
					if (mission.Group != group) {
						items.Add(new() {
							Name = mission.Group.GetName(),
							IsSeparator = true,
						});
						group = mission.Group;
					}
					items.Add(new() {
						ID = mission.ID,
						Name = mission.Name,
						Image = mission.ImagePath,
						Location = mission.Location,
						IsSeparator = false
					});
				});
				return items;
			}
		}

		public MainWindow() {
			liveSplit = ((App)Application.Current).LiveSplitClient;
			DataContext = this;
			InitializeComponent();
			MainContextMenu.DataContext = this;
			SizeToContent = SizeToContent.Height;
			((App)Application.Current).WindowPlace.Register(this);
			Focus();

			var logoVer = Version.Parse(Assembly.GetExecutingAssembly().GetName().Version.ToString());
			var logoVerStr = logoVer.Major + "." + logoVer.Minor;
			if (logoVer.Build != 0)
				logoVerStr += "." + logoVer.Build;
			Logo.ToolTip = "Croupier v" + logoVerStr;

			if (Config.Default.SpinHistory.Count > 0) {
				string[] history = new string[Config.Default.SpinHistory.Count];
				Config.Default.SpinHistory.CopyTo(history, 0);

				foreach (var item in history.Reverse()) {
					if (SpinParser.Parse(item, out var spin))
						PushSpinToHistory(spin);
				}
			}

			LoadSettings();

			if (spinHistory.Count > 0)
				SetSpinHistory(1);
			else {
				SetMission(missions[0].ID);
				Spin(currentMission.ID);
			}

			PropertyChanged += MainWindow_PropertyChanged;

			timer = new DispatcherTimer {
				Interval = TimeSpan.FromMilliseconds(1)
			};
			timer.Tick += (object sender, EventArgs e) => {
				var diff = timeElapsed ?? DateTime.Now - timerStart;

				if (autoSpinSchedule.HasValue) {
					Timer.Text = (DateTime.Now - autoSpinSchedule.Value).ToString(@"\-s");
					if (DateTime.Now > autoSpinSchedule.Value) {
						autoSpinSchedule = null;
						AutoSpin(autoSpinMission);
					}
				}
				else if (timerStopped && !timeElapsed.HasValue)
					Timer.Text = TimerFractions ? "00:00.00" : "00:00";
				else if (diff.TotalHours > 1)
					Timer.Text = diff.ToString(TimerFractions ? @"hh\:mm\:ss\.ff" : @"hh\:mm\:ss");
				else// if (diff.TotalMinutes > 1)
					Timer.Text = diff.ToString(TimerFractions ? @"mm\:ss\.ff" : @"mm\:ss");
				//else
				//	Timer.Text = diff.ToString(@"ss\.ff");
			};
			timer.Start();

			CroupierSocketServer.Connected += (object sender, int _) => {
				SendMissionsToClient();
				SendSpinToClient();
				SendSpinLockToClient();
				SendStreakToClient();
				SendTimerToClient();
			};
			CroupierSocketServer.Respin += (object sender, MissionID id) => Spin(id);
			CroupierSocketServer.AutoSpin += (object sender, MissionID id) => {
				if (SpinLock) return;
				if (Config.Default.AutoSpinCountdown > 0) {
					autoSpinSchedule = DateTime.Now + TimeSpan.FromSeconds(Config.Default.AutoSpinCountdown);
					autoSpinMission = id;
				}
				else AutoSpin(id);
			};
			CroupierSocketServer.Random += (object sender, int _) => Shuffle();
			CroupierSocketServer.ToggleSpinLock += (object sender, int _) => {
				SpinLock = !SpinLock;
			};
			CroupierSocketServer.ToggleTimer += (object sender, bool enable) => {
				ShowTimer = enable;
			};
			CroupierSocketServer.PauseTimer += (object sender, bool pause) => {
				if (pause) StopTimer();
				else ResumeTimer();
				timerManuallyStopped = pause;
			};
			CroupierSocketServer.ResetTimer += (object sender, int _) => {
				StopTimer();
				ResetTimer();
			};
			CroupierSocketServer.ResetStreak += (object sender, int _) => {
				ResetStreak();
			};
			CroupierSocketServer.MissionStart += (object sender, MissionStart start) => {
				TrackGameMissionAttempt(start);
			};
			CroupierSocketServer.MissionComplete += (object sender, MissionCompletion arg) => {
				spinCompleted = true;
				arg.KillsValidated = CheckSpinKillsValid();
				if (arg.SA && (arg.KillsValidated || Config.Default.StreakRequireValidKills == false)) IncrementStreak();
				else ResetStreak();
				liveSplit.Split();
				StopTimer();
				TrackGameMissionCompletion(arg);
			};
			CroupierSocketServer.MissionFailed += (object sender, int _) => {
				if (spinCompleted) return;

				if (hasRestartedSinceSpin || (DateTime.Now - timerStart).TotalSeconds > Config.Default.StreakReplanWindow)
					ResetStreak();

				hasRestartedSinceSpin = true;
			};
			CroupierSocketServer.Missions += (object sender, List<MissionID> missions) => {
				missionPool.Clear();
				missionPool.AddRange(missions);
				EditMapPoolWindowInst?.UpdateMissionPool(missionPool);
			};
			CroupierSocketServer.Prev += (object sender, int _) => {
				disableClientUpdate = true;
				PreviousSpin();
				disableClientUpdate = false;
				SendSpinToClient();
			};
			CroupierSocketServer.Next += (object sender, int _) => {
				disableClientUpdate = true;
				NextSpin();
				disableClientUpdate = false;
				SendSpinToClient();
			};
			CroupierSocketServer.SpinData += (object sender, string data) => {
				if (SpinParser.Parse(data, out var spin)) {
					disableClientUpdate = true;
					SetSpin(spin);
					UpdateSpinHistory();
					PostConditionUpdate();
					ResetCurrentSpinProgress();
					StartTimer();
					disableClientUpdate = false;
				}
			};
			CroupierSocketServer.LoadStarted += (object sender, int _) => {
				StopTimer();
			};
			CroupierSocketServer.LoadFinished += (object sender, int _) => {
				if (!spinCompleted) ResumeTimer();
				SendTimerToClient();
			};
			CroupierSocketServer.KillValidation += (object sender, string data) => {
				if (data.Length == 0) return;

				var firstLine = data.Split("\n")[0];
				var validationStrings = firstLine.Split(",");

				foreach (var v in validationStrings) {
					var segments = v.Split(":");
					if (segments.Length != 4) return;
					var kv = new KillValidation {
						target = (TargetID)int.Parse(segments[0]),
						killValidation = (KillValidationType)int.Parse(segments[1]),
						disguiseValidation = int.Parse(segments[2]) != 0,
						specificTarget = (TargetID)int.Parse(segments[3]),
					};
					for (var i = 0; i < conditions.Count; ++i) {
						var cond = conditions[i];
						if (cond.Target.ID != kv.target) continue;
						cond.KillValidation = kv;
					}
				}

				TrackKillValidation();
			};
			HitmapsSpinLink.ReceiveNewSpinData += (object sender, string data) => {
				if (SpinParser.Parse(data, out var spin)) {
					SetSpin(spin);
					UpdateSpinHistory();
					PostConditionUpdate();
					ResetCurrentSpinProgress();
					StartTimer(true);
					TrackNewSpin();
				}
			};

			SendMissionsToClient();
			SendSpinToClient();
		}

		private void MainWindow_PropertyChanged(object sender, PropertyChangedEventArgs e) {
			if (e.PropertyName == nameof(VerticalDisplay)
				|| e.PropertyName == nameof(StaticSize)) {
				RefitWindow();
				Task.Delay(10).ContinueWith(task => RefitWindow(), TaskScheduler.FromCurrentSynchronizationContext());
				return;
			}
		}

		private void LoadRulesetConfiguration() {
			var files = Directory.GetFiles("rulesets", "*.json");
			
			rulesets.Clear();

			foreach (var file in files.Reverse()) {
				var ruleset = Ruleset.LoadConfig(file);
				if (rulesets.Any(r => r.Name == ruleset.Name))
					continue;
				if (ruleset.Name == "Custom")
					rulesets.Insert(0, ruleset);
				else
					rulesets.Add(ruleset);
			}

			var custom = rulesets.FirstOrDefault(r => r.Name == "Custom");
			var fallbackDefaultRuleset = rulesets.FirstOrDefault(r => r.Name == "RRWC2024");

			if (custom == null) {
				custom = new("Custom", new() {
					AllowDuplicateMethod = false,
					AllowDuplicateDisguise = Config.Default.Ruleset_AllowDuplicateDisguises,
					AnyDisguise = Config.Default.Ruleset_EnableAnyDisguise,
					AnyExplosives = Config.Default.Ruleset_AnyExplosiveKillTypes,
					Banned = ["Slow", "Hard", "Extreme", "Impossible", "Buggy", "EasterEgg"],
					GenericEliminations = Config.Default.Ruleset_GenericEliminations,
					ImpactExplosives = Config.Default.Ruleset_ImpactExplosiveKillTypes,
					LiveComplications = Config.Default.Ruleset_LiveComplications,
					LiveComplicationChance = Config.Default.Ruleset_LiveComplicationChance,
					LiveComplicationsExcludeStandard = Config.Default.Ruleset_LiveComplicationsExcludeStandard,
					LoudRemoteExplosives = Config.Default.Ruleset_LoudRemoteExplosiveKillTypes,
					LoudSMGIsLargeFirearm = Config.Default.Ruleset_LoudSMGIsLargeFirearm,
					MeleeKillTypes = Config.Default.Ruleset_MeleeKillTypes,
					RemoteExplosives = Config.Default.Ruleset_RemoteExplosiveKillTypes,
					SuitOnly = Config.Default.Ruleset_SuitOnlyMode,
					ThrownKillTypes = Config.Default.Ruleset_ThrownKillTypes,
				}, fallbackDefaultRuleset?.Tags);
				custom.Save();
				rulesets.Add(custom);
			}
			//new("RR14", RulesetPreset.RR14),
			//new("RRWC 2023", RulesetPreset.RRWC2023),
			//new("RR12", RulesetPreset.RR12),
			//new("RR11", RulesetPreset.RR11),
			//new("Croupier", RulesetPreset.Croupier),
			//new("Custom", RulesetPreset.Custom),

			var newRuleset = rulesets.FirstOrDefault(r => r.Name == Config.Default.Ruleset);
			if (newRuleset != null)
				Ruleset.Current = newRuleset;
			else if (rulesets.Count > 0) {
				Ruleset.Current = rulesets.First();
				Config.Default.Ruleset = Ruleset.Current?.Name ?? "Custom";
			}
			else
				Config.Default.Ruleset = Ruleset.Current?.Name ?? "Custom";
		}

		private void LoadSettings() {
			if (!Enum.IsDefined(typeof(MissionPoolPresetID), Config.Default.MissionPool))
				Config.Default.MissionPool = MissionPoolPresetID.MainMissions;

			LoadRulesetConfiguration();

			VerticalDisplay = Config.Default.VerticalDisplay;
			TopmostEnabled = Config.Default.AlwaysOnTop;
			RightToLeft = Config.Default.RightToLeft;
			StaticSize = Config.Default.StaticSize;
			StaticSizeLHS = Config.Default.StaticSizeLHS;
			ShowTimer = Config.Default.Timer;
			ShowStreak = Config.Default.Streak;
			ShowStreakPB = Config.Default.ShowStreakPB;
			streak = Config.Default.StreakCurrent;
			TimerMultiSpin = Config.Default.TimerMultiSpin;
			TimerFractions = Config.Default.TimerFractions;
			KillValidations = Config.Default.KillValidations;
			TargetNameFormat = TargetNameFormatMethods.FromString(Config.Default.TargetNameFormat);
			LoadMissionPool();
			UpdateStreakStatus();

			foreach (var bookmark in Config.Default.Bookmarks) {
				BookmarkEntries.Add(new(bookmark, BookmarkEntries.Count));
			}
		}

		private bool CheckSpinKillsValid() {
			foreach (var cond in spin.Conditions) {
				if (!cond.KillValidation.IsValid) return false;
			}
			return true;
		}

		private void EditSpinWindow_SetCondition(object _sender, SpinCondition condition) {
			if (currentMission.ID != condition.Target.Mission) return;
			for (var i = 0; i < conditions.Count; i++) {
				if (conditions[i].Target != condition.Target) continue;
				conditions[i] = condition;
				break;
			}

			UpdateSpinHistory();
			PostConditionUpdate();
		}

		private void EditMapPoolWindow_AddMissionToPool(object sender, MissionID e) {
			if (missionPool.Contains(e)) return;
			missionPool.Add(e);
			SendMissionsToClient();
			OnPropertyChanged(nameof(ShuffleButtonEnabled));
			SaveCustomMissionPool();
		}

		private void EditMapPoolWindow_RemoveMissionFromPool(object sender, MissionID e) {
			missionPool.Remove(e);
			SendMissionsToClient();
			OnPropertyChanged(nameof(ShuffleButtonEnabled));
			SaveCustomMissionPool();
		}

		private void SaveCustomMissionPool() {
			if (Config.Default.MissionPool != MissionPoolPresetID.Custom)
				return;
			
			Config.Default.CustomMissionPool ??= [];
			Config.Default.CustomMissionPool.Clear();

			foreach (var mission in missionPool) {
				if (!Mission.GetMissionCodename(mission, out var name))
					continue;
				Config.Default.CustomMissionPool.Add(name);
			}
		}

		private void LoadMissionPool()
		{
			missionPool.Clear();
			missionPool.AddRange(Config.Default.MissionPool.GetMissions());
		}

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			ThemeManager.SetCurrentTheme(this, new Uri("/Croupier;component/Resources/DarkTheme.xaml", UriKind.Relative));

			MissionSelect.ItemsSource = MissionListItems;
			ContextMenuTargetNameFormat.ItemsSource = TargetNameFormatEntries;
			ContextMenuTargetNameFormat.DataContext = this;
			ContextMenuHistory.ItemsSource = HistoryEntries;
			ContextMenuHistory.DataContext = this;
			ContextMenuBookmarks.ItemsSource = BookmarkEntries;
			ContextMenuBookmarks.DataContext = this;

			var idx = MissionListItems.ToList().FindIndex(item => item.ID == currentMission.ID);
			MissionSelect.SelectedIndex = idx;

			if (Config.Default.CheckUpdate) {
				DoUpdateCheck();
				CheckDailySpinsAsync();
			}
		}

		public void AutoSpin(MissionID id = MissionID.NONE) {
			if (id != MissionID.NONE && currentMission != null && currentMission.ID == id && !spinCompleted)
				return;

			Spin(id);
		}

		public void Spin(MissionID id = MissionID.NONE) {
			if (id == MissionID.NONE) id = currentMission.ID;
			if (!SetMission(id)) return;

			if (!spinCompleted)
				ResetStreak();

			autoSpinSchedule = null;

			Generator gen = new(Ruleset.Current, currentMission);
			spin = gen.GenerateSpin();
			SetSpin(spin);
			spinHistoryIndex = 1;
			PushCurrentSpinToHistory();
			PostConditionUpdate();

			if (!TimerMultiSpin || Config.Default.TimerResetMission == id) {
				ResetTimer();
				StopTimer();
			}

			ResetCurrentSpinProgress();
			StartTimer();

			Config.Default.SpinIsRandom = true;
			Config.Save();

			TrackNewSpin();
		}

		public void ResetCurrentSpinProgress() {
			spinCompleted = false;
			hasRestartedSinceSpin = false;
		}

		public bool SetMission(MissionID id) {
			currentMission = missions.Find(m => m.ID == id);
			if (currentMission == null) return false;
			var idx = MissionListItems.ToList().FindIndex(item => item.ID == id);
			MissionSelect.SelectedIndex = idx;
			return idx != -1;
		}

		public bool PushSpinToHistory(Spin spin) {
			if (spin == null || spin.Conditions.Count == 0) return false;
			if (spin.Conditions == conditions) return false;
			if (spinHistory.Count > 0 && spinHistory[^1].ToString() == spin.ToString())
				return true;

			spinHistory.Add(spin);
			if (HistoryEntries.Count < MAX_HISTORY_ENTRIES)
				HistoryEntries.Insert(0, new(spin.ToString(), spinHistory.Count - 1));
			while (spinHistory.Count > MAX_HISTORY_ENTRIES)
				spinHistory.RemoveAt(0);
			SyncHistoryEntries();
			return true;
		}

		public bool UpdateSpinHistory()
		{
			if (spinHistory.Count == 0) return false;
			var spin = spinHistory[^1];
			spin.Conditions.Clear();
			spin.Conditions.AddRange(conditions);
			SyncHistoryEntries();
			return true;
		}

		public bool PushCurrentSpinToHistory() {
			return PushSpinToHistory(new Spin([..conditions]));
		}

		public void PreviousSpin() {
			if (spinHistoryIndex < 1) spinHistoryIndex = 1;
			if (spinHistoryIndex >= spinHistory.Count) {
				spinHistoryIndex = spinHistory.Count;
				return;
			}

			SetSpinHistory(spinHistoryIndex + 1);
			ResetCurrentSpinProgress();
			ResetTimer();
		}

		public void NextSpin() {
			if (spinHistoryIndex > spinHistory.Count)
				spinHistoryIndex = spinHistory.Count;
			if (spinHistoryIndex <= 1) {
				spinHistoryIndex = 1;
				return;
			}

			SetSpinHistory(spinHistoryIndex - 1);
			ResetCurrentSpinProgress();
			ResetTimer();
		}

		public void SetSpinHistory(int idx) {
			spinHistoryIndex = idx;
			SetSpin(spinHistory[^spinHistoryIndex]);
			SyncHistoryEntries();
			SendSpinToClient();
		}

		public void SyncHistoryEntries() {
			var fwdIndex = spinHistory.Count - spinHistoryIndex;

			Config.Default.SpinHistory.Clear();

			for (var i = 0; i < spinHistory.Count && i < HistoryEntries.Count; ++i) {
				var absIdx = spinHistory.Count - i - 1;
				var entry = HistoryEntries[i];
				entry.Name = spinHistory[absIdx].ToString();
				entry.Index = absIdx;
				entry.IsSelected = absIdx == fwdIndex;
				Config.Default.SpinHistory.Add(entry.Name);
			}

			Config.Save();
			OnPropertyChanged(nameof(HistoryEntries));
		}

		public void SyncBookmarks()
		{
			Config.Default.Bookmarks.Clear();
			
			for (var i = 2; i < BookmarkEntries.Count; ++i) {
				Config.Default.Bookmarks.Add(BookmarkEntries[i].Name);
			}

			Config.Save();
		}

		public void SetSpin(Spin spin) {
			if (spin == null) return;
			conditions.Clear();
			conditions.AddRange(spin.Conditions);
			this.spin = spin;
			SetMission(spin.Mission);
			EditSpinWindowInst?.UpdateConditions(conditions);
			PostConditionUpdate();
		}

		private void PostConditionUpdate() {
			Config.Default.SpinIsRandom = false;
			Config.Save();
			SendSpinToClient();
			SetupSpinUI();
			RefitWindow();
		}

		private void UpdateStreakStatus() {
			SendStreakToClient();

			Config.Default.StreakCurrent = streak;

			if (streak > Config.Default.StreakPB) {
				Config.Default.StreakPB = streak;
				if (Config.Default.StreakPB > Config.Default.Stats.TopStreak)
					Config.Default.Stats.TopStreak = Config.Default.StreakPB;
			}

			if (Config.Default.ShowStreakPB)
				Streak.Text = "Streak: " + streak + " (PB: " + Config.Default.StreakPB + ")";
			else
				Streak.Text = "Streak: " + streak;

			Config.Save();
		}

		private void IncrementStreak() {
			++streak;
			UpdateStreakStatus();
		}

		private void ResetStreak() {
			streak = 0;
			UpdateStreakStatus();
		}

		private void TrackNewSpin() {
			var stats = Config.Default.Stats;
			var spinStats = stats.GetSpinStats(spin);
			var missionStats = stats.GetMissionStats(spin.Mission);
			spinStats.IsCustom = Config.Default.SpinIsRandom;

			var spinStr = spin.ToString();
			if (Config.Default.SpinIsRandom)
				++stats.NumRandomSpins;

			if (spinStats.DailyID == 0) {
				if (dailySpin1 != null && spinStr == dailySpin1.spin)
					spinStats.DailyID = dailySpin1.id;
				else if (dailySpin2 != null && spinStr == dailySpin2.spin)
					spinStats.DailyID = dailySpin2.id;
				else if (dailySpin3 != null && spinStr == dailySpin3.spin)
					spinStats.DailyID = dailySpin3.id;

				if (spinStats.DailyID != 0)
					++stats.NumDailySpins;
				
				++missionStats.NumSpins;
			}

			loadout = [];
			usedEntrance = null;
			trackedValidKills = [];

			Config.Save();
		}

		private void TrackKillValidation() {
			var stats = Config.Default.Stats;

			foreach (var cond in spin.Conditions) {
				if (!cond.KillValidation.IsValid)
					continue;
				if (trackedValidKills.Contains(cond.Target.ID))
					continue;

				trackedValidKills.Add(cond.Target.ID);
				++stats.NumValidKills;
			}

			Config.Save();
		}

		private void TrackGameMissionAttempt(MissionStart start) {
			var stats = Config.Default.Stats;
			var locationId = start.Location.ToLower();
			loadout = start.Loadout;
			usedEntrance = Locations.Entrances.Find(e => e.ID == locationId);

			var spinStats = stats.GetSpinStats(spin);
			var missionStats = stats.GetMissionStats(spin.Mission);

			spinStats.IsCustom = !Config.Default.SpinIsRandom;
			if (spinStats.IsCustom) {
				++stats.NumCustomSpins;
				++missionStats.NumSpins;
			}
			
			++missionStats.NumAttempts;

			if (spinStats.Attempts == 0)
				++stats.NumUniqueAttempts;

			++spinStats.Attempts;
			++stats.NumAttempts;

			Config.Save();
		}

		private void TrackGameMissionCompletion(MissionCompletion mc) {
			var stats = Config.Default.Stats;
			if (!mc.SA) {
				if (mc.KillsValidated)
					++stats.NumNonSAWins;
				return;
			}

			if (!mc.KillsValidated)
				return;

			var spinStats = stats.GetSpinStats(spin);
			var missionStats = stats.GetMissionStats(spin.Mission);
			++stats.NumWins;
			++missionStats.NumWins;

			if (spinStats.Completions.Count == 0)
				++stats.NumUniqueWins;

			spinStats.Completions.Add(new() {
				IGT = mc.IGT > 0 ? mc.IGT : 0,
				RTA = (DateTime.Now - timerStart).TotalSeconds,
				Mission = spin.Mission,
				StartLocation = usedEntrance?.ID ?? "",
				Loadout = [..loadout],
			});

			Config.Save();

			OnPropertyChanged(nameof(DailySpin1Completed));
			OnPropertyChanged(nameof(DailySpin2Completed));
			OnPropertyChanged(nameof(DailySpin3Completed));

			OnPropertyChanged(nameof(DailySpin1Label));
			OnPropertyChanged(nameof(DailySpin2Label));
			OnPropertyChanged(nameof(DailySpin3Label));
		}

		private void StartTimer(bool overrideManual = false) {
			if (!overrideManual && timerManuallyStopped) return;
			else timerManuallyStopped = false;

			if (spinCompleted) liveSplit.StartOrSplit();
			else liveSplit.StartTimer();
			liveSplit.Resume();

			if (TimerMultiSpin && !timerStopped) {
				timeElapsed = DateTime.Now - timerStart;
			}

			timerStopped = false;
			timerStart = timeElapsed.HasValue ? DateTime.Now - timeElapsed.Value : DateTime.Now;
			timeElapsed = null;
			SendTimerToClient();
		}

		private void StopTimer() {
			liveSplit.Pause();

			if (timerStopped) return;
			timeElapsed = DateTime.Now - timerStart;
			timerStopped = true;
			SendTimerToClient();
		}

		private void ResumeTimer() {
			liveSplit.Resume();

			if (!timerStopped) return;
			timerStart = timeElapsed.HasValue ? DateTime.Now - timeElapsed.Value : DateTime.Now;
			timeElapsed = null;
			timerStopped = false;
		}

		private void ResetTimer() {
			liveSplit.Reset();
			if (!timerStopped)
				liveSplit.StartOrSplit();

			timeElapsed = null;
			timerStart = DateTime.Now;
			SendTimerToClient();
		}

		public void SendSpinLockToClient() {
			if (disableClientUpdate) return;
			CroupierSocketServer.Send("SpinLock:" + (SpinLock ? "1" : "0"));
		}

		public void SendTimerToClient() {
			if (disableClientUpdate) return;
			double elapsed = (timeElapsed ?? DateTime.Now - timerStart).TotalMilliseconds;
			CroupierSocketServer.Send($"Timer:{(timerStopped ? 0 : 1)}," + elapsed);
		}

		public void SendSpinToClient() {
			if (disableClientUpdate) return;
			var spinData = "";
			conditions.ForEach(condition => {
				if (spinData.Length > 0) spinData += ", ";
				spinData += $"{condition}";
			});
			CroupierSocketServer.Send("SpinData:" + spinData);
		}

		public void SendMissionsToClient() {
			if (disableClientUpdate) return;
			var missions = "";
			missionPool.ForEach(mission => {
				if (!Mission.GetMissionCodename(mission, out var name))
					return;
				missions += missions.Length > 0 ? $",{name}" : name;
			});
			CroupierSocketServer.Send("Missions:" + missions);
		}

		public void SendStreakToClient() {
			if (disableClientUpdate) return;
			CroupierSocketServer.Send($"Streak:{streak}");
		}

		private void OnMouseDown(object sender, MouseButtonEventArgs e) {
			if (e.ChangedButton == MouseButton.Left)
				this.DragMove();
		}

		private void MissionSelect_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			if (e.AddedItems.Count == 0) return;
			var item = (MissionComboBoxItem)e.AddedItems[0];
			if (currentMission == null || currentMission.ID != item.ID)
				Spin(item.ID);
		}

		private void Shuffle() {
			if (missionPool.Count == 0) return;
			Spin(missionPool[random.Next(missionPool.Count)]);
		}

		private ICommand _historyEntrySelectCommand;
		private ICommand _bookmarkEntrySelectCommand;
		private ICommand _targetNameFormatSelectCommand;

		public ICommand HistoryEntrySelectCommand {
			get {
				return _historyEntrySelectCommand ??= new RelayCommand(param => this.HistoryEntrySelected(param));
			}
		}

		public ICommand BookmarkEntrySelectCommand {
			get {
				return _bookmarkEntrySelectCommand ??= new RelayCommand(param => this.BookmarkEntrySelected(param));
			}
		}

		public ICommand TargetNameFormatSelectCommand {
			get {
				return _targetNameFormatSelectCommand ??= new RelayCommand(param => this.TargetNameFormatSelected(param));
			}
		}

		private void HistoryEntrySelected(object param) {
			var index = param as int?;
			if (index == null || index < 0 || index >= spinHistory.Count) return;
			SetSpinHistory(Math.Abs(index.Value - spinHistory.Count));
		}

		private void BookmarkEntrySelected(object param) {
			var index = param as int?;
			if (index == null || index < 0 || index >= BookmarkEntries.Count) return;
			if (index == 0) {
				BookmarkEntries.Add(new(new Spin(conditions).ToString(), BookmarkEntries.Count));
				SyncBookmarks();
			}
			else if (index == 1) {
				var currentSpin = new Spin(conditions);
				var currentSpinStr = currentSpin.ToString();
				for (int i = 2; i < BookmarkEntries.Count; ++i) {
					if (currentSpinStr != BookmarkEntries[i].Name) continue;
					BookmarkEntries.RemoveAt(i);
					for (; i < BookmarkEntries.Count; ++i)
						--BookmarkEntries[i].Index;
					break;
				}
				SyncBookmarks();
			}
			else if (SpinParser.Parse(BookmarkEntries[index.Value].Name, out var spin)) {
				SetSpin(spin);
				SyncHistoryEntries();
				SendSpinToClient();
			}
		}

		private void TargetNameFormatSelected(object param) {
			var index = param as int?;
			if (index == null || index < 0 || index >= TargetNameFormatEntries.Count)
				return;
			TargetNameFormat = TargetNameFormatEntries[index.Value].ID;
		}

		private void ContextMenu_Exit(object sender, RoutedEventArgs e) {
			Application.Current.Shutdown();
		}

		private void Window_Deactivated(object sender, EventArgs e) {
			Window window = (Window)sender;
			window.Topmost = TopmostEnabled;
		}

		private void OnSizeChange(object sender, SizeChangedEventArgs e) {
			if (e.WidthChanged) {
				var numColumns = GetNumColumns();
				if (!StaticSize) {
					if (numColumns == 2) {
						Config.Default.Width2Column = e.NewSize.Width;
						Config.Save();
					}
					else if (numColumns == 1) {
						Config.Default.Width1Column = e.NewSize.Width;
						Config.Save();
					}
				}
				
				RefitWindow(true);
			}
		}

		protected virtual void OnPropertyChanged(string propertyName) {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		private void Window_Loaded(object sender, RoutedEventArgs e) {
			RefitWindow();
		}

		private int GetNumColumns() {
			if (VerticalDisplay) return 1;
			if (HorizontalSpinDisplay) return conditions.Count;
			return conditions.Count switch {
				3 or 4 or 5 => 2,
				_ => 1,
			};
		}

		private int GetNumRows() {
			if (HorizontalSpinDisplay) return 1;
			if (VerticalDisplay) return conditions.Count;
			return conditions.Count switch {
				2 or 3 or 4 => 2,
				5 => 3,
				_ => 1,
			};
		}

		private void SetupSpinUI() {
			var numColumns = GetNumColumns();
			ContentGrid.Columns = numColumns;
			ContentGrid.Children.Clear();

			foreach (var condition in conditions) {
				var control = new ContentPresenter {
					Content = condition,
					DataContext = condition,
					ContentTemplate = (DataTemplate)Resources["SpinConditionDataTemplate"],
				};
				ContentGrid.Children.Add(control);
			}
		}

		private void RefitWindow(bool keepSize = false) {
			if (StaticSize) {
				var h = SpinGridHeight * GetNumRows() + 53 + (ShowStatusBar ? StatusGrid.ActualHeight : 0);
				SizeToContent = SizeToContent.Manual;
				MinHeight = h;
				MaxHeight = h;
				double v = (Width / 50) * 1.1;
				SpinFontSize = Math.Max(11.5, v);
				OnPropertyChanged(nameof(SpinGridWidth));
				OnPropertyChanged(nameof(SpinGridHeight));
				return;
			}

			var numColumns = StaticSize ? 2 : GetNumColumns();

			var width = Math.Max(numColumns switch {
				1 => Config.Default.Width1Column,
				2 => Config.Default.Width2Column,
				_ => Width,
			}, 450);

			if (!keepSize) {
				this.Width = width;
			}

			// Scale content (poorly)
			var scale = GetContentScale();
			//double rowScaling = Math.Ceiling((double)numItems / numColumns);
			double contentHeight = width / scale;
			contentHeight += HeaderGrid.ActualHeight + StatusGrid.ActualHeight + 2;
			MinHeight = contentHeight;
			MaxHeight = contentHeight;
			SizeToContent = SizeToContent.Manual;

			ScaleFonts(width);
		}

		private double GetContentScale() {
			if (VerticalDisplay) {
				return conditions.Count switch {
					1 => 2.75,
					2 => 1.5,
					4 => 0.7,
					5 => 0.6,
					_ => 1,
				};
			}
			return conditions.Count switch {
				1 or 3 or 4 => 3.0,
				2 => 1.5,
				5 => 2.0,
				_ => 1,
			};
		}

		private void ScaleFonts(double width) {
			var numColumns = GetNumColumns();
			// Scale fonts (poorly)
			double w = width;
			var scale = numColumns > 1 ? 0.75 : 1.3;
			double v = (w / 35) * scale;
			SpinFontSize = Math.Max(10, v);
		}

		private void Window_Closing(object sender, CancelEventArgs e) {
			timer.Stop();
			StatisticsWindowInst?.Close();
			Config.Save(true);
		}

		private void CopySpinCommand_Executed(object sender, ExecutedRoutedEventArgs e) {
			// Have a few attempts at accessing the clipboard. In Windows this is usually a data race vs. other processes.
			for (var i = 0; i < 10; ++i) {
				try {
					Clipboard.SetText(new Spin(conditions).ToString());
					return;
				} catch {
				}
				System.Threading.Thread.Sleep(10);
			}

			// Give up for now and inform the use to retry.
			MessageBox.Show(
				"Clipboard access failed. Another process may have attempted to access the clipboard at the same time. Please try again.",
				"Copy Spin Failed"
			);
		}

		private void PasteSpinCommand_Executed(object sender, ExecutedRoutedEventArgs e) {
			var text = Clipboard.GetText();
			if (SpinParser.Parse(text, out var spin)) {
				if (this.spin == null || this.spin.ToString() == spin.ToString())
					return;
				SetSpin(spin);
				spinHistoryIndex = 1;
				PushCurrentSpinToHistory();
				PostConditionUpdate();
				ResetCurrentSpinProgress();
				ResetTimer();
				TrackNewSpin();
			}
		}

		private void EditMapPoolCommand_Executed(object sender, ExecutedRoutedEventArgs e) {
			if (EditMapPoolWindowInst != null) {
				EditMapPoolWindowInst.Activate();
				return;
			}
			EditMapPoolWindowInst = new() {
				Owner = this,
				WindowStartupLocation = WindowStartupLocation.CenterOwner
			};
			EditMapPoolWindowInst.AddMissionToPool += EditMapPoolWindow_AddMissionToPool;
			EditMapPoolWindowInst.RemoveMissionFromPool += EditMapPoolWindow_RemoveMissionFromPool;
			EditMapPoolWindowInst.Closed += (object sender, EventArgs e) => {
				EditMapPoolWindowInst = null;
			};
			EditMapPoolWindowInst.UpdateMissionPool(missionPool);
			EditMapPoolWindowInst.Show();
		}

		private void EditRulesetsCommand_Executed(object sender, ExecutedRoutedEventArgs e) {
			LoadRulesetConfiguration();

			if (EditRulesetWindowInst != null) {
				EditRulesetWindowInst.Activate();
				return;
			}
			EditRulesetWindowInst = new(rulesets) {
				Owner = this,
				WindowStartupLocation = WindowStartupLocation.CenterOwner
			};
			EditRulesetWindowInst.Closed += (object sender, EventArgs e) => {
				EditRulesetWindowInst = null;
			};
			EditRulesetWindowInst.Show();
		}

		private void ResetStreakCommand_Executed(object sender, ExecutedRoutedEventArgs e) {
			ResetStreak();
		}

		private void ShowStatisticsWindowCommand_Executed(object sender, ExecutedRoutedEventArgs e) {
			if (StatisticsWindowInst != null) {
				StatisticsWindowInst.Activate();
				return;
			}
			StatisticsWindowInst = new() {
				Owner = null,
				WindowStartupLocation = WindowStartupLocation.CenterOwner
			};
			StatisticsWindowInst.Closed += (object sender, EventArgs e) => {
				StatisticsWindowInst = null;
			};
			StatisticsWindowInst.Show();
		}

		private void StreakSettingsCommand_Executed(object sender, ExecutedRoutedEventArgs e) {
			TimerSettingsCommand_Executed(sender, e);
		}

		private void TimerSettingsCommand_Executed(object sender, ExecutedRoutedEventArgs e) {
			if (TimerSettingsWindowInst != null) {
				TimerSettingsWindowInst.Activate();
				return;
			}
			TimerSettingsWindowInst = new() {
				Owner = this,
				WindowStartupLocation = WindowStartupLocation.CenterOwner
			};
			TimerSettingsWindowInst.Closed += (object sender, EventArgs e) => {
				TimerSettingsWindowInst = null;
			};
			TimerSettingsWindowInst.ResetStreak += (object sender, int _) => ResetStreak();
			TimerSettingsWindowInst.ResetStreakPB += (object sender, int _) => UpdateStreakStatus();
			TimerSettingsWindowInst.Show();
		}

		private void CheckDailySpinsCommand_Executed(object sender, ExecutedRoutedEventArgs e) {
			CheckDailySpinsAsync();
		}

		private void DailySpin1Command_Executed(object sender, ExecutedRoutedEventArgs e) {
			if (SpinParser.Parse(dailySpin1.spin, out var spin)) {
				SetSpin(spin);
				spinHistoryIndex = 1;
				PushCurrentSpinToHistory();
				PostConditionUpdate();
				ResetCurrentSpinProgress();
				ResetTimer();
				TrackNewSpin();
			}
		}
		
		private void DailySpin2Command_Executed(object sender, ExecutedRoutedEventArgs e) {
			if (SpinParser.Parse(dailySpin2.spin, out var spin)) {
				SetSpin(spin);
				spinHistoryIndex = 1;
				PushCurrentSpinToHistory();
				PostConditionUpdate();
				ResetCurrentSpinProgress();
				ResetTimer();
				TrackNewSpin();
			}
		}

		private void DailySpin3Command_Executed(object sender, ExecutedRoutedEventArgs e) {
			if (SpinParser.Parse(dailySpin3.spin, out var spin)) {
				SetSpin(spin);
				spinHistoryIndex = 1;
				PushCurrentSpinToHistory();
				PostConditionUpdate();
				ResetCurrentSpinProgress();
				ResetTimer();
				TrackNewSpin();
			}
		}

		private void DailySpin1Command_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
			e.CanExecute = dailySpin1 != null && SpinParser.Parse(dailySpin1.spin, out _);
		}

		private void DailySpin2Command_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
			e.CanExecute = dailySpin2 != null && SpinParser.Parse(dailySpin2.spin, out _);
		}

		private void DailySpin3Command_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
			e.CanExecute = dailySpin3 != null && SpinParser.Parse(dailySpin3.spin, out _);
		}

		private void DebugWindowCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
#if DEBUG
			e.CanExecute = true;
#endif
		}

		private void DebugWindowCommand_Executed(object sender, ExecutedRoutedEventArgs e) {
			if (DebugWindowInst != null) {
				DebugWindowInst.Activate();
				return;
			}
			DebugWindowInst = new(this) {
				Owner = this,
				WindowStartupLocation = WindowStartupLocation.CenterOwner
			};
			DebugWindowInst.Closed += (object sender, EventArgs e) => {
				DebugWindowInst = null;
			};
			DebugWindowInst.Show();
		}

		private void ShowHitmapsWindowCommand_Executed(object sender, ExecutedRoutedEventArgs e) {
			if (HitmapsWindowInst != null) {
				HitmapsWindowInst.Activate();
				return;
			}
			HitmapsWindowInst = new() {
				Owner = this,
				WindowStartupLocation = WindowStartupLocation.CenterOwner
			};
			HitmapsWindowInst.Closed += (object sender, EventArgs e) => {
				HitmapsWindowInst = null;
			};
			HitmapsWindowInst.Show();
		}

		private void ShowLiveSplitWindowCommand_Executed(object sender, ExecutedRoutedEventArgs e) {
			if (LiveSplitWindowInst != null) {
				LiveSplitWindowInst.Activate();
				return;
			}
			LiveSplitWindowInst = new() {
				Owner = this,
				WindowStartupLocation = WindowStartupLocation.CenterOwner
			};
			LiveSplitWindowInst.Closed += (object sender, EventArgs e) => {
				LiveSplitWindowInst = null;
			};
			LiveSplitWindowInst.Show();
		}

		private void EditSpinCommand_Executed(object sender, ExecutedRoutedEventArgs e) {
			if (EditSpinWindowInst != null) {
				EditSpinWindowInst.Activate();
				return;
			}
			EditSpinWindowInst = new(conditions) {
				Owner = this,
				WindowStartupLocation = WindowStartupLocation.CenterOwner
			};
			EditSpinWindowInst.SetCondition += EditSpinWindow_SetCondition;
			EditSpinWindowInst.Closed += (object sender, EventArgs e) => {
				EditSpinWindowInst = null;
			};
			EditSpinWindowInst.Show();
		}

		private void PrevSpinCommand_Executed(object sender, ExecutedRoutedEventArgs e) {
			PreviousSpin();
		}

		private void PrevSpinCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
			e.CanExecute = spinHistory.Count > 1 && spinHistoryIndex < spinHistory.Count;
		}

		private void NextSpinCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
			e.CanExecute = spinHistoryIndex > 1;
		}

		private void NextSpinCommand_Executed(object sender, ExecutedRoutedEventArgs e) {
			NextSpin();
		}

		private void RespinCommand_Executed(object sender, ExecutedRoutedEventArgs e) {
			Spin();
		}

		private void ShuffleCommand_Executed(object sender, ExecutedRoutedEventArgs e) {
			Shuffle();
		}

		private void ResetTimerCommand_Executed(object sender, ExecutedRoutedEventArgs e) {
			timerManuallyStopped = false;
			StopTimer();
			ResetTimer();
		}

		private void StartTimerCommand_Executed(object sender, ExecutedRoutedEventArgs e) {
			timerManuallyStopped = false;
			StartTimer();
		}

		private void StartTimerCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
			e.CanExecute = timerStopped;
		}

		private void StopTimerCommand_Executed(object sender, ExecutedRoutedEventArgs e) {
			timerManuallyStopped = true;
			StopTimer();
		}

		private void StopTimerCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
			e.CanExecute = !timerManuallyStopped;
		}

		private void ShuffleCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
			e.CanExecute = ShuffleButtonEnabled;
		}

		private void PasteSpinCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
			//e.CanExecute = Croupier.Spin.Parse(Clipboard.GetText(), out _);
			e.CanExecute = SpinParser.Parse(Clipboard.GetText(), out _);
		}

		private void CheckUpdateCommand_Executed(object sender, ExecutedRoutedEventArgs e) {
			DoUpdateCheck(true);
		}

		private void Command_AlwaysCanExecute(object sender, CanExecuteRoutedEventArgs e) {
			e.CanExecute = true;
		}

		private async void CheckDailySpinsAsync() {
			var result = await DailySpinChecker.CheckForDailySpinsAsync();

			for (var i = 0; i < result.Length; ++i) {
				var spin = result[i];
				if (i == 0) dailySpin1 = spin;
				else if (i == 1) dailySpin2 = spin;
				else if (i == 2) dailySpin3 = spin;
			}

			OnPropertyChanged(nameof(DailySpin1Completed));
			OnPropertyChanged(nameof(DailySpin2Completed));
			OnPropertyChanged(nameof(DailySpin3Completed));

			OnPropertyChanged(nameof(DailySpin1Label));
			OnPropertyChanged(nameof(DailySpin2Label));
			OnPropertyChanged(nameof(DailySpin3Label));

			OnPropertyChanged(nameof(DailySpin1Tooltip));
			OnPropertyChanged(nameof(DailySpin2Tooltip));
			OnPropertyChanged(nameof(DailySpin3Tooltip));
		}

		private static async void DoUpdateCheck(bool informOnFail = false) {
			try {
				var result = await UpdateChecker.CheckForUpdateAsync();
				if (result == null) {
					if (informOnFail) {
						MessageBox.Show(
							$"Croupier is up-to-date!",
							"Update Check - Croupier",
							MessageBoxButton.OK
						);
					}
					return;
				}

				var msgRes = MessageBox.Show(
					$"Click Yes to download {result.name}. Click No to update later.",
					"Update Available - Croupier",
					MessageBoxButton.YesNo
				);
				if (msgRes == MessageBoxResult.Yes)
					UpdateChecker.OpenUrl(result.url);
			}
			catch (Exception ex) {
				if (informOnFail) MessageBox.Show(
					$"Update check error: {ex.Message}",
					"Update Check - Croupier",
					MessageBoxButton.OK
				);
			}
		}
	}
}
