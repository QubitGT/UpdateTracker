using System.Runtime.CompilerServices;

namespace Fusion;

internal static class ScheduledRequestsExt
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsSet(this ref ScheduledRequests requests, ScheduledRequests target)
	{
		return (requests & target) == target;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Set(this ref ScheduledRequests requests, ScheduledRequests target)
	{
		requests |= target;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Clear(this ref ScheduledRequests requests, ScheduledRequests target)
	{
		requests &= ~target;
	}
}
