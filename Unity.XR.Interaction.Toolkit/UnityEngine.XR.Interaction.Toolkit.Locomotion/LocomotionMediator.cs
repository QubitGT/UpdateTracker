using System.Collections.Generic;
using Unity.XR.CoreUtils;

namespace UnityEngine.XR.Interaction.Toolkit.Locomotion;

[AddComponentMenu("XR/Locomotion/Locomotion Mediator", 11)]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Locomotion.LocomotionMediator.html")]
[RequireComponent(typeof(XRBodyTransformer))]
public class LocomotionMediator : MonoBehaviour
{
	private class LocomotionProviderData
	{
		public LocomotionState state;

		public int locomotionEndFrame;
	}

	private XRBodyTransformer m_XRBodyTransformer;

	private readonly Dictionary<LocomotionProvider, LocomotionProviderData> m_ProviderDataMap = new Dictionary<LocomotionProvider, LocomotionProviderData>();

	private static readonly List<LocomotionProvider> s_ProvidersToRemove = new List<LocomotionProvider>();

	public XROrigin xrOrigin
	{
		get
		{
			return m_XRBodyTransformer.xrOrigin;
		}
		set
		{
			m_XRBodyTransformer.xrOrigin = value;
		}
	}

	public XRBodyTransformer bodyTransformer => m_XRBodyTransformer;

	protected void Awake()
	{
		m_XRBodyTransformer = GetComponent<XRBodyTransformer>();
	}

	protected void Update()
	{
		s_ProvidersToRemove.Clear();
		foreach (KeyValuePair<LocomotionProvider, LocomotionProviderData> item in m_ProviderDataMap)
		{
			LocomotionProvider key = item.Key;
			if (key == null)
			{
				s_ProvidersToRemove.Add(key);
				continue;
			}
			LocomotionProviderData value = item.Value;
			if (value.state == LocomotionState.Preparing && key.canStartMoving)
			{
				ChangeState(key, value, LocomotionState.Moving);
			}
			else if (value.state == LocomotionState.Ended && Time.frameCount > value.locomotionEndFrame)
			{
				ChangeState(key, value, LocomotionState.Idle);
			}
		}
		if (s_ProvidersToRemove.Count <= 0)
		{
			return;
		}
		foreach (LocomotionProvider item2 in s_ProvidersToRemove)
		{
			m_ProviderDataMap.Remove(item2);
		}
	}

	internal bool TryPrepareLocomotion(LocomotionProvider provider)
	{
		if (!m_ProviderDataMap.TryGetValue(provider, out var value))
		{
			value = new LocomotionProviderData();
			m_ProviderDataMap[provider] = value;
		}
		else if (GetProviderLocomotionState(provider).IsActive())
		{
			return false;
		}
		ChangeState(provider, value, LocomotionState.Preparing);
		return true;
	}

	internal bool TryStartLocomotion(LocomotionProvider provider)
	{
		if (!m_ProviderDataMap.TryGetValue(provider, out var value))
		{
			value = new LocomotionProviderData();
			m_ProviderDataMap[provider] = value;
		}
		else if (GetProviderLocomotionState(provider) == LocomotionState.Moving)
		{
			return false;
		}
		ChangeState(provider, value, LocomotionState.Moving);
		return true;
	}

	internal bool TryEndLocomotion(LocomotionProvider provider)
	{
		if (!m_ProviderDataMap.TryGetValue(provider, out var value))
		{
			return false;
		}
		if (!value.state.IsActive())
		{
			return false;
		}
		ChangeState(provider, value, LocomotionState.Ended);
		return true;
	}

	private void ChangeState(LocomotionProvider provider, LocomotionProviderData providerData, LocomotionState state)
	{
		if (providerData.state != state)
		{
			LocomotionState state2 = providerData.state;
			providerData.state = state;
			if (state == LocomotionState.Ended)
			{
				providerData.locomotionEndFrame = Time.frameCount;
			}
			provider.OnLocomotionStateChanging(state2, state, m_XRBodyTransformer);
		}
	}

	public LocomotionState GetProviderLocomotionState(LocomotionProvider provider)
	{
		if (!m_ProviderDataMap.TryGetValue(provider, out var value))
		{
			return LocomotionState.Idle;
		}
		return value.state;
	}
}
