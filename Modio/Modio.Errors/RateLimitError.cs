namespace Modio.Errors;

public class RateLimitError : Error
{
	public readonly int RetryAfterSeconds;

	internal RateLimitError(RateLimitErrorCode code, int retryAfterSeconds)
		: base((ErrorCode)code)
	{
		RetryAfterSeconds = retryAfterSeconds;
	}
}
