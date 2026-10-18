using System;
using Meta.XR.Util;
using UnityEngine;

namespace Meta.XR.MRUtilityKit.SceneDecorator;

[Serializable]
[Feature(Feature.Scene)]
public class GridDistribution : SceneDecorator.IDistribution
{
	[SerializeField]
	private float spacingX = 1f;

	[SerializeField]
	private float spacingY = 1f;

	public void Distribute(SceneDecorator sceneDecorator, MRUKAnchor sceneAnchor, SceneDecoration sceneDecoration)
	{
		Vector3 vector = Vector3.one;
		if (sceneAnchor.PlaneRect.HasValue)
		{
			vector = (sceneAnchor.PlaneRect.HasValue ? new Vector3(sceneAnchor.PlaneRect.Value.width, sceneAnchor.PlaneRect.Value.height, 1f) : Vector3.one);
		}
		if (sceneAnchor.VolumeBounds.HasValue)
		{
			vector = sceneAnchor.VolumeBounds?.size ?? Vector3.one;
		}
		Vector2 vector2 = new Vector2(Mathf.Max(Mathf.Ceil(vector.x / spacingX), 1f), Mathf.Max(Mathf.Ceil(vector.y / spacingY), 1f));
		Vector2 vector3 = vector / vector2;
		for (int i = 0; (float)i < vector2.x; i++)
		{
			for (int j = 0; (float)j < vector2.y; j++)
			{
				Vector2 localPos = new Vector2((new Vector2(i, j) * vector3).x - vector.x / 2f, (new Vector2(i, j) * vector3).y - vector.y / 2f);
				Vector2 localPosNormalized = new Vector2((new Vector2(i, j) * vector3).x / vector.x, (new Vector2(i, j) * vector3).y / vector.y);
				sceneDecorator.GenerateOn(localPos, localPosNormalized, sceneAnchor, sceneDecoration);
			}
		}
	}
}
