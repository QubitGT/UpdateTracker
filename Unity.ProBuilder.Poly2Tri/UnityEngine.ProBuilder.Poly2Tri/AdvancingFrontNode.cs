namespace UnityEngine.ProBuilder.Poly2Tri;

internal class AdvancingFrontNode
{
	public AdvancingFrontNode Next;

	public AdvancingFrontNode Prev;

	public double Value;

	public TriangulationPoint Point;

	public DelaunayTriangle Triangle;

	public bool HasNext => Next != null;

	public bool HasPrev => Prev != null;

	public AdvancingFrontNode(TriangulationPoint point)
	{
		Point = point;
		Value = point.X;
	}
}
