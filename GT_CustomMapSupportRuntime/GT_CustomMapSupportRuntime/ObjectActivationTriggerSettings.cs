using System.Collections.Generic;
using UnityEngine;

namespace GT_CustomMapSupportRuntime;

[RequireComponent(typeof(Collider))]
public class ObjectActivationTriggerSettings : TriggerSettings
{
	[Tooltip("Should this Trigger sync to all players, or only be processed for the person who triggered it?\nObjectActivationTriggers generally need to do this to ensure activated/deactivated objects are in the same state for all players. Disable with caution.")]
	public bool syncedToAllPlayers = true;

	[Tooltip("Any objects that should be activated when this is triggered")]
	public List<GameObject> objectsToActivate = new List<GameObject>();

	[Tooltip("Any objects that should be deactivated when this is triggered")]
	public List<GameObject> objectsToDeactivate = new List<GameObject>();

	[Tooltip("Any other triggers that should be reset when this is triggered. Resetting a Trigger will reset it's internal triggerCount to 0.")]
	public List<GameObject> triggersToReset = new List<GameObject>();

	[Tooltip("If TRUE, only the TriggerCount for the Triggers in \"Triggers to Reset\" will be reset. LastTriggerTime will be unchanged.")]
	public bool onlyResetTriggerCount;

	public override void PropagateProperties()
	{
		syncedToAllPlayers_private = syncedToAllPlayers;
	}
}
