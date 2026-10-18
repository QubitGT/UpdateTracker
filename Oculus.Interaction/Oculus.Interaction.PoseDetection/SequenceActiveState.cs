using System.Collections.Generic;
using System.Threading.Tasks;
using Oculus.Interaction.PoseDetection.Debug;
using UnityEngine;

namespace Oculus.Interaction.PoseDetection;

public class SequenceActiveState : MonoBehaviour, IActiveState
{
	private class DebugModel : ActiveStateModel<SequenceActiveState>
	{
		protected override Task<IEnumerable<IActiveState>> GetChildrenAsync(SequenceActiveState activeState)
		{
			return Task.FromResult((IEnumerable<IActiveState>)new Sequence[1] { activeState._sequence });
		}
	}

	[Tooltip("The Sequence that will drive this component.")]
	[SerializeField]
	private Sequence _sequence;

	[Tooltip("If true, this ActiveState will become Active as soon as the first sequence step becomes Active.")]
	[SerializeField]
	private bool _activateIfStepsStarted;

	[Tooltip("If true, this ActiveState will be active when the supplied Sequence is Active.")]
	[SerializeField]
	private bool _activateIfStepsComplete = true;

	public bool Active
	{
		get
		{
			if (!_activateIfStepsStarted || _sequence.CurrentActivationStep <= 0 || _sequence.Active)
			{
				if (_activateIfStepsComplete)
				{
					return _sequence.Active;
				}
				return false;
			}
			return true;
		}
	}

	protected virtual void Start()
	{
	}

	static SequenceActiveState()
	{
	}

	public void InjectAllSequenceActiveState(Sequence sequence, bool activateIfStepsStarted, bool activateIfStepsComplete)
	{
		InjectSequence(sequence);
		InjectActivateIfStepsStarted(activateIfStepsStarted);
		InjectActivateIfStepsComplete(activateIfStepsComplete);
	}

	public void InjectSequence(Sequence sequence)
	{
		_sequence = sequence;
	}

	public void InjectActivateIfStepsStarted(bool activateIfStepsStarted)
	{
		_activateIfStepsStarted = activateIfStepsStarted;
	}

	public void InjectActivateIfStepsComplete(bool activateIfStepsComplete)
	{
		_activateIfStepsComplete = activateIfStepsComplete;
	}
}
