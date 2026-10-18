using System.Threading.Tasks;
using Modio.Authentication;
using Modio.Errors;
using Modio.Extensions;
using Modio.Unity.Settings;
using Modio.Users;
using UnityEngine;
using UnityEngine.Events;

namespace Modio.Unity.UI.Panels.Authentication;

public class ModioAuthenticationPanel : ModioPanelBase
{
	[SerializeField]
	private UnityEvent<Error> _onError;

	[SerializeField]
	private UnityEvent<Error> _onOffline;

	private bool _fallbackToEmailAuth;

	[ModioDebugMenu(ShowInBrowserMenu = false, ShowInSettingsMenu = true)]
	private static bool ForceShowTermsOfUse { get; set; }

	public void OpenAuthFlow()
	{
		if (User.Current != null && User.Current.IsAuthenticated)
		{
			Debug.LogWarning("Attempted to open Auth Flow when already logged in");
			return;
		}
		if (!ModioClient.IsInitialized)
		{
			ModioWaitingPanelGeneric panelOfType = ModioPanelManager.GetPanelOfType<ModioWaitingPanelGeneric>();
			if (panelOfType != null && !panelOfType.HasFocus)
			{
				panelOfType.OpenPanel();
			}
		}
		ModioClient.OnInitialized -= OnPluginReady;
		ModioClient.OnInitialized += OnPluginReady;
	}

	protected override void OnDestroy()
	{
		ModioClient.OnInitialized -= OnPluginReady;
		base.OnDestroy();
	}

	private void OnPluginReady()
	{
		if (ModioClient.AuthService != null && !(ModioClient.AuthService is IPotentialModioEmailAuthService { IsEmailPlatform: not false }))
		{
			OpenPanel();
			AttemptSso(agreedToTerms: false).ForgetTaskSafely();
		}
		else
		{
			GetTermsAndShowPanel().ForgetTaskSafely();
		}
	}

	private async Task GetTermsAndShowPanel()
	{
		OpenPanel();
		ModioWaitingPanelGeneric waitingPanel = ModioPanelManager.GetPanelOfType<ModioWaitingPanelGeneric>();
		if (waitingPanel != null && !waitingPanel.HasFocus)
		{
			waitingPanel.OpenPanel();
		}
		Error item = (await TermsOfUse.Get()).Item1;
		waitingPanel?.ClosePanel();
		if (item.Code == ErrorCode.CANNOT_OPEN_CONNECTION)
		{
			_onOffline.Invoke(item);
		}
		else if (ModioClient.AuthService != null && !(ModioClient.AuthService is IPotentialModioEmailAuthService { IsEmailPlatform: not false }))
		{
			ModioPanelManager.GetPanelOfType<ModioAuthenticationTermsOfServicePanel>()?.OpenPanel();
		}
		else
		{
			ModioPanelManager.GetPanelOfType<ModioAuthenticationIEmailPanel>()?.OpenPanel();
		}
	}

	private void LateUpdate()
	{
		if (base.HasFocus)
		{
			if (_fallbackToEmailAuth)
			{
				_fallbackToEmailAuth = false;
				ModioPanelManager.GetPanelOfType<ModioAuthenticationIEmailPanel>()?.OpenPanel();
			}
			else
			{
				ClosePanel();
			}
		}
	}

	public async Task AttemptSso(bool agreedToTerms)
	{
		ModioWaitingPanelGeneric waitingPanel = ModioPanelManager.GetPanelOfType<ModioWaitingPanelGeneric>();
		waitingPanel?.OpenPanel();
		Error error = ((!ForceShowTermsOfUse || agreedToTerms) ? (await ModioClient.AuthService.Authenticate(agreedToTerms)) : new Error(ErrorCode.USER_NO_ACCEPT_TERMS_OF_USE));
		if (!error)
		{
			waitingPanel?.ClosePanel();
			ModioLog.Verbose?.Log("Signed in successfully");
		}
		else
		{
			if (!agreedToTerms && error.Code == ErrorCode.USER_NO_ACCEPT_TERMS_OF_USE)
			{
				ModioLog.Message?.Log("User hasn't agreed to terms");
				OpenPanel();
				GetTermsAndShowPanel().ForgetTaskSafely();
				return;
			}
			if (!error.IsSilent)
			{
				ModioLog.Error?.Log($"SSO failed: {error.GetMessage()} (agreed to terms {agreedToTerms})");
			}
			_onError?.Invoke(error);
			waitingPanel?.ClosePanel();
			if (error.Code != ErrorCode.CANNOT_OPEN_CONNECTION)
			{
				ModioComponentUISettings platformSettings = ModioClient.Settings.GetPlatformSettings<ModioComponentUISettings>();
				if (platformSettings != null && platformSettings.FallbackToEmailAuthentication)
				{
					_fallbackToEmailAuth = true;
				}
			}
		}
		ModioPanelManager.GetPanelOfType<ModioAuthenticationTermsOfServicePanel>()?.ClosePanel();
	}
}
