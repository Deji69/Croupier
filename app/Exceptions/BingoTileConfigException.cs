using System;

namespace Croupier.Exceptions {
	[Serializable]
	public class BingoTileConfigException : BingoConfigException {
		public BingoTileConfigException() : base() { }
		public BingoTileConfigException(string message) : base(message) { }
		public BingoTileConfigException(string message, Exception inner) : base(message, inner) { }
	}
}
