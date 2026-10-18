using System;
using System.Buffers;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace VYaml.Internal;

internal static class StreamHelper
{
	private const int ArrayMexLength = 2147483591;

	public static async ValueTask<ReusableByteSequenceBuilder> ReadAsSequenceAsync(Stream stream, CancellationToken cancellation = default(CancellationToken))
	{
		ReusableByteSequenceBuilder builder = ReusableByteSequenceBuilderPool.Rent();
		try
		{
			if (stream is MemoryStream memoryStream && memoryStream.TryGetBuffer(out var buffer2))
			{
				cancellation.ThrowIfCancellationRequested();
				memoryStream.Seek(buffer2.Count, SeekOrigin.Current);
				builder.Add(buffer2.AsMemory(), returnToPool: false);
				return builder;
			}
			byte[] buffer = ArrayPool<byte>.Shared.Rent(65536);
			int offset = 0;
			int num;
			do
			{
				if (offset == buffer.Length)
				{
					builder.Add(buffer, returnToPool: true);
					buffer = ArrayPool<byte>.Shared.Rent(NewArrayCapacity(buffer.Length));
					offset = 0;
				}
				try
				{
					num = await stream.ReadAsync(buffer.AsMemory(offset, buffer.Length - offset), cancellation).ConfigureAwait(continueOnCapturedContext: false);
				}
				catch
				{
					ArrayPool<byte>.Shared.Return(buffer);
					throw;
				}
				offset += num;
			}
			while (num != 0);
			builder.Add(buffer.AsMemory(0, offset), returnToPool: true);
		}
		catch (Exception)
		{
			ReusableByteSequenceBuilderPool.Return(builder);
			throw;
		}
		return builder;
	}

	private static int NewArrayCapacity(int size)
	{
		int num = size * 2;
		if ((uint)num > 2147483591u)
		{
			num = 2147483591;
		}
		return num;
	}
}
