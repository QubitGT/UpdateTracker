using System;
using UnityEngine;

public readonly struct OVRBounded3D : IOVRAnchorComponent<OVRBounded3D>, IEquatable<OVRBounded3D>
{
	public static readonly OVRBounded3D Null;

	OVRPlugin.SpaceComponentType IOVRAnchorComponent<OVRBounded3D>.Type => Type;

	ulong IOVRAnchorComponent<OVRBounded3D>.Handle => Handle;

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

	internal OVRPlugin.SpaceComponentType Type => OVRPlugin.SpaceComponentType.Bounded3D;

	internal ulong Handle { get; }

	public Bounds BoundingBox
	{
		get
		{
			if (!OVRPlugin.GetSpaceBoundingBox3D(Handle, out var bounds))
			{
				throw new InvalidOperationException("Could not get BoundingBox");
			}
			return ConvertBounds(bounds);
		}
	}

	OVRBounded3D IOVRAnchorComponent<OVRBounded3D>.FromAnchor(OVRAnchor anchor)
	{
		return new OVRBounded3D(anchor);
	}

	OVRTask<bool> IOVRAnchorComponent<OVRBounded3D>.SetEnabledAsync(bool enabled, double timeout)
	{
		throw new NotSupportedException("The Bounded3D component cannot be enabled or disabled.");
	}

	public bool Equals(OVRBounded3D other)
	{
		return Handle == other.Handle;
	}

	public static bool operator ==(OVRBounded3D lhs, OVRBounded3D rhs)
	{
		return lhs.Equals(rhs);
	}

	public static bool operator !=(OVRBounded3D lhs, OVRBounded3D rhs)
	{
		return !lhs.Equals(rhs);
	}

	public override bool Equals(object obj)
	{
		if (obj is OVRBounded3D other)
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
		return $"{Handle}.Bounded3D";
	}

	private OVRBounded3D(OVRAnchor anchor)
	{
		Handle = anchor.Handle;
	}

	private Bounds ConvertBounds(OVRPlugin.Boundsf openXrBounds)
	{
		Vector3 vector = openXrBounds.Size.FromSize3f();
		Vector3 vector2 = openXrBounds.Pos.FromFlippedXVector3f();
		vector2.x -= vector.x;
		Vector3 vector3 = vector * 0.5f;
		return new Bounds(vector2 + vector3, vector);
	}
}
