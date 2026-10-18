using System;
using Unity.Collections;
using UnityEngine;

public readonly struct OVRBounded2D : IOVRAnchorComponent<OVRBounded2D>, IEquatable<OVRBounded2D>
{
	public static readonly OVRBounded2D Null;

	OVRPlugin.SpaceComponentType IOVRAnchorComponent<OVRBounded2D>.Type => Type;

	ulong IOVRAnchorComponent<OVRBounded2D>.Handle => Handle;

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

	internal OVRPlugin.SpaceComponentType Type => OVRPlugin.SpaceComponentType.Bounded2D;

	internal ulong Handle { get; }

	public Rect BoundingBox
	{
		get
		{
			if (!OVRPlugin.GetSpaceBoundingBox2D(Handle, out var rect))
			{
				throw new InvalidOperationException("Could not get BoundingBox");
			}
			return ConvertRect(rect);
		}
	}

	OVRBounded2D IOVRAnchorComponent<OVRBounded2D>.FromAnchor(OVRAnchor anchor)
	{
		return new OVRBounded2D(anchor);
	}

	OVRTask<bool> IOVRAnchorComponent<OVRBounded2D>.SetEnabledAsync(bool enabled, double timeout)
	{
		throw new NotSupportedException("The Bounded2D component cannot be enabled or disabled.");
	}

	public bool Equals(OVRBounded2D other)
	{
		return Handle == other.Handle;
	}

	public static bool operator ==(OVRBounded2D lhs, OVRBounded2D rhs)
	{
		return lhs.Equals(rhs);
	}

	public static bool operator !=(OVRBounded2D lhs, OVRBounded2D rhs)
	{
		return !lhs.Equals(rhs);
	}

	public override bool Equals(object obj)
	{
		if (obj is OVRBounded2D other)
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
		return $"{Handle}.Bounded2D";
	}

	private OVRBounded2D(OVRAnchor anchor)
	{
		Handle = anchor.Handle;
	}

	private Rect ConvertRect(OVRPlugin.Rectf openXrRect)
	{
		Vector2 size = openXrRect.Size.FromSizef();
		Vector2 position = openXrRect.Pos.FromFlippedXVector2f();
		position.x -= size.x;
		return new Rect(position, size);
	}

	public bool TryGetBoundaryPointsCount(out int count)
	{
		return OVRPlugin.GetSpaceBoundary2DCount(Handle, out count);
	}

	public bool TryGetBoundaryPoints(NativeArray<Vector2> positions)
	{
		if (!positions.IsCreated)
		{
			throw new ArgumentException("NativeArray is not created", "positions");
		}
		if (!OVRPlugin.GetSpaceBoundary2D(Handle, positions, out var count))
		{
			return false;
		}
		int num = 0;
		int num2 = count - 1;
		while (num <= num2)
		{
			Vector2 vector = positions[num2];
			Vector2 vector2 = positions[num];
			positions[num] = new Vector2(0f - vector.x, vector.y);
			positions[num2] = new Vector2(0f - vector2.x, vector2.y);
			num++;
			num2--;
		}
		return true;
	}
}
