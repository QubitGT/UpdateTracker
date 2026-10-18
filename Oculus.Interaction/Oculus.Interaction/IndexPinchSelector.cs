using System;
using Oculus.Interaction.Input;
using UnityEngine;

namespace Oculus.Interaction;

public class IndexPinchSelector : MonoBehaviour, ISelector
{
	[Tooltip("The hand to check.")]
	[SerializeField]
	[Interface(typeof(IHand), new Type[] { })]
	private UnityEngine.Object _hand;

	private bool _isIndexFingerPinching;

	protected bool _started;

	public IHand Hand { get; private set; }

	public event Action WhenSelected = delegate
	{
	};

	public event Action WhenUnselected = delegate
	{
	};

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

	private void HandleHandUpdated()
	{
		bool isIndexFingerPinching = _isIndexFingerPinching;
		_isIndexFingerPinching = Hand.GetIndexFingerIsPinching();
		if (isIndexFingerPinching != _isIndexFingerPinching)
		{
			if (_isIndexFingerPinching)
			{
				this.WhenSelected();
			}
			else
			{
				this.WhenUnselected();
			}
		}
	}

	public void InjectAllIndexPinchSelector(IHand hand)
	{
		InjectHand(hand);
	}

	public void InjectHand(IHand hand)
	{
		_hand = hand as UnityEngine.Object;
		Hand = hand;
	}
}
