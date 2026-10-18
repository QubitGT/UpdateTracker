using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Oculus.Interaction;

public class SnapInteractable : Interactable<SnapInteractor, SnapInteractable>, IRigidbodyRef
{
	[SerializeField]
	private Rigidbody _rigidbody;

	[FormerlySerializedAs("_snapPosesProvider")]
	[FormerlySerializedAs("_posesProvider")]
	[SerializeField]
	[Optional]
	[Interface(typeof(ISnapPoseDelegate), new Type[] { })]
	private UnityEngine.Object _snapPoseDelegate;

	[SerializeField]
	[Optional]
	[Interface(typeof(IMovementProvider), new Type[] { })]
	private UnityEngine.Object _movementProvider;

	private static CollisionInteractionRegistry<SnapInteractor, SnapInteractable> _registry;

	public Rigidbody Rigidbody => _rigidbody;

	private ISnapPoseDelegate SnapPoseDelegate { get; set; }

	private IMovementProvider MovementProvider { get; set; }

	private void Reset()
	{
		_rigidbody = GetComponentInParent<Rigidbody>();
	}

	protected override void Awake()
	{
		base.Awake();
		MovementProvider = _movementProvider as IMovementProvider;
		SnapPoseDelegate = _snapPoseDelegate as ISnapPoseDelegate;
	}

	protected override void Start()
	{
		this.BeginStart(ref _started, delegate
		{
			base.Start();
		});
		if (_registry == null)
		{
			_registry = new CollisionInteractionRegistry<SnapInteractor, SnapInteractable>();
			SetRegistry(_registry);
		}
		if (MovementProvider == null)
		{
			MovementProvider = base.gameObject.AddComponent<MoveTowardsTargetProvider>();
			_movementProvider = MovementProvider as MonoBehaviour;
		}
		this.EndStart(ref _started);
	}

	protected override void InteractorAdded(SnapInteractor interactor)
	{
		base.InteractorAdded(interactor);
		if (SnapPoseDelegate != null)
		{
			SnapPoseDelegate.TrackElement(interactor.Identifier, interactor.SnapPose);
		}
	}

	protected override void InteractorRemoved(SnapInteractor interactor)
	{
		base.InteractorRemoved(interactor);
		if (SnapPoseDelegate != null)
		{
			SnapPoseDelegate.UntrackElement(interactor.Identifier);
		}
	}

	protected override void SelectingInteractorAdded(SnapInteractor interactor)
	{
		base.SelectingInteractorAdded(interactor);
		if (SnapPoseDelegate != null)
		{
			SnapPoseDelegate.SnapElement(interactor.Identifier, interactor.SnapPose);
		}
	}

	protected override void SelectingInteractorRemoved(SnapInteractor interactor)
	{
		base.SelectingInteractorRemoved(interactor);
		if (SnapPoseDelegate != null)
		{
			SnapPoseDelegate.UnsnapElement(interactor.Identifier);
		}
	}

	public void InteractorHoverUpdated(SnapInteractor interactor)
	{
		if (SnapPoseDelegate != null)
		{
			SnapPoseDelegate.MoveTrackedElement(interactor.Identifier, interactor.SnapPose);
		}
	}

	public bool PoseForInteractor(SnapInteractor interactor, out Pose result)
	{
		if (SnapPoseDelegate != null)
		{
			return SnapPoseDelegate.SnapPoseForElement(interactor.Identifier, interactor.SnapPose, out result);
		}
		result = base.transform.GetPose();
		return true;
	}

	public IMovement GenerateMovement(in Pose from, SnapInteractor interactor)
	{
		if (PoseForInteractor(interactor, out var result))
		{
			IMovement movement = MovementProvider.CreateMovement();
			movement.StopAndSetPose(from);
			movement.MoveTo(result);
			return movement;
		}
		return null;
	}

	public void InjectAllSnapInteractable(Rigidbody rigidbody)
	{
		InjectRigidbody(rigidbody);
	}

	public void InjectRigidbody(Rigidbody rigidbody)
	{
		_rigidbody = rigidbody;
	}

	public void InjectOptionalMovementProvider(IMovementProvider provider)
	{
		_movementProvider = provider as UnityEngine.Object;
		MovementProvider = provider;
	}

	public void InjectOptionalSnapPoseDelegate(ISnapPoseDelegate snapPoseDelegate)
	{
		_snapPoseDelegate = snapPoseDelegate as UnityEngine.Object;
		SnapPoseDelegate = snapPoseDelegate;
	}
}
