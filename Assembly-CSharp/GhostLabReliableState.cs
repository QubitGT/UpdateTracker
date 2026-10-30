using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

[AddComponentMenu("GorillaTag/Environment/Ghost Lab Reliable State")]
public class GhostLabReliableState : NetworkComponent
{
	public GhostLab.EntranceDoorsState doorState;

	public int singleDoorCount;

	public bool[] singleDoorOpen;

	protected override void Awake()
	{
		base.Awake();
		singleDoorOpen = new bool[singleDoorCount];
	}

	public override void OnOwnerChange(Player newOwner, Player previousOwner)
	{
		base.OnOwnerChange(newOwner, previousOwner);
		_ = PhotonNetwork.LocalPlayer;
	}

	protected override void WriteDataPUN(PhotonStream stream, PhotonMessageInfo info)
	{
		if (base.IsMine || info.Sender.IsMasterClient)
		{
			stream.SendNext(doorState);
			for (int i = 0; i < singleDoorOpen.Length; i++)
			{
				stream.SendNext(singleDoorOpen[i]);
			}
		}
	}

	protected override void ReadDataPUN(PhotonStream stream, PhotonMessageInfo info)
	{
		if (base.IsMine || info.Sender.IsMasterClient)
		{
			doorState = (GhostLab.EntranceDoorsState)stream.ReceiveNext();
			for (int i = 0; i < singleDoorOpen.Length; i++)
			{
				singleDoorOpen[i] = (bool)stream.ReceiveNext();
			}
		}
	}

	public void UpdateEntranceDoorsState(GhostLab.EntranceDoorsState newState)
	{
		if (!NetworkSystem.Instance.InRoom || NetworkSystem.Instance.IsMasterClient)
		{
			doorState = newState;
		}
		else if (NetworkSystem.Instance.InRoom && !NetworkSystem.Instance.IsMasterClient)
		{
			SendRPC("RemoteEntranceDoorState", RpcTarget.MasterClient, newState);
		}
	}

	public void UpdateSingleDoorState(int singleDoorIndex)
	{
		if (!NetworkSystem.Instance.InRoom || NetworkSystem.Instance.IsMasterClient)
		{
			singleDoorOpen[singleDoorIndex] = !singleDoorOpen[singleDoorIndex];
		}
		else if (NetworkSystem.Instance.InRoom && !NetworkSystem.Instance.IsMasterClient)
		{
			SendRPC("RemoteSingleDoorState", RpcTarget.MasterClient, singleDoorIndex);
		}
	}

	[PunRPC]
	public void RemoteEntranceDoorState(GhostLab.EntranceDoorsState newState, PhotonMessageInfo info)
	{
		MonkeAgent.IncrementRPCCall(info, "RemoteEntranceDoorState");
		if (base.IsMine)
		{
			doorState = newState;
		}
	}

	[PunRPC]
	public void RemoteSingleDoorState(int doorIndex, PhotonMessageInfo info)
	{
		MonkeAgent.IncrementRPCCall(info, "RemoteSingleDoorState");
		if (base.IsMine && doorIndex < singleDoorCount)
		{
			singleDoorOpen[doorIndex] = !singleDoorOpen[doorIndex];
		}
	}
}
