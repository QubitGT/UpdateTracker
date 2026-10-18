using System;
using System.Runtime.CompilerServices;

public struct OVRTaskBuilder<T>
{
	private abstract class PooledStateMachine : IDisposable
	{
		public OVRTask<T>? Task;

		public Action MoveNext;

		public abstract void Dispose();
	}

	private class PooledStateMachine<TStateMachine> : PooledStateMachine, OVRObjectPool.IPoolObject where TStateMachine : IAsyncStateMachine
	{
		public TStateMachine StateMachine;

		public static PooledStateMachine<TStateMachine> Get()
		{
			return OVRObjectPool.Get<PooledStateMachine<TStateMachine>>();
		}

		public override void Dispose()
		{
			OVRObjectPool.Return(this);
		}

		public PooledStateMachine()
		{
			MoveNext = ExecuteMoveNext;
		}

		private void ExecuteMoveNext()
		{
			StateMachine.MoveNext();
		}

		void OVRObjectPool.IPoolObject.OnGet()
		{
			StateMachine = default(TStateMachine);
			Task = null;
		}

		void OVRObjectPool.IPoolObject.OnReturn()
		{
			StateMachine = default(TStateMachine);
			Task = null;
		}
	}

	private PooledStateMachine _pooledStateMachine;

	private OVRTask<T>? _task;

	public OVRTask<T> Task
	{
		get
		{
			if (_task.HasValue)
			{
				return _task.Value;
			}
			OVRTask<T>? oVRTask;
			if (_pooledStateMachine != null)
			{
				PooledStateMachine pooledStateMachine = _pooledStateMachine;
				OVRTask<T> valueOrDefault = pooledStateMachine.Task.GetValueOrDefault();
				OVRTask<T> value;
				if (!pooledStateMachine.Task.HasValue)
				{
					valueOrDefault = OVRTask.FromGuid<T>(Guid.NewGuid());
					pooledStateMachine.Task = valueOrDefault;
					value = valueOrDefault;
				}
				else
				{
					value = valueOrDefault;
				}
				oVRTask = (_task = value);
				return oVRTask.Value;
			}
			oVRTask = (_task = OVRTask.FromGuid<T>(Guid.NewGuid()));
			return oVRTask.Value;
		}
	}

	public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : INotifyCompletion where TStateMachine : IAsyncStateMachine
	{
		PooledStateMachine pooledStateMachine = GetPooledStateMachine<TStateMachine>();
		((PooledStateMachine<TStateMachine>)pooledStateMachine).StateMachine = stateMachine;
		awaiter.OnCompleted(pooledStateMachine.MoveNext);
	}

	public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : ICriticalNotifyCompletion where TStateMachine : IAsyncStateMachine
	{
		PooledStateMachine pooledStateMachine = GetPooledStateMachine<TStateMachine>();
		((PooledStateMachine<TStateMachine>)pooledStateMachine).StateMachine = stateMachine;
		awaiter.UnsafeOnCompleted(pooledStateMachine.MoveNext);
	}

	public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine
	{
		((PooledStateMachine<TStateMachine>)GetPooledStateMachine<TStateMachine>()).StateMachine = stateMachine;
		stateMachine.MoveNext();
	}

	public static OVRTaskBuilder<T> Create()
	{
		return default(OVRTaskBuilder<T>);
	}

	private PooledStateMachine GetPooledStateMachine<TStateMachine>() where TStateMachine : IAsyncStateMachine
	{
		if (_pooledStateMachine == null)
		{
			_pooledStateMachine = PooledStateMachine<TStateMachine>.Get();
			_pooledStateMachine.Task = _task;
		}
		return _pooledStateMachine;
	}

	public void SetException(Exception exception)
	{
		Task.SetException(exception);
		_pooledStateMachine?.Dispose();
		_pooledStateMachine = null;
	}

	public void SetResult(T result)
	{
		Task.SetResult(result);
		_pooledStateMachine?.Dispose();
		_pooledStateMachine = null;
	}

	public void SetStateMachine(IAsyncStateMachine stateMachine)
	{
	}
}
