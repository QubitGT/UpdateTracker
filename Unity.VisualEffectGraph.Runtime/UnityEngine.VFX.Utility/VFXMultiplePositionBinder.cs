using System.Collections.Generic;
using UnityEngine.Serialization;

namespace UnityEngine.VFX.Utility;

[AddComponentMenu("VFX/Property Binders/Multiple Position Binder")]
[VFXBinder("Point Cache/Multiple Position Binder")]
internal class VFXMultiplePositionBinder : VFXBinderBase
{
	[VFXPropertyBinding(new string[] { "UnityEngine.Texture2D" })]
	[FormerlySerializedAs("PositionMapParameter")]
	public ExposedProperty PositionMapProperty = "PositionMap";

	[VFXPropertyBinding(new string[] { "System.Int32" })]
	[FormerlySerializedAs("PositionCountParameter")]
	public ExposedProperty PositionCountProperty = "PositionCount";

	public GameObject[] Targets;

	public bool EveryFrame;

	private Texture2D positionMap;

	private int count;

	protected override void OnEnable()
	{
		base.OnEnable();
		UpdateTexture();
	}

	public override bool IsValid(VisualEffect component)
	{
		if (Targets != null && component.HasTexture(PositionMapProperty))
		{
			return component.HasInt(PositionCountProperty);
		}
		return false;
	}

	public override void UpdateBinding(VisualEffect component)
	{
		if (EveryFrame || Application.isEditor)
		{
			UpdateTexture();
		}
		component.SetTexture(PositionMapProperty, positionMap);
		component.SetInt(PositionCountProperty, count);
	}

	private void UpdateTexture()
	{
		if (Targets == null || Targets.Length == 0)
		{
			return;
		}
		List<Vector3> list = new List<Vector3>();
		GameObject[] targets = Targets;
		foreach (GameObject gameObject in targets)
		{
			if (gameObject != null)
			{
				list.Add(gameObject.transform.position);
			}
		}
		count = list.Count;
		if (positionMap == null || positionMap.width != count)
		{
			positionMap = new Texture2D(count, 1, TextureFormat.RGBAFloat, mipChain: false);
		}
		List<Color> list2 = new List<Color>();
		foreach (Vector3 item in list)
		{
			list2.Add(new Color(item.x, item.y, item.z));
		}
		positionMap.name = base.gameObject.name + "_PositionMap";
		positionMap.filterMode = FilterMode.Point;
		positionMap.wrapMode = TextureWrapMode.Repeat;
		positionMap.SetPixels(list2.ToArray(), 0);
		positionMap.Apply();
	}

	public override string ToString()
	{
		return $"Multiple Position Binder ({count} positions)";
	}
}
