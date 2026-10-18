using System;
using UnityEngine;

namespace Oculus.Interaction.Locomotion;

public class StepLocomotionBroadcaster : MonoBehaviour, ILocomotionEventBroadcaster
{
	[SerializeField]
	private Transform _origin;

	[SerializeField]
	private float _stepLength = 0.5f;

	protected bool _started;

	private UniqueIdentifier _identifier;

	public Transform Origin
	{
		get
		{
			return _origin;
		}
		set
		{
			_origin = value;
		}
	}

	public float StepLength
	{
		get
		{
			return _stepLength;
		}
		set
		{
			_stepLength = value;
		}
	}

	public int Identifier => _identifier.ID;

	public event Action<LocomotionEvent> WhenLocomotionPerformed = delegate
	{
	};

	protected virtual void Awake()
	{
		_identifier = UniqueIdentifier.Generate(Context.Global.GetInstance(), this);
	}

	protected virtual void Start()
	{
		this.BeginStart(ref _started);
		this.EndStart(ref _started);
	}

	public void StepLeft()
	{
		Step(Vector2Int.left);
	}

	public void StepRight()
	{
		Step(Vector2Int.right);
	}

	public void StepForward()
	{
		Step(Vector2Int.up);
	}

	public void StepBackward()
	{
		Step(Vector2Int.down);
	}

	public void Step(Vector2Int relativeDirection)
	{
		Vector3 forward = _origin.forward;
		Vector3 up = Vector3.up;
		Vector3 position = (Quaternion.LookRotation(Vector3.ProjectOnPlane(forward, up).normalized, up) * new Vector3(relativeDirection.x, 0f, relativeDirection.y)).normalized * _stepLength;
		LocomotionEvent obj = new LocomotionEvent(Identifier, position, LocomotionEvent.TranslationType.Relative);
		this.WhenLocomotionPerformed(obj);
	}
}
