namespace Unity.Cinemachine;

internal class Vertex
{
	public readonly Point64 pt;

	public Vertex? next;

	public Vertex? prev;

	public VertexFlags flags;

	public Vertex(Point64 pt, VertexFlags flags, Vertex? prev)
	{
		this.pt = pt;
		this.flags = flags;
		next = null;
		this.prev = prev;
	}
}
