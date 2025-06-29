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
		public Ruleset? Ruleset { get; set; }
		private readonly ReadOnlyObservableCollection<Ruleset> Rulesets;

		public EditRulesetWindow(ReadOnlyObservableCollection<Ruleset> rulesets) {
			InitializeComponent();
			DataContext = this;
			Rulesets = rulesets;
			RulesetComboBox.ItemsSource = rulesets;

			var ruleset = Rulesets.FirstOrDefault(r => r.Name == Config.Default.Ruleset);
			if (ruleset != null) {
				var idx = Rulesets.IndexOf(ruleset);
				if (idx == -1) idx = 0;
				SelectRuleset(Rulesets[idx]);
			}
			else SelectRuleset(Rulesets[0]);
		}

		private void RulesetComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			var comboBox = (ComboBox)sender;
			if (comboBox.SelectedValue == null) {
				comboBox.SelectedIndex = Ruleset != null ? Rulesets.IndexOf(Ruleset) : -1;
				return;
			}

			var id = comboBox.SelectedIndex;
			if (id >= 0 && id < Rulesets.Count)
				Ruleset = Rulesets[id];

			if (Ruleset != null)
				SelectRuleset(Ruleset);
		}


		private void SelectRuleset(Ruleset ruleset) {
			var idx = Rulesets.IndexOf(ruleset);
			if (idx == -1) return;
			if (idx != RulesetComboBox.SelectedIndex)
				RulesetComboBox.SelectedIndex = idx;
			Ruleset = ruleset;

			Ruleset.Current = Ruleset;
			Config.Default.Ruleset = ruleset.Name;
			Config.Save();
		}

		public event PropertyChangedEventHandler? PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName) {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
