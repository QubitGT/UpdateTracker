using System;
using System.Collections.Generic;
using System.Linq;
using Modio.Mods;
using Modio.Unity.UI.Panels;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Modio.Unity.UI.Components;

public class ModioUIGroup : MonoBehaviour
{
	private static readonly Dictionary<Mod, ModioUIMod> TempActive = new Dictionary<Mod, ModioUIMod>();

	private ModioUIMod _template;

	private readonly List<ModioUIMod> _active = new List<ModioUIMod>();

	private readonly List<ModioUIMod> _inactive = new List<ModioUIMod>();

	private (IReadOnlyList<Mod> mods, int selectionIndex) _displayOnEnable;

	[SerializeField]
	[Tooltip("(Optional) The root layout to rebuild before performing selections")]
	private RectTransform _layoutRebuilder;

	private void Awake()
	{
		_template = GetComponentInChildren<ModioUIMod>();
		if (_template != null)
		{
			_template.gameObject.SetActive(value: false);
			_inactive.Add(_template);
		}
		else
		{
			Debug.LogWarning("ModioUIGroup " + base.gameObject.name + " could not find a child ModioUIMod template, disabling.", this);
			base.enabled = false;
		}
	}

	private void OnEnable()
	{
		if (_displayOnEnable.mods != null)
		{
			SetMods(_displayOnEnable.mods, _displayOnEnable.selectionIndex);
			_displayOnEnable = default((IReadOnlyList<Mod>, int));
		}
	}

	public void SetMods(IReadOnlyList<Mod> mods, int selectionIndex = 0)
	{
		if (!base.enabled)
		{
			_displayOnEnable = (mods: mods, selectionIndex: selectionIndex);
			return;
		}
		if (mods == null)
		{
			mods = Array.Empty<Mod>();
		}
		TempActive.Clear();
		foreach (ModioUIMod item in _active)
		{
			if (mods.Contains(item.Mod) && !TempActive.ContainsKey(item.Mod))
			{
				TempActive.Add(item.Mod, item);
				continue;
			}
			item.gameObject.SetActive(value: false);
			item.SetMod(null);
			_inactive.Add(item);
		}
		_active.Clear();
		for (int i = 0; i < mods.Count; i++)
		{
			ModioUIMod value;
			bool num = TempActive.Remove(mods[i], out value);
			if (!num)
			{
				if (_inactive.Any())
				{
					int index = _inactive.Count - 1;
					value = _inactive[index];
					_inactive.RemoveAt(index);
				}
				else
				{
					value = UnityEngine.Object.Instantiate(_template.gameObject, _template.transform.parent).GetComponent<ModioUIMod>();
				}
				value.SetMod(mods[i]);
			}
			value.transform.SetSiblingIndex(i);
			if (!num)
			{
				value.gameObject.SetActive(value: true);
			}
			_active.Add(value);
		}
		EventSystem current2 = EventSystem.current;
		if (current2 == null)
		{
			ModioLog.Error?.Log("You are missing an event system, which the Modio UI requires to work. Consider adding ModioUI_InputCapture to your scene");
			return;
		}
		GameObject currentSelectedGameObject = current2.currentSelectedGameObject;
		bool flag = currentSelectedGameObject == null || !currentSelectedGameObject.activeInHierarchy;
		if (!flag && _active.Count > 0 && selectionIndex == 0)
		{
			flag |= currentSelectedGameObject.transform.parent == _active[0].transform.parent;
		}
		if (flag)
		{
			if (_layoutRebuilder != null)
			{
				LayoutRebuilder.ForceRebuildLayoutImmediate(_layoutRebuilder);
			}
			ModioPanelBase currentFocusedPanel = ModioPanelManager.GetInstance().CurrentFocusedPanel;
			if (_active.Count > 0)
			{
				currentFocusedPanel.SetSelectedGameObject(_active[Mathf.Min(selectionIndex, _active.Count - 1)].gameObject);
			}
			else if (currentFocusedPanel != null)
			{
				currentFocusedPanel.DoDefaultSelection();
			}
		}
	}
}
