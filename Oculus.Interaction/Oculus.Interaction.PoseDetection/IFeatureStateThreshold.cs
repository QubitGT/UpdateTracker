namespace Oculus.Interaction.PoseDetection;

public interface IFeatureStateThreshold<TFeatureState>
{
	float ToFirstWhenBelow { get; }

	float ToSecondWhenAbove { get; }

	TFeatureState FirstState { get; }

	TFeatureState SecondState { get; }
}
