using System;
using System.Collections.Generic;

namespace Liv.Lck;

internal class LckEventErrorLogger : IDisposable
{
	private class EventSubscription<TEvent> : IDisposable
	{
		private readonly ILckEventBus _eventBus;

		private readonly Action<TEvent> _callback;

		public EventSubscription(ILckEventBus eventBus, Action<TEvent> callback)
		{
			_eventBus = eventBus;
			_callback = callback;
			_eventBus.AddListener(_callback);
		}

		public void Dispose()
		{
			_eventBus.RemoveListener(_callback);
		}
	}

	private readonly ILckEventBus _eventBus;

	private readonly Action<ILckResult> _logAction;

	private readonly List<IDisposable> _subscriptions = new List<IDisposable>();

	public LckEventErrorLogger(ILckEventBus eventBus, Action<ILckResult> logAction)
	{
		_eventBus = eventBus;
		_logAction = logAction;
	}

	public void Monitor<TEvent, TResult>() where TEvent : LckEvents.IEventWithResult<TResult> where TResult : ILckResult
	{
		EventSubscription<TEvent> item = new EventSubscription<TEvent>(_eventBus, OnEventReceived<TEvent, TResult>);
		_subscriptions.Add(item);
	}

	private void OnEventReceived<TEvent, TResult>(TEvent evt) where TEvent : LckEvents.IEventWithResult<TResult> where TResult : ILckResult
	{
		if (!evt.Result.Success)
		{
			_logAction(evt.Result);
		}
	}

	public void Dispose()
	{
		foreach (IDisposable subscription in _subscriptions)
		{
			subscription.Dispose();
		}
		_subscriptions.Clear();
	}
}
