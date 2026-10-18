using System;
using UnityEngine;

namespace Oculus.Interaction.PoseDetection;

[Serializable]
public class FingerFeatureStateThreshold : IFeatureStateThreshold<string>
{
	[SerializeField]
	[Tooltip("The angle at which a state will transition from A > B (or B > A)")]
	private float _thresholdMidpoint;

	[SerializeField]
	[Tooltip("How far the angle must exceed the midpoint until the transition can occur. This is to prevent rapid flickering at transition edges.")]
	private float _thresholdWidth;

	[SerializeField]
	[Tooltip("State to transition to when value passes below the threshold")]
	private string _firstState;

	[SerializeField]
	[Tooltip("State to transition to when value passes above the threshold")]
	private string _secondState;

	public float ThresholdMidpoint => _thresholdMidpoint;

	public float ThresholdWidth => _thresholdWidth;

	public float ToFirstWhenBelow => _thresholdMidpoint - _thresholdWidth * 0.5f;

	public float ToSecondWhenAbove => _thresholdMidpoint + _thresholdWidth * 0.5f;

	public string FirstState => _firstState;

	public string SecondState => _secondState;

	public FingerFeatureStateThreshold()
	{
	}

	public FingerFeatureStateThreshold(float thresholdMidpoint, float thresholdWidth, string firstState, string secondState)
	{
		_thresholdMidpoint = thresholdMidpoint;
		_thresholdWidth = thresholdWidth;
		_firstState = firstState;
		_secondState = secondState;
	}
}
