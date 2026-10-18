using System;
using System.Collections.Generic;

namespace UnityEngine.XR.Management;

public abstract class XRLoaderHelper : XRLoader
{
	protected Dictionary<Type, ISubsystem> m_SubsystemInstanceMap = new Dictionary<Type, ISubsystem>();

	public override T GetLoadedSubsystem<T>()
	{
		Type typeFromHandle = typeof(T);
		m_SubsystemInstanceMap.TryGetValue(typeFromHandle, out var value);
		return value as T;
	}

	protected void StartSubsystem<T>() where T : class, ISubsystem
	{
		GetLoadedSubsystem<T>()?.Start();
	}

	protected void StopSubsystem<T>() where T : class, ISubsystem
	{
		GetLoadedSubsystem<T>()?.Stop();
	}

	protected void DestroySubsystem<T>() where T : class, ISubsystem
	{
		T loadedSubsystem = GetLoadedSubsystem<T>();
		if (loadedSubsystem != null)
		{
			Type typeFromHandle = typeof(T);
			if (m_SubsystemInstanceMap.ContainsKey(typeFromHandle))
			{
				m_SubsystemInstanceMap.Remove(typeFromHandle);
			}
			loadedSubsystem.Destroy();
		}
	}

	protected void CreateSubsystem<TDescriptor, TSubsystem>(List<TDescriptor> descriptors, string id) where TDescriptor : ISubsystemDescriptor where TSubsystem : ISubsystem
	{
		if (descriptors == null)
		{
			throw new ArgumentNullException("descriptors");
		}
		SubsystemManager.GetSubsystemDescriptors(descriptors);
		if (descriptors.Count <= 0)
		{
			return;
		}
		foreach (TDescriptor descriptor in descriptors)
		{
			ISubsystem subsystem = null;
			if (string.Compare(descriptor.id, id, ignoreCase: true) == 0)
			{
				subsystem = descriptor.Create();
			}
			if (subsystem != null)
			{
				m_SubsystemInstanceMap[typeof(TSubsystem)] = subsystem;
				break;
			}
		}
	}

	[Obsolete("This method is obsolete. Please use the geenric CreateSubsystem method.", false)]
	protected void CreateIntegratedSubsystem<TDescriptor, TSubsystem>(List<TDescriptor> descriptors, string id) where TDescriptor : IntegratedSubsystemDescriptor where TSubsystem : IntegratedSubsystem
	{
		CreateSubsystem<TDescriptor, TSubsystem>(descriptors, id);
	}

	[Obsolete("This method is obsolete. Please use the generic CreateSubsystem method.", false)]
	protected void CreateStandaloneSubsystem<TDescriptor, TSubsystem>(List<TDescriptor> descriptors, string id) where TDescriptor : SubsystemDescriptor where TSubsystem : Subsystem
	{
		CreateSubsystem<TDescriptor, TSubsystem>(descriptors, id);
	}

	public override bool Deinitialize()
	{
		m_SubsystemInstanceMap.Clear();
		return base.Deinitialize();
	}
}
