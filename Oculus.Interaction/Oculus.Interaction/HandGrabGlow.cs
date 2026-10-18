using System;
using Oculus.Interaction.Grab;
using Oculus.Interaction.GrabAPI;
using Oculus.Interaction.HandGrab;
using Oculus.Interaction.Input;
using UnityEngine;

namespace Oculus.Interaction;

public class HandGrabGlow : MonoBehaviour
{
	public enum GlowType
	{
		Fill = 27,
		Outline,
		Both
	}

	private enum GlowState
	{
		None,
		Hover,
		Selected,
		SelectedGlowOut
	}

	private enum GrabState
	{
		None,
		Pinch,
		Palm
	}

	[SerializeField]
	[Interface(typeof(IHandGrabInteractor), new Type[] { typeof(IInteractor) })]
	private UnityEngine.Object _handGrabInteractor;

	[SerializeField]
	private HandVisual _handVisual;

	[SerializeField]
	private SkinnedMeshRenderer _handRenderer;

	[SerializeField]
	private MaterialPropertyBlockEditor _materialEditor;

	[SerializeField]
	private Color _glowColorGrabing;

	[SerializeField]
	private Color _glowColorHover;

	[SerializeField]
	[Range(0f, 1f)]
	private float _colorChangeSpeed = 0.5f;

	[SerializeField]
	[Range(0f, 0.25f)]
	private float _glowFadeStartTime = 0.2f;

	[SerializeField]
	[Range(0f, 1f)]
	private float _glowStrengthChangeSpeed = 0.5f;

	[SerializeField]
	private bool _fadeOut;

	[SerializeField]
	[Range(0f, 1f)]
	[Tooltip("Recommended from 0.7 to 1.0")]
	private float _gradientLength = 0.85f;

	[SerializeField]
	private GlowType _glowType = GlowType.Outline;

	private GlowState _state;

	private float _accumulatedSelectedTime;

	private GrabState _grabState;

	private float _glowFadeValue;

	private Color _currentColor;

	private IHandGrabInteractor HandGrabInteractor;

	private IInteractor Interactor;

	private float[] _glowStregth = new float[5];

	private readonly int _generateGlowID = Shader.PropertyToID("_GenerateGlow");

	private readonly int _glowColorID = Shader.PropertyToID("_GlowColor");

	private readonly int _glowTypeID = Shader.PropertyToID("_GlowType");

	private readonly int _glowParameterID = Shader.PropertyToID("_GlowParameter");

	private readonly int[] _fingersGlowIDs = new int[5]
	{
		Shader.PropertyToID("_ThumbGlowValue"),
		Shader.PropertyToID("_IndexGlowValue"),
		Shader.PropertyToID("_MiddleGlowValue"),
		Shader.PropertyToID("_RingGlowValue"),
		Shader.PropertyToID("_PinkyGlowValue")
	};

	protected bool _started;

	protected virtual void Awake()
	{
		_glowFadeValue = 1f;
		_state = GlowState.None;
		_grabState = GrabState.None;
		HandGrabInteractor = _handGrabInteractor as IHandGrabInteractor;
		Interactor = _handGrabInteractor as IInteractor;
	}

	protected virtual void Start()
	{
		this.BeginStart(ref _started);
		HandFingerMaskGenerator.GenerateFingerMask(_handRenderer, _handVisual, _materialEditor.MaterialPropertyBlock);
		this.EndStart(ref _started);
	}

	protected virtual void OnEnable()
	{
		if (_started)
		{
			Interactor.WhenPostprocessed += UpdateVisual;
		}
	}

	protected virtual void OnDisable()
	{
		if (_started)
		{
			Interactor.WhenPostprocessed -= UpdateVisual;
		}
	}

	private void SetMaterialPropertyBlockValues()
	{
		MaterialPropertyBlock materialPropertyBlock = _materialEditor.MaterialPropertyBlock;
		if (materialPropertyBlock == null)
		{
			return;
		}
		materialPropertyBlock.SetInt(_generateGlowID, 1);
		materialPropertyBlock.SetColor(_glowColorID, _currentColor);
		if (_glowType == GlowType.Fill || _glowType == GlowType.Both)
		{
			float num = _gradientLength;
			if (_fadeOut)
			{
				num *= _glowFadeValue;
			}
			materialPropertyBlock.SetFloat(_glowParameterID, num);
		}
		else
		{
			materialPropertyBlock.SetFloat(_glowParameterID, _glowFadeValue);
		}
		for (int i = 0; i < _fingersGlowIDs.Length; i++)
		{
			materialPropertyBlock.SetFloat(_fingersGlowIDs[i], Mathf.Clamp01(_glowStregth[i]));
		}
		materialPropertyBlock.SetInt(_glowTypeID, (int)_glowType);
	}

	private void UpdateFingerGlowStrength(int fingerIndex, float strength)
	{
		float num = Mathf.Lerp(_glowStregth[fingerIndex], strength, _glowStrengthChangeSpeed);
		_glowStregth[fingerIndex] = num;
	}

	private bool FingerOptionalOrRequired(GrabbingRule rules, HandFinger finger)
	{
		if (rules[finger] != FingerRequirement.Optional)
		{
			return rules[finger] == FingerRequirement.Required;
		}
		return true;
	}

	private void UpdateGlowStrength()
	{
		float b = 0f;
		for (int i = 1; i < 5; i++)
		{
			HandFinger finger = (HandFinger)i;
			bool flag = FingerOptionalOrRequired(HandGrabInteractor.TargetInteractable.PinchGrabRules, finger);
			float a = ((TargetSupportsPinch() && flag) ? HandGrabInteractor.HandGrabApi.GetFingerPinchStrength(finger) : 0f);
			bool flag2 = FingerOptionalOrRequired(HandGrabInteractor.TargetInteractable.PalmGrabRules, finger);
			float b2 = ((TargetSupportsPalm() && flag2) ? HandGrabInteractor.HandGrabApi.GetFingerPalmStrength(finger) : 0f);
			float strength = Mathf.Max(a, b2);
			UpdateFingerGlowStrength(i, strength);
			b = Mathf.Max(a, b);
		}
		bool flag3 = FingerOptionalOrRequired(HandGrabInteractor.TargetInteractable.PalmGrabRules, HandFinger.Thumb);
		float a2 = ((TargetSupportsPalm() && flag3) ? HandGrabInteractor.HandGrabApi.GetFingerPalmStrength(HandFinger.Thumb) : 0f);
		UpdateFingerGlowStrength(0, Mathf.Max(a2, b));
	}

	private void UpdateGlowState()
	{
		if (Interactor.State == InteractorState.Hover)
		{
			_state = GlowState.Hover;
		}
		else if (Interactor.State == InteractorState.Select)
		{
			if (_state == GlowState.Hover || _state == GlowState.None)
			{
				_accumulatedSelectedTime = 0f;
				_state = GlowState.Selected;
			}
			else if (_state == GlowState.Selected)
			{
				_accumulatedSelectedTime += Time.deltaTime;
				if (_fadeOut && _accumulatedSelectedTime >= _glowFadeStartTime)
				{
					_state = GlowState.SelectedGlowOut;
				}
			}
		}
		else
		{
			_state = GlowState.None;
		}
	}

	private void UpdateGlowColorAndFade()
	{
		if (_state == GlowState.Hover)
		{
			_glowFadeValue = 1f;
			_currentColor = Color.Lerp(_currentColor, _fadeOut ? _glowColorGrabing : _glowColorHover, _colorChangeSpeed);
		}
		else if (_state == GlowState.Selected)
		{
			if (_fadeOut)
			{
				_glowFadeValue = Mathf.Lerp(_glowFadeValue, 0.5f, 0.8f);
				_currentColor = _glowColorGrabing;
			}
			else
			{
				_glowFadeValue = 1f;
				_currentColor = Color.Lerp(_currentColor, _glowColorGrabing, _colorChangeSpeed);
			}
		}
		else if (_state == GlowState.SelectedGlowOut)
		{
			_glowFadeValue = Mathf.Lerp(_glowFadeValue, 1.15f, 0.3f);
			_currentColor = _glowColorGrabing;
		}
		else
		{
			_glowFadeValue = Mathf.Lerp(_glowFadeValue, 0f, 0.15f);
		}
	}

	private bool TargetSupportsPinch()
	{
		if (HandGrabInteractor.TargetInteractable == null)
		{
			return false;
		}
		return (HandGrabInteractor.SupportedGrabTypes & HandGrabInteractor.TargetInteractable.SupportedGrabTypes & GrabTypeFlags.Pinch) != 0;
	}

	private bool TargetSupportsPalm()
	{
		if (HandGrabInteractor.TargetInteractable == null)
		{
			return false;
		}
		return (HandGrabInteractor.SupportedGrabTypes & HandGrabInteractor.TargetInteractable.SupportedGrabTypes & GrabTypeFlags.Palm) != 0;
	}

	private void UpdateGrabState()
	{
		if (HandGrabInteractor.TargetInteractable == null)
		{
			_grabState = GrabState.None;
			return;
		}
		GrabbingRule fingers = HandGrabInteractor.TargetInteractable.PinchGrabRules;
		bool flag = HandGrabInteractor.HandGrabApi.IsHandPinchGrabbing(in fingers);
		if (TargetSupportsPinch() && flag && (_grabState == GrabState.None || _grabState == GrabState.Pinch))
		{
			_grabState = GrabState.Pinch;
			return;
		}
		GrabbingRule fingers2 = HandGrabInteractor.TargetInteractable.PalmGrabRules;
		bool flag2 = HandGrabInteractor.HandGrabApi.IsHandPalmGrabbing(in fingers2);
		if (TargetSupportsPalm() && flag2 && (_grabState == GrabState.None || _grabState == GrabState.Palm))
		{
			_grabState = GrabState.Palm;
		}
		else
		{
			_grabState = GrabState.None;
		}
	}

	private void ClearGlow()
	{
		MaterialPropertyBlock materialPropertyBlock = _materialEditor.MaterialPropertyBlock;
		int[] fingersGlowIDs = _fingersGlowIDs;
		foreach (int nameID in fingersGlowIDs)
		{
			materialPropertyBlock.SetFloat(nameID, 0f);
		}
		materialPropertyBlock.SetInt(_generateGlowID, 0);
	}

	private void UpdateVisual()
	{
		GlowState state = _state;
		UpdateGrabState();
		UpdateGlowState();
		if (state != _state && _state == GlowState.None)
		{
			ClearGlow();
		}
		else if (_state != 0)
		{
			UpdateGlowStrength();
			UpdateGlowColorAndFade();
			SetMaterialPropertyBlockValues();
		}
	}

	public void InjectAllHandGrabGlow(IHandGrabInteractor handGrabInteractor, SkinnedMeshRenderer handRenderer, MaterialPropertyBlockEditor materialEditor, HandVisual handVisual, Color grabbingColor, Color hoverColor, float colorChangeSpeed, float fadeStartTime, float glowStrengthChangeSpeed, bool fadeOut, float gradientLength, GlowType glowType)
	{
		InjectHandGrabInteractor(handGrabInteractor);
		InjectHandRenderer(handRenderer);
		InjectMaterialPropertyBlockEditor(materialEditor);
		InjectHandVisual(handVisual);
		InjectGlowColors(grabbingColor, hoverColor);
		InjectVisualChangeSpeed(colorChangeSpeed, fadeStartTime, glowStrengthChangeSpeed);
		InjectFadeOut(fadeOut);
		InjectGradientLength(gradientLength);
		InjectGlowType(glowType);
	}

	public void InjectHandGrabInteractor(IHandGrabInteractor handGrabInteractor)
	{
		_handGrabInteractor = handGrabInteractor as UnityEngine.Object;
		Interactor = handGrabInteractor as IInteractor;
		HandGrabInteractor = handGrabInteractor;
	}

	public void InjectHandRenderer(SkinnedMeshRenderer handRenderer)
	{
		_handRenderer = handRenderer;
	}

	public void InjectMaterialPropertyBlockEditor(MaterialPropertyBlockEditor materialEditor)
	{
		_materialEditor = materialEditor;
	}

	public void InjectHandVisual(HandVisual handVisual)
	{
		_handVisual = handVisual;
	}

	public void InjectGlowColors(Color grabbingColor, Color hoverColor)
	{
		_glowColorGrabing = grabbingColor;
		_glowColorHover = hoverColor;
	}

	public void InjectVisualChangeSpeed(float colorChangeSpeed, float fadeStartTime, float glowStrengthChangeSpeed)
	{
		_colorChangeSpeed = colorChangeSpeed;
		_glowFadeStartTime = fadeStartTime;
		_glowStrengthChangeSpeed = glowStrengthChangeSpeed;
	}

	public void InjectFadeOut(bool fadeOut)
	{
		_fadeOut = fadeOut;
	}

	public void InjectGradientLength(float gradientLength)
	{
		_gradientLength = Mathf.Clamp01(gradientLength);
	}

	public void InjectGlowType(GlowType glowType)
	{
		_glowType = glowType;
	}
}
