using System;
using System.Collections.Generic;

namespace Technie.PhysicsCreator.QHull;

public class QuickHull3D
{
	public const int CLOCKWISE = 1;

	public const int INDEXED_FROM_ONE = 2;

	public const int INDEXED_FROM_ZERO = 4;

	public const int POINT_RELATIVE = 8;

	public const double AUTOMATIC_TOLERANCE = -1.0;

	protected int findIndex = -1;

	protected double charLength;

	protected bool debug;

	protected Vertex[] pointBuffer = new Vertex[0];

	protected int[] vertexPointIndices = new int[0];

	private Face[] discardedFaces = new Face[3];

	private Vertex[] maxVtxs = new Vertex[3];

	private Vertex[] minVtxs = new Vertex[3];

	protected List<Face> faces = new List<Face>(16);

	protected List<HalfEdge> horizon = new List<HalfEdge>(16);

	private FaceList newFaces = new FaceList();

	private VertexList unclaimed = new VertexList();

	private VertexList claimed = new VertexList();

	protected int numVertices;

	protected int numFaces;

	protected int numPoints;

	protected double explicitTolerance = -1.0;

	protected double tolerance;

	private const double DOUBLE_PREC = 2.220446049250313E-16;

	private const int NONCONVEX_WRT_LARGER_FACE = 1;

	private const int NONCONVEX = 2;

	public bool getDebug()
	{
		return debug;
	}

	public void setDebug(bool enable)
	{
		debug = enable;
	}

	public double getDistanceTolerance()
	{
		return tolerance;
	}

	public void setExplicitDistanceTolerance(double tol)
	{
		explicitTolerance = tol;
	}

	public double getExplicitDistanceTolerance()
	{
		return explicitTolerance;
	}

	private void addPointToFace(Vertex vtx, Face face)
	{
		vtx.face = face;
		if (face.outside == null)
		{
			claimed.add(vtx);
		}
		else
		{
			claimed.insertBefore(vtx, face.outside);
		}
		face.outside = vtx;
	}

	private void removePointFromFace(Vertex vtx, Face face)
	{
		if (vtx == face.outside)
		{
			if (vtx.next != null && vtx.next.face == face)
			{
				face.outside = vtx.next;
			}
			else
			{
				face.outside = null;
			}
		}
		claimed.delete(vtx);
	}

	private Vertex removeAllPointsFromFace(Face face)
	{
		if (face.outside != null)
		{
			Vertex vertex = face.outside;
			while (vertex.next != null && vertex.next.face == face)
			{
				vertex = vertex.next;
			}
			claimed.delete(face.outside, vertex);
			vertex.next = null;
			return face.outside;
		}
		return null;
	}

	public QuickHull3D()
	{
	}

	public QuickHull3D(double[] coords)
	{
		build(coords, coords.Length / 3);
	}

	public QuickHull3D(Point3d[] points)
	{
		build(points, points.Length);
	}

	private HalfEdge findHalfEdge(Vertex tail, Vertex head)
	{
		foreach (Face face in faces)
		{
			HalfEdge halfEdge = face.findEdge(tail, head);
			if (halfEdge != null)
			{
				return halfEdge;
			}
		}
		return null;
	}

	protected void setHull(double[] coords, int nump, int[][] faceIndices, int numf)
	{
		initBuffers(nump);
		setPoints(coords, nump);
		computeMaxAndMin();
		for (int i = 0; i < numf; i++)
		{
			Face face = Face.create(pointBuffer, faceIndices[i]);
			HalfEdge halfEdge = face.he0;
			do
			{
				HalfEdge halfEdge2 = findHalfEdge(halfEdge.head(), halfEdge.tail());
				if (halfEdge2 != null)
				{
					halfEdge.setOpposite(halfEdge2);
				}
				halfEdge = halfEdge.next;
			}
			while (halfEdge != face.he0);
			faces.Add(face);
		}
	}

	public void build(double[] coords)
	{
		build(coords, coords.Length / 3);
	}

	public void build(double[] coords, int nump)
	{
		if (nump < 4)
		{
			throw new SystemException("Less than four input points specified");
		}
		if (coords.Length / 3 < nump)
		{
			throw new SystemException("Coordinate array too small for specified number of points");
		}
		initBuffers(nump);
		setPoints(coords, nump);
		buildHull();
	}

	public void build(Point3d[] points)
	{
		build(points, points.Length);
	}

	public void build(Point3d[] points, int nump)
	{
		if (nump < 4)
		{
			throw new SystemException("Less than four input points specified");
		}
		if (points.Length < nump)
		{
			throw new SystemException("Point array too small for specified number of points");
		}
		initBuffers(nump);
		setPoints(points, nump);
		buildHull();
	}

	public void triangulate()
	{
		double minArea = 1000.0 * charLength * 2.220446049250313E-16;
		newFaces.clear();
		foreach (Face face2 in faces)
		{
			if (face2.mark == 1)
			{
				face2.triangulate(newFaces, minArea);
			}
		}
		for (Face face = newFaces.first(); face != null; face = face.next)
		{
			faces.Add(face);
		}
	}

	protected void initBuffers(int nump)
	{
		if (pointBuffer.Length < nump)
		{
			Vertex[] array = new Vertex[nump];
			vertexPointIndices = new int[nump];
			for (int i = 0; i < pointBuffer.Length; i++)
			{
				array[i] = pointBuffer[i];
			}
			for (int j = pointBuffer.Length; j < nump; j++)
			{
				array[j] = new Vertex();
			}
			pointBuffer = array;
		}
		faces.Clear();
		claimed.clear();
		numFaces = 0;
		numPoints = nump;
	}

	protected void setPoints(double[] coords, int nump)
	{
		for (int i = 0; i < nump; i++)
		{
			Vertex obj = pointBuffer[i];
			obj.pnt.set(coords[i * 3], coords[i * 3 + 1], coords[i * 3 + 2]);
			obj.index = i;
		}
	}

	protected void setPoints(Point3d[] pnts, int nump)
	{
		for (int i = 0; i < nump; i++)
		{
			Vertex obj = pointBuffer[i];
			obj.pnt.set(pnts[i]);
			obj.index = i;
		}
	}

	protected void computeMaxAndMin()
	{
		Vector3d vector3d = new Vector3d();
		Vector3d vector3d2 = new Vector3d();
		for (int i = 0; i < 3; i++)
		{
			maxVtxs[i] = (minVtxs[i] = pointBuffer[0]);
		}
		vector3d.set(pointBuffer[0].pnt);
		vector3d2.set(pointBuffer[0].pnt);
		for (int j = 1; j < numPoints; j++)
		{
			Point3d pnt = pointBuffer[j].pnt;
			if (pnt.x > vector3d.x)
			{
				vector3d.x = pnt.x;
				maxVtxs[0] = pointBuffer[j];
			}
			else if (pnt.x < vector3d2.x)
			{
				vector3d2.x = pnt.x;
				minVtxs[0] = pointBuffer[j];
			}
			if (pnt.y > vector3d.y)
			{
				vector3d.y = pnt.y;
				maxVtxs[1] = pointBuffer[j];
			}
			else if (pnt.y < vector3d2.y)
			{
				vector3d2.y = pnt.y;
				minVtxs[1] = pointBuffer[j];
			}
			if (pnt.z > vector3d.z)
			{
				vector3d.z = pnt.z;
				maxVtxs[2] = pointBuffer[j];
			}
			else if (pnt.z < vector3d2.z)
			{
				vector3d2.z = pnt.z;
				minVtxs[2] = pointBuffer[j];
			}
		}
		charLength = Math.Max(vector3d.x - vector3d2.x, vector3d.y - vector3d2.y);
		charLength = Math.Max(vector3d.z - vector3d2.z, charLength);
		if (explicitTolerance == -1.0)
		{
			tolerance = 6.661338147750939E-16 * (Math.Max(Math.Abs(vector3d.x), Math.Abs(vector3d2.x)) + Math.Max(Math.Abs(vector3d.y), Math.Abs(vector3d2.y)) + Math.Max(Math.Abs(vector3d.z), Math.Abs(vector3d2.z)));
		}
		else
		{
			tolerance = explicitTolerance;
		}
	}

	protected void createInitialSimplex()
	{
		double num = 0.0;
		int num2 = 0;
		for (int i = 0; i < 3; i++)
		{
			double num3 = maxVtxs[i].pnt.get(i) - minVtxs[i].pnt.get(i);
			if (num3 > num)
			{
				num = num3;
				num2 = i;
			}
		}
		if (num <= tolerance)
		{
			throw new SystemException("Input points appear to be coincident");
		}
		Vertex[] array = new Vertex[4]
		{
			maxVtxs[num2],
			minVtxs[num2],
			null,
			null
		};
		Vector3d vector3d = new Vector3d();
		Vector3d vector3d2 = new Vector3d();
		Vector3d vector3d3 = new Vector3d();
		Vector3d vector3d4 = new Vector3d();
		double num4 = 0.0;
		vector3d.sub(array[1].pnt, array[0].pnt);
		vector3d.normalize();
		for (int j = 0; j < numPoints; j++)
		{
			vector3d2.sub(pointBuffer[j].pnt, array[0].pnt);
			vector3d4.cross(vector3d, vector3d2);
			double num5 = vector3d4.normSquared();
			if (num5 > num4 && pointBuffer[j] != array[0] && pointBuffer[j] != array[1])
			{
				num4 = num5;
				array[2] = pointBuffer[j];
				vector3d3.set(vector3d4);
			}
		}
		if (Math.Sqrt(num4) <= 100.0 * tolerance)
		{
			throw new SystemException("Input points appear to be colinear");
		}
		vector3d3.normalize();
		double num6 = 0.0;
		double num7 = array[2].pnt.dot(vector3d3);
		for (int k = 0; k < numPoints; k++)
		{
			double num8 = Math.Abs(pointBuffer[k].pnt.dot(vector3d3) - num7);
			if (num8 > num6 && pointBuffer[k] != array[0] && pointBuffer[k] != array[1] && pointBuffer[k] != array[2])
			{
				num6 = num8;
				array[3] = pointBuffer[k];
			}
		}
		if (Math.Abs(num6) <= 100.0 * tolerance)
		{
			throw new SystemException("Input points appear to be coplanar");
		}
		Face[] array2 = new Face[4];
		if (array[3].pnt.dot(vector3d3) - num7 < 0.0)
		{
			array2[0] = Face.createTriangle(array[0], array[1], array[2]);
			array2[1] = Face.createTriangle(array[3], array[1], array[0]);
			array2[2] = Face.createTriangle(array[3], array[2], array[1]);
			array2[3] = Face.createTriangle(array[3], array[0], array[2]);
			for (int l = 0; l < 3; l++)
			{
				int num9 = (l + 1) % 3;
				array2[l + 1].getEdge(1).setOpposite(array2[num9 + 1].getEdge(0));
				array2[l + 1].getEdge(2).setOpposite(array2[0].getEdge(num9));
			}
		}
		else
		{
			array2[0] = Face.createTriangle(array[0], array[2], array[1]);
			array2[1] = Face.createTriangle(array[3], array[0], array[1]);
			array2[2] = Face.createTriangle(array[3], array[1], array[2]);
			array2[3] = Face.createTriangle(array[3], array[2], array[0]);
			for (int m = 0; m < 3; m++)
			{
				int num10 = (m + 1) % 3;
				array2[m + 1].getEdge(0).setOpposite(array2[num10 + 1].getEdge(1));
				array2[m + 1].getEdge(2).setOpposite(array2[0].getEdge((3 - m) % 3));
			}
		}
		for (int n = 0; n < 4; n++)
		{
			faces.Add(array2[n]);
		}
		for (int num11 = 0; num11 < numPoints; num11++)
		{
			Vertex vertex = pointBuffer[num11];
			if (vertex == array[0] || vertex == array[1] || vertex == array[2] || vertex == array[3])
			{
				continue;
			}
			num6 = tolerance;
			Face face = null;
			for (int num12 = 0; num12 < 4; num12++)
			{
				double num13 = array2[num12].distanceToPlane(vertex.pnt);
				if (num13 > num6)
				{
					face = array2[num12];
					num6 = num13;
				}
			}
			if (face != null)
			{
				addPointToFace(vertex, face);
			}
		}
	}

	public int getNumVertices()
	{
		return numVertices;
	}

	public Point3d[] getVertices()
	{
		Point3d[] array = new Point3d[numVertices];
		for (int i = 0; i < numVertices; i++)
		{
			array[i] = pointBuffer[vertexPointIndices[i]].pnt;
		}
		return array;
	}

	public int getVertices(double[] coords)
	{
		for (int i = 0; i < numVertices; i++)
		{
			Point3d pnt = pointBuffer[vertexPointIndices[i]].pnt;
			coords[i * 3] = pnt.x;
			coords[i * 3 + 1] = pnt.y;
			coords[i * 3 + 2] = pnt.z;
		}
		return numVertices;
	}

	public int[] getVertexPointIndices()
	{
		int[] array = new int[numVertices];
		for (int i = 0; i < numVertices; i++)
		{
			array[i] = vertexPointIndices[i];
		}
		return array;
	}

	public int getNumFaces()
	{
		return faces.Count;
	}

	public int[][] getFaces()
	{
		return getFaces(0);
	}

	public int[][] getFaces(int indexFlags)
	{
		int[][] array = new int[faces.Count][];
		int num = 0;
		foreach (Face face in faces)
		{
			array[num] = new int[face.numVertices()];
			getFaceIndices(array[num], face, indexFlags);
			num++;
		}
		return array;
	}

	private void getFaceIndices(int[] indices, Face face, int flags)
	{
		bool flag = (flags & 1) == 0;
		bool flag2 = (flags & 2) != 0;
		bool flag3 = (flags & 8) != 0;
		HalfEdge halfEdge = face.he0;
		int num = 0;
		do
		{
			int num2 = halfEdge.head().index;
			if (flag3)
			{
				num2 = vertexPointIndices[num2];
			}
			if (flag2)
			{
				num2++;
			}
			indices[num++] = num2;
			halfEdge = (flag ? halfEdge.next : halfEdge.prev);
		}
		while (halfEdge != face.he0);
	}

	protected void resolveUnclaimedPoints(FaceList newFaces)
	{
		Vertex vertex = unclaimed.first();
		for (Vertex vertex2 = vertex; vertex2 != null; vertex2 = vertex)
		{
			vertex = vertex2.next;
			double num = tolerance;
			Face face = null;
			for (Face face2 = newFaces.first(); face2 != null; face2 = face2.next)
			{
				if (face2.mark == 1)
				{
					double num2 = face2.distanceToPlane(vertex2.pnt);
					if (num2 > num)
					{
						num = num2;
						face = face2;
					}
					if (num > 1000.0 * tolerance)
					{
						break;
					}
				}
			}
			if (face != null)
			{
				addPointToFace(vertex2, face);
			}
		}
	}

	protected void deleteFacePoints(Face face, Face absorbingFace)
	{
		Vertex vertex = removeAllPointsFromFace(face);
		if (vertex == null)
		{
			return;
		}
		if (absorbingFace == null)
		{
			unclaimed.addAll(vertex);
			return;
		}
		Vertex vertex2 = vertex;
		for (Vertex vertex3 = vertex2; vertex3 != null; vertex3 = vertex2)
		{
			vertex2 = vertex3.next;
			if (absorbingFace.distanceToPlane(vertex3.pnt) > tolerance)
			{
				addPointToFace(vertex3, absorbingFace);
			}
			else
			{
				unclaimed.add(vertex3);
			}
		}
	}

	protected double oppFaceDistance(HalfEdge he)
	{
		return he.face.distanceToPlane(he.opposite.face.getCentroid());
	}

	private bool doAdjacentMerge(Face face, int mergeType)
	{
		HalfEdge halfEdge = face.he0;
		bool flag = true;
		do
		{
			Face face2 = halfEdge.oppositeFace();
			bool flag2 = false;
			if (mergeType == 2)
			{
				if (oppFaceDistance(halfEdge) > 0.0 - tolerance || oppFaceDistance(halfEdge.opposite) > 0.0 - tolerance)
				{
					flag2 = true;
				}
			}
			else if (face.area > face2.area)
			{
				if (oppFaceDistance(halfEdge) > 0.0 - tolerance)
				{
					flag2 = true;
				}
				else if (oppFaceDistance(halfEdge.opposite) > 0.0 - tolerance)
				{
					flag = false;
				}
			}
			else if (oppFaceDistance(halfEdge.opposite) > 0.0 - tolerance)
			{
				flag2 = true;
			}
			else if (oppFaceDistance(halfEdge) > 0.0 - tolerance)
			{
				flag = false;
			}
			if (flag2)
			{
				int num = face.mergeAdjacentFace(halfEdge, discardedFaces);
				for (int i = 0; i < num; i++)
				{
					deleteFacePoints(discardedFaces[i], face);
				}
				return true;
			}
			halfEdge = halfEdge.next;
		}
		while (halfEdge != face.he0);
		if (!flag)
		{
			face.mark = 2;
		}
		return false;
	}

	protected void calculateHorizon(Point3d eyePnt, HalfEdge edge0, Face face, List<HalfEdge> horizon)
	{
		deleteFacePoints(face, null);
		face.mark = 3;
		HalfEdge halfEdge;
		if (edge0 == null)
		{
			edge0 = face.getEdge(0);
			halfEdge = edge0;
		}
		else
		{
			halfEdge = edge0.getNext();
		}
		do
		{
			Face face2 = halfEdge.oppositeFace();
			if (face2.mark == 1)
			{
				if (face2.distanceToPlane(eyePnt) > tolerance)
				{
					calculateHorizon(eyePnt, halfEdge.getOpposite(), face2, horizon);
				}
				else
				{
					horizon.Add(halfEdge);
				}
			}
			halfEdge = halfEdge.getNext();
		}
		while (halfEdge != edge0);
	}

	private HalfEdge addAdjoiningFace(Vertex eyeVtx, HalfEdge he)
	{
		Face face = Face.createTriangle(eyeVtx, he.tail(), he.head());
		faces.Add(face);
		face.getEdge(-1).setOpposite(he.getOpposite());
		return face.getEdge(0);
	}

	protected void addNewFaces(FaceList newFaces, Vertex eyeVtx, List<HalfEdge> horizon)
	{
		newFaces.clear();
		HalfEdge halfEdge = null;
		HalfEdge halfEdge2 = null;
		foreach (HalfEdge item in horizon)
		{
			HalfEdge halfEdge3 = addAdjoiningFace(eyeVtx, item);
			if (halfEdge != null)
			{
				halfEdge3.next.setOpposite(halfEdge);
			}
			else
			{
				halfEdge2 = halfEdge3;
			}
			newFaces.add(halfEdge3.getFace());
			halfEdge = halfEdge3;
		}
		halfEdge2.next.setOpposite(halfEdge);
	}

	protected Vertex nextPointToAdd()
	{
		if (!claimed.isEmpty())
		{
			Face face = claimed.first().face;
			Vertex result = null;
			double num = 0.0;
			Vertex vertex = face.outside;
			while (vertex != null && vertex.face == face)
			{
				double num2 = face.distanceToPlane(vertex.pnt);
				if (num2 > num)
				{
					num = num2;
					result = vertex;
				}
				vertex = vertex.next;
			}
			return result;
		}
		return null;
	}

	protected void addPointToHull(Vertex eyeVtx)
	{
		horizon.Clear();
		unclaimed.clear();
		removePointFromFace(eyeVtx, eyeVtx.face);
		calculateHorizon(eyeVtx.pnt, null, eyeVtx.face, horizon);
		newFaces.clear();
		addNewFaces(newFaces, eyeVtx, horizon);
		for (Face face = newFaces.first(); face != null; face = face.next)
		{
			if (face.mark == 1)
			{
				while (doAdjacentMerge(face, 1))
				{
				}
			}
		}
		for (Face face2 = newFaces.first(); face2 != null; face2 = face2.next)
		{
			if (face2.mark == 2)
			{
				face2.mark = 1;
				while (doAdjacentMerge(face2, 2))
				{
				}
			}
		}
		resolveUnclaimedPoints(newFaces);
	}

	protected void buildHull()
	{
		int num = 0;
		computeMaxAndMin();
		createInitialSimplex();
		Vertex eyeVtx;
		while ((eyeVtx = nextPointToAdd()) != null)
		{
			addPointToHull(eyeVtx);
			num++;
		}
		reindexFacesAndVertices();
	}

	private void markFaceVertices(Face face, int mark)
	{
		HalfEdge firstEdge = face.getFirstEdge();
		HalfEdge halfEdge = firstEdge;
		do
		{
			halfEdge.head().index = mark;
			halfEdge = halfEdge.next;
		}
		while (halfEdge != firstEdge);
	}

	protected void reindexFacesAndVertices()
	{
		for (int i = 0; i < numPoints; i++)
		{
			pointBuffer[i].index = -1;
		}
		numFaces = 0;
		for (int j = 0; j < faces.Count; j++)
		{
			Face face = faces[j];
			if (face.mark != 1)
			{
				faces.RemoveAt(j);
				j--;
			}
			else
			{
				markFaceVertices(face, 0);
				numFaces++;
			}
		}
		numVertices = 0;
		for (int k = 0; k < numPoints; k++)
		{
			Vertex vertex = pointBuffer[k];
			if (vertex.index == 0)
			{
				vertexPointIndices[numVertices] = k;
				vertex.index = numVertices++;
			}
		}
	}

	protected bool checkFaceConvexity(Face face, double tol)
	{
		HalfEdge halfEdge = face.he0;
		do
		{
			face.checkConsistency();
			if (oppFaceDistance(halfEdge) > tol)
			{
				return false;
			}
			if (oppFaceDistance(halfEdge.opposite) > tol)
			{
				return false;
			}
			if (halfEdge.next.oppositeFace() == halfEdge.oppositeFace())
			{
				return false;
			}
			halfEdge = halfEdge.next;
		}
		while (halfEdge != face.he0);
		return true;
	}

	protected bool checkFaces(double tol)
	{
		bool result = true;
		foreach (Face face in faces)
		{
			if (face.mark == 1 && !checkFaceConvexity(face, tol))
			{
				result = false;
			}
		}
		return result;
	}

	public bool check()
	{
		return check(getDistanceTolerance());
	}

	public bool check(double tol)
	{
		double num = 10.0 * tol;
		if (!checkFaces(tolerance))
		{
			return false;
		}
		for (int i = 0; i < numPoints; i++)
		{
			Point3d pnt = pointBuffer[i].pnt;
			foreach (Face face in faces)
			{
				if (face.mark == 1 && face.distanceToPlane(pnt) > num)
				{
					return false;
				}
			}
		}
		return true;
	}
}
