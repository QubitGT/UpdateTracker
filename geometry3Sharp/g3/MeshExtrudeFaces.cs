using System;
using System.Collections.Generic;
using System.Linq;

namespace g3;

public class MeshExtrudeFaces
{
	public DMesh3 Mesh;

	public int[] Triangles;

	public SetGroupBehavior Group = SetGroupBehavior.AutoGenerate;

	public Func<Vector3d, Vector3f, int, Vector3d> ExtrudedPositionF;

	public List<Index2i> EdgePairs;

	public MeshVertexSelection ExtrudeVertices;

	public int[] JoinTriangles;

	public int JoinGroupID;

	public bool JoinIncomplete;

	public MeshExtrudeFaces(DMesh3 mesh, int[] triangles, bool bForceCopyArray = false)
	{
		Mesh = mesh;
		if (bForceCopyArray)
		{
			Triangles = (int[])triangles.Clone();
		}
		else
		{
			Triangles = triangles;
		}
		ExtrudedPositionF = (Vector3d pos, Vector3f normal, int idx) => pos + Vector3d.AxisY;
	}

	public MeshExtrudeFaces(DMesh3 mesh, IEnumerable<int> triangles)
	{
		Mesh = mesh;
		Triangles = triangles.ToArray();
		ExtrudedPositionF = (Vector3d pos, Vector3f normal, int idx) => pos + Vector3d.AxisY;
	}

	public virtual ValidationStatus Validate()
	{
		return ValidationStatus.Ok;
	}

	public virtual bool Extrude()
	{
		MeshEditor meshEditor = new MeshEditor(Mesh);
		meshEditor.SeparateTriangles(Triangles, bComputeEdgePairs: true, out EdgePairs);
		MeshNormals meshNormals = null;
		bool hasVertexNormals = Mesh.HasVertexNormals;
		if (!hasVertexNormals)
		{
			meshNormals = new MeshNormals(Mesh);
			meshNormals.Compute();
		}
		ExtrudeVertices = new MeshVertexSelection(Mesh);
		ExtrudeVertices.SelectTriangleVertices(Triangles);
		Vector3d[] array = new Vector3d[ExtrudeVertices.Count];
		int num = 0;
		foreach (int extrudeVertex in ExtrudeVertices)
		{
			Vector3d vertex = Mesh.GetVertex(extrudeVertex);
			Vector3f arg = (hasVertexNormals ? Mesh.GetVertexNormal(extrudeVertex) : ((Vector3f)meshNormals.Normals[extrudeVertex]));
			array[num++] = ExtrudedPositionF(vertex, arg, extrudeVertex);
		}
		num = 0;
		foreach (int extrudeVertex2 in ExtrudeVertices)
		{
			Mesh.SetVertex(extrudeVertex2, array[num++]);
		}
		JoinGroupID = Group.GetGroupID(Mesh);
		JoinTriangles = meshEditor.StitchUnorderedEdges(EdgePairs, JoinGroupID, bAbortOnFailure: false, out JoinIncomplete);
		if (JoinTriangles != null)
		{
			return !JoinIncomplete;
		}
		return false;
	}
}
