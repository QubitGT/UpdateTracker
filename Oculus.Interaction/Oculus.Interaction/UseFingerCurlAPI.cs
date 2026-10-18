using System;
using Oculus.Interaction.GrabAPI;
using Oculus.Interaction.Input;
using UnityEngine;

namespace Oculus.Interaction;

public class UseFingerCurlAPI : MonoBehaviour, IFingerUseAPI
{
	[SerializeField]
	[Interface(typeof(IHand), new Type[] { })]
	private UnityEngine.Object _hand;

	private IFingerAPI _grabAPI = new PalmGrabAPI();

	private int _lastDataVersion = -1;

	protected bool _started;

	private IHand Hand { get; set; }

	protected virtual void Awake()
	{
		Hand = _hand as IHand;
	}

	protected virtual void Start()
	{
		this.BeginStart(ref _started);
		this.EndStart(ref _started);
	}

	public float GetFingerUseStrength(HandFinger finger)
	{
		if (_lastDataVersion != Hand.CurrentDataVersion)
		{
			_lastDataVersion = Hand.CurrentDataVersion;
			_grabAPI.Update(Hand);
		}
		return _grabAPI.GetFingerGrabScore(finger);
	}

	public void InjectAllUseFingerCurlAPI(IHand hand)
	{
		InjectHand(hand);
	}

	public void InjectHand(IHand hand)
	{
		_hand = hand as UnityEngine.Object;
		Hand = hand;
	}
}
