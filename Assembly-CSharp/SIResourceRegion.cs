using UnityEngine;

[AddComponentMenu("GorillaTag/Super Infection/SI Resource Region")]
public class SIResourceRegion : SpawnRegion<GameEntity, SIResourceRegion>
{
	public SIResource resourcePrefab;

	public float LastSpawnTime { get; set; }
}
