namespace Technie.PhysicsCreator.QHull;

public class HalfEdge
{
	public Vertex vertex;

	public Face face;

	public HalfEdge next;

	public HalfEdge prev;

	public HalfEdge opposite;

	public HalfEdge(Vertex v, Face f)
	{
		vertex = v;
		face = f;
	}

	public HalfEdge()
	{
	}

	public void setNext(HalfEdge edge)
	{
		next = edge;
	}

	public HalfEdge getNext()
	{
		return next;
	}

	public void setPrev(HalfEdge edge)
	{
		prev = edge;
	}

	public HalfEdge getPrev()
	{
		return prev;
	}

	public Face getFace()
	{
		return face;
	}

	public HalfEdge getOpposite()
	{
		return opposite;
	}

	public void setOpposite(HalfEdge edge)
	{
		opposite = edge;
		edge.opposite = this;
	}

	public Vertex head()
	{
		return vertex;
	}

	public Vertex tail()
	{
		if (prev == null)
		{
			return null;
		}
		return prev.vertex;
	}

	public Face oppositeFace()
	{
		if (opposite == null)
		{
			return null;
		}
		return opposite.face;
	}

	public string getVertexString()
	{
		if (tail() != null)
		{
			return tail().index + "-" + head().index;
		}
		return "?-" + head().index;
	}

	public double length()
	{
		if (tail() != null)
		{
			return head().pnt.distance(tail().pnt);
		}
		return -1.0;
	}

	public double lengthSquared()
	{
		if (tail() != null)
		{
			return head().pnt.distanceSquared(tail().pnt);
		}
		return -1.0;
	}
}
