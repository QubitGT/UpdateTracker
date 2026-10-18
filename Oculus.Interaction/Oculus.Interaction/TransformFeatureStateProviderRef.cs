using System;
using Oculus.Interaction.PoseDetection;
using UnityEngine;

namespace Oculus.Interaction;

public class TransformFeatureStateProviderRef : MonoBehaviour, ITransformFeatureStateProvider
{
	[SerializeField]
	[Interface(typeof(ITransformFeatureStateProvider), new Type[] { })]
	private UnityEngine.Object _transformFeatureStateProvider;

	public ITransformFeatureStateProvider TransformFeatureStateProvider { get; private set; }

	protected virtual void Awake()
	{
		TransformFeatureStateProvider = _transformFeatureStateProvider as ITransformFeatureStateProvider;
	}

	protected virtual void Start()
	{
	}

	public bool IsStateActive(TransformConfig config, TransformFeature feature, FeatureStateActiveMode mode, string stateId)
	{
		return TransformFeatureStateProvider.IsStateActive(config, feature, mode, stateId);
	}

	public bool GetCurrentState(TransformConfig config, TransformFeature transformFeature, out string currentState)
	{
		return TransformFeatureStateProvider.GetCurrentState(config, transformFeature, out currentState);
	}

	public void RegisterConfig(TransformConfig transformConfig)
	{
		TransformFeatureStateProvider.RegisterConfig(transformConfig);
	}

	public void UnRegisterConfig(TransformConfig transformConfig)
	{
		TransformFeatureStateProvider.UnRegisterConfig(transformConfig);
	}

	public void GetFeatureVectorAndWristPos(TransformConfig config, TransformFeature transformFeature, bool isHandVector, ref Vector3? featureVec, ref Vector3? wristPos)
	{
		TransformFeatureStateProvider.GetFeatureVectorAndWristPos(config, transformFeature, isHandVector, ref featureVec, ref wristPos);
	}

	public void InjectAllTransformFeatureStateProviderRef(ITransformFeatureStateProvider transformFeatureStateProvider)
	{
		InjectTransformFeatureStateProvider(transformFeatureStateProvider);
	}

	public void InjectTransformFeatureStateProvider(ITransformFeatureStateProvider transformFeatureStateProvider)
	{
		_transformFeatureStateProvider = transformFeatureStateProvider as UnityEngine.Object;
		TransformFeatureStateProvider = transformFeatureStateProvider;
	}
}
