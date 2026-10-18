using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Unity.Collections;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.VFX;

namespace UnityEngine.Rendering.Universal;

public abstract class ScriptableRenderer : IDisposable
{
	private static class Profiling
	{
		public static class RenderBlock
		{
			private const string k_Name = "RenderPassBlock";

			public static readonly ProfilingSampler beforeRendering = new ProfilingSampler("RenderPassBlock.BeforeRendering");

			public static readonly ProfilingSampler mainRenderingOpaque = new ProfilingSampler("RenderPassBlock.MainRenderingOpaque");

			public static readonly ProfilingSampler mainRenderingTransparent = new ProfilingSampler("RenderPassBlock.MainRenderingTransparent");

			public static readonly ProfilingSampler afterRendering = new ProfilingSampler("RenderPassBlock.AfterRendering");
		}

		public static class RenderPass
		{
			private const string k_Name = "ScriptableRenderPass";

			public static readonly ProfilingSampler configure = new ProfilingSampler("ScriptableRenderPass.Configure");

			public static readonly ProfilingSampler setRenderPassAttachments = new ProfilingSampler("ScriptableRenderPass.SetRenderPassAttachments");
		}

		public static readonly ProfilingSampler setMRTAttachmentsList = new ProfilingSampler("NativeRenderPass SetNativeRenderPassMRTAttachmentList");

		public static readonly ProfilingSampler setAttachmentList = new ProfilingSampler("NativeRenderPass SetNativeRenderPassAttachmentList");

		public static readonly ProfilingSampler execute = new ProfilingSampler("NativeRenderPass ExecuteNativeRenderPass");

		public static readonly ProfilingSampler setupFrameData = new ProfilingSampler("NativeRenderPass SetupNativeRenderPassFrameData");

		private const string k_Name = "ScriptableRenderer";

		public static readonly ProfilingSampler setPerCameraShaderVariables = new ProfilingSampler("ScriptableRenderer.SetPerCameraShaderVariables");

		public static readonly ProfilingSampler sortRenderPasses = new ProfilingSampler("Sort Render Passes");

		public static readonly ProfilingSampler recordRenderGraph = new ProfilingSampler("On Record Render Graph");

		public static readonly ProfilingSampler setupLights = new ProfilingSampler("ScriptableRenderer.SetupLights");

		public static readonly ProfilingSampler setupCamera = new ProfilingSampler("Setup Camera Properties");

		public static readonly ProfilingSampler vfxProcessCamera = new ProfilingSampler("VFX Process Camera");

		public static readonly ProfilingSampler addRenderPasses = new ProfilingSampler("ScriptableRenderer.AddRenderPasses");

		public static readonly ProfilingSampler setupRenderPasses = new ProfilingSampler("ScriptableRenderer.SetupRenderPasses");

		public static readonly ProfilingSampler clearRenderingState = new ProfilingSampler("ScriptableRenderer.ClearRenderingState");

		public static readonly ProfilingSampler internalStartRendering = new ProfilingSampler("ScriptableRenderer.InternalStartRendering");

		public static readonly ProfilingSampler internalFinishRenderingCommon = new ProfilingSampler("ScriptableRenderer.InternalFinishRenderingCommon");

		public static readonly ProfilingSampler drawGizmos = new ProfilingSampler("DrawGizmos");

		public static readonly ProfilingSampler drawWireOverlay = new ProfilingSampler("DrawWireOverlay");

		internal static readonly ProfilingSampler beginXRRendering = new ProfilingSampler("Begin XR Rendering");

		internal static readonly ProfilingSampler endXRRendering = new ProfilingSampler("End XR Rendering");

		internal static readonly ProfilingSampler initRenderGraphFrame = new ProfilingSampler("Initialize Frame");

		internal static readonly ProfilingSampler setEditorTarget = new ProfilingSampler("Set Editor Target");
	}

	internal struct RenderPassDescriptor
	{
		internal int w;

		internal int h;

		internal int samples;

		internal int depthID;

		internal RenderPassDescriptor(int width, int height, int sampleCount, int rtID)
		{
			w = width;
			h = height;
			samples = sampleCount;
			depthID = rtID;
		}
	}

	public class RenderingFeatures
	{
		[Obsolete("cameraStacking has been deprecated use SupportedCameraRenderTypes() in ScriptableRenderer instead.", true)]
		public bool cameraStacking { get; set; }

		public bool msaa { get; set; } = true;

	}

	private static class RenderPassBlock
	{
		public static readonly int BeforeRendering = 0;

		public static readonly int MainRenderingOpaque = 1;

		public static readonly int MainRenderingTransparent = 2;

		public static readonly int AfterRendering = 3;
	}

	private class VFXProcessCameraPassData
	{
		internal UniversalRenderingData renderingData;

		internal Camera camera;

		internal VFXCameraXRSettings cameraXRSettings;

		internal XRPass xrPass;
	}

	private class DrawGizmosPassData
	{
		public RendererListHandle gizmoRenderList;

		public TextureHandle color;

		public TextureHandle depth;
	}

	private class DrawWireOverlayPassData
	{
		public RendererListHandle wireOverlayList;
	}

	private class BeginXRPassData
	{
		internal UniversalCameraData cameraData;
	}

	private class EndXRPassData
	{
		public UniversalCameraData cameraData;
	}

	private class DummyData
	{
	}

	private class PassData
	{
		internal ScriptableRenderer renderer;

		internal UniversalCameraData cameraData;

		internal bool isTargetBackbuffer;

		internal Vector2Int cameraTargetSizeCopy;
	}

	internal struct RenderBlocks : IDisposable
	{
		public struct BlockRange : IDisposable
		{
			private int m_Current;

			private int m_End;

			public int Current => m_Current;

			public BlockRange(int begin, int end)
			{
				m_Current = ((begin < end) ? begin : end);
				m_End = ((end >= begin) ? end : begin);
				m_Current--;
			}

			public BlockRange GetEnumerator()
			{
				return this;
			}

			public bool MoveNext()
			{
				return ++m_Current < m_End;
			}

			public void Dispose()
			{
			}
		}

		private NativeArray<RenderPassEvent> m_BlockEventLimits;

		private NativeArray<int> m_BlockRanges;

		private NativeArray<int> m_BlockRangeLengths;

		public RenderBlocks(List<ScriptableRenderPass> activeRenderPassQueue)
		{
			m_BlockEventLimits = new NativeArray<RenderPassEvent>(4, Allocator.Temp);
			m_BlockRanges = new NativeArray<int>(m_BlockEventLimits.Length + 1, Allocator.Temp);
			m_BlockRangeLengths = new NativeArray<int>(m_BlockRanges.Length, Allocator.Temp);
			m_BlockEventLimits[RenderPassBlock.BeforeRendering] = RenderPassEvent.BeforeRenderingPrePasses;
			m_BlockEventLimits[RenderPassBlock.MainRenderingOpaque] = RenderPassEvent.AfterRenderingOpaques;
			m_BlockEventLimits[RenderPassBlock.MainRenderingTransparent] = RenderPassEvent.AfterRenderingPostProcessing;
			m_BlockEventLimits[RenderPassBlock.AfterRendering] = (RenderPassEvent)2147483647;
			FillBlockRanges(activeRenderPassQueue);
			m_BlockEventLimits.Dispose();
			for (int i = 0; i < m_BlockRanges.Length - 1; i++)
			{
				m_BlockRangeLengths[i] = m_BlockRanges[i + 1] - m_BlockRanges[i];
			}
		}

		public void Dispose()
		{
			m_BlockRangeLengths.Dispose();
			m_BlockRanges.Dispose();
		}

		private void FillBlockRanges(List<ScriptableRenderPass> activeRenderPassQueue)
		{
			int index = 0;
			int i = 0;
			m_BlockRanges[index++] = 0;
			for (int j = 0; j < m_BlockEventLimits.Length - 1; j++)
			{
				for (; i < activeRenderPassQueue.Count && activeRenderPassQueue[i].renderPassEvent < m_BlockEventLimits[j]; i++)
				{
				}
				m_BlockRanges[index++] = i;
			}
			m_BlockRanges[index] = activeRenderPassQueue.Count;
		}

		public int GetLength(int index)
		{
			return m_BlockRangeLengths[index];
		}

		public BlockRange GetRange(int index)
		{
			return new BlockRange(m_BlockRanges[index], m_BlockRanges[index + 1]);
		}
	}

	internal const int kRenderPassMapSize = 10;

	internal const int kRenderPassMaxCount = 20;

	private int m_LastBeginSubpassPassIndex;

	private Dictionary<Hash128, int[]> m_MergeableRenderPassesMap = new Dictionary<Hash128, int[]>(10);

	private int[][] m_MergeableRenderPassesMapArrays;

	private Hash128[] m_PassIndexToPassHash = new Hash128[20];

	private Dictionary<Hash128, int> m_RenderPassesAttachmentCount = new Dictionary<Hash128, int>(10);

	private int m_firstPassIndexOfLastMergeableGroup;

	private AttachmentDescriptor[] m_ActiveColorAttachmentDescriptors = new AttachmentDescriptor[8]
	{
		RenderingUtils.emptyAttachment,
		RenderingUtils.emptyAttachment,
		RenderingUtils.emptyAttachment,
		RenderingUtils.emptyAttachment,
		RenderingUtils.emptyAttachment,
		RenderingUtils.emptyAttachment,
		RenderingUtils.emptyAttachment,
		RenderingUtils.emptyAttachment
	};

	private AttachmentDescriptor m_ActiveDepthAttachmentDescriptor;

	private bool[] m_IsActiveColorAttachmentTransient = new bool[8];

	internal RenderBufferStoreAction[] m_FinalColorStoreAction = new RenderBufferStoreAction[8];

	internal RenderBufferStoreAction m_FinalDepthStoreAction;

	internal bool hasReleasedRTs = true;

	internal static ScriptableRenderer current = null;

	private StoreActionsOptimization m_StoreActionsOptimizationSetting;

	private static bool m_UseOptimizedStoreActions = false;

	private const int k_RenderPassBlockCount = 4;

	protected static readonly RTHandle k_CameraTarget = RTHandles.Alloc(BuiltinRenderTextureType.CameraTarget);

	private List<ScriptableRenderPass> m_ActiveRenderPassQueue = new List<ScriptableRenderPass>(32);

	private List<ScriptableRendererFeature> m_RendererFeatures = new List<ScriptableRendererFeature>(10);

	private RTHandle m_CameraColorTarget;

	private RTHandle m_CameraDepthTarget;

	private RTHandle m_CameraResolveTarget;

	private bool m_FirstTimeCameraColorTargetIsBound = true;

	private bool m_FirstTimeCameraDepthTargetIsBound = true;

	private bool m_IsPipelineExecuting;

	internal bool disableNativeRenderPassInFeatures;

	internal bool useRenderPassEnabled;

	private static RenderTargetIdentifier[] m_ActiveColorAttachmentIDs = new RenderTargetIdentifier[8];

	private static RTHandle[] m_ActiveColorAttachments = new RTHandle[8];

	private static RTHandle m_ActiveDepthAttachment;

	private ContextContainer m_frameData = new ContextContainer();

	private static RenderBufferStoreAction[] m_ActiveColorStoreActions = new RenderBufferStoreAction[8];

	private static RenderBufferStoreAction m_ActiveDepthStoreAction = RenderBufferStoreAction.Store;

	private static RenderTargetIdentifier[][] m_TrimmedColorAttachmentCopyIDs = new RenderTargetIdentifier[9][]
	{
		Array.Empty<RenderTargetIdentifier>(),
		new RenderTargetIdentifier[1],
		new RenderTargetIdentifier[2],
		new RenderTargetIdentifier[3],
		new RenderTargetIdentifier[4],
		new RenderTargetIdentifier[5],
		new RenderTargetIdentifier[6],
		new RenderTargetIdentifier[7],
		new RenderTargetIdentifier[8]
	};

	private static RTHandle[][] m_TrimmedColorAttachmentCopies = new RTHandle[9][]
	{
		Array.Empty<RTHandle>(),
		new RTHandle[1],
		new RTHandle[2],
		new RTHandle[3],
		new RTHandle[4],
		new RTHandle[5],
		new RTHandle[6],
		new RTHandle[7],
		new RTHandle[8]
	};

	private static Plane[] s_Planes = new Plane[6];

	private static Vector4[] s_VectorPlanes = new Vector4[6];

	[Obsolete("cameraDepth has been renamed to cameraDepthTarget. (UnityUpgradable) -> cameraDepthTarget", true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public RenderTargetIdentifier cameraDepth => m_CameraDepthTarget.nameID;

	protected ProfilingSampler profilingExecute { get; set; }

	internal DebugHandler DebugHandler { get; }

	[Obsolete("Use cameraColorTargetHandle", true)]
	public RenderTargetIdentifier cameraColorTarget
	{
		get
		{
			throw new NotSupportedException("cameraColorTarget has been deprecated. Use cameraColorTargetHandle instead");
		}
	}

	[Obsolete("This rendering path is for compatibility mode only (when Render Graph is disabled). Use Render Graph API instead.", false)]
	public RTHandle cameraColorTargetHandle
	{
		get
		{
			if (!m_IsPipelineExecuting)
			{
				Debug.LogError("You can only call cameraColorTargetHandle inside the scope of a ScriptableRenderPass. Otherwise the pipeline camera target texture might have not been created or might have already been disposed.");
				return null;
			}
			return m_CameraColorTarget;
		}
	}

	[Obsolete("Use cameraDepthTargetHandle", true)]
	public RenderTargetIdentifier cameraDepthTarget
	{
		get
		{
			throw new NotSupportedException("cameraDepthTarget has been deprecated. Use cameraDepthTargetHandle instead");
		}
	}

	[Obsolete("This rendering path is for compatibility mode only (when Render Graph is disabled). Use Render Graph API instead.", false)]
	public RTHandle cameraDepthTargetHandle
	{
		get
		{
			if (!m_IsPipelineExecuting)
			{
				Debug.LogError("You can only call cameraDepthTargetHandle inside the scope of a ScriptableRenderPass. Otherwise the pipeline camera target texture might have not been created or might have already been disposed.");
				return null;
			}
			return m_CameraDepthTarget;
		}
	}

	protected List<ScriptableRendererFeature> rendererFeatures => m_RendererFeatures;

	protected List<ScriptableRenderPass> activeRenderPassQueue => m_ActiveRenderPassQueue;

	public RenderingFeatures supportedRenderingFeatures { get; set; } = new RenderingFeatures();


	public GraphicsDeviceType[] unsupportedGraphicsDeviceTypes { get; set; } = new GraphicsDeviceType[0];


	internal ContextContainer frameData => m_frameData;

	internal bool useDepthPriming { get; set; }

	internal bool stripShadowsOffVariants { get; set; }

	internal bool stripAdditionalLightOffVariants { get; set; }

	internal virtual bool supportsNativeRenderPassRendergraphCompiler => false;

	public virtual bool supportsGPUOcclusion => false;

	internal void ResetNativeRenderPassFrameData()
	{
		if (m_MergeableRenderPassesMapArrays == null)
		{
			m_MergeableRenderPassesMapArrays = new int[10][];
		}
		for (int i = 0; i < 10; i++)
		{
			if (m_MergeableRenderPassesMapArrays[i] == null)
			{
				m_MergeableRenderPassesMapArrays[i] = new int[20];
			}
			for (int j = 0; j < 20; j++)
			{
				m_MergeableRenderPassesMapArrays[i][j] = -1;
			}
		}
		m_firstPassIndexOfLastMergeableGroup = 0;
	}

	internal void SetupNativeRenderPassFrameData(UniversalCameraData cameraData, bool isRenderPassEnabled)
	{
		using (new ProfilingScope(Profiling.setupFrameData))
		{
			_ = m_ActiveRenderPassQueue.Count;
			m_MergeableRenderPassesMap.Clear();
			m_RenderPassesAttachmentCount.Clear();
			uint num = 0u;
			for (int i = 0; i < m_ActiveRenderPassQueue.Count; i++)
			{
				ScriptableRenderPass scriptableRenderPass = m_ActiveRenderPassQueue[i];
				if (IsRenderPassEnabled(scriptableRenderPass))
				{
					if (i >= 20)
					{
						Debug.LogError($"Exceeded the maximum number of Render Passes (${20}). Please consider using Render Graph to support a higher number of render passes with Native RenderPass, note support will be enabled by default.");
						return;
					}
					scriptableRenderPass.renderPassQueueIndex = i;
					RenderPassDescriptor desc = InitializeRenderPassDescriptor(cameraData, scriptableRenderPass);
					Hash128 hash = CreateRenderPassHash(desc, num);
					m_PassIndexToPassHash[i] = hash;
					if (!m_MergeableRenderPassesMap.ContainsKey(hash))
					{
						m_MergeableRenderPassesMap.Add(hash, m_MergeableRenderPassesMapArrays[m_MergeableRenderPassesMap.Count]);
						m_RenderPassesAttachmentCount.Add(hash, 0);
						m_firstPassIndexOfLastMergeableGroup = i;
					}
					else if (m_MergeableRenderPassesMap[hash][GetValidPassIndexCount(m_MergeableRenderPassesMap[hash]) - 1] != i - 1)
					{
						num++;
						hash = CreateRenderPassHash(desc, num);
						m_PassIndexToPassHash[i] = hash;
						m_MergeableRenderPassesMap.Add(hash, m_MergeableRenderPassesMapArrays[m_MergeableRenderPassesMap.Count]);
						m_RenderPassesAttachmentCount.Add(hash, 0);
						m_firstPassIndexOfLastMergeableGroup = i;
					}
					m_MergeableRenderPassesMap[hash][GetValidPassIndexCount(m_MergeableRenderPassesMap[hash])] = i;
				}
			}
			for (int j = 0; j < m_ActiveRenderPassQueue.Count; j++)
			{
				m_ActiveRenderPassQueue[j].m_ColorAttachmentIndices = new NativeArray<int>(8, Allocator.Temp);
				m_ActiveRenderPassQueue[j].m_InputAttachmentIndices = new NativeArray<int>(8, Allocator.Temp);
			}
		}
	}

	internal void UpdateFinalStoreActions(int[] currentMergeablePasses, UniversalCameraData cameraData, bool isLastMergeableGroup)
	{
		for (int i = 0; i < m_FinalColorStoreAction.Length; i++)
		{
			m_FinalColorStoreAction[i] = RenderBufferStoreAction.Store;
		}
		m_FinalDepthStoreAction = RenderBufferStoreAction.Store;
		foreach (int num in currentMergeablePasses)
		{
			if (!m_UseOptimizedStoreActions || num == -1)
			{
				break;
			}
			ScriptableRenderPass scriptableRenderPass = m_ActiveRenderPassQueue[num];
			int num2 = (scriptableRenderPass.overrideCameraTarget ? GetFirstAllocatedRTHandle(scriptableRenderPass).rt.descriptor.msaaSamples : ((cameraData.targetTexture != null) ? cameraData.targetTexture.descriptor.msaaSamples : cameraData.cameraTargetDescriptor.msaaSamples));
			bool flag = cameraData.renderer != null && cameraData.renderer.supportedRenderingFeatures.msaa;
			if (!cameraData.camera.allowMSAA || !flag)
			{
				num2 = 1;
			}
			for (int k = 0; k < m_FinalColorStoreAction.Length; k++)
			{
				if (m_FinalColorStoreAction[k] == RenderBufferStoreAction.Store || m_FinalColorStoreAction[k] == RenderBufferStoreAction.StoreAndResolve || scriptableRenderPass.overriddenColorStoreActions[k])
				{
					m_FinalColorStoreAction[k] = scriptableRenderPass.colorStoreActions[k];
				}
				if (num2 > 1)
				{
					if (m_FinalColorStoreAction[k] == RenderBufferStoreAction.Store)
					{
						m_FinalColorStoreAction[k] = RenderBufferStoreAction.StoreAndResolve;
					}
					else if (m_FinalColorStoreAction[k] == RenderBufferStoreAction.DontCare)
					{
						m_FinalColorStoreAction[k] = RenderBufferStoreAction.Resolve;
					}
					else if (isLastMergeableGroup && m_FinalColorStoreAction[k] == RenderBufferStoreAction.Resolve)
					{
						m_FinalColorStoreAction[k] = RenderBufferStoreAction.StoreAndResolve;
					}
				}
			}
			if (m_FinalDepthStoreAction == RenderBufferStoreAction.Store || (m_FinalDepthStoreAction == RenderBufferStoreAction.StoreAndResolve && scriptableRenderPass.depthStoreAction == RenderBufferStoreAction.Resolve) || scriptableRenderPass.overriddenDepthStoreAction)
			{
				m_FinalDepthStoreAction = scriptableRenderPass.depthStoreAction;
			}
		}
	}

	internal void SetNativeRenderPassMRTAttachmentList(ScriptableRenderPass renderPass, UniversalCameraData cameraData, bool needCustomCameraColorClear, ClearFlag cameraClearFlag)
	{
		using (new ProfilingScope(Profiling.setMRTAttachmentsList))
		{
			int renderPassQueueIndex = renderPass.renderPassQueueIndex;
			Hash128 key = m_PassIndexToPassHash[renderPassQueueIndex];
			int[] array = m_MergeableRenderPassesMap[key];
			if (array.First() != renderPassQueueIndex)
			{
				return;
			}
			m_RenderPassesAttachmentCount[key] = 0;
			UpdateFinalStoreActions(array, cameraData, renderPassQueueIndex == m_firstPassIndexOfLastMergeableGroup);
			int num = 0;
			bool flag = false;
			int[] array2 = array;
			foreach (int num2 in array2)
			{
				if (num2 == -1)
				{
					break;
				}
				ScriptableRenderPass scriptableRenderPass = m_ActiveRenderPassQueue[num2];
				for (int j = 0; j < scriptableRenderPass.m_ColorAttachmentIndices.Length; j++)
				{
					scriptableRenderPass.m_ColorAttachmentIndices[j] = -1;
				}
				for (int k = 0; k < scriptableRenderPass.m_InputAttachmentIndices.Length; k++)
				{
					scriptableRenderPass.m_InputAttachmentIndices[k] = -1;
				}
				uint validColorBufferCount = RenderingUtils.GetValidColorBufferCount(scriptableRenderPass.colorAttachmentHandles);
				for (int l = 0; l < validColorBufferCount; l++)
				{
					AttachmentDescriptor attachmentDescriptor = new AttachmentDescriptor((scriptableRenderPass.renderTargetFormat[l] != 0) ? scriptableRenderPass.renderTargetFormat[l] : UniversalRenderPipeline.MakeRenderTextureGraphicsFormat(cameraData.isHdrEnabled, cameraData.hdrColorBufferPrecision, Graphics.preserveFramebufferAlpha));
					RTHandle rTHandle = (scriptableRenderPass.overrideCameraTarget ? scriptableRenderPass.colorAttachmentHandles[l] : m_CameraColorTarget);
					int num3 = FindAttachmentDescriptorIndexInList(rTHandle.nameID, m_ActiveColorAttachmentDescriptors);
					if (m_UseOptimizedStoreActions)
					{
						attachmentDescriptor.storeAction = m_FinalColorStoreAction[l];
					}
					if (num3 == -1)
					{
						m_ActiveColorAttachmentDescriptors[num] = attachmentDescriptor;
						bool flag2 = (scriptableRenderPass.clearFlag & ClearFlag.Color) != 0;
						m_ActiveColorAttachmentDescriptors[num].ConfigureTarget(rTHandle.nameID, !flag2, storeResults: true);
						if (scriptableRenderPass.colorAttachmentHandles[l].nameID == m_CameraColorTarget.nameID && needCustomCameraColorClear && (cameraClearFlag & ClearFlag.Color) != 0)
						{
							m_ActiveColorAttachmentDescriptors[num].ConfigureClear(cameraData.backgroundColor);
						}
						else if (flag2)
						{
							m_ActiveColorAttachmentDescriptors[num].ConfigureClear(CoreUtils.ConvertSRGBToActiveColorSpace(scriptableRenderPass.clearColor));
						}
						scriptableRenderPass.m_ColorAttachmentIndices[l] = num;
						num++;
						m_RenderPassesAttachmentCount[key]++;
					}
					else
					{
						scriptableRenderPass.m_ColorAttachmentIndices[l] = num3;
					}
				}
				if (PassHasInputAttachments(scriptableRenderPass))
				{
					flag = true;
					SetupInputAttachmentIndices(scriptableRenderPass);
				}
				m_ActiveDepthAttachmentDescriptor = new AttachmentDescriptor(SystemInfo.GetGraphicsFormat(DefaultFormat.DepthStencil));
				bool flag3 = (cameraClearFlag & ClearFlag.DepthStencil) != 0;
				m_ActiveDepthAttachmentDescriptor.ConfigureTarget(scriptableRenderPass.overrideCameraTarget ? scriptableRenderPass.depthAttachmentHandle.nameID : m_CameraDepthTarget.nameID, !flag3, storeResults: true);
				if (flag3)
				{
					m_ActiveDepthAttachmentDescriptor.ConfigureClear(Color.black);
				}
				if (m_UseOptimizedStoreActions)
				{
					m_ActiveDepthAttachmentDescriptor.storeAction = m_FinalDepthStoreAction;
				}
			}
			if (flag)
			{
				SetupTransientInputAttachments(m_RenderPassesAttachmentCount[key]);
			}
		}
	}

	private bool IsDepthOnlyRenderTexture(RenderTexture t)
	{
		if (t.graphicsFormat == GraphicsFormat.None)
		{
			return true;
		}
		return false;
	}

	internal void SetNativeRenderPassAttachmentList(ScriptableRenderPass renderPass, UniversalCameraData cameraData, RTHandle passColorAttachment, RTHandle passDepthAttachment, ClearFlag finalClearFlag, Color finalClearColor)
	{
		using (new ProfilingScope(Profiling.setAttachmentList))
		{
			int renderPassQueueIndex = renderPass.renderPassQueueIndex;
			Hash128 key = m_PassIndexToPassHash[renderPassQueueIndex];
			int[] array = m_MergeableRenderPassesMap[key];
			if (array.First() != renderPassQueueIndex)
			{
				return;
			}
			m_RenderPassesAttachmentCount[key] = 0;
			UpdateFinalStoreActions(array, cameraData, renderPassQueueIndex == m_firstPassIndexOfLastMergeableGroup);
			int num = 0;
			int[] array2 = array;
			foreach (int num2 in array2)
			{
				if (num2 == -1)
				{
					break;
				}
				ScriptableRenderPass scriptableRenderPass = m_ActiveRenderPassQueue[num2];
				for (int j = 0; j < scriptableRenderPass.m_ColorAttachmentIndices.Length; j++)
				{
					scriptableRenderPass.m_ColorAttachmentIndices[j] = -1;
				}
				bool flag = cameraData.targetTexture != null;
				bool flag2 = (scriptableRenderPass.colorAttachmentHandle.rt != null && IsDepthOnlyRenderTexture(scriptableRenderPass.colorAttachmentHandle.rt)) || (flag && IsDepthOnlyRenderTexture(cameraData.targetTexture));
				AttachmentDescriptor attachmentDescriptor;
				int msaaSamples;
				RenderTargetIdentifier target;
				if (new RenderTargetIdentifier(passColorAttachment.nameID, 0) != BuiltinRenderTextureType.CameraTarget)
				{
					attachmentDescriptor = new AttachmentDescriptor(flag2 ? passColorAttachment.rt.descriptor.depthStencilFormat : passColorAttachment.rt.descriptor.graphicsFormat);
					msaaSamples = passColorAttachment.rt.descriptor.msaaSamples;
					target = passColorAttachment.nameID;
				}
				else
				{
					attachmentDescriptor = new AttachmentDescriptor((scriptableRenderPass.renderTargetFormat[0] != 0) ? scriptableRenderPass.renderTargetFormat[0] : UniversalRenderPipeline.MakeRenderTextureGraphicsFormat(cameraData.isHdrEnabled, cameraData.hdrColorBufferPrecision, Graphics.preserveFramebufferAlpha));
					msaaSamples = cameraData.cameraTargetDescriptor.msaaSamples;
					target = (flag ? new RenderTargetIdentifier(cameraData.targetTexture) : ((RenderTargetIdentifier)BuiltinRenderTextureType.CameraTarget));
				}
				attachmentDescriptor.ConfigureTarget(target, (finalClearFlag & ClearFlag.Color) == 0, storeResults: true);
				if (PassHasInputAttachments(scriptableRenderPass))
				{
					SetupInputAttachmentIndices(scriptableRenderPass);
				}
				m_ActiveDepthAttachmentDescriptor = new AttachmentDescriptor(SystemInfo.GetGraphicsFormat(DefaultFormat.DepthStencil));
				m_ActiveDepthAttachmentDescriptor.ConfigureTarget((passDepthAttachment.nameID != BuiltinRenderTextureType.CameraTarget) ? passDepthAttachment.nameID : (flag ? new RenderTargetIdentifier(cameraData.targetTexture.depthBuffer) : ((RenderTargetIdentifier)BuiltinRenderTextureType.Depth)), (finalClearFlag & ClearFlag.Depth) == 0, storeResults: true);
				if (finalClearFlag != 0)
				{
					if (cameraData.renderType != CameraRenderType.Overlay || (flag2 && (finalClearFlag & ClearFlag.Color) != 0))
					{
						attachmentDescriptor.ConfigureClear(finalClearColor);
					}
					if ((finalClearFlag & ClearFlag.Depth) != 0)
					{
						m_ActiveDepthAttachmentDescriptor.ConfigureClear(Color.black);
					}
				}
				if (msaaSamples > 1)
				{
					attachmentDescriptor.ConfigureResolveTarget(target);
					if (RenderingUtils.MultisampleDepthResolveSupported())
					{
						m_ActiveDepthAttachmentDescriptor.ConfigureResolveTarget(m_ActiveDepthAttachmentDescriptor.loadStoreTarget);
					}
				}
				if (m_UseOptimizedStoreActions)
				{
					attachmentDescriptor.storeAction = m_FinalColorStoreAction[0];
					m_ActiveDepthAttachmentDescriptor.storeAction = m_FinalDepthStoreAction;
				}
				int num3 = FindAttachmentDescriptorIndexInList(num, attachmentDescriptor, m_ActiveColorAttachmentDescriptors);
				if (num3 == -1)
				{
					scriptableRenderPass.m_ColorAttachmentIndices[0] = num;
					m_ActiveColorAttachmentDescriptors[num] = attachmentDescriptor;
					num++;
					m_RenderPassesAttachmentCount[key]++;
				}
				else
				{
					scriptableRenderPass.m_ColorAttachmentIndices[0] = num3;
				}
			}
		}
	}

	internal void ExecuteNativeRenderPass(ScriptableRenderContext context, ScriptableRenderPass renderPass, UniversalCameraData cameraData, ref RenderingData renderingData)
	{
		using (new ProfilingScope(Profiling.execute))
		{
			int renderPassQueueIndex = renderPass.renderPassQueueIndex;
			Hash128 key = m_PassIndexToPassHash[renderPassQueueIndex];
			int[] array = m_MergeableRenderPassesMap[key];
			int num = m_RenderPassesAttachmentCount[key];
			bool flag = (renderPass.colorAttachmentHandle.rt != null && IsDepthOnlyRenderTexture(renderPass.colorAttachmentHandle.rt)) || (cameraData.targetTexture != null && IsDepthOnlyRenderTexture(cameraData.targetTexture));
			bool flag2 = flag || !renderPass.overrideCameraTarget || (renderPass.overrideCameraTarget && renderPass.depthAttachmentHandle.nameID != BuiltinRenderTextureType.CameraTarget);
			NativeArray<AttachmentDescriptor> attachments = new NativeArray<AttachmentDescriptor>((!flag2 || flag) ? 1 : (num + 1), Allocator.Temp);
			for (int i = 0; i < num; i++)
			{
				attachments[i] = m_ActiveColorAttachmentDescriptors[i];
			}
			if (flag2 && !flag)
			{
				attachments[num] = m_ActiveDepthAttachmentDescriptor;
			}
			RenderPassDescriptor renderPassDescriptor = InitializeRenderPassDescriptor(cameraData, renderPass);
			int validPassIndexCount = GetValidPassIndexCount(array);
			uint subPassAttachmentIndicesCount = GetSubPassAttachmentIndicesCount(renderPass);
			NativeArray<int> colors = new NativeArray<int>((int)((!flag) ? subPassAttachmentIndicesCount : 0), Allocator.Temp);
			if (!flag)
			{
				for (int j = 0; j < subPassAttachmentIndicesCount; j++)
				{
					colors[j] = renderPass.m_ColorAttachmentIndices[j];
				}
			}
			if (validPassIndexCount == 1 || array[0] == renderPassQueueIndex)
			{
				if (PassHasInputAttachments(renderPass))
				{
					Debug.LogWarning("First pass in a RenderPass should not have input attachments.");
				}
				context.BeginRenderPass(renderPassDescriptor.w, renderPassDescriptor.h, Math.Max(renderPassDescriptor.samples, 1), attachments, (!flag2) ? (-1) : ((!flag) ? num : 0));
				attachments.Dispose();
				context.BeginSubPass(colors);
				m_LastBeginSubpassPassIndex = renderPassQueueIndex;
			}
			else if (!AreAttachmentIndicesCompatible(m_ActiveRenderPassQueue[m_LastBeginSubpassPassIndex], m_ActiveRenderPassQueue[renderPassQueueIndex]))
			{
				context.EndSubPass();
				if (PassHasInputAttachments(m_ActiveRenderPassQueue[renderPassQueueIndex]))
				{
					context.BeginSubPass(colors, m_ActiveRenderPassQueue[renderPassQueueIndex].m_InputAttachmentIndices);
				}
				else
				{
					context.BeginSubPass(colors);
				}
				m_LastBeginSubpassPassIndex = renderPassQueueIndex;
			}
			else if (PassHasInputAttachments(m_ActiveRenderPassQueue[renderPassQueueIndex]))
			{
				context.EndSubPass();
				context.BeginSubPass(colors, m_ActiveRenderPassQueue[renderPassQueueIndex].m_InputAttachmentIndices);
				m_LastBeginSubpassPassIndex = renderPassQueueIndex;
			}
			colors.Dispose();
			renderPass.Execute(context, ref renderingData);
			context.ExecuteCommandBuffer(renderingData.commandBuffer);
			renderingData.commandBuffer.Clear();
			if (validPassIndexCount == 1 || array[validPassIndexCount - 1] == renderPassQueueIndex)
			{
				context.EndSubPass();
				context.EndRenderPass();
				m_LastBeginSubpassPassIndex = 0;
			}
			for (int k = 0; k < m_ActiveColorAttachmentDescriptors.Length; k++)
			{
				m_ActiveColorAttachmentDescriptors[k] = RenderingUtils.emptyAttachment;
				m_IsActiveColorAttachmentTransient[k] = false;
			}
			m_ActiveDepthAttachmentDescriptor = RenderingUtils.emptyAttachment;
		}
	}

	internal void SetupInputAttachmentIndices(ScriptableRenderPass pass)
	{
		int validInputAttachmentCount = GetValidInputAttachmentCount(pass);
		pass.m_InputAttachmentIndices = new NativeArray<int>(validInputAttachmentCount, Allocator.Temp);
		for (int i = 0; i < validInputAttachmentCount; i++)
		{
			pass.m_InputAttachmentIndices[i] = FindAttachmentDescriptorIndexInList(pass.m_InputAttachments[i], m_ActiveColorAttachmentDescriptors);
			if (pass.m_InputAttachmentIndices[i] == -1)
			{
				Debug.LogWarning("RenderPass Input attachment not found in the current RenderPass");
			}
			else if (!m_IsActiveColorAttachmentTransient[pass.m_InputAttachmentIndices[i]])
			{
				m_IsActiveColorAttachmentTransient[pass.m_InputAttachmentIndices[i]] = pass.IsInputAttachmentTransient(i);
			}
		}
	}

	internal void SetupTransientInputAttachments(int attachmentCount)
	{
		for (int i = 0; i < attachmentCount; i++)
		{
			if (m_IsActiveColorAttachmentTransient[i])
			{
				m_ActiveColorAttachmentDescriptors[i].loadAction = RenderBufferLoadAction.DontCare;
				m_ActiveColorAttachmentDescriptors[i].storeAction = RenderBufferStoreAction.DontCare;
				m_ActiveColorAttachmentDescriptors[i].loadStoreTarget = BuiltinRenderTextureType.None;
			}
		}
	}

	internal static uint GetSubPassAttachmentIndicesCount(ScriptableRenderPass pass)
	{
		uint num = 0u;
		foreach (int colorAttachmentIndex in pass.m_ColorAttachmentIndices)
		{
			if (colorAttachmentIndex >= 0)
			{
				num++;
			}
		}
		return num;
	}

	internal static bool AreAttachmentIndicesCompatible(ScriptableRenderPass lastSubPass, ScriptableRenderPass currentSubPass)
	{
		uint subPassAttachmentIndicesCount = GetSubPassAttachmentIndicesCount(lastSubPass);
		uint subPassAttachmentIndicesCount2 = GetSubPassAttachmentIndicesCount(currentSubPass);
		if (subPassAttachmentIndicesCount2 != subPassAttachmentIndicesCount)
		{
			return false;
		}
		uint num = 0u;
		for (int i = 0; i < subPassAttachmentIndicesCount2; i++)
		{
			for (int j = 0; j < subPassAttachmentIndicesCount; j++)
			{
				if (currentSubPass.m_ColorAttachmentIndices[i] == lastSubPass.m_ColorAttachmentIndices[j])
				{
					num++;
				}
			}
		}
		return num == subPassAttachmentIndicesCount2;
	}

	internal static uint GetValidColorAttachmentCount(AttachmentDescriptor[] colorAttachments)
	{
		uint num = 0u;
		if (colorAttachments != null)
		{
			for (int i = 0; i < colorAttachments.Length; i++)
			{
				if (colorAttachments[i] != RenderingUtils.emptyAttachment)
				{
					num++;
				}
			}
		}
		return num;
	}

	internal static int GetValidInputAttachmentCount(ScriptableRenderPass renderPass)
	{
		int num = renderPass.m_InputAttachments.Length;
		if (num != 8)
		{
			return num;
		}
		for (int i = 0; i < num; i++)
		{
			if (renderPass.m_InputAttachments[i] == null)
			{
				return i;
			}
		}
		return num;
	}

	internal static int FindAttachmentDescriptorIndexInList(int attachmentIdx, AttachmentDescriptor attachmentDescriptor, AttachmentDescriptor[] attachmentDescriptors)
	{
		int result = -1;
		for (int i = 0; i <= attachmentIdx; i++)
		{
			AttachmentDescriptor attachmentDescriptor2 = attachmentDescriptors[i];
			if (attachmentDescriptor2.loadStoreTarget == attachmentDescriptor.loadStoreTarget && attachmentDescriptor2.graphicsFormat == attachmentDescriptor.graphicsFormat)
			{
				result = i;
				break;
			}
		}
		return result;
	}

	internal static int FindAttachmentDescriptorIndexInList(RenderTargetIdentifier target, AttachmentDescriptor[] attachmentDescriptors)
	{
		for (int i = 0; i < attachmentDescriptors.Length; i++)
		{
			AttachmentDescriptor attachmentDescriptor = attachmentDescriptors[i];
			if (attachmentDescriptor.loadStoreTarget == target)
			{
				return i;
			}
		}
		return -1;
	}

	internal static int GetValidPassIndexCount(int[] array)
	{
		if (array == null)
		{
			return 0;
		}
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] == -1)
			{
				return i;
			}
		}
		return array.Length - 1;
	}

	internal static RTHandle GetFirstAllocatedRTHandle(ScriptableRenderPass pass)
	{
		for (int i = 0; i < pass.colorAttachmentHandles.Length; i++)
		{
			if (pass.colorAttachmentHandles[i].rt != null)
			{
				return pass.colorAttachmentHandles[i];
			}
		}
		return pass.colorAttachmentHandles[0];
	}

	internal static bool PassHasInputAttachments(ScriptableRenderPass renderPass)
	{
		if (renderPass.m_InputAttachments.Length == 8)
		{
			return renderPass.m_InputAttachments[0] != null;
		}
		return true;
	}

	internal static Hash128 CreateRenderPassHash(int width, int height, int depthID, int sample, uint hashIndex)
	{
		return new Hash128((uint)((width << 4) + height), (uint)depthID, (uint)sample, hashIndex);
	}

	internal static Hash128 CreateRenderPassHash(RenderPassDescriptor desc, uint hashIndex)
	{
		return CreateRenderPassHash(desc.w, desc.h, desc.depthID, desc.samples, hashIndex);
	}

	internal static void GetRenderTextureDescriptor(UniversalCameraData cameraData, ScriptableRenderPass renderPass, out RenderTextureDescriptor targetRT)
	{
		if (!renderPass.overrideCameraTarget || (renderPass.colorAttachmentHandle.rt == null && renderPass.depthAttachmentHandle.rt == null))
		{
			targetRT = cameraData.cameraTargetDescriptor;
			if (cameraData.targetTexture != null)
			{
				targetRT.width = cameraData.scaledWidth;
				targetRT.height = cameraData.scaledHeight;
			}
		}
		else
		{
			RTHandle firstAllocatedRTHandle = GetFirstAllocatedRTHandle(renderPass);
			targetRT = ((firstAllocatedRTHandle.rt != null) ? firstAllocatedRTHandle.rt.descriptor : renderPass.depthAttachmentHandle.rt.descriptor);
		}
	}

	private RenderPassDescriptor InitializeRenderPassDescriptor(UniversalCameraData cameraData, ScriptableRenderPass renderPass)
	{
		GetRenderTextureDescriptor(cameraData, renderPass, out var targetRT);
		RTHandle rTHandle = (renderPass.overrideCameraTarget ? renderPass.depthAttachmentHandle : cameraDepthTargetHandle);
		int rtID = ((targetRT.graphicsFormat == GraphicsFormat.None && targetRT.depthStencilFormat != 0) ? renderPass.colorAttachmentHandle.GetHashCode() : rTHandle.GetHashCode());
		return new RenderPassDescriptor(targetRT.width, targetRT.height, targetRT.msaaSamples, rtID);
	}

	public virtual int SupportedCameraStackingTypes()
	{
		return 0;
	}

	public bool SupportsCameraStackingType(CameraRenderType cameraRenderType)
	{
		return (SupportedCameraStackingTypes() & (1 << (int)cameraRenderType)) != 0;
	}

	protected internal virtual bool SupportsMotionVectors()
	{
		return false;
	}

	protected internal virtual bool SupportsCameraOpaque()
	{
		return false;
	}

	protected internal virtual bool SupportsCameraNormals()
	{
		return false;
	}

	public static void SetCameraMatrices(CommandBuffer cmd, ref CameraData cameraData, bool setInverseMatrices)
	{
		SetCameraMatrices(CommandBufferHelpers.GetRasterCommandBuffer(cmd), cameraData.universalCameraData, setInverseMatrices, cameraData.IsCameraProjectionMatrixFlipped());
	}

	public static void SetCameraMatrices(CommandBuffer cmd, UniversalCameraData cameraData, bool setInverseMatrices)
	{
		SetCameraMatrices(CommandBufferHelpers.GetRasterCommandBuffer(cmd), cameraData, setInverseMatrices, cameraData.IsCameraProjectionMatrixFlipped());
	}

	internal static void SetCameraMatrices(RasterCommandBuffer cmd, UniversalCameraData cameraData, bool setInverseMatrices, bool isTargetFlipped)
	{
		if (cameraData.xr.enabled)
		{
			cameraData.PushBuiltinShaderConstantsXR(cmd, isTargetFlipped);
			XRSystemUniversal.MarkShaderProperties(cmd, cameraData.xrUniversal, isTargetFlipped);
			return;
		}
		Matrix4x4 viewMatrix = cameraData.GetViewMatrix();
		Matrix4x4 projectionMatrix = cameraData.GetProjectionMatrix();
		cmd.SetViewProjectionMatrices(viewMatrix, projectionMatrix);
		if (setInverseMatrices)
		{
			Matrix4x4 gPUProjectionMatrix = cameraData.GetGPUProjectionMatrix(isTargetFlipped);
			Matrix4x4 matrix4x = Matrix4x4.Inverse(viewMatrix);
			Matrix4x4 matrix4x2 = Matrix4x4.Inverse(gPUProjectionMatrix);
			Matrix4x4 value = matrix4x * matrix4x2;
			Matrix4x4 value2 = Matrix4x4.Scale(new Vector3(1f, 1f, -1f)) * viewMatrix;
			Matrix4x4 inverse = value2.inverse;
			cmd.SetGlobalMatrix(ShaderPropertyId.worldToCameraMatrix, value2);
			cmd.SetGlobalMatrix(ShaderPropertyId.cameraToWorldMatrix, inverse);
			cmd.SetGlobalMatrix(ShaderPropertyId.inverseViewMatrix, matrix4x);
			cmd.SetGlobalMatrix(ShaderPropertyId.inverseProjectionMatrix, matrix4x2);
			cmd.SetGlobalMatrix(ShaderPropertyId.inverseViewAndProjectionMatrix, value);
		}
	}

	private void SetPerCameraShaderVariables(RasterCommandBuffer cmd, UniversalCameraData cameraData)
	{
		SetPerCameraShaderVariables(cmd, cameraData, new Vector2Int(cameraData.cameraTargetDescriptor.width, cameraData.cameraTargetDescriptor.height), cameraData.IsCameraProjectionMatrixFlipped());
	}

	private void SetPerCameraShaderVariables(RasterCommandBuffer cmd, UniversalCameraData cameraData, Vector2Int cameraTargetSizeCopy, bool isTargetFlipped)
	{
		using (new ProfilingScope(Profiling.setPerCameraShaderVariables))
		{
			Camera camera = cameraData.camera;
			float num = cameraTargetSizeCopy.x;
			float num2 = cameraTargetSizeCopy.y;
			float num3 = camera.pixelWidth;
			float num4 = camera.pixelHeight;
			if (cameraData.renderType == CameraRenderType.Overlay)
			{
				num3 = cameraData.pixelWidth;
				num4 = cameraData.pixelHeight;
			}
			if (cameraData.xr.enabled)
			{
				num3 = cameraTargetSizeCopy.x;
				num4 = cameraTargetSizeCopy.y;
				useRenderPassEnabled = false;
			}
			if (camera.allowDynamicResolution)
			{
				if (cameraData.xr.enabled)
				{
					num = cameraData.xr.renderTargetScaledWidth;
					num2 = cameraData.xr.renderTargetScaledHeight;
				}
				else
				{
					num *= ScalableBufferManager.widthScaleFactor;
					num2 *= ScalableBufferManager.heightScaleFactor;
				}
			}
			float nearClipPlane = camera.nearClipPlane;
			float farClipPlane = camera.farClipPlane;
			float num5 = (Mathf.Approximately(nearClipPlane, 0f) ? 0f : (1f / nearClipPlane));
			float num6 = (Mathf.Approximately(farClipPlane, 0f) ? 0f : (1f / farClipPlane));
			float w = (camera.orthographic ? 1f : 0f);
			float num7 = 1f - farClipPlane * num5;
			float num8 = farClipPlane * num5;
			Vector4 value = new Vector4(num7, num8, num7 * num6, num8 * num6);
			if (SystemInfo.usesReversedZBuffer)
			{
				value.y += value.x;
				value.x = 0f - value.x;
				value.w += value.z;
				value.z = 0f - value.z;
			}
			if (cameraData.renderType == CameraRenderType.Overlay)
			{
				float x = (isTargetFlipped ? (-1f) : 1f);
				cmd.SetGlobalVector(value: new Vector4(x, nearClipPlane, farClipPlane, 1f * num6), nameID: ShaderPropertyId.projectionParams);
			}
			Vector4 value3 = new Vector4(camera.orthographicSize * cameraData.aspectRatio, camera.orthographicSize, 0f, w);
			cmd.SetGlobalVector(ShaderPropertyId.worldSpaceCameraPos, cameraData.worldSpaceCameraPos);
			cmd.SetGlobalVector(ShaderPropertyId.screenParams, new Vector4(num3, num4, 1f + 1f / num3, 1f + 1f / num4));
			cmd.SetGlobalVector(ShaderPropertyId.scaledScreenParams, new Vector4(num, num2, 1f + 1f / num, 1f + 1f / num2));
			cmd.SetGlobalVector(ShaderPropertyId.zBufferParams, value);
			cmd.SetGlobalVector(ShaderPropertyId.orthoParams, value3);
			cmd.SetGlobalVector(ShaderPropertyId.screenSize, new Vector4(num, num2, 1f / num, 1f / num2));
			cmd.SetKeyword(in ShaderGlobalKeywords.SCREEN_COORD_OVERRIDE, cameraData.useScreenCoordOverride);
			cmd.SetGlobalVector(ShaderPropertyId.screenSizeOverride, cameraData.screenSizeOverride);
			cmd.SetGlobalVector(ShaderPropertyId.screenCoordScaleBias, cameraData.screenCoordScaleBias);
			cmd.SetGlobalVector(ShaderPropertyId.rtHandleScale, Vector4.one);
			float val = Math.Min((float)(0.0 - Math.Log(num3 / num, 2.0)), 0f);
			float val2 = Math.Min(cameraData.taaSettings.mipBias, 0f);
			val = Math.Min(val, val2);
			cmd.SetGlobalVector(ShaderPropertyId.globalMipBias, new Vector2(val, Mathf.Pow(2f, val)));
			SetCameraMatrices(cmd, cameraData, setInverseMatrices: true, isTargetFlipped);
		}
	}

	private void SetPerCameraBillboardProperties(RasterCommandBuffer cmd, UniversalCameraData cameraData)
	{
		Matrix4x4 worldToCameraMatrix = cameraData.GetViewMatrix();
		Vector3 worldSpaceCameraPos = cameraData.worldSpaceCameraPos;
		cmd.SetKeyword(in ShaderGlobalKeywords.BillboardFaceCameraPos, QualitySettings.billboardsFaceCameraPosition);
		CalculateBillboardProperties(in worldToCameraMatrix, out var billboardTangent, out var billboardNormal, out var cameraXZAngle);
		cmd.SetGlobalVector(ShaderPropertyId.billboardNormal, new Vector4(billboardNormal.x, billboardNormal.y, billboardNormal.z, 0f));
		cmd.SetGlobalVector(ShaderPropertyId.billboardTangent, new Vector4(billboardTangent.x, billboardTangent.y, billboardTangent.z, 0f));
		cmd.SetGlobalVector(ShaderPropertyId.billboardCameraParams, new Vector4(worldSpaceCameraPos.x, worldSpaceCameraPos.y, worldSpaceCameraPos.z, cameraXZAngle));
	}

	private static void CalculateBillboardProperties(in Matrix4x4 worldToCameraMatrix, out Vector3 billboardTangent, out Vector3 billboardNormal, out float cameraXZAngle)
	{
		Matrix4x4 transpose = worldToCameraMatrix.transpose;
		Vector3 vector = new Vector3(transpose.m00, transpose.m10, transpose.m20);
		Vector3 vector2 = new Vector3(transpose.m01, transpose.m11, transpose.m21);
		Vector3 lhs = new Vector3(transpose.m02, transpose.m12, transpose.m22);
		Vector3 up = Vector3.up;
		Vector3 vector3 = Vector3.Cross(lhs, up);
		billboardTangent = ((!Mathf.Approximately(vector3.sqrMagnitude, 0f)) ? vector3.normalized : vector);
		billboardNormal = Vector3.Cross(up, billboardTangent);
		billboardNormal = ((!Mathf.Approximately(billboardNormal.sqrMagnitude, 0f)) ? billboardNormal.normalized : vector2);
		Vector3 vector4 = new Vector3(0f, 0f, 1f);
		float y = vector4.x * billboardTangent.z - vector4.z * billboardTangent.x;
		float x = vector4.x * billboardTangent.x + vector4.z * billboardTangent.z;
		cameraXZAngle = Mathf.Atan2(y, x);
		if (cameraXZAngle < 0f)
		{
			cameraXZAngle += MathF.PI * 2f;
		}
	}

	private void SetPerCameraClippingPlaneProperties(RasterCommandBuffer cmd, UniversalCameraData cameraData)
	{
		SetPerCameraClippingPlaneProperties(cmd, in cameraData, cameraData.IsCameraProjectionMatrixFlipped());
	}

	private void SetPerCameraClippingPlaneProperties(RasterCommandBuffer cmd, in UniversalCameraData cameraData, bool isTargetFlipped)
	{
		Matrix4x4 gPUProjectionMatrix = cameraData.GetGPUProjectionMatrix(isTargetFlipped);
		Matrix4x4 viewMatrix = cameraData.GetViewMatrix();
		Matrix4x4 worldToProjectionMatrix = CoreMatrixUtils.MultiplyProjectionMatrix(gPUProjectionMatrix, viewMatrix, cameraData.camera.orthographic);
		Plane[] array = s_Planes;
		GeometryUtility.CalculateFrustumPlanes(worldToProjectionMatrix, array);
		Vector4[] array2 = s_VectorPlanes;
		for (int i = 0; i < array.Length; i++)
		{
			array2[i] = new Vector4(array[i].normal.x, array[i].normal.y, array[i].normal.z, array[i].distance);
		}
		cmd.SetGlobalVectorArray(ShaderPropertyId.cameraWorldClipPlanes, array2);
	}

	private static void SetShaderTimeValues(IBaseCommandBuffer cmd, float time, float deltaTime, float smoothDeltaTime)
	{
		float f = time / 8f;
		float f2 = time / 4f;
		float f3 = time / 2f;
		float num = time - ShaderUtils.PersistentDeltaTime;
		Vector4 value = time * new Vector4(0.05f, 1f, 2f, 3f);
		Vector4 value2 = new Vector4(Mathf.Sin(f), Mathf.Sin(f2), Mathf.Sin(f3), Mathf.Sin(time));
		Vector4 value3 = new Vector4(Mathf.Cos(f), Mathf.Cos(f2), Mathf.Cos(f3), Mathf.Cos(time));
		Vector4 value4 = new Vector4(deltaTime, 1f / deltaTime, smoothDeltaTime, 1f / smoothDeltaTime);
		Vector4 value5 = new Vector4(time, Mathf.Sin(time), Mathf.Cos(time), 0f);
		Vector4 value6 = new Vector4(num, Mathf.Sin(num), Mathf.Cos(num), 0f);
		cmd.SetGlobalVector(ShaderPropertyId.time, value);
		cmd.SetGlobalVector(ShaderPropertyId.sinTime, value2);
		cmd.SetGlobalVector(ShaderPropertyId.cosTime, value3);
		cmd.SetGlobalVector(ShaderPropertyId.deltaTime, value4);
		cmd.SetGlobalVector(ShaderPropertyId.timeParameters, value5);
		cmd.SetGlobalVector(ShaderPropertyId.lastTimeParameters, value6);
	}

	[Obsolete("This rendering path is for compatibility mode only (when Render Graph is disabled). Use Render Graph API instead.", false)]
	internal virtual RTHandle GetCameraColorFrontBuffer(CommandBuffer cmd)
	{
		return null;
	}

	[Obsolete("This rendering path is for compatibility mode only (when Render Graph is disabled). Use Render Graph API instead.", false)]
	internal virtual RTHandle GetCameraColorBackBuffer(CommandBuffer cmd)
	{
		return null;
	}

	public ScriptableRenderer(ScriptableRendererData data)
	{
		profilingExecute = new ProfilingSampler("ScriptableRenderer.Execute: " + data.name);
		foreach (ScriptableRendererFeature rendererFeature in data.rendererFeatures)
		{
			if (!(rendererFeature == null))
			{
				rendererFeature.Create();
				m_RendererFeatures.Add(rendererFeature);
			}
		}
		ResetNativeRenderPassFrameData();
		useRenderPassEnabled = data.useNativeRenderPass;
		Clear(CameraRenderType.Base);
		m_ActiveRenderPassQueue.Clear();
		if ((bool)UniversalRenderPipeline.asset)
		{
			m_StoreActionsOptimizationSetting = UniversalRenderPipeline.asset.storeActionsOptimization;
		}
		m_UseOptimizedStoreActions = m_StoreActionsOptimizationSetting != StoreActionsOptimization.Store;
	}

	public void Dispose()
	{
		for (int i = 0; i < m_RendererFeatures.Count; i++)
		{
			if (!(rendererFeatures[i] == null))
			{
				try
				{
					rendererFeatures[i].Dispose();
				}
				catch (Exception exception)
				{
					Debug.LogException(exception);
				}
			}
		}
		Dispose(disposing: true);
		hasReleasedRTs = true;
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		DebugHandler?.Dispose();
	}

	internal virtual void ReleaseRenderTargets()
	{
	}

	[Obsolete("Use RTHandles for colorTarget and depthTarget", true)]
	public void ConfigureCameraTarget(RenderTargetIdentifier colorTarget, RenderTargetIdentifier depthTarget)
	{
		throw new NotSupportedException("ConfigureCameraTarget with RenderTargetIdentifier has been deprecated. Use it with RTHandles instead");
	}

	[Obsolete("This rendering path is for compatibility mode only (when Render Graph is disabled). Use Render Graph API instead.", false)]
	public void ConfigureCameraTarget(RTHandle colorTarget, RTHandle depthTarget)
	{
		m_CameraColorTarget = colorTarget;
		m_CameraDepthTarget = depthTarget;
	}

	[Obsolete("This rendering path is for compatibility mode only (when Render Graph is disabled). Use Render Graph API instead.", false)]
	internal void ConfigureCameraTarget(RTHandle colorTarget, RTHandle depthTarget, RTHandle resolveTarget)
	{
		m_CameraColorTarget = colorTarget;
		m_CameraDepthTarget = depthTarget;
		m_CameraResolveTarget = resolveTarget;
	}

	[Obsolete("This rendering path is for compatibility mode only (when Render Graph is disabled). Use Render Graph API instead.", false)]
	internal void ConfigureCameraColorTarget(RTHandle colorTarget)
	{
		m_CameraColorTarget = colorTarget;
	}

	[Obsolete("This rendering path is for compatibility mode only (when Render Graph is disabled). Use Render Graph API instead.", false)]
	public abstract void Setup(ScriptableRenderContext context, ref RenderingData renderingData);

	[Obsolete("This rendering path is for compatibility mode only (when Render Graph is disabled). Use Render Graph API instead.", false)]
	public virtual void SetupLights(ScriptableRenderContext context, ref RenderingData renderingData)
	{
	}

	public virtual void SetupCullingParameters(ref ScriptableCullingParameters cullingParameters, ref CameraData cameraData)
	{
	}

	public virtual void FinishRendering(CommandBuffer cmd)
	{
	}

	public virtual void OnBeginRenderGraphFrame()
	{
	}

	internal virtual void OnRecordRenderGraph(RenderGraph renderGraph, ScriptableRenderContext context)
	{
	}

	public virtual void OnEndRenderGraphFrame()
	{
	}

	private void InitRenderGraphFrame(RenderGraph renderGraph)
	{
		PassData passData;
		using IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder = renderGraph.AddUnsafePass<PassData>(Profiling.initRenderGraphFrame.name, out passData, Profiling.initRenderGraphFrame, ".\\Library\\PackageCache\\com.unity.render-pipelines.universal@bc6f352be672\\Runtime\\ScriptableRenderer.cs", 914);
		passData.renderer = this;
		unsafeRenderGraphBuilder.AllowPassCulling(value: false);
		unsafeRenderGraphBuilder.SetRenderFunc(delegate(PassData data, UnsafeGraphContext rgContext)
		{
			UnsafeCommandBuffer cmd = rgContext.cmd;
			float time = Time.time;
			float deltaTime = Time.deltaTime;
			float smoothDeltaTime = Time.smoothDeltaTime;
			ClearRenderingState(cmd);
			SetShaderTimeValues(cmd, time, deltaTime, smoothDeltaTime);
		});
	}

	internal void ProcessVFXCameraCommand(RenderGraph renderGraph)
	{
		UniversalRenderingData renderingData = frameData.Get<UniversalRenderingData>();
		UniversalCameraData universalCameraData = frameData.Get<UniversalCameraData>();
		XRPass xr = universalCameraData.xr;
		VFXProcessCameraPassData passData;
		using IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder = renderGraph.AddUnsafePass<VFXProcessCameraPassData>("ProcessVFXCameraCommand", out passData, Profiling.vfxProcessCamera, ".\\Library\\PackageCache\\com.unity.render-pipelines.universal@bc6f352be672\\Runtime\\ScriptableRenderer.cs", 952);
		passData.camera = universalCameraData.camera;
		passData.renderingData = renderingData;
		passData.cameraXRSettings.viewTotal = ((!xr.enabled) ? 1u : 2u);
		passData.cameraXRSettings.viewCount = ((!xr.enabled) ? 1u : ((uint)xr.viewCount));
		passData.cameraXRSettings.viewOffset = (uint)xr.multipassId;
		passData.xrPass = (xr.enabled ? xr : null);
		unsafeRenderGraphBuilder.AllowPassCulling(value: false);
		unsafeRenderGraphBuilder.SetRenderFunc(delegate(VFXProcessCameraPassData data, UnsafeGraphContext context)
		{
			if (data.xrPass != null)
			{
				data.xrPass.StartSinglePass(context.cmd);
			}
			CommandBufferHelpers.VFXManager_ProcessCameraCommand(data.camera, context.cmd, data.cameraXRSettings, data.renderingData.cullResults);
			if (data.xrPass != null)
			{
				data.xrPass.StopSinglePass(context.cmd);
			}
		});
	}

	internal void SetupRenderGraphCameraProperties(RenderGraph renderGraph, bool isTargetBackbuffer)
	{
		PassData passData;
		using IRasterRenderGraphBuilder rasterRenderGraphBuilder = renderGraph.AddRasterRenderPass<PassData>(Profiling.setupCamera.name, out passData, Profiling.setupCamera, ".\\Library\\PackageCache\\com.unity.render-pipelines.universal@bc6f352be672\\Runtime\\ScriptableRenderer.cs", 981);
		passData.renderer = this;
		passData.cameraData = frameData.Get<UniversalCameraData>();
		passData.cameraTargetSizeCopy = new Vector2Int(passData.cameraData.cameraTargetDescriptor.width, passData.cameraData.cameraTargetDescriptor.height);
		passData.isTargetBackbuffer = isTargetBackbuffer;
		rasterRenderGraphBuilder.AllowGlobalStateModification(value: true);
		rasterRenderGraphBuilder.SetRenderFunc(delegate(PassData data, RasterGraphContext context)
		{
			bool flag = !SystemInfo.graphicsUVStartsAtTop || data.isTargetBackbuffer;
			if (data.cameraData.renderType == CameraRenderType.Base)
			{
				context.cmd.SetupCameraProperties(data.cameraData.camera);
				data.renderer.SetPerCameraShaderVariables(context.cmd, data.cameraData, data.cameraTargetSizeCopy, !flag);
			}
			else
			{
				data.renderer.SetPerCameraShaderVariables(context.cmd, data.cameraData, data.cameraTargetSizeCopy, !flag);
				data.renderer.SetPerCameraClippingPlaneProperties(context.cmd, in data.cameraData, !flag);
				data.renderer.SetPerCameraBillboardProperties(context.cmd, data.cameraData);
			}
			float time = Time.time;
			float deltaTime = Time.deltaTime;
			float smoothDeltaTime = Time.smoothDeltaTime;
			SetShaderTimeValues(context.cmd, time, deltaTime, smoothDeltaTime);
		});
	}

	internal void DrawRenderGraphGizmos(RenderGraph renderGraph, ContextContainer frameData, TextureHandle color, TextureHandle depth, GizmoSubset gizmoSubset)
	{
	}

	internal void DrawRenderGraphWireOverlay(RenderGraph renderGraph, ContextContainer frameData, TextureHandle color)
	{
	}

	internal void BeginRenderGraphXRRendering(RenderGraph renderGraph)
	{
		UniversalCameraData universalCameraData = frameData.Get<UniversalCameraData>();
		if (!universalCameraData.xr.enabled)
		{
			return;
		}
		bool flag = XRSystem.GetRenderViewportScale() == 1f;
		universalCameraData.xrUniversal.canFoveateIntermediatePasses = !PlatformAutoDetect.isXRMobile || flag;
		BeginXRPassData passData;
		using IRasterRenderGraphBuilder rasterRenderGraphBuilder = renderGraph.AddRasterRenderPass<BeginXRPassData>("BeginXRRendering", out passData, Profiling.beginXRRendering, ".\\Library\\PackageCache\\com.unity.render-pipelines.universal@bc6f352be672\\Runtime\\ScriptableRenderer.cs", 1129);
		passData.cameraData = universalCameraData;
		rasterRenderGraphBuilder.AllowGlobalStateModification(value: true);
		rasterRenderGraphBuilder.SetRenderFunc(delegate(BeginXRPassData data, RasterGraphContext context)
		{
			if (data.cameraData.xr.enabled)
			{
				if (data.cameraData.xrUniversal.isLateLatchEnabled)
				{
					data.cameraData.xrUniversal.canMarkLateLatch = true;
				}
				data.cameraData.xr.StartSinglePass(context.cmd);
				if (data.cameraData.xr.supportsFoveatedRendering)
				{
					context.cmd.ConfigureFoveatedRendering(data.cameraData.xr.foveatedRenderingInfo);
					if (XRSystem.foveatedRenderingCaps.HasFlag(FoveatedRenderingCaps.NonUniformRaster))
					{
						context.cmd.SetKeyword(in ShaderGlobalKeywords.FoveatedRenderingNonUniformRaster, value: true);
					}
				}
			}
		});
	}

	internal void EndRenderGraphXRRendering(RenderGraph renderGraph)
	{
		UniversalCameraData universalCameraData = frameData.Get<UniversalCameraData>();
		if (!universalCameraData.xr.enabled)
		{
			return;
		}
		EndXRPassData passData;
		using IRasterRenderGraphBuilder rasterRenderGraphBuilder = renderGraph.AddRasterRenderPass<EndXRPassData>("EndXRRendering", out passData, Profiling.endXRRendering, ".\\Library\\PackageCache\\com.unity.render-pipelines.universal@bc6f352be672\\Runtime\\ScriptableRenderer.cs", 1169);
		passData.cameraData = universalCameraData;
		rasterRenderGraphBuilder.AllowGlobalStateModification(value: true);
		rasterRenderGraphBuilder.SetRenderFunc(delegate(EndXRPassData data, RasterGraphContext context)
		{
			if (data.cameraData.xr.enabled)
			{
				data.cameraData.xr.StopSinglePass(context.cmd);
			}
			if (XRSystem.foveatedRenderingCaps != 0)
			{
				if (XRSystem.foveatedRenderingCaps.HasFlag(FoveatedRenderingCaps.NonUniformRaster))
				{
					context.cmd.SetKeyword(in ShaderGlobalKeywords.FoveatedRenderingNonUniformRaster, value: false);
				}
				context.cmd.ConfigureFoveatedRendering(IntPtr.Zero);
			}
		});
	}

	private void SetEditorTarget(RenderGraph renderGraph)
	{
		DummyData passData;
		using IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder = renderGraph.AddUnsafePass<DummyData>("SetEditorTarget", out passData, Profiling.setEditorTarget, ".\\Library\\PackageCache\\com.unity.render-pipelines.universal@bc6f352be672\\Runtime\\ScriptableRenderer.cs", 1201);
		unsafeRenderGraphBuilder.AllowPassCulling(value: false);
		unsafeRenderGraphBuilder.SetRenderFunc(delegate(DummyData data, UnsafeGraphContext context)
		{
			context.cmd.SetRenderTarget(BuiltinRenderTextureType.CameraTarget, RenderBufferLoadAction.Load, RenderBufferStoreAction.Store, RenderBufferLoadAction.Load, RenderBufferStoreAction.DontCare);
		});
	}

	internal void RecordRenderGraph(RenderGraph renderGraph, ScriptableRenderContext context)
	{
		using (new ProfilingScope(ProfilingSampler.Get(URPProfileId.RecordRenderGraph)))
		{
			OnBeginRenderGraphFrame();
			using (new ProfilingScope(Profiling.sortRenderPasses))
			{
				SortStable(m_ActiveRenderPassQueue);
			}
			InitRenderGraphFrame(renderGraph);
			using (new ProfilingScope(Profiling.recordRenderGraph))
			{
				OnRecordRenderGraph(renderGraph, context);
			}
			OnEndRenderGraphFrame();
		}
	}

	internal void FinishRenderGraphRendering(CommandBuffer cmd)
	{
		UniversalCameraData universalCameraData = frameData.Get<UniversalCameraData>();
		OnFinishRenderGraphRendering(cmd);
		InternalFinishRenderingCommon(cmd, universalCameraData.resolveFinalTarget);
	}

	internal virtual void OnFinishRenderGraphRendering(CommandBuffer cmd)
	{
	}

	internal void RecordCustomRenderGraphPassesInEventRange(RenderGraph renderGraph, RenderPassEvent eventStart, RenderPassEvent eventEnd)
	{
		if (eventStart == eventEnd)
		{
			return;
		}
		foreach (ScriptableRenderPass item in m_ActiveRenderPassQueue)
		{
			if (item.renderPassEvent >= eventStart && item.renderPassEvent < eventEnd)
			{
				item.RecordRenderGraph(renderGraph, m_frameData);
			}
		}
	}

	internal void CalculateSplitEventRange(RenderPassEvent startInjectionPoint, RenderPassEvent targetEvent, out RenderPassEvent startEvent, out RenderPassEvent splitEvent, out RenderPassEvent endEvent)
	{
		int renderPassEventRange = ScriptableRenderPass.GetRenderPassEventRange(startInjectionPoint);
		startEvent = startInjectionPoint;
		endEvent = startEvent + renderPassEventRange;
		splitEvent = (RenderPassEvent)Math.Clamp((int)targetEvent, (int)startEvent, (int)endEvent);
	}

	internal void RecordCustomRenderGraphPasses(RenderGraph renderGraph, RenderPassEvent startInjectionPoint, RenderPassEvent endInjectionPoint)
	{
		int renderPassEventRange = ScriptableRenderPass.GetRenderPassEventRange(endInjectionPoint);
		RecordCustomRenderGraphPassesInEventRange(renderGraph, startInjectionPoint, endInjectionPoint + renderPassEventRange);
	}

	internal void RecordCustomRenderGraphPasses(RenderGraph renderGraph, RenderPassEvent injectionPoint)
	{
		RecordCustomRenderGraphPasses(renderGraph, injectionPoint, injectionPoint);
	}

	internal void SetPerCameraProperties(ScriptableRenderContext context, UniversalCameraData cameraData, Camera camera, CommandBuffer cmd)
	{
		if (cameraData.renderType == CameraRenderType.Base)
		{
			context.SetupCameraProperties(camera);
			SetPerCameraShaderVariables(CommandBufferHelpers.GetRasterCommandBuffer(cmd), cameraData);
		}
		else
		{
			SetPerCameraShaderVariables(CommandBufferHelpers.GetRasterCommandBuffer(cmd), cameraData);
			SetPerCameraClippingPlaneProperties(CommandBufferHelpers.GetRasterCommandBuffer(cmd), cameraData);
			SetPerCameraBillboardProperties(CommandBufferHelpers.GetRasterCommandBuffer(cmd), cameraData);
		}
	}

	[Obsolete("This rendering path is for compatibility mode only (when Render Graph is disabled). Use Render Graph API instead.", false)]
	public void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		bool flag = DebugDisplaySettings<UniversalRenderPipelineDebugDisplaySettings>.Instance.renderingSettings.sceneOverrideMode == DebugSceneOverrideMode.None;
		hasReleasedRTs = false;
		m_IsPipelineExecuting = true;
		UniversalCameraData universalCameraData = renderingData.frameData.Get<UniversalCameraData>();
		Camera camera = universalCameraData.camera;
		if (rendererFeatures.Count != 0 && !renderingData.cameraData.isPreviewCamera)
		{
			SetupRenderPasses(in renderingData);
		}
		CommandBuffer commandBuffer = renderingData.commandBuffer;
		CommandBuffer cmd = (renderingData.cameraData.xr.enabled ? null : commandBuffer);
		using (new ProfilingScope(cmd, profilingExecute))
		{
			InternalStartRendering(context, ref renderingData);
			float time = Time.time;
			float deltaTime = Time.deltaTime;
			float smoothDeltaTime = Time.smoothDeltaTime;
			ClearRenderingState(CommandBufferHelpers.GetRasterCommandBuffer(commandBuffer));
			SetShaderTimeValues(CommandBufferHelpers.GetRasterCommandBuffer(commandBuffer), time, deltaTime, smoothDeltaTime);
			context.ExecuteCommandBuffer(commandBuffer);
			commandBuffer.Clear();
			using (new ProfilingScope(Profiling.sortRenderPasses))
			{
				SortStable(m_ActiveRenderPassQueue);
			}
			using (new ProfilingScope(Profiling.RenderPass.configure))
			{
				foreach (ScriptableRenderPass item in activeRenderPassQueue)
				{
					item.Configure(commandBuffer, universalCameraData.cameraTargetDescriptor);
				}
				context.ExecuteCommandBuffer(commandBuffer);
				commandBuffer.Clear();
			}
			SetupNativeRenderPassFrameData(universalCameraData, useRenderPassEnabled);
			RenderBlocks renderBlocks = new RenderBlocks(m_ActiveRenderPassQueue);
			try
			{
				using (new ProfilingScope(Profiling.setupLights))
				{
					SetupLights(context, ref renderingData);
				}
				using (new ProfilingScope(Profiling.setupCamera))
				{
					SetPerCameraProperties(context, universalCameraData, camera, commandBuffer);
					VFXCameraXRSettings camXRSettings = default(VFXCameraXRSettings);
					camXRSettings.viewTotal = ((!universalCameraData.xr.enabled) ? 1u : 2u);
					camXRSettings.viewCount = ((!universalCameraData.xr.enabled) ? 1u : ((uint)universalCameraData.xr.viewCount));
					camXRSettings.viewOffset = (uint)universalCameraData.xr.multipassId;
					if (universalCameraData.xr.enabled)
					{
						universalCameraData.xr.StartSinglePass(commandBuffer);
					}
					VFXManager.ProcessCameraCommand(camera, commandBuffer, camXRSettings, renderingData.cullResults);
					if (universalCameraData.xr.enabled)
					{
						universalCameraData.xr.StopSinglePass(commandBuffer);
					}
				}
				if (renderBlocks.GetLength(RenderPassBlock.BeforeRendering) > 0)
				{
					using (new ProfilingScope(Profiling.RenderBlock.beforeRendering))
					{
						ExecuteBlock(RenderPassBlock.BeforeRendering, in renderBlocks, context, ref renderingData);
					}
				}
				using (new ProfilingScope(Profiling.setupCamera))
				{
					SetPerCameraProperties(context, universalCameraData, camera, commandBuffer);
					SetShaderTimeValues(CommandBufferHelpers.GetRasterCommandBuffer(commandBuffer), time, deltaTime, smoothDeltaTime);
				}
				context.ExecuteCommandBuffer(commandBuffer);
				commandBuffer.Clear();
				BeginXRRendering(commandBuffer, context, ref renderingData.cameraData);
				if (renderBlocks.GetLength(RenderPassBlock.MainRenderingOpaque) > 0)
				{
					using (new ProfilingScope(Profiling.RenderBlock.mainRenderingOpaque))
					{
						ExecuteBlock(RenderPassBlock.MainRenderingOpaque, in renderBlocks, context, ref renderingData);
					}
				}
				if (renderBlocks.GetLength(RenderPassBlock.MainRenderingTransparent) > 0)
				{
					using (new ProfilingScope(Profiling.RenderBlock.mainRenderingTransparent))
					{
						ExecuteBlock(RenderPassBlock.MainRenderingTransparent, in renderBlocks, context, ref renderingData);
					}
				}
				if (universalCameraData.xr.enabled)
				{
					universalCameraData.xrUniversal.canMarkLateLatch = false;
				}
				if (renderBlocks.GetLength(RenderPassBlock.AfterRendering) > 0)
				{
					using (new ProfilingScope(Profiling.RenderBlock.afterRendering))
					{
						ExecuteBlock(RenderPassBlock.AfterRendering, in renderBlocks, context, ref renderingData);
					}
				}
				EndXRRendering(commandBuffer, context, ref renderingData.cameraData);
				InternalFinishRenderingExecute(context, commandBuffer, universalCameraData.resolveFinalTarget);
				for (int i = 0; i < m_ActiveRenderPassQueue.Count; i++)
				{
					m_ActiveRenderPassQueue[i].m_ColorAttachmentIndices.Dispose();
					m_ActiveRenderPassQueue[i].m_InputAttachmentIndices.Dispose();
				}
			}
			finally
			{
				((IDisposable)renderBlocks).Dispose();
			}
		}
		context.ExecuteCommandBuffer(commandBuffer);
		commandBuffer.Clear();
	}

	public void EnqueuePass(ScriptableRenderPass pass)
	{
		m_ActiveRenderPassQueue.Add(pass);
		if (disableNativeRenderPassInFeatures)
		{
			pass.useNativeRenderPass = false;
		}
	}

	protected static ClearFlag GetCameraClearFlag(ref CameraData cameraData)
	{
		return GetCameraClearFlag(cameraData.universalCameraData);
	}

	protected static ClearFlag GetCameraClearFlag(UniversalCameraData cameraData)
	{
		CameraClearFlags clearFlags = cameraData.camera.clearFlags;
		if (cameraData.renderType == CameraRenderType.Overlay)
		{
			if (!cameraData.clearDepth)
			{
				return ClearFlag.None;
			}
			return ClearFlag.DepthStencil;
		}
		DebugHandler debugHandler = cameraData.renderer.DebugHandler;
		if (debugHandler != null && debugHandler.IsActiveForCamera(cameraData.isPreviewCamera) && debugHandler.IsScreenClearNeeded)
		{
			return ClearFlag.All;
		}
		if (clearFlags == CameraClearFlags.Skybox && RenderSettings.skybox != null && cameraData.postProcessEnabled && cameraData.xr.enabled)
		{
			return ClearFlag.All;
		}
		if ((clearFlags == CameraClearFlags.Skybox && RenderSettings.skybox != null) || clearFlags == CameraClearFlags.Nothing)
		{
			if (cameraData.cameraTargetDescriptor.msaaSamples > 1)
			{
				cameraData.camera.backgroundColor = Color.black;
				return ClearFlag.All;
			}
			return ClearFlag.DepthStencil;
		}
		return ClearFlag.All;
	}

	internal void OnPreCullRenderPasses(in CameraData cameraData)
	{
		for (int i = 0; i < rendererFeatures.Count; i++)
		{
			if (rendererFeatures[i].isActive)
			{
				rendererFeatures[i].OnCameraPreCull(this, in cameraData);
			}
		}
	}

	internal void AddRenderPasses(ref RenderingData renderingData)
	{
		using (new ProfilingScope(Profiling.addRenderPasses))
		{
			for (int i = 0; i < rendererFeatures.Count; i++)
			{
				if (rendererFeatures[i].isActive)
				{
					if (!rendererFeatures[i].SupportsNativeRenderPass())
					{
						disableNativeRenderPassInFeatures = true;
					}
					rendererFeatures[i].AddRenderPasses(this, ref renderingData);
					disableNativeRenderPassInFeatures = false;
				}
			}
			int count = activeRenderPassQueue.Count;
			for (int num = count - 1; num >= 0; num--)
			{
				if (activeRenderPassQueue[num] == null)
				{
					activeRenderPassQueue.RemoveAt(num);
				}
			}
			if (count > 0 && m_StoreActionsOptimizationSetting == StoreActionsOptimization.Auto)
			{
				m_UseOptimizedStoreActions = false;
			}
		}
	}

	[Obsolete("This rendering path is for compatibility mode only (when Render Graph is disabled). Use Render Graph API instead.", false)]
	protected void SetupRenderPasses(in RenderingData renderingData)
	{
		using (new ProfilingScope(Profiling.setupRenderPasses))
		{
			for (int i = 0; i < rendererFeatures.Count; i++)
			{
				if (rendererFeatures[i].isActive)
				{
					rendererFeatures[i].SetupRenderPasses(this, in renderingData);
				}
			}
		}
	}

	private static void ClearRenderingState(IBaseCommandBuffer cmd)
	{
		using (new ProfilingScope(Profiling.clearRenderingState))
		{
			cmd.SetKeyword(in ShaderGlobalKeywords.MainLightShadows, value: false);
			cmd.SetKeyword(in ShaderGlobalKeywords.MainLightShadowCascades, value: false);
			cmd.SetKeyword(in ShaderGlobalKeywords.AdditionalLightsVertex, value: false);
			cmd.SetKeyword(in ShaderGlobalKeywords.AdditionalLightsPixel, value: false);
			cmd.SetKeyword(in ShaderGlobalKeywords.ClusterLightLoop, value: false);
			cmd.SetKeyword(in ShaderGlobalKeywords.ForwardPlus, value: false);
			cmd.SetKeyword(in ShaderGlobalKeywords.AdditionalLightShadows, value: false);
			cmd.SetKeyword(in ShaderGlobalKeywords.ReflectionProbeBlending, value: false);
			cmd.SetKeyword(in ShaderGlobalKeywords.ReflectionProbeBoxProjection, value: false);
			cmd.SetKeyword(in ShaderGlobalKeywords.ReflectionProbeAtlas, value: false);
			cmd.SetKeyword(in ShaderGlobalKeywords.SoftShadows, value: false);
			cmd.SetKeyword(in ShaderGlobalKeywords.SoftShadowsLow, value: false);
			cmd.SetKeyword(in ShaderGlobalKeywords.SoftShadowsMedium, value: false);
			cmd.SetKeyword(in ShaderGlobalKeywords.SoftShadowsHigh, value: false);
			cmd.SetKeyword(in ShaderGlobalKeywords.MixedLightingSubtractive, value: false);
			cmd.SetKeyword(in ShaderGlobalKeywords.LightmapShadowMixing, value: false);
			cmd.SetKeyword(in ShaderGlobalKeywords.ShadowsShadowMask, value: false);
			cmd.SetKeyword(in ShaderGlobalKeywords.LinearToSRGBConversion, value: false);
			cmd.SetKeyword(in ShaderGlobalKeywords.LightLayers, value: false);
			cmd.SetGlobalVector(ScreenSpaceAmbientOcclusionPass.s_AmbientOcclusionParamID, Vector4.zero);
		}
	}

	internal void Clear(CameraRenderType cameraType)
	{
		m_ActiveColorAttachments[0] = k_CameraTarget;
		for (int i = 1; i < m_ActiveColorAttachments.Length; i++)
		{
			m_ActiveColorAttachments[i] = null;
		}
		for (int j = 0; j < m_ActiveColorAttachments.Length; j++)
		{
			m_ActiveColorAttachmentIDs[j] = m_ActiveColorAttachments[j]?.nameID ?? ((RenderTargetIdentifier)0);
		}
		m_ActiveDepthAttachment = k_CameraTarget;
		m_FirstTimeCameraColorTargetIsBound = cameraType == CameraRenderType.Base;
		m_FirstTimeCameraDepthTargetIsBound = true;
		m_CameraColorTarget = null;
		m_CameraDepthTarget = null;
	}

	[Obsolete("This rendering path is for compatibility mode only (when Render Graph is disabled). Use Render Graph API instead.", false)]
	private void ExecuteBlock(int blockIndex, in RenderBlocks renderBlocks, ScriptableRenderContext context, ref RenderingData renderingData, bool submit = false)
	{
		UniversalCameraData cameraData = renderingData.frameData.Get<UniversalCameraData>();
		foreach (int item in renderBlocks.GetRange(blockIndex))
		{
			ScriptableRenderPass renderPass = m_ActiveRenderPassQueue[item];
			ExecuteRenderPass(context, renderPass, cameraData, ref renderingData);
		}
		if (submit)
		{
			context.Submit();
		}
	}

	[Obsolete("This rendering path is for compatibility mode only (when Render Graph is disabled). Use Render Graph API instead.", false)]
	private bool IsRenderPassEnabled(ScriptableRenderPass renderPass)
	{
		if (renderPass.useNativeRenderPass)
		{
			return useRenderPassEnabled;
		}
		return false;
	}

	[Obsolete("This rendering path is for compatibility mode only (when Render Graph is disabled). Use Render Graph API instead.", false)]
	private void ExecuteRenderPass(ScriptableRenderContext context, ScriptableRenderPass renderPass, UniversalCameraData cameraData, ref RenderingData renderingData)
	{
		using (new ProfilingScope(renderPass.profilingSampler))
		{
			CommandBuffer commandBuffer = renderingData.commandBuffer;
			if (cameraData.xr.supportsFoveatedRendering && ((renderPass.renderPassEvent >= RenderPassEvent.BeforeRenderingPrePasses && renderPass.renderPassEvent < RenderPassEvent.BeforeRenderingPostProcessing) || (renderPass.renderPassEvent > RenderPassEvent.AfterRendering && XRSystem.foveatedRenderingCaps.HasFlag(FoveatedRenderingCaps.FoveationImage))))
			{
				commandBuffer.SetFoveatedRenderingMode(FoveatedRenderingMode.Enabled);
			}
			using (new ProfilingScope(Profiling.RenderPass.setRenderPassAttachments))
			{
				SetRenderPassAttachments(commandBuffer, renderPass, cameraData);
			}
			context.ExecuteCommandBuffer(commandBuffer);
			commandBuffer.Clear();
			if (IsRenderPassEnabled(renderPass) && cameraData.isRenderPassSupportedCamera)
			{
				ExecuteNativeRenderPass(context, renderPass, cameraData, ref renderingData);
			}
			else
			{
				renderPass.Execute(context, ref renderingData);
				context.ExecuteCommandBuffer(commandBuffer);
				commandBuffer.Clear();
			}
			if (cameraData.xr.enabled)
			{
				if (cameraData.xr.supportsFoveatedRendering)
				{
					commandBuffer.SetFoveatedRenderingMode(FoveatedRenderingMode.Disabled);
				}
				XRSystemUniversal.UnmarkShaderProperties(CommandBufferHelpers.GetRasterCommandBuffer(commandBuffer), cameraData.xrUniversal);
				context.ExecuteCommandBuffer(commandBuffer);
				commandBuffer.Clear();
			}
		}
	}

	internal bool IsSceneFilteringEnabled(Camera camera)
	{
		return false;
	}

	[Obsolete("This rendering path is for compatibility mode only (when Render Graph is disabled). Use Render Graph API instead.", false)]
	private void SetRenderPassAttachments(CommandBuffer cmd, ScriptableRenderPass renderPass, UniversalCameraData cameraData)
	{
		Camera camera = cameraData.camera;
		ClearFlag cameraClearFlag = GetCameraClearFlag(cameraData);
		if (RenderingUtils.GetValidColorBufferCount(renderPass.colorAttachmentHandles) == 0)
		{
			return;
		}
		if (RenderingUtils.IsMRT(renderPass.colorAttachmentHandles))
		{
			bool flag = false;
			bool flag2 = false;
			int num = RenderingUtils.IndexOf(renderPass.colorAttachmentHandles, m_CameraColorTarget);
			if (num != -1 && m_FirstTimeCameraColorTargetIsBound)
			{
				m_FirstTimeCameraColorTargetIsBound = false;
				flag = (cameraClearFlag & ClearFlag.Color) != (renderPass.clearFlag & ClearFlag.Color) || cameraData.backgroundColor != renderPass.clearColor;
			}
			RenderTargetIdentifier renderTargetIdentifier = m_CameraDepthTarget.nameID;
			if (cameraData.xr.enabled)
			{
				renderTargetIdentifier = new RenderTargetIdentifier(renderTargetIdentifier, 0, CubemapFace.Unknown, -1);
			}
			if (new RenderTargetIdentifier(renderPass.depthAttachmentHandle.nameID, 0) == new RenderTargetIdentifier(renderTargetIdentifier, 0) && m_FirstTimeCameraDepthTargetIsBound)
			{
				m_FirstTimeCameraDepthTargetIsBound = false;
				flag2 = (cameraClearFlag & ClearFlag.DepthStencil) != (renderPass.clearFlag & ClearFlag.DepthStencil);
			}
			if (flag)
			{
				if ((cameraClearFlag & ClearFlag.Color) != 0 && (!IsRenderPassEnabled(renderPass) || !cameraData.isRenderPassSupportedCamera))
				{
					SetRenderTarget(cmd, renderPass.colorAttachmentHandles[num], renderPass.depthAttachmentHandle, ClearFlag.Color, cameraData.backgroundColor);
				}
				if ((renderPass.clearFlag & ClearFlag.Color) != 0)
				{
					uint num2 = RenderingUtils.CountDistinct(renderPass.colorAttachmentHandles, m_CameraColorTarget);
					RTHandle[] array = m_TrimmedColorAttachmentCopies[num2];
					int num3 = 0;
					for (int i = 0; i < renderPass.colorAttachmentHandles.Length; i++)
					{
						if (renderPass.colorAttachmentHandles[i] != null && renderPass.colorAttachmentHandles[i].nameID != 0 && renderPass.colorAttachmentHandles[i].nameID != m_CameraColorTarget.nameID)
						{
							array[num3] = renderPass.colorAttachmentHandles[i];
							num3++;
						}
					}
					RenderTargetIdentifier[] array2 = m_TrimmedColorAttachmentCopyIDs[num2];
					for (int j = 0; j < num2; j++)
					{
						array2[j] = array[j].nameID;
					}
					if (num3 != num2)
					{
						Debug.LogError("writeIndex and otherTargetsCount values differed. writeIndex:" + num3 + " otherTargetsCount:" + num2);
					}
					if (!IsRenderPassEnabled(renderPass) || !cameraData.isRenderPassSupportedCamera)
					{
						SetRenderTarget(cmd, array, array2, m_CameraDepthTarget, ClearFlag.Color, renderPass.clearColor);
					}
				}
			}
			ClearFlag clearFlag = ClearFlag.None;
			clearFlag |= (flag2 ? (cameraClearFlag & ClearFlag.DepthStencil) : (renderPass.clearFlag & ClearFlag.DepthStencil));
			clearFlag |= ((!flag) ? (renderPass.clearFlag & ClearFlag.Color) : (IsRenderPassEnabled(renderPass) ? (cameraClearFlag & ClearFlag.Color) : ClearFlag.None));
			if (IsRenderPassEnabled(renderPass) && cameraData.isRenderPassSupportedCamera)
			{
				SetNativeRenderPassMRTAttachmentList(renderPass, cameraData, flag, clearFlag);
			}
			if (RenderingUtils.SequenceEqual(renderPass.colorAttachmentHandles, m_ActiveColorAttachments) && !(renderPass.depthAttachmentHandle.nameID != m_ActiveDepthAttachment) && clearFlag == ClearFlag.None)
			{
				return;
			}
			int num4 = RenderingUtils.LastValid(renderPass.colorAttachmentHandles);
			if (num4 < 0)
			{
				return;
			}
			int num5 = num4 + 1;
			RTHandle[] array3 = m_TrimmedColorAttachmentCopies[num5];
			for (int k = 0; k < num5; k++)
			{
				array3[k] = renderPass.colorAttachmentHandles[k];
			}
			RenderTargetIdentifier[] array4 = m_TrimmedColorAttachmentCopyIDs[num5];
			for (int l = 0; l < num5; l++)
			{
				array4[l] = array3[l].nameID;
			}
			if (!IsRenderPassEnabled(renderPass) || !cameraData.isRenderPassSupportedCamera)
			{
				RTHandle depthAttachmentHandle = m_CameraDepthTarget;
				if (renderPass.overrideCameraTarget)
				{
					depthAttachmentHandle = renderPass.depthAttachmentHandle;
				}
				else
				{
					m_FirstTimeCameraDepthTargetIsBound = false;
				}
				SetRenderTarget(cmd, array3, array4, depthAttachmentHandle, clearFlag, renderPass.clearColor);
			}
			if (cameraData.xr.enabled)
			{
				bool renderIntoTexture = RenderingUtils.IndexOf(renderPass.colorAttachmentHandles, cameraData.xr.renderTarget) == -1;
				cameraData.PushBuiltinShaderConstantsXR(CommandBufferHelpers.GetRasterCommandBuffer(cmd), renderIntoTexture);
				XRSystemUniversal.MarkShaderProperties(CommandBufferHelpers.GetRasterCommandBuffer(cmd), cameraData.xrUniversal, renderIntoTexture);
			}
			return;
		}
		RTHandle colorAttachmentHandle = renderPass.colorAttachmentHandle;
		RTHandle depthAttachmentHandle2 = renderPass.depthAttachmentHandle;
		if (!renderPass.overrideCameraTarget)
		{
			if (renderPass.renderPassEvent < RenderPassEvent.BeforeRenderingPrePasses)
			{
				return;
			}
			colorAttachmentHandle = m_CameraColorTarget;
			depthAttachmentHandle2 = m_CameraDepthTarget;
		}
		ClearFlag clearFlag2 = ClearFlag.None;
		Color color;
		if (colorAttachmentHandle.nameID == m_CameraColorTarget.nameID && m_FirstTimeCameraColorTargetIsBound)
		{
			m_FirstTimeCameraColorTargetIsBound = false;
			clearFlag2 |= cameraClearFlag & ClearFlag.Color;
			if (SystemInfo.usesLoadStoreActions && new RenderTargetIdentifier(colorAttachmentHandle.nameID, 0) != BuiltinRenderTextureType.CameraTarget)
			{
				clearFlag2 |= renderPass.clearFlag;
			}
			color = cameraData.backgroundColor;
			if (m_FirstTimeCameraDepthTargetIsBound)
			{
				m_FirstTimeCameraDepthTargetIsBound = false;
				clearFlag2 |= cameraClearFlag & ClearFlag.DepthStencil;
			}
		}
		else
		{
			clearFlag2 |= renderPass.clearFlag & ClearFlag.Color;
			color = renderPass.clearColor;
		}
		if (new RenderTargetIdentifier(m_CameraDepthTarget.nameID, 0) != BuiltinRenderTextureType.CameraTarget && (depthAttachmentHandle2.nameID == m_CameraDepthTarget.nameID || colorAttachmentHandle.nameID == m_CameraDepthTarget.nameID) && m_FirstTimeCameraDepthTargetIsBound)
		{
			m_FirstTimeCameraDepthTargetIsBound = false;
			clearFlag2 |= cameraClearFlag & ClearFlag.DepthStencil;
		}
		else
		{
			clearFlag2 |= renderPass.clearFlag & ClearFlag.DepthStencil;
		}
		if (IsSceneFilteringEnabled(camera))
		{
			color.a = 0f;
			clearFlag2 &= ~ClearFlag.Depth;
		}
		if (DebugHandler != null && DebugHandler.IsActiveForCamera(cameraData.isPreviewCamera))
		{
			DebugHandler.TryGetScreenClearColor(ref color);
		}
		if (IsRenderPassEnabled(renderPass) && cameraData.isRenderPassSupportedCamera)
		{
			SetNativeRenderPassAttachmentList(renderPass, cameraData, colorAttachmentHandle, depthAttachmentHandle2, clearFlag2, color);
			return;
		}
		bool flag3 = false;
		if (colorAttachmentHandle.nameID != m_ActiveColorAttachments[0])
		{
			flag3 = true;
		}
		for (int m = 1; m < m_ActiveColorAttachments.Length; m++)
		{
			if (renderPass.colorAttachmentHandles[m] != m_ActiveColorAttachments[m])
			{
				flag3 = true;
				break;
			}
		}
		if (flag3 || depthAttachmentHandle2.nameID != m_ActiveDepthAttachment || clearFlag2 != 0 || renderPass.colorStoreActions[0] != m_ActiveColorStoreActions[0] || renderPass.depthStoreAction != m_ActiveDepthStoreAction)
		{
			SetRenderTarget(cmd, colorAttachmentHandle, depthAttachmentHandle2, clearFlag2, color, renderPass.colorStoreActions[0], renderPass.depthStoreAction);
			if (cameraData.xr.enabled)
			{
				bool renderIntoTexture2 = colorAttachmentHandle.nameID != cameraData.xr.renderTarget;
				cameraData.PushBuiltinShaderConstantsXR(CommandBufferHelpers.GetRasterCommandBuffer(cmd), renderIntoTexture2);
				XRSystemUniversal.MarkShaderProperties(CommandBufferHelpers.GetRasterCommandBuffer(cmd), cameraData.xrUniversal, renderIntoTexture2);
			}
		}
	}

	[Obsolete("This rendering path is for compatibility mode only (when Render Graph is disabled). Use Render Graph API instead.", false)]
	private void BeginXRRendering(CommandBuffer cmd, ScriptableRenderContext context, ref CameraData cameraData)
	{
		if (!cameraData.xr.enabled)
		{
			return;
		}
		if (cameraData.xrUniversal.isLateLatchEnabled)
		{
			cameraData.xrUniversal.canMarkLateLatch = true;
		}
		cameraData.xr.StartSinglePass(cmd);
		if (cameraData.xr.supportsFoveatedRendering)
		{
			cmd.ConfigureFoveatedRendering(cameraData.xr.foveatedRenderingInfo);
			if (XRSystem.foveatedRenderingCaps.HasFlag(FoveatedRenderingCaps.NonUniformRaster))
			{
				cmd.SetKeyword(in ShaderGlobalKeywords.FoveatedRenderingNonUniformRaster, value: true);
			}
		}
		context.ExecuteCommandBuffer(cmd);
		cmd.Clear();
	}

	[Obsolete("This rendering path is for compatibility mode only (when Render Graph is disabled). Use Render Graph API instead.", false)]
	private void EndXRRendering(CommandBuffer cmd, ScriptableRenderContext context, ref CameraData cameraData)
	{
		if (!cameraData.xr.enabled)
		{
			return;
		}
		cameraData.xr.StopSinglePass(cmd);
		if (XRSystem.foveatedRenderingCaps != 0)
		{
			if (XRSystem.foveatedRenderingCaps.HasFlag(FoveatedRenderingCaps.NonUniformRaster))
			{
				cmd.SetKeyword(in ShaderGlobalKeywords.FoveatedRenderingNonUniformRaster, value: false);
			}
			cmd.ConfigureFoveatedRendering(IntPtr.Zero);
		}
		context.ExecuteCommandBuffer(cmd);
		cmd.Clear();
	}

	[Obsolete("This rendering path is for compatibility mode only (when Render Graph is disabled). Use Render Graph API instead.", false)]
	internal static void SetRenderTarget(CommandBuffer cmd, RTHandle colorAttachment, RTHandle depthAttachment, ClearFlag clearFlag, Color clearColor)
	{
		m_ActiveColorAttachments[0] = colorAttachment;
		for (int i = 1; i < m_ActiveColorAttachments.Length; i++)
		{
			m_ActiveColorAttachments[i] = null;
		}
		for (int j = 0; j < m_ActiveColorAttachments.Length; j++)
		{
			m_ActiveColorAttachmentIDs[j] = m_ActiveColorAttachments[j]?.nameID ?? ((RenderTargetIdentifier)0);
		}
		m_ActiveColorStoreActions[0] = RenderBufferStoreAction.Store;
		m_ActiveDepthStoreAction = RenderBufferStoreAction.Store;
		for (int k = 1; k < m_ActiveColorStoreActions.Length; k++)
		{
			m_ActiveColorStoreActions[k] = RenderBufferStoreAction.Store;
		}
		m_ActiveDepthAttachment = depthAttachment;
		RenderBufferLoadAction colorLoadAction = (((clearFlag & ClearFlag.Color) != 0) ? RenderBufferLoadAction.DontCare : RenderBufferLoadAction.Load);
		RenderBufferLoadAction depthLoadAction = (((clearFlag & ClearFlag.Depth) != 0 || (clearFlag & ClearFlag.Stencil) != 0) ? RenderBufferLoadAction.DontCare : RenderBufferLoadAction.Load);
		if (colorAttachment.rt == null && depthAttachment.rt == null && depthAttachment.nameID == k_CameraTarget.nameID)
		{
			SetRenderTarget(cmd, colorAttachment, colorLoadAction, RenderBufferStoreAction.Store, colorAttachment, depthLoadAction, RenderBufferStoreAction.Store, clearFlag, clearColor);
		}
		else
		{
			SetRenderTarget(cmd, colorAttachment, colorLoadAction, RenderBufferStoreAction.Store, depthAttachment, depthLoadAction, RenderBufferStoreAction.Store, clearFlag, clearColor);
		}
	}

	[Obsolete("This rendering path is for compatibility mode only (when Render Graph is disabled). Use Render Graph API instead.", false)]
	internal static void SetRenderTarget(CommandBuffer cmd, RTHandle colorAttachment, RTHandle depthAttachment, ClearFlag clearFlag, Color clearColor, RenderBufferStoreAction colorStoreAction, RenderBufferStoreAction depthStoreAction)
	{
		m_ActiveColorAttachments[0] = colorAttachment;
		for (int i = 1; i < m_ActiveColorAttachments.Length; i++)
		{
			m_ActiveColorAttachments[i] = null;
		}
		for (int j = 0; j < m_ActiveColorAttachments.Length; j++)
		{
			m_ActiveColorAttachmentIDs[j] = m_ActiveColorAttachments[j]?.nameID ?? ((RenderTargetIdentifier)0);
		}
		m_ActiveColorStoreActions[0] = colorStoreAction;
		m_ActiveDepthStoreAction = depthStoreAction;
		for (int k = 1; k < m_ActiveColorStoreActions.Length; k++)
		{
			m_ActiveColorStoreActions[k] = RenderBufferStoreAction.Store;
		}
		m_ActiveDepthAttachment = depthAttachment;
		RenderBufferLoadAction colorLoadAction = (((clearFlag & ClearFlag.Color) != 0) ? RenderBufferLoadAction.DontCare : RenderBufferLoadAction.Load);
		RenderBufferLoadAction depthLoadAction = (((clearFlag & ClearFlag.Depth) != 0) ? RenderBufferLoadAction.DontCare : RenderBufferLoadAction.Load);
		if (!m_UseOptimizedStoreActions)
		{
			if (colorStoreAction != RenderBufferStoreAction.StoreAndResolve)
			{
				colorStoreAction = RenderBufferStoreAction.Store;
			}
			if (depthStoreAction != RenderBufferStoreAction.StoreAndResolve)
			{
				depthStoreAction = RenderBufferStoreAction.Store;
			}
		}
		SetRenderTarget(cmd, colorAttachment, colorLoadAction, colorStoreAction, depthAttachment, depthLoadAction, depthStoreAction, clearFlag, clearColor);
	}

	[Obsolete("This rendering path is for compatibility mode only (when Render Graph is disabled). Use Render Graph API instead.", false)]
	private static void SetRenderTarget(CommandBuffer cmd, RTHandle colorAttachment, RenderBufferLoadAction colorLoadAction, RenderBufferStoreAction colorStoreAction, RTHandle depthAttachment, RenderBufferLoadAction depthLoadAction, RenderBufferStoreAction depthStoreAction, ClearFlag clearFlags, Color clearColor)
	{
		if (depthAttachment.nameID == BuiltinRenderTextureType.CameraTarget)
		{
			CoreUtils.SetRenderTarget(cmd, colorAttachment, colorLoadAction, colorStoreAction, colorAttachment, depthLoadAction, depthStoreAction, clearFlags, clearColor);
		}
		else
		{
			CoreUtils.SetRenderTarget(cmd, colorAttachment, colorLoadAction, colorStoreAction, depthAttachment, depthLoadAction, depthStoreAction, clearFlags, clearColor);
		}
	}

	[Obsolete("This rendering path is for compatibility mode only (when Render Graph is disabled). Use Render Graph API instead.", false)]
	private static void SetRenderTarget(CommandBuffer cmd, RTHandle[] colorAttachments, RenderTargetIdentifier[] colorAttachmentIDs, RTHandle depthAttachment, ClearFlag clearFlag, Color clearColor)
	{
		m_ActiveColorAttachments = colorAttachments;
		m_ActiveColorAttachmentIDs = colorAttachmentIDs;
		m_ActiveDepthAttachment = depthAttachment;
		CoreUtils.SetRenderTarget(cmd, m_ActiveColorAttachmentIDs, depthAttachment, clearFlag, clearColor);
	}

	internal virtual void SwapColorBuffer(CommandBuffer cmd)
	{
	}

	internal virtual void EnableSwapBufferMSAA(bool enable)
	{
	}

	[Conditional("UNITY_EDITOR")]
	private void DrawGizmos(ScriptableRenderContext context, Camera camera, GizmoSubset gizmoSubset, ref RenderingData renderingData)
	{
	}

	[Conditional("UNITY_EDITOR")]
	private void DrawWireOverlay(ScriptableRenderContext context, Camera camera)
	{
		context.DrawWireOverlay(camera);
	}

	private void InternalStartRendering(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		using (new ProfilingScope(Profiling.internalStartRendering))
		{
			for (int i = 0; i < m_ActiveRenderPassQueue.Count; i++)
			{
				m_ActiveRenderPassQueue[i].OnCameraSetup(renderingData.commandBuffer, ref renderingData);
			}
		}
		context.ExecuteCommandBuffer(renderingData.commandBuffer);
		renderingData.commandBuffer.Clear();
	}

	private void InternalFinishRenderingCommon(CommandBuffer cmd, bool resolveFinalTarget)
	{
		using (new ProfilingScope(Profiling.internalFinishRenderingCommon))
		{
			for (int i = 0; i < m_ActiveRenderPassQueue.Count; i++)
			{
				m_ActiveRenderPassQueue[i].FrameCleanup(cmd);
			}
			if (resolveFinalTarget)
			{
				for (int j = 0; j < m_ActiveRenderPassQueue.Count; j++)
				{
					m_ActiveRenderPassQueue[j].OnFinishCameraStackRendering(cmd);
				}
				FinishRendering(cmd);
				m_IsPipelineExecuting = false;
			}
			m_ActiveRenderPassQueue.Clear();
		}
	}

	private void InternalFinishRenderingExecute(ScriptableRenderContext context, CommandBuffer cmd, bool resolveFinalTarget)
	{
		InternalFinishRenderingCommon(cmd, resolveFinalTarget);
		ResetNativeRenderPassFrameData();
		context.ExecuteCommandBuffer(cmd);
		cmd.Clear();
	}

	private protected int AdjustAndGetScreenMSAASamples(RenderGraph renderGraph, bool useIntermediateColorTarget)
	{
		if (!SystemInfo.supportsMultisampledBackBuffer)
		{
			return 1;
		}
		if (UniversalRenderPipeline.canOptimizeScreenMSAASamples && useIntermediateColorTarget && renderGraph.nativeRenderPassesEnabled && Screen.msaaSamples > 1)
		{
			Screen.SetMSAASamples(1);
		}
		if (Application.platform != RuntimePlatform.OSXPlayer && Application.platform != RuntimePlatform.IPhonePlayer)
		{
			return Mathf.Max(Screen.msaaSamples, 1);
		}
		return Mathf.Max(UniversalRenderPipeline.startFrameScreenMSAASamples, 1);
	}

	internal static void SortStable(List<ScriptableRenderPass> list)
	{
		for (int i = 1; i < list.Count; i++)
		{
			ScriptableRenderPass scriptableRenderPass = list[i];
			int num = i - 1;
			while (num >= 0 && scriptableRenderPass < list[num])
			{
				list[num + 1] = list[num];
				num--;
			}
			list[num + 1] = scriptableRenderPass;
		}
	}
}
