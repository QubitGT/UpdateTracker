namespace Meta.Voice.Net.Encoding.Wit;

public struct WitChunkHeader
{
	public bool invalid;

	public int jsonLength;

	public ulong binaryLength;
}
