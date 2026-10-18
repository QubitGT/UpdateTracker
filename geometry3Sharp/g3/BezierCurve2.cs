using System;

namespace g3;

public class BezierCurve2 : BaseCurve2, IParametricCurve2d
{
	private int mDegree;

	private int mNumCtrlPoints;

	private Vector2d[] mCtrlPoint;

	private Vector2d[] mDer1CtrlPoint;

	private Vector2d[] mDer2CtrlPoint;

	private Vector2d[] mDer3CtrlPoint;

	private DenseMatrix mChoose;

	public int Degree => mDegree;

	public Vector2d[] ControlPoints => mCtrlPoint;

	public bool IsClosed => false;

	public double ParamLength => mTMax - mTMin;

	public bool HasArcLength => true;

	public double ArcLength => GetTotalLength();

	public bool IsTransformable => true;

	public BezierCurve2(int degree, Vector2d[] ctrlPoint, bool bTakeOwnership = false)
		: base(0.0, 1.0)
	{
		if (degree < 2)
		{
			throw new Exception("BezierCurve2() The degree must be three or larger\n");
		}
		mDegree = degree;
		mNumCtrlPoints = mDegree + 1;
		if (bTakeOwnership)
		{
			mCtrlPoint = ctrlPoint;
		}
		else
		{
			mCtrlPoint = new Vector2d[ctrlPoint.Length];
			Array.Copy(ctrlPoint, mCtrlPoint, ctrlPoint.Length);
		}
		mDer1CtrlPoint = new Vector2d[mNumCtrlPoints - 1];
		for (int i = 0; i < mNumCtrlPoints - 1; i++)
		{
			mDer1CtrlPoint[i] = mCtrlPoint[i + 1] - mCtrlPoint[i];
		}
		mDer2CtrlPoint = new Vector2d[mNumCtrlPoints - 2];
		for (int i = 0; i < mNumCtrlPoints - 2; i++)
		{
			mDer2CtrlPoint[i] = mDer1CtrlPoint[i + 1] - mDer1CtrlPoint[i];
		}
		if (degree >= 3)
		{
			mDer3CtrlPoint = new Vector2d[mNumCtrlPoints - 3];
			for (int i = 0; i < mNumCtrlPoints - 3; i++)
			{
				mDer3CtrlPoint[i] = mDer2CtrlPoint[i + 1] - mDer2CtrlPoint[i];
			}
		}
		else
		{
			mDer3CtrlPoint = null;
		}
		mChoose = new DenseMatrix(mNumCtrlPoints, mNumCtrlPoints);
		mChoose[0, 0] = 1.0;
		mChoose[1, 0] = 1.0;
		mChoose[1, 1] = 1.0;
		for (int i = 2; i <= mDegree; i++)
		{
			mChoose[i, 0] = 1.0;
			mChoose[i, i] = 1.0;
			for (int j = 1; j < i; j++)
			{
				mChoose[i, j] = mChoose[i - 1, j - 1] + mChoose[i - 1, j];
			}
		}
	}

	protected BezierCurve2()
		: base(0.0, 1.0)
	{
	}

	public override Vector2d GetPosition(double t)
	{
		double num = 1.0 - t;
		double num2 = t;
		Vector2d vector2d = num * mCtrlPoint[0];
		for (int i = 1; i < mDegree; i++)
		{
			double num3 = mChoose[mDegree, i] * num2;
			vector2d = (vector2d + mCtrlPoint[i] * num3) * num;
			num2 *= t;
		}
		return vector2d + mCtrlPoint[mDegree] * num2;
	}

	public override Vector2d GetFirstDerivative(double t)
	{
		double num = 1.0 - t;
		double num2 = t;
		Vector2d vector2d = num * mDer1CtrlPoint[0];
		int num3 = mDegree - 1;
		for (int i = 1; i < num3; i++)
		{
			double num4 = mChoose[num3, i] * num2;
			vector2d = (vector2d + mDer1CtrlPoint[i] * num4) * num;
			num2 *= t;
		}
		vector2d += mDer1CtrlPoint[num3] * num2;
		return vector2d * mDegree;
	}

	public override Vector2d GetSecondDerivative(double t)
	{
		double num = 1.0 - t;
		double num2 = t;
		Vector2d vector2d = num * mDer2CtrlPoint[0];
		int num3 = mDegree - 2;
		for (int i = 1; i < num3; i++)
		{
			double num4 = mChoose[num3, i] * num2;
			vector2d = (vector2d + mDer2CtrlPoint[i] * num4) * num;
			num2 *= t;
		}
		vector2d += mDer2CtrlPoint[num3] * num2;
		return vector2d * (mDegree * (mDegree - 1));
	}

	public override Vector2d GetThirdDerivative(double t)
	{
		if (mDegree < 3)
		{
			return Vector2d.Zero;
		}
		double num = 1.0 - t;
		double num2 = t;
		Vector2d vector2d = num * mDer3CtrlPoint[0];
		int num3 = mDegree - 3;
		for (int i = 1; i < num3; i++)
		{
			double num4 = mChoose[num3, i] * num2;
			vector2d = (vector2d + mDer3CtrlPoint[i] * num4) * num;
			num2 *= t;
		}
		vector2d += mDer3CtrlPoint[num3] * num2;
		return vector2d * (mDegree * (mDegree - 1) * (mDegree - 2));
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
		return new BezierCurve2
		{
			mDegree = mDegree,
			mNumCtrlPoints = mNumCtrlPoints,
			mCtrlPoint = (Vector2d[])mCtrlPoint.Clone(),
			mDer1CtrlPoint = (Vector2d[])mDer1CtrlPoint.Clone(),
			mDer2CtrlPoint = (Vector2d[])mDer2CtrlPoint.Clone(),
			mDer3CtrlPoint = (Vector2d[])mDer3CtrlPoint.Clone(),
			mChoose = new DenseMatrix(mChoose)
		};
	}

	public void Transform(ITransform2 xform)
	{
		for (int i = 0; i < mCtrlPoint.Length; i++)
		{
			mCtrlPoint[i] = xform.TransformP(mCtrlPoint[i]);
		}
		for (int j = 0; j < mNumCtrlPoints - 1; j++)
		{
			mDer1CtrlPoint[j] = mCtrlPoint[j + 1] - mCtrlPoint[j];
		}
		for (int k = 0; k < mNumCtrlPoints - 2; k++)
		{
			mDer2CtrlPoint[k] = mDer1CtrlPoint[k + 1] - mDer1CtrlPoint[k];
		}
		if (mDegree >= 3)
		{
			for (int l = 0; l < mNumCtrlPoints - 3; l++)
			{
				mDer3CtrlPoint[l] = mDer2CtrlPoint[l + 1] - mDer2CtrlPoint[l];
			}
		}
	}
}
