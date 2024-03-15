using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
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

		public async Task Start() {
			if (!Config.Default.LiveSplitEnabled)
				return;
			if (started)
				await Stop();

			started = true;
			connected = false;

			socket = new Socket(SocketType.Stream, ProtocolType.Tcp);

			await Task.Run(() => {
				Status("Connecting...");
				var attempts = 1;

				while (!needToStop) {
					var task = socket.ConnectAsync(Config.Default.LiveSplitIP, Config.Default.LiveSplitPort);

					try {
						while (!task.Wait(1000));
					} catch (SocketException e) {
						Status($"{e.Message}. Reattempting (attempts: {attempts}).");
						System.Diagnostics.Debug.WriteLine("[LIVESPLIT] " + e.Message);
						break;
					}

					if (!socket.Connected)
						continue;

					Status("Connected.");
					connected = true;

					while (socket.Connected && !needToStop)
						Thread.Sleep(2000);

					if (needToStop)
						break;

					connected = false;
				}

				if (connected)
					socket.Disconnect(true);

				connected = false;
				started = false;
			});
			
			Status("Stopped.");
			needToStop = false;
		}

		public Task Stop() {
			return Task.Run(() => {
				if (!started) return;
				needToStop = true;
				while (started) Thread.Sleep(500);
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

		public bool Reset() {
			return Send("reset");
		}

		protected void Status(string text) {
			status = text;
			App.Current.Dispatcher.Invoke(new Action(() => OnStatusChange?.Invoke(this, text)));
		}
	}
}
