using System.Collections.Generic;
using UnityEngine;

namespace Unity.XR.CoreUtils;

public static class BoundsUtils
{
	private static readonly List<Renderer> k_Renderers = new List<Renderer>();

	private static readonly List<Transform> k_Transforms = new List<Transform>();

	public static Bounds GetBounds(List<GameObject> gameObjects)
	{
		Bounds? bounds = null;
		foreach (GameObject gameObject in gameObjects)
		{
			Bounds bounds2 = GetBounds(gameObject.transform);
			if (!bounds.HasValue)
			{
				bounds = bounds2;
				continue;
			}
			bounds2.Encapsulate(bounds.Value);
			bounds = bounds2;
		}
		return bounds.GetValueOrDefault();
	}

	public static Bounds GetBounds(Transform[] transforms)
	{
		Bounds? bounds = null;
		for (int i = 0; i < transforms.Length; i++)
		{
			Bounds bounds2 = GetBounds(transforms[i]);
			if (!bounds.HasValue)
			{
				bounds = bounds2;
				continue;
			}
			bounds2.Encapsulate(bounds.Value);
			bounds = bounds2;
		}
		return bounds.GetValueOrDefault();
	}

	public static Bounds GetBounds(Transform transform)
	{
		transform.GetComponentsInChildren(k_Renderers);
		Bounds bounds = GetBounds(k_Renderers);
		if (bounds.size == Vector3.zero)
		{
			transform.GetComponentsInChildren(k_Transforms);
			if (k_Transforms.Count > 0)
			{
				bounds.center = k_Transforms[0].position;
			}
			foreach (Transform k_Transform in k_Transforms)
			{
				bounds.Encapsulate(k_Transform.position);
			}
		}
		return bounds;
	}

	public static Bounds GetBounds(List<Renderer> renderers)
	{
		if (renderers.Count > 0)
		{
			Renderer renderer = renderers[0];
			Bounds result = new Bounds(renderer.transform.position, Vector3.zero);
			{
				foreach (Renderer renderer2 in renderers)
				{
					if (renderer2.bounds.size != Vector3.zero)
					{
						result.Encapsulate(renderer2.bounds);
					}
				}
				return result;
			}
		}
		return default(Bounds);
	}

	public static Bounds GetBounds<T>(List<T> colliders) where T : Collider
	{
		if (colliders.Count > 0)
		{
			T val = colliders[0];
			Bounds result = new Bounds(val.transform.position, Vector3.zero);
			{
				foreach (T collider in colliders)
				{
					if (collider.bounds.size != Vector3.zero)
					{
						result.Encapsulate(collider.bounds);
					}
				}
				return result;
			}
		}
		return default(Bounds);
	}

	public static Bounds GetBounds(List<Vector3> points)
	{
		Bounds result = default(Bounds);
		if (points.Count < 1)
		{
			return result;
		}
		Vector3 vector = points[0];
		Vector3 max = vector;
		for (int i = 1; i < points.Count; i++)
		{
			Vector3 vector2 = points[i];
			if (vector2.x < vector.x)
			{
				vector.x = vector2.x;
			}
			if (vector2.y < vector.y)
			{
				vector.y = vector2.y;
			}
			if (vector2.z < vector.z)
			{
				vector.z = vector2.z;
			}
			if (vector2.x > max.x)
			{
				max.x = vector2.x;
			}
			if (vector2.y > max.y)
			{
				max.y = vector2.y;
			}
			if (vector2.z > max.z)
			{
				max.z = vector2.z;
			}
		}
		result.SetMinMax(vector, max);
		return result;
	}
}
