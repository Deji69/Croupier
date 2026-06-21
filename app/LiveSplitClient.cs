using System;
using System.IO.Pipes;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Croupier {
	public class LiveSplitClient {
		public event EventHandler<string>? OnStatusChange;
		private string status = "";
		private bool started = false;
		private bool connected = false;
		private bool needToStop = false;
		private Socket? socket = null;
		private NamedPipeClientStream? pipe = null;
		private static CancellationTokenSource CancelConnection = new();

		public string CurrentStatus => status;

		public async Task StartAsync() {
			if (!Config.Default.LiveSplitEnabled)
				return;
			if (started)
				await Stop();

			CancelConnection = new();
			started = true;
			connected = false;

			if (Config.Default.LiveSplitUseSocketServer)
				socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
			else
				pipe = new NamedPipeClientStream(".", "LiveSplit", PipeDirection.InOut, PipeOptions.Asynchronous);

			Status("Connecting...");

			while (!CancelConnection.IsCancellationRequested) {
				try {
					if (pipe != null) await pipe.ConnectAsync(CancelConnection.Token);
					else if (socket != null) await socket.ConnectAsync(Config.Default.LiveSplitIP, Config.Default.LiveSplitPort, CancelConnection.Token);
				} catch (SocketException e) {
					if (CancelConnection.IsCancellationRequested) break;
					Status($"{e.Message}\nCheck LiveSplit Server is running (Control > Start Server) and that the IP and Port are correct.");
					System.Diagnostics.Debug.WriteLine("[LIVESPLIT] " + e.Message);
					await Task.Delay(5000);
				}

				if ((pipe != null && !pipe.IsConnected) || (socket != null && !socket.Connected))
					continue;

				Status("Connected.");
				connected = true;

				while (((pipe != null && pipe.IsConnected) || (socket != null && socket.Connected)) && !CancelConnection.IsCancellationRequested) {
					await Task.Delay(2000);
				}

				if (CancelConnection.IsCancellationRequested)
					break;

				Status("Disconnected.");
				connected = false;
			}

			if (connected) {
				pipe?.Dispose();
				socket?.Disconnect(true);
			}

			connected = false;
			started = false;
			
			Status("Stopped.");
		}

		public Task Stop() {
			CancelConnection.Cancel();
			return Task.Run(() => {
				if (!started) return;
				needToStop = true;
				while (started) Task.Delay(500);
			});
		}

		public async Task<bool> Send(string command) {
			if (pipe != null)
				return await SendViaPipe(command);
			if (socket != null)
				return await SendViaSocket(command);
			return false;
		}

		public async Task<bool> SendViaPipe(string command) {
			if (!connected || pipe == null || !pipe.IsConnected)
				return false;
			try {
				await pipe.WriteAsync(Encoding.ASCII.GetBytes($"{command}\r\n"), CancelConnection.Token);
			} catch {
				return false;
			}
			return true;
		}

		public async Task<bool> SendViaSocket(string command) {
			if (!connected || socket == null || !socket.Connected)
				return false;
			try {
				await socket.SendAsync(Encoding.ASCII.GetBytes($"{command}\r\n"));
			} catch {
				return false;
			}
			return true;
		}

		public async Task<string?> Receive(string command) {
			if (socket != null)
				return await ReceiveViaSocket(command);
			if (pipe != null)
				return await ReceiveViaPipe(command);
			return null;
		}

		public async Task<string?> ReceiveViaPipe(string command) {
			if (!connected || pipe == null || !pipe.IsConnected)
				return null;
			var buffer = new byte[1024];

			if (!await this.Send(command))
				return null;

			var size = await pipe.ReadAsync(buffer);
			if (size == 0)
				return null;
			var response = Encoding.ASCII.GetString(buffer, 0, size);
			var resArr = response.Split("\n", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
			var result = resArr.Last();
			return result;
		}

		public async Task<string?> ReceiveViaSocket(string command) {
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

		// 'splint' because I think that used to be the actual LiveSplit command and I still find it funny
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
			if (!needToStop && OnStatusChange != null)
				App.Current.Dispatcher.Invoke(new Action(() => OnStatusChange?.Invoke(this, text)));
		}
	}
}
