using Meta.XR.Util;
using UnityEngine;

namespace Meta.XR.MRUtilityKit.SceneDecorator;

[Feature(Feature.Scene)]
public class NotInsideMask : Mask
{
	[SerializeField]
	public MRUKAnchor.SceneLabels Labels;

	public override float SampleMask(Candidate c)
	{
		return 0f;
	}

	public override bool Check(Candidate c)
	{
		Bounds? prefabBounds = Utilities.GetPrefabBounds(c.decorationPrefab);
		foreach (MRUKRoom room in MRUK.Instance.Rooms)
		{
			MRUKAnchor sceneObject;
			bool flag = room.IsPositionInSceneVolume(c.hit.point, out sceneObject, testVerticalBounds: true, prefabBounds.Value.extents.x);
			if (sceneObject != null && sceneObject.HasAnyLabel(Labels))
			{
				return !flag;
			}
		}
		return true;
	}
}
