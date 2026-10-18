using System;
using System.Runtime.InteropServices;
using Fusion;

namespace Meta.XR.MultiplayerBlocks.Colocation.Fusion;

[Serializable]
[StructLayout(LayoutKind.Explicit, Size = 20)]
[NetworkStructWeaved(5)]
internal struct FusionPlayer : INetworkStruct, IEquatable<FusionPlayer>
{
	[FieldOffset(0)]
	public ulong playerId;

	[FieldOffset(8)]
	public ulong oculusId;

	[FieldOffset(16)]
	public uint colocationGroupId;

	public FusionPlayer(Player player)
	{
		playerId = player.playerId;
		oculusId = player.oculusId;
		colocationGroupId = player.colocationGroupId;
	}

	public Player GetPlayer()
	{
		return new Player(playerId, oculusId, colocationGroupId);
	}

	public bool Equals(FusionPlayer other)
	{
		return GetPlayer().Equals(other.GetPlayer());
	}
}
