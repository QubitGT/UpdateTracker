using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Drawing;

public static class DrawingUtilities
{
	private static List<Component> componentBuffer = new List<Component>();

	public static Bounds BoundsFrom(GameObject gameObject)
	{
		return BoundsFrom(gameObject.transform);
	}

	public static Bounds BoundsFrom(Transform transform)
	{
		transform.gameObject.GetComponents(componentBuffer);
		Bounds result = new Bounds(transform.position, Vector3.zero);
		for (int i = 0; i < componentBuffer.Count; i++)
		{
			Component component = componentBuffer[i];
			if (component is Collider collider)
			{
				result.Encapsulate(collider.bounds);
			}
			else if (component is Collider2D collider2D)
			{
				result.Encapsulate(collider2D.bounds);
			}
			else if (component is MeshRenderer meshRenderer)
			{
				result.Encapsulate(meshRenderer.bounds);
			}
			else if (component is SpriteRenderer spriteRenderer)
			{
				result.Encapsulate(spriteRenderer.bounds);
			}
		}
		componentBuffer.Clear();
		int childCount = transform.childCount;
		for (int j = 0; j < childCount; j++)
		{
			result.Encapsulate(BoundsFrom(transform.GetChild(j)));
		}
		return result;
	}

	public static Bounds BoundsFrom(List<Vector3> points)
	{
		if (points.Count == 0)
		{
			throw new ArgumentException("At least 1 point is required");
		}
		Vector3 vector = points[0];
		Vector3 vector2 = points[0];
		for (int i = 0; i < points.Count; i++)
		{
			vector = Vector3.Min(vector, points[i]);
			vector2 = Vector3.Max(vector2, points[i]);
		}
		return new Bounds((vector2 + vector) * 0.5f, (vector2 - vector) * 0.5f);
	}

	public static Bounds BoundsFrom(Vector3[] points)
	{
		if (points.Length == 0)
		{
			throw new ArgumentException("At least 1 point is required");
		}
		Vector3 vector = points[0];
		Vector3 vector2 = points[0];
		for (int i = 0; i < points.Length; i++)
		{
			vector = Vector3.Min(vector, points[i]);
			vector2 = Vector3.Max(vector2, points[i]);
		}
		return new Bounds((vector2 + vector) * 0.5f, (vector2 - vector) * 0.5f);
	}

	public static Bounds BoundsFrom(NativeArray<float3> points)
	{
		if (points.Length == 0)
		{
			throw new ArgumentException("At least 1 point is required");
		}
		float3 @float = points[0];
		float3 float2 = points[0];
		for (int i = 0; i < points.Length; i++)
		{
			@float = math.min(@float, points[i]);
			float2 = math.max(float2, points[i]);
		}
		return new Bounds((float2 + @float) * 0.5f, (float2 - @float) * 0.5f);
	}
}
