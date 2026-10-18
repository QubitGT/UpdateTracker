namespace Fusion.Photon.Realtime;

internal enum JoinMode : byte
{
	Default,
	CreateIfNotExists,
	JoinOrRejoin,
	RejoinOnly
}
