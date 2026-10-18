namespace g3;

public class PlaneProjectionTarget : IProjectionTarget
{
	public Vector3d Origin;

	public Vector3d Normal;

	public Vector3d Project(Vector3d vPoint, int identifier = -1)
	{
		Vector3d vector3d = vPoint - Origin;
		return Origin + (vector3d - vector3d.Dot(Normal) * Normal);
	}
}
