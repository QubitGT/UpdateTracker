using UnityEngine;

namespace Oculus.Interaction.Samples;

public class EnableOVRConcurrentHandsAndControllers : MonoBehaviour
{
	private void OnEnable()
	{
		if (OVRPlugin.SetSimultaneousHandsAndControllersEnabled(enabled: true))
		{
			Debug.Log("Concurrent hands and controllers mode succesfully set.");
		}
		else
		{
			Debug.LogWarning("Concurrent Hands and controllers not supported.");
		}
	}

	private void OnDisable()
	{
		if (OVRPlugin.SetSimultaneousHandsAndControllersEnabled(enabled: false))
		{
			Debug.Log("Concurrent hands and controllers mode succesfully unset.");
		}
		else
		{
			Debug.LogWarning("Concurrent Hands and controllers not supported.");
		}
	}
}
