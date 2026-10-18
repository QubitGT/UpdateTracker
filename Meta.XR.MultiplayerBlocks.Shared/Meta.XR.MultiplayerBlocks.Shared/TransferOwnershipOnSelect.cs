using System;
using Meta.XR.BuildingBlocks;
using Oculus.Interaction;
using UnityEngine;

namespace Meta.XR.MultiplayerBlocks.Shared;

public class TransferOwnershipOnSelect : MonoBehaviour
{
	public bool UseGravity;

	private Grabbable _grabbable;

	private Rigidbody _rigidbody;

	private ITransferOwnership _transferOwnership;

	private void Awake()
	{
		_grabbable = GetComponentInChildren<Grabbable>();
		if (_grabbable == null)
		{
			throw new InvalidOperationException("Object requires a Grabbable component");
		}
		_grabbable.WhenPointerEventRaised += OnPointerEventRaised;
		_transferOwnership = this.GetInterfaceComponent<ITransferOwnership>();
		if (_transferOwnership == null)
		{
			throw new InvalidOperationException("Object requires an ITransferOwnership component");
		}
		if (UseGravity)
		{
			_rigidbody = GetComponent<Rigidbody>();
			if (_rigidbody == null)
			{
				throw new InvalidOperationException("Object requires a Rigidbody component when useGravity enabled");
			}
		}
	}

	private void OnDestroy()
	{
		if (_grabbable != null)
		{
			_grabbable.WhenPointerEventRaised -= OnPointerEventRaised;
		}
	}

	private void OnPointerEventRaised(PointerEvent pointerEvent)
	{
		if (!(_grabbable == null) && pointerEvent.Type == PointerEventType.Select && _grabbable.SelectingPointsCount == 1 && !_transferOwnership.HasOwnership())
		{
			_transferOwnership.TransferOwnershipToLocalPlayer();
		}
	}

	private void LateUpdate()
	{
		if (_transferOwnership.HasOwnership() && UseGravity)
		{
			_rigidbody.isKinematic = _rigidbody.IsLocked();
		}
	}
}
