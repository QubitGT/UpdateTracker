using System;
using GorillaTag;
using Photon.Pun;
using UnityEngine;

internal abstract class GorillaWrappedSerializer : MonoBehaviour, IPunObservable, IPunInstantiateMagicCallback, IOnPhotonViewPreNetDestroy, IPhotonViewCallback
{
	protected bool successfullInstantiate;

	protected IWrappedSerializable serializeTarget;

	private Type targetType;

	protected GameObject targetObject;

	[SerializeField]
	protected NetworkView netView;

	public NetworkView NetView => netView;

	public bool IsLocallyOwned => netView.IsMine;

	public bool IsValid => netView.IsValid;

	private void Awake()
	{
		if (netView == null)
		{
			netView = GetComponent<NetworkView>();
		}
	}

	void IPunInstantiateMagicCallback.OnPhotonInstantiate(PhotonMessageInfo info)
	{
		if (!(netView == null) && netView.IsValid)
		{
			PhotonMessageInfoWrapped wrappedInfo = new PhotonMessageInfoWrapped(info);
			ProcessSpawn(wrappedInfo);
		}
	}

	private void ProcessSpawn(PhotonMessageInfoWrapped wrappedInfo)
	{
		successfullInstantiate = OnSpawnSetupCheck(wrappedInfo, out targetObject, out targetType);
		if (successfullInstantiate)
		{
			if (targetObject?.GetComponent(targetType) is IWrappedSerializable wrappedSerializable)
			{
				serializeTarget = wrappedSerializable;
			}
			if (serializeTarget == null)
			{
				successfullInstantiate = false;
			}
		}
		if (successfullInstantiate)
		{
			OnSuccesfullySpawned(wrappedInfo);
		}
		else
		{
			FailedToSpawn();
		}
	}

	protected virtual bool OnSpawnSetupCheck(PhotonMessageInfoWrapped wrappedInfo, out GameObject outTargetObject, out Type outTargetType)
	{
		outTargetType = typeof(IWrappedSerializable);
		outTargetObject = base.gameObject;
		return true;
	}

	protected abstract void OnSuccesfullySpawned(PhotonMessageInfoWrapped info);

	private void FailedToSpawn()
	{
		Debug.LogError("Failed to network instantiate");
		MonkeAgentCleanup.RegisterForDestroy(netView.GetView);
		netView.GetView.ObservedComponents.Remove(this);
	}

	protected abstract void OnFailedSpawn();

	protected virtual bool ValidOnSerialize(PhotonStream stream, in PhotonMessageInfo info)
	{
		if (info.Sender != info.photonView.Owner)
		{
			return false;
		}
		return true;
	}

	void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (successfullInstantiate && serializeTarget != null && ValidOnSerialize(stream, in info))
		{
			if (stream.IsWriting)
			{
				serializeTarget.OnSerializeWrite(stream, info);
			}
			else
			{
				serializeTarget.OnSerializeRead(stream, info);
			}
		}
	}

	void IOnPhotonViewPreNetDestroy.OnPreNetDestroy(PhotonView rootView)
	{
		OnBeforeDespawn();
	}

	protected abstract void OnBeforeDespawn();

	public virtual T AddRPCComponent<T>() where T : RPCNetworkBase
	{
		T val = base.gameObject.AddComponent<T>();
		netView.GetView.RefreshRpcMonoBehaviourCache();
		val.SetClassTarget(serializeTarget, this);
		return val;
	}

	public void SendRPC(string rpcName, bool targetOthers, params object[] data)
	{
		RpcTarget target = (targetOthers ? RpcTarget.Others : RpcTarget.MasterClient);
		netView.SendRPC(rpcName, target, data);
	}

	public void SendRPC(string rpcName, NetPlayer targetPlayer, params object[] data)
	{
		netView.GetView.RPC(rpcName, ((PunNetPlayer)targetPlayer).PlayerRef, data);
	}
}
