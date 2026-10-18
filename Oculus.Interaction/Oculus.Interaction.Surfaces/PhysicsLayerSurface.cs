using UnityEngine;

namespace Oculus.Interaction.Surfaces;

public class PhysicsLayerSurface : MonoBehaviour, ISurface
{
	[SerializeField]
	[Tooltip("Collision layers to detect hits against. -1 includes all layers.")]
	private LayerMask _layerMask = -1;

	[SerializeField]
	[Optional]
	[Tooltip("When using ClosestSurfacePoint, the maximum number of Colliders to check")]
	private int _closeCollidersCacheSize = 20;

	private Collider[] _cachedCloseColliders;

	private SphereCollider _sphereCollider;

	public LayerMask LayerMask
	{
		get
		{
			return _layerMask;
		}
		set
		{
			_layerMask = value;
		}
	}

	public int CloseCollidersCacheSize
	{
		get
		{
			return _closeCollidersCacheSize;
		}
		set
		{
			_closeCollidersCacheSize = value;
		}
	}

	public Transform Transform => null;

	protected virtual void Awake()
	{
		_sphereCollider = base.gameObject.AddComponent<SphereCollider>();
		_sphereCollider.isTrigger = true;
		_sphereCollider.enabled = false;
	}

	private void OnDestroy()
	{
		if (_sphereCollider != null)
		{
			Object.Destroy(_sphereCollider);
		}
	}

	public bool ClosestSurfacePoint(in Vector3 point, out SurfaceHit surfaceHit, float maxDistance = 0f)
	{
		if (_cachedCloseColliders == null || _cachedCloseColliders.Length != _closeCollidersCacheSize)
		{
			_cachedCloseColliders = new Collider[_closeCollidersCacheSize];
		}
		float num = ((maxDistance > 0f) ? maxDistance : float.MaxValue);
		surfaceHit = default(SurfaceHit);
		int value = _layerMask.value;
		int num2 = Physics.OverlapSphereNonAlloc(point, num, _cachedCloseColliders, value, QueryTriggerInteraction.Ignore);
		if (num2 == 0)
		{
			return false;
		}
		float num3 = num;
		bool result = false;
		_sphereCollider.enabled = true;
		for (int i = 0; i < num2; i++)
		{
			Collider collider = _cachedCloseColliders[i];
			_sphereCollider.radius = num3;
			if (Physics.ComputePenetration(_sphereCollider, point, Quaternion.identity, collider, collider.transform.position, collider.transform.rotation, out var direction, out var _) && collider.Raycast(new Ray(point, -direction), out var hitInfo, num3) && hitInfo.distance < num3)
			{
				result = true;
				num3 = hitInfo.distance;
				surfaceHit = new SurfaceHit
				{
					Point = hitInfo.point,
					Normal = hitInfo.normal,
					Distance = hitInfo.distance
				};
			}
		}
		_sphereCollider.enabled = false;
		return result;
	}

	public bool Raycast(in Ray ray, out SurfaceHit surfaceHit, float maxDistance = 0f)
	{
		int value = _layerMask.value;
		float maxDistance2 = ((maxDistance > 0f) ? maxDistance : float.MaxValue);
		if (Physics.Raycast(ray, out var hitInfo, maxDistance2, value, QueryTriggerInteraction.Ignore))
		{
			surfaceHit = new SurfaceHit
			{
				Point = hitInfo.point,
				Normal = hitInfo.normal,
				Distance = hitInfo.distance
			};
			return true;
		}
		surfaceHit = default(SurfaceHit);
		return false;
	}

	bool ISurface.Raycast(in Ray ray, out SurfaceHit hit, float maxDistance)
	{
		return Raycast(in ray, out hit, maxDistance);
	}

	bool ISurface.ClosestSurfacePoint(in Vector3 point, out SurfaceHit hit, float maxDistance)
	{
		return ClosestSurfacePoint(in point, out hit, maxDistance);
	}
}
