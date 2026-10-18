using System;
using System.Collections;

namespace g3;

public class StandardSculptCurveDeformation : SculptCurveDeformation
{
	public Func<int, double, Vector3d> DeformF;

	public double SmoothAlpha = 0.10000000149011612;

	public int SmoothIterations = 1;

	public DVector<Vector3d> NewV;

	public BitArray ModifiedV;

	public StandardSculptCurveDeformation()
	{
		NewV = new DVector<Vector3d>();
		NewV.resize(256);
		ModifiedV = new BitArray(256);
	}

	public override DeformInfo Apply(Frame3f vNextPos)
	{
		Interval1d empty = Interval1d.Empty;
		int vertexCount = base.Curve.VertexCount;
		if (vertexCount > NewV.size)
		{
			NewV.resize(vertexCount);
		}
		if (vertexCount > ModifiedV.Length)
		{
			ModifiedV = new BitArray(2 * vertexCount);
		}
		ModifiedV.SetAll(value: false);
		bool flag = SmoothAlpha > 0.0 && SmoothIterations > 0;
		double num = base.Radius * base.Radius;
		if (DeformF != null)
		{
			for (int i = 0; i < vertexCount; i++)
			{
				double lengthSquared = (base.Curve[i] - vPreviousPos.Origin).LengthSquared;
				if (!(lengthSquared < num))
				{
					continue;
				}
				double arg = base.WeightFunc(Math.Sqrt(lengthSquared), base.Radius);
				Vector3d value = DeformF(i, arg);
				if (!flag)
				{
					if (i > 0)
					{
						empty.Contain(value.DistanceSquared(base.Curve[i - 1]));
					}
					if (i < vertexCount - 1)
					{
						empty.Contain(value.DistanceSquared(base.Curve[i + 1]));
					}
				}
				NewV[i] = value;
				ModifiedV[i] = true;
			}
		}
		if (flag)
		{
			for (int j = 0; j < SmoothIterations; j++)
			{
				bool num2 = !base.Curve.Closed;
				int num3 = (base.Curve.Closed ? vertexCount : (vertexCount - 1));
				for (int k = (num2 ? 1 : 0); k < num3; k++)
				{
					Vector3d vector3d = (ModifiedV[k] ? NewV[k] : base.Curve[k]);
					double lengthSquared2 = (vector3d - vPreviousPos.Origin).LengthSquared;
					if (ModifiedV[k] || lengthSquared2 < num)
					{
						double num4 = SmoothAlpha * base.WeightFunc(Math.Sqrt(lengthSquared2), base.Radius);
						int num5 = ((k == 0) ? (vertexCount - 1) : (k - 1));
						int num6 = (k + 1) % vertexCount;
						Vector3d obj = (ModifiedV[num5] ? NewV[num5] : base.Curve[num5]);
						Vector3d vector3d2 = (ModifiedV[num6] ? NewV[num6] : base.Curve[num6]);
						Vector3d vector3d3 = (obj + vector3d2) * 0.5;
						NewV[k] = (1.0 - num4) * vector3d + num4 * vector3d3;
						ModifiedV[k] = true;
						if (k > 0)
						{
							empty.Contain(NewV[k].DistanceSquared(base.Curve[k - 1]));
						}
						if (k < vertexCount - 1)
						{
							empty.Contain(NewV[k].DistanceSquared(base.Curve[k + 1]));
						}
					}
				}
			}
		}
		for (int l = 0; l < vertexCount; l++)
		{
			if (ModifiedV[l])
			{
				base.Curve[l] = NewV[l];
			}
		}
		DeformInfo result = default(DeformInfo);
		result.bNoChange = false;
		result.minEdgeLenSqr = empty.a;
		result.maxEdgeLenSqr = empty.b;
		return result;
	}
}
