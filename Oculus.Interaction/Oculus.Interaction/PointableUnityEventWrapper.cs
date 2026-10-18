using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Oculus.Interaction;

public class PointableUnityEventWrapper : MonoBehaviour
{
	[Tooltip("The Pointable component to wrap.")]
	[SerializeField]
	[Interface(typeof(IPointable), new Type[] { })]
	private UnityEngine.Object _pointable;

	private IPointable Pointable;

	private HashSet<int> _pointers;

	[Tooltip("Raised when the IPointable is released.")]
	[SerializeField]
	private UnityEvent<PointerEvent> _whenRelease;

	[Tooltip("Raised when the IPointable is hovered.")]
	[SerializeField]
	private UnityEvent<PointerEvent> _whenHover;

	[Tooltip("Raised when the IPointable is unhovered (it was hovered but now it isn't).")]
	[SerializeField]
	private UnityEvent<PointerEvent> _whenUnhover;

	[Tooltip("Raised when the IPointable is selected.")]
	[SerializeField]
	private UnityEvent<PointerEvent> _whenSelect;

	[Tooltip("Raised when the IPointable is unselected (it was selected but now it isn't).")]
	[SerializeField]
	private UnityEvent<PointerEvent> _whenUnselect;

	[Tooltip("Raised when the IPointable moves.")]
	[SerializeField]
	private UnityEvent<PointerEvent> _whenMove;

	[Tooltip("Raised when the IPointable is canceled.")]
	[SerializeField]
	private UnityEvent<PointerEvent> _whenCancel;

	protected bool _started;

	public UnityEvent<PointerEvent> WhenRelease => _whenRelease;

	public UnityEvent<PointerEvent> WhenHover => _whenHover;

	public UnityEvent<PointerEvent> WhenUnhover => _whenUnhover;

	public UnityEvent<PointerEvent> WhenSelect => _whenSelect;

	public UnityEvent<PointerEvent> WhenUnselect => _whenUnselect;

	public UnityEvent<PointerEvent> WhenMove => _whenMove;

	public UnityEvent<PointerEvent> WhenCancel => _whenCancel;

	protected virtual void Awake()
	{
		Pointable = _pointable as IPointable;
	}

	protected virtual void Start()
	{
		this.BeginStart(ref _started);
		_pointers = new HashSet<int>();
		this.EndStart(ref _started);
	}

	protected virtual void OnEnable()
	{
		if (_started)
		{
			Pointable.WhenPointerEventRaised += HandlePointerEventRaised;
		}
	}

	protected virtual void OnDisable()
	{
		if (_started)
		{
			Pointable.WhenPointerEventRaised -= HandlePointerEventRaised;
		}
	}

	private void HandlePointerEventRaised(PointerEvent evt)
	{
		switch (evt.Type)
		{
		case PointerEventType.Hover:
			_whenHover.Invoke(evt);
			_pointers.Add(evt.Identifier);
			break;
		case PointerEventType.Unhover:
			_whenUnhover.Invoke(evt);
			_pointers.Remove(evt.Identifier);
			break;
		case PointerEventType.Select:
			_whenSelect.Invoke(evt);
			break;
		case PointerEventType.Unselect:
			if (_pointers.Contains(evt.Identifier))
			{
				_whenRelease.Invoke(evt);
			}
			_whenUnselect.Invoke(evt);
			break;
		case PointerEventType.Move:
			_whenMove.Invoke(evt);
			break;
		case PointerEventType.Cancel:
			_whenCancel.Invoke(evt);
			_pointers.Remove(evt.Identifier);
			break;
		}
	}

	public void InjectAllPointableUnityEventWrapper(IPointable pointable)
	{
		InjectPointable(pointable);
	}

	public void InjectPointable(IPointable pointable)
	{
		_pointable = pointable as UnityEngine.Object;
		Pointable = pointable;
	}
}
