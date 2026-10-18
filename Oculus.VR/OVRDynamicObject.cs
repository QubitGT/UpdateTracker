using System;

public readonly struct OVRDynamicObject : IOVRAnchorComponent<OVRDynamicObject>, IEquatable<OVRDynamicObject>
{
	public static readonly OVRDynamicObject Null;

	OVRPlugin.SpaceComponentType IOVRAnchorComponent<OVRDynamicObject>.Type => Type;

	ulong IOVRAnchorComponent<OVRDynamicObject>.Handle => Handle;

	public bool IsNull => Handle == 0;

	public bool IsEnabled
	{
		get
		{
			bool enabled = default(bool);
			bool changePending = default(bool);
			if (!IsNull && OVRPlugin.GetSpaceComponentStatus(Handle, Type, out enabled, out changePending) && enabled)
			{
				return !changePending;
			}
			return false;
		}
	}

	internal OVRPlugin.SpaceComponentType Type => OVRPlugin.SpaceComponentType.DynamicObject;

	internal ulong Handle { get; }

	public OVRAnchor.TrackableType TrackableType
	{
		get
		{
			if (OVRPlugin.GetSpaceDynamicObjectData(Handle, out var data).IsSuccess())
			{
				return (data.ClassType == OVRPlugin.DynamicObjectClass.Keyboard) ? OVRAnchor.TrackableType.Keyboard : OVRAnchor.TrackableType.None;
			}
			return OVRAnchor.TrackableType.None;
		}
	}

	OVRDynamicObject IOVRAnchorComponent<OVRDynamicObject>.FromAnchor(OVRAnchor anchor)
	{
		return new OVRDynamicObject(anchor);
	}

	OVRTask<bool> IOVRAnchorComponent<OVRDynamicObject>.SetEnabledAsync(bool enabled, double timeout)
	{
		throw new NotSupportedException("The DynamicObject component cannot be enabled or disabled.");
	}

	public bool Equals(OVRDynamicObject other)
	{
		return Handle == other.Handle;
	}

	public static bool operator ==(OVRDynamicObject lhs, OVRDynamicObject rhs)
	{
		return lhs.Equals(rhs);
	}

	public static bool operator !=(OVRDynamicObject lhs, OVRDynamicObject rhs)
	{
		return !lhs.Equals(rhs);
	}

	public override bool Equals(object obj)
	{
		if (obj is OVRDynamicObject other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Handle.GetHashCode() * 486187739 + ((int)Type).GetHashCode();
	}

	public override string ToString()
	{
		return $"{Handle}.DynamicObject";
	}

	private OVRDynamicObject(OVRAnchor anchor)
	{
		Handle = anchor.Handle;
	}
}
