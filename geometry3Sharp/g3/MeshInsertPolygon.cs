using System;
using System.Collections.Generic;

namespace g3;

public class MeshInsertPolygon
{
	public DMesh3 Mesh;

	public GeneralPolygon2d Polygon;

	public bool SimplifyInsertion = true;

	public MeshInsertUVPolyCurve OuterInsert;

	public List<MeshInsertUVPolyCurve> HoleInserts;

	public HashSet<int> InsertedPolygonEdges;

	public MeshFaceSelection InteriorTriangles;

	public bool Insert()
	{
		OuterInsert = new MeshInsertUVPolyCurve(Mesh, Polygon.Outer);
		if (!OuterInsert.Apply() || OuterInsert.Loops.Count == 0)
		{
			return false;
		}
		if (SimplifyInsertion)
		{
			OuterInsert.Simplify();
		}
		HoleInserts = new List<MeshInsertUVPolyCurve>(Polygon.Holes.Count);
		for (int i = 0; i < Polygon.Holes.Count; i++)
		{
			MeshInsertUVPolyCurve meshInsertUVPolyCurve = new MeshInsertUVPolyCurve(Mesh, Polygon.Holes[i]);
			meshInsertUVPolyCurve.Apply();
			if (SimplifyInsertion)
			{
				meshInsertUVPolyCurve.Simplify();
			}
			HoleInserts.Add(meshInsertUVPolyCurve);
		}
		int num = -1;
		EdgeLoop edgeLoop = OuterInsert.Loops[0];
		for (int j = 0; j < edgeLoop.EdgeCount; j++)
		{
			if (Mesh.IsEdge(edgeLoop.Edges[j]))
			{
				Index2i edgeT = Mesh.GetEdgeT(edgeLoop.Edges[j]);
				Vector3d triCentroid = Mesh.GetTriCentroid(edgeT.a);
				bool flag = Polygon.Outer.Contains(triCentroid.xy);
				Vector3d triCentroid2 = Mesh.GetTriCentroid(edgeT.b);
				bool flag2 = Polygon.Outer.Contains(triCentroid2.xy);
				if (flag && !flag2)
				{
					num = edgeT.a;
					break;
				}
				if (flag2 && !flag)
				{
					num = edgeT.b;
					break;
				}
			}
		}
		if (num == -1)
		{
			throw new Exception("MeshPolygonsInserter: could not find seed triangle!");
		}
		InsertedPolygonEdges = new HashSet<int>(edgeLoop.Edges);
		foreach (MeshInsertUVPolyCurve holeInsert in HoleInserts)
		{
			int[] edges = holeInsert.Loops[0].Edges;
			foreach (int item in edges)
			{
				InsertedPolygonEdges.Add(item);
			}
		}
		InteriorTriangles = new MeshFaceSelection(Mesh);
		InteriorTriangles.FloodFill(num, null, (int eid) => !InsertedPolygonEdges.Contains(eid));
		return true;
	}
}
