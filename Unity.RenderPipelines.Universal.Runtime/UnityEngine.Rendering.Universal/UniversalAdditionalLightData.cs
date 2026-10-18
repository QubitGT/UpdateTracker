using System;

namespace UnityEngine.Rendering.Universal;

[DisallowMultipleComponent]
[RequireComponent(typeof(Light))]
public class UniversalAdditionalLightData : MonoBehaviour, ISerializationCallbackReceiver, IAdditionalData
{
	private enum Version
	{
		Initial = 0,
		RenderingLayers = 2,
		SoftShadowQuality = 3,
		RenderingLayersMask = 4,
		Count = 5
	}

	[Tooltip("Controls if light Shadow Bias parameters use pipeline settings.")]
	[SerializeField]
	private bool m_UsePipelineSettings = true;

	public static readonly int AdditionalLightsShadowResolutionTierCustom = -1;

	public static readonly int AdditionalLightsShadowResolutionTierLow = 0;

	public static readonly int AdditionalLightsShadowResolutionTierMedium = 1;

	public static readonly int AdditionalLightsShadowResolutionTierHigh = 2;

	public static readonly int AdditionalLightsShadowDefaultResolutionTier = AdditionalLightsShadowResolutionTierHigh;

	public static readonly int AdditionalLightsShadowDefaultCustomResolution = 128;

	[NonSerialized]
	private Light m_Light;

	public static readonly int AdditionalLightsShadowMinimumResolution = 128;

	[Tooltip("Controls if light shadow resolution uses pipeline settings.")]
	[SerializeField]
	private int m_AdditionalLightsShadowResolutionTier = AdditionalLightsShadowDefaultResolutionTier;

	[SerializeField]
	private bool m_CustomShadowLayers;

	[SerializeField]
	private Vector2 m_LightCookieSize = Vector2.one;

	[SerializeField]
	private Vector2 m_LightCookieOffset = Vector2.zero;

	[SerializeField]
	private SoftShadowQuality m_SoftShadowQuality;

	[SerializeField]
	private RenderingLayerMask m_RenderingLayersMask = RenderingLayerMask.defaultRenderingLayerMask;

	[SerializeField]
	private RenderingLayerMask m_ShadowRenderingLayersMask = RenderingLayerMask.defaultRenderingLayerMask;

	[SerializeField]
	private Version m_Version = Version.Count;

	[Obsolete("This is obsolete, please use m_RenderingLayerMask instead. #from(2023.1)", false)]
	[SerializeField]
	private LightLayerEnum m_LightLayerMask = LightLayerEnum.LightLayerDefault;

	[Obsolete("This is obsolete, please use m_RenderingLayerMask instead. #from(2023.1)", false)]
	[SerializeField]
	private LightLayerEnum m_ShadowLayerMask = LightLayerEnum.LightLayerDefault;

	[SerializeField]
	[Obsolete("This is obsolete, please use m_RenderingLayersMask instead. #from(6000.2)", false)]
	private uint m_RenderingLayers = 1u;

	[SerializeField]
	[Obsolete("This is obsolete, please use renderingLayersMask instead. #from(6000.2)", false)]
	private uint m_ShadowRenderingLayers = 1u;

	public bool usePipelineSettings
	{
		get
		{
			return m_UsePipelineSettings;
		}
		set
		{
			m_UsePipelineSettings = value;
		}
	}

	internal Light light
	{
		get
		{
			if (!m_Light)
			{
				TryGetComponent<Light>(out m_Light);
			}
			return m_Light;
		}
	}

	public int additionalLightsShadowResolutionTier => m_AdditionalLightsShadowResolutionTier;

	public bool customShadowLayers
	{
		get
		{
			return m_CustomShadowLayers;
		}
		set
		{
			if (m_CustomShadowLayers != value)
			{
				m_CustomShadowLayers = value;
				SyncLightAndShadowLayers();
			}
		}
	}

	[Tooltip("Controls the size of the cookie mask currently assigned to the light.")]
	public Vector2 lightCookieSize
	{
		get
		{
			return m_LightCookieSize;
		}
		set
		{
			m_LightCookieSize = value;
		}
	}

	[Tooltip("Controls the offset of the cookie mask currently assigned to the light.")]
	public Vector2 lightCookieOffset
	{
		get
		{
			return m_LightCookieOffset;
		}
		set
		{
			m_LightCookieOffset = value;
		}
	}

	[Tooltip("Controls the filtering quality of soft shadows. Higher quality has lower performance.")]
	public SoftShadowQuality softShadowQuality
	{
		get
		{
			return m_SoftShadowQuality;
		}
		set
		{
			m_SoftShadowQuality = value;
		}
	}

	public RenderingLayerMask renderingLayers
	{
		get
		{
			return m_RenderingLayersMask;
		}
		set
		{
			if ((int)m_RenderingLayersMask != (int)value)
			{
				m_RenderingLayersMask = value;
				SyncLightAndShadowLayers();
			}
		}
	}

	public RenderingLayerMask shadowRenderingLayers
	{
		get
		{
			return m_ShadowRenderingLayersMask;
		}
		set
		{
			if ((int)value != (int)m_ShadowRenderingLayersMask)
			{
				m_ShadowRenderingLayersMask = value;
				SyncLightAndShadowLayers();
			}
		}
	}

	[Obsolete("This is obsolete, please use renderingLayerMask instead. #from(2023.1)", true)]
	public LightLayerEnum lightLayerMask
	{
		get
		{
			return m_LightLayerMask;
		}
		set
		{
			m_LightLayerMask = value;
		}
	}

	[Obsolete("This is obsolete, please use shadowRenderingLayerMask instead. #from(2023.1)", true)]
	public LightLayerEnum shadowLayerMask
	{
		get
		{
			return m_ShadowLayerMask;
		}
		set
		{
			m_ShadowLayerMask = value;
		}
	}

	private void SyncLightAndShadowLayers()
	{
		if ((bool)light)
		{
			light.renderingLayerMask = (m_CustomShadowLayers ? m_ShadowRenderingLayersMask : m_RenderingLayersMask);
		}
	}

	void ISerializationCallbackReceiver.OnBeforeSerialize()
	{
		if (m_Version == Version.Count)
		{
			m_Version = Version.RenderingLayersMask;
		}
	}

	void ISerializationCallbackReceiver.OnAfterDeserialize()
	{
		if (m_Version == Version.Count)
		{
			m_Version = Version.Initial;
		}
		if (m_Version < Version.RenderingLayers)
		{
			m_RenderingLayers = (uint)m_LightLayerMask;
			m_ShadowRenderingLayers = (uint)m_ShadowLayerMask;
			m_Version = Version.RenderingLayers;
		}
		if (m_Version < Version.SoftShadowQuality)
		{
			m_SoftShadowQuality = (SoftShadowQuality)Math.Clamp((int)(m_SoftShadowQuality + 1), 0, 3);
			m_Version = Version.SoftShadowQuality;
		}
		if (m_Version < Version.RenderingLayersMask)
		{
			m_RenderingLayersMask = m_RenderingLayers;
			m_ShadowRenderingLayersMask = m_ShadowRenderingLayers;
			m_Version = Version.RenderingLayersMask;
		}
	}
}
