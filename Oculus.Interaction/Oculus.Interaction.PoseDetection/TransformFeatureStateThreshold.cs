using System;
using UnityEngine;

namespace Oculus.Interaction.PoseDetection;

[Serializable]
public class TransformFeatureStateThreshold : IFeatureStateThreshold<string>
{
	[SerializeField]
	[Tooltip("The value at which a state will transition from A > B (or B > A)")]
	private float _thresholdMidpoint;

	[SerializeField]
	[Tooltip("How far the transform value must exceed the midpoint until the transition can occur. This is to prevent rapid flickering at transition edges.")]
	private float _thresholdWidth;

	[SerializeField]
	[Tooltip("State to transition to when value passes below the threshold")]
	private string _firstState;

	[SerializeField]
	[Tooltip("State to transition to when value passes above the threshold")]
	private string _secondState;

	public float ToFirstWhenBelow => _thresholdMidpoint - _thresholdWidth * 0.5f;

	public float ToSecondWhenAbove => _thresholdMidpoint + _thresholdWidth * 0.5f;

	public string FirstState => _firstState;

	public string SecondState => _secondState;

	public TransformFeatureStateThreshold()
	{
	}

	public TransformFeatureStateThreshold(float thresholdMidpoint, float thresholdWidth, string firstState, string secondState)
	{
		_thresholdMidpoint = thresholdMidpoint;
		_thresholdWidth = thresholdWidth;
		_firstState = firstState;
		_secondState = secondState;
	}
}
