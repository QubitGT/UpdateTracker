using UnityEngine;

namespace Meta.XR.MRUtilityKit.BuildingBlocks;

public class VisualizeEnvRaycast : MonoBehaviour
{
	[SerializeField]
	[Tooltip("Supply a LineRenderer to visualize the raycast ray")]
	private LineRenderer _raycastLine;

	[SerializeField]
	[Tooltip("Supply a Transform to see the ray hit point")]
	private Transform _raycastHitPoint;

	[SerializeField]
	internal SpaceLocator _spaceLocator;

	private EnvironmentRaycastManager _raycastManager;

	private void Awake()
	{
		_raycastManager = Object.FindFirstObjectByType<EnvironmentRaycastManager>();
	}

	private void Update()
	{
		VisualizeRay();
	}

	private void VisualizeRay()
	{
		if (!(_raycastManager == null))
		{
			Ray raycastRay = _spaceLocator.GetRaycastRay();
			EnvironmentRaycastHit hit;
			bool flag = _raycastManager.Raycast(raycastRay, out hit) || hit.status == EnvironmentRaycastHitStatus.HitPointOccluded;
			bool flag2 = hit.normalConfidence > 0f;
			_raycastLine.enabled = flag;
			_raycastHitPoint.gameObject.SetActive(flag && flag2);
			if (_raycastLine != null)
			{
				_raycastLine.SetPosition(0, raycastRay.origin);
				_raycastLine.SetPosition(1, hit.point);
			}
			if (_raycastHitPoint != null && flag2)
			{
				_raycastHitPoint.SetPositionAndRotation(hit.point, Quaternion.LookRotation(hit.normal));
			}
		}
	}
}
