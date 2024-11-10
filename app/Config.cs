using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Croupier {
	class Config {
		public static event EventHandler<int>? OnSave;

		public static Config Default = new();

		public List<string> CustomMissionPool { get; set; } = [];
		public MissionPoolPresetID MissionPool { get; set; } = MissionPoolPresetID.MainMissions;
		public string Ruleset { get; set; } = "";
		public bool SpinIsRandom { get; set; } = false;
		public List<string> SpinHistory { get; set; } = [];
		public List<string> Bookmarks { get; set; } = [];

		public bool CheckUpdate { get; set; } = true;
		public bool AlwaysOnTop { get; set; } = false;
		public bool RightToLeft { get; set; } = false;
		public bool StaticSize { get; set; } = false;
		public bool StaticSizeLHS { get; set; } = false;
		public bool VerticalDisplay { get; set; } = false;
		public bool KillValidations { get; set; } = true;
		public bool Timer { get; set; } = false;
		public bool TimerMultiSpin { get; set; } = false;
		public bool TimerFractions { get; set; } = false;
		public MissionID TimerResetMission { get; set; } = MissionID.NONE;
		public TimingMode TimingMode { get; set; } = TimingMode.LRT;

		public bool Streak { get; set; } = false;
		public bool ShowStreakPB { get; set; } = false;
		public int StreakCurrent { get; set; } = 0;
		public int StreakPB { get; set; } = 0;
		public int StreakReplanWindow { get; set; } = 60;
		public bool StreakRequireValidKills { get; set; } = true;

		public string TargetNameFormat { get; set; } = "";
		public int AutoSpinCountdown { get; set; } = 0;
		
		public double Width1Column { get; set; } = 0;
		public double Width2Column { get; set; } = 0;

		public bool LiveSplitEnabled { get; set; } = false;
		public string LiveSplitIP { get; set; } = "127.0.0.1";
		public int LiveSplitPort { get; set; } = 16834;

		public Stats Stats { get; set; } = new Stats();

		static public bool Load()
		{
			try {
				var json = File.ReadAllText("config.json");
				Default = JsonSerializer.Deserialize<Config>(json, jsonSerializerOptions)!;
			}
			catch (FileNotFoundException) { }
			return true;
		}

		static public void Save(bool skipCallbacks = false)
		{
			var json = JsonSerializer.Serialize(Default, jsonSerializerOptions);
			File.WriteAllText("config.json", json);
			if (!skipCallbacks) OnSave?.Invoke(null, 0);
		}

		private static readonly JsonSerializerOptions jsonSerializerOptions = new() {
			AllowTrailingCommas = true,
			WriteIndented = true,
			IncludeFields = true,
			ReadCommentHandling = JsonCommentHandling.Skip
		};
	}
}
