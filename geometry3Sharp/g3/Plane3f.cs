namespace g3;

public struct Plane3f
{
	public Vector3f Normal;

	public float Constant;

	public Plane3f(Vector3f normal, float constant)
	{
		Normal = normal;
		Constant = constant;
	}

	public Plane3f(Vector3f normal, Vector3f point)
	{
		Normal = normal;
		Constant = Normal.Dot(point);
	}

	public Plane3f(Vector3f p0, Vector3f p1, Vector3f p2)
	{
		Vector3f vector3f = p1 - p0;
		Vector3f v = p2 - p0;
		Normal = vector3f.UnitCross(v);
		Constant = Normal.Dot(p0);
	}

	public float DistanceTo(Vector3f p)
	{
		return Normal.Dot(p) - Constant;
	}

	public int WhichSide(Vector3f p)
	{
		float num = DistanceTo(p);
		if (num < 0f)
		{
			return -1;
		}
		if (num > 0f)
		{
			return 1;
		}
		return 0;
	}
}
