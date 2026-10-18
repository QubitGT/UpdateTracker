namespace g3;

public class SculptCurveMove : StandardSculptCurveDeformation
{
	public SculptCurveMove()
	{
		SmoothAlpha = 0.0;
		SmoothIterations = 0;
	}

	public override DeformInfo Apply(Frame3f vNextPos)
	{
		if (((Vector3d)(vNextPos.Origin - vPreviousPos.Origin)).Length < 9.999999747378752E-05)
		{
			DeformInfo result = default(DeformInfo);
			result.bNoChange = true;
			result.maxEdgeLenSqr = 0.0;
			result.minEdgeLenSqr = double.MaxValue;
			return result;
		}
		DeformF = delegate(int idx, double t)
		{
			Vector3d v = vPreviousPos.ToFrameP(base.Curve[idx]);
			Vector3d b = vNextPos.FromFrameP(v);
			return Vector3d.Lerp(base.Curve[idx], b, t);
		};
		return base.Apply(vNextPos);
	}
}
