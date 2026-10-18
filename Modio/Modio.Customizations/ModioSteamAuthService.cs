using System;
using System.Threading.Tasks;
using Modio.API;
using Modio.API.SchemaDefinitions;
using Modio.Authentication;
using Modio.Errors;
using Modio.Users;

namespace Modio.Customizations;

public class ModioSteamAuthService : IModioAuthService, IGetActiveUserIdentifier, IPotentialModioEmailAuthService
{
	private ISteamCredentialProvider _credentialProvider;

	private bool _isAttemptInProgress;

	private string _encryptedAppTicket;

	private Error _encryptedAppTicketError = Error.None;

	public bool IsEmailPlatform => false;

	public ModioAPI.Portal Portal => ModioAPI.Portal.Steam;

	public Task<string> GetActiveUserIdentifier()
	{
		return Task.FromResult("steam_user");
	}

	public ModioSteamAuthService(ISteamCredentialProvider credentialProvider)
	{
		_credentialProvider = credentialProvider;
	}

	public ModioSteamAuthService()
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
			ModioLog.Verbose?.Log("Starting Steam Authentication, requesting Steam Encrypted App Ticket...");
			_credentialProvider.RequestEncryptedAppTicket(OnGetEncryptedAppTicket);
			while (_encryptedAppTicket == null && _encryptedAppTicketError.Equals(Error.None))
			{
				ModioLog.Verbose?.Log("Waiting for requested Steam Encrypted App Ticket... \n" + $"Ticket: {_encryptedAppTicket} | Error: {_encryptedAppTicketError}");
				await Task.Yield();
			}
			if ((bool)_encryptedAppTicketError)
			{
				return ReturnErrorAndReset(new Error(_encryptedAppTicketError.Code, _encryptedAppTicketError.CustomMessage));
			}
			ModioLog.Verbose?.Log("Received Steam Encrypted App Ticket, requesting Mod.io authentication...");
			var (error2, accessTokenObject) = await ModioAPI.Authentication.AuthenticateViaSteam(new SteamAuthenticationRequest(_encryptedAppTicket, displayedTerms, thirdPartyEmail, 0L));
			if ((bool)error2)
			{
				return ReturnErrorAndReset(error2);
			}
			try
			{
				ModioLog.Verbose?.Log("Steam authentication successful, authenticating local user...");
				User.Current.OnAuthenticated(accessTokenObject.Value.AccessToken);
			}
			catch (Exception arg)
			{
				ModioLog.Error?.Log("Internal: Failed to deserialize user/token object from Steam AccessTokenObject and assign to UserData." + $"\n{arg}");
				return new Error(ErrorCode.INTERNAL_FAILED_TO_DESERIALIZE_OBJECT);
			}
			return ReturnErrorAndReset(Error.None);
		}
		return new Error(ErrorCode.USER_AUTHENTICATION_IN_PROGRESS);
	}

	private void OnGetEncryptedAppTicket(bool success, string encryptedAppTicketOrError)
	{
		ModioLog.Verbose?.Log($"Got Steam Encrypted App Ticket: {success} | Ticket: {encryptedAppTicketOrError} | " + $"attemptInProgress: {_isAttemptInProgress} | ");
		if (_isAttemptInProgress)
		{
			if (!success)
			{
				ModioLog.Error?.Log(encryptedAppTicketOrError);
				_encryptedAppTicketError = new Error(ErrorCode.STEAM_FAILED_TO_GET_APP_TICKET, encryptedAppTicketOrError);
			}
			else
			{
				_encryptedAppTicket = encryptedAppTicketOrError;
			}
		}
	}

	public void SetCredentialProvider(ISteamCredentialProvider credentialProvider)
	{
		_credentialProvider = credentialProvider;
	}

	private Error ValidateAttempt()
	{
		if (_credentialProvider != null)
		{
			return Error.None;
		}
		ModioLog.Error?.Log($"{typeof(ModioSteamAuthService)} cannot authenticate as no Steam Credential Provider" + " has been set! Call ModioSteamAuthService.SetCredentialProvider before calling Authenticate or use a constructor that takes a Credential Provider parameter..");
		return new Error(ErrorCode.NOT_INITIALIZED);
	}

	private Error ReturnErrorAndReset(Error error)
	{
		_encryptedAppTicketError = Error.None;
		_encryptedAppTicket = null;
		_isAttemptInProgress = false;
		return error;
	}
}
