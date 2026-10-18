namespace Unity.Collections;

internal sealed class BitField64DebugView
{
	private BitField64 Data;

	public bool[] Bits
	{
		get
		{
			bool[] array = new bool[64];
			for (int i = 0; i < 64; i++)
			{
				array[i] = Data.IsSet(i);
			}
			return array;
		}
	}

	public BitField64DebugView(BitField64 data)
	{
		Data = data;
	}
}
