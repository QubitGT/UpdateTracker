namespace Technie.PhysicsCreator.QHull;

public class Vertex
{
	public Point3d pnt;

	public int index;

	public Vertex prev;

	public Vertex next;

	public Face face;

	public Vertex()
	{
		pnt = new Point3d();
	}

	public Vertex(double x, double y, double z, int idx)
	{
		pnt = new Point3d(x, y, z);
		index = idx;
	}
}
