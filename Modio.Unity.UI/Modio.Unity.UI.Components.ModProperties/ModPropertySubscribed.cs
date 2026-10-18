using System;
using Modio.Mods;
using UnityEngine;

namespace Modio.Unity.UI.Components.ModProperties;

[Serializable]
public class ModPropertySubscribed : IModProperty
{
	[SerializeField]
	private GameObject _notSubscribedActive;

	[SerializeField]
	private GameObject _subscribedActive;

	public void OnModUpdate(Mod mod)
	{
		if (_notSubscribedActive != null)
		{
			_notSubscribedActive.SetActive(!mod.IsSubscribed);
		}
		if (_subscribedActive != null)
		{
			_subscribedActive.SetActive(mod.IsSubscribed);
		}
	}
}
