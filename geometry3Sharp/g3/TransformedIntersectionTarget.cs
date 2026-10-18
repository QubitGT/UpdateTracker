using System;

namespace g3;

public class TransformedIntersectionTarget : IIntersectionTarget
{
	private DMeshIntersectionTarget BaseTarget;

	public Func<Ray3d, Ray3d> MapToBaseF;

	public Func<Vector3d, Vector3d> MapFromBasePosF;

	public Func<Vector3d, Vector3d> MapFromBaseNormalF;

	public bool HasNormal => BaseTarget.HasNormal;

	public bool RayIntersect(Ray3d ray, out Vector3d vHit, out Vector3d vHitNormal)
	{
		Ray3d ray2 = MapToBaseF(ray);
		if (BaseTarget.RayIntersect(ray2, out vHit, out vHitNormal))
		{
			vHit = MapFromBasePosF(vHit);
			vHitNormal = MapFromBasePosF(vHitNormal);
			return true;
		}
		return false;
	}
}
