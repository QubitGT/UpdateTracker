using Technie.PhysicsCreator.Rigid;
using UnityEngine;

namespace Technie.PhysicsCreator;

public class AxisAlignedBoxFitter
{
	public void Fit(Hull hull, Vector3[] meshVertices, int[] meshIndices)
	{
		Vector3[] selectedVertices = hull.GetSelectedVertices(meshVertices, meshIndices);
		RotatedBoxFitter.ApplyToHull(RotatedBoxFitter.FindTightestBox(new ConstructionPlane(Vector3.zero, Vector3.up, Vector3.right), selectedVertices), hull);
	}
}
