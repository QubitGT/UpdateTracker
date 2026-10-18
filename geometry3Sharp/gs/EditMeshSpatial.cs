using System;
using System.Collections.Generic;
using g3;

namespace gs;

public class EditMeshSpatial : ISpatial
{
	public DMesh3 SourceMesh;

	public DMeshAABBTree3 SourceSpatial;

	public DMesh3 EditMesh;

	private HashSet<int> RemovedT = new HashSet<int>();

	private HashSet<int> AddedT = new HashSet<int>();

	public bool SupportsNearestTriangle => false;

	public bool SupportsPointContainment => false;

	public bool SupportsTriangleRayIntersection => true;

	public void RemoveTriangle(int tid)
	{
		if (AddedT.Contains(tid))
		{
			AddedT.Remove(tid);
		}
		else
		{
			RemovedT.Add(tid);
		}
	}

	public void AddTriangle(int tid)
	{
		AddedT.Add(tid);
	}

	public int FindNearestTriangle(Vector3d p, double fMaxDist = double.MaxValue)
	{
		return -1;
	}

	public bool IsInside(Vector3d p)
	{
		return false;
	}

	public int FindNearestHitTriangle(Ray3d ray, double fMaxDist = double.MaxValue)
	{
		Func<int, bool> triangleFilterF = SourceSpatial.TriangleFilterF;
		SourceSpatial.TriangleFilterF = source_filter;
		int num = SourceSpatial.FindNearestHitTriangle(ray);
		SourceSpatial.TriangleFilterF = triangleFilterF;
		int hit_tid;
		IntrRay3Triangle3 intrRay3Triangle = find_added_hit(ref ray, out hit_tid);
		if (num == -1 && hit_tid == -1)
		{
			return -1;
		}
		if (num == -1)
		{
			return hit_tid;
		}
		if (hit_tid == -1)
		{
			return num;
		}
		IntrRay3Triangle3 intrRay3Triangle2 = ((num != -1) ? MeshQueries.TriangleIntersection(SourceMesh, num, ray) : null);
		if (!(intrRay3Triangle.RayParameter < intrRay3Triangle2.RayParameter))
		{
			return num;
		}
		return hit_tid;
	}

	private bool source_filter(int tid)
	{
		return !RemovedT.Contains(tid);
	}

	private IntrRay3Triangle3 find_added_hit(ref Ray3d ray, out int hit_tid)
	{
		hit_tid = -1;
		IntrRay3Triangle3 result = null;
		double num = double.MaxValue;
		Triangle3d t = default(Triangle3d);
		foreach (int item in AddedT)
		{
			Index3i triangle = EditMesh.GetTriangle(item);
			t.V0 = EditMesh.GetVertex(triangle.a);
			t.V1 = EditMesh.GetVertex(triangle.b);
			t.V2 = EditMesh.GetVertex(triangle.c);
			IntrRay3Triangle3 intrRay3Triangle = new IntrRay3Triangle3(ray, t);
			if (intrRay3Triangle.Find() && intrRay3Triangle.RayParameter < num)
			{
				num = intrRay3Triangle.RayParameter;
				hit_tid = item;
				result = intrRay3Triangle;
			}
		}
		return result;
	}
}
