using System.Collections.Generic;

namespace g3;

public interface ISampledCurve3d
{
	int VertexCount { get; }

	int SegmentCount { get; }

	bool Closed { get; }

	IEnumerable<Vector3d> Vertices { get; }

	Vector3d GetVertex(int i);

	Segment3d GetSegment(int i);
}
