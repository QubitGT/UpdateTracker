using System;
using System.Threading.Tasks;
using Modio.API;
using Modio.API.SchemaDefinitions;
using Modio.Errors;
using Modio.Users;

namespace Modio.Authentication;

public class ModioEmailAuthService : IModioAuthService, IGetActiveUserIdentifier, IPotentialModioEmailAuthService
{
	private class EmailCodePrompter : IEmailCodePrompter
	{
		private readonly Func<Task<string>> _codePrompt;

		public EmailCodePrompter(Func<Task<string>> codePrompt)
		{
			_codePrompt = codePrompt;
		}

		public Task<string> ShowCodePrompt()
		{
			return _codePrompt();
		}
	}

	private IEmailCodePrompter _codePrompter;

	private bool _isAttemptInProgress;

	public bool IsEmailPlatform => true;

	public ModioAPI.Portal Portal => ModioAPI.Portal.None;

	public ModioEmailAuthService(Func<Task<string>> codePrompter)
	{
		_codePrompter = new EmailCodePrompter(codePrompter);
	}

	public ModioEmailAuthService(IEmailCodePrompter codePrompter)
	{
		_codePrompter = codePrompter;
	}

	public ModioEmailAuthService()
	{
	}

	public async Task<Error> Authenticate(bool displayedTerms, string thirdPartyEmail = null)
	{
		if (!_isAttemptInProgress)
		{
			_isAttemptInProgress = true;
			Error error = ValidateAttempt();
			if ((bool)error)
			{
				return ReturnErrorAndReset(error);
			}
			Error item = (await ModioAPI.Authentication.RequestEmailSecurityCode(new EmailAuthenticationRequest(thirdPartyEmail))).Item1;
			if ((bool)item)
			{
				return ReturnErrorAndReset(item);
			}
			string text = await _codePrompter.ShowCodePrompt();
			if (string.IsNullOrEmpty(text))
			{
				return ReturnErrorAndReset(new Error(ErrorCode.OPERATION_CANCELLED));
			}
			return await ExchangeCode(text);
		}
		return new Error(ErrorCode.USER_AUTHENTICATION_IN_PROGRESS);
	}

	public async Task<Error> AuthenticateWithoutEmailRequest()
	{
		Error error = ValidateAttempt();
		if ((bool)error)
		{
			return ReturnErrorAndReset(error);
		}
		string text = await _codePrompter.ShowCodePrompt();
		if (string.IsNullOrEmpty(text))
		{
			return ReturnErrorAndReset(new Error(ErrorCode.OPERATION_CANCELLED));
		}
		return await ExchangeCode(text);
	}

	private async Task<Error> ExchangeCode(string code)
	{
		var (error, accessTokenObject) = await ModioAPI.Authentication.ExchangeEmailSecurityCode(new EmailAuthenticationSecurityCodeRequest(code));
		if (!error)
		{
			User.Current.OnAuthenticated(accessTokenObject.Value.AccessToken);
		}
		return ReturnErrorAndReset(error);
	}

	private Error ValidateAttempt()
	{
		if (_codePrompter != null)
		{
			return Error.None;
		}
		ModioLog.Error?.Log($"{typeof(ModioEmailAuthService)} cannot authenticate as no Code Prompter has been set! Call ModioEmailAuthPlatform.SetCodePrompter before calling Authenticate or use a constructor that takes a Code Prompter parameter..");
		return new Error(ErrorCode.NOT_INITIALIZED);
	}

	private Error ReturnErrorAndReset(Error error)
	{
		_isAttemptInProgress = false;
		return error;
	}

	public void SetCodePrompter(IEmailCodePrompter codePrompter)
	{
		_codePrompter = codePrompter;
	}

	public void SetCodePrompter(Func<Task<string>> codePrompter)
	{
		_codePrompter = new EmailCodePrompter(codePrompter);
	}

	public Task<string> GetActiveUserIdentifier()
	{
		return Task.FromResult("user");
	}
}
