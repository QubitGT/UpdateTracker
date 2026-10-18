using UnityEngine;

namespace Valve.VR.Extras;

public class SteamVR_GazeTracker : MonoBehaviour
{
	public bool isInGaze;

	public float gazeInCutoff = 0.15f;

	public float gazeOutCutoff = 0.4f;

	protected Transform hmdTrackedObject;

	public event GazeEventHandler GazeOn;

	public event GazeEventHandler GazeOff;

	public virtual void OnGazeOn(GazeEventArgs gazeEventArgs)
	{
		if (this.GazeOn != null)
		{
			this.GazeOn(this, gazeEventArgs);
		}
	}

	public virtual void OnGazeOff(GazeEventArgs gazeEventArgs)
	{
		if (this.GazeOff != null)
		{
			this.GazeOff(this, gazeEventArgs);
		}
	}

	protected virtual void Update()
	{
		if (hmdTrackedObject == null)
		{
			SteamVR_TrackedObject[] array = Object.FindObjectsByType<SteamVR_TrackedObject>(FindObjectsSortMode.None);
			foreach (SteamVR_TrackedObject steamVR_TrackedObject in array)
			{
				if (steamVR_TrackedObject.index == SteamVR_TrackedObject.EIndex.Hmd)
				{
					hmdTrackedObject = steamVR_TrackedObject.transform;
					break;
				}
			}
		}
		if (!hmdTrackedObject)
		{
			return;
		}
		Ray ray = new Ray(hmdTrackedObject.position, hmdTrackedObject.forward);
		Plane plane = new Plane(hmdTrackedObject.forward, base.transform.position);
		float enter = 0f;
		if (plane.Raycast(ray, out enter))
		{
			float num = Vector3.Distance(hmdTrackedObject.position + hmdTrackedObject.forward * enter, base.transform.position);
			if (num < gazeInCutoff && !isInGaze)
			{
				isInGaze = true;
				GazeEventArgs gazeEventArgs = default(GazeEventArgs);
				gazeEventArgs.distance = num;
				OnGazeOn(gazeEventArgs);
			}
			else if (num >= gazeOutCutoff && isInGaze)
			{
				isInGaze = false;
				GazeEventArgs gazeEventArgs2 = default(GazeEventArgs);
				gazeEventArgs2.distance = num;
				OnGazeOff(gazeEventArgs2);
			}
		}
	}
}
