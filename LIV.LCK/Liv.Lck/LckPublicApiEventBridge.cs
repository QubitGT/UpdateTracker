using System;
using System.Collections.Generic;

namespace Liv.Lck;

internal class LckPublicApiEventBridge : IDisposable
{
	private readonly ILckEventBus _eventBus;

	private readonly List<IDisposable> _forwarders = new List<IDisposable>();

	internal LckPublicApiEventBridge(ILckEventBus eventBus)
	{
		_eventBus = eventBus;
	}

	public void Forward<TEvent, TResult>(Action<TResult> publicEventInvoker) where TEvent : LckEvents.IEventWithResult<TResult> where TResult : ILckResult
	{
		_forwarders.Add(new LckEventForwarder<TEvent, TResult>(_eventBus, (TEvent evt) => evt.Result, publicEventInvoker));
	}

	public void Forward<TEvent, TResult>(Func<TEvent, TResult> selector, Action<TResult> publicEventInvoker)
	{
		_forwarders.Add(new LckEventForwarder<TEvent, TResult>(_eventBus, selector, publicEventInvoker));
	}

	public void Dispose()
	{
		foreach (IDisposable forwarder in _forwarders)
		{
			forwarder.Dispose();
		}
		_forwarders.Clear();
	}
}
