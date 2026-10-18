using System;
using Modio.Mods;
using Modio.Users;
using UnityEngine;

namespace Modio.Unity.UI.Components.ModProperties;

[Serializable]
public class ModPropertyCanAffordPurchase : IModProperty
{
	[SerializeField]
	private GameObject[] _activateWhenCanAfford;

	[SerializeField]
	private GameObject[] _activateWhenCanNotAfford;

	public void OnModUpdate(Mod mod)
	{
		long num = User.Current?.Wallet.Balance ?? 0;
		bool flag = mod.Price <= num;
		GameObject[] activateWhenCanAfford = _activateWhenCanAfford;
		for (int i = 0; i < activateWhenCanAfford.Length; i++)
		{
			activateWhenCanAfford[i].SetActive(flag);
		}
		activateWhenCanAfford = _activateWhenCanNotAfford;
		for (int i = 0; i < activateWhenCanAfford.Length; i++)
		{
			activateWhenCanAfford[i].SetActive(!flag);
		}
	}
}
