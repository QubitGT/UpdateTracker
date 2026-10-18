using System.Collections.Generic;
using UnityEngine;

namespace Meta.XR.MRUtilityKit;

public interface ICustomAnchorPrefabSpawner
{
	GameObject CustomPrefabSelection(MRUKAnchor anchor, List<GameObject> prefabs);

	Vector3 CustomPrefabScaling(Vector3 localScale);

	Vector2 CustomPrefabScaling(Vector2 localScale);

	Vector3 CustomPrefabAlignment(Bounds anchorVolumeBounds, Bounds? prefabBounds);

	Vector3 CustomPrefabAlignment(Rect anchorPlaneRect, Bounds? prefabBounds);
}
