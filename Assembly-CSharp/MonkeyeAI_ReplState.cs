using GorillaExtensions;
using Photon.Pun;
using UnityEngine;

[AddComponentMenu("GorillaTag/Environment/Monkeye AI_Repl State")]
public class MonkeyeAI_ReplState : NetworkComponent
{
	public enum EStates
	{
		Sleeping,
		Patrolling,
		Chasing,
		ReturnToSleepPt,
		GoToSleep,
		BeginAttack,
		OpenFloor,
		DropPlayer,
		CloseFloor
	}

	public EStates state;

	public string userId;

	public Vector3 attackPos;

	public float timer;

	public bool floorEnabled;

	public bool portalEnabled;

	public bool freezePlayer;

	public float alpha;

	protected override void WriteDataPUN(PhotonStream stream, PhotonMessageInfo info)
	{
		stream.SendNext(userId);
		stream.SendNext(attackPos);
		stream.SendNext(timer);
		stream.SendNext(floorEnabled);
		stream.SendNext(portalEnabled);
		stream.SendNext(freezePlayer);
		stream.SendNext(alpha);
		stream.SendNext(state);
	}

	protected override void ReadDataPUN(PhotonStream stream, PhotonMessageInfo info)
	{
		if (info.photonView.Owner != null && info.Sender.ActorNumber == info.photonView.Owner.ActorNumber)
		{
			userId = (string)stream.ReceiveNext();
			ref Vector3 v = ref attackPos;
			Vector3 newVal = (Vector3)stream.ReceiveNext();
			v.SetValueSafe(in newVal);
			timer = (float)stream.ReceiveNext();
			floorEnabled = (bool)stream.ReceiveNext();
			portalEnabled = (bool)stream.ReceiveNext();
			freezePlayer = (bool)stream.ReceiveNext();
			alpha = ((float)stream.ReceiveNext()).ClampSafe(0f, 1f);
			state = (EStates)stream.ReceiveNext();
		}
	}
}
