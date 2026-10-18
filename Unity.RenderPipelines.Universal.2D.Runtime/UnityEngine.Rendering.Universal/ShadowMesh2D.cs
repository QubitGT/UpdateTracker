using System;
using Unity.Collections;

namespace UnityEngine.Rendering.Universal;

[Serializable]
internal class ShadowMesh2D : ShadowShape2D
{
	public enum EdgeProcessing
	{
		None,
		Clipping
	}

	internal const int k_CapsuleCapSegments = 8;

	internal const float k_TrimEdgeUninitialized = -1f;

	[SerializeField]
	private Mesh m_Mesh;

	[SerializeField]
	private Bounds m_LocalBounds;

	[SerializeField]
	private EdgeProcessing m_EdgeProcessing = EdgeProcessing.Clipping;

	[SerializeField]
	private float m_TrimEdge = -1f;

	[SerializeField]
	private bool m_FlipX;

	[SerializeField]
	private bool m_FlipY;

	[SerializeField]
	private float m_InitialTrim;

	internal BoundingSphere m_BoundingSphere;

	public Mesh mesh => m_Mesh;

	public BoundingSphere boundingSphere => m_BoundingSphere;

	public EdgeProcessing edgeProcessing
	{
		get
		{
			return m_EdgeProcessing;
		}
		set
		{
			m_EdgeProcessing = value;
		}
	}

	public float trimEdge
	{
		get
		{
			return m_TrimEdge;
		}
		set
		{
			m_TrimEdge = value;
		}
	}

	internal static void DuplicateShadowMesh(Mesh source, out Mesh dest)
	{
		dest = new Mesh();
		dest.Clear();
		if (source != null)
		{
			dest.vertices = source.vertices;
			dest.tangents = source.tangents;
			dest.triangles = source.triangles;
			dest.bounds = source.bounds;
		}
	}

	internal void CopyFrom(ShadowMesh2D source)
	{
		DuplicateShadowMesh(source.m_Mesh, out m_Mesh);
		m_TrimEdge = source.trimEdge;
		m_LocalBounds = source.m_LocalBounds;
		m_EdgeProcessing = source.edgeProcessing;
	}

	internal void AddCircle(Vector3 center, float r, NativeArray<Vector3> generatedVertices, NativeArray<int> generatedIndices, bool reverseWindingOrder, ref int vertexWritePos, ref int indexWritePos)
	{
		float num = (reverseWindingOrder ? 1 : (-1));
		float num2 = 16f;
		int num3 = vertexWritePos;
		for (int i = 0; (float)i < num2; i++)
		{
			float f = num * (MathF.PI * 2f * (float)i / num2);
			float x = r * Mathf.Cos(f) + center.x;
			float y = r * Mathf.Sin(f) + center.y;
			generatedIndices[indexWritePos++] = vertexWritePos;
			generatedIndices[indexWritePos++] = (((float)(i + 1) < num2) ? (vertexWritePos + 1) : num3);
			generatedVertices[vertexWritePos++] = new Vector3(x, y, 0f);
		}
	}

	internal void AddCapsuleCap(Vector3 center, float r, Vector3 otherCenter, NativeArray<Vector3> generatedVertices, NativeArray<int> generatedIndices, bool reverseWindingOrder, ref int vertexWritePos, ref int indexWritePos)
	{
		float num = 8f;
		Vector3 normalized = (otherCenter - center).normalized;
		float num2 = Mathf.Acos(Vector3.Dot(normalized, new Vector3(1f, 0f, 0f)));
		float num3 = ((Vector3.Dot(normalized, new Vector3(0f, 1f, 0f)) < 0f) ? (-1f) : 1f);
		float num4 = num2 * num3;
		float num6;
		float num7;
		if (reverseWindingOrder)
		{
			float num5 = MathF.PI / 2f;
			num6 = num4 + num5;
			num7 = num6 + MathF.PI;
		}
		else
		{
			float num8 = 4.712389f;
			num6 = num4 + num8;
			num7 = num6 - MathF.PI;
		}
		float num9 = num7 - num6;
		float f;
		for (int i = 0; (float)i < num; i++)
		{
			f = num9 * (float)i / num + num6;
			float x = r * Mathf.Cos(f) + center.x;
			float y = r * Mathf.Sin(f) + center.y;
			generatedIndices[indexWritePos++] = vertexWritePos;
			generatedIndices[indexWritePos++] = vertexWritePos + 1;
			generatedVertices[vertexWritePos++] = new Vector3(x, y, 0f);
		}
		f = num9 + num6;
		generatedVertices[vertexWritePos++] = new Vector3(r * Mathf.Cos(f) + center.x, r * Mathf.Sin(f) + center.y, 0f);
	}

	internal void AddCapsule(Vector3 pt0, Vector3 pt1, float r0, float r1, NativeArray<Vector3> generatedVertices, NativeArray<int> generatedIndices, bool reverseWindingOrder, ref int vertexWritePos, ref int indexWritePos)
	{
		Vector3 normalized = (pt1 - pt0).normalized;
		new Vector3(normalized.y, 0f - normalized.x, 0f);
		new Vector3(0f - normalized.y, normalized.x, 0f);
		if (pt1.x < pt0.x)
		{
			Vector3 vector = pt0;
			pt0 = pt1;
			pt1 = vector;
		}
		int value = vertexWritePos;
		AddCapsuleCap(pt0, r0, pt1, generatedVertices, generatedIndices, reverseWindingOrder, ref vertexWritePos, ref indexWritePos);
		generatedIndices[indexWritePos++] = vertexWritePos - 1;
		generatedIndices[indexWritePos++] = vertexWritePos;
		AddCapsuleCap(pt1, r1, pt0, generatedVertices, generatedIndices, reverseWindingOrder, ref vertexWritePos, ref indexWritePos);
		generatedIndices[indexWritePos++] = vertexWritePos - 1;
		generatedIndices[indexWritePos++] = value;
	}

	internal int AddShape(NativeArray<Vector3> vertices, NativeArray<int> indices, int indicesProcessed, NativeArray<Vector3> generatedVertices, NativeArray<int> generatedIndices, ref int vertexWritePos, ref int indexWritePos)
	{
		int num = indicesProcessed;
		int num2 = indices[num];
		int num3 = indices[num];
		int value = vertexWritePos;
		generatedVertices[vertexWritePos++] = vertices[num2];
		bool flag = true;
		while (num < indices.Length && flag)
		{
			int num4 = indices[num++];
			int num5 = indices[num++];
			generatedIndices[indexWritePos++] = vertexWritePos - 1;
			if (num5 != num3)
			{
				generatedIndices[indexWritePos++] = vertexWritePos;
				generatedVertices[vertexWritePos++] = vertices[num5];
				flag = num4 == num2;
			}
			else
			{
				generatedIndices[indexWritePos++] = value;
				flag = false;
			}
			num2 = num5;
		}
		return num;
	}

	public override void SetShape(NativeArray<Vector3> vertices, NativeArray<int> indices, NativeArray<float> radii, Matrix4x4 transform, WindingOrder windingOrder = WindingOrder.Clockwise, bool allowTriming = true, bool createInteriorGeometry = false)
	{
		if (m_TrimEdge == -1f)
		{
			m_TrimEdge = m_InitialTrim;
		}
		if (m_Mesh == null)
		{
			m_Mesh = new Mesh();
		}
		if (indices.Length == 0)
		{
			m_Mesh.Clear();
			return;
		}
		bool flag = windingOrder == WindingOrder.CounterClockwise;
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < indices.Length; i += 2)
		{
			int num3 = indices[i];
			int num4 = indices[i + 1];
			if (radii[num3] > 0f || radii[num4] > 0f)
			{
				if (num3 == num4)
				{
					num++;
				}
				else
				{
					num2++;
				}
			}
		}
		int num5 = num2 * 2;
		int num6 = num2 * 8;
		int num7 = num * 2 * 8;
		int num8 = (indices.Length >> 1) - (num2 + num);
		int num9 = 2 * (num8 + num5 + 2 * num6 + num7);
		int length = num9;
		NativeArray<Vector3> inVertices = new NativeArray<Vector3>(length, Allocator.Temp);
		NativeArray<int> indices2 = new NativeArray<int>(num9, Allocator.Temp);
		int vertexWritePos = 0;
		int indexWritePos = 0;
		int num10 = 0;
		while (num10 < indices.Length)
		{
			int index = indices[num10];
			int index2 = indices[num10 + 1];
			float num11 = radii[index];
			float r = radii[index2];
			if (radii[index] > 0f || radii[index2] > 0f)
			{
				Vector3 vector = vertices[index];
				Vector3 pt = vertices[index2];
				if (vertices[index].x == vertices[index2].x && vertices[index].y == vertices[index2].y)
				{
					AddCircle(vector, num11, inVertices, indices2, flag, ref vertexWritePos, ref indexWritePos);
				}
				else
				{
					AddCapsule(vector, pt, num11, r, inVertices, indices2, flag, ref vertexWritePos, ref indexWritePos);
				}
				num10 += 2;
			}
			else
			{
				num10 = AddShape(vertices, indices, num10, inVertices, indices2, ref vertexWritePos, ref indexWritePos);
			}
		}
		for (int j = 0; j < inVertices.Length; j++)
		{
			inVertices[j] = transform.MultiplyPoint(inVertices[j]);
		}
		ShadowUtility.CalculateEdgesFromLines(ref indices2, out var outEdges, out var outShapeStartingEdge, out var outShapeIsClosedArray);
		if (flag)
		{
			ShadowUtility.ReverseWindingOrder(ref outShapeStartingEdge, ref outEdges);
		}
		if (m_EdgeProcessing == EdgeProcessing.Clipping)
		{
			ShadowUtility.ClipEdges(ref inVertices, ref outEdges, ref outShapeStartingEdge, ref outShapeIsClosedArray, trimEdge, out var outVertices, out var outEdges2, out var outShapeStartingEdge2);
			if (outShapeStartingEdge2.Length > 0)
			{
				m_LocalBounds = ShadowUtility.GenerateShadowMesh(m_Mesh, outVertices, outEdges2, outShapeStartingEdge2, outShapeIsClosedArray, allowContraction: true, createInteriorGeometry, OutlineTopology.Lines);
			}
			else
			{
				m_LocalBounds = default(Bounds);
				m_Mesh.Clear();
			}
			outVertices.Dispose();
			outEdges2.Dispose();
			outShapeStartingEdge2.Dispose();
		}
		else
		{
			m_LocalBounds = ShadowUtility.GenerateShadowMesh(m_Mesh, inVertices, outEdges, outShapeStartingEdge, outShapeIsClosedArray, allowContraction: true, createInteriorGeometry, OutlineTopology.Lines);
		}
		inVertices.Dispose();
		indices2.Dispose();
		outEdges.Dispose();
		outShapeIsClosedArray.Dispose();
		outShapeStartingEdge.Dispose();
	}

	private bool AreDegenerateVertices(NativeArray<Vector3> vertices)
	{
		if (vertices.Length == 0)
		{
			return true;
		}
		int index = vertices.Length - 1;
		for (int i = 0; i < vertices.Length; i++)
		{
			if (vertices[index].x != vertices[i].x || vertices[index].y != vertices[i].y)
			{
				return false;
			}
			index = i;
		}
		return true;
	}

	public override void SetShape(NativeArray<Vector3> vertices, NativeArray<int> indices, OutlineTopology outlineTopology, WindingOrder windingOrder = WindingOrder.Clockwise, bool allowTrimming = true, bool createInteriorGeometry = false)
	{
		if (AreDegenerateVertices(vertices) || indices.Length == 0)
		{
			m_Mesh.Clear();
			return;
		}
		if (m_TrimEdge == -1f)
		{
			m_TrimEdge = m_InitialTrim;
		}
		bool flag = false;
		if (m_Mesh == null)
		{
			m_Mesh = new Mesh();
		}
		NativeArray<ShadowEdge> outEdges;
		NativeArray<int> outShapeStartingEdge;
		NativeArray<bool> outShapeIsClosedArray;
		if (outlineTopology == OutlineTopology.Triangles)
		{
			ShadowUtility.CalculateEdgesFromTriangles(ref vertices, ref indices, duplicatesVertices: true, out var newVertices, out outEdges, out outShapeStartingEdge, out outShapeIsClosedArray);
			flag = true;
			vertices = newVertices;
		}
		else
		{
			ShadowUtility.CalculateEdgesFromLines(ref indices, out outEdges, out outShapeStartingEdge, out outShapeIsClosedArray);
		}
		if (windingOrder == WindingOrder.CounterClockwise)
		{
			ShadowUtility.ReverseWindingOrder(ref outShapeStartingEdge, ref outEdges);
		}
		if (m_EdgeProcessing == EdgeProcessing.Clipping && allowTrimming)
		{
			ShadowUtility.ClipEdges(ref vertices, ref outEdges, ref outShapeStartingEdge, ref outShapeIsClosedArray, trimEdge, out var outVertices, out var outEdges2, out var outShapeStartingEdge2);
			m_LocalBounds = ShadowUtility.GenerateShadowMesh(m_Mesh, outVertices, outEdges2, outShapeStartingEdge2, outShapeIsClosedArray, allowTrimming, createInteriorGeometry, outlineTopology);
			outVertices.Dispose();
			outEdges2.Dispose();
			outShapeStartingEdge2.Dispose();
		}
		else
		{
			m_LocalBounds = ShadowUtility.GenerateShadowMesh(m_Mesh, vertices, outEdges, outShapeStartingEdge, outShapeIsClosedArray, allowTrimming, createInteriorGeometry, outlineTopology);
		}
		if (flag)
		{
			vertices.Dispose();
		}
		outEdges.Dispose();
		outShapeStartingEdge.Dispose();
		outShapeIsClosedArray.Dispose();
	}

	public void SetShapeWithLines(NativeArray<Vector3> vertices, NativeArray<int> indices, bool allowTrimming)
	{
		SetShape(vertices, indices, OutlineTopology.Lines, WindingOrder.Clockwise, allowTrimming);
	}

	public override void SetFlip(bool flipX, bool flipY)
	{
		m_FlipX = flipX;
		m_FlipY = flipY;
	}

	public override void GetFlip(out bool flipX, out bool flipY)
	{
		flipX = m_FlipX;
		flipY = m_FlipY;
	}

	public override void SetDefaultTrim(float trim)
	{
		m_InitialTrim = trim;
	}

	public void UpdateBoundingSphere(Transform transform)
	{
		Vector3 vector = transform.TransformPoint(m_LocalBounds.max);
		Vector3 vector2 = transform.TransformPoint(m_LocalBounds.min);
		Vector3 vector3 = 0.5f * (vector + vector2);
		float rad = Vector3.Magnitude(vector - vector3);
		m_BoundingSphere = new BoundingSphere(vector3, rad);
	}
}
