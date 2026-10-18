using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections;

namespace UnityEngine.Rendering.RenderGraphModule.NativeRenderPassCompiler;

internal class CompilerContextData : IDisposable, RenderGraph.ICompiledGraph
{
	public ref struct NativePassIterator
	{
		private readonly CompilerContextData m_Ctx;

		private int m_Index;

		public ref readonly NativePassData Current => ref m_Ctx.nativePassData.ElementAt(m_Index);

		public NativePassIterator(CompilerContextData ctx)
		{
			m_Ctx = ctx;
			m_Index = -1;
		}

		public bool MoveNext()
		{
			bool flag;
			do
			{
				m_Index++;
				flag = m_Index < m_Ctx.nativePassData.Length;
			}
			while (flag && !m_Ctx.nativePassData.ElementAt(m_Index).IsValid());
			return flag;
		}

		public NativePassIterator GetEnumerator()
		{
			return this;
		}
	}

	public ResourcesData resources;

	public NativeList<PassData> passData;

	public Dictionary<int, GraphicsFence> fences;

	public DynamicArray<Name> passNames;

	public NativeList<PassInputData> inputData;

	public NativeList<PassOutputData> outputData;

	public NativeList<PassFragmentData> fragmentData;

	public NativeList<ResourceHandle> createData;

	public NativeList<ResourceHandle> destroyData;

	public NativeList<PassRandomWriteData> randomAccessResourceData;

	public NativeList<NativePassData> nativePassData;

	public NativeList<SubPassDescriptor> nativeSubPassData;

	private bool m_AreNativeListsAllocated;

	public NativePassIterator NativePasses => new NativePassIterator(this);

	public CompilerContextData()
	{
		fences = new Dictionary<int, GraphicsFence>();
		resources = new ResourcesData();
		passNames = new DynamicArray<Name>(0, resize: false);
	}

	private void AllocateNativeDataStructuresIfNeeded(int estimatedNumPasses)
	{
		if (!m_AreNativeListsAllocated)
		{
			passData = new NativeList<PassData>(estimatedNumPasses, AllocatorManager.Persistent);
			inputData = new NativeList<PassInputData>(estimatedNumPasses * 2, AllocatorManager.Persistent);
			outputData = new NativeList<PassOutputData>(estimatedNumPasses * 2, AllocatorManager.Persistent);
			fragmentData = new NativeList<PassFragmentData>(estimatedNumPasses * 4, AllocatorManager.Persistent);
			randomAccessResourceData = new NativeList<PassRandomWriteData>(4, AllocatorManager.Persistent);
			nativePassData = new NativeList<NativePassData>(estimatedNumPasses, AllocatorManager.Persistent);
			nativeSubPassData = new NativeList<SubPassDescriptor>(estimatedNumPasses, AllocatorManager.Persistent);
			createData = new NativeList<ResourceHandle>(estimatedNumPasses * 2, AllocatorManager.Persistent);
			destroyData = new NativeList<ResourceHandle>(estimatedNumPasses * 2, AllocatorManager.Persistent);
			m_AreNativeListsAllocated = true;
		}
	}

	public void Initialize(RenderGraphResourceRegistry resourceRegistry, int estimatedNumPasses)
	{
		resources.Initialize(resourceRegistry);
		passNames.Reserve(estimatedNumPasses);
		AllocateNativeDataStructuresIfNeeded(estimatedNumPasses);
	}

	public void Clear()
	{
		passNames.Clear();
		resources.Clear();
		if (m_AreNativeListsAllocated)
		{
			passData.Clear();
			fences.Clear();
			inputData.Clear();
			outputData.Clear();
			fragmentData.Clear();
			randomAccessResourceData.Clear();
			nativePassData.Clear();
			nativeSubPassData.Clear();
			createData.Clear();
			destroyData.Clear();
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ref ResourceUnversionedData UnversionedResourceData(ResourceHandle h)
	{
		return ref resources.unversionedData[h.iType].ElementAt(h.index);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ref ResourceVersionedData VersionedResourceData(ResourceHandle h)
	{
		return ref resources[h];
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ReadOnlySpan<ResourceReaderData> Readers(ResourceHandle h)
	{
		int first = resources.IndexReader(h, 0);
		int numReaders = resources[h].numReaders;
		return NativeListExtensions.MakeReadOnlySpan(ref resources.readerData[h.iType], first, numReaders);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ref ResourceReaderData ResourceReader(ResourceHandle h, int i)
	{
		_ = ref resources[h];
		return ref resources.readerData[h.iType].ElementAt(resources.IndexReader(h, 0) + i);
	}

	public bool AddToFragmentList(TextureAccess access, int listFirstIndex, int numItems)
	{
		for (int i = listFirstIndex; i < listFirstIndex + numItems; i++)
		{
			if (fragmentData.ElementAt(i).resource.index == access.textureHandle.handle.index)
			{
				return false;
			}
		}
		ref NativeList<PassFragmentData> reference = ref fragmentData;
		PassFragmentData value = new PassFragmentData(access.textureHandle.handle, access.flags, access.mipLevel, access.depthSlice);
		reference.Add(in value);
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Name GetFullPassName(int passId)
	{
		return passNames[passId];
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public string GetPassName(int passId)
	{
		return passNames[passId].name;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public string GetResourceName(ResourceHandle h)
	{
		return resources.resourceNames[h.iType][h.index].name;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public string GetResourceVersionedName(ResourceHandle h)
	{
		return GetResourceName(h) + " V" + h.version;
	}

	public bool AddToRandomAccessResourceList(ResourceHandle h, int randomWriteSlotIndex, bool preserveCounterValue, int listFirstIndex, int numItems)
	{
		for (int i = listFirstIndex; i < listFirstIndex + numItems; i++)
		{
			if (randomAccessResourceData[i].resource.index == h.index && randomAccessResourceData[i].resource.type == h.type)
			{
				if (randomAccessResourceData[i].resource.version != h.version)
				{
					throw new Exception("Trying to UseTextureRandomWrite two versions of the same resource");
				}
				return false;
			}
		}
		ref NativeList<PassRandomWriteData> reference = ref randomAccessResourceData;
		PassRandomWriteData value = new PassRandomWriteData(h, randomWriteSlotIndex, preserveCounterValue);
		reference.Add(in value);
		return true;
	}

	public void TagAllPasses(int value)
	{
		for (int i = 0; i < passData.Length; i++)
		{
			passData.ElementAt(i).tag = value;
		}
	}

	public void CullAllPasses(bool isCulled)
	{
		for (int i = 0; i < passData.Length; i++)
		{
			passData.ElementAt(i).culled = isCulled;
		}
	}

	internal List<NativePassData> GetNativePasses()
	{
		List<NativePassData> list = new List<NativePassData>();
		NativePassIterator enumerator = NativePasses.GetEnumerator();
		while (enumerator.MoveNext())
		{
			list.Add(enumerator.Current);
		}
		return list;
	}

	~CompilerContextData()
	{
		Cleanup();
	}

	public void Dispose()
	{
		Cleanup();
		GC.SuppressFinalize(this);
	}

	private void Cleanup()
	{
		resources.Dispose();
		if (m_AreNativeListsAllocated)
		{
			passData.Dispose();
			inputData.Dispose();
			outputData.Dispose();
			fragmentData.Dispose();
			createData.Dispose();
			destroyData.Dispose();
			randomAccessResourceData.Dispose();
			nativePassData.Dispose();
			nativeSubPassData.Dispose();
			m_AreNativeListsAllocated = false;
		}
	}
}
