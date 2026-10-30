using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

namespace GorillaLocomotion.Gameplay;

public class RopeSwingManager : NetworkSceneObject
{
	private Dictionary<int, GorillaRopeSwing> ropes = new Dictionary<int, GorillaRopeSwing>();

	public static RopeSwingManager instance { get; private set; }

	private void Awake()
	{
		if (instance != null && instance != this)
		{
			GTDev.LogWarning("Instance of RopeSwingManager already exists. Destroying.");
			Object.Destroy(this);
		}
		else if (instance == null)
		{
			instance = this;
		}
	}

	private void RegisterInstance(GorillaRopeSwing t)
	{
		ropes.Add(t.ropeId, t);
	}

	private void UnregisterInstance(GorillaRopeSwing t)
	{
		ropes.Remove(t.ropeId);
	}

	public static void Register(GorillaRopeSwing t)
	{
		instance.RegisterInstance(t);
	}

	public static void Unregister(GorillaRopeSwing t)
	{
		instance.UnregisterInstance(t);
	}

	public void SendSetVelocity_RPC(int ropeId, int boneIndex, Vector3 velocity, bool wholeRope)
	{
		if (NetworkSystem.Instance.InRoom)
		{
			photonView.RPC("SetVelocity", RpcTarget.All, ropeId, boneIndex, velocity, wholeRope);
		}
		else
		{
			SetVelocityShared(ropeId, boneIndex, velocity, wholeRope, default(PhotonMessageInfoWrapped));
		}
	}

	public bool TryGetRope(int ropeId, out GorillaRopeSwing result)
	{
		return ropes.TryGetValue(ropeId, out result);
	}

	[PunRPC]
	public void SetVelocity(int ropeId, int boneIndex, Vector3 velocity, bool wholeRope, PhotonMessageInfo info)
	{
		PhotonMessageInfoWrapped info2 = new PhotonMessageInfoWrapped(info);
		SetVelocityShared(ropeId, boneIndex, velocity, wholeRope, info2);
		Utils.Log("Receiving RPC for ropes");
	}

	private void SetVelocityShared(int ropeId, int boneIndex, Vector3 velocity, bool wholeRope, PhotonMessageInfoWrapped info)
	{
		if (info.Sender != null)
		{
			MonkeAgent.IncrementRPCCall(info, "SetVelocityShared");
		}
		if (TryGetRope(ropeId, out var result) && result != null)
		{
			result.SetVelocity(boneIndex, velocity, wholeRope, info);
		}
	}
}
