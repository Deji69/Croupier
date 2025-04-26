using System.Net.Sockets;
using System.Threading.Tasks;
using System;
using System.Text;
using System.Linq;

namespace Croupier {
	public class LiveSplitClient {
		public event EventHandler<string>? OnStatusChange;
		private string status = "";
		private bool started = false;
		private bool connected = false;
		private bool needToStop = false;
		private Socket? socket = null;

		public string CurrentStatus => status;

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

		public async Task<bool> Send(string command) {
			if (!connected || socket == null || !socket.Connected)
				return false;
			await socket.SendAsync(Encoding.ASCII.GetBytes($"{command}\r\n"));
			return true;
		}

		public async Task<string?> Receive(string command) {
			if (!connected || socket == null || !socket.Connected)
				return null;
			var buffer = new byte[1024];

			if (!await this.Send(command))
				return null;

			var size = await socket.ReceiveAsync(buffer);
			if (size == 0)
				return null;
			var response = Encoding.ASCII.GetString(buffer, 0, size);
			var resArr = response.Split("\n", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
			var result = resArr.Last();
			return result;
		}

		public async void StartTimer() {
			await Send("starttimer");
		}

		public async void StartOrSplit() {
			await Send("startorsplit");
		}

		public async void Split() {
			await Send("split");
		}

		public async void Unsplit() {
			await Send("unsplit");
		}

		public async void SkipSplint() {
			await Send("skipsplit");
		}

		public async void Pause() {
			await Send("pause");
		}

		public async void Resume() {
			await Send("resume");
		}

		public async void Reset() {
			await Send("reset");
		}

		public async Task<int> GetSplitIndex() {
			var res = await Receive("getsplitindex");
			if (int.TryParse(res, out var splitIndex))
				return splitIndex;
			return -1;
		}

		protected void Status(string text) {
			status = text;
			App.Current.Dispatcher.Invoke(new Action(() => OnStatusChange?.Invoke(this, text)));
		}
	}
}
