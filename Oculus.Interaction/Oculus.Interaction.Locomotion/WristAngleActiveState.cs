using System;
using Oculus.Interaction.Input;
using UnityEngine;

namespace Oculus.Interaction.Locomotion;

public class WristAngleActiveState : MonoBehaviour, IActiveState
{
	[SerializeField]
	[Interface(typeof(IHand), new Type[] { })]
	private UnityEngine.Object _hand;

	[SerializeField]
	private Transform _shoulder;

	[SerializeField]
	private float _minAngle = -70f;

	[SerializeField]
	private float _maxAngle = 170f;

	private float _currentAngle;

	private const float _wristLimit = -70f;

	protected bool _started;

	public IHand Hand { get; private set; }

	public float MinAngle
	{
		get
		{
			return _minAngle;
		}
		set
		{
			_minAngle = value;
		}
	}

	public float MaxAngle
	{
		get
		{
			return _maxAngle;
		}
		set
		{
			_maxAngle = value;
		}
	}

	public bool Active { get; private set; }

	protected virtual void Awake()
	{
		Hand = _hand as IHand;
	}

	protected virtual void Start()
	{
		this.BeginStart(ref _started);
		this.EndStart(ref _started);
	}

	protected virtual void Update()
	{
		_currentAngle = CalculateAngle();
		Active = _currentAngle > _minAngle && _currentAngle < _maxAngle;
	}

	private float CalculateAngle()
	{
		if (!Hand.GetJointPose(HandJointId.HandWristRoot, out var pose))
		{
			return _currentAngle;
		}
		bool flag = Hand.Handedness == Handedness.Right;
		Vector3 up = Vector3.up;
		Vector3 normalized = (pose.position - _shoulder.position).normalized;
		Vector3 normalized2 = Vector3.Cross(up, normalized).normalized;
		normalized2 = (flag ? normalized2 : (-normalized2));
		float num = Vector3.SignedAngle(Vector3.ProjectOnPlane(pose.rotation * (flag ? Constants.RightThumbSide : Constants.LeftThumbSide), normalized).normalized, normalized2, normalized);
		num = ((Hand.Handedness == Handedness.Right) ? (0f - num) : num);
		if (num < -70f)
		{
			num += 360f;
		}
		return num;
	}

	public void InjectAllWristAngleActiveState(IHand hand, Transform shoulder)
	{
		InjectHand(hand);
		InjectShoulder(shoulder);
	}

	public void InjectHand(IHand hand)
	{
		Hand = hand;
		_hand = hand as UnityEngine.Object;
	}

	public void InjectShoulder(Transform shoulder)
	{
		_shoulder = shoulder;
	}
}
