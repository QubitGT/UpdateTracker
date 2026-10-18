using System.Collections.Generic;
using System.Text;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace UnityEngine.Rendering.Universal;

internal class RTHandleResourcePool
{
	protected Dictionary<int, SortedList<int, (RTHandle resource, int frameIndex)>> m_ResourcePool = new Dictionary<int, SortedList<int, (RTHandle, int)>>();

	protected List<int> m_RemoveList = new List<int>(32);

	protected static int s_CurrentStaleResourceCount = 0;

	protected static int s_StaleResourceLifetime = 3;

	protected static int s_StaleResourceMaxCapacity = 32;

	internal int staleResourceCapacity
	{
		get
		{
			return s_StaleResourceMaxCapacity;
		}
		set
		{
			if (s_StaleResourceMaxCapacity != value)
			{
				s_StaleResourceMaxCapacity = value;
				Cleanup();
			}
		}
	}

	internal bool AddResourceToPool(in TextureDesc texDesc, RTHandle resource, int currentFrameIndex)
	{
		if (s_CurrentStaleResourceCount >= s_StaleResourceMaxCapacity)
		{
			return false;
		}
		int hashCodeWithNameHash = GetHashCodeWithNameHash(in texDesc);
		if (!m_ResourcePool.TryGetValue(hashCodeWithNameHash, out SortedList<int, (RTHandle, int)> value))
		{
			value = new SortedList<int, (RTHandle, int)>(s_StaleResourceMaxCapacity);
			m_ResourcePool.Add(hashCodeWithNameHash, value);
		}
		value.Add(resource.GetInstanceID(), (resource, currentFrameIndex));
		s_CurrentStaleResourceCount++;
		return true;
	}

	internal bool TryGetResource(in TextureDesc texDesc, out RTHandle resource, bool usepool = true)
	{
		int hashCodeWithNameHash = GetHashCodeWithNameHash(in texDesc);
		if (usepool && m_ResourcePool.TryGetValue(hashCodeWithNameHash, out SortedList<int, (RTHandle, int)> value) && value.Count > 0)
		{
			resource = value.Values[value.Count - 1].Item1;
			value.RemoveAt(value.Count - 1);
			s_CurrentStaleResourceCount--;
			return true;
		}
		resource = null;
		return false;
	}

	internal void Cleanup()
	{
		foreach (KeyValuePair<int, SortedList<int, (RTHandle, int)>> item in m_ResourcePool)
		{
			foreach (KeyValuePair<int, (RTHandle, int)> item2 in item.Value)
			{
				item2.Value.Item1.Release();
			}
		}
		m_ResourcePool.Clear();
		s_CurrentStaleResourceCount = 0;
	}

	protected static bool ShouldReleaseResource(int lastUsedFrameIndex, int currentFrameIndex)
	{
		return lastUsedFrameIndex + s_StaleResourceLifetime < currentFrameIndex;
	}

	internal void PurgeUnusedResources(int currentFrameIndex)
	{
		m_RemoveList.Clear();
		foreach (KeyValuePair<int, SortedList<int, (RTHandle, int)>> item in m_ResourcePool)
		{
			SortedList<int, (RTHandle, int)> value = item.Value;
			IList<int> keys = value.Keys;
			IList<(RTHandle, int)> values = value.Values;
			for (int i = 0; i < value.Count; i++)
			{
				(RTHandle, int) tuple = values[i];
				if (ShouldReleaseResource(tuple.Item2, currentFrameIndex))
				{
					tuple.Item1.Release();
					m_RemoveList.Add(keys[i]);
					s_CurrentStaleResourceCount--;
				}
			}
			foreach (int remove in m_RemoveList)
			{
				value.Remove(remove);
			}
		}
	}

	internal void LogDebugInfo()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendFormat("RTHandleResourcePool for frame {0}, Total stale resources {1}", Time.frameCount, s_CurrentStaleResourceCount);
		stringBuilder.AppendLine();
		foreach (KeyValuePair<int, SortedList<int, (RTHandle, int)>> item in m_ResourcePool)
		{
			SortedList<int, (RTHandle, int)> value = item.Value;
			_ = value.Keys;
			IList<(RTHandle, int)> values = value.Values;
			for (int i = 0; i < value.Count; i++)
			{
				(RTHandle, int) tuple = values[i];
				stringBuilder.AppendFormat("Resrouce in pool: Name {0} Last active frame index {1} Size {2} x {3} x {4}", tuple.Item1.name, tuple.Item2, tuple.Item1.rt.descriptor.width, tuple.Item1.rt.descriptor.height, tuple.Item1.rt.descriptor.volumeDepth);
				stringBuilder.AppendLine();
			}
		}
		Debug.Log(stringBuilder);
	}

	internal int GetHashCodeWithNameHash(in TextureDesc texDesc)
	{
		return texDesc.GetHashCode() * 23 + texDesc.name.GetHashCode();
	}

	internal static TextureDesc CreateTextureDesc(RenderTextureDescriptor desc, TextureSizeMode textureSizeMode = TextureSizeMode.Explicit, int anisoLevel = 1, float mipMapBias = 0f, FilterMode filterMode = FilterMode.Point, TextureWrapMode wrapMode = TextureWrapMode.Clamp, string name = "")
	{
		GraphicsFormat format = ((desc.depthStencilFormat != 0) ? desc.depthStencilFormat : desc.graphicsFormat);
		TextureDesc result = new TextureDesc(desc.width, desc.height);
		result.sizeMode = textureSizeMode;
		result.slices = desc.volumeDepth;
		result.format = format;
		result.filterMode = filterMode;
		result.wrapMode = wrapMode;
		result.dimension = desc.dimension;
		result.enableRandomWrite = desc.enableRandomWrite;
		result.useMipMap = desc.useMipMap;
		result.autoGenerateMips = desc.autoGenerateMips;
		result.isShadowMap = desc.shadowSamplingMode != ShadowSamplingMode.None;
		result.anisoLevel = anisoLevel;
		result.mipMapBias = mipMapBias;
		result.msaaSamples = (MSAASamples)desc.msaaSamples;
		result.bindTextureMS = desc.bindMS;
		result.useDynamicScale = desc.useDynamicScale;
		result.memoryless = RenderTextureMemoryless.None;
		result.vrUsage = VRTextureUsage.None;
		result.name = name;
		result.enableShadingRate = desc.enableShadingRate;
		return result;
	}
}
