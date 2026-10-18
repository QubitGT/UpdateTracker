using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Unity.Cinemachine;

[StructLayout(LayoutKind.Sequential, Size = 1)]
internal struct LocMinSorter : IComparer<LocalMinima>
{
	public int Compare(LocalMinima locMin1, LocalMinima locMin2)
	{
		return locMin2.vertex.pt.Y.CompareTo(locMin1.vertex.pt.Y);
	}
}
