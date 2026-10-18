using System;
using System.Collections;
using System.Collections.Generic;

namespace g3;

public class MeshVertexSelection : IEnumerable<int>, IEnumerable
{
	public DMesh3 Mesh;

	private HashSet<int> Selected;

	private List<int> temp;

	public int Count => Selected.Count;

	public MeshVertexSelection(DMesh3 mesh)
	{
		Mesh = mesh;
		Selected = new HashSet<int>();
		temp = new List<int>();
	}

	public MeshVertexSelection(DMesh3 mesh, MeshFaceSelection convertT)
		: this(mesh)
	{
		foreach (int item in convertT)
		{
			Index3i triangle = mesh.GetTriangle(item);
			add(triangle.a);
			add(triangle.b);
			add(triangle.c);
		}
	}

	public MeshVertexSelection(DMesh3 mesh, MeshEdgeSelection convertE)
		: this(mesh)
	{
		foreach (int item in convertE)
		{
			Index2i edgeV = mesh.GetEdgeV(item);
			add(edgeV.a);
			add(edgeV.b);
		}
	}

	public HashSet<int> ExtractSelected()
	{
		HashSet<int> selected = Selected;
		Selected = new HashSet<int>();
		return selected;
	}

	public IEnumerator<int> GetEnumerator()
	{
		return Selected.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return Selected.GetEnumerator();
	}

	private void add(int vID)
	{
		Selected.Add(vID);
	}

	private void remove(int vID)
	{
		Selected.Remove(vID);
	}

	public bool IsSelected(int vID)
	{
		return Selected.Contains(vID);
	}

	public void Select(int vID)
	{
		if (Mesh.IsVertex(vID))
		{
			add(vID);
		}
	}

	public void Select(int[] vertices)
	{
		for (int i = 0; i < vertices.Length; i++)
		{
			if (Mesh.IsVertex(vertices[i]))
			{
				add(vertices[i]);
			}
		}
	}

	public void Select(IEnumerable<int> vertices)
	{
		foreach (int vertex in vertices)
		{
			if (Mesh.IsVertex(vertex))
			{
				add(vertex);
			}
		}
	}

	public void SelectTriangleVertices(int[] triangles)
	{
		for (int i = 0; i < triangles.Length; i++)
		{
			Index3i triangle = Mesh.GetTriangle(triangles[i]);
			add(triangle.a);
			add(triangle.b);
			add(triangle.c);
		}
	}

	public void SelectTriangleVertices(IEnumerable<int> triangles)
	{
		foreach (int triangle2 in triangles)
		{
			Index3i triangle = Mesh.GetTriangle(triangle2);
			add(triangle.a);
			add(triangle.b);
			add(triangle.c);
		}
	}

	public void SelectTriangleVertices(MeshFaceSelection triangles)
	{
		foreach (int triangle2 in triangles)
		{
			Index3i triangle = Mesh.GetTriangle(triangle2);
			add(triangle.a);
			add(triangle.b);
			add(triangle.c);
		}
	}

	public void SelectInteriorVertices(MeshFaceSelection triangles)
	{
		HashSet<int> hashSet = new HashSet<int>();
		foreach (int triangle2 in triangles)
		{
			Index3i triangle = Mesh.GetTriangle(triangle2);
			for (int i = 0; i < 3; i++)
			{
				int num = triangle[i];
				if (Selected.Contains(num) || hashSet.Contains(num))
				{
					continue;
				}
				bool flag = true;
				foreach (int item in Mesh.VtxTrianglesItr(num))
				{
					if (!triangles.IsSelected(item))
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					add(num);
				}
				else
				{
					hashSet.Add(num);
				}
			}
		}
	}

	public void SelectConnectedBoundaryV(int vSeed)
	{
		if (!Mesh.IsBoundaryVertex(vSeed))
		{
			throw new Exception("MeshConnectedComponents.FindConnectedBoundaryV: vSeed is not a boundary vertex");
		}
		HashSet<int> hashSet = ((Selected.Count == 0) ? Selected : new HashSet<int>());
		hashSet.Add(vSeed);
		List<int> list = temp;
		list.Clear();
		list.Add(vSeed);
		while (list.Count > 0)
		{
			int vID = list[list.Count - 1];
			list.RemoveAt(list.Count - 1);
			foreach (int item in Mesh.VtxVerticesItr(vID))
			{
				if (Mesh.IsBoundaryVertex(item) && !hashSet.Contains(item))
				{
					hashSet.Add(item);
					list.Add(item);
				}
			}
		}
		if (hashSet != Selected)
		{
			foreach (int item2 in hashSet)
			{
				add(item2);
			}
		}
		temp.Clear();
	}

	public void SelectEdgeVertices(int[] edges)
	{
		for (int i = 0; i < edges.Length; i++)
		{
			Index2i edgeV = Mesh.GetEdgeV(edges[i]);
			add(edgeV.a);
			add(edgeV.b);
		}
	}

	public void SelectEdgeVertices(IEnumerable<int> edges)
	{
		foreach (int edge in edges)
		{
			Index2i edgeV = Mesh.GetEdgeV(edge);
			add(edgeV.a);
			add(edgeV.b);
		}
	}

	public void Deselect(int vID)
	{
		remove(vID);
	}

	public void Deselect(int[] vertices)
	{
		for (int i = 0; i < vertices.Length; i++)
		{
			remove(vertices[i]);
		}
	}

	public void Deselect(IEnumerable<int> vertices)
	{
		foreach (int vertex in vertices)
		{
			remove(vertex);
		}
	}

	public void DeselectEdge(int eid)
	{
		Index2i edgeV = Mesh.GetEdgeV(eid);
		remove(edgeV.a);
		remove(edgeV.b);
	}

	public void DeselectEdges(IEnumerable<int> edges)
	{
		foreach (int edge in edges)
		{
			Index2i edgeV = Mesh.GetEdgeV(edge);
			remove(edgeV.a);
			remove(edgeV.b);
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

	public void ExpandToOneRingNeighbours(Func<int, bool> FilterF = null)
	{
		temp.Clear();
		foreach (int item in Selected)
		{
			foreach (int item2 in Mesh.VtxVerticesItr(item))
			{
				if ((FilterF == null || FilterF(item2)) && !IsSelected(item2))
				{
					temp.Add(item2);
				}
			}
		}
		for (int i = 0; i < temp.Count; i++)
		{
			add(temp[i]);
		}
	}

	public void ExpandToOneRingNeighbours(int nRings, Func<int, bool> FilterF = null)
	{
		for (int i = 0; i < nRings; i++)
		{
			ExpandToOneRingNeighbours(FilterF);
		}
	}

	public void FloodFill(int vSeed, Func<int, bool> VertIncludedF = null)
	{
		FloodFill(new int[1] { vSeed }, VertIncludedF);
	}

	public void FloodFill(int[] Seeds, Func<int, bool> VertIncludedF = null)
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
			foreach (int item in Mesh.VtxVerticesItr(back))
			{
				if (!IsSelected(item) && VertIncludedF(item))
				{
					add(item);
					dVector.push_back(item);
				}
			}
		}
	}
}
