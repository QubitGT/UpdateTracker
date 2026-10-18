using System;
using System.Collections;
using Modio.Unity.UI.Search;
using UnityEngine;
using UnityEngine.UI;

namespace Modio.Unity.UI.Components.SearchProperties;

[Serializable]
public class SearchPropertyEndlessScroll : ISearchProperty, IPropertyMonoBehaviourEvents
{
	[SerializeField]
	private ScrollRect _scrollRect;

	[SerializeField]
	private float _distanceFromBottomToLoadContent = 300f;

	private Coroutine _monitorCoroutine;

	private ModioUISearch _search;

	private bool _hasRunStart;

	public void OnSearchUpdate(ModioUISearch search)
	{
		_search = search;
	}

	public void Start()
	{
		_hasRunStart = true;
		OnEnable();
	}

	public void OnDestroy()
	{
	}

	public void OnEnable()
	{
		if (_hasRunStart)
		{
			_monitorCoroutine = _scrollRect.StartCoroutine(MonitorCo());
		}
	}

	private IEnumerator MonitorCo()
	{
		while (true)
		{
			if (0f - (((RectTransform)_scrollRect.transform).rect.height + _scrollRect.content.offsetMin.y) < _distanceFromBottomToLoadContent && _search != null && _search.CanGetMoreMods && !_search.IsSearching)
			{
				_search.GetNextPageAdditivelyForLastSearch();
			}
			yield return null;
		}
	}

	public void OnDisable()
	{
		if (_monitorCoroutine != null)
		{
			_scrollRect.StopCoroutine(_monitorCoroutine);
		}
	}
}
