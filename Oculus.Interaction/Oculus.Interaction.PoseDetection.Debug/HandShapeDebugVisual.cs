using System;
using System.Collections.Generic;
using System.Linq;
using Oculus.Interaction.Input;
using TMPro;
using UnityEngine;

namespace Oculus.Interaction.PoseDetection.Debug;

public class HandShapeDebugVisual : MonoBehaviour
{
	[SerializeField]
	[Interface(typeof(IFingerFeatureStateProvider), new Type[] { })]
	private UnityEngine.Object _fingerFeatureStateProvider;

	private IFingerFeatureStateProvider FingerFeatureStateProvider;

	[SerializeField]
	private ShapeRecognizerActiveState _shapeRecognizerActiveState;

	[SerializeField]
	private Renderer _target;

	[SerializeField]
	private Color _normalColor = Color.red;

	[SerializeField]
	private Color _activeColor = Color.green;

	[SerializeField]
	private GameObject _fingerFeatureDebugVisualPrefab;

	[SerializeField]
	private Transform _fingerFeatureParent;

	[SerializeField]
	private Vector3 _fingerSpacingVec = new Vector3(0f, -1f, 0f);

	[SerializeField]
	private Vector3 _fingerFeatureSpacingVec = new Vector3(1f, 0f, 0f);

	[SerializeField]
	private Vector3 _fingerFeatureDebugLocalScale = new Vector3(0.3f, 0.3f, 0.3f);

	[SerializeField]
	private TextMeshPro _targetText;

	private Material _material;

	private bool _lastActiveValue;

	protected virtual void Awake()
	{
		FingerFeatureStateProvider = _fingerFeatureStateProvider as IFingerFeatureStateProvider;
		_material = _target.material;
		_material.color = (_lastActiveValue ? _activeColor : _normalColor);
		if (_fingerFeatureParent == null)
		{
			_fingerFeatureParent = base.transform;
		}
	}

	protected virtual void Start()
	{
		Vector3 zero = Vector3.zero;
		foreach (var item in from s in AllFeatureStates()
			group s by s.Item1 into @group
			select new
			{
				HandFinger = @group.Key,
				FingerFeatures = @group.SelectMany(((HandFinger, IReadOnlyList<ShapeRecognizer.FingerFeatureConfig>) item) => item.Item2)
			})
		{
			Vector3 localPosition = zero;
			foreach (ShapeRecognizer.FingerFeatureConfig fingerFeature in item.FingerFeatures)
			{
				FingerFeatureDebugVisual component = UnityEngine.Object.Instantiate(_fingerFeatureDebugVisualPrefab, _fingerFeatureParent).GetComponent<FingerFeatureDebugVisual>();
				component.Initialize(item.HandFinger, fingerFeature, FingerFeatureStateProvider);
				Transform obj = component.transform;
				obj.localScale = _fingerFeatureDebugLocalScale;
				obj.localRotation = Quaternion.identity;
				obj.localPosition = localPosition;
				localPosition += _fingerFeatureSpacingVec;
			}
			zero += _fingerSpacingVec;
		}
		string text = "";
		foreach (ShapeRecognizer shape in _shapeRecognizerActiveState.Shapes)
		{
			text += shape.ShapeName;
		}
		_targetText.text = $"{_shapeRecognizerActiveState.Handedness} Hand: {text} ";
	}

	private IEnumerable<(HandFinger, IReadOnlyList<ShapeRecognizer.FingerFeatureConfig>)> AllFeatureStates()
	{
		foreach (ShapeRecognizer shape in _shapeRecognizerActiveState.Shapes)
		{
			foreach (var fingerFeatureConfig in shape.GetFingerFeatureConfigs())
			{
				yield return fingerFeatureConfig;
			}
		}
	}

	protected virtual void OnDestroy()
	{
		UnityEngine.Object.Destroy(_material);
	}

	protected virtual void Update()
	{
		bool active = _shapeRecognizerActiveState.Active;
		if (_lastActiveValue != active)
		{
			_material.color = (active ? _activeColor : _normalColor);
			_lastActiveValue = active;
		}
	}
}
