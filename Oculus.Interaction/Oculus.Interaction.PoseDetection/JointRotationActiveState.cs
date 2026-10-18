using System;
using System.Collections.Generic;
using Oculus.Interaction.Input;
using UnityEngine;

namespace Oculus.Interaction.PoseDetection;

public class JointRotationActiveState : MonoBehaviour, IActiveState, ITimeConsumer
{
	public enum RelativeTo
	{
		Hand,
		World
	}

	public enum WorldAxis
	{
		PositiveX,
		NegativeX,
		PositiveY,
		NegativeY,
		PositiveZ,
		NegativeZ
	}

	public enum HandAxis
	{
		Pronation,
		Supination,
		RadialDeviation,
		UlnarDeviation,
		Extension,
		Flexion
	}

	[Serializable]
	public struct JointRotationFeatureState
	{
		public readonly Vector3 TargetAxis;

		public readonly float Amount;

		public JointRotationFeatureState(Vector3 targetAxis, float amount)
		{
			TargetAxis = targetAxis;
			Amount = amount;
		}
	}

	[Serializable]
	public class JointRotationFeatureConfigList
	{
		[SerializeField]
		private List<JointRotationFeatureConfig> _values;

		public List<JointRotationFeatureConfig> Values => _values;
	}

	[Serializable]
	public class JointRotationFeatureConfig : FeatureConfigBase<HandJointId>
	{
		[Tooltip("The detection axis will be in this coordinate space.")]
		[SerializeField]
		private RelativeTo _relativeTo;

		[Tooltip("The world axis used for detection.")]
		[SerializeField]
		private WorldAxis _worldAxis = WorldAxis.PositiveZ;

		[Tooltip("The axis of the hand root pose used for detection.")]
		[SerializeField]
		private HandAxis _handAxis = HandAxis.RadialDeviation;

		public RelativeTo RelativeTo
		{
			get
			{
				return _relativeTo;
			}
			set
			{
				_relativeTo = value;
			}
		}

		public WorldAxis WorldAxis
		{
			get
			{
				return _worldAxis;
			}
			set
			{
				_worldAxis = value;
			}
		}

		public HandAxis HandAxis
		{
			get
			{
				return _handAxis;
			}
			set
			{
				_handAxis = value;
			}
		}
	}

	[Tooltip("Provided joints will be sourced from this IHand.")]
	[SerializeField]
	[Interface(typeof(IHand), new Type[] { })]
	private UnityEngine.Object _hand;

	[Tooltip("JointDeltaProvider caches joint deltas to avoid unnecessary recomputing of deltas.")]
	[SerializeField]
	[Interface(typeof(IJointDeltaProvider), new Type[] { })]
	private UnityEngine.Object _jointDeltaProvider;

	[SerializeField]
	private JointRotationFeatureConfigList _featureConfigs;

	[SerializeField]
	private JointRotationFeatureConfigList _featureConfigurations;

	[Tooltip("The angular velocity used for the detection threshold, in degrees per second.")]
	[SerializeField]
	[Min(0f)]
	private float _degreesPerSecond = 120f;

	[Tooltip("The degrees per second value will be modified by this width to create differing enter/exit thresholds. Used to prevent chattering at the threshold edge.")]
	[SerializeField]
	[Min(0f)]
	private float _thresholdWidth = 30f;

	[Tooltip("A new state must be maintaned for at least this many seconds before the Active property changes.")]
	[SerializeField]
	[Min(0f)]
	private float _minTimeInState = 0.05f;

	private Func<float> _timeProvider = () => Time.time;

	private Dictionary<JointRotationFeatureConfig, JointRotationFeatureState> _featureStates = new Dictionary<JointRotationFeatureConfig, JointRotationFeatureState>();

	private JointDeltaConfig _jointDeltaConfig;

	private IJointDeltaProvider JointDeltaProvider;

	private int _lastStateUpdateFrame;

	private float _lastStateChangeTime;

	private float _lastUpdateTime;

	private bool _internalState;

	private bool _activeState;

	protected bool _started;

	public IHand Hand { get; private set; }

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

	public IReadOnlyList<JointRotationFeatureConfig> FeatureConfigs => _featureConfigurations.Values;

	public IReadOnlyDictionary<JointRotationFeatureConfig, JointRotationFeatureState> FeatureStates => _featureStates;

	public void SetTimeProvider(Func<float> timeProvider)
	{
		_timeProvider = timeProvider;
	}

	protected virtual void Awake()
	{
		Hand = _hand as IHand;
		JointDeltaProvider = _jointDeltaProvider as IJointDeltaProvider;
	}

	protected virtual void Start()
	{
		this.BeginStart(ref _started);
		IList<HandJointId> list = new List<HandJointId>();
		foreach (JointRotationFeatureConfig featureConfig in FeatureConfigs)
		{
			list.Add(featureConfig.Feature);
			_featureStates.Add(featureConfig, default(JointRotationFeatureState));
		}
		_jointDeltaConfig = new JointDeltaConfig(GetInstanceID(), list);
		_lastUpdateTime = _timeProvider();
		this.EndStart(ref _started);
	}

	private bool CheckAllJointRotations()
	{
		bool flag = true;
		float num = _timeProvider() - _lastUpdateTime;
		float num2 = (_internalState ? (_degreesPerSecond + _thresholdWidth * 0.5f) : (_degreesPerSecond - _thresholdWidth * 0.5f));
		num2 *= num;
		foreach (JointRotationFeatureConfig featureConfig in FeatureConfigs)
		{
			if (Hand.GetJointPose(HandJointId.HandWristRoot, out var pose) && Hand.GetJointPose(featureConfig.Feature, out var _) && JointDeltaProvider.GetRotationDelta(featureConfig.Feature, out var delta))
			{
				Vector3 worldTargetAxis = GetWorldTargetAxis(pose, featureConfig);
				delta.ToAngleAxis(out var angle, out var axis);
				float num3 = Mathf.Abs(Vector3.Dot(axis, worldTargetAxis));
				float num4 = angle * num3;
				_featureStates[featureConfig] = new JointRotationFeatureState(worldTargetAxis, (num2 <= 0f) ? 1f : Mathf.Clamp01(num4 / num2));
				bool flag2 = num4 > num2;
				flag = flag && flag2;
			}
			else
			{
				flag = false;
			}
		}
		return flag;
	}

	protected virtual void Update()
	{
		UpdateActiveState();
	}

	protected virtual void OnEnable()
	{
		if (_started)
		{
			JointDeltaProvider.RegisterConfig(_jointDeltaConfig);
		}
	}

	protected virtual void OnDisable()
	{
		if (_started)
		{
			JointDeltaProvider.UnRegisterConfig(_jointDeltaConfig);
		}
	}

	private void UpdateActiveState()
	{
		if (Time.frameCount > _lastStateUpdateFrame)
		{
			_lastStateUpdateFrame = Time.frameCount;
			bool flag = CheckAllJointRotations();
			if (flag != _internalState)
			{
				_internalState = flag;
				_lastStateChangeTime = _timeProvider();
			}
			if (_timeProvider() - _lastStateChangeTime >= _minTimeInState)
			{
				_activeState = _internalState;
			}
			_lastUpdateTime = _timeProvider();
		}
	}

	private Vector3 GetWorldTargetAxis(Pose wristPose, JointRotationFeatureConfig config)
	{
		RelativeTo relativeTo = config.RelativeTo;
		if (relativeTo == RelativeTo.Hand || relativeTo != RelativeTo.World)
		{
			return GetHandAxisVector(config.HandAxis, wristPose);
		}
		return GetWorldAxisVector(config.WorldAxis);
	}

	private Vector3 GetWorldAxisVector(WorldAxis axis)
	{
		return axis switch
		{
			WorldAxis.NegativeX => Vector3.left, 
			WorldAxis.PositiveY => Vector3.up, 
			WorldAxis.NegativeY => Vector3.down, 
			WorldAxis.PositiveZ => Vector3.forward, 
			WorldAxis.NegativeZ => Vector3.back, 
			_ => Vector3.right, 
		};
	}

	private Vector3 GetHandAxisVector(HandAxis axis, Pose wristPose)
	{
		return axis switch
		{
			HandAxis.Pronation => wristPose.rotation * ((Hand.Handedness == Handedness.Left) ? Constants.LeftProximal : Constants.RightProximal), 
			HandAxis.Supination => wristPose.rotation * ((Hand.Handedness == Handedness.Left) ? Constants.LeftDistal : Constants.RightDistal), 
			HandAxis.RadialDeviation => wristPose.rotation * ((Hand.Handedness == Handedness.Left) ? Constants.LeftPalmar : Constants.RightPalmar), 
			HandAxis.UlnarDeviation => wristPose.rotation * ((Hand.Handedness == Handedness.Left) ? Constants.LeftDorsal : Constants.RightDorsal), 
			HandAxis.Extension => wristPose.rotation * ((Hand.Handedness == Handedness.Left) ? Constants.LeftThumbSide : Constants.RightThumbSide), 
			HandAxis.Flexion => wristPose.rotation * ((Hand.Handedness == Handedness.Left) ? Constants.LeftPinkySide : Constants.RightPinkySide), 
			_ => Vector3.zero, 
		};
	}

	public void InjectAllJointRotationActiveState(JointRotationFeatureConfigList featureConfigs, IHand hand, IJointDeltaProvider jointDeltaProvider)
	{
		InjectFeatureConfigList(featureConfigs);
		InjectHand(hand);
		InjectJointDeltaProvider(jointDeltaProvider);
	}

	public void InjectFeatureConfigList(JointRotationFeatureConfigList featureConfigs)
	{
		_featureConfigs = featureConfigs;
	}

	public void InjectHand(IHand hand)
	{
		_hand = hand as UnityEngine.Object;
		Hand = hand;
	}

	public void InjectJointDeltaProvider(IJointDeltaProvider jointDeltaProvider)
	{
		JointDeltaProvider = jointDeltaProvider;
		_jointDeltaProvider = jointDeltaProvider as UnityEngine.Object;
	}

	[Obsolete("Use SetTimeProvider()")]
	public void InjectOptionalTimeProvider(Func<float> timeProvider)
	{
		_timeProvider = timeProvider;
	}
}
