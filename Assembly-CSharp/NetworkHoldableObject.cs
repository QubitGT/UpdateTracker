using Photon.Pun;
using UnityEngine;

public abstract class NetworkHoldableObject : NetworkComponent, IHoldableObject
{
	public virtual bool TwoHanded => false;

	public abstract void OnHover(InteractionPoint pointHovered, GameObject hoveringHand);

	public abstract void OnGrab(InteractionPoint pointGrabbed, GameObject grabbingHand);

	public abstract void DropItemCleanup();

	public virtual bool OnRelease(DropZone zoneReleased, GameObject releasingHand)
	{
		if (EquipmentInteractor.instance.rightHandHeldEquipment == this && releasingHand != EquipmentInteractor.instance.rightHand)
		{
			return false;
		}
		if (EquipmentInteractor.instance.leftHandHeldEquipment == this && releasingHand != EquipmentInteractor.instance.leftHand)
		{
			return false;
		}
		return true;
	}

	protected override void ReadDataPUN(PhotonStream stream, PhotonMessageInfo info)
	{
	}

	protected override void WriteDataPUN(PhotonStream stream, PhotonMessageInfo info)
	{
	}

	GameObject IHoldableObject.get_gameObject()
	{
		return base.gameObject;
	}

	string IHoldableObject.get_name()
	{
		return base.name;
	}

	void IHoldableObject.set_name(string value)
	{
		base.name = value;
	}
}
