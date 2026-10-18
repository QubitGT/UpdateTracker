using System;
using Oculus.Interaction.Input;
using UnityEngine;

namespace Oculus.Interaction;

public class HandPokeOvershootGlow : MonoBehaviour
{
	public enum GlowType
	{
		Fill = 30,
		Outline,
		Both
	}

	[SerializeField]
	[Interface(typeof(IHand), new Type[] { })]
	private UnityEngine.Object _hand;

	[SerializeField]
	private PokeInteractor _pokeInteractor;

	[SerializeField]
	private HandVisual _handVisual;

	[SerializeField]
	private SkinnedMeshRenderer _handRenderer;

	[SerializeField]
	private MaterialPropertyBlockEditor _materialEditor;

	[SerializeField]
	private Color _glowColor;

	[SerializeField]
	private float _overshootMaxDistance = 0.15f;

	[SerializeField]
	private HandFinger _pokeFinger = HandFinger.Index;

	[SerializeField]
	[Range(0f, 1f)]
	private float _maxGradientLength;

	[SerializeField]
	private GlowType _glowType = GlowType.Outline;

	private IHand Hand;

	private bool _glowEnabled;

	private readonly int _glowFingerIndexID = Shader.PropertyToID("_FingerGlowIndex");

	private readonly int _generateGlowID = Shader.PropertyToID("_GenerateGlow");

	private readonly int _glowColorID = Shader.PropertyToID("_GlowColor");

	private readonly int _glowTypeID = Shader.PropertyToID("_GlowType");

	private readonly int _glowParameterID = Shader.PropertyToID("_GlowParameter");

	private readonly int _glowMaxLengthID = Shader.PropertyToID("_GlowMaxLength");

	protected bool _started;

	protected virtual void Awake()
	{
		Hand = _hand as IHand;
	}

	protected virtual void Start()
	{
		_glowEnabled = false;
		this.BeginStart(ref _started);
		HandFingerMaskGenerator.GenerateFingerMask(_handRenderer, _handVisual, _materialEditor.MaterialPropertyBlock);
		this.EndStart(ref _started);
	}

	protected virtual void OnEnable()
	{
		if (_started)
		{
			_pokeInteractor.WhenPostprocessed += UpdateVisual;
		}
	}

	protected virtual void OnDisable()
	{
		if (_started)
		{
			_pokeInteractor.WhenPostprocessed -= UpdateVisual;
		}
	}

	private void UpdateOvershoot(float normalizedDistance)
	{
		if (!(_materialEditor == null))
		{
			MaterialPropertyBlock materialPropertyBlock = _materialEditor.MaterialPropertyBlock;
			materialPropertyBlock.SetFloat(_glowParameterID, Mathf.Clamp01(normalizedDistance));
			materialPropertyBlock.SetInt(_generateGlowID, 1);
			materialPropertyBlock.SetColor(_glowColorID, _glowColor);
			materialPropertyBlock.SetInt(_glowTypeID, (int)_glowType);
			materialPropertyBlock.SetInt(_glowFingerIndexID, (int)_pokeFinger);
			materialPropertyBlock.SetFloat(_glowMaxLengthID, _maxGradientLength);
		}
	}

	private void UpdateVisual()
	{
		if (_pokeInteractor.State == InteractorState.Select)
		{
			_glowEnabled = true;
			Vector3 touchPoint = _pokeInteractor.TouchPoint;
			Vector3 origin = _pokeInteractor.Origin;
			float normalizedDistance = Mathf.Clamp01(Vector3.Distance(touchPoint, origin) / _overshootMaxDistance);
			UpdateOvershoot(normalizedDistance);
		}
		else if (_glowEnabled && !(_materialEditor == null))
		{
			_materialEditor.MaterialPropertyBlock.SetInt(_generateGlowID, 0);
			_glowEnabled = false;
		}
	}

	public void InjectAllHandPokeOvershootGlow(IHand hand, PokeInteractor pokeInteractor, MaterialPropertyBlockEditor materialEditor, Color glowColor, float distanceMultiplier, Transform wristTransform, GlowType glowType)
	{
		InjectHand(hand);
		InjectPokeInteractor(pokeInteractor);
		InjectMaterialPropertyBlockEditor(materialEditor);
		InjectGlowColor(glowColor);
		InjectOvershootMaxDistance(distanceMultiplier);
		InjectGlowType(glowType);
	}

	public void InjectAllHandPokeOvershootGlow(IHand hand, PokeInteractor pokeInteractor, HandVisual handVisual, SkinnedMeshRenderer handRenderer, MaterialPropertyBlockEditor materialEditor)
	{
		InjectHand(hand);
		InjectPokeInteractor(pokeInteractor);
		InjectHandVisual(handVisual);
		InjectHandRenderer(handRenderer);
		InjectMaterialPropertyBlockEditor(materialEditor);
	}

	public void InjectHand(IHand hand)
	{
		_hand = hand as UnityEngine.Object;
		Hand = hand;
	}

	public void InjectPokeInteractor(PokeInteractor pokeInteractor)
	{
		_pokeInteractor = pokeInteractor;
	}

	public void InjectHandRenderer(SkinnedMeshRenderer handRenderer)
	{
		_handRenderer = handRenderer;
	}

	public void InjectHandVisual(HandVisual handVisual)
	{
		_handVisual = handVisual;
	}

	public void InjectMaterialPropertyBlockEditor(MaterialPropertyBlockEditor materialEditor)
	{
		_materialEditor = materialEditor;
	}

	public void InjectGlowColor(Color glowColor)
	{
		_glowColor = glowColor;
	}

	public void InjectOvershootMaxDistance(float overshootMaxDistance)
	{
		_overshootMaxDistance = overshootMaxDistance;
	}

	public void InjectGlowType(GlowType glowType)
	{
		_glowType = glowType;
	}
}
