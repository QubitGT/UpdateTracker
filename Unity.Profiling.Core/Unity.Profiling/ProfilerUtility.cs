using System;
using System.Runtime.InteropServices;

namespace Unity.Profiling;

[StructLayout(LayoutKind.Sequential, Size = 1)]
internal struct ProfilerUtility
{
	public static byte GetProfilerMarkerDataType<T>()
	{
		return Type.GetTypeCode(typeof(T)) switch
		{
			TypeCode.Int32 => 2, 
			TypeCode.UInt32 => 3, 
			TypeCode.Int64 => 4, 
			TypeCode.UInt64 => 5, 
			TypeCode.Single => 6, 
			TypeCode.Double => 7, 
			TypeCode.String => 9, 
			_ => throw new ArgumentException($"Type {typeof(T)} is unsupported by ProfilerCounter."), 
		};
	}
}
