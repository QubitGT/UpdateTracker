using System.Collections.Generic;

namespace Fusion;

internal static class ListSpecialized
{
	public static int BinarySearchSpecialized(this List<NetworkId> list, NetworkId value)
	{
		int num = 0;
		int num2 = list.Count - 1;
		while (num <= num2)
		{
			int num3 = num + (num2 - num >> 1);
			int num4 = (int)(list[num3].Raw - value.Raw);
			if (num4 == 0)
			{
				return num3;
			}
			if (num4 < 0)
			{
				num = num3 + 1;
			}
			else
			{
				num2 = num3 - 1;
			}
		}
		return ~num;
	}

	public static bool AddUnique(this List<NetworkId> list, NetworkId value)
	{
		int num = list.BinarySearchSpecialized(value);
		if (num >= 0)
		{
			return false;
		}
		list.Insert(~num, value);
		return true;
	}

	public static bool RemoveUnique(this List<NetworkId> list, NetworkId value)
	{
		int num = list.BinarySearchSpecialized(value);
		if (num < 0)
		{
			return false;
		}
		list.RemoveAt(num);
		return true;
	}
}
