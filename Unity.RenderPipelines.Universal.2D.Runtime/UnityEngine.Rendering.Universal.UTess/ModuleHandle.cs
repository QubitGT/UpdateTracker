using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

namespace UnityEngine.Rendering.Universal.UTess;

[StructLayout(LayoutKind.Sequential, Size = 1)]
internal struct ModuleHandle
{
	internal static readonly int kMaxArea = 65536;

	internal static readonly int kMaxEdgeCount = 65536;

	internal static readonly int kMaxIndexCount = 65536;

	internal static readonly int kMaxVertexCount = 65536;

	internal static readonly int kMaxTriangleCount = kMaxIndexCount / 3;

	internal static readonly int kMaxRefineIterations = 48;

	internal static readonly int kMaxSmoothenIterations = 256;

	internal static readonly float kIncrementAreaFactor = 1.2f;

	internal static void Copy<T>(NativeArray<T> src, int srcIndex, NativeArray<T> dst, int dstIndex, int length) where T : struct
	{
		NativeArray<T>.Copy(src, srcIndex, dst, dstIndex, length);
	}

	internal static void Copy<T>(NativeArray<T> src, NativeArray<T> dst, int length) where T : struct
	{
		Copy(src, 0, dst, 0, length);
	}

	internal unsafe static void InsertionSort<T, U>(void* array, int lo, int hi, U comp) where T : struct where U : IComparer<T>
	{
		for (int i = lo; i < hi; i++)
		{
			int num = i;
			T val = UnsafeUtility.ReadArrayElement<T>(array, i + 1);
			while (num >= lo && comp.Compare(val, UnsafeUtility.ReadArrayElement<T>(array, num)) < 0)
			{
				UnsafeUtility.WriteArrayElement(array, num + 1, UnsafeUtility.ReadArrayElement<T>(array, num));
				num--;
			}
			UnsafeUtility.WriteArrayElement(array, num + 1, val);
		}
	}

	internal static int GetLower<T, U, X>(NativeArray<T> values, int count, U check, X condition) where T : struct where U : struct where X : ICondition2<T, U>
	{
		int num = 0;
		int num2 = count - 1;
		int result = num - 1;
		while (num <= num2)
		{
			int num3 = num + num2 >> 1;
			float t = 0f;
			if (condition.Test(values[num3], check, ref t))
			{
				result = num3;
				num = num3 + 1;
			}
			else
			{
				num2 = num3 - 1;
			}
		}
		return result;
	}

	internal static int GetUpper<T, U, X>(NativeArray<T> values, int count, U check, X condition) where T : struct where U : struct where X : ICondition2<T, U>
	{
		int num = 0;
		int num2 = count - 1;
		int result = num2 + 1;
		while (num <= num2)
		{
			int num3 = num + num2 >> 1;
			float t = 0f;
			if (condition.Test(values[num3], check, ref t))
			{
				result = num3;
				num2 = num3 - 1;
			}
			else
			{
				num = num3 + 1;
			}
		}
		return result;
	}

	internal static int GetEqual<T, U, X>(NativeArray<T> values, int count, U check, X condition) where T : struct where U : struct where X : ICondition2<T, U>
	{
		int num = 0;
		int num2 = count - 1;
		while (num <= num2)
		{
			int num3 = num + num2 >> 1;
			float t = 0f;
			condition.Test(values[num3], check, ref t);
			if (t == 0f)
			{
				return num3;
			}
			if (t <= 0f)
			{
				num = num3 + 1;
			}
			else
			{
				num2 = num3 - 1;
			}
		}
		return -1;
	}

	internal static float OrientFast(float2 a, float2 b, float2 c)
	{
		float num = 1.110223E-16f;
		float num2 = (b.y - a.y) * (c.x - b.x) - (b.x - a.x) * (c.y - b.y);
		if (math.abs(num2) < num)
		{
			return 0f;
		}
		return num2;
	}

	internal static double OrientFastDouble(double2 a, double2 b, double2 c)
	{
		double num = 1.1102230246251565E-16;
		double num2 = (b.y - a.y) * (c.x - b.x) - (b.x - a.x) * (c.y - b.y);
		if (math.abs(num2) < num)
		{
			return 0.0;
		}
		return num2;
	}

	internal static UCircle CircumCircle(UTriangle tri)
	{
		float num = tri.va.x * tri.va.x;
		float num2 = tri.vb.x * tri.vb.x;
		float num3 = tri.vc.x * tri.vc.x;
		float num4 = tri.va.y * tri.va.y;
		float num5 = tri.vb.y * tri.vb.y;
		float num6 = tri.vc.y * tri.vc.y;
		float num7 = 2f * ((tri.vb.x - tri.va.x) * (tri.vc.y - tri.va.y) - (tri.vb.y - tri.va.y) * (tri.vc.x - tri.va.x));
		float num8 = ((tri.vc.y - tri.va.y) * (num2 - num + num5 - num4) + (tri.va.y - tri.vb.y) * (num3 - num + num6 - num4)) / num7;
		float num9 = ((tri.va.x - tri.vc.x) * (num2 - num + num5 - num4) + (tri.vb.x - tri.va.x) * (num3 - num + num6 - num4)) / num7;
		float num10 = tri.va.x - num8;
		float num11 = tri.va.y - num9;
		UCircle result = default(UCircle);
		result.center = new float2(num8, num9);
		result.radius = math.sqrt(num10 * num10 + num11 * num11);
		return result;
	}

	internal static bool IsInsideCircle(UCircle c, float2 v)
	{
		return math.distance(v, c.center) < c.radius;
	}

	internal static float TriangleArea(float2 va, float2 vb, float2 vc)
	{
		float3 @float = new float3(va.x, va.y, 0f);
		float3 float2 = new float3(vb.x, vb.y, 0f);
		return math.abs(math.cross(y: @float - new float3(vc.x, vc.y, 0f), x: @float - float2).z) * 0.5f;
	}

	internal static float Sign(float2 p1, float2 p2, float2 p3)
	{
		return (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);
	}

	internal static bool IsInsideTriangle(float2 pt, float2 v1, float2 v2, float2 v3)
	{
		float num = Sign(pt, v1, v2);
		float num2 = Sign(pt, v2, v3);
		float num3 = Sign(pt, v3, v1);
		bool flag = num < 0f || num2 < 0f || num3 < 0f;
		bool flag2 = num > 0f || num2 > 0f || num3 > 0f;
		return !(flag && flag2);
	}

	internal static bool IsInsideTriangleApproximate(float2 pt, float2 v1, float2 v2, float2 v3)
	{
		float num = TriangleArea(v1, v2, v3);
		float num2 = TriangleArea(pt, v1, v2);
		float num3 = TriangleArea(pt, v2, v3);
		float num4 = TriangleArea(pt, v3, v1);
		float num5 = 1.110223E-16f;
		return Mathf.Abs(num - (num2 + num3 + num4)) < num5;
	}

	internal static bool IsInsideCircle(float2 a, float2 b, float2 c, float2 p)
	{
		float num = math.dot(a, a);
		float num2 = math.dot(b, b);
		float num3 = math.dot(c, c);
		float x = a.x;
		float y = a.y;
		float x2 = b.x;
		float y2 = b.y;
		float x3 = c.x;
		float y3 = c.y;
		float num4 = (num * (y3 - y2) + num2 * (y - y3) + num3 * (y2 - y)) / (x * (y3 - y2) + x2 * (y - y3) + x3 * (y2 - y));
		float num5 = (num * (x3 - x2) + num2 * (x - x3) + num3 * (x2 - x)) / (y * (x3 - x2) + y2 * (x - x3) + y3 * (x2 - x));
		float2 y4 = default(float2);
		y4.x = num4 / 2f;
		y4.y = num5 / 2f;
		float num6 = math.distance(a, y4);
		float num7 = math.distance(p, y4);
		return num6 - num7 > 1E-05f;
	}

	internal static void BuildTriangles(NativeArray<float2> vertices, int vertexCount, NativeArray<int> indices, int indexCount, ref NativeArray<UTriangle> triangles, ref int triangleCount, ref float maxArea, ref float avgArea, ref float minArea)
	{
		for (int i = 0; i < indexCount; i += 3)
		{
			UTriangle uTriangle = default(UTriangle);
			int index = indices[i];
			int index2 = indices[i + 1];
			int index3 = indices[i + 2];
			uTriangle.va = vertices[index];
			uTriangle.vb = vertices[index2];
			uTriangle.vc = vertices[index3];
			uTriangle.c = CircumCircle(uTriangle);
			uTriangle.area = TriangleArea(uTriangle.va, uTriangle.vb, uTriangle.vc);
			maxArea = math.max(uTriangle.area, maxArea);
			minArea = math.min(uTriangle.area, minArea);
			avgArea += uTriangle.area;
			triangles[triangleCount++] = uTriangle;
		}
		avgArea /= triangleCount;
	}

	internal static void BuildTriangles(NativeArray<float2> vertices, int vertexCount, NativeArray<int> indices, int indexCount, ref NativeArray<UTriangle> triangles, ref int triangleCount, ref float maxArea, ref float avgArea, ref float minArea, ref float maxEdge, ref float avgEdge, ref float minEdge)
	{
		for (int i = 0; i < indexCount; i += 3)
		{
			UTriangle uTriangle = default(UTriangle);
			int index = indices[i];
			int index2 = indices[i + 1];
			int index3 = indices[i + 2];
			uTriangle.va = vertices[index];
			uTriangle.vb = vertices[index2];
			uTriangle.vc = vertices[index3];
			uTriangle.c = CircumCircle(uTriangle);
			uTriangle.area = TriangleArea(uTriangle.va, uTriangle.vb, uTriangle.vc);
			maxArea = math.max(uTriangle.area, maxArea);
			minArea = math.min(uTriangle.area, minArea);
			avgArea += uTriangle.area;
			float num = math.distance(uTriangle.va, uTriangle.vb);
			float num2 = math.distance(uTriangle.vb, uTriangle.vc);
			float num3 = math.distance(uTriangle.vc, uTriangle.va);
			maxEdge = math.max(num, maxEdge);
			maxEdge = math.max(num2, maxEdge);
			maxEdge = math.max(num3, maxEdge);
			minEdge = math.min(num, minEdge);
			minEdge = math.min(num2, minEdge);
			minEdge = math.min(num3, minEdge);
			avgEdge += num;
			avgEdge += num2;
			avgEdge += num3;
			triangles[triangleCount++] = uTriangle;
		}
		avgArea /= triangleCount;
		avgEdge /= indexCount;
	}

	internal static void BuildTrianglesAndEdges(NativeArray<float2> vertices, int vertexCount, NativeArray<int> indices, int indexCount, ref NativeArray<UTriangle> triangles, ref int triangleCount, ref NativeArray<int4> delaEdges, ref int delaEdgeCount, ref float maxArea, ref float avgArea, ref float minArea)
	{
		for (int i = 0; i < indexCount; i += 3)
		{
			UTriangle uTriangle = default(UTriangle);
			int num = indices[i];
			int num2 = indices[i + 1];
			int num3 = indices[i + 2];
			uTriangle.va = vertices[num];
			uTriangle.vb = vertices[num2];
			uTriangle.vc = vertices[num3];
			uTriangle.c = CircumCircle(uTriangle);
			uTriangle.area = TriangleArea(uTriangle.va, uTriangle.vb, uTriangle.vc);
			maxArea = math.max(uTriangle.area, maxArea);
			minArea = math.min(uTriangle.area, minArea);
			avgArea += uTriangle.area;
			uTriangle.indices = new int3(num, num2, num3);
			delaEdges[delaEdgeCount++] = new int4(math.min(num, num2), math.max(num, num2), triangleCount, -1);
			delaEdges[delaEdgeCount++] = new int4(math.min(num2, num3), math.max(num2, num3), triangleCount, -1);
			delaEdges[delaEdgeCount++] = new int4(math.min(num3, num), math.max(num3, num), triangleCount, -1);
			triangles[triangleCount++] = uTriangle;
		}
		avgArea /= triangleCount;
	}

	private static void CopyGraph(NativeArray<float2> srcPoints, int srcPointCount, ref NativeArray<float2> dstPoints, ref int dstPointCount, NativeArray<int2> srcEdges, int srcEdgeCount, ref NativeArray<int2> dstEdges, ref int dstEdgeCount)
	{
		dstEdgeCount = srcEdgeCount;
		dstPointCount = srcPointCount;
		Copy(srcEdges, dstEdges, srcEdgeCount);
		Copy(srcPoints, dstPoints, srcPointCount);
	}

	private static void CopyGeometry(NativeArray<int> srcIndices, int srcIndexCount, ref NativeArray<int> dstIndices, ref int dstIndexCount, NativeArray<float2> srcVertices, int srcVertexCount, ref NativeArray<float2> dstVertices, ref int dstVertexCount)
	{
		dstIndexCount = srcIndexCount;
		dstVertexCount = srcVertexCount;
		Copy(srcIndices, dstIndices, srcIndexCount);
		Copy(srcVertices, dstVertices, srcVertexCount);
	}

	private static void TransferOutput(NativeArray<int2> srcEdges, int srcEdgeCount, ref NativeArray<int2> dstEdges, ref int dstEdgeCount, NativeArray<int> srcIndices, int srcIndexCount, ref NativeArray<int> dstIndices, ref int dstIndexCount, NativeArray<float2> srcVertices, int srcVertexCount, ref NativeArray<float2> dstVertices, ref int dstVertexCount)
	{
		dstEdgeCount = srcEdgeCount;
		dstIndexCount = srcIndexCount;
		dstVertexCount = srcVertexCount;
		Copy(srcEdges, dstEdges, srcEdgeCount);
		Copy(srcIndices, dstIndices, srcIndexCount);
		Copy(srcVertices, dstVertices, srcVertexCount);
	}

	private static void GraphConditioner(NativeArray<float2> points, ref NativeArray<float2> pgPoints, ref int pgPointCount, ref NativeArray<int2> pgEdges, ref int pgEdgeCount, bool resetTopology)
	{
		float2 @float = new float2(float.PositiveInfinity, float.PositiveInfinity);
		float2 float2 = float2.zero;
		for (int i = 0; i < points.Length; i++)
		{
			@float = math.min(points[i], @float);
			float2 = math.max(points[i], float2);
		}
		float2 float3 = (float2 - @float) * 0.5f;
		float num = 0.0001f;
		pgPointCount = ((!resetTopology) ? pgPointCount : 0);
		int num2 = pgPointCount;
		pgPoints[pgPointCount++] = new float2(@float.x, @float.y);
		pgPoints[pgPointCount++] = new float2(@float.x - num, @float.y + float3.y);
		pgPoints[pgPointCount++] = new float2(@float.x, float2.y);
		pgPoints[pgPointCount++] = new float2(@float.x + float3.x, float2.y + num);
		pgPoints[pgPointCount++] = new float2(float2.x, float2.y);
		pgPoints[pgPointCount++] = new float2(float2.x + num, @float.y + float3.y);
		pgPoints[pgPointCount++] = new float2(float2.x, @float.y);
		pgPoints[pgPointCount++] = new float2(@float.x + float3.x, @float.y - num);
		pgEdgeCount = 8;
		pgEdges[0] = new int2(num2, num2 + 1);
		pgEdges[1] = new int2(num2 + 1, num2 + 2);
		pgEdges[2] = new int2(num2 + 2, num2 + 3);
		pgEdges[3] = new int2(num2 + 3, num2 + 4);
		pgEdges[4] = new int2(num2 + 4, num2 + 5);
		pgEdges[5] = new int2(num2 + 5, num2 + 6);
		pgEdges[6] = new int2(num2 + 6, num2 + 7);
		pgEdges[7] = new int2(num2 + 7, num2);
	}

	private static void Reorder(int startVertexCount, int index, ref NativeArray<int> indices, ref int indexCount, ref NativeArray<float2> vertices, ref int vertexCount)
	{
		bool flag = false;
		for (int i = 0; i < indexCount; i++)
		{
			if (indices[i] == index)
			{
				flag = true;
				break;
			}
		}
		if (flag)
		{
			return;
		}
		vertexCount--;
		vertices[index] = vertices[vertexCount];
		for (int j = 0; j < indexCount; j++)
		{
			if (indices[j] == vertexCount)
			{
				indices[j] = index;
			}
		}
	}

	internal static void VertexCleanupConditioner(int startVertexCount, ref NativeArray<int> indices, ref int indexCount, ref NativeArray<float2> vertices, ref int vertexCount)
	{
		for (int i = startVertexCount; i < vertexCount; i++)
		{
			Reorder(startVertexCount, i, ref indices, ref indexCount, ref vertices, ref vertexCount);
		}
	}

	public static float4 ConvexQuad(Allocator allocator, NativeArray<float2> points, NativeArray<int2> edges, ref NativeArray<float2> outVertices, ref int outVertexCount, ref NativeArray<int> outIndices, ref int outIndexCount, ref NativeArray<int2> outEdges, ref int outEdgeCount)
	{
		float4 zero = float4.zero;
		outEdgeCount = 0;
		outIndexCount = 0;
		outVertexCount = 0;
		if (points.Length < 3 || points.Length >= kMaxVertexCount)
		{
			return zero;
		}
		int pgEdgeCount = 0;
		int pgPointCount = 0;
		NativeArray<int2> pgEdges = new NativeArray<int2>(kMaxEdgeCount, allocator);
		NativeArray<float2> pgPoints = new NativeArray<float2>(kMaxVertexCount, allocator);
		GraphConditioner(points, ref pgPoints, ref pgPointCount, ref pgEdges, ref pgEdgeCount, resetTopology: true);
		Tessellator.Tessellate(allocator, pgPoints, pgPointCount, pgEdges, pgEdgeCount, ref outVertices, ref outVertexCount, ref outIndices, ref outIndexCount);
		pgPoints.Dispose();
		pgEdges.Dispose();
		return zero;
	}

	public static float4 Tessellate(Allocator allocator, NativeArray<float2> points, NativeArray<int2> edges, ref NativeArray<float2> outVertices, ref int outVertexCount, ref NativeArray<int> outIndices, ref int outIndexCount, ref NativeArray<int2> outEdges, ref int outEdgeCount)
	{
		float4 zero = float4.zero;
		outEdgeCount = 0;
		outIndexCount = 0;
		outVertexCount = 0;
		if (points.Length < 3 || points.Length >= kMaxVertexCount)
		{
			return zero;
		}
		bool flag = false;
		bool flag2 = false;
		int outputEdgeCount = 0;
		int outputPointCount = 0;
		NativeArray<int2> outputEdges = new NativeArray<int2>(edges.Length * 8, allocator);
		NativeArray<float2> outputPoints = new NativeArray<float2>(points.Length * 4, allocator);
		if (edges.Length != 0)
		{
			flag = PlanarGraph.Validate(allocator, points, points.Length, edges, edges.Length, ref outputPoints, ref outputPointCount, ref outputEdges, ref outputEdgeCount);
		}
		if (!flag)
		{
			outEdgeCount = edges.Length;
			outVertexCount = points.Length;
			Copy(edges, outEdges, edges.Length);
			Copy(points, outVertices, points.Length);
		}
		if (outputPointCount > 2 && outputEdgeCount > 2)
		{
			NativeArray<int> outputIndices = new NativeArray<int>(outputPointCount * 8, allocator);
			NativeArray<float2> outputVertices = new NativeArray<float2>(outputPointCount * 4, allocator);
			int indexCount = 0;
			int vertexCount = 0;
			if (Tessellator.Tessellate(allocator, outputPoints, outputPointCount, outputEdges, outputEdgeCount, ref outputVertices, ref vertexCount, ref outputIndices, ref indexCount))
			{
				TransferOutput(outputEdges, outputEdgeCount, ref outEdges, ref outEdgeCount, outputIndices, indexCount, ref outIndices, ref outIndexCount, outputVertices, vertexCount, ref outVertices, ref outVertexCount);
				if (flag2)
				{
					outEdgeCount = 0;
				}
			}
			outputVertices.Dispose();
			outputIndices.Dispose();
		}
		outputPoints.Dispose();
		outputEdges.Dispose();
		return zero;
	}
}
