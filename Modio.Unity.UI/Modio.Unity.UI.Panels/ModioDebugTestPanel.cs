using System.Linq;
using Modio.API;
using Modio.Unity.UI.Input;
using UnityEngine;

namespace Modio.Unity.UI.Panels;

public class ModioDebugTestPanel : ModioPanelBase
{
	private bool _hasDoneHookup;

	private ModioDebugMenu _modioDebugMenu;

	private ModioSettings _settings;

	protected override void Awake()
	{
		_modioDebugMenu = GetComponent<ModioDebugMenu>();
		_modioDebugMenu.Awake();
	}

	private void OnEnable()
	{
		if (ModioClient.Settings.TryGetPlatformSettings<ModioEnableDebugMenu>(out var _))
		{
			ModioUIInput.AddHandler(ModioUIInput.ModioAction.DeveloperMenu, base.OpenPanel);
		}
		if (!ModioServices.TryResolve<ModioSettings>(out var result))
		{
			ModioUnitySettings modioUnitySettings = Resources.Load<ModioUnitySettings>("mod.io/v3_config_local");
			if (modioUnitySettings == null)
			{
				modioUnitySettings = Resources.Load<ModioUnitySettings>("mod.io/v3_config");
			}
			if (modioUnitySettings == null)
			{
				Debug.LogError("Couldn't find bound Settings or settings file");
				return;
			}
			result = modioUnitySettings.Settings;
		}
		_settings = result.ShallowClone();
	}

	private void OnDisable()
	{
		ModioUIInput.RemoveHandler(ModioUIInput.ModioAction.DeveloperMenu, base.OpenPanel);
	}

	public override void OnGainedFocus(GainedFocusCause selectionBehaviour)
	{
		FindAllHookups();
		base.OnGainedFocus(selectionBehaviour);
		if (selectionBehaviour == GainedFocusCause.OpeningFromClosed)
		{
			_modioDebugMenu.SetToDefaults();
		}
	}

	private void FindAllHookups()
	{
		if (!_hasDoneHookup)
		{
			_hasDoneHookup = true;
			_modioDebugMenu.AddAllMethodsOrPropertiesWithAttribute((ModioDebugMenuAttribute attribute) => attribute.ShowInBrowserMenu);
			_modioDebugMenu.AddLabel("\nNetwork Settings");
			_modioDebugMenu.AddToggle("Fake Disconnected (global)", () => Get<ModioAPITestSettings>().FakeDisconnected, delegate(bool on)
			{
				Get<ModioAPITestSettings>().FakeDisconnected = on;
			});
			_modioDebugMenu.AddTextField("Fake Disconnected (regex)", () => Get<ModioAPITestSettings>().FakeDisconnectedOnEndpointRegex, delegate(string regex)
			{
				Get<ModioAPITestSettings>().FakeDisconnectedOnEndpointRegex = regex;
			});
			_modioDebugMenu.AddToggle("Fake Ratelimit (global)", () => Get<ModioAPITestSettings>().RateLimitError, delegate(bool on)
			{
				Get<ModioAPITestSettings>().RateLimitError = on;
			});
			_modioDebugMenu.AddTextField("Fake Ratelimit (regex)", () => Get<ModioAPITestSettings>().RateLimitOnEndpointRegex, delegate(string regex)
			{
				Get<ModioAPITestSettings>().RateLimitOnEndpointRegex = regex;
			});
		}
		T Get<T>() where T : IModioServiceSettings, new()
		{
			T val = _settings.GetPlatformSettings<T>();
			if (val == null)
			{
				val = new T();
				_settings.PlatformSettings = _settings.PlatformSettings.Append(val).ToArray();
			}
			return val;
		}
	}
}
