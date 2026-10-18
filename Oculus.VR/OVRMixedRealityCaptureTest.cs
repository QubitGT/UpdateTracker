using System;
using UnityEngine;

public class OVRMixedRealityCaptureTest : MonoBehaviour
{
	private enum CameraMode
	{
		Normal,
		OverrideFov,
		ThirdPerson
	}

	private bool inited;

	private CameraMode currentMode;

	public Camera defaultExternalCamera;

	private OVRPlugin.Fovf defaultFov;

	private void Start()
	{
		if (!defaultExternalCamera)
		{
			Debug.LogWarning("defaultExternalCamera undefined");
		}
		if (!OVRManager.instance.enableMixedReality)
		{
			OVRManager.instance.enableMixedReality = true;
		}
	}

	private void Initialize()
	{
		if (!inited && OVRPlugin.IsMixedRealityInitialized())
		{
			OVRPlugin.ResetDefaultExternalCamera();
			Debug.LogFormat("GetExternalCameraCount before adding manual external camera {0}", OVRPlugin.GetExternalCameraCount());
			UpdateDefaultExternalCamera();
			Debug.LogFormat("GetExternalCameraCount after adding manual external camera {0}", OVRPlugin.GetExternalCameraCount());
			OVRPlugin.GetMixedRealityCameraInfo(0, out var _, out var cameraIntrinsics);
			defaultFov = cameraIntrinsics.FOVPort;
			inited = true;
		}
	}

	private void UpdateDefaultExternalCamera()
	{
		OVRPlugin.CameraIntrinsics cameraIntrinsics = default(OVRPlugin.CameraIntrinsics);
		OVRPlugin.CameraExtrinsics cameraExtrinsics = default(OVRPlugin.CameraExtrinsics);
		cameraIntrinsics.IsValid = OVRPlugin.Bool.True;
		cameraIntrinsics.LastChangedTimeSeconds = Time.time;
		float num = defaultExternalCamera.fieldOfView * (MathF.PI / 180f);
		float num2 = Mathf.Atan(Mathf.Tan(num * 0.5f) * 1.7777778f) * 2f;
		OVRPlugin.Fovf fOVPort = default(OVRPlugin.Fovf);
		fOVPort.UpTan = (fOVPort.DownTan = Mathf.Tan(num * 0.5f));
		fOVPort.LeftTan = (fOVPort.RightTan = Mathf.Tan(num2 * 0.5f));
		cameraIntrinsics.FOVPort = fOVPort;
		cameraIntrinsics.VirtualNearPlaneDistanceMeters = defaultExternalCamera.nearClipPlane;
		cameraIntrinsics.VirtualFarPlaneDistanceMeters = defaultExternalCamera.farClipPlane;
		cameraIntrinsics.ImageSensorPixelResolution.w = 1920;
		cameraIntrinsics.ImageSensorPixelResolution.h = 1080;
		cameraExtrinsics.IsValid = OVRPlugin.Bool.True;
		cameraExtrinsics.LastChangedTimeSeconds = Time.time;
		cameraExtrinsics.CameraStatusData = OVRPlugin.CameraStatus.CameraStatus_Calibrated;
		cameraExtrinsics.AttachedToNode = OVRPlugin.Node.None;
		OVRCameraRig componentInParent = Camera.main.GetComponentInParent<OVRCameraRig>();
		if ((bool)componentInParent)
		{
			OVRPose oVRPose = componentInParent.trackingSpace.ToOVRPose();
			OVRPose oVRPose2 = defaultExternalCamera.transform.ToOVRPose();
			cameraExtrinsics.RelativePose = (oVRPose.Inverse() * oVRPose2).ToPosef();
		}
		else
		{
			cameraExtrinsics.RelativePose = OVRPlugin.Posef.identity;
		}
		if (!OVRPlugin.SetDefaultExternalCamera("UnityExternalCamera", ref cameraIntrinsics, ref cameraExtrinsics))
		{
			Debug.LogError("SetDefaultExternalCamera() failed");
		}
	}

	private void Update()
	{
		if (!inited)
		{
			Initialize();
		}
		else
		{
			if (!defaultExternalCamera || !OVRPlugin.IsMixedRealityInitialized())
			{
				return;
			}
			if (OVRInput.GetDown(OVRInput.Button.One))
			{
				if (currentMode == CameraMode.ThirdPerson)
				{
					currentMode = CameraMode.Normal;
				}
				else
				{
					currentMode++;
				}
				Debug.LogFormat("Camera mode change to {0}", currentMode);
			}
			if (currentMode == CameraMode.Normal)
			{
				UpdateDefaultExternalCamera();
				OVRPlugin.OverrideExternalCameraFov(0, useOverriddenFov: false, default(OVRPlugin.Fovf));
				OVRPlugin.OverrideExternalCameraStaticPose(0, useOverriddenPose: false, OVRPlugin.Posef.identity);
			}
			else if (currentMode == CameraMode.OverrideFov)
			{
				OVRPlugin.Fovf fovf = defaultFov;
				OVRPlugin.Fovf fov = default(OVRPlugin.Fovf);
				fov.LeftTan = fovf.LeftTan * 2f;
				fov.RightTan = fovf.RightTan * 2f;
				fov.UpTan = fovf.UpTan * 2f;
				fov.DownTan = fovf.DownTan * 2f;
				OVRPlugin.OverrideExternalCameraFov(0, useOverriddenFov: true, fov);
				OVRPlugin.OverrideExternalCameraStaticPose(0, useOverriddenPose: false, OVRPlugin.Posef.identity);
				if (!OVRPlugin.GetUseOverriddenExternalCameraFov(0))
				{
					Debug.LogWarning("FOV not overridden");
				}
			}
			else
			{
				if (currentMode != CameraMode.ThirdPerson)
				{
					return;
				}
				Camera component = GetComponent<Camera>();
				if (!(component == null))
				{
					float num = component.fieldOfView * (MathF.PI / 180f);
					float num2 = Mathf.Atan(Mathf.Tan(num * 0.5f) * component.aspect) * 2f;
					OVRPlugin.Fovf fov2 = default(OVRPlugin.Fovf);
					fov2.UpTan = (fov2.DownTan = Mathf.Tan(num * 0.5f));
					fov2.LeftTan = (fov2.RightTan = Mathf.Tan(num2 * 0.5f));
					OVRPlugin.OverrideExternalCameraFov(0, useOverriddenFov: true, fov2);
					OVRCameraRig componentInParent = Camera.main.GetComponentInParent<OVRCameraRig>();
					if ((bool)componentInParent)
					{
						OVRPose oVRPose = componentInParent.trackingSpace.ToOVRPose();
						OVRPose oVRPose2 = base.transform.ToOVRPose();
						OVRPose oVRPose3 = oVRPose.Inverse() * oVRPose2;
						OVRPlugin.Posef poseInStageOrigin = (OVRPlugin.GetTrackingTransformRelativePose(OVRPlugin.TrackingOrigin.Stage).ToOVRPose().Inverse() * oVRPose3).ToPosef();
						OVRPlugin.OverrideExternalCameraStaticPose(0, useOverriddenPose: true, poseInStageOrigin);
					}
					else
					{
						OVRPlugin.OverrideExternalCameraStaticPose(0, useOverriddenPose: false, OVRPlugin.Posef.identity);
					}
					if (!OVRPlugin.GetUseOverriddenExternalCameraFov(0))
					{
						Debug.LogWarning("FOV not overridden");
					}
					if (!OVRPlugin.GetUseOverriddenExternalCameraStaticPose(0))
					{
						Debug.LogWarning("StaticPose not overridden");
					}
				}
			}
		}
	}
}
