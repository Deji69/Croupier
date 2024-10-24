using System;
using System.ComponentModel;
using System.IO;

namespace Croupier {
	public class SpinCondition(Target target, Disguise disguise, SpinKillMethod method) : INotifyPropertyChanged {
		private KillValidation killValidation = new();
		public KillValidation KillValidation {
			get => killValidation;
			set {
				killValidation = value;
				ForceUpdate();
			}
		}

		public Target Target { get; protected set; } = target;
		public SpinKillMethod Kill { get; protected set; } = method;
		public Disguise Disguise { get; protected set; } = disguise;

		public string TargetName => Target.Name;
		public string TargetImage => Target.Image;
		public string MethodName => Kill.ToString();
		public string MethodImage => Kill.Method.Image;
		public string DisguiseName => Disguise.Name;
		public string DisguiseImage => Disguise.Image;

		public Uri TargetImagePath {
			get {
				if (Config.Default.KillValidations && killValidation.specificTarget != null)
					return killValidation.specificTarget.ImageUri;
				return Target.ImageUri;
			}
		}

		public Uri? KillStatusImagePath {
			get {
				if (!Config.Default.KillValidations)
					return null;
				switch (killValidation.killValidation) {
					case KillValidationType.Unknown:
					case KillValidationType.Incomplete:
						break;
					case KillValidationType.Valid:
						return new(Path.Combine(Environment.CurrentDirectory, "ui", killValidation.disguiseValidation ? "completed.png" : "failed.png"));
					case KillValidationType.Invalid:
						return new(Path.Combine(Environment.CurrentDirectory, "ui", "failed.png"));
				}
				return null;
			}
		}

		public Uri? MethodKillStatusImagePath {
			get {
				if (!Config.Default.KillValidations)
					return null;
				switch (killValidation.killValidation) {
					case KillValidationType.Unknown:
					case KillValidationType.Incomplete:
						break;
					case KillValidationType.Valid:
						return killValidation.disguiseValidation ? null : new(Path.Combine(Environment.CurrentDirectory, "ui", "completed.png"));
					case KillValidationType.Invalid:
						return new(Path.Combine(Environment.CurrentDirectory, "ui", "failed.png"));
				}
				return null;
			}
		}

		public Uri? DisguiseKillStatusImagePath {
			get {
				if (!Config.Default.KillValidations)
					return null;
				switch (killValidation.killValidation) {
					case KillValidationType.Unknown:
					case KillValidationType.Incomplete:
						break;
					case KillValidationType.Valid:
						return killValidation.disguiseValidation ? null : new(Path.Combine(Environment.CurrentDirectory, "ui", "failed.png"));
					case KillValidationType.Invalid:
						return killValidation.disguiseValidation ? new(Path.Combine(Environment.CurrentDirectory, "ui", "completed.png")) : new(Path.Combine(Environment.CurrentDirectory, "ui", "failed.png"));
				}
				return null;
			}
		}

		public Uri DisguiseImagePath => Disguise.ImageUri;

		public Uri MethodImagePath => Kill.Method.ImageUri;

		public string ToString(TargetNameFormat? format = null) {
			format ??= TargetNameFormatMethods.FromString(Config.Default.TargetNameFormat);
			var name = format switch {
				TargetNameFormat.Full => TargetName,
				TargetNameFormat.Short => Target.ShortName ?? Target.Initials,
				_ => Target.Initials,
			};
			return $"{name}: {Kill} / {Disguise}";
		}

		public override string ToString() => ToString();

		public void ForceUpdate() {
			OnPropertyChanged(nameof(KillValidation));
			OnPropertyChanged(nameof(KillStatusImagePath));
			OnPropertyChanged(nameof(DisguiseKillStatusImagePath));
			OnPropertyChanged(nameof(MethodKillStatusImagePath));
			OnPropertyChanged(nameof(TargetImagePath));
		}

		public event PropertyChangedEventHandler? PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName) {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
