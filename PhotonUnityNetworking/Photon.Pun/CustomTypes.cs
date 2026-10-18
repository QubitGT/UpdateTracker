using ExitGames.Client.Photon;
using Photon.Realtime;

namespace Photon.Pun;

internal static class CustomTypes
{
	public static readonly byte[] memPlayer = new byte[4];

	internal static void Register()
	{
		PhotonPeer.RegisterType(typeof(Player), 80, SerializePhotonPlayer, DeserializePhotonPlayer);
	}

	private static short SerializePhotonPlayer(StreamBuffer outStream, object customobject)
	{
		int actorNumber = ((Player)customobject).ActorNumber;
		lock (memPlayer)
		{
			byte[] array = memPlayer;
			int targetOffset = 0;
			Protocol.Serialize(actorNumber, array, ref targetOffset);
			outStream.Write(array, 0, 4);
			return 4;
		}
	}

	private static object DeserializePhotonPlayer(StreamBuffer inStream, short length)
	{
		if (length != 4)
		{
			return null;
		}
		int value;
		lock (memPlayer)
		{
			inStream.Read(memPlayer, 0, length);
			int offset = 0;
			Protocol.Deserialize(out value, memPlayer, ref offset);
		}
		if (PhotonNetwork.CurrentRoom != null)
		{
			return PhotonNetwork.CurrentRoom.GetPlayer(value);
		}
		return null;
	}
}
