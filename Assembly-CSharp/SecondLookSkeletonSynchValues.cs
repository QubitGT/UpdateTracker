using GorillaExtensions;
using Photon.Pun;
using UnityEngine;

[AddComponentMenu("GorillaTag/Environment/Second Look Skeleton Synch Values")]
public class SecondLookSkeletonSynchValues : NetworkComponent
{
	public SecondLookSkeleton.GhostState currentState;

	public Vector3 position;

	public Quaternion rotation;

	public SecondLookSkeleton mySkeleton;

	public int currentNode;

	public int nextNode;

	public int angerPoint;

	protected override void OnOwnerSwitched(NetPlayer newOwningPlayer)
	{
		base.OnOwnerSwitched(newOwningPlayer);
		if (newOwningPlayer.IsLocal)
		{
			mySkeleton.SetNodes();
			if (mySkeleton.currentState != currentState)
			{
				mySkeleton.ChangeState(currentState);
			}
		}
	}

	protected override void WriteDataPUN(PhotonStream stream, PhotonMessageInfo info)
	{
		if (base.IsMine || info.Sender.IsMasterClient)
		{
			stream.SendNext(mySkeleton.currentState);
			stream.SendNext(mySkeleton.spookyGhost.transform.position);
			stream.SendNext(mySkeleton.spookyGhost.transform.rotation);
			stream.SendNext(currentNode);
			stream.SendNext(nextNode);
			stream.SendNext(angerPoint);
		}
	}

	protected override void ReadDataPUN(PhotonStream stream, PhotonMessageInfo info)
	{
		if (base.IsMine || info.Sender.IsMasterClient)
		{
			currentState = (SecondLookSkeleton.GhostState)stream.ReceiveNext();
			ref Vector3 v = ref position;
			Vector3 newVal = (Vector3)stream.ReceiveNext();
			v.SetValueSafe(in newVal);
			ref Quaternion q = ref rotation;
			Quaternion newVal2 = (Quaternion)stream.ReceiveNext();
			q.SetValueSafe(in newVal2);
			currentNode = (int)stream.ReceiveNext();
			nextNode = (int)stream.ReceiveNext();
			angerPoint = (int)stream.ReceiveNext();
			if (mySkeleton.tapped && currentState != mySkeleton.currentState)
			{
				mySkeleton.ChangeState(currentState);
			}
		}
	}

	[PunRPC]
	public void RemoteActivateGhost(PhotonMessageInfo info)
	{
		MonkeAgent.IncrementRPCCall(info, "RemoteActivateGhost");
		if (base.IsMine)
		{
			mySkeleton.RemoteActivateGhost();
		}
	}

	[PunRPC]
	public void RemotePlayerSeen(PhotonMessageInfo info)
	{
		MonkeAgent.IncrementRPCCall(info, "RemotePlayerSeen");
		NetPlayer player = NetworkSystem.Instance.GetPlayer(info.Sender);
		if (!mySkeleton.playersSeen.Contains(player))
		{
			mySkeleton.RemotePlayerSeen(player);
		}
	}

	[PunRPC]
	public void RemotePlayerCaught(PhotonMessageInfo info)
	{
		MonkeAgent.IncrementRPCCall(info, "RemotePlayerCaught");
		if (base.IsMine)
		{
			NetPlayer player = NetworkSystem.Instance.GetPlayer(info.Sender);
			if (mySkeleton.currentState == SecondLookSkeleton.GhostState.Chasing)
			{
				mySkeleton.RemotePlayerCaught(player);
			}
		}
	}
}
