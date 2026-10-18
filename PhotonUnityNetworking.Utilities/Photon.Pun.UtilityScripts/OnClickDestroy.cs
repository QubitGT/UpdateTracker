using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Photon.Pun.UtilityScripts;

public class OnClickDestroy : MonoBehaviourPun, IPointerClickHandler, IEventSystemHandler
{
	public PointerEventData.InputButton Button;

	public KeyCode ModifierKey;

	public bool DestroyByRpc;

	void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
	{
		if (PhotonNetwork.InRoom && (ModifierKey == KeyCode.None || Input.GetKey(ModifierKey)) && eventData.button == Button)
		{
			if (DestroyByRpc)
			{
				base.photonView.RPC("DestroyRpc", RpcTarget.AllBuffered);
			}
			else
			{
				PhotonNetwork.Destroy(base.gameObject);
			}
		}
	}

	[PunRPC]
	public IEnumerator DestroyRpc()
	{
		Object.Destroy(base.gameObject);
		yield return 0;
	}
}
