using System;
using System.Runtime.InteropServices;
using Fusion;

namespace Meta.XR.MultiplayerBlocks.Colocation.Fusion;

[Serializable]
[StructLayout(LayoutKind.Explicit, Size = 280)]
[NetworkStructWeaved(70)]
internal struct FusionAnchor : INetworkStruct, IEquatable<FusionAnchor>
{
	[FieldOffset(0)]
	public NetworkBool isAutomaticAnchor;

	[FieldOffset(4)]
	public NetworkBool isAlignmentAnchor;

	[FieldOffset(8)]
	public ulong ownerOculusId;

	[FieldOffset(16)]
	public uint colocationGroupId;

	[FieldOffset(20)]
	public NetworkString<_64> automaticAnchorUuid;

	public FusionAnchor(Anchor anchor)
	{
		isAutomaticAnchor = anchor.isAutomaticAnchor;
		isAlignmentAnchor = anchor.isAlignmentAnchor;
		ownerOculusId = anchor.ownerOculusId;
		colocationGroupId = anchor.colocationGroupId;
		automaticAnchorUuid = anchor.automaticAnchorUuid.ToString();
	}

	public Anchor GetAnchor()
	{
		if (!Guid.TryParse(automaticAnchorUuid.ToString(), out var result))
		{
			Logger.Log("Failed to parse Anchor UUID string", LogLevel.Error);
		}
		return new Anchor(isAutomaticAnchor, isAlignmentAnchor, ownerOculusId, colocationGroupId, result);
	}

	public bool Equals(FusionAnchor other)
	{
		return GetAnchor().Equals(other.GetAnchor());
	}
}
