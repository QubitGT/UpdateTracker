namespace UnityEngine.Rendering;

internal static class ProbeVolumePositioning
{
	internal static Vector3[] m_Axes = new Vector3[6];

	internal static Vector3[] m_AABBCorners = new Vector3[8];

	public static bool OBBIntersect(in ProbeReferenceVolume.Volume a, in ProbeReferenceVolume.Volume b)
	{
		a.CalculateCenterAndSize(out var center, out var size);
		b.CalculateCenterAndSize(out var center2, out var size2);
		float num = size.sqrMagnitude / 2f;
		float num2 = size2.sqrMagnitude / 2f;
		if (Vector3.SqrMagnitude(center - center2) > num + num2)
		{
			return false;
		}
		m_Axes[0] = a.X.normalized;
		m_Axes[1] = a.Y.normalized;
		m_Axes[2] = a.Z.normalized;
		m_Axes[3] = b.X.normalized;
		m_Axes[4] = b.Y.normalized;
		m_Axes[5] = b.Z.normalized;
		for (int i = 0; i < 6; i++)
		{
			Vector2 vector = ProjectOBB(in a, m_Axes[i]);
			Vector2 vector2 = ProjectOBB(in b, m_Axes[i]);
			if (vector.y < vector2.x || vector2.y < vector.x)
			{
				return false;
			}
		}
		return true;
	}

	public static bool OBBContains(in ProbeReferenceVolume.Volume obb, Vector3 point)
	{
		float sqrMagnitude = obb.X.sqrMagnitude;
		float sqrMagnitude2 = obb.Y.sqrMagnitude;
		float sqrMagnitude3 = obb.Z.sqrMagnitude;
		point -= obb.corner;
		point = new Vector3(Vector3.Dot(point, obb.X), Vector3.Dot(point, obb.Y), Vector3.Dot(point, obb.Z));
		if (0f < point.x && point.x < sqrMagnitude && 0f < point.y && point.y < sqrMagnitude2)
		{
			if (0f < point.z)
			{
				return point.z < sqrMagnitude3;
			}
			return false;
		}
		return false;
	}

	public static bool OBBAABBIntersect(in ProbeReferenceVolume.Volume a, in Bounds b, in Bounds aAABB)
	{
		if (!aAABB.Intersects(b))
		{
			return false;
		}
		Vector3 min = b.min;
		Vector3 max = b.max;
		m_AABBCorners[0] = new Vector3(min.x, min.y, min.z);
		m_AABBCorners[1] = new Vector3(max.x, min.y, min.z);
		m_AABBCorners[2] = new Vector3(max.x, max.y, min.z);
		m_AABBCorners[3] = new Vector3(min.x, max.y, min.z);
		m_AABBCorners[4] = new Vector3(min.x, min.y, max.z);
		m_AABBCorners[5] = new Vector3(max.x, min.y, max.z);
		m_AABBCorners[6] = new Vector3(max.x, max.y, max.z);
		m_AABBCorners[7] = new Vector3(min.x, max.y, max.z);
		m_Axes[0] = a.X.normalized;
		m_Axes[1] = a.Y.normalized;
		m_Axes[2] = a.Z.normalized;
		for (int i = 0; i < 3; i++)
		{
			Vector2 vector = ProjectOBB(in a, m_Axes[i]);
			Vector2 vector2 = ProjectAABB(in m_AABBCorners, m_Axes[i]);
			if (vector.y < vector2.x || vector2.y < vector.x)
			{
				return false;
			}
		}
		return true;
	}

	private static Vector2 ProjectOBB(in ProbeReferenceVolume.Volume a, Vector3 axis)
	{
		float num = Vector3.Dot(axis, a.corner);
		float num2 = num;
		for (int i = 0; i < 2; i++)
		{
			for (int j = 0; j < 2; j++)
			{
				for (int k = 0; k < 2; k++)
				{
					Vector3 rhs = a.corner + a.X * i + a.Y * j + a.Z * k;
					float num3 = Vector3.Dot(axis, rhs);
					if (num3 < num)
					{
						num = num3;
					}
					else if (num3 > num2)
					{
						num2 = num3;
					}
				}
			}
		}
		return new Vector2(num, num2);
	}

	private static Vector2 ProjectAABB(in Vector3[] corners, Vector3 axis)
	{
		float num = Vector3.Dot(axis, corners[0]);
		float num2 = num;
		for (int i = 1; i < 8; i++)
		{
			float num3 = Vector3.Dot(axis, corners[i]);
			if (num3 < num)
			{
				num = num3;
			}
			else if (num3 > num2)
			{
				num2 = num3;
			}
		}
		return new Vector2(num, num2);
	}
}
