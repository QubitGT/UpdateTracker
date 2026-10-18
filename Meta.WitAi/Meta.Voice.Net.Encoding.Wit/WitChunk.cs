using System;
using System.Linq;
using Meta.WitAi.Json;

namespace Meta.Voice.Net.Encoding.Wit;

public struct WitChunk
{
	public WitChunkHeader header;

	public string jsonString;

	public WitResponseNode jsonData;

	public byte[] binaryData;

	public override bool Equals(object other)
	{
		if (other is WitChunk other2)
		{
			return Equals(other2);
		}
		return false;
	}

	private bool Equals(WitChunk other)
	{
		if (header.Equals(other.header) && jsonString == other.jsonString && object.Equals(jsonData, other.jsonData))
		{
			return binaryData.SequenceEqual(other.binaryData);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(header, jsonString, jsonData, binaryData);
	}
}
