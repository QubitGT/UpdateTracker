using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Unity.Collections;
using Unity.Mathematics;
using andywiecko.BurstTriangulator;

namespace g3;

public class GeneralPolygon2d : IDuplicatable<GeneralPolygon2d>
{
	private Polygon2d outer;

	private bool bOuterIsCW;

	private List<Polygon2d> holes = new List<Polygon2d>();

	public Polygon2d Outer
	{
		get
		{
			return outer;
		}
		set
		{
			outer = value;
			bOuterIsCW = outer.IsClockwise;
		}
	}

	private bool HasHoles => holes.Count > 0;

	public ReadOnlyCollection<Polygon2d> Holes => holes.AsReadOnly();

	public double Area
	{
		get
		{
			double num = (bOuterIsCW ? (-1.0) : 1.0);
			double num2 = num * Outer.SignedArea;
			foreach (Polygon2d hole in holes)
			{
				num2 += num * hole.SignedArea;
			}
			return num2;
		}
	}

	public double HoleArea
	{
		get
		{
			double num = 0.0;
			foreach (Polygon2d hole in Holes)
			{
				num += Math.Abs(hole.SignedArea);
			}
			return num;
		}
	}

	public double Perimeter
	{
		get
		{
			double num = outer.Perimeter;
			foreach (Polygon2d hole in holes)
			{
				num += hole.Perimeter;
			}
			return num;
		}
	}

	public AxisAlignedBox2d Bounds
	{
		get
		{
			AxisAlignedBox2d bounds = outer.GetBounds();
			foreach (Polygon2d hole in holes)
			{
				bounds.Contain(hole.GetBounds());
			}
			return bounds;
		}
	}

	public int VertexCount
	{
		get
		{
			int num = outer.VertexCount;
			foreach (Polygon2d hole in holes)
			{
				num += hole.VertexCount;
			}
			return num;
		}
	}

	public GeneralPolygon2d()
	{
	}

	public GeneralPolygon2d(Polygon2d outer)
	{
		Outer = outer;
	}

	public GeneralPolygon2d(GeneralPolygon2d copy)
	{
		outer = new Polygon2d(copy.outer);
		bOuterIsCW = copy.bOuterIsCW;
		holes = new List<Polygon2d>();
		foreach (Polygon2d hole in copy.holes)
		{
			holes.Add(new Polygon2d(hole));
		}
	}

	public GeneralPolygon2d(IEnumerable<DCurve3> curves, out Frame3f frame, out IEnumerable<Vector3d> AllVerticesItr)
	{
		OrthogonalPlaneFit3 orthogonalPlaneFit = new OrthogonalPlaneFit3(curves.ElementAt(0).Vertices);
		frame = new Frame3f(orthogonalPlaneFit.Origin, orthogonalPlaneFit.Normal);
		AllVerticesItr = null;
		int num = 0;
		foreach (DCurve3 curf in curves)
		{
			List<Vector3d> list = curf.Vertices.ToList();
			List<Vector2d> list2 = new List<Vector2d>();
			foreach (Vector3d item in list)
			{
				Vector2f vector2f = frame.ToPlaneUV((Vector3f)item, 3);
				if (num != 0 && !Outer.Contains(vector2f))
				{
					break;
				}
				list2.Add(vector2f);
			}
			Polygon2d polygon2d = new Polygon2d(list2);
			if (num == 0)
			{
				polygon2d = new Polygon2d(list2);
				polygon2d.Reverse();
				Outer = polygon2d;
				list.Reverse();
				AllVerticesItr = list;
			}
			else
			{
				try
				{
					try
					{
						AddHole(polygon2d);
						AllVerticesItr = AllVerticesItr.Concat(list);
					}
					catch (Exception)
					{
						polygon2d.Reverse();
						AddHole(polygon2d);
						list.Reverse();
						AllVerticesItr = AllVerticesItr.Concat(list);
					}
				}
				catch (Exception)
				{
				}
			}
			num++;
		}
	}

	public virtual GeneralPolygon2d Duplicate()
	{
		return new GeneralPolygon2d(this);
	}

	public void AddHole(Polygon2d hole, bool bCheckContainment = true, bool bCheckOrientation = true)
	{
		if (outer == null)
		{
			throw new Exception("GeneralPolygon2d.AddHole: outer polygon not set!");
		}
		if (bCheckContainment)
		{
			if (!outer.Contains(hole))
			{
				throw new Exception("GeneralPolygon2d.AddHole: outer does not contain hole!");
			}
			foreach (Polygon2d hole2 in holes)
			{
				if (hole.Intersects(hole2))
				{
					throw new Exception("GeneralPolygon2D.AddHole: new hole intersects existing hole!");
				}
			}
		}
		if (bCheckOrientation && ((bOuterIsCW && hole.IsClockwise) || (!bOuterIsCW && !hole.IsClockwise)))
		{
			throw new Exception("GeneralPolygon2D.AddHole: new hole has same orientation as outer polygon!");
		}
		holes.Add(hole);
	}

	public void ClearHoles()
	{
		holes.Clear();
	}

	public void Translate(Vector2d translate)
	{
		outer.Translate(translate);
		foreach (Polygon2d hole in holes)
		{
			hole.Translate(translate);
		}
	}

	public void Rotate(Matrix2d rotation, Vector2d origin)
	{
		outer.Rotate(rotation, origin);
		foreach (Polygon2d hole in holes)
		{
			hole.Rotate(rotation, origin);
		}
	}

	public void Scale(Vector2d scale, Vector2d origin)
	{
		outer.Scale(scale, origin);
		foreach (Polygon2d hole in holes)
		{
			hole.Scale(scale, origin);
		}
	}

	public void Transform(Func<Vector2d, Vector2d> transformF)
	{
		outer.Transform(transformF);
		foreach (Polygon2d hole in holes)
		{
			hole.Transform(transformF);
		}
	}

	public void Reverse()
	{
		Outer.Reverse();
		bOuterIsCW = Outer.IsClockwise;
		foreach (Polygon2d hole in Holes)
		{
			hole.Reverse();
		}
	}

	public bool Contains(Vector2d vTest)
	{
		if (!outer.Contains(vTest))
		{
			return false;
		}
		foreach (Polygon2d hole in holes)
		{
			if (hole.Contains(vTest))
			{
				return false;
			}
		}
		return true;
	}

	public bool Contains(Polygon2d poly)
	{
		if (!outer.Contains(poly))
		{
			return false;
		}
		foreach (Polygon2d hole in holes)
		{
			if (hole.Contains(poly))
			{
				return false;
			}
		}
		return true;
	}

	public bool Contains(Segment2d seg)
	{
		if (!outer.Contains(seg))
		{
			return false;
		}
		foreach (Polygon2d hole in holes)
		{
			if (hole.Intersects(seg))
			{
				return false;
			}
		}
		return true;
	}

	public bool Intersects(Polygon2d poly)
	{
		if (outer.Intersects(poly))
		{
			return true;
		}
		foreach (Polygon2d hole in holes)
		{
			if (hole.Intersects(poly))
			{
				return true;
			}
		}
		return false;
	}

	public Vector2d PointAt(int iSegment, double fSegT, int iHoleIndex = -1)
	{
		if (iHoleIndex == -1)
		{
			return outer.PointAt(iSegment, fSegT);
		}
		return holes[iHoleIndex].PointAt(iSegment, fSegT);
	}

	public Segment2d Segment(int iSegment, int iHoleIndex = -1)
	{
		if (iHoleIndex == -1)
		{
			return outer.Segment(iSegment);
		}
		return holes[iHoleIndex].Segment(iSegment);
	}

	public Vector2d GetNormal(int iSegment, double segT, int iHoleIndex = -1)
	{
		if (iHoleIndex == -1)
		{
			return outer.GetNormal(iSegment, segT);
		}
		return holes[iHoleIndex].GetNormal(iSegment, segT);
	}

	public double DistanceSquared(Vector2d p, out int iHoleIndex, out int iNearSeg, out double fNearSegT)
	{
		iNearSeg = (iHoleIndex = -1);
		fNearSegT = double.MaxValue;
		double num = outer.DistanceSquared(p, out iNearSeg, out fNearSegT);
		for (int i = 0; i < Holes.Count; i++)
		{
			int iNearSeg2;
			double fNearSegT2;
			double num2 = Holes[i].DistanceSquared(p, out iNearSeg2, out fNearSegT2);
			if (num2 < num)
			{
				num = num2;
				iHoleIndex = i;
				iNearSeg = iNearSeg2;
				fNearSegT = fNearSegT2;
			}
		}
		return num;
	}

	public IEnumerable<Segment2d> AllSegmentsItr()
	{
		foreach (Segment2d item in outer.SegmentItr())
		{
			yield return item;
		}
		foreach (Polygon2d hole in holes)
		{
			foreach (Segment2d item2 in hole.SegmentItr())
			{
				yield return item2;
			}
		}
	}

	public IEnumerable<Vector2d> AllVerticesItr()
	{
		foreach (Vector2d vertex in outer.Vertices)
		{
			yield return vertex;
		}
		foreach (Polygon2d hole in holes)
		{
			foreach (Vector2d vertex2 in hole.Vertices)
			{
				yield return vertex2;
			}
		}
	}

	public IEnumerable<Index2i> AllEdgesItr()
	{
		int k = Outer.VertexCount;
		for (int j = 0; j < k; j++)
		{
			yield return new Index2i(j, (j != k - 1) ? (j + 1) : 0);
		}
		foreach (Polygon2d hole in Holes)
		{
			for (int j = 0; j < hole.VertexCount; j++)
			{
				yield return new Index2i(k + j, (j != hole.VertexCount - 1) ? (k + j + 1) : k);
			}
			k += hole.VertexCount;
		}
	}

	private NativeArray<int> ToEdges()
	{
		int vertexCount = VertexCount;
		NativeArray<int> result = new NativeArray<int>(vertexCount * 2, Allocator.Persistent);
		int num = 0;
		foreach (Index2i item in AllEdgesItr())
		{
			result[num] = item.a;
			num++;
			result[num] = item.b;
			num++;
		}
		return result;
	}

	private NativeArray<float2> ToHoleSeeds()
	{
		List<Vector2d> list = new List<Vector2d>();
		foreach (Polygon2d hole in Holes)
		{
			list.Add(hole.PointInPolygon());
		}
		return new NativeArray<float2>(list.Select((Vector2d vertex) => (float2)vertex).ToArray(), Allocator.Persistent);
	}

	public Index3i[] GetMesh()
	{
		Triangulator triangulator = new Triangulator(Allocator.Persistent);
		triangulator.Input.Positions = new NativeArray<float2>((from vertex in AllVerticesItr()
			select (float2)vertex).ToArray(), Allocator.Persistent);
		triangulator.Input.ConstraintEdges = ToEdges();
		triangulator.Input.HoleSeeds = ToHoleSeeds();
		triangulator.Settings.RestoreBoundary = true;
		triangulator.Settings.ConstrainEdges = true;
		Triangulator triangulator2 = triangulator;
		try
		{
			triangulator2.Run();
			if (!triangulator2.Output.Status.IsCreated || triangulator2.Output.Status.Value != 0)
			{
				throw new Exception("Could not create Delaunay Triangulation");
			}
			int[] array = triangulator2.Output.Triangles.AsArray().ToArray();
			int num = array.Length / 3;
			Index3i[] array2 = new Index3i[num];
			long num2 = 0L;
			for (int i = 0; i < num; i++)
			{
				array2[i] = new Index3i(array[num2++], array[num2++], array[num2++]);
			}
			triangulator2.Input.Positions.Dispose();
			triangulator2.Input.ConstraintEdges.Dispose();
			triangulator2.Input.HoleSeeds.Dispose();
			triangulator2.Dispose();
			return array2;
		}
		catch
		{
			triangulator2.Input.Positions.Dispose();
			triangulator2.Input.ConstraintEdges.Dispose();
			triangulator2.Input.HoleSeeds.Dispose();
			triangulator2.Dispose();
			return null;
		}
	}

	public void Simplify(double clusterTol = 0.0001, double lineDeviationTol = 0.01, bool bSimplifyStraightLines = true)
	{
		Outer.Simplify(clusterTol, lineDeviationTol, bSimplifyStraightLines);
		foreach (Polygon2d hole in holes)
		{
			hole.Simplify(clusterTol, lineDeviationTol, bSimplifyStraightLines);
		}
	}

	public bool IsOutside(Segment2d seg)
	{
		bool IsOutside = true;
		if (Outer.IsMember(seg, out IsOutside))
		{
			if (IsOutside)
			{
				return true;
			}
			return false;
		}
		foreach (Polygon2d hole in Holes)
		{
			if (hole.IsMember(seg, out IsOutside))
			{
				if (IsOutside)
				{
					return true;
				}
				return false;
			}
		}
		return false;
	}
}
