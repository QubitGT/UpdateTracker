using UnityEngine;
using UnityEngine.Events;

[AddComponentMenu("GorillaTag/Super Infection/SI Hand Scanner")]
public class SIHandScanner : MonoBehaviour
{
	public UnityEvent<int> onHandScanned;

	public void HandScanned(SIPlayer scannedPlayer)
	{
		if (scannedPlayer.gamePlayer.IsLocal())
		{
			onHandScanned.Invoke(NetworkSystem.Instance.LocalPlayerID);
		}
	}
}
