using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Oculus.Interaction.Samples;

public class AnimatorOverrideLayerWeigth : MonoBehaviour
{
	[SerializeField]
	[FormerlySerializedAs("animator")]
	private Animator _animator;

	[SerializeField]
	[FormerlySerializedAs("overrideLayer")]
	private string _overrideLayer = "Selected Layer";

	[SerializeField]
	[FormerlySerializedAs("transitionDuration")]
	public float _transitionDuration = 0.2f;

	[SerializeField]
	[FormerlySerializedAs("transitionCurve")]
	public AnimationCurve _transitionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	[Space]
	[SerializeField]
	[Optional(OptionalAttribute.Flag.DontHide)]
	[Tooltip("If provided, the animation layer will be syncronized with the isOn state of the toggle")]
	public Toggle _toggle;

	private bool _layerIsActive;

	private int _layerIndex = -1;

	protected bool _started;

	public float TransitionDuration
	{
		get
		{
			return _transitionDuration;
		}
		set
		{
			_transitionDuration = value;
		}
	}

	public AnimationCurve TransitionCurve
	{
		get
		{
			return _transitionCurve;
		}
		set
		{
			_transitionCurve = value;
		}
	}

	protected virtual void Reset()
	{
		_animator = GetComponent<Animator>();
		_toggle = GetComponent<Toggle>();
	}

	protected virtual void Start()
	{
		this.BeginStart(ref _started);
		_layerIndex = _animator.GetLayerIndex(_overrideLayer);
		this.EndStart(ref _started);
	}

	protected virtual void OnEnable()
	{
		if (_started)
		{
			if (_layerIsActive)
			{
				_animator.SetLayerWeight(_layerIndex, 1f);
			}
			if (_toggle != null)
			{
				_toggle.onValueChanged.AddListener(SetOverrideLayerActive);
				SetOverrideLayerActive(_toggle.isOn);
			}
		}
	}

	protected virtual void OnDisable()
	{
		if (_started && _toggle != null)
		{
			_toggle.onValueChanged.RemoveListener(SetOverrideLayerActive);
		}
	}

	public void SetOverrideLayerActive(bool active)
	{
		_layerIsActive = active;
		if ((double)_transitionDuration > 0.0)
		{
			StopAllCoroutines();
			StartCoroutine(LayerTransition(_layerIndex, active ? 1f : 0f));
		}
		else
		{
			_animator.SetLayerWeight(_layerIndex, active ? 1f : 0f);
		}
	}

	private IEnumerator LayerTransition(int layerIndex, float targetWeight)
	{
		float startTime = Time.time;
		float startWeight = _animator.GetLayerWeight(layerIndex);
		while (true)
		{
			float num = (Time.time - startTime) / _transitionDuration;
			float t = _transitionCurve.Evaluate(Mathf.Clamp01(num));
			float weight = Mathf.Lerp(startWeight, targetWeight, t);
			_animator.SetLayerWeight(layerIndex, weight);
			if ((double)num >= 1.0)
			{
				break;
			}
			yield return null;
		}
	}

	public void InjectAllAnimatorOverrideLayerWeigth(Animator animator, string overrideLayer)
	{
		InjectAnimator(animator);
		InjectOverrideLayer(overrideLayer);
	}

	public void InjectAnimator(Animator animator)
	{
		_animator = animator;
	}

	public void InjectOverrideLayer(string overrideLayer)
	{
		_overrideLayer = overrideLayer;
	}

	public void InjectOptionalToggle(Toggle toggle)
	{
		_toggle = toggle;
	}
}
