public static class MothershipClientContext
{
	public static string MothershipId;

	public static string Token;

	public static bool IsClientLoggedIn()
	{
		if (!string.IsNullOrEmpty(MothershipId))
		{
			return !string.IsNullOrEmpty(Token);
		}
		return false;
	}

	public static void ForgetAllCredentials()
	{
		MothershipId = (Token = "");
	}
}
