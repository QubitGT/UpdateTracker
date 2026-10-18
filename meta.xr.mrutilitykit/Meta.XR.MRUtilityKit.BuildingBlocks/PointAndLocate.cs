using UnityEngine;

namespace Meta.XR.MRUtilityKit.BuildingBlocks;

public class PointAndLocate : SpaceLocator
{
	[Tooltip("Assign a Transform to use that as raycast origin")]
	[SerializeField]
	internal Transform _raycastOrigin;

	protected override Transform RaycastOrigin => _raycastOrigin;

	public void Locate()
	{
		TryLocateSpace(out var _);
	}

	protected internal override Ray GetRaycastRay()
	{
		return new Ray(RaycastOrigin.position, RaycastOrigin.forward);
	}
}
