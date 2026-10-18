using System;
using System.Linq;
using UnityEngine;

namespace Modio.Unity.UI.Components;

public abstract class ModioUIPropertiesBase<TOwner, TProperty> : MonoBehaviour where TOwner : Component, IModioUIPropertiesOwner
{
	protected TOwner Owner;

	private IPropertyMonoBehaviourEvents[] _monoBehaviourEvents;

	protected abstract TProperty[] Properties { get; }

	protected virtual void Awake()
	{
		Owner = GetComponentInParent<TOwner>();
		if (Owner != null)
		{
			Owner.AddUpdatePropertiesListener(UpdateProperties);
			_monoBehaviourEvents = (Properties.Any((TProperty property) => property is IPropertyMonoBehaviourEvents) ? Properties.OfType<IPropertyMonoBehaviourEvents>().ToArray() : Array.Empty<IPropertyMonoBehaviourEvents>());
		}
		else
		{
			Debug.LogWarning(GetType().Name + " " + base.gameObject.name + " could not find a TOwner, disabling.", this);
			base.enabled = false;
		}
	}

	protected virtual void Start()
	{
		IPropertyMonoBehaviourEvents[] monoBehaviourEvents = _monoBehaviourEvents;
		for (int i = 0; i < monoBehaviourEvents.Length; i++)
		{
			monoBehaviourEvents[i].Start();
		}
		UpdateProperties();
	}

	protected void OnDestroy()
	{
		if ((bool)Owner)
		{
			Owner.RemoveUpdatePropertiesListener(UpdateProperties);
		}
		IPropertyMonoBehaviourEvents[] monoBehaviourEvents = _monoBehaviourEvents;
		for (int i = 0; i < monoBehaviourEvents.Length; i++)
		{
			monoBehaviourEvents[i].OnDestroy();
		}
	}

	private void OnEnable()
	{
		IPropertyMonoBehaviourEvents[] monoBehaviourEvents = _monoBehaviourEvents;
		for (int i = 0; i < monoBehaviourEvents.Length; i++)
		{
			monoBehaviourEvents[i].OnEnable();
		}
	}

	private void OnDisable()
	{
		IPropertyMonoBehaviourEvents[] monoBehaviourEvents = _monoBehaviourEvents;
		for (int i = 0; i < monoBehaviourEvents.Length; i++)
		{
			monoBehaviourEvents[i].OnDisable();
		}
	}

	protected abstract void UpdateProperties();
}
