using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace g3;

public class MeshEdgeSelection : IEnumerable<int>, IEnumerable
{
	public DMesh3 Mesh;

	private HashSet<int> Selected;

	private List<int> temp;

	private BitArray tempBits;

	protected BitArray Bitmap
	{
		get
		{
			if (tempBits == null)
			{
				tempBits = new BitArray(Mesh.MaxEdgeID);
			}
			return tempBits;
		}
	}

	public int Count => Selected.Count;

	public MeshEdgeSelection(DMesh3 mesh)
	{
		Mesh = mesh;
		Selected = new HashSet<int>();
		temp = new List<int>();
	}

	public MeshEdgeSelection(MeshEdgeSelection copy)
	{
		Mesh = copy.Mesh;
		Selected = new HashSet<int>(copy.Selected);
		temp = new List<int>();
	}

	public MeshEdgeSelection(DMesh3 mesh, MeshVertexSelection convertV, int minCount = 2)
		: this(mesh)
	{
		minCount = MathUtil.Clamp(minCount, 1, 2);
		foreach (int item in mesh.EdgeIndices())
		{
			Index2i edgeV = mesh.GetEdgeV(item);
			if ((convertV.IsSelected(edgeV.a) ? 1 : 0) + (convertV.IsSelected(edgeV.b) ? 1 : 0) >= minCount)
			{
				add(item);
			}
		}
	}

	public MeshEdgeSelection(DMesh3 mesh, MeshFaceSelection convertT, int minCount = 1)
		: this(mesh)
	{
		minCount = MathUtil.Clamp(minCount, 1, 2);
		if (minCount == 1)
		{
			foreach (int item in convertT)
			{
				Index3i triEdges = mesh.GetTriEdges(item);
				add(triEdges.a);
				add(triEdges.b);
				add(triEdges.c);
			}
			return;
		}
		foreach (int item2 in mesh.EdgeIndices())
		{
			Index2i edgeT = mesh.GetEdgeT(item2);
			if (convertT.IsSelected(edgeT.a) && convertT.IsSelected(edgeT.b))
			{
				add(item2);
			}
		}
	}

	public IEnumerator<int> GetEnumerator()
	{
		return Selected.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return Selected.GetEnumerator();
	}

	private void add(int eid)
	{
		Selected.Add(eid);
	}

	private void remove(int eid)
	{
		Selected.Remove(eid);
	}

	public bool IsSelected(int eid)
	{
		return Selected.Contains(eid);
	}

	public void Select(int eid)
	{
		if (Mesh.IsEdge(eid))
		{
			add(eid);
		}
	}

	public void Select(int[] edges)
	{
		for (int i = 0; i < edges.Length; i++)
		{
			if (Mesh.IsEdge(edges[i]))
			{
				add(edges[i]);
			}
		}
	}

	public void Select(List<int> edges)
	{
		for (int i = 0; i < edges.Count; i++)
		{
			if (Mesh.IsEdge(edges[i]))
			{
				add(edges[i]);
			}
		}
	}

	public void Select(IEnumerable<int> edges)
	{
		foreach (int edge in edges)
		{
			if (Mesh.IsEdge(edge))
			{
				add(edge);
			}
		}
	}

	public void Select(Func<int, bool> selectF)
	{
		temp.Clear();
		int maxEdgeID = Mesh.MaxEdgeID;
		for (int i = 0; i < maxEdgeID; i++)
		{
			if (Mesh.IsEdge(i) && selectF(i))
			{
				temp.Add(i);
			}
		}
		Select(temp);
	}

	public void SelectVertexEdges(int[] vertices)
	{
		foreach (int vID in vertices)
		{
			foreach (int item in Mesh.VtxEdgesItr(vID))
			{
				add(item);
			}
		}
	}

	public void SelectVertexEdges(IEnumerable<int> vertices)
	{
		foreach (int vertex in vertices)
		{
			foreach (int item in Mesh.VtxEdgesItr(vertex))
			{
				add(item);
			}
		}
	}

	public void SelectTriangleEdges(IEnumerable<int> triangles)
	{
		foreach (int triangle in triangles)
		{
			Index3i triEdges = Mesh.GetTriEdges(triangle);
			add(triEdges.a);
			add(triEdges.b);
			add(triEdges.c);
		}
	}

	public void SelectBoundaryTriEdges(MeshFaceSelection triangles)
	{
		foreach (int triangle in triangles)
		{
			Index3i triEdges = Mesh.GetTriEdges(triangle);
			for (int i = 0; i < 3; i++)
			{
				Index2i edgeT = Mesh.GetEdgeT(triEdges[i]);
				int tid = ((edgeT.a == triangle) ? edgeT.b : edgeT.a);
				if (!triangles.IsSelected(tid))
				{
					add(triEdges[i]);
				}
			}
		}
	}

	public void Deselect(int tid)
	{
		remove(tid);
	}

	public void Deselect(int[] edges)
	{
		for (int i = 0; i < edges.Length; i++)
		{
			remove(edges[i]);
		}
	}

	public void Deselect(IEnumerable<int> edges)
	{
		foreach (int edge in edges)
		{
			remove(edge);
		}
	}

	public void DeselectAll()
	{
		Selected.Clear();
	}

	public int[] ToArray()
	{
		return Selected.ToArray();
	}
}
