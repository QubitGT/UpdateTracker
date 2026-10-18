using System;
using System.Collections.Generic;
using UnityEngine;

namespace Oculus.Interaction.Locomotion;

public class LocomotionEventsConnection : MonoBehaviour, ILocomotionEventHandler, ILocomotionEventBroadcaster
{
	[SerializeField]
	[Interface(typeof(ILocomotionEventBroadcaster), new Type[] { })]
	[Optional(OptionalAttribute.Flag.DontHide)]
	private List<UnityEngine.Object> _broadcasters;

	[Obsolete("Use the list of Handlers instead")]
	[SerializeField]
	[Interface(typeof(ILocomotionEventHandler), new Type[] { })]
	[Optional(OptionalAttribute.Flag.Obsolete)]
	private UnityEngine.Object _handler;

	[SerializeField]
	[Interface(typeof(ILocomotionEventHandler), new Type[] { })]
	private List<UnityEngine.Object> _handlers;

	private bool _started;

	private List<ILocomotionEventBroadcaster> Broadcasters { get; set; }

	private List<ILocomotionEventHandler> Handlers { get; set; }

	public event Action<LocomotionEvent> WhenLocomotionPerformed = delegate
	{
	};

	public event Action<LocomotionEvent, Pose> WhenLocomotionEventHandled = delegate
	{
	};

	protected virtual void Awake()
	{
		if (Broadcasters == null)
		{
			Broadcasters = _broadcasters.ConvertAll((UnityEngine.Object b) => b as ILocomotionEventBroadcaster);
		}
		if (Handlers == null)
		{
			Handlers = _handlers.ConvertAll((UnityEngine.Object b) => b as ILocomotionEventHandler);
			if (_handler is ILocomotionEventHandler item && !Handlers.Contains(item))
			{
				Handlers.Add(item);
			}
		}
	}

	protected virtual void Start()
	{
		this.BeginStart(ref _started);
		this.EndStart(ref _started);
	}

	protected virtual void OnEnable()
	{
		if (!_started)
		{
			return;
		}
		foreach (ILocomotionEventBroadcaster broadcaster in Broadcasters)
		{
			broadcaster.WhenLocomotionPerformed += HandleLocomotionEvent;
		}
		foreach (ILocomotionEventHandler handler in Handlers)
		{
			handler.WhenLocomotionEventHandled += HandlerWhenLocomotionEventHandled;
		}
	}

	protected virtual void OnDisable()
	{
		if (!_started)
		{
			return;
		}
		foreach (ILocomotionEventBroadcaster broadcaster in Broadcasters)
		{
			broadcaster.WhenLocomotionPerformed -= HandleLocomotionEvent;
		}
		foreach (ILocomotionEventHandler handler in Handlers)
		{
			handler.WhenLocomotionEventHandled -= HandlerWhenLocomotionEventHandled;
		}
	}

	private void HandlerWhenLocomotionEventHandled(LocomotionEvent arg1, Pose arg2)
	{
		this.WhenLocomotionEventHandled(arg1, arg2);
	}

	public void HandleLocomotionEvent(LocomotionEvent locomotionEvent)
	{
		if (!_started || !base.isActiveAndEnabled)
		{
			return;
		}
		this.WhenLocomotionPerformed(locomotionEvent);
		foreach (ILocomotionEventHandler handler in Handlers)
		{
			handler.HandleLocomotionEvent(locomotionEvent);
		}
	}

	public void InjectAllLocomotionBroadcastersHandlerConnection(List<ILocomotionEventHandler> handlers)
	{
		InjectHandlers(handlers);
	}

	public void InjectOptionalBroadcasters(List<ILocomotionEventBroadcaster> broadcasters)
	{
		Broadcasters = broadcasters;
		_broadcasters = broadcasters.ConvertAll((ILocomotionEventBroadcaster b) => b as UnityEngine.Object);
	}

	public void InjectHandlers(List<ILocomotionEventHandler> handlers)
	{
		Handlers = handlers;
		_handlers = handlers.ConvertAll((ILocomotionEventHandler b) => b as UnityEngine.Object);
	}

	[Obsolete("Use the list version instead")]
	public void InjectHandler(ILocomotionEventHandler handler)
	{
		_handler = handler as UnityEngine.Object;
	}
}
