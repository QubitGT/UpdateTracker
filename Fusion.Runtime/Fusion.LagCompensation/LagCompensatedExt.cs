using System.Collections.Generic;
using UnityEngine;

namespace Fusion.LagCompensation;

public static class LagCompensatedExt
{
	public static void SortReference(this List<LagCompensatedHit> hits, Vector3 reference)
	{
		if (hits.Count > 0)
		{
			for (int i = 0; i < hits.Count; i++)
			{
				LagCompensatedHit value = hits[i];
				Vector3 point = hits[i].Point;
				point.x -= reference.x;
				point.y -= reference.y;
				point.z -= reference.z;
				value._sortAux = point.sqrMagnitude;
				hits[i] = value;
			}
			LagCompensatedHit.QuickSort(hits, 0, hits.Count - 1);
		}
	}

	public static void SortDistance(this List<LagCompensatedHit> hits)
	{
		if (hits.Count > 0)
		{
			LagCompensatedHit.QuickSortDistance(hits, 0, hits.Count - 1);
		}
	}
}
