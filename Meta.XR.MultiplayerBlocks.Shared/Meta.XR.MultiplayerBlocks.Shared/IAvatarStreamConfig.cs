namespace Meta.XR.MultiplayerBlocks.Shared;

public interface IAvatarStreamConfig
{
	void SetAvatarStreamLOD(AvatarStreamLOD lod);

	void SetAvatarUpdateIntervalInS(float interval);
}
