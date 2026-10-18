using Unity.XR.CoreUtils;
using UnityEngine.XR.Interaction.Toolkit.Locomotion;

namespace UnityEngine.XR.Interaction.Toolkit.Utilities;

public static class LocomotionUtility
{
	public static Vector3 GetCameraFloorWorldPosition(this XROrigin xrOrigin)
	{
		Vector3 cameraInOriginSpacePos = xrOrigin.CameraInOriginSpacePos;
		Vector3 position = new Vector3(cameraInOriginSpacePos.x, 0f, cameraInOriginSpacePos.z);
		return xrOrigin.Origin.transform.TransformPoint(position);
	}

	internal static bool TryGetOriginTransform(LocomotionProvider locomotionProvider, out Transform originTransform)
	{
		if (locomotionProvider != null)
		{
			return TryGetOriginTransform(locomotionProvider.mediator, out originTransform);
		}
		originTransform = null;
		return false;
	}

	internal static bool TryGetOriginTransform(LocomotionMediator mediator, out Transform originTransform)
	{
		if (mediator != null)
		{
			XROrigin xrOrigin = mediator.xrOrigin;
			if (xrOrigin != null)
			{
				GameObject origin = xrOrigin.Origin;
				if (origin != null)
				{
					originTransform = origin.transform;
					return true;
				}
			}
		}
		originTransform = null;
		return false;
	}

	internal static bool TryGetOriginTransform(XRBodyTransformer bodyTransformer, out Transform originTransform)
	{
		if (bodyTransformer != null)
		{
			XROrigin xrOrigin = bodyTransformer.xrOrigin;
			if (xrOrigin != null)
			{
				GameObject origin = xrOrigin.Origin;
				if (origin != null)
				{
					originTransform = origin.transform;
					return true;
				}
			}
		}
		originTransform = null;
		return false;
	}
}
