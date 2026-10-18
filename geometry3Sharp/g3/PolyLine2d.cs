using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace g3;

public class PolyLine2d : IEnumerable<Vector2d>, IEnumerable
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

	public Vector2d End => vertices[vertices.Count - 1];

	public ReadOnlyCollection<Vector2d> Vertices => vertices.AsReadOnly();

	public int VertexCount => vertices.Count;

	public AxisAlignedBox2d Bounds => GetBounds();

	[Obsolete("This method name is confusing. Will remove in future. Use ArcLength instead")]
	public double Length => ArcLength;

	public double ArcLength
	{
		get
		{
			double num = 0.0;
			int count = vertices.Count;
			for (int i = 0; i < count - 1; i++)
			{
				num += vertices[i].Distance(vertices[i + 1]);
			}
			return num;
		}
	}

	public PolyLine2d()
	{
		vertices = new List<Vector2d>();
		Timestamp = 0;
	}

	public PolyLine2d(PolyLine2d copy)
	{
		vertices = new List<Vector2d>(copy.vertices);
		Timestamp = 0;
	}

	public PolyLine2d(Polygon2d copy, bool bDuplicateFirstLast)
	{
		vertices = new List<Vector2d>(copy.VerticesItr(bDuplicateFirstLast));
		Timestamp = 0;
	}

	public PolyLine2d(IList<Vector2d> copy)
	{
		vertices = new List<Vector2d>(copy);
		Timestamp = 0;
	}

	public PolyLine2d(IEnumerable<Vector2d> copy)
	{
		vertices = new List<Vector2d>(copy);
		Timestamp = 0;
	}

	public PolyLine2d(Vector2d[] v)
	{
		vertices = new List<Vector2d>(v);
		Timestamp = 0;
	}

	public PolyLine2d(VectorArray2d v)
	{
		vertices = new List<Vector2d>(v.AsVector2d());
		Timestamp = 0;
	}

	public virtual void AppendVertex(Vector2d v)
	{
		vertices.Add(v);
		Timestamp++;
	}

	public virtual void AppendVertices(IEnumerable<Vector2d> v)
	{
		vertices.AddRange(v);
		Timestamp++;
	}

	public virtual void Reverse()
	{
		vertices.Reverse();
		Timestamp++;
	}

	public Vector2d GetTangent(int i)
	{
		if (i == 0)
		{
			return (vertices[1] - vertices[0]).Normalized;
		}
		if (i == vertices.Count - 1)
		{
			return (vertices[vertices.Count - 1] - vertices[vertices.Count - 2]).Normalized;
		}
		return (vertices[i + 1] - vertices[i - 1]).Normalized;
	}

	public Vector2d GetNormal(int i)
	{
		return GetTangent(i).Perp;
	}

	public AxisAlignedBox2d GetBounds()
	{
		if (vertices.Count == 0)
		{
			return AxisAlignedBox2d.Empty;
		}
		AxisAlignedBox2d result = new AxisAlignedBox2d(vertices[0]);
		for (int i = 1; i < vertices.Count; i++)
		{
			result.Contain(vertices[i]);
		}
		return result;
	}

	public double DistanceSquared(Vector2d point)
	{
		double num = double.MaxValue;
		for (int i = 0; i < vertices.Count - 1; i++)
		{
			double num2 = new Segment2d(vertices[i], vertices[i + 1]).DistanceSquared(point);
			if (num2 < num)
			{
				num = num2;
			}
		}
		return num;
	}

	public Segment2d Segment(int iSegment)
	{
		return new Segment2d(vertices[iSegment], vertices[iSegment + 1]);
	}

	public IEnumerable<Segment2d> SegmentItr()
	{
		int i = 0;
		while (i < vertices.Count - 1)
		{
			yield return new Segment2d(vertices[i], vertices[i + 1]);
			int num = i + 1;
			i = num;
		}
	}

	public IEnumerator<Vector2d> GetEnumerator()
	{
		return vertices.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return vertices.GetEnumerator();
	}

	public void VertexOffset(double dist)
	{
		Vector2d[] array = new Vector2d[vertices.Count];
		for (int i = 0; i < vertices.Count; i++)
		{
			array[i] = vertices[i] + dist * GetNormal(i);
		}
		for (int j = 0; j < vertices.Count; j++)
		{
			vertices[j] = array[j];
		}
	}

	public bool TrimStart(double dist)
	{
		int count = vertices.Count;
		int num = 0;
		double num2 = vertices[num].Distance(vertices[num + 1]);
		double num3 = 0.0;
		while (num < count - 2 && num3 + num2 < dist)
		{
			num3 += num2;
			num++;
			num2 = vertices[num].Distance(vertices[num + 1]);
		}
		if (num == count - 2 && num3 + num2 <= dist)
		{
			return false;
		}
		double t = (dist - num3) / num2;
		Vector2d value = Segment(num).PointBetween(t);
		if (num > 0)
		{
			vertices.RemoveRange(0, num);
		}
		vertices[0] = value;
		return true;
	}

	public bool TrimEnd(double dist)
	{
		int count = vertices.Count;
		int num = count - 1;
		double num2 = vertices[num].Distance(vertices[num - 1]);
		double num3 = 0.0;
		while (num > 1 && num3 + num2 < dist)
		{
			num3 += num2;
			num--;
			num2 = vertices[num].Distance(vertices[num - 1]);
		}
		if (num == 1 && num3 + num2 <= dist)
		{
			return false;
		}
		double num4 = (dist - num3) / num2;
		Vector2d value = Segment(num - 1).PointBetween(1.0 - num4);
		if (num < count - 1)
		{
			vertices.RemoveRange(num, count - 1 - num);
		}
		vertices[num] = value;
		return true;
	}

	public bool Trim(double each_end_dist)
	{
		if (ArcLength < 2.0 * each_end_dist)
		{
			return false;
		}
		if (TrimEnd(each_end_dist))
		{
			return TrimStart(each_end_dist);
		}
		return false;
	}

	protected static void simplifyDP(double tol, Vector2d[] v, int j, int k, bool[] mk)
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

	public virtual void Simplify(double clusterTol = 0.0001, double lineDeviationTol = 0.01, bool bSimplifyStraightLines = true)
	{
		int count = vertices.Count;
		Vector2d[] array = new Vector2d[count];
		bool[] array2 = new bool[count];
		int i;
		for (i = 0; i < count; i++)
		{
			array2[i] = false;
		}
		double num = clusterTol * clusterTol;
		array[0] = vertices[0];
		int num2;
		i = (num2 = 1);
		int num3 = 0;
		for (; i < count; i++)
		{
			if (!((vertices[i] - vertices[num3]).LengthSquared < num))
			{
				array[num2++] = vertices[i];
				num3 = i;
			}
		}
		if (num3 < count - 1)
		{
			array[num2++] = vertices[count - 1];
		}
		if (lineDeviationTol > 0.0)
		{
			array2[0] = (array2[num2 - 1] = true);
			simplifyDP(lineDeviationTol, array, 0, num2 - 1, array2);
		}
		else
		{
			for (i = 0; i < num2; i++)
			{
				array2[i] = true;
			}
		}
		vertices = new List<Vector2d>();
		for (i = 0; i < num2; i++)
		{
			if (array2[i])
			{
				vertices.Add(array[i]);
			}
		}
		Timestamp++;
	}

	public PolyLine2d Transform(ITransform2 xform)
	{
		int count = vertices.Count;
		for (int i = 0; i < count; i++)
		{
			vertices[i] = xform.TransformP(vertices[i]);
		}
		return this;
	}

	public static PolyLine2d MakeBoxSpiral(Vector2d center, double len, double spacing)
	{
		PolyLine2d polyLine2d = new PolyLine2d();
		polyLine2d.AppendVertex(center);
		Vector2d v = center;
		v.x += spacing / 2.0;
		polyLine2d.AppendVertex(v);
		v.y += spacing;
		polyLine2d.AppendVertex(v);
		double num = spacing / 2.0 + spacing;
		double num2 = spacing / 2.0;
		double num3 = spacing;
		double num4 = -1.0;
		while (num < len)
		{
			num2 += spacing;
			v.x += num4 * num2;
			polyLine2d.AppendVertex(v);
			num += num2;
			num3 += spacing;
			v.y += num4 * num3;
			polyLine2d.AppendVertex(v);
			num += num3;
			num4 *= -1.0;
		}
		return polyLine2d;
	}
}
