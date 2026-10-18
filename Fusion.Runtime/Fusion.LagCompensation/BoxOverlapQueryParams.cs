using System;
using UnityEngine;

namespace Fusion.LagCompensation;

[Serializable]
public struct BoxOverlapQueryParams
{
	public QueryParams QueryParams;

	public Vector3 Center;

	public Vector3 Extents;

	public Quaternion Rotation;

	public int StaticHitsCapacity;

	public BoxOverlapQueryParams(QueryParams queryParams, Vector3 center, Vector3 extents, Quaternion rotation, int staticHitsCapacity)
	{
		QueryParams = queryParams;
		Center = center;
		Extents = extents;
		Rotation = rotation;
		StaticHitsCapacity = staticHitsCapacity;
	}
}
