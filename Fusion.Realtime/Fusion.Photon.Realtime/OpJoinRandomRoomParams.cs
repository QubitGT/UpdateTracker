using ExitGames.Client.Photon;

namespace Fusion.Photon.Realtime;

internal class OpJoinRandomRoomParams
{
	public Hashtable ExpectedCustomRoomProperties;

	public int ExpectedMaxPlayers;

	public MatchmakingMode MatchingType;

	public TypedLobby TypedLobby;

	public string SqlLobbyFilter;

	public string[] ExpectedUsers;

	public object Ticket;
}
