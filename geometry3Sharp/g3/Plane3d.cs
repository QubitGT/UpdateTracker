namespace g3;

public struct Plane3d
{
	public Vector3d Normal;

	public double Constant;

	public Plane3d(Vector3d normal, double constant)
	{
		Normal = normal;
		Constant = constant;
	}

	public Plane3d(Vector3d normal, Vector3d point)
	{
		Normal = normal;
		Constant = Normal.Dot(point);
	}

	public Plane3d(Vector3d p0, Vector3d p1, Vector3d p2)
	{
		Vector3d vector3d = p1 - p0;
		Vector3d v = p2 - p0;
		Normal = vector3d.UnitCross(v);
		Constant = Normal.Dot(p0);
	}

	public double DistanceTo(Vector3d p)
	{
		return Normal.Dot(p) - Constant;
	}

	public int WhichSide(Vector3d p)
	{
		double num = DistanceTo(p);
		if (num < 0.0)
		{
			return -1;
		}
		if (num > 0.0)
		{
			return 1;
		}
		return 0;
	}
}
