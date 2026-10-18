using Oculus.Interaction.Input;

namespace Oculus.Interaction.PoseDetection;

public interface IFingerFeatureStateProvider
{
	bool GetCurrentState(HandFinger finger, FingerFeature fingerFeature, out string currentState);

	bool IsStateActive(HandFinger finger, FingerFeature feature, FeatureStateActiveMode mode, string stateId);

	float? GetFeatureValue(HandFinger finger, FingerFeature fingerFeature);
}
