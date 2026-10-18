using System;
using System.Collections;
using System.Collections.Generic;

namespace g3;

public class MeshFaceSelection : IEnumerable<int>, IEnumerable
{
	public DMesh3 Mesh;

	private HashSet<int> Selected;

	private List<int> temp;

	private List<int> temp2;

	private BitArray tempBits;

	protected BitArray Bitmap
	{
		get
		{
			if (tempBits == null)
			{
				tempBits = new BitArray(Mesh.MaxTriangleID);
			}
			return tempBits;
		}
	}

	public int Count => Selected.Count;

	public MeshFaceSelection(DMesh3 mesh)
	{
		Mesh = mesh;
		Selected = new HashSet<int>();
		temp = new List<int>();
		temp2 = new List<int>();
	}

	public MeshFaceSelection(MeshFaceSelection copy)
	{
		Mesh = copy.Mesh;
		Selected = new HashSet<int>(copy.Selected);
		temp = new List<int>();
		temp2 = new List<int>();
	}

	public MeshFaceSelection(DMesh3 mesh, MeshVertexSelection convertV, int minCount = 3)
		: this(mesh)
	{
		minCount = MathUtil.Clamp(minCount, 1, 3);
		if (minCount == 1)
		{
			foreach (int item in convertV)
			{
				foreach (int item2 in mesh.VtxTrianglesItr(item))
				{
					add(item2);
				}
			}
			return;
		}
		foreach (int item3 in mesh.TriangleIndices())
		{
			Index3i triangle = mesh.GetTriangle(item3);
			if (minCount == 3)
			{
				if (convertV.IsSelected(triangle.a) && convertV.IsSelected(triangle.b) && convertV.IsSelected(triangle.c))
				{
					add(item3);
				}
			}
			else if ((convertV.IsSelected(triangle.a) ? 1 : 0) + (convertV.IsSelected(triangle.b) ? 1 : 0) + (convertV.IsSelected(triangle.c) ? 1 : 0) >= minCount)
			{
				add(item3);
			}
		}
	}

	public MeshFaceSelection(DMesh3 mesh, int group_id)
		: this(mesh)
	{
		SelectGroup(group_id);
	}

	public IEnumerator<int> GetEnumerator()
	{
		return Selected.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return Selected.GetEnumerator();
	}

	private void add(int tid)
	{
		Selected.Add(tid);
	}

	private void remove(int tid)
	{
		Selected.Remove(tid);
	}

	public bool IsSelected(int tid)
	{
		return Selected.Contains(tid);
	}

	public void Select(int tid)
	{
		if (Mesh.IsTriangle(tid))
		{
			add(tid);
		}
	}

	public void Select(int[] triangles)
	{
		for (int i = 0; i < triangles.Length; i++)
		{
			if (Mesh.IsTriangle(triangles[i]))
			{
				add(triangles[i]);
			}
		}
	}

	public void Select(List<int> triangles)
	{
		for (int i = 0; i < triangles.Count; i++)
		{
			if (Mesh.IsTriangle(triangles[i]))
			{
				add(triangles[i]);
			}
		}
	}

	public void Select(IEnumerable<int> triangles)
	{
		foreach (int triangle in triangles)
		{
			if (Mesh.IsTriangle(triangle))
			{
				add(triangle);
			}
		}
	}

	public void Select(Func<int, bool> selectF)
	{
		temp.Clear();
		int maxTriangleID = Mesh.MaxTriangleID;
		for (int i = 0; i < maxTriangleID; i++)
		{
			if (Mesh.IsTriangle(i) && selectF(i))
			{
				temp.Add(i);
			}
		}
		Select(temp);
	}

	public void SelectVertexOneRing(int vid)
	{
		foreach (int item in Mesh.VtxTrianglesItr(vid))
		{
			add(item);
		}
	}

	public void SelectVertexOneRings(int[] vertices)
	{
		foreach (int vID in vertices)
		{
			foreach (int item in Mesh.VtxTrianglesItr(vID))
			{
				add(item);
			}
		}
	}

	public void SelectVertexOneRings(IEnumerable<int> vertices)
	{
		foreach (int vertex in vertices)
		{
			foreach (int item in Mesh.VtxTrianglesItr(vertex))
			{
				add(item);
			}
		}
	}

	public void SelectEdgeTris(int eid)
	{
		Index2i edgeT = Mesh.GetEdgeT(eid);
		add(edgeT.a);
		if (edgeT.b != -1)
		{
			add(edgeT.b);
		}
	}

	public void Deselect(int tid)
	{
		remove(tid);
	}

	public void Deselect(int[] triangles)
	{
		for (int i = 0; i < triangles.Length; i++)
		{
			remove(triangles[i]);
		}
	}

	public void Deselect(IEnumerable<int> triangles)
	{
		foreach (int triangle in triangles)
		{
			remove(triangle);
		}
	}

	public void DeselectAll()
	{
		Selected.Clear();
	}

	public void SelectGroup(int gid)
	{
		int maxTriangleID = Mesh.MaxTriangleID;
		for (int i = 0; i < maxTriangleID; i++)
		{
			if (Mesh.IsTriangle(i) && Mesh.GetTriangleGroup(i) == gid)
			{
				add(i);
			}
		}
	}

	public void SelectGroupInverse(int gid)
	{
		int maxTriangleID = Mesh.MaxTriangleID;
		for (int i = 0; i < maxTriangleID; i++)
		{
			if (Mesh.IsTriangle(i) && Mesh.GetTriangleGroup(i) != gid)
			{
				add(i);
			}
		}
	}

	public void DeselectGroup(int gid)
	{
		int maxTriangleID = Mesh.MaxTriangleID;
		for (int i = 0; i < maxTriangleID; i++)
		{
			if (Mesh.IsTriangle(i) && Mesh.GetTriangleGroup(i) == gid)
			{
				remove(i);
			}
		}
	}

	public int[] ToArray()
	{
		int[] array = new int[Selected.Count];
		int num = 0;
		foreach (int item in Selected)
		{
			array[num++] = item;
		}
		return array;
	}

	public List<int> FindNeighbourTris()
	{
		List<int> list = new List<int>();
		foreach (int item in Selected)
		{
			Index3i triNeighbourTris = Mesh.GetTriNeighbourTris(item);
			for (int i = 0; i < 3; i++)
			{
				if (triNeighbourTris[i] != -1 && !IsSelected(triNeighbourTris[i]))
				{
					list.Add(triNeighbourTris[i]);
				}
			}
		}
		return list;
	}

	public List<int> FindBorderTris()
	{
		List<int> list = new List<int>();
		foreach (int item in Selected)
		{
			Index3i triNeighbourTris = Mesh.GetTriNeighbourTris(item);
			if (!IsSelected(triNeighbourTris.a) || !IsSelected(triNeighbourTris.b) || !IsSelected(triNeighbourTris.c))
			{
				list.Add(item);
			}
		}
		return list;
	}

	public void ExpandToFaceNeighbours(Func<int, bool> FilterF = null)
	{
		temp.Clear();
		foreach (int item in Selected)
		{
			Index3i triNeighbourTris = Mesh.GetTriNeighbourTris(item);
			for (int i = 0; i < 3; i++)
			{
				if ((FilterF == null || FilterF(triNeighbourTris[i])) && triNeighbourTris[i] != -1 && !IsSelected(triNeighbourTris[i]))
				{
					temp.Add(triNeighbourTris[i]);
				}
			}
		}
		for (int j = 0; j < temp.Count; j++)
		{
			add(temp[j]);
		}
	}

	public void ExpandToFaceNeighbours(int rounds, Func<int, bool> FilterF = null)
	{
		for (int i = 0; i < rounds; i++)
		{
			ExpandToFaceNeighbours(FilterF);
		}
	}

	public void ExpandToOneRingNeighbours(Func<int, bool> FilterF = null)
	{
		temp.Clear();
		foreach (int item in Selected)
		{
			Index3i triangle = Mesh.GetTriangle(item);
			for (int i = 0; i < 3; i++)
			{
				int vID = triangle[i];
				foreach (int item2 in Mesh.VtxTrianglesItr(vID))
				{
					if ((FilterF == null || FilterF(item2)) && !IsSelected(item2))
					{
						temp.Add(item2);
					}
				}
			}
		}
		for (int j = 0; j < temp.Count; j++)
		{
			add(temp[j]);
		}
	}

	public void ExpandToOneRingNeighbours(int nRings, Func<int, bool> FilterF = null)
	{
		if (nRings == 1)
		{
			ExpandToOneRingNeighbours(FilterF);
			return;
		}
		List<int> list = temp;
		List<int> list2 = temp2;
		list2.Clear();
		list2.AddRange(Selected);
		Bitmap.SetAll(value: false);
		foreach (int item in Selected)
		{
			Bitmap.Set(item, value: true);
		}
		for (int i = 0; i < nRings; i++)
		{
			list.Clear();
			foreach (int item2 in list2)
			{
				Index3i triangle = Mesh.GetTriangle(item2);
				for (int j = 0; j < 3; j++)
				{
					int vID = triangle[j];
					foreach (int item3 in Mesh.VtxTrianglesItr(vID))
					{
						if ((FilterF == null || FilterF(item3)) && !Bitmap.Get(item3))
						{
							list.Add(item3);
							Bitmap.Set(item3, value: true);
						}
					}
				}
			}
			for (int k = 0; k < list.Count; k++)
			{
				add(list[k]);
			}
			List<int> list3 = list2;
			list2 = list;
			list = list3;
		}
	}

	public void ContractBorderByOneRingNeighbours()
	{
		temp.Clear();
		foreach (int item in Selected)
		{
			Index3i triangle = Mesh.GetTriangle(item);
			for (int i = 0; i < 3; i++)
			{
				int num = triangle[i];
				foreach (int item2 in Mesh.VtxTrianglesItr(num))
				{
					if (!IsSelected(item2))
					{
						temp.Add(num);
						break;
					}
				}
			}
		}
		foreach (int item3 in temp)
		{
			foreach (int item4 in Mesh.VtxTrianglesItr(item3))
			{
				Deselect(item4);
			}
		}
	}

	public void FloodFill(int tSeed, Func<int, bool> TriFilterF = null, Func<int, bool> EdgeFilterF = null)
	{
		FloodFill(new int[1] { tSeed }, TriFilterF, EdgeFilterF);
	}

	public void FloodFill(int[] Seeds, Func<int, bool> TriFilterF = null, Func<int, bool> EdgeFilterF = null)
	{
		DVector<int> dVector = new DVector<int>(Seeds);
		for (int i = 0; i < Seeds.Length; i++)
		{
			add(Seeds[i]);
		}
		while (dVector.size > 0)
		{
			int back = dVector.back;
			dVector.pop_back();
			Index3i triNeighbourTris = Mesh.GetTriNeighbourTris(back);
			for (int j = 0; j < 3; j++)
			{
				int num = triNeighbourTris[j];
				if (num != -1 && !IsSelected(num) && (TriFilterF == null || TriFilterF(num)) && (EdgeFilterF == null || EdgeFilterF(Mesh.GetTriEdge(back, j))))
				{
					add(num);
					dVector.push_back(num);
				}
			}
		}
	}

	public bool ClipFins(bool bClipLoners)
	{
		temp.Clear();
		foreach (int item in Selected)
		{
			if (is_fin(item, bClipLoners))
			{
				temp.Add(item);
			}
		}
		if (temp.Count == 0)
		{
			return false;
		}
		foreach (int item2 in temp)
		{
			remove(item2);
		}
		return true;
	}

	public bool FillEars(bool bFillTinyHoles)
	{
		temp.Clear();
		foreach (int item in Selected)
		{
			Index3i triNeighbourTris = Mesh.GetTriNeighbourTris(item);
			for (int i = 0; i < 3; i++)
			{
				int num = triNeighbourTris[i];
				if (!IsSelected(num) && is_ear(num, bFillTinyHoles))
				{
					temp.Add(num);
				}
			}
		}
		if (temp.Count == 0)
		{
			return false;
		}
		foreach (int item2 in temp)
		{
			add(item2);
		}
		return true;
	}

	public bool LocalOptimize(bool bClipFins, bool bFillEars, bool bFillTinyHoles = true, bool bClipLoners = true, bool bRemoveBowties = false)
	{
		bool result = false;
		bool flag = false;
		int num = 0;
		HashSet<int> tempHash = new HashSet<int>();
		while (!flag)
		{
			flag = true;
			if (num++ == 25)
			{
				break;
			}
			if (bClipFins && ClipFins(bClipLoners))
			{
				flag = false;
			}
			if (bFillEars && FillEars(bFillTinyHoles))
			{
				flag = false;
			}
			if (bRemoveBowties && remove_bowties(tempHash))
			{
				flag = false;
			}
			if (!flag)
			{
				result = true;
			}
		}
		if (bRemoveBowties)
		{
			remove_bowties(tempHash);
		}
		return result;
	}

	public bool LocalOptimize(bool bRemoveBowties = true)
	{
		return LocalOptimize(bClipFins: true, bFillEars: true, bFillTinyHoles: true, bClipLoners: true, bRemoveBowties);
	}

	public bool RemoveBowties()
	{
		return remove_bowties(null);
	}

	public bool remove_bowties(HashSet<int> tempHash)
	{
		bool result = false;
		bool flag = false;
		HashSet<int> hashSet = ((tempHash == null) ? new HashSet<int>() : tempHash);
		while (!flag)
		{
			flag = true;
			hashSet.Clear();
			foreach (int item in Selected)
			{
				Index3i triangle = Mesh.GetTriangle(item);
				hashSet.Add(triangle.a);
				hashSet.Add(triangle.b);
				hashSet.Add(triangle.c);
			}
			foreach (int item2 in hashSet)
			{
				if (is_bowtie_vtx(item2))
				{
					Deselect(Mesh.VtxTrianglesItr(item2));
					flag = false;
				}
			}
			if (!flag)
			{
				result = true;
			}
		}
		return result;
	}

	private bool is_bowtie_vtx(int vid)
	{
		int num = 0;
		foreach (int item in Mesh.VtxEdgesItr(vid))
		{
			Index2i edgeT = Mesh.GetEdgeT(item);
			if (edgeT.b != -1)
			{
				bool num2 = IsSelected(edgeT.a);
				bool flag = IsSelected(edgeT.b);
				if (num2 != flag)
				{
					num++;
				}
			}
			else if (IsSelected(edgeT.a))
			{
				num++;
			}
		}
		return num > 2;
	}

	private void count_nbrs(int tid, out int nbr_in, out int nbr_out, out int bdry_e)
	{
		Index3i triNeighbourTris = Mesh.GetTriNeighbourTris(tid);
		nbr_in = 0;
		nbr_out = 0;
		bdry_e = 0;
		for (int i = 0; i < 3; i++)
		{
			int num = triNeighbourTris[i];
			if (num == -1)
			{
				bdry_e++;
			}
			else if (IsSelected(num))
			{
				nbr_in++;
			}
			else
			{
				nbr_out++;
			}
		}
	}

	private bool is_ear(int tid, bool include_tiny_holes)
	{
		if (IsSelected(tid))
		{
			return false;
		}
		count_nbrs(tid, out var nbr_in, out var nbr_out, out var bdry_e);
		if (bdry_e == 2 && nbr_in == 1)
		{
			return true;
		}
		if (nbr_in == 2)
		{
			if (bdry_e == 1 || nbr_out == 1)
			{
				return true;
			}
		}
		else if (include_tiny_holes && nbr_in == 3)
		{
			return true;
		}
		return false;
	}

	private bool is_fin(int tid, bool include_loners)
	{
		if (!IsSelected(tid))
		{
			return false;
		}
		count_nbrs(tid, out var nbr_in, out var nbr_out, out var _);
		if (nbr_in != 1 || nbr_out != 2)
		{
			if (include_loners && nbr_in == 0)
			{
				return nbr_out == 3;
			}
			return false;
		}
		return true;
	}
}
