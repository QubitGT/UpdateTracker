using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine.Scripting.APIUpdating;

namespace UnityEngine.XR.Interaction.Toolkit.Locomotion.Comfort;

[AddComponentMenu("XR/Locomotion/Tunneling Vignette Controller", 11)]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Locomotion.Comfort.TunnelingVignetteController.html")]
[MovedFrom("UnityEngine.XR.Interaction.Toolkit")]
public class TunnelingVignetteController : MonoBehaviour
{
	private static class ShaderPropertyLookup
	{
		public static readonly int apertureSize = Shader.PropertyToID("_ApertureSize");

		public static readonly int featheringEffect = Shader.PropertyToID("_FeatheringEffect");

		public static readonly int vignetteColor = Shader.PropertyToID("_VignetteColor");

		public static readonly int vignetteColorBlend = Shader.PropertyToID("_VignetteColorBlend");
	}

	private class ProviderRecord
	{
		public ITunnelingVignetteProvider provider { get; }

		public EaseState easeState { get; set; }

		public float dynamicApertureSize { get; set; } = 1f;


		public bool easeInLockEnded { get; set; }

		public float dynamicEaseOutDelayTime { get; set; }

		public ProviderRecord(ITunnelingVignetteProvider provider)
		{
			this.provider = provider;
		}
	}

	private const string k_DefaultShader = "VR/TunnelingVignette";

	[SerializeField]
	private VignetteParameters m_DefaultParameters = new VignetteParameters();

	[SerializeField]
	private VignetteParameters m_CurrentParameters = new VignetteParameters();

	[SerializeField]
	private List<LocomotionVignetteProvider> m_LocomotionVignetteProviders = new List<LocomotionVignetteProvider>();

	private readonly List<ProviderRecord> m_ProviderRecords = new List<ProviderRecord>();

	private MeshRenderer m_MeshRender;

	private MeshFilter m_MeshFilter;

	private Material m_SharedMaterial;

	private MaterialPropertyBlock m_VignettePropertyBlock;

	public VignetteParameters defaultParameters
	{
		get
		{
			return m_DefaultParameters;
		}
		set
		{
			m_DefaultParameters = value;
		}
	}

	public VignetteParameters currentParameters => m_CurrentParameters;

	public List<LocomotionVignetteProvider> locomotionVignetteProviders
	{
		get
		{
			return m_LocomotionVignetteProviders;
		}
		set
		{
			m_LocomotionVignetteProviders = value;
		}
	}

	internal static event Action<ITunnelingVignetteProvider> vignetteProviderQueued;

	public void BeginTunnelingVignette(ITunnelingVignetteProvider provider)
	{
		foreach (ProviderRecord providerRecord in m_ProviderRecords)
		{
			if (providerRecord.provider == provider)
			{
				providerRecord.easeState = EaseState.EasingIn;
				return;
			}
		}
		m_ProviderRecords.Add(new ProviderRecord(provider)
		{
			easeState = EaseState.EasingIn
		});
		TunnelingVignetteController.vignetteProviderQueued?.Invoke(provider);
	}

	public void EndTunnelingVignette(ITunnelingVignetteProvider provider)
	{
		VignetteParameters vignetteParameters = provider.vignetteParameters ?? m_DefaultParameters;
		foreach (ProviderRecord providerRecord in m_ProviderRecords)
		{
			if (providerRecord.provider == provider)
			{
				providerRecord.easeState = ((vignetteParameters.easeInTimeLock && !providerRecord.easeInLockEnded) ? EaseState.EasingInHoldBeforeEasingOut : ((vignetteParameters.easeOutDelayTime > 0f && providerRecord.dynamicEaseOutDelayTime < vignetteParameters.easeOutDelayTime) ? EaseState.EasingOutDelay : EaseState.EasingOut));
				return;
			}
		}
		EaseState easeState = (vignetteParameters.easeInTimeLock ? EaseState.EasingInHoldBeforeEasingOut : ((vignetteParameters.easeOutDelayTime > 0f) ? EaseState.EasingOutDelay : EaseState.EasingOut));
		m_ProviderRecords.Add(new ProviderRecord(provider)
		{
			easeState = easeState
		});
	}

	[Conditional("UNITY_EDITOR")]
	internal void PreviewInEditor(VignetteParameters previewParameters)
	{
		if (!Application.isPlaying && base.gameObject.activeInHierarchy)
		{
			UpdateTunnelingVignette(previewParameters);
		}
	}

	protected virtual void Awake()
	{
		m_CurrentParameters.CopyFrom(VignetteParameters.Defaults.noEffect);
		UpdateTunnelingVignette(VignetteParameters.Defaults.noEffect);
	}

	[Conditional("UNITY_EDITOR")]
	protected virtual void Reset()
	{
		m_DefaultParameters.CopyFrom(VignetteParameters.Defaults.defaultEffect);
		m_CurrentParameters.CopyFrom(VignetteParameters.Defaults.noEffect);
		UpdateTunnelingVignette(m_DefaultParameters);
	}

	protected virtual void Update()
	{
		if (m_LocomotionVignetteProviders.Count > 0)
		{
			foreach (LocomotionVignetteProvider locomotionVignetteProvider in m_LocomotionVignetteProviders)
			{
				LocomotionProvider locomotionProvider = locomotionVignetteProvider.locomotionProvider;
				if (locomotionVignetteProvider.enabled && !(locomotionProvider == null))
				{
					if (locomotionProvider.isLocomotionActive)
					{
						BeginTunnelingVignette(locomotionVignetteProvider);
					}
					else if (locomotionProvider.locomotionState == LocomotionState.Ended)
					{
						EndTunnelingVignette(locomotionVignetteProvider);
					}
				}
			}
		}
		if (m_ProviderRecords.Count == 0)
		{
			return;
		}
		foreach (ProviderRecord providerRecord2 in m_ProviderRecords)
		{
			VignetteParameters vignetteParameters = providerRecord2.provider.vignetteParameters ?? m_DefaultParameters;
			float dynamicApertureSize = providerRecord2.dynamicApertureSize;
			switch (providerRecord2.easeState)
			{
			case EaseState.NotEasing:
				providerRecord2.dynamicApertureSize = 1f;
				providerRecord2.dynamicEaseOutDelayTime = 0f;
				providerRecord2.easeInLockEnded = false;
				continue;
			case EaseState.EasingIn:
			{
				float num3 = Mathf.Max(vignetteParameters.easeInTime, 0f);
				float apertureSize2 = vignetteParameters.apertureSize;
				providerRecord2.easeInLockEnded = false;
				if (num3 > 0f && dynamicApertureSize > apertureSize2)
				{
					float num4 = dynamicApertureSize + (apertureSize2 - 1f) / num3 * Time.unscaledDeltaTime;
					providerRecord2.dynamicApertureSize = ((num4 < apertureSize2) ? apertureSize2 : num4);
				}
				else
				{
					providerRecord2.dynamicApertureSize = apertureSize2;
				}
				continue;
			}
			case EaseState.EasingInHoldBeforeEasingOut:
				if (!providerRecord2.easeInLockEnded)
				{
					float num = Mathf.Max(vignetteParameters.easeInTime, 0f);
					float apertureSize = vignetteParameters.apertureSize;
					if (num > 0f && dynamicApertureSize > apertureSize)
					{
						float num2 = dynamicApertureSize + (apertureSize - 1f) / num * Time.unscaledDeltaTime;
						providerRecord2.dynamicApertureSize = ((num2 < apertureSize) ? apertureSize : num2);
						continue;
					}
					providerRecord2.easeInLockEnded = true;
					if (!(vignetteParameters.easeOutDelayTime > 0f) || !(providerRecord2.dynamicEaseOutDelayTime < vignetteParameters.easeOutDelayTime))
					{
						providerRecord2.easeState = EaseState.EasingOut;
						break;
					}
					providerRecord2.easeState = EaseState.EasingOutDelay;
				}
				else
				{
					if (!(vignetteParameters.easeOutDelayTime > 0f))
					{
						providerRecord2.easeState = EaseState.EasingOutDelay;
						break;
					}
					providerRecord2.easeState = EaseState.EasingOutDelay;
				}
				goto case EaseState.EasingOutDelay;
			case EaseState.EasingOutDelay:
			{
				float dynamicEaseOutDelayTime = providerRecord2.dynamicEaseOutDelayTime;
				float num5 = Mathf.Max(vignetteParameters.easeOutDelayTime, 0f);
				if (num5 > 0f && dynamicEaseOutDelayTime < num5)
				{
					dynamicEaseOutDelayTime += Time.unscaledDeltaTime;
					providerRecord2.dynamicEaseOutDelayTime = ((dynamicEaseOutDelayTime > num5) ? num5 : dynamicEaseOutDelayTime);
				}
				if (!(providerRecord2.dynamicEaseOutDelayTime >= num5))
				{
					continue;
				}
				providerRecord2.easeState = EaseState.EasingOut;
				break;
			}
			case EaseState.EasingOut:
				break;
			default:
				continue;
			}
			float num6 = Mathf.Max(vignetteParameters.easeOutTime, 0f);
			float apertureSize3 = vignetteParameters.apertureSize;
			if (num6 > 0f && dynamicApertureSize < 1f)
			{
				float num7 = dynamicApertureSize + (1f - apertureSize3) / num6 * Time.unscaledDeltaTime;
				providerRecord2.dynamicApertureSize = ((num7 > 1f) ? 1f : num7);
			}
			else
			{
				providerRecord2.dynamicApertureSize = 1f;
			}
			if (providerRecord2.dynamicApertureSize >= 1f)
			{
				providerRecord2.easeState = EaseState.NotEasing;
			}
		}
		float num8 = 1f;
		ProviderRecord providerRecord = null;
		foreach (ProviderRecord providerRecord3 in m_ProviderRecords)
		{
			float dynamicApertureSize2 = providerRecord3.dynamicApertureSize;
			if (dynamicApertureSize2 < num8)
			{
				providerRecord = providerRecord3;
				num8 = dynamicApertureSize2;
			}
		}
		if (providerRecord != null)
		{
			m_CurrentParameters.CopyFrom(providerRecord.provider.vignetteParameters ?? m_DefaultParameters);
		}
		m_CurrentParameters.apertureSize = num8;
		UpdateTunnelingVignette(m_CurrentParameters);
	}

	private void UpdateTunnelingVignette(VignetteParameters parameters)
	{
		if (parameters == null)
		{
			parameters = m_DefaultParameters;
		}
		if (TrySetUpMaterial())
		{
			m_MeshRender.GetPropertyBlock(m_VignettePropertyBlock);
			m_VignettePropertyBlock.SetFloat(ShaderPropertyLookup.apertureSize, parameters.apertureSize);
			m_VignettePropertyBlock.SetFloat(ShaderPropertyLookup.featheringEffect, parameters.featheringEffect);
			m_VignettePropertyBlock.SetColor(ShaderPropertyLookup.vignetteColor, parameters.vignetteColor);
			m_VignettePropertyBlock.SetColor(ShaderPropertyLookup.vignetteColorBlend, parameters.vignetteColorBlend);
			m_MeshRender.SetPropertyBlock(m_VignettePropertyBlock);
		}
		Transform transform = base.transform;
		Vector3 localPosition = transform.localPosition;
		if (!Mathf.Approximately(localPosition.y, parameters.apertureVerticalPosition))
		{
			localPosition.y = parameters.apertureVerticalPosition;
			transform.localPosition = localPosition;
		}
	}

	private bool TrySetUpMaterial()
	{
		if (m_MeshRender == null)
		{
			m_MeshRender = GetComponent<MeshRenderer>();
		}
		if (m_MeshRender == null)
		{
			m_MeshRender = base.gameObject.AddComponent<MeshRenderer>();
		}
		if (m_VignettePropertyBlock == null)
		{
			m_VignettePropertyBlock = new MaterialPropertyBlock();
		}
		if (m_MeshFilter == null)
		{
			m_MeshFilter = GetComponent<MeshFilter>();
		}
		if (m_MeshFilter == null)
		{
			m_MeshFilter = base.gameObject.AddComponent<MeshFilter>();
		}
		if (m_MeshFilter.sharedMesh == null)
		{
			Debug.LogWarning("The default mesh for the TunnelingVignetteController is not set. Make sure to import it from the Tunneling Vignette Sample of XR Interaction Toolkit.", this);
			return false;
		}
		if (m_MeshRender.sharedMaterial == null)
		{
			Shader shader = Shader.Find("VR/TunnelingVignette");
			if (shader == null)
			{
				Debug.LogWarning("The default material for the TunnelingVignetteController is not set, and the default Shader: VR/TunnelingVignette cannot be found. Make sure they are imported from the Tunneling Vignette Sample of XR Interaction Toolkit.", this);
				return false;
			}
			Debug.LogWarning("The default material for the TunnelingVignetteController is not set. Make sure it is imported from the Tunneling Vignette Sample of XR Interaction Toolkit. + Try creating a material using the default Shader: VR/TunnelingVignette", this);
			m_SharedMaterial = new Material(shader)
			{
				name = "DefaultTunnelingVignette"
			};
			m_MeshRender.sharedMaterial = m_SharedMaterial;
		}
		else
		{
			m_SharedMaterial = m_MeshRender.sharedMaterial;
		}
		return true;
	}
}
