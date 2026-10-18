using System.ComponentModel;

namespace Oculus.Platform;

public enum LaunchResult
{
	[Description("UNKNOWN")]
	Unknown,
	[Description("SUCCESS")]
	Success,
	[Description("FAILED_ROOM_FULL")]
	FailedRoomFull,
	[Description("FAILED_GAME_ALREADY_STARTED")]
	FailedGameAlreadyStarted,
	[Description("FAILED_ROOM_NOT_FOUND")]
	FailedRoomNotFound,
	[Description("FAILED_USER_DECLINED")]
	FailedUserDeclined,
	[Description("FAILED_OTHER_REASON")]
	FailedOtherReason
}
