#define DEBUG
using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Fusion;

public readonly struct NetworkSpawnOp
{
	internal class AsyncOpData
	{
		public NetworkSpawnStatus Status;

		public NetworkObject Object;

		public event Action Completed;

		public void Complete(in NetworkSpawnOp op)
		{
			Assert.Check(Status == NetworkSpawnStatus.Queued);
			Assert.Check(op.Status != NetworkSpawnStatus.Queued);
			Status = op.Status;
			Object = op.Object;
			this.Completed?.Invoke();
		}
	}

	public struct Awaiter : INotifyCompletion
	{
		private NetworkSpawnOp _op;

		public bool IsCompleted => _op.Status != NetworkSpawnStatus.Queued;

		public Awaiter(in NetworkSpawnOp op)
		{
			_op = op;
		}

		public NetworkObject GetResult()
		{
			if (!IsCompleted)
			{
				SpinWait spinWait = default(SpinWait);
				while (!IsCompleted)
				{
					spinWait.SpinOnce();
				}
			}
			NetworkSpawnStatus status = _op.Status;
			Assert.Check(status != NetworkSpawnStatus.Queued);
			if (status != NetworkSpawnStatus.Spawned)
			{
				throw new NetworkObjectSpawnException(status);
			}
			return _op.Object;
		}

		public void OnCompleted(Action continuation)
		{
			if (IsCompleted)
			{
				continuation();
				return;
			}
			if (_op._data is AsyncOpData asyncOpData)
			{
				SynchronizationContext capturedContext = SynchronizationContext.Current;
				asyncOpData.Completed += delegate
				{
					if (capturedContext != null)
					{
						capturedContext.Post(delegate
						{
							continuation();
						}, null);
					}
					else
					{
						continuation();
					}
				};
				return;
			}
			throw new NotSupportedException();
		}
	}

	public readonly NetworkRunner Runner;

	internal readonly NetworkSpawnStatus _status;

	internal readonly object _data;

	public NetworkObject Object
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			if (_status != 0 || !(_data is AsyncOpData { Object: var @object }))
			{
				return (NetworkObject)_data;
			}
			return @object;
		}
	}

	public NetworkSpawnStatus Status
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			if (_status != 0 || !(_data is AsyncOpData { Status: var status }))
			{
				return _status;
			}
			return status;
		}
	}

	public bool IsSpawned => Status == NetworkSpawnStatus.Spawned;

	public bool IsQueued => Status == NetworkSpawnStatus.Queued;

	public bool IsFailed => Status != NetworkSpawnStatus.Spawned && Status != NetworkSpawnStatus.Queued;

	internal NetworkSpawnOp(NetworkRunner runner, NetworkSpawnStatus status, NetworkObject data)
	{
		Runner = runner;
		_status = status;
		_data = data;
	}

	internal NetworkSpawnOp(NetworkRunner runner, NetworkSpawnStatus status, AsyncOpData data)
	{
		Runner = runner;
		_status = status;
		_data = data;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal NetworkObject ConsumeSyncSpawn(NetworkObjectTypeId typeId)
	{
		if (_status == NetworkSpawnStatus.Queued || _status == NetworkSpawnStatus.Spawned)
		{
			return (NetworkObject)_data;
		}
		throw new NetworkObjectSpawnException(_status, typeId);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal NetworkSpawnStatus ConsumeSyncSpawn(out NetworkObject obj)
	{
		obj = (NetworkObject)_data;
		return _status;
	}

	public Awaiter GetAwaiter()
	{
		return new Awaiter(in this);
	}
}
