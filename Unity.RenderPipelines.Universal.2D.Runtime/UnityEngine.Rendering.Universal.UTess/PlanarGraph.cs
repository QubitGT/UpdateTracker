using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

namespace UnityEngine.Rendering.Universal.UTess;

[StructLayout(LayoutKind.Sequential, Size = 1)]
internal struct PlanarGraph
{
	private static readonly double kEpsilon = 1E-05;

	private static readonly int kMaxIntersectionTolerance = 4;

	internal unsafe static void RemoveDuplicateEdges(ref NativeArray<int2> edges, ref int edgeCount, NativeArray<int> duplicates, int duplicateCount)
	{
		if (duplicateCount == 0)
		{
			for (int i = 0; i < edgeCount; i++)
			{
				int2 value = edges[i];
				value.x = math.min(edges[i].x, edges[i].y);
				value.y = math.max(edges[i].x, edges[i].y);
				edges[i] = value;
			}
		}
		else
		{
			for (int j = 0; j < edgeCount; j++)
			{
				int2 value2 = edges[j];
				int x = duplicates[value2.x];
				int y = duplicates[value2.y];
				value2.x = math.min(x, y);
				value2.y = math.max(x, y);
				edges[j] = value2;
			}
		}
		ModuleHandle.InsertionSort<int2, TessEdgeCompare>(NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(edges), 0, edgeCount - 1, default(TessEdgeCompare));
		int num = 1;
		for (int k = 1; k < edgeCount; k++)
		{
			int2 @int = edges[k - 1];
			int2 value3 = edges[k];
			if ((value3.x != @int.x || value3.y != @int.y) && value3.x != value3.y)
			{
				edges[num++] = value3;
			}
		}
		edgeCount = num;
	}

	internal static bool CheckCollinear(double2 a0, double2 a1, double2 b0, double2 b1)
	{
		double2 @double = a0;
		double2 double2 = a1;
		double2 double3 = b0;
		double2 double4 = b1;
		double num = (double2.y - @double.y) / (double2.x - @double.x);
		double num2 = (double3.y - @double.y) / (double3.x - @double.x);
		double num3 = (double4.y - @double.y) / (double4.x - @double.x);
		if ((!math.isinf(num) || !math.isinf(num2) || !math.isinf(num3)) && math.abs(num - num2) > kEpsilon)
		{
			return math.abs(num - num3) > kEpsilon;
		}
		return false;
	}

	internal static bool LineLineIntersection(double2 a0, double2 a1, double2 b0, double2 b1)
	{
		double num = ModuleHandle.OrientFastDouble(a0, b0, b1);
		double num2 = ModuleHandle.OrientFastDouble(a1, b0, b1);
		if ((num > kEpsilon && num2 > kEpsilon) || (num < 0.0 - kEpsilon && num2 < 0.0 - kEpsilon))
		{
			return false;
		}
		double num3 = ModuleHandle.OrientFastDouble(b0, a0, a1);
		double num4 = ModuleHandle.OrientFastDouble(b1, a0, a1);
		if ((num3 > kEpsilon && num4 > kEpsilon) || (num3 < 0.0 - kEpsilon && num4 < 0.0 - kEpsilon))
		{
			return false;
		}
		if (math.abs(num) < kEpsilon && math.abs(num2) < kEpsilon && math.abs(num3) < kEpsilon && math.abs(num4) < kEpsilon)
		{
			return CheckCollinear(a0, a1, b0, b1);
		}
		return true;
	}

	internal static bool LineLineIntersection(double2 p1, double2 p2, double2 p3, double2 p4, ref double2 result)
	{
		double num = p2.x - p1.x;
		double num2 = p2.y - p1.y;
		double num3 = p4.x - p3.x;
		double num4 = p4.y - p3.y;
		double num5 = num * num4 - num2 * num3;
		if (math.abs(num5) < kEpsilon)
		{
			return false;
		}
		double num6 = p3.x - p1.x;
		double num7 = p3.y - p1.y;
		double num8 = (num6 * num4 - num7 * num3) / num5;
		if (num8 >= 0.0 - kEpsilon && num8 <= 1.0 + kEpsilon)
		{
			result.x = p1.x + num8 * num;
			result.y = p1.y + num8 * num2;
			return true;
		}
		return false;
	}

	internal unsafe static bool CalculateEdgeIntersections(NativeArray<int2> edges, int edgeCount, NativeArray<double2> points, int pointCount, ref NativeArray<int2> results, ref NativeArray<double2> intersects, ref int resultCount)
	{
		resultCount = 0;
		for (int i = 0; i < edgeCount; i++)
		{
			for (int j = i + 1; j < edgeCount; j++)
			{
				int2 @int = edges[i];
				int2 int2 = edges[j];
				if (@int.x == int2.x || @int.x == int2.y || @int.y == int2.x || @int.y == int2.y)
				{
					continue;
				}
				double2 @double = points[@int.x];
				double2 double2 = points[@int.y];
				double2 double3 = points[int2.x];
				double2 double4 = points[int2.y];
				double2 result = double2.zero;
				if (LineLineIntersection(@double, double2, double3, double4) && LineLineIntersection(@double, double2, double3, double4, ref result))
				{
					if (resultCount >= intersects.Length)
					{
						return false;
					}
					intersects[resultCount] = result;
					results[resultCount++] = new int2(i, j);
				}
			}
		}
		if (resultCount > edgeCount * kMaxIntersectionTolerance)
		{
			return false;
		}
		IntersectionCompare comp = default(IntersectionCompare);
		comp.edges = edges;
		comp.points = points;
		ModuleHandle.InsertionSort<int2, IntersectionCompare>(NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(results), 0, resultCount - 1, comp);
		return true;
	}

	internal static bool CalculateTJunctions(NativeArray<int2> edges, int edgeCount, NativeArray<double2> points, int pointCount, NativeArray<int2> results, ref int resultCount)
	{
		resultCount = 0;
		for (int i = 0; i < edgeCount; i++)
		{
			for (int j = 0; j < pointCount; j++)
			{
				int2 @int = edges[i];
				if (@int.x == j || @int.y == j)
				{
					continue;
				}
				double2 a = points[@int.x];
				double2 a2 = points[@int.y];
				double2 b = points[j];
				double2 b2 = points[j];
				if (LineLineIntersection(a, a2, b, b2))
				{
					if (resultCount >= results.Length)
					{
						return false;
					}
					results[resultCount++] = new int2(i, j);
				}
			}
		}
		return true;
	}

	internal unsafe static bool CutEdges(ref NativeArray<double2> points, ref int pointCount, ref NativeArray<int2> edges, ref int edgeCount, ref NativeArray<int2> tJunctions, ref int tJunctionCount, NativeArray<int2> intersections, NativeArray<double2> intersects, int intersectionCount)
	{
		for (int i = 0; i < intersectionCount; i++)
		{
			int2 @int = intersections[i];
			int x = @int.x;
			int y = @int.y;
			int2 zero = int2.zero;
			zero.x = x;
			zero.y = pointCount;
			tJunctions[tJunctionCount++] = zero;
			int2 zero2 = int2.zero;
			zero2.x = y;
			zero2.y = pointCount;
			tJunctions[tJunctionCount++] = zero2;
			if (pointCount >= points.Length)
			{
				return false;
			}
			points[pointCount++] = intersects[i];
		}
		ModuleHandle.InsertionSort<int2, TessJunctionCompare>(NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(tJunctions), 0, tJunctionCount - 1, default(TessJunctionCompare));
		for (int num = tJunctionCount - 1; num >= 0; num--)
		{
			int2 int2 = tJunctions[num];
			int x2 = int2.x;
			int2 value = edges[x2];
			int num2 = value.x;
			int num3 = value.y;
			double2 @double = points[num2];
			double2 double2 = points[num3];
			if (@double.x - double2.x < 0.0 || (@double.x == double2.x && @double.y - double2.y < 0.0))
			{
				int num4 = num2;
				num2 = num3;
				num3 = num4;
			}
			value.x = num2;
			int x3 = (value.y = int2.y);
			edges[x2] = value;
			while (num > 0 && tJunctions[num - 1].x == x2)
			{
				int y2 = tJunctions[--num].y;
				int2 value2 = default(int2);
				value2.x = x3;
				value2.y = y2;
				edges[edgeCount++] = value2;
				x3 = y2;
			}
			int2 value3 = default(int2);
			value3.x = x3;
			value3.y = num3;
			edges[edgeCount++] = value3;
		}
		return true;
	}

	internal static void RemoveDuplicatePoints(ref NativeArray<double2> points, ref int pointCount, ref NativeArray<int> duplicates, ref int duplicateCount, Allocator allocator)
	{
		TessLink link = TessLink.CreateLink(pointCount, allocator);
		for (int i = 0; i < pointCount; i++)
		{
			for (int j = i + 1; j < pointCount; j++)
			{
				if (math.distance(points[i], points[j]) < kEpsilon)
				{
					link.Link(i, j);
				}
			}
		}
		duplicateCount = 0;
		for (int k = 0; k < pointCount; k++)
		{
			int num = link.Find(k);
			if (num != k)
			{
				duplicateCount++;
				points[num] = math.min(points[k], points[num]);
			}
		}
		if (duplicateCount != 0)
		{
			int num2 = pointCount;
			pointCount = 0;
			for (int l = 0; l < num2; l++)
			{
				if (link.Find(l) == l)
				{
					duplicates[l] = pointCount;
					points[pointCount++] = points[l];
				}
				else
				{
					duplicates[l] = -1;
				}
			}
			for (int m = 0; m < num2; m++)
			{
				if (duplicates[m] < 0)
				{
					duplicates[m] = duplicates[link.Find(m)];
				}
			}
		}
		TessLink.DestroyLink(link);
	}

	internal static bool Validate(Allocator allocator, NativeArray<float2> inputPoints, int pointCount, NativeArray<int2> inputEdges, int edgeCount, ref NativeArray<float2> outputPoints, ref int outputPointCount, ref NativeArray<int2> outputEdges, ref int outputEdgeCount)
	{
		float num = 10000f;
		int num2 = edgeCount;
		bool flag = true;
		bool flag2 = false;
		NativeArray<int> duplicates = new NativeArray<int>(ModuleHandle.kMaxEdgeCount, allocator);
		NativeArray<int2> edges = new NativeArray<int2>(ModuleHandle.kMaxEdgeCount, allocator);
		NativeArray<int2> tJunctions = new NativeArray<int2>(ModuleHandle.kMaxEdgeCount, allocator);
		NativeArray<int2> results = new NativeArray<int2>(ModuleHandle.kMaxEdgeCount, allocator);
		NativeArray<double2> points = new NativeArray<double2>(pointCount * 8, allocator);
		NativeArray<double2> intersects = new NativeArray<double2>(pointCount * 8, allocator);
		for (int i = 0; i < pointCount; i++)
		{
			points[i] = inputPoints[i] * num;
		}
		ModuleHandle.Copy(inputEdges, edges, edgeCount);
		RemoveDuplicateEdges(ref edges, ref edgeCount, duplicates, 0);
		while (flag && --num2 > 0)
		{
			int resultCount = 0;
			flag2 = CalculateEdgeIntersections(edges, edgeCount, points, pointCount, ref results, ref intersects, ref resultCount);
			if (!flag2)
			{
				break;
			}
			int resultCount2 = 0;
			flag2 = CalculateTJunctions(edges, edgeCount, points, pointCount, tJunctions, ref resultCount2);
			if (!flag2)
			{
				break;
			}
			flag2 = CutEdges(ref points, ref pointCount, ref edges, ref edgeCount, ref tJunctions, ref resultCount2, results, intersects, resultCount);
			if (!flag2)
			{
				break;
			}
			int duplicateCount = 0;
			RemoveDuplicatePoints(ref points, ref pointCount, ref duplicates, ref duplicateCount, allocator);
			RemoveDuplicateEdges(ref edges, ref edgeCount, duplicates, duplicateCount);
			flag = resultCount != 0 || resultCount2 != 0;
		}
		if (flag2)
		{
			outputEdgeCount = edgeCount;
			outputPointCount = pointCount;
			ModuleHandle.Copy(edges, outputEdges, edgeCount);
			for (int j = 0; j < pointCount; j++)
			{
				outputPoints[j] = new float2((float)(points[j].x / (double)num), (float)(points[j].y / (double)num));
			}
		}
		edges.Dispose();
		points.Dispose();
		intersects.Dispose();
		duplicates.Dispose();
		tJunctions.Dispose();
		results.Dispose();
		if (flag2)
		{
			return num2 > 0;
		}
		return false;
	}
}
