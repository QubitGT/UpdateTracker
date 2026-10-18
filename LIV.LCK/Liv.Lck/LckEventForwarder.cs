using System;

namespace Liv.Lck;

internal class LckEventForwarder<TEvent, TResult> : IDisposable
{
	private readonly ILckEventBus _eventBus;

	private readonly Func<TEvent, TResult> _selector;

	private readonly Action<TResult> _forwardingAction;

	internal LckEventForwarder(ILckEventBus eventBus, Func<TEvent, TResult> selector, Action<TResult> forwardingAction)
	{
		_eventBus = eventBus;
		_selector = selector;
		_forwardingAction = forwardingAction;
		_eventBus.AddListener<TEvent>(OnEventReceived);
	}

	private void OnEventReceived(TEvent evt)
	{
		TResult obj = _selector(evt);
		_forwardingAction(obj);
	}

	public void Dispose()
	{
		_eventBus.RemoveListener<TEvent>(OnEventReceived);
	}
}
