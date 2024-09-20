using Croupier.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Croupier
{
	public class SpinParseContext {
		public MissionID Mission { get; set; } = MissionID.NONE;
		public List<SpinCondition> Conditions { get; set; } = [];
	}

	public class Spin(List<SpinCondition> conditions) {
		public readonly List<SpinCondition> Conditions = conditions;

		public Spin() : this([]) { }

		public int LargeFirearmCount { 
			get {
				return this.Conditions.Count(c => c.Method.IsLargeFirearm);
			}
		}

		public MissionID Mission {
			get {
				if (Conditions.Count == 0) return MissionID.NONE;
				return Conditions.First().Target.Mission;
			}
		}

		public bool HasDisguise(Disguise disguise) {
			if (disguise == null) return false;
			return Conditions.Exists(cond => cond.Disguise.Name == disguise.Name);
		}

		public bool HasMethod(KillMethod method) {
			return Conditions.Exists(cond => cond.Method.IsSameMethod(method));
		}

		public override string ToString() {
			var str = "";

			foreach (var cond in Conditions) {
				if (str.Length > 0) str += ", ";
				str += cond.ToString();
			}

			return str;
		}
	}

	public class SpinCondition : INotifyPropertyChanged {
		private KillValidation killValidation = new();
		public KillValidation KillValidation {
			get => killValidation;
			set {
				killValidation = value;
				ForceUpdate();
			}
		}

		public Target Target { get; set; }

		public KillMethod Method { get; set; }

		public Disguise Disguise { get; set; }

		public string TargetName { get { return Target.Name; } }
		public string TargetImage { get { return Target.Image; } }
		public string MethodName { get { return Method.DisplayText; } }
		public string MethodSerialized { get { return Method.Serialized; } }
		public string MethodImage { get { return Method.Image; } }
		public string DisguiseName { get { return Disguise.Name; } }
		public string DisguiseImage { get { return Disguise.Image; } }
		
		public Uri TargetImagePath {
			get {
				if (Config.Default.KillValidations) {
					if (killValidation.specificTarget != TargetID.Unknown) {
						if (Target.targetInfos.TryGetValue(killValidation.specificTarget, out var info))
							return new Uri(Path.Combine(Environment.CurrentDirectory, "actors", info.Image));
					}
				}
				return Target.ImageUri;
			}
		}
		
		public Uri KillStatusImagePath {
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
		
		public Uri MethodKillStatusImagePath {
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
		
		public Uri DisguiseKillStatusImagePath {
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

		public Uri DisguiseImagePath { get { return Disguise.ImageUri; } }

		public Uri MethodImagePath { get { return Method.ImageUri; } }

		public override string ToString() {
			var format = TargetNameFormatMethods.FromString(Config.Default.TargetNameFormat);
			var targetKey = format switch {
				TargetNameFormat.Full => TargetName,
				TargetNameFormat.Short => Target.ShortName ?? Target.GetTargetKey(TargetName),
				_ => Target.GetTargetKey(TargetName),
			};
			return $"{targetKey}: {MethodSerialized} / {DisguiseName}";
		}

		public static bool Parse(string str, SpinParseContext context)
		{
			var tokens = str.Split(":", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
			if (tokens.Length > 1 && Target.GetTargetFromKey(tokens[0], out Target target)) {
				SpinCondition cond = new();

				if (context.Mission != MissionID.NONE && context.Mission != target.Mission)
					return false;

				context.Mission = target.Mission;
				cond.Target = target;

				var toks = tokens[1].Split("/", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
				Disguise disguise = null;

				if (toks.Length > 1) {
					var disguises = Mission.GetDisguiseDictionary(context.Mission);
					if (!disguises.TryGetValue(Strings.TokenCharacterRegex.Replace(toks[1], "").ToLower(), out disguise))
						disguise = null;
				}

				if (KillMethod.Parse(toks[0], out KillMethod method)) {
					if (target.Type == TargetType.Soders) {
						if (method.Type == KillMethodType.Standard) {
							switch (method.Standard) {
								case StandardKillMethod.Electrocution:
									method.Type = KillMethodType.Specific;
									method.Standard = null;
									method.Specific = SpecificKillMethod.Soders_Electrocution;
									break;
								case StandardKillMethod.Explosion:
									method.Type = KillMethodType.Specific;
									method.Standard = null;
									method.Specific = SpecificKillMethod.Soders_Explosion;
									break;
								case StandardKillMethod.ConsumedPoison:
									method.Type = KillMethodType.Specific;
									method.Standard = null;
									method.Specific = SpecificKillMethod.Soders_PoisonStemCells;
									break;
							}
						}
					}

					cond.Disguise = disguise ?? Mission.GetSuitDisguise(context.Mission);
					cond.Method = method;
					context.Conditions.Add(cond);
					return true;
				}
			}
			return false;
		}

		public void ForceUpdate()
		{
			OnPropertyChanged(nameof(KillValidation));
			OnPropertyChanged(nameof(KillStatusImagePath));
			OnPropertyChanged(nameof(DisguiseKillStatusImagePath));
			OnPropertyChanged(nameof(MethodKillStatusImagePath));
			OnPropertyChanged(nameof(TargetImagePath));
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
