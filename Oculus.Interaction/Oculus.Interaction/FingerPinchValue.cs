using System;
using Oculus.Interaction.Input;
using UnityEngine;

namespace Oculus.Interaction;

public class FingerPinchValue : MonoBehaviour, IAxis1D
{
	[SerializeField]
	[Interface(typeof(IHand), new Type[] { })]
	private UnityEngine.Object _hand;

	[SerializeField]
	private HandFinger _finger = HandFinger.Index;

	[SerializeField]
	[Range(0f, 1f)]
	private float _changeRate = 1f;

	[SerializeField]
	private AnimationCurve _curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	private float _value;

	protected bool _started;

	private bool _firstCall;

	public IHand Hand { get; private set; }

	public HandFinger Finger
	{
		get
		{
			return _finger;
		}
		set
		{
			_finger = value;
		}
	}

	public float ChangeRate
	{
		get
		{
			return _changeRate;
		}
		private set
		{
			_changeRate = value;
		}
	}

	public AnimationCurve Curve
	{
		get
		{
			return _curve;
		}
		set
		{
			_curve = value;
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
			_firstCall = true;
			Hand.WhenHandUpdated += HandleHandUpdated;
		}
	}

	protected virtual void OnDisable()
	{
		if (_started)
		{
			Hand.WhenHandUpdated -= HandleHandUpdated;
		}
	}

	public float Value()
	{
		return _value;
	}

	private void HandleHandUpdated()
	{
		float fingerPinchStrength = Hand.GetFingerPinchStrength(Finger);
		fingerPinchStrength = Curve.Evaluate(fingerPinchStrength);
		if (_firstCall)
		{
			_firstCall = false;
			_value = fingerPinchStrength;
		}
		else
		{
			_value = Mathf.Lerp(_value, fingerPinchStrength, _changeRate);
		}
	}

	public void InjectAllFingerPinchValue(IHand hand)
	{
		InjectHand(hand);
	}

	public void InjectHand(IHand hand)
	{
		_hand = hand as UnityEngine.Object;
		Hand = hand;
	}
}
