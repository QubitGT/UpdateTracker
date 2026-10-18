using System.Linq;
using Modio.API;
using Modio.Authentication;
using Modio.Extensions;
using Modio.FileIO;
using Modio.Unity.Settings;
using UnityEngine;

namespace Modio.Unity.UI.Panels;

public class ModioExampleSettingsPanel : ModioPanelBase
{
	private bool _hasDoneSetup;

	private ModioSettings _settings = new ModioSettings();

	private ModioDebugMenu _debugMenu;

	private void OnEnable()
	{
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

	public override void OnGainedFocus(GainedFocusCause selectionBehaviour)
	{
		base.OnGainedFocus(selectionBehaviour);
		if (selectionBehaviour == GainedFocusCause.OpeningFromClosed)
		{
			SetupButtons();
		}
	}

	private void SetupButtons()
	{
		if (_hasDoneSetup)
		{
			return;
		}
		_hasDoneSetup = true;
		_debugMenu = GetComponent<ModioDebugMenu>();
		_debugMenu.AddLabel("Enter the Game Id and Key for the game you'd like to browse mods for.\nThen, scroll down and hit 'Apply changed settings'");
		_debugMenu.AddTextField("Game Id:", () => _settings.GameId, delegate(long id)
		{
			_settings.GameId = id;
			if (_settings.ServerURL.Contains("api-staging"))
			{
				_settings.ServerURL = StagingUrl();
			}
			else if (_settings.ServerURL.Contains("test"))
			{
				_settings.ServerURL = TestUrl(_settings.GameId);
			}
			else
			{
				_settings.ServerURL = ProductionUrl(_settings.GameId);
			}
		});
		_debugMenu.AddTextField("Game Key:", () => _settings.APIKey, delegate(string key)
		{
			_settings.APIKey = key;
		});
		_debugMenu.AddToggle("Production Environment", () => !_settings.ServerURL.Contains("api-staging") && !_settings.ServerURL.Contains("test"), delegate(bool production)
		{
			if (production)
			{
				_settings.ServerURL = ProductionUrl(_settings.GameId);
			}
			_debugMenu.SetToDefaults();
		});
		_debugMenu.AddToggle("Staging Environment", () => _settings.ServerURL.Contains("api-staging"), delegate(bool staging)
		{
			if (staging)
			{
				_settings.ServerURL = StagingUrl();
			}
			_debugMenu.SetToDefaults();
		});
		_debugMenu.AddToggle("Test Environment", () => _settings.ServerURL.Contains("test"), delegate(bool test)
		{
			if (test)
			{
				_settings.ServerURL = TestUrl(_settings.GameId);
			}
			_debugMenu.SetToDefaults();
		});
		_debugMenu.AddTextField("Default Language:", () => _settings.DefaultLanguage, delegate(string isoCode)
		{
			_settings.DefaultLanguage = isoCode;
		});
		_debugMenu.AddLabel("\nTUI Settings");
		_debugMenu.AddToggle("Show monetization", () => Get<ModioComponentUISettings>().ShowMonetizationUI, delegate(bool on)
		{
			Get<ModioComponentUISettings>().ShowMonetizationUI = on;
		});
		_debugMenu.AddToggle("Show enabled", () => Get<ModioComponentUISettings>().ShowEnableModToggle, delegate(bool on)
		{
			Get<ModioComponentUISettings>().ShowEnableModToggle = on;
		});
		_debugMenu.AddToggle("Fallback to email authentication", () => Get<ModioComponentUISettings>().FallbackToEmailAuthentication, delegate(bool on)
		{
			Get<ModioComponentUISettings>().FallbackToEmailAuthentication = on;
		});
		_debugMenu.AddLabel("\nDisk Settings");
		_debugMenu.AddToggle("Override Disk Space Remaining", () => Get<ModioDiskTestSettings>().OverrideDiskSpaceRemaining, delegate(bool on)
		{
			Get<ModioDiskTestSettings>().OverrideDiskSpaceRemaining = on;
		});
		_debugMenu.AddTextField("Fake Bytes Remaining", () => Get<ModioDiskTestSettings>().BytesRemaining, delegate(int on)
		{
			Get<ModioDiskTestSettings>().BytesRemaining = on;
		});
		_debugMenu.AddLabel("\nNetwork Settings");
		_debugMenu.AddToggle("Fake Disconnected (global)", () => Get<ModioAPITestSettings>().FakeDisconnected, delegate(bool on)
		{
			Get<ModioAPITestSettings>().FakeDisconnected = on;
		});
		_debugMenu.AddTextField("Fake Disconnected (regex)", () => Get<ModioAPITestSettings>().FakeDisconnectedOnEndpointRegex, delegate(string regex)
		{
			Get<ModioAPITestSettings>().FakeDisconnectedOnEndpointRegex = regex;
		});
		_debugMenu.AddToggle("Fake Ratelimit (global)", () => Get<ModioAPITestSettings>().RateLimitError, delegate(bool on)
		{
			Get<ModioAPITestSettings>().RateLimitError = on;
		});
		_debugMenu.AddTextField("Fake Ratelimit (regex)", () => Get<ModioAPITestSettings>().RateLimitOnEndpointRegex, delegate(string regex)
		{
			Get<ModioAPITestSettings>().RateLimitOnEndpointRegex = regex;
		});
		_debugMenu.AddLabel("\nIn browser debug menu");
		_debugMenu.AddToggle("Enable", () => _settings.TryGetPlatformSettings<ModioEnableDebugMenu>(out var _), delegate(bool on)
		{
			if (on)
			{
				Get<ModioEnableDebugMenu>();
			}
			else
			{
				_settings.PlatformSettings = _settings.PlatformSettings.Where((IModioServiceSettings s) => !(s is ModioEnableDebugMenu)).ToArray();
			}
		});
		_debugMenu.AddLabel("");
		_debugMenu.AddButton("Apply Changed Settings", delegate
		{
			ModioServices.BindInstance(_settings);
			ModioClient.Shutdown().ForgetTaskSafely();
			ClosePanel();
		});
		_debugMenu.AddButton("Cancel Changed Settings", delegate
		{
			if (ModioServices.TryResolve<ModioSettings>(out var result))
			{
				_settings = result.ShallowClone();
			}
			_debugMenu.SetToDefaults();
		});
		_debugMenu.AddLabel("\nAuth Platform");
		ModioMultiplatformAuthResolver.Initialize();
		foreach (IModioAuthService modioAuthPlatform in ModioMultiplatformAuthResolver.AuthBindings)
		{
			_debugMenu.AddToggle(ModioDebugMenu.Nicify(modioAuthPlatform.GetType().Name), () => ModioMultiplatformAuthResolver.ServiceOverride == modioAuthPlatform, delegate(bool on)
			{
				if (on)
				{
					ModioMultiplatformAuthResolver.ServiceOverride = modioAuthPlatform;
				}
				_debugMenu.SetToDefaults();
				if (ModioClient.IsInitialized)
				{
					ModioClient.Shutdown().ForgetTaskSafely();
				}
			});
		}
		_debugMenu.AddLabel("\nMisc Discovered Settings");
		_debugMenu.AddAllMethodsOrPropertiesWithAttribute((ModioDebugMenuAttribute attribute) => attribute.ShowInSettingsMenu);
		_debugMenu.SetToDefaults();
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

	private string StagingUrl()
	{
		return "https://api-staging.moddemo.io/v1";
	}

	private string ProductionUrl(long gameId)
	{
		return $"https://g-{gameId}.modapi.io/v1";
	}

	private string TestUrl(long gameId)
	{
		return $"https://g-{gameId}.test.mod.io/v1";
	}
}
