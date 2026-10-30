using System;
using GorillaExtensions;
using Photon.Pun;
using UnityEngine;

public class ThrowableBugReliableState : NetworkComponent, IRequestableOwnershipGuardCallbacks
{
	public Vector3 travelingDirection = Vector3.zero;

	protected override void WriteDataPUN(PhotonStream stream, PhotonMessageInfo info)
	{
		stream.SendNext(travelingDirection);
	}

	protected override void ReadDataPUN(PhotonStream stream, PhotonMessageInfo info)
	{
		ref Vector3 v = ref travelingDirection;
		Vector3 newVal = (Vector3)stream.ReceiveNext();
		v.SetValueSafe(in newVal);
	}

	public void OnOwnershipTransferred(NetPlayer toPlayer, NetPlayer fromPlayer)
	{
		throw new NotImplementedException();
	}

	public bool OnOwnershipRequest(NetPlayer fromPlayer)
	{
		throw new NotImplementedException();
	}

	public void OnMyOwnerLeft()
	{
		throw new NotImplementedException();
	}

	public bool OnMasterClientAssistedTakeoverRequest(NetPlayer fromPlayer, NetPlayer toPlayer)
	{
		throw new NotImplementedException();
	}

	public void OnMyCreatorLeft()
	{
		throw new NotImplementedException();
	}
}
