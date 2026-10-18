using System;
using System.Collections.Generic;
using System.Linq;

namespace g3;

public class NURBSCurve2 : BaseCurve2, IParametricCurve2d
{
	public struct CurveDerivatives
	{
		public Vector2d p;

		public Vector2d d1;

		public Vector2d d2;

		public Vector2d d3;

		public bool bPosition;

		public bool bDer1;

		public bool bDer2;

		public bool bDer3;

		public void init()
		{
			bPosition = (bDer1 = (bDer2 = (bDer3 = false)));
		}

		public void init(bool pos, bool der1, bool der2, bool der3)
		{
			bPosition = pos;
			bDer1 = der1;
			bDer2 = der2;
			bDer3 = der3;
		}
	}

	protected int mNumCtrlPoints;

	protected Vector2d[] mCtrlPoint;

	protected double[] mCtrlWeight;

	protected bool mLoop;

	protected BSplineBasis mBasis;

	protected int mReplicate;

	protected bool is_closed;

	public bool IsClosed
	{
		get
		{
			return is_closed;
		}
		set
		{
			is_closed = value;
		}
	}

	public double ParamLength => mTMax - mTMin;

	public bool HasArcLength => true;

	public double ArcLength => GetTotalLength();

	public bool IsTransformable => true;

	public NURBSCurve2(int numCtrlPoints, Vector2d[] ctrlPoint, double[] ctrlWeight, int degree, bool loop, bool open)
		: base(0.0, 1.0)
	{
		if (numCtrlPoints < 2)
		{
			throw new Exception("NURBSCurve2(): only received " + numCtrlPoints + " control points!");
		}
		if (degree < 1 || degree > numCtrlPoints - 1)
		{
			throw new Exception("NURBSCurve2(): invalid degree " + degree);
		}
		mLoop = loop;
		mNumCtrlPoints = numCtrlPoints;
		mReplicate = (loop ? (open ? 1 : degree) : 0);
		CreateControl(ctrlPoint, ctrlWeight);
		mBasis = new BSplineBasis(mNumCtrlPoints + mReplicate, degree, open);
	}

	public NURBSCurve2(int numCtrlPoints, Vector2d[] ctrlPoint, double[] ctrlWeight, int degree, bool loop, double[] knot, bool bIsInteriorKnot = true)
		: base(0.0, 1.0)
	{
		if (numCtrlPoints < 2)
		{
			throw new Exception("NURBSCurve2(): only received " + numCtrlPoints + " control points!");
		}
		if (degree < 1 || degree > numCtrlPoints - 1)
		{
			throw new Exception("NURBSCurve2(): invalid degree " + degree);
		}
		if (loop)
		{
			throw new Exception("NURBSCUrve2(): loop mode is broken?");
		}
		mLoop = loop;
		mNumCtrlPoints = numCtrlPoints;
		mReplicate = (loop ? 1 : 0);
		CreateControl(ctrlPoint, ctrlWeight);
		mBasis = new BSplineBasis(mNumCtrlPoints + mReplicate, degree, knot, bIsInteriorKnot);
	}

	protected NURBSCurve2()
		: base(0.0, 1.0)
	{
	}

	public int GetNumCtrlPoints()
	{
		return mNumCtrlPoints;
	}

	public int GetDegree()
	{
		return mBasis.GetDegree();
	}

	public bool IsUniform()
	{
		return mBasis.IsUniform();
	}

	public void SetControlPoint(int i, Vector2d ctrl)
	{
		if (0 <= i && i < mNumCtrlPoints)
		{
			mCtrlPoint[i] = ctrl;
			if (i < mReplicate)
			{
				mCtrlPoint[mNumCtrlPoints + i] = ctrl;
			}
		}
	}

	public Vector2d GetControlPoint(int i)
	{
		if (0 <= i && i < mNumCtrlPoints)
		{
			return mCtrlPoint[i];
		}
		return new Vector2d(double.MaxValue, double.MaxValue);
	}

	public void SetControlWeight(int i, double weight)
	{
		if (0 <= i && i < mNumCtrlPoints)
		{
			mCtrlWeight[i] = weight;
			if (i < mReplicate)
			{
				mCtrlWeight[mNumCtrlPoints + i] = weight;
			}
		}
	}

	public double GetControlWeight(int i)
	{
		if (0 <= i && i < mNumCtrlPoints)
		{
			return mCtrlWeight[i];
		}
		return double.MaxValue;
	}

	public void SetKnot(int i, double value)
	{
		mBasis.SetInteriorKnot(i, value);
	}

	public double GetKnot(int i)
	{
		return mBasis.GetInteriorKnot(i);
	}

	public override Vector2d GetPosition(double t)
	{
		int minIndex = 0;
		int maxIndex = 0;
		mBasis.Compute(t, 0, ref minIndex, ref maxIndex);
		if (maxIndex >= mCtrlWeight.Length)
		{
			maxIndex = mCtrlWeight.Length - 1;
		}
		Vector2d zero = Vector2d.Zero;
		double num = 0.0;
		for (int i = minIndex; i <= maxIndex; i++)
		{
			double num2 = mBasis.GetD0(i) * mCtrlWeight[i];
			zero += num2 * mCtrlPoint[i];
			num += num2;
		}
		return 1.0 / num * zero;
	}

	public override Vector2d GetFirstDerivative(double t)
	{
		int minIndex = 0;
		int maxIndex = 0;
		mBasis.Compute(t, 0, ref minIndex, ref maxIndex);
		mBasis.Compute(t, 1, ref minIndex, ref maxIndex);
		if (maxIndex >= mCtrlWeight.Length)
		{
			maxIndex = mCtrlWeight.Length - 1;
		}
		Vector2d zero = Vector2d.Zero;
		double num = 0.0;
		for (int i = minIndex; i <= maxIndex; i++)
		{
			double num2 = mBasis.GetD0(i) * mCtrlWeight[i];
			zero += num2 * mCtrlPoint[i];
			num += num2;
		}
		double num3 = 1.0 / num;
		Vector2d vector2d = num3 * zero;
		Vector2d zero2 = Vector2d.Zero;
		double num4 = 0.0;
		for (int i = minIndex; i <= maxIndex; i++)
		{
			double num2 = mBasis.GetD1(i) * mCtrlWeight[i];
			zero2 += num2 * mCtrlPoint[i];
			num4 += num2;
		}
		return num3 * (zero2 - num4 * vector2d);
	}

	public override Vector2d GetSecondDerivative(double t)
	{
		CurveDerivatives result = default(CurveDerivatives);
		result.init(pos: false, der1: false, der2: true, der3: false);
		Get(t, ref result);
		return result.d2;
	}

	public override Vector2d GetThirdDerivative(double t)
	{
		CurveDerivatives result = default(CurveDerivatives);
		result.init(pos: false, der1: false, der2: false, der3: true);
		Get(t, ref result);
		return result.d3;
	}

	public void Get(double t, ref CurveDerivatives result)
	{
		int minIndex = 0;
		int maxIndex = 0;
		if (result.bDer3)
		{
			mBasis.Compute(t, 0, ref minIndex, ref maxIndex);
			mBasis.Compute(t, 1, ref minIndex, ref maxIndex);
			mBasis.Compute(t, 2, ref minIndex, ref maxIndex);
			mBasis.Compute(t, 3, ref minIndex, ref maxIndex);
		}
		else if (result.bDer2)
		{
			mBasis.Compute(t, 0, ref minIndex, ref maxIndex);
			mBasis.Compute(t, 1, ref minIndex, ref maxIndex);
			mBasis.Compute(t, 2, ref minIndex, ref maxIndex);
		}
		else if (result.bDer1)
		{
			mBasis.Compute(t, 0, ref minIndex, ref maxIndex);
			mBasis.Compute(t, 1, ref minIndex, ref maxIndex);
		}
		else
		{
			mBasis.Compute(t, 0, ref minIndex, ref maxIndex);
		}
		if (maxIndex >= mCtrlWeight.Length)
		{
			maxIndex = mCtrlWeight.Length - 1;
		}
		Vector2d zero = Vector2d.Zero;
		double num = 0.0;
		for (int i = minIndex; i <= maxIndex; i++)
		{
			double num2 = mBasis.GetD0(i) * mCtrlWeight[i];
			zero += num2 * mCtrlPoint[i];
			num += num2;
		}
		double num3 = 1.0 / num;
		Vector2d vector2d = (result.p = num3 * zero);
		result.bPosition = true;
		if (!result.bDer1 && !result.bDer2 && !result.bDer3)
		{
			return;
		}
		Vector2d zero2 = Vector2d.Zero;
		double num4 = 0.0;
		for (int i = minIndex; i <= maxIndex; i++)
		{
			double num2 = mBasis.GetD1(i) * mCtrlWeight[i];
			zero2 += num2 * mCtrlPoint[i];
			num4 += num2;
		}
		Vector2d vector2d2 = (result.d1 = num3 * (zero2 - num4 * vector2d));
		result.bDer1 = true;
		if (!result.bDer2 && !result.bDer3)
		{
			return;
		}
		Vector2d zero3 = Vector2d.Zero;
		double num5 = 0.0;
		for (int i = minIndex; i <= maxIndex; i++)
		{
			double num2 = mBasis.GetD2(i) * mCtrlWeight[i];
			zero3 += num2 * mCtrlPoint[i];
			num5 += num2;
		}
		Vector2d vector2d3 = (result.d2 = num3 * (zero3 - 2.0 * num4 * vector2d2 - num5 * vector2d));
		result.bDer2 = true;
		if (result.bDer3)
		{
			Vector2d zero4 = Vector2d.Zero;
			double num6 = 0.0;
			for (int i = minIndex; i <= maxIndex; i++)
			{
				double num2 = mBasis.GetD3(i) * mCtrlWeight[i];
				zero4 += num2 * mCtrlPoint[i];
				num6 += num2;
			}
			result.d3 = num3 * (zero4 - 3.0 * num4 * vector2d3 - 3.0 * num5 * vector2d2 - num6 * vector2d);
		}
	}

	public BSplineBasis GetBasis()
	{
		return mBasis;
	}

	protected void CreateControl(Vector2d[] ctrlPoint, double[] ctrlWeight)
	{
		int num = mNumCtrlPoints + mReplicate;
		mCtrlPoint = new Vector2d[num];
		Array.Copy(ctrlPoint, mCtrlPoint, mNumCtrlPoints);
		mCtrlWeight = new double[num];
		Array.Copy(ctrlWeight, mCtrlWeight, mNumCtrlPoints);
		for (int i = 0; i < mReplicate; i++)
		{
			mCtrlPoint[mNumCtrlPoints + i] = ctrlPoint[i];
			mCtrlWeight[mNumCtrlPoints + i] = ctrlWeight[i];
		}
	}

	public Vector2d SampleT(double t)
	{
		return GetPosition(t);
	}

	public Vector2d TangentT(double t)
	{
		return GetFirstDerivative(t).Normalized;
	}

	public Vector2d SampleArcLength(double a)
	{
		double time = GetTime(a);
		return GetPosition(time);
	}

	public void Reverse()
	{
		throw new NotSupportedException("NURBSCurve2.Reverse: how to reverse?!?");
	}

	public IParametricCurve2d Clone()
	{
		return new NURBSCurve2
		{
			mNumCtrlPoints = mNumCtrlPoints,
			mCtrlPoint = (Vector2d[])mCtrlPoint.Clone(),
			mCtrlWeight = (double[])mCtrlWeight.Clone(),
			mLoop = mLoop,
			mBasis = mBasis.Clone(),
			mReplicate = mReplicate,
			is_closed = is_closed
		};
	}

	public void Transform(ITransform2 xform)
	{
		for (int i = 0; i < mCtrlPoint.Length; i++)
		{
			mCtrlPoint[i] = xform.TransformP(mCtrlPoint[i]);
		}
	}

	public List<double> GetParamIntervals()
	{
		List<double> list = new List<double>();
		list.Add(0.0);
		for (int i = 0; i < mBasis.KnotCount; i++)
		{
			double knot = mBasis.GetKnot(i);
			if (knot != list.Last())
			{
				list.Add(knot);
			}
		}
		if (list.Last() != 1.0)
		{
			list.Add(1.0);
		}
		return list;
	}

	public List<double> GetContinuousParamIntervals()
	{
		List<double> list = new List<double>();
		double num = -1.0;
		int num2 = 0;
		for (int i = 0; i < mBasis.KnotCount; i++)
		{
			double knot = mBasis.GetKnot(i);
			if (knot == num)
			{
				num2++;
				continue;
			}
			if (num2 > 1)
			{
				list.Add(num);
			}
			num = knot;
			num2 = 1;
		}
		if (list.Last() != 1.0)
		{
			list.Add(1.0);
		}
		return list;
	}
}
