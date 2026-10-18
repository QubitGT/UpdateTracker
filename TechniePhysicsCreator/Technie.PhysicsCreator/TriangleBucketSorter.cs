using System.Collections.Generic;

namespace Technie.PhysicsCreator;

public class TriangleBucketSorter : IComparer<TriangleBucket>
{
	public int Compare(TriangleBucket lhs, TriangleBucket rhs)
	{
		if (lhs.Area < rhs.Area)
		{
			return 1;
		}
		if (lhs.Area > rhs.Area)
		{
			return -1;
		}
		return 0;
	}
}
