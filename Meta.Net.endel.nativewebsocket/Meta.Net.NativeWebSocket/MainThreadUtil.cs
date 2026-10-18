using System;
using System.Collections;
using System.Threading;
using UnityEngine;

namespace Meta.Net.NativeWebSocket;

public class MainThreadUtil : MonoBehaviour
{
	public static MainThreadUtil Instance { get; private set; }

	public static SynchronizationContext synchronizationContext { get; private set; }

	private void Awake()
	{
		base.gameObject.hideFlags = HideFlags.HideAndDontSave;
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	public static void Setup()
	{
		Instance = new GameObject("MainThreadUtil").AddComponent<MainThreadUtil>();
		synchronizationContext = SynchronizationContext.Current;
	}

	public static void Run(IEnumerator waitForUpdate)
	{
		if (!Instance)
		{
			Debug.LogWarning("Attempting to run on main thread after shutdown.");
			throw new Exception("Attempting to run on main thread after shutdown.");
		}
		synchronizationContext.Post(delegate
		{
			Instance.StartCoroutine(waitForUpdate);
		}, null);
	}
}
