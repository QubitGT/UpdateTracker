using Technie.PhysicsCreator.Rigid;
using UnityEngine;

namespace Technie.PhysicsCreator;

public class AlignedCapsuleFitter
{
	public CapsuleDef Fit(Hull hull, Vector3[] meshVertices, int[] meshIndices)
	{
		hull.FindConvexHull(meshVertices, meshIndices, out var hullVertices, out var hullIndices, showErrorInLog: false);
		if (hullVertices == null || hullVertices.Length == 0)
		{
			return default(CapsuleDef);
		}
		return Fit(hullVertices, hullIndices);
	}

	public CapsuleDef Fit(Vector3[] hullVertices, int[] hullIndices)
	{
		if (hullVertices == null || hullVertices.Length == 0 || hullIndices == null || hullIndices.Length == 0)
		{
			return default(CapsuleDef);
		}
		RotatedBox rotatedBox = RotatedBoxFitter.FindTightestBox(new ConstructionPlane(Vector3.zero), hullVertices);
		ConstructionPlane constructionPlane;
		CapsuleAxis capsuleDirection;
		if (rotatedBox.size.x > rotatedBox.size.y && rotatedBox.size.x > rotatedBox.size.z)
		{
			constructionPlane = new ConstructionPlane(rotatedBox.center, Vector3.right, Vector3.forward);
			capsuleDirection = CapsuleAxis.X;
		}
		else if (rotatedBox.size.y > rotatedBox.size.z)
		{
			constructionPlane = new ConstructionPlane(rotatedBox.center, Vector3.up, Vector3.right);
			capsuleDirection = CapsuleAxis.Y;
		}
		else
		{
			constructionPlane = new ConstructionPlane(rotatedBox.center, Vector3.forward, Vector3.right);
			capsuleDirection = CapsuleAxis.Z;
		}
		RotatedCapsuleFitter.Refine(RotatedCapsuleFitter.FitCapsule(constructionPlane, hullVertices), constructionPlane, hullVertices, out var bestCapsule, out var _);
		CapsuleDef result = default(CapsuleDef);
		result.capsuleDirection = capsuleDirection;
		result.capsuleRadius = bestCapsule.radius;
		result.capsuleHeight = bestCapsule.height;
		result.capsuleCenter = bestCapsule.center;
		result.capsulePosition = Vector3.zero;
		result.capsuleRotation = Quaternion.identity;
		return result;
	}
}
