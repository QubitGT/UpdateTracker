using GorillaExtensions;
using Photon.Pun;
using Unity.Mathematics;
using UnityEngine;

namespace GorillaTagScripts;

public class BuilderItemReliableState : MonoBehaviour, IPunObservable
{
	public Vector3 rightHandAttachPos = Vector3.zero;

	public Quaternion rightHandAttachRot = Quaternion.identity;

	public Vector3 leftHandAttachPos = Vector3.zero;

	public Quaternion leftHandAttachRot = Quaternion.identity;

	public bool dirty;

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.IsWriting)
		{
			stream.SendNext(rightHandAttachPos);
			stream.SendNext(rightHandAttachRot);
			stream.SendNext(leftHandAttachPos);
			stream.SendNext(leftHandAttachRot);
			return;
		}
		rightHandAttachPos = (Vector3)stream.ReceiveNext();
		rightHandAttachRot = (Quaternion)stream.ReceiveNext();
		leftHandAttachPos = (Vector3)stream.ReceiveNext();
		leftHandAttachRot = (Quaternion)stream.ReceiveNext();
		ref Vector3 v = ref rightHandAttachPos;
		float maxVal = 10000f;
		if (!v.IsValid(in maxVal))
		{
			rightHandAttachPos = Vector3.zero;
		}
		if (!rightHandAttachRot.IsValid())
		{
			rightHandAttachRot = quaternion.identity;
		}
		ref Vector3 v2 = ref leftHandAttachPos;
		maxVal = 10000f;
		if (!v2.IsValid(in maxVal))
		{
			leftHandAttachPos = Vector3.zero;
		}
		if (!leftHandAttachRot.IsValid())
		{
			leftHandAttachRot = quaternion.identity;
		}
		dirty = true;
	}
}
