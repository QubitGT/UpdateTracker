using Fusion;
using Meta.XR.MultiplayerBlocks.Shared;

namespace Meta.XR.MultiplayerBlocks.Fusion;

[NetworkBehaviourWeaved(0)]
public class TransferOwnershipFusion : NetworkBehaviour, ITransferOwnership
{
	public void TransferOwnershipToLocalPlayer()
	{
		base.Object.RequestStateAuthority();
	}

	public bool HasOwnership()
	{
		return base.HasStateAuthority;
	}

	[WeaverGenerated]
	public override void CopyBackingFieldsToState(bool P_0)
	{
	}

	[WeaverGenerated]
	public override void CopyStateToBackingFields()
	{
	}
}
