using System;
using System.Threading.Tasks;
using Modio.API;
using Modio.Authentication;
using Modio.Errors;
using Modio.Users;

namespace Modio.Customizations;

public class ModioWssAuthService : IModioAuthService, IGetActiveUserIdentifier, IPotentialModioEmailAuthService
{
	private IWssAuthPrompter _authPrompter;

	private bool _isAttemptInProgress;

	private ExternalAuthenticationToken _authToken;

	public bool IsEmailPlatform => false;

	public ModioAPI.Portal Portal => ModioAPI.Portal.None;

	public Task<string> GetActiveUserIdentifier()
	{
		return Task.FromResult("linked_account_user");
	}

	public ModioWssAuthService(IWssAuthPrompter prompter)
	{
		_authPrompter = prompter;
	}

	public ModioWssAuthService()
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
			var (error2, authToken) = await Wss.BeginAuthenticationProcess();
			if ((bool)error2)
			{
				return ReturnErrorAndReset(error2);
			}
			_authToken = authToken;
			_authPrompter.ShowPrompt(authToken.url, authToken.code);
			var (error3, wssLoginSuccess) = await authToken.task;
			if ((bool)error3)
			{
				return ReturnErrorAndReset(error3);
			}
			_authToken = default(ExternalAuthenticationToken);
			try
			{
				User.Current.OnAuthenticated(wssLoginSuccess.access_token);
				ModioLog.Verbose?.Log("Wss authentication successful, authenticating local user...");
			}
			catch (Exception arg)
			{
				ModioLog.Error?.Log("Internal: Failed to deserialize user/token object from WssMessage and assign to UserData." + $"\n{arg}");
				return ReturnErrorAndReset(new Error(ErrorCode.INTERNAL_FAILED_TO_DESERIALIZE_OBJECT));
			}
			return ReturnErrorAndReset(Error.None);
		}
		return new Error(ErrorCode.USER_AUTHENTICATION_IN_PROGRESS);
	}

	private Error ValidateAttempt()
	{
		if (_authPrompter != null)
		{
			return Error.None;
		}
		ModioLog.Error?.Log($"{typeof(ModioWssAuthService)} cannot authenticate as no Prompter has been set! " + "Call ModioWssAuthService.SetPrompter before calling Authenticate or use a constructor that takes a Prompter parameter..");
		return new Error(ErrorCode.NOT_INITIALIZED);
	}

	private Error ReturnErrorAndReset(Error error)
	{
		_isAttemptInProgress = false;
		return error;
	}

	public void SetPrompter(IWssAuthPrompter prompter)
	{
		_authPrompter = prompter;
	}

	public bool InProgress()
	{
		return _isAttemptInProgress;
	}

	public void Cancel()
	{
		if (_isAttemptInProgress && _authToken.task != null)
		{
			_authToken.Cancel();
		}
	}
}
