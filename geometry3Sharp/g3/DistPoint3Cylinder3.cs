using System;

namespace g3;

public class DistPoint3Cylinder3
{
	private Vector3d point;

	private Cylinder3d cylinder;

	public double DistanceSquared = -1.0;

	public double SignedDistance;

	public Vector3d CylinderClosest;

	public Vector3d Point
	{
		get
		{
			return point;
		}
		set
		{
			point = value;
			DistanceSquared = -1.0;
		}
	}

	public Cylinder3d Cylinder
	{
		get
		{
			return cylinder;
		}
		set
		{
			cylinder = value;
			DistanceSquared = -1.0;
		}
	}

	public bool IsInside => SignedDistance < 0.0;

	public double SolidDistance
	{
		get
		{
			if (!(SignedDistance < 0.0))
			{
				return SignedDistance;
			}
			return 0.0;
		}
	}

	public DistPoint3Cylinder3(Vector3d PointIn, Cylinder3d CylinderIn)
	{
		point = PointIn;
		cylinder = CylinderIn;
	}

	public DistPoint3Cylinder3 Compute()
	{
		GetSquared();
		return this;
	}

	public double Get()
	{
		return Math.Sqrt(GetSquared());
	}

	public double GetSquared()
	{
		if (DistanceSquared >= 0.0)
		{
			return DistanceSquared;
		}
		if (cylinder.Height >= double.MaxValue)
		{
			return get_squared_infinite();
		}
		Vector3d direction = cylinder.Axis.Direction;
		Vector3d v = Vector3d.Zero;
		Vector3d v2 = Vector3d.Zero;
		Vector3d.ComputeOrthogonalComplement(1, direction, ref v, ref v2);
		double num = Cylinder.Height / 2.0;
		Vector3d v3 = point - cylinder.Axis.Origin;
		Vector3d vector3d = new Vector3d(v.Dot(v3), v2.Dot(v3), direction.Dot(v3));
		double num2 = 0.0;
		Vector3d zero = Vector3d.Zero;
		double num3 = cylinder.Radius * cylinder.Radius;
		double num4 = vector3d[0] * vector3d[0] + vector3d[1] * vector3d[1];
		double num5 = Math.Sqrt(num4);
		double num6 = num5 - Cylinder.Radius;
		double num7 = Cylinder.Radius / num5;
		Vector3d vector3d2 = new Vector3d(num7 * vector3d.x, num7 * vector3d.y, vector3d.z);
		bool flag = num4 >= num3;
		zero = vector3d2;
		num2 = num6;
		if (vector3d2.z >= num)
		{
			zero = (flag ? vector3d2 : vector3d);
			zero.z = num;
			num2 = zero.Distance(vector3d);
			flag = true;
		}
		else if (vector3d2.z <= 0.0 - num)
		{
			zero = (flag ? vector3d2 : vector3d);
			zero.z = 0.0 - num;
			num2 = zero.Distance(vector3d);
			flag = true;
		}
		else if (!flag)
		{
			if (vector3d2.z > 0.0 && Math.Abs(vector3d2.z - num) < Math.Abs(num6))
			{
				zero = vector3d;
				zero.z = num;
				num2 = zero.Distance(vector3d);
			}
			else if (vector3d2.z < 0.0 && Math.Abs(vector3d2.z - (0.0 - num)) < Math.Abs(num6))
			{
				zero = vector3d;
				zero.z = 0.0 - num;
				num2 = zero.Distance(vector3d);
			}
		}
		SignedDistance = (flag ? Math.Abs(num2) : (0.0 - Math.Abs(num2)));
		CylinderClosest = cylinder.Axis.Origin + zero.x * v + zero.y * v2 + zero.z * direction;
		DistanceSquared = num2 * num2;
		return DistanceSquared;
	}

	public double get_squared_infinite()
	{
		Vector3d direction = cylinder.Axis.Direction;
		Vector3d v = Vector3d.Zero;
		Vector3d v2 = Vector3d.Zero;
		Vector3d.ComputeOrthogonalComplement(1, direction, ref v, ref v2);
		Vector3d v3 = point - cylinder.Axis.Origin;
		Vector3d vector3d = new Vector3d(v.Dot(v3), v2.Dot(v3), direction.Dot(v3));
		double num = 0.0;
		Vector3d zero = Vector3d.Zero;
		double num2 = Math.Sqrt(vector3d[0] * vector3d[0] + vector3d[1] * vector3d[1]);
		num = num2 - Cylinder.Radius;
		double num3 = Cylinder.Radius / num2;
		zero = new Vector3d(num3 * vector3d.x, num3 * vector3d.y, vector3d.z);
		CylinderClosest = cylinder.Axis.Origin + zero.x * v + zero.y * v2 + zero.z * direction;
		SignedDistance = num;
		DistanceSquared = num * num;
		return DistanceSquared;
	}
}
