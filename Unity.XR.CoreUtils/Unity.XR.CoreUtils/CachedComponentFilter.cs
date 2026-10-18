using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.XR.CoreUtils;

public class CachedComponentFilter<TFilterType, TRootType> : IDisposable where TFilterType : class where TRootType : Component
{
	private readonly List<TFilterType> m_MasterComponentStorage;

	private static readonly List<TFilterType> k_TempComponentList = new List<TFilterType>();

	private static readonly List<IComponentHost<TFilterType>> k_TempHostComponentList = new List<IComponentHost<TFilterType>>();

	private bool m_DisposedValue;

	public CachedComponentFilter(TRootType componentRoot, CachedSearchType cachedSearchType = CachedSearchType.Children | CachedSearchType.Self, bool includeDisabled = true)
	{
		m_MasterComponentStorage = CollectionPool<List<TFilterType>, TFilterType>.GetCollection();
		k_TempComponentList.Clear();
		k_TempHostComponentList.Clear();
		if ((cachedSearchType & CachedSearchType.Self) == CachedSearchType.Self)
		{
			componentRoot.GetComponents(k_TempComponentList);
			componentRoot.GetComponents(k_TempHostComponentList);
			FilteredCopyToMaster(includeDisabled);
		}
		if ((cachedSearchType & CachedSearchType.Parents) == CachedSearchType.Parents)
		{
			Transform parent = componentRoot.transform.parent;
			while (parent != null && !(parent.GetComponent<TRootType>() != null))
			{
				parent.GetComponents(k_TempComponentList);
				parent.GetComponents(k_TempHostComponentList);
				FilteredCopyToMaster(includeDisabled);
				parent = parent.transform.parent;
			}
		}
		if ((cachedSearchType & CachedSearchType.Children) != CachedSearchType.Children)
		{
			return;
		}
		foreach (Transform item in componentRoot.transform)
		{
			item.GetComponentsInChildren(k_TempComponentList);
			item.GetComponentsInChildren(k_TempHostComponentList);
			FilteredCopyToMaster(includeDisabled, componentRoot);
		}
	}

	public CachedComponentFilter(TFilterType[] componentList, bool includeDisabled = true)
	{
		if (componentList != null)
		{
			m_MasterComponentStorage = CollectionPool<List<TFilterType>, TFilterType>.GetCollection();
			k_TempComponentList.Clear();
			k_TempComponentList.AddRange(componentList);
			FilteredCopyToMaster(includeDisabled);
		}
	}

	public void StoreMatchingComponents<TChildType>(List<TChildType> outputList) where TChildType : class, TFilterType
	{
		foreach (TFilterType item2 in m_MasterComponentStorage)
		{
			if (item2 is TChildType item)
			{
				outputList.Add(item);
			}
		}
	}

	public TChildType[] GetMatchingComponents<TChildType>() where TChildType : class, TFilterType
	{
		int num = 0;
		foreach (TFilterType item in m_MasterComponentStorage)
		{
			if (item is TChildType)
			{
				num++;
			}
		}
		TChildType[] array = new TChildType[num];
		num = 0;
		foreach (TFilterType item2 in m_MasterComponentStorage)
		{
			if (item2 is TChildType val)
			{
				array[num] = val;
				num++;
			}
		}
		return array;
	}

	private void FilteredCopyToMaster(bool includeDisabled)
	{
		if (includeDisabled)
		{
			m_MasterComponentStorage.AddRange(k_TempComponentList);
			{
				foreach (IComponentHost<TFilterType> k_TempHostComponent in k_TempHostComponentList)
				{
					m_MasterComponentStorage.AddRange(k_TempHostComponent.HostedComponents);
				}
				return;
			}
		}
		foreach (TFilterType k_TempComponent in k_TempComponentList)
		{
			Behaviour behaviour = k_TempComponent as Behaviour;
			if (!(behaviour != null) || behaviour.enabled)
			{
				m_MasterComponentStorage.Add(k_TempComponent);
			}
		}
		foreach (IComponentHost<TFilterType> k_TempHostComponent2 in k_TempHostComponentList)
		{
			Behaviour behaviour2 = k_TempHostComponent2 as Behaviour;
			if (!(behaviour2 != null) || behaviour2.enabled)
			{
				m_MasterComponentStorage.AddRange(k_TempHostComponent2.HostedComponents);
			}
		}
	}

	private void FilteredCopyToMaster(bool includeDisabled, TRootType requiredRoot)
	{
		if (includeDisabled)
		{
			foreach (TFilterType k_TempComponent in k_TempComponentList)
			{
				Component component = k_TempComponent as Component;
				if (!(component.transform == requiredRoot) && !(component.GetComponentInParent<TRootType>() != requiredRoot))
				{
					m_MasterComponentStorage.Add(k_TempComponent);
				}
			}
			{
				foreach (IComponentHost<TFilterType> k_TempHostComponent in k_TempHostComponentList)
				{
					Component component2 = k_TempHostComponent as Component;
					if (!(component2.transform == requiredRoot) && !(component2.GetComponentInParent<TRootType>() != requiredRoot))
					{
						m_MasterComponentStorage.AddRange(k_TempHostComponent.HostedComponents);
					}
				}
				return;
			}
		}
		foreach (TFilterType k_TempComponent2 in k_TempComponentList)
		{
			Behaviour behaviour = k_TempComponent2 as Behaviour;
			if (behaviour.enabled && !(behaviour.transform == requiredRoot) && !(behaviour.GetComponentInParent<TRootType>() != requiredRoot))
			{
				m_MasterComponentStorage.Add(k_TempComponent2);
			}
		}
		foreach (IComponentHost<TFilterType> k_TempHostComponent2 in k_TempHostComponentList)
		{
			Behaviour behaviour2 = k_TempHostComponent2 as Behaviour;
			if (behaviour2.enabled && !(behaviour2.transform == requiredRoot) && !(behaviour2.GetComponentInParent<TRootType>() != requiredRoot))
			{
				m_MasterComponentStorage.AddRange(k_TempHostComponent2.HostedComponents);
			}
		}
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!m_DisposedValue)
		{
			if (disposing && m_MasterComponentStorage != null)
			{
				CollectionPool<List<TFilterType>, TFilterType>.RecycleCollection(m_MasterComponentStorage);
			}
			m_DisposedValue = true;
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
	}
}
