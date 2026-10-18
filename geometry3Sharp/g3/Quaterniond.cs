using System;
using Unity.Mathematics;
using UnityEngine;

namespace g3;

public struct Quaterniond
{
	public double x;

	public double y;

	public double z;

	public double w;

	public static readonly Quaterniond Zero = new Quaterniond(0.0, 0.0, 0.0, 0.0);

	public static readonly Quaterniond Identity = new Quaterniond(0.0, 0.0, 0.0, 1.0);

	public double this[int key]
	{
		get
		{
			return key switch
			{
				0 => x, 
				1 => y, 
				2 => z, 
				_ => w, 
			};
		}
		set
		{
			switch (key)
			{
			case 0:
				x = value;
				break;
			case 1:
				y = value;
				break;
			case 2:
				z = value;
				break;
			default:
				w = value;
				break;
			}
		}
	}

	public double LengthSquared => x * x + y * y + z * z + w * w;

	public double Length => Math.Sqrt(x * x + y * y + z * z + w * w);

	public Quaterniond Normalized
	{
		get
		{
			Quaterniond result = new Quaterniond(this);
			result.Normalize();
			return result;
		}
	}

	public Vector3d AxisX
	{
		get
		{
			double num = 2.0 * y;
			double num2 = 2.0 * z;
			double num3 = num * w;
			double num4 = num2 * w;
			double num5 = num * x;
			double num6 = num2 * x;
			double num7 = num * y;
			double num8 = num2 * z;
			return new Vector3d(1.0 - (num7 + num8), num5 + num4, num6 - num3);
		}
	}

	public Vector3d AxisY
	{
		get
		{
			double num = 2.0 * x;
			double num2 = 2.0 * y;
			double num3 = 2.0 * z;
			double num4 = num * w;
			double num5 = num3 * w;
			double num6 = num * x;
			double num7 = num2 * x;
			double num8 = num3 * y;
			double num9 = num3 * z;
			return new Vector3d(num7 - num5, 1.0 - (num6 + num9), num8 + num4);
		}
	}

	public Vector3d AxisZ
	{
		get
		{
			double num = 2.0 * x;
			double num2 = 2.0 * y;
			double num3 = 2.0 * z;
			double num4 = num * w;
			double num5 = num2 * w;
			double num6 = num * x;
			double num7 = num3 * x;
			double num8 = num2 * y;
			double num9 = num3 * y;
			return new Vector3d(num7 + num5, num9 - num4, 1.0 - (num6 + num8));
		}
	}

	public Quaterniond(double x, double y, double z, double w)
	{
		this.x = x;
		this.y = y;
		this.z = z;
		this.w = w;
	}

	public Quaterniond(double[] v2)
	{
		x = v2[0];
		y = v2[1];
		z = v2[2];
		w = v2[3];
	}

	public Quaterniond(Quaterniond q2)
	{
		x = q2.x;
		y = q2.y;
		z = q2.z;
		w = q2.w;
	}

	public Quaterniond(Vector3d axis, double AngleDeg)
	{
		x = (y = (z = 0.0));
		w = 1.0;
		SetAxisAngleD(axis, AngleDeg);
	}

	public Quaterniond(Vector3d vFrom, Vector3d vTo)
	{
		x = (y = (z = 0.0));
		w = 1.0;
		SetFromTo(vFrom, vTo);
	}

	public Quaterniond(Quaterniond p, Quaterniond q, double t)
	{
		x = (y = (z = 0.0));
		w = 1.0;
		SetToSlerp(p, q, t);
	}

	public Quaterniond(Matrix3d mat)
	{
		x = (y = (z = 0.0));
		w = 1.0;
		SetFromRotationMatrix(mat);
	}

	public double Normalize(double epsilon = 0.0)
	{
		double num = Length;
		if (num > epsilon)
		{
			double num2 = 1.0 / num;
			x *= num2;
			y *= num2;
			z *= num2;
			w *= num2;
		}
		else
		{
			num = 0.0;
			x = (y = (z = (w = 0.0)));
		}
		return num;
	}

	public double Dot(Quaterniond q2)
	{
		return x * q2.x + y * q2.y + z * q2.z + w * q2.w;
	}

	public static Quaterniond operator -(Quaterniond q2)
	{
		return new Quaterniond(0.0 - q2.x, 0.0 - q2.y, 0.0 - q2.z, 0.0 - q2.w);
	}

	public static Quaterniond operator *(Quaterniond a, Quaterniond b)
	{
		double num = a.w * b.w - a.x * b.x - a.y * b.y - a.z * b.z;
		double num2 = a.w * b.x + a.x * b.w + a.y * b.z - a.z * b.y;
		double num3 = a.w * b.y + a.y * b.w + a.z * b.x - a.x * b.z;
		double num4 = a.w * b.z + a.z * b.w + a.x * b.y - a.y * b.x;
		return new Quaterniond(num2, num3, num4, num);
	}

	public static Quaterniond operator *(Quaterniond q1, double d)
	{
		return new Quaterniond(d * q1.x, d * q1.y, d * q1.z, d * q1.w);
	}

	public static Quaterniond operator *(double d, Quaterniond q1)
	{
		return new Quaterniond(d * q1.x, d * q1.y, d * q1.z, d * q1.w);
	}

	public static Quaterniond operator -(Quaterniond q1, Quaterniond q2)
	{
		return new Quaterniond(q1.x - q2.x, q1.y - q2.y, q1.z - q2.z, q1.w - q2.w);
	}

	public static Quaterniond operator +(Quaterniond q1, Quaterniond q2)
	{
		return new Quaterniond(q1.x + q2.x, q1.y + q2.y, q1.z + q2.z, q1.w + q2.w);
	}

	public static Vector3d operator *(Quaterniond q, Vector3d v)
	{
		double num = 2.0 * q.x;
		double num2 = 2.0 * q.y;
		double num3 = 2.0 * q.z;
		double num4 = num * q.w;
		double num5 = num2 * q.w;
		double num6 = num3 * q.w;
		double num7 = num * q.x;
		double num8 = num2 * q.x;
		double num9 = num3 * q.x;
		double num10 = num2 * q.y;
		double num11 = num3 * q.y;
		double num12 = num3 * q.z;
		return new Vector3d(v.x * (1.0 - (num10 + num12)) + v.y * (num8 - num6) + v.z * (num9 + num5), v.x * (num8 + num6) + v.y * (1.0 - (num7 + num12)) + v.z * (num11 - num4), v.x * (num9 - num5) + v.y * (num11 + num4) + v.z * (1.0 - (num7 + num10)));
	}

	public Quaterniond Inverse()
	{
		double lengthSquared = LengthSquared;
		if (lengthSquared > 0.0)
		{
			double num = 1.0 / lengthSquared;
			return new Quaterniond((0.0 - x) * num, (0.0 - y) * num, (0.0 - z) * num, w * num);
		}
		return Zero;
	}

	public static Quaterniond Inverse(Quaterniond q)
	{
		return q.Inverse();
	}

	public Quaterniond Conjugate()
	{
		return new Quaterniond(0.0 - x, 0.0 - y, 0.0 - z, w);
	}

	public Matrix3d ToRotationMatrix()
	{
		double num = 2.0 * x;
		double num2 = 2.0 * y;
		double num3 = 2.0 * z;
		double num4 = num * w;
		double num5 = num2 * w;
		double num6 = num3 * w;
		double num7 = num * x;
		double num8 = num2 * x;
		double num9 = num3 * x;
		double num10 = num2 * y;
		double num11 = num3 * y;
		double num12 = num3 * z;
		return new Matrix3d(1.0 - (num10 + num12), num8 - num6, num9 + num5, num8 + num6, 1.0 - (num7 + num12), num11 - num4, num9 - num5, num11 + num4, 1.0 - (num7 + num10));
	}

	public void SetAxisAngleD(Vector3d axis, double AngleDeg)
	{
		double num = Math.PI / 180.0 * AngleDeg;
		double num2 = 0.5 * num;
		double num3 = Math.Sin(num2);
		w = Math.Cos(num2);
		x = num3 * axis.x;
		y = num3 * axis.y;
		z = num3 * axis.z;
	}

	public static Quaterniond AxisAngleD(Vector3d axis, double angleDeg)
	{
		return new Quaterniond(axis, angleDeg);
	}

	public static Quaterniond AxisAngleR(Vector3d axis, double angleRad)
	{
		return new Quaterniond(axis, angleRad * 57.295780181884766);
	}

	public void SetFromTo(Vector3d vFrom, Vector3d vTo)
	{
		Vector3d normalized = vFrom.Normalized;
		Vector3d normalized2 = vTo.Normalized;
		Vector3d normalized3 = (normalized + normalized2).Normalized;
		w = normalized.Dot(normalized3);
		if (w != 0.0)
		{
			Vector3d vector3d = normalized.Cross(normalized3);
			x = vector3d.x;
			y = vector3d.y;
			z = vector3d.z;
		}
		else if (Math.Abs(normalized.x) >= Math.Abs(normalized.y))
		{
			double num = 1.0 / Math.Sqrt(normalized.x * normalized.x + normalized.z * normalized.z);
			x = (0.0 - normalized.z) * num;
			y = 0.0;
			z = normalized.x * num;
		}
		else
		{
			double num = 1.0 / Math.Sqrt(normalized.y * normalized.y + normalized.z * normalized.z);
			x = 0.0;
			y = normalized.z * num;
			z = (0.0 - normalized.y) * num;
		}
		Normalize();
	}

	public static Quaterniond FromTo(Vector3d vFrom, Vector3d vTo)
	{
		return new Quaterniond(vFrom, vTo);
	}

	public static Quaterniond FromToConstrained(Vector3d vFrom, Vector3d vTo, Vector3d vAround)
	{
		double angleDeg = MathUtil.PlaneAngleSignedD(vFrom, vTo, vAround);
		return AxisAngleD(vAround, angleDeg);
	}

	public void SetToSlerp(Quaterniond p, Quaterniond q, double t)
	{
		double num = Math.Acos(p.Dot(q));
		if (Math.Abs(num) >= 1E-08)
		{
			double num2 = Math.Sin(num);
			double num3 = 1.0 / num2;
			double num4 = t * num;
			double num5 = Math.Sin(num - num4) * num3;
			double num6 = Math.Sin(num4) * num3;
			x = num5 * p.x + num6 * q.x;
			y = num5 * p.y + num6 * q.y;
			z = num5 * p.z + num6 * q.z;
			w = num5 * p.w + num6 * q.w;
		}
		else
		{
			x = p.x;
			y = p.y;
			z = p.z;
			w = p.w;
		}
	}

	public static Quaterniond Slerp(Quaterniond p, Quaterniond q, double t)
	{
		return new Quaterniond(p, q, t);
	}

	public void SetFromRotationMatrix(Matrix3d rot)
	{
		SetFromRotationMatrix(ref rot);
	}

	public void SetFromRotationMatrix(ref Matrix3d rot)
	{
		Index3i index3i = new Index3i(1, 2, 0);
		double num = rot[0, 0] + rot[1, 1] + rot[2, 2];
		if (num > 0.0)
		{
			double num2 = Math.Sqrt(num + 1.0);
			w = 0.5 * num2;
			num2 = 0.5 / num2;
			x = (rot[2, 1] - rot[1, 2]) * num2;
			y = (rot[0, 2] - rot[2, 0]) * num2;
			z = (rot[1, 0] - rot[0, 1]) * num2;
		}
		else
		{
			int num3 = 0;
			if (rot[1, 1] > rot[0, 0])
			{
				num3 = 1;
			}
			if (rot[2, 2] > rot[num3, num3])
			{
				num3 = 2;
			}
			int num4 = index3i[num3];
			int num5 = index3i[num4];
			double num2 = Math.Sqrt(rot[num3, num3] - rot[num4, num4] - rot[num5, num5] + 1.0);
			Vector3d vector3d = new Vector3d(x, y, z);
			vector3d[num3] = 0.5 * num2;
			num2 = 0.5 / num2;
			w = (rot[num5, num4] - rot[num4, num5]) * num2;
			vector3d[num4] = (rot[num4, num3] + rot[num3, num4]) * num2;
			vector3d[num5] = (rot[num5, num3] + rot[num3, num5]) * num2;
			x = vector3d.x;
			y = vector3d.y;
			z = vector3d.z;
		}
		Normalize();
	}

	public bool EpsilonEqual(Quaterniond q2, double epsilon)
	{
		if (Math.Abs(x - q2.x) <= epsilon && Math.Abs(y - q2.y) <= epsilon && Math.Abs(z - q2.z) <= epsilon)
		{
			return Math.Abs(w - q2.w) <= epsilon;
		}
		return false;
	}

	public static implicit operator Quaterniond(Quaternionf q)
	{
		return new Quaterniond(q.x, q.y, q.z, q.w);
	}

	public static explicit operator Quaternionf(Quaterniond q)
	{
		return new Quaternionf((float)q.x, (float)q.y, (float)q.z, (float)q.w);
	}

	public override string ToString()
	{
		return $"{x:F8} {y:F8} {z:F8} {w:F8}";
	}

	public string ToString(string fmt)
	{
		return $"{x.ToString(fmt)} {y.ToString(fmt)} {z.ToString(fmt)} {w.ToString(fmt)}";
	}

	public static implicit operator Quaterniond(Quaternion q)
	{
		return new Quaterniond(q.x, q.y, q.z, q.w);
	}

	public static explicit operator Quaternion(Quaterniond q)
	{
		return new Quaternion((float)q.x, (float)q.y, (float)q.z, (float)q.w);
	}

	public static implicit operator Quaterniond(quaternion q)
	{
		float4 value = q.value;
		return new Quaterniond(value.x, value.y, value.z, value.w);
	}

	public static explicit operator quaternion(Quaterniond q)
	{
		return new quaternion((float)q.x, (float)q.y, (float)q.z, (float)q.w);
	}
}
