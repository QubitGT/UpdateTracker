using System;
using Oculus.Interaction.Input;
using UnityEngine;

namespace Oculus.Interaction;

public class PinchPointerVisual : MonoBehaviour
{
	[SerializeField]
	[Interface(typeof(IInteractor), new Type[] { })]
	private UnityEngine.Object _interactor;

	private IInteractor Interactor;

	[SerializeField]
	private SkinnedMeshRenderer _skinnedMeshRenderer;

	[SerializeField]
	private Vector3 _localOffset = Vector3.zero;

	[SerializeField]
	private AnimationCurve _remapCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	[SerializeField]
	private Vector2 _alphaRange = new Vector2(0.1f, 0.4f);

	[SerializeField]
	private Color _tint = Color.white;

	[SerializeField]
	[Interface(typeof(IAxis1D), new Type[] { })]
	[Optional]
	private UnityEngine.Object _progress;

	private IAxis1D Progress;

	protected bool _started;

	public Vector3 LocalOffset
	{
		get
		{
			return _localOffset;
		}
		set
		{
			_localOffset = value;
		}
	}

	public AnimationCurve RemapCurve
	{
		get
		{
			return _remapCurve;
		}
		set
		{
			_remapCurve = value;
		}
	}

	public Vector2 AlphaRange
	{
		get
		{
			return _alphaRange;
		}
		set
		{
			_alphaRange = value;
		}
	}

	public Color Tint
	{
		get
		{
			return _tint;
		}
		set
		{
			_tint = value;
		}
	}

	protected virtual void Awake()
	{
		Interactor = _interactor as IInteractor;
		Progress = _progress as IAxis1D;
	}

	protected virtual void Start()
	{
		this.BeginStart(ref _started);
		this.EndStart(ref _started);
	}

	protected virtual void OnEnable()
	{
		if (_started)
		{
			Interactor.WhenStateChanged += HandleStateChanged;
			Interactor.WhenPreprocessed += HandlePostprocessed;
		}
	}

	protected virtual void OnDisable()
	{
		if (_started)
		{
			Interactor.WhenStateChanged -= HandleStateChanged;
			Interactor.WhenPreprocessed -= HandlePostprocessed;
		}
	}

	public void SetPositionAndRotation(Vector3 position, Quaternion rotation)
	{
		position += rotation * _localOffset;
		base.transform.SetPositionAndRotation(position, rotation);
	}

	private void HandleStateChanged(InteractorStateChangeArgs stateArgs)
	{
		if (stateArgs.NewState == InteractorState.Disabled)
		{
			_skinnedMeshRenderer.enabled = false;
		}
		else
		{
			_skinnedMeshRenderer.enabled = true;
		}
	}

	private void HandlePostprocessed()
	{
		if (Progress != null)
		{
			float num = _remapCurve.Evaluate(Progress.Value());
			_skinnedMeshRenderer.SetBlendShapeWeight(0, num * 100f);
			_skinnedMeshRenderer.SetBlendShapeWeight(1, num * 100f);
			UpdateColor(Interactor.State == InteractorState.Select, num);
		}
	}

	private void UpdateColor(bool highlight, float mappedPinchStrength)
	{
		Color tint = Tint;
		tint.a *= (highlight ? 1f : Mathf.Lerp(_alphaRange.x, _alphaRange.y, mappedPinchStrength));
		_skinnedMeshRenderer.material.color = tint;
	}

	public void InjectAllPinchPointerVisual(IInteractor interactor, SkinnedMeshRenderer skinnedMeshRenderer)
	{
		InjectInteractor(interactor);
		InjectSkinnedMeshRenderer(skinnedMeshRenderer);
	}

	public void InjectInteractor(IInteractor interactor)
	{
		Interactor = interactor;
		_interactor = interactor as UnityEngine.Object;
	}

	public void InjectSkinnedMeshRenderer(SkinnedMeshRenderer skinnedMeshRenderer)
	{
		_skinnedMeshRenderer = skinnedMeshRenderer;
	}
}
