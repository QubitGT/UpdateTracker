using System;
using System.Collections.Generic;

public readonly struct OVRAnchorContainer : IOVRAnchorComponent<OVRAnchorContainer>, IEquatable<OVRAnchorContainer>
{
	public static readonly OVRAnchorContainer Null;

	OVRPlugin.SpaceComponentType IOVRAnchorComponent<OVRAnchorContainer>.Type => Type;

	ulong IOVRAnchorComponent<OVRAnchorContainer>.Handle => Handle;

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

	internal OVRPlugin.SpaceComponentType Type => OVRPlugin.SpaceComponentType.SpaceContainer;

	internal ulong Handle { get; }

	public Guid[] Uuids
	{
		get
		{
			if (!OVRPlugin.GetSpaceContainer(Handle, out var containerUuids))
			{
				throw new InvalidOperationException("Could not get Uuids");
			}
			return containerUuids;
		}
	}

	OVRAnchorContainer IOVRAnchorComponent<OVRAnchorContainer>.FromAnchor(OVRAnchor anchor)
	{
		return new OVRAnchorContainer(anchor);
	}

	OVRTask<bool> IOVRAnchorComponent<OVRAnchorContainer>.SetEnabledAsync(bool enabled, double timeout)
	{
		throw new NotSupportedException("The AnchorContainer component cannot be enabled or disabled.");
	}

	public bool Equals(OVRAnchorContainer other)
	{
		return Handle == other.Handle;
	}

	public static bool operator ==(OVRAnchorContainer lhs, OVRAnchorContainer rhs)
	{
		return lhs.Equals(rhs);
	}

	public static bool operator !=(OVRAnchorContainer lhs, OVRAnchorContainer rhs)
	{
		return !lhs.Equals(rhs);
	}

	public override bool Equals(object obj)
	{
		if (obj is OVRAnchorContainer other)
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
		return $"{Handle}.AnchorContainer";
	}

	private OVRAnchorContainer(OVRAnchor anchor)
	{
		Handle = anchor.Handle;
	}

	[Obsolete("Use FetchAnchorsAsync instead")]
	public OVRTask<bool> FetchChildrenAsync(List<OVRAnchor> anchors)
	{
		return OVRAnchor.FetchAnchorsAsync(Uuids, anchors);
	}

	public OVRTask<OVRResult<List<OVRAnchor>, OVRAnchor.FetchResult>> FetchAnchorsAsync(List<OVRAnchor> anchors)
	{
		return OVRAnchor.FetchAnchorsAsync(anchors, new OVRAnchor.FetchOptions
		{
			Uuids = Uuids
		});
	}
}
