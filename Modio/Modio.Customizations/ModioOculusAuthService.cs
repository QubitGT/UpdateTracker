using System;
using System.Threading.Tasks;
using Modio.API;
using Modio.API.SchemaDefinitions;
using Modio.Authentication;
using Modio.Errors;
using Modio.Users;

namespace Modio.Customizations;

public class ModioOculusAuthService : IModioAuthService, IGetActiveUserIdentifier, IPotentialModioEmailAuthService
{
	private IOculusCredentialProvider _credentialProvider;

	private bool _isAttemptInProgress;

	public bool IsEmailPlatform => false;

	public ModioAPI.Portal Portal => ModioAPI.Portal.None;

	public Task<string> GetActiveUserIdentifier()
	{
		return Task.FromResult("oculus_user");
	}

	public ModioOculusAuthService(IOculusCredentialProvider credentialProvider)
	{
		_credentialProvider = credentialProvider;
	}

	public ModioOculusAuthService()
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
			ModioLog.Verbose?.Log("Starting Oculus Authentication, requesting ...");
			var (error2, oculusUserId) = await _credentialProvider.GetOculusUserId();
			if ((bool)error2)
			{
				return ReturnErrorAndReset(error2);
			}
			string oculusAccessToken = await _credentialProvider.GetOculusAccessToken();
			string nonce = await _credentialProvider.GetOculusUserProof();
			string oculusDevice = _credentialProvider.GetOculusDevice();
			var (error3, accessTokenObject) = await ModioAPI.Authentication.AuthenticateViaOculus(new MetaQuestAuthenticationRequest(oculusDevice, nonce, oculusUserId, oculusAccessToken, termsAgreed: true, thirdPartyEmail, 0L));
			if ((bool)error3)
			{
				return ReturnErrorAndReset(error3);
			}
			try
			{
				ModioLog.Verbose?.Log("Oculus authentication successful, authenticating local user...");
				User.Current.OnAuthenticated(accessTokenObject.Value.AccessToken);
			}
			catch (Exception arg)
			{
				ModioLog.Error?.Log("Internal: Failed to deserialize user/token object from Oculus AccessTokenObject and assign to UserData." + $"\n{arg}");
				return new Error(ErrorCode.INTERNAL_FAILED_TO_DESERIALIZE_OBJECT);
			}
			return ReturnErrorAndReset(Error.None);
		}
		return new Error(ErrorCode.USER_AUTHENTICATION_IN_PROGRESS);
	}

	public void SetCredentialProvider(IOculusCredentialProvider credentialProvider)
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
		_isAttemptInProgress = false;
		return error;
	}
}
