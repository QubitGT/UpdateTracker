using System;

namespace Technie.PhysicsCreator.QHull;

public class Face
{
	public HalfEdge he0;

	private Vector3d normal;

	public double area;

	private Point3d centroid;

	public double planeOffset;

	public int index;

	public int numVerts;

	public Face next;

	public const int VISIBLE = 1;

	public const int NON_CONVEX = 2;

	public const int DELETED = 3;

	public int mark = 1;

	public Vertex outside;

	public void computeCentroid(Point3d centroid)
	{
		centroid.setZero();
		HalfEdge halfEdge = he0;
		do
		{
			centroid.add(halfEdge.head().pnt);
			halfEdge = halfEdge.next;
		}
		while (halfEdge != he0);
		centroid.scale(1.0 / (double)numVerts);
	}

	public void computeNormal(Vector3d normal, double minArea)
	{
		computeNormal(normal);
		if (!(area < minArea))
		{
			return;
		}
		HalfEdge halfEdge = null;
		double num = 0.0;
		HalfEdge halfEdge2 = he0;
		do
		{
			double num2 = halfEdge2.lengthSquared();
			if (num2 > num)
			{
				halfEdge = halfEdge2;
				num = num2;
			}
			halfEdge2 = halfEdge2.next;
		}
		while (halfEdge2 != he0);
		Point3d pnt = halfEdge.head().pnt;
		Point3d pnt2 = halfEdge.tail().pnt;
		double num3 = Math.Sqrt(num);
		double num4 = (pnt.x - pnt2.x) / num3;
		double num5 = (pnt.y - pnt2.y) / num3;
		double num6 = (pnt.z - pnt2.z) / num3;
		double num7 = normal.x * num4 + normal.y * num5 + normal.z * num6;
		normal.x -= num7 * num4;
		normal.y -= num7 * num5;
		normal.z -= num7 * num6;
		normal.normalize();
	}

	public void computeNormal(Vector3d normal)
	{
		HalfEdge halfEdge = he0.next;
		HalfEdge halfEdge2 = halfEdge.next;
		Point3d pnt = he0.head().pnt;
		Point3d pnt2 = halfEdge.head().pnt;
		double num = pnt2.x - pnt.x;
		double num2 = pnt2.y - pnt.y;
		double num3 = pnt2.z - pnt.z;
		normal.setZero();
		numVerts = 2;
		while (halfEdge2 != he0)
		{
			double num4 = num;
			double num5 = num2;
			double num6 = num3;
			Point3d pnt3 = halfEdge2.head().pnt;
			num = pnt3.x - pnt.x;
			num2 = pnt3.y - pnt.y;
			num3 = pnt3.z - pnt.z;
			normal.x += num5 * num3 - num6 * num2;
			normal.y += num6 * num - num4 * num3;
			normal.z += num4 * num2 - num5 * num;
			halfEdge2 = halfEdge2.next;
			numVerts++;
		}
		area = normal.norm();
		normal.scale(1.0 / area);
	}

	private void computeNormalAndCentroid()
	{
		computeNormal(normal);
		computeCentroid(centroid);
		planeOffset = normal.dot(centroid);
		int num = 0;
		HalfEdge halfEdge = he0;
		do
		{
			num++;
			halfEdge = halfEdge.next;
		}
		while (halfEdge != he0);
		if (num != numVerts)
		{
			throw new InternalErrorException("face " + getVertexString() + " numVerts=" + numVerts + " should be " + num);
		}
	}

	private void computeNormalAndCentroid(double minArea)
	{
		computeNormal(normal, minArea);
		computeCentroid(centroid);
		planeOffset = normal.dot(centroid);
	}

	public static Face createTriangle(Vertex v0, Vertex v1, Vertex v2)
	{
		return createTriangle(v0, v1, v2, 0.0);
	}

	public static Face createTriangle(Vertex v0, Vertex v1, Vertex v2, double minArea)
	{
		Face face = new Face();
		HalfEdge halfEdge = new HalfEdge(v0, face);
		HalfEdge halfEdge2 = new HalfEdge(v1, face);
		HalfEdge halfEdge3 = (halfEdge.prev = new HalfEdge(v2, face));
		halfEdge.next = halfEdge2;
		halfEdge2.prev = halfEdge;
		halfEdge2.next = halfEdge3;
		halfEdge3.prev = halfEdge2;
		halfEdge3.next = halfEdge;
		face.he0 = halfEdge;
		face.computeNormalAndCentroid(minArea);
		return face;
	}

	public static Face create(Vertex[] vtxArray, int[] indices)
	{
		Face face = new Face();
		HalfEdge halfEdge = null;
		for (int i = 0; i < indices.Length; i++)
		{
			HalfEdge halfEdge2 = new HalfEdge(vtxArray[indices[i]], face);
			if (halfEdge != null)
			{
				halfEdge2.setPrev(halfEdge);
				halfEdge.setNext(halfEdge2);
			}
			else
			{
				face.he0 = halfEdge2;
			}
			halfEdge = halfEdge2;
		}
		face.he0.setPrev(halfEdge);
		halfEdge.setNext(face.he0);
		face.computeNormalAndCentroid();
		return face;
	}

	public Face()
	{
		normal = new Vector3d();
		centroid = new Point3d();
		mark = 1;
	}

	public HalfEdge getEdge(int i)
	{
		HalfEdge prev = he0;
		while (i > 0)
		{
			prev = prev.next;
			i--;
		}
		while (i < 0)
		{
			prev = prev.prev;
			i++;
		}
		return prev;
	}

	public HalfEdge getFirstEdge()
	{
		return he0;
	}

	public HalfEdge findEdge(Vertex vt, Vertex vh)
	{
		HalfEdge halfEdge = he0;
		do
		{
			if (halfEdge.head() == vh && halfEdge.tail() == vt)
			{
				return halfEdge;
			}
			halfEdge = halfEdge.next;
		}
		while (halfEdge != he0);
		return null;
	}

	public double distanceToPlane(Point3d p)
	{
		return normal.x * p.x + normal.y * p.y + normal.z * p.z - planeOffset;
	}

	public Vector3d getNormal()
	{
		return normal;
	}

	public Point3d getCentroid()
	{
		return centroid;
	}

	public int numVertices()
	{
		return numVerts;
	}

	public string getVertexString()
	{
		string text = null;
		HalfEdge halfEdge = he0;
		do
		{
			text = ((text != null) ? (text + " " + halfEdge.head().index) : (halfEdge.head().index.ToString() ?? ""));
			halfEdge = halfEdge.next;
		}
		while (halfEdge != he0);
		return text;
	}

	public void getVertexIndices(int[] idxs)
	{
		HalfEdge halfEdge = he0;
		int num = 0;
		do
		{
			idxs[num++] = halfEdge.head().index;
			halfEdge = halfEdge.next;
		}
		while (halfEdge != he0);
	}

	private Face connectHalfEdges(HalfEdge hedgePrev, HalfEdge hedge)
	{
		Face result = null;
		if (hedgePrev.oppositeFace() == hedge.oppositeFace())
		{
			Face face = hedge.oppositeFace();
			if (hedgePrev == he0)
			{
				he0 = hedge;
			}
			HalfEdge opposite;
			if (face.numVertices() == 3)
			{
				opposite = hedge.getOpposite().prev.getOpposite();
				face.mark = 3;
				result = face;
			}
			else
			{
				opposite = hedge.getOpposite().next;
				if (face.he0 == opposite.prev)
				{
					face.he0 = opposite;
				}
				opposite.prev = opposite.prev.prev;
				opposite.prev.next = opposite;
			}
			hedge.prev = hedgePrev.prev;
			hedge.prev.next = hedge;
			hedge.opposite = opposite;
			opposite.opposite = hedge;
			face.computeNormalAndCentroid();
		}
		else
		{
			hedgePrev.next = hedge;
			hedge.prev = hedgePrev;
		}
		return result;
	}

	public void checkConsistency()
	{
		HalfEdge halfEdge = he0;
		double num = 0.0;
		int num2 = 0;
		if (numVerts < 3)
		{
			throw new InternalErrorException("degenerate face: " + getVertexString());
		}
		do
		{
			HalfEdge opposite = halfEdge.getOpposite();
			if (opposite == null)
			{
				throw new InternalErrorException("face " + getVertexString() + ": unreflected half edge " + halfEdge.getVertexString());
			}
			if (opposite.getOpposite() != halfEdge)
			{
				throw new InternalErrorException("face " + getVertexString() + ": opposite half edge " + opposite.getVertexString() + " has opposite " + opposite.getOpposite().getVertexString());
			}
			if (opposite.head() != halfEdge.tail() || halfEdge.head() != opposite.tail())
			{
				throw new InternalErrorException("face " + getVertexString() + ": half edge " + halfEdge.getVertexString() + " reflected by " + opposite.getVertexString());
			}
			Face face = opposite.face;
			if (face == null)
			{
				throw new InternalErrorException("face " + getVertexString() + ": no face on half edge " + opposite.getVertexString());
			}
			if (face.mark == 3)
			{
				throw new InternalErrorException("face " + getVertexString() + ": opposite face " + face.getVertexString() + " not on hull");
			}
			double num3 = Math.Abs(distanceToPlane(halfEdge.head().pnt));
			if (num3 > num)
			{
				num = num3;
			}
			num2++;
			halfEdge = halfEdge.next;
		}
		while (halfEdge != he0);
		if (num2 != numVerts)
		{
			throw new InternalErrorException("face " + getVertexString() + " numVerts=" + numVerts + " should be " + num2);
		}
	}

	public int mergeAdjacentFace(HalfEdge hedgeAdj, Face[] discarded)
	{
		Face face = hedgeAdj.oppositeFace();
		int result = 0;
		discarded[result++] = face;
		face.mark = 3;
		HalfEdge opposite = hedgeAdj.getOpposite();
		HalfEdge prev = hedgeAdj.prev;
		HalfEdge halfEdge = hedgeAdj.next;
		HalfEdge prev2 = opposite.prev;
		HalfEdge halfEdge2 = opposite.next;
		while (prev.oppositeFace() == face)
		{
			prev = prev.prev;
			halfEdge2 = halfEdge2.next;
		}
		while (halfEdge.oppositeFace() == face)
		{
			prev2 = prev2.prev;
			halfEdge = halfEdge.next;
		}
		for (HalfEdge halfEdge3 = halfEdge2; halfEdge3 != prev2.next; halfEdge3 = halfEdge3.next)
		{
			halfEdge3.face = this;
		}
		if (hedgeAdj == he0)
		{
			he0 = halfEdge;
		}
		Face face2 = connectHalfEdges(prev2, halfEdge);
		if (face2 != null)
		{
			discarded[result++] = face2;
		}
		face2 = connectHalfEdges(prev, halfEdge2);
		if (face2 != null)
		{
			discarded[result++] = face2;
		}
		computeNormalAndCentroid();
		checkConsistency();
		return result;
	}

	private double areaSquared(HalfEdge hedge0, HalfEdge hedge1)
	{
		Point3d pnt = hedge0.tail().pnt;
		Point3d pnt2 = hedge0.head().pnt;
		Point3d pnt3 = hedge1.head().pnt;
		double num = pnt2.x - pnt.x;
		double num2 = pnt2.y - pnt.y;
		double num3 = pnt2.z - pnt.z;
		double num4 = pnt3.x - pnt.x;
		double num5 = pnt3.y - pnt.y;
		double num6 = pnt3.z - pnt.z;
		double num7 = num2 * num6 - num3 * num5;
		double num8 = num3 * num4 - num * num6;
		double num9 = num * num5 - num2 * num4;
		return num7 * num7 + num8 * num8 + num9 * num9;
	}

	public void triangulate(FaceList newFaces, double minArea)
	{
		if (numVertices() < 4)
		{
			return;
		}
		Vertex v = he0.head();
		HalfEdge halfEdge = he0.next;
		HalfEdge opposite = halfEdge.opposite;
		Face face = null;
		for (halfEdge = halfEdge.next; halfEdge != he0.prev; halfEdge = halfEdge.next)
		{
			Face face2 = createTriangle(v, halfEdge.prev.head(), halfEdge.head(), minArea);
			face2.he0.next.setOpposite(opposite);
			face2.he0.prev.setOpposite(halfEdge.opposite);
			opposite = face2.he0;
			newFaces.add(face2);
			if (face == null)
			{
				face = face2;
			}
		}
		halfEdge = new HalfEdge(he0.prev.prev.head(), this);
		halfEdge.setOpposite(opposite);
		halfEdge.prev = he0;
		halfEdge.prev.next = halfEdge;
		halfEdge.next = he0.prev;
		halfEdge.next.prev = halfEdge;
		computeNormalAndCentroid(minArea);
		checkConsistency();
		for (Face face3 = face; face3 != null; face3 = face3.next)
		{
			face3.checkConsistency();
		}
	}
}
