using System.Collections.Generic;
using Technie.PhysicsCreator.Rigid;
using UnityEngine;

namespace Technie.PhysicsCreator;

public class SphereFitter
{
	public Sphere Fit(Hull hull, Vector3[] meshVertices, int[] meshIndices)
	{
		Sphere sphere = new Sphere();
		if (CalculateBoundingSphere(hull, meshVertices, meshIndices, out var sphereCenter, out var sphereRadius))
		{
			sphere.center = sphereCenter;
			sphere.radius = sphereRadius;
		}
		else
		{
			sphere.center = Vector3.zero;
			sphere.radius = 0f;
		}
		return sphere;
	}

	public Sphere Fit(Vector3[] hullVertices, int[] hullIndices)
	{
		if (hullVertices == null || hullVertices.Length == 0)
		{
			return new Sphere();
		}
		return SphereUtils.MinSphere(new List<Vector3>(hullVertices));
	}

	private bool CalculateBoundingSphere(Hull hull, Vector3[] meshVertices, int[] meshIndices, out Vector3 sphereCenter, out float sphereRadius)
	{
		int[] selectedFaces = hull.GetSelectedFaces();
		if (selectedFaces.Length == 0)
		{
			sphereCenter = Vector3.zero;
			sphereRadius = 0f;
			return false;
		}
		List<Vector3> list = new List<Vector3>();
		foreach (int num in selectedFaces)
		{
			Vector3 item = meshVertices[meshIndices[num * 3]];
			Vector3 item2 = meshVertices[meshIndices[num * 3 + 1]];
			Vector3 item3 = meshVertices[meshIndices[num * 3 + 2]];
			list.Add(item);
			list.Add(item2);
			list.Add(item3);
		}
		Sphere sphere = SphereUtils.MinSphere(list);
		sphereCenter = sphere.center;
		sphereRadius = sphere.radius;
		return true;
	}
}
