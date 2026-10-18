using System;

namespace Fusion;

[Flags]
public enum RpcSendMessageResult
{
	None = 0,
	SentToServerForForwarding = 0x101,
	SentToTargetClient = 0x102,
	SentBroadcast = 0x503,
	[Obsolete("Not used anymore")]
	NotSentTargetObjectNotConfirmed = 0xA04,
	NotSentTargetObjectNotInPlayerInterest = 0xA05,
	NotSentTargetClientNotAvailable = 0x206,
	NotSentBroadcastNoActiveConnections = 0x607,
	NotSentBroadcastNoConfirmedNorInterestedClients = 0xE08,
	MaskSent = 0x100,
	MaskNotSent = 0x200,
	MaskBroadcast = 0x400,
	MaskCulled = 0x800
}
