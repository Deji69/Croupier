using System;

namespace Croupier.Exceptions {
	[Serializable]
	public class BingoException : Exception {
		public BingoException() : base() { }
		public BingoException(string message) : base(message) { }
		public BingoException(string message, Exception inner) : base(message, inner) { }
	}
}
