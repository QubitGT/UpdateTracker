using System;

namespace g3;

public class ImplicitLine3d : BoundedImplicitFunction3d, ImplicitFunction3d
{
	public Segment3d Segment;

	public double Radius;

	public double Value(ref Vector3d pt)
	{
		return Math.Sqrt(Segment.DistanceSquared(pt)) - Radius;
	}

	public AxisAlignedBox3d Bounds()
	{
		Vector3d vector3d = Radius * Vector3d.One;
		Vector3d p = Segment.P0;
		Vector3d p2 = Segment.P1;
		AxisAlignedBox3d result = new AxisAlignedBox3d(p - vector3d, p + vector3d);
		result.Contain(p2 - vector3d);
		result.Contain(p2 + vector3d);
		return result;
	}
}
