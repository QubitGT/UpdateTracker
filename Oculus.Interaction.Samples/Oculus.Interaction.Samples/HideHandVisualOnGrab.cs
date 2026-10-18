using System;
using Oculus.Interaction.HandGrab;
using UnityEngine;

namespace Oculus.Interaction.Samples;

public class HideHandVisualOnGrab : MonoBehaviour
{
	[SerializeField]
	private HandGrabInteractor _handGrabInteractor;

	[SerializeField]
	[Interface(typeof(IHandVisual), new Type[] { })]
	private UnityEngine.Object _handVisual;

	private IHandVisual HandVisual;

	protected virtual void Awake()
	{
		HandVisual = _handVisual as IHandVisual;
	}

	protected virtual void Start()
	{
	}

	protected virtual void Update()
	{
		GameObject gameObject = null;
		if (_handGrabInteractor.State == InteractorState.Select)
		{
			gameObject = _handGrabInteractor.SelectedInteractable?.gameObject;
		}
		if ((bool)gameObject)
		{
			if (gameObject.TryGetComponent<ShouldHideHandOnGrab>(out var _))
			{
				HandVisual.ForceOffVisibility = true;
			}
		}
		else
		{
			HandVisual.ForceOffVisibility = false;
		}
	}

	public void InjectAll(HandGrabInteractor handGrabInteractor, IHandVisual handVisual)
	{
		InjectHandGrabInteractor(handGrabInteractor);
		InjectHandVisual(handVisual);
	}

	private void InjectHandGrabInteractor(HandGrabInteractor handGrabInteractor)
	{
		_handGrabInteractor = handGrabInteractor;
	}

	private void InjectHandVisual(IHandVisual handVisual)
	{
		_handVisual = handVisual as UnityEngine.Object;
		HandVisual = handVisual;
	}
}
