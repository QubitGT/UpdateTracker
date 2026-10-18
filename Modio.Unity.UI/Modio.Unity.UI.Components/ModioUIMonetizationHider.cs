using Modio.API;
using Modio.Unity.Settings;
using UnityEngine;

namespace Modio.Unity.UI.Components;

public class ModioUIMonetizationHider : MonoBehaviour
{
	private bool _isOffline;

	private bool _isMonetizationDisabled;

	private void Start()
	{
		ModioClient.OnInitialized += OnPluginInitialized;
		ModioAPI.OnOfflineStatusChanged += OnOfflineStatusChanged;
	}

	private void OnDestroy()
	{
		ModioClient.OnInitialized -= OnPluginInitialized;
		ModioAPI.OnOfflineStatusChanged -= OnOfflineStatusChanged;
	}

	private void OnOfflineStatusChanged(bool isOffline)
	{
		_isOffline = isOffline;
		ChangeActiveStateIfNeeded();
	}

	private void OnPluginInitialized()
	{
		ModioSettings modioSettings = ModioServices.Resolve<ModioSettings>();
		_isMonetizationDisabled = !(modioSettings.GetPlatformSettings<ModioComponentUISettings>()?.ShowMonetizationUI ?? false);
		ChangeActiveStateIfNeeded();
	}

	private void ChangeActiveStateIfNeeded()
	{
		base.gameObject.SetActive(!_isOffline && !_isMonetizationDisabled);
	}
}
