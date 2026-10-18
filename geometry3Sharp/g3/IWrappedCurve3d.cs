using System.Collections.Generic;

namespace g3;

public class IWrappedCurve3d : ISampledCurve3d
{
	public IList<Vector3d> VertexList;

	public bool Closed { get; set; }

	public int VertexCount
	{
		get
		{
			if (VertexList != null)
			{
				return VertexList.Count;
			}
			return 0;
		}
	}

	public int SegmentCount
	{
		get
		{
			if (!Closed)
			{
				return VertexCount - 1;
			}
			return VertexCount;
		}
	}

	public IEnumerable<Vector3d> Vertices => VertexList;

	public Vector3d GetVertex(int i)
	{
		return VertexList[i];
	}

	public Segment3d GetSegment(int iSegment)
	{
		if (!Closed)
		{
			return new Segment3d(VertexList[iSegment], VertexList[iSegment + 1]);
		}
		return new Segment3d(VertexList[iSegment], VertexList[(iSegment + 1) % VertexList.Count]);
	}
}
