using System;
using UnityEngine;

namespace Oculus.Interaction;

public class GrabInteractable : PointerInteractable<GrabInteractor, GrabInteractable>, IRigidbodyRef, ICollidersRef
{
	private Collider[] _colliders;

	[Tooltip("The Rigidbody of the object.")]
	[SerializeField]
	private Rigidbody _rigidbody;

	[Tooltip("An optional origin point for the grab.")]
	[SerializeField]
	[Optional]
	private Transform _grabSource;

	[Tooltip("If true, use the closest point to the interactor as the grab source.")]
	[SerializeField]
	private bool _useClosestPointAsGrabSource;

	[Tooltip(" ")]
	[SerializeField]
	private float _releaseDistance;

	[Tooltip("Forces a release on all other grabbing interactors when grabbed by a new interactor.")]
	[SerializeField]
	private bool _resetGrabOnGrabsUpdated = true;

	[Tooltip("The PhysicsGrabbable used when you grab the interactable.")]
	[SerializeField]
	[Optional(OptionalAttribute.Flag.Obsolete)]
	[Obsolete("Use Grabbable and/or RigidbodyKinematicLocker instead")]
	private PhysicsGrabbable _physicsGrabbable;

	private static CollisionInteractionRegistry<GrabInteractor, GrabInteractable> _grabRegistry;

	public Collider[] Colliders => _colliders;

	public Rigidbody Rigidbody => _rigidbody;

	public bool UseClosestPointAsGrabSource
	{
		get
		{
			return _useClosestPointAsGrabSource;
		}
		set
		{
			_useClosestPointAsGrabSource = value;
		}
	}

	public float ReleaseDistance
	{
		get
		{
			return _releaseDistance;
		}
		set
		{
			_releaseDistance = value;
		}
	}

	public bool ResetGrabOnGrabsUpdated
	{
		get
		{
			return _resetGrabOnGrabsUpdated;
		}
		set
		{
			_resetGrabOnGrabsUpdated = value;
		}
	}

	protected override void Awake()
	{
		base.Awake();
	}

	protected override void Start()
	{
		this.BeginStart(ref _started, delegate
		{
			base.Start();
		});
		if (_grabRegistry == null)
		{
			_grabRegistry = new CollisionInteractionRegistry<GrabInteractor, GrabInteractable>();
			SetRegistry(_grabRegistry);
		}
		_colliders = Rigidbody.GetComponentsInChildren<Collider>();
		this.EndStart(ref _started);
	}

	public Pose GetGrabSourceForTarget(Pose target)
	{
		if (_grabSource == null && !_useClosestPointAsGrabSource)
		{
			return target;
		}
		if (_useClosestPointAsGrabSource)
		{
			return new Pose(Collisions.ClosestPointToColliders(target.position, _colliders), target.rotation);
		}
		return _grabSource.GetPose();
	}

	[Obsolete("Use Grabbable instead")]
	public void ApplyVelocities(Vector3 linearVelocity, Vector3 angularVelocity)
	{
		if (!(_physicsGrabbable == null))
		{
			_physicsGrabbable.ApplyVelocities(linearVelocity, angularVelocity);
		}
	}

	public void InjectAllGrabInteractable(Rigidbody rigidbody)
	{
		InjectRigidbody(rigidbody);
	}

	public void InjectRigidbody(Rigidbody rigidbody)
	{
		_rigidbody = rigidbody;
	}

	public void InjectOptionalGrabSource(Transform grabSource)
	{
		_grabSource = grabSource;
	}

	public void InjectOptionalReleaseDistance(float releaseDistance)
	{
		_releaseDistance = releaseDistance;
	}

	[Obsolete("Use Grabbable instead")]
	public void InjectOptionalPhysicsGrabbable(PhysicsGrabbable physicsGrabbable)
	{
		_physicsGrabbable = physicsGrabbable;
	}
}
