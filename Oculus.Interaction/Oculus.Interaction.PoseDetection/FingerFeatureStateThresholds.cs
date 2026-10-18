using System.Collections.Generic;
using UnityEngine;

namespace Oculus.Interaction.PoseDetection;

[CreateAssetMenu(menuName = "Meta/Interaction/SDK/Pose Detection/Finger Thresholds")]
public class FingerFeatureStateThresholds : ScriptableObject, IFeatureThresholds<FingerFeature, string>
{
	[SerializeField]
	[Tooltip("List of all supported finger features, along with the state entry/exit thresholds.")]
	private List<FingerFeatureThresholds> _featureThresholds;

	[SerializeField]
	[Tooltip("Length of time that the finger must be in the new state before the feature state provider will use the new value.")]
	private double _minTimeInState;

	public IReadOnlyList<IFeatureStateThresholds<FingerFeature, string>> FeatureStateThresholds => _featureThresholds;

	public double MinTimeInState => _minTimeInState;

	public void Construct(List<FingerFeatureThresholds> featureThresholds, double minTimeInState)
	{
		_featureThresholds = featureThresholds;
		_minTimeInState = minTimeInState;
	}
}
