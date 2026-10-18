using System;
using UnityEngine;

namespace Oculus.Interaction;

public static class MonoBehaviourStartExtensions
{
	public static void BeginStart(this MonoBehaviour monoBehaviour, ref bool started, Action baseStart = null)
	{
		if (!started)
		{
			monoBehaviour.enabled = false;
			started = true;
			baseStart?.Invoke();
			started = false;
		}
		else
		{
			baseStart?.Invoke();
		}
	}

	public static void EndStart(this MonoBehaviour monoBehaviour, ref bool started)
	{
		if (!started)
		{
			started = true;
			monoBehaviour.enabled = true;
		}
	}
}
