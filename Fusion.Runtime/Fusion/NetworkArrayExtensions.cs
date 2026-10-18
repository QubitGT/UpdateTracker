using System;

namespace Fusion;

public static class NetworkArrayExtensions
{
	public static int IndexOf<T>(this NetworkArray<T> array, T elem) where T : unmanaged, IEquatable<T>
	{
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].Equals(elem))
			{
				return i;
			}
		}
		return -1;
	}

	public static ref T GetRef<T>(this NetworkArray<T> array, int index) where T : unmanaged
	{
		return ref array.GetRef(index);
	}
}
