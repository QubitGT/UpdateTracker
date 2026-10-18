using System;
using Meta.XR.Util;
using UnityEngine;

[HelpURL("https://developer.oculus.com/documentation/unity/move-eye-tracking/")]
[Feature(Feature.EyeTracking)]
public class OVREyeGaze : MonoBehaviour
{
	public enum EyeId
	{
		Left,
		Right
	}

	public enum EyeTrackingMode
	{
		HeadSpace,
		WorldSpace,
		TrackingSpace
	}

	public EyeId Eye;

	[Range(0f, 1f)]
	public float ConfidenceThreshold = 0.5f;

	public bool ApplyPosition = true;

	public bool ApplyRotation = true;

	private OVRPlugin.EyeGazesState _currentEyeGazesState;

	[Tooltip("Reference frame for eye. Reference frame should be set in the forward direction of the eye. It is there to calculate the initial offset of the eye GameObject. If it's null, then world reference frame will be used.")]
	public Transform ReferenceFrame;

	[Tooltip("HeadSpace: Tracking mode will convert the eye pose from tracking space to local space which is relative to the VR camera rig. For example, we can use this setting to correctly show the eye movement of a character which is facing in another direction than the source.\nWorldSpace: Tracking mode will convert the eye pose from tracking space to world space.\nTrackingSpace: Track eye is relative to OVRCameraRig. This is raw pose information from VR tracking space.")]
	public EyeTrackingMode TrackingMode;

	private Quaternion _initialRotationOffset;

	private Transform _viewTransform;

	private const OVRPermissionsRequester.Permission EyeTrackingPermission = OVRPermissionsRequester.Permission.EyeTracking;

	private Action<string> _onPermissionGranted;

	private static int _trackingInstanceCount;

	public bool EyeTrackingEnabled => OVRPlugin.eyeTrackingEnabled;

	public float Confidence { get; private set; }

	private void Awake()
	{
		_onPermissionGranted = OnPermissionGranted;
	}

	private void Start()
	{
		PrepareHeadDirection();
	}

	private void OnEnable()
	{
		_trackingInstanceCount++;
		if (!StartEyeTracking())
		{
			base.enabled = false;
		}
	}

	private void OnPermissionGranted(string permissionId)
	{
		if (permissionId == OVRPermissionsRequester.GetPermissionId(OVRPermissionsRequester.Permission.EyeTracking))
		{
			OVRPermissionsRequester.PermissionGranted -= _onPermissionGranted;
			base.enabled = true;
		}
	}

	private bool StartEyeTracking()
	{
		if (!OVRPermissionsRequester.IsPermissionGranted(OVRPermissionsRequester.Permission.EyeTracking))
		{
			OVRPermissionsRequester.PermissionGranted -= _onPermissionGranted;
			OVRPermissionsRequester.PermissionGranted += _onPermissionGranted;
			return false;
		}
		if (!OVRPlugin.StartEyeTracking())
		{
			Debug.LogWarning("[OVREyeGaze] Failed to start eye tracking.");
			return false;
		}
		return true;
	}

	private void OnDisable()
	{
		if (--_trackingInstanceCount == 0)
		{
			OVRPlugin.StopEyeTracking();
		}
	}

	private void OnDestroy()
	{
		OVRPermissionsRequester.PermissionGranted -= _onPermissionGranted;
	}

	private void Update()
	{
		if (!OVRPlugin.GetEyeGazesState(OVRPlugin.Step.Render, -1, ref _currentEyeGazesState))
		{
			return;
		}
		OVRPlugin.EyeGazeState eyeGazeState = _currentEyeGazesState.EyeGazes[(int)Eye];
		if (!eyeGazeState.IsValid)
		{
			return;
		}
		Confidence = eyeGazeState.Confidence;
		if (!(Confidence < ConfidenceThreshold))
		{
			OVRPose trackingSpacePose = eyeGazeState.Pose.ToOVRPose();
			switch (TrackingMode)
			{
			case EyeTrackingMode.HeadSpace:
				trackingSpacePose = trackingSpacePose.ToHeadSpacePose();
				break;
			case EyeTrackingMode.WorldSpace:
				trackingSpacePose = trackingSpacePose.ToWorldSpacePose(Camera.main);
				break;
			}
			if (ApplyPosition)
			{
				base.transform.position = trackingSpacePose.position;
			}
			if (ApplyRotation)
			{
				base.transform.rotation = CalculateEyeRotation(trackingSpacePose.orientation);
			}
		}
	}

	private Quaternion CalculateEyeRotation(Quaternion eyeRotation)
	{
		return Quaternion.LookRotation(_viewTransform.rotation * eyeRotation * Vector3.forward, _viewTransform.up) * _initialRotationOffset;
	}

	private void PrepareHeadDirection()
	{
		string text = "HeadLookAtDirection";
		_viewTransform = new GameObject(text).transform;
		if ((bool)ReferenceFrame)
		{
			_viewTransform.SetPositionAndRotation(ReferenceFrame.position, ReferenceFrame.rotation);
		}
		else
		{
			_viewTransform.SetPositionAndRotation(base.transform.position, Quaternion.identity);
		}
		_viewTransform.parent = base.transform.parent;
		_initialRotationOffset = Quaternion.Inverse(_viewTransform.rotation) * base.transform.rotation;
	}
}
