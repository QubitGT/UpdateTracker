namespace Technie.PhysicsCreator.QHull;

public class Point3d : Vector3d
{
	public Point3d()
	{
	}

	public Point3d(Vector3d v)
	{
		set(v);
	}

	public Point3d(double x, double y, double z)
	{
		set(x, y, z);
	}
}
