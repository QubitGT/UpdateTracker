using UnityEngine;
using UnityEngine.Events;

namespace Oculus.Interaction.Locomotion;

public class LocomotionGateUnityEventWrapper : MonoBehaviour
{
	[SerializeField]
	private LocomotionGate _locomotionGate;

	public UnityEvent WhenEnterLocomotion;

	public UnityEvent WhenExitLocomotion;

	public UnityEvent WhenChangedToTurn;

	public UnityEvent WhenChangedToTeleport;

	protected bool _started;

	protected virtual void Start()
	{
		this.BeginStart(ref _started);
		this.EndStart(ref _started);
	}

	protected virtual void OnEnable()
	{
		if (_started)
		{
			_locomotionGate.WhenActiveModeChanged += HandleActiveModeChanged;
		}
	}

	protected virtual void OnDisable()
	{
		if (_started)
		{
			_locomotionGate.WhenActiveModeChanged -= HandleActiveModeChanged;
		}
	}

	private void HandleActiveModeChanged(LocomotionGate.LocomotionModeEventArgs locomotionModeArgs)
	{
		if (locomotionModeArgs.PreviousMode == LocomotionGate.LocomotionMode.None)
		{
			WhenEnterLocomotion.Invoke();
		}
		else if (locomotionModeArgs.NewMode == LocomotionGate.LocomotionMode.Teleport)
		{
			WhenChangedToTeleport.Invoke();
		}
		else if (locomotionModeArgs.NewMode == LocomotionGate.LocomotionMode.Turn)
		{
			WhenChangedToTurn.Invoke();
		}
		else if (locomotionModeArgs.NewMode == LocomotionGate.LocomotionMode.None)
		{
			WhenExitLocomotion.Invoke();
		}
	}

	public void InjectAllLocomotionGateUnityEventWrapper(LocomotionGate locomotionGate)
	{
		InjectLocomotionGate(locomotionGate);
	}

	public void InjectLocomotionGate(LocomotionGate locomotionGate)
	{
		_locomotionGate = locomotionGate;
	}
}
