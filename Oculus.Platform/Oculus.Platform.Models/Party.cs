using System;

namespace Oculus.Platform.Models;

public class Party
{
	public readonly ulong ID;

	public readonly UserList InvitedUsersOptional;

	[Obsolete("Deprecated in favor of InvitedUsersOptional")]
	public readonly UserList InvitedUsers;

	public readonly User LeaderOptional;

	[Obsolete("Deprecated in favor of LeaderOptional")]
	public readonly User Leader;

	public readonly UserList UsersOptional;

	[Obsolete("Deprecated in favor of UsersOptional")]
	public readonly UserList Users;

	public Party(IntPtr o)
	{
		ID = CAPI.ovr_Party_GetID(o);
		IntPtr intPtr = CAPI.ovr_Party_GetInvitedUsers(o);
		InvitedUsers = new UserList(intPtr);
		if (intPtr == IntPtr.Zero)
		{
			InvitedUsersOptional = null;
		}
		else
		{
			InvitedUsersOptional = InvitedUsers;
		}
		IntPtr intPtr2 = CAPI.ovr_Party_GetLeader(o);
		Leader = new User(intPtr2);
		if (intPtr2 == IntPtr.Zero)
		{
			LeaderOptional = null;
		}
		else
		{
			LeaderOptional = Leader;
		}
		IntPtr intPtr3 = CAPI.ovr_Party_GetUsers(o);
		Users = new UserList(intPtr3);
		if (intPtr3 == IntPtr.Zero)
		{
			UsersOptional = null;
		}
		else
		{
			UsersOptional = Users;
		}
	}
}
