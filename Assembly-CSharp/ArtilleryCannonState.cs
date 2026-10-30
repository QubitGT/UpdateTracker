using System;
using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(XSceneRefTarget))]
[AddComponentMenu("GorillaTag/Environment/Artillery Cannon State")]
public class ArtilleryCannonState : NetworkComponent
{
	internal struct CrankSyncState
	{
		public int holderActorNr;

		public bool isLeftHand;

		public float angle;
	}

	private enum ArtilleryMsg : byte
	{
		CrankGrabLeft,
		CrankGrabRight,
		CrankRelease,
		CrankInput,
		Fire
	}

	internal const int CRANK_PITCH = 0;

	internal const int CRANK_YAW = 1;

	[Header("Rotation Limits")]
	[SerializeField]
	private float pitchMin = -10f;

	[SerializeField]
	private float pitchMax = 60f;

	[Tooltip("How many degrees the cannon rotates per degree of crank rotation")]
	[SerializeField]
	private float degreesPerCrankDegree = 0.5f;

	[Header("Firing")]
	[SerializeField]
	private float fireCooldown = 2f;

	private float currentPitch;

	private float currentYaw;

	private float lastFireTime;

	internal CrankSyncState pitchCrankSync;

	internal CrankSyncState yawCrankSync;

	private const float GRAB_GRACE_PERIOD = 1f;

	private float pitchPendingGrabTime;

	private float yawPendingGrabTime;

	internal float CurrentPitch => currentPitch;

	internal float CurrentYaw => currentYaw;

	internal float PitchMin => pitchMin;

	internal float PitchMax => pitchMax;

	internal float DegreesPerCrankDegree => degreesPerCrankDegree;

	private int LocalActorNr
	{
		get
		{
			if (PhotonNetwork.LocalPlayer == null)
			{
				return -1;
			}
			return PhotonNetwork.LocalPlayer.ActorNumber;
		}
	}

	internal event Action onRotationChanged;

	internal event Action onFired;

	protected override void Awake()
	{
		base.Awake();
		pitchCrankSync.holderActorNr = -1;
		yawCrankSync.holderActorNr = -1;
	}

	internal void UpdateLocalCrankState(int crankIndex, bool isLeftHand, float angle)
	{
		ref CrankSyncState reference = ref crankIndex == 0 ? ref pitchCrankSync : ref yawCrankSync;
		int localActorNr = LocalActorNr;
		if (reference.holderActorNr == localActorNr)
		{
			reference.isLeftHand = isLeftHand;
			reference.angle = angle;
		}
	}

	internal static VRRig FindRigForActor(int actorNr)
	{
		if (VRRigCache.Instance.TryGetVrrig(actorNr, out var playerRig))
		{
			return playerRig.Rig;
		}
		return null;
	}

	internal bool NotifyCrankGrabbed(int crankIndex, bool isLeftHand)
	{
		ref CrankSyncState reference = ref crankIndex == 0 ? ref pitchCrankSync : ref yawCrankSync;
		if (reference.holderActorNr != -1)
		{
			return false;
		}
		reference.holderActorNr = LocalActorNr;
		crankIndex == 0 ? ref pitchPendingGrabTime : ref yawPendingGrabTime = Time.time;
		if (PhotonNetwork.InRoom)
		{
			SendRPC("RPC_ArtilleryMessage", RpcTarget.MasterClient, (!isLeftHand) ? ((byte)1) : ((byte)0), (byte)crankIndex, 0f);
		}
		return true;
	}

	internal void NotifyCrankReleased(int crankIndex, float finalAngle)
	{
		ref CrankSyncState reference = ref crankIndex == 0 ? ref pitchCrankSync : ref yawCrankSync;
		reference.holderActorNr = -1;
		reference.angle = finalAngle;
		crankIndex == 0 ? ref pitchPendingGrabTime : ref yawPendingGrabTime = 0f;
		if (PhotonNetwork.InRoom)
		{
			SendRPC("RPC_ArtilleryMessage", RpcTarget.All, (byte)2, (byte)crankIndex, finalAngle);
		}
	}

	internal void NotifyCrankInput(int crankIndex, float degrees)
	{
		if (crankIndex == 0)
		{
			currentPitch = Mathf.Clamp(currentPitch + degrees * degreesPerCrankDegree, pitchMin, pitchMax);
		}
		else
		{
			currentYaw += degrees * degreesPerCrankDegree;
		}
		this.onRotationChanged?.Invoke();
		if (PhotonNetwork.InRoom)
		{
			float num = ((crankIndex == 0) ? currentPitch : currentYaw);
			SendRPC("RPC_ArtilleryMessage", RpcTarget.MasterClient, (byte)3, (byte)crankIndex, num);
		}
	}

	internal bool TryFire()
	{
		if (Time.time < lastFireTime + fireCooldown)
		{
			return false;
		}
		lastFireTime = Time.time;
		if (PhotonNetwork.InRoom)
		{
			SendRPC("RPC_ArtilleryMessage", RpcTarget.Others, (byte)4, (byte)0, 0f);
		}
		return true;
	}

	[PunRPC]
	public void RPC_ArtilleryMessage(byte msgType, byte crankIndex, float floatParam, PhotonMessageInfo info)
	{
		switch ((ArtilleryMsg)msgType)
		{
		case ArtilleryMsg.CrankGrabLeft:
		case ArtilleryMsg.CrankGrabRight:
			if (PhotonNetwork.IsMasterClient)
			{
				ref CrankSyncState reference = ref crankIndex == 0 ? ref pitchCrankSync : ref yawCrankSync;
				if (reference.holderActorNr == -1)
				{
					reference.holderActorNr = info.Sender.ActorNumber;
					reference.isLeftHand = msgType == 0;
				}
			}
			break;
		case ArtilleryMsg.CrankRelease:
		{
			ref CrankSyncState reference2 = ref crankIndex == 0 ? ref pitchCrankSync : ref yawCrankSync;
			if (reference2.holderActorNr == info.Sender.ActorNumber)
			{
				reference2.holderActorNr = -1;
				reference2.angle = floatParam;
			}
			break;
		}
		case ArtilleryMsg.CrankInput:
			if (PhotonNetwork.IsMasterClient && (crankIndex == 0 ? ref pitchCrankSync : ref yawCrankSync).holderActorNr == info.Sender.ActorNumber)
			{
				if (crankIndex == 0)
				{
					currentPitch = Mathf.Clamp(floatParam, pitchMin, pitchMax);
				}
				else
				{
					currentYaw = floatParam;
				}
				this.onRotationChanged?.Invoke();
			}
			break;
		case ArtilleryMsg.Fire:
		{
			int actorNumber = info.Sender.ActorNumber;
			if (pitchCrankSync.holderActorNr == actorNumber || yawCrankSync.holderActorNr == actorNumber)
			{
				this.onFired?.Invoke();
			}
			break;
		}
		}
	}

	protected override void WriteDataPUN(PhotonStream stream, PhotonMessageInfo info)
	{
		stream.SendNext(currentPitch);
		stream.SendNext(currentYaw);
		stream.SendNext(pitchCrankSync.holderActorNr);
		stream.SendNext(pitchCrankSync.isLeftHand);
		stream.SendNext(pitchCrankSync.angle);
		stream.SendNext(yawCrankSync.holderActorNr);
		stream.SendNext(yawCrankSync.isLeftHand);
		stream.SendNext(yawCrankSync.angle);
	}

	protected override void ReadDataPUN(PhotonStream stream, PhotonMessageInfo info)
	{
		float num = (float)stream.ReceiveNext();
		float num2 = (float)stream.ReceiveNext();
		int localActorNr = LocalActorNr;
		ReadCrankSyncPUN(stream, ref pitchCrankSync, ref pitchPendingGrabTime, localActorNr);
		ReadCrankSyncPUN(stream, ref yawCrankSync, ref yawPendingGrabTime, localActorNr);
		if (pitchCrankSync.holderActorNr != localActorNr)
		{
			currentPitch = num;
		}
		if (yawCrankSync.holderActorNr != localActorNr)
		{
			currentYaw = num2;
		}
		this.onRotationChanged?.Invoke();
	}

	private void ReadCrankSyncPUN(PhotonStream stream, ref CrankSyncState crank, ref float pendingTime, int localActor)
	{
		int num = (int)stream.ReceiveNext();
		bool isLeftHand = (bool)stream.ReceiveNext();
		float angle = (float)stream.ReceiveNext();
		if (pendingTime > 0f && crank.holderActorNr == localActor)
		{
			if (num == localActor)
			{
				pendingTime = 0f;
			}
			else if (num != -1)
			{
				pendingTime = 0f;
			}
			else
			{
				if (!(Time.time - pendingTime > 1f))
				{
					return;
				}
				pendingTime = 0f;
			}
		}
		crank.holderActorNr = num;
		crank.isLeftHand = isLeftHand;
		crank.angle = angle;
	}
}
