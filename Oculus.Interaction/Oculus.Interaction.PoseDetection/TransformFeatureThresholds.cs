using System;
using System.Collections.Generic;
using UnityEngine;

namespace Oculus.Interaction.PoseDetection;

[Serializable]
public class TransformFeatureThresholds : IFeatureStateThresholds<TransformFeature, string>
{
	[SerializeField]
	[Tooltip("Which feature this collection of thresholds controls. Each feature should exist at most once.")]
	private TransformFeature _feature;

	[SerializeField]
	[Tooltip("List of state transitions, with thresold settings. The entries in this list must be in ascending order, based on their 'midpoint' values.")]
	private List<TransformFeatureStateThreshold> _thresholds;

	[SerializeField]
	[Tooltip("Length of time that the transform must be in the new state before the feature state provider will use the new value.")]
	private double _minTimeInState;

	public TransformFeature Feature => _feature;

	public IReadOnlyList<IFeatureStateThreshold<string>> Thresholds => _thresholds;

	public double MinTimeInState => _minTimeInState;

	public TransformFeatureThresholds()
	{
	}

	public TransformFeatureThresholds(TransformFeature featureTransform, IEnumerable<TransformFeatureStateThreshold> thresholds)
	{
		_feature = featureTransform;
		_thresholds = new List<TransformFeatureStateThreshold>(thresholds);
	}
}
