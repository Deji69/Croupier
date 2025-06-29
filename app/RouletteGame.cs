using Croupier.Exceptions;
using System;
using System.Windows;

namespace Croupier {
	public class RouletteGame {
		public event EventHandler<Spin?>? SpinEdited;
		public event EventHandler<Spin?>? SpinGenerated;
		public event EventHandler<Spin>? KillValidationUpdated;

		private readonly GameController controller;
		private Generator? generator = null;
		private Spin? spin = null;

		public Spin? Spin => spin;

		public RouletteGame(GameController controller) {
			this.controller = controller;

			CroupierSocketServer.KillValidation += (sender, data) => {
				if (spin == null) return;
				if (data.Length == 0) return;

				var firstLine = data.Split("\n")[0];
				var validationStrings = firstLine.Split(",");

				foreach (var v in validationStrings) {
					var segments = v.Split(":");
					if (segments.Length != 4) return;
					var target = Roulette.Main.GetTargetByInitials(segments[0]);
					var specificTarget = Roulette.Main.GetTargetByInitials(segments[3]);
					var kv = new KillValidation {
						target = target,
						killValidation = (KillValidationType)int.Parse(segments[1]),
						disguiseValidation = int.Parse(segments[2]) != 0,
						specificTarget = specificTarget,
					};
					for (var i = 0; i < spin.Conditions.Count; ++i) {
						var cond = spin.Conditions[i];
						if (cond.Target.Initials != kv.target?.Initials) continue;
						cond.KillValidation = kv;
					}
				}

				KillValidationUpdated?.Invoke(this, spin);
			};
		}

		public bool AreKillsValid() {
			if (spin == null) return true;
			foreach (var cond in spin.Conditions) {
				if (!cond.KillValidation.IsValid) return false;
			}
			return true;
		}

		public void GenerateSpin() {
			if (Ruleset.Current == null) {
				MessageBox.Show("No ruleset active. Please select a ruleset from the ruleset window.");
				return;
			}

			spin = GetGenerator().Spin(Mission.Get(controller.MissionID));
			SpinGenerated?.Invoke(this, spin);
			SpinEdited?.Invoke(this, spin);
			Config.Default.SpinIsRandom = true;
			Config.Save();
		}

		public bool GetFinishResult(MissionCompletion arg) {
			arg.KillsValidated = AreKillsValid();
			return arg.SA && (arg.KillsValidated || Config.Default.StreakRequireValidKills == false);
		}

		public void SetSpin(Spin? spin) {
			this.spin = spin;
			if (spin != null)
				controller.MissionID = spin.Mission;

			Config.Default.SpinIsRandom = false;
			controller.ResetProgress();
			if (!controller.IsPlayingRoulette)
				controller.Mode = GameMode.Roulette;
			SpinEdited?.Invoke(this, Spin);
			Config.Save();
		}

		public void SetSpinCondition(SpinCondition condition) {
			if (Spin == null)
				throw new CroupierException("Spin edit paradox. There is no spin?!");
			var editedIdx = -1;
			for (var i = 0; i < Spin.Conditions.Count; i++) {
				if (Spin.Conditions[i].Target != condition.Target) continue;
				Spin.Conditions[i] = condition;
				editedIdx = i;
				break;
			}
			if (editedIdx != -1)
				SpinEdited?.Invoke(this, Spin);
		}

		private Generator GetGenerator() {
			if (generator == null) {
				if (Ruleset.Current == null)
					throw new RouletteSpinException("No ruleset active. Please select a ruleset in the ruleset window.");
				generator = Roulette.Main.CreateGenerator(Ruleset.Current);
			}
			return generator;
		}
	}
}
