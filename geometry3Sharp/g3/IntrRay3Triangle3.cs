namespace g3;

public class IntrRay3Triangle3
{
	private Ray3d ray;

	private Triangle3d triangle;

	public int Quantity;

	public IntersectionResult Result;

	public IntersectionType Type;

	public double RayParameter;

	public Vector3d TriangleBaryCoords;

	public Ray3d Ray
	{
		get
		{
			return ray;
		}
		set
		{
			ray = value;
			Result = IntersectionResult.NotComputed;
		}
	}

	public Triangle3d Triangle
	{
		get
		{
			return triangle;
		}
		set
		{
			triangle = value;
			Result = IntersectionResult.NotComputed;
		}
	}

	public bool IsSimpleIntersection
	{
		get
		{
			if (Result == IntersectionResult.Intersects)
			{
				return Type == IntersectionType.Point;
			}
			return false;
		}
	}

	public IntrRay3Triangle3(Ray3d r, Triangle3d t)
	{
		ray = r;
		triangle = t;
	}

	public IntrRay3Triangle3 Compute()
	{
		Find();
		return this;
	}

	public bool Find()
	{
		if (Result != 0)
		{
			return Result != IntersectionResult.NoIntersection;
		}
		Vector3d v = ray.Origin - triangle.V0;
		Vector3d vector3d = triangle.V1 - triangle.V0;
		Vector3d v2 = triangle.V2 - triangle.V0;
		Vector3d v3 = vector3d.Cross(v2);
		double num = ray.Direction.Dot(v3);
		double num2;
		if (num > 1E-08)
		{
			num2 = 1.0;
		}
		else
		{
			if (!(num < -1E-08))
			{
				Result = IntersectionResult.NoIntersection;
				return false;
			}
			num2 = -1.0;
			num = 0.0 - num;
		}
		double num3 = num2 * ray.Direction.Dot(v.Cross(v2));
		if (num3 >= 0.0)
		{
			double num4 = num2 * ray.Direction.Dot(vector3d.Cross(v));
			if (num4 >= 0.0 && num3 + num4 <= num)
			{
				double num5 = (0.0 - num2) * v.Dot(v3);
				if (num5 >= 0.0)
				{
					double num6 = 1.0 / num;
					RayParameter = num5 * num6;
					double num7 = num3 * num6;
					double num8 = num4 * num6;
					TriangleBaryCoords = new Vector3d(1.0 - num7 - num8, num7, num8);
					Type = IntersectionType.Point;
					Quantity = 1;
					Result = IntersectionResult.Intersects;
					return true;
				}
			}
		}
		Result = IntersectionResult.NoIntersection;
		return false;
	}

	public static bool Intersects(ref Ray3d ray, ref Vector3d V0, ref Vector3d V1, ref Vector3d V2, out double rayT)
	{
		Vector3d v = ray.Origin - V0;
		Vector3d vector3d = V1 - V0;
		Vector3d v2 = V2 - V0;
		Vector3d v3 = vector3d.Cross(ref v2);
		rayT = double.MaxValue;
		double num = ray.Direction.Dot(ref v3);
		double num2;
		if (num > 1E-08)
		{
			num2 = 1.0;
		}
		else
		{
			if (!(num < -1E-08))
			{
				return false;
			}
			num2 = -1.0;
			num = 0.0 - num;
		}
		Vector3d v4 = v.Cross(ref v2);
		double num3 = num2 * ray.Direction.Dot(ref v4);
		if (num3 >= 0.0)
		{
			v4 = vector3d.Cross(ref v);
			double num4 = num2 * ray.Direction.Dot(ref v4);
			if (num4 >= 0.0 && num3 + num4 <= num)
			{
				double num5 = (0.0 - num2) * v.Dot(ref v3);
				if (num5 >= 0.0)
				{
					double num6 = 1.0 / num;
					rayT = num5 * num6;
					return true;
				}
			}
		}
		return false;
	}
}
