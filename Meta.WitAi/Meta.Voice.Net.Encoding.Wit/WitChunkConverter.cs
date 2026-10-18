using System;
using System.Text;
using Lib.Wit.Runtime.Utilities.Logging;
using Meta.Voice.Logging;
using Meta.WitAi;
using Meta.WitAi.Json;
using UnityEngine;

namespace Meta.Voice.Net.Encoding.Wit;

[LogCategory(LogCategory.Encoding)]
public class WitChunkConverter : ILogSource
{
	private static readonly UTF8Encoding TextEncoding = new UTF8Encoding();

	private WitChunk _currentChunk;

	private int _headerDecoded;

	private byte[] _headerBytes = new byte[17];

	private int _jsonDecoded;

	private StringBuilder _jsonBuilder = new StringBuilder();

	private ulong _binaryDecoded;

	private const int FLAG_SIZE = 1;

	private const int LONG_SIZE = 8;

	private const int HEADER_SIZE = 17;

	private const byte FLAG_NO_JSON_NO_BINARY = 0;

	private const byte FLAG_NO_JSON_YES_BINARY = 1;

	private const byte FLAG_YES_JSON_NO_BINARY = 2;

	private const byte FLAG_YES_JSON_YES_BINARY = 3;

	public IVLogger Logger { get; } = LoggerRegistry.Instance.GetLogger(LogCategory.Encoding);


	private bool IsHeaderDecoded => _headerDecoded >= 17;

	private bool IsJsonDecoded => _jsonDecoded >= _currentChunk.header.jsonLength;

	private bool IsBinaryDecoded => _binaryDecoded >= _currentChunk.header.binaryLength;

	private void ResetChunk()
	{
		_headerDecoded = 0;
		_jsonDecoded = 0;
		_jsonBuilder.Clear();
		_binaryDecoded = 0uL;
		_currentChunk.jsonString = null;
		_currentChunk.jsonData = null;
		_currentChunk.binaryData = null;
	}

	public void Decode(byte[] buffer, int bufferOffset, int bufferLength, Action<WitChunk> onChunkDecoded, Action<byte[], int, int> customBinaryDecoder = null)
	{
		while (bufferLength > 0)
		{
			int num = DecodeChunk(buffer, bufferOffset, bufferLength, onChunkDecoded, customBinaryDecoder);
			bufferOffset += num;
			bufferLength -= num;
		}
	}

	private int DecodeChunk(byte[] buffer, int bufferOffset, int bufferLength, Action<WitChunk> onChunkDecoded, Action<byte[], int, int> customBinaryDecoder)
	{
		int num = 0;
		if (!IsHeaderDecoded)
		{
			num = DecodeHeader(buffer, bufferOffset, bufferLength);
			if (!IsHeaderDecoded)
			{
				return num;
			}
			bufferOffset += num;
			bufferLength -= num;
			if (_currentChunk.header.invalid)
			{
				Logger.Error("WitChunk Header Decode Failed: Header is invalid\nHeader: {0}", WitRequestSettings.GetByteString(_headerBytes, 0, 17));
				ResetChunk();
				return num;
			}
			if (customBinaryDecoder == null)
			{
				byte[] binaryData = _currentChunk.binaryData;
				int num2 = ((binaryData != null) ? binaryData.Length : 0);
				int num3 = (int)_currentChunk.header.binaryLength;
				if (num2 != num3)
				{
					_currentChunk.binaryData = new byte[num3];
				}
			}
		}
		if (!IsJsonDecoded)
		{
			int num4 = DecodeJson(buffer, bufferOffset, bufferLength);
			num += num4;
			bufferOffset += num4;
			bufferLength -= num4;
			if (IsJsonDecoded && customBinaryDecoder != null)
			{
				onChunkDecoded?.Invoke(_currentChunk);
			}
		}
		if (!IsBinaryDecoded)
		{
			int num5 = DecodeBinary(buffer, bufferOffset, bufferLength, customBinaryDecoder);
			num += num5;
		}
		if (IsJsonDecoded && IsBinaryDecoded)
		{
			if (customBinaryDecoder == null)
			{
				onChunkDecoded?.Invoke(_currentChunk);
			}
			ResetChunk();
		}
		return num;
	}

	private int DecodeHeader(byte[] buffer, int bufferOffset, int bufferLength)
	{
		int headerDecoded = _headerDecoded;
		int b = 17 - _headerDecoded;
		int num = Mathf.Min(bufferLength, b);
		Array.Copy(buffer, bufferOffset, _headerBytes, headerDecoded, num);
		_headerDecoded += num;
		if (IsHeaderDecoded)
		{
			_currentChunk.header = GetHeader(_headerBytes, 0);
		}
		return num;
	}

	private int DecodeJson(byte[] buffer, int bufferOffset, int bufferLength)
	{
		int jsonDecoded = _jsonDecoded;
		int b = _currentChunk.header.jsonLength - jsonDecoded;
		int num = Mathf.Min(bufferLength, b);
		if (num <= 0)
		{
			return 0;
		}
		string value = DecodeString(buffer, bufferOffset, num);
		_jsonBuilder.Append(value);
		_jsonDecoded += num;
		if (IsJsonDecoded)
		{
			string jsonString = _jsonBuilder.ToString();
			_currentChunk.jsonString = jsonString;
			_currentChunk.jsonData = JsonConvert.DeserializeToken(jsonString);
		}
		return num;
	}

	private int DecodeBinary(byte[] buffer, int bufferOffset, int bufferLength, Action<byte[], int, int> customBinaryDecoder)
	{
		ulong binaryDecoded = _binaryDecoded;
		ulong num = _currentChunk.header.binaryLength - binaryDecoded;
		int num2 = Mathf.Min(bufferLength, (int)num);
		if (num2 <= 0)
		{
			return 0;
		}
		if (customBinaryDecoder != null)
		{
			customBinaryDecoder(buffer, bufferOffset, num2);
		}
		else if (_currentChunk.binaryData != null)
		{
			Array.Copy(buffer, bufferOffset, _currentChunk.binaryData, (int)_binaryDecoded, num2);
		}
		_binaryDecoded += (ulong)num2;
		return num2;
	}

	public static string DecodeString(byte[] rawData, int offset, int length)
	{
		return TextEncoding.GetString(rawData, offset, length);
	}

	public static byte[] Encode(WitChunk chunkData)
	{
		if (string.IsNullOrEmpty(chunkData.jsonString))
		{
			chunkData.jsonString = chunkData.jsonData?.ToString();
		}
		return Encode(chunkData.jsonString, chunkData.binaryData);
	}

	public static byte[] Encode(byte[] binaryData)
	{
		return Encode((byte[])null, binaryData);
	}

	public static byte[] Encode(WitResponseNode jsonToken, byte[] binaryData = null)
	{
		return Encode(jsonToken?.ToString(), binaryData);
	}

	public static byte[] Encode(string jsonString, byte[] binaryData = null)
	{
		return Encode(EncodeString(jsonString), binaryData);
	}

	public static byte[] Encode(byte[] jsonData, byte[] binaryData)
	{
		int num = ((jsonData != null) ? jsonData.Length : 0);
		int num2 = ((binaryData != null) ? binaryData.Length : 0);
		byte[] array = new byte[17 + num + num2];
		array[0] = EncodeFlag(num > 0, num2 > 0);
		int offset = 1;
		EncodeLength(array, ref offset, num);
		EncodeLength(array, ref offset, num2);
		EncodeBytes(array, ref offset, jsonData);
		EncodeBytes(array, ref offset, binaryData);
		return array;
	}

	public static byte[] EncodeString(string stringData)
	{
		if (!string.IsNullOrEmpty(stringData))
		{
			return TextEncoding.GetBytes(stringData);
		}
		return null;
	}

	private static byte EncodeFlag(bool hasJson, bool hasBinary)
	{
		if (hasJson)
		{
			if (!hasBinary)
			{
				return 2;
			}
			return 3;
		}
		if (!hasBinary)
		{
			return 0;
		}
		return 1;
	}

	private static void EncodeLength(byte[] destination, ref int offset, long length)
	{
		byte[] bytes = BitConverter.GetBytes(length);
		EncodeBytes(destination, ref offset, bytes);
	}

	private static void EncodeBytes(byte[] destination, ref int offset, byte[] source)
	{
		if (source != null && source.Length != 0)
		{
			Array.Copy(source, 0, destination, offset, source.Length);
			offset += source.Length;
		}
	}

	private static WitChunkHeader GetHeader(byte[] bytes, int offset)
	{
		WitChunkHeader result = default(WitChunkHeader);
		byte b = bytes[offset];
		bool flag = (b & 1) != 0;
		bool flag2 = (SafeShift(b, 1) & 1) != 0;
		result.invalid = (SafeShift(b, 2) & 0x3F) != 0;
		long num = BitConverter.ToInt64(bytes, offset + 1);
		result.jsonLength = (int)num;
		result.invalid |= num < 0 && flag2;
		result.invalid |= num > 0 && !flag2;
		long num2 = (long)(result.binaryLength = (ulong)BitConverter.ToInt64(bytes, offset + 1 + 8));
		result.invalid |= num2 < 0 && flag;
		result.invalid |= num2 > 0 && !flag;
		return result;
	}

	private static int SafeShift(byte flags, int index)
	{
		if (!BitConverter.IsLittleEndian)
		{
			return flags << index;
		}
		return flags >> index;
	}
}
