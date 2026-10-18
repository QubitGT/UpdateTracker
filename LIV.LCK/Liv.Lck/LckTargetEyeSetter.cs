using UnityEngine;

namespace Liv.Lck;

[RequireComponent(typeof(Camera))]
public class LckTargetEyeSetter : MonoBehaviour
{
	private void OnValidate()
	{
		GetComponent<Camera>().stereoTargetEye = StereoTargetEyeMask.None;
	}
}
