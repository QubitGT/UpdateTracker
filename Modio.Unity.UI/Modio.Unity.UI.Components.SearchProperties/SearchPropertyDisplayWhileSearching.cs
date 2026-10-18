using System;
using Modio.Unity.UI.Search;
using UnityEngine;

namespace Modio.Unity.UI.Components.SearchProperties;

[Serializable]
public class SearchPropertyDisplayWhileSearching : ISearchProperty
{
	private enum AdditiveLoadBehaviour
	{
		Ignore,
		OnlyDuringAdditiveLoad,
		HideDuringAdditiveLoad
	}

	[SerializeField]
	private GameObject _displayWhileSearching;

	[SerializeField]
	private AdditiveLoadBehaviour _additiveLoadBehaviour;

	public void OnSearchUpdate(ModioUISearch search)
	{
		bool flag = _additiveLoadBehaviour switch
		{
			AdditiveLoadBehaviour.Ignore => true, 
			AdditiveLoadBehaviour.OnlyDuringAdditiveLoad => search.IsAdditiveSearch, 
			AdditiveLoadBehaviour.HideDuringAdditiveLoad => !search.IsAdditiveSearch, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
		_displayWhileSearching.SetActive(search.IsSearching && flag);
	}
}
