using System;

namespace UnityEngine.Rendering;

internal struct InstanceAllocators
{
	private InstanceAllocator m_InstanceAlloc_MeshRenderer;

	private InstanceAllocator m_InstanceAlloc_SpeedTree;

	private InstanceAllocator m_SharedInstanceAlloc;

	public void Initialize()
	{
		m_InstanceAlloc_MeshRenderer = default(InstanceAllocator);
		m_InstanceAlloc_SpeedTree = default(InstanceAllocator);
		m_InstanceAlloc_MeshRenderer.Initialize(0, 2);
		m_InstanceAlloc_SpeedTree.Initialize(1, 2);
		m_SharedInstanceAlloc = default(InstanceAllocator);
		m_SharedInstanceAlloc.Initialize();
	}

	public void Dispose()
	{
		m_InstanceAlloc_MeshRenderer.Dispose();
		m_InstanceAlloc_SpeedTree.Dispose();
		m_SharedInstanceAlloc.Dispose();
	}

	private InstanceAllocator GetInstanceAllocator(InstanceType type)
	{
		return type switch
		{
			InstanceType.MeshRenderer => m_InstanceAlloc_MeshRenderer, 
			InstanceType.SpeedTree => m_InstanceAlloc_SpeedTree, 
			_ => throw new ArgumentException("Allocator for this type is not created."), 
		};
	}

	public int GetInstanceHandlesLength(InstanceType type)
	{
		return GetInstanceAllocator(type).length;
	}

	public int GetInstancesLength(InstanceType type)
	{
		return GetInstanceAllocator(type).GetNumAllocated();
	}

	public InstanceHandle AllocateInstance(InstanceType type)
	{
		return InstanceHandle.FromInt(GetInstanceAllocator(type).AllocateInstance());
	}

	public void FreeInstance(InstanceHandle instance)
	{
		GetInstanceAllocator(instance.type).FreeInstance(instance.index);
	}

	public SharedInstanceHandle AllocateSharedInstance()
	{
		SharedInstanceHandle result = default(SharedInstanceHandle);
		result.index = m_SharedInstanceAlloc.AllocateInstance();
		return result;
	}

	public void FreeSharedInstance(SharedInstanceHandle instance)
	{
		m_SharedInstanceAlloc.FreeInstance(instance.index);
	}
}
