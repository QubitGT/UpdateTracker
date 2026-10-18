using System;

public struct OVRSpaceUser : IDisposable
{
	internal ulong _handle;

	public bool Valid
	{
		get
		{
			if (_handle != 0L)
			{
				return Id != 0;
			}
			return false;
		}
	}

	public ulong Id
	{
		get
		{
			if (_handle != 0L)
			{
				if (!OVRPlugin.GetSpaceUserId(_handle, out var spaceUserId))
				{
					return 0uL;
				}
				return spaceUserId;
			}
			return 0uL;
		}
	}

	public static bool TryCreate(ulong platformUserId, out OVRSpaceUser spaceUser)
	{
		spaceUser = default(OVRSpaceUser);
		return OVRPlugin.CreateSpaceUser(platformUserId, out spaceUser._handle);
	}

	public static bool TryCreate(string platformUserId, out OVRSpaceUser spaceUser)
	{
		if (ulong.TryParse(platformUserId, out var result))
		{
			return TryCreate(result, out spaceUser);
		}
		spaceUser = default(OVRSpaceUser);
		return false;
	}

	[Obsolete("Constructor ignores validation. Use TryCreate(*) methods instead.", false)]
	public OVRSpaceUser(ulong spaceUserId)
	{
		OVRPlugin.CreateSpaceUser(spaceUserId, out _handle);
	}

	public void Dispose()
	{
		if (_handle != 0L)
		{
			OVRPlugin.DestroySpaceUser(_handle);
			_handle = 0uL;
		}
	}
}
