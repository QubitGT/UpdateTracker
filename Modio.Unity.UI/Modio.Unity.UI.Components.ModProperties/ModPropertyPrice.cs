using System;
using Modio.Mods;
using UnityEngine;

namespace Modio.Unity.UI.Components.ModProperties;

[Serializable]
public class ModPropertyPrice : ModPropertyNumberBase
{
	[SerializeField]
	private GameObject _disableIfFree;

	[SerializeField]
	private bool _alsoDisableIfPurchased;

	[SerializeField]
	private GameObject _enableIfPurchased;

	protected override long GetValue(Mod mod)
	{
		if (_disableIfFree != null)
		{
			_disableIfFree.SetActive(mod.IsMonetized && (!_alsoDisableIfPurchased || !mod.IsPurchased));
		}
		if (_enableIfPurchased != null)
		{
			_enableIfPurchased.SetActive(mod.IsPurchased);
		}
		return mod.Price;
	}
}
