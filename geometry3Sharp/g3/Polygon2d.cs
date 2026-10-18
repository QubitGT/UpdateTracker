using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace g3;

public class Polygon2d : IDuplicatable<Polygon2d>
{
	protected List<Vector2d> vertices;

	public int Timestamp;

	public Vector2d this[int key]
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

	public Vector2d Start => vertices[0];

	public ReadOnlyCollection<Vector2d> Vertices => vertices.AsReadOnly();

	public int VertexCount => vertices.Count;

	public AxisAlignedBox2d Bounds => GetBounds();

	public bool IsClockwise => SignedArea < 0.0;

	public double SignedArea
	{
		get
		{
			double num = 0.0;
			int count = vertices.Count;
			if (count == 0)
			{
				return 0.0;
			}
			Vector2d vector2d = vertices[0];
			Vector2d zero = Vector2d.Zero;
			for (int i = 0; i < count; i++)
			{
				zero = vertices[(i + 1) % count];
				num += vector2d.x * zero.y - vector2d.y * zero.x;
				vector2d = zero;
			}
			return num * 0.5;
		}
	}

	public double Area => Math.Abs(SignedArea);

	public double Perimeter
	{
		get
		{
			double num = 0.0;
			int count = vertices.Count;
			for (int i = 0; i < count; i++)
			{
				num += vertices[i].Distance(vertices[(i + 1) % count]);
			}
			return num;
		}
	}

	public double ArcLength => Perimeter;

	public double AverageEdgeLength
	{
		get
		{
			double num = 0.0;
			int count = vertices.Count;
			for (int i = 1; i < count; i++)
			{
				num += vertices[i].Distance(vertices[i - 1]);
			}
			num += vertices[count - 1].Distance(vertices[0]);
			return num / (double)count;
		}
	}

	public Polygon2d()
	{
		vertices = new List<Vector2d>();
		Timestamp = 0;
	}

	public Polygon2d(Polygon2d copy)
	{
		vertices = new List<Vector2d>(copy.vertices);
		Timestamp = 0;
	}

	public Polygon2d(IList<Vector2d> copy)
	{
		vertices = new List<Vector2d>(copy);
		Timestamp = 0;
	}

	public Polygon2d(IEnumerable<Vector2d> copy)
	{
		vertices = new List<Vector2d>(copy);
		Timestamp = 0;
	}

	public Polygon2d(Vector2d[] v)
	{
		vertices = new List<Vector2d>(v);
		Timestamp = 0;
	}

	public Polygon2d(VectorArray2d v)
	{
		vertices = new List<Vector2d>(v.AsVector2d());
		Timestamp = 0;
	}

	public Polygon2d(double[] values)
	{
		int num = values.Length / 2;
		vertices = new List<Vector2d>(num);
		for (int i = 0; i < num; i++)
		{
			vertices.Add(new Vector2d(values[2 * i], values[2 * i + 1]));
		}
		Timestamp = 0;
	}

	public Polygon2d(Func<int, Vector2d> SourceF, int N)
	{
		vertices = new List<Vector2d>();
		for (int i = 0; i < N; i++)
		{
			vertices.Add(SourceF(i));
		}
		Timestamp = 0;
	}

	public virtual Polygon2d Duplicate()
	{
		return new Polygon2d(this)
		{
			Timestamp = Timestamp
		};
	}

	public void AppendVertex(Vector2d v)
	{
		vertices.Add(v);
		Timestamp++;
	}

	public void AppendVertices(IEnumerable<Vector2d> v)
	{
		vertices.AddRange(v);
		Timestamp++;
	}

	public void RemoveVertex(int idx)
	{
		vertices.RemoveAt(idx);
		Timestamp++;
	}

	public void SetVertices(List<Vector2d> newVertices, bool bTakeOwnership)
	{
		if (bTakeOwnership)
		{
			vertices = newVertices;
			return;
		}
		vertices.Clear();
		int count = newVertices.Count;
		for (int i = 0; i < count; i++)
		{
			vertices.Add(newVertices[i]);
		}
	}

	public void Reverse()
	{
		vertices.Reverse();
		Timestamp++;
	}

	public Vector2d GetTangent(int i)
	{
		Vector2d vector2d = vertices[(i + 1) % vertices.Count];
		Vector2d vector2d2 = vertices[(i == 0) ? (vertices.Count - 1) : (i - 1)];
		return (vector2d - vector2d2).Normalized;
	}

	public Vector2d GetNormal(int i)
	{
		return GetTangent(i).Perp;
	}

	public Vector2d GetNormal_FaceAvg(int i)
	{
		Vector2d vector2d = vertices[(i + 1) % vertices.Count];
		Vector2d vector2d2 = vertices[(i == 0) ? (vertices.Count - 1) : (i - 1)];
		vector2d -= vertices[i];
		vector2d.Normalize();
		vector2d2 -= vertices[i];
		vector2d2.Normalize();
		Vector2d result = vector2d.Perp - vector2d2.Perp;
		if (result.Normalize() == 0.0)
		{
			return (vector2d + vector2d2).Normalized;
		}
		return result;
	}

	public AxisAlignedBox2d GetBounds()
	{
		AxisAlignedBox2d empty = AxisAlignedBox2d.Empty;
		empty.Contain(vertices);
		return empty;
	}

	public IEnumerable<Segment2d> SegmentItr()
	{
		int i = 0;
		while (i < vertices.Count)
		{
			yield return new Segment2d(vertices[i], vertices[(i + 1) % vertices.Count]);
			int num = i + 1;
			i = num;
		}
	}

	public IEnumerable<Vector2d> VerticesItr(bool bRepeatFirstAtEnd)
	{
		int N = vertices.Count;
		int i = 0;
		while (i < N)
		{
			yield return vertices[i];
			int num = i + 1;
			i = num;
		}
		if (bRepeatFirstAtEnd)
		{
			yield return vertices[0];
		}
	}

	public IEnumerable<Index2i> EdgeItr()
	{
		int i = 0;
		while (i < VertexCount)
		{
			yield return new Index2i(i, (i != VertexCount - 1) ? (i + 1) : 0);
			int num = i + 1;
			i = num;
		}
	}

	public bool BiContains(Segment2d seg)
	{
		foreach (Segment2d item in SegmentItr())
		{
			if (item.BiEquals(seg))
			{
				return true;
			}
		}
		return false;
	}

	public bool IsMember(Segment2d seg, out bool IsOutside)
	{
		IsOutside = true;
		if (Vertices.Contains(seg.P0) && Vertices.Contains(seg.P1))
		{
			if (BiContains(seg))
			{
				IsOutside = false;
			}
			return true;
		}
		return false;
	}

	public void NeighboursP(int iVertex, ref Vector2d p0, ref Vector2d p1)
	{
		int count = vertices.Count;
		p0 = vertices[(iVertex == 0) ? (count - 1) : (iVertex - 1)];
		p1 = vertices[(iVertex + 1) % count];
	}

	public void NeighboursV(int iVertex, ref Vector2d v0, ref Vector2d v1, bool bNormalize = false)
	{
		int count = vertices.Count;
		v0 = vertices[(iVertex == 0) ? (count - 1) : (iVertex - 1)] - vertices[iVertex];
		v1 = vertices[(iVertex + 1) % count] - vertices[iVertex];
		if (bNormalize)
		{
			v0.Normalize();
			v1.Normalize();
		}
	}

	public double OpeningAngleDeg(int iVertex)
	{
		Vector2d v = Vector2d.Zero;
		Vector2d v2 = Vector2d.Zero;
		NeighboursV(iVertex, ref v, ref v2, bNormalize: true);
		return Vector2d.AngleD(v, v2);
	}

	public double WindingIntegral(Vector2d P)
	{
		double num = 0.0;
		int count = vertices.Count;
		Vector2d vector2d = vertices[0] - P;
		Vector2d zero = Vector2d.Zero;
		for (int i = 0; i < count; i++)
		{
			zero = vertices[(i + 1) % count] - P;
			num += Math.Atan2(vector2d.x * zero.y - vector2d.y * zero.x, vector2d.x * zero.x + vector2d.y * zero.y);
			vector2d = zero;
		}
		return num / (Math.PI * 2.0);
	}

	public bool Contains(Vector2d P)
	{
		int num = 0;
		int count = vertices.Count;
		Vector2d P2 = vertices[0];
		Vector2d zero = Vector2d.Zero;
		for (int i = 0; i < count; i++)
		{
			zero = vertices[(i + 1) % count];
			if (P2.y <= P.y)
			{
				if (zero.y > P.y && MathUtil.IsLeft(ref P2, ref zero, ref P) > 0.0)
				{
					num++;
				}
			}
			else if (zero.y <= P.y && MathUtil.IsLeft(ref P2, ref zero, ref P) < 0.0)
			{
				num--;
			}
			P2 = zero;
		}
		return num != 0;
	}

	public bool Contains(Polygon2d o)
	{
		int vertexCount = o.VertexCount;
		for (int i = 0; i < vertexCount; i++)
		{
			if (!Contains(o[i]))
			{
				return false;
			}
		}
		if (Intersects(o))
		{
			return false;
		}
		return true;
	}

	public bool Contains(Segment2d o)
	{
		if (!Contains(o.P0) || !Contains(o.P1))
		{
			return false;
		}
		foreach (Segment2d item in SegmentItr())
		{
			if (item.Intersects(o))
			{
				return false;
			}
		}
		return true;
	}

	public bool Intersects(Polygon2d o)
	{
		if (!GetBounds().Intersects(o.GetBounds()))
		{
			return false;
		}
		foreach (Segment2d item in SegmentItr())
		{
			foreach (Segment2d item2 in o.SegmentItr())
			{
				if (item.Intersects(item2))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool Intersects(Segment2d o)
	{
		if (Contains(o.P0) || Contains(o.P1))
		{
			return true;
		}
		foreach (Segment2d item in SegmentItr())
		{
			if (item.Intersects(o))
			{
				return true;
			}
		}
		return false;
	}

	public List<Vector2d> FindIntersections(Polygon2d o)
	{
		List<Vector2d> list = new List<Vector2d>();
		if (!GetBounds().Intersects(o.GetBounds()))
		{
			return list;
		}
		foreach (Segment2d item in SegmentItr())
		{
			foreach (Segment2d item2 in o.SegmentItr())
			{
				if (!item.Intersects(item2))
				{
					continue;
				}
				IntrSegment2Segment2 intrSegment2Segment = new IntrSegment2Segment2(item, item2);
				if (intrSegment2Segment.Find())
				{
					list.Add(intrSegment2Segment.Point0);
					if (intrSegment2Segment.Quantity == 2)
					{
						list.Add(intrSegment2Segment.Point1);
					}
				}
			}
		}
		return list;
	}

	public List<Vector2d> FindIntersections(Segment2d s)
	{
		List<Vector2d> list = new List<Vector2d>();
		foreach (Segment2d item in SegmentItr())
		{
			if (!item.Intersects(s))
			{
				continue;
			}
			IntrSegment2Segment2 intrSegment2Segment = new IntrSegment2Segment2(item, s);
			if (intrSegment2Segment.Find())
			{
				list.Add(intrSegment2Segment.Point0);
				if (intrSegment2Segment.Quantity == 2)
				{
					list.Add(intrSegment2Segment.Point1);
				}
			}
		}
		return list;
	}

	public Segment2d Segment(int iSegment)
	{
		return new Segment2d(vertices[iSegment], vertices[(iSegment + 1) % vertices.Count]);
	}

	public Vector2d PointAt(int iSegment, double fSegT)
	{
		return new Segment2d(vertices[iSegment], vertices[(iSegment + 1) % vertices.Count]).PointAt(fSegT);
	}

	public Vector2d GetNormal(int iSeg, double segT)
	{
		double num = (segT / new Segment2d(vertices[iSeg], vertices[(iSeg + 1) % vertices.Count]).Extent + 1.0) / 2.0;
		Vector2d normal = GetNormal(iSeg);
		Vector2d normal2 = GetNormal((iSeg + 1) % vertices.Count);
		return ((1.0 - num) * normal + num * normal2).Normalized;
	}

	public double DistanceSquared(Vector2d p, out int iNearSeg, out double fNearSegT)
	{
		iNearSeg = -1;
		fNearSegT = double.MaxValue;
		double num = double.MaxValue;
		int count = vertices.Count;
		for (int i = 0; i < count; i++)
		{
			Segment2d segment2d = new Segment2d(vertices[i], vertices[(i + 1) % count]);
			double num2 = (p - segment2d.Center).Dot(segment2d.Direction);
			double num3 = double.MaxValue;
			num3 = ((num2 >= segment2d.Extent) ? segment2d.P1.DistanceSquared(p) : ((!(num2 <= 0.0 - segment2d.Extent)) ? (segment2d.PointAt(num2) - p).LengthSquared : segment2d.P0.DistanceSquared(p)));
			if (num3 < num)
			{
				num = num3;
				iNearSeg = i;
				fNearSegT = num2;
			}
		}
		return num;
	}

	public double DistanceSquared(Vector2d p)
	{
		int iNearSeg;
		double fNearSegT;
		return DistanceSquared(p, out iNearSeg, out fNearSegT);
	}

	public Polygon2d Translate(Vector2d translate)
	{
		int count = vertices.Count;
		for (int i = 0; i < count; i++)
		{
			vertices[i] += translate;
		}
		Timestamp++;
		return this;
	}

	public Polygon2d Rotate(Matrix2d rotation, Vector2d origin)
	{
		int count = vertices.Count;
		for (int i = 0; i < count; i++)
		{
			vertices[i] = rotation * (vertices[i] - origin) + origin;
		}
		Timestamp++;
		return this;
	}

	public Polygon2d Scale(Vector2d scale, Vector2d origin)
	{
		int count = vertices.Count;
		for (int i = 0; i < count; i++)
		{
			vertices[i] = scale * (vertices[i] - origin) + origin;
		}
		Timestamp++;
		return this;
	}

	public Polygon2d Transform(Func<Vector2d, Vector2d> transformF)
	{
		int count = vertices.Count;
		for (int i = 0; i < count; i++)
		{
			vertices[i] = transformF(vertices[i]);
		}
		Timestamp++;
		return this;
	}

	public Polygon2d Transform(ITransform2 xform)
	{
		int count = vertices.Count;
		for (int i = 0; i < count; i++)
		{
			vertices[i] = xform.TransformP(vertices[i]);
		}
		Timestamp++;
		return this;
	}

	public void VtxNormalOffset(double dist, bool bUseFaceAvg = false)
	{
		Vector2d[] array = new Vector2d[vertices.Count];
		if (bUseFaceAvg)
		{
			for (int i = 0; i < vertices.Count; i++)
			{
				array[i] = vertices[i] + dist * GetNormal_FaceAvg(i);
			}
		}
		else
		{
			for (int j = 0; j < vertices.Count; j++)
			{
				array[j] = vertices[j] + dist * GetNormal(j);
			}
		}
		for (int k = 0; k < vertices.Count; k++)
		{
			vertices[k] = array[k];
		}
		Timestamp++;
	}

	public void PolyOffset(double dist)
	{
		Vector2d[] array = new Vector2d[vertices.Count];
		for (int i = 0; i < vertices.Count; i++)
		{
			Vector2d vector2d = vertices[i];
			Vector2d vector2d2 = vertices[(i + 1) % vertices.Count];
			Vector2d vector2d3 = vertices[(i == 0) ? (vertices.Count - 1) : (i - 1)];
			Vector2d normalized = (vector2d2 - vector2d).Normalized;
			Vector2d normalized2 = (vector2d3 - vector2d).Normalized;
			Line2d line2d = new Line2d(vector2d + dist * normalized.Perp, normalized);
			Line2d other = new Line2d(vector2d - dist * normalized2.Perp, normalized2);
			array[i] = line2d.IntersectionPoint(ref other);
			if (array[i] == Vector2d.MaxValue)
			{
				array[i] = vertices[i] + dist * GetNormal_FaceAvg(i);
			}
		}
		for (int j = 0; j < vertices.Count; j++)
		{
			vertices[j] = array[j];
		}
		Timestamp++;
	}

	private static void simplifyDP(double tol, Vector2d[] v, int j, int k, bool[] mk)
	{
		if (k <= j + 1)
		{
			return;
		}
		int num = j;
		double num2 = 0.0;
		double num3 = tol * tol;
		Segment2d segment2d = new Segment2d(v[j], v[k]);
		for (int i = j + 1; i < k; i++)
		{
			double num4 = segment2d.DistanceSquared(v[i]);
			if (!(num4 <= num2))
			{
				num = i;
				num2 = num4;
			}
		}
		if (num2 > num3)
		{
			mk[num] = true;
			simplifyDP(tol, v, j, num, mk);
			simplifyDP(tol, v, num, k, mk);
		}
	}

	public void Simplify(double clusterTol = 0.0001, double lineDeviationTol = 0.01, bool bSimplifyStraightLines = true)
	{
		int count = vertices.Count;
		if (count < 3)
		{
			return;
		}
		Vector2d[] array = new Vector2d[count + 1];
		bool[] array2 = new bool[count + 1];
		int i;
		for (i = 0; i < count + 1; i++)
		{
			array2[i] = false;
		}
		double num = clusterTol * clusterTol;
		array[0] = vertices[0];
		i = 1;
		int num2 = 1;
		int index = 0;
		for (; i < count; i++)
		{
			if (!((vertices[i] - vertices[index]).LengthSquared < num))
			{
				array[num2++] = vertices[i];
				index = i;
			}
		}
		bool flag = false;
		switch (num2)
		{
		case 1:
			array[num2++] = vertices[1];
			array[num2++] = vertices[2];
			flag = true;
			break;
		case 2:
			array[num2++] = vertices[0];
			flag = true;
			break;
		}
		array[num2++] = vertices[0];
		int num3 = 0;
		if (!flag && lineDeviationTol > 0.0)
		{
			array2[0] = (array2[num2 - 1] = true);
			simplifyDP(lineDeviationTol, array, 0, num2 - 1, array2);
			for (i = 0; i < num2 - 1; i++)
			{
				if (array2[i])
				{
					num3++;
				}
			}
		}
		else
		{
			for (i = 0; i < num2; i++)
			{
				array2[i] = true;
			}
			num3 = num2 - 1;
		}
		switch (num3)
		{
		case 2:
			for (i = 1; i < num2 - 1; i++)
			{
				if (!array2[1])
				{
					array2[1] = true;
				}
				else if (!array2[num2 - 2])
				{
					array2[num2 - 2] = true;
				}
			}
			num3++;
			break;
		case 1:
			array2[1] = true;
			array2[2] = true;
			num3 += 2;
			break;
		}
		vertices = new List<Vector2d>();
		for (i = 0; i < num2 - 1; i++)
		{
			if (array2[i])
			{
				vertices.Add(array[i]);
			}
		}
		Timestamp++;
	}

	public void Chamfer(double chamfer_dist, double minConvexAngleDeg = 30.0, double minConcaveAngleDeg = 30.0)
	{
		if (IsClockwise)
		{
			throw new Exception("must be ccw?");
		}
		List<Vector2d> list = new List<Vector2d>();
		int count = Vertices.Count;
		int num = 0;
		do
		{
			Vector2d vector2d = Vertices[num];
			int index = ((num == 0) ? (count - 1) : (num - 1));
			Vector2d vector2d2 = Vertices[index];
			int num2 = (num + 1) % count;
			Vector2d vector2d3 = Vertices[num2];
			Vector2d vector2d4 = vector2d2 - vector2d;
			double num3 = vector2d4.Normalize();
			Vector2d vector2d5 = vector2d3 - vector2d;
			double num4 = vector2d5.Normalize();
			if (num3 < 9.999999974752427E-07 || num4 < 9.999999974752427E-07)
			{
				num = num2;
				continue;
			}
			double num5 = Vector2d.AngleD(vector2d4, vector2d5);
			double num6 = ((vector2d4.Perp.Dot(vector2d5) > 0.0) ? minConcaveAngleDeg : minConvexAngleDeg);
			if (num5 > num6)
			{
				list.Add(vector2d);
				num = num2;
				continue;
			}
			double num7 = Math.Min(chamfer_dist, num3 * 0.5);
			Vector2d item = vector2d + num7 * vector2d4;
			double num8 = Math.Min(chamfer_dist, num4 * 0.5);
			Vector2d item2 = vector2d + num8 * vector2d5;
			list.Add(item);
			list.Add(item2);
			num = num2;
		}
		while (num != 0);
		vertices = list;
		Timestamp++;
	}

	public Vector2d PointInPolygon()
	{
		AxisAlignedBox2d bounds = Bounds;
		Vector2d corner = bounds.GetCorner(3);
		Vector2d corner2 = bounds.GetCorner(1);
		if (Vertices.Contains(corner) && Vertices.Contains(corner2))
		{
			corner = bounds.GetCorner(2);
			corner2 = bounds.GetCorner(0);
		}
		List<Vector2d> list = FindIntersections(new Segment2d(corner, corner2));
		Segment2d segment2d = new Segment2d(list[0], list[1]);
		if (Contains(segment2d.Center))
		{
			return segment2d.Center;
		}
		throw new Exception("Failed to find a point in the polygon");
	}

	public Box2d MinimalBoundingBox(double epsilon)
	{
		return new ContMinBox2(vertices, epsilon, QueryNumberType.QT_DOUBLE, isConvexPolygon: false).MinBox;
	}

	public static Polygon2d MakeRectangle(Vector2d center, double width, double height)
	{
		VectorArray2d vectorArray2d = new VectorArray2d(4);
		vectorArray2d.Set(0, center.x - width / 2.0, center.y - height / 2.0);
		vectorArray2d.Set(1, center.x + width / 2.0, center.y - height / 2.0);
		vectorArray2d.Set(2, center.x + width / 2.0, center.y + height / 2.0);
		vectorArray2d.Set(3, center.x - width / 2.0, center.y + height / 2.0);
		return new Polygon2d(vectorArray2d);
	}

	public static Polygon2d MakeCircle(double fRadius, int nSteps, double angleShiftRad = 0.0)
	{
		VectorArray2d vectorArray2d = new VectorArray2d(nSteps);
		for (int i = 0; i < nSteps; i++)
		{
			double num = (double)i / (double)nSteps;
			double num2 = Math.PI * 2.0 * num + angleShiftRad;
			vectorArray2d.Set(i, fRadius * Math.Cos(num2), fRadius * Math.Sin(num2));
		}
		return new Polygon2d(vectorArray2d);
	}
}
