using System.Threading.Tasks;
using Modio.Authentication;
using Modio.Errors;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Modio.Unity.UI.Panels.Authentication;

public class ModioAuthenticationIEmailPanel : ModioPanelBase, IEmailCodePrompter
{
	[SerializeField]
	private TMP_InputField _emailField;

	[SerializeField]
	private UnityEvent<Error> _onError;

	private ModioEmailAuthService _authService;

	private bool _isCodeEntered;

	private string _authCode = string.Empty;

	public override void OnGainedFocus(GainedFocusCause context)
	{
		base.OnGainedFocus(context);
		_authService = ModioServices.Resolve<ModioEmailAuthService>();
		_authService.SetCodePrompter(this);
	}

	public async void OnPressSubmitEmail()
	{
		ClosePanel();
		ModioPanelManager.GetPanelOfType<ModioAuthenticationWaitingPanel>().OpenPanel();
		await AuthenticationRequest(_emailField.text, _authService.Authenticate(displayedTerms: true, _emailField.text));
	}

	public async void OnPressIHaveCode()
	{
		ClosePanel();
		await AuthenticationRequest(_emailField.text, _authService.AuthenticateWithoutEmailRequest());
	}

	private void OnCodeEntered(string code)
	{
		_authCode = code;
		_isCodeEntered = true;
	}

	private async Task AuthenticationRequest(string email, Task<Error> authMethod)
	{
		Error error = await authMethod;
		ModioPanelManager.GetPanelOfType<ModioAuthenticationWaitingPanel>()?.ClosePanel();
		if (!error)
		{
			return;
		}
		if (error.Code == ErrorCode.OPERATION_CANCELLED)
		{
			ModioLog.Verbose?.Log("Cancelling Email Authentication Request.");
			return;
		}
		if (error.Code == ErrorCode.VALIDATION_ERRORS)
		{
			error = new Error(ErrorCode.EMAIL_LOGIN_CODE_INVALID);
		}
		if (!error.IsSilent)
		{
			ModioLog.Error?.Log("Error authenticating with email: " + error.GetMessage() + "\nEmail: " + email);
		}
		_onError.Invoke(error);
	}

	public async Task<string> ShowCodePrompt()
	{
		_isCodeEntered = false;
		_authCode = string.Empty;
		ModioPanelManager.GetPanelOfType<ModioAuthenticationWaitingPanel>()?.ClosePanel();
		ModioPanelManager.GetPanelOfType<ModioAuthenticationCodePanel>()?.OpenPanel(_emailField.text, OnCodeEntered);
		while (!_isCodeEntered)
		{
			await Task.Delay(1000);
		}
		ModioLog.Verbose?.Log("Code entered: " + _authCode);
		return _authCode;
	}
}
