namespace Meta.XR.MultiplayerBlocks.Shared;

public interface ITransferOwnership
{
	void TransferOwnershipToLocalPlayer();

	bool HasOwnership();
}
