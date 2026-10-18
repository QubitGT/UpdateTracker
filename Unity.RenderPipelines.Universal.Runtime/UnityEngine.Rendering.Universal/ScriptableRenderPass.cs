using System;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.Collections;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace UnityEngine.Rendering.Universal;

public abstract class ScriptableRenderPass : IRenderGraphRecorder
{
	[Obsolete("This rendering path is for compatibility mode only (when Render Graph is disabled). Use Render Graph API instead.", false)]
	public static RTHandle k_CameraTarget = RTHandles.Alloc(BuiltinRenderTextureType.CameraTarget);

	private RenderBufferStoreAction[] m_ColorStoreActions = new RenderBufferStoreAction[1];

	private RenderBufferStoreAction m_DepthStoreAction;

	private bool[] m_OverriddenColorStoreActions = new bool[1];

	private bool m_OverriddenDepthStoreAction;

	private ProfilingSampler m_ProfingSampler;

	private string m_PassName;

	private RenderGraphSettings m_RenderGraphSettings;

	internal NativeArray<int> m_ColorAttachmentIndices;

	internal NativeArray<int> m_InputAttachmentIndices;

	private RTHandle[] m_ColorAttachments;

	internal RTHandle[] m_InputAttachments = new RTHandle[8];

	internal bool[] m_InputAttachmentIsTransient = new bool[8];

	private RTHandle m_DepthAttachment;

	private ScriptableRenderPassInput m_Input;

	private ClearFlag m_ClearFlag;

	private Color m_ClearColor = Color.black;

	public RenderPassEvent renderPassEvent { get; set; }

	[Obsolete("Use colorAttachmentHandles", true)]
	public RenderTargetIdentifier[] colorAttachments
	{
		get
		{
			throw new NotSupportedException("colorAttachments has been deprecated. Use colorAttachmentHandles instead.");
		}
	}

	[Obsolete("Use colorAttachmentHandle", true)]
	public RenderTargetIdentifier[] colorAttachment
	{
		get
		{
			throw new NotSupportedException("colorAttachment has been deprecated. Use colorAttachmentHandle instead.");
		}
	}

	[Obsolete("Use depthAttachmentHandle", true)]
	public RenderTargetIdentifier depthAttachment
	{
		get
		{
			throw new NotSupportedException("depthAttachment has been deprecated. Use depthAttachmentHandle instead.");
		}
	}

	public RTHandle[] colorAttachmentHandles => m_ColorAttachments;

	public RTHandle colorAttachmentHandle => m_ColorAttachments[0];

	public RTHandle depthAttachmentHandle => m_DepthAttachment;

	public RenderBufferStoreAction[] colorStoreActions => m_ColorStoreActions;

	public RenderBufferStoreAction depthStoreAction => m_DepthStoreAction;

	internal bool[] overriddenColorStoreActions => m_OverriddenColorStoreActions;

	internal bool overriddenDepthStoreAction => m_OverriddenDepthStoreAction;

	public ScriptableRenderPassInput input => m_Input;

	public ClearFlag clearFlag => m_ClearFlag;

	public Color clearColor => m_ClearColor;

	public bool requiresIntermediateTexture { get; set; }

	protected internal ProfilingSampler profilingSampler
	{
		get
		{
			if (m_RenderGraphSettings == null)
			{
				m_RenderGraphSettings = GraphicsSettings.GetRenderPipelineSettings<RenderGraphSettings>();
			}
			if (!m_RenderGraphSettings.enableRenderCompatibilityMode)
			{
				return null;
			}
			return m_ProfingSampler;
		}
		set
		{
			m_ProfingSampler = value;
			m_PassName = ((value != null) ? value.name : GetType().Name);
		}
	}

	protected internal string passName => m_PassName;

	internal bool overrideCameraTarget { get; set; }

	internal bool isBlitRenderPass { get; set; }

	internal bool useNativeRenderPass { get; set; }

	internal int renderPassQueueIndex { get; set; }

	internal GraphicsFormat[] renderTargetFormat { get; set; }

	[EditorBrowsable(EditorBrowsableState.Never)]
	public virtual void FrameCleanup(CommandBuffer cmd)
	{
		OnCameraCleanup(cmd);
	}

	internal static DebugHandler GetActiveDebugHandler(UniversalCameraData cameraData)
	{
		DebugHandler debugHandler = cameraData.renderer.DebugHandler;
		if (debugHandler != null && debugHandler.IsActiveForCamera(cameraData.isPreviewCamera))
		{
			return debugHandler;
		}
		return null;
	}

	public ScriptableRenderPass()
	{
		renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
		m_ColorAttachments = new RTHandle[8] { k_CameraTarget, null, null, null, null, null, null, null };
		m_DepthAttachment = k_CameraTarget;
		m_InputAttachments = new RTHandle[8];
		m_InputAttachmentIsTransient = new bool[8];
		m_ColorStoreActions = new RenderBufferStoreAction[8];
		m_DepthStoreAction = RenderBufferStoreAction.Store;
		m_OverriddenColorStoreActions = new bool[8];
		m_OverriddenDepthStoreAction = false;
		m_ClearFlag = ClearFlag.None;
		m_ClearColor = Color.black;
		overrideCameraTarget = false;
		isBlitRenderPass = false;
		useNativeRenderPass = true;
		renderPassQueueIndex = -1;
		renderTargetFormat = new GraphicsFormat[8];
		profilingSampler = new ProfilingSampler(GetType().Name);
	}

	public void ConfigureInput(ScriptableRenderPassInput passInput)
	{
		m_Input = passInput;
	}

	[Obsolete("This rendering path is for compatibility mode only (when Render Graph is disabled). Use Render Graph API instead.", false)]
	public void ConfigureColorStoreAction(RenderBufferStoreAction storeAction, uint attachmentIndex = 0u)
	{
		m_ColorStoreActions[attachmentIndex] = storeAction;
		m_OverriddenColorStoreActions[attachmentIndex] = true;
	}

	[Obsolete("This rendering path is for compatibility mode only (when Render Graph is disabled). Use Render Graph API instead.", false)]
	public void ConfigureColorStoreActions(RenderBufferStoreAction[] storeActions)
	{
		int num = Math.Min(storeActions.Length, m_ColorStoreActions.Length);
		for (uint num2 = 0u; num2 < num; num2++)
		{
			m_ColorStoreActions[num2] = storeActions[num2];
			m_OverriddenColorStoreActions[num2] = true;
		}
	}

	[Obsolete("This rendering path is for compatibility mode only (when Render Graph is disabled). Use Render Graph API instead.", false)]
	public void ConfigureDepthStoreAction(RenderBufferStoreAction storeAction)
	{
		m_DepthStoreAction = storeAction;
		m_OverriddenDepthStoreAction = true;
	}

	[Obsolete("This rendering path is for compatibility mode only (when Render Graph is disabled). Use Render Graph API instead.", false)]
	internal void ConfigureInputAttachments(RTHandle input, bool isTransient = false)
	{
		m_InputAttachments[0] = input;
		m_InputAttachmentIsTransient[0] = isTransient;
	}

	[Obsolete("This rendering path is for compatibility mode only (when Render Graph is disabled). Use Render Graph API instead.", false)]
	internal void ConfigureInputAttachments(RTHandle[] inputs)
	{
		m_InputAttachments = inputs;
	}

	[Obsolete("This rendering path is for compatibility mode only (when Render Graph is disabled). Use Render Graph API instead.", false)]
	internal void ConfigureInputAttachments(RTHandle[] inputs, bool[] isTransient)
	{
		ConfigureInputAttachments(inputs);
		m_InputAttachmentIsTransient = isTransient;
	}

	[Obsolete("This rendering path is for compatibility mode only (when Render Graph is disabled). Use Render Graph API instead.", false)]
	internal void SetInputAttachmentTransient(int idx, bool isTransient)
	{
		m_InputAttachmentIsTransient[idx] = isTransient;
	}

	[Obsolete("This rendering path is for compatibility mode only (when Render Graph is disabled). Use Render Graph API instead.", false)]
	internal bool IsInputAttachmentTransient(int idx)
	{
		return m_InputAttachmentIsTransient[idx];
	}

	[Obsolete("This rendering path is for compatibility mode only (when Render Graph is disabled). Use Render Graph API instead.", false)]
	public void ResetTarget()
	{
		overrideCameraTarget = false;
		m_DepthAttachment = null;
		m_ColorAttachments[0] = null;
		for (int i = 1; i < m_ColorAttachments.Length; i++)
		{
			m_ColorAttachments[i] = null;
		}
	}

	[Obsolete("Use RTHandles for colorAttachment and depthAttachment", true)]
	public void ConfigureTarget(RenderTargetIdentifier colorAttachment, RenderTargetIdentifier depthAttachment)
	{
		throw new NotSupportedException("ConfigureTarget with RenderTargetIdentifier has been deprecated. Use RTHandles instead");
	}

	[Obsolete("This rendering path is for compatibility mode only (when Render Graph is disabled). Use Render Graph API instead.", false)]
	public void ConfigureTarget(RTHandle colorAttachment, RTHandle depthAttachment)
	{
		overrideCameraTarget = true;
		m_DepthAttachment = depthAttachment;
		m_ColorAttachments[0] = colorAttachment;
		for (int i = 1; i < m_ColorAttachments.Length; i++)
		{
			m_ColorAttachments[i] = null;
		}
	}

	[Obsolete("Use RTHandles for colorAttachments and depthAttachment", true)]
	public void ConfigureTarget(RenderTargetIdentifier[] colorAttachments, RenderTargetIdentifier depthAttachment)
	{
		throw new NotSupportedException("ConfigureTarget with RenderTargetIdentifier has been deprecated. Use it with RTHandles instead");
	}

	[Obsolete("This rendering path is for compatibility mode only (when Render Graph is disabled). Use Render Graph API instead.", false)]
	public void ConfigureTarget(RTHandle[] colorAttachments, RTHandle depthAttachment)
	{
		overrideCameraTarget = true;
		uint validColorBufferCount = RenderingUtils.GetValidColorBufferCount(colorAttachments);
		if (validColorBufferCount > SystemInfo.supportedRenderTargetCount)
		{
			Debug.LogError("Trying to set " + validColorBufferCount + " renderTargets, which is more than the maximum supported:" + SystemInfo.supportedRenderTargetCount);
		}
		if (colorAttachments.Length > m_ColorAttachments.Length)
		{
			Debug.LogError("Trying to set " + colorAttachments.Length + " color attachments, which is more than the maximum supported:" + m_ColorAttachments.Length);
		}
		for (int i = 0; i < colorAttachments.Length; i++)
		{
			m_ColorAttachments[i] = colorAttachments[i];
		}
		for (int j = colorAttachments.Length; j < m_ColorAttachments.Length; j++)
		{
			m_ColorAttachments[j] = null;
		}
		m_DepthAttachment = depthAttachment;
	}

	[Obsolete("This rendering path is for compatibility mode only (when Render Graph is disabled). Use Render Graph API instead.", false)]
	internal void ConfigureTarget(RTHandle[] colorAttachments, RTHandle depthAttachment, GraphicsFormat[] formats)
	{
		ConfigureTarget(colorAttachments, depthAttachment);
		for (int i = 0; i < formats.Length; i++)
		{
			renderTargetFormat[i] = formats[i];
		}
	}

	[Obsolete("Use RTHandle for colorAttachment", true)]
	public void ConfigureTarget(RenderTargetIdentifier colorAttachment)
	{
		throw new NotSupportedException("ConfigureTarget with RenderTargetIdentifier has been deprecated. Use it with RTHandles instead");
	}

	[Obsolete("This rendering path is for compatibility mode only (when Render Graph is disabled). Use Render Graph API instead.", false)]
	public void ConfigureTarget(RTHandle colorAttachment)
	{
		ConfigureTarget(colorAttachment, k_CameraTarget);
	}

	[Obsolete("Use RTHandles for colorAttachments", true)]
	public void ConfigureTarget(RenderTargetIdentifier[] colorAttachments)
	{
		throw new NotSupportedException("ConfigureTarget with RenderTargetIdentifier has been deprecated. Use it with RTHandles instead");
	}

	[Obsolete("This rendering path is for compatibility mode only (when Render Graph is disabled). Use Render Graph API instead.", false)]
	public void ConfigureTarget(RTHandle[] colorAttachments)
	{
		ConfigureTarget(colorAttachments, k_CameraTarget);
	}

	[Obsolete("This rendering path is for compatibility mode only (when Render Graph is disabled). Use Render Graph API instead.", false)]
	public void ConfigureClear(ClearFlag clearFlag, Color clearColor)
	{
		m_ClearFlag = clearFlag;
		m_ClearColor = clearColor;
	}

	[Obsolete("This rendering path is for compatibility mode only (when Render Graph is disabled). Use Render Graph API instead.", false)]
	public virtual void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
	{
	}

	[Obsolete("This rendering path is for compatibility mode only (when Render Graph is disabled). Use Render Graph API instead.", false)]
	public virtual void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
	{
	}

	public virtual void OnCameraCleanup(CommandBuffer cmd)
	{
	}

	[Obsolete("This rendering path is for compatibility mode only (when Render Graph is disabled). Use Render Graph API instead.", false)]
	public virtual void OnFinishCameraStackRendering(CommandBuffer cmd)
	{
	}

	[Obsolete("This rendering path is for compatibility mode only (when Render Graph is disabled). Use Render Graph API instead.", false)]
	public virtual void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		Debug.LogWarning("Execute is not implemented, the pass " + ToString() + " won't be executed in the current render loop.");
	}

	public virtual void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
	{
		Debug.LogWarning("The render pass " + ToString() + " does not have an implementation of the RecordRenderGraph method. Please implement this method, or consider turning on Compatibility Mode (RenderGraph disabled) in the menu Edit > Project Settings > Graphics > URP. Otherwise the render pass will have no effect. For more information, refer to https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@latest/index.html?subfolder=/manual/customizing-urp.html.");
	}

	[Obsolete("Use RTHandles for source and destination", true)]
	public void Blit(CommandBuffer cmd, RenderTargetIdentifier source, RenderTargetIdentifier destination, Material material = null, int passIndex = 0)
	{
		throw new NotSupportedException("Blit with RenderTargetIdentifier has been deprecated. Use RTHandles instead");
	}

	[Obsolete("This rendering path is for compatibility mode only (when Render Graph is disabled). Use Render Graph API instead.", false)]
	public void Blit(CommandBuffer cmd, RTHandle source, RTHandle destination, Material material = null, int passIndex = 0)
	{
		if (material == null)
		{
			Blitter.BlitCameraTexture(cmd, source, destination, 0f, source.rt.filterMode == FilterMode.Bilinear);
		}
		else
		{
			Blitter.BlitCameraTexture(cmd, source, destination, material, passIndex);
		}
	}

	[Obsolete("This rendering path is for compatibility mode only (when Render Graph is disabled). Use Render Graph API instead.", false)]
	public void Blit(CommandBuffer cmd, ref RenderingData data, Material material, int passIndex = 0)
	{
		ScriptableRenderer renderer = data.cameraData.renderer;
		Blit(cmd, renderer.cameraColorTargetHandle, renderer.GetCameraColorFrontBuffer(cmd), material, passIndex);
		renderer.SwapColorBuffer(cmd);
	}

	[Obsolete("This rendering path is for compatibility mode only (when Render Graph is disabled). Use Render Graph API instead.", false)]
	public void Blit(CommandBuffer cmd, ref RenderingData data, RTHandle source, Material material, int passIndex = 0)
	{
		ScriptableRenderer renderer = data.cameraData.renderer;
		Blit(cmd, source, renderer.cameraColorTargetHandle, material, passIndex);
	}

	public DrawingSettings CreateDrawingSettings(ShaderTagId shaderTagId, ref RenderingData renderingData, SortingCriteria sortingCriteria)
	{
		ContextContainer frameData = renderingData.frameData;
		UniversalRenderingData renderingData2 = frameData.Get<UniversalRenderingData>();
		UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
		UniversalLightData lightData = frameData.Get<UniversalLightData>();
		return RenderingUtils.CreateDrawingSettings(shaderTagId, renderingData2, cameraData, lightData, sortingCriteria);
	}

	public DrawingSettings CreateDrawingSettings(ShaderTagId shaderTagId, UniversalRenderingData renderingData, UniversalCameraData cameraData, UniversalLightData lightData, SortingCriteria sortingCriteria)
	{
		return RenderingUtils.CreateDrawingSettings(shaderTagId, renderingData, cameraData, lightData, sortingCriteria);
	}

	public DrawingSettings CreateDrawingSettings(List<ShaderTagId> shaderTagIdList, ref RenderingData renderingData, SortingCriteria sortingCriteria)
	{
		ContextContainer frameData = renderingData.frameData;
		UniversalRenderingData renderingData2 = frameData.Get<UniversalRenderingData>();
		UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
		UniversalLightData lightData = frameData.Get<UniversalLightData>();
		return RenderingUtils.CreateDrawingSettings(shaderTagIdList, renderingData2, cameraData, lightData, sortingCriteria);
	}

	public DrawingSettings CreateDrawingSettings(List<ShaderTagId> shaderTagIdList, UniversalRenderingData renderingData, UniversalCameraData cameraData, UniversalLightData lightData, SortingCriteria sortingCriteria)
	{
		return RenderingUtils.CreateDrawingSettings(shaderTagIdList, renderingData, cameraData, lightData, sortingCriteria);
	}

	public static bool operator <(ScriptableRenderPass lhs, ScriptableRenderPass rhs)
	{
		return lhs.renderPassEvent < rhs.renderPassEvent;
	}

	public static bool operator >(ScriptableRenderPass lhs, ScriptableRenderPass rhs)
	{
		return lhs.renderPassEvent > rhs.renderPassEvent;
	}

	internal static int GetRenderPassEventRange(RenderPassEvent renderPassEvent)
	{
		int num = RenderPassEventsEnumValues.values.Length;
		int num2 = 0;
		for (int i = 0; i < num; i++)
		{
			if (RenderPassEventsEnumValues.values[num2] == (int)renderPassEvent)
			{
				break;
			}
			num2++;
		}
		if (num2 >= num)
		{
			Debug.LogError("GetRenderPassEventRange: invalid renderPassEvent value cannot be found in the RenderPassEvent enumeration");
			return 0;
		}
		if (num2 + 1 >= num)
		{
			return 50;
		}
		return (int)(RenderPassEventsEnumValues.values[num2 + 1] - renderPassEvent);
	}
}
