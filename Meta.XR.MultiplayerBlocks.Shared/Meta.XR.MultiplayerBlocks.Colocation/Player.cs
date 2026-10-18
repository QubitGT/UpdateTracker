using System;

namespace Meta.XR.MultiplayerBlocks.Colocation;

[Serializable]
internal struct Player : IEquatable<Player>
{
	public ulong playerId;

	public ulong oculusId;

	public uint colocationGroupId;

	public Player(ulong playerId, ulong oculusId, uint colocationGroupId)
	{
		this.playerId = playerId;
		this.oculusId = oculusId;
		this.colocationGroupId = colocationGroupId;
	}

	public bool Equals(Player other)
	{
		if (playerId == other.playerId && oculusId == other.oculusId)
		{
			return colocationGroupId == other.colocationGroupId;
		}
		return false;
	}
}
