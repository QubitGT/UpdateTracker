namespace Unity.Cinemachine;

internal struct IntersectNode
{
	public readonly Point64 pt;

	public readonly Active edge1;

	public readonly Active edge2;

	public IntersectNode(Point64 pt, Active edge1, Active edge2)
	{
		this.pt = pt;
		this.edge1 = edge1;
		this.edge2 = edge2;
	}
}
