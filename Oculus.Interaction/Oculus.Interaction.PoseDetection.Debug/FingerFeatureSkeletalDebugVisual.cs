using System.Collections.Generic;
using Oculus.Interaction.Input;
using UnityEngine;

namespace Oculus.Interaction.PoseDetection.Debug;

public class FingerFeatureSkeletalDebugVisual : MonoBehaviour
{
	[SerializeField]
	private FingerFeatureStateProvider _fingerFeatureStateProvider;

	[SerializeField]
	private LineRenderer _lineRenderer;

	[SerializeField]
	private Color _normalColor = Color.red;

	[SerializeField]
	private Color _activeColor = Color.green;

	[SerializeField]
	private float _lineWidth = 0.005f;

	private IHand _hand;

	private bool _lastFeatureActiveValue;

	private IReadOnlyList<HandJointId> _jointsCovered;

	private HandFinger _finger;

	private ShapeRecognizer.FingerFeatureConfig _fingerFeatureConfig;

	private bool _initializedPositions;

	private bool _initialized;

	protected virtual void Awake()
	{
		UpdateFeatureActiveValueAndVisual(newValue: false);
	}

	private void UpdateFeatureActiveValueAndVisual(bool newValue)
	{
		Color color = (newValue ? _activeColor : _normalColor);
		_lineRenderer.startColor = color;
		_lineRenderer.endColor = color;
		_lastFeatureActiveValue = newValue;
	}

	public void Initialize(IHand hand, HandFinger finger, ShapeRecognizer.FingerFeatureConfig fingerFeatureConfig)
	{
		_hand = hand;
		_initialized = true;
		FingerShapes valueProvider = _fingerFeatureStateProvider.GetValueProvider(finger);
		_jointsCovered = valueProvider.GetJointsAffected(finger, fingerFeatureConfig.Feature);
		_finger = finger;
		_fingerFeatureConfig = fingerFeatureConfig;
		_initializedPositions = false;
	}

	protected virtual void Update()
	{
		if (!_initialized || !_hand.IsTrackedDataValid)
		{
			ToggleLineRendererEnableState(enableState: false);
			return;
		}
		ToggleLineRendererEnableState(enableState: true);
		UpdateDebugSkeletonLineRendererJoints();
		UpdateFeatureActiveValue();
	}

	private void ToggleLineRendererEnableState(bool enableState)
	{
		if (_lineRenderer.enabled != enableState)
		{
			_lineRenderer.enabled = enableState;
		}
	}

	private void UpdateDebugSkeletonLineRendererJoints()
	{
		if (!_initializedPositions)
		{
			_lineRenderer.positionCount = _jointsCovered.Count;
			_initializedPositions = true;
		}
		if (Mathf.Abs(_lineRenderer.startWidth - _lineWidth) > Mathf.Epsilon)
		{
			_lineRenderer.startWidth = _lineWidth;
			_lineRenderer.endWidth = _lineWidth;
		}
		int count = _jointsCovered.Count;
		for (int i = 0; i < count; i++)
		{
			if (_hand.GetJointPose(_jointsCovered[i], out var pose))
			{
				_lineRenderer.SetPosition(i, pose.position);
			}
		}
	}

	private void UpdateFeatureActiveValue()
	{
		bool flag = _fingerFeatureStateProvider.IsStateActive(_finger, _fingerFeatureConfig.Feature, _fingerFeatureConfig.Mode, _fingerFeatureConfig.State);
		if (flag != _lastFeatureActiveValue)
		{
			UpdateFeatureActiveValueAndVisual(flag);
		}
	}
}
