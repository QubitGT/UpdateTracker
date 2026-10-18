using System.Threading;
using UnityEngine;

namespace Cysharp.Threading.Tasks.Triggers;

[DisallowMultipleComponent]
public sealed class AsyncMouseOverTrigger : AsyncTriggerBase<AsyncUnit>
{
	private void OnMouseOver()
	{
		RaiseEvent(AsyncUnit.Default);
	}

	public IAsyncOnMouseOverHandler GetOnMouseOverAsyncHandler()
	{
		return new AsyncTriggerHandler<AsyncUnit>(this, callOnce: false);
	}

	public IAsyncOnMouseOverHandler GetOnMouseOverAsyncHandler(CancellationToken cancellationToken)
	{
		return new AsyncTriggerHandler<AsyncUnit>(this, cancellationToken, callOnce: false);
	}

	public UniTask OnMouseOverAsync()
	{
		return ((IAsyncOnMouseOverHandler)new AsyncTriggerHandler<AsyncUnit>(this, callOnce: true)).OnMouseOverAsync();
	}

	public UniTask OnMouseOverAsync(CancellationToken cancellationToken)
	{
		return ((IAsyncOnMouseOverHandler)new AsyncTriggerHandler<AsyncUnit>(this, cancellationToken, callOnce: true)).OnMouseOverAsync();
	}
}
