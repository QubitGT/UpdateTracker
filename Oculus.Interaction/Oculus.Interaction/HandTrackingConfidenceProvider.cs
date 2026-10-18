using System;
using System.Collections.Generic;
using Oculus.Interaction.Input;
using UnityEngine;

namespace Oculus.Interaction;

public class HandTrackingConfidenceProvider : MonoBehaviour
{
	[SerializeField]
	[Interface(typeof(IInteractor), new Type[] { })]
	private UnityEngine.Object _interactor;

	private IInteractor Interactor;

	[SerializeField]
	[Interface(typeof(IHand), new Type[] { })]
	private UnityEngine.Object _hand;

	private static Dictionary<int, HandTrackingConfidenceProvider> _interactorTrackingConfidence;

	protected bool _started;

	private IHand Hand { get; set; }

	protected virtual void Reset()
	{
		_interactor = GetComponent<IInteractor>() as UnityEngine.Object;
		_hand = GetComponent<IHand>() as UnityEngine.Object;
	}

	protected virtual void Awake()
	{
		if (_interactorTrackingConfidence == null)
		{
			_interactorTrackingConfidence = new Dictionary<int, HandTrackingConfidenceProvider>();
		}
		Interactor = _interactor as IInteractor;
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
			int identifier = Interactor.Identifier;
			if (_interactorTrackingConfidence != null && !_interactorTrackingConfidence.ContainsKey(identifier))
			{
				_interactorTrackingConfidence.Add(identifier, this);
			}
			else
			{
				Debug.LogError("This interactor was already added to HandTrackingConfidenceProvider. Ensure each interactor is paired just once");
			}
		}
	}

	protected virtual void OnDisable()
	{
		if (_started)
		{
			int identifier = Interactor.Identifier;
			if (_interactorTrackingConfidence != null && _interactorTrackingConfidence.ContainsKey(identifier))
			{
				_interactorTrackingConfidence.Remove(Interactor.Identifier);
			}
		}
	}

	public static bool TryGetTrackingConfidence(int key, out bool isTrackingHighConfidence)
	{
		if (_interactorTrackingConfidence != null && _interactorTrackingConfidence.ContainsKey(key))
		{
			isTrackingHighConfidence = _interactorTrackingConfidence[key].Hand.IsHighConfidence;
			return true;
		}
		isTrackingHighConfidence = true;
		return false;
	}

	public void InjectAllHandTrackingConfidenceProvider(IInteractor interactor, IHand hand)
	{
		InjectInteractor(interactor);
		InjectHand(hand);
	}

	public void InjectInteractor(IInteractor interactor)
	{
		_interactor = interactor as UnityEngine.Object;
		Interactor = interactor;
	}

	public void InjectHand(IHand hand)
	{
		_hand = hand as UnityEngine.Object;
		Hand = hand;
	}
}
