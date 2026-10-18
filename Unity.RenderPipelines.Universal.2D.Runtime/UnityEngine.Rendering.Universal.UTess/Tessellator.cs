using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

namespace UnityEngine.Rendering.Universal.UTess;

internal struct Tessellator
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	private struct TestHullPointL : ICondition2<UHull, float2>
	{
		public bool Test(UHull h, float2 p, ref float t)
		{
			t = ModuleHandle.OrientFast(h.a, h.b, p);
			return t < 0f;
		}
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	private struct TestHullPointU : ICondition2<UHull, float2>
	{
		public bool Test(UHull h, float2 p, ref float t)
		{
			t = ModuleHandle.OrientFast(h.a, h.b, p);
			return t > 0f;
		}
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	private struct TestHullEventLe : ICondition2<UHull, UEvent>
	{
		public bool Test(UHull h, UEvent p, ref float t)
		{
			t = FindSplit(h, p);
			return t <= 0f;
		}
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	private struct TestHullEventE : ICondition2<UHull, UEvent>
	{
		public bool Test(UHull h, UEvent p, ref float t)
		{
			t = FindSplit(h, p);
			return t == 0f;
		}
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	private struct TestEdgePointE : ICondition2<int2, int2>
	{
		public bool Test(int2 h, int2 p, ref float t)
		{
			t = default(TessEdgeCompare).Compare(h, p);
			return t == 0f;
		}
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	private struct TestCellE : ICondition2<int3, int3>
	{
		public bool Test(int3 h, int3 p, ref float t)
		{
			t = default(TessCellCompare).Compare(h, p);
			return t == 0f;
		}
	}

	private NativeArray<int2> m_Edges;

	private NativeArray<UStar> m_Stars;

	private NativeArray<int3> m_Cells;

	private int m_CellCount;

	private NativeArray<int> m_ILArray;

	private NativeArray<int> m_IUArray;

	private NativeArray<int> m_SPArray;

	private int m_NumEdges;

	private int m_NumHulls;

	private int m_NumPoints;

	private int m_StarCount;

	private NativeArray<int> m_Flags;

	private NativeArray<int> m_Neighbors;

	private NativeArray<int> m_Constraints;

	private Allocator m_Allocator;

	private static float FindSplit(UHull hull, UEvent edge)
	{
		float num = 0f;
		num = ((!(hull.a.x < edge.a.x)) ? ModuleHandle.OrientFast(edge.b, edge.a, hull.a) : ModuleHandle.OrientFast(hull.a, hull.b, edge.a));
		if (0f != num)
		{
			return num;
		}
		num = ((!(edge.b.x < hull.b.x)) ? ModuleHandle.OrientFast(edge.b, edge.a, hull.b) : ModuleHandle.OrientFast(hull.a, hull.b, edge.b));
		if (0f != num)
		{
			return num;
		}
		return hull.idx - edge.idx;
	}

	private void SetAllocator(Allocator allocator)
	{
		m_Allocator = allocator;
	}

	private bool AddPoint(NativeArray<UHull> hulls, int hullCount, NativeArray<float2> points, float2 p, int idx)
	{
		int lower = ModuleHandle.GetLower(hulls, hullCount, p, default(TestHullPointL));
		int upper = ModuleHandle.GetUpper(hulls, hullCount, p, default(TestHullPointU));
		if (lower < 0 || upper < 0)
		{
			return false;
		}
		for (int i = lower; i < upper; i++)
		{
			UHull value = hulls[i];
			int num = value.ilcount;
			while (num > 1 && ModuleHandle.OrientFast(points[value.ilarray[num - 2]], points[value.ilarray[num - 1]], p) > 0f)
			{
				int3 value2 = default(int3);
				value2.x = value.ilarray[num - 1];
				value2.y = value.ilarray[num - 2];
				value2.z = idx;
				m_Cells[m_CellCount++] = value2;
				num--;
			}
			value.ilcount = num + 1;
			if (value.ilcount > value.ilarray.Length)
			{
				return false;
			}
			value.ilarray[num] = idx;
			num = value.iucount;
			while (num > 1 && ModuleHandle.OrientFast(points[value.iuarray[num - 2]], points[value.iuarray[num - 1]], p) < 0f)
			{
				int3 value3 = default(int3);
				value3.x = value.iuarray[num - 2];
				value3.y = value.iuarray[num - 1];
				value3.z = idx;
				m_Cells[m_CellCount++] = value3;
				num--;
			}
			value.iucount = num + 1;
			if (value.iucount > value.iuarray.Length)
			{
				return false;
			}
			value.iuarray[num] = idx;
			hulls[i] = value;
		}
		return true;
	}

	private static void InsertHull(NativeArray<UHull> Hulls, int Pos, ref int Count, UHull Value)
	{
		if (Count < Hulls.Length - 1)
		{
			for (int num = Count; num > Pos; num--)
			{
				Hulls[num] = Hulls[num - 1];
			}
			Hulls[Pos] = Value;
			Count++;
		}
	}

	private static void EraseHull(NativeArray<UHull> Hulls, int Pos, ref int Count)
	{
		if (Count < Hulls.Length)
		{
			for (int i = Pos; i < Count - 1; i++)
			{
				Hulls[i] = Hulls[i + 1];
			}
			Count--;
		}
	}

	private bool SplitHulls(NativeArray<UHull> hulls, ref int hullCount, NativeArray<float2> points, UEvent evt)
	{
		int lower = ModuleHandle.GetLower(hulls, hullCount, evt, default(TestHullEventLe));
		if (lower < 0)
		{
			return false;
		}
		UHull value = hulls[lower];
		UHull value2 = default(UHull);
		value2.a = evt.a;
		value2.b = evt.b;
		value2.idx = evt.idx;
		int value3 = value.iuarray[value.iucount - 1];
		value2.iuarray = new ArraySlice<int>(m_IUArray, value2.idx * m_NumHulls, m_NumHulls);
		value2.iucount = value.iucount;
		for (int i = 0; i < value2.iucount; i++)
		{
			value2.iuarray[i] = value.iuarray[i];
		}
		value.iuarray[0] = value3;
		value.iucount = 1;
		hulls[lower] = value;
		value2.ilarray = new ArraySlice<int>(m_ILArray, value2.idx * m_NumHulls, m_NumHulls);
		value2.ilarray[0] = value3;
		value2.ilcount = 1;
		InsertHull(hulls, lower + 1, ref hullCount, value2);
		return true;
	}

	private bool MergeHulls(NativeArray<UHull> hulls, ref int hullCount, NativeArray<float2> points, UEvent evt)
	{
		float2 a = evt.a;
		evt.a = evt.b;
		evt.b = a;
		int equal = ModuleHandle.GetEqual(hulls, hullCount, evt, default(TestHullEventE));
		if (equal < 0)
		{
			return false;
		}
		UHull uHull = hulls[equal];
		UHull value = hulls[equal - 1];
		value.iucount = uHull.iucount;
		for (int i = 0; i < value.iucount; i++)
		{
			value.iuarray[i] = uHull.iuarray[i];
		}
		hulls[equal - 1] = value;
		EraseHull(hulls, equal, ref hullCount);
		return true;
	}

	private static void InsertUniqueEdge(NativeArray<int2> edges, int2 e, ref int edgeCount)
	{
		TessEdgeCompare tessEdgeCompare = default(TessEdgeCompare);
		bool flag = true;
		int num = 0;
		while (flag && num < edgeCount)
		{
			if (tessEdgeCompare.Compare(e, edges[num]) == 0)
			{
				flag = false;
			}
			num++;
		}
		if (flag)
		{
			edges[edgeCount++] = e;
		}
	}

	private unsafe void PrepareDelaunay(NativeArray<int2> edges, int edgeCount)
	{
		m_StarCount = m_CellCount * 3;
		m_Stars = new NativeArray<UStar>(m_StarCount, m_Allocator);
		m_SPArray = new NativeArray<int>(m_StarCount * m_StarCount, m_Allocator);
		int edgeCount2 = 0;
		NativeArray<int2> edges2 = new NativeArray<int2>(m_StarCount, m_Allocator);
		for (int i = 0; i < edgeCount; i++)
		{
			int2 @int = edges[i];
			@int.x = ((edges[i].x < edges[i].y) ? edges[i].x : edges[i].y);
			@int.y = ((edges[i].x > edges[i].y) ? edges[i].x : edges[i].y);
			edges[i] = @int;
			InsertUniqueEdge(edges2, @int, ref edgeCount2);
		}
		m_Edges = new NativeArray<int2>(edgeCount2, m_Allocator);
		for (int j = 0; j < edgeCount2; j++)
		{
			m_Edges[j] = edges2[j];
		}
		edges2.Dispose();
		ModuleHandle.InsertionSort<int2, TessEdgeCompare>(NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(m_Edges), 0, m_Edges.Length - 1, default(TessEdgeCompare));
		for (int k = 0; k < m_StarCount; k++)
		{
			UStar value = m_Stars[k];
			value.points = new ArraySlice<int>(m_SPArray, k * m_StarCount, m_StarCount);
			value.pointCount = 0;
			m_Stars[k] = value;
		}
		for (int l = 0; l < m_CellCount; l++)
		{
			int x = m_Cells[l].x;
			int y = m_Cells[l].y;
			int z = m_Cells[l].z;
			UStar value2 = m_Stars[x];
			UStar value3 = m_Stars[y];
			UStar value4 = m_Stars[z];
			value2.points[value2.pointCount++] = y;
			value2.points[value2.pointCount++] = z;
			value3.points[value3.pointCount++] = z;
			value3.points[value3.pointCount++] = x;
			value4.points[value4.pointCount++] = x;
			value4.points[value4.pointCount++] = y;
			m_Stars[x] = value2;
			m_Stars[y] = value3;
			m_Stars[z] = value4;
		}
	}

	private int OppositeOf(int a, int b)
	{
		ArraySlice<int> points = m_Stars[b].points;
		int i = 1;
		for (int pointCount = m_Stars[b].pointCount; i < pointCount; i += 2)
		{
			if (points[i] == a)
			{
				return points[i - 1];
			}
		}
		return -1;
	}

	private int FindConstraint(int a, int b)
	{
		int2 check = default(int2);
		check.x = ((a < b) ? a : b);
		check.y = ((a > b) ? a : b);
		return ModuleHandle.GetEqual(m_Edges, m_Edges.Length, check, default(TestEdgePointE));
	}

	private void AddTriangle(int i, int j, int k)
	{
		UStar value = m_Stars[i];
		UStar value2 = m_Stars[j];
		UStar value3 = m_Stars[k];
		value.points[value.pointCount++] = j;
		value.points[value.pointCount++] = k;
		value2.points[value2.pointCount++] = k;
		value2.points[value2.pointCount++] = i;
		value3.points[value3.pointCount++] = i;
		value3.points[value3.pointCount++] = j;
		m_Stars[i] = value;
		m_Stars[j] = value2;
		m_Stars[k] = value3;
	}

	private void RemovePair(int r, int j, int k)
	{
		UStar value = m_Stars[r];
		ArraySlice<int> points = value.points;
		int i = 1;
		for (int pointCount = value.pointCount; i < pointCount; i += 2)
		{
			if (points[i - 1] == j && points[i] == k)
			{
				points[i - 1] = points[pointCount - 2];
				points[i] = points[pointCount - 1];
				value.points = points;
				value.pointCount -= 2;
				m_Stars[r] = value;
				break;
			}
		}
	}

	private void RemoveTriangle(int i, int j, int k)
	{
		RemovePair(i, j, k);
		RemovePair(j, k, i);
		RemovePair(k, i, j);
	}

	private void EdgeFlip(int i, int j)
	{
		int num = OppositeOf(i, j);
		int num2 = OppositeOf(j, i);
		RemoveTriangle(i, j, num);
		RemoveTriangle(j, i, num2);
		AddTriangle(i, num2, num);
		AddTriangle(j, num, num2);
	}

	private bool Flip(NativeArray<float2> points, ref NativeArray<int> stack, ref int stackCount, int a, int b, int x)
	{
		int num = OppositeOf(a, b);
		if (num < 0)
		{
			return true;
		}
		if (b < a)
		{
			int num2 = a;
			a = b;
			b = num2;
			int num3 = x;
			x = num;
			num = num3;
		}
		if (FindConstraint(a, b) != -1)
		{
			return true;
		}
		if (ModuleHandle.IsInsideCircle(points[a], points[b], points[x], points[num]))
		{
			if (2 + stackCount >= stack.Length)
			{
				return false;
			}
			stack[stackCount++] = a;
			stack[stackCount++] = b;
		}
		return true;
	}

	private NativeArray<int3> GetCells(ref int count)
	{
		NativeArray<int3> result = new NativeArray<int3>(m_NumPoints * (m_NumPoints + 1), m_Allocator);
		count = 0;
		int i = 0;
		for (int length = m_Stars.Length; i < length; i++)
		{
			ArraySlice<int> points = m_Stars[i].points;
			int j = 0;
			for (int pointCount = m_Stars[i].pointCount; j < pointCount; j += 2)
			{
				int num = points[j];
				int num2 = points[j + 1];
				if (i < math.min(num, num2))
				{
					int3 value = default(int3);
					value.x = i;
					value.y = num;
					value.z = num2;
					result[count++] = value;
				}
			}
		}
		return result;
	}

	internal bool ApplyDelaunay(NativeArray<float2> points, NativeArray<int2> edges)
	{
		if (m_CellCount == 0)
		{
			return false;
		}
		NativeArray<int> stack = new NativeArray<int>(m_NumPoints * (m_NumPoints + 1), m_Allocator);
		int stackCount = 0;
		bool flag = true;
		PrepareDelaunay(edges, m_NumEdges);
		int num = 0;
		while (flag && num < m_NumPoints)
		{
			UStar uStar = m_Stars[num];
			for (int i = 1; i < uStar.pointCount; i += 2)
			{
				int num2 = uStar.points[i];
				if (num2 < num || FindConstraint(num, num2) >= 0)
				{
					continue;
				}
				int index = uStar.points[i - 1];
				int num3 = -1;
				for (int j = 1; j < uStar.pointCount; j += 2)
				{
					if (uStar.points[j - 1] == num2)
					{
						num3 = uStar.points[j];
						break;
					}
				}
				if (num3 >= 0 && ModuleHandle.IsInsideCircle(points[num], points[num2], points[index], points[num3]))
				{
					if (2 + stackCount >= stack.Length)
					{
						flag = false;
						break;
					}
					stack[stackCount++] = num;
					stack[stackCount++] = num2;
				}
			}
			num++;
		}
		int num4 = m_NumPoints * m_NumPoints;
		while (stackCount > 0 && flag)
		{
			int num5 = stack[stackCount - 1];
			stackCount--;
			int num6 = stack[stackCount - 1];
			stackCount--;
			int num7 = -1;
			int num8 = -1;
			UStar uStar2 = m_Stars[num6];
			for (int k = 1; k < uStar2.pointCount; k += 2)
			{
				int num9 = uStar2.points[k - 1];
				int num10 = uStar2.points[k];
				if (num9 == num5)
				{
					num8 = num10;
				}
				else if (num10 == num5)
				{
					num7 = num9;
				}
			}
			if (num7 >= 0 && num8 >= 0 && ModuleHandle.IsInsideCircle(points[num6], points[num5], points[num7], points[num8]))
			{
				EdgeFlip(num6, num5);
				flag = Flip(points, ref stack, ref stackCount, num7, num6, num8) && Flip(points, ref stack, ref stackCount, num6, num8, num7) && Flip(points, ref stack, ref stackCount, num8, num5, num7) && Flip(points, ref stack, ref stackCount, num5, num7, num8) && --num4 > 0;
			}
		}
		stack.Dispose();
		return flag;
	}

	private int FindNeighbor(NativeArray<int3> cells, int count, int a, int b, int c)
	{
		int num = a;
		int y = b;
		int z = c;
		if (b < c)
		{
			if (b < a)
			{
				num = b;
				y = c;
				z = a;
			}
		}
		else if (c < a)
		{
			num = c;
			y = a;
			z = b;
		}
		if (num < 0)
		{
			return -1;
		}
		int3 check = default(int3);
		check.x = num;
		check.y = y;
		check.z = z;
		return ModuleHandle.GetEqual(cells, count, check, default(TestCellE));
	}

	private unsafe NativeArray<int3> Constrain(ref int count)
	{
		NativeArray<int3> cells = GetCells(ref count);
		int num = count;
		for (int i = 0; i < num; i++)
		{
			int3 value = cells[i];
			int x = value.x;
			int y = value.y;
			int z = value.z;
			if (y < z)
			{
				if (y < x)
				{
					value.x = y;
					value.y = z;
					value.z = x;
				}
			}
			else if (z < x)
			{
				value.x = z;
				value.y = x;
				value.z = y;
			}
			cells[i] = value;
		}
		ModuleHandle.InsertionSort<int3, TessCellCompare>(NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(cells), 0, m_CellCount - 1, default(TessCellCompare));
		m_Flags = new NativeArray<int>(num, m_Allocator);
		m_Neighbors = new NativeArray<int>(num * 3, m_Allocator);
		m_Constraints = new NativeArray<int>(num * 3, m_Allocator);
		NativeArray<int> nativeArray = new NativeArray<int>(num * 3, m_Allocator);
		NativeArray<int> nativeArray2 = new NativeArray<int>(num * 3, m_Allocator);
		int num2 = 1;
		int num3 = 0;
		int num4 = 0;
		for (int j = 0; j < num; j++)
		{
			int3 @int = cells[j];
			for (int k = 0; k < 3; k++)
			{
				int num5 = k;
				int num6 = (k + 1) % 3;
				num5 = ((num5 == 0) ? @int.x : ((k == 1) ? @int.y : @int.z));
				num6 = num6 switch
				{
					1 => @int.y, 
					0 => @int.x, 
					_ => @int.z, 
				};
				int c = OppositeOf(num6, num5);
				int num8 = (m_Neighbors[3 * j + k] = FindNeighbor(cells, count, num6, num5, c));
				int num9 = num8;
				num8 = (m_Constraints[3 * j + k] = ((-1 != FindConstraint(num5, num6)) ? 1 : 0));
				int num11 = num8;
				if (num9 < 0)
				{
					if (num11 != 0)
					{
						nativeArray[num3++] = j;
						continue;
					}
					nativeArray2[num4++] = j;
					m_Flags[j] = 1;
				}
			}
		}
		while (num4 > 0 || num3 > 0)
		{
			while (num4 > 0)
			{
				int num12 = nativeArray2[num4 - 1];
				num4--;
				if (m_Flags[num12] == -num2)
				{
					continue;
				}
				m_Flags[num12] = num2;
				_ = cells[num12];
				for (int l = 0; l < 3; l++)
				{
					int num13 = m_Neighbors[3 * num12 + l];
					if (num13 >= 0 && m_Flags[num13] == 0)
					{
						if (m_Constraints[3 * num12 + l] != 0)
						{
							nativeArray[num3++] = num13;
							continue;
						}
						nativeArray2[num4++] = num13;
						m_Flags[num13] = num2;
					}
				}
			}
			for (int m = 0; m < num3; m++)
			{
				nativeArray2[m] = nativeArray[m];
			}
			num4 = num3;
			num3 = 0;
			num2 = -num2;
		}
		nativeArray2.Dispose();
		nativeArray.Dispose();
		return cells;
	}

	internal NativeArray<int3> RemoveExterior(ref int cellCount)
	{
		int count = 0;
		NativeArray<int3> nativeArray = Constrain(ref count);
		NativeArray<int3> result = new NativeArray<int3>(count, m_Allocator);
		cellCount = 0;
		for (int i = 0; i < count; i++)
		{
			if (m_Flags[i] == -1)
			{
				result[cellCount++] = nativeArray[i];
			}
		}
		nativeArray.Dispose();
		return result;
	}

	internal NativeArray<int3> RemoveInterior(int cellCount)
	{
		int count = 0;
		NativeArray<int3> nativeArray = Constrain(ref count);
		NativeArray<int3> result = new NativeArray<int3>(count, m_Allocator);
		cellCount = 0;
		for (int i = 0; i < count; i++)
		{
			if (m_Flags[i] == 1)
			{
				result[cellCount++] = nativeArray[i];
			}
		}
		nativeArray.Dispose();
		return result;
	}

	internal unsafe bool Triangulate(NativeArray<float2> points, int pointCount, NativeArray<int2> edges, int edgeCount)
	{
		m_NumEdges = edgeCount;
		m_NumHulls = edgeCount * 2;
		m_NumPoints = pointCount;
		m_CellCount = 0;
		m_Cells = new NativeArray<int3>(ModuleHandle.kMaxTriangleCount, m_Allocator);
		m_ILArray = new NativeArray<int>(m_NumHulls * (m_NumHulls + 1), m_Allocator);
		m_IUArray = new NativeArray<int>(m_NumHulls * (m_NumHulls + 1), m_Allocator);
		NativeArray<UHull> hulls = new NativeArray<UHull>(m_NumPoints * 8, m_Allocator);
		int hullCount = 0;
		NativeArray<UEvent> nativeArray = new NativeArray<UEvent>(m_NumPoints + m_NumEdges * 2, m_Allocator);
		int num = 0;
		for (int i = 0; i < m_NumPoints; i++)
		{
			UEvent value = default(UEvent);
			value.a = points[i];
			value.b = default(float2);
			value.idx = i;
			value.type = 0;
			nativeArray[num++] = value;
		}
		for (int j = 0; j < m_NumEdges; j++)
		{
			int2 @int = edges[j];
			float2 @float = points[@int.x];
			float2 float2 = points[@int.y];
			if (@float.x < float2.x)
			{
				UEvent value2 = default(UEvent);
				value2.a = @float;
				value2.b = float2;
				value2.idx = j;
				value2.type = 2;
				UEvent value3 = default(UEvent);
				value3.a = float2;
				value3.b = @float;
				value3.idx = j;
				value3.type = 1;
				nativeArray[num++] = value2;
				nativeArray[num++] = value3;
			}
			else if (@float.x > float2.x)
			{
				UEvent value4 = default(UEvent);
				value4.a = float2;
				value4.b = @float;
				value4.idx = j;
				value4.type = 2;
				UEvent value5 = default(UEvent);
				value5.a = @float;
				value5.b = float2;
				value5.idx = j;
				value5.type = 1;
				nativeArray[num++] = value4;
				nativeArray[num++] = value5;
			}
		}
		ModuleHandle.InsertionSort<UEvent, TessEventCompare>(NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(nativeArray), 0, num - 1, default(TessEventCompare));
		bool flag = true;
		float x = nativeArray[0].a.x - (1f + math.abs(nativeArray[0].a.x)) * math.pow(2f, -16f);
		UHull value6 = default(UHull);
		value6.a.x = x;
		value6.a.y = 1f;
		value6.b.x = x;
		value6.b.y = 0f;
		value6.idx = -1;
		value6.ilarray = new ArraySlice<int>(m_ILArray, m_NumHulls * m_NumHulls, m_NumHulls);
		value6.iuarray = new ArraySlice<int>(m_IUArray, m_NumHulls * m_NumHulls, m_NumHulls);
		value6.ilcount = 0;
		value6.iucount = 0;
		hulls[hullCount++] = value6;
		int k = 0;
		for (int num2 = num; k < num2; k++)
		{
			flag = nativeArray[k].type switch
			{
				0 => AddPoint(hulls, hullCount, points, nativeArray[k].a, nativeArray[k].idx), 
				2 => SplitHulls(hulls, ref hullCount, points, nativeArray[k]), 
				_ => MergeHulls(hulls, ref hullCount, points, nativeArray[k]), 
			};
			if (!flag)
			{
				break;
			}
		}
		nativeArray.Dispose();
		hulls.Dispose();
		return flag;
	}

	internal static bool Tessellate(Allocator allocator, NativeArray<float2> pgPoints, int pgPointCount, NativeArray<int2> pgEdges, int pgEdgeCount, ref NativeArray<float2> outputVertices, ref int vertexCount, ref NativeArray<int> outputIndices, ref int indexCount)
	{
		Tessellator tessellator = default(Tessellator);
		tessellator.SetAllocator(allocator);
		int num = 0;
		int cellCount = 0;
		bool flag = true;
		flag = tessellator.Triangulate(pgPoints, pgPointCount, pgEdges, pgEdgeCount) && tessellator.ApplyDelaunay(pgPoints, pgEdges);
		if (flag)
		{
			NativeArray<int3> nativeArray = tessellator.RemoveExterior(ref cellCount);
			for (int i = 0; i < cellCount; i++)
			{
				ushort num2 = (ushort)nativeArray[i].x;
				ushort num3 = (ushort)nativeArray[i].y;
				ushort num4 = (ushort)nativeArray[i].z;
				if (num2 != num3 && num3 != num4 && num2 != num4)
				{
					outputIndices[indexCount++] = num2;
					outputIndices[indexCount++] = num4;
					outputIndices[indexCount++] = num3;
				}
				num = math.max(math.max(math.max(nativeArray[i].x, nativeArray[i].y), nativeArray[i].z), num);
			}
			num = ((num != 0) ? (num + 1) : 0);
			for (int j = 0; j < num; j++)
			{
				outputVertices[vertexCount++] = pgPoints[j];
			}
			nativeArray.Dispose();
		}
		tessellator.Cleanup();
		return flag;
	}

	internal void Cleanup()
	{
		if (m_Edges.IsCreated)
		{
			m_Edges.Dispose();
		}
		if (m_Stars.IsCreated)
		{
			m_Stars.Dispose();
		}
		if (m_SPArray.IsCreated)
		{
			m_SPArray.Dispose();
		}
		if (m_Cells.IsCreated)
		{
			m_Cells.Dispose();
		}
		if (m_ILArray.IsCreated)
		{
			m_ILArray.Dispose();
		}
		if (m_IUArray.IsCreated)
		{
			m_IUArray.Dispose();
		}
		if (m_Flags.IsCreated)
		{
			m_Flags.Dispose();
		}
		if (m_Neighbors.IsCreated)
		{
			m_Neighbors.Dispose();
		}
		if (m_Constraints.IsCreated)
		{
			m_Constraints.Dispose();
		}
	}
}
