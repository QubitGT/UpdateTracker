using Oculus.Interaction.Input;

namespace Oculus.Interaction.PoseDetection;

internal class FingerFeatureStateDictionary
{
	private struct HandFingerState
	{
		public FeatureStateProvider<FingerFeature, string> StateProvider;
	}

	private readonly HandFingerState[] _fingerState = new HandFingerState[5];

	public void InitializeFinger(HandFinger finger, FeatureStateProvider<FingerFeature, string> stateProvider)
	{
		_fingerState[(int)finger] = new HandFingerState
		{
			StateProvider = stateProvider
		};
	}

	public FeatureStateProvider<FingerFeature, string> GetStateProvider(HandFinger finger)
	{
		return _fingerState[(int)finger].StateProvider;
	}
}
