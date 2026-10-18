using UnityEngine;

namespace GT_CustomMapSupportRuntime;

[RequireComponent(typeof(Collider))]
public class MovingSurfaceSettings : MonoBehaviour
{
	[HideInInspector]
	[Tooltip("Assign an ID that is unique for each moving surface in your map. should NOT be -1")]
	public int uniqueId = -1;
}
