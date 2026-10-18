using System;

namespace UnityEngine.Animations.Rigging;

[Serializable]
public struct AffineTransform
{
	public Vector3 translation;

	public Quaternion rotation;

	public static AffineTransform identity { get; } = new AffineTransform(Vector3.zero, Quaternion.identity);


	public AffineTransform(Vector3 t, Quaternion r)
	{
		translation = t;
		rotation = r;
	}

	public void Set(Vector3 t, Quaternion r)
	{
		translation = t;
		rotation = r;
	}

	public Vector3 Transform(Vector3 p)
	{
		return rotation * p + translation;
	}

	public Vector3 InverseTransform(Vector3 p)
	{
		return Quaternion.Inverse(rotation) * (p - translation);
	}

	public AffineTransform Inverse()
	{
		Quaternion quaternion = Quaternion.Inverse(rotation);
		return new AffineTransform(quaternion * -translation, quaternion);
	}

	public AffineTransform InverseMul(AffineTransform transform)
	{
		Quaternion quaternion = Quaternion.Inverse(rotation);
		return new AffineTransform(quaternion * (transform.translation - translation), quaternion * transform.rotation);
	}

	public static Vector3 operator *(AffineTransform lhs, Vector3 rhs)
	{
		return lhs.rotation * rhs + lhs.translation;
	}

	public static AffineTransform operator *(AffineTransform lhs, AffineTransform rhs)
	{
		return new AffineTransform(lhs.Transform(rhs.translation), lhs.rotation * rhs.rotation);
	}

	public static AffineTransform operator *(Quaternion lhs, AffineTransform rhs)
	{
		return new AffineTransform(lhs * rhs.translation, lhs * rhs.rotation);
	}

	public static AffineTransform operator *(AffineTransform lhs, Quaternion rhs)
	{
		return new AffineTransform(lhs.translation, lhs.rotation * rhs);
	}
}
