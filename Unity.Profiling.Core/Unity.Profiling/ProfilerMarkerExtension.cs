using System.Diagnostics;
using System.Runtime.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Profiling.LowLevel.Unsafe;

namespace Unity.Profiling;

public static class ProfilerMarkerExtension
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Conditional("ENABLE_PROFILER")]
	public unsafe static void Begin(this ProfilerMarker marker, int metadata)
	{
		ProfilerMarkerData profilerMarkerData = default(ProfilerMarkerData);
		profilerMarkerData.Type = 2;
		profilerMarkerData.Size = (uint)UnsafeUtility.SizeOf<int>();
		profilerMarkerData.Ptr = &metadata;
		ProfilerMarkerData profilerMarkerData2 = profilerMarkerData;
		ProfilerUnsafeUtility.BeginSampleWithMetadata(marker.Handle, 1, &profilerMarkerData2);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Conditional("ENABLE_PROFILER")]
	public unsafe static void Begin(this ProfilerMarker marker, uint metadata)
	{
		ProfilerMarkerData profilerMarkerData = default(ProfilerMarkerData);
		profilerMarkerData.Type = 3;
		profilerMarkerData.Size = (uint)UnsafeUtility.SizeOf<uint>();
		profilerMarkerData.Ptr = &metadata;
		ProfilerMarkerData profilerMarkerData2 = profilerMarkerData;
		ProfilerUnsafeUtility.BeginSampleWithMetadata(marker.Handle, 1, &profilerMarkerData2);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Conditional("ENABLE_PROFILER")]
	public unsafe static void Begin(this ProfilerMarker marker, long metadata)
	{
		ProfilerMarkerData profilerMarkerData = default(ProfilerMarkerData);
		profilerMarkerData.Type = 4;
		profilerMarkerData.Size = (uint)UnsafeUtility.SizeOf<long>();
		profilerMarkerData.Ptr = &metadata;
		ProfilerMarkerData profilerMarkerData2 = profilerMarkerData;
		ProfilerUnsafeUtility.BeginSampleWithMetadata(marker.Handle, 1, &profilerMarkerData2);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Conditional("ENABLE_PROFILER")]
	public unsafe static void Begin(this ProfilerMarker marker, ulong metadata)
	{
		ProfilerMarkerData profilerMarkerData = default(ProfilerMarkerData);
		profilerMarkerData.Type = 5;
		profilerMarkerData.Size = (uint)UnsafeUtility.SizeOf<ulong>();
		profilerMarkerData.Ptr = &metadata;
		ProfilerMarkerData profilerMarkerData2 = profilerMarkerData;
		ProfilerUnsafeUtility.BeginSampleWithMetadata(marker.Handle, 1, &profilerMarkerData2);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Conditional("ENABLE_PROFILER")]
	public unsafe static void Begin(this ProfilerMarker marker, float metadata)
	{
		ProfilerMarkerData profilerMarkerData = default(ProfilerMarkerData);
		profilerMarkerData.Type = 6;
		profilerMarkerData.Size = (uint)UnsafeUtility.SizeOf<float>();
		profilerMarkerData.Ptr = &metadata;
		ProfilerMarkerData profilerMarkerData2 = profilerMarkerData;
		ProfilerUnsafeUtility.BeginSampleWithMetadata(marker.Handle, 1, &profilerMarkerData2);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Conditional("ENABLE_PROFILER")]
	public unsafe static void Begin(this ProfilerMarker marker, double metadata)
	{
		ProfilerMarkerData profilerMarkerData = default(ProfilerMarkerData);
		profilerMarkerData.Type = 7;
		profilerMarkerData.Size = (uint)UnsafeUtility.SizeOf<double>();
		profilerMarkerData.Ptr = &metadata;
		ProfilerMarkerData profilerMarkerData2 = profilerMarkerData;
		ProfilerUnsafeUtility.BeginSampleWithMetadata(marker.Handle, 1, &profilerMarkerData2);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Conditional("ENABLE_PROFILER")]
	public unsafe static void Begin(this ProfilerMarker marker, string metadata)
	{
		ProfilerMarkerData profilerMarkerData = default(ProfilerMarkerData);
		profilerMarkerData.Type = 9;
		ProfilerMarkerData profilerMarkerData2 = profilerMarkerData;
		fixed (char* ptr = metadata)
		{
			profilerMarkerData2.Size = (uint)((metadata.Length + 1) * 2);
			profilerMarkerData2.Ptr = ptr;
			ProfilerUnsafeUtility.BeginSampleWithMetadata(marker.Handle, 1, &profilerMarkerData2);
		}
	}
}
