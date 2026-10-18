using System.Collections.Generic;
using UnityEngine;

namespace Technie.PhysicsCreator;

public class TriangleBucket
{
	private List<Triangle> triangles;

	private Vector3 averagedNormal;

	private Vector3 averagedCenter;

	private float totalArea;

	public float Area => totalArea;

	public TriangleBucket(Triangle initialTriangle)
	{
		triangles = new List<Triangle>();
		triangles.Add(initialTriangle);
		CalculateNormal();
		CalcTotalArea();
	}

	public void Add(Triangle t)
	{
		triangles.Add(t);
		CalculateNormal();
		CalcTotalArea();
	}

	public void Add(TriangleBucket otherBucket)
	{
		foreach (Triangle triangle in otherBucket.triangles)
		{
			triangles.Add(triangle);
		}
		CalculateNormal();
		CalcTotalArea();
	}

	private void CalculateNormal()
	{
		averagedNormal = Vector3.zero;
		foreach (Triangle triangle in triangles)
		{
			averagedNormal += triangle.normal * triangle.area;
		}
		averagedNormal.Normalize();
	}

	public Vector3 GetAverageNormal()
	{
		return averagedNormal;
	}

	public Vector3 GetAverageCenter()
	{
		return triangles[0].center;
	}

	private void CalcTotalArea()
	{
		totalArea = 0f;
		foreach (Triangle triangle in triangles)
		{
			totalArea += triangle.area;
		}
	}
}
