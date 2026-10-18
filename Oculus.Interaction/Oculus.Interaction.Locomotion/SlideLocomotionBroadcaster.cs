using System;
using Oculus.Interaction.Input;
using UnityEngine;

namespace Oculus.Interaction.Locomotion;

public class SlideLocomotionBroadcaster : MonoBehaviour, ILocomotionEventBroadcaster
{
	[SerializeField]
	[Interface(typeof(IAxis2D), new Type[] { })]
	private UnityEngine.Object _axis2D;

	private IAxis2D Axis2D;

	[SerializeField]
	[Optional]
	private Transform _aiming;

	[SerializeField]
	[Optional]
	private AnimationCurve _verticalDeadZone = AnimationCurve.Linear(-1f, -1f, 1f, 1f);

	[SerializeField]
	[Optional]
	private AnimationCurve _horizontalDeadZone = AnimationCurve.Linear(-1f, -1f, 1f, 1f);

	private Action<LocomotionEvent> _whenLocomotionPerformed = delegate
	{
	};

	private UniqueIdentifier _identifier;

	protected bool _started;

	public Transform Aiming
	{
		get
		{
			return _aiming;
		}
		set
		{
			_aiming = value;
		}
	}

	public AnimationCurve VerticalDeadZone
	{
		get
		{
			return _verticalDeadZone;
		}
		set
		{
			_verticalDeadZone = value;
		}
	}

	public AnimationCurve HorizontalDeadZone
	{
		get
		{
			return _horizontalDeadZone;
		}
		set
		{
			_horizontalDeadZone = value;
		}
	}

	public int Identifier => _identifier.ID;

	public event Action<LocomotionEvent> WhenLocomotionPerformed
	{
		add
		{
			_whenLocomotionPerformed = (Action<LocomotionEvent>)Delegate.Combine(_whenLocomotionPerformed, value);
		}
		remove
		{
			_whenLocomotionPerformed = (Action<LocomotionEvent>)Delegate.Remove(_whenLocomotionPerformed, value);
		}
	}

	protected virtual void Awake()
	{
		_identifier = UniqueIdentifier.Generate(Context.Global.GetInstance(), this);
		Axis2D = _axis2D as IAxis2D;
	}

	protected virtual void Start()
	{
		this.BeginStart(ref _started);
		this.EndStart(ref _started);
	}

	protected virtual void Update()
	{
		Vector2 vector = ProcessAxisSensitivity();
		Pose pose = StepDirection(new Vector3(vector.x, 0f, vector.y));
		if (!Mathf.Approximately(pose.position.sqrMagnitude, 0f))
		{
			LocomotionEvent obj = new LocomotionEvent(Identifier, pose, LocomotionEvent.TranslationType.Velocity, LocomotionEvent.RotationType.None);
			_whenLocomotionPerformed(obj);
		}
	}

	private Vector2 ProcessAxisSensitivity()
	{
		Vector2 result = Axis2D.Value();
		if (_horizontalDeadZone != null)
		{
			result.x = _horizontalDeadZone.Evaluate(result.x);
		}
		if (_verticalDeadZone != null)
		{
			result.y = _verticalDeadZone.Evaluate(result.y);
		}
		return result;
	}

	private Pose StepDirection(Vector3 axisValue)
	{
		if (_aiming == null)
		{
			return new Pose(axisValue, Quaternion.identity);
		}
		return new Pose(_aiming.right * axisValue.x + _aiming.up * axisValue.y + _aiming.forward * axisValue.z, _aiming.rotation);
	}

	public void InjectAllSlideLocomotionBroadcaster(IAxis2D axis2D)
	{
		InjectAxis2D(axis2D);
	}

	public void InjectAxis2D(IAxis2D axis2D)
	{
		_axis2D = axis2D as UnityEngine.Object;
		Axis2D = axis2D;
	}
}
