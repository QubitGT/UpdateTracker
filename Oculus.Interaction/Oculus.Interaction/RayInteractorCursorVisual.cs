using UnityEngine;

namespace Oculus.Interaction;

public class RayInteractorCursorVisual : MonoBehaviour
{
	[SerializeField]
	private RayInteractor _rayInteractor;

	[SerializeField]
	private Renderer _renderer;

	[SerializeField]
	private Color _hoverColor = Color.black;

	[SerializeField]
	private Color _selectColor = Color.black;

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

	private int _shaderInnerColor = Shader.PropertyToID("_Color");

	private int _shaderOutlineColor = Shader.PropertyToID("_OutlineColor");

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

	public Color HoverColor
	{
		get
		{
			return _hoverColor;
		}
		set
		{
			_hoverColor = value;
		}
	}

	public Color SelectColor
	{
		get
		{
			return _selectColor;
		}
		set
		{
			_selectColor = value;
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
		UpdateVisual();
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
			if (_renderer.enabled)
			{
				_renderer.enabled = false;
			}
			return;
		}
		if (!_rayInteractor.CollisionInfo.HasValue)
		{
			_renderer.enabled = false;
			return;
		}
		if (!_renderer.enabled)
		{
			_renderer.enabled = true;
		}
		Vector3 normal = _rayInteractor.CollisionInfo.Value.Normal;
		base.transform.position = _rayInteractor.End + normal * _offsetAlongNormal;
		base.transform.rotation = Quaternion.LookRotation(_rayInteractor.CollisionInfo.Value.Normal, Vector3.up);
		if (PlayerHead != null)
		{
			float num = Vector3.Distance(base.transform.position, PlayerHead.position);
			base.transform.localScale = _startScale * num;
		}
		bool flag = _rayInteractor.State == InteractorState.Select;
		_renderer.material.SetFloat(_shaderRadialGradientScale, flag ? 0.12f : 0.2f);
		_renderer.material.SetFloat(_shaderRadialGradientIntensity, 1f);
		_renderer.material.SetFloat(_shaderRadialGradientBackgroundOpacity, 1f);
		_renderer.material.SetColor(_shaderInnerColor, flag ? _selectColor : _hoverColor);
		_renderer.material.SetColor(_shaderOutlineColor, _outlineColor);
	}

	private void UpdateVisualState(InteractorStateChangeArgs args)
	{
		UpdateVisual();
	}

	public void InjectAllRayInteractorCursorVisual(RayInteractor rayInteractor, Renderer renderer)
	{
		InjectRayInteractor(rayInteractor);
		InjectRenderer(renderer);
	}

	public void InjectRayInteractor(RayInteractor rayInteractor)
	{
		_rayInteractor = rayInteractor;
	}

	public void InjectRenderer(Renderer renderer)
	{
		_renderer = renderer;
	}
}
