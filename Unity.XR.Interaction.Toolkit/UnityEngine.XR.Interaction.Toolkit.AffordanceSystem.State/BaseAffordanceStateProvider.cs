using System;
using System.Collections.Generic;
using Unity.Jobs;
using Unity.XR.CoreUtils.Bindings;
using Unity.XR.CoreUtils.Bindings.Variables;
using Unity.XR.CoreUtils.Collections;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver;

namespace UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.State;

[Obsolete("The Affordance System namespace and all associated classes have been deprecated. The existing affordance system will be moved, replaced and updated with a new interaction feedback system in a future version of XRI.")]
public abstract class BaseAffordanceStateProvider : MonoBehaviour
{
	[SerializeField]
	[Range(0f, 5f)]
	[Tooltip("Duration of transition in seconds. 0 means no smoothing.")]
	private float m_TransitionDuration = 0.125f;

	private readonly BindableVariable<AffordanceStateData> m_AffordanceStateData = new BindableVariable<AffordanceStateData>();

	private AffordanceStateData m_AffordanceStateDataBeforeSet;

	private AffordanceStateData m_PreviousAffordanceStateData;

	private readonly HashSetList<IAsyncAffordanceStateReceiver> m_AsyncAffordanceReceivers = new HashSetList<IAsyncAffordanceStateReceiver>();

	private readonly HashSetList<ISynchronousAffordanceStateReceiver> m_SynchronousAffordanceReceivers = new HashSetList<ISynchronousAffordanceStateReceiver>();

	private readonly List<JobHandle> m_ScheduledJobs = new List<JobHandle>();

	private readonly BindingsGroup m_BindingsGroup = new BindingsGroup();

	private float m_TimeSinceLastStateUpdate;

	private bool m_IsFirstFrame = true;

	private bool m_CompletingTweens;

	private float m_InterpolationSpeed = 8f;

	private float m_MaxTransitionDuration = 5f;

	public float transitionDuration
	{
		get
		{
			return m_TransitionDuration;
		}
		set
		{
			m_TransitionDuration = value;
			RefreshTransitionDuration();
		}
	}

	public bool isCurrentlyTransitioning
	{
		get
		{
			if (m_CompletingTweens)
			{
				return m_ScheduledJobs.Count > 0;
			}
			return true;
		}
	}

	public IReadOnlyBindableVariable<AffordanceStateData> currentAffordanceStateData => m_AffordanceStateData;

	protected virtual void OnValidate()
	{
		RefreshTransitionDuration();
	}

	protected virtual void OnEnable()
	{
		RefreshTransitionDuration();
		BindToProviders();
	}

	protected virtual void OnDisable()
	{
		ClearBindings();
	}

	protected virtual void Update()
	{
		if (m_IsFirstFrame)
		{
			OnAffordanceStateUpdated(m_AffordanceStateData.Value);
			DoTween(1f);
			m_IsFirstFrame = false;
		}
		else
		{
			DoTween((m_InterpolationSpeed > 0f) ? (Time.deltaTime * m_InterpolationSpeed) : 1f);
		}
	}

	protected virtual void BindToProviders()
	{
		ClearBindings();
		m_IsFirstFrame = true;
		AddBinding(m_AffordanceStateData.SubscribeAndUpdate(OnAffordanceStateUpdated));
	}

	protected virtual void ClearBindings()
	{
		m_BindingsGroup.Clear();
	}

	protected void AddBinding(IEventBinding binding)
	{
		m_BindingsGroup.AddBinding(binding);
	}

	public void UpdateAffordanceState(AffordanceStateData newAffordanceStateData)
	{
		m_AffordanceStateDataBeforeSet = m_AffordanceStateData.Value;
		m_AffordanceStateData.Value = newAffordanceStateData;
	}

	private void OnAffordanceStateUpdated(AffordanceStateData newAffordanceStateData)
	{
		m_PreviousAffordanceStateData = m_AffordanceStateDataBeforeSet;
		for (int i = 0; i < m_AsyncAffordanceReceivers.Count; i++)
		{
			m_AsyncAffordanceReceivers[i].OnAffordanceStateUpdated(m_PreviousAffordanceStateData, newAffordanceStateData);
		}
		for (int j = 0; j < m_SynchronousAffordanceReceivers.Count; j++)
		{
			m_SynchronousAffordanceReceivers[j].OnAffordanceStateUpdated(m_PreviousAffordanceStateData, newAffordanceStateData);
		}
		m_TimeSinceLastStateUpdate = 0f;
		m_CompletingTweens = false;
	}

	public bool RegisterAffordanceReceiver(IAffordanceStateReceiver receiver)
	{
		if (receiver is IAsyncAffordanceStateReceiver receiver2)
		{
			return RegisterAffordanceReceiver(receiver2);
		}
		if (receiver is ISynchronousAffordanceStateReceiver receiver3)
		{
			return RegisterAffordanceReceiver(receiver3);
		}
		if (receiver != null)
		{
			Debug.LogError("Unhandled type of IAffordanceStateReceiver: " + receiver.GetType().Name, this);
		}
		return false;
	}

	private bool RegisterAffordanceReceiver(IAsyncAffordanceStateReceiver receiver)
	{
		return m_AsyncAffordanceReceivers.Add(receiver);
	}

	private bool RegisterAffordanceReceiver(ISynchronousAffordanceStateReceiver receiver)
	{
		return m_SynchronousAffordanceReceivers.Add(receiver);
	}

	public bool UnregisterAffordanceReceiver(IAffordanceStateReceiver receiver)
	{
		if (receiver is IAsyncAffordanceStateReceiver receiver2)
		{
			return UnregisterAffordanceReceiver(receiver2);
		}
		if (receiver is ISynchronousAffordanceStateReceiver receiver3)
		{
			return UnregisterAffordanceReceiver(receiver3);
		}
		if (receiver != null)
		{
			Debug.LogError("Unhandled type of IAffordanceStateReceiver: " + receiver.GetType().Name, this);
		}
		return false;
	}

	private bool UnregisterAffordanceReceiver(IAsyncAffordanceStateReceiver receiver)
	{
		CompleteJobs();
		return m_AsyncAffordanceReceivers.Remove(receiver);
	}

	private bool UnregisterAffordanceReceiver(ISynchronousAffordanceStateReceiver receiver)
	{
		return m_SynchronousAffordanceReceivers.Remove(receiver);
	}

	private bool CompleteJobs()
	{
		for (int i = 0; i < m_ScheduledJobs.Count; i++)
		{
			m_ScheduledJobs[i].Complete();
		}
		bool result = m_ScheduledJobs.Count > 0;
		m_ScheduledJobs.Clear();
		return result;
	}

	private void DoTween(float tweenTarget)
	{
		if (CompleteJobs())
		{
			for (int i = 0; i < m_AsyncAffordanceReceivers.Count; i++)
			{
				m_AsyncAffordanceReceivers[i].UpdateStateFromCompletedJob();
			}
		}
		float num = tweenTarget;
		if (m_TimeSinceLastStateUpdate > m_MaxTransitionDuration || num > 0.99f)
		{
			if (m_CompletingTweens)
			{
				return;
			}
			num = 1f;
			m_CompletingTweens = true;
		}
		for (int j = 0; j < m_AsyncAffordanceReceivers.Count; j++)
		{
			m_ScheduledJobs.Add(m_AsyncAffordanceReceivers[j].HandleTween(num));
		}
		for (int k = 0; k < m_SynchronousAffordanceReceivers.Count; k++)
		{
			m_SynchronousAffordanceReceivers[k].HandleTween(num);
		}
		m_TimeSinceLastStateUpdate += Time.deltaTime;
	}

	private void RefreshTransitionDuration()
	{
		m_InterpolationSpeed = ((m_TransitionDuration > 0f) ? (1f / m_TransitionDuration) : 0f);
		m_MaxTransitionDuration = m_TransitionDuration * 4f;
	}
}
