using UnityEngine;

namespace Liv.Lck.Cosmetics;

[CreateAssetMenu(fileName = "NewLckCosmeticType", menuName = "LIV/LCK/LCK Cosmetics/Cosmetic Type")]
public class LckCosmeticType : ScriptableObject
{
	[Tooltip("The string value for this cosmetic type (e.g 'Keychain', 'Skin').")]
	public string TypeValue;
}
