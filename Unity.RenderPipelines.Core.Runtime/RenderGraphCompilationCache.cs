using System.Collections.Generic;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.NativeRenderPassCompiler;

internal class RenderGraphCompilationCache
{
	private struct HashEntry<T>
	{
		public int hash;

		public int lastFrameUsed;

		public T compiledGraph;
	}

	private DynamicArray<HashEntry<RenderGraph.CompiledGraph>> m_HashEntries = new DynamicArray<HashEntry<RenderGraph.CompiledGraph>>();

	private DynamicArray<HashEntry<CompilerContextData>> m_NativeHashEntries = new DynamicArray<HashEntry<CompilerContextData>>();

	private Stack<RenderGraph.CompiledGraph> m_CompiledGraphPool = new Stack<RenderGraph.CompiledGraph>();

	private Stack<CompilerContextData> m_NativeCompiledGraphPool = new Stack<CompilerContextData>();

	private static DynamicArray<HashEntry<RenderGraph.CompiledGraph>>.SortComparer s_EntryComparer = HashEntryComparer;

	private static DynamicArray<HashEntry<CompilerContextData>>.SortComparer s_NativeEntryComparer = HashEntryComparer;

	private const int k_CachedGraphCount = 20;

	private static int s_Hash;

	private static int HashEntryComparer<T>(HashEntry<T> a, HashEntry<T> b)
	{
		if (a.lastFrameUsed < b.lastFrameUsed)
		{
			return -1;
		}
		if (a.lastFrameUsed > b.lastFrameUsed)
		{
			return 1;
		}
		return 0;
	}

	public RenderGraphCompilationCache()
	{
		for (int i = 0; i < 20; i++)
		{
			m_CompiledGraphPool.Push(new RenderGraph.CompiledGraph());
			m_NativeCompiledGraphPool.Push(new CompilerContextData());
		}
	}

	private bool GetCompilationCache<T>(int hash, int frameIndex, out T outGraph, DynamicArray<HashEntry<T>> hashEntries, Stack<T> pool, DynamicArray<HashEntry<T>>.SortComparer comparer) where T : RenderGraph.ICompiledGraph
	{
		s_Hash = hash;
		int num = hashEntries.FindIndex((HashEntry<T> value) => value.hash == s_Hash);
		if (num != -1)
		{
			ref HashEntry<T> reference = ref hashEntries[num];
			outGraph = reference.compiledGraph;
			reference.lastFrameUsed = frameIndex;
			return true;
		}
		if (pool.Count != 0)
		{
			HashEntry<T> hashEntry = default(HashEntry<T>);
			hashEntry.hash = hash;
			hashEntry.lastFrameUsed = frameIndex;
			hashEntry.compiledGraph = pool.Pop();
			HashEntry<T> value2 = hashEntry;
			hashEntries.Add(in value2);
			outGraph = value2.compiledGraph;
			return false;
		}
		hashEntries.QuickSort(comparer);
		ref HashEntry<T> reference2 = ref hashEntries[0];
		reference2.hash = hash;
		reference2.lastFrameUsed = frameIndex;
		reference2.compiledGraph.Clear();
		outGraph = reference2.compiledGraph;
		return false;
	}

	public bool GetCompilationCache(int hash, int frameIndex, out RenderGraph.CompiledGraph outGraph)
	{
		return GetCompilationCache(hash, frameIndex, out outGraph, m_HashEntries, m_CompiledGraphPool, s_EntryComparer);
	}

	public bool GetCompilationCache(int hash, int frameIndex, out CompilerContextData outGraph)
	{
		return GetCompilationCache(hash, frameIndex, out outGraph, m_NativeHashEntries, m_NativeCompiledGraphPool, s_NativeEntryComparer);
	}

	public void Cleanup()
	{
		for (int i = 0; i < m_HashEntries.size; i++)
		{
			m_HashEntries[i].compiledGraph.Clear();
		}
		m_HashEntries.Clear();
		RenderGraph.CompiledGraph[] array = m_CompiledGraphPool.ToArray();
		for (int j = 0; j < array.Length; j++)
		{
			array[j].Clear();
		}
		for (int k = 0; k < m_NativeHashEntries.size; k++)
		{
			m_NativeHashEntries[k].compiledGraph.Dispose();
		}
		m_NativeHashEntries.Clear();
		CompilerContextData[] array2 = m_NativeCompiledGraphPool.ToArray();
		for (int l = 0; l < array2.Length; l++)
		{
			array2[l].Dispose();
		}
	}
}
