using Modio.Unity.UI.Panels;
using UnityEngine;
using UnityEngine.UI;

namespace Modio.Unity.UI.Components.Selectables;

public class ModioUIToggleDeactivateWhenPanelLostFocus : MonoBehaviour
{
	[SerializeField]
	private ModioPanelBase _panel;

	private Toggle _toggle;

	private void Awake()
	{
		_toggle = GetComponent<Toggle>();
		_toggle.onValueChanged.AddListener(OnValueChanged);
	}

	private void OnValueChanged(bool isOn)
	{
		if (!(_panel == null))
		{
			_panel.OnHasFocusChanged -= PanelChangedFocus;
			if (isOn)
			{
				_panel.OnHasFocusChanged += PanelChangedFocus;
			}
		}
	}

	private void OnDestroy()
	{
		if (_panel != null)
		{
			_panel.OnHasFocusChanged -= PanelChangedFocus;
		}
	}

	private void PanelChangedFocus(bool hasFocus)
	{
		if (!hasFocus)
		{
			_toggle.isOn = false;
		}
	}
}
