using UnityEngine;

namespace Oculus.Interaction.Surfaces;

public class ColliderSurface : MonoBehaviour, ISurface, IBounds
{
	[Tooltip("The Surface will be represented by this collider.")]
	[SerializeField]
	private Collider _collider;

	public Transform Transform => base.transform;

	public Bounds Bounds => _collider.bounds;

	protected virtual void Start()
	{
	}

	public bool Raycast(in Ray ray, out SurfaceHit hit, float maxDistance = 0f)
	{
		hit = default(SurfaceHit);
		if (_collider.Raycast(ray, out var hitInfo, (maxDistance <= 0f) ? float.MaxValue : maxDistance))
		{
			hit.Point = hitInfo.point;
			hit.Normal = hitInfo.normal;
			hit.Distance = hitInfo.distance;
			return true;
		}
		return false;
	}

	public bool ClosestSurfacePoint(in Vector3 point, out SurfaceHit hit, float maxDistance = 0f)
	{
		Vector3 direction = _collider.ClosestPoint(point) - point;
		Ray ray;
		if (direction.x == 0f && direction.y == 0f && direction.z == 0f)
		{
			Vector3 vector = _collider.bounds.center - point;
			ray = new Ray(point - vector, vector);
			return Raycast(in ray, out hit, float.MaxValue);
		}
		ray = new Ray(point, direction);
		return Raycast(in ray, out hit, maxDistance);
	}

	public void InjectAllColliderSurface(Collider collider)
	{
		InjectCollider(collider);
	}

	public void InjectCollider(Collider collider)
	{
		_collider = collider;
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
