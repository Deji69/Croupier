using System.Net.Sockets;
using System.Threading.Tasks;
using System;
using System.Text;

namespace Croupier {
	public class LiveSplitClient {
		public event EventHandler<string> OnStatusChange;
		private string status;
		private bool started = false;
		private bool connected = false;
		private bool needToStop = false;
		private Socket socket = null;

		public string CurrentStatus {
			get => status;
		}

		public async Task StartAsync() {
			if (!Config.Default.LiveSplitEnabled)
				return;
			if (started)
				await Stop();

			started = true;
			connected = false;

			socket = new Socket(SocketType.Stream, ProtocolType.Tcp);

			Status("Connecting...");

			while (!needToStop) {
				try {
					await socket.ConnectAsync(Config.Default.LiveSplitIP, Config.Default.LiveSplitPort).WaitAsync(TimeSpan.FromSeconds(5));
				} catch (SocketException e) {
					if (needToStop) break;
					Status($"{e.Message}\nCheck LiveSplit Server is running (Control > Start Server) and that the IP and Port are correct.");
					System.Diagnostics.Debug.WriteLine("[LIVESPLIT] " + e.Message);
				}

				if (!socket.Connected)
					continue;

				Status("Connected.");
				connected = true;

				try {
					while (socket.Connected && !needToStop) {
						Send("ping");
						await Task.Delay(2000);
					}
				} catch (SocketException e) {
					Status($"Disconnected: {e.Message}");
					socket.Disconnect(true);
					await Task.Delay(5000);
				}

				if (needToStop)
					break;

				connected = false;
			}

			if (connected)
				socket.Disconnect(true);

			connected = false;
			started = false;
			
			Status("Stopped.");
			needToStop = false;
		}

		public Task Stop() {
			return Task.Run(() => {
				if (!started) return;
				needToStop = true;
				while (started) Task.Delay(500);
			});
		}

		public bool Send(string command) {
			if (!connected)
				return false;
			socket.Send(Encoding.ASCII.GetBytes($"{command}\r\n"));
			return true;
		}

		public bool StartTimer() {
			return Send("starttimer");
		}

		public bool StartOrSplit() {
			return Send("startorsplit");
		}

		public bool Split() {
			return Send("split");
		}

		public bool Unsplit() {
			return Send("unsplit");
		}

		public bool SkipSplint() {
			return Send("skipsplit");
		}

		public bool Pause() {
			return Send("pause");
		}

		public bool Resume() {
			return Send("resume");
		}

		public bool Reset() {
			return Send("reset");
		}

		protected void Status(string text) {
			status = text;
			App.Current.Dispatcher.Invoke(new Action(() => OnStatusChange?.Invoke(this, text)));
		}
	}
}
