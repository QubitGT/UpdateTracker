using System;
using Technie.PhysicsCreator.Rigid;
using UnityEngine;

namespace Technie.PhysicsCreator;

public class RotatedCapsuleFitter
{
	public CapsuleDef Fit(Hull hull, Vector3[] meshVertices, int[] meshIndices)
	{
		hull.FindConvexHull(meshVertices, meshIndices, out var hullVertices, out var hullIndices, showErrorInLog: false);
		if (hullVertices == null || hullVertices.Length == 0)
		{
			return default(CapsuleDef);
		}
		ConstructionPlane constructionPlane = FindBestCapsulePlane(hullVertices, hullIndices);
		Refine(FitCapsule(constructionPlane, hullVertices), constructionPlane, hullVertices, out var bestCapsule, out var bestPlane);
		return ToDef(bestCapsule, bestPlane);
	}

	public CapsuleDef Fit(Vector3[] hullVertices, int[] hullIndices)
	{
		if (hullVertices == null || hullVertices.Length == 0 || hullIndices == null || hullIndices.Length == 0)
		{
			return default(CapsuleDef);
		}
		ConstructionPlane constructionPlane = FindBestCapsulePlane(hullVertices, hullIndices);
		Refine(FitCapsule(constructionPlane, hullVertices), constructionPlane, hullVertices, out var bestCapsule, out var bestPlane);
		return ToDef(bestCapsule, bestPlane);
	}

	public ConstructionPlane FindBestCapsulePlane(Vector3[] hullVertices, int[] hullIndices)
	{
		BoxDef boxDef = new RotatedBoxFitter().Fit(hullVertices, hullIndices);
		Vector3 c = boxDef.boxPosition + boxDef.boxRotation * boxDef.collisionBox.center;
		if (boxDef.collisionBox.size.x > boxDef.collisionBox.size.y && boxDef.collisionBox.size.x > boxDef.collisionBox.size.z)
		{
			return new ConstructionPlane(c, boxDef.boxRotation * Vector3.right, boxDef.boxRotation * Vector3.forward);
		}
		if (boxDef.collisionBox.size.y > boxDef.collisionBox.size.z)
		{
			return new ConstructionPlane(c, boxDef.boxRotation * Vector3.up, boxDef.boxRotation * Vector3.right);
		}
		return new ConstructionPlane(c, boxDef.boxRotation * Vector3.forward, boxDef.boxRotation * Vector3.right);
	}

	public static CapsuleDef ToDef(RotatedCapsule capsule, ConstructionPlane plane)
	{
		CapsuleDef result = default(CapsuleDef);
		result.capsuleCenter = Vector3.zero;
		result.capsuleDirection = CapsuleAxis.Z;
		result.capsuleRadius = capsule.radius;
		result.capsuleHeight = capsule.height;
		result.capsulePosition = plane.center;
		result.capsuleRotation = plane.rotation;
		return result;
	}

	public static void Refine(RotatedCapsule inputCapule, ConstructionPlane inputPlane, Vector3[] hullVertices, out RotatedCapsule bestCapsule, out ConstructionPlane bestPlane)
	{
		bestPlane = inputPlane;
		bestCapsule = inputCapule;
		System.Random random = new System.Random(1234);
		int num = 1024;
		for (int i = 0; i < num; i++)
		{
			float magnitude = Mathf.Min(bestCapsule.height, bestCapsule.radius) * 0.01f;
			ConstructionPlane constructionPlane = new ConstructionPlane(bestPlane, new Vector3(Jitter(magnitude, random), Jitter(magnitude, random), Jitter(magnitude, random)));
			RotatedCapsule rotatedCapsule = FitCapsule(constructionPlane, hullVertices);
			if (rotatedCapsule.CalcVolume() < bestCapsule.CalcVolume())
			{
				bestCapsule = rotatedCapsule;
				bestPlane = constructionPlane;
			}
		}
	}

	private static float Jitter(float magnitude, System.Random random)
	{
		return (float)(random.NextDouble() * (double)(magnitude * 2f) - (double)magnitude);
	}

	public static RotatedCapsule FitCapsule(ConstructionPlane plane, Vector3[] points)
	{
		RotatedCapsule result = default(RotatedCapsule);
		result.center = plane.center;
		result.dir = plane.normal;
		foreach (Vector3 vector in points)
		{
			Vector3 vector2 = ProjectOntoAxis(plane, vector);
			float b = Vector3.Distance(vector2, vector);
			float num = Vector3.Distance(plane.center, vector2);
			result.radius = Mathf.Max(result.radius, b);
			result.height = Mathf.Max(result.height, num * 2f);
		}
		return result;
	}

	private static Vector3 ProjectOntoAxis(ConstructionPlane plane, Vector3 point)
	{
		Vector3 rhs = point - plane.center;
		float num = Vector3.Dot(plane.normal, rhs);
		return plane.center + plane.normal * num;
	}

	public static Vector3 FindCenter(Vector3[] vertices)
	{
		if (vertices == null || vertices.Length == 0)
		{
			return Vector3.zero;
		}
		Vector3 zero = Vector3.zero;
		for (int i = 0; i < vertices.Length; i++)
		{
			zero += vertices[i];
		}
		return zero / vertices.Length;
	}
}
