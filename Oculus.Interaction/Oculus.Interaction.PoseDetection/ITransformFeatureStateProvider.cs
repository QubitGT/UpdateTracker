using UnityEngine;

namespace Oculus.Interaction.PoseDetection;

public interface ITransformFeatureStateProvider
{
	bool IsStateActive(TransformConfig config, TransformFeature feature, FeatureStateActiveMode mode, string stateId);

	bool GetCurrentState(TransformConfig config, TransformFeature transformFeature, out string currentState);

	void RegisterConfig(TransformConfig transformConfig);

	void UnRegisterConfig(TransformConfig transformConfig);

	void GetFeatureVectorAndWristPos(TransformConfig config, TransformFeature transformFeature, bool isHandVector, ref Vector3? featureVec, ref Vector3? wristPos);
}
