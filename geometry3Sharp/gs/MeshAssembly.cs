using System.Collections.Generic;
using g3;

namespace gs;

public class MeshAssembly
{
	public DMesh3 SourceMesh;

	public bool HasNoVoids;

	public List<DMesh3> ClosedSolids;

	public List<DMesh3> OpenMeshes;

	public MeshAssembly(DMesh3 sourceMesh)
	{
		SourceMesh = sourceMesh;
		ClosedSolids = new List<DMesh3>();
		OpenMeshes = new List<DMesh3>();
	}

	public void Decompose()
	{
		process();
	}

	private void process()
	{
		DMesh3 dMesh = SourceMesh;
		if (!dMesh.CachedIsClosed)
		{
			dMesh = new DMesh3(SourceMesh);
			new RemoveDuplicateTriangles(dMesh).Apply();
			new MergeCoincidentEdges(dMesh).Apply();
		}
		DMesh3[] array = MeshConnectedComponents.Separate(dMesh);
		List<DMesh3> list = new List<DMesh3>();
		DMesh3[] array2 = array;
		foreach (DMesh3 dMesh2 in array2)
		{
			if (!dMesh2.CachedIsClosed)
			{
				OpenMeshes.Add(dMesh2);
			}
			else
			{
				list.Add(dMesh2);
			}
		}
		if (list.Count != 0)
		{
			if (list.Count == 1)
			{
				ClosedSolids = new List<DMesh3> { list[0] };
			}
			if (HasNoVoids)
			{
				ClosedSolids = process_solids_novoid(list);
			}
			else
			{
				ClosedSolids = process_solids(list);
			}
		}
	}

	private List<DMesh3> process_solids(List<DMesh3> solid_components)
	{
		DMesh3 dMesh = new DMesh3(SourceMesh.Components | MeshComponents.FaceGroups);
		MeshEditor meshEditor = new MeshEditor(dMesh);
		foreach (DMesh3 solid_component in solid_components)
		{
			meshEditor.AppendMesh(solid_component, dMesh.AllocateTriangleGroup());
		}
		return new List<DMesh3> { dMesh };
	}

	private List<DMesh3> process_solids_novoid(List<DMesh3> solid_components)
	{
		return solid_components;
	}
}
