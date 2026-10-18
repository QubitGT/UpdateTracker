using System;
using System.Collections.Generic;
using Oculus.Interaction.Body.Input;
using UnityEngine;

namespace Oculus.Interaction.Body.PoseDetection;

public class BodyPoseComparerActiveState : MonoBehaviour, IActiveState, ITimeConsumer
{
	public struct BodyPoseComparerFeatureState
	{
		public readonly float Delta;

		public readonly float MaxDelta;

		public BodyPoseComparerFeatureState(float delta, float maxDelta)
		{
			Delta = delta;
			MaxDelta = maxDelta;
		}
	}

	[Serializable]
	public class JointComparerConfig
	{
		[Tooltip("The joint to compare from each Body Pose")]
		public BodyJointId Joint = BodyJointId.Body_Head;

		[Min(0f)]
		[Tooltip("The maximum angle that two joint rotations can be from each other to be considered equal.")]
		public float MaxDelta = 30f;

		[Tooltip("The width of the threshold when transitioning states. Width / 2 is added to MaxDelta when leaving Active state, and subtracted when entering.")]
		[Min(0f)]
		public float Width = 4f;
	}

	[Tooltip("The first body pose to compare.")]
	[SerializeField]
	[Interface(typeof(IBodyPose), new Type[] { })]
	private UnityEngine.Object _poseA;

	private IBodyPose PoseA;

	[Tooltip("The second body pose to compare.")]
	[SerializeField]
	[Interface(typeof(IBodyPose), new Type[] { })]
	private UnityEngine.Object _poseB;

	private IBodyPose PoseB;

	[SerializeField]
	private List<JointComparerConfig> _configs = new List<JointComparerConfig>
	{
		new JointComparerConfig()
	};

	[Tooltip("A new state must be maintaned for at least this many seconds before the Active property changes.")]
	[SerializeField]
	private float _minTimeInState = 0.05f;

	private Func<float> _timeProvider = () => Time.time;

	private Dictionary<JointComparerConfig, BodyPoseComparerFeatureState> _featureStates = new Dictionary<JointComparerConfig, BodyPoseComparerFeatureState>();

	private bool _isActive;

	private bool _internalActive;

	private float _lastStateChangeTime;

	public float MinTimeInState
	{
		get
		{
			return _minTimeInState;
		}
		set
		{
			_minTimeInState = value;
		}
	}

	public IReadOnlyDictionary<JointComparerConfig, BodyPoseComparerFeatureState> FeatureStates => _featureStates;

	public bool Active
	{
		get
		{
			if (!base.isActiveAndEnabled)
			{
				return false;
			}
			bool internalActive = _internalActive;
			_internalActive = true;
			foreach (JointComparerConfig config in _configs)
			{
				float num = (internalActive ? (config.MaxDelta + config.Width / 2f) : (config.MaxDelta - config.Width / 2f));
				float delta;
				bool flag = GetJointDelta(config.Joint, out delta) && Mathf.Abs(delta) <= num;
				_featureStates[config] = new BodyPoseComparerFeatureState(delta, num);
				_internalActive &= flag;
			}
			float num2 = _timeProvider();
			if (internalActive != _internalActive)
			{
				_lastStateChangeTime = num2;
			}
			if (num2 - _lastStateChangeTime >= _minTimeInState)
			{
				_isActive = _internalActive;
			}
			return _isActive;
		}
	}

	public void SetTimeProvider(Func<float> timeProvider)
	{
		_timeProvider = timeProvider;
	}

	protected virtual void Awake()
	{
		PoseA = _poseA as IBodyPose;
		PoseB = _poseB as IBodyPose;
	}

	protected virtual void Start()
	{
	}

	private bool GetJointDelta(BodyJointId joint, out float delta)
	{
		if (!PoseA.GetJointPoseLocal(joint, out var pose) || !PoseB.GetJointPoseLocal(joint, out var pose2))
		{
			delta = 0f;
			return false;
		}
		delta = Quaternion.Angle(pose.rotation, pose2.rotation);
		return true;
	}

	public void InjectAllBodyPoseComparerActiveState(IBodyPose poseA, IBodyPose poseB, IEnumerable<JointComparerConfig> configs)
	{
		InjectPoseA(poseA);
		InjectPoseB(poseB);
		InjectJoints(configs);
	}

	public void InjectPoseA(IBodyPose poseA)
	{
		_poseA = poseA as UnityEngine.Object;
		PoseA = poseA;
	}

	public void InjectPoseB(IBodyPose poseB)
	{
		_poseB = poseB as UnityEngine.Object;
		PoseB = poseB;
	}

	public void InjectJoints(IEnumerable<JointComparerConfig> configs)
	{
		_configs = new List<JointComparerConfig>(configs);
	}

	[Obsolete("Use SetTimeProvider()")]
	public void InjectOptionalTimeProvider(Func<float> timeProvider)
	{
		_timeProvider = timeProvider;
	}
}
