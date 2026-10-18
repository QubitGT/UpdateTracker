using System;
using System.Collections.Generic;
using Oculus.Interaction.Input;
using UnityEngine;

namespace Oculus.Interaction.PoseDetection;

public class FingerFeatureStateProvider : MonoBehaviour, IFingerFeatureStateProvider, ITimeConsumer
{
	[Serializable]
	public struct FingerStateThresholds
	{
		[Tooltip("Which finger the state thresholds apply to.")]
		public HandFinger Finger;

		[Tooltip("State threshold configuration")]
		public FingerFeatureStateThresholds StateThresholds;
	}

	[SerializeField]
	[Interface(typeof(IHand), new Type[] { })]
	[Tooltip("Data source used to retrieve finger bone rotations.")]
	private UnityEngine.Object _hand;

	[SerializeField]
	[Tooltip("Contains state transition threasholds for each finger. Must contain 5 entries (one for each finger). Each finger must exist in the list exactly once.")]
	private List<FingerStateThresholds> _fingerStateThresholds;

	[Header("Advanced Settings")]
	[SerializeField]
	[Tooltip("If true, disables proactive evaluation of any FingerFeature that has been queried at least once. This will force lazy-evaluation of state within calls to IsStateActive, which means you must call IsStateActive for each feature manually each frame to avoid missing transitions between states.")]
	private bool _disableProactiveEvaluation;

	protected bool _started;

	private FingerFeatureStateDictionary _state;

	private Func<float> _timeProvider = () => Time.time;

	private FingerShapes _fingerShapes = DefaultFingerShapes;

	private ReadOnlyHandJointPoses _handJointPoses;

	public IHand Hand { get; private set; }

	public static FingerShapes DefaultFingerShapes { get; } = new FingerShapes();


	public void SetTimeProvider(Func<float> timeProvider)
	{
		_timeProvider = timeProvider;
	}

	protected virtual void Awake()
	{
		Hand = _hand as IHand;
		_state = new FingerFeatureStateDictionary();
		_handJointPoses = ReadOnlyHandJointPoses.Empty;
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
			ReadStateThresholds();
		}
	}

	protected virtual void OnDisable()
	{
		if (_started)
		{
			Hand.WhenHandUpdated -= HandDataAvailable;
			_handJointPoses = ReadOnlyHandJointPoses.Empty;
		}
	}

	private void ReadStateThresholds()
	{
		HandFingerFlags handFingerFlags = HandFingerFlags.None;
		foreach (FingerStateThresholds fingerStateThreshold in _fingerStateThresholds)
		{
			handFingerFlags |= HandFingerUtils.ToFlags(fingerStateThreshold.Finger);
			HandFinger finger = fingerStateThreshold.Finger;
			FeatureStateProvider<FingerFeature, string> featureStateProvider = _state.GetStateProvider(finger);
			if (featureStateProvider == null)
			{
				Func<float> timeProvider = () => _timeProvider();
				featureStateProvider = new FeatureStateProvider<FingerFeature, string>((FingerFeature feature) => GetFeatureValue(finger, feature), (FingerFeature feature) => (int)feature, timeProvider);
				_state.InitializeFinger(fingerStateThreshold.Finger, featureStateProvider);
			}
			featureStateProvider.InitializeThresholds(fingerStateThreshold.StateThresholds);
		}
	}

	private void HandDataAvailable()
	{
		int currentDataVersion = Hand.CurrentDataVersion;
		if (!Hand.GetJointPosesFromWrist(out _handJointPoses))
		{
			return;
		}
		if (!_disableProactiveEvaluation)
		{
			for (int i = 0; i < 5; i++)
			{
				FeatureStateProvider<FingerFeature, string> stateProvider = _state.GetStateProvider((HandFinger)i);
				stateProvider.LastUpdatedFrameId = currentDataVersion;
				stateProvider.ReadTouchedFeatureStates();
			}
		}
		else
		{
			for (int j = 0; j < 5; j++)
			{
				_state.GetStateProvider((HandFinger)j).LastUpdatedFrameId = currentDataVersion;
			}
		}
	}

	public bool GetCurrentState(HandFinger finger, FingerFeature fingerFeature, out string currentState)
	{
		if (!IsDataValid())
		{
			currentState = null;
			return false;
		}
		currentState = GetCurrentFingerFeatureState(finger, fingerFeature);
		return currentState != null;
	}

	private string GetCurrentFingerFeatureState(HandFinger finger, FingerFeature fingerFeature)
	{
		return _state.GetStateProvider(finger).GetCurrentFeatureState(fingerFeature);
	}

	public float? GetFeatureValue(HandFinger finger, FingerFeature fingerFeature)
	{
		if (!IsDataValid())
		{
			return null;
		}
		return _fingerShapes.GetValue(finger, fingerFeature, Hand);
	}

	private bool IsDataValid()
	{
		return _handJointPoses.Count > 0;
	}

	public FingerShapes GetValueProvider(HandFinger finger)
	{
		return _fingerShapes;
	}

	public bool IsStateActive(HandFinger finger, FingerFeature feature, FeatureStateActiveMode mode, string stateId)
	{
		string currentFingerFeatureState = GetCurrentFingerFeatureState(finger, feature);
		return mode switch
		{
			FeatureStateActiveMode.Is => currentFingerFeatureState == stateId, 
			FeatureStateActiveMode.IsNot => currentFingerFeatureState != stateId, 
			_ => false, 
		};
	}

	public void InjectAllFingerFeatureStateProvider(IHand hand, List<FingerStateThresholds> fingerStateThresholds, FingerShapes fingerShapes, bool disableProactiveEvaluation)
	{
		InjectHand(hand);
		InjectFingerStateThresholds(fingerStateThresholds);
		InjectFingerShapes(fingerShapes);
		InjectDisableProactiveEvaluation(disableProactiveEvaluation);
	}

	public void InjectHand(IHand hand)
	{
		_hand = hand as UnityEngine.Object;
		Hand = hand;
	}

	public void InjectFingerStateThresholds(List<FingerStateThresholds> fingerStateThresholds)
	{
		_fingerStateThresholds = fingerStateThresholds;
	}

	public void InjectFingerShapes(FingerShapes fingerShapes)
	{
		_fingerShapes = fingerShapes;
	}

	public void InjectDisableProactiveEvaluation(bool disableProactiveEvaluation)
	{
		_disableProactiveEvaluation = disableProactiveEvaluation;
	}

	[Obsolete("Use SetTimeProvider()")]
	public void InjectOptionalTimeProvider(Func<float> timeProvider)
	{
		_timeProvider = timeProvider;
	}
}
