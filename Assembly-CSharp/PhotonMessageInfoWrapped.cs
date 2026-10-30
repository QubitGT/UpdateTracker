using Photon.Pun;

public struct PhotonMessageInfoWrapped
{
	public readonly int senderID;

	public readonly int sentTick;

	public readonly PhotonMessageInfo punInfo;

	public readonly NetPlayer Sender;

	public double SentServerTime => (double)(uint)sentTick / 1000.0;

	public PhotonMessageInfoWrapped(PhotonMessageInfo info)
	{
		senderID = info.Sender?.ActorNumber ?? (-1);
		Sender = NetPlayer.Get(info.Sender);
		sentTick = info.SentServerTimestamp;
		punInfo = info;
	}

	public PhotonMessageInfoWrapped(int playerID, int tick)
	{
		senderID = playerID;
		Sender = NetworkSystem.Instance.GetPlayer(senderID);
		sentTick = tick;
		punInfo = default(PhotonMessageInfo);
	}

	private PhotonMessageInfoWrapped(NetPlayer sender)
	{
		Sender = sender;
		senderID = sender.ActorNumber;
		sentTick = PhotonNetwork.ServerTimestamp;
		punInfo = default(PhotonMessageInfo);
	}

	public static implicit operator PhotonMessageInfoWrapped(PhotonMessageInfo info)
	{
		return new PhotonMessageInfoWrapped(info);
	}

	public static PhotonMessageInfoWrapped GetLocalDefault()
	{
		return new PhotonMessageInfoWrapped(NetworkSystem.Instance.LocalPlayer);
	}
}
