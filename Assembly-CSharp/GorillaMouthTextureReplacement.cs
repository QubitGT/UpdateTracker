using GorillaTag;
using GorillaTag.CosmeticSystem;
using UnityEngine;

[AddComponentMenu("GorillaTag/Cosmetics/Gorilla Mouth Texture Replacement")]
public class GorillaMouthTextureReplacement : MonoBehaviour, ISpawnable
{
	[SerializeField]
	private Texture2D newMouthAtlas;

	private VRRig myRig;

	public bool IsSpawned { get; set; }

	public ECosmeticSelectSide CosmeticSelectedSide { get; set; }

	public void OnDespawn()
	{
	}

	public void OnSpawn(VRRig rig)
	{
		myRig = rig;
	}

	private void OnEnable()
	{
		myRig.GetComponent<GorillaMouthFlap>().SetMouthTextureReplacement(newMouthAtlas);
	}

	private void OnDisable()
	{
		myRig.GetComponent<GorillaMouthFlap>().ClearMouthTextureReplacement();
	}
}
