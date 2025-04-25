using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Croupier
{
	public class ClientMessage(string data)
	{
		public string Data { get; set; } = data;
	}

	public class CroupierSocketServer
	{
		public static event EventHandler<MissionID>? Respin;
		public static event EventHandler<MissionID>? AutoSpin;
		public static event EventHandler<List<MissionID>>? Missions;
		public static event EventHandler<string>? SpinData;
		public static event EventHandler<string>? KillValidation;
		public static event EventHandler<int>? Random;
		public static event EventHandler<int>? Prev;
		public static event EventHandler<int>? Next;
		public static event EventHandler<int>? ToggleSpinLock;
		public static event EventHandler<MissionStart>? MissionStart;
		public static event EventHandler<MissionCompletion>? MissionComplete;
		public static event EventHandler<int>? MissionOutroBegin;
		public static event EventHandler<int>? MissionFailed;
		public static event EventHandler<int>? ResetTimer;
		public static event EventHandler<int>? ResetStreak;
		public static event EventHandler<bool>? PauseTimer;
		public static event EventHandler<bool>? ToggleTimer;
		public static event EventHandler<int>? SplitTimer;
		public static event EventHandler<int>? LoadStarted;
		public static event EventHandler<int>? LoadFinished;
		public static event EventHandler<int>? Connected;
		private static readonly CancellationTokenSource CancelConnection = new();
		private static readonly BlockingCollection<ClientMessage> clientMessages = [];

		private const int PORT = 4747;

		public static void Start() {
			try {
				var ipAddress = IPAddress.Parse("127.0.0.1");
				var serverSocket = new TcpListener(ipAddress, PORT);
				List<Thread> threads = [];
				serverSocket.Start();
				System.Diagnostics.Debug.WriteLine("[SOCKET] Waiting for connection...");

				App.Current.Exit += OnExit;

				Task.Run(async () => {
					while (!CancelConnection.IsCancellationRequested) {
						var client = await serverSocket.AcceptTcpClientAsync();
						var stream = client.GetStream();

						System.Diagnostics.Debug.WriteLine("[SOCKET] Client connected.");
						var receive = HandleClientReceiveAsync(client, CancelConnection.Token);
						var send = HandleClientSendAsync(client, CancelConnection.Token);

						App.Current.Dispatcher.Invoke(new Action(() => Connected?.Invoke(null, 0)));
						await receive.WaitAsync(CancelConnection.Token);
						await send.WaitAsync(CancelConnection.Token);
					}
				});
			}
			catch (Exception ex) {
				MessageBox.Show($"Socket server error. The port {PORT} may not be available.\n{ex.Message}");
			}
		}

		public static void Send(ClientMessage message) {
			clientMessages.Add(message);
		}

		public static void Send(string message) {
			clientMessages.Add(new(message));
		}

		private static void OnExit(object sender, System.Windows.ExitEventArgs e) {
			CancelConnection.Cancel();
		}

		private static async Task HandleClientReceiveAsync(TcpClient client, CancellationToken ct) {
			var stream = client.GetStream();
			var buffer = new byte[1024];

			try {
				while (!ct.IsCancellationRequested && client.Connected) {
					var bytesRead = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length), ct);

					if (bytesRead <= 0) break;
					var data = Encoding.UTF8.GetString(buffer, 0, bytesRead);
					foreach (var msg in data.Split("\n", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)) {
						ProcessReceivedMessage(msg);
					}
					Console.WriteLine("Received from client: " + data);
				}
			} catch (System.IO.IOException) { }

			Console.WriteLine("[SOCKET] Client disconnected.");
		}

		private static async Task HandleClientSendAsync(TcpClient client, CancellationToken ct) {
			await Task.Run(async () => {
				var stream = client.GetStream();

				try {
					while (!ct.IsCancellationRequested && client.Connected) {
						if (clientMessages.Count <= 0) {
							Thread.Sleep(100);
							continue;
						}

						while (clientMessages.TryTake(out var message)) {
							var buffer = Encoding.UTF8.GetBytes(message.Data + "\n");
							await stream.WriteAsync(buffer.AsMemory(0, buffer.Length), ct);
						}
					}
				} catch (System.IO.IOException) { }

				Console.WriteLine("[SOCKET] Client disconnected.");
			}, ct);
		}

		public static void SpoofMessage(string msg) {
			ProcessReceivedMessage(msg);
		}

		private static void ProcessReceivedMessage(string msg) {
			var firstSplit = msg.Split(":", 2);
			var cmd = firstSplit.First();
			var rest = firstSplit.Length > 1 ? firstSplit.Last().Split("\t", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries) : [];

			switch (cmd) {
				case "Respin":
					var mission = rest.Length > 0 ? MissionIDMethods.FromKey(rest.First()) : MissionID.NONE;
					App.Current.Dispatcher.Invoke(new Action(() => Respin?.Invoke(null, mission)));
					return;
				case "Random":
					App.Current.Dispatcher.Invoke(new Action(() => Random?.Invoke(null, 0)));
					return;
				case "Missions":
					List<MissionID> missions = [];
					if (rest.Length > 0) {
						foreach (var token in rest.First().Split(",")) {
							var mission1 = MissionIDMethods.FromName(token);
							if (mission1 == MissionID.NONE) continue;
							missions.Add(mission1);
						}
					}
					App.Current.Dispatcher.Invoke(new Action(() => Missions?.Invoke(null, missions)));
					return;
				case "AutoSpin":
					var mission2 = rest.Length > 0 ? MissionIDMethods.FromKey(rest.First()) : MissionID.NONE;
					App.Current.Dispatcher.Invoke(new Action(() => AutoSpin?.Invoke(null, mission2)));
					return;
				case "Prev":
					App.Current.Dispatcher.Invoke(new Action(() => Prev?.Invoke(null, 0)));
					return;
				case "Next":
					App.Current.Dispatcher.Invoke(new Action(() => Next?.Invoke(null, 0)));
					return;
				case "SpinData":
					App.Current.Dispatcher.Invoke(new Action(() => SpinData?.Invoke(null, rest.First())));
					return;
				case "MissionStart":
					App.Current.Dispatcher.Invoke(new Action(() => MissionStart?.Invoke(null, new() {
						Location = rest.First(),
						Loadout = JsonSerializer.Deserialize<string[]>(rest[1])!,
					})));
					return;
				case "MissionComplete":
					App.Current.Dispatcher.Invoke(new Action(() => MissionComplete?.Invoke(null, new() {
						SA = int.Parse(rest.First()) == 1,
						IGT = double.Parse(rest[1])
					})));
					return;
				case "MissionOutroBegin":
					App.Current.Dispatcher.Invoke(new Action(() => MissionOutroBegin?.Invoke(null, 0)));
					return;
				case "MissionFailed":
					App.Current.Dispatcher.Invoke(new Action(() => MissionFailed?.Invoke(null, 0)));
					return;
				case "ToggleSpinLock":
					App.Current.Dispatcher.Invoke(new Action(() => ToggleSpinLock?.Invoke(null, 0)));
					return;
				case "ResetStreak":
					App.Current.Dispatcher.Invoke(new Action(() => ResetStreak?.Invoke(null, 0)));
					return;
				case "ResetTimer":
					App.Current.Dispatcher.Invoke(new Action(() => ResetTimer?.Invoke(null, 0)));
					return;
				case "PauseTimer":
					var data = rest.Length > 0 ? rest.First() : "";
					var pause = data.Length > 0 && data[0] != '0';
					App.Current.Dispatcher.Invoke(new Action(() => PauseTimer?.Invoke(null, pause)));
					return;
				case "ToggleTimer":
					var data1 = rest.Length > 0 ? rest.First() : "";
					var enable = data1.Length > 0 && data1[0] != '0';
					App.Current.Dispatcher.Invoke(new Action(() => ToggleTimer?.Invoke(null, enable)));
					return;
				case "SplitTimer":
					App.Current.Dispatcher.Invoke(new Action(() => SplitTimer?.Invoke(null, 0)));
					return;
				case "LoadStarted":
					App.Current.Dispatcher.Invoke(new Action(() => LoadStarted?.Invoke(null, 0)));
					return;
				case "LoadFinished":
					App.Current.Dispatcher.Invoke(new Action(() => LoadFinished?.Invoke(null, 0)));
					return;
				case "KillValidation":
					App.Current.Dispatcher.Invoke(new Action(() => KillValidation?.Invoke(null, rest.First())));
					return;
			}
		}
	}
}
