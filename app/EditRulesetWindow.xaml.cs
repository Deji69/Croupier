using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Croupier
{
	/// <summary>
	/// Interaction logic for EditRulesetWindow.xaml
	/// </summary>
	public partial class EditRulesetWindow : Window, INotifyPropertyChanged
	{
		public event EventHandler<Ruleset> ApplyRuleset;

		public bool GenericEliminations {
			get { return Ruleset.genericEliminations; }
			set {
				Ruleset.genericEliminations = value;
				CustomRulesetChanged();
				OnPropertyChanged(nameof(GenericEliminations));
			}
		}
		public bool LiveComplications
		{
			get { return Ruleset.liveComplications; }
			set
			{
				Ruleset.liveComplications = value;
				CustomRulesetChanged();
				OnPropertyChanged(nameof(LiveComplications));
			}
		}
		public bool LiveComplicationsExcludeStandard
		{
			get { return Ruleset.liveComplicationsExcludeStandard; }
			set
			{
				Ruleset.liveComplicationsExcludeStandard = value;
				CustomRulesetChanged();
				OnPropertyChanged(nameof(LiveComplicationsExcludeStandard));
			}
		}
		public int LiveComplicationChance
		{
			get { return Ruleset.liveComplicationChance; }
			set
			{
				Ruleset.liveComplicationChance = value;
				CustomRulesetChanged();
				OnPropertyChanged(nameof(LiveComplicationChance));
			}
		}
		public bool MeleeKillTypes
		{
			get { return Ruleset.meleeKillTypes; }
			set
			{
				Ruleset.meleeKillTypes = value;
				CustomRulesetChanged();
				OnPropertyChanged(nameof(MeleeKillTypes));
			}
		}
		public bool ThrownKillTypes
		{
			get { return Ruleset.thrownKillTypes; }
			set
			{
				Ruleset.thrownKillTypes = value;
				CustomRulesetChanged();
				OnPropertyChanged(nameof(ThrownKillTypes));
			}
		}
		public bool AnyExplosiveKillTypes {
			get {
				return Ruleset.enableAnyExplosives;
			}
			set {
				Ruleset.enableAnyExplosives = value;
				CustomRulesetChanged();
				OnPropertyChanged(nameof(AnyExplosiveKillTypes));
			}
		}
		public bool RemoteExplosiveKillTypes {
			get {
				return Ruleset.enableRemoteExplosives;
			}
			set {
				Ruleset.enableRemoteExplosives = value;
				CustomRulesetChanged();
				OnPropertyChanged(nameof(RemoteExplosiveKillTypes));
			}
		}
		public bool ImpactExplosiveKillTypes {
			get {
				return Ruleset.enableImpactExplosives;
			}
			set {
				Ruleset.enableImpactExplosives = value;
				CustomRulesetChanged();
				OnPropertyChanged(nameof(ImpactExplosiveKillTypes));
			}
		}
		public bool LoudRemoteExplosiveKillTypes {
			get {
				return Ruleset.enableLoudRemoteExplosives;
			}
			set {
				Ruleset.enableLoudRemoteExplosives = value;
				CustomRulesetChanged();
				OnPropertyChanged(nameof(LoudRemoteExplosiveKillTypes));
			}
		}

		public bool MediumConditions
		{
			get { return Ruleset.enableMedium; }
			set
			{
				Ruleset.enableMedium = value;
				CustomRulesetChanged();
				OnPropertyChanged(nameof(MediumConditions));
			}
		}
		public bool HardConditions
		{
			get { return Ruleset.enableHard; }
			set
			{
				Ruleset.enableHard = value;
				CustomRulesetChanged();
				OnPropertyChanged(nameof(HardConditions));
			}
		}
		public bool ExtremeConditions
		{
			get { return Ruleset.enableExtreme; }
			set
			{
				Ruleset.enableExtreme = value;
				CustomRulesetChanged();
				OnPropertyChanged(nameof(ExtremeConditions));
			}
		}
		public bool BuggyConditions
		{
			get { return Ruleset.enableBuggy; }
			set
			{
				Ruleset.enableBuggy = value;
				CustomRulesetChanged();
				OnPropertyChanged(nameof(BuggyConditions));
			}
		}
		public bool ImpossibleConditions
		{
			get { return Ruleset.enableImpossible; }
			set
			{
				Ruleset.enableImpossible = value;
				CustomRulesetChanged();
				OnPropertyChanged(nameof(ImpossibleConditions));
			}
		}
		public bool EasterEggConditions {
			get {
				return Ruleset.enableEasterEggConditions;
			}
			set {
				Ruleset.enableEasterEggConditions = value;
				CustomRulesetChanged();
				OnPropertyChanged(nameof(EasterEggConditions));
			}
		}
		public bool SuitOnlyMode {
			get {
				return Ruleset.suitOnlyMode;
			}
			set {
				Ruleset.suitOnlyMode = value;
				CustomRulesetChanged();
				OnPropertyChanged(nameof(SuitOnlyMode));
			}
		}
		public bool EnableAnyDisguise {
			get {
				return Ruleset.enableAnyDisguise;
			}
			set {
				Ruleset.enableAnyDisguise = value;
				CustomRulesetChanged();
				OnPropertyChanged(nameof(EnableAnyDisguise));
			}
		}
		public bool AllowDuplicateDisguises {
			get {
				return Ruleset.allowDuplicateDisguise;
			}
			set {
				Ruleset.allowDuplicateDisguise = value;
				CustomRulesetChanged();
				OnPropertyChanged(nameof(AllowDuplicateDisguises));
			}
		}

		private Ruleset _Ruleset;
		public Ruleset Ruleset {
			get { return _Ruleset; }
			set {
				_Ruleset = value;
				OnPropertyChanged(nameof(Ruleset));
				OnPropertyChanged(nameof(LiveComplications));
				OnPropertyChanged(nameof(LiveComplicationsExcludeStandard));
				OnPropertyChanged(nameof(LiveComplicationChance));
				OnPropertyChanged(nameof(GenericEliminations));
				OnPropertyChanged(nameof(MeleeKillTypes));
				OnPropertyChanged(nameof(ThrownKillTypes));
				OnPropertyChanged(nameof(RemoteExplosiveKillTypes));
				OnPropertyChanged(nameof(ImpactExplosiveKillTypes));
				OnPropertyChanged(nameof(LoudRemoteExplosiveKillTypes));
				OnPropertyChanged(nameof(MediumConditions));
				OnPropertyChanged(nameof(HardConditions));
				OnPropertyChanged(nameof(ExtremeConditions));
				OnPropertyChanged(nameof(BuggyConditions));
				OnPropertyChanged(nameof(ImpossibleConditions));
				OnPropertyChanged(nameof(EasterEggConditions));
				OnPropertyChanged(nameof(SuitOnlyMode));
				OnPropertyChanged(nameof(EnableAnyDisguise));
				OnPropertyChanged(nameof(AllowDuplicateDisguises));
			}
		}
		private readonly ObservableCollection<Ruleset> Rulesets;

		public EditRulesetWindow(ObservableCollection<Ruleset> rulesets)
		{
			InitializeComponent();
			DataContext = this;
			Rulesets = rulesets;
			RulesetComboBox.ItemsSource = rulesets;

			var customRuleset = Rulesets.First(r => r.Preset == RulesetPreset.Custom);
			if (customRuleset != null) {
				customRuleset.enableEasterEggConditions = Config.Default.Ruleset_BannedEasterEgg;
				customRuleset.enableBuggy = Config.Default.Ruleset_BannedBuggy;
				customRuleset.enableExtreme = Config.Default.Ruleset_BannedExtreme;
				customRuleset.enableHard = Config.Default.Ruleset_BannedHard;
				customRuleset.enableImpossible = Config.Default.Ruleset_BannedImpossible;
				customRuleset.enableMedium = Config.Default.Ruleset_BannedMedium;
				customRuleset.genericEliminations = Config.Default.Ruleset_GenericEliminations;
				customRuleset.liveComplicationChance = Config.Default.Ruleset_LiveComplicationChance;
				customRuleset.liveComplications = Config.Default.Ruleset_LiveComplications;
				customRuleset.liveComplicationsExcludeStandard = Config.Default.Ruleset_LiveComplicationsExcludeStandard;
				customRuleset.meleeKillTypes = Config.Default.Ruleset_MeleeKillTypes;
				customRuleset.thrownKillTypes = Config.Default.Ruleset_ThrownKillTypes;
				customRuleset.enableAnyExplosives = Config.Default.Ruleset_AnyExplosiveKillTypes;
				customRuleset.enableRemoteExplosives = Config.Default.Ruleset_RemoteExplosiveKillTypes;
				customRuleset.enableLoudRemoteExplosives = Config.Default.Ruleset_LoudRemoteExplosiveKillTypes;
				customRuleset.enableImpactExplosives = Config.Default.Ruleset_ImpactExplosiveKillTypes;
				customRuleset.suitOnlyMode = Config.Default.Ruleset_SuitOnlyMode;
				customRuleset.allowDuplicateDisguise = Config.Default.Ruleset_AllowDuplicateDisguises;
				customRuleset.enableAnyDisguise = Config.Default.Ruleset_EnableAnyDisguise;
			}

			var ruleset = Rulesets.FirstOrDefault(r => r.Name == Config.Default.Ruleset);
			if (ruleset != null) {
				var idx = Rulesets.IndexOf(ruleset);
				if (idx == -1) idx = 0;
				SelectRuleset(Rulesets[idx]);
			}
			else SelectRuleset(Rulesets[0]);
		}

		private void RulesetComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var comboBox = (ComboBox)sender;
			if (comboBox.SelectedValue == null) {
				comboBox.SelectedIndex = Rulesets.IndexOf(Ruleset);
				return;
			}

			var id = comboBox.SelectedIndex;
			if (id >= 0 && id < Rulesets.Count)
				Ruleset = Rulesets[id];

			SelectRuleset(Ruleset);
		}


		private void SelectRuleset(Ruleset ruleset) {
			var idx = Rulesets.IndexOf(ruleset);
			if (idx == -1) return;
			if (idx != RulesetComboBox.SelectedIndex)
				RulesetComboBox.SelectedIndex = idx;
			Ruleset = ruleset;
			ApplyRuleset?.Invoke(this, Ruleset);

			Config.Default.Ruleset = ruleset.Name;
			Config.Save();
		}

		private void CustomRulesetChanged() {
			for (int i = 0; i < Rulesets.Count; i++) {
				if (Rulesets[i].Preset != RulesetPreset.Custom) continue;
				if (Ruleset.Preset != Rulesets[i].Preset) {
					var customisedRuleset = Ruleset;
					Ruleset.ApplyPresetDefaults();
					SelectRuleset(Rulesets[i]);
					Ruleset = customisedRuleset;

					Config.Default.Ruleset_LiveComplications = Ruleset.liveComplications;
					Config.Default.Ruleset_LiveComplicationsExcludeStandard = Ruleset.liveComplicationsExcludeStandard;
					Config.Default.Ruleset_LiveComplicationChance = Ruleset.liveComplicationChance;
					Config.Default.Ruleset_GenericEliminations = Ruleset.genericEliminations;
					Config.Default.Ruleset_MeleeKillTypes = Ruleset.meleeKillTypes;
					Config.Default.Ruleset_ThrownKillTypes = Ruleset.thrownKillTypes;
					Config.Default.Ruleset_AnyExplosiveKillTypes = Ruleset.enableAnyExplosives;
					Config.Default.Ruleset_RemoteExplosiveKillTypes = Ruleset.enableRemoteExplosives;
					Config.Default.Ruleset_ImpactExplosiveKillTypes = Ruleset.enableImpactExplosives;
					Config.Default.Ruleset_LoudRemoteExplosiveKillTypes = Ruleset.enableLoudRemoteExplosives;
					Config.Default.Ruleset_BannedMedium = Ruleset.enableMedium;
					Config.Default.Ruleset_BannedHard = Ruleset.enableHard;
					Config.Default.Ruleset_BannedExtreme = Ruleset.enableExtreme;
					Config.Default.Ruleset_BannedImpossible = Ruleset.enableImpossible;
					Config.Default.Ruleset_BannedBuggy = Ruleset.enableBuggy;
					Config.Default.Ruleset_BannedEasterEgg = Ruleset.enableEasterEggConditions;
					Config.Default.Ruleset_AllowDuplicateDisguises = Ruleset.allowDuplicateDisguise;
					Config.Default.Ruleset_SuitOnlyMode = Ruleset.suitOnlyMode;
					Config.Default.Ruleset_EnableAnyDisguise = Ruleset.enableAnyDisguise;
					Config.Save();
				}
				else ApplyRuleset?.Invoke(this, Ruleset);
				break;
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
