using System.Collections.Generic;
using UnityEngine;

namespace Oculus.Interaction;

public class TouchHandGrabInteractable : PointerInteractable<TouchHandGrabInteractor, TouchHandGrabInteractable>
{
	[SerializeField]
	private Collider _boundsCollider;

	[SerializeField]
	private List<Collider> _colliders;

	private ColliderGroup _colliderGroup;

	public ColliderGroup ColliderGroup => _colliderGroup;

	protected override void Start()
	{
		base.Start();
		_colliderGroup = new ColliderGroup(_colliders, _boundsCollider);
	}

	public void InjectAllTouchHandGrabInteractable(Collider boundsCollider, List<Collider> colliders)
	{
		InjectBoundsCollider(boundsCollider);
		InjectColliders(colliders);
	}

	public void InjectBoundsCollider(Collider boundsCollider)
	{
		_boundsCollider = boundsCollider;
	}

	public void InjectColliders(List<Collider> colliders)
	{
		_colliders = colliders;
	}
}
