using System;
using UnityEngine;

namespace Oculus.Interaction.Locomotion;

public class LocomotionActiveState : MonoBehaviour, IActiveState, ITimeConsumer
{
	[SerializeField]
	[Interface(typeof(ILocomotionEventBroadcaster), new Type[] { })]
	private UnityEngine.Object _locomotionBroadcaster;

	[SerializeField]
	private float _idleTime = 0.1f;

	private Func<float> _timeProvider = () => Time.time;

	private float _lastEventTime;

	protected bool _started;

	private ILocomotionEventBroadcaster LocomotionBroadcaster { get; set; }

	public float IdleTime
	{
		get
		{
			return _idleTime;
		}
		set
		{
			_idleTime = value;
		}
	}

	public bool Active { get; private set; }

	public void SetTimeProvider(Func<float> timeProvider)
	{
		_timeProvider = timeProvider;
	}

	protected void Awake()
	{
		if (LocomotionBroadcaster == null)
		{
			LocomotionBroadcaster = _locomotionBroadcaster as ILocomotionEventBroadcaster;
		}
	}

	protected void Start()
	{
		this.BeginStart(ref _started);
		this.EndStart(ref _started);
	}

	protected void OnEnable()
	{
		if (_started)
		{
			LocomotionBroadcaster.WhenLocomotionPerformed += HandleLocomotionPerformed;
		}
	}

	protected void OnDisable()
	{
		if (_started)
		{
			Active = false;
			LocomotionBroadcaster.WhenLocomotionPerformed -= HandleLocomotionPerformed;
		}
	}

	protected void Update()
	{
		if (Active && _timeProvider() - _lastEventTime > _idleTime)
		{
			Active = false;
		}
	}

	private void HandleLocomotionPerformed(LocomotionEvent obj)
	{
		if (obj.Translation != 0 || obj.Rotation != 0)
		{
			_lastEventTime = _timeProvider();
			Active = true;
		}
	}
}
