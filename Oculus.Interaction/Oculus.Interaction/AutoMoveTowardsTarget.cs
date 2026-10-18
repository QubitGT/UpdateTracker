using System;
using UnityEngine;

namespace Oculus.Interaction;

public class AutoMoveTowardsTarget : IMovement
{
	private PoseTravelData _travellingData;

	private IPointableElement _pointableElement;

	public Action<AutoMoveTowardsTarget> WhenAborted = delegate
	{
	};

	private UniqueIdentifier _identifier;

	private Tween _tween;

	private Pose _target;

	private Pose _source;

	private bool _eventRegistered;

	public Pose Pose => _tween.Pose;

	public bool Stopped
	{
		get
		{
			if (_tween != null)
			{
				return _tween.Stopped;
			}
			return true;
		}
	}

	public bool Aborting { get; private set; }

	public int Identifier => _identifier.ID;

	public AutoMoveTowardsTarget(PoseTravelData travellingData, IPointableElement pointableElement)
	{
		_identifier = UniqueIdentifier.Generate(Context.Global.GetInstance(), this);
		_travellingData = travellingData;
		_pointableElement = pointableElement;
	}

	public void MoveTo(Pose target)
	{
		AbortSelfAligment();
		_target = target;
		_tween = _travellingData.CreateTween(in _source, in target);
		if (!_eventRegistered)
		{
			_pointableElement.WhenPointerEventRaised += HandlePointerEventRaised;
			_eventRegistered = true;
		}
	}

	public void UpdateTarget(Pose target)
	{
		_target = target;
		_tween.UpdateTarget(_target);
	}

	public void StopAndSetPose(Pose pose)
	{
		if (_eventRegistered)
		{
			_pointableElement.WhenPointerEventRaised -= HandlePointerEventRaised;
			_eventRegistered = false;
		}
		_source = pose;
		if (_tween != null && !_tween.Stopped)
		{
			GeneratePointerEvent(PointerEventType.Hover);
			GeneratePointerEvent(PointerEventType.Select);
			Aborting = true;
			WhenAborted(this);
		}
	}

	public void Tick()
	{
		_tween.Tick();
		if (Aborting)
		{
			GeneratePointerEvent(PointerEventType.Move);
			if (_tween.Stopped)
			{
				AbortSelfAligment();
			}
		}
	}

	private void HandlePointerEventRaised(PointerEvent evt)
	{
		if (evt.Type == PointerEventType.Select || evt.Type == PointerEventType.Unselect)
		{
			AbortSelfAligment();
		}
	}

	private void AbortSelfAligment()
	{
		if (Aborting)
		{
			Aborting = false;
			GeneratePointerEvent(PointerEventType.Unselect);
			GeneratePointerEvent(PointerEventType.Unhover);
		}
	}

	private void GeneratePointerEvent(PointerEventType pointerEventType)
	{
		PointerEvent evt = new PointerEvent(Identifier, pointerEventType, Pose);
		_pointableElement.ProcessPointerEvent(evt);
	}
}
