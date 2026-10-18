using System;
using System.Collections.Generic;

namespace g3;

public class PlanarSpansFiller
{
	public DMesh3 Mesh;

	public Vector3d PlaneOrigin;

	public Vector3d PlaneNormal;

	public double FillTargetEdgeLen = double.MaxValue;

	public bool MergeFillBoundary;

	private Vector3d PlaneX;

	private Vector3d PlaneY;

	private List<EdgeSpan> FillSpans;

	private Polygon2d SpansPoly;

	private AxisAlignedBox2d Bounds;

	public PlanarSpansFiller(DMesh3 mesh, IList<EdgeSpan> spans)
	{
		Mesh = mesh;
		FillSpans = new List<EdgeSpan>(spans);
		Bounds = AxisAlignedBox2d.Empty;
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

	public bool Fill()
	{
		compute_polygon();
		Vector2d shiftOrigin = Bounds.Center;
		double scale = 1.0 / Bounds.MaxDim;
		SpansPoly.Translate(-shiftOrigin);
		SpansPoly.Scale(scale * Vector2d.One, Vector2d.Zero);
		new Dictionary<PlanarComplex.Element, int>();
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
		int[] array = null;
		MeshInsertUVPolyCurve meshInsertUVPolyCurve = new MeshInsertUVPolyCurve(dMesh, SpansPoly);
		ValidationStatus num4 = meshInsertUVPolyCurve.Validate(9.999999974752427E-07 * scale);
		bool flag = true;
		if (num4 == ValidationStatus.Ok && meshInsertUVPolyCurve.Apply())
		{
			meshInsertUVPolyCurve.Simplify();
			array = meshInsertUVPolyCurve.CurveVertices;
			flag = false;
		}
		if (flag)
		{
			return false;
		}
		List<int> list = new List<int>();
		foreach (int item in dMesh.TriangleIndices())
		{
			Vector3d triCentroid = dMesh.GetTriCentroid(item);
			if (!SpansPoly.Contains(triCentroid.xy))
			{
				list.Add(item);
			}
		}
		foreach (int item2 in list)
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
		if (MergeFillBoundary && array != null)
		{
			throw new NotImplementedException("PlanarSpansFiller: merge fill boundary not implemented!");
		}
		new MeshEditor(Mesh).AppendMesh(dMesh, mergeMapV, out var _, Mesh.AllocateTriangleGroup());
		return true;
	}

	private void compute_polygon()
	{
		SpansPoly = new Polygon2d();
		for (int i = 0; i < FillSpans.Count; i++)
		{
			int[] vertices = FillSpans[i].Vertices;
			foreach (int vID in vertices)
			{
				Vector2d v = to2D(Mesh.GetVertex(vID));
				SpansPoly.AppendVertex(v);
			}
		}
		Bounds = SpansPoly.Bounds;
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
