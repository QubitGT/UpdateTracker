using UnityEngine;

namespace Photon.Pun.UtilityScripts;

public class EventSystemSpawner : MonoBehaviour
{
	private void OnEnable()
	{
		Debug.LogError("PUN Demos are not compatible with the New Input System, unless you enable \"Both\" in: Edit > Project Settings > Player > Active Input Handling. Pausing App.");
		Debug.Break();
	}
}
