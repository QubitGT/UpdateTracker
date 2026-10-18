namespace g3;

public class PlaneIntersectionTarget : IIntersectionTarget
{
	public Frame3f PlaneFrame;

	public int NormalAxis = 2;

	public bool HasNormal => true;

	public bool RayIntersect(Ray3d ray, out Vector3d vHit, out Vector3d vHitNormal)
	{
		Vector3f vector3f = PlaneFrame.RayPlaneIntersection((Vector3f)ray.Origin, (Vector3f)ray.Direction, NormalAxis);
		vHit = vector3f;
		vHitNormal = Vector3f.AxisY;
		return vector3f != Vector3f.Invalid;
	}
}
