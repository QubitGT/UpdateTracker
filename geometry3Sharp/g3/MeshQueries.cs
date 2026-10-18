using System;
using System.Collections.Generic;

namespace g3;

public static class MeshQueries
{
	public static DistPoint3Triangle3 TriangleDistance(DMesh3 mesh, int ti, Vector3d point)
	{
		if (!mesh.IsTriangle(ti))
		{
			return null;
		}
		Triangle3d triangleIn = default(Triangle3d);
		mesh.GetTriVertices(ti, ref triangleIn.V0, ref triangleIn.V1, ref triangleIn.V2);
		DistPoint3Triangle3 distPoint3Triangle = new DistPoint3Triangle3(point, triangleIn);
		distPoint3Triangle.GetSquared();
		return distPoint3Triangle;
	}

	public static Frame3f NearestPointFrame(DMesh3 mesh, ISpatial spatial, Vector3d queryPoint, bool bForceFaceNormal = false)
	{
		int num = spatial.FindNearestTriangle(queryPoint);
		Vector3d triangleClosest = TriangleDistance(mesh, num, queryPoint).TriangleClosest;
		if (mesh.HasVertexNormals && !bForceFaceNormal)
		{
			return SurfaceFrame(mesh, num, triangleClosest);
		}
		return new Frame3f(triangleClosest, mesh.GetTriNormal(num));
	}

	public static double NearestPointDistance(DMesh3 mesh, ISpatial spatial, Vector3d queryPoint, double maxDist = double.MaxValue)
	{
		int num = spatial.FindNearestTriangle(queryPoint, maxDist);
		if (num == -1)
		{
			return double.MaxValue;
		}
		Triangle3d triangle = default(Triangle3d);
		mesh.GetTriVertices(num, ref triangle.V0, ref triangle.V1, ref triangle.V2);
		Vector3d closestPoint;
		Vector3d baryCoords;
		return Math.Sqrt(DistPoint3Triangle3.DistanceSqr(ref queryPoint, ref triangle, out closestPoint, out baryCoords));
	}

	public static DistTriangle3Triangle3 TriangleTriangleDistance(DMesh3 mesh1, int ti, DMesh3 mesh2, int tj, Func<Vector3d, Vector3d> TransformF = null)
	{
		if (!mesh1.IsTriangle(ti) || !mesh2.IsTriangle(tj))
		{
			return null;
		}
		Triangle3d triangle0in = default(Triangle3d);
		Triangle3d triangle1in = default(Triangle3d);
		mesh1.GetTriVertices(ti, ref triangle0in.V0, ref triangle0in.V1, ref triangle0in.V2);
		mesh2.GetTriVertices(tj, ref triangle1in.V0, ref triangle1in.V1, ref triangle1in.V2);
		if (TransformF != null)
		{
			triangle1in.V0 = TransformF(triangle1in.V0);
			triangle1in.V1 = TransformF(triangle1in.V1);
			triangle1in.V2 = TransformF(triangle1in.V2);
		}
		DistTriangle3Triangle3 distTriangle3Triangle = new DistTriangle3Triangle3(triangle0in, triangle1in);
		distTriangle3Triangle.Compute();
		return distTriangle3Triangle;
	}

	public static IntrRay3Triangle3 TriangleIntersection(DMesh3 mesh, int ti, Ray3d ray)
	{
		if (!mesh.IsTriangle(ti))
		{
			return null;
		}
		Triangle3d t = default(Triangle3d);
		mesh.GetTriVertices(ti, ref t.V0, ref t.V1, ref t.V2);
		IntrRay3Triangle3 intrRay3Triangle = new IntrRay3Triangle3(ray, t);
		intrRay3Triangle.Find();
		return intrRay3Triangle;
	}

	public static IntrTriangle3Triangle3 TrianglesIntersection(DMesh3 mesh1, int ti, DMesh3 mesh2, int tj, Func<Vector3d, Vector3d> TransformF = null)
	{
		if (!mesh1.IsTriangle(ti) || !mesh2.IsTriangle(tj))
		{
			return null;
		}
		Triangle3d t = default(Triangle3d);
		Triangle3d t2 = default(Triangle3d);
		mesh1.GetTriVertices(ti, ref t.V0, ref t.V1, ref t.V2);
		mesh2.GetTriVertices(tj, ref t2.V0, ref t2.V1, ref t2.V2);
		if (TransformF != null)
		{
			t2.V0 = TransformF(t2.V0);
			t2.V1 = TransformF(t2.V1);
			t2.V2 = TransformF(t2.V2);
		}
		IntrTriangle3Triangle3 intrTriangle3Triangle = new IntrTriangle3Triangle3(t, t2);
		intrTriangle3Triangle.Find();
		return intrTriangle3Triangle;
	}

	public static DistTriangle3Triangle3 TrianglesDistance(DMesh3 mesh1, int ti, DMesh3 mesh2, int tj, Func<Vector3d, Vector3d> TransformF = null)
	{
		if (!mesh1.IsTriangle(ti) || !mesh2.IsTriangle(tj))
		{
			return null;
		}
		Triangle3d triangle0in = default(Triangle3d);
		Triangle3d triangle1in = default(Triangle3d);
		mesh1.GetTriVertices(ti, ref triangle0in.V0, ref triangle0in.V1, ref triangle0in.V2);
		mesh2.GetTriVertices(tj, ref triangle1in.V0, ref triangle1in.V1, ref triangle1in.V2);
		if (TransformF != null)
		{
			triangle1in.V0 = TransformF(triangle1in.V0);
			triangle1in.V1 = TransformF(triangle1in.V1);
			triangle1in.V2 = TransformF(triangle1in.V2);
		}
		DistTriangle3Triangle3 distTriangle3Triangle = new DistTriangle3Triangle3(triangle0in, triangle1in);
		distTriangle3Triangle.GetSquared();
		return distTriangle3Triangle;
	}

	public static bool RayHitPointFrame(DMesh3 mesh, ISpatial spatial, Ray3d ray, out Frame3f hitPosFrame, bool bForceFaceNormal = false)
	{
		hitPosFrame = default(Frame3f);
		int num = spatial.FindNearestHitTriangle(ray);
		if (num == -1)
		{
			return false;
		}
		IntrRay3Triangle3 intrRay3Triangle = TriangleIntersection(mesh, num, ray);
		if (intrRay3Triangle.Result != IntersectionResult.Intersects)
		{
			return false;
		}
		Vector3d vector3d = ray.PointAt(intrRay3Triangle.RayParameter);
		if (mesh.HasVertexNormals && !bForceFaceNormal)
		{
			hitPosFrame = SurfaceFrame(mesh, num, vector3d);
		}
		else
		{
			hitPosFrame = new Frame3f(vector3d, mesh.GetTriNormal(num));
		}
		return true;
	}

	public static Frame3f SurfaceFrame(DMesh3 mesh, int tID, Vector3d point, bool bForceFaceNormal = false)
	{
		if (!mesh.IsTriangle(tID))
		{
			throw new Exception("MeshQueries.SurfaceFrame: triangle " + tID + " does not exist!");
		}
		Triangle3d triangle3d = default(Triangle3d);
		mesh.GetTriVertices(tID, ref triangle3d.V0, ref triangle3d.V1, ref triangle3d.V2);
		Vector3d bary = triangle3d.BarycentricCoords(point);
		point = triangle3d.PointAt(bary);
		if (mesh.HasVertexNormals && !bForceFaceNormal)
		{
			Vector3d triBaryNormal = mesh.GetTriBaryNormal(tID, bary.x, bary.y, bary.z);
			return new Frame3f(point, triBaryNormal);
		}
		return new Frame3f(point, mesh.GetTriNormal(tID));
	}

	public static Vector3d BaryCoords(DMesh3 mesh, int tID, Vector3d point)
	{
		if (!mesh.IsTriangle(tID))
		{
			throw new Exception("MeshQueries.SurfaceFrame: triangle " + tID + " does not exist!");
		}
		Triangle3d triangle3d = default(Triangle3d);
		mesh.GetTriVertices(tID, ref triangle3d.V0, ref triangle3d.V1, ref triangle3d.V2);
		return triangle3d.BarycentricCoords(point);
	}

	public static double TriDistanceSqr(DMesh3 mesh, int ti, Vector3d point)
	{
		Vector3d v = Vector3d.Zero;
		Vector3d v2 = Vector3d.Zero;
		Vector3d v3 = Vector3d.Zero;
		mesh.GetTriVertices(ti, ref v, ref v2, ref v3);
		Vector3d vector3d = v - point;
		Vector3d v4 = v2 - v;
		Vector3d v5 = v3 - v;
		double lengthSquared = v4.LengthSquared;
		double num = v4.Dot(ref v5);
		double lengthSquared2 = v5.LengthSquared;
		double num2 = vector3d.Dot(ref v4);
		double num3 = vector3d.Dot(ref v5);
		double lengthSquared3 = vector3d.LengthSquared;
		double num4 = Math.Abs(lengthSquared * lengthSquared2 - num * num);
		double num5 = num * num3 - lengthSquared2 * num2;
		double num6 = num * num2 - lengthSquared * num3;
		double num7;
		if (num5 + num6 <= num4)
		{
			if (num5 < 0.0)
			{
				if (num6 < 0.0)
				{
					if (num2 < 0.0)
					{
						num6 = 0.0;
						if (0.0 - num2 >= lengthSquared)
						{
							num5 = 1.0;
							num7 = lengthSquared + 2.0 * num2 + lengthSquared3;
						}
						else
						{
							num5 = (0.0 - num2) / lengthSquared;
							num7 = num2 * num5 + lengthSquared3;
						}
					}
					else
					{
						num5 = 0.0;
						if (num3 >= 0.0)
						{
							num6 = 0.0;
							num7 = lengthSquared3;
						}
						else if (0.0 - num3 >= lengthSquared2)
						{
							num6 = 1.0;
							num7 = lengthSquared2 + 2.0 * num3 + lengthSquared3;
						}
						else
						{
							num6 = (0.0 - num3) / lengthSquared2;
							num7 = num3 * num6 + lengthSquared3;
						}
					}
				}
				else
				{
					num5 = 0.0;
					if (num3 >= 0.0)
					{
						num6 = 0.0;
						num7 = lengthSquared3;
					}
					else if (0.0 - num3 >= lengthSquared2)
					{
						num6 = 1.0;
						num7 = lengthSquared2 + 2.0 * num3 + lengthSquared3;
					}
					else
					{
						num6 = (0.0 - num3) / lengthSquared2;
						num7 = num3 * num6 + lengthSquared3;
					}
				}
			}
			else if (num6 < 0.0)
			{
				num6 = 0.0;
				if (num2 >= 0.0)
				{
					num5 = 0.0;
					num7 = lengthSquared3;
				}
				else if (0.0 - num2 >= lengthSquared)
				{
					num5 = 1.0;
					num7 = lengthSquared + 2.0 * num2 + lengthSquared3;
				}
				else
				{
					num5 = (0.0 - num2) / lengthSquared;
					num7 = num2 * num5 + lengthSquared3;
				}
			}
			else
			{
				double num8 = 1.0 / num4;
				num5 *= num8;
				num6 *= num8;
				num7 = num5 * (lengthSquared * num5 + num * num6 + 2.0 * num2) + num6 * (num * num5 + lengthSquared2 * num6 + 2.0 * num3) + lengthSquared3;
			}
		}
		else if (num5 < 0.0)
		{
			double num9 = num + num2;
			double num10 = lengthSquared2 + num3;
			if (num10 > num9)
			{
				double num11 = num10 - num9;
				double num12 = lengthSquared - 2.0 * num + lengthSquared2;
				if (num11 >= num12)
				{
					num5 = 1.0;
					num6 = 0.0;
					num7 = lengthSquared + 2.0 * num2 + lengthSquared3;
				}
				else
				{
					num5 = num11 / num12;
					num6 = 1.0 - num5;
					num7 = num5 * (lengthSquared * num5 + num * num6 + 2.0 * num2) + num6 * (num * num5 + lengthSquared2 * num6 + 2.0 * num3) + lengthSquared3;
				}
			}
			else
			{
				num5 = 0.0;
				if (num10 <= 0.0)
				{
					num6 = 1.0;
					num7 = lengthSquared2 + 2.0 * num3 + lengthSquared3;
				}
				else if (num3 >= 0.0)
				{
					num6 = 0.0;
					num7 = lengthSquared3;
				}
				else
				{
					num6 = (0.0 - num3) / lengthSquared2;
					num7 = num3 * num6 + lengthSquared3;
				}
			}
		}
		else if (num6 < 0.0)
		{
			double num9 = num + num3;
			double num10 = lengthSquared + num2;
			if (num10 > num9)
			{
				double num11 = num10 - num9;
				double num12 = lengthSquared - 2.0 * num + lengthSquared2;
				if (num11 >= num12)
				{
					num6 = 1.0;
					num5 = 0.0;
					num7 = lengthSquared2 + 2.0 * num3 + lengthSquared3;
				}
				else
				{
					num6 = num11 / num12;
					num5 = 1.0 - num6;
					num7 = num5 * (lengthSquared * num5 + num * num6 + 2.0 * num2) + num6 * (num * num5 + lengthSquared2 * num6 + 2.0 * num3) + lengthSquared3;
				}
			}
			else
			{
				num6 = 0.0;
				if (num10 <= 0.0)
				{
					num5 = 1.0;
					num7 = lengthSquared + 2.0 * num2 + lengthSquared3;
				}
				else if (num2 >= 0.0)
				{
					num5 = 0.0;
					num7 = lengthSquared3;
				}
				else
				{
					num5 = (0.0 - num2) / lengthSquared;
					num7 = num2 * num5 + lengthSquared3;
				}
			}
		}
		else
		{
			double num11 = lengthSquared2 + num3 - num - num2;
			if (num11 <= 0.0)
			{
				num5 = 0.0;
				num6 = 1.0;
				num7 = lengthSquared2 + 2.0 * num3 + lengthSquared3;
			}
			else
			{
				double num12 = lengthSquared - 2.0 * num + lengthSquared2;
				if (num11 >= num12)
				{
					num5 = 1.0;
					num6 = 0.0;
					num7 = lengthSquared + 2.0 * num2 + lengthSquared3;
				}
				else
				{
					num5 = num11 / num12;
					num6 = 1.0 - num5;
					num7 = num5 * (lengthSquared * num5 + num * num6 + 2.0 * num2) + num6 * (num * num5 + lengthSquared2 * num6 + 2.0 * num3) + lengthSquared3;
				}
			}
		}
		if (num7 < 0.0)
		{
			num7 = 0.0;
		}
		return num7;
	}

	public static int FindNearestVertex_LinearSearch(DMesh3 mesh, Vector3d p)
	{
		int result = -1;
		double num = double.MaxValue;
		foreach (int item in mesh.VertexIndices())
		{
			double num2 = mesh.GetVertex(item).DistanceSquared(p);
			if (num2 < num)
			{
				num = num2;
				result = item;
			}
		}
		return result;
	}

	public static int FindNearestTriangle_LinearSearch(DMesh3 mesh, Vector3d p)
	{
		int result = -1;
		double num = double.MaxValue;
		foreach (int item in mesh.TriangleIndices())
		{
			double num2 = TriDistanceSqr(mesh, item, p);
			if (num2 < num)
			{
				num = num2;
				result = item;
			}
		}
		return result;
	}

	public static int FindHitTriangle_LinearSearch(DMesh3 mesh, Ray3d ray)
	{
		int result = -1;
		double num = double.MaxValue;
		Triangle3d t = default(Triangle3d);
		foreach (int item in mesh.TriangleIndices())
		{
			mesh.GetTriVertices(item, ref t.V0, ref t.V1, ref t.V2);
			IntrRay3Triangle3 intrRay3Triangle = new IntrRay3Triangle3(ray, t);
			if (intrRay3Triangle.Find() && intrRay3Triangle.RayParameter < num)
			{
				num = intrRay3Triangle.RayParameter;
				result = item;
			}
		}
		return result;
	}

	public static Index2i FindIntersectingTriangles_LinearSearch(DMesh3 mesh1, DMesh3 mesh2)
	{
		foreach (int item in mesh1.TriangleIndices())
		{
			Vector3d v = Vector3d.Zero;
			Vector3d v2 = Vector3d.Zero;
			Vector3d v3 = Vector3d.Zero;
			mesh1.GetTriVertices(item, ref v, ref v2, ref v3);
			foreach (int item2 in mesh2.TriangleIndices())
			{
				Vector3d v4 = Vector3d.Zero;
				Vector3d v5 = Vector3d.Zero;
				Vector3d v6 = Vector3d.Zero;
				mesh2.GetTriVertices(item2, ref v4, ref v5, ref v6);
				if (new IntrTriangle3Triangle3(new Triangle3d(v, v2, v3), new Triangle3d(v4, v5, v6)).Test())
				{
					return new Index2i(item, item2);
				}
			}
		}
		return Index2i.Max;
	}

	public static Index2i FindNearestTriangles_LinearSearch(DMesh3 mesh1, DMesh3 mesh2, out double fNearestSqr)
	{
		Index2i result = Index2i.Max;
		fNearestSqr = double.MaxValue;
		foreach (int item in mesh1.TriangleIndices())
		{
			Vector3d v = Vector3d.Zero;
			Vector3d v2 = Vector3d.Zero;
			Vector3d v3 = Vector3d.Zero;
			mesh1.GetTriVertices(item, ref v, ref v2, ref v3);
			foreach (int item2 in mesh2.TriangleIndices())
			{
				Vector3d v4 = Vector3d.Zero;
				Vector3d v5 = Vector3d.Zero;
				Vector3d v6 = Vector3d.Zero;
				mesh2.GetTriVertices(item2, ref v4, ref v5, ref v6);
				DistTriangle3Triangle3 distTriangle3Triangle = new DistTriangle3Triangle3(new Triangle3d(v, v2, v3), new Triangle3d(v4, v5, v6));
				if (distTriangle3Triangle.GetSquared() < fNearestSqr)
				{
					fNearestSqr = distTriangle3Triangle.GetSquared();
					result = new Index2i(item, item2);
				}
			}
		}
		fNearestSqr = Math.Sqrt(fNearestSqr);
		return result;
	}

	public static void EdgeLengthStats(DMesh3 mesh, out double minEdgeLen, out double maxEdgeLen, out double avgEdgeLen, int samples = 0)
	{
		minEdgeLen = double.MaxValue;
		maxEdgeLen = double.MinValue;
		avgEdgeLen = 0.0;
		int num = 0;
		int maxEdgeID = mesh.MaxEdgeID;
		int num2 = ((samples == 0) ? 1 : (num2 = 31337));
		int num3 = ((samples == 0) ? maxEdgeID : samples);
		Vector3d a = Vector3d.Zero;
		Vector3d b = Vector3d.Zero;
		int num4 = 0;
		int num5 = 0;
		do
		{
			if (mesh.IsEdge(num4))
			{
				mesh.GetEdgeV(num4, ref a, ref b);
				double num6 = a.Distance(b);
				if (num6 < minEdgeLen)
				{
					minEdgeLen = num6;
				}
				if (num6 > maxEdgeLen)
				{
					maxEdgeLen = num6;
				}
				avgEdgeLen += num6;
				num++;
			}
			num4 = (num4 + num2) % maxEdgeID;
		}
		while (num4 != 0 && num5++ < num3);
		avgEdgeLen /= num;
	}

	public static void EdgeLengthStatsFromEdges(DMesh3 mesh, IEnumerable<int> EdgeItr, out double minEdgeLen, out double maxEdgeLen, out double avgEdgeLen, int samples = 0)
	{
		minEdgeLen = double.MaxValue;
		maxEdgeLen = double.MinValue;
		avgEdgeLen = 0.0;
		int num = 0;
		_ = mesh.MaxEdgeID;
		Vector3d a = Vector3d.Zero;
		Vector3d b = Vector3d.Zero;
		foreach (int item in EdgeItr)
		{
			if (mesh.IsEdge(item))
			{
				mesh.GetEdgeV(item, ref a, ref b);
				double num2 = a.Distance(b);
				if (num2 < minEdgeLen)
				{
					minEdgeLen = num2;
				}
				if (num2 > maxEdgeLen)
				{
					maxEdgeLen = num2;
				}
				avgEdgeLen += num2;
				num++;
			}
		}
		avgEdgeLen /= num;
	}
}
