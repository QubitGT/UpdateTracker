using Meta.WitAi.Data.Configuration;

namespace Meta.WitAi;

public class WitAuthUtility
{
	public class DefaultTokenValidatorProvider : ITokenValidationProvider
	{
		public bool IsTokenValid(string appId, string token)
		{
			return IsServerTokenValid(token);
		}

		public bool IsServerTokenValid(string serverToken)
		{
			if (serverToken != null)
			{
				return serverToken.Length == 32;
			}
			return false;
		}
	}

	public interface ITokenValidationProvider
	{
		bool IsTokenValid(string appId, string token);

		bool IsServerTokenValid(string serverToken);
	}

	private static string serverToken;

	public static ITokenValidationProvider tokenValidator = new DefaultTokenValidatorProvider();

	public const string SERVER_TOKEN_ID = "SharedServerToken";

	public static string ServerToken => "";

	public static bool IsServerTokenValid()
	{
		return tokenValidator.IsServerTokenValid(ServerToken);
	}

	public static bool IsServerTokenValid(string token)
	{
		return tokenValidator.IsServerTokenValid(token);
	}

	public static string GetAppServerToken(WitConfiguration configuration, string defaultValue = "")
	{
		return GetAppServerToken(configuration.GetApplicationId(), defaultValue);
	}

	public static string GetAppServerToken(string appId, string defaultServerToken = "")
	{
		return "";
	}

	public static string GetAppId(string serverToken, string defaultAppID = "")
	{
		return "";
	}

	public static void SetAppServerToken(string appId, string token)
	{
	}
}
