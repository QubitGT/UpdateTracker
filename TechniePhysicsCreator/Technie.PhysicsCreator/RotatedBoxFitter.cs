using System.Collections.Generic;
using Technie.PhysicsCreator.Rigid;
using UnityEngine;

namespace Technie.PhysicsCreator;

public class RotatedBoxFitter
{
	public BoxDef Fit(Hull hull, Vector3[] meshVertices, int[] meshIndices)
	{
		hull.FindConvexHull(meshVertices, meshIndices, out var hullVertices, out var hullIndices, showErrorInLog: false);
		if (hullVertices == null || hullVertices.Length == 0)
		{
			hull.FindTriangles(meshVertices, meshIndices, out hullVertices, out hullIndices);
		}
		return Fit(hullVertices, hullIndices);
	}

	public BoxDef Fit(Vector3[] hullVertices, int[] hullIndices)
	{
		List<ConstructionPlane> list = new List<ConstructionPlane>();
		int num = 64;
		int numVariants = 128;
		float angleRange = 360f / (float)num;
		list.Add(new ConstructionPlane(Vector3.zero));
		for (int i = 0; i < hullIndices.Length; i += 3)
		{
			int num2 = hullIndices[i];
			int num3 = hullIndices[i + 1];
			int num4 = hullIndices[i + 2];
			Vector3 vector = hullVertices[num2];
			Vector3 vector2 = hullVertices[num3];
			Vector3 vector3 = hullVertices[num4];
			Vector3 c = (vector + vector2 + vector3) / 3f;
			Vector3 vector4 = Vector3.Cross((vector2 - vector).normalized, (vector3 - vector).normalized);
			Vector3 rhs = ((Mathf.Abs(Vector3.Dot(vector4, Vector3.up)) > 0.9f) ? Vector3.right : Vector3.up);
			Vector3 t = Vector3.Cross(vector4, rhs);
			if (vector4.magnitude > 0.0001f)
			{
				ConstructionPlane basePlane = new ConstructionPlane(c, vector4, t);
				for (int j = 0; j < num; j++)
				{
					float angle = (float)j / (float)(num - 1) * 360f;
					ConstructionPlane item = new ConstructionPlane(basePlane, angle);
					list.Add(item);
				}
			}
		}
		List<RotatedBox> list2 = FindTightestBoxes(list, hullVertices);
		if (list2.Count > 0)
		{
			list2.Sort(new VolumeSorter());
			_ = list2[0];
			List<ConstructionPlane> list3 = new List<ConstructionPlane>();
			GeneratePlaneVariants(list2[0].plane, numVariants, angleRange, list3);
			List<RotatedBox> list4 = FindTightestBoxes(list3, hullVertices);
			list4.Sort(new VolumeSorter());
			RotatedBox rotatedBox = list4[0];
			UnifyOffsets(rotatedBox);
			return ToBoxDef(rotatedBox);
		}
		Debug.LogError("Couldn't fit box rotation to hull");
		return default(BoxDef);
	}

	private static void GeneratePlaneVariants(ConstructionPlane basePlane, int numVariants, float angleRange, List<ConstructionPlane> variantPlanes)
	{
		variantPlanes.Add(basePlane);
		for (int i = 0; i < numVariants; i++)
		{
			float t = (float)i / (float)(numVariants - 1);
			float angle = Mathf.Lerp(0f - angleRange, angleRange, t);
			variantPlanes.Add(new ConstructionPlane(basePlane, angle));
		}
	}

	private static void UnifyOffsets(RotatedBox inputBox)
	{
		Vector3 vector = inputBox.plane.rotation * inputBox.localCenter;
		inputBox.plane.center += vector;
		inputBox.localCenter = Vector3.zero;
	}

	public static BoxDef ToBoxDef(RotatedBox computedBox)
	{
		BoxDef result = default(BoxDef);
		result.boxPosition = computedBox.plane.center;
		result.boxRotation = computedBox.plane.rotation;
		result.collisionBox.center = computedBox.localCenter;
		result.collisionBox.size = computedBox.size;
		return result;
	}

	public static void ApplyToHull(RotatedBox computedBox, Hull targetHull)
	{
		targetHull.collisionBox = ToBoxDef(computedBox);
	}

	private static List<RotatedBox> FindTightestBoxes(List<ConstructionPlane> planes, Vector3[] inputVertices)
	{
		if (inputVertices == null || inputVertices.Length == 0)
		{
			return new List<RotatedBox>();
		}
		List<RotatedBox> list = new List<RotatedBox>();
		foreach (ConstructionPlane plane in planes)
		{
			RotatedBox item = FindTightestBox(plane, inputVertices);
			list.Add(item);
		}
		return list;
	}

	public static RotatedBox FindTightestBox(ConstructionPlane plane, Vector3[] inputVertices)
	{
		if (inputVertices == null || inputVertices.Length == 0)
		{
			return null;
		}
		Vector3 vector;
		Vector3 vector2 = (vector = plane.worldToPlane.MultiplyPoint(inputVertices[0]));
		foreach (Vector3 point in inputVertices)
		{
			Vector3 lhs = plane.worldToPlane.MultiplyPoint(point);
			vector2 = Vector3.Min(lhs, vector2);
			vector = Vector3.Max(lhs, vector);
		}
		Vector3 vector3 = Vector3.Lerp(vector2, vector, 0.5f);
		Vector3 c = plane.planeToWorld.MultiplyPoint(vector3);
		Vector3 s = vector - vector2;
		return new RotatedBox(plane, vector3, c, s);
	}
}
