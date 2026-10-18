using System;
using System.Collections;

namespace g3;

public class MeshDecomposition
{
	public struct Component
	{
		public int id;

		public int[] triangles;

		public int tri_count;

		public int[] source_vertices;
	}

	private DMesh3 mesh;

	public bool TrackVertexMapping = true;

	private Index2i[] mapTo;

	private DVector<int> mapToMulti;

	public int MaxComponentSize { get; set; }

	public IMeshComponentManager Manager { get; set; }

	public MeshDecomposition(DMesh3 mesh, IMeshComponentManager manager)
	{
		MaxComponentSize = 62000;
		this.mesh = mesh;
		Manager = manager;
	}

	public void BuildLinear()
	{
		int maxVertexID = mesh.MaxVertexID;
		if (TrackVertexMapping)
		{
			mapTo = new Index2i[maxVertexID];
			for (int i = 0; i < maxVertexID; i++)
			{
				mapTo[i] = Index2i.Zero;
			}
			mapToMulti = new DVector<int>();
		}
		int[] mapToCur = new int[maxVertexID];
		Array.Clear(mapToCur, 0, mapToCur.Length);
		_ = mesh.MaxTriangleID;
		int[] cur_subt = new int[MaxComponentSize];
		int subti = 0;
		int subi = 1;
		int[] cur_subv = new int[MaxComponentSize];
		BitArray vert_bits = new BitArray(mesh.MaxVertexID);
		int subvcount = 0;
		Action action = delegate
		{
			Index2i mapRange;
			int max_subv;
			Component c = extract_submesh(subi++, cur_subt, subti, mapToCur, cur_subv, out mapRange, out max_subv);
			Manager.AddComponent(c);
			Array.Clear(cur_subt, 0, subti);
			subti = 0;
			Array.Clear(mapToCur, mapRange.a, mapRange.b - mapRange.a + 1);
			Array.Clear(cur_subv, 0, max_subv);
			subvcount = 0;
			vert_bits.SetAll(value: false);
		};
		int[] tri_order_by_axis_sort = get_tri_order_by_axis_sort();
		int num = tri_order_by_axis_sort.Length;
		for (int j = 0; j < num; j++)
		{
			int num2 = tri_order_by_axis_sort[j];
			Index3i triangle = mesh.GetTriangle(num2);
			if (!vert_bits[triangle.a])
			{
				vert_bits[triangle.a] = true;
				subvcount++;
			}
			if (!vert_bits[triangle.b])
			{
				vert_bits[triangle.b] = true;
				subvcount++;
			}
			if (!vert_bits[triangle.c])
			{
				vert_bits[triangle.c] = true;
				subvcount++;
			}
			cur_subt[subti++] = num2;
			if (subti == MaxComponentSize || subvcount > MaxComponentSize - 3)
			{
				action();
			}
		}
		if (subti > 0)
		{
			action();
		}
	}

	private int[] get_tri_order_by_axis_sort()
	{
		int num = 0;
		int[] array = new int[mesh.TriangleCount];
		int maxTriangleID = mesh.MaxTriangleID;
		for (int i = 0; i < maxTriangleID; i++)
		{
			if (mesh.IsTriangle(i))
			{
				array[num++] = i;
			}
		}
		Vector3d[] centroids = new Vector3d[mesh.MaxTriangleID];
		gParallel.ForEach(mesh.TriangleIndices(), delegate(int ti)
		{
			if (mesh.IsTriangle(ti))
			{
				centroids[ti] = mesh.GetTriCentroid(ti);
			}
		});
		Array.Sort(array, delegate(int t0, int t1)
		{
			double x = centroids[t0].x;
			double x2 = centroids[t1].x;
			return (x != x2) ? ((!(x < x2)) ? 1 : (-1)) : 0;
		});
		return array;
	}

	private Component extract_submesh(int submesh_index, int[] subt, int Nt, int[] mapToCur, int[] subv, out Index2i mapRange, out int max_subv)
	{
		int num = 0;
		Component result = default(Component);
		result.id = submesh_index;
		result.triangles = new int[Nt * 3];
		result.tri_count = Nt;
		mapRange = new Index2i(int.MaxValue, int.MinValue);
		for (int i = 0; i < Nt; i++)
		{
			int tID = subt[i];
			Index3i triangle = mesh.GetTriangle(tID);
			for (int j = 0; j < 3; j++)
			{
				int num2 = triangle[j];
				if (mapToCur[num2] == 0)
				{
					mapToCur[num2] = num + 1;
					subv[num] = num2;
					if (num2 < mapRange.a)
					{
						mapRange.a = num2;
					}
					else if (num2 > mapRange.b)
					{
						mapRange.b = num2;
					}
					if (TrackVertexMapping)
					{
						add_submesh_mapv(num2, result.id, num);
					}
					num++;
				}
				result.triangles[3 * i + j] = mapToCur[num2] - 1;
			}
		}
		result.source_vertices = new int[num];
		Array.Copy(subv, result.source_vertices, num);
		max_subv = num;
		return result;
	}

	private void add_submesh_mapv(int orig_vid, int submesh_i, int submesh_vid)
	{
		if (mapTo[orig_vid].a == 0)
		{
			mapTo[orig_vid].a = submesh_i;
			mapTo[orig_vid].b = submesh_vid;
		}
		else if (mapTo[orig_vid].a > 0)
		{
			int size = mapToMulti.size;
			mapToMulti.push_back(mapTo[orig_vid].a);
			mapToMulti.push_back(mapTo[orig_vid].b);
			mapToMulti.push_back(-1);
			int size2 = mapToMulti.size;
			mapToMulti.push_back(submesh_i);
			mapToMulti.push_back(submesh_vid);
			mapToMulti.push_back(size);
			mapTo[orig_vid].a = -2;
			mapTo[orig_vid].b = size2;
		}
		else
		{
			mapTo[orig_vid].a--;
			int b = mapTo[orig_vid].b;
			int size3 = mapToMulti.size;
			mapToMulti.push_back(submesh_i);
			mapToMulti.push_back(submesh_vid);
			mapToMulti.push_back(b);
			mapTo[orig_vid].b = size3;
		}
	}
}
