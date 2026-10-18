namespace Unity.Collections;

internal sealed class BitField32DebugView
{
	private BitField32 BitField;

	public bool[] Bits
	{
		get
		{
			bool[] array = new bool[32];
			for (int i = 0; i < 32; i++)
			{
				array[i] = BitField.IsSet(i);
			}
			return array;
		}
	}

	public BitField32DebugView(BitField32 bitfield)
	{
		BitField = bitfield;
	}
}
