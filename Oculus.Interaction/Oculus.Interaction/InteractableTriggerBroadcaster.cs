using System;
using System.Collections.Generic;
using UnityEngine;

namespace Oculus.Interaction;

public class InteractableTriggerBroadcaster : MonoBehaviour
{
	public Action<IInteractable, Rigidbody> WhenTriggerEntered = delegate
	{
	};

	public Action<IInteractable, Rigidbody> WhenTriggerExited = delegate
	{
	};

	private IInteractable _interactable;

	private Dictionary<Rigidbody, bool> _rigidbodyTriggers;

	private List<Rigidbody> _rigidbodies;

	private static HashSet<InteractableTriggerBroadcaster> _broadcasters = new HashSet<InteractableTriggerBroadcaster>();

	protected bool _started;

	private bool _skippedPhysics;

	private bool _forcedGlobalPhysicsUpdate;

	protected virtual void Start()
	{
		this.BeginStart(ref _started);
		_rigidbodyTriggers = new Dictionary<Rigidbody, bool>();
		_rigidbodies = new List<Rigidbody>();
		_skippedPhysics = false;
		_forcedGlobalPhysicsUpdate = false;
		this.EndStart(ref _started);
	}

	protected virtual void OnTriggerStay(Collider collider)
	{
		if (!_started)
		{
			return;
		}
		Rigidbody attachedRigidbody = collider.attachedRigidbody;
		if (!(attachedRigidbody == null))
		{
			if (!_rigidbodyTriggers.ContainsKey(attachedRigidbody))
			{
				WhenTriggerEntered(_interactable, attachedRigidbody);
				_rigidbodyTriggers.Add(attachedRigidbody, value: true);
			}
			else
			{
				_rigidbodyTriggers[attachedRigidbody] = true;
			}
		}
	}

	protected virtual void OnEnable()
	{
		if (_started)
		{
			_broadcasters.Add(this);
		}
	}

	protected virtual void FixedUpdate()
	{
		if (Physics.autoSimulation)
		{
			UpdateTriggers();
		}
		else
		{
			_skippedPhysics = true;
		}
	}

	private void UpdateTriggers()
	{
		_rigidbodies.Clear();
		_rigidbodies.AddRange(_rigidbodyTriggers.Keys);
		foreach (Rigidbody rigidbody in _rigidbodies)
		{
			if (!_rigidbodyTriggers[rigidbody])
			{
				_rigidbodyTriggers.Remove(rigidbody);
				WhenTriggerExited(_interactable, rigidbody);
			}
			else
			{
				_rigidbodyTriggers[rigidbody] = false;
			}
		}
	}

	protected virtual void OnDisable()
	{
		if (!_started)
		{
			return;
		}
		foreach (Rigidbody key in _rigidbodyTriggers.Keys)
		{
			WhenTriggerExited(_interactable, key);
		}
		_broadcasters.Remove(this);
		_rigidbodies.Clear();
	}

	protected virtual void OnDestroy()
	{
		if (_started)
		{
			WhenTriggerEntered = null;
			WhenTriggerExited = null;
		}
	}

	public static void ForceGlobalUpdateTriggers()
	{
		foreach (InteractableTriggerBroadcaster broadcaster in _broadcasters)
		{
			broadcaster._forcedGlobalPhysicsUpdate = true;
			broadcaster.UpdateTriggers();
		}
	}

	public void InjectAllInteractableTriggerBroadcaster(IInteractable interactable)
	{
		InjectInteractable(interactable);
	}

	public void InjectInteractable(IInteractable interactable)
	{
		_interactable = interactable;
	}
}
