using System.Collections.Generic;

namespace Technie.PhysicsCreator;

public class TriangleAreaSorter : IComparer<Triangle>
{
	public int Compare(Triangle lhs, Triangle rhs)
	{
		if (lhs.area < rhs.area)
		{
			return 1;
		}
		if (lhs.area > rhs.area)
		{
			return -1;
		}
		return 0;
	}
}
