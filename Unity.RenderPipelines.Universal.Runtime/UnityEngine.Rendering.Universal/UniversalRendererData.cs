using System;
using UnityEngine.Experimental.Rendering;

namespace UnityEngine.Rendering.Universal;

[Serializable]
[ReloadGroup]
[ExcludeFromPreset]
public class UniversalRendererData : ScriptableRendererData, ISerializationCallbackReceiver
{
	[Obsolete("Moved to UniversalRenderPipelineRuntimeXRResources on GraphicsSettings. #from(2023.3)", false)]
	public XRSystemData xrSystemData;

	public PostProcessData postProcessData;

	private const int k_LatestAssetVersion = 3;

	[SerializeField]
	private int m_AssetVersion;

	[SerializeField]
	private LayerMask m_PrepassLayerMask = -1;

	[SerializeField]
	private LayerMask m_OpaqueLayerMask = -1;

	[SerializeField]
	private LayerMask m_TransparentLayerMask = -1;

	[SerializeField]
	private StencilStateData m_DefaultStencilState = new StencilStateData
	{
		passOperation = StencilOp.Replace
	};

	[SerializeField]
	private bool m_ShadowTransparentReceive = true;

	[SerializeField]
	private RenderingMode m_RenderingMode;

	[SerializeField]
	private DepthPrimingMode m_DepthPrimingMode;

	[SerializeField]
	private CopyDepthMode m_CopyDepthMode = CopyDepthMode.AfterTransparents;

	[SerializeField]
	private DepthFormat m_DepthAttachmentFormat;

	[SerializeField]
	private DepthFormat m_DepthTextureFormat;

	[SerializeField]
	private bool m_AccurateGbufferNormals;

	[SerializeField]
	private IntermediateTextureMode m_IntermediateTextureMode = IntermediateTextureMode.Always;

	[NonSerialized]
	private bool m_StripShadowsOffVariants = true;

	[NonSerialized]
	private bool m_StripAdditionalLightOffVariants = true;

	public LayerMask prepassLayerMask
	{
		get
		{
			return m_PrepassLayerMask;
		}
		set
		{
			SetDirty();
			m_PrepassLayerMask = value;
		}
	}

	public LayerMask opaqueLayerMask
	{
		get
		{
			return m_OpaqueLayerMask;
		}
		set
		{
			SetDirty();
			m_OpaqueLayerMask = value;
		}
	}

	public LayerMask transparentLayerMask
	{
		get
		{
			return m_TransparentLayerMask;
		}
		set
		{
			SetDirty();
			m_TransparentLayerMask = value;
		}
	}

	public StencilStateData defaultStencilState
	{
		get
		{
			return m_DefaultStencilState;
		}
		set
		{
			SetDirty();
			m_DefaultStencilState = value;
		}
	}

	public bool shadowTransparentReceive
	{
		get
		{
			return m_ShadowTransparentReceive;
		}
		set
		{
			SetDirty();
			m_ShadowTransparentReceive = value;
		}
	}

	public RenderingMode renderingMode
	{
		get
		{
			return m_RenderingMode;
		}
		set
		{
			SetDirty();
			m_RenderingMode = value;
		}
	}

	public DepthPrimingMode depthPrimingMode
	{
		get
		{
			return m_DepthPrimingMode;
		}
		set
		{
			SetDirty();
			m_DepthPrimingMode = value;
		}
	}

	public CopyDepthMode copyDepthMode
	{
		get
		{
			return m_CopyDepthMode;
		}
		set
		{
			SetDirty();
			m_CopyDepthMode = value;
		}
	}

	public DepthFormat depthAttachmentFormat
	{
		get
		{
			if (m_DepthAttachmentFormat != 0 && !SystemInfo.IsFormatSupported((GraphicsFormat)m_DepthAttachmentFormat, GraphicsFormatUsage.Render))
			{
				Debug.LogWarning("Selected Depth Attachment Format is not supported on this platform, falling back to Default");
				return DepthFormat.Default;
			}
			return m_DepthAttachmentFormat;
		}
		set
		{
			SetDirty();
			if (renderingMode == RenderingMode.Deferred && !GraphicsFormatUtility.IsStencilFormat((GraphicsFormat)value))
			{
				Debug.LogWarning("Depth format without stencil is not supported on Deferred renderer, falling back to Default");
				m_DepthAttachmentFormat = DepthFormat.Default;
			}
			else
			{
				m_DepthAttachmentFormat = value;
			}
		}
	}

	public DepthFormat depthTextureFormat
	{
		get
		{
			if (m_DepthTextureFormat != 0 && !SystemInfo.IsFormatSupported((GraphicsFormat)m_DepthTextureFormat, GraphicsFormatUsage.Render))
			{
				Debug.LogWarning("Selected Depth Texture Format " + m_DepthTextureFormat.ToString() + " is not supported on this platform, falling back to Default");
				return DepthFormat.Default;
			}
			return m_DepthTextureFormat;
		}
		set
		{
			SetDirty();
			m_DepthTextureFormat = value;
		}
	}

	public bool accurateGbufferNormals
	{
		get
		{
			return m_AccurateGbufferNormals;
		}
		set
		{
			SetDirty();
			m_AccurateGbufferNormals = value;
		}
	}

	public IntermediateTextureMode intermediateTextureMode
	{
		get
		{
			return m_IntermediateTextureMode;
		}
		set
		{
			SetDirty();
			m_IntermediateTextureMode = value;
		}
	}

	public bool usesDeferredLighting
	{
		get
		{
			if (m_RenderingMode != RenderingMode.Deferred)
			{
				return m_RenderingMode == RenderingMode.DeferredPlus;
			}
			return true;
		}
	}

	public bool usesClusterLightLoop
	{
		get
		{
			if (m_RenderingMode != RenderingMode.ForwardPlus)
			{
				return m_RenderingMode == RenderingMode.DeferredPlus;
			}
			return true;
		}
	}

	internal override bool stripShadowsOffVariants
	{
		get
		{
			return m_StripShadowsOffVariants;
		}
		set
		{
			m_StripShadowsOffVariants = value;
		}
	}

	internal override bool stripAdditionalLightOffVariants
	{
		get
		{
			return m_StripAdditionalLightOffVariants;
		}
		set
		{
			m_StripAdditionalLightOffVariants = value;
		}
	}

	protected override ScriptableRenderer Create()
	{
		if (!Application.isPlaying)
		{
			ReloadAllNullProperties();
		}
		return new UniversalRenderer(this);
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		ReloadAllNullProperties();
	}

	private void ReloadAllNullProperties()
	{
	}

	void ISerializationCallbackReceiver.OnBeforeSerialize()
	{
		m_AssetVersion = 3;
	}

	void ISerializationCallbackReceiver.OnAfterDeserialize()
	{
		if (m_AssetVersion <= 1)
		{
			m_CopyDepthMode = CopyDepthMode.AfterOpaques;
		}
		if (m_AssetVersion <= 2)
		{
			m_PrepassLayerMask = m_OpaqueLayerMask;
		}
		m_AssetVersion = 3;
	}
}
