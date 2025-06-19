using System;

namespace Croupier.Exceptions {
	[Serializable]
	public class BingoConfigException : CroupierException {
		public BingoConfigException() : base() { }
		public BingoConfigException(string message) : base(message) { }
		public BingoConfigException(string message, Exception inner) : base(message, inner) { }
	}
}
