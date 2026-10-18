namespace Liv.Lck.Core;

public enum CoreError
{
	InternalError,
	MissingTrackingId,
	InvalidArgument,
	UserNotLoggedIn,
	FailedToCacheCosmetics,
	ServiceUnavailable,
	RateLimiterBackoff,
	LoginAttemptExpired,
	InvalidTrackingId
}
