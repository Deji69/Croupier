using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Croupier.UI
{
	public class ClientMessage(string data)
	{
		public string Data { get; set; } = data;
	}

	public class CroupierSocketServer
	{
		public static event EventHandler<MissionID> Respin;
		public static event EventHandler<MissionID> AutoSpin;
		public static event EventHandler<List<MissionID>> Missions;
		public static event EventHandler<string> SpinData;
		public static event EventHandler<int> Random;
		public static event EventHandler<int> Prev;
		public static event EventHandler<int> Next;
		public static event EventHandler<int> ToggleSpinLock;
		public static event EventHandler<int> Connected;
		private static bool keepAlive = true;
		private static readonly BlockingCollection<ClientMessage> clientMessages = [];
		private static readonly Mutex sendMessageMutex = new();

		private const int PORT = 8898;

		public static void Start() {
			var ipAddress = IPAddress.Parse("127.0.0.1");
			var serverSocket = new TcpListener(ipAddress, PORT);
			List<Thread> threads = [];
			serverSocket.Start();
			System.Diagnostics.Debug.WriteLine("[SOCKET] Waiting for connection...");

			App.Current.Exit += OnExit;

			Task.Run(() => {
				Thread thread = null;
				while (keepAlive) {
					if (thread != null && thread.IsAlive) {
						Thread.Sleep(100);
						continue;
					}
					
					thread = new Thread(() => {
						try {
							var client = serverSocket.AcceptTcpClient();
							System.Diagnostics.Debug.WriteLine("[SOCKET] Client connected.");
							var receive = new Thread(() => HandleClientReceive(client));
							var send = new Thread(() => HandleClientSend(client));
							receive.Start();
							send.Start();
							App.Current.Dispatcher.Invoke(new Action(() => Connected?.Invoke(null, 0)));
							receive.Join();
							send.Join();
						} catch (System.Net.Sockets.SocketException) {}
					});
					thread.Start();
				}

				keepAlive = false;

				// Force the thread to end if blocked by AcceptTcpClient (causing an exception to end a thread is a valid strategy in C#?)
				serverSocket.Stop();
			});
		}

		public static void Send(ClientMessage message) {
			clientMessages.Add(message);
		}

		public static void Send(string message) {
			clientMessages.Add(new(message));
		}

		private static void OnExit(object sender, System.Windows.ExitEventArgs e)
		{
			keepAlive = false;
		}

		private static void HandleClientReceive(TcpClient client) {
			var stream = client.GetStream();
			var buffer = new byte[1024];

			try {
				while (keepAlive && client.Connected) {
					var bytesRead = stream.Read(buffer, 0, buffer.Length);
					if (bytesRead <= 0) break;
					var data = Encoding.UTF8.GetString(buffer, 0, bytesRead);
					ProcessReceivedMessage(data);
					Console.WriteLine("Received from client: " + data);
				}
			} catch (System.IO.IOException) { }

			Console.WriteLine("[SOCKET] Client disconnected.");
		}

		private static void HandleClientSend(TcpClient client) {
			var stream = client.GetStream();

			try {
				while (keepAlive && client.Connected) {
					if (clientMessages.Count <= 0) {
						Thread.Sleep(100);
						continue;
					}
				
					while (clientMessages.TryTake(out var message)) {
						var buffer = Encoding.UTF8.GetBytes(message.Data + "\n");
						stream.Write(buffer, 0, buffer.Length);
					}
				}
			} catch (System.IO.IOException) { }

			Console.WriteLine("[SOCKET] Client disconnected.");
		}

		private static void ProcessReceivedMessage(string msg) {
			var firstSplit = msg.Split(":", 2);
			var cmd = firstSplit.First();
			var rest = firstSplit.Length > 1 ? firstSplit.Last().Split(" \t\n", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries) : [];

			if (cmd == "Respin") {
				var mission = rest.Length > 0 ? MissionIDMethods.FromKey(rest.First()) : MissionID.NONE;
				App.Current.Dispatcher.Invoke(new Action(() => Respin?.Invoke(null, mission)));
				return;
			}
			else if (cmd == "Random") {
				App.Current.Dispatcher.Invoke(new Action(() => Random?.Invoke(null, 0)));
				return;
			}
			else if (cmd == "Missions") {
				List<MissionID> missions = [];
				if (rest.Length > 0) {
					foreach (var token in rest.First().Split(",")) {
						if (!Mission.GetMissionFromString(token, out var mission)) continue;
						missions.Add(mission);
					}
				}
				App.Current.Dispatcher.Invoke(new Action(() => Missions?.Invoke(null, missions)));
				return;
			}
			else if (cmd == "AutoSpin") {
				var mission = rest.Length > 0 ? MissionIDMethods.FromKey(rest.First()) : MissionID.NONE;
				App.Current.Dispatcher.Invoke(new Action(() => AutoSpin?.Invoke(null, mission)));
				return;
			}
			else if (cmd == "Prev") {
				App.Current.Dispatcher.Invoke(new Action(() => Prev?.Invoke(null, 0)));
				return;
			}
			else if (cmd == "Next") {
				App.Current.Dispatcher.Invoke(new Action(() => Next?.Invoke(null, 0)));
				return;
			}
			else if (cmd == "SpinData") {
				App.Current.Dispatcher.Invoke(new Action(() => SpinData?.Invoke(null, rest.First())));
				return;
			}
			else if (cmd == "ToggleSpinLock") {
				App.Current.Dispatcher.Invoke(new Action(() => ToggleSpinLock?.Invoke(null, 0)));
				return;
			}
		}
	}
}
