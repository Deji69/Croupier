using Croupier.GameEvents;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Croupier {
	public class BiDirectionalPipeServer(string pipeName) {
		private readonly string _pipeName = pipeName;
		private readonly ConcurrentQueue<string> _sendQueue = new();
		private readonly SemaphoreSlim _queueSemaphore = new(0);
		public event EventHandler<string>? MessageReceived;
		public event EventHandler<int>? Connected;

		public void EnqueueMessage(string message) {
			_sendQueue.Enqueue(message);
			_queueSemaphore.Release();
		}

		private void HandleReceivedMessage(string message) {
			MessageReceived?.Invoke(this, message);
			Logging.Info($"[Received]: {message}");
		}

		public async Task RunServerAsync(CancellationToken cancellationToken) {
			Logging.Info($"Server started. Monitoring pipe: \\\\.\\pipe\\{_pipeName}");

			while (!cancellationToken.IsCancellationRequested) {
				try {
					// Create a bi-directional, asynchronous pipe instance
					using var pipeServer = new NamedPipeServerStream(_pipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);

					Logging.Info("Waiting for client connection...");
					await pipeServer.WaitForConnectionAsync(cancellationToken);
					Logging.Info("Client connected successfully.");
					Connected?.Invoke(this, 0);

					// Linked token ensures worker loops drop immediately if connection drops or token cancels
					using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

					// Run Reading and Writing loops concurrently over the single connection
					var readTask = ReadFromPipeAsync(pipeServer, cts.Token);
					var writeTask = WriteToPipeAsync(pipeServer, cts.Token);

					// Wait until either loop breaks (e.g., disconnection or crash)
					await Task.WhenAny(readTask, writeTask);

					// Cancel the remaining loop active on this stream instance
					cts.Cancel();

					// Ensure exceptions/completions are properly propagated and resources cleanly unwound
					await Task.WhenAll(readTask, writeTask);
				}
				catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) {
					break;
				}
				catch (Exception ex) {
					Logging.Info($"Connection lost or error encountered: {ex.Message}");
				}

				Logging.Info("Re-initializing pipe for next connection instance...");
			}

			Logging.Info("Pipe Server terminated gracefully.");
		}

		private async Task ReadFromPipeAsync(NamedPipeServerStream pipe, CancellationToken ct) {
			var buffer = new byte[8192];
			var incompleteMessage = new MemoryStream();

			try {
				while (!ct.IsCancellationRequested) {
					int bytesRead = await pipe.ReadAsync(buffer, ct);
					if (bytesRead == 0) break; // Client disconnected gracefully

					for (int i = 0; i < bytesRead; i++) {
						if (buffer[i] == 0x00) // Null byte terminator reached
						{
							if (incompleteMessage.Length > 0) {
								string rawMessage = Encoding.UTF8.GetString(incompleteMessage.ToArray());
								HandleReceivedMessage(rawMessage);
								incompleteMessage.SetLength(0); // Reset local chunk buffer
							}
						}
						else {
							incompleteMessage.WriteByte(buffer[i]);
						}
					}
				}
			}
			finally {
				await incompleteMessage.DisposeAsync();
			}
		}

		private async Task WriteToPipeAsync(NamedPipeServerStream pipe, CancellationToken ct) {
			while (!ct.IsCancellationRequested) {
				// Non-blocking asynchronous wait for elements in the concurrent queue
				await _queueSemaphore.WaitAsync(ct);

				if (_sendQueue.TryDequeue(out var message)) {
					// Format message trailing payload with a null character string delimiter
					byte[] messageBytes = Encoding.UTF8.GetBytes(message);
					byte[] terminatedBytes = new byte[messageBytes.Length + 1];
					Buffer.BlockCopy(messageBytes, 0, terminatedBytes, 0, messageBytes.Length);
					terminatedBytes[^1] = 0x00; // Inject structural end block

					await pipe.WriteAsync(terminatedBytes, ct);
					await pipe.FlushAsync(ct); // Force outbound flushing down the Windows subsystem
				}
			}
		}
	}


	public class CroupierPipeServer {
		public static event EventHandler<MissionID>? Respin;
		public static event EventHandler<MissionID>? AutoSpin;
		public static event EventHandler<List<MissionID>>? Missions;
		public static event EventHandler<string>? Event;
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
		private static Task? serverTask = null;
		private static BiDirectionalPipeServer? pipe = null;

		public static void Start() {
			App.Current.Exit += OnExit;
			pipe = new BiDirectionalPipeServer("CroupierIPC");
			serverTask = pipe.RunServerAsync(CancelConnection.Token);
			pipe.MessageReceived += ProcessReceivedMessage;
			pipe.Connected += OnConnected;
		}

		public static void Send(string message) {
			if (pipe == null) return;
			pipe.EnqueueMessage(message);
		}

		public static void Send(dynamic obj) {
			Send("Event:" + JsonSerializer.Serialize(obj));
		}

		private static async void OnExit(object sender, ExitEventArgs e) {
			CancelConnection.Cancel();
			if (serverTask != null) await serverTask;
		}

		public static void SpoofMessage(string msg) {
			ProcessReceivedMessage(null, msg);
		}

		private static void OnConnected(object? sender, int _) {
			Connected?.Invoke(sender, 0);
		}

		private static void ProcessReceivedMessage(object? sender, string msg) {
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
					if (rest.Length < 2) return;
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
					App.Current.Dispatcher.Invoke(new Action(() => KillValidation?.Invoke(null, rest.Length > 0 ? rest.First() : "")));
					return;
			}

			var json = JsonDocument.Parse(msg);
			var ev = json.Deserialize<Event>(jsonGameEventSerializerOptions);
			App.Current.Dispatcher.Invoke(new Action(() => Event?.Invoke(null, msg)));
		}

		private static readonly JsonSerializerOptions jsonGameEventSerializerOptions = new() {
			AllowTrailingCommas = true,
			ReadCommentHandling = JsonCommentHandling.Skip,
		};
	}
}
