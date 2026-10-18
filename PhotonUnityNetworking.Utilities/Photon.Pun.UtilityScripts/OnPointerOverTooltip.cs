using UnityEngine;
using UnityEngine.EventSystems;

namespace Photon.Pun.UtilityScripts;

public class OnPointerOverTooltip : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	private void OnDestroy()
	{
		PointedAtGameObjectInfo.Instance.RemoveFocus(GetComponent<PhotonView>());
	}

	void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
	{
		PointedAtGameObjectInfo.Instance.RemoveFocus(GetComponent<PhotonView>());
	}

	void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
	{
		PointedAtGameObjectInfo.Instance.SetFocus(GetComponent<PhotonView>());
	}
}
