using System.Collections.Generic;
using UnityEngine;

namespace Meta.WitAi.Events;

public class EventRegistry
{
	[SerializeField]
	private readonly HashSet<string> _overriddenCallbacks = new HashSet<string>();

	public HashSet<string> OverriddenCallbacks => _overriddenCallbacks;

	public void RegisterOverriddenCallback(string callback)
	{
		_overriddenCallbacks.Add(callback);
	}

	public void RemoveOverriddenCallback(string callback)
	{
		if (_overriddenCallbacks.Contains(callback))
		{
			_overriddenCallbacks.Remove(callback);
		}
	}

	public bool IsCallbackOverridden(string callback)
	{
		return OverriddenCallbacks.Contains(callback);
	}
}
