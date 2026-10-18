using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Photon.Pun.UtilityScripts;

public class OnClickRpc : MonoBehaviourPun, IPointerClickHandler, IEventSystemHandler
{
	public PointerEventData.InputButton Button;

	public KeyCode ModifierKey;

	public RpcTarget Target;

	private Material originalMaterial;

	private Color originalColor;

	private bool isFlashing;

	void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
	{
		if (PhotonNetwork.InRoom && (ModifierKey == KeyCode.None || Input.GetKey(ModifierKey)) && eventData.button == Button)
		{
			base.photonView.RPC("ClickRpc", Target);
		}
	}

	[PunRPC]
	public void ClickRpc()
	{
		StartCoroutine(ClickFlash());
	}

	public IEnumerator ClickFlash()
	{
		if (isFlashing)
		{
			yield break;
		}
		isFlashing = true;
		originalMaterial = GetComponent<Renderer>().material;
		if (!originalMaterial.HasProperty("_EmissionColor"))
		{
			Debug.LogWarning("Doesn't have emission, can't flash " + base.gameObject);
			yield break;
		}
		bool wasEmissive = originalMaterial.IsKeywordEnabled("_EMISSION");
		originalMaterial.EnableKeyword("_EMISSION");
		originalColor = originalMaterial.GetColor("_EmissionColor");
		originalMaterial.SetColor("_EmissionColor", Color.white);
		for (float f = 0f; f <= 1f; f += 0.08f)
		{
			Color value = Color.Lerp(Color.white, originalColor, f);
			originalMaterial.SetColor("_EmissionColor", value);
			yield return null;
		}
		originalMaterial.SetColor("_EmissionColor", originalColor);
		if (!wasEmissive)
		{
			originalMaterial.DisableKeyword("_EMISSION");
		}
		isFlashing = false;
	}
}
