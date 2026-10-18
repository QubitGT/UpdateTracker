using System;
using System.Linq;
using Modio.Unity.UI.Search;
using UnityEngine;

namespace Modio.Unity.UI.Components.SearchProperties;

[Serializable]
public class SearchPropertyDisableObjectForSearchType : ISearchProperty
{
	[SerializeField]
	private GameObject[] _gameObjectsToHide;

	[SerializeField]
	private GameObject[] _gameObjectsToShow;

	[SerializeField]
	private bool _hideOnCustomSearch;

	[SerializeField]
	private SpecialSearchType[] _hideForSearchTypes;

	public void OnSearchUpdate(ModioUISearch search)
	{
		bool flag = (_hideOnCustomSearch && search.HasCustomSearch()) || _hideForSearchTypes.Contains(search.LastSearchPreset);
		GameObject[] gameObjectsToHide = _gameObjectsToHide;
		for (int i = 0; i < gameObjectsToHide.Length; i++)
		{
			gameObjectsToHide[i].SetActive(!flag);
		}
		gameObjectsToHide = _gameObjectsToShow;
		for (int i = 0; i < gameObjectsToHide.Length; i++)
		{
			gameObjectsToHide[i].SetActive(flag);
		}
	}
}
