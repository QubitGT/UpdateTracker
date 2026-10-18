using System;

namespace g3;

public class Ellipse2d : IParametricCurve2d
{
	public Vector2d Center;

	public Vector2d Axis0;

	public Vector2d Axis1;

	public Vector2d Extent;

	public bool IsReversed;

	public bool IsClosed => true;

	public bool IsTransformable => true;

	public double ParamLength => 1.0;

	public bool HasArcLength => false;

	public double ArcLength
	{
		get
		{
			throw new NotImplementedException("Ellipse2.ArcLength");
		}
	}

	public double Area => Math.PI * Extent.x * Extent.y;

	public double ApproxArcLen
	{
		get
		{
			double num = Math.Max(Extent.x, Extent.y);
			double num2 = Math.Min(Extent.x, Extent.y);
			double num3 = (num - num2) / (num + num2);
			double num4 = 3.0 * num3 * num3;
			double num5 = 10.0 + Math.Sqrt(4.0 - num4);
			return Math.PI * (num + num2) * (1.0 + num4 / num5);
		}
	}

	public Ellipse2d(Vector2d center, Vector2d axis0, Vector2d axis1, Vector2d extent)
	{
		Center = center;
		Axis0 = axis0;
		Axis1 = axis1;
		Extent.x = extent.x;
		Extent.y = extent.y;
		IsReversed = false;
	}

	public Ellipse2d(Vector2d center, Vector2d axis0, Vector2d axis1, double extent0, double extent1)
	{
		Center = center;
		Axis0 = axis0;
		Axis1 = axis1;
		Extent.x = extent0;
		Extent.y = extent1;
		IsReversed = false;
	}

	public Ellipse2d(Vector2d center, double rotationAngleDeg, double extent0, double extent1)
	{
		Center = center;
		Matrix2d matrix2d = new Matrix2d(rotationAngleDeg * (Math.PI / 180.0));
		Axis0 = matrix2d * Vector2d.AxisX;
		Axis1 = matrix2d * Vector2d.AxisY;
		Extent = new Vector2d(extent0, extent1);
		IsReversed = false;
	}

	public Matrix2d GetM()
	{
		Vector2d vector2d = Axis0 / Extent[0];
		Vector2d vector2d2 = Axis1 / Extent[1];
		return new Matrix2d(vector2d, vector2d) + new Matrix2d(vector2d2, vector2d2);
	}

	public Matrix2d GetMInverse()
	{
		Vector2d vector2d = Axis0 * Extent[0];
		Vector2d vector2d2 = Axis1 * Extent[1];
		return new Matrix2d(vector2d, vector2d) + new Matrix2d(vector2d2, vector2d2);
	}

	public double[] ToCoefficients()
	{
		Matrix2d A = Matrix2d.Zero;
		Vector2d B = Vector2d.Zero;
		double C = 0.0;
		ToCoefficients(ref A, ref B, ref C);
		double[] array = Convert(A, B, C);
		double num = Math.Abs(array[3]);
		int num2 = 3;
		double num3 = Math.Abs(array[5]);
		if (num3 > num)
		{
			num = num3;
			num2 = 5;
		}
		double num4 = 1.0 / num;
		for (int i = 0; i < 6; i++)
		{
			if (i != num2)
			{
				array[i] *= num4;
			}
			else
			{
				array[i] = 1.0;
			}
		}
		return array;
	}

	public void ToCoefficients(ref Matrix2d A, ref Vector2d B, ref double C)
	{
		Vector2d vector2d = Axis0 / Extent[0];
		Vector2d vector2d2 = Axis1 / Extent[1];
		A = new Matrix2d(vector2d, vector2d) + new Matrix2d(vector2d2, vector2d2);
		B = -2.0 * (A * Center);
		C = A.QForm(Center, Center) - 1.0;
	}

	public bool FromCoefficients(double[] coeff)
	{
		Matrix2d A = Matrix2d.Zero;
		Vector2d B = Vector2d.Zero;
		double C = 0.0;
		Convert(coeff, ref A, ref B, ref C);
		return FromCoefficients(A, B, C);
	}

	public bool FromCoefficients(Matrix2d A, Vector2d B, double C)
	{
		throw new NotImplementedException("Ellipse2.FromCoefficients: need EigenDecomposition");
	}

	public double Evaluate(Vector2d point)
	{
		Vector2d v = point - Center;
		double num = Axis0.Dot(v) / Extent[0];
		double num2 = Axis1.Dot(v) / Extent[1];
		return num * num + num2 * num2 - 1.0;
	}

	public bool Contains(Vector2d point)
	{
		return Evaluate(point) <= 0.0;
	}

	private static void Convert(double[] coeff, ref Matrix2d A, ref Vector2d B, ref double C)
	{
		C = coeff[0];
		B.x = coeff[1];
		B.y = coeff[2];
		A.m00 = coeff[3];
		A.m01 = 0.5 * coeff[4];
		A.m10 = A.m01;
		A.m11 = coeff[5];
	}

	private static double[] Convert(Matrix2d A, Vector2d B, double C)
	{
		return new double[6]
		{
			C,
			B.x,
			B.y,
			A.m00,
			2.0 * A.m01,
			A.m11
		};
	}

	public void Reverse()
	{
		IsReversed = !IsReversed;
	}

	public IParametricCurve2d Clone()
	{
		return new Ellipse2d(Center, Axis0, Axis1, Extent)
		{
			IsReversed = IsReversed
		};
	}

	public void Transform(ITransform2 xform)
	{
		Center = xform.TransformP(Center);
		Axis0 = xform.TransformN(Axis0);
		Axis1 = xform.TransformN(Axis1);
		Extent.x = xform.TransformScalar(Extent.x);
		Extent.y = xform.TransformScalar(Extent.y);
	}

	public Vector2d SampleDeg(double degrees)
	{
		double num = degrees * (Math.PI / 180.0);
		double num2 = Math.Cos(num);
		double num3 = Math.Sin(num);
		return Center + Extent.x * num2 * Axis0 + Extent.y * num3 * Axis1;
	}

	public Vector2d SampleRad(double radians)
	{
		double num = Math.Cos(radians);
		double num2 = Math.Sin(radians);
		return Center + Extent.x * num * Axis0 + Extent.y * num2 * Axis1;
	}

	public Vector2d SampleT(double t)
	{
		double num = (IsReversed ? ((0.0 - t) * (Math.PI * 2.0)) : (t * (Math.PI * 2.0)));
		double num2 = Math.Cos(num);
		double num3 = Math.Sin(num);
		return Center + Extent.x * num2 * Axis0 + Extent.y * num3 * Axis1;
	}

	public Vector2d TangentT(double t)
	{
		double num = (IsReversed ? ((0.0 - t) * (Math.PI * 2.0)) : (t * (Math.PI * 2.0)));
		double num2 = Math.Cos(num);
		double num3 = Math.Sin(num);
		Vector2d vector2d = (0.0 - Extent.x) * num3 * Axis0 + Extent.y * num2 * Axis1;
		if (IsReversed)
		{
			vector2d = -vector2d;
		}
		vector2d.Normalize();
		return vector2d;
	}

	public Vector2d SampleArcLength(double a)
	{
		throw new NotImplementedException("Ellipse2.SampleArcLength");
	}
}
