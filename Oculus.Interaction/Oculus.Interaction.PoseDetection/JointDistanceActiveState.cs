using System;
using Oculus.Interaction.Input;
using UnityEngine;

namespace Oculus.Interaction.PoseDetection;

public class JointDistanceActiveState : MonoBehaviour, IActiveState
{
	[Tooltip("The IHand that JointIdA will be sourced from.")]
	[SerializeField]
	[Interface(typeof(IHand), new Type[] { })]
	private UnityEngine.Object _handA;

	private IHand HandA;

	[Tooltip("The joint of HandA to use for distance check.")]
	[SerializeField]
	private HandJointId _jointIdA;

	[Tooltip("The joint of HandA to use for distance check.")]
	[SerializeField]
	private HandJointId _jointA;

	[Tooltip("The IHand that JointIdB will be sourced from.")]
	[SerializeField]
	[Interface(typeof(IHand), new Type[] { })]
	private UnityEngine.Object _handB;

	private IHand HandB;

	[Tooltip("The joint of HandB to use for distance check.")]
	[SerializeField]
	private HandJointId _jointIdB;

	[Tooltip("The joint of HandB to use for distance check.")]
	[SerializeField]
	private HandJointId _jointB;

	[Tooltip("The ActiveState will become Active when joints are within this distance from each other.")]
	[SerializeField]
	private float _distance = 0.05f;

	[Tooltip("The distance value will be modified by this width to create differing enter/exit thresholds. Used to prevent chattering at the threshold edge.")]
	[SerializeField]
	private float _thresholdWidth = 0.02f;

	[Tooltip("A new state must be maintaned for at least this many seconds before the Active property changes.")]
	[SerializeField]
	private float _minTimeInState = 0.05f;

	private bool _activeState;

	private bool _internalState;

	private float _lastStateChangeTime;

	private int _lastStateUpdateFrame;

	public HandJointId JointIdA
	{
		get
		{
			return _jointA;
		}
		set
		{
			_jointA = value;
		}
	}

	public HandJointId JointIdB
	{
		get
		{
			return _jointB;
		}
		set
		{
			_jointB = value;
		}
	}

	public bool Active
	{
		get
		{
			if (!base.isActiveAndEnabled)
			{
				return false;
			}
			UpdateActiveState();
			return _activeState;
		}
	}

	protected virtual void Awake()
	{
		HandA = _handA as IHand;
		HandB = _handB as IHand;
	}

	protected virtual void Start()
	{
	}

	protected virtual void Update()
	{
		UpdateActiveState();
	}

	private void UpdateActiveState()
	{
		if (Time.frameCount > _lastStateUpdateFrame)
		{
			_lastStateUpdateFrame = Time.frameCount;
			bool flag = JointDistanceWithinThreshold();
			if (flag != _internalState)
			{
				_internalState = flag;
				_lastStateChangeTime = Time.unscaledTime;
			}
			if (Time.unscaledTime - _lastStateChangeTime >= _minTimeInState)
			{
				_activeState = _internalState;
			}
		}
	}

	private bool JointDistanceWithinThreshold()
	{
		if (HandA.GetJointPose(JointIdA, out var pose) && HandB.GetJointPose(JointIdB, out var pose2))
		{
			float num = (_internalState ? (_distance + _thresholdWidth * 0.5f) : (_distance - _thresholdWidth * 0.5f));
			return Vector3.Distance(pose.position, pose2.position) <= num;
		}
		return false;
	}

	public void InjectAllJointDistanceActiveState(IHand handA, IHand handB)
	{
		InjectHandA(handA);
		InjectHandB(handB);
	}

	public void InjectHandA(IHand handA)
	{
		_handA = handA as UnityEngine.Object;
		HandA = handA;
	}

	[Obsolete("Use the JointIdA setter instead")]
	public void InjectJointIdA(HandJointId jointIdA)
	{
		JointIdA = jointIdA;
	}

	public void InjectHandB(IHand handB)
	{
		_handB = handB as UnityEngine.Object;
		HandB = handB;
	}

	[Obsolete("Use the JointIdB setter instead")]
	public void InjectJointIdB(HandJointId jointIdB)
	{
		JointIdB = jointIdB;
	}

	public void InjectOptionalDistance(float val)
	{
		_distance = val;
	}

	public void InjectOptionalThresholdWidth(float val)
	{
		_thresholdWidth = val;
	}

	public void InjectOptionalMinTimeInState(float val)
	{
		_minTimeInState = val;
	}
}
