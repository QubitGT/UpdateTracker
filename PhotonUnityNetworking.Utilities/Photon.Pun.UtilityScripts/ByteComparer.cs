using System.Collections.Generic;

namespace Photon.Pun.UtilityScripts;

public class ByteComparer : IComparer<byte>
{
	public int Compare(byte x, byte y)
	{
		if (x != y)
		{
			if (x >= y)
			{
				return 1;
			}
			return -1;
		}
		return 0;
	}
}
