using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

namespace Liv.Lck;

internal class LckEventBus : ILckEventBus
{
	private readonly Dictionary<Type, Delegate> _delegates = new Dictionary<Type, Delegate>();

	[Preserve]
	public LckEventBus()
	{
	}

	public void AddListener<T>(Action<T> listener)
	{
		if (!_delegates.ContainsKey(typeof(T)))
		{
			_delegates[typeof(T)] = null;
		}
		_delegates[typeof(T)] = (Action<T>)Delegate.Combine((Action<T>)_delegates[typeof(T)], listener);
	}

	public void RemoveListener<T>(Action<T> listener)
	{
		if (_delegates.TryGetValue(typeof(T), out var value))
		{
			Action<T> action = (Action<T>)Delegate.Remove((Action<T>)value, listener);
			if (action == null)
			{
				_delegates.Remove(typeof(T));
			}
			else
			{
				_delegates[typeof(T)] = action;
			}
		}
	}

	public void Trigger<T>(T eventData)
	{
		if (_delegates.TryGetValue(typeof(T), out var value))
		{
			(value as Action<T>)?.Invoke(eventData);
		}
	}
}
