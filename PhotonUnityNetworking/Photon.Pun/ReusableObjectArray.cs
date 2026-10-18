using System.Collections.Generic;

namespace Photon.Pun;

internal static class ReusableObjectArray
{
	private static readonly Dictionary<int, object[]> m_bag = new Dictionary<int, object[]>(10);

	internal static object[] GetArray(int size)
	{
		if (m_bag.TryGetValue(size, out var value))
		{
			return value;
		}
		value = new object[size];
		m_bag[size] = value;
		return value;
	}
}
