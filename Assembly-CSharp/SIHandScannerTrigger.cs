using UnityEngine;
using UnityEngine.Events;

[AddComponentMenu("GorillaTag/Super Infection/SI Hand Scanner Trigger")]
public class SIHandScannerTrigger : MonoBehaviour, IClickable
{
	public SIHandScanner parentScanner;

	public UnityEvent onHandScanned;

	private void Awake()
	{
		if (parentScanner == null)
		{
			parentScanner = GetComponentInParent<SIHandScanner>();
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		SIScannableHand component = other.GetComponent<SIScannableHand>();
		if (!(component == null))
		{
			OnPlayerScanned(component.parentPlayer);
		}
	}

	private void OnPlayerScanned(SIPlayer player)
	{
		parentScanner.HandScanned(player);
		onHandScanned.Invoke();
	}

	public void Click(bool leftHand = false)
	{
		OnPlayerScanned(VRRig.LocalRig.GetComponent<SIPlayer>());
	}
}
