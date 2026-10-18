using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace g3;

public class PolyLine3d : IEnumerable<Vector3d>, IEnumerable
{
	protected List<Vector3d> vertices;

	public int Timestamp;

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

	public Vector3d End => vertices[vertices.Count - 1];

	public ReadOnlyCollection<Vector3d> Vertices => vertices.AsReadOnly();

	public int VertexCount => vertices.Count;

	public PolyLine3d()
	{
		vertices = new List<Vector3d>();
		Timestamp = 0;
	}

	public PolyLine3d(PolyLine3d copy)
	{
		vertices = new List<Vector3d>(copy.vertices);
		Timestamp = 0;
	}

	public PolyLine3d(Vector3d[] v)
	{
		vertices = new List<Vector3d>(v);
		Timestamp = 0;
	}

	public PolyLine3d(IEnumerable<Vector3d> v)
	{
		vertices = new List<Vector3d>(v);
		Timestamp = 0;
	}

	public PolyLine3d(VectorArray3d v)
	{
		vertices = new List<Vector3d>(v.AsVector3d());
		Timestamp = 0;
	}

	public void AppendVertex(Vector3d v)
	{
		vertices.Add(v);
		Timestamp++;
	}

	public Vector3d GetTangent(int i)
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

	public AxisAlignedBox3d GetBounds()
	{
		if (vertices.Count == 0)
		{
			return AxisAlignedBox3d.Empty;
		}
		AxisAlignedBox3d result = new AxisAlignedBox3d(vertices[0]);
		for (int i = 1; i < vertices.Count; i++)
		{
			result.Contain(vertices[i]);
		}
		return result;
	}

	public IEnumerable<Segment3d> SegmentItr()
	{
		int i = 0;
		while (i < vertices.Count - 1)
		{
			yield return new Segment3d(vertices[i], vertices[i + 1]);
			int num = i + 1;
			i = num;
		}
	}

	public IEnumerator<Vector3d> GetEnumerator()
	{
		return vertices.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return vertices.GetEnumerator();
	}
}
