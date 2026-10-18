using System;
using Oculus.Interaction.Input;
using UnityEngine;

namespace Oculus.Interaction;

public class HandRayPinchGlow : MonoBehaviour
{
	public enum GlowType
	{
		Fill = 17,
		Outline = 18,
		Both = 16
	}

	[SerializeField]
	[Interface(typeof(IHand), new Type[] { })]
	private UnityEngine.Object _hand;

	[SerializeField]
	private RayInteractor _rayInteractor;

	[SerializeField]
	private MaterialPropertyBlockEditor _materialEditor;

	[SerializeField]
	private Color _glowColor;

	[SerializeField]
	private GlowType _glowType = GlowType.Outline;

	private IHand Hand;

	private readonly int _generateGlowID = Shader.PropertyToID("_GenerateGlow");

	private readonly int _glowPositionID = Shader.PropertyToID("_GlowPosition");

	private readonly int _glowColorID = Shader.PropertyToID("_GlowColor");

	private readonly int _glowTypeID = Shader.PropertyToID("_GlowType");

	private readonly int _glowParameterID = Shader.PropertyToID("_GlowParameter");

	private readonly int _glowMaxLengthID = Shader.PropertyToID("_GlowMaxLength");

	private bool _glowEnabled;

	protected bool _started;

	protected virtual void Awake()
	{
		Hand = _hand as IHand;
	}

	protected virtual void Start()
	{
		_glowEnabled = false;
		this.BeginStart(ref _started);
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

	private void UpdateVisualState(InteractorStateChangeArgs args)
	{
		UpdateVisual();
	}

	private void UpdateGlow(Vector3 glowPosition, float pinchStrength, float glowMaxLength)
	{
		if (!(_materialEditor == null))
		{
			MaterialPropertyBlock materialPropertyBlock = _materialEditor.MaterialPropertyBlock;
			materialPropertyBlock.SetInt(_generateGlowID, 1);
			materialPropertyBlock.SetColor(_glowColorID, _glowColor);
			materialPropertyBlock.SetFloat(_glowParameterID, pinchStrength);
			materialPropertyBlock.SetFloat(_glowMaxLengthID, glowMaxLength);
			materialPropertyBlock.SetInt(_glowTypeID, (int)_glowType);
			materialPropertyBlock.SetVector(_glowPositionID, glowPosition);
		}
	}

	private void UpdateVisual()
	{
		if (_rayInteractor.State == InteractorState.Disabled)
		{
			if (_glowEnabled && !(_materialEditor == null))
			{
				_materialEditor.MaterialPropertyBlock.SetInt(_generateGlowID, 0);
				_glowEnabled = false;
			}
			return;
		}
		_glowEnabled = true;
		if (Hand.GetJointPose(HandJointId.HandThumbTip, out var pose) && Hand.GetJointPose(HandJointId.HandIndexTip, out var pose2) && Hand.GetRootPose(out var pose3))
		{
			float fingerPinchStrength = Hand.GetFingerPinchStrength(HandFinger.Index);
			Vector3 vector = (pose.position + pose2.position) / 2f;
			float glowMaxLength = Vector3.Distance(pose3.position, vector) * 0.9f;
			UpdateGlow(vector, fingerPinchStrength, glowMaxLength);
		}
	}

	public void InjectAllHandRayPinchGlow(IHand hand, RayInteractor interactor, MaterialPropertyBlockEditor materialEditor, Color color, GlowType glowType)
	{
		InjectHand(hand);
		InjectRayInteractor(interactor);
		InjectMaterialPropertyBlockEditor(materialEditor);
		InjectGlowColor(color);
		InjectGlowType(glowType);
	}

	public void InjectHand(IHand hand)
	{
		_hand = hand as UnityEngine.Object;
		Hand = hand;
	}

	public void InjectRayInteractor(RayInteractor interactor)
	{
		_rayInteractor = interactor;
	}

	public void InjectMaterialPropertyBlockEditor(MaterialPropertyBlockEditor materialEditor)
	{
		_materialEditor = materialEditor;
	}

	public void InjectGlowColor(Color color)
	{
		_glowColor = color;
	}

	public void InjectGlowType(GlowType glowType)
	{
		_glowType = glowType;
	}
}
