using System;
using UnityEngine;

namespace Fusion.LagCompensation;

[Serializable]
public struct RaycastQueryParams
{
	public QueryParams QueryParams;

	public Vector3 Origin;

	public Vector3 Direction;

	public float Length;

	public int StaticHitsCapacity;

	public RaycastQueryParams(QueryParams queryParams, Vector3 origin, Vector3 direction, float length, int staticHitsCapacity = 64)
	{
		QueryParams = queryParams;
		Origin = origin;
		Direction = direction;
		Length = length;
		StaticHitsCapacity = staticHitsCapacity;
	}
}
