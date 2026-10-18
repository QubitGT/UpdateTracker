using Oculus.Interaction.Input;
using TMPro;
using UnityEngine;

namespace Oculus.Interaction.PoseDetection.Debug;

public class FingerFeatureDebugVisual : MonoBehaviour
{
	[SerializeField]
	private Renderer _target;

	[SerializeField]
	private Color _normalColor = Color.red;

	[SerializeField]
	private Color _activeColor = Color.green;

	[SerializeField]
	private TextMeshPro _targetText;

	private IFingerFeatureStateProvider _fingerFeatureState;

	private Material _material;

	private bool _lastActiveValue;

	private HandFinger _handFinger;

	private ShapeRecognizer.FingerFeatureConfig _featureConfig;

	private bool _initialized;

	protected virtual void Awake()
	{
		_material = _target.material;
		_material.color = (_lastActiveValue ? _activeColor : _normalColor);
	}

	protected virtual void OnDestroy()
	{
		Object.Destroy(_material);
	}

	public void Initialize(HandFinger handFinger, ShapeRecognizer.FingerFeatureConfig config, IFingerFeatureStateProvider fingerFeatureState)
	{
		_initialized = true;
		_handFinger = handFinger;
		_featureConfig = config;
		_fingerFeatureState = fingerFeatureState;
	}

	protected virtual void Update()
	{
		if (_initialized)
		{
			FingerFeature feature = _featureConfig.Feature;
			bool flag = false;
			if (_fingerFeatureState.GetCurrentState(_handFinger, feature, out var currentState))
			{
				float? featureValue = _fingerFeatureState.GetFeatureValue(_handFinger, feature);
				flag = _fingerFeatureState.IsStateActive(_handFinger, feature, _featureConfig.Mode, _featureConfig.State);
				string text = (featureValue.HasValue ? featureValue.Value.ToString("F2") : "--");
				_targetText.text = $"{_handFinger} {feature}" + currentState + " (" + text + ")";
			}
			else
			{
				_targetText.text = $"{_handFinger} {feature}\n";
			}
			if (flag != _lastActiveValue)
			{
				_material.color = (flag ? _activeColor : _normalColor);
				_lastActiveValue = flag;
			}
		}
	}
}
