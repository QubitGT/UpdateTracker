using System;

namespace Oculus.Platform.Models;

public class BlockedUser
{
	public readonly ulong Id;

	public BlockedUser(IntPtr o)
	{
		Id = CAPI.ovr_BlockedUser_GetId(o);
	}
}
