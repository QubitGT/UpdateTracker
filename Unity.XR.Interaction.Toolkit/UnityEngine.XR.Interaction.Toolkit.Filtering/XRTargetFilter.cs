using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Utilities.Pooling;

namespace UnityEngine.XR.Interaction.Toolkit.Filtering;

[AddComponentMenu("XR/XR Target Filter", 11)]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Filtering.XRTargetFilter.html")]
public sealed class XRTargetFilter : XRBaseTargetFilter, IEnumerable<XRTargetEvaluator>, IEnumerable
{
	private static readonly LinkedPool<List<XRTargetEvaluator>> s_EvaluatorListPool = new LinkedPool<List<XRTargetEvaluator>>(() => new List<XRTargetEvaluator>(), null, delegate(List<XRTargetEvaluator> list)
	{
		list.Clear();
	}, null, collectionCheck: false);

	private static readonly Dictionary<IXRInteractable, float> s_InteractableFinalScoreMap = new Dictionary<IXRInteractable, float>();

	private static readonly Comparison<IXRInteractable> s_InteractableScoreComparison = InteractableScoreDescendingComparison;

	private List<IXRInteractor> m_LinkedInteractors = new List<IXRInteractor>();

	[SerializeReference]
	private List<XRTargetEvaluator> m_Evaluators = new List<XRTargetEvaluator>();

	private bool m_IsAwake;

	internal List<IXRInteractor> linkedInteractors => m_LinkedInteractors;

	internal List<XRTargetEvaluator> evaluators => m_Evaluators;

	public int evaluatorCount => m_Evaluators.Count;

	internal bool isProcessing { get; private set; }

	public override bool canProcess
	{
		get
		{
			if (!isProcessing)
			{
				return base.canProcess;
			}
			return false;
		}
	}

	public event Action<IXRInteractor> interactorLinked;

	public event Action<IXRInteractor> interactorUnlinked;

	private static int InteractableScoreDescendingComparison(IXRInteractable x, IXRInteractable y)
	{
		float num = s_InteractableFinalScoreMap[x];
		float num2 = s_InteractableFinalScoreMap[y];
		if (num < num2)
		{
			return 1;
		}
		if (num > num2)
		{
			return -1;
		}
		return 0;
	}

	private void Awake()
	{
		m_IsAwake = true;
		List<XRTargetEvaluator> v;
		using (s_EvaluatorListPool.Get(out v))
		{
			GetEvaluators(v);
			for (int i = 0; i < v.Count; i++)
			{
				if (!m_IsAwake)
				{
					break;
				}
				v[i].AwakeInternal();
			}
		}
	}

	private void OnEnable()
	{
		List<XRTargetEvaluator> v;
		using (s_EvaluatorListPool.Get(out v))
		{
			GetEvaluators(v);
			for (int i = 0; i < v.Count; i++)
			{
				if (!base.isActiveAndEnabled)
				{
					break;
				}
				XRTargetEvaluator xRTargetEvaluator = v[i];
				if (xRTargetEvaluator.enabled)
				{
					xRTargetEvaluator.EnableInternal();
				}
			}
		}
	}

	private void OnDisable()
	{
		List<XRTargetEvaluator> v;
		using (s_EvaluatorListPool.Get(out v))
		{
			GetEnabledEvaluators(v);
			for (int i = 0; i < v.Count; i++)
			{
				if (base.isActiveAndEnabled)
				{
					break;
				}
				v[i].DisableInternal();
			}
		}
	}

	private void OnDestroy()
	{
		m_IsAwake = false;
		List<XRTargetEvaluator> v;
		using (s_EvaluatorListPool.Get(out v))
		{
			GetEvaluators(v);
			foreach (XRTargetEvaluator item in v)
			{
				item.DisposeInternal();
			}
		}
	}

	private void Reset()
	{
		AddEvaluator<XRDistanceEvaluator>().Reset();
	}

	internal void RegisterEvaluatorHandlers(XRTargetEvaluator evaluator)
	{
		if (!(evaluator is IXRTargetEvaluatorLinkable iXRTargetEvaluatorLinkable))
		{
			return;
		}
		interactorLinked += iXRTargetEvaluatorLinkable.OnLink;
		interactorUnlinked += iXRTargetEvaluatorLinkable.OnUnlink;
		foreach (IXRInteractor linkedInteractor in m_LinkedInteractors)
		{
			iXRTargetEvaluatorLinkable.OnLink(linkedInteractor);
		}
	}

	internal void UnregisterEvaluatorHandlers(XRTargetEvaluator evaluator)
	{
		if (!(evaluator is IXRTargetEvaluatorLinkable iXRTargetEvaluatorLinkable))
		{
			return;
		}
		interactorLinked -= iXRTargetEvaluatorLinkable.OnLink;
		interactorUnlinked -= iXRTargetEvaluatorLinkable.OnUnlink;
		foreach (IXRInteractor linkedInteractor in m_LinkedInteractors)
		{
			iXRTargetEvaluatorLinkable.OnUnlink(linkedInteractor);
		}
	}

	public void GetLinkedInteractors(List<IXRInteractor> results)
	{
		if (results == null)
		{
			throw new ArgumentNullException("results");
		}
		results.Clear();
		results.AddRange(m_LinkedInteractors);
	}

	public void GetEvaluators(List<XRTargetEvaluator> results)
	{
		if (results == null)
		{
			throw new ArgumentNullException("results");
		}
		results.Clear();
		results.AddRange(m_Evaluators);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return ((IEnumerable)m_Evaluators).GetEnumerator();
	}

	public IEnumerator<XRTargetEvaluator> GetEnumerator()
	{
		return m_Evaluators.GetEnumerator();
	}

	public XRTargetEvaluator GetEvaluatorAt(int index)
	{
		return m_Evaluators[index];
	}

	public XRTargetEvaluator GetEvaluator(Type type)
	{
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		foreach (XRTargetEvaluator evaluator in m_Evaluators)
		{
			if (type.IsInstanceOfType(evaluator))
			{
				return evaluator;
			}
		}
		return null;
	}

	public T GetEvaluator<T>()
	{
		return (T)(object)GetEvaluator(typeof(T));
	}

	public void GetEnabledEvaluators(List<XRTargetEvaluator> results)
	{
		if (results == null)
		{
			throw new ArgumentNullException("results");
		}
		results.Clear();
		foreach (XRTargetEvaluator evaluator in m_Evaluators)
		{
			if (evaluator.enabled)
			{
				results.Add(evaluator);
			}
		}
	}

	public XRTargetEvaluator AddEvaluator(Type evaluatorType)
	{
		if (evaluatorType == null)
		{
			throw new ArgumentNullException("evaluatorType");
		}
		XRTargetEvaluator xRTargetEvaluator = XRTargetEvaluator.CreateInstance(evaluatorType, this);
		if (xRTargetEvaluator == null)
		{
			return null;
		}
		m_Evaluators.Add(xRTargetEvaluator);
		if (m_IsAwake)
		{
			xRTargetEvaluator.AwakeInternal();
			if (base.isActiveAndEnabled && xRTargetEvaluator.enabled)
			{
				xRTargetEvaluator.EnableInternal();
			}
		}
		return xRTargetEvaluator;
	}

	public T AddEvaluator<T>() where T : XRTargetEvaluator
	{
		return AddEvaluator(typeof(T)) as T;
	}

	public void RemoveEvaluatorAt(int index)
	{
		if (isProcessing)
		{
			throw new InvalidOperationException("Cannot remove evaluators while a filter " + base.name + " is processing.");
		}
		XRTargetEvaluator xRTargetEvaluator = m_Evaluators[index];
		if (m_IsAwake && xRTargetEvaluator != null)
		{
			if (base.isActiveAndEnabled && xRTargetEvaluator.enabled)
			{
				xRTargetEvaluator.DisableInternal();
			}
			xRTargetEvaluator.DisposeInternal();
		}
		m_Evaluators.RemoveAt(index);
	}

	public void RemoveEvaluator(XRTargetEvaluator evaluator)
	{
		if (isProcessing)
		{
			throw new InvalidOperationException("Cannot remove evaluators while a filter " + base.name + " is processing.");
		}
		int num = m_Evaluators.IndexOf(evaluator);
		if (num >= 0)
		{
			RemoveEvaluatorAt(num);
		}
	}

	public void MoveEvaluatorTo(XRTargetEvaluator evaluator, int newIndex)
	{
		int num = m_Evaluators.IndexOf(evaluator);
		if (num >= 0 && num != newIndex)
		{
			m_Evaluators.RemoveAt(num);
			m_Evaluators.Insert(newIndex, evaluator);
		}
	}

	public override void Link(IXRInteractor interactor)
	{
		if (interactor == null)
		{
			throw new ArgumentNullException("interactor");
		}
		if (!m_LinkedInteractors.Contains(interactor))
		{
			m_LinkedInteractors.Add(interactor);
			this.interactorLinked?.Invoke(interactor);
		}
	}

	public override void Unlink(IXRInteractor interactor)
	{
		if (interactor == null)
		{
			throw new ArgumentNullException("interactor");
		}
		if (isProcessing)
		{
			throw new InvalidOperationException($"Cannot unlink an interactor {interactor} while the filter {base.name} is processing.");
		}
		if (m_LinkedInteractors.Remove(interactor))
		{
			this.interactorUnlinked?.Invoke(interactor);
		}
	}

	public override void Process(IXRInteractor interactor, List<IXRInteractable> targets, List<IXRInteractable> results)
	{
		if (isProcessing)
		{
			throw new InvalidOperationException("Process for filter " + base.name + " is already running, cannot start a new one.");
		}
		isProcessing = true;
		try
		{
			results.Clear();
			List<XRTargetEvaluator> v;
			using (s_EvaluatorListPool.Get(out v))
			{
				GetEnabledEvaluators(v);
				foreach (IXRInteractable target in targets)
				{
					float num = 1f;
					foreach (XRTargetEvaluator item in v)
					{
						float weightedScore = item.GetWeightedScore(interactor, target);
						num *= weightedScore;
						if (num <= 0f)
						{
							break;
						}
					}
					if (num >= 0f)
					{
						results.Add(target);
						s_InteractableFinalScoreMap[target] = num;
					}
				}
			}
			results.Sort(s_InteractableScoreComparison);
		}
		finally
		{
			isProcessing = false;
			s_InteractableFinalScoreMap.Clear();
		}
	}
}
