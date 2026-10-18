using System;

namespace SouthPointe.Serialization.MessagePack;

internal static class ArrayHelper
{
	internal static void AdjustSize(ref byte[] bytes, int length)
	{
		if (bytes.Length < length)
		{
			int num;
			for (num = bytes.Length; num < length; num *= 2)
			{
			}
			Array.Resize(ref bytes, num);
		}
	}
}
