using System;

namespace Croupier.Exceptions {
	public class BingoGeneratorException : CroupierException {
		public BingoGeneratorException() : base() { }
		public BingoGeneratorException(string message) : base(message) { }
		public BingoGeneratorException(string message, Exception inner) : base(message, inner) { }
	}
}
