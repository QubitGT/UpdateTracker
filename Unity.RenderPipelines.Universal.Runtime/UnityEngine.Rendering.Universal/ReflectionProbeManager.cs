using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine.Experimental.Rendering;

namespace UnityEngine.Rendering.Universal;

internal struct ReflectionProbeManager : IDisposable
{
	private struct CachedProbe
	{
		public uint updateCount;

		public Hash128 imageContentsHash;

		public int size;

		public int mipCount;

		public unsafe fixed int dataIndices[7];

		public unsafe fixed int levels[7];

		public Texture texture;

		public int lastUsed;

		public Vector4 hdrData;
	}

	private static class ShaderProperties
	{
		public static readonly int BoxMin = Shader.PropertyToID("urp_ReflProbes_BoxMin");

		public static readonly int BoxMax = Shader.PropertyToID("urp_ReflProbes_BoxMax");

		public static readonly int ProbePosition = Shader.PropertyToID("urp_ReflProbes_ProbePosition");

		public static readonly int MipScaleOffset = Shader.PropertyToID("urp_ReflProbes_MipScaleOffset");

		public static readonly int Count = Shader.PropertyToID("urp_ReflProbes_Count");

		public static readonly int Atlas = Shader.PropertyToID("urp_ReflProbes_Atlas");
	}

	private int2 m_Resolution;

	private RenderTexture m_AtlasTexture0;

	private RenderTexture m_AtlasTexture1;

	private RTHandle m_AtlasTexture0Handle;

	private BuddyAllocator m_AtlasAllocator;

	private Dictionary<int, CachedProbe> m_Cache;

	private Dictionary<int, int> m_WarningCache;

	private List<int> m_NeedsUpdate;

	private List<int> m_NeedsRemove;

	private Vector4[] m_BoxMax;

	private Vector4[] m_BoxMin;

	private Vector4[] m_ProbePosition;

	private Vector4[] m_MipScaleOffset;

	private const int k_MaxMipCount = 7;

	private const string k_ReflectionProbeAtlasName = "URP Reflection Probe Atlas";

	public RenderTexture atlasRT => m_AtlasTexture0;

	public RTHandle atlasRTHandle => m_AtlasTexture0Handle;

	public static ReflectionProbeManager Create()
	{
		ReflectionProbeManager result = default(ReflectionProbeManager);
		result.Init();
		return result;
	}

	private void Init()
	{
		int maxVisibleReflectionProbes = UniversalRenderPipeline.maxVisibleReflectionProbes;
		m_Resolution = 1;
		GraphicsFormat graphicsFormat = GraphicsFormat.B10G11R11_UFloatPack32;
		if (!SystemInfo.IsFormatSupported(graphicsFormat, GraphicsFormatUsage.Render))
		{
			graphicsFormat = GraphicsFormat.R16G16B16A16_SFloat;
		}
		m_AtlasTexture0 = new RenderTexture(new RenderTextureDescriptor
		{
			width = m_Resolution.x,
			height = m_Resolution.y,
			volumeDepth = 1,
			dimension = TextureDimension.Tex2D,
			graphicsFormat = graphicsFormat,
			useMipMap = false,
			msaaSamples = 1
		});
		m_AtlasTexture0.name = "URP Reflection Probe Atlas";
		m_AtlasTexture0.filterMode = FilterMode.Bilinear;
		m_AtlasTexture0.hideFlags = HideFlags.HideAndDontSave;
		m_AtlasTexture0.Create();
		m_AtlasTexture0Handle = RTHandles.Alloc(m_AtlasTexture0, transferOwnership: true);
		m_AtlasTexture1 = new RenderTexture(m_AtlasTexture0.descriptor);
		m_AtlasTexture1.name = "URP Reflection Probe Atlas";
		m_AtlasTexture1.filterMode = FilterMode.Bilinear;
		m_AtlasTexture1.hideFlags = HideFlags.HideAndDontSave;
		m_AtlasAllocator = new BuddyAllocator(math.floorlog2(SystemInfo.maxTextureSize) - 2, 2);
		m_Cache = new Dictionary<int, CachedProbe>(maxVisibleReflectionProbes);
		m_WarningCache = new Dictionary<int, int>(maxVisibleReflectionProbes);
		m_NeedsUpdate = new List<int>(maxVisibleReflectionProbes);
		m_NeedsRemove = new List<int>(maxVisibleReflectionProbes);
		m_BoxMax = new Vector4[maxVisibleReflectionProbes];
		m_BoxMin = new Vector4[maxVisibleReflectionProbes];
		m_ProbePosition = new Vector4[maxVisibleReflectionProbes];
		m_MipScaleOffset = new Vector4[maxVisibleReflectionProbes * 7];
	}

	public unsafe void UpdateGpuData(CommandBuffer cmd, ref CullingResults cullResults)
	{
		NativeArray<VisibleReflectionProbe> visibleReflectionProbes = cullResults.visibleReflectionProbes;
		int num = math.min(visibleReflectionProbes.Length, UniversalRenderPipeline.maxVisibleReflectionProbes);
		int renderedFrameCount = Time.renderedFrameCount;
		int key;
		foreach (KeyValuePair<int, CachedProbe> item3 in m_Cache)
		{
			item3.Deconstruct(out key, out var value);
			int item = key;
			CachedProbe cachedProbe = value;
			if (Math.Abs(cachedProbe.lastUsed - renderedFrameCount) <= 1 && (bool)cachedProbe.texture && cachedProbe.size == cachedProbe.texture.width)
			{
				continue;
			}
			m_NeedsRemove.Add(item);
			for (int i = 0; i < 7; i++)
			{
				if (cachedProbe.dataIndices[i] != -1)
				{
					m_AtlasAllocator.Free(new BuddyAllocation(cachedProbe.levels[i], cachedProbe.dataIndices[i]));
				}
			}
		}
		foreach (int item4 in m_NeedsRemove)
		{
			m_Cache.Remove(item4);
		}
		m_NeedsRemove.Clear();
		foreach (KeyValuePair<int, int> item5 in m_WarningCache)
		{
			item5.Deconstruct(out key, out var value2);
			int item2 = key;
			if (Math.Abs(value2 - renderedFrameCount) > 1)
			{
				m_NeedsRemove.Add(item2);
			}
		}
		foreach (int item6 in m_NeedsRemove)
		{
			m_WarningCache.Remove(item6);
		}
		m_NeedsRemove.Clear();
		bool flag = false;
		int2 @int = math.int2(0, 0);
		for (int j = 0; j < num; j++)
		{
			VisibleReflectionProbe visibleReflectionProbe = visibleReflectionProbes[j];
			Texture texture = visibleReflectionProbe.texture;
			int instanceID = visibleReflectionProbe.reflectionProbe.GetInstanceID();
			CachedProbe value3;
			bool flag2 = m_Cache.TryGetValue(instanceID, out value3);
			if (!texture)
			{
				continue;
			}
			if (!flag2)
			{
				value3.size = texture.width;
				int num2 = math.ceillog2(value3.size * 4) + 1;
				int num3 = m_AtlasAllocator.levelCount + 2 - num2;
				value3.mipCount = math.min(num2, 7);
				value3.texture = texture;
				int k;
				for (k = 0; k < value3.mipCount; k++)
				{
					int num4 = math.min(num3 + k, m_AtlasAllocator.levelCount - 1);
					if (!m_AtlasAllocator.TryAllocate(num4, out var allocation))
					{
						break;
					}
					value3.levels[k] = allocation.level;
					value3.dataIndices[k] = allocation.index;
					int4 int2 = (int4)(GetScaleOffset(num4, allocation.index, includePadding: true, yflip: false) * m_Resolution.xyxy);
					@int = math.max(@int, int2.zw + int2.xy);
				}
				if (k < value3.mipCount)
				{
					if (!m_WarningCache.ContainsKey(instanceID))
					{
						flag = true;
					}
					m_WarningCache[instanceID] = renderedFrameCount;
					for (int l = 0; l < k; l++)
					{
						m_AtlasAllocator.Free(new BuddyAllocation(value3.levels[l], value3.dataIndices[l]));
					}
					for (int m = 0; m < 7; m++)
					{
						value3.dataIndices[m] = -1;
					}
					continue;
				}
				for (; k < 7; k++)
				{
					value3.dataIndices[k] = -1;
				}
			}
			if ((!flag2 || value3.updateCount != texture.updateCount) | (value3.hdrData != visibleReflectionProbe.hdrData))
			{
				value3.updateCount = texture.updateCount;
				m_NeedsUpdate.Add(instanceID);
			}
			if (visibleReflectionProbe.reflectionProbe.mode == ReflectionProbeMode.Realtime && visibleReflectionProbe.reflectionProbe.refreshMode == ReflectionProbeRefreshMode.EveryFrame)
			{
				value3.lastUsed = -1;
			}
			else
			{
				value3.lastUsed = renderedFrameCount;
			}
			value3.hdrData = visibleReflectionProbe.hdrData;
			m_Cache[instanceID] = value3;
		}
		if (math.any(m_Resolution < @int))
		{
			@int = math.max(m_Resolution, math.ceilpow2(@int));
			RenderTextureDescriptor descriptor = m_AtlasTexture0.descriptor;
			descriptor.width = @int.x;
			descriptor.height = @int.y;
			m_AtlasTexture1.width = @int.x;
			m_AtlasTexture1.height = @int.y;
			m_AtlasTexture1.Create();
			if (m_AtlasTexture0.width != 1)
			{
				if (SystemInfo.copyTextureSupport != 0)
				{
					Graphics.CopyTexture(m_AtlasTexture0, 0, 0, 0, 0, m_Resolution.x, m_Resolution.y, m_AtlasTexture1, 0, 0, 0, 0);
				}
				else
				{
					Graphics.Blit(m_AtlasTexture0, m_AtlasTexture1, (float2)m_Resolution / (float2)@int, Vector2.zero);
				}
			}
			m_AtlasTexture0.Release();
			RenderTexture atlasTexture = m_AtlasTexture1;
			RenderTexture atlasTexture2 = m_AtlasTexture0;
			m_AtlasTexture0 = atlasTexture;
			m_AtlasTexture1 = atlasTexture2;
			m_Resolution = @int;
		}
		int num5 = 0;
		for (int n = 0; n < num; n++)
		{
			VisibleReflectionProbe visibleReflectionProbe2 = visibleReflectionProbes[n];
			int instanceID2 = visibleReflectionProbe2.reflectionProbe.GetInstanceID();
			int num6 = n - num5;
			if (!m_Cache.TryGetValue(instanceID2, out var value4) || !visibleReflectionProbe2.texture)
			{
				num5++;
				continue;
			}
			m_BoxMax[num6] = new Vector4(visibleReflectionProbe2.bounds.max.x, visibleReflectionProbe2.bounds.max.y, visibleReflectionProbe2.bounds.max.z, visibleReflectionProbe2.blendDistance);
			m_BoxMin[num6] = new Vector4(visibleReflectionProbe2.bounds.min.x, visibleReflectionProbe2.bounds.min.y, visibleReflectionProbe2.bounds.min.z, visibleReflectionProbe2.importance);
			m_ProbePosition[num6] = new Vector4(visibleReflectionProbe2.localToWorldMatrix.m03, visibleReflectionProbe2.localToWorldMatrix.m13, visibleReflectionProbe2.localToWorldMatrix.m23, (visibleReflectionProbe2.isBoxProjection ? 1 : (-1)) * value4.mipCount);
			for (int num7 = 0; num7 < value4.mipCount; num7++)
			{
				m_MipScaleOffset[num6 * 7 + num7] = GetScaleOffset(value4.levels[num7], value4.dataIndices[num7], includePadding: false, yflip: false);
			}
		}
		if (flag)
		{
			Debug.LogWarning("A number of reflection probes have been skipped due to the reflection probe atlas being full.\nTo fix this, you can decrease the number or resolution of probes.");
		}
		using (new ProfilingScope(cmd, ProfilingSampler.Get(URPProfileId.UpdateReflectionProbeAtlas)))
		{
			cmd.SetRenderTarget(m_AtlasTexture0);
			foreach (int item7 in m_NeedsUpdate)
			{
				CachedProbe cachedProbe2 = m_Cache[item7];
				for (int num8 = 0; num8 < cachedProbe2.mipCount; num8++)
				{
					int num9 = cachedProbe2.levels[num8];
					int dataIndex = cachedProbe2.dataIndices[num8];
					float4 scaleOffset = GetScaleOffset(num9, dataIndex, includePadding: true, !SystemInfo.graphicsUVStartsAtTop);
					int num10 = (1 << m_AtlasAllocator.levelCount + 1 - num9) - 2;
					Blitter.BlitCubeToOctahedral2DQuadWithPadding(cmd, cachedProbe2.texture, new Vector2(num10, num10), scaleOffset, num8, bilinear: true, 2, cachedProbe2.hdrData);
				}
			}
			cmd.SetGlobalVectorArray(ShaderProperties.BoxMin, m_BoxMin);
			cmd.SetGlobalVectorArray(ShaderProperties.BoxMax, m_BoxMax);
			cmd.SetGlobalVectorArray(ShaderProperties.ProbePosition, m_ProbePosition);
			cmd.SetGlobalVectorArray(ShaderProperties.MipScaleOffset, m_MipScaleOffset);
			cmd.SetGlobalFloat(ShaderProperties.Count, num - num5);
			cmd.SetGlobalTexture(ShaderProperties.Atlas, m_AtlasTexture0);
		}
		m_NeedsUpdate.Clear();
	}

	private float4 GetScaleOffset(int level, int dataIndex, bool includePadding, bool yflip)
	{
		int num = 1 << m_AtlasAllocator.levelCount + 1 - level;
		uint2 @uint = SpaceFillingCurves.DecodeMorton2D((uint)dataIndex);
		float2 xy = (float)(num - ((!includePadding) ? 2 : 0)) / (float2)m_Resolution;
		float2 zw = ((float2)@uint * (float)num + ((!includePadding) ? 1 : 0)) / m_Resolution;
		if (yflip)
		{
			zw.y = 1f - zw.y - xy.y;
		}
		return math.float4(xy, zw);
	}

	public void Dispose()
	{
		if ((bool)m_AtlasTexture0)
		{
			m_AtlasTexture0.Release();
			m_AtlasTexture0Handle.Release();
		}
		m_AtlasAllocator.Dispose();
		Object.DestroyImmediate(m_AtlasTexture0);
		Object.DestroyImmediate(m_AtlasTexture1);
		this = default(ReflectionProbeManager);
	}
}
