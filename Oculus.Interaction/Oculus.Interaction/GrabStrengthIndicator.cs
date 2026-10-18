using System;
using Oculus.Interaction.Grab;
using Oculus.Interaction.HandGrab;
using Oculus.Interaction.Input;
using UnityEngine;

namespace Oculus.Interaction;

public class GrabStrengthIndicator : MonoBehaviour
{
	[SerializeField]
	[Interface(typeof(IHandGrabInteractor), new Type[] { typeof(IInteractor) })]
	private UnityEngine.Object _handGrabInteractor;

	[SerializeField]
	private MaterialPropertyBlockEditor _handMaterialPropertyBlockEditor;

	[SerializeField]
	private float _glowLerpSpeed = 2f;

	[SerializeField]
	private float _glowColorLerpSpeed = 2f;

	[SerializeField]
	private Color _fingerGlowColorWithInteractable;

	[SerializeField]
	private Color _fingerGlowColorWithNoInteractable;

	[SerializeField]
	private Color _fingerGlowColorHover;

	private readonly int[] _handShaderGlowPropertyIds = new int[5]
	{
		Shader.PropertyToID("_ThumbGlowValue"),
		Shader.PropertyToID("_IndexGlowValue"),
		Shader.PropertyToID("_MiddleGlowValue"),
		Shader.PropertyToID("_RingGlowValue"),
		Shader.PropertyToID("_PinkyGlowValue")
	};

	private readonly int _fingerGlowColorPropertyId = Shader.PropertyToID("_FingerGlowColor");

	private Color _currentGlowColor;

	protected bool _started;

	private IHandGrabInteractor HandGrab { get; set; }

	private IInteractor Interactor { get; set; }

	public float GlowLerpSpeed
	{
		get
		{
			return _glowLerpSpeed;
		}
		set
		{
			_glowLerpSpeed = value;
		}
	}

	public float GlowColorLerpSpeed
	{
		get
		{
			return _glowColorLerpSpeed;
		}
		set
		{
			_glowColorLerpSpeed = value;
		}
	}

	public Color FingerGlowColorWithInteractable
	{
		get
		{
			return _fingerGlowColorWithInteractable;
		}
		set
		{
			_fingerGlowColorWithInteractable = value;
		}
	}

	public Color FingerGlowColorWithNoInteractable
	{
		get
		{
			return _fingerGlowColorWithNoInteractable;
		}
		set
		{
			_fingerGlowColorWithNoInteractable = value;
		}
	}

	public Color FingerGlowColorHover
	{
		get
		{
			return _fingerGlowColorHover;
		}
		set
		{
			_fingerGlowColorHover = value;
		}
	}

	private void Awake()
	{
		HandGrab = _handGrabInteractor as IHandGrabInteractor;
		Interactor = _handGrabInteractor as IInteractor;
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
			Interactor.WhenPostprocessed += UpdateVisual;
			_currentGlowColor = _fingerGlowColorWithNoInteractable;
		}
	}

	protected virtual void OnDisable()
	{
		if (_started)
		{
			Interactor.WhenPostprocessed -= UpdateVisual;
		}
	}

	private void UpdateVisual()
	{
		bool flag = Interactor.State == InteractorState.Select;
		bool hasSelectedInteractable = Interactor.HasSelectedInteractable;
		bool hasCandidate = Interactor.HasCandidate;
		Color b = _fingerGlowColorHover;
		if (flag)
		{
			b = (hasSelectedInteractable ? _fingerGlowColorWithInteractable : _fingerGlowColorWithNoInteractable);
		}
		_currentGlowColor = Color.Lerp(_currentGlowColor, b, Time.deltaTime * _glowColorLerpSpeed);
		_handMaterialPropertyBlockEditor.MaterialPropertyBlock.SetColor(_fingerGlowColorPropertyId, _currentGlowColor);
		for (int i = 0; i < 5; i++)
		{
			if ((flag && !hasSelectedInteractable) || (!flag && !hasCandidate))
			{
				UpdateGlowValue(i, 0f);
				continue;
			}
			float num = 0f;
			HandFinger handFinger = (HandFinger)i;
			if ((HandGrab.SupportedGrabTypes & GrabTypeFlags.Pinch) != 0 && HandGrab.TargetInteractable != null && (HandGrab.TargetInteractable.SupportedGrabTypes & GrabTypeFlags.Pinch) != 0 && HandGrab.TargetInteractable.PinchGrabRules[handFinger] != 0)
			{
				num = Mathf.Max(num, HandGrab.HandGrabApi.GetFingerPinchStrength(handFinger));
			}
			if ((HandGrab.SupportedGrabTypes & GrabTypeFlags.Palm) != 0 && HandGrab.TargetInteractable != null && (HandGrab.TargetInteractable.SupportedGrabTypes & GrabTypeFlags.Palm) != 0 && HandGrab.TargetInteractable.PalmGrabRules[handFinger] != 0)
			{
				num = Mathf.Max(num, HandGrab.HandGrabApi.GetFingerPalmStrength(handFinger));
			}
			UpdateGlowValue(i, num);
		}
		_handMaterialPropertyBlockEditor.UpdateMaterialPropertyBlock();
	}

	private void UpdateGlowValue(int fingerIndex, float glowValue)
	{
		float value = Mathf.MoveTowards(_handMaterialPropertyBlockEditor.MaterialPropertyBlock.GetFloat(_handShaderGlowPropertyIds[fingerIndex]), glowValue, _glowLerpSpeed * Time.deltaTime);
		_handMaterialPropertyBlockEditor.MaterialPropertyBlock.SetFloat(_handShaderGlowPropertyIds[fingerIndex], value);
	}

	public void InjectAllGrabStrengthIndicator(IHandGrabInteractor handGrabInteractor, MaterialPropertyBlockEditor handMaterialPropertyBlockEditor)
	{
		InjectHandGrab(handGrabInteractor);
		InjectHandMaterialPropertyBlockEditor(handMaterialPropertyBlockEditor);
	}

	public void InjectHandGrab(IHandGrabInteractor handGrab)
	{
		_handGrabInteractor = handGrab as UnityEngine.Object;
		HandGrab = handGrab;
		Interactor = handGrab as IInteractor;
	}

	public void InjectHandMaterialPropertyBlockEditor(MaterialPropertyBlockEditor handMaterialPropertyBlockEditor)
	{
		_handMaterialPropertyBlockEditor = handMaterialPropertyBlockEditor;
	}
}
