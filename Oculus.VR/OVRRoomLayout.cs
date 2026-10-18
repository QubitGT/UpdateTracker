using System;
using System.Collections.Generic;

public readonly struct OVRRoomLayout : IOVRAnchorComponent<OVRRoomLayout>, IEquatable<OVRRoomLayout>
{
	public static readonly OVRRoomLayout Null;

	OVRPlugin.SpaceComponentType IOVRAnchorComponent<OVRRoomLayout>.Type => Type;

	ulong IOVRAnchorComponent<OVRRoomLayout>.Handle => Handle;

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

	internal OVRPlugin.SpaceComponentType Type => OVRPlugin.SpaceComponentType.RoomLayout;

	internal ulong Handle { get; }

	OVRRoomLayout IOVRAnchorComponent<OVRRoomLayout>.FromAnchor(OVRAnchor anchor)
	{
		return new OVRRoomLayout(anchor);
	}

	OVRTask<bool> IOVRAnchorComponent<OVRRoomLayout>.SetEnabledAsync(bool enabled, double timeout)
	{
		throw new NotSupportedException("The RoomLayout component cannot be enabled or disabled.");
	}

	public bool Equals(OVRRoomLayout other)
	{
		return Handle == other.Handle;
	}

	public static bool operator ==(OVRRoomLayout lhs, OVRRoomLayout rhs)
	{
		return lhs.Equals(rhs);
	}

	public static bool operator !=(OVRRoomLayout lhs, OVRRoomLayout rhs)
	{
		return !lhs.Equals(rhs);
	}

	public override bool Equals(object obj)
	{
		if (obj is OVRRoomLayout other)
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
		return $"{Handle}.RoomLayout";
	}

	private OVRRoomLayout(OVRAnchor anchor)
	{
		Handle = anchor.Handle;
	}

	[Obsolete("Use FetchAnchorsAsync instead.")]
	public OVRTask<bool> FetchLayoutAnchorsAsync(List<OVRAnchor> anchors)
	{
		if (!OVRPlugin.GetSpaceRoomLayout(Handle, out var roomLayout))
		{
			throw new InvalidOperationException("Could not get Room Layout");
		}
		List<Guid> list;
		using (new OVRObjectPool.ListScope<Guid>(out list))
		{
			list.Add(roomLayout.floorUuid);
			list.Add(roomLayout.ceilingUuid);
			list.AddRange(roomLayout.wallUuids);
			return OVRAnchor.FetchAnchorsAsync(list, anchors);
		}
	}

	public OVRTask<OVRResult<List<OVRAnchor>, OVRAnchor.FetchResult>> FetchAnchorsAsync(List<OVRAnchor> anchors)
	{
		if (anchors == null)
		{
			throw new ArgumentNullException("anchors");
		}
		if (!OVRPlugin.GetSpaceRoomLayout(Handle, out var roomLayout))
		{
			throw new InvalidOperationException("Could not get Room Layout");
		}
		List<Guid> list;
		using (new OVRObjectPool.ListScope<Guid>(out list))
		{
			list.Add(roomLayout.floorUuid);
			list.Add(roomLayout.ceilingUuid);
			list.AddRange(roomLayout.wallUuids);
			return OVRAnchor.FetchAnchorsAsync(anchors, new OVRAnchor.FetchOptions
			{
				Uuids = list
			});
		}
	}

	public bool TryGetRoomLayout(out Guid ceiling, out Guid floor, out Guid[] walls)
	{
		ceiling = Guid.Empty;
		floor = Guid.Empty;
		walls = null;
		if (!OVRPlugin.GetSpaceRoomLayout(Handle, out var roomLayout))
		{
			return false;
		}
		ceiling = roomLayout.ceilingUuid;
		floor = roomLayout.floorUuid;
		walls = roomLayout.wallUuids;
		return true;
	}
}
