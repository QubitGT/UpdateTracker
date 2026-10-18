using UnityEngine;

namespace GT_CustomMapSupportRuntime;

public class SizeChangerSettings : MonoBehaviour
{
	public enum ChangerType
	{
		Static,
		Continuous,
		Radius
	}

	[Tooltip(" Type: How the zone computes size: - Static: size set by Min Scale, transition speed set by Static Easing. Max Scale is ignored. - Continuous: Max Scale at Start Pos blending to Min Scale at End Pos. - Radius: Min Scale within Start Radius of Start Pos, growing to Max Scale at or beyond End Radius.")]
	public ChangerType type;

	[Tooltip("Static mode only. Transition speed. Bigger number = slower transition. 0 = instant.")]
	public float staticEasing;

	[Tooltip("Largest size in the range.1 = normal gorilla size.Used by Continuous and Radius.Default 0 is not a valid size; set this before testing.")]
	public float maxScale;

	[Tooltip("Smallest size in the range. 1 = normal gorilla size, 0.01 = smallest possible.Static mode scales players to this value.Default 0 is not a valid size; set this before testing.")]
	public float minScale;

	[Tooltip("Empty GameObject reference. - Continuous: start of the size gradient (Max Scale here). - Radius: the center point. Not used by Static.")]
	public Transform? startPos;

	[Tooltip("Empty GameObject reference.  - Continuous only: end of the size gradient (Min Scale here).")]
	public Transform? endPos;

	[Tooltip("[Optional] Scale changes reposition the player relative to this point, so scaling pivots around it instead of happening in place.Prevents clipping into geometry when growing in tight spaces.")]
	public Transform? scaleAwayFromPoint;

	[Tooltip("Normally size only applies while the player's head is inside the collider, which thin volumes like doorways can miss.On = the zone stays active as long as the player's body overlaps it.")]
	public bool alwaysControlWhenEntered;

	[Tooltip("Currently does nothing; leave at 0. Overlapping zones resolve as most recently entered wins. ")]
	public int priority;

	[Tooltip("Radius mode only. Distance from Start Pos where Min Scale applies. Inner edge of the transition.")]
	public float startRadius;

	[Tooltip("Radius mode only. Distance from Start Pos where Max Scale applies. Outer edge of the transition.")]
	public float endRadius;
}
