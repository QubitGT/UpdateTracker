using System;
using UnityEngine;

namespace Oculus.Interaction.Locomotion;

public class LocomotorSound : MonoBehaviour
{
	[SerializeField]
	[Interface(typeof(ILocomotionEventHandler), new Type[] { })]
	private UnityEngine.Object _locomotor;

	[SerializeField]
	private AdjustableAudio _translationSound;

	[SerializeField]
	private AdjustableAudio _translationDeniedSound;

	[SerializeField]
	private AdjustableAudio _snapTurnSound;

	[SerializeField]
	private AnimationCurve _translationCurve = AnimationCurve.EaseInOut(0f, 0f, 2f, 1f);

	[SerializeField]
	private AnimationCurve _rotationCurve = AnimationCurve.EaseInOut(0f, 0f, 180f, 1f);

	[SerializeField]
	private float _pitchVariance = 0.05f;

	protected bool _started;

	private ILocomotionEventHandler Locomotor { get; set; }

	protected virtual void Awake()
	{
		Locomotor = _locomotor as ILocomotionEventHandler;
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
			Locomotor.WhenLocomotionEventHandled += HandleLocomotionEvent;
		}
	}

	protected virtual void OnDisable()
	{
		if (_started)
		{
			Locomotor.WhenLocomotionEventHandled -= HandleLocomotionEvent;
		}
	}

	private void HandleLocomotionEvent(LocomotionEvent locomotionEvent, Pose delta)
	{
		if (locomotionEvent.Translation == LocomotionEvent.TranslationType.Absolute || locomotionEvent.Translation == LocomotionEvent.TranslationType.AbsoluteEyeLevel || locomotionEvent.Translation == LocomotionEvent.TranslationType.Relative)
		{
			PlayTranslationSound(delta.position.magnitude);
		}
		if (locomotionEvent.Rotation == LocomotionEvent.RotationType.Relative)
		{
			PlayRotationSound(delta.rotation.y * delta.rotation.w);
		}
		if (locomotionEvent.Translation == LocomotionEvent.TranslationType.None && locomotionEvent.Rotation == LocomotionEvent.RotationType.None)
		{
			PlayDenialSound(delta.position.magnitude);
		}
	}

	private void PlayTranslationSound(float translationDistance)
	{
		float num = _translationCurve.Evaluate(translationDistance);
		float pitchT = num + UnityEngine.Random.Range(0f - _pitchVariance, _pitchVariance);
		_translationSound.PlayAudio(num, pitchT);
	}

	private void PlayDenialSound(float translationDistance)
	{
		float num = _translationCurve.Evaluate(translationDistance);
		float pitchT = num + UnityEngine.Random.Range(0f - _pitchVariance, _pitchVariance);
		_translationDeniedSound.PlayAudio(num, pitchT);
	}

	private void PlayRotationSound(float rotationLength)
	{
		float num = _rotationCurve.Evaluate(Mathf.Abs(rotationLength));
		float pitchT = num + UnityEngine.Random.Range(0f - _pitchVariance, _pitchVariance);
		_snapTurnSound.PlayAudio(num, pitchT, rotationLength);
	}

	public void InjectAllLocomotorSound(ILocomotionEventHandler locomotor)
	{
		InjectPlayerLocomotor(locomotor);
	}

	public void InjectPlayerLocomotor(ILocomotionEventHandler locomotor)
	{
		_locomotor = locomotor as UnityEngine.Object;
		Locomotor = locomotor;
	}
}
