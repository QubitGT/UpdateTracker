using System;
using Meta.XR.Util;
using UnityEngine;

namespace Meta.XR.MRUtilityKit.SceneDecorator;

[Serializable]
[Feature(Feature.Scene)]
public class StaggeredConcentricDistribution : SceneDecorator.IDistribution
{
	[SerializeField]
	public float stepSize;

	private const float regionRadius = 0.70710677f;

	public void Distribute(SceneDecorator sceneDecorator, MRUKAnchor sceneAnchor, SceneDecoration sceneDecoration)
	{
		Vector2 vector = new Vector2(-1f, -1f);
		Vector2 vector2 = new Vector2(-1f, 1f);
		Vector2 vector3 = vector2 - vector;
		float magnitude = vector3.magnitude;
		float num = Mathf.Sqrt(magnitude);
		float sqrMagnitude = vector.sqrMagnitude;
		float sqrMagnitude2 = vector2.sqrMagnitude;
		int num2 = (int)Mathf.Floor((sqrMagnitude + 0.70710677f) / stepSize);
		int num3 = (int)Mathf.Floor((sqrMagnitude2 + 0.70710677f) / stepSize);
		sqrMagnitude2 -= 0.70710677f;
		for (int i = (int)Mathf.Ceil((sqrMagnitude - 0.70710677f) / stepSize); i < num2; i++)
		{
			float num4 = stepSize * (float)i;
			for (int j = (int)Mathf.Ceil(Mathf.Max(sqrMagnitude2, num - num4) / stepSize); j < num3; j++)
			{
				float num5 = stepSize * (float)j;
				float num6 = num4 + ((j % 3 != 0) ? 0f : (0.15f * stepSize));
				float num7 = num5 + ((i % 3 != 0) ? 0f : (0.15f * stepSize));
				if (!(num > num6 + num7))
				{
					float num8 = num6 * num6;
					float num9 = (num8 - num7 * num7 + magnitude) / (2f * num);
					Vector2 vector4 = vector + num9 * vector3 / num;
					Vector2 vector5 = vector3 * Mathf.Sqrt(num8 - num9 * num9) / num;
					Vector2 vector6 = new Vector2(vector4.x + vector5.y, vector4.y - vector5.x);
					vector4 = new Vector2(vector4.x - vector5.y, vector4.y + vector5.x);
					sceneDecorator.GenerateOn(vector4, vector4, sceneAnchor, sceneDecoration);
					if (vector6 != vector4)
					{
						sceneDecorator.GenerateOn(vector6, vector6, sceneAnchor, sceneDecoration);
					}
				}
			}
		}
	}
}
