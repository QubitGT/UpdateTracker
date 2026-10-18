using System.Collections.Generic;

namespace Oculus.Interaction.PoseDetection;

public interface IFeatureThresholds<TFeature, TFeatureState>
{
	IReadOnlyList<IFeatureStateThresholds<TFeature, TFeatureState>> FeatureStateThresholds { get; }

	double MinTimeInState { get; }
}
