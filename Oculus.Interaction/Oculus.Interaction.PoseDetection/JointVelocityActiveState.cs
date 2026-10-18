using System;
using System.Collections.Generic;
using Oculus.Interaction.Input;
using UnityEngine;

namespace Oculus.Interaction.PoseDetection;

public class JointVelocityActiveState : MonoBehaviour, IActiveState, ITimeConsumer
{
	public enum RelativeTo
	{
		Hand,
		World,
		Head
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

	public enum HeadAxis
	{
		HeadForward,
		HeadBackward,
		HeadUp,
		HeadDown,
		HeadLeft,
		HeadRight
	}

	public enum HandAxis
	{
		PalmForward,
		PalmBackward,
		WristUp,
		WristDown,
		WristForward,
		WristBackward
	}

	[Serializable]
	public struct JointVelocityFeatureState
	{
		public readonly Vector3 TargetVector;

		public readonly float Amount;

		public JointVelocityFeatureState(Vector3 targetVector, float velocity)
		{
			TargetVector = targetVector;
			Amount = velocity;
		}
	}

	[Serializable]
	public class JointVelocityFeatureConfigList
	{
		[SerializeField]
		private List<JointVelocityFeatureConfig> _values;

		public List<JointVelocityFeatureConfig> Values => _values;
	}

	[Serializable]
	public class JointVelocityFeatureConfig : FeatureConfigBase<HandJointId>
	{
		[Tooltip("The detection axis will be in this coordinate space.")]
		[SerializeField]
		private RelativeTo _relativeTo;

		[Tooltip("The world axis used for detection.")]
		[SerializeField]
		private WorldAxis _worldAxis = WorldAxis.PositiveZ;

		[Tooltip("The axis of the hand root pose used for detection.")]
		[SerializeField]
		private HandAxis _handAxis = HandAxis.WristForward;

		[Tooltip("The axis of the head pose used for detection.")]
		[SerializeField]
		private HeadAxis _headAxis;

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

		public HeadAxis HeadAxis
		{
			get
			{
				return _headAxis;
			}
			set
			{
				_headAxis = value;
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

	[Tooltip("Reference to the Hmd providing the HeadAxis pose.")]
	[SerializeField]
	[Optional]
	[Interface(typeof(IHmd), new Type[] { })]
	private UnityEngine.Object _hmd;

	[SerializeField]
	private JointVelocityFeatureConfigList _featureConfigs;

	[SerializeField]
	private JointVelocityFeatureConfigList _featureConfigurations;

	[Tooltip("The velocity used for the detection threshold, in units per second.")]
	[SerializeField]
	[Min(0f)]
	private float _minVelocity = 0.5f;

	[Tooltip("The min velocity value will be modified by this width to create differing enter/exit thresholds. Used to prevent chattering at the threshold edge.")]
	[SerializeField]
	[Min(0f)]
	private float _thresholdWidth = 0.02f;

	[Tooltip("A new state must be maintaned for at least this many seconds before the Active property changes.")]
	[SerializeField]
	[Min(0f)]
	private float _minTimeInState = 0.05f;

	private Func<float> _timeProvider = () => Time.time;

	private Dictionary<JointVelocityFeatureConfig, JointVelocityFeatureState> _featureStates = new Dictionary<JointVelocityFeatureConfig, JointVelocityFeatureState>();

	private JointDeltaConfig _jointDeltaConfig;

	private int _lastStateUpdateFrame;

	private float _lastStateChangeTime;

	private float _lastUpdateTime;

	private bool _internalState;

	private bool _activeState;

	protected bool _started;

	public IHand Hand { get; private set; }

	public IJointDeltaProvider JointDeltaProvider { get; private set; }

	public IHmd Hmd { get; private set; }

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

	public IReadOnlyList<JointVelocityFeatureConfig> FeatureConfigs => _featureConfigurations.Values;

	public IReadOnlyDictionary<JointVelocityFeatureConfig, JointVelocityFeatureState> FeatureStates => _featureStates;

	public void SetTimeProvider(Func<float> timeProvider)
	{
		_timeProvider = timeProvider;
	}

	protected virtual void Awake()
	{
		Hand = _hand as IHand;
		JointDeltaProvider = _jointDeltaProvider as IJointDeltaProvider;
		if (_hmd != null)
		{
			Hmd = _hmd as IHmd;
		}
	}

	protected virtual void Start()
	{
		this.BeginStart(ref _started);
		IList<HandJointId> list = new List<HandJointId>();
		foreach (JointVelocityFeatureConfig featureConfig in FeatureConfigs)
		{
			list.Add(featureConfig.Feature);
			_featureStates.Add(featureConfig, default(JointVelocityFeatureState));
		}
		_jointDeltaConfig = new JointDeltaConfig(GetInstanceID(), list);
		_lastUpdateTime = _timeProvider();
		this.EndStart(ref _started);
	}

	private bool CheckAllJointVelocities()
	{
		bool flag = true;
		float num = _timeProvider() - _lastUpdateTime;
		float num2 = (_internalState ? (_minVelocity + _thresholdWidth * 0.5f) : (_minVelocity - _thresholdWidth * 0.5f));
		num2 *= num;
		foreach (JointVelocityFeatureConfig featureConfig in FeatureConfigs)
		{
			if (Hand.GetJointPose(HandJointId.HandWristRoot, out var pose) && Hand.GetJointPose(featureConfig.Feature, out var _) && JointDeltaProvider.GetPositionDelta(featureConfig.Feature, out var delta))
			{
				Vector3 worldTargetVector = GetWorldTargetVector(pose, featureConfig);
				float num3 = Vector3.Dot(delta, worldTargetVector);
				_featureStates[featureConfig] = new JointVelocityFeatureState(worldTargetVector, (num2 > 0f) ? Mathf.Clamp01(num3 / num2) : 1f);
				bool flag2 = num3 > num2;
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
			bool flag = CheckAllJointVelocities();
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

	private Vector3 GetWorldTargetVector(Pose wristPose, JointVelocityFeatureConfig config)
	{
		return config.RelativeTo switch
		{
			RelativeTo.World => GetWorldAxisVector(config.WorldAxis), 
			RelativeTo.Head => GetHeadAxisVector(config.HeadAxis), 
			_ => GetHandAxisVector(config.HandAxis, wristPose), 
		};
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
			HandAxis.PalmForward => wristPose.rotation * ((Hand.Handedness == Handedness.Left) ? Constants.LeftPalmar : Constants.RightPalmar), 
			HandAxis.PalmBackward => wristPose.rotation * ((Hand.Handedness == Handedness.Left) ? Constants.LeftDorsal : Constants.RightDorsal), 
			HandAxis.WristUp => wristPose.rotation * ((Hand.Handedness == Handedness.Left) ? Constants.LeftThumbSide : Constants.RightThumbSide), 
			HandAxis.WristDown => wristPose.rotation * ((Hand.Handedness == Handedness.Left) ? Constants.LeftPinkySide : Constants.RightPinkySide), 
			HandAxis.WristForward => wristPose.rotation * ((Hand.Handedness == Handedness.Left) ? Constants.LeftDistal : Constants.RightDistal), 
			HandAxis.WristBackward => wristPose.rotation * ((Hand.Handedness == Handedness.Left) ? Constants.LeftProximal : Constants.RightProximal), 
			_ => Vector3.zero, 
		};
	}

	private Vector3 GetHeadAxisVector(HeadAxis axis)
	{
		Hmd.TryGetRootPose(out var pose);
		return axis switch
		{
			HeadAxis.HeadForward => pose.forward, 
			HeadAxis.HeadBackward => -pose.forward, 
			HeadAxis.HeadUp => pose.up, 
			HeadAxis.HeadDown => -pose.up, 
			HeadAxis.HeadRight => pose.right, 
			HeadAxis.HeadLeft => -pose.right, 
			_ => Vector3.zero, 
		};
	}

	public void InjectAllJointVelocityActiveState(JointVelocityFeatureConfigList featureConfigs, IHand hand, IJointDeltaProvider jointDeltaProvider)
	{
		InjectFeatureConfigList(featureConfigs);
		InjectHand(hand);
		InjectJointDeltaProvider(jointDeltaProvider);
	}

	public void InjectFeatureConfigList(JointVelocityFeatureConfigList featureConfigs)
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

	public void InjectOptionalHmd(IHmd hmd)
	{
		_hmd = hmd as UnityEngine.Object;
		Hmd = hmd;
	}
}
