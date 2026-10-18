using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace UnityEngine.Rendering;

internal struct CPUSharedInstanceData : IDisposable
{
	internal readonly struct ReadOnly
	{
		public readonly NativeArray<int>.ReadOnly instanceIndices;

		public readonly NativeArray<SharedInstanceHandle>.ReadOnly instances;

		public readonly NativeArray<int>.ReadOnly rendererGroupIDs;

		public readonly NativeArray<SmallIntegerArray>.ReadOnly materialIDArrays;

		public readonly NativeArray<int>.ReadOnly meshIDs;

		public readonly NativeArray<AABB>.ReadOnly localAABBs;

		public readonly NativeArray<CPUSharedInstanceFlags>.ReadOnly flags;

		public readonly NativeArray<uint>.ReadOnly lodGroupAndMasks;

		public readonly NativeArray<GPUDrivenMeshLodInfo>.ReadOnly meshLodInfos;

		public readonly NativeArray<int>.ReadOnly gameObjectLayers;

		public readonly NativeArray<int>.ReadOnly refCounts;

		public int handlesLength => instanceIndices.Length;

		public int instancesLength => instances.Length;

		public ReadOnly(in CPUSharedInstanceData instanceData)
		{
			instanceIndices = instanceData.m_InstanceIndices.AsArray().AsReadOnly();
			instances = instanceData.instances.GetSubArray(0, instanceData.instancesLength).AsReadOnly();
			rendererGroupIDs = instanceData.rendererGroupIDs.GetSubArray(0, instanceData.instancesLength).AsReadOnly();
			materialIDArrays = instanceData.materialIDArrays.GetSubArray(0, instanceData.instancesLength).AsReadOnly();
			meshIDs = instanceData.meshIDs.GetSubArray(0, instanceData.instancesLength).AsReadOnly();
			localAABBs = instanceData.localAABBs.GetSubArray(0, instanceData.instancesLength).AsReadOnly();
			flags = instanceData.flags.GetSubArray(0, instanceData.instancesLength).AsReadOnly();
			lodGroupAndMasks = instanceData.lodGroupAndMasks.GetSubArray(0, instanceData.instancesLength).AsReadOnly();
			meshLodInfos = instanceData.meshLodInfos.GetSubArray(0, instanceData.instancesLength).AsReadOnly();
			gameObjectLayers = instanceData.gameObjectLayers.GetSubArray(0, instanceData.instancesLength).AsReadOnly();
			refCounts = instanceData.refCounts.GetSubArray(0, instanceData.instancesLength).AsReadOnly();
		}

		public int SharedInstanceToIndex(SharedInstanceHandle instance)
		{
			return instanceIndices[instance.index];
		}

		public SharedInstanceHandle IndexToSharedInstance(int index)
		{
			return instances[index];
		}

		public bool IsValidSharedInstance(SharedInstanceHandle instance)
		{
			if (instance.valid && instance.index < instanceIndices.Length)
			{
				int num = instanceIndices[instance.index];
				if (num >= 0 && num < instances.Length)
				{
					return instances[num].Equals(instance);
				}
				return false;
			}
			return false;
		}

		public bool IsValidIndex(int index)
		{
			if (index >= 0 && index < instances.Length)
			{
				SharedInstanceHandle sharedInstanceHandle = instances[index];
				return index == instanceIndices[sharedInstanceHandle.index];
			}
			return false;
		}

		public int InstanceToIndex(in CPUInstanceData.ReadOnly instanceData, InstanceHandle instance)
		{
			int index = instanceData.InstanceToIndex(instance);
			SharedInstanceHandle instance2 = instanceData.sharedInstances[index];
			return SharedInstanceToIndex(instance2);
		}
	}

	private const int k_InvalidIndex = -1;

	private const uint k_InvalidLODGroupAndMask = uint.MaxValue;

	private NativeArray<int> m_StructData;

	private NativeList<int> m_InstanceIndices;

	public NativeArray<SharedInstanceHandle> instances;

	public NativeArray<int> rendererGroupIDs;

	public NativeArray<SmallIntegerArray> materialIDArrays;

	public NativeArray<int> meshIDs;

	public NativeArray<AABB> localAABBs;

	public NativeArray<CPUSharedInstanceFlags> flags;

	public NativeArray<uint> lodGroupAndMasks;

	public NativeArray<GPUDrivenMeshLodInfo> meshLodInfos;

	public NativeArray<int> gameObjectLayers;

	public NativeArray<int> refCounts;

	public int instancesLength
	{
		get
		{
			return m_StructData[0];
		}
		set
		{
			m_StructData[0] = value;
		}
	}

	public int instancesCapacity
	{
		get
		{
			return m_StructData[1];
		}
		set
		{
			m_StructData[1] = value;
		}
	}

	public int handlesLength => m_InstanceIndices.Length;

	public void Initialize(int initCapacity)
	{
		m_StructData = new NativeArray<int>(2, Allocator.Persistent);
		instancesCapacity = initCapacity;
		m_InstanceIndices = new NativeList<int>(Allocator.Persistent);
		instances = new NativeArray<SharedInstanceHandle>(instancesCapacity, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		instances.FillArray(in SharedInstanceHandle.Invalid);
		rendererGroupIDs = new NativeArray<int>(instancesCapacity, Allocator.Persistent);
		materialIDArrays = new NativeArray<SmallIntegerArray>(instancesCapacity, Allocator.Persistent);
		meshIDs = new NativeArray<int>(instancesCapacity, Allocator.Persistent);
		localAABBs = new NativeArray<AABB>(instancesCapacity, Allocator.Persistent);
		flags = new NativeArray<CPUSharedInstanceFlags>(instancesCapacity, Allocator.Persistent);
		lodGroupAndMasks = new NativeArray<uint>(instancesCapacity, Allocator.Persistent);
		ref NativeArray<uint> array = ref lodGroupAndMasks;
		uint value = uint.MaxValue;
		array.FillArray(in value);
		meshLodInfos = new NativeArray<GPUDrivenMeshLodInfo>(instancesCapacity, Allocator.Persistent);
		gameObjectLayers = new NativeArray<int>(instancesCapacity, Allocator.Persistent);
		refCounts = new NativeArray<int>(instancesCapacity, Allocator.Persistent);
	}

	public void Dispose()
	{
		m_StructData.Dispose();
		m_InstanceIndices.Dispose();
		instances.Dispose();
		rendererGroupIDs.Dispose();
		foreach (SmallIntegerArray materialIDArray in materialIDArrays)
		{
			materialIDArray.Dispose();
		}
		materialIDArrays.Dispose();
		meshIDs.Dispose();
		localAABBs.Dispose();
		flags.Dispose();
		lodGroupAndMasks.Dispose();
		meshLodInfos.Dispose();
		gameObjectLayers.Dispose();
		refCounts.Dispose();
	}

	private void Grow(int newCapacity)
	{
		ArrayExtensions.ResizeArray(ref instances, newCapacity);
		instances.FillArray(in SharedInstanceHandle.Invalid, instancesCapacity);
		ArrayExtensions.ResizeArray(ref rendererGroupIDs, newCapacity);
		ArrayExtensions.ResizeArray(ref materialIDArrays, newCapacity);
		ref NativeArray<SmallIntegerArray> array = ref materialIDArrays;
		SmallIntegerArray value = default(SmallIntegerArray);
		array.FillArray(in value, instancesCapacity);
		ArrayExtensions.ResizeArray(ref meshIDs, newCapacity);
		ArrayExtensions.ResizeArray(ref localAABBs, newCapacity);
		ArrayExtensions.ResizeArray(ref flags, newCapacity);
		ArrayExtensions.ResizeArray(ref lodGroupAndMasks, newCapacity);
		ref NativeArray<uint> array2 = ref lodGroupAndMasks;
		uint value2 = uint.MaxValue;
		array2.FillArray(in value2, instancesCapacity);
		ArrayExtensions.ResizeArray(ref meshLodInfos, newCapacity);
		ArrayExtensions.ResizeArray(ref gameObjectLayers, newCapacity);
		ArrayExtensions.ResizeArray(ref refCounts, newCapacity);
		instancesCapacity = newCapacity;
	}

	private void AddUnsafe(SharedInstanceHandle instance)
	{
		if (instance.index >= m_InstanceIndices.Length)
		{
			int length = m_InstanceIndices.Length;
			m_InstanceIndices.ResizeUninitialized(instance.index + 1);
			for (int i = length; i < m_InstanceIndices.Length - 1; i++)
			{
				m_InstanceIndices[i] = -1;
			}
		}
		m_InstanceIndices[instance.index] = instancesLength;
		instances[instancesLength] = instance;
		int num = instancesLength + 1;
		instancesLength = num;
	}

	public int SharedInstanceToIndex(SharedInstanceHandle instance)
	{
		return m_InstanceIndices[instance.index];
	}

	public SharedInstanceHandle IndexToSharedInstance(int index)
	{
		return instances[index];
	}

	public int InstanceToIndex(in CPUInstanceData instanceData, InstanceHandle instance)
	{
		int index = instanceData.InstanceToIndex(instance);
		SharedInstanceHandle instance2 = instanceData.sharedInstances[index];
		return SharedInstanceToIndex(instance2);
	}

	public bool IsValidInstance(SharedInstanceHandle instance)
	{
		if (instance.valid && instance.index < m_InstanceIndices.Length)
		{
			int num = m_InstanceIndices[instance.index];
			if (num >= 0 && num < instancesLength)
			{
				return instances[num].Equals(instance);
			}
			return false;
		}
		return false;
	}

	public bool IsFreeInstanceHandle(SharedInstanceHandle instance)
	{
		if (instance.valid)
		{
			if (instance.index < m_InstanceIndices.Length)
			{
				return m_InstanceIndices[instance.index] == -1;
			}
			return true;
		}
		return false;
	}

	public bool IsValidIndex(int index)
	{
		if (index >= 0 && index < instancesLength)
		{
			SharedInstanceHandle sharedInstanceHandle = instances[index];
			return index == m_InstanceIndices[sharedInstanceHandle.index];
		}
		return false;
	}

	public int GetFreeInstancesCount()
	{
		return instancesCapacity - instancesLength;
	}

	public void EnsureFreeInstances(int instancesCount)
	{
		int freeInstancesCount = GetFreeInstancesCount();
		int num = instancesCount - freeInstancesCount;
		if (num > 0)
		{
			Grow(instancesCapacity + num + 256);
		}
	}

	public void AddNoGrow(SharedInstanceHandle instance)
	{
		AddUnsafe(instance);
		SetDefault(instance);
	}

	public void Add(SharedInstanceHandle instance)
	{
		EnsureFreeInstances(1);
		AddNoGrow(instance);
	}

	public void Remove(SharedInstanceHandle instance)
	{
		int num = SharedInstanceToIndex(instance);
		int index = instancesLength - 1;
		instances[num] = instances[index];
		rendererGroupIDs[num] = rendererGroupIDs[index];
		materialIDArrays[num].Dispose();
		materialIDArrays[num] = materialIDArrays[index];
		materialIDArrays[index] = default(SmallIntegerArray);
		meshIDs[num] = meshIDs[index];
		localAABBs[num] = localAABBs[index];
		flags[num] = flags[index];
		lodGroupAndMasks[num] = lodGroupAndMasks[index];
		meshLodInfos[num] = meshLodInfos[index];
		gameObjectLayers[num] = gameObjectLayers[index];
		refCounts[num] = refCounts[index];
		m_InstanceIndices[instances[index].index] = num;
		m_InstanceIndices[instance.index] = -1;
		instancesLength--;
	}

	public int Get_RendererGroupID(SharedInstanceHandle instance)
	{
		return rendererGroupIDs[SharedInstanceToIndex(instance)];
	}

	public int Get_MeshID(SharedInstanceHandle instance)
	{
		return meshIDs[SharedInstanceToIndex(instance)];
	}

	public unsafe ref AABB Get_LocalAABB(SharedInstanceHandle instance)
	{
		return ref UnsafeUtility.ArrayElementAsRef<AABB>(localAABBs.GetUnsafePtr(), SharedInstanceToIndex(instance));
	}

	public CPUSharedInstanceFlags Get_Flags(SharedInstanceHandle instance)
	{
		return flags[SharedInstanceToIndex(instance)];
	}

	public uint Get_LODGroupAndMask(SharedInstanceHandle instance)
	{
		return lodGroupAndMasks[SharedInstanceToIndex(instance)];
	}

	public int Get_GameObjectLayer(SharedInstanceHandle instance)
	{
		return gameObjectLayers[SharedInstanceToIndex(instance)];
	}

	public int Get_RefCount(SharedInstanceHandle instance)
	{
		return refCounts[SharedInstanceToIndex(instance)];
	}

	public unsafe ref SmallIntegerArray Get_MaterialIDs(SharedInstanceHandle instance)
	{
		return ref UnsafeUtility.ArrayElementAsRef<SmallIntegerArray>(materialIDArrays.GetUnsafePtr(), SharedInstanceToIndex(instance));
	}

	public void Set_RendererGroupID(SharedInstanceHandle instance, int rendererGroupID)
	{
		rendererGroupIDs[SharedInstanceToIndex(instance)] = rendererGroupID;
	}

	public void Set_MeshID(SharedInstanceHandle instance, int meshID)
	{
		meshIDs[SharedInstanceToIndex(instance)] = meshID;
	}

	public void Set_LocalAABB(SharedInstanceHandle instance, in AABB localAABB)
	{
		localAABBs[SharedInstanceToIndex(instance)] = localAABB;
	}

	public void Set_Flags(SharedInstanceHandle instance, CPUSharedInstanceFlags instanceFlags)
	{
		flags[SharedInstanceToIndex(instance)] = instanceFlags;
	}

	public void Set_LODGroupAndMask(SharedInstanceHandle instance, uint lodGroupAndMask)
	{
		lodGroupAndMasks[SharedInstanceToIndex(instance)] = lodGroupAndMask;
	}

	public void Set_GameObjectLayer(SharedInstanceHandle instance, int gameObjectLayer)
	{
		gameObjectLayers[SharedInstanceToIndex(instance)] = gameObjectLayer;
	}

	public void Set_RefCount(SharedInstanceHandle instance, int refCount)
	{
		refCounts[SharedInstanceToIndex(instance)] = refCount;
	}

	public void Set_MaterialIDs(SharedInstanceHandle instance, in SmallIntegerArray materialIDs)
	{
		int index = SharedInstanceToIndex(instance);
		materialIDArrays[index].Dispose();
		materialIDArrays[index] = materialIDs;
	}

	public void Set(SharedInstanceHandle instance, int rendererGroupID, in SmallIntegerArray materialIDs, int meshID, in AABB localAABB, TransformUpdateFlags transformUpdateFlags, InstanceFlags instanceFlags, uint lodGroupAndMask, GPUDrivenMeshLodInfo meshLodInfo, int gameObjectLayer, int refCount)
	{
		int index = SharedInstanceToIndex(instance);
		rendererGroupIDs[index] = rendererGroupID;
		materialIDArrays[index].Dispose();
		materialIDArrays[index] = materialIDs;
		meshIDs[index] = meshID;
		localAABBs[index] = localAABB;
		flags[index] = new CPUSharedInstanceFlags
		{
			transformUpdateFlags = transformUpdateFlags,
			instanceFlags = instanceFlags
		};
		lodGroupAndMasks[index] = lodGroupAndMask;
		meshLodInfos[index] = meshLodInfo;
		gameObjectLayers[index] = gameObjectLayer;
		refCounts[index] = refCount;
	}

	public void SetDefault(SharedInstanceHandle instance)
	{
		SmallIntegerArray materialIDs = default(SmallIntegerArray);
		AABB localAABB = default(AABB);
		Set(instance, 0, in materialIDs, 0, in localAABB, TransformUpdateFlags.None, InstanceFlags.None, uint.MaxValue, default(GPUDrivenMeshLodInfo), 0, 0);
	}

	public ReadOnly AsReadOnly()
	{
		return new ReadOnly(in this);
	}
}
