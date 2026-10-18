using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace g3;

public class DCurve3 : ISampledCurve3d
{
	protected List<Vector3d> vertices;

	public int Timestamp;

	public bool Closed { get; set; }

	public int VertexCount => vertices.Count;

	public int SegmentCount
	{
		get
		{
			if (!Closed)
			{
				return vertices.Count - 1;
			}
			return vertices.Count;
		}
	}

	public Vector3d this[int key]
	{
		get
		{
			return vertices[key];
		}
		set
		{
			vertices[key] = value;
			Timestamp++;
		}
	}

	public Vector3d Start => vertices[0];

	public Vector3d End
	{
		get
		{
			if (!Closed)
			{
				return vertices.Last();
			}
			return vertices[0];
		}
	}

	public IEnumerable<Vector3d> Vertices => vertices;

	public double ArcLength => CurveUtils.ArcLength(vertices, Closed);

	public DCurve3()
	{
		vertices = new List<Vector3d>();
		Closed = false;
		Timestamp = 1;
	}

	public DCurve3(List<Vector3d> verticesIn, bool bClosed, bool bTakeOwnership = false)
	{
		if (bTakeOwnership)
		{
			vertices = verticesIn;
		}
		else
		{
			vertices = new List<Vector3d>(verticesIn);
		}
		Closed = bClosed;
		Timestamp = 1;
	}

	public DCurve3(IEnumerable<Vector3d> verticesIn, bool bClosed)
	{
		vertices = new List<Vector3d>(verticesIn);
		Closed = bClosed;
		Timestamp = 1;
	}

	public DCurve3(DCurve3 copy)
	{
		vertices = new List<Vector3d>(copy.vertices);
		Closed = copy.Closed;
		Timestamp = 1;
	}

	public DCurve3(ISampledCurve3d icurve)
	{
		vertices = new List<Vector3d>(icurve.Vertices);
		Closed = icurve.Closed;
		Timestamp = 1;
	}

	public DCurve3(Polygon2d poly, int ix = 0, int iy = 1)
	{
		int vertexCount = poly.VertexCount;
		vertices = new List<Vector3d>(vertexCount);
		for (int i = 0; i < vertexCount; i++)
		{
			Vector3d zero = Vector3d.Zero;
			zero[ix] = poly[i].x;
			zero[iy] = poly[i].y;
			vertices.Add(zero);
		}
		Closed = true;
		Timestamp = 1;
	}

	public DCurve3(Vector3[] v_in, bool bClosed)
	{
		Closed = bClosed;
		vertices = ((IEnumerable<Vector3>)v_in.ToList()).Select((Func<Vector3, Vector3d>)((Vector3 vertex) => vertex)).ToList();
		Timestamp = 1;
	}

	public void AppendVertex(Vector3d v)
	{
		vertices.Add(v);
		Timestamp++;
	}

	public Vector3d GetVertex(int i)
	{
		return vertices[i];
	}

	public IEnumerable<Vector3d> VertexItr()
	{
		return vertices;
	}

	public void SetVertex(int i, Vector3d v)
	{
		vertices[i] = v;
		Timestamp++;
	}

	public void SetVertices(VectorArray3d v)
	{
		vertices = new List<Vector3d>();
		for (int i = 0; i < v.Count; i++)
		{
			vertices.Add(v[i]);
		}
		Timestamp++;
	}

	public void SetVertices(IEnumerable<Vector3d> v)
	{
		vertices = new List<Vector3d>(v);
		Timestamp++;
	}

	public void SetVertices(List<Vector3d> vertices, bool bTakeOwnership)
	{
		if (bTakeOwnership)
		{
			this.vertices = vertices;
		}
		else
		{
			this.vertices = new List<Vector3d>(vertices);
		}
		Timestamp++;
	}

	public void ClearVertices()
	{
		vertices = new List<Vector3d>();
		Closed = false;
		Timestamp++;
	}

	public void RemoveVertex(int idx)
	{
		vertices.RemoveAt(idx);
		Timestamp++;
	}

	public void Reverse()
	{
		vertices.Reverse();
		Timestamp++;
	}

	public Segment3d GetSegment(int iSegment)
	{
		if (!Closed)
		{
			return new Segment3d(vertices[iSegment], vertices[iSegment + 1]);
		}
		return new Segment3d(vertices[iSegment], vertices[(iSegment + 1) % vertices.Count]);
	}

	public IEnumerable<Segment3d> SegmentItr()
	{
		if (Closed)
		{
			int NV = vertices.Count;
			int i = 0;
			while (i < NV)
			{
				yield return new Segment3d(vertices[i], vertices[(i + 1) % NV]);
				int num = i + 1;
				i = num;
			}
		}
		else
		{
			int NV = vertices.Count - 1;
			int i = 0;
			while (i < NV)
			{
				yield return new Segment3d(vertices[i], vertices[i + 1]);
				int num = i + 1;
				i = num;
			}
		}
	}

	public Vector3d PointAt(int iSegment, double fSegT)
	{
		return new Segment3d(vertices[iSegment], vertices[(iSegment + 1) % vertices.Count]).PointAt(fSegT);
	}

	public AxisAlignedBox3d GetBoundingBox()
	{
		AxisAlignedBox3d empty = AxisAlignedBox3d.Empty;
		foreach (Vector3d vertex in vertices)
		{
			empty.Contain(vertex);
		}
		return empty;
	}

	public Vector3d Tangent(int i)
	{
		return CurveUtils.GetTangent(vertices, i, Closed);
	}

	public Vector3d Centroid(int i)
	{
		if (Closed)
		{
			int count = vertices.Count;
			if (i == 0)
			{
				return 0.5 * (vertices[1] + vertices[count - 1]);
			}
			return 0.5 * (vertices[(i + 1) % count] + vertices[i - 1]);
		}
		if (i == 0 || i == vertices.Count - 1)
		{
			return vertices[i];
		}
		return 0.5 * (vertices[i + 1] + vertices[i - 1]);
	}

	public Index2i Neighbours(int i)
	{
		int count = vertices.Count;
		if (Closed)
		{
			if (i == 0)
			{
				return new Index2i(count - 1, 1);
			}
			return new Index2i(i - 1, (i + 1) % count);
		}
		if (i == 0)
		{
			return new Index2i(-1, 1);
		}
		if (i == count - 1)
		{
			return new Index2i(count - 2, -1);
		}
		return new Index2i(i - 1, i + 1);
	}

	public double OpeningAngleDeg(int i)
	{
		int num = i - 1;
		int num2 = i + 1;
		if (Closed)
		{
			int count = vertices.Count;
			num = ((i == 0) ? (count - 1) : num);
			num2 %= count;
		}
		else if (i == 0 || i == vertices.Count - 1)
		{
			return 180.0;
		}
		Vector3d v = vertices[num] - vertices[i];
		Vector3d v2 = vertices[num2] - vertices[i];
		v.Normalize();
		v2.Normalize();
		return Vector3d.AngleD(v, v2);
	}

	public int NearestVertex(Vector3d p)
	{
		double num = double.MaxValue;
		int result = -1;
		int count = vertices.Count;
		for (int i = 0; i < count; i++)
		{
			double num2 = vertices[i].DistanceSquared(ref p);
			if (num2 < num)
			{
				num = num2;
				result = i;
			}
		}
		return result;
	}

	public double DistanceSquared(Vector3d p, out int iNearSeg, out double fNearSegT)
	{
		iNearSeg = -1;
		fNearSegT = double.MaxValue;
		double num = double.MaxValue;
		int num2 = (Closed ? vertices.Count : (vertices.Count - 1));
		for (int i = 0; i < num2; i++)
		{
			int index = i;
			int index2 = (i + 1) % vertices.Count;
			Segment3d segment3d = new Segment3d(vertices[index], vertices[index2]);
			double num3 = (p - segment3d.Center).Dot(segment3d.Direction);
			double num4 = double.MaxValue;
			num4 = ((num3 >= segment3d.Extent) ? segment3d.P1.DistanceSquared(p) : ((!(num3 <= 0.0 - segment3d.Extent)) ? (segment3d.PointAt(num3) - p).LengthSquared : segment3d.P0.DistanceSquared(p)));
			if (num4 < num)
			{
				num = num4;
				iNearSeg = i;
				fNearSegT = num3;
			}
		}
		return num;
	}

	public double DistanceSquared(Vector3d p)
	{
		int iNearSeg;
		double fNearSegT;
		return DistanceSquared(p, out iNearSeg, out fNearSegT);
	}

	public DCurve3 ResampleSharpTurns(double sharp_thresh = 90.0, double flat_thresh = 189.0, double corner_t = 0.01)
	{
		int count = vertices.Count;
		DCurve3 dCurve = new DCurve3
		{
			Closed = Closed
		};
		double t = 1.0 - corner_t;
		for (int i = 0; i < count; i++)
		{
			double num = Math.Abs(OpeningAngleDeg(i));
			if (!(num > flat_thresh) || i <= 0)
			{
				if (num > sharp_thresh)
				{
					dCurve.AppendVertex(vertices[i]);
					continue;
				}
				Vector3d b = vertices[(i + 1) % count];
				Vector3d a = vertices[(i == 0) ? (count - 1) : (i - 1)];
				dCurve.AppendVertex(Vector3d.Lerp(a, vertices[i], t));
				dCurve.AppendVertex(vertices[i]);
				dCurve.AppendVertex(Vector3d.Lerp(vertices[i], b, corner_t));
			}
		}
		return dCurve;
	}

	public Vector3d Center()
	{
		Vector3d zero = Vector3d.Zero;
		int num = SegmentCount;
		if (!Closed)
		{
			num++;
		}
		foreach (Vector3d vertex in Vertices)
		{
			zero += vertex;
		}
		return zero / num;
	}

	public Vector3d CenterMark()
	{
		Vector3d vector3d = Center();
		return GetSegment(NearestSegment(vector3d)).NearestPoint(vector3d);
	}

	public int NearestSegment(Vector3d position)
	{
		DistanceSquared(position, out var iNearSeg, out var _);
		return iNearSeg;
	}
}
