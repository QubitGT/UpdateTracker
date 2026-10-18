using UnityEngine;
using UnityEngine.EventSystems;

namespace Photon.Pun.UtilityScripts;

public class OnClickInstantiate : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	public enum InstantiateOption
	{
		Mine,
		Scene
	}

	public PointerEventData.InputButton Button;

	public KeyCode ModifierKey;

	public GameObject Prefab;

	[SerializeField]
	private InstantiateOption InstantiateType;

	void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
	{
		if (PhotonNetwork.InRoom && (ModifierKey == KeyCode.None || Input.GetKey(ModifierKey)) && eventData.button == Button)
		{
			switch (InstantiateType)
			{
			case InstantiateOption.Mine:
				PhotonNetwork.Instantiate(Prefab.name, eventData.pointerCurrentRaycast.worldPosition + new Vector3(0f, 0.5f, 0f), Quaternion.identity, 0);
				break;
			case InstantiateOption.Scene:
				PhotonNetwork.InstantiateRoomObject(Prefab.name, eventData.pointerCurrentRaycast.worldPosition + new Vector3(0f, 0.5f, 0f), Quaternion.identity, 0);
				break;
			}
		}
	}
}
