using System;
using System.Collections;
using System.Runtime.ExceptionServices;

namespace Fusion;

public sealed class FusionCoroutine : ICoroutine, IAsyncOperation, IEnumerator, IDisposable
{
	private readonly IEnumerator _inner;

	private Action<IAsyncOperation> _completed;

	private float _progress;

	private Action _activateAsync;

	public bool IsDone { get; private set; }

	public ExceptionDispatchInfo Error { get; private set; }

	object IEnumerator.Current => _inner.Current;

	public event Action<IAsyncOperation> Completed
	{
		add
		{
			_completed = (Action<IAsyncOperation>)Delegate.Combine(_completed, value);
			if (IsDone)
			{
				value(this);
			}
		}
		remove
		{
			_completed = (Action<IAsyncOperation>)Delegate.Remove(_completed, value);
		}
	}

	public FusionCoroutine(IEnumerator inner)
	{
		_inner = inner ?? throw new ArgumentNullException("inner");
	}

	bool IEnumerator.MoveNext()
	{
		try
		{
			if (_inner.MoveNext())
			{
				return true;
			}
			IsDone = true;
			_completed?.Invoke(this);
			return false;
		}
		catch (Exception source)
		{
			IsDone = true;
			Error = ExceptionDispatchInfo.Capture(source);
			_completed?.Invoke(this);
			return false;
		}
	}

	void IEnumerator.Reset()
	{
		_inner.Reset();
		IsDone = false;
		Error = null;
	}

	public void Dispose()
	{
		if (_inner is IDisposable disposable)
		{
			disposable.Dispose();
		}
	}
}
