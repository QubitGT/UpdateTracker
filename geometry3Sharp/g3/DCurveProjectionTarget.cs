namespace g3;

public class DCurveProjectionTarget : IProjectionTarget
{
	public DCurve3 Curve;

	public DCurveProjectionTarget(DCurve3 curve)
	{
		Curve = curve;
	}

	public Vector3d Project(Vector3d vPoint, int identifier = -1)
	{
		Vector3d result = Vector3d.Zero;
		double num = double.MaxValue;
		int vertexCount = Curve.VertexCount;
		int num2 = (Curve.Closed ? vertexCount : (vertexCount - 1));
		for (int i = 0; i < num2; i++)
		{
			Vector3d vector3d = new Segment3d(Curve[i], Curve[(i + 1) % vertexCount]).NearestPoint(vPoint);
			double num3 = vector3d.DistanceSquared(vPoint);
			if (num3 < num)
			{
				num = num3;
				result = vector3d;
			}
		}
		if (!(num < double.MaxValue))
		{
			return vPoint;
		}
		return result;
	}
}
