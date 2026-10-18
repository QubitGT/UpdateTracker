using System;
using System.Collections.Generic;
using UnityEngine;

namespace Oculus.Interaction;

public class AutoMoveTowardsTargetProvider : MonoBehaviour, IMovementProvider
{
	[SerializeField]
	private PoseTravelData _travellingData = PoseTravelData.DEFAULT;

	[SerializeField]
	[Interface(typeof(IPointableElement), new Type[] { })]
	private UnityEngine.Object _pointableElement;

	private bool _started;

	public List<AutoMoveTowardsTarget> _movers = new List<AutoMoveTowardsTarget>();

	public PoseTravelData TravellingData
	{
		get
		{
			return _travellingData;
		}
		set
		{
			_travellingData = value;
		}
	}

	public IPointableElement PointableElement { get; private set; }

	protected virtual void Awake()
	{
		PointableElement = _pointableElement as IPointableElement;
	}

	protected virtual void Start()
	{
		this.BeginStart(ref _started);
		this.EndStart(ref _started);
	}

	private void LateUpdate()
	{
		for (int num = _movers.Count - 1; num >= 0; num--)
		{
			AutoMoveTowardsTarget autoMoveTowardsTarget = _movers[num];
			if (autoMoveTowardsTarget.Aborting)
			{
				autoMoveTowardsTarget.Tick();
				if (autoMoveTowardsTarget.Stopped)
				{
					_movers.Remove(autoMoveTowardsTarget);
				}
			}
		}
	}

	public IMovement CreateMovement()
	{
		AutoMoveTowardsTarget autoMoveTowardsTarget = new AutoMoveTowardsTarget(_travellingData, PointableElement);
		autoMoveTowardsTarget.WhenAborted = (Action<AutoMoveTowardsTarget>)Delegate.Combine(autoMoveTowardsTarget.WhenAborted, new Action<AutoMoveTowardsTarget>(HandleAborted));
		return autoMoveTowardsTarget;
	}

	private void HandleAborted(AutoMoveTowardsTarget mover)
	{
		mover.WhenAborted = (Action<AutoMoveTowardsTarget>)Delegate.Remove(mover.WhenAborted, new Action<AutoMoveTowardsTarget>(HandleAborted));
		_movers.Add(mover);
	}

	public void InjectAllAutoMoveTowardsTargetProvider(IPointableElement pointableElement)
	{
		InjectPointableElement(pointableElement);
	}

	public void InjectPointableElement(IPointableElement pointableElement)
	{
		PointableElement = pointableElement;
		_pointableElement = pointableElement as UnityEngine.Object;
	}
}
