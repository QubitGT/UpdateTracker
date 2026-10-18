using System;

namespace Oculus.Platform.Models;

public class Challenge
{
	public readonly ChallengeCreationType CreationType;

	public readonly string Description;

	public readonly DateTime EndDate;

	public readonly ulong ID;

	public readonly UserList InvitedUsersOptional;

	[Obsolete("Deprecated in favor of InvitedUsersOptional")]
	public readonly UserList InvitedUsers;

	public readonly Leaderboard Leaderboard;

	public readonly UserList ParticipantsOptional;

	[Obsolete("Deprecated in favor of ParticipantsOptional")]
	public readonly UserList Participants;

	public readonly DateTime StartDate;

	public readonly string Title;

	public readonly ChallengeVisibility Visibility;

	public Challenge(IntPtr o)
	{
		CreationType = CAPI.ovr_Challenge_GetCreationType(o);
		Description = CAPI.ovr_Challenge_GetDescription(o);
		EndDate = CAPI.ovr_Challenge_GetEndDate(o);
		ID = CAPI.ovr_Challenge_GetID(o);
		IntPtr intPtr = CAPI.ovr_Challenge_GetInvitedUsers(o);
		InvitedUsers = new UserList(intPtr);
		if (intPtr == IntPtr.Zero)
		{
			InvitedUsersOptional = null;
		}
		else
		{
			InvitedUsersOptional = InvitedUsers;
		}
		Leaderboard = new Leaderboard(CAPI.ovr_Challenge_GetLeaderboard(o));
		IntPtr intPtr2 = CAPI.ovr_Challenge_GetParticipants(o);
		Participants = new UserList(intPtr2);
		if (intPtr2 == IntPtr.Zero)
		{
			ParticipantsOptional = null;
		}
		else
		{
			ParticipantsOptional = Participants;
		}
		StartDate = CAPI.ovr_Challenge_GetStartDate(o);
		Title = CAPI.ovr_Challenge_GetTitle(o);
		Visibility = CAPI.ovr_Challenge_GetVisibility(o);
	}
}
