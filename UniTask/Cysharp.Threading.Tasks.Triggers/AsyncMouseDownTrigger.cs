using System.Threading;
using UnityEngine;

namespace Cysharp.Threading.Tasks.Triggers;

[DisallowMultipleComponent]
public sealed class AsyncMouseDownTrigger : AsyncTriggerBase<AsyncUnit>
{
	private void OnMouseDown()
	{
		RaiseEvent(AsyncUnit.Default);
	}

	public IAsyncOnMouseDownHandler GetOnMouseDownAsyncHandler()
	{
		return new AsyncTriggerHandler<AsyncUnit>(this, callOnce: false);
	}

	public IAsyncOnMouseDownHandler GetOnMouseDownAsyncHandler(CancellationToken cancellationToken)
	{
		return new AsyncTriggerHandler<AsyncUnit>(this, cancellationToken, callOnce: false);
	}

	public UniTask OnMouseDownAsync()
	{
		return ((IAsyncOnMouseDownHandler)new AsyncTriggerHandler<AsyncUnit>(this, callOnce: true)).OnMouseDownAsync();
	}

	public UniTask OnMouseDownAsync(CancellationToken cancellationToken)
	{
		return ((IAsyncOnMouseDownHandler)new AsyncTriggerHandler<AsyncUnit>(this, cancellationToken, callOnce: true)).OnMouseDownAsync();
	}
}
