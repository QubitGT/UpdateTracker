namespace Liv.Lck.Core;

internal enum CosmeticsReturnCode : uint
{
	Ok,
	Panic,
	FailedToRetrieveState,
	InvalidArgument,
	BackendError,
	FailedToCacheCosmetics,
	FailedToNotifyOnCosmeticAvailable,
	MutexLockError,
	Unauthorized
}
