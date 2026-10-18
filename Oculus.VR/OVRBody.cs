using System;
using Meta.XR.Util;
using UnityEngine;

[HelpURL("https://developer.oculus.com/documentation/unity/move-body-tracking/")]
[Feature(Feature.BodyTracking)]
public class OVRBody : MonoBehaviour, OVRSkeleton.IOVRSkeletonDataProvider, OVRSkeletonRenderer.IOVRSkeletonRendererDataProvider
{
	private OVRPlugin.BodyState _bodyState;

	private OVRPlugin.Quatf[] _boneRotations;

	private OVRPlugin.Vector3f[] _boneTranslations;

	private bool _dataChangedSinceLastQuery;

	private bool _hasData;

	private const OVRPermissionsRequester.Permission BodyTrackingPermission = OVRPermissionsRequester.Permission.BodyTracking;

	private Action<string> _onPermissionGranted;

	[SerializeField]
	[Tooltip("The skeleton data type to be provided. Should be sync with OVRSkeleton. For selecting the tracking mode on the device, check settings in OVRManager.")]
	private OVRPlugin.BodyJointSet _providedSkeletonType;

	private static int _trackingInstanceCount;

	public OVRPlugin.BodyJointSet ProvidedSkeletonType
	{
		get
		{
			return _providedSkeletonType;
		}
		set
		{
			_providedSkeletonType = value;
		}
	}

	public OVRPlugin.BodyState? BodyState
	{
		get
		{
			if (!_hasData)
			{
				return null;
			}
			return _bodyState;
		}
	}

	public static OVRPlugin.BodyTrackingFidelity2 Fidelity
	{
		get
		{
			return OVRRuntimeSettings.GetRuntimeSettings().BodyTrackingFidelity;
		}
		set
		{
			OVRRuntimeSettings.GetRuntimeSettings().BodyTrackingFidelity = value;
			OVRPlugin.RequestBodyTrackingFidelity(value);
		}
	}

	private void Awake()
	{
		_onPermissionGranted = OnPermissionGranted;
	}

	private void OnEnable()
	{
		_dataChangedSinceLastQuery = false;
		_hasData = false;
		OVRManager oVRManager = UnityEngine.Object.FindAnyObjectByType<OVRManager>();
		if (oVRManager != null && oVRManager.SimultaneousHandsAndControllersEnabled)
		{
			Debug.LogWarning("Currently, Body API and simultaneous hands and controllers cannot be enabled at the same time", this);
			base.enabled = false;
			return;
		}
		if (_providedSkeletonType == OVRPlugin.BodyJointSet.FullBody && OVRRuntimeSettings.GetRuntimeSettings().BodyTrackingJointSet == OVRPlugin.BodyJointSet.UpperBody)
		{
			Debug.LogWarning("[OVRBody] Full body skeleton is used, but Full body tracking is disabled. Check settings in OVRManager.");
		}
		_trackingInstanceCount++;
		if (!StartBodyTracking())
		{
			base.enabled = false;
			return;
		}
		if (OVRPlugin.nativeXrApi == OVRPlugin.XrApi.OpenXR)
		{
			GetBodyState(OVRPlugin.Step.Render);
			return;
		}
		base.enabled = false;
		Debug.LogWarning("[OVRBody] Body tracking is only supported by OpenXR and is unavailable.");
	}

	private void OnPermissionGranted(string permissionId)
	{
		if (permissionId == OVRPermissionsRequester.GetPermissionId(OVRPermissionsRequester.Permission.BodyTracking))
		{
			OVRPermissionsRequester.PermissionGranted -= _onPermissionGranted;
			base.enabled = true;
		}
	}

	private static bool StartBodyTracking()
	{
		OVRPlugin.BodyJointSet bodyTrackingJointSet = OVRRuntimeSettings.GetRuntimeSettings().BodyTrackingJointSet;
		if (!OVRPlugin.StartBodyTracking2(bodyTrackingJointSet))
		{
			Debug.LogWarning(string.Format("[{0}] Failed to start body tracking with joint set {1}.", "OVRBody", bodyTrackingJointSet));
			return false;
		}
		OVRPlugin.BodyTrackingFidelity2 bodyTrackingFidelity = OVRRuntimeSettings.GetRuntimeSettings().BodyTrackingFidelity;
		if (!OVRPlugin.RequestBodyTrackingFidelity(bodyTrackingFidelity))
		{
			Debug.LogWarning(string.Format("[{0}] Failed to set Body Tracking fidelity to: {1}", "OVRBody", bodyTrackingFidelity));
		}
		return true;
	}

	private void OnDisable()
	{
		if (--_trackingInstanceCount == 0)
		{
			OVRPlugin.StopBodyTracking();
		}
	}

	private void OnDestroy()
	{
		OVRPermissionsRequester.PermissionGranted -= _onPermissionGranted;
	}

	private void Update()
	{
		GetBodyState(OVRPlugin.Step.Render);
	}

	public static bool SetRequestedJointSet(OVRPlugin.BodyJointSet jointSet)
	{
		OVRPlugin.BodyJointSet bodyTrackingJointSet = OVRRuntimeSettings.GetRuntimeSettings().BodyTrackingJointSet;
		if (jointSet != bodyTrackingJointSet)
		{
			OVRRuntimeSettings.GetRuntimeSettings().BodyTrackingJointSet = jointSet;
			if (_trackingInstanceCount > 0)
			{
				OVRPlugin.StopBodyTracking();
				return StartBodyTracking();
			}
		}
		return true;
	}

	public static bool SuggestBodyTrackingCalibrationOverride(float height)
	{
		OVRPlugin.BodyTrackingCalibrationInfo calibrationInfo = default(OVRPlugin.BodyTrackingCalibrationInfo);
		calibrationInfo.BodyHeight = height;
		return OVRPlugin.SuggestBodyTrackingCalibrationOverride(calibrationInfo);
	}

	public static bool ResetBodyTrackingCalibration()
	{
		return OVRPlugin.ResetBodyTrackingCalibration();
	}

	public OVRPlugin.BodyTrackingCalibrationState GetBodyTrackingCalibrationStatus()
	{
		if (!_hasData)
		{
			return OVRPlugin.BodyTrackingCalibrationState.Invalid;
		}
		return _bodyState.CalibrationStatus;
	}

	public OVRPlugin.BodyTrackingFidelity2 GetBodyTrackingFidelityStatus()
	{
		return _bodyState.Fidelity;
	}

	private void GetBodyState(OVRPlugin.Step step)
	{
		if (OVRPlugin.GetBodyState4(step, _providedSkeletonType, ref _bodyState))
		{
			_hasData = true;
			_dataChangedSinceLastQuery = true;
		}
		else
		{
			_hasData = false;
		}
	}

	OVRSkeleton.SkeletonType OVRSkeleton.IOVRSkeletonDataProvider.GetSkeletonType()
	{
		return _providedSkeletonType switch
		{
			OVRPlugin.BodyJointSet.UpperBody => OVRSkeleton.SkeletonType.Body, 
			OVRPlugin.BodyJointSet.FullBody => OVRSkeleton.SkeletonType.FullBody, 
			_ => OVRSkeleton.SkeletonType.None, 
		};
	}

	OVRSkeleton.SkeletonPoseData OVRSkeleton.IOVRSkeletonDataProvider.GetSkeletonPoseData()
	{
		if (!_hasData)
		{
			return default(OVRSkeleton.SkeletonPoseData);
		}
		if (_dataChangedSinceLastQuery)
		{
			Array.Resize(ref _boneRotations, _bodyState.JointLocations.Length);
			Array.Resize(ref _boneTranslations, _bodyState.JointLocations.Length);
			for (int i = 0; i < _bodyState.JointLocations.Length; i++)
			{
				OVRPlugin.BodyJointLocation bodyJointLocation = _bodyState.JointLocations[i];
				if (bodyJointLocation.OrientationValid)
				{
					_boneRotations[i] = bodyJointLocation.Pose.Orientation;
				}
				if (bodyJointLocation.PositionValid)
				{
					_boneTranslations[i] = bodyJointLocation.Pose.Position;
				}
			}
			_dataChangedSinceLastQuery = false;
		}
		OVRSkeleton.SkeletonPoseData result = default(OVRSkeleton.SkeletonPoseData);
		result.IsDataValid = true;
		result.IsDataHighConfidence = _bodyState.Confidence > 0.5f;
		result.RootPose = _bodyState.JointLocations[0].Pose;
		result.RootScale = 1f;
		result.BoneRotations = _boneRotations;
		result.BoneTranslations = _boneTranslations;
		result.SkeletonChangedCount = (int)_bodyState.SkeletonChangedCount;
		return result;
	}

	OVRSkeletonRenderer.SkeletonRendererData OVRSkeletonRenderer.IOVRSkeletonRendererDataProvider.GetSkeletonRendererData()
	{
		if (!_hasData)
		{
			return default(OVRSkeletonRenderer.SkeletonRendererData);
		}
		OVRSkeletonRenderer.SkeletonRendererData result = default(OVRSkeletonRenderer.SkeletonRendererData);
		result.RootScale = 1f;
		result.IsDataValid = true;
		result.IsDataHighConfidence = true;
		result.ShouldUseSystemGestureMaterial = false;
		return result;
	}

	bool OVRSkeleton.IOVRSkeletonDataProvider.get_enabled()
	{
		return base.enabled;
	}
}
