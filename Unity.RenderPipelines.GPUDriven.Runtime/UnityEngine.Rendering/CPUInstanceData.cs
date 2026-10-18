using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace UnityEngine.Rendering;

internal struct CPUInstanceData : IDisposable
{
	internal readonly struct ReadOnly
	{
		public readonly NativeArray<int>.ReadOnly instanceIndices;

		public readonly NativeArray<InstanceHandle>.ReadOnly instances;

		public readonly NativeArray<SharedInstanceHandle>.ReadOnly sharedInstances;

		public readonly ParallelBitArray localToWorldIsFlippedBits;

		public readonly NativeArray<AABB>.ReadOnly worldAABBs;

		public readonly NativeArray<int>.ReadOnly tetrahedronCacheIndices;

		public readonly ParallelBitArray movedInCurrentFrameBits;

		public readonly ParallelBitArray movedInPreviousFrameBits;

		public readonly ParallelBitArray visibleInPreviousFrameBits;

		public readonly EditorInstanceDataArrays.ReadOnly editorData;

		public readonly NativeArray<GPUDrivenRendererMeshLodData>.ReadOnly meshLodData;

		public int handlesLength => instanceIndices.Length;

		public int instancesLength => instances.Length;

		public ReadOnly(in CPUInstanceData instanceData)
		{
			instanceIndices = instanceData.m_InstanceIndices.AsArray().AsReadOnly();
			instances = instanceData.instances.GetSubArray(0, instanceData.instancesLength).AsReadOnly();
			sharedInstances = instanceData.sharedInstances.GetSubArray(0, instanceData.instancesLength).AsReadOnly();
			localToWorldIsFlippedBits = instanceData.localToWorldIsFlippedBits.GetSubArray(instanceData.instancesLength);
			worldAABBs = instanceData.worldAABBs.GetSubArray(0, instanceData.instancesLength).AsReadOnly();
			tetrahedronCacheIndices = instanceData.tetrahedronCacheIndices.GetSubArray(0, instanceData.instancesLength).AsReadOnly();
			movedInCurrentFrameBits = instanceData.movedInCurrentFrameBits.GetSubArray(instanceData.instancesLength);
			movedInPreviousFrameBits = instanceData.movedInPreviousFrameBits.GetSubArray(instanceData.instancesLength);
			visibleInPreviousFrameBits = instanceData.visibleInPreviousFrameBits.GetSubArray(instanceData.instancesLength);
			editorData = new EditorInstanceDataArrays.ReadOnly(in instanceData);
			meshLodData = instanceData.meshLodData.GetSubArray(0, instanceData.instancesLength).AsReadOnly();
		}

		public int InstanceToIndex(InstanceHandle instance)
		{
			return instanceIndices[instance.index];
		}

		public InstanceHandle IndexToInstance(int index)
		{
			return instances[index];
		}

		public bool IsValidInstance(InstanceHandle instance)
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
				InstanceHandle instanceHandle = instances[index];
				return index == instanceIndices[instanceHandle.index];
			}
			return false;
		}
	}

	private const int k_InvalidIndex = -1;

	private NativeArray<int> m_StructData;

	private NativeList<int> m_InstanceIndices;

	public NativeArray<InstanceHandle> instances;

	public NativeArray<SharedInstanceHandle> sharedInstances;

	public ParallelBitArray localToWorldIsFlippedBits;

	public NativeArray<AABB> worldAABBs;

	public NativeArray<int> tetrahedronCacheIndices;

	public ParallelBitArray movedInCurrentFrameBits;

	public ParallelBitArray movedInPreviousFrameBits;

	public ParallelBitArray visibleInPreviousFrameBits;

	public EditorInstanceDataArrays editorData;

	public NativeArray<GPUDrivenRendererMeshLodData> meshLodData;

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
		instances = new NativeArray<InstanceHandle>(instancesCapacity, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		instances.FillArray(in InstanceHandle.Invalid);
		sharedInstances = new NativeArray<SharedInstanceHandle>(instancesCapacity, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		sharedInstances.FillArray(in SharedInstanceHandle.Invalid);
		localToWorldIsFlippedBits = new ParallelBitArray(instancesCapacity, Allocator.Persistent);
		worldAABBs = new NativeArray<AABB>(instancesCapacity, Allocator.Persistent);
		tetrahedronCacheIndices = new NativeArray<int>(instancesCapacity, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		ref NativeArray<int> array = ref tetrahedronCacheIndices;
		int value = -1;
		array.FillArray(in value);
		movedInCurrentFrameBits = new ParallelBitArray(instancesCapacity, Allocator.Persistent);
		movedInPreviousFrameBits = new ParallelBitArray(instancesCapacity, Allocator.Persistent);
		visibleInPreviousFrameBits = new ParallelBitArray(instancesCapacity, Allocator.Persistent);
		editorData.Initialize(initCapacity);
		meshLodData = new NativeArray<GPUDrivenRendererMeshLodData>(instancesCapacity, Allocator.Persistent);
	}

	public void Dispose()
	{
		m_StructData.Dispose();
		m_InstanceIndices.Dispose();
		instances.Dispose();
		sharedInstances.Dispose();
		localToWorldIsFlippedBits.Dispose();
		worldAABBs.Dispose();
		tetrahedronCacheIndices.Dispose();
		movedInCurrentFrameBits.Dispose();
		movedInPreviousFrameBits.Dispose();
		visibleInPreviousFrameBits.Dispose();
		editorData.Dispose();
		meshLodData.Dispose();
	}

	private void Grow(int newCapacity)
	{
		ArrayExtensions.ResizeArray(ref instances, newCapacity);
		instances.FillArray(in InstanceHandle.Invalid, instancesCapacity);
		ArrayExtensions.ResizeArray(ref sharedInstances, newCapacity);
		sharedInstances.FillArray(in SharedInstanceHandle.Invalid, instancesCapacity);
		localToWorldIsFlippedBits.Resize(newCapacity);
		ArrayExtensions.ResizeArray(ref worldAABBs, newCapacity);
		ArrayExtensions.ResizeArray(ref tetrahedronCacheIndices, newCapacity);
		ref NativeArray<int> array = ref tetrahedronCacheIndices;
		int value = -1;
		array.FillArray(in value, instancesCapacity);
		movedInCurrentFrameBits.Resize(newCapacity);
		movedInPreviousFrameBits.Resize(newCapacity);
		visibleInPreviousFrameBits.Resize(newCapacity);
		editorData.Grow(newCapacity);
		ArrayExtensions.ResizeArray(ref meshLodData, newCapacity);
		instancesCapacity = newCapacity;
	}

	private void AddUnsafe(InstanceHandle instance)
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

	public int InstanceToIndex(InstanceHandle instance)
	{
		return m_InstanceIndices[instance.index];
	}

	public InstanceHandle IndexToInstance(int index)
	{
		return instances[index];
	}

	public bool IsValidInstance(InstanceHandle instance)
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

	public bool IsFreeInstanceHandle(InstanceHandle instance)
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
			InstanceHandle instanceHandle = instances[index];
			return index == m_InstanceIndices[instanceHandle.index];
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

	public void AddNoGrow(InstanceHandle instance)
	{
		AddUnsafe(instance);
		SetDefault(instance);
	}

	public void Add(InstanceHandle instance)
	{
		EnsureFreeInstances(1);
		AddNoGrow(instance);
	}

	public void Remove(InstanceHandle instance)
	{
		int num = InstanceToIndex(instance);
		int num2 = instancesLength - 1;
		instances[num] = instances[num2];
		sharedInstances[num] = sharedInstances[num2];
		localToWorldIsFlippedBits.Set(num, localToWorldIsFlippedBits.Get(num2));
		worldAABBs[num] = worldAABBs[num2];
		tetrahedronCacheIndices[num] = tetrahedronCacheIndices[num2];
		movedInCurrentFrameBits.Set(num, movedInCurrentFrameBits.Get(num2));
		movedInPreviousFrameBits.Set(num, movedInPreviousFrameBits.Get(num2));
		visibleInPreviousFrameBits.Set(num, visibleInPreviousFrameBits.Get(num2));
		editorData.Remove(num, num2);
		meshLodData[num] = meshLodData[num2];
		m_InstanceIndices[instances[num2].index] = num;
		m_InstanceIndices[instance.index] = -1;
		instancesLength--;
	}

	public void Set(InstanceHandle instance, SharedInstanceHandle sharedInstance, bool localToWorldIsFlipped, in AABB worldAABB, int tetrahedronCacheIndex, bool movedInCurrentFrame, bool movedInPreviousFrame, bool visibleInPreviousFrame, in GPUDrivenRendererMeshLodData meshLod)
	{
		int num = InstanceToIndex(instance);
		sharedInstances[num] = sharedInstance;
		localToWorldIsFlippedBits.Set(num, localToWorldIsFlipped);
		worldAABBs[num] = worldAABB;
		tetrahedronCacheIndices[num] = tetrahedronCacheIndex;
		movedInCurrentFrameBits.Set(num, movedInCurrentFrame);
		movedInPreviousFrameBits.Set(num, movedInPreviousFrame);
		visibleInPreviousFrameBits.Set(num, visibleInPreviousFrame);
		editorData.SetDefault(num);
		meshLodData[num] = meshLod;
	}

	public void SetDefault(InstanceHandle instance)
	{
		SharedInstanceHandle invalid = SharedInstanceHandle.Invalid;
		AABB worldAABB = default(AABB);
		GPUDrivenRendererMeshLodData meshLod = default(GPUDrivenRendererMeshLodData);
		Set(instance, invalid, localToWorldIsFlipped: false, in worldAABB, -1, movedInCurrentFrame: false, movedInPreviousFrame: false, visibleInPreviousFrame: false, in meshLod);
	}

	public SharedInstanceHandle Get_SharedInstance(InstanceHandle instance)
	{
		return sharedInstances[InstanceToIndex(instance)];
	}

	public bool Get_LocalToWorldIsFlipped(InstanceHandle instance)
	{
		return localToWorldIsFlippedBits.Get(InstanceToIndex(instance));
	}

	public AABB Get_WorldAABB(InstanceHandle instance)
	{
		return worldAABBs[InstanceToIndex(instance)];
	}

	public int Get_TetrahedronCacheIndex(InstanceHandle instance)
	{
		return tetrahedronCacheIndices[InstanceToIndex(instance)];
	}

	public unsafe ref AABB Get_WorldBounds(InstanceHandle instance)
	{
		return ref UnsafeUtility.ArrayElementAsRef<AABB>(worldAABBs.GetUnsafePtr(), InstanceToIndex(instance));
	}

	public bool Get_MovedInCurrentFrame(InstanceHandle instance)
	{
		return movedInCurrentFrameBits.Get(InstanceToIndex(instance));
	}

	public bool Get_MovedInPreviousFrame(InstanceHandle instance)
	{
		return movedInPreviousFrameBits.Get(InstanceToIndex(instance));
	}

	public bool Get_VisibleInPreviousFrame(InstanceHandle instance)
	{
		return visibleInPreviousFrameBits.Get(InstanceToIndex(instance));
	}

	public GPUDrivenRendererMeshLodData Get_MeshLodData(InstanceHandle instance)
	{
		return meshLodData[InstanceToIndex(instance)];
	}

	public void Set_SharedInstance(InstanceHandle instance, SharedInstanceHandle sharedInstance)
	{
		sharedInstances[InstanceToIndex(instance)] = sharedInstance;
	}

	public void Set_LocalToWorldIsFlipped(InstanceHandle instance, bool isFlipped)
	{
		localToWorldIsFlippedBits.Set(InstanceToIndex(instance), isFlipped);
	}

	public void Set_WorldAABB(InstanceHandle instance, in AABB worldBounds)
	{
		worldAABBs[InstanceToIndex(instance)] = worldBounds;
	}

	public void Set_TetrahedronCacheIndex(InstanceHandle instance, int tetrahedronCacheIndex)
	{
		tetrahedronCacheIndices[InstanceToIndex(instance)] = tetrahedronCacheIndex;
	}

	public void Set_MovedInCurrentFrame(InstanceHandle instance, bool movedInCurrentFrame)
	{
		movedInCurrentFrameBits.Set(InstanceToIndex(instance), movedInCurrentFrame);
	}

	public void Set_MovedInPreviousFrame(InstanceHandle instance, bool movedInPreviousFrame)
	{
		movedInPreviousFrameBits.Set(InstanceToIndex(instance), movedInPreviousFrame);
	}

	public void Set_VisibleInPreviousFrame(InstanceHandle instance, bool visibleInPreviousFrame)
	{
		visibleInPreviousFrameBits.Set(InstanceToIndex(instance), visibleInPreviousFrame);
	}

	public void Set_MeshLodData(InstanceHandle instance, GPUDrivenRendererMeshLodData meshLod)
	{
		meshLodData[InstanceToIndex(instance)] = meshLod;
	}

	public ReadOnly AsReadOnly()
	{
		return new ReadOnly(in this);
	}
}
