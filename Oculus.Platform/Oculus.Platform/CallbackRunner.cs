using System.Runtime.InteropServices;
using UnityEngine;

namespace Oculus.Platform;

public class CallbackRunner : MonoBehaviour
{
	public bool IsPersistantBetweenSceneLoads = true;

	[DllImport("LibOVRPlatformImpl64_1")]
	private static extern void ovr_UnityResetTestPlatform();

	private void Awake()
	{
		if (Object.FindObjectOfType<CallbackRunner>() != this)
		{
			Debug.LogWarning("You only need one instance of CallbackRunner");
		}
		if (IsPersistantBetweenSceneLoads)
		{
			Object.DontDestroyOnLoad(base.gameObject);
		}
	}

	private void Update()
	{
		Request.RunCallbacks();
	}

	private void OnDestroy()
	{
	}

	private void OnApplicationQuit()
	{
		Callback.OnApplicationQuit();
	}
}
