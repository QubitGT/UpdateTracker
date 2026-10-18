using System.Collections.Generic;

namespace Oculus.Interaction.PoseDetection;

public interface IFeatureStateThresholds<TFeature, TFeatureState>
{
	TFeature Feature { get; }

	IReadOnlyList<IFeatureStateThreshold<TFeatureState>> Thresholds { get; }
}
