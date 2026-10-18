using System;

public readonly struct OVRSpace : IEquatable<OVRSpace>
{
	[Obsolete("Anchor APIs no longer require a storage location.")]
	public enum StorageLocation
	{
		Local,
		Cloud
	}

	public ulong Handle { get; }

	public bool Valid => Handle != 0;

	public bool TryGetUuid(out Guid uuid)
	{
		return OVRPlugin.GetSpaceUuid(Handle, out uuid);
	}

	public OVRSpace(ulong handle)
	{
		Handle = handle;
	}

	public override string ToString()
	{
		return $"0x{Handle:x16}";
	}

	public bool Equals(OVRSpace other)
	{
		return Handle == other.Handle;
	}

	public override bool Equals(object obj)
	{
		if (obj is OVRSpace other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Handle.GetHashCode();
	}

	public static bool operator ==(OVRSpace lhs, OVRSpace rhs)
	{
		return lhs.Handle == rhs.Handle;
	}

	public static bool operator !=(OVRSpace lhs, OVRSpace rhs)
	{
		return lhs.Handle != rhs.Handle;
	}

	public static implicit operator OVRSpace(ulong handle)
	{
		return new OVRSpace(handle);
	}

	public static implicit operator ulong(OVRSpace space)
	{
		return space.Handle;
	}
}
