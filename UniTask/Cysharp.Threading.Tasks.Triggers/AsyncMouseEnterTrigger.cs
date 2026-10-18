using System.Threading;
using UnityEngine;

namespace Cysharp.Threading.Tasks.Triggers;

[DisallowMultipleComponent]
public sealed class AsyncMouseEnterTrigger : AsyncTriggerBase<AsyncUnit>
{
	private void OnMouseEnter()
	{
		RaiseEvent(AsyncUnit.Default);
	}

	public IAsyncOnMouseEnterHandler GetOnMouseEnterAsyncHandler()
	{
		return new AsyncTriggerHandler<AsyncUnit>(this, callOnce: false);
	}

	public IAsyncOnMouseEnterHandler GetOnMouseEnterAsyncHandler(CancellationToken cancellationToken)
	{
		return new AsyncTriggerHandler<AsyncUnit>(this, cancellationToken, callOnce: false);
	}

	public UniTask OnMouseEnterAsync()
	{
		return ((IAsyncOnMouseEnterHandler)new AsyncTriggerHandler<AsyncUnit>(this, callOnce: true)).OnMouseEnterAsync();
	}

	public UniTask OnMouseEnterAsync(CancellationToken cancellationToken)
	{
		return ((IAsyncOnMouseEnterHandler)new AsyncTriggerHandler<AsyncUnit>(this, cancellationToken, callOnce: true)).OnMouseEnterAsync();
	}
}
