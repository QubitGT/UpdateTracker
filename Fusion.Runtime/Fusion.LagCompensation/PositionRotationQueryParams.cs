using System;

namespace Fusion.LagCompensation;

[Serializable]
public struct PositionRotationQueryParams
{
	public QueryParams QueryParams;

	public Hitbox Hitbox;

	public PositionRotationQueryParams(QueryParams queryParams, Hitbox hitbox)
	{
		QueryParams = queryParams;
		Hitbox = hitbox;
	}
}
