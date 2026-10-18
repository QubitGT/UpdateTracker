using System;
using System.Threading.Tasks;
using Modio.Errors;

namespace Modio.Customizations;

internal static class Wss
{
	public static async Task<(Error, ExternalAuthenticationToken)> BeginAuthenticationProcess(bool restartProcess = false)
	{
		(Error, WssDeviceLoginResponse) tuple = await WssHandler.DoMessageHandshake<WssDeviceLoginResponse>(WssRequest.DeviceLogin());
		if ((bool)tuple.Item1)
		{
			return (tuple.Item1, default(ExternalAuthenticationToken));
		}
		Task<(Error, WssLoginSuccess)> task = WaitForAccessToken();
		ExternalAuthenticationToken externalAuthenticationToken = default(ExternalAuthenticationToken);
		externalAuthenticationToken.code = tuple.Item2.code;
		externalAuthenticationToken.url = tuple.Item2.login_url;
		externalAuthenticationToken.autoUrl = tuple.Item2.login_url + "?code=" + tuple.Item2.code;
		externalAuthenticationToken.expiryTime = DateTimeOffset.FromUnixTimeSeconds(tuple.Item2.date_expires).DateTime;
		externalAuthenticationToken.task = task;
		externalAuthenticationToken.cancel = delegate
		{
			WssHandler.CancelWaitingFor("login_success");
		};
		ExternalAuthenticationToken item = externalAuthenticationToken;
		return (Error.None, item);
	}

	private static async Task<(Error, WssLoginSuccess)> WaitForAccessToken()
	{
		(Error, WssMessage) tuple = await WssHandler.WaitForMessage("login_success");
		WssLoginSuccess wssLoginSuccess = default(WssLoginSuccess);
		wssLoginSuccess.access_token = "";
		wssLoginSuccess.code = -1L;
		wssLoginSuccess.date_expires = -1L;
		WssLoginSuccess output = wssLoginSuccess;
		var (error, _) = tuple;
		if (!error)
		{
			error = ((!tuple.Item2.TryGetValue<WssLoginSuccess>(out output)) ? new Error(ErrorCode.WSS_UNEXPECTED_MESSAGE) : Error.None);
		}
		WssHandler.Shutdown();
		return (error, output);
	}
}
