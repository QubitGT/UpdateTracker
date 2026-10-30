using UnityEngine;

[AddComponentMenu("GorillaTag/Critters/Critter Despawner")]
public class CritterDespawner : MonoBehaviour
{
	public void DespawnAllCritters()
	{
		CrittersManager.instance.QueueDespawnAllCritters();
	}
}
