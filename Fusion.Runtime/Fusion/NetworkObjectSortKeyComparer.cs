using System.Collections.Generic;

namespace Fusion;

public class NetworkObjectSortKeyComparer : IComparer<NetworkObject>
{
	public static readonly NetworkObjectSortKeyComparer Instance = new NetworkObjectSortKeyComparer();

	public int Compare(NetworkObject x, NetworkObject y)
	{
		return x.SortKey.CompareTo(y.SortKey);
	}
}
