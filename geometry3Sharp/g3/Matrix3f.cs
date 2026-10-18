using System;
using Unity.Mathematics;

namespace g3;

public struct Matrix3f
{
	public Vector3f Row0;

	public Vector3f Row1;

	public Vector3f Row2;

	public static readonly Matrix3f Identity = new Matrix3f(bIdentity: true);

	public static readonly Matrix3f Zero = new Matrix3f(bIdentity: false);

	public float this[int r, int c]
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

	public float this[int i]
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

	public float Determinant
	{
		get
		{
			float x = Row0.x;
			float y = Row0.y;
			float z = Row0.z;
			float x2 = Row1.x;
			float y2 = Row1.y;
			float z2 = Row1.z;
			float x3 = Row2.x;
			float y3 = Row2.y;
			float z3 = Row2.z;
			float num = z3 * y2 - y3 * z2;
			float num2 = 0f - (z3 * y - y3 * z);
			float num3 = z2 * y - y2 * z;
			return x * num + x2 * num2 + x3 * num3;
		}
	}

	public Matrix3f(bool bIdentity)
	{
		if (bIdentity)
		{
			Row0 = Vector3f.AxisX;
			Row1 = Vector3f.AxisY;
			Row2 = Vector3f.AxisZ;
		}
		else
		{
			Row0 = (Row1 = (Row2 = Vector3f.Zero));
		}
	}

	public Matrix3f(float[,] mat)
	{
		Row0 = new Vector3f(mat[0, 0], mat[0, 1], mat[0, 2]);
		Row1 = new Vector3f(mat[1, 0], mat[1, 1], mat[1, 2]);
		Row2 = new Vector3f(mat[2, 0], mat[2, 1], mat[2, 2]);
	}

	public Matrix3f(float[] mat)
	{
		Row0 = new Vector3f(mat[0], mat[1], mat[2]);
		Row1 = new Vector3f(mat[3], mat[4], mat[5]);
		Row2 = new Vector3f(mat[6], mat[7], mat[8]);
	}

	public Matrix3f(double[,] mat)
	{
		Row0 = new Vector3f(mat[0, 0], mat[0, 1], mat[0, 2]);
		Row1 = new Vector3f(mat[1, 0], mat[1, 1], mat[1, 2]);
		Row2 = new Vector3f(mat[2, 0], mat[2, 1], mat[2, 2]);
	}

	public Matrix3f(double[] mat)
	{
		Row0 = new Vector3f(mat[0], mat[1], mat[2]);
		Row1 = new Vector3f(mat[3], mat[4], mat[5]);
		Row2 = new Vector3f(mat[6], mat[7], mat[8]);
	}

	public Matrix3f(Func<int, float> matBufferF)
	{
		Row0 = new Vector3f(matBufferF(0), matBufferF(1), matBufferF(2));
		Row1 = new Vector3f(matBufferF(3), matBufferF(4), matBufferF(5));
		Row2 = new Vector3f(matBufferF(6), matBufferF(7), matBufferF(8));
	}

	public Matrix3f(Func<int, int, float> matF)
	{
		Row0 = new Vector3f(matF(0, 0), matF(0, 1), matF(0, 2));
		Row1 = new Vector3f(matF(1, 0), matF(1, 1), matF(1, 2));
		Row2 = new Vector3f(matF(2, 0), matF(1, 2), matF(2, 2));
	}

	public Matrix3f(float m00, float m11, float m22)
	{
		Row0 = new Vector3f(m00, 0f, 0f);
		Row1 = new Vector3f(0f, m11, 0f);
		Row2 = new Vector3f(0f, 0f, m22);
	}

	public Matrix3f(Vector3f v1, Vector3f v2, Vector3f v3, bool bRows)
	{
		if (bRows)
		{
			Row0 = v1;
			Row1 = v2;
			Row2 = v3;
		}
		else
		{
			Row0 = new Vector3f(v1.x, v2.x, v3.x);
			Row1 = new Vector3f(v1.y, v2.y, v3.y);
			Row2 = new Vector3f(v1.z, v2.z, v3.z);
		}
	}

	public Matrix3f(float m00, float m01, float m02, float m10, float m11, float m12, float m20, float m21, float m22)
	{
		Row0 = new Vector3f(m00, m01, m02);
		Row1 = new Vector3f(m10, m11, m12);
		Row2 = new Vector3f(m20, m21, m22);
	}

	public Vector3f Row(int i)
	{
		return i switch
		{
			1 => Row1, 
			0 => Row0, 
			_ => Row2, 
		};
	}

	public Vector3f Column(int i)
	{
		return i switch
		{
			0 => new Vector3f(Row0.x, Row1.x, Row2.x), 
			1 => new Vector3f(Row0.y, Row1.y, Row2.y), 
			_ => new Vector3f(Row0.z, Row1.z, Row2.z), 
		};
	}

	public float[] ToBuffer()
	{
		return new float[9] { Row0.x, Row0.y, Row0.z, Row1.x, Row1.y, Row1.z, Row2.x, Row2.y, Row2.z };
	}

	public void ToBuffer(float[] buf)
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

	public static Matrix3f operator *(Matrix3f mat, float f)
	{
		return new Matrix3f(mat.Row0.x * f, mat.Row0.y * f, mat.Row0.z * f, mat.Row1.x * f, mat.Row1.y * f, mat.Row1.z * f, mat.Row2.x * f, mat.Row2.y * f, mat.Row2.z * f);
	}

	public static Matrix3f operator *(float f, Matrix3f mat)
	{
		return new Matrix3f(mat.Row0.x * f, mat.Row0.y * f, mat.Row0.z * f, mat.Row1.x * f, mat.Row1.y * f, mat.Row1.z * f, mat.Row2.x * f, mat.Row2.y * f, mat.Row2.z * f);
	}

	public static Vector3f operator *(Matrix3f mat, Vector3f v)
	{
		return new Vector3f(mat.Row0.x * v.x + mat.Row0.y * v.y + mat.Row0.z * v.z, mat.Row1.x * v.x + mat.Row1.y * v.y + mat.Row1.z * v.z, mat.Row2.x * v.x + mat.Row2.y * v.y + mat.Row2.z * v.z);
	}

	public Vector3f Multiply(ref Vector3f v)
	{
		return new Vector3f(Row0.x * v.x + Row0.y * v.y + Row0.z * v.z, Row1.x * v.x + Row1.y * v.y + Row1.z * v.z, Row2.x * v.x + Row2.y * v.y + Row2.z * v.z);
	}

	public void Multiply(ref Vector3f v, ref Vector3f vOut)
	{
		vOut.x = Row0.x * v.x + Row0.y * v.y + Row0.z * v.z;
		vOut.y = Row1.x * v.x + Row1.y * v.y + Row1.z * v.z;
		vOut.z = Row2.x * v.x + Row2.y * v.y + Row2.z * v.z;
	}

	public static Matrix3f operator *(Matrix3f mat1, Matrix3f mat2)
	{
		float m = mat1.Row0.x * mat2.Row0.x + mat1.Row0.y * mat2.Row1.x + mat1.Row0.z * mat2.Row2.x;
		float m2 = mat1.Row0.x * mat2.Row0.y + mat1.Row0.y * mat2.Row1.y + mat1.Row0.z * mat2.Row2.y;
		float m3 = mat1.Row0.x * mat2.Row0.z + mat1.Row0.y * mat2.Row1.z + mat1.Row0.z * mat2.Row2.z;
		float m4 = mat1.Row1.x * mat2.Row0.x + mat1.Row1.y * mat2.Row1.x + mat1.Row1.z * mat2.Row2.x;
		float m5 = mat1.Row1.x * mat2.Row0.y + mat1.Row1.y * mat2.Row1.y + mat1.Row1.z * mat2.Row2.y;
		float m6 = mat1.Row1.x * mat2.Row0.z + mat1.Row1.y * mat2.Row1.z + mat1.Row1.z * mat2.Row2.z;
		float m7 = mat1.Row2.x * mat2.Row0.x + mat1.Row2.y * mat2.Row1.x + mat1.Row2.z * mat2.Row2.x;
		float m8 = mat1.Row2.x * mat2.Row0.y + mat1.Row2.y * mat2.Row1.y + mat1.Row2.z * mat2.Row2.y;
		float m9 = mat1.Row2.x * mat2.Row0.z + mat1.Row2.y * mat2.Row1.z + mat1.Row2.z * mat2.Row2.z;
		return new Matrix3f(m, m2, m3, m4, m5, m6, m7, m8, m9);
	}

	public static Matrix3f operator +(Matrix3f mat1, Matrix3f mat2)
	{
		return new Matrix3f(mat1.Row0 + mat2.Row0, mat1.Row1 + mat2.Row1, mat1.Row2 + mat2.Row2, bRows: true);
	}

	public static Matrix3f operator -(Matrix3f mat1, Matrix3f mat2)
	{
		return new Matrix3f(mat1.Row0 - mat2.Row0, mat1.Row1 - mat2.Row1, mat1.Row2 - mat2.Row2, bRows: true);
	}

	public Matrix3f Inverse()
	{
		float x = Row0.x;
		float y = Row0.y;
		float z = Row0.z;
		float x2 = Row1.x;
		float y2 = Row1.y;
		float z2 = Row1.z;
		float x3 = Row2.x;
		float y3 = Row2.y;
		float z3 = Row2.z;
		float num = z3 * y2 - y3 * z2;
		float num2 = 0f - (z3 * y - y3 * z);
		float num3 = z2 * y - y2 * z;
		float num4 = 0f - (z3 * x2 - x3 * z2);
		float num5 = z3 * x - x3 * z;
		float num6 = 0f - (z2 * x - x2 * z);
		float num7 = y3 * x2 - x3 * y2;
		float num8 = 0f - (y3 * x - x3 * y);
		float num9 = y2 * x - x2 * y;
		float num10 = x * num + x2 * num2 + x3 * num3;
		if (Math.Abs(num10) < float.Epsilon)
		{
			throw new Exception("Matrix3f.Inverse: matrix is not invertible");
		}
		num10 = 1f / num10;
		return new Matrix3f(num * num10, num2 * num10, num3 * num10, num4 * num10, num5 * num10, num6 * num10, num7 * num10, num8 * num10, num9 * num10);
	}

	public Matrix3f Transpose()
	{
		return new Matrix3f(Row0.x, Row1.x, Row2.x, Row0.y, Row1.y, Row2.y, Row0.z, Row1.z, Row2.z);
	}

	public Quaternionf ToQuaternion()
	{
		return new Quaternionf(this);
	}

	public bool EpsilonEqual(Matrix3f m2, float epsilon)
	{
		if (Row0.EpsilonEqual(m2.Row0, epsilon) && Row1.EpsilonEqual(m2.Row1, epsilon))
		{
			return Row2.EpsilonEqual(m2.Row2, epsilon);
		}
		return false;
	}

	public static Matrix3f AxisAngleD(Vector3f axis, float angleDeg)
	{
		double num = (double)angleDeg * (Math.PI / 180.0);
		float num2 = (float)Math.Cos(num);
		float num3 = (float)Math.Sin(num);
		float num4 = 1f - num2;
		float num5 = axis[0] * axis[0];
		float num6 = axis[1] * axis[1];
		float num7 = axis[2] * axis[2];
		float num8 = axis[0] * axis[1] * num4;
		float num9 = axis[0] * axis[2] * num4;
		float num10 = axis[1] * axis[2] * num4;
		float num11 = axis[0] * num3;
		float num12 = axis[1] * num3;
		float num13 = axis[2] * num3;
		return new Matrix3f(num5 * num4 + num2, num8 - num13, num9 + num12, num8 + num13, num6 * num4 + num2, num10 - num11, num9 - num12, num10 + num11, num7 * num4 + num2);
	}

	public override string ToString()
	{
		return $"[{Row0}] [{Row1}] [{Row2}]";
	}

	public string ToString(string fmt)
	{
		return $"[{Row0.ToString(fmt)}] [{Row1.ToString(fmt)}] [{Row2.ToString(fmt)}]";
	}

	public static implicit operator Matrix3f(float3x3 m)
	{
		return new Matrix3f(m.c0, m.c1, m.c2, bRows: false);
	}

	public static explicit operator float3x3(Matrix3f m)
	{
		return new float3x3(m.Column(0), m.Column(1), m.Column(3));
	}
}
