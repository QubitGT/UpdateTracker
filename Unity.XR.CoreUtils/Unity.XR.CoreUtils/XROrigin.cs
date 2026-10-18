using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.Serialization;
using UnityEngine.SpatialTracking;
using UnityEngine.XR;

namespace Unity.XR.CoreUtils;

[AddComponentMenu("XR/XR Origin")]
[DisallowMultipleComponent]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.core-utils@2.0/api/Unity.XR.CoreUtils.XROrigin.html")]
public class XROrigin : MonoBehaviour
{
	public enum TrackingOriginMode
	{
		NotSpecified,
		Device,
		Floor,
		Unbounded
	}

	[SerializeField]
	[Tooltip("The Camera to associate with the XR device.")]
	private Camera m_Camera;

	private const float k_DefaultCameraYOffset = 1.1176f;

	[SerializeField]
	[FormerlySerializedAs("m_RigBaseGameObject")]
	private GameObject m_OriginBaseGameObject;

	[SerializeField]
	private GameObject m_CameraFloorOffsetObject;

	[SerializeField]
	private TrackingOriginMode m_RequestedTrackingOriginMode;

	[SerializeField]
	private float m_CameraYOffset = 1.1176f;

	private static readonly List<XRInputSubsystem> s_InputSubsystems = new List<XRInputSubsystem>();

	private bool m_CameraInitialized;

	private bool m_CameraInitializing;

	public Camera Camera
	{
		get
		{
			return m_Camera;
		}
		set
		{
			m_Camera = value;
		}
	}

	public Transform TrackablesParent { get; private set; }

	public GameObject Origin
	{
		get
		{
			return m_OriginBaseGameObject;
		}
		set
		{
			m_OriginBaseGameObject = value;
		}
	}

	public GameObject CameraFloorOffsetObject
	{
		get
		{
			return m_CameraFloorOffsetObject;
		}
		set
		{
			m_CameraFloorOffsetObject = value;
			MoveOffsetHeight();
		}
	}

	public TrackingOriginMode RequestedTrackingOriginMode
	{
		get
		{
			return m_RequestedTrackingOriginMode;
		}
		set
		{
			m_RequestedTrackingOriginMode = value;
			TryInitializeCamera();
		}
	}

	public float CameraYOffset
	{
		get
		{
			return m_CameraYOffset;
		}
		set
		{
			m_CameraYOffset = value;
			MoveOffsetHeight();
		}
	}

	public TrackingOriginModeFlags CurrentTrackingOriginMode { get; private set; }

	public Vector3 OriginInCameraSpacePos => m_Camera.transform.InverseTransformPoint(m_OriginBaseGameObject.transform.position);

	public Vector3 CameraInOriginSpacePos => m_OriginBaseGameObject.transform.InverseTransformPoint(m_Camera.transform.position);

	public float CameraInOriginSpaceHeight => CameraInOriginSpacePos.y;

	public event Action<ARTrackablesParentTransformChangedEventArgs> TrackablesParentTransformChanged;

	private void MoveOffsetHeight()
	{
		if (Application.isPlaying)
		{
			switch (CurrentTrackingOriginMode)
			{
			case TrackingOriginModeFlags.Floor:
				MoveOffsetHeight(0f);
				break;
			case TrackingOriginModeFlags.Device:
			case TrackingOriginModeFlags.Unbounded:
				MoveOffsetHeight(m_CameraYOffset);
				break;
			}
		}
	}

	private void MoveOffsetHeight(float y)
	{
		if (m_CameraFloorOffsetObject != null)
		{
			Transform obj = m_CameraFloorOffsetObject.transform;
			Vector3 localPosition = obj.localPosition;
			localPosition.y = y;
			obj.localPosition = localPosition;
		}
	}

	private void TryInitializeCamera()
	{
		if (Application.isPlaying)
		{
			m_CameraInitialized = SetupCamera();
			if (!m_CameraInitialized & !m_CameraInitializing)
			{
				StartCoroutine(RepeatInitializeCamera());
			}
		}
	}

	private bool SetupCamera()
	{
		bool result = true;
		SubsystemManager.GetSubsystems(s_InputSubsystems);
		if (s_InputSubsystems.Count > 0)
		{
			foreach (XRInputSubsystem s_InputSubsystem in s_InputSubsystems)
			{
				if (SetupCamera(s_InputSubsystem))
				{
					s_InputSubsystem.trackingOriginUpdated -= OnInputSubsystemTrackingOriginUpdated;
					s_InputSubsystem.trackingOriginUpdated += OnInputSubsystemTrackingOriginUpdated;
				}
				else
				{
					result = false;
				}
			}
		}
		return result;
	}

	private bool SetupCamera(XRInputSubsystem inputSubsystem)
	{
		if (inputSubsystem == null)
		{
			return false;
		}
		bool flag = true;
		switch (m_RequestedTrackingOriginMode)
		{
		case TrackingOriginMode.NotSpecified:
			CurrentTrackingOriginMode = inputSubsystem.GetTrackingOriginMode();
			break;
		case TrackingOriginMode.Device:
		case TrackingOriginMode.Floor:
		case TrackingOriginMode.Unbounded:
		{
			TrackingOriginModeFlags supportedTrackingOriginModes = inputSubsystem.GetSupportedTrackingOriginModes();
			if (supportedTrackingOriginModes == TrackingOriginModeFlags.Unknown)
			{
				return false;
			}
			TrackingOriginModeFlags trackingOriginModeFlags = ConvertTrackingOriginModeToFlag(m_RequestedTrackingOriginMode);
			if ((supportedTrackingOriginModes & trackingOriginModeFlags) == 0)
			{
				m_RequestedTrackingOriginMode = TrackingOriginMode.NotSpecified;
				CurrentTrackingOriginMode = inputSubsystem.GetTrackingOriginMode();
				Debug.LogWarning($"Attempting to set the tracking origin mode to {trackingOriginModeFlags}, but that is not supported by the SDK." + $" Supported types: {supportedTrackingOriginModes:F}. Using the current mode of {CurrentTrackingOriginMode} instead.", this);
			}
			else
			{
				flag = inputSubsystem.TrySetTrackingOriginMode(trackingOriginModeFlags);
			}
			break;
		}
		default:
			Debug.LogError(string.Format("Unhandled {0}={1}", "TrackingOriginMode", m_RequestedTrackingOriginMode));
			return false;
		}
		if (flag)
		{
			MoveOffsetHeight();
		}
		if (CurrentTrackingOriginMode == TrackingOriginModeFlags.Device || m_RequestedTrackingOriginMode == TrackingOriginMode.Device || CurrentTrackingOriginMode == TrackingOriginModeFlags.Unbounded || m_RequestedTrackingOriginMode == TrackingOriginMode.Unbounded)
		{
			flag = inputSubsystem.TryRecenter();
		}
		return flag;
	}

	private void OnInputSubsystemTrackingOriginUpdated(XRInputSubsystem inputSubsystem)
	{
		CurrentTrackingOriginMode = inputSubsystem.GetTrackingOriginMode();
		MoveOffsetHeight();
	}

	private IEnumerator RepeatInitializeCamera()
	{
		m_CameraInitializing = true;
		while (!m_CameraInitialized)
		{
			yield return null;
			if (!m_CameraInitialized)
			{
				m_CameraInitialized = SetupCamera();
			}
		}
		m_CameraInitializing = false;
	}

	public bool RotateAroundCameraUsingOriginUp(float angleDegrees)
	{
		return RotateAroundCameraPosition(m_OriginBaseGameObject.transform.up, angleDegrees);
	}

	public bool RotateAroundCameraPosition(Vector3 vector, float angleDegrees)
	{
		if (m_Camera == null || m_OriginBaseGameObject == null)
		{
			return false;
		}
		m_OriginBaseGameObject.transform.RotateAround(m_Camera.transform.position, vector, angleDegrees);
		return true;
	}

	public bool MatchOriginUp(Vector3 destinationUp)
	{
		if (m_OriginBaseGameObject == null)
		{
			return false;
		}
		if (m_OriginBaseGameObject.transform.up == destinationUp)
		{
			return true;
		}
		Quaternion quaternion = Quaternion.FromToRotation(m_OriginBaseGameObject.transform.up, destinationUp);
		m_OriginBaseGameObject.transform.rotation = quaternion * base.transform.rotation;
		return true;
	}

	public bool MatchOriginUpCameraForward(Vector3 destinationUp, Vector3 destinationForward)
	{
		if (m_Camera != null && MatchOriginUp(destinationUp))
		{
			float angleDegrees = Vector3.SignedAngle(Vector3.ProjectOnPlane(m_Camera.transform.forward, destinationUp).normalized, destinationForward, destinationUp);
			RotateAroundCameraPosition(destinationUp, angleDegrees);
			return true;
		}
		return false;
	}

	public bool MatchOriginUpOriginForward(Vector3 destinationUp, Vector3 destinationForward)
	{
		if (m_OriginBaseGameObject != null && MatchOriginUp(destinationUp))
		{
			float angleDegrees = Vector3.SignedAngle(m_OriginBaseGameObject.transform.forward, destinationForward, destinationUp);
			RotateAroundCameraPosition(destinationUp, angleDegrees);
			return true;
		}
		return false;
	}

	public bool MoveCameraToWorldLocation(Vector3 desiredWorldLocation)
	{
		if (m_Camera == null)
		{
			return false;
		}
		Vector3 vector = Matrix4x4.Rotate(m_Camera.transform.rotation).MultiplyPoint3x4(OriginInCameraSpacePos);
		m_OriginBaseGameObject.transform.position = vector + desiredWorldLocation;
		return true;
	}

	protected void Awake()
	{
		if (m_CameraFloorOffsetObject == null)
		{
			Debug.LogWarning("No Camera Floor Offset GameObject specified for XR Origin, using attached GameObject.", this);
			m_CameraFloorOffsetObject = base.gameObject;
		}
		if (m_Camera == null)
		{
			Camera main = Camera.main;
			if (main != null)
			{
				m_Camera = main;
			}
			else
			{
				Debug.LogWarning("No Main Camera is found for XR Origin, please assign the Camera field manually.", this);
			}
		}
		TrackablesParent = new GameObject("Trackables").transform;
		TrackablesParent.SetParent(base.transform, worldPositionStays: false);
		TrackablesParent.SetLocalPose(Pose.identity);
		TrackablesParent.localScale = Vector3.one;
		if ((bool)m_Camera)
		{
			UnityEngine.InputSystem.XR.TrackedPoseDriver component = m_Camera.GetComponent<UnityEngine.InputSystem.XR.TrackedPoseDriver>();
			UnityEngine.SpatialTracking.TrackedPoseDriver component2 = m_Camera.GetComponent<UnityEngine.SpatialTracking.TrackedPoseDriver>();
			if (component == null && component2 == null)
			{
				Debug.LogWarning("Camera \"" + m_Camera.name + "\" does not use a Tracked Pose Driver (Input System), so its transform will not be updated by an XR device.  In order for this to be updated, please add a Tracked Pose Driver (Input System) with bindings for position and rotation of the center eye.", this);
			}
		}
	}

	private Pose GetCameraOriginPose()
	{
		Pose identity = Pose.identity;
		Transform parent = m_Camera.transform.parent;
		if (!parent)
		{
			return identity;
		}
		return parent.TransformPose(identity);
	}

	protected void OnEnable()
	{
		Application.onBeforeRender += OnBeforeRender;
	}

	protected void OnDisable()
	{
		Application.onBeforeRender -= OnBeforeRender;
	}

	private void OnBeforeRender()
	{
		if ((bool)m_Camera)
		{
			Pose cameraOriginPose = GetCameraOriginPose();
			TrackablesParent.position = cameraOriginPose.position;
			TrackablesParent.rotation = cameraOriginPose.rotation;
		}
		if (TrackablesParent.hasChanged)
		{
			this.TrackablesParentTransformChanged?.Invoke(new ARTrackablesParentTransformChangedEventArgs(this, TrackablesParent));
			TrackablesParent.hasChanged = false;
		}
	}

	protected void OnValidate()
	{
		if (m_OriginBaseGameObject == null)
		{
			m_OriginBaseGameObject = base.gameObject;
		}
		if (Application.isPlaying && base.isActiveAndEnabled)
		{
			if (IsModeStale())
			{
				TryInitializeCamera();
			}
			else
			{
				MoveOffsetHeight();
			}
		}
		bool IsModeStale()
		{
			if (s_InputSubsystems.Count > 0)
			{
				foreach (XRInputSubsystem s_InputSubsystem in s_InputSubsystems)
				{
					TrackingOriginModeFlags trackingOriginModeFlags = ConvertTrackingOriginModeToFlag(m_RequestedTrackingOriginMode);
					if (trackingOriginModeFlags == TrackingOriginModeFlags.Unknown)
					{
						return false;
					}
					if (s_InputSubsystem != null && s_InputSubsystem.GetTrackingOriginMode() != trackingOriginModeFlags)
					{
						return true;
					}
				}
			}
			return false;
		}
	}

	private static TrackingOriginModeFlags ConvertTrackingOriginModeToFlag(TrackingOriginMode mode)
	{
		return mode switch
		{
			TrackingOriginMode.NotSpecified => TrackingOriginModeFlags.Unknown, 
			TrackingOriginMode.Device => TrackingOriginModeFlags.Device, 
			TrackingOriginMode.Floor => TrackingOriginModeFlags.Floor, 
			TrackingOriginMode.Unbounded => TrackingOriginModeFlags.Unbounded, 
			_ => TrackingOriginModeFlags.Unknown, 
		};
	}

	protected void Start()
	{
		TryInitializeCamera();
	}

	protected void OnDestroy()
	{
		foreach (XRInputSubsystem s_InputSubsystem in s_InputSubsystems)
		{
			if (s_InputSubsystem != null)
			{
				s_InputSubsystem.trackingOriginUpdated -= OnInputSubsystemTrackingOriginUpdated;
			}
		}
	}
}
