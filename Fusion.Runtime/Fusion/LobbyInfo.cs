namespace Fusion;

public class LobbyInfo
{
	public bool IsValid { get; internal set; }

	public string Name { get; internal set; }

	public string Region { get; internal set; }

	internal void Reset()
	{
		IsValid = false;
		Name = null;
		Region = null;
	}
}
