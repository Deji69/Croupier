namespace Croupier {
	public enum KillValidationType {
		Unknown = 0,
		Invalid = 1,
		Valid = 2,
		Incomplete = 3,
	};

	public class KillValidation {
		public Target? target = null;
		public KillValidationType killValidation = KillValidationType.Incomplete;
		public bool disguiseValidation = false;
		public Target? specificTarget = null;

		public bool IsValid {
			get => disguiseValidation && killValidation != KillValidationType.Invalid && killValidation != KillValidationType.Incomplete;
		}
	}
}
