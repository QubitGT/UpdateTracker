using System;
using Meta.XR.MultiplayerBlocks.Colocation;

namespace Meta.XR.MultiplayerBlocks.Shared;

internal struct NetworkBootstrapperParams
{
	public ulong myPlayerId;

	public ulong myOculusId;

	public OVRCameraRig ovrCameraRig;

	public SharedAnchorManager sharedAnchorManager;

	public AutomaticColocationLauncher colocationLauncher;

	public ColocationController colocationController;

	public Action setupColocationReadyEvents;
}
