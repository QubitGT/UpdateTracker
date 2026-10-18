using System;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace UnityEngine.XR.Interaction.Toolkit.Filtering;

[Serializable]
public abstract class XRTargetEvaluator : IDisposable
{
	[HideInInspector]
	[SerializeField]
	private XRTargetFilter m_Filter;

	[HideInInspector]
	[SerializeField]
	[XRTargetEvaluatorEnabled]
	private bool m_Enabled = true;

	[Tooltip("The weight curve of this evaluator. Use this curve to configure the returned score.\n\nThe x-axis is the normalized score (calculated in CalculateNormalizedScore) and the y-axis is the returned score of this evaluator.")]
	[SerializeField]
	private AnimationCurve m_Weight;

	private bool m_IsAwake;

	private bool m_IsEnabled;

	private bool m_IsRegistered;

	public XRTargetFilter filter => m_Filter;

	public bool enabled
	{
		get
		{
			return m_Enabled;
		}
		set
		{
			if (m_Enabled == value || disposed)
			{
				return;
			}
			if (m_Filter.isProcessing && !value)
			{
				throw new InvalidOperationException("Cannot disable an evaluator " + GetType().Name + " while its filter " + m_Filter.name + " is processing.");
			}
			m_Enabled = value;
			if (m_IsAwake && m_Filter.isActiveAndEnabled)
			{
				if (value)
				{
					EnableInternal();
				}
				else
				{
					DisableInternal();
				}
			}
		}
	}

	public AnimationCurve weight
	{
		get
		{
			return m_Weight;
		}
		set
		{
			m_Weight = value;
		}
	}

	public bool disposed => m_Filter == null;

	internal static bool IsInstanceType(Type evaluatorType)
	{
		if (evaluatorType != null && !evaluatorType.IsInterface && !evaluatorType.IsAbstract && !evaluatorType.IsGenericType)
		{
			return typeof(XRTargetEvaluator).IsAssignableFrom(evaluatorType);
		}
		return false;
	}

	internal static XRTargetEvaluator CreateInstance(Type evaluatorType, XRTargetFilter filter)
	{
		if (!IsInstanceType(evaluatorType) || !(Activator.CreateInstance(evaluatorType) is XRTargetEvaluator xRTargetEvaluator))
		{
			return null;
		}
		xRTargetEvaluator.m_Filter = filter;
		xRTargetEvaluator.m_Weight = AnimationCurve.Linear(0f, 0f, 1f, 1f);
		return xRTargetEvaluator;
	}

	private void RegisterHandlers()
	{
		if (!m_IsRegistered && !disposed)
		{
			m_IsRegistered = true;
			m_Filter.RegisterEvaluatorHandlers(this);
		}
	}

	private void UnregisterHandlers()
	{
		if (m_IsRegistered && !disposed)
		{
			m_IsRegistered = false;
			m_Filter.UnregisterEvaluatorHandlers(this);
		}
	}

	internal void AwakeInternal()
	{
		if (!m_IsAwake && !disposed)
		{
			m_IsAwake = true;
			Awake();
			RegisterHandlers();
		}
	}

	internal void DisposeInternal()
	{
		if (m_IsAwake)
		{
			m_IsAwake = false;
			UnregisterHandlers();
			OnDispose();
			m_Filter = null;
		}
	}

	internal void EnableInternal()
	{
		if (!m_IsEnabled && !disposed)
		{
			m_IsEnabled = true;
			OnEnable();
		}
	}

	internal void DisableInternal()
	{
		if (m_IsEnabled)
		{
			m_IsEnabled = false;
			OnDisable();
		}
	}

	protected virtual void Awake()
	{
	}

	protected virtual void OnDispose()
	{
	}

	protected virtual void OnEnable()
	{
	}

	protected virtual void OnDisable()
	{
	}

	public virtual void Reset()
	{
	}

	public float GetWeightedScore(IXRInteractor interactor, IXRInteractable target)
	{
		return m_Weight.Evaluate(CalculateNormalizedScore(interactor, target));
	}

	protected abstract float CalculateNormalizedScore(IXRInteractor interactor, IXRInteractable target);

	public void Dispose()
	{
		if (m_Filter != null)
		{
			m_Filter.RemoveEvaluator(this);
		}
	}
}
