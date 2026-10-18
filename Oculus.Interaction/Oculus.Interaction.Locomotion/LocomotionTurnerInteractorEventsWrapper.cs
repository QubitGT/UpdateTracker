using UnityEngine;
using UnityEngine.Events;

namespace Oculus.Interaction.Locomotion;

public class LocomotionTurnerInteractorEventsWrapper : MonoBehaviour
{
	[SerializeField]
	private LocomotionTurnerInteractor _turner;

	[SerializeField]
	private UnityEvent _whenTurnDirectionLeft;

	[SerializeField]
	private UnityEvent _whenTurnDirectionRight;

	protected bool _started;

	public UnityEvent WhenTurnDirectionLeft => _whenTurnDirectionLeft;

	public UnityEvent WhenTurnDirectionRight => _whenTurnDirectionRight;

	protected virtual void Start()
	{
		this.BeginStart(ref _started);
		this.EndStart(ref _started);
	}

	protected virtual void OnEnable()
	{
		if (_started)
		{
			_turner.WhenTurnDirectionChanged += HandleTurnDirectionChanged;
		}
	}

	protected virtual void OnDisable()
	{
		if (_started)
		{
			_turner.WhenTurnDirectionChanged -= HandleTurnDirectionChanged;
		}
	}

	private void HandleTurnDirectionChanged(float dir)
	{
		if (dir > 0f)
		{
			_whenTurnDirectionLeft.Invoke();
		}
		else if (dir < 0f)
		{
			_whenTurnDirectionRight.Invoke();
		}
	}

	public void InjectAllLocomotionTurnerInteractorEventsWrapper(LocomotionTurnerInteractor turner)
	{
		InjectTurner(turner);
	}

	public void InjectTurner(LocomotionTurnerInteractor turner)
	{
		_turner = turner;
	}
}
