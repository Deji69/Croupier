using Croupier.Properties;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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
				OnPropertyChanged(nameof(MediumConditions));
				OnPropertyChanged(nameof(HardConditions));
				OnPropertyChanged(nameof(ExtremeConditions));
				OnPropertyChanged(nameof(BuggyConditions));
				OnPropertyChanged(nameof(ImpossibleConditions));
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
				customRuleset.enableBuggy = Settings.Default.Ruleset_Custom_BannedBuggy;
				customRuleset.enableExtreme = Settings.Default.Ruleset_Custom_BannedExtreme;
				customRuleset.enableHard = Settings.Default.Ruleset_Custom_BannedHard;
				customRuleset.enableImpossible = Settings.Default.Ruleset_Custom_BannedImpossible;
				customRuleset.enableMedium = Settings.Default.Ruleset_Custom_BannedMedium;
				customRuleset.genericEliminations = Settings.Default.Ruleset_Custom_GenericEliminations;
				customRuleset.liveComplicationChance = Settings.Default.Ruleset_Custom_LiveComplicationChance;
				customRuleset.liveComplications = Settings.Default.Ruleset_Custom_LiveComplications;
				customRuleset.liveComplicationsExcludeStandard = Settings.Default.Ruleset_Custom_LiveComplicationsExcludeStandard;
				customRuleset.meleeKillTypes = Settings.Default.Ruleset_Custom_MeleeKillTypes;
				customRuleset.thrownKillTypes = Settings.Default.Ruleset_Custom_ThrownKillTypes;
			}

			var idx = Rulesets.IndexOf(Rulesets.First(r => r.Name == Settings.Default.Ruleset));
			if (idx == -1) idx = 0;
			SelectRuleset(Rulesets[idx]);
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

			Settings.Default.Ruleset = ruleset.Name;
			Settings.Default.Save();
		}

		private void CustomRulesetChanged() {
			for (int i = 0; i < Rulesets.Count; i++) {
				if (Rulesets[i].Preset != RulesetPreset.Custom) continue;
				if (Ruleset.Preset != Rulesets[i].Preset) {
					var customisedRuleset = Ruleset;
					Ruleset.ApplyPresetDefaults();
					SelectRuleset(Rulesets[i]);
					Ruleset = customisedRuleset;

					Settings.Default.Ruleset_Custom_LiveComplications = Ruleset.liveComplications;
					Settings.Default.Ruleset_Custom_LiveComplicationsExcludeStandard = Ruleset.liveComplicationsExcludeStandard;
					Settings.Default.Ruleset_Custom_LiveComplicationChance = Ruleset.liveComplicationChance;
					Settings.Default.Ruleset_Custom_GenericEliminations = Ruleset.genericEliminations;
					Settings.Default.Ruleset_Custom_MeleeKillTypes = Ruleset.meleeKillTypes;
					Settings.Default.Ruleset_Custom_ThrownKillTypes = Ruleset.thrownKillTypes;
					Settings.Default.Ruleset_Custom_BannedMedium = Ruleset.enableMedium;
					Settings.Default.Ruleset_Custom_BannedHard = Ruleset.enableHard;
					Settings.Default.Ruleset_Custom_BannedExtreme = Ruleset.enableExtreme;	
					Settings.Default.Ruleset_Custom_BannedImpossible = Ruleset.enableImpossible;
					Settings.Default.Ruleset_Custom_BannedBuggy = Ruleset.enableBuggy;
					Settings.Default.Save();
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
