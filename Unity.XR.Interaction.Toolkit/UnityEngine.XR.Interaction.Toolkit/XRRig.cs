using System;
using Unity.XR.CoreUtils;

namespace UnityEngine.XR.Interaction.Toolkit;

[AddComponentMenu("")]
[DisallowMultipleComponent]
[Obsolete("XRRig has been deprecated. Use the XROrigin component instead.", true)]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.XRRig.html")]
public class XRRig : XROrigin
{
	private const string k_ObsoleteMessage = "XRRig has been deprecated. Use the XROrigin component instead.";

	[SerializeField]
	private GameObject m_CameraGameObject;

	public GameObject rig
	{
		get
		{
			return null;
		}
		set
		{
		}
	}

	public GameObject cameraGameObject
	{
		get
		{
			return null;
		}
		set
		{
		}
	}

	public GameObject cameraFloorOffsetObject
	{
		get
		{
			return null;
		}
		set
		{
		}
	}

	public TrackingOriginMode requestedTrackingOriginMode
	{
		get
		{
			return TrackingOriginMode.NotSpecified;
		}
		set
		{
		}
	}

	public float cameraYOffset
	{
		get
		{
			return 0f;
		}
		set
		{
		}
	}

	public TrackingOriginModeFlags currentTrackingOriginMode => TrackingOriginModeFlags.Unknown;

	public Vector3 rigInCameraSpacePos => default(Vector3);

	public Vector3 cameraInRigSpacePos => default(Vector3);

	public float cameraInRigSpaceHeight => 0f;

	protected new void Awake()
	{
		Debug.LogError("XRRig has been deprecated. Use the XROrigin component instead.", this);
		throw new NotSupportedException("XRRig has been deprecated. Use the XROrigin component instead.");
	}

	public bool RotateAroundCameraUsingRigUp(float angleDegrees)
	{
		return false;
	}

	public bool MatchRigUp(Vector3 destinationUp)
	{
		return false;
	}

	public bool MatchRigUpCameraForward(Vector3 destinationUp, Vector3 destinationForward)
	{
		return false;
	}

	public bool MatchRigUpRigForward(Vector3 destinationUp, Vector3 destinationForward)
	{
		return false;
	}
}
