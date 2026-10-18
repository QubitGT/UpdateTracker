using System.IO;
using System.IO.Compression;

namespace Fusion;

internal static class CompressionUtils
{
	public static byte[] Compress(byte[] data)
	{
		using MemoryStream memoryStream = new MemoryStream();
		using GZipStream gZipStream = new GZipStream(memoryStream, CompressionMode.Compress);
		gZipStream.Write(data, 0, data.Length);
		gZipStream.Close();
		return memoryStream.ToArray();
	}

	public static byte[] Decompress(byte[] data)
	{
		using MemoryStream stream = new MemoryStream(data);
		using GZipStream gZipStream = new GZipStream(stream, CompressionMode.Decompress);
		using MemoryStream memoryStream = new MemoryStream();
		gZipStream.CopyTo(memoryStream);
		return memoryStream.ToArray();
	}

	public unsafe static void SnapshotCompress(int* Current, int* Previous, int* Result, int totalLenght, out int count)
	{
		count = 0;
		for (int i = 0; i < totalLenght; i++)
		{
			if (Current[i] != Previous[i])
			{
				Result[count++] = i;
				Result[count++] = Current[i];
			}
		}
	}

	public unsafe static void SnapshotDecompress(int* previous, int* delta, int length)
	{
		int num = 0;
		while (num < length)
		{
			int num2 = delta[num++];
			int num3 = delta[num++];
			previous[num2] = num3;
		}
	}
}
