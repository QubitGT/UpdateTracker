using System;
using System.Collections.Generic;
using UnityEngine;

namespace Oculus.Interaction;

public class CollisionInteractionRegistry<TInteractor, TInteractable> : InteractableRegistry<TInteractor, TInteractable> where TInteractor : Interactor<TInteractor, TInteractable>, IRigidbodyRef where TInteractable : Interactable<TInteractor, TInteractable>, IRigidbodyRef
{
	private Dictionary<Rigidbody, HashSet<TInteractable>> _rigidbodyCollisionMap;

	private Dictionary<TInteractable, InteractableTriggerBroadcaster> _broadcasters;

	private static readonly InteractableSet _empty;

	public CollisionInteractionRegistry()
	{
		_rigidbodyCollisionMap = new Dictionary<Rigidbody, HashSet<TInteractable>>();
		_broadcasters = new Dictionary<TInteractable, InteractableTriggerBroadcaster>();
	}

	public override void Register(TInteractable interactable)
	{
		base.Register(interactable);
		GameObject gameObject = interactable.Rigidbody.gameObject;
		if (!_broadcasters.TryGetValue(interactable, out var value))
		{
			value = gameObject.AddComponent<InteractableTriggerBroadcaster>();
			value.InjectAllInteractableTriggerBroadcaster(interactable);
			_broadcasters.Add(interactable, value);
			InteractableTriggerBroadcaster interactableTriggerBroadcaster = value;
			interactableTriggerBroadcaster.WhenTriggerEntered = (Action<IInteractable, Rigidbody>)Delegate.Combine(interactableTriggerBroadcaster.WhenTriggerEntered, new Action<IInteractable, Rigidbody>(HandleTriggerEntered));
			InteractableTriggerBroadcaster interactableTriggerBroadcaster2 = value;
			interactableTriggerBroadcaster2.WhenTriggerExited = (Action<IInteractable, Rigidbody>)Delegate.Combine(interactableTriggerBroadcaster2.WhenTriggerExited, new Action<IInteractable, Rigidbody>(HandleTriggerExited));
		}
	}

	public override void Unregister(TInteractable interactable)
	{
		base.Unregister(interactable);
		if (_broadcasters.TryGetValue(interactable, out var value))
		{
			_broadcasters.Remove(interactable);
			if (value != null)
			{
				value.enabled = false;
				InteractableTriggerBroadcaster interactableTriggerBroadcaster = value;
				interactableTriggerBroadcaster.WhenTriggerEntered = (Action<IInteractable, Rigidbody>)Delegate.Remove(interactableTriggerBroadcaster.WhenTriggerEntered, new Action<IInteractable, Rigidbody>(HandleTriggerEntered));
				InteractableTriggerBroadcaster interactableTriggerBroadcaster2 = value;
				interactableTriggerBroadcaster2.WhenTriggerExited = (Action<IInteractable, Rigidbody>)Delegate.Remove(interactableTriggerBroadcaster2.WhenTriggerExited, new Action<IInteractable, Rigidbody>(HandleTriggerExited));
				UnityEngine.Object.Destroy(value);
			}
		}
	}

	private void HandleTriggerEntered(IInteractable interactable, Rigidbody rigidbody)
	{
		TInteractable item = interactable as TInteractable;
		if (!_rigidbodyCollisionMap.ContainsKey(rigidbody))
		{
			_rigidbodyCollisionMap.Add(rigidbody, new HashSet<TInteractable>());
		}
		_rigidbodyCollisionMap[rigidbody].Add(item);
	}

	private void HandleTriggerExited(IInteractable interactable, Rigidbody rigidbody)
	{
		TInteractable item = interactable as TInteractable;
		HashSet<TInteractable> hashSet = _rigidbodyCollisionMap[rigidbody];
		hashSet.Remove(item);
		if (hashSet.Count == 0)
		{
			_rigidbodyCollisionMap.Remove(rigidbody);
		}
	}

	public override InteractableSet List(TInteractor interactor)
	{
		if (_rigidbodyCollisionMap.TryGetValue(interactor.Rigidbody, out var value))
		{
			return List(interactor, value);
		}
		return _empty;
	}
}
