namespace Meta.XR.MultiplayerBlocks.Shared;

public interface IAvatarBehaviour
{
	ulong OculusId { get; }

	int LocalAvatarIndex { get; }

	bool HasInputAuthority { get; }

	void ReceiveStreamData(byte[] bytes);
}
