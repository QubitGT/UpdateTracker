using System;
using Modio.Mods;
using Modio.Unity.UI.Search;
using TMPro;
using UnityEngine;

namespace Modio.Unity.UI.Components.SearchProperties;

[Serializable]
public class SearchPropertyTotalSize : ISearchProperty
{
	[SerializeField]
	private TMP_Text _totalFileSize;

	[SerializeField]
	[Tooltip("Bytes: \"1048576\".\r\nBytesComma: \"1,048,576\".\r\nSuffix: \"1 MB\".")]
	private StringFormatBytes _sizeFormat = StringFormatBytes.Suffix;

	[SerializeField]
	[ShowIf("IsCustomFormat")]
	private string _customSizeFormat;

	[SerializeField]
	private ModioUIMod _alsoIncludeSizeOf;

	[SerializeField]
	private bool _ignoreInstalledMods;

	private bool IsCustomFormat()
	{
		return _sizeFormat == StringFormatBytes.Custom;
	}

	public void OnSearchUpdate(ModioUISearch search)
	{
		if (!(_totalFileSize != null))
		{
			return;
		}
		long num = 0L;
		if (_alsoIncludeSizeOf != null && _alsoIncludeSizeOf.Mod != null && (!_ignoreInstalledMods || _alsoIncludeSizeOf.Mod.File.State != ModFileState.Installed))
		{
			num += _alsoIncludeSizeOf.Mod.File.FileSize;
		}
		foreach (Mod lastSearchResultMod in search.LastSearchResultMods)
		{
			if (!_ignoreInstalledMods || lastSearchResultMod.File.State != ModFileState.Installed)
			{
				num += lastSearchResultMod.File.FileSize;
			}
		}
		_totalFileSize.text = StringFormat.Bytes(_sizeFormat, num, _customSizeFormat);
	}
}
