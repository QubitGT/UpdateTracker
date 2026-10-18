using System;
using System.Collections.Generic;
using g3;

namespace gs;

public class MeshInsertProjectedPolygon
{
	public DMesh3 Mesh;

	public int SeedTriangle = -1;

	public Frame3f ProjectFrame;

	public bool SimplifyInsertion = true;

	public bool RemovePolygonInterior = true;

	public RegionOperator ModifiedRegion;

	public int[] InsertedPolygonVerts;

	public EdgeLoop InsertedLoop;

	public int[] InteriorTriangles;

	public Polygon2d Polygon;

	public MeshInsertProjectedPolygon(DMesh3 mesh, Polygon2d poly, Frame3f frame, int seedTri)
	{
		Mesh = mesh;
		Polygon = new Polygon2d(poly);
		ProjectFrame = frame;
		SeedTriangle = seedTri;
	}

	public MeshInsertProjectedPolygon(DMesh3 mesh, DCurve3 polygon3, Frame3f frame, int seedTri)
	{
		if (!polygon3.Closed)
		{
			throw new Exception("MeshInsertPolyCurve(): only closed polygon3 supported for now");
		}
		Mesh = mesh;
		ProjectFrame = frame;
		SeedTriangle = seedTri;
		Polygon = new Polygon2d();
		foreach (Vector3d vertex in polygon3.Vertices)
		{
			Vector2f vector2f = frame.ToPlaneUV((Vector3f)vertex, 2);
			Polygon.AppendVertex(vector2f);
		}
	}

	public virtual ValidationStatus Validate()
	{
		if (!Mesh.IsTriangle(SeedTriangle))
		{
			return ValidationStatus.NotATriangle;
		}
		return ValidationStatus.Ok;
	}

	public bool Insert()
	{
		Func<int, bool> func = delegate(int vid)
		{
			Vector3d vertex = Mesh.GetVertex(vid);
			Vector2f vector2f2 = ProjectFrame.ToPlaneUV((Vector3f)vertex, 2);
			return Polygon.Contains(vector2f2);
		};
		MeshVertexSelection meshVertexSelection = new MeshVertexSelection(Mesh);
		Index3i triangle = Mesh.GetTriangle(SeedTriangle);
		List<int> list = new List<int>();
		for (int i = 0; i < 3; i++)
		{
			if (func(triangle[i]))
			{
				list.Add(triangle[i]);
			}
		}
		if (list.Count == 0)
		{
			list.Add(triangle.a);
			list.Add(triangle.b);
			list.Add(triangle.c);
		}
		meshVertexSelection.FloodFill(list.ToArray(), func);
		MeshFaceSelection meshFaceSelection = new MeshFaceSelection(Mesh, meshVertexSelection, 1);
		meshFaceSelection.ExpandToOneRingNeighbours();
		meshFaceSelection.FillEars(bFillTinyHoles: true);
		RegionOperator regionOperator = new RegionOperator(Mesh, meshFaceSelection);
		DMesh3 subMesh = regionOperator.Region.SubMesh;
		Vector3d[] initialPositions = new Vector3d[subMesh.MaxVertexID];
		MeshTransforms.PerVertexTransform(subMesh, subMesh.VertexIndices(), delegate(Vector3d v, int vid)
		{
			Vector2f vector2f = ProjectFrame.ToPlaneUV((Vector3f)v, 2);
			initialPositions[vid] = v;
			return new Vector3d(vector2f.x, vector2f.y, 0.0);
		});
		DMesh3 dMesh = new DMesh3(subMesh);
		DMeshAABBTree3 dMeshAABBTree = new DMeshAABBTree3(dMesh, autoBuild: true);
		MeshInsertUVPolyCurve meshInsertUVPolyCurve = new MeshInsertUVPolyCurve(subMesh, Polygon);
		if (!meshInsertUVPolyCurve.Apply())
		{
			throw new Exception("insertUV.Apply() failed");
		}
		if (SimplifyInsertion)
		{
			meshInsertUVPolyCurve.Simplify();
		}
		int[] curveVertices = meshInsertUVPolyCurve.CurveVertices;
		EdgeLoop insertedLoop = null;
		if (meshInsertUVPolyCurve.Loops.Count == 1)
		{
			insertedLoop = meshInsertUVPolyCurve.Loops[0];
		}
		List<int> list2 = new List<int>();
		foreach (int item in subMesh.TriangleIndices())
		{
			Vector3d triCentroid = subMesh.GetTriCentroid(item);
			if (Polygon.Contains(triCentroid.xy))
			{
				list2.Add(item);
			}
		}
		if (RemovePolygonInterior)
		{
			new MeshEditor(subMesh).RemoveTriangles(list2, bRemoveIsolatedVerts: true);
			InteriorTriangles = null;
		}
		else
		{
			InteriorTriangles = list2.ToArray();
		}
		Vector3d v2 = Vector3d.Zero;
		Vector3d v3 = Vector3d.Zero;
		Vector3d v4 = Vector3d.Zero;
		foreach (int item2 in subMesh.VertexIndices())
		{
			Vector3d vPoint = subMesh.GetVertex(item2);
			int tID = dMeshAABBTree.FindNearestTriangle(vPoint);
			Index3i triangle2 = dMesh.GetTriangle(tID);
			dMesh.GetTriVertices(tID, ref v2, ref v3, ref v4);
			Vector3d vector3d = MathUtil.BarycentricCoords(ref vPoint, ref v2, ref v3, ref v4);
			Vector3d vNewPos = vector3d.x * initialPositions[triangle2.a] + vector3d.y * initialPositions[triangle2.b] + vector3d.z * initialPositions[triangle2.c];
			subMesh.SetVertex(item2, vNewPos);
		}
		return BackPropagate(regionOperator, curveVertices, insertedLoop);
	}

	protected virtual bool BackPropagate(RegionOperator regionOp, int[] insertedPolyVerts, EdgeLoop insertedLoop)
	{
		bool num = regionOp.BackPropropagate();
		if (num)
		{
			ModifiedRegion = regionOp;
			IndexUtil.Apply(insertedPolyVerts, regionOp.ReinsertSubToBaseMapV);
			InsertedPolygonVerts = insertedPolyVerts;
			if (insertedLoop != null)
			{
				InsertedLoop = MeshIndexUtil.MapLoopViaVertexMap(regionOp.ReinsertSubToBaseMapV, regionOp.Region.SubMesh, regionOp.Region.BaseMesh, insertedLoop);
				if (RemovePolygonInterior)
				{
					InsertedLoop.CorrectOrientation();
				}
			}
		}
		return num;
	}
}
