using System;
using Meta.XR.Util;
using UnityEngine;

namespace Meta.XR.MRUtilityKit.SceneDecorator;

[Serializable]
[Feature(Feature.Scene)]
public class RandomDistribution : SceneDecorator.IDistribution
{
	[SerializeField]
	[Tooltip("How many entries to generate per unit (1m)")]
	private float numPerUnit = 10f;

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
		for (int num = Mathf.Max((int)Mathf.Ceil(vector.x * vector.y * numPerUnit), 1); num > 0; num--)
		{
			float value = UnityEngine.Random.value;
			float value2 = UnityEngine.Random.value;
			sceneDecorator.GenerateOn(new Vector2(value * vector.x - vector.x / 2f, value2 * vector.y - vector.y / 2f), new Vector2(value, value2), sceneAnchor, sceneDecoration);
		}
	}
}
