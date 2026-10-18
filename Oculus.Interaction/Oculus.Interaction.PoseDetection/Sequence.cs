using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Oculus.Interaction.PoseDetection.Debug;
using UnityEngine;

namespace Oculus.Interaction.PoseDetection;

public class Sequence : MonoBehaviour, IActiveState, ITimeConsumer
{
	[Serializable]
	public class ActivationStep
	{
		[Tooltip("The IActiveState that is used to determine if the conditions of this step are fulfilled.")]
		[SerializeField]
		[Interface(typeof(IActiveState), new Type[] { })]
		private UnityEngine.Object _activeState;

		[SerializeField]
		[Tooltip("This step must be consistently active for this amount of time before continuing to the next step.")]
		private float _minActiveTime;

		[SerializeField]
		[Tooltip("Maximum time that can be spent waiting for this step to complete, before the whole sequence is abandoned. This value must be greater than minActiveTime, or zero. This value is ignored if zero, and for the first step in the list.")]
		private float _maxStepTime;

		public IActiveState ActiveState { get; private set; }

		public float MinActiveTime => _minActiveTime;

		public float MaxStepTime => _maxStepTime;

		public ActivationStep()
		{
		}

		public ActivationStep(IActiveState activeState, float minActiveTime, float maxStepTime)
		{
			ActiveState = activeState;
			_minActiveTime = minActiveTime;
			_maxStepTime = maxStepTime;
		}

		public void Start()
		{
			if (ActiveState == null)
			{
				ActiveState = _activeState as IActiveState;
			}
		}
	}

	private class DebugModel : ActiveStateModel<Sequence>
	{
		private IEnumerator GetChildrenCoroutine(Sequence sequence, TaskCompletionSource<IEnumerable<IActiveState>> tcs)
		{
			while (sequence._stepsToActivate.Any((ActivationStep s) => s.ActiveState == null))
			{
				yield return null;
			}
			List<IActiveState> list = new List<IActiveState>();
			list.AddRange(sequence._stepsToActivate.Select((ActivationStep step) => step.ActiveState));
			list.Add(sequence.RemainActiveWhile);
			tcs.SetResult(list.Where((IActiveState c) => c != null));
		}

		protected override Task<IEnumerable<IActiveState>> GetChildrenAsync(Sequence activeState)
		{
			TaskCompletionSource<IEnumerable<IActiveState>> taskCompletionSource = new TaskCompletionSource<IEnumerable<IActiveState>>();
			if (activeState.isActiveAndEnabled)
			{
				activeState.StartCoroutine(GetChildrenCoroutine(activeState, taskCompletionSource));
			}
			else
			{
				taskCompletionSource.SetResult(Enumerable.Empty<IActiveState>());
			}
			return taskCompletionSource.Task;
		}
	}

	[Tooltip("The sequence will step through these ActivationSteps one at a time, advancing when each step becomes Active. Once all steps are active, the sequence itself will become Active.")]
	[SerializeField]
	[Optional]
	private ActivationStep[] _stepsToActivate;

	[Tooltip("Once the sequence is active, it will remain active as long as this IActiveState is Active.")]
	[SerializeField]
	[Optional]
	[Interface(typeof(IActiveState), new Type[] { })]
	private UnityEngine.Object _remainActiveWhile;

	[Tooltip("Sequence will not become inactive until RemainActiveWhile has been inactive for at least this many seconds.")]
	[SerializeField]
	[Optional]
	private float _remainActiveCooldown;

	private Func<float> _timeProvider = () => Time.time;

	private float _currentStepActivatedTime;

	private float _stepFailedTime;

	private bool _currentStepWasActive;

	private float _cooldownExceededTime;

	private bool _wasRemainActive;

	private IActiveState RemainActiveWhile { get; set; }

	public int CurrentActivationStep { get; private set; }

	public bool Active { get; private set; }

	public void SetTimeProvider(Func<float> timeProvider)
	{
		_timeProvider = timeProvider;
	}

	protected virtual void Awake()
	{
		RemainActiveWhile = _remainActiveWhile as IActiveState;
		ResetState();
	}

	protected virtual void Start()
	{
		if (_stepsToActivate == null)
		{
			_stepsToActivate = Array.Empty<ActivationStep>();
		}
		ActivationStep[] stepsToActivate = _stepsToActivate;
		for (int i = 0; i < stepsToActivate.Length; i++)
		{
			stepsToActivate[i].Start();
		}
	}

	protected virtual void Update()
	{
		float num = _timeProvider();
		if (Active)
		{
			bool flag = RemainActiveWhile != null && RemainActiveWhile.Active;
			if (!flag)
			{
				if (_wasRemainActive)
				{
					_cooldownExceededTime = num + _remainActiveCooldown;
				}
				if (_cooldownExceededTime <= num)
				{
					Active = false;
				}
			}
			_wasRemainActive = flag;
			if (!Active)
			{
				ResetState();
			}
		}
		else if (CurrentActivationStep < _stepsToActivate.Length)
		{
			ActivationStep activationStep = _stepsToActivate[CurrentActivationStep];
			if (num > _stepFailedTime && CurrentActivationStep > 0 && activationStep.MaxStepTime > 0f)
			{
				ResetState();
			}
			bool active = activationStep.ActiveState.Active;
			if (active && !_currentStepWasActive)
			{
				_currentStepActivatedTime = num + activationStep.MinActiveTime;
			}
			if (num >= _currentStepActivatedTime && _currentStepWasActive)
			{
				int num2 = CurrentActivationStep + 1;
				bool num3 = !active;
				bool flag2 = num2 == _stepsToActivate.Length || _stepsToActivate[num2].ActiveState.Active;
				if (num3 || flag2)
				{
					EnterNextStep(num);
				}
			}
			_currentStepWasActive = active;
		}
		else if (RemainActiveWhile != null)
		{
			Active = RemainActiveWhile.Active;
		}
	}

	private void EnterNextStep(float time)
	{
		CurrentActivationStep++;
		_currentStepWasActive = false;
		if (CurrentActivationStep < _stepsToActivate.Length)
		{
			ActivationStep activationStep = _stepsToActivate[CurrentActivationStep];
			_stepFailedTime = time + activationStep.MaxStepTime;
		}
		else
		{
			Active = true;
			_cooldownExceededTime = time + _remainActiveCooldown;
			NativeMethods.isdk_NativeComponent_Activate(6009334026819888500uL);
		}
	}

	private void ResetState()
	{
		CurrentActivationStep = 0;
		_currentStepWasActive = false;
		_currentStepActivatedTime = 0f;
	}

	static Sequence()
	{
	}

	public void InjectOptionalStepsToActivate(ActivationStep[] stepsToActivate)
	{
		_stepsToActivate = stepsToActivate;
	}

	public void InjectOptionalRemainActiveWhile(IActiveState activeState)
	{
		_remainActiveWhile = activeState as UnityEngine.Object;
		RemainActiveWhile = activeState;
	}

	[Obsolete("Use SetTimeProvider()")]
	public void InjectOptionalTimeProvider(Func<float> timeProvider)
	{
		_timeProvider = timeProvider;
	}
}
