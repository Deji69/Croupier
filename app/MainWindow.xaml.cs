using PuppeteerSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Security.Policy;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace Croupier
{
	public class MissionComboBoxItem
	{
		public MissionID ID { get; set; }
		public string Name { get; set; }
		public string Location { get; set; }
		public Uri Image { get; set; }
		public bool IsSeparator { get; set; }
	}

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
		public static readonly RoutedUICommand EditRulesetsCommand = new("Edit Rulesets", "EditRulesets", typeof(MainWindow), [
			new KeyGesture(Key.R, ModifierKeys.Alt),
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

		private EditMapPoolWindow EditMapPoolWindowInst;
		private EditRulesetWindow EditRulesetWindowInst;
		private TimerSettingsWindow TimerSettingsWindowInst;
		private EditSpinWindow EditSpinWindowInst;
		private HitmapsWindow HitmapsWindowInst;
		private LiveSplitWindow LiveSplitWindowInst;
		private TargetNameFormat _targetNameFormat = TargetNameFormat.Initials;
		private double _spinFontSize = 16;
		private bool _rightToLeft = false;
		private bool _topmostEnabled = false;
		private bool _verticalDisplay = false;
		private bool _spinLock = false;
		private bool _editMode = false;
		private bool _staticSize = false;
		private bool _staticSizeLHS = false;
		private bool _killValidations = false;
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
			get { return _rightToLeft; }
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
			get { return _staticSize; }
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
			get { return _staticSizeLHS; }
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
			get { return _killValidations; }
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
			get { return !_editMode; }
		}
		public bool EditMode {
			get { return _editMode; }
			set {
				if (value == _editMode) return;
				_editMode = value;
				OnPropertyChanged(nameof(EditMode));
				OnPropertyChanged(nameof(ShowSpinLabels));
			}
		}
		public bool ShowStatusBar {
			get {
				return ShowTimer;
			}
		}
		public bool ShowTimer {
			get {
				return _showTimer;
			}
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
		public bool TimerMultiSpin {
			get {
				return _timerMultiSpin;
			}
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
			get {
				return _timerFractions;
			}
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
		private Ruleset rules = new("current", RulesetPreset.RR14);
		private readonly ObservableCollection<Ruleset> rulesets = [
			new("RR14", RulesetPreset.RR14),
			new("RRWC 2023", RulesetPreset.RRWC2023),
			new("RR12", RulesetPreset.RR12),
			new("RR11", RulesetPreset.RR11),
			new("Croupier", RulesetPreset.Croupier),
			new("Custom", RulesetPreset.Custom),
		];

		private readonly List<MissionID> missionPool = [];
		private Mission currentMission = null;
		private bool disableClientUpdate = false;
		private bool timerStopped = true;
		private bool timerManuallyStopped = false;
		private DateTime timerStart = DateTime.Now;
		private TimeSpan? timeElapsed = null;
		private Spin spin = null;
		private bool spinCompleted = false;
		private DateTime? autoSpinSchedule = null;
		private MissionID autoSpinMission = MissionID.NONE;
		private readonly LiveSplitClient liveSplit;
		
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

		public MainWindow()
		{
			liveSplit = ((App)Application.Current).LiveSplitClient;
			DataContext = this;
			InitializeComponent();
			MainContextMenu.DataContext = this;
			SizeToContent = SizeToContent.Height;
			((App)Application.Current).WindowPlace.Register(this);
			Focus();

			if (Config.Default.SpinHistory.Count > 0) {
				string[] history = new string[Config.Default.SpinHistory.Count];
				Config.Default.SpinHistory.CopyTo(history, 0);

				foreach (var item in history.Reverse()) {
					if (SpinParser.Parse(item, out var spin))
						PushSpinToHistory(spin);
				}
			}

			if (spinHistory.Count > 0)
				SetSpinHistory(1);
			else {
				SetMission(missions[0].ID);
				Spin(currentMission.ID);
			}

			LoadSettings();

			PropertyChanged += MainWindow_PropertyChanged;

			var timer = new DispatcherTimer {
				Interval = TimeSpan.FromMilliseconds(1)
			};
			timer.Tick += (object sender, EventArgs e) => {
				var diff = timeElapsed ?? DateTime.Now - timerStart;

				if (autoSpinSchedule.HasValue) {
					Timer.Text = (DateTime.Now - autoSpinSchedule.Value).ToString(@"\-s");
					if (DateTime.Now > autoSpinSchedule.Value) {
						autoSpinSchedule = null;
						Spin(autoSpinMission);
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
			};
			CroupierSocketServer.Respin += (object sender, MissionID id) => Spin(id);
			CroupierSocketServer.AutoSpin += (object sender, MissionID id) => {
				if (SpinLock) return;
				if (Config.Default.AutoSpinCountdown > 0) {
					autoSpinSchedule = DateTime.Now + TimeSpan.FromSeconds(Config.Default.AutoSpinCountdown);
					autoSpinMission = id;
				}
				else Spin(id);
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
			CroupierSocketServer.MissionComplete += (object sender, int _) => {
				spinCompleted = true;
				liveSplit.Split();
				StopTimer();
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
			};
			CroupierSocketServer.Next += (object sender, int _) => {
				disableClientUpdate = true;
				NextSpin();
				disableClientUpdate = false;
			};
			CroupierSocketServer.SpinData += (object sender, string data) => {
				if (SpinParser.Parse(data, out var spin)) {
					disableClientUpdate = true;
					SetSpin(spin);
					UpdateSpinHistory();
					PostConditionUpdate();
					StartTimer();
					disableClientUpdate = false;
				}
			};
			CroupierSocketServer.LoadStarted += (object sender, int _) => {
				StopTimer();
			};
			CroupierSocketServer.LoadFinished += (object sender, int _) => {
				if (!spinCompleted) ResumeTimer();
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
			};
			HitmapsSpinLink.ReceiveNewSpinData += (object sender, string data) => {
				if (SpinParser.Parse(data, out var spin)) {
					SetSpin(spin);
					UpdateSpinHistory();
					PostConditionUpdate();
					StartTimer(true);
				}
			};

			SendMissionsToClient();
			SendSpinToClient();
		}

		private void MainWindow_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(VerticalDisplay)
				|| e.PropertyName == nameof(StaticSize)) {
				RefitWindow();
				return;
			}
		}

		private void LoadSettings()
		{
			if (!Enum.IsDefined(typeof(MissionPoolPresetID), Config.Default.MissionPool))
				Config.Default.MissionPool = MissionPoolPresetID.MainMissions;

			var newRuleset = rulesets.FirstOrDefault(r => r.Name == Config.Default.Ruleset);
			if (newRuleset != null) rules = newRuleset;
			else Config.Default.Ruleset = rules.Name;

			VerticalDisplay = Config.Default.VerticalDisplay;
			TopmostEnabled = Config.Default.AlwaysOnTop;
			RightToLeft = Config.Default.RightToLeft;
			StaticSize = Config.Default.StaticSize;
			StaticSizeLHS = Config.Default.StaticSizeLHS;
			ShowTimer = Config.Default.Timer;
			TimerMultiSpin = Config.Default.TimerMultiSpin;
			TimerFractions = Config.Default.TimerFractions;
			KillValidations = Config.Default.KillValidations;
			TargetNameFormat = TargetNameFormatMethods.FromString(Config.Default.TargetNameFormat);
			LoadMissionPool();

			foreach (var bookmark in Config.Default.Bookmarks) {
				BookmarkEntries.Add(new(bookmark, BookmarkEntries.Count));
			}
		}

		private void EditRulesetWindow_ApplyRuleset(object sender, Ruleset ruleset)
		{
			rules = ruleset;
		}

		private void EditSpinWindow_SetCondition(object _sender, SpinCondition condition)
		{
			if (currentMission.ID != condition.Target.Mission) return;
			for (var i = 0; i < conditions.Count; i++) {
				if (conditions[i].Target != condition.Target) continue;
				conditions[i] = condition;
				break;
			}

			UpdateSpinHistory();
			PostConditionUpdate();
		}

		private void EditMapPoolWindow_AddMissionToPool(object sender, MissionID e)
		{
			if (missionPool.Contains(e)) return;
			missionPool.Add(e);
			SendMissionsToClient();
			OnPropertyChanged(nameof(ShuffleButtonEnabled));
			SaveCustomMissionPool();
		}

		private void EditMapPoolWindow_RemoveMissionFromPool(object sender, MissionID e)
		{
			missionPool.Remove(e);
			SendMissionsToClient();
			OnPropertyChanged(nameof(ShuffleButtonEnabled));
			SaveCustomMissionPool();
		}

		private void SaveCustomMissionPool()
		{
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

			UpdateChecker.CheckForUpdateAsync().ContinueWith(task => {
				if (task.Result == null) return;

				var result = MessageBox.Show(
					$"Click Yes to download {task.Result.name}. Click No to update later.",
					"Update Available - Croupier",
					MessageBoxButton.YesNo
				);
				if (result == MessageBoxResult.Yes)
					UpdateChecker.OpenUrl(task.Result.url);
			});
		}

		public void Spin(MissionID id = MissionID.NONE) {
			if (id == MissionID.NONE) id = currentMission.ID;
			if (!SetMission(id)) return;

			autoSpinSchedule = null;

			Generator gen = new(rules, currentMission);
			spin = gen.GenerateSpin();
			SetSpin(spin);
			spinHistoryIndex = 1;
			PushCurrentSpinToHistory();
			PostConditionUpdate();

			if ((!TimerMultiSpin && !spinCompleted) || Config.Default.TimerResetMission == id) {
				ResetTimer();
				StopTimer();
			}

			spinCompleted = false;
			StartTimer();
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
			SetMission(spin.Mission);
			EditSpinWindowInst?.UpdateConditions(conditions);
			PostConditionUpdate();
		}

		private void PostConditionUpdate() {
			SendSpinToClient();
			SetupSpinUI();
			RefitWindow();
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
		}

		private void StopTimer() {
			liveSplit.Pause();

			if (timerStopped) return;
			timeElapsed = DateTime.Now - timerStart;
			timerStopped = true;
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
		}

		public void SendSpinLockToClient()
		{
			if (disableClientUpdate) return;
			CroupierSocketServer.Send("SpinLock:" + (SpinLock ? "1" : "0"));
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

		public void SendMissionsToClient()
		{
			if (disableClientUpdate) return;
			var missions = "";
			missionPool.ForEach(mission => {
				if (!Mission.GetMissionCodename(mission, out var name))
					return;
				missions += missions.Length > 0 ? $",{name}" : name;
			});
			CroupierSocketServer.Send("Missions:" + missions);
		}

		private void OnMouseDown(object sender, MouseButtonEventArgs e)
		{
			if (e.ChangedButton == MouseButton.Left)
				this.DragMove();
		}

		private void MissionSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (e.AddedItems.Count == 0) return;
			var item = (MissionComboBoxItem)e.AddedItems[0];
			if (currentMission == null || currentMission.ID != item.ID)
				Spin(item.ID);
		}

		private void Shuffle()
		{
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

		private void HistoryEntrySelected(object param)
		{
			var index = param as int?;
			if (index == null || index < 0 || index >= spinHistory.Count) return;
			SetSpinHistory(Math.Abs(index.Value - spinHistory.Count));
		}

		private void BookmarkEntrySelected(object param)
		{
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

		private void TargetNameFormatSelected(object param)
		{
			var index = param as int?;
			if (index == null || index < 0 || index >= TargetNameFormatEntries.Count)
				return;
			TargetNameFormat = TargetNameFormatEntries[index.Value].ID;
		}

		private void ContextMenu_Exit(object sender, RoutedEventArgs e)
		{
			Application.Current.Shutdown();
		}

		private void Window_Deactivated(object sender, EventArgs e)
		{
			Window window = (Window)sender;
			window.Topmost = TopmostEnabled;
		}

		private void OnSizeChange(object sender, SizeChangedEventArgs e)
		{
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

		protected virtual void OnPropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
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
				var h = SpinGridHeight * GetNumRows() + 53 + (Config.Default.Timer ? 26 : 0);
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

		private double GetContentScale()
		{
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

		private void ScaleFonts(double width)
		{
			var numColumns = GetNumColumns();
			// Scale fonts (poorly)
			double w = width;
			var scale = numColumns > 1 ? 0.75 : 1.3;
			double v = (w / 35) * scale;
			SpinFontSize = Math.Max(10, v);
		}

		private void Window_Closing(object sender, CancelEventArgs e)
		{
		}

		private void CopySpinCommand_Executed(object sender, ExecutedRoutedEventArgs e)
		{
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

		private void PasteSpinCommand_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			var text = Clipboard.GetText();
			//if (Croupier.Spin.Parse(text, out var spin)) {
			if (SpinParser.Parse(text, out var spin)) {
				SetSpin(spin);
				spinHistoryIndex = 1;
				PushCurrentSpinToHistory();
				PostConditionUpdate();
				StopTimer();
			}
		}

		private void EditMapPoolCommand_Executed(object sender, ExecutedRoutedEventArgs e)
		{
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

		private void EditRulesetsCommand_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			if (EditRulesetWindowInst != null) {
				EditRulesetWindowInst.Activate();
				return;
			}
			EditRulesetWindowInst = new(rulesets) {
				Owner = this,
				WindowStartupLocation = WindowStartupLocation.CenterOwner
			};
			EditRulesetWindowInst.ApplyRuleset += EditRulesetWindow_ApplyRuleset;
			EditRulesetWindowInst.Closed += (object sender, EventArgs e) => {
				EditRulesetWindowInst = null;
			};
			EditRulesetWindowInst.Show();
		}

		private void TimerSettingsCommand_Executed(object sender, ExecutedRoutedEventArgs e)
		{
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
			TimerSettingsWindowInst.Show();
		}

		private void ShowHitmapsWindowCommand_Executed(object sender, ExecutedRoutedEventArgs e)
		{
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

		private void EditSpinCommand_Executed(object sender, ExecutedRoutedEventArgs e)
		{
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

		private void PrevSpinCommand_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			PreviousSpin();
		}

		private void PrevSpinCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = spinHistory.Count > 1 && spinHistoryIndex < spinHistory.Count;
		}

		private void NextSpinCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = spinHistoryIndex > 1;
		}

		private void NextSpinCommand_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			NextSpin();
		}

		private void RespinCommand_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			Spin();
		}

		private void ShuffleCommand_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			Shuffle();
		}

		private void ResetTimerCommand_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			timerManuallyStopped = false;
			StopTimer();
			ResetTimer();
		}

		private void StartTimerCommand_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			timerManuallyStopped = false;
			StartTimer();
		}

		private void StartTimerCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = timerStopped;
		}

		private void StopTimerCommand_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			timerManuallyStopped = true;
			StopTimer();
		}

		private void StopTimerCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = !timerManuallyStopped;
		}

		private void ShuffleCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = ShuffleButtonEnabled;
		}

		private void PasteSpinCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			//e.CanExecute = Croupier.Spin.Parse(Clipboard.GetText(), out _);
			e.CanExecute = SpinParser.Parse(Clipboard.GetText(), out _);
		}

		private void Command_AlwaysCanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = true;
		}
	}
}
