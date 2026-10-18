using Modio.Unity.UI.Search;
using TMPro;
using UnityEngine;

namespace Modio.Unity.UI.Components;

public class ModioUISearchField : MonoBehaviour
{
	[SerializeField]
	private TMP_InputField searchField;

	private string lastSearchPhrase;

	private bool _hasRunStart;

	private void Start()
	{
		_hasRunStart = true;
		OnEnable();
	}

	private void OnEnable()
	{
		if (_hasRunStart)
		{
			searchField.text = (lastSearchPhrase = "");
			ModioUISearch.Default.AppliedSearchPreset += OnAppliedSearchPreset;
		}
	}

	private void OnDisable()
	{
		ModioUISearch.Default.AppliedSearchPreset -= OnAppliedSearchPreset;
	}

	private void OnAppliedSearchPreset()
	{
		searchField.text = (lastSearchPhrase = "");
	}

	public void FilterView()
	{
		if (!(lastSearchPhrase == searchField.text))
		{
			lastSearchPhrase = searchField.text;
			ModioUISearch.Default.ApplySearchPhrase(searchField.text);
		}
	}
}
