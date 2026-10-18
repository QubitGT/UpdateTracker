using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class OVRExternalComposition : OVRComposition
{
	private GameObject previousMainCameraObject;

	public GameObject foregroundCameraGameObject;

	public Camera foregroundCamera;

	public GameObject backgroundCameraGameObject;

	public Camera backgroundCamera;

	private readonly object audioDataLock = new object();

	private List<float> cachedAudioData = new List<float>(16384);

	private int cachedChannels;

	public override OVRManager.CompositionMethod CompositionMethod()
	{
		return OVRManager.CompositionMethod.External;
	}

	public OVRExternalComposition(GameObject parentObject, Camera mainCamera, OVRMixedRealityCaptureConfiguration configuration)
		: base(parentObject, mainCamera, configuration)
	{
		RefreshCameraObjects(parentObject, mainCamera, configuration);
	}

	private void RefreshCameraObjects(GameObject parentObject, Camera mainCamera, OVRMixedRealityCaptureConfiguration configuration)
	{
		if (mainCamera.gameObject != previousMainCameraObject)
		{
			Debug.LogFormat("[OVRExternalComposition] Camera refreshed. Rebind camera to {0}", mainCamera.gameObject.name);
			OVRCompositionUtil.SafeDestroy(ref backgroundCameraGameObject);
			backgroundCamera = null;
			OVRCompositionUtil.SafeDestroy(ref foregroundCameraGameObject);
			foregroundCamera = null;
			RefreshCameraRig(parentObject, mainCamera);
			if (configuration.instantiateMixedRealityCameraGameObject != null)
			{
				backgroundCameraGameObject = configuration.instantiateMixedRealityCameraGameObject(mainCamera.gameObject, OVRManager.MrcCameraType.Background);
			}
			else
			{
				backgroundCameraGameObject = Object.Instantiate(mainCamera.gameObject);
			}
			backgroundCameraGameObject.name = "OculusMRC_BackgroundCamera";
			backgroundCameraGameObject.transform.parent = (cameraInTrackingSpace ? cameraRig.trackingSpace : parentObject.transform);
			if ((bool)backgroundCameraGameObject.GetComponent<AudioListener>())
			{
				Object.Destroy(backgroundCameraGameObject.GetComponent<AudioListener>());
			}
			if ((bool)backgroundCameraGameObject.GetComponent<OVRManager>())
			{
				Object.Destroy(backgroundCameraGameObject.GetComponent<OVRManager>());
			}
			backgroundCamera = backgroundCameraGameObject.GetComponent<Camera>();
			backgroundCamera.tag = "Untagged";
			UniversalAdditionalCameraData universalAdditionalCameraData = backgroundCamera.GetUniversalAdditionalCameraData();
			if (universalAdditionalCameraData != null)
			{
				universalAdditionalCameraData.allowXRRendering = false;
			}
			backgroundCamera.depth = 99990f;
			backgroundCamera.rect = new Rect(0f, 0f, 0.5f, 1f);
			backgroundCamera.cullingMask = (backgroundCamera.cullingMask & ~(int)configuration.extraHiddenLayers) | (int)configuration.extraVisibleLayers;
			if (configuration.instantiateMixedRealityCameraGameObject != null)
			{
				foregroundCameraGameObject = configuration.instantiateMixedRealityCameraGameObject(mainCamera.gameObject, OVRManager.MrcCameraType.Foreground);
			}
			else
			{
				foregroundCameraGameObject = Object.Instantiate(mainCamera.gameObject);
			}
			foregroundCameraGameObject.name = "OculusMRC_ForgroundCamera";
			foregroundCameraGameObject.transform.parent = (cameraInTrackingSpace ? cameraRig.trackingSpace : parentObject.transform);
			if ((bool)foregroundCameraGameObject.GetComponent<AudioListener>())
			{
				Object.Destroy(foregroundCameraGameObject.GetComponent<AudioListener>());
			}
			if ((bool)foregroundCameraGameObject.GetComponent<OVRManager>())
			{
				Object.Destroy(foregroundCameraGameObject.GetComponent<OVRManager>());
			}
			foregroundCamera = foregroundCameraGameObject.GetComponent<Camera>();
			foregroundCamera.tag = "Untagged";
			UniversalAdditionalCameraData universalAdditionalCameraData2 = foregroundCamera.GetUniversalAdditionalCameraData();
			if (universalAdditionalCameraData2 != null)
			{
				universalAdditionalCameraData2.allowXRRendering = false;
			}
			foregroundCamera.depth = backgroundCamera.depth + 1f;
			foregroundCamera.rect = new Rect(0.5f, 0f, 0.5f, 1f);
			foregroundCamera.clearFlags = CameraClearFlags.Color;
			foregroundCamera.backgroundColor = configuration.externalCompositionBackdropColorRift;
			foregroundCamera.cullingMask = (foregroundCamera.cullingMask & ~(int)configuration.extraHiddenLayers) | (int)configuration.extraVisibleLayers;
			previousMainCameraObject = mainCamera.gameObject;
		}
	}

	public override void Update(GameObject gameObject, Camera mainCamera, OVRMixedRealityCaptureConfiguration configuration, OVRManager.TrackingOrigin trackingOrigin)
	{
		RefreshCameraObjects(gameObject, mainCamera, configuration);
		OVRPlugin.SetHandNodePoseStateLatency(0.0);
		OVRPose oVRPose = OVRPlugin.GetTrackingTransformRelativePose(OVRPlugin.TrackingOrigin.Stage).ToOVRPose().Inverse();
		OVRPose oVRPose2 = oVRPose * OVRPlugin.GetNodePose(OVRPlugin.Node.Head, OVRPlugin.Step.Render).ToOVRPose();
		OVRPose oVRPose3 = oVRPose * OVRPlugin.GetNodePose(OVRPlugin.Node.HandLeft, OVRPlugin.Step.Render).ToOVRPose();
		OVRPose oVRPose4 = oVRPose * OVRPlugin.GetNodePose(OVRPlugin.Node.HandRight, OVRPlugin.Step.Render).ToOVRPose();
		OVRPlugin.Media.SetMrcHeadsetControllerPose(oVRPose2.ToPosef(), oVRPose3.ToPosef(), oVRPose4.ToPosef());
		backgroundCamera.clearFlags = mainCamera.clearFlags;
		backgroundCamera.backgroundColor = mainCamera.backgroundColor;
		if (configuration.dynamicCullingMask)
		{
			backgroundCamera.cullingMask = (mainCamera.cullingMask & ~(int)configuration.extraHiddenLayers) | (int)configuration.extraVisibleLayers;
		}
		backgroundCamera.nearClipPlane = mainCamera.nearClipPlane;
		backgroundCamera.farClipPlane = mainCamera.farClipPlane;
		if (configuration.dynamicCullingMask)
		{
			foregroundCamera.cullingMask = (mainCamera.cullingMask & ~(int)configuration.extraHiddenLayers) | (int)configuration.extraVisibleLayers;
		}
		foregroundCamera.nearClipPlane = mainCamera.nearClipPlane;
		foregroundCamera.farClipPlane = mainCamera.farClipPlane;
		if (OVRMixedReality.useFakeExternalCamera || OVRPlugin.GetExternalCameraCount() == 0)
		{
			OVRPose oVRPose5 = default(OVRPose);
			OVRPose oVRPose6 = default(OVRPose);
			oVRPose6.position = ((trackingOrigin == OVRManager.TrackingOrigin.EyeLevel) ? OVRMixedReality.fakeCameraEyeLevelPosition : OVRMixedReality.fakeCameraFloorLevelPosition);
			oVRPose6.orientation = OVRMixedReality.fakeCameraRotation;
			oVRPose5 = oVRPose6.ToWorldSpacePose(mainCamera);
			backgroundCamera.fieldOfView = OVRMixedReality.fakeCameraFov;
			backgroundCamera.aspect = OVRMixedReality.fakeCameraAspect;
			foregroundCamera.fieldOfView = OVRMixedReality.fakeCameraFov;
			foregroundCamera.aspect = OVRMixedReality.fakeCameraAspect;
			if (cameraInTrackingSpace)
			{
				backgroundCamera.transform.FromOVRPose(oVRPose6, isLocal: true);
				foregroundCamera.transform.FromOVRPose(oVRPose6, isLocal: true);
			}
			else
			{
				backgroundCamera.transform.FromOVRPose(oVRPose5);
				foregroundCamera.transform.FromOVRPose(oVRPose5);
			}
		}
		else
		{
			if (!OVRPlugin.GetMixedRealityCameraInfo(0, out var cameraExtrinsics, out var cameraIntrinsics))
			{
				Debug.LogError("Failed to get external camera information");
				return;
			}
			float fieldOfView = Mathf.Atan(cameraIntrinsics.FOVPort.UpTan) * 57.29578f * 2f;
			float aspect = cameraIntrinsics.FOVPort.LeftTan / cameraIntrinsics.FOVPort.UpTan;
			backgroundCamera.fieldOfView = fieldOfView;
			backgroundCamera.aspect = aspect;
			foregroundCamera.fieldOfView = fieldOfView;
			foregroundCamera.aspect = cameraIntrinsics.FOVPort.LeftTan / cameraIntrinsics.FOVPort.UpTan;
			if (cameraInTrackingSpace)
			{
				OVRPose pose = ComputeCameraTrackingSpacePose(cameraExtrinsics);
				backgroundCamera.transform.FromOVRPose(pose, isLocal: true);
				foregroundCamera.transform.FromOVRPose(pose, isLocal: true);
			}
			else
			{
				OVRPose pose2 = ComputeCameraWorldSpacePose(cameraExtrinsics, mainCamera);
				backgroundCamera.transform.FromOVRPose(pose2);
				foregroundCamera.transform.FromOVRPose(pose2);
			}
		}
		float b = Vector3.Dot(mainCamera.transform.position - foregroundCamera.transform.position, foregroundCamera.transform.forward);
		foregroundCamera.farClipPlane = Mathf.Max(foregroundCamera.nearClipPlane + 0.001f, b);
	}

	public override void Cleanup()
	{
		OVRCompositionUtil.SafeDestroy(ref backgroundCameraGameObject);
		backgroundCamera = null;
		OVRCompositionUtil.SafeDestroy(ref foregroundCameraGameObject);
		foregroundCamera = null;
		Debug.Log("ExternalComposition deactivated");
	}

	public void CacheAudioData(float[] data, int channels)
	{
		lock (audioDataLock)
		{
			if (channels != cachedChannels)
			{
				cachedAudioData.Clear();
			}
			cachedChannels = channels;
			cachedAudioData.AddRange(data);
		}
	}

	public void GetAndResetAudioData(ref float[] audioData, out int audioFrames, out int channels)
	{
		lock (audioDataLock)
		{
			if (audioData == null || audioData.Length < cachedAudioData.Count)
			{
				audioData = new float[cachedAudioData.Capacity];
			}
			cachedAudioData.CopyTo(audioData);
			audioFrames = cachedAudioData.Count;
			channels = cachedChannels;
			cachedAudioData.Clear();
		}
	}
}
