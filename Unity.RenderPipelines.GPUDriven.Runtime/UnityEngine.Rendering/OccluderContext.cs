using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace UnityEngine.Rendering;

internal struct OccluderContext : IDisposable
{
	private static class ShaderIDs
	{
		public static readonly int _SrcDepth = Shader.PropertyToID("_SrcDepth");

		public static readonly int _DstDepth = Shader.PropertyToID("_DstDepth");

		public static readonly int OccluderDepthPyramidConstants = Shader.PropertyToID("OccluderDepthPyramidConstants");
	}

	public const int k_FirstDepthMipIndex = 3;

	public const int k_MaxOccluderMips = 8;

	public const int k_MaxSilhouettePlanes = 6;

	public const int k_MaxSubviewsPerView = 6;

	public int version;

	public Vector2Int depthBufferSize;

	public NativeArray<OccluderDerivedData> subviewData;

	public int subviewValidMask;

	public NativeArray<OccluderMipBounds> occluderMipBounds;

	public Vector2Int occluderMipLayoutSize;

	public Vector2Int occluderDepthPyramidSize;

	public RTHandle occluderDepthPyramid;

	public int occlusionDebugOverlaySize;

	public GraphicsBuffer occlusionDebugOverlay;

	public bool debugNeedsClear;

	public ComputeBuffer constantBuffer;

	public NativeArray<OccluderDepthPyramidConstants> constantBufferData;

	public int subviewCount => subviewData.Length;

	public Vector2 depthBufferSizeInOccluderPixels
	{
		get
		{
			int num = 8;
			return new Vector2((float)depthBufferSize.x / (float)num, (float)depthBufferSize.y / (float)num);
		}
	}

	public bool IsSubviewValid(int subviewIndex)
	{
		if (subviewIndex < subviewCount)
		{
			return (subviewValidMask & (1 << subviewIndex)) != 0;
		}
		return false;
	}

	public void Dispose()
	{
		if (subviewData.IsCreated)
		{
			subviewData.Dispose();
		}
		if (occluderMipBounds.IsCreated)
		{
			occluderMipBounds.Dispose();
		}
		if (occluderDepthPyramid != null)
		{
			occluderDepthPyramid.Release();
			occluderDepthPyramid = null;
		}
		if (occlusionDebugOverlay != null)
		{
			occlusionDebugOverlay.Release();
			occlusionDebugOverlay = null;
		}
		if (constantBuffer != null)
		{
			constantBuffer.Release();
			constantBuffer = null;
		}
		if (constantBufferData.IsCreated)
		{
			constantBufferData.Dispose();
		}
	}

	private void UpdateMipBounds()
	{
		int num = 8;
		Vector2Int vector2Int = (depthBufferSize + (num - 1) * Vector2Int.one) / num;
		Vector2Int zero = Vector2Int.zero;
		Vector2Int zero2 = Vector2Int.zero;
		Vector2Int size = vector2Int;
		if (!occluderMipBounds.IsCreated)
		{
			occluderMipBounds = new NativeArray<OccluderMipBounds>(8, Allocator.Persistent);
		}
		for (int i = 0; i < 8; i++)
		{
			occluderMipBounds[i] = new OccluderMipBounds
			{
				offset = zero2,
				size = size
			};
			zero.x = Mathf.Max(zero.x, zero2.x + size.x);
			zero.y = Mathf.Max(zero.y, zero2.y + size.y);
			if (i == 0)
			{
				zero2.x = 0;
				zero2.y += size.y;
			}
			else
			{
				zero2.x += size.x;
			}
			size.x = (size.x + 1) / 2;
			size.y = (size.y + 1) / 2;
		}
		occluderMipLayoutSize = zero;
	}

	private void AllocateTexturesIfNecessary(bool debugOverlayEnabled)
	{
		Vector2Int vector2Int = new Vector2Int(occluderMipLayoutSize.x, occluderMipLayoutSize.y * subviewCount);
		if (occluderDepthPyramidSize.x < vector2Int.x || occluderDepthPyramidSize.y < vector2Int.y)
		{
			if (occluderDepthPyramid != null)
			{
				occluderDepthPyramid.Release();
			}
			occluderDepthPyramidSize = vector2Int;
			occluderDepthPyramid = RTHandles.Alloc(occluderDepthPyramidSize.x, occluderDepthPyramidSize.y, GraphicsFormat.R32_SFloat, 1, FilterMode.Point, TextureWrapMode.Clamp, TextureDimension.Tex2D, enableRandomWrite: true, useMipMap: false, autoGenerateMips: true, isShadowMap: false, 1, 0f, MSAASamples.None, bindTextureMS: false, useDynamicScale: false, useDynamicScaleExplicit: false, RenderTextureMemoryless.None, VRTextureUsage.None, "Occluder Depths");
		}
		int num = (debugOverlayEnabled ? (vector2Int.x * vector2Int.y) : 0);
		if (occlusionDebugOverlaySize < num)
		{
			if (occlusionDebugOverlay != null)
			{
				occlusionDebugOverlay.Release();
			}
			occlusionDebugOverlaySize = num;
			debugNeedsClear = true;
			occlusionDebugOverlay = new GraphicsBuffer(GraphicsBuffer.Target.Structured, GraphicsBuffer.UsageFlags.None, occlusionDebugOverlaySize + 4, 4);
		}
		if (num == 0)
		{
			if (occlusionDebugOverlay != null)
			{
				occlusionDebugOverlay.Release();
				occlusionDebugOverlay = null;
			}
			occlusionDebugOverlaySize = num;
		}
		if (constantBuffer == null)
		{
			constantBuffer = new ComputeBuffer(1, UnsafeUtility.SizeOf<OccluderDepthPyramidConstants>(), ComputeBufferType.Constant);
		}
		if (!constantBufferData.IsCreated)
		{
			constantBufferData = new NativeArray<OccluderDepthPyramidConstants>(1, Allocator.Persistent);
		}
	}

	internal static void SetKeyword(ComputeCommandBuffer cmd, ComputeShader cs, in LocalKeyword keyword, bool value)
	{
		if (value)
		{
			cmd.EnableKeyword(cs, in keyword);
		}
		else
		{
			cmd.DisableKeyword(cs, in keyword);
		}
	}

	private unsafe OccluderDepthPyramidConstants SetupFarDepthPyramidConstants(ReadOnlySpan<OccluderSubviewUpdate> occluderSubviewUpdates, NativeArray<Plane> silhouettePlanes)
	{
		OccluderDepthPyramidConstants result = default(OccluderDepthPyramidConstants);
		result._OccluderMipLayoutSizeX = (uint)occluderMipLayoutSize.x;
		result._OccluderMipLayoutSizeY = (uint)occluderMipLayoutSize.y;
		int length = occluderSubviewUpdates.Length;
		for (int i = 0; i < length; i++)
		{
			ref readonly OccluderSubviewUpdate reference = ref occluderSubviewUpdates[i];
			int subviewIndex = reference.subviewIndex;
			subviewData[subviewIndex] = OccluderDerivedData.FromParameters(in reference);
			subviewValidMask |= 1 << reference.subviewIndex;
			Matrix4x4 inverse = (reference.gpuProjMatrix * reference.viewMatrix * Matrix4x4.Translate(-reference.viewOffsetWorldSpace)).inverse;
			for (int j = 0; j < 16; j++)
			{
				result._InvViewProjMatrix[16 * i + j] = inverse[j];
			}
			result._SrcOffset[4 * i] = (uint)reference.depthOffset.x;
			result._SrcOffset[4 * i + 1] = (uint)reference.depthOffset.y;
			result._SrcOffset[4 * i + 2] = 0u;
			result._SrcOffset[4 * i + 3] = 0u;
			result._SrcSliceIndices |= (uint)((reference.depthSliceIndex & 0xF) << 4 * i);
			result._DstSubviewIndices |= (uint)(subviewIndex << 4 * i);
		}
		for (int k = 0; k < 6; k++)
		{
			Plane plane = new Plane(Vector3.zero, 0f);
			if (k < silhouettePlanes.Length)
			{
				plane = silhouettePlanes[k];
			}
			result._SilhouettePlanes[4 * k] = plane.normal.x;
			result._SilhouettePlanes[4 * k + 1] = plane.normal.y;
			result._SilhouettePlanes[4 * k + 2] = plane.normal.z;
			result._SilhouettePlanes[4 * k + 3] = plane.distance;
		}
		result._SilhouettePlaneCount = (uint)silhouettePlanes.Length;
		return result;
	}

	public unsafe void CreateFarDepthPyramid(ComputeCommandBuffer cmd, in OccluderParameters occluderParams, ReadOnlySpan<OccluderSubviewUpdate> occluderSubviewUpdates, in OccluderHandles occluderHandles, NativeArray<Plane> silhouettePlanes, ComputeShader occluderDepthPyramidCS, int occluderDepthDownscaleKernel)
	{
		OccluderDepthPyramidConstants value = SetupFarDepthPyramidConstants(occluderSubviewUpdates, silhouettePlanes);
		LocalKeyword keyword = new LocalKeyword(occluderDepthPyramidCS, "USE_SRC");
		LocalKeyword keyword2 = new LocalKeyword(occluderDepthPyramidCS, "SRC_IS_ARRAY");
		LocalKeyword keyword3 = new LocalKeyword(occluderDepthPyramidCS, "SRC_IS_MSAA");
		bool depthIsArray = occluderParams.depthIsArray;
		bool flag = ((RTHandle)occluderParams.depthTexture)?.isMSAAEnabled ?? false;
		int num = 11;
		for (int i = 0; i < num - 1; i += 4)
		{
			cmd.SetComputeTextureParam(occluderDepthPyramidCS, occluderDepthDownscaleKernel, ShaderIDs._DstDepth, occluderHandles.occluderDepthPyramid);
			bool flag2 = i == 0;
			SetKeyword(cmd, occluderDepthPyramidCS, in keyword, flag2);
			SetKeyword(cmd, occluderDepthPyramidCS, in keyword2, flag2 && depthIsArray);
			SetKeyword(cmd, occluderDepthPyramidCS, in keyword3, flag2 && flag);
			if (flag2)
			{
				cmd.SetComputeTextureParam(occluderDepthPyramidCS, occluderDepthDownscaleKernel, ShaderIDs._SrcDepth, occluderParams.depthTexture);
			}
			value._MipCount = (uint)Math.Min(num - 1 - i, 4);
			Vector2Int vector2Int = Vector2Int.zero;
			for (int j = 0; j < 5; j++)
			{
				Vector2Int vector2Int2 = Vector2Int.zero;
				Vector2Int vector2Int3 = Vector2Int.zero;
				int num2 = i + j;
				if (num2 == 0)
				{
					vector2Int3 = occluderParams.depthSize;
				}
				else
				{
					int num3 = num2 - 3;
					if (0 <= num3 && num3 < 8)
					{
						vector2Int2 = occluderMipBounds[num3].offset;
						vector2Int3 = occluderMipBounds[num3].size;
					}
				}
				if (j == 0)
				{
					vector2Int = vector2Int3;
				}
				value._MipOffsetAndSize[4 * j] = (uint)vector2Int2.x;
				value._MipOffsetAndSize[4 * j + 1] = (uint)vector2Int2.y;
				value._MipOffsetAndSize[4 * j + 2] = (uint)vector2Int3.x;
				value._MipOffsetAndSize[4 * j + 3] = (uint)vector2Int3.y;
			}
			constantBufferData[0] = value;
			cmd.SetBufferData(constantBuffer, constantBufferData);
			cmd.SetComputeConstantBufferParam(occluderDepthPyramidCS, ShaderIDs.OccluderDepthPyramidConstants, constantBuffer, 0, constantBuffer.stride);
			cmd.DispatchCompute(occluderDepthPyramidCS, occluderDepthDownscaleKernel, (vector2Int.x + 15) / 16, (vector2Int.y + 15) / 16, occluderSubviewUpdates.Length);
		}
	}

	public OccluderHandles Import(RenderGraph renderGraph)
	{
		RenderTargetInfo renderTargetInfo = default(RenderTargetInfo);
		renderTargetInfo.width = occluderDepthPyramidSize.x;
		renderTargetInfo.height = occluderDepthPyramidSize.y;
		renderTargetInfo.volumeDepth = 1;
		renderTargetInfo.msaaSamples = 1;
		renderTargetInfo.format = GraphicsFormat.R32_SFloat;
		renderTargetInfo.bindMS = false;
		RenderTargetInfo info = renderTargetInfo;
		OccluderHandles occluderHandles = default(OccluderHandles);
		occluderHandles.occluderDepthPyramid = renderGraph.ImportTexture(occluderDepthPyramid, info);
		OccluderHandles result = occluderHandles;
		if (occlusionDebugOverlay != null)
		{
			result.occlusionDebugOverlay = renderGraph.ImportBuffer(occlusionDebugOverlay);
		}
		return result;
	}

	public void PrepareOccluders(in OccluderParameters occluderParams)
	{
		if (subviewCount != occluderParams.subviewCount)
		{
			if (subviewData.IsCreated)
			{
				subviewData.Dispose();
			}
			subviewData = new NativeArray<OccluderDerivedData>(occluderParams.subviewCount, Allocator.Persistent);
			subviewValidMask = 0;
		}
		depthBufferSize = occluderParams.depthSize;
		bool debugOverlayEnabled = GPUResidentDrawer.GetDebugStats()?.occlusionOverlayEnabled ?? false;
		UpdateMipBounds();
		AllocateTexturesIfNecessary(debugOverlayEnabled);
	}

	internal unsafe OcclusionCullingDebugOutput GetDebugOutput()
	{
		OcclusionCullingDebugOutput occlusionCullingDebugOutput = default(OcclusionCullingDebugOutput);
		occlusionCullingDebugOutput.occluderDepthPyramid = occluderDepthPyramid;
		occlusionCullingDebugOutput.occlusionDebugOverlay = occlusionDebugOverlay;
		OcclusionCullingDebugOutput result = occlusionCullingDebugOutput;
		result.cb._DepthSizeInOccluderPixels = depthBufferSizeInOccluderPixels;
		result.cb._OccluderMipLayoutSizeX = (uint)occluderMipLayoutSize.x;
		result.cb._OccluderMipLayoutSizeY = (uint)occluderMipLayoutSize.y;
		for (int i = 0; i < this.occluderMipBounds.Length; i++)
		{
			OccluderMipBounds occluderMipBounds = this.occluderMipBounds[i];
			result.cb._OccluderMipBounds[4 * i] = (uint)occluderMipBounds.offset.x;
			result.cb._OccluderMipBounds[4 * i + 1] = (uint)occluderMipBounds.offset.y;
			result.cb._OccluderMipBounds[4 * i + 2] = (uint)occluderMipBounds.size.x;
			result.cb._OccluderMipBounds[4 * i + 3] = (uint)occluderMipBounds.size.y;
		}
		return result;
	}
}
