using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Liv.Lck.Core;

public static class InteropUtilities
{
	public static IReadOnlyCollection<IntPtr> AllocateUnmanagedStringPointers(IEnumerable<string> strings, Encoding targetEncoding)
	{
		List<IntPtr> list = new List<IntPtr>();
		foreach (string @string in strings)
		{
			byte[] bytes = targetEncoding.GetBytes(@string + "\0");
			IntPtr intPtr = Marshal.AllocHGlobal(bytes.Length);
			Marshal.Copy(bytes, 0, intPtr, bytes.Length);
			list.Add(intPtr);
		}
		return list;
	}

	public static IntPtr AllocateUnmanagedArray(IReadOnlyCollection<IntPtr> ptrs)
	{
		IntPtr intPtr = Marshal.AllocHGlobal(IntPtr.Size * ptrs.Count);
		for (int i = 0; i < ptrs.Count; i++)
		{
			Marshal.WriteIntPtr(intPtr, i * IntPtr.Size, ptrs.ElementAt(i));
		}
		return intPtr;
	}

	public static byte[] CopyUnmanagedByteArray(IntPtr byteArrayStartPtr, int byteArrayLength)
	{
		byte[] array = new byte[byteArrayLength];
		Marshal.Copy(byteArrayStartPtr, array, 0, byteArrayLength);
		return array;
	}

	public static string UTF8PointerToString(IntPtr ptr)
	{
		if (ptr == IntPtr.Zero)
		{
			return null;
		}
		int i;
		for (i = 0; Marshal.ReadByte(ptr, i) != 0; i++)
		{
		}
		byte[] array = new byte[i];
		Marshal.Copy(ptr, array, 0, i);
		return Encoding.UTF8.GetString(array);
	}

	public static IntPtr StringToUTF8Pointer(string str)
	{
		if (str == null)
		{
			return IntPtr.Zero;
		}
		byte[] bytes = Encoding.UTF8.GetBytes(str + "\0");
		IntPtr intPtr = Marshal.AllocHGlobal(bytes.Length);
		Marshal.Copy(bytes, 0, intPtr, bytes.Length);
		return intPtr;
	}

	public static void Free(IntPtr ptr)
	{
		Marshal.FreeHGlobal(ptr);
	}
}
