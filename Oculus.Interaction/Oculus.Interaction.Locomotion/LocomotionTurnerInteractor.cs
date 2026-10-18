using System;
using Oculus.Interaction.Input;
using UnityEngine;

namespace Oculus.Interaction.Locomotion;

public class LocomotionTurnerInteractor : Interactor<LocomotionTurnerInteractor, LocomotionTurnerInteractable>, IAxis1D
{
	[SerializeField]
	[Tooltip("Point in space used to drive the axis.")]
	private Transform _origin;

	[SerializeField]
	[Interface(typeof(ISelector), new Type[] { })]
	[Tooltip("Selector for the interactor.")]
	private UnityEngine.Object _selector;

	[SerializeField]
	[Tooltip("Point used to stabilize the rotation of the point")]
	private Transform _stabilizationPoint;

	[SerializeField]
	[Interface(typeof(ITrackingToWorldTransformer), new Type[] { })]
	[Tooltip("Transformer is required so calculations can be done in Tracking space")]
	private UnityEngine.Object _transformer;

	public ITrackingToWorldTransformer Transformer;

	[SerializeField]
	[Tooltip("Offset from the center point at which the pointer will be dragged")]
	private float _dragThresold = 0.1f;

	private Pose _midPoint = Pose.identity;

	private float _axisValue;

	private Action<float> _whenTurnDirectionChanged = delegate
	{
	};

	public float DragThresold
	{
		get
		{
			return _dragThresold;
		}
		set
		{
			_dragThresold = value;
		}
	}

	public Pose MidPoint => Transformer.ToWorldPose(_midPoint);

	public Pose Origin => _origin.GetPose();

	public override bool ShouldHover => base.State == InteractorState.Normal;

	public override bool ShouldUnhover => false;

	public event Action<float> WhenTurnDirectionChanged
	{
		add
		{
			_whenTurnDirectionChanged = (Action<float>)Delegate.Combine(_whenTurnDirectionChanged, value);
		}
		remove
		{
			_whenTurnDirectionChanged = (Action<float>)Delegate.Remove(_whenTurnDirectionChanged, value);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		Transformer = _transformer as ITrackingToWorldTransformer;
		base.Selector = _selector as ISelector;
	}

	protected override void Start()
	{
		this.BeginStart(ref _started, delegate
		{
			base.Start();
		});
		this.EndStart(ref _started);
	}

	protected override void HandleEnabled()
	{
		base.HandleEnabled();
		Pose pose = _origin.GetPose();
		InitializeMidPoint(pose);
	}

	protected override void DoHoverUpdate()
	{
		base.DoHoverUpdate();
		UpdatePointers();
	}

	protected override void DoSelectUpdate()
	{
		base.DoSelectUpdate();
		UpdatePointers();
	}

	private void UpdatePointers()
	{
		Pose pose = _origin.GetPose();
		UpdateMidPoint(pose, MidPoint);
		DragMidPoint(MidPoint);
		UpdateAxisValue(pose, MidPoint);
	}

	private void InitializeMidPoint(Pose pointer)
	{
		Quaternion rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(pointer.position - _stabilizationPoint.position, Vector3.up).normalized, Vector3.up);
		Vector3 position = pointer.position;
		ITrackingToWorldTransformer transformer = Transformer;
		Pose worldPose = new Pose(position, rotation);
		_midPoint = transformer.ToTrackingPose(in worldPose);
	}

	private void UpdateMidPoint(Pose pointer, Pose midPoint)
	{
		float magnitude = Vector3.ProjectOnPlane(pointer.position - _stabilizationPoint.position, Vector3.up).magnitude;
		Vector3 position = _stabilizationPoint.position + midPoint.forward * magnitude;
		position.y = pointer.position.y;
		Quaternion rotation = midPoint.rotation;
		ITrackingToWorldTransformer transformer = Transformer;
		Pose worldPose = new Pose(position, rotation);
		_midPoint = transformer.ToTrackingPose(in worldPose);
	}

	private void DragMidPoint(Pose worldMidPoint)
	{
		Vector3 position = worldMidPoint.position;
		float num = Mathf.Abs(_axisValue) - _dragThresold * base.transform.lossyScale.x;
		if (!(num <= 0f))
		{
			Vector3 right = worldMidPoint.right;
			float num2 = Math.Sign(_axisValue);
			position += right * num2 * num;
			Quaternion rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(position - _stabilizationPoint.position, Vector3.up).normalized, Vector3.up);
			ITrackingToWorldTransformer transformer = Transformer;
			Pose worldPose = new Pose(position, rotation);
			_midPoint = transformer.ToTrackingPose(in worldPose);
		}
	}

	private void UpdateAxisValue(Pose pointer, Pose origin)
	{
		float num = Mathf.Sign(_axisValue);
		Vector3 rhs = pointer.position - origin.position;
		_axisValue = Vector3.Project(pointer.position - origin.position, origin.right).magnitude * Mathf.Sign(Vector3.Dot(origin.right, rhs));
		if (num != Mathf.Sign(_axisValue))
		{
			_whenTurnDirectionChanged(num);
		}
	}

	public float Value()
	{
		return Mathf.Clamp(_axisValue / (_dragThresold * base.transform.lossyScale.x), -1f, 1f);
	}

	protected override LocomotionTurnerInteractable ComputeCandidate()
	{
		return null;
	}

	public void InjectAllLocomotionTurnerInteractor(Transform origin, ISelector selector, Transform stabilizationPoint, ITrackingToWorldTransformer transformer)
	{
		InjectOrigin(origin);
		InjectSelector(selector);
		InjectStabilizationPoint(stabilizationPoint);
		InjectTransformer(transformer);
	}

	public void InjectOrigin(Transform origin)
	{
		_origin = origin;
	}

	public void InjectSelector(ISelector selector)
	{
		_selector = selector as UnityEngine.Object;
		base.Selector = selector;
	}

	public void InjectStabilizationPoint(Transform stabilizationPoint)
	{
		_stabilizationPoint = stabilizationPoint;
	}

	public void InjectTransformer(ITrackingToWorldTransformer transformer)
	{
		_transformer = transformer as UnityEngine.Object;
		Transformer = transformer;
	}
}
