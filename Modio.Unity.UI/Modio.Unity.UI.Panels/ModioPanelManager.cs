using System.Collections.Generic;
using UnityEngine;

namespace Modio.Unity.UI.Panels;

public class ModioPanelManager : MonoBehaviour
{
	private readonly List<ModioPanelBase> _allPotentialPanels = new List<ModioPanelBase>();

	private readonly List<ModioPanelBase> _openWindows = new List<ModioPanelBase>();

	private static ModioPanelManager _instance;

	public ModioPanelBase CurrentFocusedPanel
	{
		get
		{
			if (_openWindows.Count <= 0)
			{
				return null;
			}
			return _openWindows[_openWindows.Count - 1];
		}
	}

	public static ModioPanelManager GetInstance()
	{
		if (_instance != null)
		{
			return _instance;
		}
		_instance = Object.FindAnyObjectByType<ModioPanelManager>();
		if (_instance != null)
		{
			return _instance;
		}
		_instance = new GameObject("ModioPanelManager").AddComponent<ModioPanelManager>();
		return _instance;
	}

	private void Awake()
	{
		_instance = this;
	}

	public void OpenPanel(ModioPanelBase modioPanelBase)
	{
		if (_openWindows.Count > 0)
		{
			_openWindows[_openWindows.Count - 1].OnLostFocus();
		}
		_openWindows.Add(modioPanelBase);
		modioPanelBase.OnGainedFocus(ModioPanelBase.GainedFocusCause.OpeningFromClosed);
	}

	public void ClosePanel(ModioPanelBase modioPanelBase)
	{
		bool flag = false;
		for (int num = _openWindows.Count - 1; num >= 0; num--)
		{
			if (_openWindows[num] == modioPanelBase)
			{
				if (num == _openWindows.Count - 1)
				{
					flag = true;
				}
				_openWindows.RemoveAt(num);
			}
		}
		if (flag)
		{
			modioPanelBase.OnLostFocus();
			if (_openWindows.Count > 0)
			{
				_openWindows[_openWindows.Count - 1].OnGainedFocus(ModioPanelBase.GainedFocusCause.RegainingFocusFromStackedPanel);
			}
		}
	}

	public void PushFocusSuppression()
	{
		if (_openWindows.Count > 0)
		{
			ModioPanelBase modioPanelBase = _openWindows[_openWindows.Count - 1];
			if (modioPanelBase.HasFocus)
			{
				modioPanelBase.OnLostFocus();
			}
		}
	}

	public void PopFocusSuppression(ModioPanelBase.GainedFocusCause gainedFocusCause)
	{
		if (_openWindows.Count > 0)
		{
			ModioPanelBase modioPanelBase = _openWindows[_openWindows.Count - 1];
			if (!modioPanelBase.HasFocus)
			{
				modioPanelBase.OnGainedFocus(gainedFocusCause);
			}
		}
	}

	private void LateUpdate()
	{
		if (_openWindows.Count > 0 && _openWindows[_openWindows.Count - 1].HasFocus)
		{
			_openWindows[_openWindows.Count - 1].FocusedPanelLateUpdate();
		}
	}

	public void RegisterPanel(ModioPanelBase modioPanelBase)
	{
		_allPotentialPanels.Add(modioPanelBase);
	}

	public static T GetPanelOfType<T>() where T : ModioPanelBase
	{
		foreach (ModioPanelBase allPotentialPanel in GetInstance()._allPotentialPanels)
		{
			if (allPotentialPanel is T result)
			{
				return result;
			}
		}
		return null;
	}
}
