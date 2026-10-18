using System;

namespace g3;

public class ArcLengthSoftTranslation
{
	private DCurve3 _curve;

	private Vector3d _handle;

	private double _arcradius;

	private Func<double, double, double> _weightfunc;

	public int[] roi_index;

	public double[] roi_weights;

	public Vector3d[] start_positions;

	private bool roi_valid;

	private int curve_timestamp;

	private Vector3d start_handle;

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
				invalidate_roi();
			}
		}
	}

	public Vector3d Handle
	{
		get
		{
			return _handle;
		}
		set
		{
			if (_handle != value)
			{
				_handle = value;
				invalidate_roi();
			}
		}
	}

	public double ArcRadius
	{
		get
		{
			return _arcradius;
		}
		set
		{
			if (_arcradius != value)
			{
				_arcradius = value;
				invalidate_roi();
			}
		}
	}

	public Func<double, double, double> WeightFunc
	{
		get
		{
			return _weightfunc;
		}
		set
		{
			if (_weightfunc != value)
			{
				_weightfunc = value;
				invalidate_roi();
			}
		}
	}

	public ArcLengthSoftTranslation()
	{
		Handle = Vector3d.Zero;
		ArcRadius = 1.0;
		WeightFunc = (double d, double r) => MathUtil.WyvillFalloff01(MathUtil.Clamp(d / r, 0.0, 1.0));
		roi_valid = false;
	}

	public void BeginDeformation()
	{
		UpdateROI();
		start_handle = Handle;
		if (start_positions == null || start_positions.Length != roi_index.Length)
		{
			start_positions = new Vector3d[roi_index.Length];
		}
		for (int i = 0; i < roi_index.Length; i++)
		{
			start_positions[i] = Curve.GetVertex(roi_index[i]);
		}
	}

	public void UpdateDeformation(Vector3d newHandlePos)
	{
		Vector3d vector3d = newHandlePos - start_handle;
		for (int i = 0; i < roi_index.Length; i++)
		{
			Vector3d v = start_positions[i] + roi_weights[i] * vector3d;
			Curve.SetVertex(roi_index[i], v);
		}
	}

	public void EndDeformation()
	{
	}

	private void invalidate_roi()
	{
		roi_valid = false;
	}

	private bool check_roi_valid()
	{
		if (!roi_valid)
		{
			return false;
		}
		if (Curve.Timestamp != curve_timestamp)
		{
			return false;
		}
		return true;
	}

	public void UpdateROI(int nNearVertexHint = -1)
	{
		if (check_roi_valid())
		{
			return;
		}
		int num = nNearVertexHint;
		if (nNearVertexHint < 0)
		{
			num = CurveUtils.FindNearestIndex(Curve, Handle);
		}
		int vertexCount = Curve.VertexCount;
		int num2 = 1;
		double num3 = 0.0;
		int num4 = -1;
		for (int i = num + 1; i < vertexCount; i++)
		{
			if (!(num3 < ArcRadius))
			{
				break;
			}
			double length = (Curve.GetVertex(i) - Curve.GetVertex(i - 1)).Length;
			num3 += length;
			if (num3 < ArcRadius)
			{
				num2++;
				num4 = i;
			}
		}
		double num5 = 0.0;
		int num6 = -1;
		int num7 = num - 1;
		while (num7 >= 0 && num5 < ArcRadius)
		{
			double length2 = (Curve.GetVertex(num7) - Curve.GetVertex(num7 + 1)).Length;
			num5 += length2;
			if (num5 < ArcRadius)
			{
				num2++;
				num6 = num7;
			}
			num7--;
		}
		if (roi_index == null || roi_index.Length != num2)
		{
			roi_index = new int[num2];
			roi_weights = new double[num2];
		}
		int num8 = 0;
		roi_index[num8] = num;
		roi_weights[num8++] = WeightFunc(0.0, ArcRadius);
		if (num4 >= 0)
		{
			num3 = 0.0;
			for (int j = num + 1; j <= num4; j++)
			{
				num3 += (Curve.GetVertex(j) - Curve.GetVertex(j - 1)).Length;
				roi_index[num8] = j;
				roi_weights[num8++] = WeightFunc(num3, ArcRadius);
			}
		}
		if (num6 >= 0)
		{
			num5 = 0.0;
			for (int num9 = num - 1; num9 >= num6; num9--)
			{
				num5 += (Curve.GetVertex(num9) - Curve.GetVertex(num9 + 1)).Length;
				roi_index[num8] = num9;
				roi_weights[num8++] = WeightFunc(num5, ArcRadius);
			}
		}
		roi_valid = true;
		curve_timestamp = Curve.Timestamp;
	}
}
