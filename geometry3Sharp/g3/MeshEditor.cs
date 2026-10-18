using System;
using System.Collections;
using System.Collections.Generic;

namespace g3;

public class MeshEditor
{
	public enum DuplicateTriBehavior
	{
		AssertContinue,
		AssertAbort,
		UseExisting,
		Replace
	}

	public DMesh3 Mesh;

	public MeshEditor(DMesh3 mesh)
	{
		Mesh = mesh;
	}

	public virtual int[] AddTriangleStrip(IList<Frame3f> frames, IList<Interval1d> spans, int group_id = -1)
	{
		int count = frames.Count;
		if (count != spans.Count)
		{
			throw new Exception("MeshEditor.AddTriangleStrip: spans list is not the same size!");
		}
		int[] array = new int[2 * (count - 1)];
		int num = -1;
		int v = -1;
		int num2 = 0;
		int num3 = 0;
		for (num2 = 0; num2 < count; num2++)
		{
			Frame3f frame3f = frames[num2];
			Interval1d interval1d = spans[num2];
			Vector3d v2 = frame3f.Origin + (float)interval1d.a * frame3f.Y;
			Vector3d v3 = frame3f.Origin + (float)interval1d.b * frame3f.Y;
			int num4 = Mesh.AppendVertex(v2);
			int num5 = Mesh.AppendVertex(v3);
			if (num != -1)
			{
				array[num3++] = Mesh.AppendTriangle(num, num5, v);
				array[num3++] = Mesh.AppendTriangle(num, num4, num5);
			}
			num = num4;
			v = num5;
		}
		return array;
	}

	public virtual int[] AddTriangleFan_OrderedVertexLoop(int center, int[] vertex_loop, int group_id = -1)
	{
		int num = vertex_loop.Length;
		int[] array = new int[num];
		int num2 = 0;
		num2 = 0;
		while (true)
		{
			if (num2 < num)
			{
				int kk = vertex_loop[num2];
				int jj = vertex_loop[(num2 + 1) % num];
				Index3i tv = new Index3i(center, jj, kk);
				int num3 = Mesh.AppendTriangle(tv, group_id);
				if (num3 < 0)
				{
					break;
				}
				array[num2] = num3;
				num2++;
				continue;
			}
			return array;
		}
		if (num2 > 0 && !remove_triangles(array, num2))
		{
			throw new Exception("MeshEditor.AddTriangleFan_OrderedVertexLoop: failed to add fan, and also falied to back out changes.");
		}
		return null;
	}

	public virtual int[] AddTriangleFan_OrderedEdgeLoop(int center, int[] edge_loop, int group_id = -1)
	{
		int num = edge_loop.Length;
		int[] array = new int[num];
		int num2 = 0;
		num2 = 0;
		while (true)
		{
			if (num2 < num)
			{
				if (!Mesh.IsBoundaryEdge(edge_loop[num2]))
				{
					break;
				}
				Index2i orientedBoundaryEdgeV = Mesh.GetOrientedBoundaryEdgeV(edge_loop[num2]);
				int a = orientedBoundaryEdgeV.a;
				int b = orientedBoundaryEdgeV.b;
				Index3i tv = new Index3i(center, b, a);
				int num3 = Mesh.AppendTriangle(tv, group_id);
				if (num3 < 0)
				{
					break;
				}
				array[num2] = num3;
				num2++;
				continue;
			}
			return array;
		}
		if (num2 > 0 && !remove_triangles(array, num2 - 1))
		{
			throw new Exception("MeshEditor.AddTriangleFan_OrderedEdgeLoop: failed to add fan, and also failed to back out changes.");
		}
		return null;
	}

	public virtual int[] StitchLoop(int[] vloop1, int[] vloop2, int group_id = -1)
	{
		int num = vloop1.Length;
		if (num != vloop2.Length)
		{
			throw new Exception("MeshEditor.StitchLoop: loops are not the same length!!");
		}
		int[] array = new int[num * 2];
		int num2 = 0;
		while (true)
		{
			if (num2 < num)
			{
				int num3 = vloop1[num2];
				int ii = vloop1[(num2 + 1) % num];
				int jj = vloop2[num2];
				int kk = vloop2[(num2 + 1) % num];
				Index3i tv = new Index3i(ii, num3, kk);
				Index3i tv2 = new Index3i(num3, jj, kk);
				int num4 = Mesh.AppendTriangle(tv, group_id);
				int num5 = Mesh.AppendTriangle(tv2, group_id);
				array[2 * num2] = num4;
				array[2 * num2 + 1] = num5;
				if (num4 < 0 || num5 < 0)
				{
					break;
				}
				num2++;
				continue;
			}
			return array;
		}
		if (num2 > 0 && !remove_triangles(array, 2 * num2 + 1))
		{
			throw new Exception("MeshEditor.StitchLoop: failed to add all triangles, and also failed to back out changes.");
		}
		return null;
	}

	public virtual int[] StitchVertexLoops_NearestV(int[] loop0, int[] loop1, int group_id = -1)
	{
		int num = loop0.Length;
		Index2i index2i = Index2i.Zero;
		double num2 = double.MaxValue;
		for (int i = 0; i < num; i++)
		{
			Vector3d vertex = Mesh.GetVertex(loop0[i]);
			for (int j = 0; j < num; j++)
			{
				double num3 = vertex.DistanceSquared(Mesh.GetVertex(loop1[j]));
				if (num3 < num2)
				{
					num2 = num3;
					index2i = new Index2i(i, j);
				}
			}
		}
		if (index2i.a != index2i.b)
		{
			int[] array = new int[num];
			int[] array2 = new int[num];
			for (int k = 0; k < num; k++)
			{
				array[k] = loop0[(index2i.a + k) % num];
				array2[k] = loop1[(index2i.b + k) % num];
			}
			return StitchLoop(array, array2, group_id);
		}
		return StitchLoop(loop0, loop1, group_id);
	}

	public virtual int[] StitchUnorderedEdges(List<Index2i> EdgePairs, int group_id, bool bAbortOnFailure, out bool stitch_incomplete)
	{
		int count = EdgePairs.Count;
		int[] array = new int[count * 2];
		if (!bAbortOnFailure)
		{
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = -1;
			}
		}
		stitch_incomplete = false;
		int num = 0;
		while (true)
		{
			if (num < count)
			{
				Index2i index2i = EdgePairs[num];
				Index4i edge = Mesh.GetEdge(index2i.a);
				if (edge.d != -1)
				{
					if (bAbortOnFailure)
					{
						break;
					}
					stitch_incomplete = true;
				}
				else
				{
					Index3i triangle = Mesh.GetTriangle(edge.c);
					int a = edge.a;
					int b = edge.b;
					IndexUtil.orient_tri_edge(ref a, ref b, triangle);
					Index4i edge2 = Mesh.GetEdge(index2i.b);
					if (edge2.d != -1)
					{
						if (bAbortOnFailure)
						{
							break;
						}
						stitch_incomplete = true;
					}
					else
					{
						Index3i triangle2 = Mesh.GetTriangle(edge2.c);
						int a2 = edge2.a;
						int b2 = edge2.b;
						IndexUtil.orient_tri_edge(ref a2, ref b2, triangle2);
						int num2 = a2;
						a2 = b2;
						b2 = num2;
						Index3i tv = new Index3i(b, a, b2);
						Index3i tv2 = new Index3i(a, a2, b2);
						int num3 = Mesh.AppendTriangle(tv, group_id);
						int num4 = Mesh.AppendTriangle(tv2, group_id);
						if (num3 < 0 || num4 < 0)
						{
							if (bAbortOnFailure)
							{
								break;
							}
							stitch_incomplete = true;
						}
						else
						{
							array[2 * num] = num3;
							array[2 * num + 1] = num4;
						}
					}
				}
				num++;
				continue;
			}
			return array;
		}
		if (num > 0 && !remove_triangles(array, 2 * (num - 1)))
		{
			throw new Exception("MeshEditor.StitchLoop: failed to add all triangles, and also failed to back out changes.");
		}
		return null;
	}

	public virtual int[] StitchUnorderedEdges(List<Index2i> EdgePairs, int group_id = -1, bool bAbortOnFailure = true)
	{
		bool stitch_incomplete = false;
		return StitchUnorderedEdges(EdgePairs, group_id, bAbortOnFailure, out stitch_incomplete);
	}

	public virtual int[] StitchSpan(IList<int> vspan1, IList<int> vspan2, int group_id = -1)
	{
		int count = vspan1.Count;
		if (count != vspan2.Count)
		{
			throw new Exception("MeshEditor.StitchSpan: spans are not the same length!!");
		}
		count--;
		int[] array = new int[count * 2];
		int num = 0;
		while (true)
		{
			if (num < count)
			{
				int num2 = vspan1[num];
				int ii = vspan1[num + 1];
				int jj = vspan2[num];
				int kk = vspan2[num + 1];
				Index3i tv = new Index3i(ii, num2, kk);
				Index3i tv2 = new Index3i(num2, jj, kk);
				int num3 = Mesh.AppendTriangle(tv, group_id);
				int num4 = Mesh.AppendTriangle(tv2, group_id);
				if (num3 < 0 || num4 < 0)
				{
					break;
				}
				array[2 * num] = num3;
				array[2 * num + 1] = num4;
				num++;
				continue;
			}
			return array;
		}
		if (num > 0 && !remove_triangles(array, 2 * (num - 1)))
		{
			throw new Exception("MeshEditor.StitchLoop: failed to add all triangles, and also failed to back out changes.");
		}
		return null;
	}

	public bool RemoveTriangles(IList<int> triangles, bool bRemoveIsolatedVerts)
	{
		bool result = true;
		for (int i = 0; i < triangles.Count; i++)
		{
			if (triangles[i] != -1 && Mesh.RemoveTriangle(triangles[i], bRemoveIsolatedVerts) != 0)
			{
				result = false;
			}
		}
		return result;
	}

	public bool RemoveTriangles(IEnumerable<int> triangles, bool bRemoveIsolatedVerts)
	{
		bool result = true;
		foreach (int triangle in triangles)
		{
			if (!Mesh.IsTriangle(triangle))
			{
				result = false;
			}
			else if (Mesh.RemoveTriangle(triangle, bRemoveIsolatedVerts) != 0)
			{
				result = false;
			}
		}
		return result;
	}

	public bool RemoveTriangles(Func<int, bool> selectorF, bool bRemoveIsolatedVerts)
	{
		bool result = true;
		int maxTriangleID = Mesh.MaxTriangleID;
		for (int i = 0; i < maxTriangleID; i++)
		{
			if (Mesh.IsTriangle(i) && selectorF(i) && Mesh.RemoveTriangle(i, bRemoveIsolatedVerts) != 0)
			{
				result = false;
			}
		}
		return result;
	}

	public static bool RemoveTriangles(DMesh3 Mesh, IList<int> triangles, bool bRemoveIsolatedVerts = true)
	{
		return new MeshEditor(Mesh).RemoveTriangles(triangles, bRemoveIsolatedVerts);
	}

	public static bool RemoveTriangles(DMesh3 Mesh, IEnumerable<int> triangles, bool bRemoveIsolatedVerts = true)
	{
		return new MeshEditor(Mesh).RemoveTriangles(triangles, bRemoveIsolatedVerts);
	}

	public static bool RemoveIsolatedTriangles(DMesh3 mesh)
	{
		return new MeshEditor(mesh).RemoveTriangles(delegate(int tid)
		{
			Index3i triNeighbourTris = mesh.GetTriNeighbourTris(tid);
			return triNeighbourTris.a == -1 && triNeighbourTris.b == -1 && triNeighbourTris.c == -1;
		}, bRemoveIsolatedVerts: true);
	}

	public static int RemoveFinTriangles(DMesh3 mesh, Func<DMesh3, int, bool> removeF = null, bool bRepeatToConvergence = true)
	{
		new MeshEditor(mesh);
		int num = 0;
		List<int> list = new List<int>();
		do
		{
			foreach (int item in mesh.TriangleIndices())
			{
				Index3i triNeighbourTris = mesh.GetTriNeighbourTris(item);
				if (((triNeighbourTris.a != -1) ? 1 : 0) + ((triNeighbourTris.b != -1) ? 1 : 0) + ((triNeighbourTris.c != -1) ? 1 : 0) <= 1 && (removeF == null || removeF(mesh, item)))
				{
					list.Add(item);
				}
			}
			if (list.Count == 0)
			{
				return num;
			}
			num += list.Count;
			RemoveTriangles(mesh, list);
			list.Clear();
		}
		while (bRepeatToConvergence);
		return num;
	}

	public bool SeparateTriangles(IEnumerable<int> triangles, bool bComputeEdgePairs, out List<Index2i> EdgePairs)
	{
		HashSet<int> hashSet = new HashSet<int>(triangles);
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		EdgePairs = null;
		HashSet<int> hashSet2 = null;
		List<Index2i> list = null;
		if (bComputeEdgePairs)
		{
			EdgePairs = new List<Index2i>();
			hashSet2 = new HashSet<int>();
			list = new List<Index2i>();
		}
		foreach (int triangle2 in triangles)
		{
			Index3i triEdges = Mesh.GetTriEdges(triangle2);
			for (int i = 0; i < 3; i++)
			{
				Index2i edgeT = Mesh.GetEdgeT(triEdges[i]);
				if (edgeT.b == -1 || (edgeT.a == triangle2 && hashSet.Contains(edgeT.b)) || (edgeT.b == triangle2 && hashSet.Contains(edgeT.a)))
				{
					triEdges[i] = -1;
				}
			}
			for (int j = 0; j < 3; j++)
			{
				if (triEdges[j] != -1)
				{
					Index2i edgeV = Mesh.GetEdgeV(triEdges[j]);
					if (!dictionary.ContainsKey(edgeV.a))
					{
						dictionary[edgeV.a] = Mesh.AppendVertex(Mesh, edgeV.a);
					}
					if (!dictionary.ContainsKey(edgeV.b))
					{
						dictionary[edgeV.b] = Mesh.AppendVertex(Mesh, edgeV.b);
					}
					if (bComputeEdgePairs && !hashSet2.Contains(triEdges[j]))
					{
						hashSet2.Add(triEdges[j]);
						list.Add(edgeV);
						EdgePairs.Add(new Index2i(triEdges[j], -1));
					}
				}
			}
		}
		foreach (int triangle3 in triangles)
		{
			Index3i triangle = Mesh.GetTriangle(triangle3);
			Index3i index3i = triangle;
			for (int k = 0; k < 3; k++)
			{
				if (dictionary.TryGetValue(triangle[k], out var value))
				{
					index3i[k] = value;
				}
			}
			if (index3i != triangle)
			{
				Mesh.SetTriangle(triangle3, index3i);
			}
		}
		if (bComputeEdgePairs)
		{
			for (int l = 0; l < EdgePairs.Count; l++)
			{
				Index2i index2i = list[l];
				int vA = dictionary[index2i.a];
				int vB = dictionary[index2i.b];
				int jj = Mesh.FindEdge(vA, vB);
				EdgePairs[l] = new Index2i(EdgePairs[l].a, jj);
			}
		}
		return true;
	}

	public List<int> DuplicateTriangles(IEnumerable<int> triangles, ref IndexMap MapV, int group_id = -1)
	{
		List<int> list = new List<int>();
		foreach (int triangle2 in triangles)
		{
			Index3i triangle = Mesh.GetTriangle(triangle2);
			for (int i = 0; i < 3; i++)
			{
				int num = triangle[i];
				if (!MapV.Contains(num))
				{
					int value = Mesh.AppendVertex(Mesh, num);
					MapV[num] = value;
					triangle[i] = value;
				}
				else
				{
					triangle[i] = MapV[num];
				}
			}
			int item = Mesh.AppendTriangle(triangle, group_id);
			list.Add(item);
		}
		return list;
	}

	public void ReverseTriangles(IEnumerable<int> triangles, bool bFlipVtxNormals = true)
	{
		if (!bFlipVtxNormals)
		{
			foreach (int triangle2 in triangles)
			{
				Mesh.ReverseTriOrientation(triangle2);
			}
			return;
		}
		BitArray bitArray = new BitArray(Mesh.MaxVertexID);
		foreach (int triangle3 in triangles)
		{
			Mesh.ReverseTriOrientation(triangle3);
			Index3i triangle = Mesh.GetTriangle(triangle3);
			for (int i = 0; i < 3; i++)
			{
				int num = triangle[i];
				if (!bitArray[num])
				{
					Mesh.SetVertexNormal(num, -Mesh.GetVertexNormal(num));
					bitArray[num] = true;
				}
			}
		}
	}

	public void DisconnectBowtie(int vid)
	{
		List<List<int>> list = new List<List<int>>();
		foreach (int item2 in Mesh.VtxTrianglesItr(vid))
		{
			Index3i triNeighbourTris = Mesh.GetTriNeighbourTris(item2);
			bool flag = false;
			foreach (List<int> item3 in list)
			{
				if (item3.Contains(triNeighbourTris.a) || item3.Contains(triNeighbourTris.b) || item3.Contains(triNeighbourTris.c))
				{
					item3.Add(item2);
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				List<int> item = new List<int> { item2 };
				list.Add(item);
			}
		}
		if (list.Count == 1)
		{
			return;
		}
		list.Sort(bowtie_sorter);
		for (int i = 1; i < list.Count; i++)
		{
			int num = Mesh.AppendVertex(Mesh, vid);
			foreach (int item4 in list[i])
			{
				Index3i triangle = Mesh.GetTriangle(item4);
				if (triangle.a == vid)
				{
					triangle.a = num;
				}
				else if (triangle.b == vid)
				{
					triangle.b = num;
				}
				else
				{
					triangle.c = num;
				}
				Mesh.SetTriangle(item4, triangle, bRemoveIsolatedVertices: false);
			}
		}
	}

	private static int bowtie_sorter(List<int> l1, List<int> l2)
	{
		if (l1.Count == l2.Count)
		{
			return 0;
		}
		if (l1.Count <= l2.Count)
		{
			return 1;
		}
		return -1;
	}

	public int DisconnectAllBowties(int nMaxIters = 10)
	{
		List<int> list = new List<int>(MeshIterators.BowtieVertices(Mesh));
		int num = 0;
		while (list.Count > 0 && num++ < nMaxIters)
		{
			foreach (int item in list)
			{
				DisconnectBowtie(item);
			}
			list = new List<int>(MeshIterators.BowtieVertices(Mesh));
		}
		return list.Count;
	}

	public bool ReinsertSubmesh(DSubmesh3 sub, ref int[] new_tris, out IndexMap SubToNewV, DuplicateTriBehavior eDuplicateBehavior = DuplicateTriBehavior.AssertAbort)
	{
		if (sub.BaseBorderV == null)
		{
			throw new Exception("MeshEditor.ReinsertSubmesh: Submesh does not have required boundary info. Call ComputeBoundaryInfo()!");
		}
		DMesh3 subMesh = sub.SubMesh;
		bool result = true;
		IndexFlagSet indexFlagSet = new IndexFlagSet(subMesh.MaxVertexID, subMesh.TriangleCount / 2);
		SubToNewV = new IndexMap(subMesh.MaxVertexID, subMesh.VertexCount);
		int num = 0;
		int maxTriangleID = subMesh.MaxTriangleID;
		for (int i = 0; i < maxTriangleID; i++)
		{
			if (!subMesh.IsTriangle(i))
			{
				continue;
			}
			Index3i triangle = subMesh.GetTriangle(i);
			int triangleGroup = subMesh.GetTriangleGroup(i);
			Index3i zero = Index3i.Zero;
			for (int j = 0; j < 3; j++)
			{
				int num2 = triangle[j];
				int num3 = -1;
				if (!indexFlagSet[num2])
				{
					if (subMesh.IsBoundaryVertex(num2))
					{
						int num4 = ((num2 < sub.SubToBaseV.size) ? sub.SubToBaseV[num2] : (-1));
						if (num4 >= 0 && Mesh.IsVertex(num4) && sub.BaseBorderV[num4] && Mesh.IsBoundaryVertex(num4))
						{
							num3 = num4;
						}
					}
					if (num3 == -1)
					{
						num3 = Mesh.AppendVertex(subMesh, num2);
					}
					SubToNewV[num2] = num3;
					indexFlagSet[num2] = true;
				}
				else
				{
					num3 = SubToNewV[num2];
				}
				zero[j] = num3;
			}
			if (eDuplicateBehavior != 0)
			{
				int num5 = Mesh.FindTriangle(zero.a, zero.b, zero.c);
				if (num5 != -1)
				{
					switch (eDuplicateBehavior)
					{
					case DuplicateTriBehavior.AssertAbort:
						return false;
					case DuplicateTriBehavior.UseExisting:
						if (new_tris != null)
						{
							new_tris[num++] = num5;
						}
						continue;
					case DuplicateTriBehavior.Replace:
						Mesh.RemoveTriangle(num5, bRemoveIsolatedVertices: false);
						break;
					}
				}
			}
			int num6 = Mesh.AppendTriangle(zero, triangleGroup);
			if (!Mesh.IsTriangle(num6))
			{
				result = false;
			}
			if (new_tris != null)
			{
				new_tris[num++] = num6;
			}
		}
		return result;
	}

	public bool AppendMesh(IMesh appendMesh, int appendGID = -1)
	{
		int[] mapV;
		return AppendMesh(appendMesh, out mapV, appendGID);
	}

	public bool AppendMesh(IMesh appendMesh, out int[] mapV, int appendGID = -1)
	{
		mapV = new int[appendMesh.MaxVertexID];
		foreach (int item in appendMesh.VertexIndices())
		{
			NewVertexInfo vertexAll = appendMesh.GetVertexAll(item);
			int num = Mesh.AppendVertex(vertexAll);
			mapV[item] = num;
		}
		foreach (int item2 in appendMesh.TriangleIndices())
		{
			Index3i triangle = appendMesh.GetTriangle(item2);
			triangle.a = mapV[triangle.a];
			triangle.b = mapV[triangle.b];
			triangle.c = mapV[triangle.c];
			int gid = appendMesh.GetTriangleGroup(item2);
			if (appendGID >= 0)
			{
				gid = appendGID;
			}
			Mesh.AppendTriangle(triangle, gid);
		}
		return true;
	}

	public static DMesh3 Combine(params IMesh[] appendMeshes)
	{
		DMesh3 dMesh = new DMesh3();
		MeshEditor meshEditor = new MeshEditor(dMesh);
		foreach (IMesh appendMesh in appendMeshes)
		{
			meshEditor.AppendMesh(appendMesh, dMesh.AllocateTriangleGroup());
		}
		return dMesh;
	}

	public static void Append(DMesh3 appendTo, DMesh3 append)
	{
		new MeshEditor(appendTo).AppendMesh(append, appendTo.AllocateTriangleGroup());
	}

	public bool AppendMesh(IMesh appendMesh, IndexMap mergeMapV, out int[] mapV, int appendGID = -1)
	{
		mapV = new int[appendMesh.MaxVertexID];
		foreach (int item in appendMesh.VertexIndices())
		{
			if (mergeMapV.Contains(item))
			{
				mapV[item] = mergeMapV[item];
				continue;
			}
			NewVertexInfo vertexAll = appendMesh.GetVertexAll(item);
			int num = Mesh.AppendVertex(vertexAll);
			mapV[item] = num;
		}
		foreach (int item2 in appendMesh.TriangleIndices())
		{
			Index3i triangle = appendMesh.GetTriangle(item2);
			triangle.a = mapV[triangle.a];
			triangle.b = mapV[triangle.b];
			triangle.c = mapV[triangle.c];
			int gid = appendMesh.GetTriangleGroup(item2);
			if (appendGID >= 0)
			{
				gid = appendGID;
			}
			Mesh.AppendTriangle(triangle, gid);
		}
		return true;
	}

	public void AppendBox(Frame3f frame, float size)
	{
		AppendBox(frame, size * Vector3f.One);
	}

	public void AppendBox(Frame3f frame, Vector3f size)
	{
		AppendBox(frame, size, Colorf.White);
	}

	public void AppendBox(Frame3f frame, Vector3f size, Colorf color)
	{
		TrivialBox3Generator trivialBox3Generator = new TrivialBox3Generator();
		trivialBox3Generator.Box = new Box3d(frame, size);
		trivialBox3Generator.NoSharedVertices = false;
		trivialBox3Generator.Generate();
		DMesh3 dMesh = new DMesh3();
		trivialBox3Generator.MakeMesh(dMesh);
		if (Mesh.HasVertexColors)
		{
			dMesh.EnableVertexColors(color);
		}
		AppendMesh(dMesh, Mesh.AllocateTriangleGroup());
	}

	public void AppendLine(Segment3d seg, float size)
	{
		Frame3f frame = new Frame3f(seg.Center);
		frame.AlignAxis(2, (Vector3f)seg.Direction);
		AppendBox(frame, new Vector3f(size, size, seg.Extent));
	}

	public void AppendLine(Segment3d seg, float size, Colorf color)
	{
		Frame3f frame = new Frame3f(seg.Center);
		frame.AlignAxis(2, (Vector3f)seg.Direction);
		AppendBox(frame, new Vector3f(size, size, seg.Extent), color);
	}

	public static void AppendBox(DMesh3 mesh, Vector3d pos, float size)
	{
		new MeshEditor(mesh).AppendBox(new Frame3f(pos), size);
	}

	public static void AppendBox(DMesh3 mesh, Vector3d pos, float size, Colorf color)
	{
		new MeshEditor(mesh).AppendBox(new Frame3f(pos), size * Vector3f.One, color);
	}

	public static void AppendBox(DMesh3 mesh, Vector3d pos, Vector3d normal, float size)
	{
		new MeshEditor(mesh).AppendBox(new Frame3f(pos, normal), size);
	}

	public static void AppendBox(DMesh3 mesh, Vector3d pos, Vector3d normal, float size, Colorf color)
	{
		new MeshEditor(mesh).AppendBox(new Frame3f(pos, normal), size * Vector3f.One, color);
	}

	public static void AppendBox(DMesh3 mesh, Frame3f frame, Vector3f size, Colorf color)
	{
		new MeshEditor(mesh).AppendBox(frame, size, color);
	}

	public static void AppendLine(DMesh3 mesh, Segment3d seg, float size)
	{
		Frame3f frame = new Frame3f(seg.Center);
		frame.AlignAxis(2, (Vector3f)seg.Direction);
		new MeshEditor(mesh).AppendBox(frame, new Vector3f(size, size, seg.Extent));
	}

	public void AppendPathSolid(IEnumerable<Vector3d> vertices, double radius, Colorf color)
	{
		DMesh3 dMesh = new TubeGenerator
		{
			Vertices = new List<Vector3d>(vertices),
			Polygon = Polygon2d.MakeCircle(radius, 6),
			NoSharedVertices = false
		}.Generate().MakeDMesh();
		if (Mesh.HasVertexColors)
		{
			dMesh.EnableVertexColors(color);
		}
		AppendMesh(dMesh, Mesh.AllocateTriangleGroup());
	}

	public bool RemoveAllBowtieVertices(bool bRepeatUntilClean)
	{
		int num = 0;
		do
		{
			List<int> list = new List<int>();
			foreach (int item in Mesh.VertexIndices())
			{
				if (Mesh.IsBowtieVertex(item))
				{
					list.Add(item);
				}
			}
			if (list.Count == 0)
			{
				break;
			}
			foreach (int item2 in list)
			{
				Mesh.RemoveVertex(item2);
				num++;
			}
		}
		while (bRepeatUntilClean);
		return num > 0;
	}

	public int RemoveUnusedVertices()
	{
		int num = 0;
		int maxVertexID = Mesh.MaxVertexID;
		for (int i = 0; i < maxVertexID; i++)
		{
			if (Mesh.IsVertex(i) && Mesh.GetVtxEdgeCount(i) == 0)
			{
				Mesh.RemoveVertex(i);
				num++;
			}
		}
		return num;
	}

	public static int RemoveUnusedVertices(DMesh3 mesh)
	{
		return new MeshEditor(mesh).RemoveUnusedVertices();
	}

	public int RemoveSmallComponents(double min_volume, double min_area)
	{
		MeshConnectedComponents meshConnectedComponents = new MeshConnectedComponents(Mesh);
		meshConnectedComponents.FindConnectedT();
		if (meshConnectedComponents.Count == 1)
		{
			return 0;
		}
		int num = 0;
		foreach (MeshConnectedComponents.Component component in meshConnectedComponents.Components)
		{
			Vector2d vector2d = MeshMeasurements.VolumeArea(Mesh, component.Indices, Mesh.GetVertex);
			if (vector2d.x < min_volume || vector2d.y < min_area)
			{
				RemoveTriangles(Mesh, component.Indices);
				num++;
			}
		}
		return num;
	}

	public static int RemoveSmallComponents(DMesh3 mesh, double min_volume, double min_area)
	{
		return new MeshEditor(mesh).RemoveSmallComponents(min_volume, min_area);
	}

	private bool remove_triangles(int[] tri_list, int count)
	{
		for (int i = 0; i < count; i++)
		{
			if (Mesh.IsTriangle(tri_list[i]) && Mesh.RemoveTriangle(tri_list[i], bRemoveIsolatedVertices: false) != 0)
			{
				return false;
			}
		}
		return true;
	}
}
