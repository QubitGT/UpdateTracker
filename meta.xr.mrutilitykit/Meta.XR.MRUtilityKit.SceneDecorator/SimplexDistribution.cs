using System;
using Meta.XR.Util;
using UnityEngine;

namespace Meta.XR.MRUtilityKit.SceneDecorator;

[Serializable]
[Feature(Feature.Scene)]
public class SimplexDistribution : SceneDecorator.IDistribution
{
	[Serializable]
	public struct PointSamplingConfig
	{
		public float pointsPerUnitX;

		public float pointsPerUnitY;

		public float noiseOffsetRadius;

		public static PointSamplingConfig DefaultConfig = new PointSamplingConfig
		{
			pointsPerUnitX = 1f,
			pointsPerUnitY = 1f,
			noiseOffsetRadius = 0.1f
		};
	}

	[SerializeField]
	public PointSamplingConfig pointSamplingConfig;

	public static (Vector2[], Vector2[]) GeneratePointsLocal(MRUKAnchor sceneAnchor, PointSamplingConfig config)
	{
		if (!sceneAnchor.PlaneRect.HasValue)
		{
			return (Array.Empty<Vector2>(), Array.Empty<Vector2>());
		}
		Vector2 size = sceneAnchor.PlaneRect.Value.size;
		int num = Mathf.Max(Mathf.CeilToInt(config.pointsPerUnitX * size.x), 1);
		int num2 = Mathf.Max(Mathf.CeilToInt(config.pointsPerUnitY * size.y), 1);
		Vector2 vector = new Vector2(1f / (float)(num + 1), 1f / (float)(num2 + 1));
		Vector2[] array = new Vector2[num * num2];
		Vector2[] array2 = new Vector2[num * num2];
		for (int i = 0; i < num2; i++)
		{
			for (int j = 0; j < num; j++)
			{
				float num3 = (float)(j + 1) * vector.x;
				float num4 = (float)(i + 1) * vector.y;
				Vector3 vector2 = SimplexNoise.srdnoise(new Vector2(num3, num4), 0f);
				num3 += vector2.x * config.noiseOffsetRadius;
				num4 += vector2.y * config.noiseOffsetRadius;
				Vector2 vector3 = new Vector2(num3 * size.x - size.x / 2f, num4 * size.y - size.y / 2f);
				Vector2 vector4 = new Vector2(num3, num4);
				array[j + i * num] = vector3;
				array2[j + i * num] = vector4;
			}
		}
		return (array, array2);
	}

	public void Distribute(SceneDecorator sceneDecorator, MRUKAnchor sceneAnchor, SceneDecoration sceneDecoration)
	{
		(Vector2[], Vector2[]) tuple = GeneratePointsLocal(sceneAnchor, pointSamplingConfig);
		for (int i = 0; i < tuple.Item1.Length - 1; i++)
		{
			Vector2 localPos = tuple.Item1[i];
			Vector2 localPosNormalized = tuple.Item2[i];
			sceneDecorator.GenerateOn(localPos, localPosNormalized, sceneAnchor, sceneDecoration);
		}
	}
}
