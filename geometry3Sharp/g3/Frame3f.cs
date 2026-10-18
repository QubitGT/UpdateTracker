using System;

namespace g3;

public struct Frame3f
{
	private Quaternionf rotation;

	private Vector3f origin;

	public static readonly Frame3f Identity = new Frame3f(Vector3f.Zero, Quaternionf.Identity);

	public Quaternionf Rotation
	{
		get
		{
			return rotation;
		}
		set
		{
			rotation = value;
		}
	}

	public Vector3f Origin
	{
		get
		{
			return origin;
		}
		set
		{
			origin = value;
		}
	}

	public Vector3f X => rotation.AxisX;

	public Vector3f Y => rotation.AxisY;

	public Vector3f Z => rotation.AxisZ;

	public Frame3f(Frame3f copy)
	{
		rotation = copy.rotation;
		origin = copy.origin;
	}

	public Frame3f(Vector3f origin)
	{
		rotation = Quaternionf.Identity;
		this.origin = origin;
	}

	public Frame3f(Vector3d origin)
	{
		rotation = Quaternionf.Identity;
		this.origin = (Vector3f)origin;
	}

	public Frame3f(Vector3f origin, Vector3f setZ)
	{
		rotation = Quaternionf.FromTo(Vector3f.AxisZ, setZ);
		this.origin = origin;
	}

	public Frame3f(Vector3d origin, Vector3d setZ)
	{
		rotation = Quaternionf.FromTo(Vector3f.AxisZ, (Vector3f)setZ);
		this.origin = (Vector3f)origin;
	}

	public Frame3f(Vector3f origin, Vector3f setAxis, int nAxis)
	{
		switch (nAxis)
		{
		case 0:
			rotation = Quaternionf.FromTo(Vector3f.AxisX, setAxis);
			break;
		case 1:
			rotation = Quaternionf.FromTo(Vector3f.AxisY, setAxis);
			break;
		default:
			rotation = Quaternionf.FromTo(Vector3f.AxisZ, setAxis);
			break;
		}
		this.origin = origin;
	}

	public Frame3f(Vector3f origin, Quaternionf orientation)
	{
		rotation = orientation;
		this.origin = origin;
	}

	public Frame3f(Vector3f origin, Vector3f x, Vector3f y, Vector3f z)
	{
		this.origin = origin;
		rotation = new Matrix3f(x, y, z, bRows: false).ToQuaternion();
	}

	public Vector3f GetAxis(int nAxis)
	{
		return nAxis switch
		{
			0 => rotation * Vector3f.AxisX, 
			1 => rotation * Vector3f.AxisY, 
			2 => rotation * Vector3f.AxisZ, 
			_ => throw new ArgumentOutOfRangeException("nAxis"), 
		};
	}

	public void Translate(Vector3f v)
	{
		origin += v;
	}

	public Frame3f Translated(Vector3f v)
	{
		return new Frame3f(origin + v, rotation);
	}

	public Frame3f Translated(float fDistance, int nAxis)
	{
		return new Frame3f(origin + fDistance * GetAxis(nAxis), rotation);
	}

	public void Scale(float f)
	{
		origin *= f;
	}

	public void Scale(Vector3f scale)
	{
		origin *= scale;
	}

	public Frame3f Scaled(float f)
	{
		return new Frame3f(f * origin, rotation);
	}

	public Frame3f Scaled(Vector3f scale)
	{
		return new Frame3f(scale * origin, rotation);
	}

	public void Rotate(Quaternionf q)
	{
		rotation = q * rotation;
	}

	public Frame3f Rotated(Quaternionf q)
	{
		return new Frame3f(origin, q * rotation);
	}

	public Frame3f Rotated(float fAngle, int nAxis)
	{
		return Rotated(new Quaternionf(GetAxis(nAxis), fAngle));
	}

	public void RotateAroundAxes(Quaternionf q)
	{
		rotation *= q;
	}

	public void RotateAround(Vector3f point, Quaternionf q)
	{
		Vector3f vector3f = q * (origin - point);
		rotation = q * rotation;
		origin = point + vector3f;
	}

	public Frame3f RotatedAround(Vector3f point, Quaternionf q)
	{
		Vector3f vector3f = q * (origin - point);
		return new Frame3f(point + vector3f, q * rotation);
	}

	public void AlignAxis(int nAxis, Vector3f vTo)
	{
		Quaternionf q = Quaternionf.FromTo(GetAxis(nAxis), vTo);
		Rotate(q);
	}

	public void ConstrainedAlignAxis(int nAxis, Vector3f vTo, Vector3f vAround)
	{
		float angleDeg = MathUtil.PlaneAngleSignedD(GetAxis(nAxis), vTo, vAround);
		Quaternionf q = Quaternionf.AxisAngleD(vAround, angleDeg);
		Rotate(q);
	}

	public Vector3f ProjectToPlane(Vector3f p, int nNormal)
	{
		Vector3f vector3f = p - origin;
		Vector3f axis = GetAxis(nNormal);
		return origin + (vector3f - vector3f.Dot(axis) * axis);
	}

	public Vector3f FromPlaneUV(Vector2f v, int nPlaneNormalAxis)
	{
		Vector3f vector3f = new Vector3f(v[0], v[1], 0f);
		switch (nPlaneNormalAxis)
		{
		case 0:
			vector3f[0] = 0f;
			vector3f[2] = v[0];
			break;
		case 1:
			vector3f[1] = 0f;
			vector3f[2] = v[1];
			break;
		}
		return rotation * vector3f + origin;
	}

	[Obsolete("replaced with FromPlaneUV")]
	public Vector3f FromFrameP(Vector2f v, int nPlaneNormalAxis)
	{
		return FromPlaneUV(v, nPlaneNormalAxis);
	}

	public Vector2f ToPlaneUV(Vector3f p, int nNormal)
	{
		int nAxis = 0;
		int nAxis2 = 1;
		switch (nNormal)
		{
		case 0:
			nAxis = 2;
			break;
		case 1:
			nAxis2 = 2;
			break;
		}
		Vector3f vector3f = p - origin;
		float x = vector3f.Dot(GetAxis(nAxis));
		float y = vector3f.Dot(GetAxis(nAxis2));
		return new Vector2f(x, y);
	}

	[Obsolete("Use explicit ToPlaneUV instead")]
	public Vector2f ToPlaneUV(Vector3f p, int nNormal, int nAxis0 = -1, int nAxis1 = -1)
	{
		if (nAxis0 != -1 || nAxis1 != -1)
		{
			throw new Exception("[RMS] was this being used?");
		}
		return ToPlaneUV(p, nNormal);
	}

	public float DistanceToPlane(Vector3f p, int nNormal)
	{
		return Math.Abs((p - origin).Dot(GetAxis(nNormal)));
	}

	public float DistanceToPlaneSigned(Vector3f p, int nNormal)
	{
		return (p - origin).Dot(GetAxis(nNormal));
	}

	public Vector3f ToFrameP(Vector3f v)
	{
		v.x -= origin.x;
		v.y -= origin.y;
		v.z -= origin.z;
		return rotation.InverseMultiply(ref v);
	}

	public Vector3f ToFrameP(ref Vector3f v)
	{
		Vector3f v2 = new Vector3f(v.x - origin.x, v.y - origin.y, v.z - origin.z);
		return rotation.InverseMultiply(ref v2);
	}

	public Vector3d ToFrameP(Vector3d v)
	{
		v.x -= origin.x;
		v.y -= origin.y;
		v.z -= origin.z;
		return rotation.InverseMultiply(ref v);
	}

	public Vector3d ToFrameP(ref Vector3d v)
	{
		Vector3d v2 = new Vector3d(v.x - (double)origin.x, v.y - (double)origin.y, v.z - (double)origin.z);
		return rotation.InverseMultiply(ref v2);
	}

	public Vector3f FromFrameP(Vector3f v)
	{
		return rotation * v + origin;
	}

	public Vector3f FromFrameP(ref Vector3f v)
	{
		return rotation * v + origin;
	}

	public Vector3d FromFrameP(Vector3d v)
	{
		return rotation * v + origin;
	}

	public Vector3d FromFrameP(ref Vector3d v)
	{
		return rotation * v + origin;
	}

	public Vector3f ToFrameV(Vector3f v)
	{
		return rotation.InverseMultiply(ref v);
	}

	public Vector3f ToFrameV(ref Vector3f v)
	{
		return rotation.InverseMultiply(ref v);
	}

	public Vector3d ToFrameV(Vector3d v)
	{
		return rotation.InverseMultiply(ref v);
	}

	public Vector3d ToFrameV(ref Vector3d v)
	{
		return rotation.InverseMultiply(ref v);
	}

	public Vector3f FromFrameV(Vector3f v)
	{
		return rotation * v;
	}

	public Vector3f FromFrameV(ref Vector3f v)
	{
		return rotation * v;
	}

	public Vector3d FromFrameV(ref Vector3d v)
	{
		return rotation * v;
	}

	public Vector3d FromFrameV(Vector3d v)
	{
		return rotation * v;
	}

	public Quaternionf ToFrame(Quaternionf q)
	{
		return Quaternionf.Inverse(rotation) * q;
	}

	public Quaternionf ToFrame(ref Quaternionf q)
	{
		return Quaternionf.Inverse(rotation) * q;
	}

	public Quaternionf FromFrame(Quaternionf q)
	{
		return rotation * q;
	}

	public Quaternionf FromFrame(ref Quaternionf q)
	{
		return rotation * q;
	}

	public Ray3f ToFrame(Ray3f r)
	{
		return new Ray3f(ToFrameP(ref r.Origin), ToFrameV(ref r.Direction));
	}

	public Ray3f ToFrame(ref Ray3f r)
	{
		return new Ray3f(ToFrameP(ref r.Origin), ToFrameV(ref r.Direction));
	}

	public Ray3f FromFrame(Ray3f r)
	{
		return new Ray3f(FromFrameP(ref r.Origin), FromFrameV(ref r.Direction));
	}

	public Ray3f FromFrame(ref Ray3f r)
	{
		return new Ray3f(FromFrameP(ref r.Origin), FromFrameV(ref r.Direction));
	}

	public Frame3f ToFrame(Frame3f f)
	{
		return new Frame3f(ToFrameP(ref f.origin), ToFrame(ref f.rotation));
	}

	public Frame3f ToFrame(ref Frame3f f)
	{
		return new Frame3f(ToFrameP(ref f.origin), ToFrame(ref f.rotation));
	}

	public Frame3f FromFrame(Frame3f f)
	{
		return new Frame3f(FromFrameP(ref f.origin), FromFrame(ref f.rotation));
	}

	public Frame3f FromFrame(ref Frame3f f)
	{
		return new Frame3f(FromFrameP(ref f.origin), FromFrame(ref f.rotation));
	}

	public Box3f ToFrame(ref Box3f box)
	{
		box.Center = ToFrameP(ref box.Center);
		box.AxisX = ToFrameV(ref box.AxisX);
		box.AxisY = ToFrameV(ref box.AxisY);
		box.AxisZ = ToFrameV(ref box.AxisZ);
		return box;
	}

	public Box3f FromFrame(ref Box3f box)
	{
		box.Center = FromFrameP(ref box.Center);
		box.AxisX = FromFrameV(ref box.AxisX);
		box.AxisY = FromFrameV(ref box.AxisY);
		box.AxisZ = FromFrameV(ref box.AxisZ);
		return box;
	}

	public Box3d ToFrame(ref Box3d box)
	{
		box.Center = ToFrameP(ref box.Center);
		box.AxisX = ToFrameV(ref box.AxisX);
		box.AxisY = ToFrameV(ref box.AxisY);
		box.AxisZ = ToFrameV(ref box.AxisZ);
		return box;
	}

	public Box3d FromFrame(ref Box3d box)
	{
		box.Center = FromFrameP(ref box.Center);
		box.AxisX = FromFrameV(ref box.AxisX);
		box.AxisY = FromFrameV(ref box.AxisY);
		box.AxisZ = FromFrameV(ref box.AxisZ);
		return box;
	}

	public Vector3f RayPlaneIntersection(Vector3f ray_origin, Vector3f ray_direction, int nAxisAsNormal)
	{
		Vector3f axis = GetAxis(nAxisAsNormal);
		float num = 0f - Vector3f.Dot(Origin, axis);
		float num2 = Vector3f.Dot(ray_direction, axis);
		if (MathUtil.EpsilonEqual(num2, 0f, 1E-06f))
		{
			return Vector3f.Invalid;
		}
		float num3 = (0f - (Vector3f.Dot(ray_origin, axis) + num)) / num2;
		return ray_origin + num3 * ray_direction;
	}

	public static Frame3f Interpolate(Frame3f f1, Frame3f f2, float t)
	{
		return new Frame3f(Vector3f.Lerp(f1.origin, f2.origin, t), Quaternionf.Slerp(f1.rotation, f2.rotation, t));
	}

	public bool EpsilonEqual(Frame3f f2, float epsilon)
	{
		if (origin.EpsilonEqual(f2.origin, epsilon))
		{
			return rotation.EpsilonEqual(f2.rotation, epsilon);
		}
		return false;
	}

	public override string ToString()
	{
		return ToString("F4");
	}

	public string ToString(string fmt)
	{
		return $"[Frame3f: Origin={Origin.ToString(fmt)}, X={X.ToString(fmt)}, Y={Y.ToString(fmt)}, Z={Z.ToString(fmt)}]";
	}

	public static Frame3f SolveMinRotation(Frame3f source, Frame3f target)
	{
		int num = -1;
		int num2 = -1;
		double num3 = 0.0;
		double num4 = 0.0;
		for (int i = 0; i < 3; i++)
		{
			for (int j = 0; j < 3; j++)
			{
				double value = source.GetAxis(i).Dot(target.GetAxis(j));
				double num5 = Math.Abs(value);
				if (num5 > num3)
				{
					num3 = num5;
					num4 = Math.Sign(value);
					num = i;
					num2 = j;
				}
			}
		}
		Frame3f result = source.Rotated(Quaternionf.FromTo(source.GetAxis(num), (float)num4 * target.GetAxis(num2)));
		Vector3f axis = result.GetAxis(num);
		int nAxis = -1;
		int nAxis2 = -1;
		double num6 = 0.0;
		double num7 = 0.0;
		for (int k = 0; k < 3; k++)
		{
			if (k == num)
			{
				continue;
			}
			for (int l = 0; l < 3; l++)
			{
				if (l != num2)
				{
					double value2 = result.GetAxis(k).Dot(target.GetAxis(l));
					double num8 = Math.Abs(value2);
					if (num8 > num6)
					{
						num6 = num8;
						num7 = Math.Sign(value2);
						nAxis = k;
						nAxis2 = l;
					}
				}
			}
		}
		result.ConstrainedAlignAxis(nAxis, (float)num7 * target.GetAxis(nAxis2), axis);
		return result;
	}
}
