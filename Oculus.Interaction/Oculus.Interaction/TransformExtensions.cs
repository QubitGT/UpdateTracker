using System;
using UnityEngine;

namespace Oculus.Interaction;

public static class TransformExtensions
{
	public static Vector3 InverseTransformPointUnscaled(this Transform transform, Vector3 position)
	{
		return Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one).inverse.MultiplyPoint3x4(position);
	}

	public static Vector3 TransformPointUnscaled(this Transform transform, Vector3 position)
	{
		return Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one).MultiplyPoint3x4(position);
	}

	public static Bounds TransformBounds(this Transform transform, in Bounds bounds)
	{
		Bounds result = default(Bounds);
		Vector3 min = bounds.min;
		Vector3 max = bounds.max;
		Vector3 position = transform.position;
		Vector3 position2 = transform.position;
		Matrix4x4 localToWorldMatrix = transform.localToWorldMatrix;
		for (int i = 0; i < 3; i++)
		{
			for (int j = 0; j < 3; j++)
			{
				float num = localToWorldMatrix[i, j] * min[j];
				float num2 = localToWorldMatrix[i, j] * max[j];
				position[i] += ((num < num2) ? num : num2);
				position2[i] += ((num < num2) ? num2 : num);
			}
		}
		result.SetMinMax(position, position2);
		return result;
	}

	public static Transform FindChildRecursive(this Transform parent, string name)
	{
		return parent.FindChildRecursive((Transform child) => child.name.Contains(name));
	}

	public static Transform FindChildRecursive(this Transform parent, Predicate<Transform> predicate)
	{
		foreach (Transform item in parent)
		{
			if (predicate(item))
			{
				return item;
			}
			Transform transform2 = item.FindChildRecursive(predicate);
			if (transform2 != null)
			{
				return transform2;
			}
		}
		return null;
	}
}
