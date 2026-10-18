using System;
using UnityEngine;

[Serializable]
public struct OVRPose
{
	public Vector3 position;

	public Quaternion orientation;

	public static OVRPose identity
	{
		get
		{
			OVRPose result = default(OVRPose);
			result.position = Vector3.zero;
			result.orientation = Quaternion.identity;
			return result;
		}
	}

	public override bool Equals(object obj)
	{
		if (obj is OVRPose)
		{
			return this == (OVRPose)obj;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return position.GetHashCode() ^ orientation.GetHashCode();
	}

	public static bool operator ==(OVRPose x, OVRPose y)
	{
		if (x.position == y.position)
		{
			return x.orientation == y.orientation;
		}
		return false;
	}

	public static bool operator !=(OVRPose x, OVRPose y)
	{
		return !(x == y);
	}

	public static OVRPose operator *(OVRPose lhs, OVRPose rhs)
	{
		OVRPose result = default(OVRPose);
		result.position = lhs.position + lhs.orientation * rhs.position;
		result.orientation = lhs.orientation * rhs.orientation;
		return result;
	}

	public OVRPose Inverse()
	{
		OVRPose result = default(OVRPose);
		result.orientation = Quaternion.Inverse(orientation);
		result.position = result.orientation * -position;
		return result;
	}

	public OVRPose flipZ()
	{
		OVRPose result = this;
		result.position.z = 0f - result.position.z;
		result.orientation.z = 0f - result.orientation.z;
		result.orientation.w = 0f - result.orientation.w;
		return result;
	}

	public OVRPlugin.Posef ToPosef_Legacy()
	{
		OVRPlugin.Posef result = default(OVRPlugin.Posef);
		result.Position = position.ToVector3f();
		result.Orientation = orientation.ToQuatf();
		return result;
	}

	public OVRPlugin.Posef ToPosef()
	{
		OVRPlugin.Posef result = default(OVRPlugin.Posef);
		result.Position.x = position.x;
		result.Position.y = position.y;
		result.Position.z = 0f - position.z;
		result.Orientation.x = 0f - orientation.x;
		result.Orientation.y = 0f - orientation.y;
		result.Orientation.z = orientation.z;
		result.Orientation.w = orientation.w;
		return result;
	}

	public OVRPose Rotate180AlongX()
	{
		OVRPose result = this;
		result.orientation *= Quaternion.Euler(180f, 0f, 0f);
		return result;
	}
}
