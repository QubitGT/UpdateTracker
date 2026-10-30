using GorillaNetworking;
using UnityEngine;

[AddComponentMenu("GorillaTag/Cosmetics/Wardrobe Item Button")]
public class WardrobeItemButton : GorillaPressableButton
{
	public HeadModel controlledModel;

	public CosmeticsController.CosmeticItem currentCosmeticItem;
}
