namespace g3;

public struct IntTagPair
{
	public byte type;

	public int value;

	public int intValue => (type << 24) | value;

	public IntTagPair(byte type, int value)
	{
		this.type = type;
		this.value = value;
	}

	public IntTagPair(int combined)
	{
		type = (byte)(combined >> 24);
		value = combined & 0xFFFFFF;
	}
}
