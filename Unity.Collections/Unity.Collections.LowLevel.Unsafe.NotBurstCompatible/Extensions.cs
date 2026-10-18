using System;

namespace Unity.Collections.LowLevel.Unsafe.NotBurstCompatible;

public static class Extensions
{
	public static T[] ToArray<T>(this UnsafeParallelHashSet<T> set) where T : unmanaged, IEquatable<T>
	{
		NativeArray<T> nativeArray = set.ToNativeArray(Allocator.TempJob);
		T[] result = nativeArray.ToArray();
		nativeArray.Dispose();
		return result;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public unsafe static void AddNBC(this ref UnsafeAppendBuffer buffer, string value)
	{
		if (value != null)
		{
			buffer.Add(value.Length);
			fixed (char* ptr = value)
			{
				buffer.Add(ptr, 2 * value.Length);
			}
		}
		else
		{
			buffer.Add(-1);
		}
	}

	[ExcludeFromBurstCompatTesting("Returns managed array")]
	public unsafe static byte[] ToBytesNBC(this ref UnsafeAppendBuffer buffer)
	{
		byte[] array = new byte[buffer.Length];
		fixed (byte* destination = array)
		{
			UnsafeUtility.MemCpy(destination, buffer.Ptr, buffer.Length);
		}
		return array;
	}

	[ExcludeFromBurstCompatTesting("Managed string out argument")]
	public unsafe static void ReadNextNBC(this ref UnsafeAppendBuffer.Reader reader, out string value)
	{
		reader.ReadNext(out int value2);
		if (value2 != -1)
		{
			value = new string('0', value2);
			fixed (char* destination = value)
			{
				int num = value2 * 2;
				UnsafeUtility.MemCpy(destination, reader.ReadNext(num), num);
			}
		}
		else
		{
			value = null;
		}
	}
}
