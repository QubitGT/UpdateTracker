using System;
using UnityEngine;

namespace Oculus.Interaction;

public class DistanceGrabInteractable : PointerInteractable<DistanceGrabInteractor, DistanceGrabInteractable>, IRigidbodyRef, IRelativeToRef, ICollidersRef
{
	private Collider[] _colliders;

	[Tooltip("The RigidBody of the interactable.")]
	[SerializeField]
	private Rigidbody _rigidbody;

	[Tooltip("An optional origin point for the grab.")]
	[SerializeField]
	[Optional]
	private Transform _grabSource;

	[Tooltip("Forces a release on all other grabbing interactors when grabbed by a new interactor.")]
	[SerializeField]
	private bool _resetGrabOnGrabsUpdated = true;

	[Tooltip("PhysicsGrabbable used when you grab the interactable.")]
	[SerializeField]
	[Optional(OptionalAttribute.Flag.Obsolete)]
	[Obsolete("Use Grabbable and/or RigidbodyKinematicLocker instead")]
	private PhysicsGrabbable _physicsGrabbable;

	[Tooltip("The IMovementProvider specifies how the interactable will align with the grabber when selected. If no IMovementProvider is set, the MoveTowardsTargetProvider is created and used as the provider.")]
	[Header("Snap")]
	[SerializeField]
	[Optional]
	[Interface(typeof(IMovementProvider), new Type[] { })]
	private UnityEngine.Object _movementProvider;

	public Collider[] Colliders => _colliders;

	public Rigidbody Rigidbody => _rigidbody;

	private IMovementProvider MovementProvider { get; set; }

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

	public Transform RelativeTo => _grabSource;

	protected virtual void Reset()
	{
		_rigidbody = GetComponentInParent<Rigidbody>();
	}

	protected override void Awake()
	{
		base.Awake();
		MovementProvider = _movementProvider as IMovementProvider;
	}

	protected override void Start()
	{
		this.BeginStart(ref _started, delegate
		{
			base.Start();
		});
		_colliders = Rigidbody.GetComponentsInChildren<Collider>();
		if (MovementProvider == null)
		{
			MoveTowardsTargetProvider provider = base.gameObject.AddComponent<MoveTowardsTargetProvider>();
			InjectOptionalMovementProvider(provider);
		}
		if (_grabSource == null)
		{
			_grabSource = Rigidbody.transform;
		}
		this.EndStart(ref _started);
	}

	public IMovement GenerateMovement(in Pose to)
	{
		Pose pose = _grabSource.GetPose();
		IMovement movement = MovementProvider.CreateMovement();
		movement.StopAndSetPose(pose);
		movement.MoveTo(to);
		return movement;
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

	[Obsolete("Use Grabbable instead")]
	public void InjectOptionalPhysicsGrabbable(PhysicsGrabbable physicsGrabbable)
	{
		_physicsGrabbable = physicsGrabbable;
	}

	public void InjectOptionalMovementProvider(IMovementProvider provider)
	{
		_movementProvider = provider as UnityEngine.Object;
		MovementProvider = provider;
	}
}
