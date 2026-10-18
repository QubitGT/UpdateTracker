using System;
using System.Collections.Generic;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.XR.Interaction.Toolkit.Utilities;

namespace UnityEngine.XR.Interaction.Toolkit.Locomotion;

[DefaultExecutionOrder(-210)]
[MovedFrom("UnityEngine.XR.Interaction.Toolkit")]
public abstract class LocomotionProvider : MonoBehaviour
{
	[SerializeField]
	[Tooltip("The behavior that this provider communicates with for access to the mediator's XR Body Transformer. If one is not provided, this provider will attempt to locate one during its Awake call.")]
	private LocomotionMediator m_Mediator;

	[SerializeField]
	[Tooltip("The queue order of this provider's transformations of the XR Origin. The lower the value, the earlier the transformations are applied.")]
	private int m_TransformationPriority;

	private XRBodyTransformer m_ActiveBodyTransformer;

	private XRBodyTransformer m_SubscribedTransformer;

	private bool m_AnyTransformationsQueued;

	[Tooltip("(Deprecated) The Locomotion System that this locomotion provider communicates with for exclusive access to an XR Origin. If one is not provided, the behavior will attempt to locate one during its Awake call.")]
	[Obsolete("LocomotionSystem is deprecated in XRI 3.0.0 and will be removed in a future release. Use mediator instead.", false)]
	private LocomotionSystem m_System;

	public LocomotionMediator mediator
	{
		get
		{
			return m_Mediator;
		}
		set
		{
			m_Mediator = value;
		}
	}

	public int transformationPriority
	{
		get
		{
			return m_TransformationPriority;
		}
		set
		{
			m_TransformationPriority = value;
		}
	}

	public LocomotionState locomotionState
	{
		get
		{
			if (!(m_Mediator != null))
			{
				return LocomotionState.Idle;
			}
			return m_Mediator.GetProviderLocomotionState(this);
		}
	}

	public bool isLocomotionActive => locomotionState.IsActive();

	public virtual bool canStartMoving => true;

	internal static List<LocomotionProvider> locomotionProviders { get; } = new List<LocomotionProvider>();


	[Obsolete("LocomotionSystem is deprecated in XRI 3.0.0 and will be removed in a future release. Use mediator instead.", false)]
	public LocomotionSystem system
	{
		get
		{
			return m_System;
		}
		set
		{
			m_System = value;
		}
	}

	[Obsolete("locomotionPhase is deprecated in XRI 3.0.0 and will be removed in a future release. Use locomotionState instead.", false)]
	public LocomotionPhase locomotionPhase { get; protected set; }

	public event Action<LocomotionProvider, LocomotionState> locomotionStateChanged;

	public event Action<LocomotionProvider> locomotionStarted;

	public event Action<LocomotionProvider> locomotionEnded;

	public event Action<LocomotionProvider> beforeStepLocomotion;

	public event Action<LocomotionProvider> afterStepLocomotion;

	internal static event Action<LocomotionProvider> locomotionProvidersChanged;

	[Obsolete("startLocomotion has been deprecated in XRI 3.0.0. Use beginLocomotion instead. (UnityUpgradable) -> beginLocomotion", true)]
	public event Action<LocomotionSystem> startLocomotion;

	[Obsolete("beginLocomotion is deprecated in XRI 3.0.0 and will be removed in a future release. Use locomotionStarted instead.", false)]
	public event Action<LocomotionSystem> beginLocomotion;

	[Obsolete("endLocomotion is deprecated in XRI 3.0.0 and will be removed in a future release. Use locomotionEnded instead.", false)]
	public event Action<LocomotionSystem> endLocomotion;

	protected virtual void Awake()
	{
		if (m_System == null)
		{
			m_System = GetComponentInParent<LocomotionSystem>();
			if (m_System == null)
			{
				ComponentLocatorUtility<LocomotionSystem>.TryFindComponent(out m_System);
			}
		}
		if (m_Mediator == null)
		{
			m_Mediator = GetComponentInParent<LocomotionMediator>();
			if (m_Mediator == null)
			{
				ComponentLocatorUtility<LocomotionMediator>.TryFindComponent(out m_Mediator);
			}
		}
		if (m_Mediator == null && m_System == null)
		{
			Debug.LogError("Locomotion Provider requires a Locomotion Mediator or Locomotion System (legacy) in the scene.", this);
			base.enabled = false;
		}
		locomotionProviders.Add(this);
		LocomotionProvider.locomotionProvidersChanged?.Invoke(this);
	}

	protected bool TryPrepareLocomotion()
	{
		if (m_Mediator != null)
		{
			return m_Mediator.TryPrepareLocomotion(this);
		}
		return false;
	}

	protected bool TryStartLocomotionImmediately()
	{
		if (m_Mediator != null)
		{
			return m_Mediator.TryStartLocomotion(this);
		}
		return false;
	}

	protected bool TryEndLocomotion()
	{
		if (m_Mediator != null)
		{
			return m_Mediator.TryEndLocomotion(this);
		}
		return false;
	}

	protected virtual void OnLocomotionStarting()
	{
	}

	protected virtual void OnLocomotionEnding()
	{
	}

	protected virtual void OnLocomotionStateChanging(LocomotionState state)
	{
	}

	internal void OnLocomotionStateChanging(LocomotionState oldState, LocomotionState state, XRBodyTransformer transformer)
	{
		switch (state)
		{
		case LocomotionState.Moving:
			if (oldState == LocomotionState.Ended && m_AnyTransformationsQueued)
			{
				Debug.LogWarning("LocomotionProvider (" + GetType().Name + ") changed state from LocomotionState.Ended to LocomotionState.Moving before its queued transformations have been applied. The deferred OnLocomotionEnding method call and locomotionEnded event will not be invoked.", this);
			}
			m_ActiveBodyTransformer = transformer;
			Subscribe(transformer);
			break;
		case LocomotionState.Ended:
			m_ActiveBodyTransformer = null;
			break;
		}
		OnLocomotionStateChanging(state);
		this.locomotionStateChanged?.Invoke(this, state);
		switch (state)
		{
		case LocomotionState.Moving:
			OnLocomotionStarting();
			this.locomotionStarted?.Invoke(this);
			break;
		case LocomotionState.Ended:
			if (!m_AnyTransformationsQueued)
			{
				Unsubscribe();
				OnLocomotionEnding();
				this.locomotionEnded?.Invoke(this);
			}
			break;
		}
	}

	protected bool TryQueueTransformation(IXRBodyTransformation bodyTransformation)
	{
		if (!CanQueueTransformation())
		{
			return false;
		}
		m_ActiveBodyTransformer.QueueTransformation(bodyTransformation, m_TransformationPriority);
		m_AnyTransformationsQueued = true;
		return true;
	}

	protected bool TryQueueTransformation(IXRBodyTransformation bodyTransformation, int priority)
	{
		if (!CanQueueTransformation())
		{
			return false;
		}
		m_ActiveBodyTransformer.QueueTransformation(bodyTransformation, priority);
		m_AnyTransformationsQueued = true;
		return true;
	}

	private bool CanQueueTransformation()
	{
		if (m_ActiveBodyTransformer == null)
		{
			if (locomotionState == LocomotionState.Moving)
			{
				Debug.LogError("Cannot queue transformation because reference to active XR Body Transformer is null, even though Locomotion Provider is in Moving state. This should not happen.", this);
			}
			return false;
		}
		return true;
	}

	private void Subscribe(XRBodyTransformer transformer)
	{
		if (!(m_SubscribedTransformer == transformer))
		{
			Unsubscribe();
			transformer.beforeApplyTransformations += OnBeforeApplyTransformations;
			transformer.afterApplyTransformations += OnAfterApplyTransformations;
			m_SubscribedTransformer = transformer;
		}
	}

	private void Unsubscribe()
	{
		if (!(m_SubscribedTransformer == null))
		{
			m_SubscribedTransformer.beforeApplyTransformations -= OnBeforeApplyTransformations;
			m_SubscribedTransformer.afterApplyTransformations -= OnAfterApplyTransformations;
			m_SubscribedTransformer = null;
		}
	}

	private void OnBeforeApplyTransformations(XRBodyTransformer transformer)
	{
		if (m_AnyTransformationsQueued)
		{
			this.beforeStepLocomotion?.Invoke(this);
		}
	}

	private void OnAfterApplyTransformations(ApplyBodyTransformationsEventArgs args)
	{
		if (m_AnyTransformationsQueued)
		{
			this.afterStepLocomotion?.Invoke(this);
		}
		m_AnyTransformationsQueued = false;
		if ((object)m_ActiveBodyTransformer == null)
		{
			Unsubscribe();
			OnLocomotionEnding();
			this.locomotionEnded?.Invoke(this);
		}
	}

	[Obsolete("CanBeginLocomotion is deprecated in XRI 3.0.0 and will be removed in a future release. Instead, query isLocomotionActive to check if locomotion can start.", false)]
	protected bool CanBeginLocomotion()
	{
		if (m_System == null)
		{
			return false;
		}
		return !m_System.busy;
	}

	[Obsolete("BeginLocomotion is deprecated in XRI 3.0.0 and will be removed in a future release. Instead, call TryPrepareLocomotion when locomotion start input occurs.", false)]
	protected bool BeginLocomotion()
	{
		if (m_System == null)
		{
			return false;
		}
		bool num = m_System.RequestExclusiveOperation(this) == RequestResult.Success;
		if (num)
		{
			Action<LocomotionSystem> action = this.beginLocomotion;
			if (action == null)
			{
				return num;
			}
			action(m_System);
		}
		return num;
	}

	[Obsolete("EndLocomotion is deprecated in XRI 3.0.0 and will be removed in a future release. Instead, call TryEndLocomotion when locomotion end input has completed.", false)]
	protected bool EndLocomotion()
	{
		if (m_System == null)
		{
			return false;
		}
		bool num = m_System.FinishExclusiveOperation(this) == RequestResult.Success;
		if (num)
		{
			Action<LocomotionSystem> action = this.endLocomotion;
			if (action == null)
			{
				return num;
			}
			action(m_System);
		}
		return num;
	}
}
