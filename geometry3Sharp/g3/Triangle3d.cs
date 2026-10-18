namespace g3;

public struct Triangle3d
{
	public Vector3d V0;

	public Vector3d V1;

	public Vector3d V2;

	public Vector3d this[int key]
	{
		get
		{
			return key switch
			{
				1 => V1, 
				0 => V0, 
				_ => V2, 
			};
		}
		set
		{
			switch (key)
			{
			case 0:
				V0 = value;
				break;
			case 1:
				V1 = value;
				break;
			default:
				V2 = value;
				break;
			}
		}
	}

	public Vector3d Normal => MathUtil.Normal(ref V0, ref V1, ref V2);

	public double Area => MathUtil.Area(ref V0, ref V1, ref V2);

	public double AspectRatio => MathUtil.AspectRatio(ref V0, ref V1, ref V2);

	public Triangle3d(Vector3d v0, Vector3d v1, Vector3d v2)
	{
		V0 = v0;
		V1 = v1;
		V2 = v2;
	}

	public Vector3d PointAt(double bary0, double bary1, double bary2)
	{
		return bary0 * V0 + bary1 * V1 + bary2 * V2;
	}

	public Vector3d PointAt(Vector3d bary)
	{
		return bary.x * V0 + bary.y * V1 + bary.z * V2;
	}

	public Vector3d BarycentricCoords(Vector3d point)
	{
		return MathUtil.BarycentricCoords(point, V0, V1, V2);
	}

	public static implicit operator Triangle3d(Triangle3f v)
	{
		return new Triangle3d(v.V0, v.V1, v.V2);
	}

	public static explicit operator Triangle3f(Triangle3d v)
	{
		return new Triangle3f((Vector3f)v.V0, (Vector3f)v.V1, (Vector3f)v.V2);
	}
}
