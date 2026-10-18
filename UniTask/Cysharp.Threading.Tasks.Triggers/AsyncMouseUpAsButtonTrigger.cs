using System.Threading;
using UnityEngine;

namespace Cysharp.Threading.Tasks.Triggers;

[DisallowMultipleComponent]
public sealed class AsyncMouseUpAsButtonTrigger : AsyncTriggerBase<AsyncUnit>
{
	private void OnMouseUpAsButton()
	{
		RaiseEvent(AsyncUnit.Default);
	}

	public IAsyncOnMouseUpAsButtonHandler GetOnMouseUpAsButtonAsyncHandler()
	{
		return new AsyncTriggerHandler<AsyncUnit>(this, callOnce: false);
	}

	public IAsyncOnMouseUpAsButtonHandler GetOnMouseUpAsButtonAsyncHandler(CancellationToken cancellationToken)
	{
		return new AsyncTriggerHandler<AsyncUnit>(this, cancellationToken, callOnce: false);
	}

	public UniTask OnMouseUpAsButtonAsync()
	{
		return ((IAsyncOnMouseUpAsButtonHandler)new AsyncTriggerHandler<AsyncUnit>(this, callOnce: true)).OnMouseUpAsButtonAsync();
	}

	public UniTask OnMouseUpAsButtonAsync(CancellationToken cancellationToken)
	{
		return ((IAsyncOnMouseUpAsButtonHandler)new AsyncTriggerHandler<AsyncUnit>(this, cancellationToken, callOnce: true)).OnMouseUpAsButtonAsync();
	}
}
