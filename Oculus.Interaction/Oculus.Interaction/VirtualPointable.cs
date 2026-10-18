using System;
using UnityEngine;

namespace Oculus.Interaction;

public class VirtualPointable : MonoBehaviour, IPointable
{
	[SerializeField]
	private bool _grabFlag;

	private UniqueIdentifier _id;

	private bool _currentlyGrabbing;

	public event Action<PointerEvent> WhenPointerEventRaised = delegate
	{
	};

	protected virtual void Awake()
	{
		_id = UniqueIdentifier.Generate(Context.Global.GetInstance(), this);
	}

	protected virtual void Update()
	{
		if (_currentlyGrabbing != _grabFlag)
		{
			_currentlyGrabbing = _grabFlag;
			if (_currentlyGrabbing)
			{
				this.WhenPointerEventRaised(new PointerEvent(_id.ID, PointerEventType.Hover, base.transform.GetPose()));
				this.WhenPointerEventRaised(new PointerEvent(_id.ID, PointerEventType.Select, base.transform.GetPose()));
			}
			else
			{
				this.WhenPointerEventRaised(new PointerEvent(_id.ID, PointerEventType.Unselect, base.transform.GetPose()));
				this.WhenPointerEventRaised(new PointerEvent(_id.ID, PointerEventType.Unhover, base.transform.GetPose()));
			}
		}
		else if (_currentlyGrabbing)
		{
			this.WhenPointerEventRaised(new PointerEvent(_id.ID, PointerEventType.Move, base.transform.GetPose()));
		}
	}

	public void SetGrabFlag(bool grabFlag)
	{
		_grabFlag = grabFlag;
	}

	protected virtual void OnDestroy()
	{
		UniqueIdentifier.Release(_id);
	}
}
