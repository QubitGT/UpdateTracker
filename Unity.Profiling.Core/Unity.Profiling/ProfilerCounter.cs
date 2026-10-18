using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Unity.Profiling;

public readonly struct ProfilerCounter<T> where T : unmanaged
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ProfilerCounter(ProfilerCategory category, string name, ProfilerMarkerDataUnit dataUnit)
	{
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Conditional("ENABLE_PROFILER")]
	public void Sample(T value)
	{
	}
}
