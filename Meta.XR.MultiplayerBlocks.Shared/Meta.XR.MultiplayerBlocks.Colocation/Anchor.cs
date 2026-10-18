using System;

namespace Meta.XR.MultiplayerBlocks.Colocation;

[Serializable]
internal struct Anchor : IEquatable<Anchor>
{
	public bool isAutomaticAnchor;

	public bool isAlignmentAnchor;

	public ulong ownerOculusId;

	public uint colocationGroupId;

	public Guid automaticAnchorUuid;

	public Anchor(bool isAutomaticAnchor, bool isAlignmentAnchor, ulong ownerOculusId, uint colocationGroupId, Guid automaticAnchorUuid)
	{
		this.isAutomaticAnchor = isAutomaticAnchor;
		this.isAlignmentAnchor = isAlignmentAnchor;
		this.ownerOculusId = ownerOculusId;
		this.colocationGroupId = colocationGroupId;
		this.automaticAnchorUuid = automaticAnchorUuid;
	}

	public bool Equals(Anchor other)
	{
		if (isAutomaticAnchor == other.isAutomaticAnchor && isAlignmentAnchor == other.isAlignmentAnchor && ownerOculusId == other.ownerOculusId && colocationGroupId == other.colocationGroupId)
		{
			return automaticAnchorUuid == other.automaticAnchorUuid;
		}
		return false;
	}
}
