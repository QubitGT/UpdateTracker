using System.Collections.Generic;
using UnityEngine;

namespace Technie.PhysicsCreator;

public class VolumeSorter : IComparer<RotatedBox>
{
	public int Compare(RotatedBox lhs, RotatedBox rhs)
	{
		if (Mathf.Approximately(lhs.volume, rhs.volume))
		{
			return 0;
		}
		if (lhs.volume > rhs.volume)
		{
			return 1;
		}
		return -1;
	}
}
