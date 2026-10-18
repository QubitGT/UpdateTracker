using System;
using Modio.Mods;
using TMPro;
using UnityEngine;

namespace Modio.Unity.UI.Components.ModProperties;

[Serializable]
public class ModPropertyFileSize : IModProperty
{
	[SerializeField]
	private TMP_Text _text;

	[SerializeField]
	[Tooltip("Bytes: \"1048576\".\r\nBytesComma: \"1,048,576\".\r\nSuffix: \"1 MB\".")]
	private StringFormatBytes _format = StringFormatBytes.Suffix;

	[SerializeField]
	[ShowIf("IsCustomFormat")]
	private string _customFormat;

	public void OnModUpdate(Mod mod)
	{
		_text.text = ((mod?.File == null) ? "NULL" : StringFormat.Bytes(_format, mod.File.FileSize, _customFormat));
	}

	private bool IsCustomFormat()
	{
		return _format == StringFormatBytes.Custom;
	}
}
