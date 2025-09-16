using System;
using System.IO.Pipes;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Croupier {
	public class CroupierPipeServer {
		public static void Start() {
			Task.Factory.StartNew(() => {
				var server = new NamedPipeServerStream("CroupierIPC");
			});
		}
	}
}
