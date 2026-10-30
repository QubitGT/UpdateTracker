using UnityEngine;

[AddComponentMenu("GorillaTag/Super Infection/SI Scannable Hand")]
public class SIScannableHand : MonoBehaviour
{
	public SIPlayer parentPlayer;

	private void Awake()
	{
		parentPlayer = GetComponentInParent<SIPlayer>();
	}
}
