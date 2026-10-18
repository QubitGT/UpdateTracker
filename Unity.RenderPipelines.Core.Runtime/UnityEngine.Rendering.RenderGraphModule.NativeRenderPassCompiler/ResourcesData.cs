using System;
using System.Runtime.CompilerServices;
using Unity.Collections;

namespace UnityEngine.Rendering.RenderGraphModule.NativeRenderPassCompiler;

internal class ResourcesData
{
	public NativeList<ResourceUnversionedData>[] unversionedData;

	public NativeList<ResourceVersionedData>[] versionedData;

	public NativeList<ResourceReaderData>[] readerData;

	public int[] MaxVersions;

	public int[] MaxReaders;

	public DynamicArray<Name>[] resourceNames;

	public ref ResourceVersionedData this[ResourceHandle h] => ref versionedData[h.iType].ElementAt(Index(h));

	public ResourcesData()
	{
		unversionedData = new NativeList<ResourceUnversionedData>[3];
		versionedData = new NativeList<ResourceVersionedData>[3];
		readerData = new NativeList<ResourceReaderData>[3];
		resourceNames = new DynamicArray<Name>[3];
		MaxVersions = new int[3];
		MaxReaders = new int[3];
		for (int i = 0; i < 3; i++)
		{
			resourceNames[i] = new DynamicArray<Name>(0);
		}
	}

	public void Clear()
	{
		for (int i = 0; i < 3; i++)
		{
			if (unversionedData[i].IsCreated)
			{
				unversionedData[i].Clear();
			}
			if (versionedData[i].IsCreated)
			{
				versionedData[i].Clear();
			}
			if (readerData[i].IsCreated)
			{
				readerData[i].Clear();
			}
			resourceNames[i].Clear();
		}
	}

	private void AllocateAndResizeNativeListIfNeeded<T>(ref NativeList<T> nativeList, int size, NativeArrayOptions options) where T : unmanaged
	{
		if (!nativeList.IsCreated)
		{
			nativeList = new NativeList<T>(size, AllocatorManager.Persistent);
		}
		nativeList.Resize(size, options);
	}

	public void Initialize(RenderGraphResourceRegistry resources)
	{
		for (int i = 0; i < 3; i++)
		{
			RenderGraphResourceType type = (RenderGraphResourceType)i;
			int resourceCount = resources.GetResourceCount(type);
			uint num = 0u;
			uint num2 = 0u;
			AllocateAndResizeNativeListIfNeeded(ref unversionedData[i], resourceCount, NativeArrayOptions.UninitializedMemory);
			resourceNames[i].Resize(resourceCount, keepContent: true);
			if (resourceCount > 0)
			{
				ResourceUnversionedData value = default(ResourceUnversionedData);
				value.InitializeNullResource();
				unversionedData[i][0] = value;
				resourceNames[i][0] = new Name("");
			}
			for (int j = 1; j < resourceCount; j++)
			{
				ResourceHandle res = new ResourceHandle(j, type, shared: false);
				IRenderGraphResource resourceLowLevel = resources.GetResourceLowLevel(in res);
				resourceNames[i][j] = new Name(resourceLowLevel.GetName());
				switch (i)
				{
				case 0:
				{
					resources.GetRenderTargetInfo(in res, out var outInfo);
					ref TextureDesc desc3 = ref (resourceLowLevel as TextureResource).desc;
					bool isResourceShared3 = resources.IsRenderGraphResourceShared(in res);
					unversionedData[i][j] = new ResourceUnversionedData(resourceLowLevel, ref outInfo, ref desc3, isResourceShared3);
					break;
				}
				case 1:
				{
					ref BufferDesc desc2 = ref (resourceLowLevel as BufferResource).desc;
					bool isResourceShared2 = resources.IsRenderGraphResourceShared(in res);
					unversionedData[i][j] = new ResourceUnversionedData(resourceLowLevel, ref desc2, isResourceShared2);
					break;
				}
				case 2:
				{
					ref RayTracingAccelerationStructureDesc desc = ref (resourceLowLevel as RayTracingAccelerationStructureResource).desc;
					bool isResourceShared = resources.IsRenderGraphResourceShared(in res);
					unversionedData[i][j] = new ResourceUnversionedData(resourceLowLevel, ref desc, isResourceShared);
					break;
				}
				default:
					throw new Exception("Unsupported resource type: " + i);
				}
				num = Math.Max(num, resourceLowLevel.readCount);
				num2 = Math.Max(num2, resourceLowLevel.writeCount);
			}
			MaxReaders[i] = (int)(num + 1);
			MaxVersions[i] = (int)(num2 + 1);
			AllocateAndResizeNativeListIfNeeded(ref versionedData[i], MaxVersions[i] * resourceCount, NativeArrayOptions.ClearMemory);
			AllocateAndResizeNativeListIfNeeded(ref readerData[i], MaxVersions[i] * MaxReaders[i] * resourceCount, NativeArrayOptions.ClearMemory);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int Index(ResourceHandle h)
	{
		return h.index * MaxVersions[h.iType] + h.version;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int IndexReader(ResourceHandle h, int readerID)
	{
		return (h.index * MaxVersions[h.iType] + h.version) * MaxReaders[h.iType] + readerID;
	}

	public void Dispose()
	{
		for (int i = 0; i < 3; i++)
		{
			if (versionedData[i].IsCreated)
			{
				versionedData[i].Dispose();
			}
			if (unversionedData[i].IsCreated)
			{
				unversionedData[i].Dispose();
			}
			if (readerData[i].IsCreated)
			{
				readerData[i].Dispose();
			}
		}
	}
}
