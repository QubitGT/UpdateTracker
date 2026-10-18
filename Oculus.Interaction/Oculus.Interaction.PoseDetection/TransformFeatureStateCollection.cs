using System;
using System.Collections.Generic;

namespace Oculus.Interaction.PoseDetection;

internal class TransformFeatureStateCollection
{
	public class TransformStateInfo
	{
		public TransformConfig Config;

		public FeatureStateProvider<TransformFeature, string> StateProvider;

		public TransformStateInfo(TransformConfig transformConfig, FeatureStateProvider<TransformFeature, string> stateProvider)
		{
			Config = transformConfig;
			StateProvider = stateProvider;
		}
	}

	private Dictionary<int, TransformStateInfo> _idToTransformStateInfo = new Dictionary<int, TransformStateInfo>();

	public void RegisterConfig(TransformConfig transformConfig, TransformJointData jointData, Func<float> timeProvider)
	{
		_idToTransformStateInfo.ContainsKey(transformConfig.InstanceId);
		FeatureStateProvider<TransformFeature, string> featureStateProvider = new FeatureStateProvider<TransformFeature, string>((TransformFeature feature) => TransformFeatureValueProvider.GetValue(feature, jointData, transformConfig), (TransformFeature feature) => (int)feature, timeProvider);
		TransformStateInfo value = new TransformStateInfo(transformConfig, featureStateProvider);
		featureStateProvider.InitializeThresholds(transformConfig.FeatureThresholds);
		_idToTransformStateInfo.Add(transformConfig.InstanceId, value);
	}

	public void UnRegisterConfig(TransformConfig transformConfig)
	{
		_idToTransformStateInfo.Remove(transformConfig.InstanceId);
	}

	public FeatureStateProvider<TransformFeature, string> GetStateProvider(TransformConfig transformConfig)
	{
		return _idToTransformStateInfo[transformConfig.InstanceId].StateProvider;
	}

	public void SetConfig(int configId, TransformConfig config)
	{
		_idToTransformStateInfo[configId].Config = config;
	}

	public TransformConfig GetConfig(int configId)
	{
		return _idToTransformStateInfo[configId].Config;
	}

	public void UpdateFeatureStates(int lastUpdatedFrameId, bool disableProactiveEvaluation)
	{
		foreach (TransformStateInfo value in _idToTransformStateInfo.Values)
		{
			FeatureStateProvider<TransformFeature, string> stateProvider = value.StateProvider;
			if (!disableProactiveEvaluation)
			{
				stateProvider.LastUpdatedFrameId = lastUpdatedFrameId;
				stateProvider.ReadTouchedFeatureStates();
			}
			else
			{
				stateProvider.LastUpdatedFrameId = lastUpdatedFrameId;
			}
		}
	}
}
