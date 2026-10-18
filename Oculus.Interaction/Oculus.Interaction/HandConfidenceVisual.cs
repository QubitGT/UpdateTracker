using System;
using Oculus.Interaction.Input;
using UnityEngine;

namespace Oculus.Interaction;

public class HandConfidenceVisual : MonoBehaviour
{
	[SerializeField]
	[Interface(typeof(IHand), new Type[] { })]
	private UnityEngine.Object _hand;

	[SerializeField]
	private MaterialPropertyBlockEditor _handMaterialPropertyBlockEditor;

	[SerializeField]
	private float _speed = 5f;

	private readonly int _handConfidenceId = Shader.PropertyToID("_JointsGlow");

	private float[] _jointsConfidence = new float[18];

	protected bool _started;

	private float _lastTime;

	private IHand Hand { get; set; }

	public float Speed
	{
		get
		{
			return _speed;
		}
		set
		{
			_speed = value;
		}
	}

	private void Awake()
	{
		Hand = _hand as IHand;
	}

	protected virtual void Start()
	{
		this.BeginStart(ref _started);
		_lastTime = Time.time;
		this.EndStart(ref _started);
	}

	protected virtual void OnEnable()
	{
		if (_started)
		{
			Hand.WhenHandUpdated += UpdateVisual;
		}
	}

	protected virtual void OnDisable()
	{
		if (_started)
		{
			Hand.WhenHandUpdated -= UpdateVisual;
		}
	}

	private void UpdateVisual()
	{
		float changeRate = (Time.time - _lastTime) * Speed;
		_lastTime = Time.time;
		float b = (Hand.IsHighConfidence ? 0f : 1f);
		_jointsConfidence[0] = Mathf.Lerp(_jointsConfidence[0], b, changeRate);
		FillConfidence(HandFinger.Thumb, 1, 4);
		FillConfidence(HandFinger.Index, 5, 3);
		FillConfidence(HandFinger.Middle, 8, 3);
		FillConfidence(HandFinger.Ring, 11, 3);
		FillConfidence(HandFinger.Pinky, 14, 4);
		_handMaterialPropertyBlockEditor.MaterialPropertyBlock.SetFloatArray(_handConfidenceId, _jointsConfidence);
		_handMaterialPropertyBlockEditor.UpdateMaterialPropertyBlock();
		void FillConfidence(HandFinger finger, int offset, int lenght)
		{
			int num = ((!Hand.GetFingerIsHighConfidence(finger)) ? 1 : 0);
			for (int i = offset; i < offset + lenght; i++)
			{
				_jointsConfidence[i] = Mathf.Lerp(_jointsConfidence[i], num, changeRate);
			}
		}
	}

	public void InjectAllHandConfidenceVisual(IHand hand, MaterialPropertyBlockEditor handMaterialPropertyBlockEditor)
	{
		InjectHand(hand);
		InjectHandMaterialPropertyBlockEditor(handMaterialPropertyBlockEditor);
	}

	public void InjectHand(IHand hand)
	{
		_hand = hand as UnityEngine.Object;
		Hand = hand;
	}

	public void InjectHandMaterialPropertyBlockEditor(MaterialPropertyBlockEditor handMaterialPropertyBlockEditor)
	{
		_handMaterialPropertyBlockEditor = handMaterialPropertyBlockEditor;
	}
}
