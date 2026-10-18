using System;

namespace g3;

public class InPlaceIterativeCurveSmooth
{
	private DCurve3 _curve;

	private int _startRange;

	private int _endRange;

	private float _alpha;

	public DCurve3 Curve
	{
		get
		{
			return _curve;
		}
		set
		{
			if (_curve != value)
			{
				_curve = value;
			}
		}
	}

	public int Start
	{
		get
		{
			return _startRange;
		}
		set
		{
			_startRange = value;
		}
	}

	public int End
	{
		get
		{
			return _endRange;
		}
		set
		{
			_endRange = value;
		}
	}

	public float Alpha
	{
		get
		{
			return _alpha;
		}
		set
		{
			_alpha = MathUtil.Clamp(value, 0f, 1f);
		}
	}

	public InPlaceIterativeCurveSmooth()
	{
		Start = (End = -1);
		Alpha = 0.25f;
	}

	public InPlaceIterativeCurveSmooth(DCurve3 curve, float alpha = 0.25f)
	{
		Curve = curve;
		Start = 0;
		End = Curve.VertexCount;
		Alpha = alpha;
	}

	public void UpdateDeformation(int nIterations = 1)
	{
		if (Curve.Closed)
		{
			UpdateDeformation_Closed(nIterations);
		}
		else
		{
			UpdateDeformation_Open(nIterations);
		}
	}

	public void UpdateDeformation_Closed(int nIterations = 1)
	{
		if (Start < 0 || Start > Curve.VertexCount || End > Curve.VertexCount)
		{
			throw new ArgumentOutOfRangeException("InPlaceIterativeCurveSmooth.UpdateDeformation: range is invalid");
		}
		int vertexCount = Curve.VertexCount;
		for (int i = 0; i < nIterations; i++)
		{
			for (int j = Start; j < End; j++)
			{
				int key = j % vertexCount;
				int key2 = ((j == 0) ? (vertexCount - 1) : (j - 1));
				int key3 = (j + 1) % vertexCount;
				Vector3d vector3d = Curve[key2];
				Vector3d vector3d2 = Curve[key3];
				Vector3d vector3d3 = (vector3d + vector3d2) * 0.5;
				Curve[key] = (1f - Alpha) * Curve[key] + Alpha * vector3d3;
			}
		}
	}

	public void UpdateDeformation_Open(int nIterations = 1)
	{
		if (Start < 0 || Start > Curve.VertexCount || End > Curve.VertexCount)
		{
			throw new ArgumentOutOfRangeException("InPlaceIterativeCurveSmooth.UpdateDeformation: range is invalid");
		}
		for (int i = 0; i < nIterations; i++)
		{
			for (int j = Start; j <= End; j++)
			{
				if (j != 0 && j < Curve.VertexCount - 1)
				{
					Vector3d vector3d = Curve[j - 1];
					Vector3d vector3d2 = Curve[j + 1];
					Vector3d vector3d3 = (vector3d + vector3d2) * 0.5;
					Curve[j] = (1f - Alpha) * Curve[j] + Alpha * vector3d3;
				}
			}
		}
	}
}
