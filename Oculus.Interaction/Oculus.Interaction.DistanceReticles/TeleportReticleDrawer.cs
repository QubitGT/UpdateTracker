using System;
using System.Collections;
using Oculus.Interaction.Input;
using Oculus.Interaction.Locomotion;
using UnityEngine;
using UnityEngine.Serialization;

namespace Oculus.Interaction.DistanceReticles;

public class TeleportReticleDrawer : InteractorReticle<ReticleDataTeleport>
{
	[SerializeField]
	private TeleportInteractor _interactor;

	[SerializeField]
	[FormerlySerializedAs("_validTargetRenderer")]
	private Renderer _targetRenderer;

	[SerializeField]
	[Optional(OptionalAttribute.Flag.Obsolete)]
	[Obsolete("This renderer is not in use")]
	private Renderer _invalidTargetRenderer;

	[SerializeField]
	[Optional]
	[Interface(typeof(IAxis1D), new Type[] { })]
	[FormerlySerializedAs("_progress")]
	private UnityEngine.Object _progressState;

	[SerializeField]
	[Optional]
	[Interface(typeof(IActiveState), new Type[] { })]
	private UnityEngine.Object _highlightState;

	[SerializeField]
	private Color _acceptColor = Color.white;

	[SerializeField]
	private Color _rejectColor = Color.red;

	[SerializeField]
	private AnimationCurve _acceptAnimation = AnimationCurve.Linear(0f, 1f, 1f, 0f);

	[SerializeField]
	private AnimationCurve _rejectAnimation = AnimationCurve.Linear(0f, 0f, 1f, 0.3f);

	[SerializeField]
	private float _transitionSpeed = 8f;

	private static readonly int _progressKey = Shader.PropertyToID("_Progress");

	private static readonly int _highlightKey = Shader.PropertyToID("_Highlight");

	private static readonly int _colorKey = Shader.PropertyToID("_Color");

	private static readonly int _highlightColorKey = Shader.PropertyToID("_HighlightColor");

	private bool _selectionAnimation;

	private float _animatedProgress;

	private float _currentProgress;

	private bool _acceptMode = true;

	private IAxis1D ProgressState { get; set; }

	private IActiveState HighlightState { get; set; }

	public Color AcceptColor
	{
		get
		{
			return _acceptColor;
		}
		set
		{
			_acceptColor = value;
		}
	}

	public Color RejectColor
	{
		get
		{
			return _rejectColor;
		}
		set
		{
			_rejectColor = value;
		}
	}

	public AnimationCurve AcceptAnimation
	{
		get
		{
			return _acceptAnimation;
		}
		set
		{
			_acceptAnimation = value;
		}
	}

	public AnimationCurve RejectAnimation
	{
		get
		{
			return _rejectAnimation;
		}
		set
		{
			_rejectAnimation = value;
		}
	}

	public float TransitionSpeed
	{
		get
		{
			return _transitionSpeed;
		}
		set
		{
			_transitionSpeed = value;
		}
	}

	protected override IInteractorView Interactor { get; set; }

	protected override Component InteractableComponent => _interactor.Interactable;

	protected virtual void Awake()
	{
		Interactor = _interactor;
		ProgressState = _progressState as IAxis1D;
		HighlightState = _highlightState as IActiveState;
	}

	protected override void Start()
	{
		this.BeginStart(ref _started, delegate
		{
			base.Start();
		});
		this.EndStart(ref _started);
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (_started)
		{
			_interactor.WhenStateChanged += HandleStateChanged;
			SetReticleProgress(0f);
			_targetRenderer.enabled = false;
		}
	}

	protected override void OnDisable()
	{
		if (_started)
		{
			_interactor.WhenStateChanged -= HandleStateChanged;
		}
		base.OnDisable();
	}

	protected override void Align(ReticleDataTeleport data)
	{
		bool flag = ((HighlightState != null) ? HighlightState.Active : _selectionAnimation);
		data.Highlight(flag);
		if (!data.HideReticle)
		{
			Vector3 position = data.ProcessHitPoint(_interactor.ArcEnd.Point);
			Quaternion rotation = Quaternion.LookRotation(_interactor.ArcEnd.Normal);
			base.transform.SetPositionAndRotation(position, rotation);
			_targetRenderer.enabled = true;
			float time = ((ProgressState != null) ? ProgressState.Value() : _animatedProgress);
			AnimationCurve animationCurve = (_acceptMode ? _acceptAnimation : _rejectAnimation);
			SetReticleProgress(animationCurve.Evaluate(time));
			SetReticleHighlight(flag);
		}
	}

	protected override void Draw(ReticleDataTeleport data)
	{
		TeleportInteractable interactable = _interactor.Interactable;
		_acceptMode = interactable.AllowTeleport;
		_selectionAnimation = false;
		SetReticleColor(_acceptMode ? _acceptColor : _rejectColor);
	}

	protected override void Hide()
	{
		if (_targetRenderer != null)
		{
			_targetRenderer.enabled = false;
		}
		if (_targetData != null)
		{
			_targetData.Highlight(highlight: false);
		}
	}

	private void SetReticleColor(Color color)
	{
		_targetRenderer.material.SetColor(_colorKey, color);
		_targetRenderer.material.SetColor(_highlightColorKey, color);
	}

	private void SetReticleProgress(float progress)
	{
		if (_selectionAnimation)
		{
			_currentProgress = progress;
		}
		else
		{
			_currentProgress = Mathf.MoveTowards(_currentProgress, progress, _transitionSpeed * Time.deltaTime);
		}
		_targetRenderer.material.SetFloat(_progressKey, _currentProgress);
	}

	private void SetReticleHighlight(bool highlight)
	{
		_targetRenderer.material.SetFloat(_highlightKey, highlight ? 1f : 0f);
	}

	private void HandleStateChanged(InteractorStateChangeArgs obj)
	{
		if (ProgressState == null && obj.NewState == InteractorState.Select)
		{
			StopAllCoroutines();
			StartCoroutine(SelectionAnimation());
		}
	}

	private IEnumerator SelectionAnimation()
	{
		float targetProgress = 1f;
		_animatedProgress = 0f;
		_selectionAnimation = true;
		while (!Mathf.Approximately(targetProgress, _animatedProgress))
		{
			_animatedProgress = Mathf.MoveTowards(_animatedProgress, targetProgress, _transitionSpeed * Time.deltaTime);
			yield return null;
		}
		_selectionAnimation = false;
	}

	public void InjectAllTeleportReticleDrawer(TeleportInteractor interactor, Renderer targetRenderer)
	{
		InjectInteractor(interactor);
		InjectTargetRenderer(targetRenderer);
	}

	public void InjectInteractor(TeleportInteractor interactor)
	{
		_interactor = interactor;
	}

	public void InjectTargetRenderer(Renderer targetRenderer)
	{
		_targetRenderer = targetRenderer;
	}

	[Obsolete("Use InjectTargetRenderer instead")]
	public void InjectOptionalValidTargetRenderer(Renderer validTargetRenderer)
	{
		_targetRenderer = validTargetRenderer;
	}

	[Obsolete("Not in use")]
	public void InjectOptionalInalidTargetRenderer(Renderer invalidTargetRenderer)
	{
		_invalidTargetRenderer = invalidTargetRenderer;
	}

	public void InjectOptionalProgress(IAxis1D progressState)
	{
		_progressState = progressState as UnityEngine.Object;
		ProgressState = progressState;
	}

	public void InjectOptionalHighlightState(IActiveState highlightState)
	{
		_highlightState = highlightState as UnityEngine.Object;
		HighlightState = highlightState;
	}
}
