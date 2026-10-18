namespace Photon.Realtime;

public class TypedLobby
{
	public string Name;

	public LobbyType Type;

	public static readonly TypedLobby Default = new TypedLobby();

	public bool IsDefault => string.IsNullOrEmpty(Name);

	internal TypedLobby()
	{
	}

	public TypedLobby(string name, LobbyType type)
	{
		Name = name;
		Type = type;
	}

	public override string ToString()
	{
		return $"lobby '{Name}'[{Type}]";
	}
}
