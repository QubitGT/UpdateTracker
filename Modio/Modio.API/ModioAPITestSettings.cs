using System.Text.RegularExpressions;

namespace Modio.API;

public class ModioAPITestSettings : IModioServiceSettings
{
	public bool FakeDisconnected;

	public string FakeDisconnectedOnEndpointRegex;

	public float FakeDisconnectedTimeoutDuration;

	public bool RateLimitError;

	public string RateLimitOnEndpointRegex;

	public bool ShouldFakeDisconnected(string url)
	{
		if (FakeDisconnected)
		{
			return true;
		}
		if (!string.IsNullOrEmpty(FakeDisconnectedOnEndpointRegex))
		{
			return Regex.IsMatch(url, FakeDisconnectedOnEndpointRegex);
		}
		return false;
	}

	public bool ShouldFakeRateLimit(string url)
	{
		if (RateLimitError)
		{
			return true;
		}
		if (!string.IsNullOrEmpty(RateLimitOnEndpointRegex))
		{
			return Regex.IsMatch(url, RateLimitOnEndpointRegex);
		}
		return false;
	}
}
