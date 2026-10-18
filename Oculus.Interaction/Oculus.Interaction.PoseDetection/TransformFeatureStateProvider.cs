using System;
using Oculus.Interaction.Input;
using UnityEngine;

namespace Oculus.Interaction.PoseDetection;

public class TransformFeatureStateProvider : MonoBehaviour, ITransformFeatureStateProvider, ITimeConsumer
{
	[SerializeField]
	[Interface(typeof(IHand), new Type[] { })]
	private UnityEngine.Object _hand;

	[SerializeField]
	[Interface(typeof(IHmd), new Type[] { })]
	private UnityEngine.Object _hmd;

	[SerializeField]
	[Interface(typeof(ITrackingToWorldTransformer), new Type[] { })]
	private UnityEngine.Object _trackingToWorldTransformer;

	[Header("Advanced Settings")]
	[SerializeField]
	[Tooltip("If true, disables proactive evaluation of any TransformFeature that has been queried at least once. This will force lazy-evaluation of state within calls to IsStateActive, which means you must do so each frame to avoid missing transitions between states.")]
	private bool _disableProactiveEvaluation;

	private Func<float> _timeProvider = () => Time.time;

	private TransformJointData _jointData = new TransformJointData();

	private TransformFeatureStateCollection _transformFeatureStateCollection;

	protected bool _started;

	public IHand Hand { get; private set; }

	public IHmd Hmd { get; private set; }

	public ITrackingToWorldTransformer TrackingToWorldTransformer { get; private set; }

	public void SetTimeProvider(Func<float> timeProvider)
	{
		_timeProvider = timeProvider;
	}

	protected virtual void Awake()
	{
		Hand = _hand as IHand;
		Hmd = _hmd as IHmd;
		TrackingToWorldTransformer = _trackingToWorldTransformer as ITrackingToWorldTransformer;
		_transformFeatureStateCollection = new TransformFeatureStateCollection();
	}

	public void RegisterConfig(TransformConfig transformConfig)
	{
		Func<float> timeProvider = () => _timeProvider();
		_transformFeatureStateCollection.RegisterConfig(transformConfig, _jointData, timeProvider);
	}

	public void UnRegisterConfig(TransformConfig transformConfig)
	{
		_transformFeatureStateCollection.UnRegisterConfig(transformConfig);
	}

	protected virtual void Start()
	{
		this.BeginStart(ref _started);
		this.EndStart(ref _started);
	}

	protected virtual void OnEnable()
	{
		if (_started)
		{
			Hand.WhenHandUpdated += HandDataAvailable;
		}
	}

	protected virtual void OnDisable()
	{
		if (_started)
		{
			Hand.WhenHandUpdated -= HandDataAvailable;
		}
	}

	private void HandDataAvailable()
	{
		UpdateJointData();
		UpdateStateForHand();
	}

	private void UpdateJointData()
	{
		_jointData.IsValid = Hand.GetJointPose(HandJointId.HandWristRoot, out _jointData.WristPose) && Hmd.TryGetRootPose(out _jointData.CenterEyePose);
		if (_jointData.IsValid)
		{
			_jointData.Handedness = Hand.Handedness;
			_jointData.TrackingSystemUp = TrackingToWorldTransformer.Transform.up;
			_jointData.TrackingSystemForward = TrackingToWorldTransformer.Transform.forward;
		}
	}

	private void UpdateStateForHand()
	{
		_transformFeatureStateCollection.UpdateFeatureStates(Hand.CurrentDataVersion, _disableProactiveEvaluation);
	}

	public bool IsHandDataValid()
	{
		return _jointData.IsValid;
	}

	public bool IsStateActive(TransformConfig config, TransformFeature feature, FeatureStateActiveMode mode, string stateId)
	{
		string currentFeatureState = GetCurrentFeatureState(config, feature);
		return mode switch
		{
			FeatureStateActiveMode.Is => currentFeatureState == stateId, 
			FeatureStateActiveMode.IsNot => currentFeatureState != stateId, 
			_ => false, 
		};
	}

	private string GetCurrentFeatureState(TransformConfig config, TransformFeature feature)
	{
		return _transformFeatureStateCollection.GetStateProvider(config).GetCurrentFeatureState(feature);
	}

	public bool GetCurrentState(TransformConfig config, TransformFeature transformFeature, out string currentState)
	{
		if (!IsHandDataValid())
		{
			currentState = null;
			return false;
		}
		currentState = GetCurrentFeatureState(config, transformFeature);
		return currentState != null;
	}

	public float? GetFeatureValue(TransformConfig config, TransformFeature transformFeature)
	{
		if (!IsHandDataValid())
		{
			return null;
		}
		return TransformFeatureValueProvider.GetValue(transformFeature, _jointData, config);
	}

	public void GetFeatureVectorAndWristPos(TransformConfig config, TransformFeature transformFeature, bool isHandVector, ref Vector3? featureVec, ref Vector3? wristPos)
	{
		featureVec = null;
		wristPos = null;
		if (IsHandDataValid())
		{
			featureVec = (isHandVector ? TransformFeatureValueProvider.GetHandVectorForFeature(transformFeature, in _jointData) : TransformFeatureValueProvider.GetTargetVectorForFeature(transformFeature, in _jointData, in config));
			wristPos = _jointData.WristPose.position;
		}
	}

	public void InjectAllTransformFeatureStateProvider(IHand hand, IHmd hmd, bool disableProactiveEvaluation)
	{
		InjectHand(hand);
		InjectHmd(hmd);
		_disableProactiveEvaluation = disableProactiveEvaluation;
	}

	public void InjectHand(IHand hand)
	{
		_hand = hand as UnityEngine.Object;
		Hand = hand;
	}

	public void InjectHmd(IHmd hand)
	{
		_hmd = hand as UnityEngine.Object;
		Hmd = hand;
	}

	public void InjectDisableProactiveEvaluation(bool disabled)
	{
		_disableProactiveEvaluation = disabled;
	}

	[Obsolete("Use SetTimeProvider()")]
	public void InjectOptionalTimeProvider(Func<float> timeProvider)
	{
		_timeProvider = timeProvider;
	}
}
