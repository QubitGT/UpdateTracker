using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AOT;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;

namespace Unity.Collections;

public static class AllocatorManager
{
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate int TryFunction(IntPtr allocatorState, ref Block block);

	public struct AllocatorHandle : IAllocator, IDisposable, IEquatable<AllocatorHandle>, IComparable<AllocatorHandle>
	{
		public ushort Index;

		public ushort Version;

		internal ref TableEntry TableEntry => ref SharedStatics.TableEntry.Ref.Data.ElementAt(Index);

		internal bool IsInstalled => ((SharedStatics.IsInstalled.Ref.Data.ElementAt(Index >> 6) >> (int)Index) & 1) != 0;

		public int Value => Index;

		public TryFunction Function => null;

		public AllocatorHandle Handle
		{
			get
			{
				return this;
			}
			set
			{
				this = value;
			}
		}

		public Allocator ToAllocator
		{
			get
			{
				uint index = Index;
				return (Allocator)((Version << 16) | (int)index);
			}
		}

		public bool IsCustomAllocator => Index >= 64;

		public bool IsAutoDispose => ((SharedStatics.IsAutoDispose.Ref.Data.ElementAt(Index >> 6) >> (int)Index) & 1) != 0;

		internal void IncrementVersion()
		{
		}

		internal void Rewind()
		{
		}

		internal void Install(TableEntry tableEntry)
		{
			Rewind();
			TableEntry = tableEntry;
		}

		public static implicit operator AllocatorHandle(Allocator a)
		{
			AllocatorHandle result = default(AllocatorHandle);
			result.Index = (ushort)(a & (Allocator)65535);
			result.Version = 0;
			return result;
		}

		public int TryAllocateBlock<T>(out Block block, int items) where T : unmanaged
		{
			block = new Block
			{
				Range = new Range
				{
					Items = items,
					Allocator = this
				},
				BytesPerItem = UnsafeUtility.SizeOf<T>(),
				Alignment = 1 << math.min(3, math.tzcnt(UnsafeUtility.SizeOf<T>()))
			};
			return Try(ref block);
		}

		public Block AllocateBlock<T>(int items) where T : unmanaged
		{
			TryAllocateBlock<T>(out var block, items);
			return block;
		}

		[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
		[Conditional("UNITY_DOTS_DEBUG")]
		private static void CheckAllocatedSuccessfully(int error)
		{
			if (error != 0)
			{
				throw new ArgumentException($"Error {error}: Failed to Allocate");
			}
		}

		public int Try(ref Block block)
		{
			block.Range.Allocator = this;
			return AllocatorManager.Try(ref block);
		}

		public void Dispose()
		{
			Rewind();
			TableEntry = default(TableEntry);
		}

		public override bool Equals(object obj)
		{
			if (obj is AllocatorHandle)
			{
				return Value == ((AllocatorHandle)obj).Value;
			}
			if (obj is Allocator)
			{
				return ToAllocator == (Allocator)obj;
			}
			return false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(AllocatorHandle other)
		{
			return Value == other.Value;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(Allocator other)
		{
			return ToAllocator == other;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override int GetHashCode()
		{
			return Value;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator ==(AllocatorHandle lhs, AllocatorHandle rhs)
		{
			return lhs.Value == rhs.Value;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator !=(AllocatorHandle lhs, AllocatorHandle rhs)
		{
			return lhs.Value != rhs.Value;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator <(AllocatorHandle lhs, AllocatorHandle rhs)
		{
			return lhs.Value < rhs.Value;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator >(AllocatorHandle lhs, AllocatorHandle rhs)
		{
			return lhs.Value > rhs.Value;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator <=(AllocatorHandle lhs, AllocatorHandle rhs)
		{
			return lhs.Value <= rhs.Value;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator >=(AllocatorHandle lhs, AllocatorHandle rhs)
		{
			return lhs.Value >= rhs.Value;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int CompareTo(AllocatorHandle other)
		{
			return Value - other.Value;
		}
	}

	public struct BlockHandle
	{
		public ushort Value;
	}

	public struct Range : IDisposable
	{
		public IntPtr Pointer;

		public int Items;

		public AllocatorHandle Allocator;

		public void Dispose()
		{
			Block block = default(Block);
			block.Range = this;
			Block block2 = block;
			block2.Dispose();
			this = block2.Range;
		}
	}

	public struct Block : IDisposable
	{
		public Range Range;

		public int BytesPerItem;

		public int AllocatedItems;

		public byte Log2Alignment;

		public byte Padding0;

		public ushort Padding1;

		public uint Padding2;

		public long Bytes => (long)BytesPerItem * (long)Range.Items;

		public long AllocatedBytes => (long)BytesPerItem * (long)AllocatedItems;

		public int Alignment
		{
			get
			{
				return 1 << (int)Log2Alignment;
			}
			set
			{
				Log2Alignment = (byte)(32 - math.lzcnt(math.max(1, value) - 1));
			}
		}

		public void Dispose()
		{
			TryFree();
		}

		public int TryAllocate()
		{
			Range.Pointer = IntPtr.Zero;
			return Try(ref this);
		}

		public int TryFree()
		{
			Range.Items = 0;
			return Try(ref this);
		}

		public void Allocate()
		{
			TryAllocate();
		}

		public void Free()
		{
			TryFree();
		}

		[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
		[Conditional("UNITY_DOTS_DEBUG")]
		private void CheckFailedToAllocate(int error)
		{
			if (error != 0)
			{
				throw new ArgumentException($"Error {error}: Failed to Allocate {this}");
			}
		}

		[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
		[Conditional("UNITY_DOTS_DEBUG")]
		private void CheckFailedToFree(int error)
		{
			if (error != 0)
			{
				throw new ArgumentException($"Error {error}: Failed to Free {this}");
			}
		}
	}

	public interface IAllocator : IDisposable
	{
		TryFunction Function { get; }

		AllocatorHandle Handle { get; set; }

		Allocator ToAllocator { get; }

		bool IsCustomAllocator { get; }

		bool IsAutoDispose => false;

		int Try(ref Block block);
	}

	[BurstCompile]
	internal struct StackAllocator : IAllocator, IDisposable
	{
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate int Try_000000AB_0024PostfixBurstDelegate(IntPtr allocatorState, ref Block block);

		internal static class Try_000000AB_0024BurstDirectCall
		{
			private static IntPtr Pointer;

			[BurstDiscard]
			private static void GetFunctionPointerDiscard(ref IntPtr P_0)
			{
				if (Pointer == (IntPtr)0)
				{
					Pointer = BurstCompiler.CompileFunctionPointer<Try_000000AB_0024PostfixBurstDelegate>(Try).Value;
				}
				P_0 = Pointer;
			}

			private static IntPtr GetFunctionPointer()
			{
				nint result = 0;
				GetFunctionPointerDiscard(ref result);
				return result;
			}

			public unsafe static int Invoke(IntPtr allocatorState, ref Block block)
			{
				if (BurstCompiler.IsEnabled)
				{
					IntPtr functionPointer = GetFunctionPointer();
					if (functionPointer != (IntPtr)0)
					{
						return ((delegate* unmanaged[Cdecl]<IntPtr, ref Block, int>)functionPointer)(allocatorState, ref block);
					}
				}
				return Try_0024BurstManaged(allocatorState, ref block);
			}
		}

		internal AllocatorHandle m_handle;

		internal Block m_storage;

		internal long m_top;

		public AllocatorHandle Handle
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

		public TryFunction Function => Try;

		public void Initialize(Block storage)
		{
			m_storage = storage;
			m_top = 0L;
		}

		public unsafe int Try(ref Block block)
		{
			if (block.Range.Pointer == IntPtr.Zero)
			{
				if (m_top + block.Bytes > m_storage.Bytes)
				{
					return -1;
				}
				block.Range.Pointer = (IntPtr)((byte*)(void*)m_storage.Range.Pointer + m_top);
				block.AllocatedItems = block.Range.Items;
				m_top += block.Bytes;
				return 0;
			}
			if (block.Bytes == 0L)
			{
				if ((byte*)(void*)block.Range.Pointer - (byte*)(void*)m_storage.Range.Pointer == m_top - block.AllocatedBytes)
				{
					m_top -= block.AllocatedBytes;
					block.Range.Pointer = IntPtr.Zero;
					block.AllocatedItems = 0;
					return 0;
				}
				return -1;
			}
			return -1;
		}

		[BurstCompile]
		[MonoPInvokeCallback(typeof(TryFunction))]
		public static int Try(IntPtr allocatorState, ref Block block)
		{
			return Try_000000AB_0024BurstDirectCall.Invoke(allocatorState, ref block);
		}

		public void Dispose()
		{
			m_handle.Rewind();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[BurstCompile]
		[MonoPInvokeCallback(typeof(TryFunction))]
		internal unsafe static int Try_0024BurstManaged(IntPtr allocatorState, ref Block block)
		{
			return ((StackAllocator*)(void*)allocatorState)->Try(ref block);
		}
	}

	[BurstCompile]
	internal struct SlabAllocator : IAllocator, IDisposable
	{
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate int Try_000000B9_0024PostfixBurstDelegate(IntPtr allocatorState, ref Block block);

		internal static class Try_000000B9_0024BurstDirectCall
		{
			private static IntPtr Pointer;

			[BurstDiscard]
			private static void GetFunctionPointerDiscard(ref IntPtr P_0)
			{
				if (Pointer == (IntPtr)0)
				{
					Pointer = BurstCompiler.CompileFunctionPointer<Try_000000B9_0024PostfixBurstDelegate>(Try).Value;
				}
				P_0 = Pointer;
			}

			private static IntPtr GetFunctionPointer()
			{
				nint result = 0;
				GetFunctionPointerDiscard(ref result);
				return result;
			}

			public unsafe static int Invoke(IntPtr allocatorState, ref Block block)
			{
				if (BurstCompiler.IsEnabled)
				{
					IntPtr functionPointer = GetFunctionPointer();
					if (functionPointer != (IntPtr)0)
					{
						return ((delegate* unmanaged[Cdecl]<IntPtr, ref Block, int>)functionPointer)(allocatorState, ref block);
					}
				}
				return Try_0024BurstManaged(allocatorState, ref block);
			}
		}

		internal AllocatorHandle m_handle;

		internal Block Storage;

		internal int Log2SlabSizeInBytes;

		internal FixedList4096Bytes<int> Occupied;

		internal long budgetInBytes;

		internal long allocatedBytes;

		public AllocatorHandle Handle
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

		public long BudgetInBytes => budgetInBytes;

		public long AllocatedBytes => allocatedBytes;

		internal int SlabSizeInBytes
		{
			get
			{
				return 1 << Log2SlabSizeInBytes;
			}
			set
			{
				Log2SlabSizeInBytes = (byte)(32 - math.lzcnt(math.max(1, value) - 1));
			}
		}

		internal int Slabs => (int)(Storage.Bytes >> Log2SlabSizeInBytes);

		public TryFunction Function => Try;

		internal void Initialize(Block storage, int slabSizeInBytes, long budget)
		{
			Storage = storage;
			Log2SlabSizeInBytes = 0;
			Occupied = default(FixedList4096Bytes<int>);
			budgetInBytes = budget;
			allocatedBytes = 0L;
			SlabSizeInBytes = slabSizeInBytes;
			Occupied.Length = (Slabs + 31) / 32;
		}

		public int Try(ref Block block)
		{
			if (block.Range.Pointer == IntPtr.Zero)
			{
				if (block.Bytes + allocatedBytes > budgetInBytes)
				{
					return -2;
				}
				if (block.Bytes > SlabSizeInBytes)
				{
					return -1;
				}
				for (int i = 0; i < Occupied.Length; i++)
				{
					int num = Occupied[i];
					if (num == -1)
					{
						continue;
					}
					for (int j = 0; j < 32; j++)
					{
						if ((num & (1 << j)) == 0)
						{
							Occupied[i] |= 1 << j;
							block.Range.Pointer = Storage.Range.Pointer + (int)(SlabSizeInBytes * ((long)i * 32L + j));
							block.AllocatedItems = SlabSizeInBytes / block.BytesPerItem;
							allocatedBytes += block.Bytes;
							return 0;
						}
					}
				}
				return -1;
			}
			if (block.Bytes == 0L)
			{
				long num2 = (long)block.Range.Pointer - (long)Storage.Range.Pointer >>> Log2SlabSizeInBytes;
				int index = (int)(num2 >>> 5);
				int num3 = (int)(num2 & 0x1F);
				Occupied[index] &= ~(1 << num3);
				block.Range.Pointer = IntPtr.Zero;
				int num4 = block.AllocatedItems * block.BytesPerItem;
				allocatedBytes -= num4;
				block.AllocatedItems = 0;
				return 0;
			}
			return -1;
		}

		[BurstCompile]
		[MonoPInvokeCallback(typeof(TryFunction))]
		public static int Try(IntPtr allocatorState, ref Block block)
		{
			return Try_000000B9_0024BurstDirectCall.Invoke(allocatorState, ref block);
		}

		public void Dispose()
		{
			m_handle.Rewind();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[BurstCompile]
		[MonoPInvokeCallback(typeof(TryFunction))]
		internal unsafe static int Try_0024BurstManaged(IntPtr allocatorState, ref Block block)
		{
			return ((SlabAllocator*)(void*)allocatorState)->Try(ref block);
		}
	}

	internal struct TableEntry
	{
		internal IntPtr function;

		internal IntPtr state;
	}

	internal struct Array16<T> where T : unmanaged
	{
		internal T f0;

		internal T f1;

		internal T f2;

		internal T f3;

		internal T f4;

		internal T f5;

		internal T f6;

		internal T f7;

		internal T f8;

		internal T f9;

		internal T f10;

		internal T f11;

		internal T f12;

		internal T f13;

		internal T f14;

		internal T f15;
	}

	internal struct Array256<T> where T : unmanaged
	{
		internal Array16<T> f0;

		internal Array16<T> f1;

		internal Array16<T> f2;

		internal Array16<T> f3;

		internal Array16<T> f4;

		internal Array16<T> f5;

		internal Array16<T> f6;

		internal Array16<T> f7;

		internal Array16<T> f8;

		internal Array16<T> f9;

		internal Array16<T> f10;

		internal Array16<T> f11;

		internal Array16<T> f12;

		internal Array16<T> f13;

		internal Array16<T> f14;

		internal Array16<T> f15;
	}

	internal struct Array4096<T> where T : unmanaged
	{
		internal Array256<T> f0;

		internal Array256<T> f1;

		internal Array256<T> f2;

		internal Array256<T> f3;

		internal Array256<T> f4;

		internal Array256<T> f5;

		internal Array256<T> f6;

		internal Array256<T> f7;

		internal Array256<T> f8;

		internal Array256<T> f9;

		internal Array256<T> f10;

		internal Array256<T> f11;

		internal Array256<T> f12;

		internal Array256<T> f13;

		internal Array256<T> f14;

		internal Array256<T> f15;
	}

	internal struct Array32768<T> : IIndexable<T> where T : unmanaged
	{
		internal Array4096<T> f0;

		internal Array4096<T> f1;

		internal Array4096<T> f2;

		internal Array4096<T> f3;

		internal Array4096<T> f4;

		internal Array4096<T> f5;

		internal Array4096<T> f6;

		internal Array4096<T> f7;

		public int Length
		{
			get
			{
				return 32768;
			}
			set
			{
			}
		}

		public unsafe ref T ElementAt(int index)
		{
			fixed (Array4096<T>* ptr = &f0)
			{
				return ref UnsafeUtility.AsRef<T>((byte*)ptr + (nint)index * (nint)sizeof(T));
			}
		}
	}

	internal sealed class SharedStatics
	{
		internal sealed class IsInstalled
		{
			internal static readonly SharedStatic<Long1024> Ref = SharedStatic<Long1024>.GetOrCreateUnsafe(0u, -4832911380680317357L, 0L);
		}

		internal sealed class TableEntry
		{
			internal static readonly SharedStatic<Array32768<AllocatorManager.TableEntry>> Ref = SharedStatic<Array32768<AllocatorManager.TableEntry>>.GetOrCreateUnsafe(0u, -1297938794087215229L, 0L);
		}

		internal sealed class IsAutoDispose
		{
			internal static readonly SharedStatic<Long1024> Ref = SharedStatic<Long1024>.GetOrCreateUnsafe(0u, -5725630068035020733L, 0L);
		}
	}

	internal static class Managed
	{
		internal static TryFunction[] TryFunctionDelegates = new TryFunction[32768];

		[ExcludeFromBurstCompatTesting("Uses managed delegate")]
		public static void RegisterDelegate(int index, TryFunction function)
		{
			if (index >= 32768)
			{
				throw new ArgumentException("index to be registered in TryFunction delegate table exceeds maximum.");
			}
			TryFunctionDelegates[index] = function;
		}

		[ExcludeFromBurstCompatTesting("Uses managed delegate")]
		public static void UnregisterDelegate(int index)
		{
			if (index >= 32768)
			{
				throw new ArgumentException("index to be unregistered in TryFunction delegate table exceeds maximum.");
			}
			TryFunctionDelegates[index] = null;
		}
	}

	private static class AllocatorCache<T> where T : unmanaged, IAllocator
	{
		public static FunctionPointer<TryFunction> TryFunction;

		public static TryFunction CachedFunction;
	}

	public static readonly AllocatorHandle Invalid = new AllocatorHandle
	{
		Index = 0
	};

	public static readonly AllocatorHandle None = new AllocatorHandle
	{
		Index = 1
	};

	public static readonly AllocatorHandle Temp = new AllocatorHandle
	{
		Index = 2
	};

	public static readonly AllocatorHandle TempJob = new AllocatorHandle
	{
		Index = 3
	};

	public static readonly AllocatorHandle Persistent = new AllocatorHandle
	{
		Index = 4
	};

	public static readonly AllocatorHandle AudioKernel = new AllocatorHandle
	{
		Index = 5
	};

	public const int kErrorNone = 0;

	public const int kErrorBufferOverflow = -1;

	public const ushort FirstUserIndex = 64;

	public const ushort MaxNumCustomAllocators = 32768;

	internal static readonly ushort NumGlobalScratchAllocators = (ushort)JobsUtility.ThreadIndexCount;

	internal static readonly ushort MaxNumGlobalAllocators = (ushort)JobsUtility.ThreadIndexCount;

	internal static readonly uint GlobalAllocatorBaseIndex = (uint)(32768 - MaxNumGlobalAllocators);

	internal static readonly uint FirstGlobalScratchpadAllocatorIndex = GlobalAllocatorBaseIndex;

	internal static Block AllocateBlock<T>(this ref T t, int sizeOf, int alignOf, int items) where T : unmanaged, IAllocator
	{
		Block block = default(Block);
		block.Range.Pointer = IntPtr.Zero;
		block.Range.Items = items;
		block.Range.Allocator = t.Handle;
		block.BytesPerItem = sizeOf;
		block.Alignment = math.max(64, alignOf);
		t.Try(ref block);
		return block;
	}

	internal static Block AllocateBlock<T, U>(this ref T t, U u, int items) where T : unmanaged, IAllocator where U : unmanaged
	{
		return AllocateBlock(ref t, UnsafeUtility.SizeOf<U>(), UnsafeUtility.AlignOf<U>(), items);
	}

	public unsafe static void* Allocate<T>(this ref T t, int sizeOf, int alignOf, int items = 1) where T : unmanaged, IAllocator
	{
		return (void*)AllocateBlock(ref t, sizeOf, alignOf, items).Range.Pointer;
	}

	internal unsafe static U* Allocate<T, U>(this ref T t, U u, int items) where T : unmanaged, IAllocator where U : unmanaged
	{
		return (U*)Allocate(ref t, UnsafeUtility.SizeOf<U>(), UnsafeUtility.AlignOf<U>(), items);
	}

	internal unsafe static void* AllocateStruct<T, U>(this ref T t, U u, int items) where T : unmanaged, IAllocator where U : unmanaged
	{
		return Allocate(ref t, UnsafeUtility.SizeOf<U>(), UnsafeUtility.AlignOf<U>(), items);
	}

	internal static void FreeBlock<T>(this ref T t, ref Block block) where T : unmanaged, IAllocator
	{
		block.Range.Items = 0;
		t.Try(ref block);
	}

	internal unsafe static void Free<T>(this ref T t, void* pointer, int sizeOf, int alignOf, int items) where T : unmanaged, IAllocator
	{
		if (pointer != null)
		{
			Block block = default(Block);
			block.AllocatedItems = items;
			block.Range.Pointer = (IntPtr)pointer;
			block.BytesPerItem = sizeOf;
			block.Alignment = alignOf;
			FreeBlock(ref t, ref block);
		}
	}

	internal unsafe static void Free<T, U>(this ref T t, U* pointer, int items) where T : unmanaged, IAllocator where U : unmanaged
	{
		Free(ref t, pointer, UnsafeUtility.SizeOf<U>(), UnsafeUtility.AlignOf<U>(), items);
	}

	public unsafe static void* Allocate(AllocatorHandle handle, int itemSizeInBytes, int alignmentInBytes, int items = 1)
	{
		return Allocate(ref handle, itemSizeInBytes, alignmentInBytes, items);
	}

	public unsafe static T* Allocate<T>(AllocatorHandle handle, int items = 1) where T : unmanaged
	{
		return Allocate(ref handle, default(T), items);
	}

	public unsafe static void Free(AllocatorHandle handle, void* pointer, int itemSizeInBytes, int alignmentInBytes, int items = 1)
	{
		Free(ref handle, pointer, itemSizeInBytes, alignmentInBytes, items);
	}

	public unsafe static void Free(AllocatorHandle handle, void* pointer)
	{
		Free(ref handle, (byte*)pointer, 1);
	}

	public unsafe static void Free<T>(AllocatorHandle handle, T* pointer, int items = 1) where T : unmanaged
	{
		Free(ref handle, pointer, items);
	}

	public static AllocatorHandle ConvertToAllocatorHandle(Allocator a)
	{
		ushort index = (ushort)(a & (Allocator)65535);
		ushort version = (ushort)((uint)a >> 16);
		AllocatorHandle result = default(AllocatorHandle);
		result.Index = index;
		result.Version = version;
		return result;
	}

	[BurstDiscard]
	private static void CheckDelegate(ref bool useDelegate)
	{
		useDelegate = true;
	}

	private static bool UseDelegate()
	{
		bool useDelegate = false;
		CheckDelegate(ref useDelegate);
		return useDelegate;
	}

	private static int allocate_block(ref Block block)
	{
		TableEntry tableEntry = default(TableEntry);
		tableEntry = block.Range.Allocator.TableEntry;
		return new FunctionPointer<TryFunction>(tableEntry.function).Invoke(tableEntry.state, ref block);
	}

	[BurstDiscard]
	private static void forward_mono_allocate_block(ref Block block, ref int error)
	{
		TableEntry tableEntry = default(TableEntry);
		tableEntry = block.Range.Allocator.TableEntry;
		if (block.Range.Allocator.Handle.Index >= 32768)
		{
			throw new ArgumentException("Allocator index into TryFunction delegate table exceeds maximum.");
		}
		error = Managed.TryFunctionDelegates[block.Range.Allocator.Handle.Index](tableEntry.state, ref block);
	}

	internal static Allocator LegacyOf(AllocatorHandle handle)
	{
		if (handle.Value >= 64)
		{
			return Allocator.Persistent;
		}
		return (Allocator)handle.Value;
	}

	private unsafe static int TryLegacy(ref Block block)
	{
		if (block.Range.Pointer == IntPtr.Zero)
		{
			block.Range.Pointer = (IntPtr)Memory.Unmanaged.Allocate(block.Bytes, block.Alignment, LegacyOf(block.Range.Allocator));
			block.AllocatedItems = block.Range.Items;
			if (!(block.Range.Pointer == IntPtr.Zero))
			{
				return 0;
			}
			return -1;
		}
		if (block.Bytes == 0L)
		{
			if (LegacyOf(block.Range.Allocator) != Allocator.None)
			{
				Memory.Unmanaged.Free((void*)block.Range.Pointer, LegacyOf(block.Range.Allocator));
			}
			block.Range.Pointer = IntPtr.Zero;
			block.AllocatedItems = 0;
			return 0;
		}
		return -1;
	}

	public static int Try(ref Block block)
	{
		if (block.Range.Allocator.Value < 64)
		{
			return TryLegacy(ref block);
		}
		TableEntry tableEntry = default(TableEntry);
		tableEntry = block.Range.Allocator.TableEntry;
		new FunctionPointer<TryFunction>(tableEntry.function);
		if (UseDelegate())
		{
			int error = 0;
			forward_mono_allocate_block(ref block, ref error);
			return error;
		}
		return allocate_block(ref block);
	}

	public static void Initialize()
	{
	}

	internal static void Install(AllocatorHandle handle, IntPtr allocatorState, FunctionPointer<TryFunction> functionPointer, TryFunction function, bool IsAutoDispose = false)
	{
		if (functionPointer.Value == IntPtr.Zero)
		{
			Unregister(ref handle);
		}
		else if (ConcurrentMask.Succeeded(ConcurrentMask.TryAllocate(ref SharedStatics.IsInstalled.Ref.Data, handle.Value, 1)))
		{
			handle.Install(new TableEntry
			{
				state = allocatorState,
				function = functionPointer.Value
			});
			Managed.RegisterDelegate(handle.Index, function);
			if (IsAutoDispose)
			{
				ConcurrentMask.TryAllocate(ref SharedStatics.IsAutoDispose.Ref.Data, handle.Value, 1);
			}
		}
	}

	internal static void Install(AllocatorHandle handle, IntPtr allocatorState, TryFunction function)
	{
		FunctionPointer<TryFunction> functionPointer = ((function == null) ? new FunctionPointer<TryFunction>(IntPtr.Zero) : BurstCompiler.CompileFunctionPointer(function));
		Install(handle, allocatorState, functionPointer, function);
	}

	internal static AllocatorHandle Register(IntPtr allocatorState, FunctionPointer<TryFunction> functionPointer, bool IsAutoDispose = false, bool isGlobal = false, int globalIndex = 0)
	{
		int error;
		int offset;
		if (isGlobal)
		{
			if (globalIndex < GlobalAllocatorBaseIndex)
			{
				throw new ArgumentException($"Error: {globalIndex} is less than GlobalAllocatorBaseIndex");
			}
			error = ConcurrentMask.TryAllocate(ref SharedStatics.IsInstalled.Ref.Data, globalIndex, 1);
			offset = globalIndex;
		}
		else
		{
			error = ConcurrentMask.TryAllocate(ref SharedStatics.IsInstalled.Ref.Data, out offset, 1, (int)(GlobalAllocatorBaseIndex - 1), 1);
		}
		TableEntry tableEntry = default(TableEntry);
		tableEntry.state = allocatorState;
		tableEntry.function = functionPointer.Value;
		TableEntry tableEntry2 = tableEntry;
		AllocatorHandle result = default(AllocatorHandle);
		if (ConcurrentMask.Succeeded(error))
		{
			result.Index = (ushort)offset;
			result.Install(tableEntry2);
			if (IsAutoDispose)
			{
				ConcurrentMask.TryAllocate(ref SharedStatics.IsAutoDispose.Ref.Data, offset, 1);
			}
		}
		return result;
	}

	[ExcludeFromBurstCompatTesting("Uses managed delegate")]
	public unsafe static void Register<T>(this ref T t, bool IsAutoDispose = false, bool isGlobal = false, int globalIndex = 0) where T : unmanaged, IAllocator
	{
		TryFunction function = t.Function;
		FunctionPointer<TryFunction> functionPointer;
		if (function == null)
		{
			functionPointer = new FunctionPointer<TryFunction>(IntPtr.Zero);
		}
		else
		{
			if (function != AllocatorCache<T>.CachedFunction)
			{
				AllocatorCache<T>.TryFunction = BurstCompiler.CompileFunctionPointer(function);
				AllocatorCache<T>.CachedFunction = function;
			}
			functionPointer = AllocatorCache<T>.TryFunction;
		}
		t.Handle = Register((IntPtr)UnsafeUtility.AddressOf(ref t), functionPointer, IsAutoDispose, isGlobal, globalIndex);
		Managed.RegisterDelegate(t.Handle.Index, t.Function);
	}

	public static void UnmanagedUnregister<T>(this ref T t) where T : unmanaged, IAllocator
	{
		if (t.Handle.IsInstalled)
		{
			t.Handle.Install(default(TableEntry));
			ConcurrentMask.TryFree(ref SharedStatics.IsInstalled.Ref.Data, t.Handle.Value, 1);
			ConcurrentMask.TryFree(ref SharedStatics.IsAutoDispose.Ref.Data, t.Handle.Value, 1);
		}
	}

	[ExcludeFromBurstCompatTesting("Uses managed delegate")]
	public static void Unregister<T>(this ref T t) where T : unmanaged, IAllocator
	{
		if (t.Handle.IsInstalled)
		{
			t.Handle.Dispose();
			ConcurrentMask.TryFree(ref SharedStatics.IsInstalled.Ref.Data, t.Handle.Value, 1);
			ConcurrentMask.TryFree(ref SharedStatics.IsAutoDispose.Ref.Data, t.Handle.Value, 1);
			Managed.UnregisterDelegate(t.Handle.Index);
		}
	}

	[ExcludeFromBurstCompatTesting("Register uses managed delegate")]
	internal unsafe static ref T CreateAllocator<T>(AllocatorHandle backingAllocator, bool isGlobal = false, int globalIndex = 0) where T : unmanaged, IAllocator
	{
		T* ptr = (T*)Memory.Unmanaged.Allocate(UnsafeUtility.SizeOf<T>(), 16, backingAllocator);
		*ptr = default(T);
		ref T reference = ref UnsafeUtility.AsRef<T>(ptr);
		Register(ref reference, ptr->IsAutoDispose, isGlobal, globalIndex);
		return ref reference;
	}

	[ExcludeFromBurstCompatTesting("Registration uses managed delegates")]
	internal unsafe static void DestroyAllocator<T>(this ref T t, AllocatorHandle backingAllocator) where T : unmanaged, IAllocator
	{
		Unregister(ref t);
		Memory.Unmanaged.Free(UnsafeUtility.AddressOf(ref t), backingAllocator);
	}

	public static void Shutdown()
	{
	}

	internal static bool IsCustomAllocator(AllocatorHandle allocator)
	{
		return allocator.Index >= 64;
	}

	[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
	[Conditional("UNITY_DOTS_DEBUG")]
	internal static void CheckFailedToAllocate(int error)
	{
		if (error != 0)
		{
			throw new ArgumentException("failed to allocate");
		}
	}

	[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
	[Conditional("UNITY_DOTS_DEBUG")]
	internal static void CheckFailedToFree(int error)
	{
		if (error != 0)
		{
			throw new ArgumentException("failed to free");
		}
	}

	[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
	[Conditional("UNITY_DOTS_DEBUG")]
	internal static void CheckValid(AllocatorHandle handle)
	{
	}
}
