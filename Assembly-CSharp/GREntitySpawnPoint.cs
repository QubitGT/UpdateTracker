using UnityEngine;

[AddComponentMenu("GorillaTag/Ghost Reactor/GR Entity Spawn Point")]
public class GREntitySpawnPoint : MonoBehaviour
{
	public GhostReactorSpawnConfig.SpawnPointType spawnPointType;

	public GameEntity entity;

	public GRPatrolPath patrolPath;

	public bool applyScale;
}
