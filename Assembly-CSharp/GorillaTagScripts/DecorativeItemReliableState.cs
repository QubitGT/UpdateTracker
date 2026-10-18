using GorillaExtensions;
using Photon.Pun;
using Unity.Mathematics;
using UnityEngine;

namespace GorillaTagScripts;

public class DecorativeItemReliableState : MonoBehaviour, IPunObservable
{
	public bool isSnapped;

	public Vector3 snapPosition = Vector3.zero;

	public Vector3 respawnPosition = Vector3.zero;

	public Quaternion respawnRotation = Quaternion.identity;

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.IsWriting)
		{
			stream.SendNext(isSnapped);
			stream.SendNext(snapPosition);
			stream.SendNext(respawnPosition);
			stream.SendNext(respawnRotation);
			return;
		}
		isSnapped = (bool)stream.ReceiveNext();
		snapPosition = (Vector3)stream.ReceiveNext();
		respawnPosition = (Vector3)stream.ReceiveNext();
		respawnRotation = (Quaternion)stream.ReceiveNext();
		ref Vector3 v = ref snapPosition;
		float maxVal = 10000f;
		if (!v.IsValid(in maxVal))
		{
			snapPosition = Vector3.zero;
		}
		ref Vector3 v2 = ref respawnPosition;
		maxVal = 10000f;
		if (!v2.IsValid(in maxVal))
		{
			respawnPosition = Vector3.zero;
		}
		if (!respawnRotation.IsValid())
		{
			respawnRotation = quaternion.identity;
		}
	}
}
