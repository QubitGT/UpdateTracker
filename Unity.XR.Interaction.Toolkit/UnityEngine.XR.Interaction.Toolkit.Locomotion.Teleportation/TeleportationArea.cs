using UnityEngine.Scripting.APIUpdating;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;

[AddComponentMenu("XR/Teleportation Area", 11)]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportationArea.html")]
[MovedFrom("UnityEngine.XR.Interaction.Toolkit")]
public class TeleportationArea : BaseTeleportationInteractable
{
	protected override bool GenerateTeleportRequest(IXRInteractor interactor, RaycastHit raycastHit, ref TeleportRequest teleportRequest)
	{
		if (raycastHit.collider == null)
		{
			return false;
		}
		if (IsSphereCastRay(interactor, out var _) && IsSphereCastOverlap(raycastHit))
		{
			return false;
		}
		teleportRequest.destinationPosition = raycastHit.point;
		teleportRequest.destinationRotation = base.transform.rotation;
		return true;
	}

	public override bool IsSelectableBy(IXRSelectInteractor interactor)
	{
		bool flag = base.IsSelectableBy(interactor);
		if (flag && IsSphereCastRay(interactor, out var rayInteractor) && rayInteractor.TryGetCurrent3DRaycastHit(out var raycastHit) && IsSphereCastOverlap(raycastHit))
		{
			return false;
		}
		return flag;
	}

	private static bool IsSphereCastRay(IXRInteractor interactor, out XRRayInteractor rayInteractor)
	{
		rayInteractor = interactor as XRRayInteractor;
		if (rayInteractor != null)
		{
			return rayInteractor.hitDetectionType == XRRayInteractor.HitDetectionType.SphereCast;
		}
		return false;
	}

	private static bool IsSphereCastOverlap(RaycastHit raycastHit)
	{
		if (raycastHit.distance != 0f)
		{
			return false;
		}
		Vector3 point = raycastHit.point;
		if (point.x == 0f && point.y == 0f)
		{
			return point.z == 0f;
		}
		return false;
	}
}
