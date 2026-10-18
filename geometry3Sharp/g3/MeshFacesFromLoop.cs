using System;
using System.Collections.Generic;

namespace g3;

public class MeshFacesFromLoop
{
	private struct TriWithParent
	{
		public int tID;

		public int parentID;
	}

	public DMesh3 Mesh;

	private int[] InitialLoopT;

	private List<int> PathT;

	private List<int> InteriorT;

	private List<TriWithParent> sequence = new List<TriWithParent>(32);

	private HashSet<int> used = new HashSet<int>();

	private List<int> buffer = new List<int>(32);

	public IList<int> PathTriangles => PathT;

	public IList<int> InteriorTriangles => InteriorT;

	public MeshFacesFromLoop(DMesh3 Mesh, DCurve3 SpaceCurve, ISpatial Spatial)
	{
		this.Mesh = Mesh;
		int vertexCount = SpaceCurve.VertexCount;
		InitialLoopT = new int[vertexCount];
		for (int i = 0; i < vertexCount; i++)
		{
			InitialLoopT[i] = Spatial.FindNearestTriangle(SpaceCurve[i]);
		}
		find_path();
		find_interior_from_tris();
	}

	public MeshFacesFromLoop(DMesh3 Mesh, DCurve3 SpaceCurve, ISpatial Spatial, int tSeed)
	{
		this.Mesh = Mesh;
		int vertexCount = SpaceCurve.VertexCount;
		InitialLoopT = new int[vertexCount];
		for (int i = 0; i < vertexCount; i++)
		{
			InitialLoopT[i] = Spatial.FindNearestTriangle(SpaceCurve[i]);
		}
		find_path();
		find_interior_from_seed(tSeed);
	}

	public int[] ToArray()
	{
		return InteriorT.ToArray();
	}

	public MeshFaceSelection ToSelection()
	{
		MeshFaceSelection meshFaceSelection = new MeshFaceSelection(Mesh);
		meshFaceSelection.Select(InteriorT);
		return meshFaceSelection;
	}

	private void find_interior_from_tris()
	{
		MeshFaceSelection meshFaceSelection = new MeshFaceSelection(Mesh);
		meshFaceSelection.Select(PathT);
		meshFaceSelection.ExpandToOneRingNeighbours();
		meshFaceSelection.Deselect(PathT);
		MeshConnectedComponents meshConnectedComponents = new MeshConnectedComponents(Mesh);
		meshConnectedComponents.FilterSet = meshFaceSelection;
		meshConnectedComponents.FindConnectedT();
		int count = meshConnectedComponents.Count;
		if (count < 2)
		{
			throw new Exception("MeshFacesFromLoop.find_interior: only found one connected component!");
		}
		meshConnectedComponents.SortByCount(bIncreasing: false);
		count = 2;
		MeshFaceSelection[] array = new MeshFaceSelection[count];
		bool[] array2 = new bool[count];
		for (int i = 0; i < count; i++)
		{
			array[i] = new MeshFaceSelection(Mesh);
			array[i].Select(meshConnectedComponents.Components[i].Indices);
			array2[i] = false;
		}
		HashSet<int> border_tris = new HashSet<int>(PathT);
		Func<int, bool> triFilterF = (int tid) => !border_tris.Contains(tid);
		for (int j = 0; j < count; j++)
		{
			array[j].FloodFill(meshConnectedComponents.Components[j].Indices, triFilterF);
		}
		Array.Sort(array, (MeshFaceSelection a, MeshFaceSelection b) => a.Count.CompareTo(b.Count));
		InteriorT = new List<int>(array[0]);
	}

	private void find_interior_from_seed(int tSeed)
	{
		MeshFaceSelection meshFaceSelection = new MeshFaceSelection(Mesh);
		meshFaceSelection.Select(PathT);
		meshFaceSelection.FloodFill(tSeed);
		InteriorT = new List<int>(meshFaceSelection);
	}

	private void find_path()
	{
		PathT = new List<int>();
		PathT.Add(InitialLoopT[0]);
		for (int i = 1; i <= InitialLoopT.Length; i++)
		{
			int num = PathT[PathT.Count - 1];
			int num2 = InitialLoopT[i % InitialLoopT.Length];
			if (num2 != num)
			{
				Index3i triNeighbourTris = Mesh.GetTriNeighbourTris(num);
				if (triNeighbourTris.a == num2 || triNeighbourTris.b == num2 || triNeighbourTris.c == num2)
				{
					PathT.Add(num2);
					continue;
				}
				List<int> collection = find_path(num, num2);
				PathT.AddRange(collection);
				PathT.Add(num2);
			}
		}
		if (PathT[PathT.Count - 1] == PathT[0])
		{
			PathT.RemoveAt(PathT.Count - 1);
		}
	}

	private void push_onto_sequence(int parentID)
	{
		Index3i triNeighbourTris = Mesh.GetTriNeighbourTris(parentID);
		for (int i = 0; i < 3; i++)
		{
			if (!used.Contains(triNeighbourTris[i]))
			{
				sequence.Add(new TriWithParent
				{
					tID = triNeighbourTris[i],
					parentID = parentID
				});
				used.Add(triNeighbourTris[i]);
			}
		}
	}

	private List<int> find_path(int t1, int t2)
	{
		buffer.Clear();
		sequence.Clear();
		used.Clear();
		used.Add(t1);
		push_onto_sequence(t1);
		int num = 0;
		int num2 = -1;
		while (num2 == -1)
		{
			TriWithParent triWithParent = sequence[num];
			Index3i triNeighbourTris = Mesh.GetTriNeighbourTris(triWithParent.tID);
			if (triNeighbourTris.a == t2 || triNeighbourTris.b == t2 || triNeighbourTris.c == t2)
			{
				num2 = num;
			}
			else
			{
				push_onto_sequence(triWithParent.tID);
			}
			num++;
		}
		if (num2 == -1)
		{
			throw new Exception("MeshFacesFromLoop.find_path : could not find path!!");
		}
		TriWithParent tCur = sequence[num2];
		buffer.Add(tCur.tID);
		while (tCur.parentID != t1)
		{
			tCur = sequence.Find((TriWithParent x) => x.tID == tCur.parentID);
			buffer.Add(tCur.tID);
		}
		buffer.Reverse();
		return buffer;
	}
}
