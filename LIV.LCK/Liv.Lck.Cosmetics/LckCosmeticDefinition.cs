using System.Collections.Generic;
using UnityEngine;

namespace Liv.Lck.Cosmetics;

[CreateAssetMenu(fileName = "NewCosmetic", menuName = "LIV/LCK/LCK Cosmetics/Cosmetic Definition")]
public class LckCosmeticDefinition : ScriptableObject
{
	[Tooltip("The unique ID for this cosmetic. This will be used as the main asset's name inside the bundle.")]
	public string CosmeticId;

	[Tooltip("The readable name for this cosmetic.")]
	public string CosmeticName;

	[Tooltip("The type of this cosmetic, as defined by its LckCosmeticType SO.")]
	public LckCosmeticType CosmeticType;

	[Space(10f)]
	[Tooltip("The list of assets that are the primary resources when applying this cosmetic.")]
	public List<RootCosmetic> RootCosmetics;
}
