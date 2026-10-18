using System;
using System.IO;

namespace Backtrace.Unity.Extensions;

internal static class StreamExtensions
{
	private const int _DefaultCopyBufferSize = 81920;

	public static void CopyTo(this Stream original, Stream destination)
	{
		if (destination == null)
		{
			throw new ArgumentNullException("destination");
		}
		if (!original.CanRead && !original.CanWrite)
		{
			throw new ObjectDisposedException("ObjectDisposedException");
		}
		if (!destination.CanRead && !destination.CanWrite)
		{
			throw new ObjectDisposedException("ObjectDisposedException");
		}
		if (!original.CanRead)
		{
			throw new NotSupportedException("NotSupportedException source");
		}
		if (!destination.CanWrite)
		{
			throw new NotSupportedException("NotSupportedException destination");
		}
		byte[] array = new byte[81920];
		int count;
		while ((count = original.Read(array, 0, array.Length)) != 0)
		{
			destination.Write(array, 0, count);
		}
	}
}
