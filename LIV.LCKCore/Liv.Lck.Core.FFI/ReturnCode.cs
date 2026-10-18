namespace Liv.Lck.Core.FFI;

internal enum ReturnCode : uint
{
	Ok,
	Error,
	Panic,
	InvalidArgument,
	BackendUnavailable,
	Uninitialized,
	BackendDataParsingError,
	BackendClientError,
	UserNotLoggedIn,
	NullPointer,
	LoginAttemptExpired,
	Fatal,
	RateLimiterBackoff,
	InvalidTrackingId
}
