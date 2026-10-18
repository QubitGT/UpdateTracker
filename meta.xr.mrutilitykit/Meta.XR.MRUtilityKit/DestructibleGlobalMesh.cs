using System;
using UnityEngine;

namespace Meta.XR.MRUtilityKit;

public struct DestructibleGlobalMesh
{
	public DestructibleMeshComponent DestructibleMeshComponent;

	public int MaxPointsCount;

	public float PointsPerUnitX;

	public float PointsPerUnitY;

	private bool Equals(DestructibleGlobalMesh other)
	{
		if (DestructibleMeshComponent == other.DestructibleMeshComponent && object.Equals(MaxPointsCount, other.MaxPointsCount) && Mathf.Approximately(PointsPerUnitX, other.PointsPerUnitX))
		{
			return Mathf.Approximately(PointsPerUnitY, other.PointsPerUnitY);
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is DestructibleGlobalMesh other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(DestructibleMeshComponent, MaxPointsCount, PointsPerUnitX, PointsPerUnitY);
	}

	public static bool operator ==(DestructibleGlobalMesh left, DestructibleGlobalMesh right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(DestructibleGlobalMesh left, DestructibleGlobalMesh right)
	{
		return !left.Equals(right);
	}
}
