using System;
using System.Collections.Generic;
using UnityEngine;

namespace Oculus.Interaction.PoseDetection;

[Serializable]
public class FingerFeatureThresholds : IFeatureStateThresholds<FingerFeature, string>
{
	[SerializeField]
	[Tooltip("Which feature this collection of thresholds controls. Each feature should exist at most once.")]
	private FingerFeature _feature;

	[SerializeField]
	[Tooltip("List of state transitions, with thresold settings. The entries in this list must be in ascending order, based on their 'midpoint' values.")]
	private List<FingerFeatureStateThreshold> _thresholds;

	public FingerFeature Feature => _feature;

	public IReadOnlyList<IFeatureStateThreshold<string>> Thresholds => _thresholds;

	public FingerFeatureThresholds()
	{
	}

	public FingerFeatureThresholds(FingerFeature feature, IEnumerable<FingerFeatureStateThreshold> thresholds)
	{
		_feature = feature;
		_thresholds = new List<FingerFeatureStateThreshold>(thresholds);
	}
}
