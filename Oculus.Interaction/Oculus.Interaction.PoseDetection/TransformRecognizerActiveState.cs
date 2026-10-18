using System;
using System.Collections.Generic;
using Oculus.Interaction.Input;
using UnityEngine;

namespace Oculus.Interaction.PoseDetection;

public class TransformRecognizerActiveState : MonoBehaviour, IActiveState
{
	[SerializeField]
	[Interface(typeof(IHand), new Type[] { })]
	private UnityEngine.Object _hand;

	[SerializeField]
	[Interface(typeof(ITransformFeatureStateProvider), new Type[] { })]
	private UnityEngine.Object _transformFeatureStateProvider;

	protected ITransformFeatureStateProvider TransformFeatureStateProvider;

	[SerializeField]
	private TransformFeatureConfigList _transformFeatureConfigs;

	[SerializeField]
	[Tooltip("State provider uses this to determine the state of features during real time, so edit at runtime at your own risk.")]
	private TransformConfig _transformConfig;

	protected bool _started;

	public IHand Hand { get; private set; }

	public IReadOnlyList<TransformFeatureConfig> FeatureConfigs => _transformFeatureConfigs.Values;

	public TransformConfig TransformConfig => _transformConfig;

	public bool Active
	{
		get
		{
			if (!base.isActiveAndEnabled)
			{
				return false;
			}
			foreach (TransformFeatureConfig featureConfig in FeatureConfigs)
			{
				if (!TransformFeatureStateProvider.IsStateActive(_transformConfig, featureConfig.Feature, featureConfig.Mode, featureConfig.State))
				{
					return false;
				}
			}
			return true;
		}
	}

	protected virtual void Awake()
	{
		Hand = _hand as IHand;
		TransformFeatureStateProvider = _transformFeatureStateProvider as ITransformFeatureStateProvider;
	}

	protected virtual void Start()
	{
		this.BeginStart(ref _started);
		_transformConfig.InstanceId = GetInstanceID();
		this.EndStart(ref _started);
	}

	protected virtual void OnEnable()
	{
		if (_started)
		{
			TransformFeatureStateProvider.RegisterConfig(_transformConfig);
			InitStateProvider();
		}
	}

	protected virtual void OnDisable()
	{
		if (_started)
		{
			TransformFeatureStateProvider.UnRegisterConfig(_transformConfig);
		}
	}

	private void InitStateProvider()
	{
		foreach (TransformFeatureConfig featureConfig in FeatureConfigs)
		{
			TransformFeatureStateProvider.GetCurrentState(_transformConfig, featureConfig.Feature, out var _);
		}
	}

	public void GetFeatureVectorAndWristPos(TransformFeature feature, bool isHandVector, ref Vector3? featureVec, ref Vector3? wristPos)
	{
		TransformFeatureStateProvider.GetFeatureVectorAndWristPos(TransformConfig, feature, isHandVector, ref featureVec, ref wristPos);
	}

	public void InjectAllTransformRecognizerActiveState(IHand hand, ITransformFeatureStateProvider transformFeatureStateProvider, TransformFeatureConfigList transformFeatureList, TransformConfig transformConfig)
	{
		InjectHand(hand);
		InjectTransformFeatureStateProvider(transformFeatureStateProvider);
		InjectTransformFeatureList(transformFeatureList);
		InjectTransformConfig(transformConfig);
	}

	public void InjectHand(IHand hand)
	{
		_hand = hand as UnityEngine.Object;
		Hand = hand;
	}

	public void InjectTransformFeatureStateProvider(ITransformFeatureStateProvider transformFeatureStateProvider)
	{
		TransformFeatureStateProvider = transformFeatureStateProvider;
		_transformFeatureStateProvider = transformFeatureStateProvider as UnityEngine.Object;
	}

	public void InjectTransformFeatureList(TransformFeatureConfigList transformFeatureList)
	{
		_transformFeatureConfigs = transformFeatureList;
	}

	public void InjectTransformConfig(TransformConfig transformConfig)
	{
		_transformConfig = transformConfig;
	}
}
