using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AOT;
using Unity.Burst;

namespace Unity.Collections;

[BurstCompile]
internal struct AutoFreeAllocator : AllocatorManager.IAllocator, IDisposable
{
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate int Try_000000E3_0024PostfixBurstDelegate(IntPtr state, ref AllocatorManager.Block block);

	internal static class Try_000000E3_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<Try_000000E3_0024PostfixBurstDelegate>(Try).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static int Invoke(IntPtr state, ref AllocatorManager.Block block)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					return ((delegate* unmanaged[Cdecl]<IntPtr, ref AllocatorManager.Block, int>)functionPointer)(state, ref block);
				}
			}
			return Try_0024BurstManaged(state, ref block);
		}
	}

	private ArrayOfArrays<IntPtr> m_allocated;

	private ArrayOfArrays<IntPtr> m_tofree;

	private AllocatorManager.AllocatorHandle m_handle;

	private AllocatorManager.AllocatorHandle m_backingAllocatorHandle;

	public AllocatorManager.TryFunction Function => Try;

	public AllocatorManager.AllocatorHandle Handle
	{
		get
		{
			return m_handle;
		}
		set
		{
			m_handle = value;
		}
	}

	public Allocator ToAllocator => m_handle.ToAllocator;

	public bool IsCustomAllocator => m_handle.IsCustomAllocator;

	public bool IsAutoDispose => true;

	public unsafe void Update()
	{
		int length = m_tofree.Length;
		while (length-- > 0)
		{
			int length2 = m_allocated.Length;
			while (length2-- > 0)
			{
				if (m_allocated[length2] == m_tofree[length])
				{
					Memory.Unmanaged.Free((void*)m_tofree[length], m_backingAllocatorHandle);
					m_allocated.RemoveAtSwapBack(length2);
					break;
				}
			}
		}
		m_tofree.Rewind();
		m_allocated.TrimExcess();
	}

	public void Initialize(AllocatorManager.AllocatorHandle backingAllocatorHandle)
	{
		m_allocated = new ArrayOfArrays<IntPtr>(1048576, backingAllocatorHandle);
		m_tofree = new ArrayOfArrays<IntPtr>(131072, backingAllocatorHandle);
		m_backingAllocatorHandle = backingAllocatorHandle;
	}

	public unsafe void FreeAll()
	{
		Update();
		m_handle.Rewind();
		for (int i = 0; i < m_allocated.Length; i++)
		{
			Memory.Unmanaged.Free((void*)m_allocated[i], m_backingAllocatorHandle);
		}
		m_allocated.Rewind();
	}

	public void Dispose()
	{
		FreeAll();
		m_tofree.Dispose();
		m_allocated.Dispose();
	}

	public unsafe int Try(ref AllocatorManager.Block block)
	{
		if (block.Range.Pointer == IntPtr.Zero)
		{
			if (block.Bytes == 0L)
			{
				return 0;
			}
			byte* ptr = (byte*)Memory.Unmanaged.Allocate(block.Bytes, block.Alignment, m_backingAllocatorHandle);
			block.Range.Pointer = (IntPtr)ptr;
			block.AllocatedItems = block.Range.Items;
			m_allocated.LockfreeAdd(block.Range.Pointer);
			return 0;
		}
		if (block.Range.Items == 0)
		{
			m_tofree.LockfreeAdd(block.Range.Pointer);
			block.Range.Pointer = IntPtr.Zero;
			block.AllocatedItems = 0;
			return 0;
		}
		return -1;
	}

	[BurstCompile]
	[MonoPInvokeCallback(typeof(AllocatorManager.TryFunction))]
	internal static int Try(IntPtr state, ref AllocatorManager.Block block)
	{
		return Try_000000E3_0024BurstDirectCall.Invoke(state, ref block);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	[MonoPInvokeCallback(typeof(AllocatorManager.TryFunction))]
	internal unsafe static int Try_0024BurstManaged(IntPtr state, ref AllocatorManager.Block block)
	{
		return ((AutoFreeAllocator*)(void*)state)->Try(ref block);
	}
}
