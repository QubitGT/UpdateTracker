using UnityEngine;
using UnityEngine.SpatialTracking;

namespace Valve.VR;

public class SteamVR_CameraHelper : MonoBehaviour
{
	private void Start()
	{
		if (base.gameObject.GetComponent<TrackedPoseDriver>() == null)
		{
			base.gameObject.AddComponent<TrackedPoseDriver>();
		}
	}
}
