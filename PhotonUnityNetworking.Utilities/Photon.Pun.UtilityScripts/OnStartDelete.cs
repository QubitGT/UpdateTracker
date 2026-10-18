using UnityEngine;

namespace Photon.Pun.UtilityScripts;

public class OnStartDelete : MonoBehaviour
{
	private void Start()
	{
		Object.Destroy(base.gameObject);
	}
}
