using System;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization;

namespace Meta.XR.MultiplayerBlocks.Shared;

internal static class SerializationUtils
{
	internal static string SerializeToString<T>(T obj)
	{
		if (obj == null)
		{
			return null;
		}
		DataContractSerializer dataContractSerializer = new DataContractSerializer(typeof(T));
		using MemoryStream memoryStream = new MemoryStream();
		dataContractSerializer.WriteObject(memoryStream, obj);
		return Convert.ToBase64String(Compress(memoryStream.ToArray()));
	}

	internal static T DeserializeFromString<T>(string base64)
	{
		byte[] array = Decompress(Convert.FromBase64String(base64));
		using MemoryStream memoryStream = new MemoryStream();
		DataContractSerializer dataContractSerializer = new DataContractSerializer(typeof(T));
		memoryStream.Write(array, 0, array.Length);
		memoryStream.Seek(0L, SeekOrigin.Begin);
		return (T)dataContractSerializer.ReadObject(memoryStream);
	}

	private static byte[] Compress(byte[] data)
	{
		using MemoryStream memoryStream = new MemoryStream();
		using (DeflateStream deflateStream = new DeflateStream(memoryStream, CompressionMode.Compress))
		{
			deflateStream.Write(data, 0, data.Length);
		}
		return memoryStream.ToArray();
	}

	private static byte[] Decompress(byte[] data)
	{
		using MemoryStream memoryStream = new MemoryStream();
		using (DeflateStream deflateStream = new DeflateStream(new MemoryStream(data), CompressionMode.Decompress))
		{
			deflateStream.CopyTo(memoryStream);
		}
		return memoryStream.ToArray();
	}
}
