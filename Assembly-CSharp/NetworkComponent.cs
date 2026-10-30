using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;

public abstract class NetworkComponent : NetworkView, IPunObservable, IOnPhotonViewOwnerChange, IPhotonViewCallback, IInRoomCallbacks, IPunInstantiateMagicCallback
{
	public bool IsLocallyOwned => base.IsMine;

	public bool ShouldWriteObjectData => NetworkSystem.Instance.ShouldWriteObjectData(base.gameObject);

	public bool ShouldUpdateobject => NetworkSystem.Instance.ShouldUpdateObject(base.gameObject);

	public int OwnerID => NetworkSystem.Instance.GetOwningPlayerID(base.gameObject);

	internal virtual void OnEnable()
	{
		AddToNetwork();
	}

	internal virtual void OnDisable()
	{
		PhotonNetwork.RemoveCallbackTarget(this);
	}

	protected override void Start()
	{
		base.Start();
		AddToNetwork();
	}

	private void AddToNetwork()
	{
		PhotonNetwork.AddCallbackTarget(this);
	}

	public virtual void OnPhotonInstantiate(PhotonMessageInfo info)
	{
		OnSpawned();
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.IsWriting)
		{
			WriteDataPUN(stream, info);
		}
		else if (stream.IsReading)
		{
			ReadDataPUN(stream, info);
		}
	}

	protected abstract void WriteDataPUN(PhotonStream stream, PhotonMessageInfo info);

	protected abstract void ReadDataPUN(PhotonStream stream, PhotonMessageInfo info);

	public virtual void OnSpawned()
	{
	}

	protected virtual void OnOwnerSwitched(NetPlayer newOwningPlayer)
	{
	}

	void IInRoomCallbacks.OnMasterClientSwitched(Player newMasterClient)
	{
		OnOwnerSwitched(NetworkSystem.Instance.GetPlayer(newMasterClient));
	}

	void IInRoomCallbacks.OnPlayerEnteredRoom(Player newPlayer)
	{
	}

	void IInRoomCallbacks.OnPlayerLeftRoom(Player otherPlayer)
	{
	}

	void IInRoomCallbacks.OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
	{
	}

	void IInRoomCallbacks.OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
	{
	}

	public virtual void OnOwnerChange(Player newOwner, Player previousOwner)
	{
	}
}
