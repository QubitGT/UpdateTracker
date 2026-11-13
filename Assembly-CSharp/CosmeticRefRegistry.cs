using UnityEngine;

[AddComponentMenu("GorillaTag/Cosmetics/Cosmetic Ref Registry")]
public class CosmeticRefRegistry : MonoBehaviour
{
	private GameObject[] partsTable = new GameObject[9];

	[SerializeField]
	private CosmeticRefTarget[] builtInRefTargets;

	private void Awake()
	{
		CosmeticRefTarget[] array = builtInRefTargets;
		foreach (CosmeticRefTarget cosmeticRefTarget in array)
		{
			Register(cosmeticRefTarget.id, cosmeticRefTarget.gameObject);
		}
	}

	public void Register(CosmeticRefID partID, GameObject part)
	{
		partsTable[(int)partID] = part;
	}

	public void Unregister(CosmeticRefID partID, GameObject part)
	{
		if (partsTable[(int)partID] != part)
		{
			return;
		}
		partsTable[(int)partID] = null;
		CosmeticRefTarget[] array = builtInRefTargets;
		foreach (CosmeticRefTarget cosmeticRefTarget in array)
		{
			if (cosmeticRefTarget != null && cosmeticRefTarget.id == partID)
			{
				partsTable[(int)partID] = cosmeticRefTarget.gameObject;
				break;
			}
		}
	}

	public GameObject Get(CosmeticRefID partID)
	{
		return partsTable[(int)partID];
	}
}
