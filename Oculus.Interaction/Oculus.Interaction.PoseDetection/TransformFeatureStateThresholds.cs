using System.Collections.Generic;
using UnityEngine;

namespace Oculus.Interaction.PoseDetection;

[CreateAssetMenu(menuName = "Meta/Interaction/SDK/Pose Detection/Transform Thresholds")]
public class TransformFeatureStateThresholds : ScriptableObject, IFeatureThresholds<TransformFeature, string>
{
	[SerializeField]
	[Tooltip("List of all supported transform features, along with the state entry/exit thresholds.")]
	private List<TransformFeatureThresholds> _featureThresholds;

	[SerializeField]
	[Tooltip("Length of time that the transform must be in the new state before the feature state provider will use the new value.")]
	private double _minTimeInState;

	public IReadOnlyList<IFeatureStateThresholds<TransformFeature, string>> FeatureStateThresholds => _featureThresholds;

	public double MinTimeInState => _minTimeInState;

	public void Construct(List<TransformFeatureThresholds> featureThresholds, double minTimeInState)
	{
		_featureThresholds = featureThresholds;
		_minTimeInState = minTimeInState;
	}
}
