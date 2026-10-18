using System;
using Oculus.Interaction.Input;
using UnityEngine;

namespace Oculus.Interaction;

public class RayInteractorPinchVisual : MonoBehaviour
{
	[SerializeField]
	[Interface(typeof(IHand), new Type[] { })]
	private UnityEngine.Object _hand;

	private IHand Hand;

	[SerializeField]
	private RayInteractor _rayInteractor;

	[SerializeField]
	private SkinnedMeshRenderer _skinnedMeshRenderer;

	[SerializeField]
	private AnimationCurve _remapCurve;

	[SerializeField]
	private Vector2 _alphaRange = new Vector2(0.1f, 0.4f);

	protected bool _started;

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

	protected virtual void Awake()
	{
		Hand = _hand as IHand;
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
		if (!Hand.IsTrackedDataValid || _rayInteractor.State == InteractorState.Disabled)
		{
			if (_skinnedMeshRenderer.enabled)
			{
				_skinnedMeshRenderer.enabled = false;
			}
			return;
		}
		if (!_skinnedMeshRenderer.enabled)
		{
			_skinnedMeshRenderer.enabled = true;
		}
		if (Hand.GetJointPose(HandJointId.HandIndex3, out var pose) && Hand.GetJointPose(HandJointId.HandThumb3, out var pose2))
		{
			bool flag = _rayInteractor.State == InteractorState.Select;
			Vector3 position = Vector3.Lerp(pose2.position, pose.position, 0.5f);
			Transform transform = base.transform;
			Vector3 normalized = (_rayInteractor.End - transform.position).normalized;
			transform.position = position;
			transform.rotation = Quaternion.LookRotation(normalized, Vector3.up);
			transform.localScale = Vector3.one * Hand.Scale;
			float num = _remapCurve.Evaluate(Hand.GetFingerPinchStrength(HandFinger.Index));
			_skinnedMeshRenderer.material.color = (flag ? Color.white : new Color(1f, 1f, 1f, Mathf.Lerp(_alphaRange.x, _alphaRange.y, num)));
			_skinnedMeshRenderer.SetBlendShapeWeight(0, num * 100f);
			_skinnedMeshRenderer.SetBlendShapeWeight(1, num * 100f);
		}
	}

	private void UpdateVisualState(InteractorStateChangeArgs args)
	{
		UpdateVisual();
	}

	public void InjectAllRayInteractorPinchVisual(IHand hand, RayInteractor rayInteractor, SkinnedMeshRenderer skinnedMeshRenderer)
	{
		InjectHand(hand);
		InjectRayInteractor(rayInteractor);
		InjectSkinnedMeshRenderer(skinnedMeshRenderer);
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

	public void InjectSkinnedMeshRenderer(SkinnedMeshRenderer skinnedMeshRenderer)
	{
		_skinnedMeshRenderer = skinnedMeshRenderer;
	}
}
