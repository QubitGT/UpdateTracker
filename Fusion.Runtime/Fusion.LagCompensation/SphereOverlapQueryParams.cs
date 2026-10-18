using System;
using UnityEngine;

namespace Fusion.LagCompensation;

[Serializable]
public struct SphereOverlapQueryParams
{
	public QueryParams QueryParams;

	public Vector3 Center;

	public float Radius;

	public int StaticHitsCapacity;

	public SphereOverlapQueryParams(QueryParams queryParams, Vector3 center, float radius, int staticHitsCapacity)
	{
		QueryParams = queryParams;
		Center = center;
		Radius = radius;
		StaticHitsCapacity = staticHitsCapacity;
	}
}
