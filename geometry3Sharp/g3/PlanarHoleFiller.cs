using System.Collections.Generic;

namespace g3;

public class PlanarHoleFiller
{
	private class FillLoop
	{
		public EdgeLoop edgeLoop;

		public Polygon2d poly;
	}

	public DMesh3 Mesh;

	public Vector3d PlaneOrigin;

	public Vector3d PlaneNormal;

	public double FillTargetEdgeLen = double.MaxValue;

	public bool MergeFillBoundary = true;

	public bool OutputHasCracks;

	public int FailedInsertions;

	public int FailedMerges;

	private Vector3d PlaneX;

	private Vector3d PlaneY;

	private List<FillLoop> Loops = new List<FillLoop>();

	private AxisAlignedBox2d Bounds;

	public PlanarHoleFiller(DMesh3 mesh)
	{
		Mesh = mesh;
		Bounds = AxisAlignedBox2d.Empty;
	}

	public PlanarHoleFiller(MeshPlaneCut cut)
	{
		Mesh = cut.Mesh;
		AddFillLoops(cut.CutLoops);
		SetPlane(cut.PlaneOrigin, cut.PlaneNormal);
	}

	public void SetPlane(Vector3d origin, Vector3d normal)
	{
		PlaneOrigin = origin;
		PlaneNormal = normal;
		Vector3d.ComputeOrthogonalComplement(1, PlaneNormal, ref PlaneX, ref PlaneY);
	}

	public void SetPlane(Vector3d origin, Vector3d normal, Vector3d planeX, Vector3d planeY)
	{
		PlaneOrigin = origin;
		PlaneNormal = normal;
		PlaneX = planeX;
		PlaneY = planeY;
	}

	public void AddFillLoop(EdgeLoop loop)
	{
		Loops.Add(new FillLoop
		{
			edgeLoop = loop
		});
	}

	public void AddFillLoops(IEnumerable<EdgeLoop> loops)
	{
		foreach (EdgeLoop loop in loops)
		{
			AddFillLoop(loop);
		}
	}

	public bool Fill()
	{
		compute_polygons();
		Vector2d shiftOrigin = Bounds.Center;
		double scale = 1.0 / Bounds.MaxDim;
		foreach (FillLoop loop in Loops)
		{
			loop.poly.Translate(-shiftOrigin);
			loop.poly.Scale(scale * Vector2d.One, Vector2d.Zero);
		}
		Dictionary<PlanarComplex.Element, int> dictionary = new Dictionary<PlanarComplex.Element, int>();
		PlanarComplex planarComplex = new PlanarComplex();
		for (int i = 0; i < Loops.Count; i++)
		{
			PlanarComplex.Element key = planarComplex.Add(Loops[i].poly);
			dictionary[key] = i;
		}
		PlanarComplex.SolidRegionInfo solidRegionInfo = planarComplex.FindSolidRegions(PlanarComplex.FindSolidsOptions.SortPolygons);
		List<Index2i> list = new List<Index2i>();
		List<Index2i> list2 = new List<Index2i>();
		for (int j = 0; j < solidRegionInfo.Polygons.Count; j++)
		{
			GeneralPolygon2d generalPolygon2d = solidRegionInfo.Polygons[j];
			PlanarComplex.GeneralSolid generalSolid = solidRegionInfo.PolygonsSources[j];
			float num = 1.5f;
			int num2 = 0;
			if (FillTargetEdgeLen < double.MaxValue && FillTargetEdgeLen > 0.0)
			{
				int num3 = (int)((double)(num / (float)scale) / FillTargetEdgeLen) + 1;
				num2 = ((num3 > 1) ? num3 : 0);
			}
			MeshGenerator meshGenerator = ((num2 != 0) ? new GriddedRectGenerator
			{
				IndicesMap = new Index2i(1, 2),
				Width = num,
				Height = num,
				EdgeVertices = num2
			} : new TrivialRectGenerator
			{
				IndicesMap = new Index2i(1, 2),
				Width = num,
				Height = num
			});
			DMesh3 dMesh = meshGenerator.Generate().MakeDMesh();
			dMesh.ReverseOrientation();
			List<Polygon2d> list3 = new List<Polygon2d> { generalPolygon2d.Outer };
			list3.AddRange(generalPolygon2d.Holes);
			int[][] array = new int[list3.Count][];
			for (int k = 0; k < list3.Count; k++)
			{
				MeshInsertUVPolyCurve meshInsertUVPolyCurve = new MeshInsertUVPolyCurve(dMesh, list3[k]);
				ValidationStatus num4 = meshInsertUVPolyCurve.Validate(9.999999974752427E-07 * scale);
				bool flag = true;
				if (num4 == ValidationStatus.Ok && meshInsertUVPolyCurve.Apply())
				{
					meshInsertUVPolyCurve.Simplify();
					array[k] = meshInsertUVPolyCurve.CurveVertices;
					flag = meshInsertUVPolyCurve.Loops.Count != 1 || meshInsertUVPolyCurve.Loops[0].VertexCount != list3[k].VertexCount;
				}
				if (flag)
				{
					list.Add(new Index2i(j, k));
				}
			}
			List<int> list4 = new List<int>();
			foreach (int item in dMesh.TriangleIndices())
			{
				if (!generalPolygon2d.Contains(dMesh.GetTriCentroid(item).xy))
				{
					list4.Add(item);
				}
			}
			foreach (int item2 in list4)
			{
				dMesh.RemoveTriangle(item2);
			}
			MeshTransforms.PerVertexTransform(dMesh, delegate(Vector3d v)
			{
				Vector2d xy = v.xy;
				xy /= scale;
				xy += shiftOrigin;
				return to3D(xy);
			});
			IndexMap mergeMapV = new IndexMap(bForceSparse: true);
			if (MergeFillBoundary)
			{
				for (int l = 0; l < list3.Count; l++)
				{
					if (array[l] != null)
					{
						int[] array2 = array[l];
						_ = array2.Length;
						PlanarComplex.Element key2 = ((l == 0) ? generalSolid.Outer : generalSolid.Holes[l - 1]);
						int index = dictionary[key2];
						EdgeLoop edgeLoop = Loops[index].edgeLoop;
						List<int> list5 = build_merge_map(dMesh, array2, Mesh, edgeLoop.Vertices, 9.999999974752427E-07, mergeMapV);
						if (list5 != null && list5.Count > 0)
						{
							list.Add(new Index2i(j, l));
							OutputHasCracks = true;
						}
					}
				}
			}
			new MeshEditor(Mesh).AppendMesh(dMesh, mergeMapV, out var _, Mesh.AllocateTriangleGroup());
		}
		FailedInsertions = list.Count;
		FailedMerges = list2.Count;
		if (list.Count > 0 || list2.Count > 0)
		{
			return false;
		}
		return true;
	}

	private List<int> build_merge_map(DMesh3 fillMesh, int[] fillLoopV, DMesh3 targetMesh, int[] targetLoopV, double tol, IndexMap mergeMapV)
	{
		if (fillLoopV.Length == targetLoopV.Length && build_merge_map_simple(fillMesh, fillLoopV, targetMesh, targetLoopV, tol, mergeMapV))
		{
			return null;
		}
		int num = fillLoopV.Length;
		int num2 = targetLoopV.Length;
		bool[] array = new bool[num];
		_ = new bool[num2];
		_ = new int[num];
		_ = new int[num2];
		List<int> list = new List<int>();
		SmallListSet smallListSet = new SmallListSet();
		smallListSet.Resize(num);
		double num3 = tol * tol;
		for (int i = 0; i < num; i++)
		{
			if (!fillMesh.IsVertex(fillLoopV[i]))
			{
				array[i] = true;
				list.Add(i);
				continue;
			}
			smallListSet.AllocateAt(i);
			Vector3d vertex = fillMesh.GetVertex(fillLoopV[i]);
			for (int j = 0; j < num2; j++)
			{
				Vector3d v = targetMesh.GetVertex(targetLoopV[j]);
				if (vertex.DistanceSquared(ref v) < num3)
				{
					smallListSet.Insert(i, j);
				}
			}
		}
		for (int k = 0; k < num; k++)
		{
			if (!array[k] && smallListSet.Count(k) == 1)
			{
				int num4 = smallListSet.First(k);
				mergeMapV[fillLoopV[k]] = targetLoopV[num4];
				array[k] = true;
			}
		}
		for (int l = 0; l < num; l++)
		{
			if (!array[l])
			{
				list.Add(l);
			}
		}
		return list;
	}

	private bool build_merge_map_simple(DMesh3 fillMesh, int[] fillLoopV, DMesh3 targetMesh, int[] targetLoopV, double tol, IndexMap mergeMapV)
	{
		if (fillLoopV.Length != targetLoopV.Length)
		{
			return false;
		}
		int num = fillLoopV.Length;
		for (int i = 0; i < num; i++)
		{
			if (!fillMesh.IsVertex(fillLoopV[i]))
			{
				return false;
			}
			Vector3d vertex = fillMesh.GetVertex(fillLoopV[i]);
			Vector3d vertex2 = Mesh.GetVertex(targetLoopV[i]);
			if (vertex.Distance(vertex2) > tol)
			{
				return false;
			}
		}
		for (int j = 0; j < num; j++)
		{
			mergeMapV[fillLoopV[j]] = targetLoopV[j];
		}
		return true;
	}

	private void compute_polygons()
	{
		Bounds = AxisAlignedBox2d.Empty;
		for (int i = 0; i < Loops.Count; i++)
		{
			EdgeLoop edgeLoop = Loops[i].edgeLoop;
			Polygon2d polygon2d = new Polygon2d();
			int[] vertices = edgeLoop.Vertices;
			foreach (int vID in vertices)
			{
				Vector2d v = to2D(Mesh.GetVertex(vID));
				polygon2d.AppendVertex(v);
			}
			Loops[i].poly = polygon2d;
			Bounds.Contain(polygon2d.Bounds);
		}
	}

	private bool inPolygon(Vector2d v2, List<GeneralPolygon2d> polys, bool all = false)
	{
		int num = 0;
		foreach (GeneralPolygon2d poly in polys)
		{
			if (poly.Contains(v2))
			{
				if (!all)
				{
					return true;
				}
				num++;
			}
		}
		if (all && num == polys.Count)
		{
			return true;
		}
		return false;
	}

	private Vector2d to2D(Vector3d v)
	{
		Vector3d v2 = v - PlaneOrigin;
		v2 -= v2.Dot(PlaneNormal) * PlaneNormal;
		return new Vector2d(PlaneX.Dot(v2), PlaneY.Dot(v2));
	}

	private Vector3d to3D(Vector2d v)
	{
		return PlaneOrigin + PlaneX * v.x + PlaneY * v.y;
	}
}
