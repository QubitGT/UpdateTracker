using System;
using UnityEngine;

namespace Oculus.Interaction.PoseDetection.Debug;

public class ActiveStateDebugVisual : MonoBehaviour
{
	[Tooltip("The IActiveState to debug.")]
	[SerializeField]
	[Interface(typeof(IActiveState), new Type[] { })]
	private UnityEngine.Object _activeState;

	[Tooltip("The renderer used for the color change.")]
	[SerializeField]
	private Renderer _target;

	[Tooltip("The renderer will be set to this color when ActiveState is inactive.")]
	[SerializeField]
	private Color _normalColor = Color.red;

	[Tooltip("The renderer will be set to this color when ActiveState is active.")]
	[SerializeField]
	private Color _activeColor = Color.green;

	private Material _material;

	private bool _lastActiveValue;

	private IActiveState ActiveState { get; set; }

	protected virtual void Awake()
	{
		ActiveState = _activeState as IActiveState;
		_material = _target.material;
		SetMaterialColor(_lastActiveValue ? _activeColor : _normalColor);
	}

	private void OnDestroy()
	{
		UnityEngine.Object.Destroy(_material);
	}

	protected virtual void Update()
	{
		bool active = ActiveState.Active;
		if (_lastActiveValue != active)
		{
			SetMaterialColor(active ? _activeColor : _normalColor);
			_lastActiveValue = active;
		}
	}

	private void SetMaterialColor(Color activeColor)
	{
		_material.color = activeColor;
		_target.enabled = _material.color.a > 0f;
	}
}
