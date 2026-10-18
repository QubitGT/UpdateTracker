using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Unity.Profiling;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public readonly struct ProfilerCounterValue<T> where T : unmanaged
{
	public T Value
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return default(T);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ProfilerCounterValue(string name)
	{
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ProfilerCounterValue(string name, ProfilerMarkerDataUnit dataUnit)
	{
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ProfilerCounterValue(string name, ProfilerMarkerDataUnit dataUnit, ProfilerCounterOptions counterOptions)
	{
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ProfilerCounterValue(ProfilerCategory category, string name, ProfilerMarkerDataUnit dataUnit)
	{
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ProfilerCounterValue(ProfilerCategory category, string name, ProfilerMarkerDataUnit dataUnit, ProfilerCounterOptions counterOptions)
	{
	}

	[Conditional("ENABLE_PROFILER")]
	public void Sample()
	{
	}
}
