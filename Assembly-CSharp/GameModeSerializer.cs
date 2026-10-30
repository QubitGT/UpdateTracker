using System;
using GorillaExtensions;
using GorillaGameModes;
using Photon.Pun;
using UnityEngine;

internal class GameModeSerializer : GorillaSerializerMasterOnly
{
	private GameModeType gameModeKey;

	private GorillaGameManager gameModeInstance;

	private CallLimiter broadcastTagCallLimit = new CallLimiter(12, 5f);

	public GorillaGameManager GameModeInstance => gameModeInstance;

	protected override bool OnSpawnSetupCheck(PhotonMessageInfoWrapped wrappedInfo, out GameObject outTargetObject, out Type outTargetType)
	{
		outTargetObject = null;
		outTargetType = null;
		NetPlayer player = NetworkSystem.Instance.GetPlayer(wrappedInfo.senderID);
		if (player != null)
		{
			MonkeAgent.IncrementRPCCall(wrappedInfo, "OnSpawnSetupCheck");
		}
		GameModeSerializer activeNetworkHandler = GameMode.ActiveNetworkHandler;
		if (player != null && player.InRoom)
		{
			if (!player.IsMasterClient)
			{
				GTDev.LogError("SPAWN FAIL NOT MASTER :" + player.UserId + player.NickName);
				MonkeAgent.instance.SendReport("trying to inappropriately create game managers", player.UserId, player.NickName);
				return false;
			}
			if (!netView.IsRoomView)
			{
				GTDev.LogError("SPAWN FAIL ROOM VIEW" + player.UserId + player.NickName);
				MonkeAgent.instance.SendReport("creating game manager as player object", player.UserId, player.NickName);
				return false;
			}
			if (activeNetworkHandler.IsNotNull() && activeNetworkHandler != this)
			{
				GTDev.LogError("DUPLICATE CHECK" + player.UserId + player.NickName);
				MonkeAgent.instance.SendReport("trying to create multiple game managers", player.UserId, player.NickName);
				return false;
			}
		}
		else if ((activeNetworkHandler.IsNotNull() && activeNetworkHandler != this) || !netView.IsRoomView)
		{
			GTDev.LogError("ACTIVE HANDLER CHECK FAIL" + player?.UserId + player?.NickName);
			GTDev.LogError("existing game manager! destroying newly created manager");
			return false;
		}
		object[] instantiationData = wrappedInfo.punInfo.photonView.InstantiationData;
		if (instantiationData == null || instantiationData.Length < 1 || !(instantiationData[0] is int num))
		{
			GTDev.LogError("missing instantiation data");
			return false;
		}
		gameModeKey = (GameModeType)num;
		gameModeInstance = GameMode.GetGameModeInstance(gameModeKey);
		if (gameModeInstance.IsNull() || !gameModeInstance.ValidGameMode())
		{
			return false;
		}
		serializeTarget = gameModeInstance;
		base.transform.parent = VRRigCache.Instance.NetworkParent;
		return true;
	}

	protected override void OnSuccesfullySpawned(PhotonMessageInfoWrapped info)
	{
		netView.GetView.AddCallbackTarget(this);
		GameMode.SetupGameModeRemote(this);
	}

	protected override void OnBeforeDespawn()
	{
		GameMode.RemoveNetworkLink(this);
	}

	protected override void OnFailedSpawn()
	{
	}

	[PunRPC]
	internal void RPC_ReportTag(int taggedPlayer, PhotonMessageInfo info)
	{
		ReportTag(NetworkSystem.Instance.GetPlayer(taggedPlayer), new PhotonMessageInfoWrapped(info));
	}

	[PunRPC]
	internal void RPC_ReportHit(PhotonMessageInfo info)
	{
		ReportHit(new PhotonMessageInfoWrapped(info));
	}

	private void ReportTag(NetPlayer taggedPlayer, PhotonMessageInfoWrapped info)
	{
		MonkeAgent.IncrementRPCCall(info, "ReportTag");
		NetPlayer sender = info.Sender;
		gameModeInstance.ReportTag(taggedPlayer, sender);
	}

	private void ReportHit(PhotonMessageInfoWrapped info)
	{
		MonkeAgent.IncrementRPCCall(info, "ReportContactWithLavaRPC");
		bool num = ZoneManagement.instance.IsZoneActive(GTZone.customMaps);
		bool flag = false;
		if (VRRigCache.Instance.TryGetVrrig(info.Sender, out var playerRig))
		{
			InfectionLavaController infectionLavaController = null;
			if (playerRig.Rig.zoneEntity != null)
			{
				infectionLavaController = InfectionLavaController.GetControllerForZone(playerRig.Rig.zoneEntity.currentZone);
			}
			flag = infectionLavaController != null && infectionLavaController.LavaCurrentlyActivated && (infectionLavaController.SurfaceCenter - playerRig.Rig.syncPos).sqrMagnitude < 2500f && infectionLavaController.LavaPlane.GetDistanceToPoint(playerRig.Rig.syncPos) < 5f;
		}
		if (num || flag)
		{
			GameModeInstance.HitPlayer(info.Sender);
		}
	}

	[PunRPC]
	internal void RPC_BroadcastRoundComplete(PhotonMessageInfo info)
	{
		BroadcastRoundComplete(info);
	}

	private void BroadcastRoundComplete(PhotonMessageInfoWrapped info)
	{
		MonkeAgent.IncrementRPCCall(info, "BroadcastRoundComplete");
		if (info.Sender.IsMasterClient)
		{
			gameModeInstance.HandleRoundComplete();
		}
	}

	[PunRPC]
	internal void RPC_BroadcastTag(int taggedPlayer, int taggingPlayer, PhotonMessageInfo info)
	{
		BroadcastTag(NetworkSystem.Instance.GetPlayer(taggedPlayer), NetworkSystem.Instance.GetPlayer(taggingPlayer), info);
	}

	private void BroadcastTag(NetPlayer taggedPlayer, NetPlayer taggingPlayer, PhotonMessageInfo info)
	{
		MonkeAgent.IncrementRPCCall(info, "BroadcastTag");
		if (info.Sender.IsMasterClient && taggedPlayer != null && taggingPlayer != null && broadcastTagCallLimit.CheckCallTime(Time.time))
		{
			gameModeInstance.HandleTagBroadcast(taggedPlayer, taggingPlayer);
		}
	}
}
