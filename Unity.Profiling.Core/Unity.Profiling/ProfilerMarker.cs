using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Unity.Profiling;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public readonly struct ProfilerMarker<TP1> where TP1 : unmanaged
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public readonly struct AutoScope : IDisposable
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal AutoScope(ProfilerMarker<TP1> marker, TP1 p1)
		{
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispose()
		{
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ProfilerMarker(string name, string param1Name)
	{
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ProfilerMarker(ProfilerCategory category, string name, string param1Name)
	{
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Conditional("ENABLE_PROFILER")]
	public void Begin(TP1 p1)
	{
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Conditional("ENABLE_PROFILER")]
	public void End()
	{
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public AutoScope Auto(TP1 p1)
	{
		return default(AutoScope);
	}
}
[StructLayout(LayoutKind.Sequential, Size = 1)]
public readonly struct ProfilerMarker<TP1, TP2> where TP1 : unmanaged where TP2 : unmanaged
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public readonly struct AutoScope : IDisposable
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal AutoScope(ProfilerMarker<TP1, TP2> marker, TP1 p1, TP2 p2)
		{
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispose()
		{
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ProfilerMarker(string name, string param1Name, string param2Name)
	{
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ProfilerMarker(ProfilerCategory category, string name, string param1Name, string param2Name)
	{
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Conditional("ENABLE_PROFILER")]
	public void Begin(TP1 p1, TP2 p2)
	{
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Conditional("ENABLE_PROFILER")]
	public void End()
	{
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public AutoScope Auto(TP1 p1, TP2 p2)
	{
		return default(AutoScope);
	}
}
[StructLayout(LayoutKind.Sequential, Size = 1)]
public readonly struct ProfilerMarker<TP1, TP2, TP3> where TP1 : unmanaged where TP2 : unmanaged where TP3 : unmanaged
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public readonly struct AutoScope : IDisposable
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal AutoScope(ProfilerMarker<TP1, TP2, TP3> marker, TP1 p1, TP2 p2, TP3 p3)
		{
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispose()
		{
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ProfilerMarker(string name, string param1Name, string param2Name, string param3Name)
	{
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ProfilerMarker(ProfilerCategory category, string name, string param1Name, string param2Name, string param3Name)
	{
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Conditional("ENABLE_PROFILER")]
	public void Begin(TP1 p1, TP2 p2, TP3 p3)
	{
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Conditional("ENABLE_PROFILER")]
	public void End()
	{
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public AutoScope Auto(TP1 p1, TP2 p2, TP3 p3)
	{
		return default(AutoScope);
	}
}
