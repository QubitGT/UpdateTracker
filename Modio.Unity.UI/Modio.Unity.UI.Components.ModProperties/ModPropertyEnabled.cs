using System;
using Modio.Mods;
using Modio.Unity.Settings;
using UnityEngine;
using UnityEngine.UI;

namespace Modio.Unity.UI.Components.ModProperties;

[Serializable]
public class ModPropertyEnabled : IModProperty
{
	[SerializeField]
	private Toggle _enabledToggle;

	[SerializeField]
	private Button _enableButton;

	[SerializeField]
	private Button _disableButton;

	[SerializeField]
	private GameObject _showIfInstalledWhenEnabledNotAvailable;

	private Mod _mod;

	public void OnModUpdate(Mod mod)
	{
		_mod = mod;
		bool flag = mod.File.State == ModFileState.Installed && mod.IsSubscribed;
		ModioComponentUISettings platformSettings = ModioClient.Settings.GetPlatformSettings<ModioComponentUISettings>();
		if (platformSettings == null || !platformSettings.ShowEnableModToggle)
		{
			if (_showIfInstalledWhenEnabledNotAvailable != null)
			{
				_showIfInstalledWhenEnabledNotAvailable.SetActive(flag);
			}
			flag = false;
		}
		else if (_showIfInstalledWhenEnabledNotAvailable != null)
		{
			_showIfInstalledWhenEnabledNotAvailable.SetActive(value: false);
		}
		if (_enabledToggle != null)
		{
			_enabledToggle.gameObject.SetActive(flag);
			_enabledToggle.onValueChanged.RemoveListener(OnToggleValueChanged);
			_enabledToggle.isOn = mod.IsEnabled;
			_enabledToggle.onValueChanged.AddListener(OnToggleValueChanged);
		}
		if (_enableButton != null)
		{
			_enableButton.onClick.RemoveListener(EnableButtonClicked);
			_enableButton.onClick.AddListener(EnableButtonClicked);
			_enableButton.gameObject.SetActive(!_mod.IsEnabled && flag);
		}
		if (_disableButton != null)
		{
			_disableButton.onClick.RemoveListener(DisableButtonClicked);
			_disableButton.onClick.AddListener(DisableButtonClicked);
			_disableButton.gameObject.SetActive(_mod.IsEnabled && flag);
		}
	}

	private void OnToggleValueChanged(bool isEnabled)
	{
		_mod.SetIsEnabled(isEnabled);
	}

	private void EnableButtonClicked()
	{
		OnToggleValueChanged(isEnabled: true);
	}

	private void DisableButtonClicked()
	{
		OnToggleValueChanged(isEnabled: false);
	}
}
