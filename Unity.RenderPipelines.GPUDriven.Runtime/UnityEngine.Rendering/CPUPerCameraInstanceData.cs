using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace UnityEngine.Rendering;

internal struct CPUPerCameraInstanceData : IDisposable
{
	internal struct PerCameraInstanceDataArrays : IDisposable
	{
		internal UnsafeList<byte> meshLods;

		internal UnsafeList<byte> crossFades;

		public bool IsCreated
		{
			get
			{
				if (meshLods.IsCreated)
				{
					return crossFades.IsCreated;
				}
				return false;
			}
		}

		public PerCameraInstanceDataArrays(int initCapacity)
		{
			meshLods = new UnsafeList<byte>(initCapacity, Allocator.Persistent);
			meshLods.Length = initCapacity;
			crossFades = new UnsafeList<byte>(initCapacity, Allocator.Persistent);
			crossFades.Length = initCapacity;
		}

		public void Dispose()
		{
			meshLods.Dispose();
			crossFades.Dispose();
		}

		internal void Remove(int index, int lastIndex)
		{
			meshLods[index] = meshLods[lastIndex];
			crossFades[index] = crossFades[lastIndex];
		}

		internal void Grow(int previousCapacity, int newCapacity)
		{
			meshLods.Length = newCapacity;
			crossFades.Length = newCapacity;
		}

		internal void SetDefault(int index)
		{
			meshLods[index] = byte.MaxValue;
			crossFades[index] = byte.MaxValue;
		}
	}

	public const byte k_InvalidByteData = byte.MaxValue;

	public NativeParallelHashMap<int, PerCameraInstanceDataArrays> perCameraData;

	private NativeArray<int> m_StructData;

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

	public int cameraCount => perCameraData.Count();

	public void Initialize(int initCapacity)
	{
		perCameraData = new NativeParallelHashMap<int, PerCameraInstanceDataArrays>(1, Allocator.Persistent);
		m_StructData = new NativeArray<int>(2, Allocator.Persistent);
		instancesCapacity = initCapacity;
		instancesLength = 0;
	}

	public void DeallocateCameras(NativeArray<int> cameraIDs)
	{
		foreach (int item2 in cameraIDs)
		{
			if (perCameraData.TryGetValue(item2, out var item))
			{
				item.Dispose();
				perCameraData.Remove(item2);
			}
		}
	}

	public void AllocateCameras(NativeArray<int> cameraIDs)
	{
		foreach (int item2 in cameraIDs)
		{
			if (!perCameraData.TryGetValue(item2, out var item))
			{
				item = new PerCameraInstanceDataArrays(instancesCapacity);
				perCameraData.Add(item2, item);
			}
		}
	}

	public void Remove(int index)
	{
		int lastIndex = instancesLength - 1;
		foreach (KeyValue<int, PerCameraInstanceDataArrays> perCameraDatum in perCameraData)
		{
			perCameraDatum.Value.Remove(index, lastIndex);
		}
		instancesLength--;
	}

	public void IncreaseInstanceCount()
	{
		instancesLength++;
	}

	public void Dispose()
	{
		foreach (KeyValue<int, PerCameraInstanceDataArrays> perCameraDatum in perCameraData)
		{
			perCameraDatum.Value.Dispose();
		}
		m_StructData.Dispose();
		perCameraData.Dispose();
	}

	internal void Grow(int newCapacity)
	{
		if (newCapacity < instancesCapacity)
		{
			return;
		}
		int previousCapacity = instancesCapacity;
		instancesCapacity = newCapacity;
		foreach (KeyValue<int, PerCameraInstanceDataArrays> perCameraDatum in perCameraData)
		{
			perCameraDatum.Value.Grow(previousCapacity, instancesCapacity);
		}
	}

	public void SetDefault(int index)
	{
		foreach (KeyValue<int, PerCameraInstanceDataArrays> perCameraDatum in perCameraData)
		{
			perCameraDatum.Value.SetDefault(index);
		}
	}
}
