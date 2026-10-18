using System;
using System.Runtime.InteropServices;
using Fusion;

namespace Meta.XR.MultiplayerBlocks.Colocation.Fusion;

[StructLayout(LayoutKind.Explicit, Size = 280)]
[NetworkStructWeaved(70)]
internal struct FusionShareAndLocalizeParams : INetworkStruct
{
	[FieldOffset(0)]
	public ulong requestingPlayerId;

	[FieldOffset(8)]
	public ulong requestingPlayerOculusId;

	[FieldOffset(16)]
	public NetworkString<_64> anchorUUID;

	[FieldOffset(276)]
	public NetworkBool anchorFlowSucceeded;

	public FusionShareAndLocalizeParams(ShareAndLocalizeParams data)
	{
		requestingPlayerId = data.requestingPlayerId;
		requestingPlayerOculusId = data.requestingPlayerOculusId;
		anchorUUID = data.anchorUUID.ToString();
		anchorFlowSucceeded = data.anchorFlowSucceeded;
	}

	public ShareAndLocalizeParams GetShareAndLocalizeParams()
	{
		if (!Guid.TryParse(anchorUUID.ToString(), out var result))
		{
			Logger.Log("Failed to parse shared Anchor UUID string from network", LogLevel.Error);
		}
		return new ShareAndLocalizeParams(requestingPlayerId, requestingPlayerOculusId, result, anchorFlowSucceeded);
	}
}
