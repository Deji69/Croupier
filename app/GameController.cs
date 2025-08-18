using System;
using System.Collections.Generic;

namespace Croupier {
	public enum PlayState {
		Start,
		Started,
		Playing,
		Finished,
	}

	public enum GameMode {
		Roulette,
		Bingo,
		Hybrid,
	}

	public class GameController : ViewModel {
		public static GameController Main => main;
		private static readonly GameController main = new();
		private static readonly Random random = new();

		public event EventHandler<MissionID>? MissionChanged;
		public event EventHandler<MissionID>? MissionPoolUpdated;
		public event EventHandler<MissionCompletion>? MissionCompleted;
		public event EventHandler<MissionID>? RoundStarted;
		public event EventHandler<int>? StreakUpdated;
		public event EventHandler<GameMode>? GameModeChanged;

		private readonly BingoGame bingo;
		private readonly RouletteGame roulette;
		private readonly List<MissionID> missionPool = [];
		private GameMode gameMode = GameMode.Roulette;
		private PlayState playState = PlayState.Start;
		private MissionID missionID = MissionID.PARIS_SHOWSTOPPER;
		private int streak = 0;
		private bool hasRestartedThisRound = false;
		private DateTime roundTimerStart = DateTime.Now;
		private TimeSpan pausedRoundTimeElapsed = TimeSpan.Zero;

		public List<MissionID> MissionPool => missionPool;
		public RouletteGame Roulette => roulette;
		public BingoGame Bingo => bingo;
		public bool HasRestarted => hasRestartedThisRound;
		public TimeSpan RoundTimeElapsed => DateTime.Now - roundTimerStart;
		public bool IsPlayingHybrid => Mode == GameMode.Hybrid;
		public bool IsPlayingRoulette => Mode == GameMode.Roulette || IsPlayingHybrid;
		public bool IsPlayingBingo => Mode == GameMode.Bingo || IsPlayingHybrid;

		public GameMode Mode {
			get => gameMode;
			set {
				if (value == gameMode) return;
				SetProperty(ref gameMode, value);
				Config.Default.Mode = value;
				Config.Save();
				GameModeChanged?.Invoke(this, value);
			}
		}
		public bool IsFinished {
			get => playState == PlayState.Finished;
			set => SetProperty(ref playState, PlayState.Finished);
		}
		public MissionID MissionID {
			get => missionID;
			set {
				if (missionID == value) return;
				SetProperty(ref missionID, value);
				MissionChanged?.Invoke(this, value);
			}
		}
		public int Streak {
			get => streak;
			set {
				Config.Default.StreakCurrent = streak;

				if (streak > Config.Default.StreakPB) {
					Config.Default.StreakPB = streak;
					if (Config.Default.StreakPB > Config.Default.Stats.TopStreak)
						Config.Default.Stats.TopStreak = Config.Default.StreakPB;
				}

				SetProperty(ref streak, value);
				StreakUpdated?.Invoke(this, value);
			}
		}

		public GameController() {
			bingo = new BingoGame(this);
			roulette = new RouletteGame(this);

			roulette.SpinEdited += (sender, spin) => {
				if (spin == null) return;
				if (spin.Mission != MissionID)
					MissionID = spin.Mission;
			};
			CroupierSocketServer.MissionStart += (sender, arg) => {
				IsFinished = false;
			};
			CroupierSocketServer.MissionComplete += (sender, arg) => {
				if (IsFinished) return;
				IsFinished = true;

				arg.Won = IsPlayingRoulette || IsPlayingBingo;
				
				if (IsPlayingRoulette) {
					arg.KillsValidated = Roulette.AreKillsValid();
					arg.Won = arg.Won && arg.SA && (arg.KillsValidated || Config.Default.StreakRequireValidKills == false);
				}
				if (IsPlayingBingo && Bingo.Card != null) {
					arg.Won = arg.Won && Bingo.Card.HasWon();
				}
				
				if (arg.Won) ++Streak;
				else Streak = 0;

				pausedRoundTimeElapsed = TimeSpan.Zero;

				MissionCompleted?.Invoke(this, arg);
			};
			CroupierSocketServer.MissionFailed += (sender, _) => {
				if (IsFinished) return;

				if (HasRestarted || RoundTimeElapsed.TotalSeconds > Config.Default.StreakReplanWindow)
					Streak = 0;

				hasRestartedThisRound = true;
			};
		}

		public void Shuffle() {
			if (missionPool.Count == 0) return;
			MissionID = missionPool[random.Next(missionPool.Count)];
			StartNewRound();
		}

		public void LoadConfig(Config cfg) {
			gameMode = cfg.Mode;
			streak = cfg.StreakCurrent;
			missionPool.Clear();
			missionPool.AddRange(cfg.MissionPool.GetMissions());
			Bingo.LoadConfig(cfg);
			StreakUpdated?.Invoke(this, streak);
			GameModeChanged?.Invoke(this, gameMode);
			MissionPoolUpdated?.Invoke(this, MissionID.NONE);
		}

		public void AddMissionToPool(MissionID id) {
			if (missionPool.Contains(id)) return;
			missionPool.Add(id);
			MissionPoolUpdated?.Invoke(this, id);
		}

		public void AssureRoundIsStarted() {
			if (playState != PlayState.Start) return;
			StartNewRound();
		}

		public void RemoveMissionFromPool(MissionID id) {
			missionPool.Remove(id);
			MissionPoolUpdated?.Invoke(this, id);
		}

		public void ResetProgress() {
			playState = PlayState.Start;
			hasRestartedThisRound = false;
		}

		public void StartNewRound() {
			if (playState == PlayState.Playing)
				Streak = 0;

			if (IsPlayingBingo)
				Bingo.Draw();
			
			if (IsPlayingRoulette)
				Roulette.GenerateSpin();

			ResetProgress();
			ResetRoundTimer();

			playState = PlayState.Started;

			RoundStarted?.Invoke(this, MissionID);
		}

		public void ResumeRoundTimer() {
			roundTimerStart = DateTime.Now - pausedRoundTimeElapsed;
			pausedRoundTimeElapsed = TimeSpan.Zero;
		}

		public void PauseRoundTimer() {
			pausedRoundTimeElapsed = RoundTimeElapsed;
		}

		public void ResetRoundTimer() {
			roundTimerStart = DateTime.Now;
			pausedRoundTimeElapsed = TimeSpan.Zero;
		}

		public void SelectNewMission(MissionID id) {
			if (MissionID == id) return;
			MissionID = id;
			StartNewRound();
		}
	}
}
