using System;
using Oculus.Interaction.Input;
using UnityEngine;
using UnityEngine.Serialization;

namespace Oculus.Interaction;

public class Axis1DFingerUseAPI : MonoBehaviour, IFingerUseAPI
{
	[SerializeField]
	[Interface(typeof(IHand), new Type[] { })]
	private UnityEngine.Object _hand;

	[FormerlySerializedAs("_pressureAxis")]
	[FormerlySerializedAs("_pinchPressure")]
	[SerializeField]
	[Interface(typeof(IAxis1D), new Type[] { })]
	private UnityEngine.Object _axis;

	protected IHand Hand;

	protected IAxis1D Axis;

	protected bool _started;

	protected virtual void Awake()
	{
		Hand = _hand as IHand;
		Axis = _axis as IAxis1D;
	}

	protected virtual void Start()
	{
		this.BeginStart(ref _started);
		this.EndStart(ref _started);
	}

	public float GetFingerUseStrength(HandFinger finger)
	{
		if (!Hand.GetFingerIsPinching(finger))
		{
			return 0f;
		}
		return Axis.Value();
	}

	public void InjectAllUseFingerPinchPressureApi(IHand hand, IAxis1D axis)
	{
		InjectHand(hand);
		InjectAxis(axis);
	}

	public void InjectHand(IHand hand)
	{
		_hand = hand as UnityEngine.Object;
		Hand = hand;
	}

	public void InjectAxis(IAxis1D pinchPressure)
	{
		Axis = pinchPressure;
		_axis = pinchPressure as UnityEngine.Object;
	}
}
