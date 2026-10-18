using System;

namespace Meta.WitAi.Data;

[Serializable]
public class AudioEncoding
{
	public enum Endian
	{
		Big,
		Little
	}

	public int numChannels = 1;

	public int samplerate = 16000;

	public string encoding = "signed-integer";

	public const string ENCODING_SIGNED = "signed-integer";

	public const string ENCODING_UNSIGNED = "unsigned-integer";

	public int bits = 16;

	public const int BITS_BYTE = 8;

	public const int BITS_SHORT = 16;

	public const int BITS_INT = 32;

	public const int BITS_LONG = 64;

	public Endian endian = Endian.Little;

	public override string ToString()
	{
		return $"audio/raw;bits={bits};rate={samplerate / 1000}k;encoding={encoding};endian={endian.ToString().ToLower()}";
	}
}
