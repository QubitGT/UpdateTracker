using System;
using Modio.Mods;
using TMPro;
using UnityEngine;

namespace Modio.Unity.UI.Components.ModProperties;

[Serializable]
public class ModPropertySummary : IModProperty
{
	[SerializeField]
	private TMP_Text _text;

	[SerializeField]
	private GameObject _enableIfDescriptionDiffers;

	public void OnModUpdate(Mod mod)
	{
		_text.text = mod.Summary;
		if (_enableIfDescriptionDiffers != null)
		{
			bool active = !string.IsNullOrEmpty(mod.Description) && mod.Description != mod.Summary;
			_enableIfDescriptionDiffers.SetActive(active);
		}
	}
}
