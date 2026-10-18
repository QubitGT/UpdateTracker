using Modio.Unity.UI.Components.Localization;
using Modio.Unity.UI.Search;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Modio.Unity.UI.Components;

public class ModioUISearchCategoryTab : MonoBehaviour
{
	[SerializeField]
	private ModioUISearchSettings _search;

	[SerializeField]
	private TMP_Text _label;

	[SerializeField]
	private ModioUILocalizedText _labelLocalised;

	[SerializeField]
	private bool _selectOnEnable;

	private Toggle _toggle;

	private void Awake()
	{
		_toggle = GetComponent<Toggle>();
		_toggle.onValueChanged.AddListener(OnToggleValueChanged);
	}

	private void Start()
	{
		if (_toggle.isOn)
		{
			OnToggleValueChanged(newValue: true);
		}
	}

	private void OnToggleValueChanged(bool newValue)
	{
		if (newValue && ModioUISearch.Default != null && _search != null)
		{
			_search.SetAsCustomSearchBase(ModioUISearch.Default);
			_search.Search(ModioUISearch.Default);
		}
	}

	public void SetSearch(ModioUISearchSettings searchSettings)
	{
		_search = searchSettings;
		if (_label != null)
		{
			_label.text = searchSettings.DisplayAs;
		}
		if (_labelLocalised != null)
		{
			_labelLocalised.SetKey(searchSettings.DisplayAsLocalisedKey);
		}
	}

	public void SetSelected(bool selected = true)
	{
		if (_toggle == null)
		{
			_toggle = GetComponent<Toggle>();
			_toggle.isOn = selected;
			OnToggleValueChanged(selected);
		}
		else if (_toggle.isOn != selected)
		{
			_toggle.isOn = selected;
		}
		else
		{
			OnToggleValueChanged(selected);
		}
	}
}
