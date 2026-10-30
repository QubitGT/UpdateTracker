using System.Collections;
using GorillaNetworking;
using UnityEngine;

[AddComponentMenu("GorillaTag/Cosmetics/Purchase Item Button")]
public class PurchaseItemButton : GorillaPressableButton
{
	public string buttonSide;

	public override void ButtonActivationWithHand(bool isLeftHand)
	{
		base.ButtonActivation();
		CosmeticsController.instance.PressPurchaseItemButton(this, isLeftHand);
		StartCoroutine(ButtonColorUpdate());
	}

	private IEnumerator ButtonColorUpdate()
	{
		Debug.Log("did this happen?");
		buttonRenderer.material = pressedMaterial;
		yield return new WaitForSeconds(debounceTime);
		buttonRenderer.material = (isOn ? pressedMaterial : unpressedMaterial);
	}
}
