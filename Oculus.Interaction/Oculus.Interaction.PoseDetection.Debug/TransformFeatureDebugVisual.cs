using Oculus.Interaction.Input;
using TMPro;
using UnityEngine;

namespace Oculus.Interaction.PoseDetection.Debug;

public class TransformFeatureDebugVisual : MonoBehaviour
{
	[SerializeField]
	private Renderer _target;

	[SerializeField]
	private Color _normalColor = Color.red;

	[SerializeField]
	private Color _activeColor = Color.green;

	[SerializeField]
	private TextMeshPro _targetText;

	private TransformFeatureStateProvider _transformFeatureStateProvider;

	private TransformRecognizerActiveState _transformRecognizerActiveState;

	private Material _material;

	private bool _lastActiveValue;

	private TransformFeatureConfig _targetConfig;

	private bool _initialized;

	private Handedness _handedness;

	protected virtual void Awake()
	{
		_material = _target.material;
		_material.color = (_lastActiveValue ? _activeColor : _normalColor);
	}

	private void OnDestroy()
	{
		Object.Destroy(_material);
	}

	public void Initialize(Handedness handedness, TransformFeatureConfig targetConfig, TransformFeatureStateProvider transformFeatureStateProvider, TransformRecognizerActiveState transformActiveState)
	{
		_handedness = handedness;
		_initialized = true;
		_transformFeatureStateProvider = transformFeatureStateProvider;
		_transformRecognizerActiveState = transformActiveState;
		_targetConfig = targetConfig;
	}

	protected virtual void Update()
	{
		if (_initialized)
		{
			bool flag = false;
			TransformFeature feature = _targetConfig.Feature;
			if (_transformFeatureStateProvider.GetCurrentState(_transformRecognizerActiveState.TransformConfig, feature, out var currentState))
			{
				float? featureValue = _transformFeatureStateProvider.GetFeatureValue(_transformRecognizerActiveState.TransformConfig, feature);
				flag = _transformFeatureStateProvider.IsStateActive(_transformRecognizerActiveState.TransformConfig, feature, _targetConfig.Mode, _targetConfig.State);
				string text = (featureValue.HasValue ? featureValue.Value.ToString("F2") : "--");
				_targetText.text = $"{feature}\n" + currentState + " (" + text + ")";
			}
			else
			{
				_targetText.text = $"{feature}\n";
			}
			if (flag != _lastActiveValue)
			{
				_material.color = (flag ? _activeColor : _normalColor);
				_lastActiveValue = flag;
			}
		}
	}
}
