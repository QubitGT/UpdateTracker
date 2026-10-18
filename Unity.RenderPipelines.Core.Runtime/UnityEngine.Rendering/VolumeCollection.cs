using System;
using System.Collections.Generic;

namespace UnityEngine.Rendering;

internal class VolumeCollection
{
	internal const int k_MaxLayerCount = 32;

	private readonly Dictionary<int, List<Volume>> m_SortedVolumes = new Dictionary<int, List<Volume>>();

	private readonly List<Volume> m_Volumes = new List<Volume>();

	private readonly Dictionary<int, bool> m_SortNeeded = new Dictionary<int, bool>();

	public int count => m_Volumes.Count;

	public bool Register(Volume volume, int layer)
	{
		if (volume == null)
		{
			throw new ArgumentNullException("volume", "The volume to register is null");
		}
		if (m_Volumes.Contains(volume))
		{
			return false;
		}
		m_Volumes.Add(volume);
		foreach (KeyValuePair<int, List<Volume>> sortedVolume in m_SortedVolumes)
		{
			if ((sortedVolume.Key & (1 << layer)) != 0 && !sortedVolume.Value.Contains(volume))
			{
				sortedVolume.Value.Add(volume);
			}
		}
		SetLayerIndexDirty(layer);
		return true;
	}

	public bool Unregister(Volume volume, int layer)
	{
		if (volume == null)
		{
			throw new ArgumentNullException("volume", "The volume to unregister is null");
		}
		m_Volumes.Remove(volume);
		foreach (KeyValuePair<int, List<Volume>> sortedVolume in m_SortedVolumes)
		{
			if ((sortedVolume.Key & (1 << layer)) != 0)
			{
				sortedVolume.Value.Remove(volume);
			}
		}
		SetLayerIndexDirty(layer);
		return true;
	}

	public bool ChangeLayer(Volume volume, int previousLayerIndex, int currentLayerIndex)
	{
		if (volume == null)
		{
			throw new ArgumentNullException("volume", "The volume to change layer is null");
		}
		Unregister(volume, previousLayerIndex);
		return Register(volume, currentLayerIndex);
	}

	internal static void SortByPriority(List<Volume> volumes)
	{
		for (int i = 1; i < volumes.Count; i++)
		{
			Volume volume = volumes[i];
			int num = i - 1;
			while (num >= 0 && volumes[num].priority > volume.priority)
			{
				volumes[num + 1] = volumes[num];
				num--;
			}
			volumes[num + 1] = volume;
		}
	}

	public List<Volume> GrabVolumes(LayerMask mask)
	{
		if (!m_SortedVolumes.TryGetValue(mask, out var value))
		{
			value = new List<Volume>();
			int num = m_Volumes.Count;
			for (int i = 0; i < num; i++)
			{
				Volume volume = m_Volumes[i];
				if (((int)mask & (1 << volume.gameObject.layer)) != 0)
				{
					value.Add(volume);
					m_SortNeeded[mask] = true;
				}
			}
			m_SortedVolumes.Add(mask, value);
		}
		if (m_SortNeeded.TryGetValue(mask, out var value2) && value2)
		{
			m_SortNeeded[mask] = false;
			SortByPriority(value);
		}
		return value;
	}

	public void SetLayerIndexDirty(int layerIndex)
	{
		foreach (KeyValuePair<int, List<Volume>> sortedVolume in m_SortedVolumes)
		{
			int key = sortedVolume.Key;
			if ((key & (1 << layerIndex)) != 0)
			{
				m_SortNeeded[key] = true;
			}
		}
	}

	public bool IsComponentActiveInMask<T>(LayerMask layerMask) where T : VolumeComponent
	{
		int value = layerMask.value;
		foreach (KeyValuePair<int, List<Volume>> sortedVolume in m_SortedVolumes)
		{
			if (sortedVolume.Key != value)
			{
				continue;
			}
			foreach (Volume item in sortedVolume.Value)
			{
				if (item.enabled && !(item.profileRef == null) && item.profileRef.TryGet<T>(out var component) && component.active)
				{
					return true;
				}
			}
		}
		return false;
	}
}
