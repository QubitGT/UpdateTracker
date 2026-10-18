using UnityEngine;

public abstract class OVRComposition
{
	public bool cameraInTrackingSpace;

	public OVRCameraRig cameraRig;

	protected bool usingLastAttachedNodePose;

	protected OVRPose lastAttachedNodePose;

	protected OVRComposition(GameObject parentObject, Camera mainCamera, OVRMixedRealityCaptureConfiguration configuration)
	{
		RefreshCameraRig(parentObject, mainCamera);
	}

	public abstract OVRManager.CompositionMethod CompositionMethod();

	public abstract void Update(GameObject gameObject, Camera mainCamera, OVRMixedRealityCaptureConfiguration configuration, OVRManager.TrackingOrigin trackingOrigin);

	public abstract void Cleanup();

	public virtual void RecenterPose()
	{
	}

	public void RefreshCameraRig(GameObject parentObject, Camera mainCamera)
	{
		OVRCameraRig oVRCameraRig = mainCamera.GetComponentInParent<OVRCameraRig>();
		if (oVRCameraRig == null)
		{
			oVRCameraRig = parentObject.GetComponent<OVRCameraRig>();
		}
		cameraInTrackingSpace = oVRCameraRig != null && oVRCameraRig.trackingSpace != null;
		cameraRig = oVRCameraRig;
		Debug.Log((oVRCameraRig == null) ? "[OVRComposition] CameraRig not found" : "[OVRComposition] CameraRig found");
	}

	public OVRPose ComputeCameraWorldSpacePose(OVRPlugin.CameraExtrinsics extrinsics, Camera mainCamera)
	{
		return ComputeCameraTrackingSpacePose(extrinsics).ToWorldSpacePose(mainCamera);
	}

	public OVRPose ComputeCameraTrackingSpacePose(OVRPlugin.CameraExtrinsics extrinsics)
	{
		OVRPose oVRPose = default(OVRPose);
		oVRPose = extrinsics.RelativePose.ToOVRPose();
		if (extrinsics.AttachedToNode != OVRPlugin.Node.None && OVRPlugin.GetNodePresent(extrinsics.AttachedToNode))
		{
			if (usingLastAttachedNodePose)
			{
				Debug.Log("The camera attached node get tracked");
				usingLastAttachedNodePose = false;
			}
			oVRPose = (lastAttachedNodePose = OVRPlugin.GetNodePose(extrinsics.AttachedToNode, OVRPlugin.Step.Render).ToOVRPose()) * oVRPose;
		}
		else if (extrinsics.AttachedToNode != OVRPlugin.Node.None)
		{
			if (!usingLastAttachedNodePose)
			{
				Debug.LogWarning("The camera attached node could not be tracked, using the last pose");
				usingLastAttachedNodePose = true;
			}
			oVRPose = lastAttachedNodePose * oVRPose;
		}
		return oVRPose;
	}
}
