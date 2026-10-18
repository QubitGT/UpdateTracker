using Oculus.Interaction.Input;
using TMPro;
using UnityEngine;

namespace Oculus.Interaction.PoseDetection.Debug;

public class TransformRecognizerDebugVisual : MonoBehaviour
{
	[SerializeField]
	private Hand _hand;

	[SerializeField]
	private TransformFeatureStateProvider _transformFeatureStateProvider;

	[SerializeField]
	private TransformRecognizerActiveState _transformRecognizerActiveState;

	[SerializeField]
	private Renderer _target;

	[SerializeField]
	private Color _normalColor = Color.red;

	[SerializeField]
	private Color _activeColor = Color.green;

	[SerializeField]
	private GameObject _transformFeatureDebugVisualPrefab;

	[SerializeField]
	private Transform _debugVisualParent;

	[SerializeField]
	private Vector3 _featureSpacingVec = new Vector3(1f, 0f, 0f);

	[SerializeField]
	private Vector3 _featureDebugLocalScale = new Vector3(0.3f, 0.3f, 0.3f);

	[SerializeField]
	private TextMeshPro _targetText;

	private Material _material;

	private bool _lastActiveValue;

	protected virtual void Awake()
	{
		_material = _target.material;
		_material.color = (_lastActiveValue ? _activeColor : _normalColor);
		if (_debugVisualParent == null)
		{
			_debugVisualParent = base.transform;
		}
	}

	protected virtual void Start()
	{
		Vector3 zero = Vector3.zero;
		string text = "";
		foreach (TransformFeatureConfig featureConfig in _transformRecognizerActiveState.FeatureConfigs)
		{
			TransformFeatureDebugVisual component = Object.Instantiate(_transformFeatureDebugVisualPrefab, _debugVisualParent).GetComponent<TransformFeatureDebugVisual>();
			component.Initialize(_transformRecognizerActiveState.Hand.Handedness, featureConfig, _transformFeatureStateProvider, _transformRecognizerActiveState);
			Transform obj = component.transform;
			obj.localScale = _featureDebugLocalScale;
			obj.localRotation = Quaternion.identity;
			obj.localPosition = zero;
			zero += _featureSpacingVec;
			if (!string.IsNullOrEmpty(text))
			{
				text += "\n  ";
			}
			text += $"{featureConfig.Mode} {featureConfig.State} ({_transformRecognizerActiveState.Hand.Handedness})";
		}
		_targetText.text = text ?? "";
	}

	private void OnDestroy()
	{
		Object.Destroy(_material);
	}

	private bool AllActive()
	{
		if (!_transformRecognizerActiveState.Active)
		{
			return false;
		}
		return true;
	}

	protected virtual void Update()
	{
		bool flag = AllActive();
		if (_lastActiveValue != flag)
		{
			_material.color = (flag ? _activeColor : _normalColor);
			_lastActiveValue = flag;
		}
	}
}
