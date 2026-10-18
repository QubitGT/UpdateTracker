using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Photon.Pun.UtilityScripts;

public class TabViewManager : MonoBehaviour
{
	[Serializable]
	public class TabChangeEvent : UnityEvent<string>
	{
	}

	[Serializable]
	public class Tab
	{
		public string ID = "";

		public Toggle Toggle;

		public RectTransform View;
	}

	public ToggleGroup ToggleGroup;

	public Tab[] Tabs;

	public TabChangeEvent OnTabChanged;

	protected Tab CurrentTab;

	private Dictionary<Toggle, Tab> Tab_lut;

	private void Start()
	{
		Tab_lut = new Dictionary<Toggle, Tab>();
		Tab[] tabs = Tabs;
		foreach (Tab _tab in tabs)
		{
			Tab_lut[_tab.Toggle] = _tab;
			_tab.View.gameObject.SetActive(_tab.Toggle.isOn);
			if (_tab.Toggle.isOn)
			{
				CurrentTab = _tab;
			}
			_tab.Toggle.onValueChanged.AddListener(delegate(bool isSelected)
			{
				if (isSelected)
				{
					OnTabSelected(_tab);
				}
			});
		}
	}

	public void SelectTab(string id)
	{
		Tab[] tabs = Tabs;
		foreach (Tab tab in tabs)
		{
			if (tab.ID == id)
			{
				tab.Toggle.isOn = true;
				break;
			}
		}
	}

	private void OnTabSelected(Tab tab)
	{
		CurrentTab.View.gameObject.SetActive(value: false);
		CurrentTab = Tab_lut[ToggleGroup.ActiveToggles().FirstOrDefault()];
		CurrentTab.View.gameObject.SetActive(value: true);
		OnTabChanged.Invoke(CurrentTab.ID);
	}
}
