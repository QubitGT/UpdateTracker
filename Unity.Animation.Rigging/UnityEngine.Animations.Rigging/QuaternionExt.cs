namespace UnityEngine.Animations.Rigging;

public static class QuaternionExt
{
	private const float k_FloatMin = 1E-10f;

	public static readonly Quaternion zero = new Quaternion(0f, 0f, 0f, 0f);

	public static Quaternion FromToRotation(Vector3 from, Vector3 to)
	{
		float num = Vector3.Dot(from.normalized, to.normalized);
		if (num >= 1f)
		{
			return Quaternion.identity;
		}
		if (num <= -1f)
		{
			Vector3 axis = Vector3.Cross(from, Vector3.right);
			if (axis.sqrMagnitude == 0f)
			{
				axis = Vector3.Cross(from, Vector3.up);
			}
			return Quaternion.AngleAxis(180f, axis);
		}
		return Quaternion.AngleAxis(Mathf.Acos(num) * 57.29578f, Vector3.Cross(from, to).normalized);
	}

	public static Quaternion Add(Quaternion rhs, Quaternion lhs)
	{
		float num = Mathf.Sign(Quaternion.Dot(rhs, lhs));
		return new Quaternion(rhs.x + num * lhs.x, rhs.y + num * lhs.y, rhs.z + num * lhs.z, rhs.w + num * lhs.w);
	}

	public static Quaternion Scale(Quaternion q, float scale)
	{
		return new Quaternion(q.x * scale, q.y * scale, q.z * scale, q.w * scale);
	}

	public static Quaternion NormalizeSafe(Quaternion q)
	{
		float num = Quaternion.Dot(q, q);
		if (num > 1E-10f)
		{
			float num2 = 1f / Mathf.Sqrt(num);
			return new Quaternion(q.x * num2, q.y * num2, q.z * num2, q.w * num2);
		}
		return Quaternion.identity;
	}
}
