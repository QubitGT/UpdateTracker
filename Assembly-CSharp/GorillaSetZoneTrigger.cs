using UnityEngine;

public class GorillaSetZoneTrigger : GorillaTriggerBox
{
	[SerializeField]
	private GTZone[] zones;

	public override void OnBoxTriggered()
	{
		GTDev.LogNoStackTrace("Triggered set zone box on gameobject " + base.gameObject.name);
		ZoneManagement.SetActiveZones(zones);
	}
}
