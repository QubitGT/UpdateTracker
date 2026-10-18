using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections;

namespace UnityEngine.Rendering.Universal;

[Serializable]
internal class ShadowShape2DProvider_Collider2D : ShadowShape2DProvider
{
	private struct MinMaxBounds
	{
		public Vector3 min;

		public Vector3 max;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Intersects(ref MinMaxBounds bounds)
		{
			if (min.x <= bounds.max.x && max.x >= bounds.min.x && min.y <= bounds.max.y && max.y >= bounds.min.y && min.z <= bounds.max.z)
			{
				return max.z >= bounds.min.z;
			}
			return false;
		}

		public MinMaxBounds(ref Bounds bounds)
		{
			min = bounds.min;
			max = bounds.max;
		}
	}

	private const float k_InitialTrim = 0.05f;

	private List<Bounds> m_ShadowShapeBounds;

	private List<MinMaxBounds> m_ShadowShapeMinMaxBounds;

	private MinMaxBounds m_ShadowCombinedShapeMinMaxBounds;

	private Bounds m_LastWorldCullingBounds;

	private Matrix4x4 m_LastColliderSpace;

	private bool m_ShadowDirty = true;

	private uint m_ShadowStateHash;

	private PhysicsShapeGroup2D m_ShadowShapeGroup;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool CompareApproximately(ref Bounds a, ref Bounds b)
	{
		if (!((a.min - b.min).sqrMagnitude > Mathf.Epsilon))
		{
			return !((a.max - b.max).sqrMagnitude > Mathf.Epsilon);
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void TransformBounds2D(Matrix4x4 transform, ref Bounds bounds)
	{
		Vector3 center = transform.MultiplyPoint(bounds.center);
		Vector3 extents = bounds.extents;
		Vector3 vector = transform.MultiplyVector(new Vector3(extents.x, 0f, 0f));
		Vector3 vector2 = transform.MultiplyVector(new Vector3(0f, extents.y, 0f));
		extents.x = MathF.Abs(vector.x) + MathF.Abs(vector2.x);
		extents.y = MathF.Abs(vector.y) + MathF.Abs(vector2.y);
		bounds = new Bounds
		{
			center = center,
			extents = extents
		};
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void ClearShapes(ShadowShape2D persistantShapeObject)
	{
		persistantShapeObject.SetShape(default(NativeArray<Vector3>), default(NativeArray<int>), ShadowShape2D.OutlineTopology.Lines, ShadowShape2D.WindingOrder.CounterClockwise);
	}

	private void CalculateShadows(Collider2D collider, ShadowShape2D persistantShapeObject, Bounds worldCullingBounds)
	{
		if (m_ShadowShapeGroup == null)
		{
			m_ShadowShapeGroup = new PhysicsShapeGroup2D(collider.shapeCount);
		}
		if (m_ShadowShapeBounds == null)
		{
			m_ShadowShapeBounds = new List<Bounds>(collider.shapeCount);
		}
		if (m_ShadowShapeMinMaxBounds == null)
		{
			m_ShadowShapeMinMaxBounds = new List<MinMaxBounds>();
		}
		Rigidbody2D attachedRigidbody = collider.attachedRigidbody;
		Matrix4x4 matrix4x = (attachedRigidbody ? attachedRigidbody.localToWorldMatrix : Matrix4x4.identity);
		uint shapeHash = collider.GetShapeHash();
		if (shapeHash != m_ShadowStateHash)
		{
			m_ShadowStateHash = shapeHash;
			m_ShadowShapeGroup.Clear();
			if (collider.shapeCount == 0)
			{
				ClearShapes(persistantShapeObject);
				return;
			}
			if (collider.GetShapes(m_ShadowShapeGroup) == 0)
			{
				return;
			}
			m_LastWorldCullingBounds = worldCullingBounds;
			Bounds bounds = collider.GetShapeBounds(m_ShadowShapeBounds, useRadii: true, useWorldSpace: false);
			m_ShadowCombinedShapeMinMaxBounds = new MinMaxBounds(ref bounds);
			m_ShadowShapeMinMaxBounds.Clear();
			m_ShadowShapeMinMaxBounds.Capacity = m_ShadowShapeBounds.Capacity;
			for (int i = 0; i < m_ShadowShapeBounds.Count; i++)
			{
				Bounds bounds2 = m_ShadowShapeBounds[i];
				m_ShadowShapeMinMaxBounds.Add(new MinMaxBounds(ref bounds2));
			}
			m_ShadowDirty = true;
		}
		else
		{
			if (matrix4x.Equals(m_LastColliderSpace) && CompareApproximately(ref m_LastWorldCullingBounds, ref worldCullingBounds))
			{
				return;
			}
			m_LastWorldCullingBounds = worldCullingBounds;
			m_ShadowDirty = true;
		}
		m_LastColliderSpace = matrix4x;
		if (!m_ShadowDirty || m_ShadowShapeGroup.shapeCount == 0)
		{
			return;
		}
		m_ShadowDirty = false;
		TransformBounds2D(Matrix4x4.Inverse(matrix4x), ref worldCullingBounds);
		MinMaxBounds bounds3 = new MinMaxBounds(ref worldCullingBounds);
		if (!m_ShadowCombinedShapeMinMaxBounds.Intersects(ref bounds3))
		{
			ClearShapes(persistantShapeObject);
			return;
		}
		int shapeCount = m_ShadowShapeGroup.shapeCount;
		List<PhysicsShape2D> groupShapes = m_ShadowShapeGroup.groupShapes;
		List<Vector2> groupVertices = m_ShadowShapeGroup.groupVertices;
		NativeArray<int> nativeArray = new NativeArray<int>(shapeCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		for (int j = 0; j < shapeCount; j++)
		{
			if (m_ShadowShapeMinMaxBounds[j].Intersects(ref bounds3))
			{
				PhysicsShape2D physicsShape2D = groupShapes[j];
				int vertexCount = physicsShape2D.vertexCount;
				PhysicsShapeType2D shapeType = physicsShape2D.shapeType;
				num += vertexCount;
				switch (shapeType)
				{
				case PhysicsShapeType2D.Circle:
				case PhysicsShapeType2D.Capsule:
					num2 += 2;
					break;
				case PhysicsShapeType2D.Polygon:
					num2 += 2 * vertexCount;
					break;
				case PhysicsShapeType2D.Edges:
				{
					Vector2 vector = groupVertices[physicsShape2D.vertexStartIndex];
					bool flag = (groupVertices[physicsShape2D.vertexStartIndex + physicsShape2D.vertexCount - 1] - vector).sqrMagnitude > Mathf.Epsilon;
					num2 += 2 * (flag ? (vertexCount - 1) : vertexCount);
					break;
				}
				}
				nativeArray[num3++] = j;
			}
		}
		if (num3 > 0)
		{
			NativeArray<float> radii = new NativeArray<float>(num, Allocator.Temp);
			NativeArray<Vector3> vertices = new NativeArray<Vector3>(num, Allocator.Temp);
			NativeArray<int> indices = new NativeArray<int>(num2, Allocator.Temp);
			int num4 = 0;
			int num5 = 0;
			for (int k = 0; k < num3; k++)
			{
				PhysicsShape2D physicsShape2D2 = groupShapes[nativeArray[k]];
				PhysicsShapeType2D shapeType2 = physicsShape2D2.shapeType;
				float radius = physicsShape2D2.radius;
				int vertexStartIndex = physicsShape2D2.vertexStartIndex;
				int vertexCount2 = physicsShape2D2.vertexCount;
				switch (shapeType2)
				{
				case PhysicsShapeType2D.Circle:
					radii[num4] = radius;
					indices[num5++] = num4;
					indices[num5++] = num4;
					vertices[num4++] = groupVertices[vertexStartIndex];
					break;
				case PhysicsShapeType2D.Capsule:
					radii[num4] = radius;
					indices[num5++] = num4;
					vertices[num4++] = groupVertices[vertexStartIndex++];
					radii[num4] = radius;
					indices[num5++] = num4;
					vertices[num4++] = groupVertices[vertexStartIndex++];
					break;
				case PhysicsShapeType2D.Polygon:
				{
					int value3 = num4;
					int value4 = num4;
					for (int m = 0; m < vertexCount2 - 1; m++)
					{
						radii[num4] = radius;
						vertices[num4++] = groupVertices[vertexStartIndex++];
						indices[num5++] = value4++;
						indices[num5++] = value4;
					}
					radii[num4] = radius;
					vertices[num4++] = groupVertices[vertexStartIndex++];
					indices[num5++] = value4;
					indices[num5++] = value3;
					break;
				}
				case PhysicsShapeType2D.Edges:
				{
					int value = num4;
					int value2 = num4;
					for (int l = 0; l < vertexCount2 - 1; l++)
					{
						radii[num4] = radius;
						vertices[num4++] = groupVertices[vertexStartIndex++];
						indices[num5++] = value2++;
						indices[num5++] = value2;
					}
					radii[num4] = radius;
					vertices[num4++] = groupVertices[vertexStartIndex++];
					Vector2 vector2 = groupVertices[physicsShape2D2.vertexStartIndex];
					if (!((groupVertices[physicsShape2D2.vertexStartIndex + physicsShape2D2.vertexCount - 1] - vector2).sqrMagnitude > Mathf.Epsilon))
					{
						indices[num5++] = value2;
						indices[num5++] = value;
					}
					break;
				}
				}
			}
			Matrix4x4 transform = collider.transform.worldToLocalMatrix * matrix4x;
			Renderer component;
			bool createInteriorGeometry = !collider.TryGetComponent<Renderer>(out component);
			persistantShapeObject.SetShape(vertices, indices, radii, transform, ShadowShape2D.WindingOrder.CounterClockwise, allowContraction: true, createInteriorGeometry);
			indices.Dispose();
			vertices.Dispose();
			radii.Dispose();
		}
		else
		{
			ClearShapes(persistantShapeObject);
		}
		nativeArray.Dispose();
	}

	public override bool IsShapeSource(Component sourceComponent)
	{
		return sourceComponent is Collider2D;
	}

	public override void OnPersistantDataCreated(Component sourceComponent, ShadowShape2D persistantShadowShapeData)
	{
		m_ShadowStateHash = 0u;
		m_ShadowCombinedShapeMinMaxBounds = default(MinMaxBounds);
		m_LastColliderSpace = Matrix4x4.identity;
	}

	public override void OnBeforeRender(Component sourceComponent, Bounds worldCullingBounds, ShadowShape2D persistantShadowShape)
	{
		Collider2D collider = (Collider2D)sourceComponent;
		CalculateShadows(collider, persistantShadowShape, worldCullingBounds);
	}
}
