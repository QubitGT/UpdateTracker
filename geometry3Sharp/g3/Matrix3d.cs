using System;
using Unity.Mathematics;

namespace g3;

public struct Matrix3d
{
	public Vector3d Row0;

	public Vector3d Row1;

	public Vector3d Row2;

	public static readonly Matrix3d Identity = new Matrix3d(bIdentity: true);

	public static readonly Matrix3d Zero = new Matrix3d(bIdentity: false);

	public double this[int r, int c]
	{
		get
		{
			return r switch
			{
				1 => Row1[c], 
				0 => Row0[c], 
				_ => Row2[c], 
			};
		}
		set
		{
			switch (r)
			{
			case 0:
				Row0[c] = value;
				break;
			case 1:
				Row1[c] = value;
				break;
			default:
				Row2[c] = value;
				break;
			}
		}
	}

	public double this[int i]
	{
		get
		{
			if (i <= 5)
			{
				if (i <= 2)
				{
					return Row0[i % 3];
				}
				return Row1[i % 3];
			}
			return Row2[i % 3];
		}
		set
		{
			if (i > 5)
			{
				Row2[i % 3] = value;
			}
			else if (i > 2)
			{
				Row1[i % 3] = value;
			}
			else
			{
				Row0[i % 3] = value;
			}
		}
	}

	public double Determinant
	{
		get
		{
			double x = Row0.x;
			double y = Row0.y;
			double z = Row0.z;
			double x2 = Row1.x;
			double y2 = Row1.y;
			double z2 = Row1.z;
			double x3 = Row2.x;
			double y3 = Row2.y;
			double z3 = Row2.z;
			double num = z3 * y2 - y3 * z2;
			double num2 = 0.0 - (z3 * y - y3 * z);
			double num3 = z2 * y - y2 * z;
			return x * num + x2 * num2 + x3 * num3;
		}
	}

	public Matrix3d(bool bIdentity)
	{
		if (bIdentity)
		{
			Row0 = Vector3d.AxisX;
			Row1 = Vector3d.AxisY;
			Row2 = Vector3d.AxisZ;
		}
		else
		{
			Row0 = (Row1 = (Row2 = Vector3d.Zero));
		}
	}

	public Matrix3d(float[,] mat)
	{
		Row0 = new Vector3d(mat[0, 0], mat[0, 1], mat[0, 2]);
		Row1 = new Vector3d(mat[1, 0], mat[1, 1], mat[1, 2]);
		Row2 = new Vector3d(mat[2, 0], mat[2, 1], mat[2, 2]);
	}

	public Matrix3d(float[] mat)
	{
		Row0 = new Vector3d(mat[0], mat[1], mat[2]);
		Row1 = new Vector3d(mat[3], mat[4], mat[5]);
		Row2 = new Vector3d(mat[6], mat[7], mat[8]);
	}

	public Matrix3d(double[,] mat)
	{
		Row0 = new Vector3d(mat[0, 0], mat[0, 1], mat[0, 2]);
		Row1 = new Vector3d(mat[1, 0], mat[1, 1], mat[1, 2]);
		Row2 = new Vector3d(mat[2, 0], mat[2, 1], mat[2, 2]);
	}

	public Matrix3d(double[] mat)
	{
		Row0 = new Vector3d(mat[0], mat[1], mat[2]);
		Row1 = new Vector3d(mat[3], mat[4], mat[5]);
		Row2 = new Vector3d(mat[6], mat[7], mat[8]);
	}

	public Matrix3d(Func<int, double> matBufferF)
	{
		Row0 = new Vector3d(matBufferF(0), matBufferF(1), matBufferF(2));
		Row1 = new Vector3d(matBufferF(3), matBufferF(4), matBufferF(5));
		Row2 = new Vector3d(matBufferF(6), matBufferF(7), matBufferF(8));
	}

	public Matrix3d(Func<int, int, double> matF)
	{
		Row0 = new Vector3d(matF(0, 0), matF(0, 1), matF(0, 2));
		Row1 = new Vector3d(matF(1, 0), matF(1, 1), matF(1, 2));
		Row2 = new Vector3d(matF(2, 0), matF(1, 2), matF(2, 2));
	}

	public Matrix3d(double m00, double m11, double m22)
	{
		Row0 = new Vector3d(m00, 0.0, 0.0);
		Row1 = new Vector3d(0.0, m11, 0.0);
		Row2 = new Vector3d(0.0, 0.0, m22);
	}

	public Matrix3d(Vector3d v1, Vector3d v2, Vector3d v3, bool bRows)
	{
		if (bRows)
		{
			Row0 = v1;
			Row1 = v2;
			Row2 = v3;
		}
		else
		{
			Row0 = new Vector3d(v1.x, v2.x, v3.x);
			Row1 = new Vector3d(v1.y, v2.y, v3.y);
			Row2 = new Vector3d(v1.z, v2.z, v3.z);
		}
	}

	public Matrix3d(ref Vector3d v1, ref Vector3d v2, ref Vector3d v3, bool bRows)
	{
		if (bRows)
		{
			Row0 = v1;
			Row1 = v2;
			Row2 = v3;
		}
		else
		{
			Row0 = new Vector3d(v1.x, v2.x, v3.x);
			Row1 = new Vector3d(v1.y, v2.y, v3.y);
			Row2 = new Vector3d(v1.z, v2.z, v3.z);
		}
	}

	public Matrix3d(double m00, double m01, double m02, double m10, double m11, double m12, double m20, double m21, double m22)
	{
		Row0 = new Vector3d(m00, m01, m02);
		Row1 = new Vector3d(m10, m11, m12);
		Row2 = new Vector3d(m20, m21, m22);
	}

	public Matrix3d(ref Vector3d u, ref Vector3d v)
	{
		Row0 = new Vector3d(u.x * v.x, u.x * v.y, u.x * v.z);
		Row1 = new Vector3d(u.y * v.x, u.y * v.y, u.y * v.z);
		Row2 = new Vector3d(u.z * v.x, u.z * v.y, u.z * v.z);
	}

	public Vector3d Row(int i)
	{
		return i switch
		{
			1 => Row1, 
			0 => Row0, 
			_ => Row2, 
		};
	}

	public Vector3d Column(int i)
	{
		return i switch
		{
			0 => new Vector3d(Row0.x, Row1.x, Row2.x), 
			1 => new Vector3d(Row0.y, Row1.y, Row2.y), 
			_ => new Vector3d(Row0.z, Row1.z, Row2.z), 
		};
	}

	public double[] ToBuffer()
	{
		return new double[9] { Row0.x, Row0.y, Row0.z, Row1.x, Row1.y, Row1.z, Row2.x, Row2.y, Row2.z };
	}

	public void ToBuffer(double[] buf)
	{
		buf[0] = Row0.x;
		buf[1] = Row0.y;
		buf[2] = Row0.z;
		buf[3] = Row1.x;
		buf[4] = Row1.y;
		buf[5] = Row1.z;
		buf[6] = Row2.x;
		buf[7] = Row2.y;
		buf[8] = Row2.z;
	}

	public static Matrix3d operator *(Matrix3d mat, double f)
	{
		return new Matrix3d(mat.Row0.x * f, mat.Row0.y * f, mat.Row0.z * f, mat.Row1.x * f, mat.Row1.y * f, mat.Row1.z * f, mat.Row2.x * f, mat.Row2.y * f, mat.Row2.z * f);
	}

	public static Matrix3d operator *(double f, Matrix3d mat)
	{
		return new Matrix3d(mat.Row0.x * f, mat.Row0.y * f, mat.Row0.z * f, mat.Row1.x * f, mat.Row1.y * f, mat.Row1.z * f, mat.Row2.x * f, mat.Row2.y * f, mat.Row2.z * f);
	}

	public static Vector3d operator *(Matrix3d mat, Vector3d v)
	{
		return new Vector3d(mat.Row0.x * v.x + mat.Row0.y * v.y + mat.Row0.z * v.z, mat.Row1.x * v.x + mat.Row1.y * v.y + mat.Row1.z * v.z, mat.Row2.x * v.x + mat.Row2.y * v.y + mat.Row2.z * v.z);
	}

	public Vector3d Multiply(ref Vector3d v)
	{
		return new Vector3d(Row0.x * v.x + Row0.y * v.y + Row0.z * v.z, Row1.x * v.x + Row1.y * v.y + Row1.z * v.z, Row2.x * v.x + Row2.y * v.y + Row2.z * v.z);
	}

	public void Multiply(ref Vector3d v, ref Vector3d vOut)
	{
		vOut.x = Row0.x * v.x + Row0.y * v.y + Row0.z * v.z;
		vOut.y = Row1.x * v.x + Row1.y * v.y + Row1.z * v.z;
		vOut.z = Row2.x * v.x + Row2.y * v.y + Row2.z * v.z;
	}

	public static Matrix3d operator *(Matrix3d mat1, Matrix3d mat2)
	{
		double m = mat1.Row0.x * mat2.Row0.x + mat1.Row0.y * mat2.Row1.x + mat1.Row0.z * mat2.Row2.x;
		double m2 = mat1.Row0.x * mat2.Row0.y + mat1.Row0.y * mat2.Row1.y + mat1.Row0.z * mat2.Row2.y;
		double m3 = mat1.Row0.x * mat2.Row0.z + mat1.Row0.y * mat2.Row1.z + mat1.Row0.z * mat2.Row2.z;
		double m4 = mat1.Row1.x * mat2.Row0.x + mat1.Row1.y * mat2.Row1.x + mat1.Row1.z * mat2.Row2.x;
		double m5 = mat1.Row1.x * mat2.Row0.y + mat1.Row1.y * mat2.Row1.y + mat1.Row1.z * mat2.Row2.y;
		double m6 = mat1.Row1.x * mat2.Row0.z + mat1.Row1.y * mat2.Row1.z + mat1.Row1.z * mat2.Row2.z;
		double m7 = mat1.Row2.x * mat2.Row0.x + mat1.Row2.y * mat2.Row1.x + mat1.Row2.z * mat2.Row2.x;
		double m8 = mat1.Row2.x * mat2.Row0.y + mat1.Row2.y * mat2.Row1.y + mat1.Row2.z * mat2.Row2.y;
		double m9 = mat1.Row2.x * mat2.Row0.z + mat1.Row2.y * mat2.Row1.z + mat1.Row2.z * mat2.Row2.z;
		return new Matrix3d(m, m2, m3, m4, m5, m6, m7, m8, m9);
	}

	public static Matrix3d operator +(Matrix3d mat1, Matrix3d mat2)
	{
		return new Matrix3d(mat1.Row0 + mat2.Row0, mat1.Row1 + mat2.Row1, mat1.Row2 + mat2.Row2, bRows: true);
	}

	public static Matrix3d operator -(Matrix3d mat1, Matrix3d mat2)
	{
		return new Matrix3d(mat1.Row0 - mat2.Row0, mat1.Row1 - mat2.Row1, mat1.Row2 - mat2.Row2, bRows: true);
	}

	public double InnerProduct(ref Matrix3d m2)
	{
		return Row0.Dot(ref m2.Row0) + Row1.Dot(ref m2.Row1) + Row2.Dot(ref m2.Row2);
	}

	public Matrix3d Inverse()
	{
		double x = Row0.x;
		double y = Row0.y;
		double z = Row0.z;
		double x2 = Row1.x;
		double y2 = Row1.y;
		double z2 = Row1.z;
		double x3 = Row2.x;
		double y3 = Row2.y;
		double z3 = Row2.z;
		double num = z3 * y2 - y3 * z2;
		double num2 = 0.0 - (z3 * y - y3 * z);
		double num3 = z2 * y - y2 * z;
		double num4 = 0.0 - (z3 * x2 - x3 * z2);
		double num5 = z3 * x - x3 * z;
		double num6 = 0.0 - (z2 * x - x2 * z);
		double num7 = y3 * x2 - x3 * y2;
		double num8 = 0.0 - (y3 * x - x3 * y);
		double num9 = y2 * x - x2 * y;
		double num10 = x * num + x2 * num2 + x3 * num3;
		if (Math.Abs(num10) < double.Epsilon)
		{
			throw new Exception("Matrix3d.Inverse: matrix is not invertible");
		}
		num10 = 1.0 / num10;
		return new Matrix3d(num * num10, num2 * num10, num3 * num10, num4 * num10, num5 * num10, num6 * num10, num7 * num10, num8 * num10, num9 * num10);
	}

	public Matrix3d Transpose()
	{
		return new Matrix3d(Row0.x, Row1.x, Row2.x, Row0.y, Row1.y, Row2.y, Row0.z, Row1.z, Row2.z);
	}

	public Quaterniond ToQuaternion()
	{
		return new Quaterniond(this);
	}

	public bool EpsilonEqual(Matrix3d m2, double epsilon)
	{
		if (Row0.EpsilonEqual(m2.Row0, epsilon) && Row1.EpsilonEqual(m2.Row1, epsilon))
		{
			return Row2.EpsilonEqual(m2.Row2, epsilon);
		}
		return false;
	}

	public static Matrix3d AxisAngleD(Vector3d axis, double angleDeg)
	{
		double num = angleDeg * (Math.PI / 180.0);
		double num2 = Math.Cos(num);
		double num3 = Math.Sin(num);
		double num4 = 1.0 - num2;
		double num5 = axis[0] * axis[0];
		double num6 = axis[1] * axis[1];
		double num7 = axis[2] * axis[2];
		double num8 = axis[0] * axis[1] * num4;
		double num9 = axis[0] * axis[2] * num4;
		double num10 = axis[1] * axis[2] * num4;
		double num11 = axis[0] * num3;
		double num12 = axis[1] * num3;
		double num13 = axis[2] * num3;
		return new Matrix3d(num5 * num4 + num2, num8 - num13, num9 + num12, num8 + num13, num6 * num4 + num2, num10 - num11, num9 - num12, num10 + num11, num7 * num4 + num2);
	}

	public override string ToString()
	{
		return $"[{Row0}] [{Row1}] [{Row2}]";
	}

	public string ToString(string fmt)
	{
		return $"[{Row0.ToString(fmt)}] [{Row1.ToString(fmt)}] [{Row2.ToString(fmt)}]";
	}

	public static implicit operator Matrix3d(double3x3 m)
	{
		return new Matrix3d(m.c0, m.c1, m.c2, bRows: false);
	}

	public static explicit operator double3x3(Matrix3d m)
	{
		return new double3x3(m.Column(0), m.Column(1), m.Column(3));
	}
}
