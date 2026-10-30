using UnityEngine;
using UnityEngine.Serialization;

[AddComponentMenu("GorillaTag/Critters/Release Cage When Upside Down")]
public class ReleaseCageWhenUpsideDown : MonoBehaviour
{
	public CrittersCage cage;

	[FormerlySerializedAs("dumpThreshold")]
	[FormerlySerializedAs("angle")]
	public float releaseCritterThreshold = 30f;

	private void Awake()
	{
		cage = GetComponentInChildren<CrittersCage>();
	}

	private void Update()
	{
		cage.inReleasingPosition = Vector3.Angle(base.transform.up, Vector3.down) < releaseCritterThreshold;
	}
}
