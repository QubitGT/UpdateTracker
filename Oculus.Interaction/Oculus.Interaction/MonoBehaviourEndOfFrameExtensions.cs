using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Oculus.Interaction;

internal static class MonoBehaviourEndOfFrameExtensions
{
	private static YieldInstruction _endOfFrame = new WaitForEndOfFrame();

	private static Dictionary<MonoBehaviour, Coroutine> _routines = new Dictionary<MonoBehaviour, Coroutine>();

	internal static void RegisterEndOfFrameCallback(this MonoBehaviour monoBehaviour, Action callback)
	{
		if (_routines.ContainsKey(monoBehaviour))
		{
			throw new ArgumentException("This MonoBehaviour is already registered for the EndOfFrameCallback");
		}
		Coroutine value = monoBehaviour.StartCoroutine(EndOfFrameCoroutine(callback));
		_routines.Add(monoBehaviour, value);
	}

	internal static void UnregisterEndOfFrameCallback(this MonoBehaviour monoBehaviour)
	{
		if (!_routines.ContainsKey(monoBehaviour))
		{
			throw new ArgumentException("This MonoBehaviour is not registered for the EndOfFrameCallback");
		}
		monoBehaviour.StopCoroutine(_routines[monoBehaviour]);
		_routines.Remove(monoBehaviour);
	}

	private static IEnumerator EndOfFrameCoroutine(Action callback)
	{
		while (true)
		{
			yield return _endOfFrame;
			callback();
		}
	}
}
