using System;
using Oculus.Interaction.Input;
using UnityEngine;

namespace Oculus.Interaction;

public class HandRayInteractorCursorVisual : MonoBehaviour
{
	[SerializeField]
	[Interface(typeof(IHand), new Type[] { })]
	private UnityEngine.Object _hand;

	private IHand Hand;

	[SerializeField]
	private RayInteractor _rayInteractor;

	[SerializeField]
	private GameObject _cursor;

	[SerializeField]
	private Renderer _renderer;

	[SerializeField]
	private Color _outlineColor = Color.black;

	[SerializeField]
	private float _offsetAlongNormal = 0.005f;

	[Tooltip("Players head transform, used to maintain the same cursor size on screen as it is moved in the scene.")]
	[SerializeField]
	[Optional]
	private Transform _playerHead;

	private Vector3 _startScale;

	private int _shaderRadialGradientScale = Shader.PropertyToID("_RadialGradientScale");

	private int _shaderRadialGradientIntensity = Shader.PropertyToID("_RadialGradientIntensity");

	private int _shaderRadialGradientBackgroundOpacity = Shader.PropertyToID("_RadialGradientBackgroundOpacity");

	private int _shaderOutlineColor = Shader.PropertyToID("_OutlineColor");

	[SerializeField]
	private GameObject _selectObject;

	protected bool _started;

	public Transform PlayerHead
	{
		get
		{
			return _playerHead;
		}
		set
		{
			_playerHead = value;
			if (_started && (object)value == null)
			{
				base.transform.localScale = _startScale;
			}
		}
	}

	public Color OutlineColor
	{
		get
		{
			return _outlineColor;
		}
		set
		{
			_outlineColor = value;
		}
	}

	public float OffsetAlongNormal
	{
		get
		{
			return _offsetAlongNormal;
		}
		set
		{
			_offsetAlongNormal = value;
		}
	}

	protected virtual void Start()
	{
		this.BeginStart(ref _started);
		Hand = _hand as IHand;
		_startScale = base.transform.localScale;
		this.EndStart(ref _started);
	}

	protected virtual void OnEnable()
	{
		if (_started)
		{
			_rayInteractor.WhenPostprocessed += UpdateVisual;
			_rayInteractor.WhenStateChanged += UpdateVisualState;
			UpdateVisual();
		}
	}

	protected virtual void OnDisable()
	{
		if (_started)
		{
			_rayInteractor.WhenPostprocessed -= UpdateVisual;
			_rayInteractor.WhenStateChanged -= UpdateVisualState;
		}
	}

	private void UpdateVisual()
	{
		if (_rayInteractor.State == InteractorState.Disabled)
		{
			_cursor.SetActive(value: false);
			return;
		}
		if (!_rayInteractor.CollisionInfo.HasValue)
		{
			_cursor.SetActive(value: false);
			return;
		}
		if (!_cursor.activeSelf)
		{
			_cursor.SetActive(value: true);
		}
		Vector3 normal = _rayInteractor.CollisionInfo.Value.Normal;
		base.transform.position = _rayInteractor.End + normal * _offsetAlongNormal;
		base.transform.rotation = Quaternion.LookRotation(_rayInteractor.CollisionInfo.Value.Normal, Vector3.up);
		if (PlayerHead != null)
		{
			float num = Vector3.Distance(base.transform.position, PlayerHead.position);
			base.transform.localScale = _startScale * num;
		}
		if (_rayInteractor.State == InteractorState.Select)
		{
			_selectObject.SetActive(value: true);
			_renderer.material.SetFloat(_shaderRadialGradientScale, 0.7f);
			_renderer.material.SetFloat(_shaderRadialGradientIntensity, 1f);
			_renderer.material.SetFloat(_shaderRadialGradientBackgroundOpacity, 1f);
			_renderer.material.SetColor(_shaderOutlineColor, _outlineColor);
		}
		else
		{
			_selectObject.SetActive(value: false);
			float fingerPinchStrength = Hand.GetFingerPinchStrength(HandFinger.Index);
			float a = 1f - fingerPinchStrength;
			a = Mathf.Max(a, 0.7f);
			_renderer.material.SetFloat(_shaderRadialGradientScale, a);
			_renderer.material.SetFloat(_shaderRadialGradientIntensity, fingerPinchStrength);
			_renderer.material.SetFloat(_shaderRadialGradientBackgroundOpacity, Mathf.Lerp(0.7f, 1f, fingerPinchStrength));
			_renderer.material.SetColor(_shaderOutlineColor, _outlineColor);
		}
	}

	private void UpdateVisualState(InteractorStateChangeArgs args)
	{
		UpdateVisual();
	}

	public void InjectAllHandRayInteractorCursorVisual(IHand hand, RayInteractor rayInteractor, GameObject cursor, Renderer renderer)
	{
		InjectHand(hand);
		InjectRayInteractor(rayInteractor);
		InjectCursor(cursor);
		InjectRenderer(renderer);
	}

	public void InjectHand(IHand hand)
	{
		_hand = hand as UnityEngine.Object;
		Hand = hand;
	}

	public void InjectRayInteractor(RayInteractor rayInteractor)
	{
		_rayInteractor = rayInteractor;
	}

	public void InjectCursor(GameObject cursor)
	{
		_cursor = cursor;
	}

	public void InjectRenderer(Renderer renderer)
	{
		_renderer = renderer;
	}
}
