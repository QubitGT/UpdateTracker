using System;
using UnityEngine;

namespace GorillaTagScripts;

internal struct GorillaRigHelper : IComparable<GorillaRigHelper>
{
	public VRRig rig;

	public Transform rigTransform;

	public CosmeticsThrottler.RigDrawState state;

	public float sqrDistance;

	public bool behindCamera;

	public bool inactive;

	public bool IsExcluded => sqrDistance >= float.MaxValue;

	public int CompareTo(GorillaRigHelper other)
	{
		return sqrDistance.CompareTo(other.sqrDistance);
	}
}
