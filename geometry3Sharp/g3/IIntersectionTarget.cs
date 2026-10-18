namespace g3;

public interface IIntersectionTarget
{
	bool HasNormal { get; }

	bool RayIntersect(Ray3d ray, out Vector3d vHit, out Vector3d vHitNormal);
}
