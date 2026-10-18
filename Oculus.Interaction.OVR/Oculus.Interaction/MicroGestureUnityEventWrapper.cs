using System;
using UnityEngine;
using UnityEngine.Events;

namespace Oculus.Interaction;

internal class MicroGestureUnityEventWrapper : MonoBehaviour
{
	[SerializeField]
	private OVRMicrogestureEventSource _ovrMicrogestureEventSource;

	[SerializeField]
	private UnityEvent _whenTapCenter;

	[SerializeField]
	private UnityEvent _whenSwipeUp;

	[SerializeField]
	private UnityEvent _whenSwipeDown;

	[SerializeField]
	private UnityEvent _whenSwipeLeft;

	[SerializeField]
	private UnityEvent _whenSwipeRight;

	private bool _started;

	public UnityEvent WhenTapCenter => _whenTapCenter;

	public UnityEvent WhenSwipeUp => _whenSwipeUp;

	public UnityEvent WhenSwipeDown => _whenSwipeDown;

	public UnityEvent WhenSwipeLeft => _whenSwipeLeft;

	public UnityEvent WhenSwipeRight => _whenSwipeRight;

	protected virtual void Start()
	{
		this.BeginStart(ref _started);
		this.EndStart(ref _started);
	}

	protected virtual void OnEnable()
	{
		if (_started)
		{
			OVRMicrogestureEventSource ovrMicrogestureEventSource = _ovrMicrogestureEventSource;
			ovrMicrogestureEventSource.WhenGestureRecognized = (Action<OVRHand.MicrogestureType>)Delegate.Combine(ovrMicrogestureEventSource.WhenGestureRecognized, new Action<OVRHand.MicrogestureType>(HandleGesture));
		}
	}

	protected virtual void OnDisable()
	{
		if (_started)
		{
			OVRMicrogestureEventSource ovrMicrogestureEventSource = _ovrMicrogestureEventSource;
			ovrMicrogestureEventSource.WhenGestureRecognized = (Action<OVRHand.MicrogestureType>)Delegate.Remove(ovrMicrogestureEventSource.WhenGestureRecognized, new Action<OVRHand.MicrogestureType>(HandleGesture));
		}
	}

	private void HandleGesture(OVRHand.MicrogestureType gesture)
	{
		switch (gesture)
		{
		case OVRHand.MicrogestureType.SwipeRight:
			_whenSwipeRight.Invoke();
			break;
		case OVRHand.MicrogestureType.SwipeLeft:
			_whenSwipeLeft.Invoke();
			break;
		case OVRHand.MicrogestureType.SwipeForward:
			_whenSwipeUp.Invoke();
			break;
		case OVRHand.MicrogestureType.SwipeBackward:
			_whenSwipeDown.Invoke();
			break;
		case OVRHand.MicrogestureType.ThumbTap:
			_whenTapCenter.Invoke();
			break;
		}
	}

	public void InjectAllMicroGestureUnityEventWrapper(OVRMicrogestureEventSource ovrMicrogestureEventSource)
	{
		InjectOvrMicrogestureEventSource(ovrMicrogestureEventSource);
	}

	public void InjectOvrMicrogestureEventSource(OVRMicrogestureEventSource ovrMicrogestureEventSource)
	{
		_ovrMicrogestureEventSource = ovrMicrogestureEventSource;
	}
}
