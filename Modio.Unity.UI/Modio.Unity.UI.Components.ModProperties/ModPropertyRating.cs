using System;
using Modio.Mods;
using TMPro;
using UnityEngine;

namespace Modio.Unity.UI.Components.ModProperties;

[Serializable]
public class ModPropertyRating : IModProperty
{
	[SerializeField]
	private TMP_Text _text;

	[SerializeField]
	[Tooltip("Uses string.Format().\n{0} outputs the rating percentage value.")]
	private string _format = "{0}%";

	public void OnModUpdate(Mod mod)
	{
		if (_text != null)
		{
			_text.text = string.Format(_format, mod.Stats.RatingsPercent);
		}
	}
}
