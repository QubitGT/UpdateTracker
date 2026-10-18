using UnityEngine;
using UnityEngine.UI;

namespace Photon.Pun.UtilityScripts;

public class PointedAtGameObjectInfo : MonoBehaviour
{
	public static PointedAtGameObjectInfo Instance;

	public Text text;

	private Transform focus;

	private void Start()
	{
		if (Instance != null)
		{
			Debug.LogWarning("PointedAtGameObjectInfo is already featured in the scene, gameobject is destroyed");
			Object.Destroy(base.gameObject);
		}
		Instance = this;
	}

	public void SetFocus(PhotonView pv)
	{
		focus = ((pv != null) ? pv.transform : null);
		if (pv != null)
		{
			text.text = string.Format("id {0} own: {1} {2}{3}", pv.ViewID, pv.OwnerActorNr, pv.IsRoomView ? "scn" : "", pv.IsMine ? " mine" : "");
		}
		else
		{
			text.text = string.Empty;
		}
	}

	public void RemoveFocus(PhotonView pv)
	{
		if (pv == null)
		{
			text.text = string.Empty;
		}
		else if (pv.transform == focus)
		{
			text.text = string.Empty;
		}
	}

	private void LateUpdate()
	{
		if (focus != null)
		{
			base.transform.position = Camera.main.WorldToScreenPoint(focus.position);
		}
	}
}
