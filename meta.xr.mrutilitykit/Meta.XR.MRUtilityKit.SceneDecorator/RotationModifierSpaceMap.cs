using System.Collections.Generic;
using Meta.XR.Util;
using UnityEngine;

namespace Meta.XR.MRUtilityKit.SceneDecorator;

[Feature(Feature.Scene)]
public class RotationModifierSpaceMap : Modifier
{
	[SerializeField]
	public Color RotateToColor = Color.black;

	[SerializeField]
	public float Radius = 0.02f;

	public override void ApplyModifier(GameObject decorationGO, MRUKAnchor sceneAnchor, SceneDecoration sceneDecoration, Candidate candidate)
	{
		SpaceMapGPU spaceMapGPU = Object.FindAnyObjectByType<SpaceMapGPU>();
		if (spaceMapGPU == null)
		{
			return;
		}
		List<Color> list = new List<Color>();
		List<float> list2 = new List<float>();
		for (int i = 0; i < 12; i++)
		{
			float num = 2f * SceneDecorator.PI / 12f * (float)i;
			Vector2 vector = new Vector2(Mathf.Cos(num), Mathf.Sin(num)) * Radius;
			Vector3 worldPosition = candidate.hit.point + new Vector3(vector.x, 0f, vector.y);
			Color colorAtPosition = spaceMapGPU.GetColorAtPosition(worldPosition);
			list.Add(colorAtPosition);
			list2.Add(num);
		}
		float num2 = ColorDistance(RotateToColor, list[0]);
		List<float> list3 = new List<float> { list2[0] };
		for (int j = 0; j < list.Count; j++)
		{
			float num3 = ColorDistance(RotateToColor, list[j]);
			if (num3 <= num2)
			{
				num2 = num3;
				list3.Add(list2[j]);
			}
		}
		int index = Random.Range(0, list3.Count);
		decorationGO.transform.rotation = Quaternion.Euler(0f, -1f * list3[index] * 57.29578f, 0f);
	}

	private float ColorDistance(Color a, Color b)
	{
		return Mathf.Sqrt(Mathf.Pow(a.r - b.r, 2f) + Mathf.Pow(a.g - b.g, 2f) + Mathf.Pow(a.b - b.b, 2f));
	}
}
