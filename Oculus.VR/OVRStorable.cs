using System;

public readonly struct OVRStorable : IOVRAnchorComponent<OVRStorable>, IEquatable<OVRStorable>
{
	public static readonly OVRStorable Null;

	OVRPlugin.SpaceComponentType IOVRAnchorComponent<OVRStorable>.Type => Type;

	ulong IOVRAnchorComponent<OVRStorable>.Handle => Handle;

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

	internal OVRPlugin.SpaceComponentType Type => OVRPlugin.SpaceComponentType.Storable;

	internal ulong Handle { get; }

	OVRStorable IOVRAnchorComponent<OVRStorable>.FromAnchor(OVRAnchor anchor)
	{
		return new OVRStorable(anchor);
	}

	public OVRTask<bool> SetEnabledAsync(bool enabled, double timeout = 0.0)
	{
		if (!OVRPlugin.GetSpaceComponentStatus(Handle, Type, out var enabled2, out var changePending))
		{
			return OVRTask.FromResult(result: false);
		}
		if (changePending)
		{
			return OVRAnchor.CreateDeferredSpaceComponentStatusTask(Handle, Type, enabled, timeout);
		}
		ulong requestId;
		if (enabled2 != enabled)
		{
			return OVRTask.Build(OVRPlugin.SetSpaceComponentStatus(Handle, Type, enabled, timeout, out requestId), requestId).ToTask(failureValue: false);
		}
		return OVRTask.FromResult(result: true);
	}

	[Obsolete("Use SetEnabledAsync instead.")]
	public OVRTask<bool> SetEnabledSafeAsync(bool enabled, double timeout = 0.0)
	{
		return SetEnabledAsync(enabled, timeout);
	}

	public bool Equals(OVRStorable other)
	{
		return Handle == other.Handle;
	}

	public static bool operator ==(OVRStorable lhs, OVRStorable rhs)
	{
		return lhs.Equals(rhs);
	}

	public static bool operator !=(OVRStorable lhs, OVRStorable rhs)
	{
		return !lhs.Equals(rhs);
	}

	public override bool Equals(object obj)
	{
		if (obj is OVRStorable other)
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
		return $"{Handle}.Storable";
	}

	private OVRStorable(OVRAnchor anchor)
	{
		Handle = anchor.Handle;
	}
}
