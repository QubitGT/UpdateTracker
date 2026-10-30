using UnityEngine;

[AddComponentMenu("GorillaTag/Super Infection/SI Blaster Explosion")]
public class SIBlasterExplosion : MonoBehaviour
{
	private void OnDisable()
	{
		SIGadgetBlasterProjectile.DespawnExplosion(base.gameObject);
	}
}
