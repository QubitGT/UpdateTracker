using System;
using System.Collections.Generic;
using System.Linq;

namespace g3;

public class MeshExtrudeMesh
{
	public DMesh3 Mesh;

	public SetGroupBehavior OffsetGroup = SetGroupBehavior.AutoGenerate;

	public SetGroupBehavior StitchGroups = SetGroupBehavior.AutoGenerate;

	public Func<Vector3d, Vector3f, int, Vector3d> ExtrudedPositionF;

	public bool IsPositiveOffset = true;

	public MeshBoundaryLoops InitialLoops;

	public int[] InitialTriangles;

	public int[] InitialVertices;

	public IndexMap InitialToOffsetMapV;

	private List<int> OffsetTriangles;

	public int OffsetGroupID;

	public EdgeLoop[] NewLoops;

	public int[][] StitchTriangles;

	public int[] StitchGroupIDs;

	public MeshExtrudeMesh(DMesh3 mesh)
	{
		Mesh = mesh;
		ExtrudedPositionF = (Vector3d pos, Vector3f normal, int idx) => pos + normal;
	}

	public virtual ValidationStatus Validate()
	{
		return ValidationStatus.Ok;
	}

	public virtual bool Extrude()
	{
		MeshNormals meshNormals = null;
		bool hasVertexNormals = Mesh.HasVertexNormals;
		if (!hasVertexNormals)
		{
			meshNormals = new MeshNormals(Mesh);
			meshNormals.Compute();
		}
		InitialLoops = new MeshBoundaryLoops(Mesh);
		InitialTriangles = Mesh.TriangleIndices().ToArray();
		InitialVertices = Mesh.VertexIndices().ToArray();
		InitialToOffsetMapV = new IndexMap(Mesh.MaxVertexID, Mesh.MaxVertexID);
		OffsetGroupID = OffsetGroup.GetGroupID(Mesh);
		MeshEditor meshEditor = new MeshEditor(Mesh);
		OffsetTriangles = meshEditor.DuplicateTriangles(InitialTriangles, ref InitialToOffsetMapV, OffsetGroupID);
		int[] initialVertices = InitialVertices;
		foreach (int num in initialVertices)
		{
			int vID = InitialToOffsetMapV[num];
			if (Mesh.IsVertex(vID))
			{
				Vector3d vertex = Mesh.GetVertex(num);
				Vector3f arg = (hasVertexNormals ? Mesh.GetVertexNormal(num) : ((Vector3f)meshNormals.Normals[num]));
				Vector3d vNewPos = ExtrudedPositionF(vertex, arg, num);
				Mesh.SetVertex(vID, vNewPos);
			}
		}
		if (IsPositiveOffset)
		{
			meshEditor.ReverseTriangles(InitialTriangles);
		}
		else
		{
			meshEditor.ReverseTriangles(OffsetTriangles);
		}
		NewLoops = new EdgeLoop[InitialLoops.Count];
		StitchTriangles = new int[InitialLoops.Count][];
		StitchGroupIDs = new int[InitialLoops.Count];
		int num2 = 0;
		foreach (EdgeLoop initialLoop in InitialLoops)
		{
			int[] array = new int[initialLoop.VertexCount];
			for (int j = 0; j < array.Length; j++)
			{
				array[j] = InitialToOffsetMapV[initialLoop.Vertices[j]];
			}
			StitchGroupIDs[num2] = StitchGroups.GetGroupID(Mesh);
			if (IsPositiveOffset)
			{
				StitchTriangles[num2] = meshEditor.StitchLoop(array, initialLoop.Vertices, StitchGroupIDs[num2]);
			}
			else
			{
				StitchTriangles[num2] = meshEditor.StitchLoop(initialLoop.Vertices, array, StitchGroupIDs[num2]);
			}
			NewLoops[num2] = EdgeLoop.FromVertices(Mesh, array);
			num2++;
		}
		return true;
	}
}
