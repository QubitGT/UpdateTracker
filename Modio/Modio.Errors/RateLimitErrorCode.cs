namespace Modio.Errors;

public enum RateLimitErrorCode : long
{
	RATELIMITED = 11008L,
	RATELIMITED_SAME_ENDPOINT
}
