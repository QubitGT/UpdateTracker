using UnityEngine;

[AddComponentMenu("GorillaTag/Cosmetics/Holdable Handle")]
public class HoldableHandle : InteractionPoint
{
	[SerializeField]
	private HoldableObject holdable;

	[SerializeField]
	private CapsuleCollider handleCapsuleTrigger;

	public new HoldableObject Holdable => holdable;

	public CapsuleCollider Capsule => handleCapsuleTrigger;
}
