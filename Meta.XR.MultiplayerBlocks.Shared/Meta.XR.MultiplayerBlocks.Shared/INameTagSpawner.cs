namespace Meta.XR.MultiplayerBlocks.Shared;

public interface INameTagSpawner
{
	bool IsConnected { get; }

	void Spawn(string playerName);
}
