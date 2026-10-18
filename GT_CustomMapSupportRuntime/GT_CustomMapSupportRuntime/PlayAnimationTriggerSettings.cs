using System.Collections.Generic;
using UnityEngine;

namespace GT_CustomMapSupportRuntime;

[RequireComponent(typeof(Collider))]
public class PlayAnimationTriggerSettings : TriggerSettings
{
	[Tooltip("Should this Trigger sync to all players, or only be processed for the person who triggered it?\nPlayAnimationTriggers should generally have this enabled to ensure animated objects are in the same state for all players. Disable with caution.")]
	public bool syncedToAllPlayers = true;

	[Tooltip("Any objects that should play the specified animation when this is triggered")]
	public List<GameObject> animatedObjects = new List<GameObject>();

	[Tooltip("The name of the animation state to activate for any Animator components on the \"Animated Objects\".")]
	public string animationName = "";

	public override void PropagateProperties()
	{
		syncedToAllPlayers_private = syncedToAllPlayers;
	}
}
