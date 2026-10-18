#define FUSION_UNITY
#define ENABLE_PROFILER
#define TRACE
#define DEBUG
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Fusion.Sockets;
using Fusion.Statistics;
using UnityEngine;
using UnityEngine.Profiling;

namespace Fusion;

public abstract class Simulation : ILogSource, INetPeerGroupCallbacks
{
	private class AreaOfInterestCell
	{
		public int Key;

		public NetworkObjectMeta.List Objects;

		public BitSet512 Connections;

		public bool Empty => Objects.Count == 0 && Connections.Empty();
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct AreaOfInterest
	{
		internal const int SIZE_DEFAULT = 32;

		internal const int GRID_DEFAULT = 1024;

		internal const int MAX_SHARED_RADIUS = 300;

		internal static int X_SIZE = 1024;

		internal static int Y_SIZE = 1024;

		internal static int Z_SIZE = 1024;

		public static int CELL_SIZE = 32;

		public static (int x, int y, int z) GetGridSize()
		{
			return (x: X_SIZE, y: Y_SIZE, z: Z_SIZE);
		}

		public static int GetCellSize()
		{
			return CELL_SIZE;
		}

		public static void SphereToCells(Vector3 position, float radius, HashSet<int> cells)
		{
			(int, int, int) tuple = ToCellCoords(position - new Vector3(radius, radius, radius));
			(int, int, int) tuple2 = ToCellCoords(position + new Vector3(radius, radius, radius));
			var (i, _, _) = tuple;
			for (; i <= tuple2.Item1; i++)
			{
				for (int j = tuple.Item2; j <= tuple2.Item2; j++)
				{
					for (int k = tuple.Item3; k <= tuple2.Item3; k++)
					{
						cells.Add(ToCell(i, j, k));
					}
				}
			}
		}

		public static (int x, int y, int z) ToCellCoords(Vector3 position)
		{
			int num = (int)(position.x / (float)CELL_SIZE);
			int num2 = (int)(position.y / (float)CELL_SIZE);
			int num3 = (int)(position.z / (float)CELL_SIZE);
			if (position.x < 0f)
			{
				num--;
			}
			if (position.y < 0f)
			{
				num2--;
			}
			if (position.z < 0f)
			{
				num3--;
			}
			return ClampCellCoords(num + X_SIZE / 2, num2 + Y_SIZE / 2, num3 + Z_SIZE / 2);
		}

		public static (int x, int y, int z) ToCellCoords(int index)
		{
			index--;
			int num = index / (X_SIZE * Y_SIZE);
			index -= num * X_SIZE * Y_SIZE;
			int item = index / X_SIZE;
			int item2 = index % X_SIZE;
			return (x: item2, y: item, z: num);
		}

		public static Vector3 ToCellCenter(int index)
		{
			if (index == -1)
			{
				return default(Vector3);
			}
			var (num, num2, num3) = ToCellCoords(index);
			return new Vector3((num - X_SIZE / 2) * CELL_SIZE + CELL_SIZE / 2, (num2 - Y_SIZE / 2) * CELL_SIZE + CELL_SIZE / 2, (num3 - Z_SIZE / 2) * CELL_SIZE + CELL_SIZE / 2);
		}

		public static int ToCell(Vector3 position)
		{
			var (x, y, z) = ToCellCoords(position);
			return ToCell(x, y, z);
		}

		public static int ToCell(int x, int y, int z)
		{
			(x, y, z) = ClampCellCoords(x, y, z);
			return z * X_SIZE * Y_SIZE + y * X_SIZE + x + 1;
		}

		public static (int x, int y, int z) ClampCellCoords(int x, int y, int z)
		{
			return (x: Clamp(x, X_SIZE), y: Clamp(y, Y_SIZE), z: Clamp(z, Z_SIZE));
			static int Clamp(int v, int max)
			{
				return (v >= 0) ? ((v >= max) ? (max - 1) : v) : 0;
			}
		}
	}

	internal class Client : Simulation
	{
		[CompilerGenerated]
		private sealed class _003Cget_ActivePlayers_003Ed__24 : IEnumerable<PlayerRef>, IEnumerable, IEnumerator<PlayerRef>, IEnumerator, IDisposable
		{
			private int _003C_003E1__state;

			private PlayerRef _003C_003E2__current;

			private int _003C_003El__initialThreadId;

			public Client _003C_003E4__this;

			private HashSet<PlayerRef>.Enumerator _003C_003Es__1;

			private PlayerRef _003Cplayer_003E5__2;

			PlayerRef IEnumerator<PlayerRef>.Current
			{
				[DebuggerHidden]
				get
				{
					return _003C_003E2__current;
				}
			}

			object IEnumerator.Current
			{
				[DebuggerHidden]
				get
				{
					return _003C_003E2__current;
				}
			}

			[DebuggerHidden]
			public _003Cget_ActivePlayers_003Ed__24(int _003C_003E1__state)
			{
				this._003C_003E1__state = _003C_003E1__state;
				_003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
			}

			[DebuggerHidden]
			void IDisposable.Dispose()
			{
				int num = _003C_003E1__state;
				if (num == -3 || num == 1)
				{
					try
					{
					}
					finally
					{
						_003C_003Em__Finally1();
					}
				}
				_003C_003Es__1 = default(HashSet<PlayerRef>.Enumerator);
				_003C_003E1__state = -2;
			}

			private bool MoveNext()
			{
				try
				{
					switch (_003C_003E1__state)
					{
					default:
						return false;
					case 0:
						_003C_003E1__state = -1;
						_003C_003Es__1 = _003C_003E4__this._players.GetEnumerator();
						_003C_003E1__state = -3;
						break;
					case 1:
						_003C_003E1__state = -3;
						break;
					}
					if (_003C_003Es__1.MoveNext())
					{
						_003Cplayer_003E5__2 = _003C_003Es__1.Current;
						_003C_003E2__current = _003Cplayer_003E5__2;
						_003C_003E1__state = 1;
						return true;
					}
					_003C_003Em__Finally1();
					_003C_003Es__1 = default(HashSet<PlayerRef>.Enumerator);
					return false;
				}
				catch
				{
					//try-fault
					((IDisposable)this).Dispose();
					throw;
				}
			}

			bool IEnumerator.MoveNext()
			{
				//ILSpy generated this explicit interface implementation from .override directive in MoveNext
				return this.MoveNext();
			}

			private void _003C_003Em__Finally1()
			{
				_003C_003E1__state = -1;
				((IDisposable)_003C_003Es__1).Dispose();
			}

			[DebuggerHidden]
			void IEnumerator.Reset()
			{
				throw new NotSupportedException();
			}

			[DebuggerHidden]
			IEnumerator<PlayerRef> IEnumerable<PlayerRef>.GetEnumerator()
			{
				_003Cget_ActivePlayers_003Ed__24 result;
				if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
				{
					_003C_003E1__state = 0;
					result = this;
				}
				else
				{
					result = new _003Cget_ActivePlayers_003Ed__24(0)
					{
						_003C_003E4__this = _003C_003E4__this
					};
				}
				return result;
			}

			[DebuggerHidden]
			IEnumerator IEnumerable.GetEnumerator()
			{
				return ((IEnumerable<PlayerRef>)this).GetEnumerator();
			}
		}

		private unsafe NetConnection* _server;

		private bool _stateReceived;

		private Timeline _history;

		private SimulationInput.Buffer _inputBuffer;

		private SimulationInput[] _inputArray;

		private bool? _previousWasMC;

		public Tick PreviousServerTick;

		private TimeSyncConfiguration TimeSyncConfig
		{
			get
			{
				if (base.Topology == Topologies.Shared)
				{
					return TimeSyncConfiguration.GetFromTickrate(base.RuntimeConfig.TickRate);
				}
				return _projectConfig.TimeSynchronizationOverride ?? TimeSyncConfiguration.GetFromTickrate(base.RuntimeConfig.TickRate);
			}
		}

		internal unsafe NetConnection* ServerConnection => _server;

		public override Tick LatestServerTick => _history.IsEmpty ? default(Tick) : _history.Points.Back().Tick;

		public double LatestServerTime => (double)(int)LatestServerTick * base.TickDeltaDouble;

		public unsafe bool IsConnectedToServer => _server != null;

		public unsafe NetAddress ServerAddress => IsConnectedToServer ? _server->RemoteAddress : default(NetAddress);

		public unsafe double RttToServer => (_server == null) ? 0.0 : _server->RoundTripTime;

		public override PlayerRef LocalPlayer => _callbacks.LocalPlayerRef;

		public override IEnumerable<PlayerRef> ActivePlayers
		{
			[IteratorStateMachine(typeof(_003Cget_ActivePlayers_003Ed__24))]
			get
			{
				//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
				_003Cget_ActivePlayers_003Ed__24 _003Cget_ActivePlayers_003Ed__ = new _003Cget_ActivePlayers_003Ed__24(-2);
				_003Cget_ActivePlayers_003Ed__._003C_003E4__this = this;
				return _003Cget_ActivePlayers_003Ed__;
			}
		}

		private static string NullableToString<T>(T? value) where T : struct
		{
			if (value.HasValue)
			{
				return value.Value.ToString();
			}
			return "null";
		}

		internal Client(SimulationArgs args)
			: base(args)
		{
			ClientTimeProvider clientTimeProvider = new ClientTimeProvider();
			clientTimeProvider.OnReset(Clock.Local, ResetClientSimulationState);
			_time = clientTimeProvider;
			_history = new Timeline(128);
			_inputBuffer = new SimulationInput.Buffer(_projectConfig);
			_inputArray = new SimulationInput[128];
		}

		public unsafe void Connect(NetAddress address, byte[] token = null, byte[] uniqueId = null)
		{
			NetPeerGroup.Connect(_netPeerGroup, address, token, uniqueId);
		}

		public unsafe void Connect(string ip, ushort port, byte[] token = null, byte[] uniqueId = null)
		{
			NetPeerGroup.Connect(_netPeerGroup, ip, port, token, uniqueId);
		}

		protected unsafe override void NetworkConnected(NetConnection* connection)
		{
			_server = connection;
		}

		protected unsafe override void NetworkDisconnected(NetConnection* connection, NetDisconnectReason reason)
		{
			try
			{
				Assert.Check(_server == connection);
				_server = null;
				_callbacks.OnDisconnectedFromServer(reason);
			}
			catch (Exception error)
			{
				InternalLogStreams.LogException?.Log(this, error);
			}
		}

		internal unsafe override void OnNetworkShutdown()
		{
			if (_server != null)
			{
				NetPeerGroup.Disconnect(_netPeerGroup, _server, null);
				NetworkSend();
			}
			_server = null;
		}

		internal unsafe void ResetRttToServer(double rtt = 0.0)
		{
			NetConnection.SetRtt(_server, rtt);
		}

		internal override double GetPlayerRtt(PlayerRef player)
		{
			if (player == LocalPlayer || player == PlayerRef.None)
			{
				return RttToServer;
			}
			return 0.0;
		}

		internal unsafe override PlayerRef Connection2Player(NetConnection* c)
		{
			return LocalPlayer;
		}

		internal unsafe override int Player2Connection(PlayerRef player)
		{
			return _server->LocalId.GroupIndex;
		}

		internal unsafe override void RecvPacket()
		{
			int tick = _recvContext.Header.Tick;
			TimeFeedback feedback = default(TimeFeedback);
			feedback.Read(_recvContext.Buffer);
			_stateReplicator.RecvPacket();
			_stateReceived = true;
			if (base.HasRuntimeConfig)
			{
				_history.AddPoint(new TimelinePoint(tick, tick, base.TickDeltaDouble), base.TickDeltaDouble, allowInactiveHandling: false);
				UpdateObjectTimelines();
				_time.OnFeedbackReceived(feedback);
			}
		}

		protected override void NetworkReceiveDone()
		{
			if (!(LatestServerTick != PreviousServerTick))
			{
				return;
			}
			if (_time.IsRunning())
			{
				double num = (double)((int)LatestServerTick - (int)PreviousServerTick) * base.TickDeltaDouble;
				if (num <= 1.0)
				{
					InternalLogStreams.LogTraceSnapshots?.Log(this, $"received snapshot {LatestServerTick}");
					_time.OnSnapshotReceived(RttToServer, LatestServerTick);
				}
				else
				{
					InternalLogStreams.LogTraceSnapshots?.Log(this, $"connection was lost for a long time, received snapshot {LatestServerTick}");
					ResetRttToServer(Maths.Clamp(RttToServer - num, 0.15, 0.25));
					_time.Reset(RttToServer, LatestServerTick);
				}
			}
			else
			{
				InternalLogStreams.LogTraceSnapshots?.Log(this, $"received first snapshot {LatestServerTick}");
				Assert.Check(base.HasRuntimeConfig);
				_time.Configure(base.RuntimeConfig);
				_time.Configure(TimeSyncConfig);
				_time.SetPlayerIndex(LocalPlayer.AsIndex);
				if (base.ProjectConfig.ClientsRecordFrameAndPacketTimingTraces)
				{
					_time.StartTrace();
				}
				_time.Reset(RttToServer, LatestServerTick);
			}
			PreviousServerTick = LatestServerTick;
		}

		internal override void WritePackets()
		{
			switch (base.Topology)
			{
			case Topologies.ClientServer:
				WriteInput();
				break;
			case Topologies.Shared:
				_stateReplicator.SendPacket();
				break;
			}
		}

		private unsafe void WriteInput()
		{
			NetBitBuffer.Offset offset = NetBitBuffer.GetOffset(_sendContext.Buffer);
			int num = Maths.Clamp(Mathf.CeilToInt((float)(_server->Rtt / base.TickDeltaDouble)), 3, 6);
			if (base.Config.InputTransferMode == SimulationConfig.InputTransferModes.LatestState)
			{
				num = 1;
			}
			(SimulationInput[], int) sortedInputs = GetSortedInputs();
			SimulationInput[] item = sortedInputs.Item1;
			int item2 = sortedInputs.Item2;
			int i = Math.Max(0, item2 - num);
			int num2 = i;
			EngineProfiler.InputQueue(item2 - num2);
			NetBitBufferSerializer serializer = NetBitBufferSerializer.Writer(_sendContext.Buffer);
			if (base.Config.InputTransferMode == SimulationConfig.InputTransferModes.RedundancyUncompressed)
			{
				Assert.Check(_config.InputTotalWordCount >= 1 && _config.InputTotalWordCount <= 255);
				serializer.Buffer->PadToByteBoundary();
				serializer.Buffer->WriteByte((byte)_config.InputTotalWordCount);
				for (; i < item2; i++)
				{
					while (true)
					{
						int offsetBits = serializer.Buffer->OffsetBits;
						serializer.Buffer->WriteBytesAligned(item[i]._ptr, _config.InputTotalWordCount * 4);
						if (!serializer.Buffer->OverflowOrLessThanOneByteRemaining)
						{
							break;
						}
						serializer.Buffer->OffsetBits = offsetBits;
						serializer.Buffer->ReplaceDataFromBlockWithTemp(serializer.Buffer->LengthBytes * 2);
					}
					_sendContext.Header.Inputs++;
				}
			}
			else
			{
				for (; i < item2; i++)
				{
					while (true)
					{
						int offsetBits2 = serializer.Buffer->OffsetBits;
						if (i == num2)
						{
							item[i].Serialize(_inputRoot, _config, serializer);
						}
						else
						{
							item[i].Serialize(item[i - 1], _config, serializer);
						}
						if (!serializer.Buffer->OverflowOrLessThanOneByteRemaining)
						{
							break;
						}
						serializer.Buffer->OffsetBits = offsetBits2;
						serializer.Buffer->ReplaceDataFromBlockWithTemp(serializer.Buffer->LengthBytes * 2);
					}
					_sendContext.Header.Inputs++;
				}
			}
			int length = offset.GetLength(_sendContext.Buffer);
			_fusionStatsManager.PendingSnapshot.AddToInputOutBandwidthStat(Maths.BytesRequiredForBits(length));
			EngineProfiler.InputSize(Maths.BytesRequiredForBits(length));
		}

		private (SimulationInput[], int) GetSortedInputs()
		{
			int item = _inputBuffer.CopySortedTo(_inputArray);
			return (_inputArray, item);
		}

		internal unsafe override SimulationInput GetInput(Tick tick, PlayerRef player)
		{
			if (player != LocalPlayer)
			{
				return null;
			}
			if (base.IsResimulation)
			{
				return _inputBuffer.Get(tick);
			}
			if (_inputBuffer.Full)
			{
				if (base.Topology != Topologies.Shared)
				{
					return null;
				}
				_inputBuffer.Clear();
			}
			SimulationInput simulationInput = _inputPool.Acquire();
			simulationInput.Player = LocalPlayer;
			simulationInput.Header->Tick = base.Tick;
			simulationInput.Header->InterpTo = _interpToPrev;
			simulationInput.Header->InterpFrom = _interpFromPrev;
			simulationInput.Header->InterpAlpha = _remoteAlphaPrev;
			_callbacks.InvokeOnInput(simulationInput);
			if (_inputBuffer.Add(simulationInput))
			{
				return simulationInput;
			}
			_inputPool.Release(simulationInput);
			return null;
		}

		protected override void BeforeFirstTick()
		{
			_callbacks.OnClientStart();
		}

		protected unsafe override void BeforeUpdate()
		{
			if (TryGetStructData<SimulationRuntimeConfig>(NetworkId.RuntimeConfig, out var data))
			{
				bool flag = data->MasterClient == LocalPlayer;
				if (base.Topology == Topologies.Shared && _previousWasMC.HasValue && _previousWasMC.Value != flag)
				{
					UpdateSimulationStateForMasterClientObjects(flag);
				}
				_previousWasMC = flag;
			}
		}

		protected unsafe override int BeforeSimulation()
		{
			int num = 0;
			EngineProfiler.Begin("Simulation.Client.BeforeSimulation");
			if (_stateReceived)
			{
				_stateReceived = false;
				if (IsConnectedToServer)
				{
					EngineProfiler.RoundTripTime((float)RttToServer);
					_fusionStatsManager.PendingSnapshot.AddToRoundTripTimeStat((float)RttToServer, overrideValue: true);
				}
				(SimulationInput[], int) sortedInputs = GetSortedInputs();
				SimulationInput[] item = sortedInputs.Item1;
				int item2 = sortedInputs.Item2;
				for (int i = 0; i < item2; i++)
				{
					if (item[i].Header->Tick <= LatestServerTick && _inputBuffer.Remove(item[i].Header->Tick, out var removed))
					{
						_inputPool.Release(removed);
					}
				}
				if (base.Topology == Topologies.ClientServer)
				{
					num = Math.Max(0, (int)base.Tick - (int)LatestServerTick);
					_tick = LatestServerTick;
					try
					{
						ResetPredictedObjectsToLatestServerState();
						if (num > 0)
						{
							RunClientSideResimulationLoop(num);
						}
					}
					catch (Exception error)
					{
						InternalLogStreams.LogException?.Log(this, error);
					}
				}
			}
			EngineProfiler.End();
			UpdateInterpolation();
			return num;
		}

		internal void ResetClientSimulationState()
		{
			InternalLogStreams.LogTraceSnapshots?.Log(this, $"(re)setting client simulation state to {LatestServerTick}");
			_tick = LatestServerTick;
			if (base.Topology == Topologies.ClientServer)
			{
				ResetPredictedObjectsToLatestServerState();
			}
			_inputBuffer.Clear();
			_stateReceived = false;
		}

		internal void ResetPredictedObjectsToLatestServerState()
		{
			_callbacks.OnBeforeClientSidePredictionReset();
			int num = 0;
			foreach (NetworkObjectMeta value in _metaLookup.Values)
			{
				if (value.HasSnapshots)
				{
					value.SnapshotLatest.CopyTo(value);
					value.SnapshotLatest.CopyTo(value.Previous);
					num++;
				}
			}
			_callbacks.OnAfterClientSidePredictionReset();
		}

		private void RunClientSideResimulationLoop(int ticks)
		{
			EngineProfiler.Begin("Simulation.Client.RunClientSideResimulationLoop");
			Assert.Check(base.Tick == LatestServerTick);
			InvokeOnBeforeAllTicks(resimulation: true, ticks);
			for (int i = 0; i < ticks; i++)
			{
				StepSimulation(SimulationStages.Resimulate, i == ticks - 1, i == 0, freeInput: false);
			}
			Assert.Check(base.Tick == (int)LatestServerTick + ticks);
			InvokeOnAfterAllTicks(resimulation: true, ticks);
			EngineProfiler.End();
		}

		protected override void NoSimulation()
		{
			UpdateInterpolation();
		}

		private void UpdateInterpolation()
		{
			if (base.HasRuntimeConfig && !_history.IsEmpty)
			{
				double remote = _time.Now().Remote;
				_history.UpdateInterpolationParams(remote);
				UpdateObjectInterpolationParams(remote);
				_interpFromPrev = _interpFrom;
				_interpToPrev = _interpTo;
				_remoteAlphaPrev = _remoteAlpha;
				_interpFrom = _history.Params.From;
				_interpTo = _history.Params.To;
				_remoteAlpha = _history.Params.Alpha;
			}
		}

		private void UpdateObjectInterpolationParams(double now)
		{
			foreach (KeyValuePair<NetworkId, NetworkObjectMeta> item in _metaLookup)
			{
				NetworkObjectMeta value = item.Value;
				if ((object)value.Instance != null)
				{
					value.Timeline.UpdateInterpolationParams(now);
				}
			}
		}

		private void UpdateObjectTimelines()
		{
			foreach (KeyValuePair<NetworkId, NetworkObjectMeta> item in _metaLookup)
			{
				NetworkObjectMeta value = item.Value;
				if ((object)value.Instance != null)
				{
					value.AddLatestSnapshotToTimeline();
				}
			}
		}
	}

	internal enum ObjectChangeType
	{
		Created,
		Updated,
		Destroyed
	}

	internal struct PlayerRefMapping
	{
		public int ActorId;

		public PlayerRef PlayerRef;
	}

	[StructLayout(LayoutKind.Explicit)]
	private struct PlayerSimulationData
	{
		[FieldOffset(0)]
		public PlayerRef Player;

		[FieldOffset(4)]
		public NetworkId Object;

		[FieldOffset(8)]
		public int Actor;
	}

	internal class History
	{
		internal class Entry
		{
			public Entry Prev;

			public Entry Next;

			public Tick Tick;

			public double Time;
		}

		private SimulationHistoryEntryList _entryList;

		public Entry Latest => _entryList.Tail;

		public Entry Oldest => _entryList.Head;

		public History(int capacity)
		{
			_entryList = new SimulationHistoryEntryList();
			for (int i = 0; i < capacity; i++)
			{
				_entryList.AddLast(new Entry());
			}
		}

		public Entry Add(Tick tick, double time)
		{
			Entry entry = _entryList.RemoveHead();
			entry.Tick = tick;
			entry.Time = time;
			_entryList.AddLast(entry);
			return entry;
		}
	}

	internal interface ICallbacks
	{
		bool IsSharedModeMasterClient { get; }

		bool CanReceivePlayerJoinLeaveCallbacks { get; }

		PlayerRef LocalPlayerRef { get; }

		void OnTick();

		void OnServerStart();

		void OnClientStart();

		unsafe SimulationMessageResult OnMessage(SimulationMessage* message);

		void OnAfterClientSidePredictionReset();

		void OnBeforeClientSidePredictionReset();

		void OnAfterTick();

		void OnBeforeTick();

		void OnAfterAllTicks(bool resimulation, int tickCount);

		void OnBeforeAllTicks(bool resimulation, int tickCount);

		void OnAfterSimulation();

		void OnBeforeSimulation(int forwardTickCount);

		void OnBeforeCopyPreviousState();

		void OnConnectedToServer();

		void OnDisconnectedFromServer(NetDisconnectReason reason);

		OnConnectionRequestReply OnConnectionRequest(NetAddress remoteAddress, byte[] token);

		void OnConnectionFailed(NetAddress remoteAddress, NetConnectFailedReason reason);

		void OnReliableData(PlayerRef player, ReliableId id, bool local, byte[] dataArray);

		void PlayerJoined(PlayerRef player);

		void PlayerLeft(PlayerRef player);

		void OnInternalConnectionAttempt(int attempt, int totalConnectionAttempts, out bool shouldChange, out NetAddress newAddress);

		void ObjectStateAuthorityChanged(NetworkId id, bool gained);

		void ObjectInputAuthorityChanged(NetworkId id, bool gained);

		void ObjectIsSimulatedChanged(NetworkId id, bool simulated);

		void ObjectEnterAOI(PlayerRef player, NetworkId id);

		void ObjectExitAOI(PlayerRef player, NetworkId id);

		void ObjectChanged(PlayerRef player, NetworkObjectMeta obj, ObjectChangeType changeType);

		void RemoteObjectCreated(NetworkObjectMeta obj);

		bool RemoteObjectDestroyed(NetworkId id);

		void UpdateRemotePrefabs();

		void OnInput(SimulationInput input);

		void OnInputMissing(SimulationInput input);
	}

	private enum TargetObjectVerificationResult
	{
		Ok,
		TargetNotInterestedInObject
	}

	internal struct TimeFeedback
	{
		public float OffsetAvg;

		public float OffsetDev;

		public float RecvDeltaAvg;

		public float RecvDeltaDev;

		public const double SUSPEND_THRESHOLD = 1.0;

		private const int ACCURACY = 256;

		private const int BLOCK = 6;

		public TimeFeedback(SimulationConnection sc)
		{
			OffsetAvg = (float)sc._clientOffset.Smoothed(0.5);
			OffsetDev = (float)sc._clientOffset.MedianAbsDev;
			RecvDeltaAvg = (float)sc._packetRecvDelta.Smoothed(0.5);
			RecvDeltaDev = (float)sc._packetRecvDelta.MedianAbsDev;
		}

		public TimeFeedback(double offsetAvg, double offsetDev, double recvDeltaAvg, double recvDeltaDev)
		{
			OffsetAvg = (float)offsetAvg;
			OffsetDev = (float)offsetDev;
			RecvDeltaAvg = (float)recvDeltaAvg;
			RecvDeltaDev = (float)recvDeltaDev;
		}

		public unsafe void Write(NetBitBuffer* buffer)
		{
			buffer->WriteInt32VarLength(FloatUtils.Compress(OffsetAvg, 256), 6);
			buffer->WriteInt32VarLength(FloatUtils.Compress(OffsetDev, 256), 6);
			buffer->WriteInt32VarLength(FloatUtils.Compress(RecvDeltaAvg, 256), 6);
			buffer->WriteInt32VarLength(FloatUtils.Compress(RecvDeltaDev, 256), 6);
		}

		public unsafe void Read(NetBitBuffer* buffer)
		{
			OffsetAvg = FloatUtils.Decompress(buffer->ReadInt32VarLength(6), 256f);
			OffsetDev = FloatUtils.Decompress(buffer->ReadInt32VarLength(6), 256f);
			RecvDeltaAvg = FloatUtils.Decompress(buffer->ReadInt32VarLength(6), 256f);
			RecvDeltaDev = FloatUtils.Decompress(buffer->ReadInt32VarLength(6), 256f);
		}
	}

	[StructLayout(LayoutKind.Explicit)]
	internal struct SimulationPacketHeader
	{
		public const int WIRE_SIZE_IN_BYTES = 8;

		public const int WIRE_SIZE_IN_BITS = 64;

		[FieldOffset(0)]
		public byte SimulationMessages;

		[FieldOffset(1)]
		public byte Cells;

		[FieldOffset(1)]
		public byte Inputs;

		[FieldOffset(2)]
		public byte ObjectUpdates;

		[FieldOffset(3)]
		public byte ObjectDestroys;

		[FieldOffset(4)]
		public int Tick;

		public bool Equals(SimulationPacketHeader other)
		{
			return SimulationMessages == other.SimulationMessages && Cells == other.Cells && ObjectUpdates == other.ObjectUpdates && ObjectDestroys == other.ObjectDestroys && Inputs == other.Inputs;
		}

		public override bool Equals(object obj)
		{
			return obj is SimulationPacketHeader other && Equals(other);
		}

		public override int GetHashCode()
		{
			int num = 397;
			num = (num * 397) ^ SimulationMessages.GetHashCode();
			num = (num * 397) ^ Cells.GetHashCode();
			num = (num * 397) ^ ObjectUpdates.GetHashCode();
			num = (num * 397) ^ ObjectDestroys.GetHashCode();
			return (num * 397) ^ Inputs.GetHashCode();
		}

		public unsafe void Write(NetBitBuffer* buffer)
		{
			Assert.Check(sizeof(SimulationPacketHeader) == 8);
			Assert.Always(buffer->PacketType == NetPacketType.NotifyData, "buffer->PacketType   == NetPacketType.NotifyData");
			SimulationPacketHeader simulationPacketHeader = this;
			buffer->WriteUInt64AtOffset(*(ulong*)(&simulationPacketHeader), 112, 64);
		}

		public unsafe void Read(NetBitBuffer* buffer)
		{
			Assert.Check(sizeof(SimulationPacketHeader) == 8);
			Assert.Always(buffer->PacketType == NetPacketType.NotifyData, "buffer->PacketType   == NetPacketType.NotifyData");
			Assert.Always(buffer->OffsetBits == 112, "buffer->OffsetBits             ==  NetNotifyHeader.SIZE_IN_BITS");
			SimulationPacketHeader simulationPacketHeader = this;
			*(ulong*)(&simulationPacketHeader) = buffer->ReadUInt64();
			this = simulationPacketHeader;
		}

		public override string ToString()
		{
			return string.Format("{0}: {1}, {2}: {3}, {4}: {5}, {6}: {7}, {8}: {9}, {10}: {11}", "SimulationMessages", SimulationMessages, "Cells", Cells, "ObjectUpdates", ObjectUpdates, "ObjectDestroys", ObjectDestroys, "Inputs", Inputs, "Tick", Tick);
		}
	}

	internal class RecvContext
	{
		private Simulation _simulation;

		public PlayerRef Player;

		public SimulationPacketHeader Header;

		public unsafe NetBitBuffer* Buffer;

		public SimulationConnection Connection;

		public RecvContext(Simulation simulation)
		{
			_simulation = simulation;
		}

		public unsafe void Init(SimulationConnection connection, NetBitBuffer* buffer)
		{
			Assert.Check(connection != null);
			Assert.Check(buffer != null);
			Connection = connection;
			Player = _simulation.Connection2Player(connection);
			Buffer = buffer;
			Header.Read(Buffer);
		}

		public unsafe void Done()
		{
			Connection = null;
			Buffer = null;
		}
	}

	internal class SendContext
	{
		private Simulation _simulation;

		public SimulationPacketHeader Header;

		public unsafe NetBitBuffer* Buffer;

		public unsafe SimulationPacketEnvelope* Envelope;

		public Tick Tick;

		public PlayerRef Player;

		public SimulationConnection Connection;

		public int ObjPrev;

		public unsafe bool IsWriting => Buffer != null;

		public SendContext(Simulation simulation)
		{
			_simulation = simulation;
		}

		public unsafe bool Init(SimulationConnection connection, Tick tick)
		{
			Assert.Check(Buffer == null, "Buffer should be null");
			Assert.Check(Envelope == null, "Envelope should be null");
			Assert.Check(Connection == null, "Connection should be null");
			Header = default(SimulationPacketHeader);
			ObjPrev = 0;
			Tick = tick;
			Connection = connection;
			Player = _simulation.Connection2Player(connection);
			if (!_simulation.NetworkGetBuffer(Connection.Connection, out Buffer))
			{
				InternalLogStreams.LogTraceNetwork?.Error("Out of packets");
				Reset();
				return false;
			}
			Buffer->OffsetBits += 64;
			Envelope = SimulationPacketEnvelope.Alloc(_simulation);
			if (Envelope == null)
			{
				InternalLogStreams.LogTraceNetwork?.Error("Out of envelopes");
				NetBitBuffer.ReleaseRef(ref Buffer);
				Reset();
				return false;
			}
			Envelope->Tick = Tick;
			return Buffer != null;
		}

		public unsafe void Send()
		{
			Assert.Check(Connection != null);
			Header.Write(Buffer);
			_simulation.NetworkSendBuffer(Connection.Connection, Buffer, Envelope);
			Reset();
		}

		private unsafe void Reset()
		{
			Tick = default(Tick);
			Header = default(SimulationPacketHeader);
			Player = default(PlayerRef);
			Buffer = null;
			Envelope = null;
			Connection = null;
		}
	}

	internal class Server : Simulation
	{
		private SimulationInput _inputReadTarget = null;

		private unsafe NetBitBuffer* _hostMigrationWriteBuffer;

		private const int HostMigrationBufferSize = 65536;

		private const int HostMigrationMaxTransferBufferSize = 32768;

		private readonly object _hmLock = new object();

		public override Tick LatestServerTick => _tick;

		public override PlayerRef LocalPlayer => base.IsPlayer ? _callbacks.LocalPlayerRef : PlayerRef.None;

		private Dictionary<NetworkId, NetworkObjectHeaderSnapshot> NetworkObjectMap { get; } = new Dictionary<NetworkId, NetworkObjectHeaderSnapshot>();


		internal unsafe override double GetPlayerRtt(PlayerRef player)
		{
			if (LocalPlayer == player)
			{
				return 0.0;
			}
			if (_playersConnections.TryGetValue(player, out var value))
			{
				return value.Connection->RoundTripTime;
			}
			return 0.0;
		}

		internal Server(SimulationArgs args)
			: base(args)
		{
			_time = new ServerTimeProvider();
			_time.Configure(CreateRuntimeConfiguration());
			if (args.ResumeState != null && args.ResumeTick != 0 && args.ResumeNetworkId.IsValid)
			{
				InternalLogStreams.LogTraceHostMigration?.Log($"Received Remote state: Tick={args.ResumeTick}, NetworkId={args.ResumeNetworkId}");
				_time.Reset(0.0, args.ResumeTick);
				_isResume = true;
				_tick = args.ResumeTick;
				_idCounter = Math.Max(1023u, args.ResumeNetworkId.Raw);
				ReadHostMigrationData(args.ResumeState);
			}
		}

		internal unsafe void Disconnect(PlayerRef player, byte[] token)
		{
			if (PlayerValid(player) && _playersConnections.TryGetValue(player, out var value))
			{
				NetPeerGroup.DisconnectInternal(_netPeerGroup, value.Connection, NetDisconnectReason.Requested, token);
			}
		}

		internal unsafe void Disconnect(NetAddress address)
		{
			foreach (SimulationConnection value in _connections.Values)
			{
				if (value.Connection->Address.Equals(address))
				{
					NetPeerGroup.DisconnectInternal(_netPeerGroup, value.Connection, NetDisconnectReason.Requested);
					break;
				}
			}
		}

		protected override void AfterSimulation()
		{
			if ((base.Config.ReplicationFeatures & NetworkProjectConfig.ReplicationFeatures.SchedulingAndInterestManagement) != NetworkProjectConfig.ReplicationFeatures.SchedulingAndInterestManagement)
			{
				return;
			}
			foreach (SimulationConnection value in _connections.Values)
			{
				AOI_UpdateAreaOfInterest(value);
			}
		}

		protected override int BeforeSimulation()
		{
			if (base.IsPlayer && !PlayerValid(LocalPlayer))
			{
				PlayerAdd(LocalPlayer, null);
			}
			_interpFromPrev = _interpFrom;
			_interpToPrev = _interpTo;
			_remoteAlphaPrev = _remoteAlpha;
			_interpFrom = _tick.Next(base.TickStride);
			_interpTo = _tick.Next(base.TickStride);
			_remoteAlpha = 0f;
			return 0;
		}

		protected unsafe override void NetworkDisconnected(NetConnection* connection, NetDisconnectReason reason)
		{
		}

		internal override void RecvPacket()
		{
			switch (base.Topology)
			{
			case Topologies.ClientServer:
				ReadInput();
				break;
			case Topologies.Shared:
				ReadStateTick();
				_stateReplicator.RecvPacket();
				break;
			}
		}

		protected unsafe override void NetworkConnected(NetConnection* connection)
		{
			SimulationConnection simulationConnectionByIndex = GetSimulationConnectionByIndex(connection->LocalConnectionId.GroupIndex);
			if (_globalInterestObjects == null)
			{
				return;
			}
			foreach (NetworkId globalInterestObject in _globalInterestObjects)
			{
				simulationConnectionByIndex.GetObjectData(globalInterestObject, create: true, allowFail: true);
			}
		}

		internal unsafe override void WritePackets()
		{
			if (_time.IsRunning())
			{
				double num = (double)((int)base.Tick - (int)_sendContext.Connection._latestTickAcknowledged) * base.TickDeltaDouble;
				if (num > 1.0)
				{
					_sendContext.Connection.ResetTimeFeedback();
				}
			}
			new TimeFeedback(_sendContext.Connection).Write(_sendContext.Buffer);
			_stateReplicator.SendPacket();
		}

		private SimulationRuntimeConfig CreateRuntimeConfiguration()
		{
			SimulationRuntimeConfig result = default(SimulationRuntimeConfig);
			result.ServerMode = _mode;
			result.PlayerMaxCount = _config.PlayerCount;
			result.Topology = _config.Topology;
			result.TickRate = Fusion.TickRate.Resolve(_config.TickRateSelection);
			if (base.IsServer)
			{
				result.HostPlayer = LocalPlayer;
			}
			if (_mode == SimulationModes.Host)
			{
				result.TickRate.Server = result.TickRate.Client;
			}
			return result;
		}

		internal unsafe void SpawnRuntimeConfiguration()
		{
			SimulationRuntimeConfig* ptr = AllocateStruct<SimulationRuntimeConfig>(NetworkId.RuntimeConfig);
			*ptr = CreateRuntimeConfiguration();
		}

		protected override void BeforeUpdate()
		{
			if (!HasObject(NetworkId.RuntimeConfig))
			{
				SpawnRuntimeConfiguration();
			}
		}

		internal void CreateInternalStateObjects(PlayerRef sceneInfoStateAuth)
		{
			Assert.Check(base.IsServer);
			NetworkObjectMeta networkObjectMeta = AllocateStruct(NetworkId.SceneInfo, 13);
			networkObjectMeta.StateAuthority = sceneInfoStateAuth;
			NetworkObjectMeta networkObjectMeta2 = AllocateStruct(NetworkId.PhysicsInfo, 10);
			networkObjectMeta2.StateAuthority = sceneInfoStateAuth;
		}

		protected unsafe override void BeforeFirstTick()
		{
			CreateInternalStateObjects(PlayerRef.None);
			if (base.Mode == SimulationModes.Host)
			{
				Assert.Check(LocalPlayer.IsRealPlayer);
				GetPlayerSimulationData(LocalPlayer, create: true);
			}
			_callbacks.OnServerStart();
		}

		internal unsafe override SimulationInput GetInput(Tick tick, PlayerRef player)
		{
			if (!PlayerValid(player))
			{
				return null;
			}
			if (base.IsPlayer && LocalPlayer == player)
			{
				SimulationInput simulationInput = _inputPool.Acquire();
				simulationInput.Player = LocalPlayer;
				simulationInput.Header->Tick = base.Tick;
				simulationInput.Header->InterpTo = base.TickPrevious;
				simulationInput.Header->InterpFrom = Math.Max(0, (int)base.TickPrevious - base.TickStride);
				simulationInput.Header->InterpAlpha = _localAlphaPrev;
				_callbacks.InvokeOnInput(simulationInput);
				return simulationInput;
			}
			SimulationConnection simulationConnectionForPlayer = GetSimulationConnectionForPlayer(player);
			if (simulationConnectionForPlayer == null)
			{
				return null;
			}
			SimulationInput simulationInput2 = simulationConnectionForPlayer._inputs.Get(tick);
			if (simulationInput2 == null)
			{
				if (_config.Topology == Topologies.ClientServer)
				{
					simulationInput2 = _inputPool.Acquire();
					simulationInput2.Player = player;
					simulationInput2.Header->Tick = tick;
					SimulationInputHeader lastUsedInputHeader = simulationConnectionForPlayer._inputs.GetLastUsedInputHeader();
					simulationInput2.Header->InterpTo = lastUsedInputHeader.InterpTo;
					simulationInput2.Header->InterpFrom = lastUsedInputHeader.InterpFrom;
					simulationInput2.Header->InterpAlpha = lastUsedInputHeader.InterpAlpha;
					try
					{
						_callbacks.InvokeOnInputMissing(simulationInput2);
					}
					catch (Exception error)
					{
						InternalLogStreams.LogException?.Log(this, error);
					}
				}
			}
			else
			{
				double? insertTime = simulationConnectionForPlayer._inputs.GetInsertTime(tick);
				Assert.Check(insertTime.HasValue);
				if (insertTime.HasValue)
				{
					simulationConnectionForPlayer.InputReceiveDelta(tick, insertTime.Value, _updateTime);
				}
				simulationConnectionForPlayer._inputs.Remove(tick, out var _);
			}
			if (_config.Topology == Topologies.Shared)
			{
				if (simulationInput2 != null)
				{
					_inputPool.Release(simulationInput2);
				}
				return null;
			}
			return simulationInput2;
		}

		private unsafe void ReadInput()
		{
			SimulationConnection connection = _recvContext.Connection;
			NetBitBuffer.Offset offset = NetBitBuffer.GetOffset(_recvContext.Buffer);
			NetBitBufferSerializer serializer = NetBitBufferSerializer.Reader(_recvContext.Buffer);
			int inputs = _recvContext.Header.Inputs;
			if (inputs > 0)
			{
				if (base.Config.InputTransferMode == SimulationConfig.InputTransferModes.RedundancyUncompressed)
				{
					serializer.Buffer->SeekToByteBoundary();
					int num = serializer.Buffer->ReadByte();
					for (int i = 0; i < inputs; i++)
					{
						Assert.Check(serializer.Buffer->IsOnEvenByte);
						SimulationInputHeader simulationInputHeader = *(SimulationInputHeader*)((byte*)serializer.Buffer->Data + serializer.Buffer->OffsetBytes);
						if (simulationInputHeader.Tick > base.Tick)
						{
							if (_inputReadTarget == null)
							{
								_inputReadTarget = _inputPool.Acquire();
							}
							_inputReadTarget.Player = _recvContext.Player;
							serializer.Buffer->ReadBytesAligned(_inputReadTarget._ptr, num * 4);
							if (!connection._inputs.Full && connection._inputs.Add(_inputReadTarget, _updateTime))
							{
								_inputReadTarget = null;
							}
						}
						else
						{
							serializer.Buffer->Advance(num * 4 * 8, writing: false);
							if (_tickUpdateTimes.TryGetValue(simulationInputHeader.Tick, out var value))
							{
								connection.InputReceiveDelta(simulationInputHeader.Tick, _updateTime, value);
							}
							else
							{
								connection.InputReceiveDelta(simulationInputHeader.Tick, _updateTime, (double)(int)simulationInputHeader.Tick * base.TickDeltaDouble);
							}
						}
					}
				}
				else
				{
					SimulationInput inputRoot = _inputRoot;
					inputRoot.Clear(_config.InputTotalWordCount);
					for (int j = 0; j < inputs; j++)
					{
						SimulationInput simulationInput = _inputPool.Acquire();
						simulationInput.Player = _recvContext.Player;
						simulationInput.Serialize(inputRoot, _config, serializer);
						inputRoot.CopyFrom(simulationInput, _config.InputTotalWordCount);
						if (simulationInput.Header->Tick > base.Tick)
						{
							if (connection._inputs.Full)
							{
								_inputPool.Release(simulationInput);
							}
							else if (!connection._inputs.Add(simulationInput, _updateTime))
							{
								_inputPool.Release(simulationInput);
							}
						}
						else
						{
							if (_tickUpdateTimes.TryGetValue(simulationInput.Header->Tick, out var value2))
							{
								connection.InputReceiveDelta(simulationInput.Header->Tick, _updateTime, value2);
							}
							else
							{
								connection.InputReceiveDelta(simulationInput.Header->Tick, _updateTime, (double)(int)simulationInput.Header->Tick * base.TickDeltaDouble);
							}
							_inputPool.Release(simulationInput);
						}
					}
				}
			}
			int length = offset.GetLength(_recvContext.Buffer);
			_fusionStatsManager.PendingSnapshot.AddToInputInBandwidthStat(Maths.BytesRequiredForBits(length));
		}

		private void ReadStateTick()
		{
			SimulationConnection connection = _recvContext.Connection;
			int tick = _recvContext.Header.Tick;
			if (_tickUpdateTimes.TryGetValue(tick, out var value))
			{
				connection.InputReceiveDelta(tick, _updateTime, value);
			}
			else
			{
				connection.InputReceiveDelta(tick, _updateTime, (double)tick * base.TickDeltaDouble);
			}
		}

		internal (Dictionary<NetworkId, NetworkObjectHeaderPtr>, Dictionary<NetworkId, List<NetworkId>>) GetResumeObjectHeader()
		{
			Dictionary<NetworkId, NetworkObjectHeaderPtr> dictionary = new Dictionary<NetworkId, NetworkObjectHeaderPtr>();
			Dictionary<NetworkId, List<NetworkId>> dictionary2 = new Dictionary<NetworkId, List<NetworkId>>();
			foreach (KeyValuePair<NetworkId, NetworkObjectHeaderSnapshot> item in NetworkObjectMap)
			{
				NetworkObjectHeader header = item.Value.Header;
				if (!header.Id.IsValid || header.Id.Raw <= 1023)
				{
					continue;
				}
				dictionary.Add(header.Id, item.Value.HeaderPtr);
				if (header.NestingRoot.IsValid)
				{
					if (!dictionary2.TryGetValue(header.NestingRoot, out var value))
					{
						dictionary2.Add(header.NestingRoot, value = new List<NetworkId>());
					}
					value.Add(header.Id);
				}
			}
			return (dictionary, dictionary2);
		}

		internal unsafe void DisposeHostMigration()
		{
			lock (_hmLock)
			{
				foreach (NetworkObjectHeaderSnapshot value in NetworkObjectMap.Values)
				{
					value.Release();
				}
				NetworkObjectMap.Clear();
				NetBitBuffer.ReleaseRef(ref _hostMigrationWriteBuffer);
			}
		}

		internal unsafe int WriteHostMigrationData(ref byte[] target, int targetBytes)
		{
			Assert.Check(target.Length <= 32768);
			Assert.Check(targetBytes <= 32768);
			if (_metaMigration.Count == 0 && _metaMigrationRemoved.Count == 0)
			{
				InternalLogStreams.LogTraceHostMigration?.Log("No migration data to write");
				return 0;
			}
			if (_hostMigrationWriteBuffer == null)
			{
				_hostMigrationWriteBuffer = NetBitBuffer.Allocate(0, 65536);
			}
			_hostMigrationWriteBuffer->Clear();
			_hostMigrationWriteBuffer->OffsetBits = 0;
			NetworkId id = _metaMigration.Head.Id;
			bool flag = true;
			int num = 0;
			while (_metaMigration.Count > 0 && (flag || _metaMigration.Head.Id != id) && _hostMigrationWriteBuffer->OffsetBits < targetBytes * 8)
			{
				flag = false;
				int offsetBits = _hostMigrationWriteBuffer->OffsetBits;
				NetworkObjectMeta networkObjectMeta = _metaMigration.RemoveHead();
				NetworkObjectHeaderSnapshotRef migration = networkObjectMeta.Migration;
				NetworkId.Write(_hostMigrationWriteBuffer, networkObjectMeta.Id);
				_hostMigrationWriteBuffer->WriteInt32(networkObjectMeta.WordCount);
				Span<int> raw = networkObjectMeta.Raw;
				Span<int> raw2 = migration.Raw;
				raw2[0] = raw[0];
				int num2 = 1;
				for (int i = num2; i < networkObjectMeta.WordCount; i++)
				{
					if (raw[i] != raw2[i])
					{
						_hostMigrationWriteBuffer->WriteInt32VarLength(i - num2);
						_hostMigrationWriteBuffer->WriteInt32VarLength(raw[i]);
						raw2[i] = raw[i];
						num2 = i;
					}
				}
				InternalLogStreams.LogTraceHostMigration?.Log($"Migration {networkObjectMeta.Id}/{networkObjectMeta.Migration.Header.Id}: " + $"WordCount={networkObjectMeta.WordCount}/{networkObjectMeta.Migration.Header.WordCount}, " + $"Type={networkObjectMeta.Type}/{networkObjectMeta.Migration.Header.Type}, " + $"CRC={migration.SnapshotCRC}");
				if (num2 == 1)
				{
					_hostMigrationWriteBuffer->OffsetBits = offsetBits;
				}
				else
				{
					_hostMigrationWriteBuffer->WriteInt32VarLength(int.MaxValue);
					InternalLogStreams.LogTraceHostMigration?.Log($"Write end mark at {_hostMigrationWriteBuffer->OffsetBits}");
					num++;
				}
				_metaMigration.AddLast(networkObjectMeta);
			}
			while (_metaMigrationRemoved.Count > 0 && _hostMigrationWriteBuffer->OffsetBits < targetBytes * 8)
			{
				NetworkId networkId = _metaMigrationRemoved.Dequeue();
				NetworkId.Write(_hostMigrationWriteBuffer, networkId);
				_hostMigrationWriteBuffer->WriteInt32(0);
				InternalLogStreams.LogTraceHostMigration?.Log($"Migration {networkId}: Removed");
				num++;
			}
			if (num == 0)
			{
				InternalLogStreams.LogTraceHostMigration?.Log("No migration data written");
				return 0;
			}
			_hostMigrationWriteBuffer->PadToByteBoundary();
			if (target.Length < _hostMigrationWriteBuffer->OffsetBytes)
			{
				target = new byte[_hostMigrationWriteBuffer->OffsetBytes * 2];
			}
			fixed (byte* destination = target)
			{
				Native.MemCpy(destination, _hostMigrationWriteBuffer->Data, _hostMigrationWriteBuffer->OffsetBytes);
			}
			InternalLogStreams.LogTraceHostMigration?.Log(string.Format("WriteHostMigrationData: {0}={1}, {2}={3}", "_metaMigration", _metaMigration.Count, "count", num));
			return _hostMigrationWriteBuffer->OffsetBytes;
		}

		private void ReadHostMigrationData(byte[] data)
		{
			foreach (NetworkObjectHeaderSnapshot value in NetworkObjectMap.Values)
			{
				value.Release();
			}
			NetworkObjectMap.Clear();
			ProcessHostMigrationData(data, NetworkObjectMap, _allocator);
		}

		internal unsafe static void ProcessHostMigrationData(byte[] data, Dictionary<NetworkId, NetworkObjectHeaderSnapshot> networkObjectMap, Allocator allocator)
		{
			if (data == null || data.Length == 0)
			{
				return;
			}
			NetBitBuffer netBitBuffer = default(NetBitBuffer);
			InternalLogStreams.LogTraceHostMigration?.Log($"ProcessHostMigrationData: {data.Length}");
			fixed (byte* data2 = data)
			{
				netBitBuffer.Data = (ulong*)data2;
				netBitBuffer.OffsetBits = 0;
				netBitBuffer.LengthBytes = data.Length;
				while (!netBitBuffer.DoneOrOverflow && netBitBuffer.CanRead(36))
				{
					NetworkId networkId = NetworkId.Read(&netBitBuffer);
					int num = netBitBuffer.ReadInt32();
					bool flag = num > 0;
					if (flag && !allocator.CanAllocSize(num * 4))
					{
						InternalLogStreams.LogTraceHostMigration?.Error($"Migration {networkId}: Invalid WordCount {num}");
						break;
					}
					try
					{
						NetworkObjectHeaderSnapshot value;
						bool flag2 = networkObjectMap.TryGetValue(networkId, out value);
						if (flag2 && !flag)
						{
							value.Release();
							networkObjectMap.Remove(networkId);
							InternalLogStreams.LogTraceHostMigration?.Log($"Migration {networkId}: Removed");
							continue;
						}
						if (!flag2)
						{
							value = new NetworkObjectHeaderSnapshot(allocator);
							value.Init(num);
							value.Header = new NetworkObjectHeader(networkId, checked((short)num), 0, default(NetworkObjectTypeId), default(NetworkId), default(NetworkObjectNestingKey), (NetworkObjectHeaderFlags)0);
							networkObjectMap[networkId] = value;
						}
						Span<int> raw = value.Raw;
						int num2 = 1;
						while (!netBitBuffer.DoneOrOverflow)
						{
							if (!netBitBuffer.CanRead(32))
							{
								InternalLogStreams.LogTraceHostMigration?.Warn($"Migration {networkId}: Not enough data to read offset");
								break;
							}
							int num3 = netBitBuffer.ReadInt32VarLength();
							if (num3 == int.MaxValue)
							{
								InternalLogStreams.LogTraceHostMigration?.Log($"Migration {networkId}: End Mark at {netBitBuffer.OffsetBits}");
								break;
							}
							num2 += num3;
							if (num2 < 0 || num2 >= value.Header.WordCount)
							{
								InternalLogStreams.LogTraceHostMigration?.Error($"WordOffset {num3} exceeds WordCount {value.Header.WordCount} for {value.Header.Id}");
								while (!netBitBuffer.DoneOrOverflow && netBitBuffer.CanRead(32) && netBitBuffer.ReadInt32VarLength() != int.MaxValue)
								{
								}
								break;
							}
							if (!netBitBuffer.CanRead(32))
							{
								InternalLogStreams.LogTraceHostMigration?.Warn($"Migration {networkId}: Not enough data to read data");
								break;
							}
							int num4 = netBitBuffer.ReadInt32VarLength();
							raw[num2] = num4;
						}
						InternalLogStreams.LogTraceHostMigration?.Log($"Migration {value.Header.Id}: " + $"WordCount={value.Header.WordCount}, " + $"Type={value.Header.Type}, " + $"CRC={value.BuildCRC()}");
					}
					catch (Exception message)
					{
						InternalLogStreams.LogTraceHostMigration?.Log($"Migration {networkId}: Failed to read data");
						InternalLogStreams.LogTraceHostMigration?.Error(message);
						break;
					}
				}
				InternalLogStreams.LogTraceHostMigration?.Log($"Total {networkObjectMap.Count} on storage");
			}
		}
	}

	internal class StateReplicator
	{
		private enum WriteResult
		{
			Written,
			NothingToSend,
			PacketFull
		}

		internal const int DATA_BLOCK_SIZE = 6;

		internal const int OFFSET_BLOCK_SIZE = 4;

		protected const int HEADER_BLOCK_SIZE = 8;

		protected const int GLOBAL_BLOCK_SIZE = 8;

		private readonly Simulation _simulation;

		private readonly List<NetworkObjectMeta.List> _aoiQuery;

		private readonly bool _notUsingAreaOfInterest;

		private readonly SimulationConfig.DataConsistency _dataConsistency;

		private HashSet<int> _changedWords = new HashSet<int>();

		private bool _loggedWordCheck;

		private bool _logged0 = false;

		private NetworkObjectMeta _runtimeConfig;

		private NetworkObjectMeta _sceneInfo;

		private NetworkObjectMeta _physicsInfo;

		public StateReplicator(Simulation simulation)
		{
			_simulation = simulation;
			_dataConsistency = _simulation._config.ObjectDataConsistency;
			_aoiQuery = new List<NetworkObjectMeta.List>();
			_notUsingAreaOfInterest = (_simulation.Config.ReplicationFeatures & NetworkProjectConfig.ReplicationFeatures.SchedulingAndInterestManagement) == 0;
		}

		public unsafe void SendPacket()
		{
			if (_simulation.Topology == Topologies.Shared || _simulation.Mode != SimulationModes.Client)
			{
				SendContext sendContext = _simulation._sendContext;
				NetBitBuffer.Offset offset = NetBitBuffer.GetOffset(sendContext.Buffer);
				sendContext.Header.Tick = _simulation.Tick;
				WriteObjectDestroys();
				WriteStructs();
				if (_simulation.Config.SchedulingEnabled)
				{
					Profiler.BeginSample("WriteUsingScheduling");
					WriteUsingScheduling();
					Profiler.EndSample();
				}
				else
				{
					WriteUsingAllObjects();
				}
				_simulation._fusionStatsManager.PendingSnapshot.AddToOutObjectUpdatesStat(sendContext.Header.ObjectUpdates);
			}
		}

		public void RecvPacket()
		{
			if (_simulation.Topology == Topologies.Shared || _simulation.Mode == SimulationModes.Client)
			{
				RecvContext recvContext = _simulation._recvContext;
				ReadObjectDestroys();
				ReadObjectUpdates();
				_simulation._fusionStatsManager.PendingSnapshot.AddToInObjectUpdatesStat(recvContext.Header.ObjectUpdates);
			}
		}

		private unsafe NetworkObjectHeader ReadHeader(RecvContext rc, NetworkId id)
		{
			NetworkObjectHeader result = default(NetworkObjectHeader);
			Span<int> span = new Span<int>(&result, 20);
			span[0] = (int)id.Raw;
			for (int i = 1; i < 20; i++)
			{
				if (rc.Buffer->ReadBoolean())
				{
					span[i] = (int)Maths.ZigZagDecode(rc.Buffer->ReadInt64VarLength(6));
					_simulation._fusionStatsManager.PendingSnapshot.AddToWordsReadCountStat(1);
				}
			}
			return result;
		}

		private unsafe void SkipObjectData(RecvContext rc, NetworkBufferSerializerInfo[] serializers, int word, bool clearChangedWords)
		{
			Assert.Check(serializers != null);
			if (clearChangedWords)
			{
				_changedWords.Clear();
			}
			int num = 0;
			while ((num = rc.Buffer->ReadInt32VarLength(4)) > 0)
			{
				word += num;
				_changedWords.Add(word);
				if (word >= 0 && word < serializers.Length && serializers[word].Serializer != null)
				{
					word = serializers[word].Serializer.Skip(rc, word);
				}
				else
				{
					rc.Buffer->ReadInt64VarLength(6);
				}
			}
		}

		private unsafe void SkipObject(RecvContext rc, NetworkId id, NetworkObjectMeta meta, bool skipHeader)
		{
			_changedWords.Clear();
			NetworkBufferSerializerInfo[] array = meta?.Serializers;
			if (array == null || array.Length == 0)
			{
				array = NetworkObjectMeta.GetSerializers(rc.Connection.PendingDeleteMainTRSP.Contains(id));
			}
			int word = 0;
			if (skipHeader)
			{
				for (int i = 1; i < 20; i++)
				{
					if (rc.Buffer->ReadBoolean())
					{
						_changedWords.Add(i);
						rc.Buffer->ReadInt64VarLength(6);
					}
				}
				word = 19;
			}
			SkipObjectData(rc, array, word, clearChangedWords: false);
		}

		private void ForceResendChangedWords(RecvContext rc, NetworkId id)
		{
			Assert.Check(_simulation.Topology == Topologies.Shared);
			_changedWords.Clear();
		}

		private unsafe bool ReadObjectDataIntoPtr(NetworkObjectMeta meta, Span<int> p, int word)
		{
			RecvContext recvContext = _simulation._recvContext;
			NetworkBufferSerializerInfo[] serializers = meta.Serializers;
			NetBitBuffer* buffer = recvContext.Buffer;
			int num = 0;
			int num2 = serializers.Length;
			int num3 = 0;
			bool flag = false;
			while (buffer->MoreToRead && (num = buffer->ReadInt32VarLength(4)) > 0)
			{
				Assert.Check(!buffer->DoneOrOverflow);
				if (buffer->DoneOrOverflow)
				{
					InternalLogStreams.LogDebug?.Error("Buffer DoneOrOverflow");
					return false;
				}
				word += num;
				if (word < 0 || word >= meta.WordCount)
				{
					InternalLogStreams.LogError?.Once(ref _loggedWordCheck)?.Log($"Word check: Id={meta.Id}, Type={meta.Type}, SA={meta.StateAuthority}, {word} > {meta.WordCount}. {recvContext.Player}");
					return false;
				}
				if (word < 7)
				{
					int num4 = (int)Maths.ZigZagDecode(buffer->ReadInt64VarLength(6));
					if (num4 != p[word])
					{
						InternalLogStreams.LogError?.Once(ref flag)?.Log($"tried to write over header's read-only part (word:{word}, value:{num4}, orig-value:{p[word]}, header: {meta.Header})");
					}
					continue;
				}
				if (word < num2 && serializers[word].Serializer != null)
				{
					word = serializers[word].Serializer.Read(recvContext, meta, serializers[word], p, word);
				}
				else
				{
					int num5 = (int)Maths.ZigZagDecode(buffer->ReadInt64VarLength(6));
					if (buffer->Overflow)
					{
						InternalLogStreams.LogDebug?.Error("Buffer Overflow");
						return false;
					}
					p[word] = num5;
				}
				num3++;
			}
			_simulation._fusionStatsManager.PendingSnapshot.AddToWordsReadCountStat(num3);
			return true;
		}

		private unsafe void ReadObjectUpdates()
		{
			RecvContext recvContext = _simulation._recvContext;
			int num = 0;
			for (int i = 0; i < recvContext.Header.ObjectUpdates; i++)
			{
				int offsetBits = recvContext.Buffer->OffsetBits;
				Assert.Check(!recvContext.Buffer->DoneOrOverflow);
				int word = 0;
				int num2 = num + Maths.ZigZagDecode(recvContext.Buffer->ReadInt32VarLength(6));
				NetworkId networkId = new NetworkId((uint)num2);
				num = num2;
				bool flag = recvContext.Buffer->ReadBoolean();
				NetworkObjectMeta meta;
				bool flag2 = _simulation.TryGetMeta(networkId, out meta);
				Assert.Check(flag2 == (meta != null));
				bool flag3 = false;
				int offsetBits2 = recvContext.Buffer->OffsetBits;
				if ((meta != null && networkId == NetworkId.SceneInfo && _simulation.IsLocalSimulationStateAuthority(ref meta.Header)) || (_simulation.Topology == Topologies.ClientServer && recvContext.Connection.ObjectData_IsDestroyUnconfirmed(networkId).GetValueOrDefault()))
				{
					SkipObject(recvContext, networkId, meta, flag);
					continue;
				}
				NetworkObjectHeader header = default(NetworkObjectHeader);
				if (flag)
				{
					header = ReadHeader(recvContext, networkId);
					word = 19;
					if (!flag2)
					{
						meta = _simulation.AllocateObject(in header);
						_simulation._callbacks.RemoteObjectCreated(meta);
						flag2 = (flag3 = true);
						if (_simulation.Topology == Topologies.Shared)
						{
							recvContext.Connection.GetObjectData(meta.Id, create: true).Status = NetworkObjectConnectionDataStatus.CreatedConfirmed;
						}
					}
					else if (header.WordCount != meta.WordCount)
					{
						InternalLogStreams.LogError?.Log($"Unconfirmed header word count mismatch: {header.WordCount} != {meta.WordCount}");
					}
				}
				else if (!flag2)
				{
					InternalLogStreams.LogWarn?.Once(ref _logged0)?.Log($"Object does not exist: {networkId}, but is marked as confirmed. This indicates invalid data from the remote host");
					SkipObject(recvContext, networkId, null, flag);
					continue;
				}
				Assert.Check(flag2);
				NetworkObjectHeaderSnapshotRef snapshot = meta.NextSnapshot(recvContext.Header.Tick);
				if (flag)
				{
					snapshot.Header = header;
				}
				if (!ReadObjectDataIntoPtr(meta, snapshot.Raw, word))
				{
					recvContext.Buffer->OffsetBits = offsetBits2;
					SkipObject(recvContext, networkId, meta, flag);
					continue;
				}
				if (_simulation.Topology != Topologies.Shared)
				{
					Span<int> behaviourChangedTickArray = meta.GetBehaviourChangedTickArray(snapshot);
					for (int j = 0; j < snapshot.Header.BehaviourCount; j++)
					{
						behaviourChangedTickArray[j] = recvContext.Header.Tick;
					}
				}
				_simulation._fusionStatsManager.ObjectStatisticsManager.AddToNetworkObjectInBandwidth(meta.Id, Maths.BytesRequiredForBits(recvContext.Buffer->OffsetBits - offsetBits));
				_simulation._fusionStatsManager.ObjectStatisticsManager.AddToNetworkObjectInPackets(meta.Id, 1);
				bool? flag4 = null;
				bool? flag5 = null;
				if (_simulation.Topology == Topologies.Shared)
				{
					SimulationConnection simulationConnectionForPlayer = _simulation.GetSimulationConnectionForPlayer(_simulation.LocalPlayer);
					NetworkObjectConnectionData objectData = simulationConnectionForPlayer.GetObjectData(meta.Id, create: true);
					bool flag6 = _simulation.IsStateAuthority(snapshot.Header.StateAuthority, _simulation.LocalPlayer);
					bool flag7 = _simulation.IsStateAuthority(meta.StateAuthority, _simulation.LocalPlayer);
					bool flag8 = meta.StateAuthority != snapshot.Header.StateAuthority;
					if (flag6 && (!flag7 || flag3))
					{
						snapshot.CopyTo(meta);
						snapshot.CopyTo(meta.Previous);
						snapshot.CopyTo(meta.Shadow);
						simulationConnectionForPlayer.SetActive(objectData, meta);
						if (!meta.IsStruct)
						{
							flag5 = true;
							_simulation._callbacks.ObjectIsSimulatedChanged(meta.Id, simulated: true);
						}
					}
					else if (!flag6 && flag7)
					{
						meta.Timeline.Clear();
						simulationConnectionForPlayer.SetIdle(objectData);
						if (!meta.IsStruct)
						{
							snapshot.CopyTo(meta);
							snapshot.CopyTo(meta.Previous);
							flag5 = false;
							_simulation._callbacks.ObjectIsSimulatedChanged(meta.Id, simulated: false);
						}
					}
					else if (flag8)
					{
						flag5 = false;
					}
				}
				else if (snapshot.Header.InputAuthority == _simulation.LocalPlayer && (meta.InputAuthority != _simulation.LocalPlayer || flag3))
				{
					flag4 = true;
					_simulation._callbacks.ObjectIsSimulatedChanged(meta.Id, simulated: true);
				}
				else if (snapshot.Header.InputAuthority != _simulation.LocalPlayer && meta.InputAuthority == _simulation.LocalPlayer)
				{
					flag4 = false;
					_simulation._callbacks.ObjectIsSimulatedChanged(meta.Id, simulated: false);
				}
				if (flag3)
				{
					snapshot.CopyTo(meta);
					snapshot.CopyTo(meta.Previous);
					if (_simulation.Topology == Topologies.Shared && _simulation.IsStateAuthority(snapshot.Header.StateAuthority, _simulation.LocalPlayer))
					{
						snapshot.CopyTo(meta.Shadow);
					}
				}
				else if (snapshot.Header.Type.IsStruct || !_simulation.IsSimulated(meta))
				{
					snapshot.CopyTo(meta);
				}
				if (flag3 && meta.Type == NetworkObjectTypeId.PlayerData)
				{
					PlayerSimulationData* dataAs = meta.GetDataAs<PlayerSimulationData>();
					Assert.Check(dataAs->Player.IsRealPlayer, dataAs->Player);
					_simulation._invokeJoinedLeaveQueue.Enqueue((dataAs->Player, true));
					if (!_simulation.IsServer)
					{
						_simulation._players.Add(dataAs->Player);
					}
				}
				_simulation._callbacks.ObjectChanged(recvContext.Player, meta, (!flag3) ? ObjectChangeType.Updated : ObjectChangeType.Created);
				if ((bool)meta.Instance)
				{
					NetworkBehaviour[] networkedBehaviours = meta.Instance.NetworkedBehaviours;
					bool flag9 = (meta.PlayerData.Flags & NetworkObjectHeaderPlayerDataFlags.AllInterestFlags) != 0;
					bool flag10 = (snapshot.Header.PlayerData.Flags & NetworkObjectHeaderPlayerDataFlags.AllInterestFlags) != 0;
					if (flag9 != flag10)
					{
						if (flag10)
						{
							try
							{
								if (_simulation.IsClient && _simulation.HasRuntimeConfig)
								{
									meta.Timeline.Clear();
								}
								for (int k = 0; k < networkedBehaviours.Length; k++)
								{
									if (networkedBehaviours[k] is IInterestEnter interestEnter)
									{
										interestEnter.InterestEnter(_simulation.LocalPlayer);
									}
								}
							}
							catch (Exception error)
							{
								InternalLogStreams.LogException?.Log(error);
							}
						}
						else
						{
							try
							{
								for (int l = 0; l < networkedBehaviours.Length; l++)
								{
									if (networkedBehaviours[l] is IInterestExit interestExit)
									{
										interestExit.InterestExit(_simulation.LocalPlayer);
									}
								}
							}
							catch (Exception error2)
							{
								InternalLogStreams.LogException?.Log(error2);
							}
						}
					}
				}
				meta.PlayerData = meta.SnapshotLatest.Header.PlayerData;
				if (flag5.HasValue)
				{
					_simulation._callbacks.ObjectStateAuthorityChanged(meta.Id, flag5.Value);
				}
				if (flag4.HasValue)
				{
					_simulation._callbacks.ObjectInputAuthorityChanged(meta.Id, flag4.Value);
				}
			}
		}

		public void OnObjectSpawnedLocal(NetworkId id)
		{
			if (!_simulation.IsClient && (!_simulation.Config.SchedulingEnabled || _simulation.Config.AreaOfInterestEnabled))
			{
				return;
			}
			if (_simulation.IsClient)
			{
				Assert.Check(_simulation.Topology == Topologies.Shared);
				_simulation.GetSimulationConnectionForPlayer(_simulation.LocalPlayer)?.GetObjectData(id, create: true);
				return;
			}
			foreach (SimulationConnection connection in _simulation.Connections)
			{
				connection.GetObjectData(id, create: true);
			}
		}

		private unsafe void WriteObjectDestroys()
		{
			SendContext sendContext = _simulation._sendContext;
			int num = Math.Min(_simulation._config.MaxObjectDestroysSentPerPacket, sendContext.Connection.DestroysPending);
			for (int i = 0; i < num; i++)
			{
				if (!sendContext.Connection.DestroyedNextId(out var id))
				{
					break;
				}
				id.Write(sendContext.Buffer);
				sendContext.Header.ObjectDestroys++;
				sendContext.Envelope->AddObjectPacketData(_simulation, id, default(Tick), NetworkObjectPacketFlags.Destroy);
			}
		}

		private unsafe void ReadObjectDestroys()
		{
			RecvContext recvContext = _simulation._recvContext;
			int objectDestroys = recvContext.Header.ObjectDestroys;
			for (int i = 0; i < objectDestroys; i++)
			{
				NetworkId id = NetworkId.Read(recvContext.Buffer);
				if (!_simulation._callbacks.RemoteObjectDestroyed(id))
				{
					_simulation.Destroy(id, NetworkObjectDestroyFlags.DestroyState, recvContext.Player);
				}
			}
		}

		private void WriteUsingAllObjects()
		{
			EngineProfiler.Begin("WriteUsingAllObjects");
			foreach (KeyValuePair<NetworkId, NetworkObjectMeta> item in _simulation._metaLookup)
			{
				Assert.Check(item.Key == item.Value.Id, item.Key, item.Value.Id);
				if ((item.Value.Flags & NetworkObjectHeaderFlags.Struct) != NetworkObjectHeaderFlags.Struct)
				{
					WriteResult writeResult = ScanAndWriteObject(item.Value, null);
					if (writeResult == WriteResult.PacketFull)
					{
						break;
					}
				}
			}
			EngineProfiler.End();
		}

		private void WriteLevelUsingScheduling(int level, ref NetworkObjectConnectionData sent)
		{
			SendContext sendContext = _simulation._sendContext;
			Tick tick = _simulation.Tick;
			NetworkObjectConnectionData networkObjectConnectionData = sendContext.Connection.ObjectPriorityList.GetLevelList(level).Head;
			bool isClient = _simulation.IsClient;
			while (networkObjectConnectionData != null)
			{
				NetworkObjectConnectionData networkObjectConnectionData2 = networkObjectConnectionData;
				networkObjectConnectionData = networkObjectConnectionData.Next;
				if (networkObjectConnectionData2.MetaCache == null || networkObjectConnectionData2.MetaCache.Id != networkObjectConnectionData2.Id)
				{
					networkObjectConnectionData2.MetaCache = _simulation.GetMeta(networkObjectConnectionData2.Id);
				}
				NetworkObjectMeta metaCache = networkObjectConnectionData2.MetaCache;
				if (isClient && !_simulation.IsLocalSimulationStateAuthority(ref metaCache.Header))
				{
					sendContext.Connection.SetIdle(networkObjectConnectionData2);
					continue;
				}
				if (metaCache.ScannedTick == tick)
				{
					int num = networkObjectConnectionData2.UniqueDataChanges.MaxTick;
					if (num < metaCache.ChangedTick)
					{
						num = metaCache.ChangedTick;
					}
					if (num > 0 && num <= networkObjectConnectionData2.TickSent)
					{
						continue;
					}
				}
				if (ScanAndWriteObject(metaCache, networkObjectConnectionData2) == WriteResult.Written)
				{
					sendContext.Connection.ObjectPriorityList.RemoveSent(networkObjectConnectionData2);
					networkObjectConnectionData2.PriorityLevel = metaCache.GetPriority(sendContext.Player);
					networkObjectConnectionData2.Next = sent;
					sent = networkObjectConnectionData2;
				}
			}
		}

		private void WriteUsingScheduling()
		{
			SendContext sendContext = _simulation._sendContext;
			NetworkObjectConnectionData sent = null;
			for (int i = 0; i < 5; i++)
			{
				WriteLevelUsingScheduling(i, ref sent);
			}
			sendContext.Connection.ObjectPriorityList.IncreasePriorities();
			while (sent != null)
			{
				NetworkObjectConnectionData networkObjectConnectionData = sent;
				sent = sent.Next;
				networkObjectConnectionData.Next = null;
				networkObjectConnectionData.Prev = null;
				sendContext.Connection.ObjectPriorityList.Add(networkObjectConnectionData);
			}
		}

		internal bool HasObjectInterest(PlayerRef player, NetworkId id)
		{
			if (_notUsingAreaOfInterest)
			{
				return true;
			}
			SimulationConnection simulationConnection = _simulation?.GetSimulationConnectionForPlayer(player);
			if (simulationConnection != null && !simulationConnection.AreaOfInterestHasBeenUpdated)
			{
				return true;
			}
			if (simulationConnection.TryGetObjectData(id, out var data) && data.HasAnyPlayerDataFlag(NetworkObjectHeaderPlayerDataFlags.AllInterestFlags))
			{
				return NetworkObjectMeta.IsActive(data.PriorityLevel);
			}
			return false;
		}

		internal void UpdateChangedStructSet()
		{
			_simulation.TryGetMeta(NetworkId.RuntimeConfig, out _runtimeConfig);
			_simulation.TryGetMeta(NetworkId.SceneInfo, out _sceneInfo);
			_simulation.TryGetMeta(NetworkId.PhysicsInfo, out _physicsInfo);
			foreach (NetworkId @struct in _simulation._structs)
			{
				if (_simulation.TryGetMeta(@struct, out var meta))
				{
					ScanStructForChanges(meta);
				}
			}
		}

		private void WriteStructs()
		{
			SendContext sendContext = _simulation._sendContext;
			SimulationConnection connection = sendContext.Connection;
			if (_runtimeConfig != null)
			{
				NetworkObjectConnectionData objectData = connection.GetObjectData(_runtimeConfig.Id, create: true);
				if (!CheckNothingToSendTicks(_runtimeConfig, objectData))
				{
					WriteObject(_runtimeConfig, objectData);
				}
			}
			if (_sceneInfo != null)
			{
				NetworkObjectConnectionData objectData2 = connection.GetObjectData(_sceneInfo.Id, create: true);
				if (!CheckNothingToSendTicks(_sceneInfo, objectData2))
				{
					WriteObject(_sceneInfo, objectData2);
				}
			}
			if (_physicsInfo != null)
			{
				NetworkObjectConnectionData objectData3 = connection.GetObjectData(_physicsInfo.Id, create: true);
				if (!CheckNothingToSendTicks(_physicsInfo, objectData3))
				{
					WriteObject(_physicsInfo, objectData3);
				}
			}
			if (_simulation._structsVersion != connection.ActiveStructsVersion)
			{
				connection.ActiveStructsVersion = _simulation._structsVersion;
				connection.ActiveStructs.Clear();
				foreach (NetworkId @struct in _simulation._structs)
				{
					if (_simulation.TryGetMeta(@struct, out var meta) && meta.Type == NetworkObjectTypeId.PlayerData)
					{
						NetworkObjectConnectionData objectData4 = connection.GetObjectData(@struct, create: true);
						objectData4.MetaCache = meta;
						connection.ActiveStructs.Add(objectData4);
					}
				}
			}
			for (int i = 0; i < 5; i++)
			{
				if (connection.ActiveStructs.Count <= 0)
				{
					break;
				}
				NetworkObjectConnectionData networkObjectConnectionData = connection.ActiveStructs[++connection.ActiveStructsIndex % connection.ActiveStructs.Count];
				int num = networkObjectConnectionData.UniqueDataChanges.MaxTick;
				if (num < networkObjectConnectionData.MetaCache.ChangedTick)
				{
					num = networkObjectConnectionData.MetaCache.ChangedTick;
				}
				if ((num <= 0 || !(num <= networkObjectConnectionData.TickSent)) && WriteObject(networkObjectConnectionData.MetaCache, networkObjectConnectionData) == WriteResult.PacketFull)
				{
					break;
				}
			}
		}

		private unsafe bool ScanStructForChanges(NetworkObjectMeta meta)
		{
			bool result = false;
			if (meta.ScannedTick < _simulation.Tick)
			{
				meta.ScannedTick = _simulation.Tick;
				int* changes = meta.Changes;
				Span<int> raw = meta.Shadow.Raw;
				Span<int> raw2 = meta.Raw;
				short wordCount = meta.WordCount;
				for (int i = 0; i < wordCount; i++)
				{
					if (raw[i] != raw2[i])
					{
						raw[i] = raw2[i];
						changes[i] = _simulation.Tick;
						result = true;
					}
				}
			}
			return result;
		}

		private unsafe WriteResult ScanAndWriteObject(NetworkObjectMeta meta, NetworkObjectConnectionData data)
		{
			if (!(_simulation is Server) && !_simulation.IsLocalSimulationStateAuthority(ref meta.Header))
			{
				return WriteResult.NothingToSend;
			}
			int num = 0;
			bool flag = (meta.Flags & NetworkObjectHeaderFlags.Struct) == 0 && BehaviourUtils.IsAlive(meta.Instance) && meta.Instance.NetworkedBehaviours != null && meta.Instance.NetworkedBehaviours.Length != 0;
			if (meta.ScannedTick < _simulation.Tick)
			{
				meta.ScannedTick = _simulation.Tick;
				int* changes = meta.Changes;
				Tick changedTick = meta.ChangedTick;
				Span<int> raw = meta.Shadow.Raw;
				Span<int> raw2 = meta.Raw;
				int i;
				for (i = 0; i < 20; i++)
				{
					if (raw[i] != raw2[i])
					{
						raw[i] = raw2[i];
						changes[i] = (changedTick = _simulation.Tick);
						num++;
					}
				}
				if (flag)
				{
					bool flag2 = _simulation.Topology == Topologies.Shared;
					NetworkBehaviour[] networkedBehaviours = meta.Instance.NetworkedBehaviours;
					Span<int> behaviourChangedTickArray = meta.BehaviourChangedTickArray;
					Assert.Check(networkedBehaviours.Length == meta.BehaviourCount);
					for (int j = 0; j < networkedBehaviours.Length; j++)
					{
						Assert.Check(networkedBehaviours[j].WordOffset >= i);
						i = networkedBehaviours[j].WordOffset;
						for (int num2 = networkedBehaviours[j].WordOffset + networkedBehaviours[j].WordCount; i < num2; i++)
						{
							if (raw[i] != raw2[i])
							{
								raw[i] = raw2[i];
								changes[i] = (changedTick = _simulation.Tick);
								num++;
								if (flag2)
								{
									behaviourChangedTickArray[j] = _simulation.Tick;
								}
							}
						}
					}
				}
				for (short wordCount = meta.WordCount; i < wordCount; i++)
				{
					if (raw[i] != raw2[i])
					{
						raw[i] = raw2[i];
						changes[i] = (changedTick = _simulation.Tick);
						num++;
					}
				}
				meta.ChangedTick = changedTick;
			}
			_simulation._fusionStatsManager.PendingSnapshot.AddToWordsWrittenCountStat(num);
			return WriteObject(meta, data);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private unsafe static void WriteWord(NetBitBuffer* buffer, ReadOnlySpan<int> ptr, int word, int previous)
		{
			Assert.Check(word - previous > 0);
			long num = ptr[word];
			num = (num >> 63) ^ (num << 1);
			uint num2 = (uint)(word - previous);
			ulong num3 = 0uL;
			int num4 = 0;
			int num5 = (Maths.BitScanReverse(num2) + 4) / 4;
			num3 |= (uint)(1 << num5 - 1 << num4);
			num4 += num5;
			num3 |= num2 << num4;
			num4 += num5 * 4;
			num5 = (Maths.BitScanReverse(num) + 6) / 6;
			num3 |= (uint)(1 << num5 - 1 << num4);
			num4 += num5;
			num3 |= (ulong)(num << num4);
			num4 += num5 * 6;
			if (num4 > 64)
			{
				buffer->WriteInt32VarLength(word - previous, 4);
				buffer->WriteInt64VarLength(Maths.ZigZagEncode((long)ptr[word]), 6);
			}
			else
			{
				buffer->WriteUInt64(num3, num4);
			}
			Assert.Check(!buffer->Overflow);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static bool CheckNothingToSendTicks(NetworkObjectMeta meta, NetworkObjectConnectionData data)
		{
			int num = data.UniqueDataChanges.MaxTick;
			if (num < meta.ChangedTick)
			{
				num = meta.ChangedTick;
			}
			return num > 0 && num <= data.TickSent;
		}

		private unsafe WriteResult WriteObject(NetworkObjectMeta meta, NetworkObjectConnectionData data)
		{
			SendContext sendContext = _simulation._sendContext;
			if (sendContext.Header.ObjectUpdates == byte.MaxValue)
			{
				return WriteResult.PacketFull;
			}
			if (data == null)
			{
				data = sendContext.Connection.GetObjectData(meta.Id, create: true);
			}
			if (CheckNothingToSendTicks(meta, data))
			{
				return WriteResult.NothingToSend;
			}
			Span<int> raw = meta.Raw;
			int* changes = meta.Changes;
			int offsetBits = sendContext.Buffer->OffsetBits;
			Assert.Check(!sendContext.Buffer->Overflow);
			short wordCount = meta.WordCount;
			int i = 1;
			int num = 0;
			if (_simulation is Server)
			{
				var (playerData, playerUniqueDataChanges) = data.GetPlayerData();
				meta.Header.PlayerData = playerData;
				for (int j = 0; j < 1; j++)
				{
					(changes + 9)[j] = playerUniqueDataChanges.Changes[j];
				}
			}
			Assert.Check(num <= wordCount, num, wordCount);
			int value = Maths.ZigZagEncode((int)meta.Id.Raw - sendContext.ObjPrev);
			sendContext.Buffer->WriteInt32VarLength(value, 6);
			bool value2 = data.Status == NetworkObjectConnectionDataStatus.CreatedUnconfirmed;
			NetworkBufferSerializerInfo[] serializers = meta.Serializers;
			int num2 = serializers.Length;
			Tick tick = ((_dataConsistency == SimulationConfig.DataConsistency.Eventual) ? data.TickSent : data.TickAcknowledged);
			if (sendContext.Buffer->WriteBoolean(value2))
			{
				for (; i < 20; i++)
				{
					if (sendContext.Buffer->WriteBoolean(raw[i] != 0))
					{
						sendContext.Buffer->WriteInt64VarLength(Maths.ZigZagEncode((long)raw[i]), 6);
					}
				}
				i = 20;
				num = i - 1;
			}
			else
			{
				for (; i < 20; i++)
				{
					if (changes[i] > tick)
					{
						WriteWord(sendContext.Buffer, raw, i, num);
						num = i;
					}
				}
			}
			Assert.Check(num <= wordCount, num, wordCount);
			ulong filter = data.Filter;
			if (filter != ulong.MaxValue && (bool)meta.Instance && meta.Instance.NetworkedBehaviours != null)
			{
				NetworkBehaviour[] networkedBehaviours = meta.Instance.NetworkedBehaviours;
				for (int k = 0; k < networkedBehaviours.Length; k++)
				{
					int num3 = networkedBehaviours[k].WordOffset + networkedBehaviours[k].WordCount;
					if ((filter & (ulong)(1L << networkedBehaviours[k].ObjectIndex)) == 0)
					{
						i = num3;
						continue;
					}
					for (i = networkedBehaviours[k].WordOffset; i < num3; i++)
					{
						if (changes[i] > tick)
						{
							if (i < num2 && serializers[i].Serializer != null)
							{
								i = serializers[i].Serializer.Write(sendContext, meta, serializers[i], raw, i, num);
							}
							else
							{
								WriteWord(sendContext.Buffer, raw, i, num);
							}
							num = i;
						}
					}
				}
			}
			Assert.Check(num <= wordCount, num, wordCount);
			for (; i < wordCount; i++)
			{
				if (changes[i] > tick)
				{
					if (i < num2 && serializers[i].Serializer != null)
					{
						i = serializers[i].Serializer.Write(sendContext, meta, serializers[i], raw, i, num);
					}
					else
					{
						WriteWord(sendContext.Buffer, raw, i, num);
					}
					num = i;
				}
			}
			Assert.Check(num <= wordCount, num, wordCount);
			if (_simulation.IsServer)
			{
				meta.Header.PlayerData = default(NetworkObjectHeader.PlayerUniqueData);
				for (int l = 0; l < 1; l++)
				{
					(changes + 9)[l] = 0;
				}
			}
			if (num == 0)
			{
				sendContext.Buffer->OffsetBits = offsetBits;
				data.TickSent = _simulation.Tick;
				return WriteResult.NothingToSend;
			}
			sendContext.Buffer->WriteInt32VarLength(0, 4);
			if (Maths.BytesRequiredForBits(sendContext.Buffer->OffsetBits) > 44880)
			{
				sendContext.Buffer->OffsetBits = offsetBits;
				return WriteResult.PacketFull;
			}
			sendContext.Header.ObjectUpdates++;
			sendContext.Envelope->AddObjectPacketData(_simulation, meta.Id, data.TickSent, (NetworkObjectPacketFlags)0);
			sendContext.ObjPrev = (int)meta.Id.Raw;
			data.TickSent = _simulation.Tick;
			_simulation._fusionStatsManager.ObjectStatisticsManager.AddToNetworkObjectOutBandwidth(meta.Id, Maths.BytesRequiredForBits(sendContext.Buffer->OffsetBits - offsetBits));
			_simulation._fusionStatsManager.ObjectStatisticsManager.AddToNetworkObjectOutPackets(meta.Id, 1);
			if (_simulation._config.Topology == Topologies.Shared && (bool)meta.Instance && meta.Instance.HasStateAuthority)
			{
				meta.NextSnapshot(_simulation.Tick).CopyFrom(meta);
				meta.AddLatestSnapshotToTimeline();
			}
			return WriteResult.Written;
		}

		public unsafe void OnPacketLost(NetConnection* c, SimulationPacketEnvelope* envelope)
		{
			if (envelope->ObjectDataCount <= 0)
			{
				return;
			}
			SimulationConnection simulationConnection = _simulation.GetSimulationConnection(c);
			for (int i = 0; i < envelope->ObjectDataCount; i++)
			{
				NetworkObjectPacketData networkObjectPacketData = envelope->ObjectData[i];
				if ((networkObjectPacketData.Flags & NetworkObjectPacketFlags.Destroy) == NetworkObjectPacketFlags.Destroy)
				{
					simulationConnection.ObjectData_Destroyed(networkObjectPacketData.Id);
					continue;
				}
				NetworkObjectConnectionData objectData = simulationConnection.GetObjectData(networkObjectPacketData.Id, create: false);
				if (objectData != null)
				{
					if (objectData.TickSent > networkObjectPacketData.ResetTick)
					{
						objectData.TickSent = networkObjectPacketData.ResetTick;
					}
					if (objectData.TickAcknowledged > networkObjectPacketData.ResetTick)
					{
						objectData.TickAcknowledged = networkObjectPacketData.ResetTick;
					}
				}
			}
		}

		public unsafe void OnPacketDelivered(NetConnection* c, SimulationPacketEnvelope* envelope)
		{
			SimulationConnection simulationConnection = _simulation.GetSimulationConnection(c);
			if (simulationConnection != null && envelope->Tick > 0)
			{
				simulationConnection._latestTickAcknowledged = envelope->Tick;
			}
			if (envelope->ObjectDataCount <= 0)
			{
				return;
			}
			bool flag = (_simulation.Config.ReplicationFeatures & NetworkProjectConfig.ReplicationFeatures.Scheduling) == NetworkProjectConfig.ReplicationFeatures.Scheduling;
			for (int i = 0; i < envelope->ObjectDataCount; i++)
			{
				NetworkObjectPacketData networkObjectPacketData = envelope->ObjectData[i];
				if ((networkObjectPacketData.Flags & NetworkObjectPacketFlags.Destroy) == NetworkObjectPacketFlags.Destroy)
				{
					simulationConnection.ObjectData_Remove(networkObjectPacketData.Id);
					continue;
				}
				NetworkObjectConnectionData objectData = simulationConnection.GetObjectData(networkObjectPacketData.Id, create: false);
				if (objectData != null)
				{
					objectData.TickAcknowledged = Math.Max(objectData.TickAcknowledged, envelope->Tick);
				}
				if (objectData != null && objectData.Status <= NetworkObjectConnectionDataStatus.CreatedConfirmed)
				{
					objectData.Status = NetworkObjectConnectionDataStatus.CreatedConfirmed;
					if (flag && !objectData.HasAnyPlayerDataFlag(NetworkObjectHeaderPlayerDataFlags.AllInterestFlags) && networkObjectPacketData.ResetTick >= objectData.TickMin && _simulation.TryGetMeta(networkObjectPacketData.Id, out var _))
					{
						simulationConnection.SetIdle(objectData);
					}
				}
			}
		}

		[Conditional("STATE_REPLICATOR_DEBUG")]
		private void AddDebugWord(int word, int value = 0)
		{
		}

		[Conditional("STATE_REPLICATOR_DEBUG")]
		private void ClearDebugWords()
		{
		}

		[Conditional("STATE_REPLICATOR_DEBUG")]
		private void DumpDebugWordsReceive(NetworkObjectMeta meta, bool unconfirmed, bool created)
		{
		}

		[Conditional("STATE_REPLICATOR_DEBUG")]
		private void DumpDebugWordsSend(NetworkObjectMeta meta)
		{
		}
	}

	[CompilerGenerated]
	private sealed class _003Cget_ActivePlayers_003Ed__138 : IEnumerable<PlayerRef>, IEnumerable, IEnumerator<PlayerRef>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private PlayerRef _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		public Simulation _003C_003E4__this;

		private Dictionary<int, SimulationConnection>.ValueCollection.Enumerator _003C_003Es__1;

		private SimulationConnection _003Cvalue_003E5__2;

		PlayerRef IEnumerator<PlayerRef>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003Cget_ActivePlayers_003Ed__138(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			int num = _003C_003E1__state;
			if (num == -3 || num == 2)
			{
				try
				{
				}
				finally
				{
					_003C_003Em__Finally1();
				}
			}
			_003C_003Es__1 = default(Dictionary<int, SimulationConnection>.ValueCollection.Enumerator);
			_003Cvalue_003E5__2 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			try
			{
				switch (_003C_003E1__state)
				{
				default:
					return false;
				case 0:
					_003C_003E1__state = -1;
					if (_003C_003E4__this.IsPlayer)
					{
						_003C_003E2__current = _003C_003E4__this.LocalPlayer;
						_003C_003E1__state = 1;
						return true;
					}
					goto IL_006a;
				case 1:
					_003C_003E1__state = -1;
					goto IL_006a;
				case 2:
					{
						_003C_003E1__state = -3;
						_003Cvalue_003E5__2 = null;
						goto IL_00e7;
					}
					IL_00e7:
					if (_003C_003Es__1.MoveNext())
					{
						_003Cvalue_003E5__2 = _003C_003Es__1.Current;
						_003C_003E2__current = _003C_003E4__this.Connection2Player(_003Cvalue_003E5__2);
						_003C_003E1__state = 2;
						return true;
					}
					_003C_003Em__Finally1();
					_003C_003Es__1 = default(Dictionary<int, SimulationConnection>.ValueCollection.Enumerator);
					break;
					IL_006a:
					if (!_003C_003E4__this.IsServer)
					{
						break;
					}
					_003C_003Es__1 = _003C_003E4__this._connections.Values.GetEnumerator();
					_003C_003E1__state = -3;
					goto IL_00e7;
				}
				return false;
			}
			catch
			{
				//try-fault
				((IDisposable)this).Dispose();
				throw;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		private void _003C_003Em__Finally1()
		{
			_003C_003E1__state = -1;
			((IDisposable)_003C_003Es__1).Dispose();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}

		[DebuggerHidden]
		IEnumerator<PlayerRef> IEnumerable<PlayerRef>.GetEnumerator()
		{
			_003Cget_ActivePlayers_003Ed__138 result;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				result = this;
			}
			else
			{
				result = new _003Cget_ActivePlayers_003Ed__138(0)
				{
					_003C_003E4__this = _003C_003E4__this
				};
			}
			return result;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<PlayerRef>)this).GetEnumerator();
		}
	}

	[CompilerGenerated]
	private sealed class _003Cget_Connections_003Ed__156 : IEnumerable<SimulationConnection>, IEnumerable, IEnumerator<SimulationConnection>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private SimulationConnection _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		public Simulation _003C_003E4__this;

		private Dictionary<int, SimulationConnection>.ValueCollection.Enumerator _003C_003Es__1;

		private SimulationConnection _003Cvalue_003E5__2;

		SimulationConnection IEnumerator<SimulationConnection>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003Cget_Connections_003Ed__156(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			int num = _003C_003E1__state;
			if (num == -3 || num == 1)
			{
				try
				{
				}
				finally
				{
					_003C_003Em__Finally1();
				}
			}
			_003C_003Es__1 = default(Dictionary<int, SimulationConnection>.ValueCollection.Enumerator);
			_003Cvalue_003E5__2 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			try
			{
				int num = _003C_003E1__state;
				if (num != 0)
				{
					if (num != 1)
					{
						return false;
					}
					_003C_003E1__state = -3;
					_003Cvalue_003E5__2 = null;
				}
				else
				{
					_003C_003E1__state = -1;
					if (!_003C_003E4__this.IsServer)
					{
						goto IL_00b8;
					}
					_003C_003Es__1 = _003C_003E4__this._connections.Values.GetEnumerator();
					_003C_003E1__state = -3;
				}
				if (_003C_003Es__1.MoveNext())
				{
					_003Cvalue_003E5__2 = _003C_003Es__1.Current;
					_003C_003E2__current = _003Cvalue_003E5__2;
					_003C_003E1__state = 1;
					return true;
				}
				_003C_003Em__Finally1();
				_003C_003Es__1 = default(Dictionary<int, SimulationConnection>.ValueCollection.Enumerator);
				goto IL_00b8;
				IL_00b8:
				return false;
			}
			catch
			{
				//try-fault
				((IDisposable)this).Dispose();
				throw;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		private void _003C_003Em__Finally1()
		{
			_003C_003E1__state = -1;
			((IDisposable)_003C_003Es__1).Dispose();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}

		[DebuggerHidden]
		IEnumerator<SimulationConnection> IEnumerable<SimulationConnection>.GetEnumerator()
		{
			_003Cget_Connections_003Ed__156 result;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				result = this;
			}
			else
			{
				result = new _003Cget_Connections_003Ed__156(0)
				{
					_003C_003E4__this = _003C_003E4__this
				};
			}
			return result;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<SimulationConnection>)this).GetEnumerator();
		}
	}

	private Dictionary<int, AreaOfInterestCell> _aoiCells = new Dictionary<int, AreaOfInterestCell>();

	private Stack<AreaOfInterestCell> _aoiCellsPool = new Stack<AreaOfInterestCell>();

	private Dictionary<int, HashSet<int>> _aoiConnections = new Dictionary<int, HashSet<int>>();

	private ulong _interpolateSequence;

	private bool _isShutdown;

	private bool _isWaitingForShutdown;

	internal NetworkRunner Runner;

	private ICallbacks _callbacks;

	private Tick _tick;

	private SimulationModes _mode;

	private SimulationStages _stage;

	private SimulationConfig _config;

	private NetworkProjectConfig _projectConfig;

	internal ITimeProvider _time;

	private Tick _interpTo;

	private Tick _interpFrom;

	private float _remoteAlpha;

	private float _localAlpha;

	private Tick _interpToPrev;

	private Tick _interpFromPrev;

	private float _remoteAlphaPrev;

	private float _localAlphaPrev;

	private SimulationInput _inputRoot;

	private SimulationInput.Pool _inputPool;

	private SimulationInputCollection _inputCollection;

	private StateReplicator _stateReplicator;

	internal Dictionary<int, SimulationConnection> _connections = new Dictionary<int, SimulationConnection>();

	private Dictionary<PlayerRef, SimulationConnection> _playersConnections = new Dictionary<PlayerRef, SimulationConnection>(PlayerRef.Comparer);

	private HashSet<PlayerRef> _players;

	private double _updateTime;

	private bool _isResume;

	private Tick _sendTick;

	private bool _isLastTick;

	private bool _isFirstTick;

	private bool _isResimulation;

	private bool _isInTick;

	private bool _isInitialLocalTick;

	private bool? _isPaused;

	internal FusionStatisticsManager _fusionStatsManager;

	private Dictionary<Tick, double> _tickUpdateTimes;

	private Queue<(PlayerRef, bool)> _invokeJoinedLeaveQueue = new Queue<(PlayerRef, bool)>();

	private HashSet<NetworkId> _globalInterestObjects;

	private Dictionary<ulong, PlayerRefMapping> _uniqueIdPlayerRefMapping = new Dictionary<ulong, PlayerRefMapping>();

	private SendContext _sendContext;

	private RecvContext _recvContext;

	private int _reliableSend;

	internal INetSocket _netSocket;

	internal unsafe NetPeer* _netPeer;

	private unsafe NetPeerGroup* _netPeerGroup;

	private System.Random _netPeerRng;

	private Stack<NetworkObjectHeaderSnapshot> _snapshotsPool = new Stack<NetworkObjectHeaderSnapshot>();

	private uint _idCounter = 1023u;

	private Dictionary<NetworkId, NetworkObjectMeta> _metaLookup;

	private Dictionary<PlayerRef, NetworkId> _playerDataLookup;

	private Dictionary<PlayerRef, NetworkId> _playerLeftTempObjectCache;

	private HashSet<NetworkId> _structs;

	private int _structsVersion = 1;

	private Allocator _allocator;

	private Allocator _allocatorObjects;

	private readonly Dictionary<NetworkObjectTypeId, NetworkObjectMeta> _metaSceneLookup = new Dictionary<NetworkObjectTypeId, NetworkObjectMeta>(NetworkObjectTypeId.Comparer);

	private NetworkObjectMeta.ListMigration _metaMigration;

	private readonly Queue<NetworkId> _metaMigrationRemoved = new Queue<NetworkId>();

	public abstract Tick LatestServerTick { get; }

	internal unsafe SimulationRuntimeConfig RuntimeConfig => *RuntimeConfigPtr;

	internal unsafe SimulationRuntimeConfig* RuntimeConfigPtr
	{
		get
		{
			if (TryGetStructData<SimulationRuntimeConfig>(NetworkId.RuntimeConfig, out var data))
			{
				return data;
			}
			throw new InvalidOperationException("RuntimeConfig can only be read after the first state update form the server has arrived, add a guard check with Simulation.HasObject(NetworkId.RuntimeConfig)");
		}
	}

	internal ulong InterpolateSequence => _interpolateSequence;

	internal bool HasRuntimeConfig
	{
		get
		{
			NetworkObjectMeta meta;
			return TryGetStruct(NetworkId.RuntimeConfig, out meta);
		}
	}

	public int TickStride => IsServer ? RuntimeConfig.TickRate.ServerTickStride : RuntimeConfig.TickRate.ClientTickStride;

	public int TickRate => RuntimeConfig.TickRate.Client;

	public double TickDeltaDouble => RuntimeConfig.TickRate.ClientTickDelta;

	public float TickDeltaFloat => (float)TickDeltaDouble;

	public int SendRate => IsServer ? RuntimeConfig.TickRate.ServerSend : RuntimeConfig.TickRate.ClientSend;

	public double SendDelta => IsServer ? RuntimeConfig.TickRate.ServerSendDelta : RuntimeConfig.TickRate.ClientSendDelta;

	public float DeltaTime => IsServer ? ((float)RuntimeConfig.TickRate.ServerTickDelta) : ((float)RuntimeConfig.TickRate.ClientTickDelta);

	public bool IsShutdown => _isShutdown;

	public float LocalAlpha => _localAlpha;

	public bool IsResimulation => _isResimulation;

	public bool IsLastTick => _isLastTick;

	public bool IsFirstTick => _isFirstTick;

	public bool IsForward => !_isResimulation;

	public bool IsLocalPlayerFirstExecution => _stage == SimulationStages.Forward;

	public Tick Tick => _tick;

	public Tick TickPrevious => Math.Max(0, (int)_tick - TickStride);

	public double Time => (double)(int)_tick * TickDeltaDouble;

	public int InputCount => _inputCollection.Count;

	public Topologies Topology => _config.Topology;

	public SimulationModes Mode => _mode;

	public SimulationStages Stage => _stage;

	public SimulationConfig Config => _config;

	public NetworkProjectConfig ProjectConfig => _projectConfig;

	public float RemoteAlpha => _remoteAlpha;

	public Tick RemoteTickPrevious => _interpFrom;

	public Tick RemoteTick => _interpTo;

	public bool IsClient => this is Client;

	public bool IsServer => this is Server;

	public bool IsPlayer => _mode == SimulationModes.Client || _mode == SimulationModes.Host;

	public bool IsSinglePlayer => _mode == SimulationModes.Host && _config.PlayerCount == 1;

	public bool IsMasterClient => _callbacks.IsSharedModeMasterClient;

	public virtual IEnumerable<PlayerRef> ActivePlayers
	{
		[IteratorStateMachine(typeof(_003Cget_ActivePlayers_003Ed__138))]
		get
		{
			//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
			_003Cget_ActivePlayers_003Ed__138 _003Cget_ActivePlayers_003Ed__ = new _003Cget_ActivePlayers_003Ed__138(-2);
			_003Cget_ActivePlayers_003Ed__._003C_003E4__this = this;
			return _003Cget_ActivePlayers_003Ed__;
		}
	}

	public bool IsRunning => !_isShutdown;

	internal StateReplicator Replicator => _stateReplicator;

	internal ICallbacks Callbacks => _callbacks;

	internal bool IsResume => _isResume;

	internal bool IsInTick => _isInTick;

	internal bool IsPaused => _isPaused.HasValue && _isPaused.Value;

	internal bool IsWaitingForTheInitialTick => _isInitialLocalTick;

	internal bool IsSceneInfoReady => !IsWaitingForTheInitialTick || Topology != Topologies.Shared;

	internal IEnumerable<SimulationConnection> Connections
	{
		[IteratorStateMachine(typeof(_003Cget_Connections_003Ed__156))]
		get
		{
			//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
			_003Cget_Connections_003Ed__156 _003Cget_Connections_003Ed__ = new _003Cget_Connections_003Ed__156(-2);
			_003Cget_Connections_003Ed__._003C_003E4__this = this;
			return _003Cget_Connections_003Ed__;
		}
	}

	public unsafe NetAddress LocalAddress => _netPeer->Address;

	public unsafe NetConfig* NetConfigPointer => NetPeer.GetConfigPointer(_netPeer);

	public abstract PlayerRef LocalPlayer { get; }

	internal unsafe int ReliableDataSendRate
	{
		get
		{
			return (_netPeerGroup != null && _netPeerGroup->ReliableSendInterval != 0.0) ? ((int)(1.0 / _netPeerGroup->ReliableSendInterval)) : 0;
		}
		set
		{
			if (_netPeerGroup != null)
			{
				int sendRate = SendRate;
				if (value < 1)
				{
					InternalLogStreams.LogDebug?.Warn(this, $"Reliable Data Send Rate of {value}hz is too low, setting to {1}hz");
					value = 1;
				}
				if (value > sendRate)
				{
					InternalLogStreams.LogDebug?.Warn(this, $"Reliable Data Send Rate of {value}hz is too high, setting to {sendRate}hz");
					value = sendRate;
				}
				_netPeerGroup->ReliableSendInterval = 1f / (float)value;
				InternalLogStreams.LogDebug?.Log(this, $"Reliable Data Send Rate set to {value}hz");
			}
		}
	}

	internal uint IdCounter => _idCounter;

	public int ObjectCount => _metaLookup.Count;

	public Dictionary<NetworkId, NetworkObjectMeta> Objects => _metaLookup;

	public void GetAreaOfInterestGizmoData(List<(Vector3 center, Vector3 size, int playerCount, int objectCount)> result)
	{
		result.Clear();
		foreach (KeyValuePair<int, AreaOfInterestCell> aoiCell in _aoiCells)
		{
			Vector3 item = AreaOfInterest.ToCellCenter(aoiCell.Key);
			Vector3 item2 = Vector3.one * AreaOfInterest.CELL_SIZE;
			result.Add((item, item2, aoiCell.Value.Connections.GetSetCount(), aoiCell.Value.Objects.Count));
		}
	}

	public List<NetworkId> GetObjectsInAreaOfInterestForPlayer(PlayerRef player)
	{
		List<NetworkId> list = new List<NetworkId>();
		if (!Runner.IsServer || _config.ReplicationFeatures != NetworkProjectConfig.ReplicationFeatures.SchedulingAndInterestManagement)
		{
			return list;
		}
		if (!player.IsRealPlayer || player.IsNone)
		{
			InternalLogStreams.LogDebug?.Error($"Player {player} is not valid.");
			return list;
		}
		SimulationConnection simulationConnectionForPlayer = GetSimulationConnectionForPlayer(player);
		if (simulationConnectionForPlayer == null)
		{
			return list;
		}
		foreach (int areaOfInterestCell2 in simulationConnectionForPlayer.AreaOfInterestCells)
		{
			AreaOfInterestCell areaOfInterestCell = AOI_GetCell(areaOfInterestCell2, create: false);
			if (areaOfInterestCell != null)
			{
				for (NetworkObjectMeta networkObjectMeta = areaOfInterestCell.Objects.Head; networkObjectMeta != null; networkObjectMeta = NetworkObjectMeta.List.Next(networkObjectMeta))
				{
					list.Add(networkObjectMeta.Id);
				}
			}
		}
		return list;
	}

	public void GetObjectsAndPlayersInAreaOfInterestCell(int cellKey, List<PlayerRef> players, List<NetworkId> objects)
	{
		players.Clear();
		objects.Clear();
		AreaOfInterestCell areaOfInterestCell = AOI_GetCell(cellKey, create: false);
		if (areaOfInterestCell == null)
		{
			return;
		}
		for (NetworkObjectMeta networkObjectMeta = areaOfInterestCell.Objects.Head; networkObjectMeta != null; networkObjectMeta = NetworkObjectMeta.List.Next(networkObjectMeta))
		{
			objects.Add(networkObjectMeta.Id);
		}
		if (!areaOfInterestCell.Connections.Empty())
		{
			BitSet512.Iterator iterator = areaOfInterestCell.Connections.GetIterator();
			int index;
			while (iterator.Next(out index))
			{
				players.Add(GetSimulationConnectionByIndex(index).Player);
			}
		}
	}

	private AreaOfInterestCell AOI_GetCell(int cellKey, bool create)
	{
		Assert.Check(cellKey > 0);
		if (!_aoiCells.TryGetValue(cellKey, out var value))
		{
			if (!create)
			{
				return null;
			}
			if (_aoiCellsPool.Count > 0)
			{
				value = _aoiCellsPool.Pop();
				Assert.Check(value.Objects.Count == 0);
				Assert.Check(value.Connections.Empty());
			}
			else
			{
				value = new AreaOfInterestCell();
			}
			value.Key = cellKey;
			_aoiCells.Add(cellKey, value);
		}
		Assert.Check(value.Key == cellKey, value.Key, cellKey);
		return value;
	}

	private void AOI_ReleaseCell(AreaOfInterestCell cell)
	{
		if (cell.Empty)
		{
			int key = cell.Key;
			cell.Key = 0;
			Assert.Check(_aoiCells.ContainsKey(key));
			_aoiCells.Remove(key);
			_aoiCellsPool.Push(cell);
		}
	}

	internal void AOI_RemoveConnection(SimulationConnection sc)
	{
		int connectionIndex = sc.ConnectionIndex;
		if (!_aoiConnections.TryGetValue(connectionIndex, out var value))
		{
			return;
		}
		foreach (int item in value)
		{
			AreaOfInterestCell areaOfInterestCell = AOI_GetCell(item, create: false);
			Assert.Check(areaOfInterestCell != null);
			areaOfInterestCell.Connections.Clear(connectionIndex);
			if (areaOfInterestCell.Empty)
			{
				AOI_ReleaseCell(areaOfInterestCell);
			}
		}
		value.Clear();
	}

	internal void AOI_UpdateAreaOfInterest(SimulationConnection sc)
	{
		if (sc?.AreaOfInterestCells == null || (!sc.AreaOfInterestHasBeenUpdated && sc.AreaOfInterestCells.Count == 0))
		{
			return;
		}
		int connectionIndex = sc.ConnectionIndex;
		if (!_aoiConnections.TryGetValue(connectionIndex, out var value))
		{
			_aoiConnections.Add(connectionIndex, value = new HashSet<int>());
		}
		sc.AreaOfInterestHasBeenUpdated = true;
		HashSet<int> areaOfInterestCells = sc.AreaOfInterestCells;
		bool flag = false;
		foreach (int item in value)
		{
			if (areaOfInterestCells.Contains(item))
			{
				continue;
			}
			flag = true;
			AreaOfInterestCell areaOfInterestCell = AOI_GetCell(item, create: false);
			if (areaOfInterestCell != null)
			{
				Assert.Check(areaOfInterestCell.Connections.IsSet(connectionIndex));
				areaOfInterestCell.Connections.Clear(connectionIndex);
				NetworkObjectMeta networkObjectMeta = areaOfInterestCell.Objects.Head;
				while (networkObjectMeta != null)
				{
					NetworkObjectMeta networkObjectMeta2 = networkObjectMeta;
					networkObjectMeta = NetworkObjectMeta.List.Next(networkObjectMeta);
					ExitAreaOfInterest(sc, networkObjectMeta2.Id);
				}
				if (areaOfInterestCell.Empty)
				{
					AOI_ReleaseCell(areaOfInterestCell);
				}
			}
		}
		if (!flag && value.Count == areaOfInterestCells.Count)
		{
			return;
		}
		foreach (int item2 in areaOfInterestCells)
		{
			if (!value.Contains(item2))
			{
				AreaOfInterestCell areaOfInterestCell2 = AOI_GetCell(item2, create: true);
				Assert.Check(!areaOfInterestCell2.Connections.IsSet(connectionIndex));
				areaOfInterestCell2.Connections.Set(connectionIndex);
				NetworkObjectMeta networkObjectMeta3 = areaOfInterestCell2.Objects.Head;
				while (networkObjectMeta3 != null)
				{
					NetworkObjectMeta networkObjectMeta4 = networkObjectMeta3;
					networkObjectMeta3 = NetworkObjectMeta.List.Next(networkObjectMeta3);
					EnterAreaOfInterest(sc, networkObjectMeta4.Id);
				}
			}
		}
		value.Clear();
		value.UnionWith(areaOfInterestCells);
	}

	internal bool AOI_Query(SimulationConnection sc, List<NetworkObjectMeta.List> result, bool clearResult)
	{
		if (clearResult)
		{
			result.Clear();
		}
		int count = result.Count;
		if (_aoiConnections.TryGetValue(sc.ConnectionIndex, out var value))
		{
			foreach (int item in value)
			{
				AreaOfInterestCell areaOfInterestCell = AOI_GetCell(item, create: false);
				if (areaOfInterestCell.Objects.Count > 0)
				{
					result.Add(areaOfInterestCell.Objects);
				}
			}
		}
		return result.Count > count;
	}

	internal void AOI_RemoveFromAreaOfInterest(NetworkObjectMeta meta, bool invokeExit = false)
	{
		Assert.Check(meta.Id.IsValid);
		if (meta.AreaOfInterestCell == 0)
		{
			return;
		}
		AreaOfInterestCell areaOfInterestCell = AOI_GetCell(meta.AreaOfInterestCell, create: false);
		areaOfInterestCell.Objects.Remove(meta);
		if (invokeExit)
		{
			NetworkId id = meta.Id;
			BitSet512.Iterator iterator = areaOfInterestCell.Connections.GetIterator();
			int index;
			while (iterator.Next(out index))
			{
				ExitAreaOfInterest(index, id);
			}
		}
	}

	internal void AOI_UpdateAreaOfInterest(NetworkObjectMeta meta, int newCellKey)
	{
		if (meta.AreaOfInterestCell == newCellKey)
		{
			return;
		}
		NetworkId id = meta.Id;
		AreaOfInterestCell areaOfInterestCell;
		if (meta.AreaOfInterestCell > 0)
		{
			areaOfInterestCell = AOI_GetCell(meta.AreaOfInterestCell, create: false);
			areaOfInterestCell.Objects.Remove(meta);
			if (areaOfInterestCell.Empty)
			{
				AOI_ReleaseCell(areaOfInterestCell);
				areaOfInterestCell = null;
			}
		}
		else
		{
			areaOfInterestCell = null;
		}
		AreaOfInterestCell areaOfInterestCell2 = AOI_GetCell(newCellKey, create: true);
		meta.AreaOfInterestCell = newCellKey;
		areaOfInterestCell2.Objects.AddLast(meta);
		if (areaOfInterestCell != null)
		{
			if (!areaOfInterestCell.Connections.Equals(areaOfInterestCell2.Connections))
			{
				BitSet512.Iterator iterator = areaOfInterestCell.Connections.GetIterator();
				iterator._set.AndNot(areaOfInterestCell2.Connections);
				int index;
				while (iterator.Next(out index))
				{
					ExitAreaOfInterest(index, id);
				}
				BitSet512.Iterator iterator2 = areaOfInterestCell2.Connections.GetIterator();
				iterator2._set.AndNot(areaOfInterestCell.Connections);
				int index2;
				while (iterator2.Next(out index2))
				{
					EnterAreaOfInterest(index2, id);
				}
			}
		}
		else
		{
			BitSet512.Iterator iterator3 = areaOfInterestCell2.Connections.GetIterator();
			int index3;
			while (iterator3.Next(out index3))
			{
				EnterAreaOfInterest(index3, id);
			}
		}
	}

	internal void EnterAreaOfInterest(int connection, NetworkId id)
	{
		EnterAreaOfInterest(GetSimulationConnectionByIndex(connection), id);
	}

	internal void EnterAreaOfInterest(SimulationConnection connection, NetworkId id)
	{
		Assert.Always(TryGetMeta(id, out var meta), "Object not found");
		Assert.Always(meta.AreaOfInterestCell > 0, "AOI Cell not correct");
		NetworkObjectConnectionData objectData = connection.GetObjectData(id, create: true);
		if ((objectData.UniqueData.Flags & NetworkObjectHeaderPlayerDataFlags.AllInterestFlags) == 0)
		{
			_callbacks.ObjectEnterAOI(connection, id);
		}
		Assert.Check((objectData.UniqueData.Flags & NetworkObjectHeaderPlayerDataFlags.InAreaOfInterest) == 0, "Already has interest in");
		connection.SetActive(objectData, meta);
		objectData.SetPlayerDataFlag(NetworkObjectHeaderPlayerDataFlags.InAreaOfInterest, this);
	}

	internal void ExitAreaOfInterest(int connection, NetworkId id)
	{
		ExitAreaOfInterest(GetSimulationConnectionByIndex(connection), id);
	}

	internal void ExitAreaOfInterest(SimulationConnection connection, NetworkId id)
	{
		NetworkObjectConnectionData objectData = connection.GetObjectData(id, create: false);
		if (objectData != null)
		{
			if ((objectData.UniqueData.Flags & NetworkObjectHeaderPlayerDataFlags.AllInterestFlags) == NetworkObjectHeaderPlayerDataFlags.InAreaOfInterest)
			{
				_callbacks.ObjectExitAOI(connection, id);
			}
			if (TryGetMeta(id, out var meta) && meta.AreaOfInterestCell > 0)
			{
				connection.SetActive(objectData, meta);
			}
			if ((objectData.UniqueData.Flags & NetworkObjectHeaderPlayerDataFlags.InAreaOfInterest) == NetworkObjectHeaderPlayerDataFlags.InAreaOfInterest)
			{
				objectData.ClearPlayerDataFlag(NetworkObjectHeaderPlayerDataFlags.InAreaOfInterest, this);
			}
		}
	}

	internal void InterpolateSequenceIncrement()
	{
		_interpolateSequence++;
	}

	internal abstract double GetPlayerRtt(PlayerRef player);

	internal abstract void RecvPacket();

	internal abstract void WritePackets();

	internal abstract SimulationInput GetInput(Tick tick, PlayerRef player);

	internal unsafe Simulation(SimulationArgs args)
	{
		Assert.Check(sizeof(NetworkObjectHeader) == 80, "NetworkObjectHeader size != WORDS * REPLICATE_WORD_SIZE");
		_fusionStatsManager = new FusionStatisticsManager();
		_mode = args.Mode;
		_config = args.Config.Simulation;
		_projectConfig = args.Config;
		_allocator = Allocator.Create(_projectConfig.Heap.ToAllocatorConfig());
		_allocatorObjects = Allocator.Create(_projectConfig.Heap.ToAllocatorConfig());
		_callbacks = args.Callbacks;
		_isShutdown = false;
		_isWaitingForShutdown = false;
		_isInitialLocalTick = true;
		_inputPool = new SimulationInput.Pool(_config, _allocator);
		_inputRoot = _inputPool.Acquire();
		_inputCollection = new SimulationInputCollection(_config.PlayerCount);
		_players = new HashSet<PlayerRef>(PlayerRef.Comparer);
		_metaLookup = new Dictionary<NetworkId, NetworkObjectMeta>(NetworkId.Comparer);
		_playerDataLookup = new Dictionary<PlayerRef, NetworkId>();
		_playerLeftTempObjectCache = new Dictionary<PlayerRef, NetworkId>();
		_structs = new HashSet<NetworkId>(NetworkId.Comparer);
		_sendContext = new SendContext(this);
		_recvContext = new RecvContext(this);
		NetworkInit(args.Socket, args.Address);
		_stateReplicator = new StateReplicator(this);
		if ((Config.ReplicationFeatures & NetworkProjectConfig.ReplicationFeatures.SchedulingAndInterestManagement) == NetworkProjectConfig.ReplicationFeatures.SchedulingAndInterestManagement)
		{
			_globalInterestObjects = new HashSet<NetworkId>();
		}
		_tickUpdateTimes = new Dictionary<Tick, double>();
	}

	internal unsafe PlayerRef Connection2Player(SimulationConnection c)
	{
		Assert.Check(c);
		PlayerRef player = _connections[c.Connection->LocalId.GroupIndex].Player;
		Assert.Check(player == c.Player);
		Assert.Check(_connections[c.Connection->LocalId.GroupIndex] == c);
		return player;
	}

	internal unsafe virtual PlayerRef Connection2Player(NetConnection* c)
	{
		Assert.Check(c);
		return _connections[c->LocalId.GroupIndex].Player;
	}

	internal virtual int Player2Connection(PlayerRef player)
	{
		if (_playersConnections.TryGetValue(player, out var value))
		{
			return value.ConnectionIndex;
		}
		return -1;
	}

	internal void RegisterUniqueIdPlayerMapping(int actorid, byte[] id, PlayerRef playerRef)
	{
		Assert.Check(id);
		Assert.Check(id.Length == 8);
		_uniqueIdPlayerRefMapping[BitConverter.ToUInt64(id, 0)] = new PlayerRefMapping
		{
			ActorId = actorid,
			PlayerRef = playerRef
		};
		InternalLogStreams.LogInfo?.Log(this, $"RegisterUniqueIdPlayerMapping actorid:{actorid} id:{BitConverter.ToUInt64(id, 0)}, player:{playerRef}");
	}

	internal PlayerRefMapping? GetPlayerRefMapping(byte[] id)
	{
		Assert.Check(id);
		Assert.Check(id.Length == 8);
		if (_uniqueIdPlayerRefMapping.TryGetValue(BitConverter.ToUInt64(id, 0), out var value))
		{
			return value;
		}
		InternalLogStreams.LogWarn?.Log($"no player mapping for {BitConverter.ToUInt64(id, 0)} exists");
		return null;
	}

	internal unsafe PlayerRefMapping? GetPlayerRefMapping(byte* id)
	{
		Assert.Check(id);
		byte[] array = new byte[8];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = id[i];
		}
		return GetPlayerRefMapping(array);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void CalculateUpdateTime()
	{
		if (!IsClient)
		{
			double updateTime = _updateTime;
			_updateTime = _time.Now().Local;
			Assert.Check(_updateTime > updateTime, "Current Update Time must be bigger than previous Update Time {0} {1}", _updateTime, updateTime);
		}
	}

	private void StepSimulation(SimulationStages stage, bool lastTick, bool firstTick, bool freeInput)
	{
		EngineProfiler.Begin("Simulation.StepSimulation");
		try
		{
			bool isResimulation = stage == SimulationStages.Resimulate;
			_isLastTick = lastTick;
			_isFirstTick = firstTick;
			_isResimulation = isResimulation;
			if (IsLastTick && !IsResimulation)
			{
				Assert.Check(!IsResimulation, "IsResimulation should be false");
				_callbacks.OnBeforeCopyPreviousState();
				foreach (NetworkObjectMeta value in _metaLookup.Values)
				{
					if (value.IsObject)
					{
						value.Previous.CopyFrom(value);
					}
				}
			}
			_tick = _tick.Next(TickStride);
			if (IsServer)
			{
				_tickUpdateTimes.Remove((int)_tick - TickRate);
				_tickUpdateTimes.Add(_tick, _updateTime);
			}
			InvokeTick(stage, freeInput);
		}
		catch (Exception error)
		{
			InternalLogStreams.LogException?.Log(this, error);
		}
		finally
		{
			_isLastTick = false;
			_isFirstTick = false;
			_isResimulation = false;
			EngineProfiler.End();
		}
	}

	protected virtual void AfterUpdate()
	{
	}

	protected unsafe virtual void NetworkConnected(NetConnection* connection)
	{
	}

	protected unsafe virtual void NetworkDisconnected(NetConnection* connection, NetDisconnectReason reason)
	{
	}

	protected virtual void NetworkReceiveDone()
	{
	}

	protected virtual void NoSimulation()
	{
	}

	protected virtual int BeforeSimulation()
	{
		return 0;
	}

	protected virtual void BeforeFirstTick()
	{
	}

	internal void SinglePlayerSetPaused(bool paused)
	{
		if (IsSinglePlayer)
		{
			_isPaused = paused;
		}
	}

	internal unsafe void RequestStateAuthority(NetworkId id, bool wants)
	{
		if (Topology == Topologies.Shared)
		{
			Assert.Check(sizeof(SimulationMessageInternal_SharedModeRequestStateAuthority) == 8, "SharedModeRequestStateAuthority unexpected size");
			SimulationMessageInternal_SharedModeRequestStateAuthority buffer = default(SimulationMessageInternal_SharedModeRequestStateAuthority);
			buffer.Acquire = (wants ? 1 : 0);
			buffer.Object = id;
			SendInternalSimulationMessage(SimulationMessageInternalTypes.SharedModeRequestStateAuthority, buffer);
		}
	}

	internal unsafe void SetPlayerAlwaysInterested(PlayerRef player, NetworkId id, bool alwaysInterested)
	{
		if ((Config.ReplicationFeatures & NetworkProjectConfig.ReplicationFeatures.SchedulingAndInterestManagement) != NetworkProjectConfig.ReplicationFeatures.SchedulingAndInterestManagement)
		{
			return;
		}
		NetworkObjectMeta meta;
		if (Topology == Topologies.Shared)
		{
			Assert.Check(sizeof(SimulationMessageInternal_SharedModeSetAlwaysInterested) == 12, "SharedModeSetAlwaysInterested unexpected size");
			SimulationMessageInternal_SharedModeSetAlwaysInterested buffer = default(SimulationMessageInternal_SharedModeSetAlwaysInterested);
			buffer.Interested = (alwaysInterested ? 1 : 0);
			buffer.Object = id;
			buffer.Player = player.AsIndex;
			SendInternalSimulationMessage(SimulationMessageInternalTypes.SharedModeSetAlwaysInterested, buffer);
		}
		else if (!(LocalPlayer == player) && PlayerValid(player) && TryGetMeta(id, out meta))
		{
			if (alwaysInterested)
			{
				GetSimulationConnectionForPlayer(player)?.AddAlwaysInterested(meta);
			}
			else
			{
				GetSimulationConnectionForPlayer(player)?.RemoveAlwaysInterested(meta);
			}
		}
	}

	internal unsafe void TempFree<T>(ref T* ptr) where T : unmanaged
	{
		if (_allocator.IsPointerInHeap(ptr))
		{
			Allocator.Free(_allocator, ref ptr);
		}
		else
		{
			Assert.AlwaysFail("Pointer not part of temp allocator");
		}
	}

	[return: NotNull]
	internal unsafe void* TempAlloc(int size)
	{
		return Allocator.AllocAndClear(_allocator, size);
	}

	[return: NotNull]
	internal unsafe T* TempAlloc<T>() where T : unmanaged
	{
		return (T*)TempAlloc(sizeof(T));
	}

	[return: NotNull]
	internal unsafe T* TempAllocArray<T>(int length) where T : unmanaged
	{
		return (T*)TempAlloc(sizeof(T) * length);
	}

	[return: NotNull]
	internal unsafe T* TempDoubleArray<T>(ref T* oldArray, int oldLength) where T : unmanaged
	{
		int length = oldLength * 2;
		T* ptr = TempAllocArray<T>(length);
		Native.MemCpy(ptr, oldArray, sizeof(T) * oldLength);
		TempFree(ref oldArray);
		return ptr;
	}

	internal void ShutdownNativeSocket()
	{
		if (!_isShutdown)
		{
			NetworkShutdown();
		}
	}

	internal void Dispose()
	{
		if (!_isShutdown)
		{
			_isShutdown = true;
			_inputPool.Dispose();
			_inputPool = null;
			_inputRoot = null;
			Allocator.Dispose(_allocator);
			_allocator = null;
			Allocator.Dispose(_allocatorObjects);
			_allocatorObjects = null;
			HostMigrationDispose();
		}
	}

	internal void Destroy(NetworkId id, NetworkObjectDestroyFlags flags, PlayerRef destroyingPlayer = default(PlayerRef))
	{
		if (TryGetMeta(id, out var meta))
		{
			AOI_RemoveFromAreaOfInterest(meta);
		}
		InternalLogStreams.LogTraceObject?.Log(this, $"Destroy({id}, {flags})");
		if (!IsServer)
		{
			if (flags.Get(NetworkObjectDestroyFlags.DestroyState))
			{
				GetSimulationConnectionByIndex(0)?.ObjectData_Destroyed(id);
				FreeObject(id);
			}
			else if (flags.Get(NetworkObjectDestroyFlags.DestroyedByEngine))
			{
				if (meta != null)
				{
					InternalLogStreams.LogTraceObject?.Log(this, $"Requeuing {id}");
					_callbacks.RemoteObjectCreated(meta);
				}
			}
			else if (flags.Get(NetworkObjectDestroyFlags.DestroyedByReplicator))
			{
				GetSimulationConnectionByIndex(0)?.ObjectData_Destroyed(id);
			}
			return;
		}
		Assert.Check(flags.Get(NetworkObjectDestroyFlags.DestroyState));
		foreach (SimulationConnection value in _connections.Values)
		{
			value.ObjectData_Destroyed(id);
		}
		FreeObject(id);
	}

	internal bool PlayerValid(PlayerRef player)
	{
		return _players.Contains(player);
	}

	internal void PlayerAdd(PlayerRef player, SimulationConnection connection)
	{
		InternalLogStreams.LogInfo?.Log($"adding player {player}");
		Assert.Always(_players.Add(player), "player can't exist");
		if (connection != null)
		{
			connection.Player = player;
			_playersConnections.Add(player, connection);
		}
		else
		{
			Assert.Always(IsServer && Mode == SimulationModes.Host && LocalPlayer == player, "if no connection is given the playerref passed has to be the local player on host");
		}
	}

	internal void PlayerRemove(PlayerRef player)
	{
		Assert.Always(_players.Remove(player), "player must exist");
		if (!IsServer || Mode != SimulationModes.Host || !(LocalPlayer == player))
		{
			Assert.Always(_playersConnections.Remove(player), "player connection must exist");
		}
	}

	internal unsafe bool IsHostPlayer(PlayerRef player)
	{
		SimulationRuntimeConfig* data;
		return TryGetStructData<SimulationRuntimeConfig>(NetworkId.RuntimeConfig, out data) && data->HostPlayer == player;
	}

	public unsafe bool TryGetHostPlayer(out PlayerRef player)
	{
		if (TryGetStructData<SimulationRuntimeConfig>(NetworkId.RuntimeConfig, out var data))
		{
			player = data->HostPlayer;
			return true;
		}
		player = default(PlayerRef);
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool? IsInterestedIn(NetworkObjectMeta meta, PlayerRef player)
	{
		if (!Config.AreaOfInterestEnabled)
		{
			return true;
		}
		if (IsClient)
		{
			if (IsLocalSimulationStateAuthority(ref meta.Header))
			{
				return true;
			}
			if (LocalPlayer != player)
			{
				if (IsHostPlayer(player))
				{
					return true;
				}
				return null;
			}
			return (meta.PlayerData.Flags & NetworkObjectHeaderPlayerDataFlags.AllInterestFlags) != 0;
		}
		if (!PlayerValid(player))
		{
			return null;
		}
		if (IsHostPlayer(player))
		{
			return true;
		}
		SimulationConnection simulationConnectionForPlayer = GetSimulationConnectionForPlayer(player);
		if (simulationConnectionForPlayer != null && simulationConnectionForPlayer.TryGetObjectData(meta.Id, out var data))
		{
			return data.HasAnyPlayerDataFlag(NetworkObjectHeaderPlayerDataFlags.AllInterestFlags);
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe bool IsInputAuthority(PlayerRef inputAuthority, PlayerRef playerRef)
	{
		if (inputAuthority.IsNone)
		{
			return false;
		}
		if (inputAuthority == playerRef)
		{
			return true;
		}
		PlayerRef player;
		if (playerRef.IsNone)
		{
			return TryGetHostPlayer(out player) && player == inputAuthority;
		}
		SimulationRuntimeConfig* data;
		if (inputAuthority.IsMasterClient)
		{
			return TryGetStructData<SimulationRuntimeConfig>(NetworkId.RuntimeConfig, out data) && data->MasterClient == playerRef;
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool IsInputAuthority([NotNull] NetworkObjectMeta meta, PlayerRef playerRef)
	{
		return IsInputAuthority(meta.InputAuthority, playerRef);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe bool IsStateAuthority(PlayerRef stateSource, PlayerRef playerRef)
	{
		if (stateSource == playerRef)
		{
			return true;
		}
		PlayerRef player;
		if (stateSource.IsNone)
		{
			return TryGetHostPlayer(out player) && player == playerRef;
		}
		SimulationRuntimeConfig* data;
		if (stateSource.IsMasterClient)
		{
			return TryGetStructData<SimulationRuntimeConfig>(NetworkId.RuntimeConfig, out data) && data->MasterClient == playerRef;
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal bool IsStateAuthority([NotNull] NetworkObjectMeta meta, PlayerRef playerRef)
	{
		return IsStateAuthority(meta.StateAuthority, playerRef);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal bool IsLocalSimulationInputAuthority([In][RequiresLocation] ref NetworkObjectHeader obj)
	{
		return LocalPlayer.IsRealPlayer && obj.InputAuthority == LocalPlayer;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal bool IsLocalSimulationStateAuthority([In][RequiresLocation] ref NetworkObjectHeader obj)
	{
		return (Topology == Topologies.ClientServer) ? IsServer : IsStateAuthority(obj.StateAuthority, LocalPlayer);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe PlayerRef GetStateAuthority(PlayerRef objectStateAuthority)
	{
		if (objectStateAuthority == PlayerRef.None)
		{
			return PlayerRef.None;
		}
		if (Topology == Topologies.ClientServer)
		{
			return default(PlayerRef);
		}
		SimulationRuntimeConfig* data;
		if (Topology == Topologies.Shared && objectStateAuthority.IsMasterClient)
		{
			return TryGetStructData<SimulationRuntimeConfig>(NetworkId.RuntimeConfig, out data) ? data->MasterClient : default(PlayerRef);
		}
		return objectStateAuthority;
	}

	internal unsafe byte[] GetPlayerConnectionToken(PlayerRef player)
	{
		if (PlayerValid(player))
		{
			SimulationConnection simulationConnectionForPlayer = GetSimulationConnectionForPlayer(player);
			if (simulationConnectionForPlayer == null)
			{
				return null;
			}
			if (simulationConnectionForPlayer.Connection->ConnectionToken != null && simulationConnectionForPlayer.Connection->ConnectionTokenLength > 0)
			{
				byte[] array = new byte[simulationConnectionForPlayer.Connection->ConnectionTokenLength];
				fixed (byte* destination = array)
				{
					Native.MemCpy(destination, simulationConnectionForPlayer.Connection->ConnectionToken, simulationConnectionForPlayer.Connection->ConnectionTokenLength);
				}
				return array;
			}
		}
		return null;
	}

	internal unsafe NetAddress GetPlayerAddress(PlayerRef player)
	{
		if (PlayerValid(player))
		{
			SimulationConnection simulationConnectionForPlayer = GetSimulationConnectionForPlayer(player);
			if (simulationConnectionForPlayer != null)
			{
				return simulationConnectionForPlayer.Connection->Address;
			}
		}
		return default(NetAddress);
	}

	internal unsafe long GetPlayerUniqueId(PlayerRef player)
	{
		if (PlayerValid(player))
		{
			SimulationConnection simulationConnectionForPlayer = GetSimulationConnectionForPlayer(player);
			if (simulationConnectionForPlayer != null)
			{
				return simulationConnectionForPlayer.Connection->UniqueIdHash;
			}
		}
		return 0L;
	}

	public SimulationInput GetInputForPlayer(PlayerRef player)
	{
		return _inputCollection.GetByPlayer(player);
	}

	private unsafe void DeletePlayerSimulationDataOnDisconnect(PlayerRef player)
	{
		if (!IsClient)
		{
			PlayerSimulationData* playerSimulationData = GetPlayerSimulationData(player, create: false);
			if (playerSimulationData != null && _playerDataLookup.TryGetValue(player, out var value))
			{
				_playerDataLookup.Remove(player);
				Destroy(value, NetworkObjectDestroyFlags.DestroyState);
			}
		}
	}

	private unsafe PlayerSimulationData* GetPlayerSimulationData(PlayerRef player, bool create)
	{
		if (player.IsMasterClient && TryGetStructData<SimulationRuntimeConfig>(NetworkId.RuntimeConfig, out var data))
		{
			player = data->MasterClient;
		}
		if (player.IsNone)
		{
			return null;
		}
		Assert.Always(player.IsRealPlayer, "invalid player {0}", player);
		if (_playerDataLookup.TryGetValue(player, out var value) && TryGetStructData<PlayerSimulationData>(value, out var data2))
		{
			return data2;
		}
		foreach (NetworkId @struct in _structs)
		{
			if (TryGetStruct(@struct, out var meta) && meta.Type == NetworkObjectTypeId.PlayerData)
			{
				PlayerSimulationData* dataAs = meta.GetDataAs<PlayerSimulationData>();
				if (!(dataAs->Player != player))
				{
					_playerDataLookup.Add(player, @struct);
					return dataAs;
				}
			}
		}
		PlayerSimulationData* ptr = ((create && IsServer) ? AllocateStruct<PlayerSimulationData>(GetNextId(), 0, NetworkObjectTypeId.PlayerData) : null);
		if (ptr != null)
		{
			ptr->Player = player;
			_invokeJoinedLeaveQueue.Enqueue((player, true));
		}
		return ptr;
	}

	private void InvokePlayerJoinedLeft()
	{
		if (!_callbacks.CanReceivePlayerJoinLeaveCallbacks)
		{
			return;
		}
		while (_invokeJoinedLeaveQueue.Count > 0)
		{
			try
			{
				(PlayerRef, bool) tuple = _invokeJoinedLeaveQueue.Dequeue();
				var (playerRef, _) = tuple;
				if (tuple.Item2)
				{
					InternalLogStreams.LogDebug?.Log(this, $"Player Joined: {playerRef}");
					_callbacks.PlayerJoined(playerRef);
					if (!IsServer || !Config.SchedulingWithoutAOI || !(playerRef != LocalPlayer))
					{
						continue;
					}
					foreach (NetworkObjectMeta value in _metaLookup.Values)
					{
						if (!value.IsStruct)
						{
							GetSimulationConnectionForPlayer(playerRef)?.GetObjectData(value.Id, create: true);
						}
					}
					continue;
				}
				InternalLogStreams.LogDebug?.Log(this, $"Player Left: {playerRef}");
				_callbacks.PlayerLeft(playerRef);
			}
			catch (Exception error)
			{
				InternalLogStreams.LogException?.Log(error);
			}
		}
	}

	internal unsafe int? GetPlayerActorId(PlayerRef player)
	{
		PlayerSimulationData* playerSimulationData = GetPlayerSimulationData(player, create: true);
		return (playerSimulationData != null) ? playerSimulationData->Actor : 0;
	}

	internal unsafe NetworkId GetPlayerObjectId(PlayerRef player)
	{
		PlayerSimulationData* playerSimulationData = GetPlayerSimulationData(player, create: false);
		NetworkId value;
		if (playerSimulationData == null)
		{
			return _playerLeftTempObjectCache.TryGetValue(player, out value) ? value : default(NetworkId);
		}
		return playerSimulationData->Object;
	}

	internal unsafe void SetPlayerObjectId(PlayerRef player, NetworkId id)
	{
		if (Topology == Topologies.ClientServer)
		{
			if (!IsClient && PlayerValid(player))
			{
				GetPlayerSimulationData(player, create: true)->Object = id;
			}
		}
		else if (IsClient)
		{
			if (TryGetMeta(id, out var meta) && IsStateAuthority(meta.StateAuthority, LocalPlayer) && player == LocalPlayer)
			{
				SimulationMessageInternal_SetPlayerObject buffer = default(SimulationMessageInternal_SetPlayerObject);
				buffer.Object = id;
				SendInternalSimulationMessage(SimulationMessageInternalTypes.SetPlayerObject, buffer);
				GetPlayerSimulationData(player, create: true)->Object = id;
			}
		}
		else if (PlayerValid(player))
		{
			GetPlayerSimulationData(player, create: true)->Object = id;
		}
	}

	public unsafe bool HasAnyActiveConnections()
	{
		NetConnectionMap.Iterator iterator = NetPeerGroup.ConnectionIterator(_netPeerGroup);
		while (iterator.Next())
		{
			if (iterator.Current->ConnectionStatus != NetConnectionStatus.Connected)
			{
				continue;
			}
			return true;
		}
		return false;
	}

	private void InvokeOnBeforeSimulation(int forwardTickCount)
	{
		EngineProfiler.Begin("InvokeOnBeforeSimulation");
		try
		{
			_callbacks.OnBeforeSimulation(forwardTickCount);
		}
		catch (Exception error)
		{
			InternalLogStreams.LogException?.Log(this, error);
		}
		EngineProfiler.End();
	}

	private void InvokeOnAfterSimulation()
	{
		EngineProfiler.Begin("InvokeOnAfterSimulation");
		try
		{
			_callbacks.OnAfterSimulation();
		}
		catch (Exception error)
		{
			InternalLogStreams.LogException?.Log(this, error);
		}
		EngineProfiler.End();
	}

	private void InvokeOnBeforeAllTicks(bool resimulation, int ticks)
	{
		EngineProfiler.Begin("InvokeOnBeforeAllTicks");
		try
		{
			_isResimulation = resimulation;
			_callbacks.OnBeforeAllTicks(resimulation, ticks);
			_isResimulation = false;
		}
		catch (Exception error)
		{
			InternalLogStreams.LogException?.Log(this, error);
		}
		EngineProfiler.End();
	}

	private void InvokeOnAfterAllTicks(bool resimulation, int ticks)
	{
		EngineProfiler.Begin("InvokeOnAfterAllTicks");
		try
		{
			_isResimulation = resimulation;
			_callbacks.OnAfterAllTicks(resimulation, ticks);
			_isResimulation = false;
		}
		catch (Exception error)
		{
			InternalLogStreams.LogException?.Log(this, error);
		}
		EngineProfiler.End();
	}

	protected virtual void BeforeUpdate()
	{
	}

	protected virtual void AfterSimulation()
	{
	}

	private void UpdateSimulationStateForMasterClientObjects(bool isMasterClient)
	{
		SimulationConnection simulationConnectionForPlayer = GetSimulationConnectionForPlayer(LocalPlayer);
		if (simulationConnectionForPlayer == null)
		{
			return;
		}
		HashSet<NetworkId> hashSet = new HashSet<NetworkId>();
		foreach (KeyValuePair<NetworkId, NetworkObjectMeta> item in _metaLookup)
		{
			if (!item.Key.IsReserved && (!item.Value.Instance || (item.Value.Instance.Flags & NetworkObjectFlags.MasterClientObject) == NetworkObjectFlags.MasterClientObject))
			{
				NetworkObjectConnectionData objectData = simulationConnectionForPlayer.GetObjectData(item.Key, create: true);
				NetworkObjectMeta value = item.Value;
				value.SnapshotLatest.CopyTo(value.Shadow);
				value.SnapshotLatest.CopyTo(value.Previous);
				value.SnapshotLatest.CopyTo(value);
				simulationConnectionForPlayer.SetActive(objectData, item.Value);
				hashSet.Add(item.Key);
			}
		}
		foreach (NetworkId item2 in hashSet)
		{
			_callbacks.ObjectIsSimulatedChanged(item2, isMasterClient);
			_callbacks.ObjectStateAuthorityChanged(item2, isMasterClient);
		}
	}

	private int CalculateForwardTicks()
	{
		if (HasRuntimeConfig && _time.IsRunning())
		{
			int val = (int)(_time.Now().Local * (double)TickRate);
			val = Math.Min(val, (int)LatestServerTick + TickRate);
			int val2 = (val - (int)_tick) / TickStride;
			return Math.Min(Math.Max(val2, 0), TickRate);
		}
		return 0;
	}

	public int Update(double dt)
	{
		if (_isShutdown || dt == 0.0)
		{
			return 0;
		}
		EngineProfiler.Begin("Simulation.Update");
		BeforeUpdate();
		NetworkRecv();
		if (HasRuntimeConfig && _time.IsRunning())
		{
			_time.Update(dt);
			if (!Runner.OnGameStartedInvoked)
			{
				Runner.OnRuntimeConfigReady();
			}
			if (IsServer)
			{
				CalculateUpdateTime();
			}
		}
		int num = CalculateForwardTicks();
		if (!_isWaitingForShutdown && num > 0)
		{
			InvokeOnBeforeSimulation(num);
			EngineProfiler.Begin("BeforeSimulation");
			int value = BeforeSimulation();
			EngineProfiler.End();
			_fusionStatsManager.PendingSnapshot.AddToResimulationStat(value);
			try
			{
				InvokeOnBeforeAllTicks(resimulation: false, num);
				for (int i = 0; i < num; i++)
				{
					StepSimulation(SimulationStages.Forward, i == num - 1, i == 0, IsServer);
				}
				InvokeOnAfterAllTicks(resimulation: false, num);
			}
			catch (Exception error)
			{
				InternalLogStreams.LogException?.Log(this, error);
			}
			InvokeOnAfterSimulation();
			EngineProfiler.Begin("AfterSimulation");
			AfterSimulation();
			EngineProfiler.End();
			_fusionStatsManager.PendingSnapshot.AddToForwardTicksStat(num);
			try
			{
				PreparePackets();
			}
			catch (Exception error2)
			{
				InternalLogStreams.LogException?.Log(this, error2);
			}
		}
		else
		{
			NoSimulation();
		}
		NetworkSend();
		Assert.Check(_stage == (SimulationStages)0, "Invalid Simulation.Stage {0}", _stage);
		EngineProfiler.End();
		AfterUpdate();
		if (HasRuntimeConfig && _time.IsRunning() && IsPlayer)
		{
			_localAlphaPrev = _localAlpha;
			_localAlpha = (float)Maths.Clamp01((_time.Now().Local - (double)(int)_tick * TickDeltaDouble) * (double)TickRate);
			if (!IsClient)
			{
				_remoteAlpha = _localAlpha;
			}
			if (IsClient)
			{
				_time.Log(_fusionStatsManager);
			}
		}
		return num;
	}

	private void UpdateAreaOfInterest()
	{
		if (!IsServer || (Config.ReplicationFeatures & NetworkProjectConfig.ReplicationFeatures.SchedulingAndInterestManagement) != NetworkProjectConfig.ReplicationFeatures.SchedulingAndInterestManagement)
		{
			return;
		}
		EngineProfiler.Begin("UpdateAreaOfInterest");
		foreach (NetworkObjectMeta value in _metaLookup.Values)
		{
			if ((value.Flags & NetworkObjectHeaderFlags.AreaOfInterest) != NetworkObjectHeaderFlags.AreaOfInterest)
			{
				continue;
			}
			Assert.Check(value.HasMainTRSP, value.Instance.gameObject.name);
			if (value.HasMainTRSP)
			{
				Vector3? vector = ResolveCellPosition(value);
				if (vector.HasValue)
				{
					AOI_UpdateAreaOfInterest(value, AreaOfInterest.ToCell(vector.Value));
				}
				else
				{
					InternalLogStreams.LogDebug?.Error($"could not resolve aoi position for {value.Id}");
				}
			}
		}
		EngineProfiler.End();
		Vector3? ResolveCellPosition(NetworkObjectMeta m)
		{
			ref NetworkTRSPData mainTRSPData = ref m.MainTRSPData;
			while ((bool)mainTRSPData.AreaOfInterestOverride)
			{
				if (!TryGetMeta(mainTRSPData.AreaOfInterestOverride, out var meta) || !meta.HasMainTRSP)
				{
					return null;
				}
				mainTRSPData = ref meta.MainTRSPData;
			}
			while ((bool)mainTRSPData.Parent.Object)
			{
				if (!TryGetMeta(mainTRSPData.Parent.Object, out var meta2) || !meta2.HasMainTRSP)
				{
					return null;
				}
				mainTRSPData = ref meta2.MainTRSPData;
			}
			return mainTRSPData.Position;
		}
	}

	private unsafe void PreparePackets()
	{
		int num = _tick.Raw - _sendTick.Raw;
		if (num < TickRate / SendRate)
		{
			return;
		}
		_sendTick = _tick;
		if ((Config.ReplicationFeatures & NetworkProjectConfig.ReplicationFeatures.SchedulingAndInterestManagement) == NetworkProjectConfig.ReplicationFeatures.SchedulingAndInterestManagement)
		{
			UpdateAreaOfInterest();
		}
		EngineProfiler.Begin("SendPackets");
		_stateReplicator.UpdateChangedStructSet();
		NetConnectionMap.Iterator iterator = NetPeerGroup.ConnectionIterator(_netPeerGroup);
		while (iterator.Next())
		{
			NetConnection* current = iterator.Current;
			SimulationConnection simulationConnection = GetSimulationConnection(current);
			if (simulationConnection != null)
			{
				double connectionIdleTime = NetPeerGroup.GetConnectionIdleTime(_netPeerGroup, current);
				if ((!(connectionIdleTime >= 1.0) || !(_netPeerGroup->Time - simulationConnection.LastSend < 0.5)) && _sendContext.Init(simulationConnection, _tick))
				{
					WriteMessages();
					WritePackets();
					_fusionStatsManager.PendingSnapshot.AddToOutPacketsStat(1);
					_fusionStatsManager.PendingSnapshot.AddToOutBandwidthStat(Maths.BytesRequiredForBits(_sendContext.Buffer->OffsetBits));
					_sendContext.Send();
					simulationConnection.LastSend = _netPeerGroup->Time;
				}
			}
		}
		EngineProfiler.End();
	}

	private unsafe void WriteMessages()
	{
		NetBitBuffer.Offset offset = NetBitBuffer.GetOffset(_sendContext.Buffer);
		bool flag = _sendContext.Connection.MessagesOut.Count > 0;
		int num = 9088 - _sendContext.Buffer->OffsetBits;
		if (num > 0)
		{
			ConsumeAndWriteMessagesIntoBuffer(ref _sendContext.Connection.MessagesOut, _sendContext.Buffer, num, ref _sendContext.Envelope->Messages);
			if (flag)
			{
				if (_sendContext.Header.SimulationMessages == 0)
				{
					SimulationMessageEnvelope* head = _sendContext.Connection.MessagesOut.Head;
					Assert.Always(head->Message->Offset > 0, "Message offset invalid {0}", head->Message->Offset);
					Assert.Always(!head->Message->GetFlag(256), "Message has FLAG_DUMMY");
					InternalLogStreams.LogError?.Log(this, $"Message {*head->Message} (sequence: {head->Sequence}) is too large to be serialized and will be discarded");
					head->Message->SetDummy();
				}
				else
				{
					InternalLogStreams.LogTraceSimulationMessage?.Log(this, $"Consumed {_sendContext.Header.SimulationMessages} messages, remaining: {_sendContext.Connection.MessagesOut.Count}, bit capacity: {num}, bit left: {9088 - _sendContext.Buffer->OffsetBits}");
				}
			}
		}
		else if (flag)
		{
			InternalLogStreams.LogTraceSimulationMessage?.Log(this, $"No space to consume messages after snapshot serialization ({num}).");
		}
	}

	private void InvokeTick(SimulationStages stage, bool releaseAllInputs)
	{
		try
		{
			Assert.Check(_inputCollection.Count == 0, "InputCollection Size should be 0");
			_stage = stage;
			InterpolateSequenceIncrement();
			foreach (PlayerRef player in _players)
			{
				SimulationInput input = GetInput(_tick, player);
				if (input != null)
				{
					_inputCollection.AddInput(input);
				}
			}
			if (IsClient && IsFirstTick && (IsResimulation || Config.Topology == Topologies.Shared))
			{
				SimulationStages stage2 = _stage;
				try
				{
					_stage = SimulationStages.Forward;
					_callbacks.UpdateRemotePrefabs();
				}
				finally
				{
					_stage = stage2;
				}
			}
			if (_isInitialLocalTick)
			{
				_isInitialLocalTick = false;
				BeforeFirstTick();
			}
			EngineProfiler.Begin("Simulation.BeforeTick");
			_callbacks.OnBeforeTick();
			EngineProfiler.End();
			DeliverMessages(_tick);
			try
			{
				_isInTick = true;
				InvokePlayerJoinedLeft();
				_playerLeftTempObjectCache.Clear();
				_callbacks.OnTick();
			}
			catch (Exception error)
			{
				InternalLogStreams.LogError?.Log(this, "OnTick Threw Exception");
				InternalLogStreams.LogException?.Log(this, error);
			}
			finally
			{
				_isInTick = false;
			}
			EngineProfiler.Begin("Simulation.AfterTick");
			_callbacks.OnAfterTick();
			EngineProfiler.End();
		}
		catch (Exception error2)
		{
			InternalLogStreams.LogException?.Log(this, error2);
		}
		finally
		{
			_stage = (SimulationStages)0;
			try
			{
				if (releaseAllInputs)
				{
					for (int i = 0; i < _inputCollection.Count; i++)
					{
						_inputPool.Release(_inputCollection.GetByIndex(i));
					}
				}
			}
			finally
			{
				_inputCollection.Clear();
			}
		}
	}

	private unsafe static ref SimulationMessageInternalTypes GetMessageInternalType(SimulationMessage* message)
	{
		Assert.Check(condition: true, "SimulationMessageInternalTypes size should be 4");
		Span<byte> rawData = SimulationMessage.GetRawData(message);
		return ref rawData.AsRef<SimulationMessageInternalTypes>();
	}

	private unsafe static ref T GetMessageInternalData<T>(SimulationMessage* message) where T : unmanaged
	{
		Assert.Check(condition: true, "SimulationMessageInternalTypes size should be 4");
		Span<byte> rawData = SimulationMessage.GetRawData(message);
		return ref rawData.Slice(4, rawData.Length - 4).AsRef<T>();
	}

	private unsafe void OnMessageInternal(SimulationMessage* message)
	{
		InternalLogStreams.LogTraceNetwork?.Log($"OnMessageInternal({GetMessageInternalType(message)})");
	}

	private unsafe void DeliverMessages(int tick)
	{
		EngineProfiler.Begin("Simulation.DeliverMessages");
		NetConnectionMap.Iterator iterator = NetPeerGroup.ConnectionIterator(_netPeerGroup);
		while (iterator.Next())
		{
			if (iterator.Current->ConnectionStatus != NetConnectionStatus.Connected || !TryGetSimulationConnectionLogErrorIfFailed(iterator.Current, out var result))
			{
				continue;
			}
			SimulationMessageList messagesIn = default(SimulationMessageList);
			try
			{
				while (result.MessagesIn.Count > 0)
				{
					SimulationMessageEnvelope* envelope = result.MessagesIn.Head;
					if (envelope->Message->Tick > tick)
					{
						if (!envelope->Message->GetFlag(128))
						{
							InternalLogStreams.LogTraceSimulationMessage?.Log(this, $"Not handling RPC: tick {envelope->Message->Tick} > {tick} (player: {LocalPlayer}) {LogUtils.GetDump(envelope)}");
							break;
						}
						InternalLogStreams.LogTraceSimulationMessage?.Log(this, $"Handling RPC ahead of time due to its flags (tick: {tick}, player: {LocalPlayer}) {LogUtils.GetDump(envelope)}");
					}
					if (envelope->Message->GetFlag(8))
					{
						Assert.Check(envelope->Sequence == 0, "Head Sequence must be 0");
					}
					else
					{
						if (envelope->Sequence != result.MessagesInSequence + 1)
						{
							InternalLogStreams.LogTraceSimulationMessage?.Log(this, $"Not handling RPC: sequence {envelope->Sequence} != {result.MessagesInSequence + 1} (player: {LocalPlayer}) {LogUtils.GetDump(envelope)}");
							break;
						}
						result.MessagesInSequence++;
					}
					result.MessagesIn.Remove(envelope);
					int count = result.MessagesIn.Count;
					try
					{
						if (envelope->Message->GetFlag(64))
						{
							OnMessageInternal(envelope->Message);
							continue;
						}
						SimulationMessageResult simulationMessageResult = _callbacks.OnMessage(envelope->Message);
						if (simulationMessageResult == SimulationMessageResult.Retry)
						{
							if (!envelope->Message->GetFlag(8))
							{
								InternalLogStreams.LogTraceSimulationMessage?.Log(this, $"Reliable RPC {envelope->Sequence} will be retried (player: {LocalPlayer}) {LogUtils.GetDump(envelope)}");
								result.MessagesIn.AddFirst(envelope);
								result.MessagesInSequence--;
								envelope = null;
								break;
							}
							InternalLogStreams.LogTraceSimulationMessage?.Log(this, $"Unreliable will be retried (player: {LocalPlayer}) {LogUtils.GetDump(envelope)}");
							messagesIn.AddLast(envelope);
							envelope = null;
						}
					}
					finally
					{
						if (envelope != null)
						{
							Assert.Check(count == result.MessagesIn.Count);
							SimulationMessageEnvelope.Free(this, ref envelope);
						}
					}
				}
			}
			finally
			{
				if (messagesIn.Count > 0)
				{
					InternalLogStreams.LogTraceSimulationMessage?.Log(this, $"Requeuing {messagesIn.Count} messages for retry (player: {LocalPlayer})");
					messagesIn.Concat(result.MessagesIn);
					result.MessagesIn = messagesIn;
				}
			}
		}
		EngineProfiler.End();
	}

	internal unsafe void FreeMessages(ref SimulationMessageList list)
	{
		while (list.Count > 0)
		{
			SimulationMessageEnvelope* envelope = list.RemoveHead();
			SimulationMessageEnvelope.Free(this, ref envelope);
		}
		list = default(SimulationMessageList);
	}

	private unsafe void ConsumeAndWriteMessagesIntoBuffer(ref SimulationMessageList inList, NetBitBuffer* buffer, int bitCapacity, ref SimulationMessageList outList, bool allowFirstMessageOverflow = true)
	{
		int num = buffer->OffsetBits + bitCapacity;
		InternalLogStreams.LogTraceNetwork?.Log($"{LogUtils.GetDump(buffer)} Messages-Start");
		buffer->PadToByteBoundary();
		bool flag = allowFirstMessageOverflow;
		bool isServer = IsServer;
		while (inList.Count > 0)
		{
			SimulationMessageEnvelope* envelope = inList.Head;
			int offsetBits = buffer->OffsetBits;
			PlayerRef? playerRef = null;
			if (isServer && envelope->Message->IsTargeted())
			{
				playerRef = envelope->Message->Target;
				envelope->Message->Target = default(PlayerRef);
			}
			InternalLogStreams.LogTraceNetwork?.Log($"{LogUtils.GetDump(buffer)} Message");
			try
			{
				int bitCount = SimulationMessageEnvelope.GetBitCount(envelope, buffer);
				if (!buffer->CheckBitCount(bitCount))
				{
					InternalLogStreams.LogTraceNetwork?.Log($"{LogUtils.GetDump(buffer)} Message-Sequence:{envelope->Sequence} would overflow by {bitCount - (buffer->LengthBits - buffer->OffsetBits)} bits");
					break;
				}
				SimulationMessageEnvelope.Write(envelope, buffer);
				InternalLogStreams.LogTraceNetwork?.Log($"{LogUtils.GetDump(buffer)} Message-Sequence:{envelope->Sequence}");
				if (buffer->OffsetBits >= num)
				{
					Assert.Check(!buffer->Overflow, "Buffer should not overflow");
					if (!flag)
					{
						buffer->OffsetBits = offsetBits;
						break;
					}
				}
			}
			finally
			{
				if (playerRef.HasValue)
				{
					envelope->Message->Target = playerRef.Value;
				}
			}
			flag = false;
			_sendContext.Header.SimulationMessages++;
			SimulationMessageEnvelope* ptr = inList.RemoveHead();
			Assert.Check(ptr == envelope, "SimulationMessageList Head != Msg Head");
			if (envelope->Message->GetFlag(8))
			{
				SimulationMessageEnvelope.Free(this, ref envelope);
			}
			else
			{
				outList.AddLast(envelope);
			}
		}
		EngineProfiler.RpcOut(_sendContext.Header.SimulationMessages);
		buffer->PadToByteBoundary();
	}

	private unsafe void ResolveMessageSourceAndTarget(SimulationMessage* msg, PlayerRef sourcePlayer)
	{
		if (IsServer)
		{
			Assert.Check(msg->Source.IsNone, "Messages arriving to server should not have Source set");
			msg->Source = sourcePlayer;
			if (msg->GetFlag(32))
			{
				Assert.Check(msg->Target.IsNone, "Messages to the server should not have target set");
			}
			else if (msg->GetFlag(16))
			{
				Assert.Check(msg->Target.IsRealPlayer, "Messages to a player should have target set");
			}
			else
			{
				Assert.Check(msg->Target.IsNone, "Messages without a target should not have target set");
			}
		}
		else
		{
			Assert.Check(!msg->GetFlag(32), "Got forwarded to a client? With server?");
			Assert.Check(msg->Target.IsNone, "If a message reaches a client, it should have it's target set");
			if (msg->GetFlag(16))
			{
				msg->Target = LocalPlayer;
			}
		}
	}

	private unsafe void RecvMessages()
	{
		InternalLogStreams.LogTraceNetwork?.Log($"{LogUtils.GetDump(_recvContext.Buffer)} Messages-Start");
		NetBitBuffer.Offset offset = NetBitBuffer.GetOffset(_recvContext.Buffer);
		_recvContext.Buffer->SeekToByteBoundary();
		Assert.Check(!_recvContext.Buffer->Overflow, "Buffer should not overflow");
		int num = _recvContext.Header.SimulationMessages;
		EngineProfiler.RpcIn(num);
		if (_recvContext.Header.SimulationMessages > 0)
		{
			InternalLogStreams.LogTraceSimulationMessage?.Log($"ReadMessagesFromBuffer: {_recvContext.Header.SimulationMessages} messages");
		}
		while (--num >= 0)
		{
			InternalLogStreams.LogTraceNetwork?.Log($"{LogUtils.GetDump(_recvContext.Buffer)} Message");
			SimulationMessageEnvelope* envelope = SimulationMessageEnvelope.Read(this, _recvContext.Buffer);
			Assert.Always(!_recvContext.Buffer->Overflow, "_recvContext.Buffer->Overflow == false");
			ResolveMessageSourceAndTarget(envelope->Message, _recvContext.Player);
			InternalLogStreams.LogTraceNetwork?.Log($"{LogUtils.GetDump(_recvContext.Buffer)} Message-Sequence:{envelope->Sequence}");
			SimulationMessageEnvelope* followingMessage2;
			if (envelope->Message->IsUnreliable)
			{
				InternalLogStreams.LogTraceSimulationMessage?.Log(this, $"Enqueuing (unreliable) {LogUtils.GetDump(envelope)}");
				_recvContext.Connection.MessagesIn.AddLast(envelope);
			}
			else if (envelope->Sequence <= _recvContext.Connection.MessagesInSequence)
			{
				InternalLogStreams.LogTraceSimulationMessage?.Log(this, $"Dropping (fast, min number: {_recvContext.Connection.MessagesInSequence}) {LogUtils.GetDump(envelope)}");
				SimulationMessageEnvelope.Free(this, ref envelope);
			}
			else if (CanAppendQueue(_recvContext.Connection.MessagesIn, envelope, out followingMessage2))
			{
				InternalLogStreams.LogTraceSimulationMessage?.Log(this, $"Enqueuing (fast) {LogUtils.GetDump(envelope)}");
				_recvContext.Connection.MessagesIn.AddLast(envelope);
			}
			else if (followingMessage2 != null)
			{
				InternalLogStreams.LogTraceSimulationMessage?.Log(this, $"Enqueuing (slow, before {followingMessage2->Sequence}) {LogUtils.GetDump(envelope)}");
				_recvContext.Connection.MessagesIn.AddBefore(envelope, followingMessage2);
			}
			else
			{
				InternalLogStreams.LogTraceSimulationMessage?.Log(this, $"Dropping (slow, already a message with this number) {LogUtils.GetDump(envelope)}");
				SimulationMessageEnvelope.Free(this, ref envelope);
			}
		}
		_recvContext.Buffer->SeekToByteBoundary();
		unsafe static bool CanAppendQueue(SimulationMessageList list, SimulationMessageEnvelope* messageEnvelope, out SimulationMessageEnvelope* followingMessage)
		{
			SimulationMessageEnvelope* ptr = list.Tail;
			while (ptr != null && ptr->Message->IsUnreliable)
			{
				ptr = ptr->Prev;
			}
			if (ptr == null || ptr->Sequence < messageEnvelope->Sequence)
			{
				followingMessage = null;
				return true;
			}
			SimulationMessageEnvelope* ptr2 = list.Head;
			while (ptr2 != null && (ptr2->Message->IsUnreliable || ptr2->Sequence < messageEnvelope->Sequence))
			{
				ptr2 = ptr2->Next;
			}
			Assert.Always(ptr2 != null, "Expected next list element");
			followingMessage = ((ptr2->Sequence == messageEnvelope->Sequence) ? null : ptr2);
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal SimulationConnection GetSimulationConnectionByIndex(int index)
	{
		SimulationConnection value;
		return _connections.TryGetValue(index, out value) ? value : null;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal SimulationConnection GetSimulationConnectionForPlayer(PlayerRef player)
	{
		if (_playersConnections.TryGetValue(player, out var value))
		{
			return value;
		}
		return null;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal bool TryGetSimulationConnectionForPlayer(PlayerRef player, out SimulationConnection sc)
	{
		return _playersConnections.TryGetValue(player, out sc);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal int? GetConnectionIndexForPlayer(PlayerRef player)
	{
		if (_playersConnections.TryGetValue(player, out var value))
		{
			return value.ConnectionIndex;
		}
		return null;
	}

	private unsafe bool TryGetSimulationConnectionLogErrorIfFailed(NetConnection* c, out SimulationConnection result)
	{
		SimulationConnection simulationConnection = _connections[c->LocalConnectionId.GroupIndex];
		if (simulationConnection.ConnectionId == c->LocalConnectionId)
		{
			if (simulationConnection.Connection != c)
			{
				InternalLogStreams.LogError?.Log($"SimulationConnection.Connection != NetConnection for {c->LocalConnectionId}");
			}
			result = simulationConnection;
			return true;
		}
		InternalLogStreams.LogError?.Log($"Failed getting SimulationConnection for {c->LocalConnectionId}");
		result = null;
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private unsafe SimulationConnection GetSimulationConnection(NetConnection* c)
	{
		SimulationConnection simulationConnection = _connections[c->LocalConnectionId.GroupIndex];
		if (simulationConnection.ConnectionId == c->LocalConnectionId)
		{
			Assert.Check(simulationConnection.Connection == c, "SimulationConnection.Connection != NetConnection");
			return simulationConnection;
		}
		return null;
	}

	internal void AddToGlobalObjectInterest(NetworkObjectMeta meta)
	{
		if (_globalInterestObjects == null)
		{
			return;
		}
		Assert.Check((Config.ReplicationFeatures & NetworkProjectConfig.ReplicationFeatures.SchedulingAndInterestManagement) == NetworkProjectConfig.ReplicationFeatures.SchedulingAndInterestManagement);
		_globalInterestObjects.Add(meta.Id);
		foreach (SimulationConnection value in _connections.Values)
		{
			value.GetObjectData(meta.Id, create: true).SetPlayerDataFlag(NetworkObjectHeaderPlayerDataFlags.ForceInterest, this);
		}
	}

	internal void RemoveFromGlobalObjectInterest(NetworkId id)
	{
		if (_globalInterestObjects == null)
		{
			return;
		}
		Assert.Check((Config.ReplicationFeatures & NetworkProjectConfig.ReplicationFeatures.SchedulingAndInterestManagement) == NetworkProjectConfig.ReplicationFeatures.SchedulingAndInterestManagement);
		_globalInterestObjects.Remove(id);
		foreach (SimulationConnection value in _connections.Values)
		{
			value.GetObjectData(id, create: false)?.ClearPlayerDataFlag(NetworkObjectHeaderPlayerDataFlags.ForceInterest, this);
		}
	}

	internal unsafe void SendReliableData(int connection, int target, ReliableKey key, byte[] data)
	{
		SimulationConnection simulationConnectionByIndex = GetSimulationConnectionByIndex(connection);
		ReliableId rid = default(ReliableId);
		rid.Key = key;
		rid.Target = target;
		rid.Source = LocalPlayer.AsIndex;
		rid.SourceSend = ++_reliableSend;
		fixed (byte* data2 = data)
		{
			NetPeerGroup.SendReliable(_netPeerGroup, simulationConnectionByIndex.Connection, rid, data2, data.Length);
		}
	}

	internal void NotifyWaitingForShutdown()
	{
		_isWaitingForShutdown = true;
	}

	UnityEngine.Object ILogSource.GetUnityObject()
	{
		return Runner;
	}

	[Conditional("DEBUG")]
	internal void DumpObject(NetworkId id, StringBuilder sb)
	{
		NetworkObjectMeta meta;
		if (id.IsReserved)
		{
			if (!TryGetStruct(id, out meta))
			{
				return;
			}
		}
		else if (!TryGetMeta(id, out meta))
		{
			return;
		}
		DumpObject(meta, sb);
	}

	[Conditional("DEBUG")]
	internal void DumpObject(NetworkObjectMeta meta, StringBuilder sb)
	{
		if (meta == null)
		{
			sb.AppendLine("null");
			return;
		}
		sb.AppendLine(meta.Header.ToString());
		sb.Append(BinUtils.WordsToHex(meta.Data));
	}

	internal string DumpObject(NetworkId id)
	{
		StringBuilder stringBuilder = new StringBuilder();
		DumpObject(id, stringBuilder);
		return stringBuilder.ToString();
	}

	internal string DumpObject(NetworkObjectMeta meta)
	{
		StringBuilder stringBuilder = new StringBuilder();
		DumpObject(meta, stringBuilder);
		return stringBuilder.ToString();
	}

	private unsafe void NetworkInit(INetSocket socket, NetAddress address)
	{
		NetConfig config = _projectConfig.Network.ToNetConfig(address);
		config.Simulation = _projectConfig.NetworkConditions.Create();
		config.PacketSize = 8192;
		config.ConnectionGroups = 1;
		if (IsSinglePlayer)
		{
			config.MaxConnections = 0;
		}
		else if (IsClient)
		{
			config.MaxConnections = 1;
		}
		else
		{
			Assert.Check(IsServer);
			if (IsPlayer)
			{
				config.MaxConnections = _config.PlayerCount - 1;
			}
			else
			{
				config.MaxConnections = _config.PlayerCount;
			}
		}
		_netSocket = socket;
		_netPeer = NetPeer.Initialize(config, _netSocket);
		_netPeerGroup = NetPeer.GetGroup(_netPeer, 0);
		_netPeerRng = new System.Random(Environment.TickCount);
	}

	private unsafe void NetworkSend()
	{
		if (_netPeer != null)
		{
			EngineProfiler.Begin("Simulation.NetworkSend");
			NetPeer.Send(_netPeer, _netSocket);
			EngineProfiler.End();
		}
	}

	private unsafe void NetworkRecv()
	{
		if (_netPeer != null)
		{
			EngineProfiler.Begin("Simulation.NetworkRecv");
			if (_netPeerRng == null)
			{
				_netPeerRng = new System.Random(Environment.TickCount);
			}
			NetPeer.Recv(_netPeer, _netSocket, _netPeerRng);
			NetPeerGroup.Update(_netPeerGroup, this);
			NetworkReceiveDone();
			EngineProfiler.End();
		}
	}

	private unsafe void NetworkShutdown()
	{
		OnNetworkShutdown();
		foreach (SimulationConnection value in _connections.Values)
		{
			FreeMessages(ref value.MessagesIn);
			FreeMessages(ref value.MessagesOut);
		}
		NetPeer.Destroy(_netPeer, _netSocket, this);
		_netPeer = null;
		_netPeerGroup = null;
		_netSocket = null;
	}

	internal virtual void OnNetworkShutdown()
	{
	}

	private unsafe bool NetworkGetBuffer(NetConnection* connection, out NetBitBuffer* buffer)
	{
		if (_netPeer == null)
		{
			buffer = null;
			return false;
		}
		return NetPeerGroup.GetNotifyDataBuffer(_netPeerGroup, connection, out buffer);
	}

	private unsafe bool NetworkSendBuffer(NetConnection* connection, NetBitBuffer* buffer, SimulationPacketEnvelope* envelope)
	{
		if (_netPeer == null)
		{
			return false;
		}
		bool flag = NetPeerGroup.SendNotifyDataBuffer(_netPeerGroup, connection, buffer, envelope);
		if (!flag)
		{
			Assert.AlwaysFail("SendNotifyDataBuffer failed");
		}
		return flag;
	}

	internal unsafe bool NetworkSendPing(NetAddress address, void* data, int length)
	{
		if (_netPeer == null)
		{
			return false;
		}
		return NetPeerGroup.SendUnconnectedData(_netPeerGroup, address, data, length);
	}

	unsafe void INetPeerGroupCallbacks.OnConnectionAttempt(NetConnection* connection, int attempt, int totalConnectionAttempts)
	{
		Assert.Check(IsClient);
		_callbacks.OnInternalConnectionAttempt(attempt, totalConnectionAttempts, out var shouldChange, out var newAddress);
		if (shouldChange)
		{
			NetPeerGroup.ChangeConnectionAddressDuringConnecting(_netPeerGroup, connection, newAddress);
		}
	}

	unsafe void INetPeerGroupCallbacks.OnUnconnectedData(NetBitBuffer* buffer)
	{
	}

	unsafe void INetPeerGroupCallbacks.OnConnected(NetConnection* connection)
	{
		InternalLogStreams.LogTraceNetwork?.Log(this, $"OnConnected {connection->LocalConnectionId.GroupIndex} / {connection->Address}");
		SimulationConnection simulationConnection = new SimulationConnection(this);
		_connections.Add(connection->LocalConnectionId.GroupIndex, simulationConnection);
		simulationConnection.Connection = connection;
		simulationConnection.ConnectionId = connection->LocalConnectionId;
		if (IsServer)
		{
			PlayerRefMapping? playerRefMapping = GetPlayerRefMapping(connection->UniqueId);
			if (!playerRefMapping.HasValue)
			{
				throw new Exception();
			}
			GetPlayerSimulationData(playerRefMapping.Value.PlayerRef, create: true);
			PlayerAdd(playerRefMapping.Value.PlayerRef, simulationConnection);
		}
		else
		{
			PlayerAdd(_callbacks.LocalPlayerRef, simulationConnection);
		}
		NetworkConnected(connection);
		try
		{
			if (IsClient)
			{
				_callbacks.OnConnectedToServer();
			}
		}
		catch (Exception error)
		{
			InternalLogStreams.LogException?.Log(this, error);
		}
	}

	unsafe void INetPeerGroupCallbacks.OnDisconnected(NetConnection* connection, NetDisconnectReason reason)
	{
		InternalLogStreams.LogTraceNetwork?.Log(this, $"Disconnected: Address={connection->Address}, Reason={reason}");
		SimulationConnection simulationConnection = GetSimulationConnection(connection);
		if (simulationConnection == null)
		{
			InternalLogStreams.LogWarn?.Log($"got disconnect for {connection->Address} without a simulation connection, reason: {reason}");
		}
		PlayerRef playerRef = Connection2Player(connection);
		NetworkDisconnected(connection, reason);
		if (IsServer)
		{
			if (simulationConnection != null)
			{
				AOI_RemoveConnection(simulationConnection);
			}
			PlayerRefMapping? playerRefMapping = GetPlayerRefMapping(connection->UniqueId);
			if (!playerRefMapping.HasValue)
			{
				throw new Exception();
			}
			DeletePlayerSimulationDataOnDisconnect(playerRefMapping.Value.PlayerRef);
		}
		PlayerRemove(playerRef);
		_playersConnections.Remove(playerRef);
		_connections.Remove(connection->LocalConnectionId.GroupIndex);
		simulationConnection?.Free(this);
	}

	unsafe void INetPeerGroupCallbacks.OnReliableData(NetConnection* connection, ReliableId id, byte* data)
	{
		if (IsServer)
		{
			if (id.Target != -1 && id.Target != LocalPlayer.AsIndex)
			{
				if ((ProjectConfig.Network.ReliableDataTransferModes & NetworkConfiguration.ReliableDataTransfers.ClientToClientWithServerProxy) == NetworkConfiguration.ReliableDataTransfers.ClientToClientWithServerProxy)
				{
					if (_playersConnections.TryGetValue(PlayerRef.FromIndex(id.Target), out var value))
					{
						NetPeerGroup.SendReliable(_netPeerGroup, value.Connection, id, data, id.SliceLength);
					}
					else
					{
						InternalLogStreams.LogDebug?.Error(this, $"Target client connection ({id.Target}) not found to send reliable data");
					}
				}
				else
				{
					InternalLogStreams.LogDebug?.Error(this, "Disconnecting client for sending server-proxied reliable data when not allowed");
					NetPeerGroup.Disconnect(_netPeerGroup, connection, null);
				}
				return;
			}
			if ((ProjectConfig.Network.ReliableDataTransferModes & NetworkConfiguration.ReliableDataTransfers.ClientToServer) != NetworkConfiguration.ReliableDataTransfers.ClientToServer)
			{
				NetPeerGroup.Disconnect(_netPeerGroup, connection, null);
				InternalLogStreams.LogDebug?.Error(this, "Disconnecting client for sending reliable data when not allowed");
				return;
			}
		}
		byte[] array = new byte[id.SliceLength];
		fixed (byte* destination = array)
		{
			Native.MemCpy(destination, data, id.SliceLength);
		}
		_callbacks.OnReliableData(Connection2Player(connection), id, local: false, array);
	}

	OnConnectionRequestReply INetPeerGroupCallbacks.OnConnectionRequest(NetAddress remoteAddres, byte[] token, byte[] uniqueid)
	{
		ulong key = BitConverter.ToUInt64(uniqueid, 0);
		if (_uniqueIdPlayerRefMapping.TryGetValue(key, out var _))
		{
			return _callbacks.OnConnectionRequest(remoteAddres, token);
		}
		return OnConnectionRequestReply.Waiting;
	}

	void INetPeerGroupCallbacks.OnConnectionFailed(NetAddress address, NetConnectFailedReason reason)
	{
		try
		{
			_callbacks.OnConnectionFailed(address, reason);
		}
		catch (Exception error)
		{
			InternalLogStreams.LogException?.Log(this, error);
		}
	}

	unsafe void INetPeerGroupCallbacks.OnUnreliableData(NetConnection* connection, NetBitBuffer* buffer)
	{
		Assert.AlwaysFail("Not implemented");
	}

	unsafe void INetPeerGroupCallbacks.OnNotifyData(NetConnection* c, NetBitBuffer* buffer)
	{
		TryGetSimulationConnectionLogErrorIfFailed(c, out var result);
		_recvContext.Init(result, buffer);
		_fusionStatsManager.PendingSnapshot.AddToInPacketsStat(1);
		_fusionStatsManager.PendingSnapshot.AddToInBandwidthStat(Maths.BytesRequiredForBits(buffer->LengthBits));
		_recvContext.Connection.PacketReceiveDelta();
		try
		{
			RecvMessages();
			RecvPacket();
		}
		catch (Exception error)
		{
			_recvContext.Done();
			InternalLogStreams.LogException?.Log(this, error);
		}
	}

	private unsafe void OnEnvelopeLost(NetConnection* connection, SimulationPacketEnvelope* envelope)
	{
		if (connection->ConnectionStatus == NetConnectionStatus.Connected)
		{
			SimulationConnection simulationConnection = GetSimulationConnection(connection);
			if (envelope->Messages.Count > 0)
			{
				InternalLogStreams.LogTraceSimulationMessage?.Warn(this, $"Lost {envelope->Messages.Count} messages, requeing");
				while (envelope->Messages.Count > 0)
				{
					SimulationMessageEnvelope* ptr = envelope->Messages.RemoveHead();
					InternalLogStreams.LogTraceSimulationMessage?.Warn(this, $"Requeued {LogUtils.GetDump(ptr)}");
					simulationConnection.MessagesOut.AddFirst(ptr);
				}
			}
			_stateReplicator.OnPacketLost(connection, envelope);
		}
		else
		{
			FreeMessages(ref envelope->Messages);
		}
	}

	private unsafe void OnEnvelopeDelivered(NetConnection* connection, SimulationPacketEnvelope* envelope)
	{
		FreeMessages(ref envelope->Messages);
		_stateReplicator.OnPacketDelivered(connection, envelope);
	}

	unsafe void INetPeerGroupCallbacks.OnNotifyDispose(ref NetSendEnvelope envelope)
	{
		switch (envelope.PacketType)
		{
		case NetPacketType.NotifyData:
		{
			SimulationPacketEnvelope* envelope2 = envelope.TakeUserData<SimulationPacketEnvelope>();
			FreeMessages(ref envelope2->Messages);
			SimulationPacketEnvelope.Free(this, ref envelope2);
			break;
		}
		case NetPacketType.NotifyReliableData:
		{
			byte* memory = envelope.TakeUserData<byte>();
			Native.Free(ref memory);
			break;
		}
		}
	}

	unsafe void INetPeerGroupCallbacks.OnNotifyLost(NetConnection* connection, ref NetSendEnvelope envelope)
	{
		switch (envelope.PacketType)
		{
		case NetPacketType.NotifyData:
		{
			SimulationPacketEnvelope* envelope2 = envelope.TakeUserData<SimulationPacketEnvelope>();
			OnEnvelopeLost(connection, envelope2);
			SimulationPacketEnvelope.Free(this, ref envelope2);
			break;
		}
		case NetPacketType.NotifyReliableData:
		{
			byte* memory = envelope.TakeUserData<byte>();
			Native.Free(ref memory);
			break;
		}
		}
	}

	unsafe void INetPeerGroupCallbacks.OnNotifyDelivered(NetConnection* connection, ref NetSendEnvelope envelope)
	{
		switch (envelope.PacketType)
		{
		case NetPacketType.NotifyData:
		{
			SimulationPacketEnvelope* envelope2 = envelope.TakeUserData<SimulationPacketEnvelope>();
			OnEnvelopeDelivered(connection, envelope2);
			SimulationPacketEnvelope.Free(this, ref envelope2);
			break;
		}
		case NetPacketType.NotifyReliableData:
		{
			byte* memory = envelope.TakeUserData<byte>();
			Native.Free(ref memory);
			break;
		}
		}
	}

	internal NetworkObjectHeaderSnapshot GetSnapshot()
	{
		return (_snapshotsPool.Count > 0) ? _snapshotsPool.Pop() : new NetworkObjectHeaderSnapshot(_allocator);
	}

	internal int GetObjectsAllocatorUsedSegmentsInBytes()
	{
		return _allocatorObjects.GetTotalSegmentsUsedInBytes();
	}

	internal int GetGeneralAllocatorUsedSegmentsInBytes()
	{
		return _allocator.GetTotalSegmentsUsedInBytes();
	}

	internal int GetObjectsAllocatorFreeSegmentsInBytes()
	{
		return _allocatorObjects.GetFreeSegmentsInBytes();
	}

	internal int GetGeneralAllocatorFreeSegmentsInBytes()
	{
		return _allocator.GetFreeSegmentsInBytes();
	}

	internal void GetMemorySnapshot(MemoryStatisticsSnapshot.TargetAllocator targetAllocator, ref MemoryStatisticsSnapshot snapshot)
	{
		Allocator allocator = ((targetAllocator == MemoryStatisticsSnapshot.TargetAllocator.General) ? _allocator : _allocatorObjects);
		allocator.GetMemorySnapshot(ref snapshot);
	}

	internal void SnapshotRelease(NetworkObjectHeaderSnapshot snapshot)
	{
		snapshot.Release();
		_snapshotsPool.Push(snapshot);
	}

	internal void SnapshotRelease(ref NetworkObjectHeaderSnapshot snapshot)
	{
		if (snapshot != null)
		{
			NetworkObjectHeaderSnapshot snapshot2 = snapshot;
			snapshot = null;
			SnapshotRelease(snapshot2);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal bool IsSimulated(NetworkObjectMeta meta)
	{
		return meta != null && (bool)meta.Instance && meta.Instance.IsInSimulation;
	}

	internal bool HasObject(NetworkId id)
	{
		if (_metaLookup.TryGetValue(id, out var value))
		{
			Assert.Check(id == value.Id);
			return true;
		}
		return false;
	}

	internal void LogAllObjectIds()
	{
		InternalLogStreams.LogWarn?.Log(string.Join(", ", _metaLookup.Select((KeyValuePair<NetworkId, NetworkObjectMeta> x) => $"{x.Key} == {x.Value.Id}")));
	}

	internal bool TryGetMeta(NetworkId id, out NetworkObjectMeta meta)
	{
		meta = null;
		if (_metaLookup.TryGetValue(id, out meta))
		{
			Assert.Check(id == meta.Id);
			return true;
		}
		return false;
	}

	internal NetworkObjectMeta GetMeta(NetworkId id)
	{
		NetworkObjectMeta networkObjectMeta = _metaLookup[id];
		Assert.Check(networkObjectMeta.Id == id);
		return networkObjectMeta;
	}

	internal unsafe NetworkId GetNextId()
	{
		NetworkId result = default(NetworkId);
		result.Raw = ++_idCounter;
		if (IsClient)
		{
			Assert.Check(Topology == Topologies.Shared);
			Assert.Check(LocalPlayer.IsRealPlayer);
			result.Raw &= 524287u;
			result.Raw |= (((Client)this).ServerConnection->Counter << 19) & 0xFFF80000u;
		}
		return result;
	}

	internal NetworkObjectHeaderSnapshotRef GetLatestSnapshot(NetworkId id)
	{
		if (TryGetMeta(id, out var meta) && meta.HasSnapshots)
		{
			return meta.SnapshotLatest;
		}
		return default(NetworkObjectHeaderSnapshotRef);
	}

	internal unsafe bool TryGetStructData<T>(NetworkId id, out T* data) where T : unmanaged
	{
		if (TryGetStruct(id, out var meta))
		{
			data = meta.GetDataAs<T>();
			return true;
		}
		data = null;
		return false;
	}

	internal bool TryGetStruct(NetworkId id, out NetworkObjectMeta meta)
	{
		if (_metaLookup.TryGetValue(id, out meta))
		{
			Assert.Check(id == meta.Id);
			Assert.Check((meta.Flags & NetworkObjectHeaderFlags.Struct) == NetworkObjectHeaderFlags.Struct);
			return true;
		}
		return false;
	}

	internal bool TryGetInstance(NetworkId id, out NetworkObject instance)
	{
		Assert.Check(!id.IsReserved);
		if (_metaLookup.TryGetValue(id, out var value) && BehaviourUtils.IsAlive(value.Instance))
		{
			Assert.Check(id == value.Id);
			instance = value.Instance;
			return true;
		}
		instance = null;
		return false;
	}

	internal unsafe T* AllocateStruct<T>(NetworkId id, int extraWords = 0, NetworkObjectTypeId? objectTypeId = null) where T : unmanaged
	{
		NetworkObjectMeta networkObjectMeta = AllocateStruct(id, Native.RoundToAlignment(sizeof(T), 4) / 4 + extraWords, objectTypeId);
		return networkObjectMeta.GetDataAs<T>();
	}

	internal NetworkObjectMeta AllocateStruct(NetworkId id, int words, NetworkObjectTypeId? objectTypeId = null)
	{
		Assert.Check(id.IsValid);
		InternalLogStreams.LogDebug?.Log(this, $"allocating struct {id} (words: {words})");
		int wordCount = 20 + words;
		return AllocateObject(id, wordCount, objectTypeId.GetValueOrDefault(), 0, default(NetworkId), default(NetworkObjectNestingKey), NetworkObjectHeaderFlags.Struct);
	}

	internal unsafe NetworkObjectMeta AllocateObject(in NetworkObjectHeader header)
	{
		Assert.Check(header.Id.IsValid);
		int blockByteSize = _projectConfig.Heap.ToAllocatorConfig().BlockByteSize;
		Assert.Always(header.WordCount >= 20 && header.WordCount * 4 <= blockByteSize, "{0} >= NetworkObjectHeader.WORDS && {1} <= {2}", header.WordCount, header.WordCount * 4, blockByteSize);
		Assert.Always(!HasObject(header.Id), "id already exists");
		void* ptr = Allocator.AllocAndClear(_allocatorObjects, header.WordCount * 4);
		NetworkObjectHeader* ptr2 = (NetworkObjectHeader*)ptr;
		*ptr2 = header;
		NetworkObjectMeta networkObjectMeta = new NetworkObjectMeta(this, _allocator);
		networkObjectMeta.Init((int*)ptr, header.WordCount, header.BehaviourCount, header.Flags);
		_metaLookup.Add(ptr2->Id, networkObjectMeta);
		HostMigrationAfterAllocateObject(networkObjectMeta);
		if ((header.Flags & NetworkObjectHeaderFlags.Struct) == NetworkObjectHeaderFlags.Struct)
		{
			_structs.Add(header.Id);
			_structsVersion++;
		}
		return networkObjectMeta;
	}

	internal NetworkObjectMeta AllocateObject(NetworkId id, int wordCount, NetworkObjectTypeId type = default(NetworkObjectTypeId), int behaviourCount = 0, NetworkId nestingRoot = default(NetworkId), NetworkObjectNestingKey nestingKey = default(NetworkObjectNestingKey), NetworkObjectHeaderFlags flags = (NetworkObjectHeaderFlags)0)
	{
		NetworkObjectHeader header = new NetworkObjectHeader(id, (short)wordCount, (short)behaviourCount, type, nestingRoot, nestingKey, flags);
		return AllocateObject(in header);
	}

	internal unsafe void FreeObject(NetworkId id)
	{
		if (!id.IsValid || !_metaLookup.TryGetValue(id, out var value))
		{
			return;
		}
		if (id.Raw <= 1023)
		{
			InternalLogStreams.LogError?.Log($"Trying do free an internal object that never should be freed: {id}");
			return;
		}
		_metaLookup.Remove(id);
		if ((IsServer || Config.Topology == Topologies.Shared) && Config.SchedulingEnabled)
		{
			foreach (SimulationConnection value2 in _connections.Values)
			{
				if (value2.TryGetObjectData(id, out var data))
				{
					value2.ObjectPriorityList.Remove(data);
				}
			}
		}
		if (IsServer && Config.AreaOfInterestEnabled)
		{
			AOI_RemoveFromAreaOfInterest(value, invokeExit: true);
		}
		HostMigrationAfterFreeObject(value);
		if ((value.Flags & NetworkObjectHeaderFlags.Struct) == NetworkObjectHeaderFlags.Struct)
		{
			_structs.Remove(id);
			_structsVersion++;
			if (value.Type == NetworkObjectTypeId.PlayerData)
			{
				PlayerSimulationData* dataAs = value.GetDataAs<PlayerSimulationData>();
				PlayerRef player = dataAs->Player;
				if (player.IsRealPlayer)
				{
					if (dataAs->Object != default(NetworkId))
					{
						_playerLeftTempObjectCache.Add(player, dataAs->Object);
					}
					_invokeJoinedLeaveQueue.Enqueue((player, false));
					if (!IsServer)
					{
						_players.Remove(player);
					}
				}
			}
		}
		value.Release(_allocatorObjects);
	}

	internal int GetRpcSourceAuthorityMask(NetworkObjectMeta meta, PlayerRef player)
	{
		return AuthorityMasks.Create(IsStateAuthority(meta.StateAuthority, player), IsInputAuthority(meta.InputAuthority, player));
	}

	internal int GetLocalAuthorityMask([In][RequiresLocation] ref NetworkObjectHeader obj)
	{
		return AuthorityMasks.Create(IsLocalSimulationStateAuthority(ref obj), IsLocalSimulationInputAuthority(ref obj));
	}

	internal unsafe RpcTargetStatus GetRpcTargetStatus(PlayerRef target)
	{
		if (target == LocalPlayer)
		{
			return RpcTargetStatus.Self;
		}
		if (IsServer)
		{
			if (target.IsNone)
			{
				return RpcTargetStatus.Self;
			}
			if (GetConnectionIndexForPlayer(target).HasValue && NetPeerGroup.TryGetConnectionByIndex(_netPeerGroup, GetConnectionIndexForPlayer(target).Value, out var connection) && connection->Active && connection->ConnectionStatus == NetConnectionStatus.Connected)
			{
				return RpcTargetStatus.Remote;
			}
			return RpcTargetStatus.Unreachable;
		}
		if (target.IsRealPlayer || target.IsNone)
		{
			return RpcTargetStatus.Remote;
		}
		return RpcTargetStatus.Unreachable;
	}

	internal unsafe RpcSendMessageResult SendMessage(ref SimulationMessage* message)
	{
		int num = 0;
		try
		{
			NetworkId messageTargetObjectIdForVerification = GetMessageTargetObjectIdForVerification(message);
			message->Tick = Tick;
			if (IsClient)
			{
				InternalLogStreams.LogTraceSimulationMessage?.Log(this, $"Sending to server {LogUtils.GetDump(message)}");
				PlayerRef none = PlayerRef.None;
				NetConnection* connectionByIndex = NetPeerGroup.GetConnectionByIndex(_netPeerGroup, 0);
				Assert.Check(_netPeerGroup->ConnectionCount == 1);
				if (!connectionByIndex->Active || connectionByIndex->ConnectionStatus != NetConnectionStatus.Connected)
				{
					InternalLogStreams.LogDebug?.Warn(string.Format("Failed to send {0} to {1}: connection not active and/or connected", "SimulationMessage", none));
					return RpcSendMessageResult.NotSentTargetClientNotAvailable;
				}
				if (!VerifyMessageTargetObject(connectionByIndex, messageTargetObjectIdForVerification, out var result))
				{
					InternalLogStreams.LogDebug?.Warn(this, $"Message not sent to {none}. Reason: {result} {LogUtils.GetDump(message)}");
					return VerifyResultToSendMessageResult(result);
				}
				SendMessageInternal(message, connectionByIndex);
				num = 1;
				return RpcSendMessageResult.SentToServerForForwarding;
			}
			if (message->IsTargeted())
			{
				InternalLogStreams.LogTraceSimulationMessage?.Log(this, $"Server sending to a specific player {LogUtils.GetDump(message)}");
				Assert.Check(message->GetFlag(16));
				Assert.Check(message->Target.IsRealPlayer);
				PlayerRef target = message->Target;
				message->Target = default(PlayerRef);
				if (!GetConnectionIndexForPlayer(target).HasValue || !NetPeerGroup.TryGetConnectionByIndex(_netPeerGroup, GetConnectionIndexForPlayer(target).Value, out var connection))
				{
					InternalLogStreams.LogDebug?.Warn(string.Format("Failed to send {0} to {1}: connection not found", "SimulationMessage", target));
					return RpcSendMessageResult.NotSentTargetClientNotAvailable;
				}
				if (!connection->Active || connection->ConnectionStatus != NetConnectionStatus.Connected)
				{
					InternalLogStreams.LogDebug?.Warn(string.Format("Failed to send {0} to {1}: connection not active and/or connected", "SimulationMessage", target));
					return RpcSendMessageResult.NotSentTargetClientNotAvailable;
				}
				if (!VerifyMessageTargetObject(connection, messageTargetObjectIdForVerification, out var result2))
				{
					InternalLogStreams.LogDebug?.Warn(this, $"Message not sent to {target}. Reason: {result2} {LogUtils.GetDump(message)}");
					return VerifyResultToSendMessageResult(result2);
				}
				SendMessageInternal(message, connection);
				num = 1;
				return RpcSendMessageResult.SentToTargetClient;
			}
			InternalLogStreams.LogTraceSimulationMessage?.Log(this, $"Server broadcasting to clients {LogUtils.GetDump(message)}");
			NetConnectionMap.Iterator iterator = NetPeerGroup.ConnectionIterator(_netPeerGroup);
			bool flag = false;
			while (iterator.Next())
			{
				if (iterator.Current->ConnectionStatus == NetConnectionStatus.Connected)
				{
					flag = true;
					PlayerRef playerRef = Connection2Player(iterator.Current);
					if (!VerifyMessageTargetObject(iterator.Current, messageTargetObjectIdForVerification, out var result3))
					{
						InternalLogStreams.LogTraceSimulationMessage?.Log(this, $"Server broadcast message not sent to {playerRef}. Reason: {result3} {LogUtils.GetDump(message)}");
						continue;
					}
					SendMessageInternal(message, iterator.Current);
					num++;
				}
			}
			return (!flag) ? RpcSendMessageResult.NotSentBroadcastNoActiveConnections : ((num == 0) ? RpcSendMessageResult.NotSentBroadcastNoConfirmedNorInterestedClients : RpcSendMessageResult.SentBroadcast);
		}
		finally
		{
			if (num == 0)
			{
				SimulationMessage.Free(this, ref message);
			}
		}
		static RpcSendMessageResult VerifyResultToSendMessageResult(TargetObjectVerificationResult status)
		{
			if (status != TargetObjectVerificationResult.TargetNotInterestedInObject)
			{
				throw new ArgumentOutOfRangeException("status");
			}
			return RpcSendMessageResult.NotSentTargetObjectNotInPlayerInterest;
		}
	}

	internal unsafe bool ForwardMessage(SimulationMessage* message, PlayerRef target, bool required)
	{
		Assert.Check(IsServer, "Only server can forward messages");
		Assert.Check(message->GetFlag(2), "Only received messages are to be forwarded");
		if (!TryGetSimulationConnectionForPlayer(target, out var sc))
		{
			InternalLogStreams.LogDebug?.Error(this, $"Failed to forward to {target}: simulation connection not found {LogUtils.GetDump(message)}");
			return false;
		}
		if (!NetPeerGroup.TryGetConnectionByIndex(_netPeerGroup, sc.ConnectionIndex, out var connection))
		{
			if (required)
			{
				InternalLogStreams.LogDebug?.Error(this, $"Failed to forward to {target}: connection not found {LogUtils.GetDump(message)}");
			}
			return false;
		}
		if (!connection->Active || connection->ConnectionStatus != NetConnectionStatus.Connected)
		{
			if (required)
			{
				InternalLogStreams.LogDebug?.Error(this, $"Failed to forward to {target}: connection not active and/or connected {LogUtils.GetDump(message)}");
			}
			return false;
		}
		NetworkId messageTargetObjectIdForVerification = GetMessageTargetObjectIdForVerification(message);
		if (!VerifyMessageTargetObject(connection, messageTargetObjectIdForVerification, out var result))
		{
			if (required)
			{
				InternalLogStreams.LogDebug?.Warn(this, $"Failed to forward to {target} to {messageTargetObjectIdForVerification}: {result} {LogUtils.GetDump(message)}");
			}
			else
			{
				InternalLogStreams.LogTraceSimulationMessage?.Log(this, $"Failed to forward to {target} to {messageTargetObjectIdForVerification}: {result} {LogUtils.GetDump(message)}");
			}
			return false;
		}
		message->Tick = Tick;
		if (message->IsTargeted())
		{
			Assert.Check(message->Target == target, "When forwarding a targeted message, target should match the target player");
			message->Target = PlayerRef.None;
		}
		if (message->Offset == 0)
		{
			Assert.Check(message->Offset == 0 || message->Offset == message->Capacity);
			message->Offset = message->Capacity;
		}
		InternalLogStreams.LogTraceSimulationMessage?.Log(this, $"Forwarding to {target} {LogUtils.GetDump(message)}");
		SendMessageInternal(message, connection);
		return true;
	}

	internal unsafe NetworkId GetMessageTargetObjectIdForVerification(SimulationMessage* message)
	{
		if (message->GetFlag(1) || message->GetFlag(4) || message->GetFlag(64))
		{
			return default(NetworkId);
		}
		return SimulationMessage.GetRawData(message).Read<RpcHeader>().Object;
	}

	internal unsafe void SendInternalSimulationMessage<T>(SimulationMessageInternalTypes type, T buffer, PlayerRef? target = null) where T : unmanaged
	{
		Assert.Check(type > (SimulationMessageInternalTypes)0);
		SimulationMessage* message = SimulationMessage.Allocate(this, 4 + sizeof(T));
		message->Flags |= 64;
		GetMessageInternalType(message) = type;
		GetMessageInternalData<T>(message) = buffer;
		if (target.HasValue)
		{
			Assert.Check(IsServer);
			message->Target = target.Value;
			message->Flags |= 16;
		}
		else
		{
			message->Target = default(PlayerRef);
			if (IsClient)
			{
				message->Flags |= 32;
			}
		}
		message->Offset = message->Capacity;
		SendMessage(ref message);
	}

	private unsafe bool VerifyMessageTargetObject(NetConnection* netConnection, NetworkId id, out TargetObjectVerificationResult result)
	{
		if (!id.IsValid)
		{
			result = TargetObjectVerificationResult.Ok;
			return true;
		}
		if (!TryGetSimulationConnectionLogErrorIfFailed(netConnection, out var result2))
		{
			result = TargetObjectVerificationResult.Ok;
			return true;
		}
		if (result2.ObjectData_IsCreateUnconfirmed(id).GetValueOrDefault())
		{
			result = TargetObjectVerificationResult.Ok;
			return true;
		}
		if ((Config.ReplicationFeatures & NetworkProjectConfig.ReplicationFeatures.SchedulingAndInterestManagement) == NetworkProjectConfig.ReplicationFeatures.SchedulingAndInterestManagement && IsServer)
		{
			PlayerRef player = Connection2Player(netConnection);
			if (!Replicator.HasObjectInterest(player, id))
			{
				result = TargetObjectVerificationResult.TargetNotInterestedInObject;
				return false;
			}
		}
		result = TargetObjectVerificationResult.Ok;
		return true;
	}

	private unsafe void SendMessageInternal(SimulationMessage* message, NetConnection* netConnection)
	{
		InternalLogStreams.LogTraceSimulationMessage?.Log(this, $"Sending to {Connection2Player(netConnection)} {LogUtils.GetDump(message)}");
		if (TryGetSimulationConnectionLogErrorIfFailed(netConnection, out var result))
		{
			ulong sequence = (message->GetFlag(8) ? 0 : (++result.MessagesOutSequence));
			result.MessagesOut.AddLast(SimulationMessageEnvelope.Allocate(this, message, sequence));
		}
	}

	private void HostMigrationAfterFreeObject(NetworkObjectMeta meta)
	{
		if (Mode == SimulationModes.Host)
		{
			_metaMigration.Remove(meta);
			_metaMigrationRemoved.Enqueue(meta.Id);
		}
		if (meta.Type.IsSceneObject && _metaSceneLookup.ContainsKey(meta.Type))
		{
			_metaSceneLookup.Remove(meta.Type);
		}
	}

	private void HostMigrationAfterAllocateObject(NetworkObjectMeta meta)
	{
		if (meta.Type.IsSceneObject)
		{
			_metaSceneLookup[meta.Type] = meta;
		}
		if (Mode == SimulationModes.Host)
		{
			_metaMigration.AddLast(meta);
		}
	}

	private void HostMigrationDispose()
	{
		if (this is Server server)
		{
			server.DisposeHostMigration();
		}
	}

	internal bool TryGetSceneInstance(NetworkObjectTypeId sceneObjectTypeId, out NetworkObject instance)
	{
		Assert.Check(sceneObjectTypeId.IsSceneObject);
		if (_metaSceneLookup.TryGetValue(sceneObjectTypeId, out var value) && BehaviourUtils.IsAlive(value.Instance))
		{
			instance = value.Instance;
			return true;
		}
		instance = null;
		return false;
	}
}
