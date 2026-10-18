using UnityEngine;

namespace GT_CustomMapSupportRuntime;

[RequireComponent(typeof(Collider))]
public class LuauTriggerSettings : TriggerSettings
{
	[Tooltip("Should this Trigger sync to all players, or only be processed for the person who triggered it?\nLuau Triggers generally shouldn't need to do this, but doing so will sync it's internal TriggerCount to all players.")]
	public bool syncedToAllPlayers;

	public override void PropagateProperties()
	{
		syncedToAllPlayers_private = syncedToAllPlayers;
	}
}
