#define DEBUG
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AOT;
using Unity.Collections.LowLevel.Unsafe;

namespace Fusion;

public struct DynamicHeap
{
	private struct BlockList
	{
		public int Count;

		public unsafe Block* Head;

		public unsafe Block* Tail;

		public unsafe void AddFirst(Block* item)
		{
			Assert.Check(!IsInList(item));
			item->Next = Head;
			item->Prev = null;
			if (Head != null)
			{
				Head->Prev = item;
				Head = item;
			}
			else
			{
				Head = item;
				Tail = item;
			}
			Count++;
		}

		public unsafe void MoveFirst(Block* item)
		{
			if (Head != item)
			{
				Remove(item);
				AddFirst(item);
			}
		}

		public unsafe void MoveLast(Block* item)
		{
			if (Tail != item)
			{
				Remove(item);
				AddLast(item);
			}
		}

		public unsafe void AddLast(Block* item)
		{
			Assert.Check(!IsInList(item));
			item->Next = null;
			item->Prev = Tail;
			if (Tail != null)
			{
				Tail->Next = item;
				Tail = item;
			}
			else
			{
				Head = item;
				Tail = item;
			}
			Count++;
		}

		public unsafe void AddBefore(Block* before, Block* item)
		{
			Assert.Check(Count > 0);
			Assert.Check(IsInList(before));
			Assert.Check(!IsInList(item));
			if (before == Head)
			{
				AddFirst(item);
			}
			else
			{
				item->Next = before;
				item->Prev = before->Prev;
				before->Prev->Next = item;
				before->Prev = item;
				Count++;
			}
			Assert.Check(IsInList(before));
			Assert.Check(IsInList(item));
		}

		public unsafe void AddAfter(Block* after, Block* item)
		{
			Assert.Check(Count > 0);
			Assert.Check(IsInList(after));
			Assert.Check(!IsInList(item));
			if (after == Tail)
			{
				AddLast(item);
			}
			else
			{
				item->Next = after->Next;
				item->Prev = after;
				after->Next->Prev = item;
				after->Next = item;
				Count++;
			}
			Assert.Check(IsInList(after));
			Assert.Check(IsInList(item));
		}

		public unsafe bool TryRemoveHead(out Block* head)
		{
			if (Count == 0)
			{
				head = null;
				return false;
			}
			head = RemoveHead();
			return true;
		}

		public unsafe Block* RemoveHead()
		{
			Assert.Check(Count > 0);
			Assert.Check(Head != null);
			Assert.Check(IsInList(Head));
			Block* head = Head;
			Remove(head);
			return head;
		}

		public unsafe void Remove(Block* item)
		{
			Assert.Check(IsInList(item));
			if (item->Prev != null)
			{
				item->Prev->Next = item->Next;
			}
			if (item->Next != null)
			{
				item->Next->Prev = item->Prev;
			}
			if (item == Tail)
			{
				Tail = item->Prev;
			}
			if (item == Head)
			{
				Head = item->Next;
			}
			item->Prev = null;
			item->Next = null;
			Count--;
		}

		private unsafe bool IsInList(Block* item)
		{
			for (Block* ptr = Head; ptr != null; ptr = ptr->Next)
			{
				if (ptr == item)
				{
					return true;
				}
			}
			return false;
		}
	}

	internal struct Config
	{
		public int BlockPageCount;

		public static Config Default
		{
			get
			{
				Config result = default(Config);
				result.BlockPageCount = 64;
				return result;
			}
		}
	}

	internal enum Phase
	{
		Idle,
		Mark,
		Sweep,
		Free
	}

	private class TypeData
	{
		public int Stride;

		public ushort Offset;

		public int[] Pointers;

		public Type Type;

		public TypeData()
		{
		}

		public TypeData(int stride, ushort offset, int[] pointers, Type type)
		{
			Stride = stride;
			Offset = offset;
			Pointers = pointers;
			Type = type;
		}
	}

	public unsafe delegate void CollectGarbageDelegate(DynamicHeap* heap, void** dynamicRoots, int dynamicRootsLength);

	private struct PageList
	{
		public int Count;

		public unsafe Page* Head;

		public unsafe Page* Tail;

		public unsafe void AddFirst(Page* item)
		{
			Assert.Check(!Contains(item));
			item->Next = Head;
			item->Prev = null;
			if (Head != null)
			{
				Head->Prev = item;
				Head = item;
			}
			else
			{
				Head = item;
				Tail = item;
			}
			Count++;
		}

		public unsafe void AddLast(Page* item)
		{
			Assert.Check(!Contains(item));
			item->Next = null;
			item->Prev = Tail;
			if (Tail != null)
			{
				Tail->Next = item;
				Tail = item;
			}
			else
			{
				Head = item;
				Tail = item;
			}
			Count++;
		}

		public unsafe void AddBefore(Page* before, Page* item)
		{
			Assert.Check(Count > 0);
			Assert.Check(Contains(before));
			Assert.Check(!Contains(item));
			if (before == Head)
			{
				AddFirst(item);
			}
			else
			{
				item->Next = before;
				item->Prev = before->Prev;
				before->Prev->Next = item;
				before->Prev = item;
				Count++;
			}
			Assert.Check(Contains(before));
			Assert.Check(Contains(item));
		}

		public unsafe void MoveFirst(Page* item)
		{
			if (Head != item)
			{
				Remove(item);
				AddFirst(item);
			}
		}

		public unsafe void MoveLast(Page* item)
		{
			if (Tail != item)
			{
				Remove(item);
				AddLast(item);
			}
		}

		public unsafe void AddAfter(Page* after, Page* item)
		{
			Assert.Check(Count > 0);
			Assert.Check(Contains(after));
			Assert.Check(!Contains(item));
			if (after == Tail)
			{
				AddLast(item);
			}
			else
			{
				item->Next = after->Next;
				item->Prev = after;
				after->Next->Prev = item;
				after->Next = item;
				Count++;
			}
			Assert.Check(Contains(after));
			Assert.Check(Contains(item));
		}

		public unsafe bool TryRemoveHead(out Page* head)
		{
			if (Count == 0)
			{
				head = null;
				return false;
			}
			head = RemoveHead();
			return true;
		}

		public unsafe Page* RemoveHead()
		{
			Assert.Check(Count > 0);
			Assert.Check(Head != null);
			Assert.Check(Contains(Head));
			Page* head = Head;
			Remove(head);
			return head;
		}

		public unsafe void Remove(Page* item)
		{
			Assert.Check(Contains(item));
			if (item->Prev != null)
			{
				item->Prev->Next = item->Next;
			}
			if (item->Next != null)
			{
				item->Next->Prev = item->Prev;
			}
			if (item == Tail)
			{
				Tail = item->Prev;
			}
			if (item == Head)
			{
				Head = item->Next;
			}
			item->Prev = null;
			item->Next = null;
			Count--;
		}

		public unsafe bool Contains(Page* item)
		{
			for (Page* ptr = Head; ptr != null; ptr = ptr->Next)
			{
				if (ptr == item)
				{
					return true;
				}
			}
			return false;
		}
	}

	[Flags]
	private enum ObjectFlags : byte
	{
		Tracked = 1,
		Root = 2,
		Pointer = 4,
		Simple = 8,
		ForceAlive = 0x10,
		Garbage = 0x20
	}

	[AttributeUsage(AttributeTargets.Field)]
	public sealed class Ignore : Attribute
	{
	}

	[StructLayout(LayoutKind.Explicit)]
	private struct Object
	{
		public const int SIZE = 8;

		public const int WORDS = 1;

		[FieldOffset(0)]
		public ObjectFlags Flags;

		[FieldOffset(1)]
		public byte Block;

		[FieldOffset(2)]
		public ushort Gen;

		[FieldOffset(4)]
		public ushort Type;

		[FieldOffset(6)]
		public ushort Array;
	}

	[StructLayout(LayoutKind.Explicit)]
	private struct ObjectFree
	{
		public const int SIZE = 8;

		public const int WORDS = 1;

		[FieldOffset(4)]
		public int Next;
	}

	private struct Block
	{
		public byte Index;

		public unsafe Block* Prev;

		public unsafe Block* Next;

		public unsafe Page* Pages;

		public PageList PagesFree;

		public unsafe byte* Memory;
	}

	private struct Page
	{
		public unsafe Block* Block;

		public int Index;

		public unsafe Page* Prev;

		public unsafe Page* Next;

		public int Bin;

		public int Use;

		public unsafe byte* Memory;

		public unsafe ObjectFree* ObjectsFree;

		public int ObjectsFreeCount;

		public int ObjectsComitted;

		public int ObjectsAllocated;
	}

	private struct Bin
	{
		public int Index;

		public PageList Pages;

		public int ObjectWords;

		public int ObjectStride;

		public int ObjectCapacity;
	}

	private static class BinSizes
	{
		public static readonly int[] Sizes = new int[49]
		{
			0, 1, 2, 3, 4, 5, 6, 7, 8, 10,
			12, 14, 16, 20, 24, 28, 32, 40, 48, 56,
			64, 80, 96, 112, 128, 160, 192, 224, 256, 320,
			384, 448, 512, 640, 768, 896, 1024, 1280, 1536, 1792,
			2048, 2560, 3072, 3584, 4096, 5120, 6144, 7168, 8192
		};
	}

	internal const int WORD_SHIFT = 3;

	internal const int WORD_SIZE = 8;

	internal const int PAGE_SHIFT = 15;

	internal const int PAGE_SIZE = 32768;

	internal const int PAGE_WORD_COUNT = 4096;

	internal const int BIN_COUNT = 49;

	internal const int MAX_BLOCK_COUNT = 255;

	private BlockList _blocksFreePages;

	private unsafe Block** _blocks;

	private int _blocksUsed;

	private unsafe Bin* _bins;

	private unsafe int* _typeMap;

	private int _typeMapLength;

	private unsafe int* _typeMapStrides;

	private ushort _gcGen;

	private int _gcBlock;

	private int _gcBlockPage;

	private Phase _gcPhase;

	private unsafe Object** _gcStack;

	private int _gcStackCount;

	private int _gcStackCapacity;

	private Config _config;

	private unsafe Object** _rootList;

	private int _rootListCapacity;

	private int _rootListCount;

	private int _objectsAllocated;

	private int _memoryAllocated;

	private static Dictionary<Type, TypeData> _types = null;

	private static Dictionary<ushort, TypeData> _typesByOffset = null;

	private static byte[] _debruijnTable = new byte[32]
	{
		0, 9, 1, 10, 13, 21, 2, 29, 11, 14,
		16, 18, 22, 25, 3, 30, 8, 12, 20, 28,
		15, 17, 24, 7, 19, 27, 23, 6, 26, 5,
		4, 31
	};

	internal int MemoryReserved => (_blocksUsed - 1) * _config.BlockPageCount * 32768;

	internal double MemoryAllocated => Math.Round((double)_memoryAllocated / 1024.0 / 1024.0, 3);

	internal int ObjectsAllocated => _objectsAllocated;

	internal int GCRoots => _rootListCount;

	internal Phase GCPhase => _gcPhase;

	internal unsafe static void Destroy(DynamicHeap* heap)
	{
		if (heap != null)
		{
			for (int i = 1; i < heap->_blocksUsed; i++)
			{
				Destroy(heap->_blocks[i]);
			}
			Native.Free(ref heap->_bins);
			Native.Free(ref heap->_blocks);
			Native.Free(ref heap->_gcStack);
			Native.Free(ref heap->_rootList);
			Native.Free(ref heap->_typeMap);
			Native.Free(ref heap->_typeMapStrides);
			Native.Free(ref heap);
		}
	}

	private unsafe static void Destroy(Block* block)
	{
		Native.Free(ref block->Pages);
		Native.Free(ref block->Memory);
		Native.Free(ref block);
	}

	internal unsafe static DynamicHeap* Create(params Type[] types)
	{
		return Create(Config.Default, types);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static T* SetForcedAlive<T>(T* ptr) where T : unmanaged
	{
		Assert.Check(_types.ContainsKey(typeof(T)));
		if (ptr != null)
		{
			Object* ptr2 = (Object*)((byte*)ptr - 8);
			Assert.Check(ptr2->Type > 0);
			ptr2->Flags |= ObjectFlags.ForceAlive;
		}
		return ptr;
	}

	internal unsafe static DynamicHeap* Create(Config config, params Type[] types)
	{
		RegisterTypes(types);
		DynamicHeap* ptr = Native.MallocAndClear<DynamicHeap>();
		ptr->_blocks = Native.MallocAndClearPtrArray<Block>(255);
		ptr->_blocksUsed = 1;
		ptr->_gcGen = 1;
		ptr->_config = config;
		ptr->_bins = Native.MallocAndClearArray<Bin>(49);
		ptr->_gcStackCapacity = 1024;
		ptr->_gcStack = Native.MallocAndClearPtrArray<Object>(ptr->_gcStackCapacity);
		ptr->_rootListCount = 0;
		ptr->_rootListCapacity = 1024;
		ptr->_rootList = Native.MallocAndClearPtrArray<Object>(ptr->_rootListCapacity);
		for (int i = 0; i < 49; i++)
		{
			Bin* ptr2 = ptr->_bins + i;
			ptr2->Index = i;
			ptr2->ObjectWords = BinSizes.Sizes[ptr2->Index];
			ptr2->ObjectStride = ptr2->ObjectWords * 8;
			ptr2->ObjectCapacity = ((ptr2->ObjectWords > 0) ? (4096 / ptr2->ObjectWords) : 0);
		}
		ushort maxOffset = _types.Max((KeyValuePair<Type, TypeData> x) => x.Value.Offset);
		ushort offset = _types.First((KeyValuePair<Type, TypeData> x) => x.Value.Offset == maxOffset).Value.Offset;
		ptr->_typeMapLength = maxOffset + offset;
		ptr->_typeMap = Native.MallocAndClearArray<int>(ptr->_typeMapLength);
		ptr->_typeMapStrides = Native.MallocAndClearArray<int>(ptr->_typeMapLength);
		foreach (TypeData item in from x in _types
			select x.Value into x
			orderby x.Offset
			select x)
		{
			Assert.Check(item.Offset < ptr->_typeMapLength);
			Assert.Check(ptr->_typeMap[(int)item.Offset] == 0);
			ptr->_typeMap[(int)item.Offset] = item.Pointers.Length;
			ptr->_typeMapStrides[(int)item.Offset] = item.Stride;
			for (int j = 0; j < item.Pointers.Length; j++)
			{
				Assert.Check(item.Offset + 1 + j < ptr->_typeMapLength);
				Assert.Check(ptr->_typeMap[item.Offset + 1 + j] == 0);
				ptr->_typeMap[item.Offset + 1 + j] = item.Pointers[j];
			}
		}
		return ptr;
	}

	private unsafe static ushort NextGen(DynamicHeap* heap)
	{
		ushort num = ++heap->_gcGen;
		if (num == 0)
		{
			num = ++heap->_gcGen;
		}
		Assert.Check(heap->_gcGen != 0);
		return num;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private unsafe static Bin* GetBinByIndex(DynamicHeap* heap, int binIndex)
	{
		Assert.Check(binIndex > 0 && binIndex < 49);
		return heap->_bins + binIndex;
	}

	private unsafe static int GetBinIndexForSize(DynamicHeap* heap, int size)
	{
		int bin = GetBin(size);
		Assert.Check(bin > 0 && bin < 49);
		Bin* ptr = heap->_bins + bin;
		Assert.Check(ptr->ObjectStride >= size);
		Assert.Check(ptr->ObjectWords >= WordCount(size));
		Bin* ptr2 = heap->_bins + (bin - 1);
		Assert.Check(ptr2->ObjectStride < size);
		Assert.Check(ptr2->ObjectWords < WordCount(size));
		return bin;
	}

	private unsafe static byte* AllocateInternal(DynamicHeap* heap, int size, out byte block)
	{
		if (size < 8 || size >= 32768)
		{
			throw new InvalidOperationException($"invalid size {size}");
		}
		Assert.Check(WordCount(size) * 8 >= size);
		int num = GetBinIndexForSize(heap, size);
		bool flag = false;
		Bin* binByIndex;
		while (true)
		{
			binByIndex = GetBinByIndex(heap, num);
			Assert.Check(binByIndex->ObjectStride >= size);
			Assert.Check(binByIndex->ObjectWords >= WordCount(size));
			if (binByIndex->Pages.Head != null)
			{
				byte* ptr = TryAllocateFromPage(heap, binByIndex->Pages.Head, size, out block);
				if (ptr != null)
				{
					heap->_objectsAllocated++;
					heap->_memoryAllocated += binByIndex->ObjectStride;
					return ptr;
				}
			}
			if (flag)
			{
				num = GetBinIndexForSize(heap, size);
				binByIndex = GetBinByIndex(heap, num);
				Assert.Check(binByIndex->ObjectStride >= size);
				Assert.Check(binByIndex->ObjectWords >= WordCount(size));
				break;
			}
			if (num + 1 >= 49)
			{
				break;
			}
			num++;
			flag = true;
		}
		Assert.Check(PagesWithAvailableObjectsInBin(binByIndex) == 0);
		Page* ptr2 = AllocatePage(heap);
		Assert.Check(ptr2->ObjectsFree == null);
		Assert.Check(ptr2->ObjectsComitted == 0);
		Assert.Check(ptr2->ObjectsAllocated == 0);
		Assert.Check(ptr2->Prev == null);
		Assert.Check(ptr2->Next == null);
		Assert.Check(ptr2->Bin == 0);
		ptr2->Bin = binByIndex->Index;
		binByIndex->Pages.AddFirst(ptr2);
		byte* ptr3 = TryAllocateFromPage(heap, binByIndex->Pages.Head, size, out block);
		if (ptr3 == null)
		{
			ThrowHeapCorrupted();
		}
		heap->_objectsAllocated++;
		heap->_memoryAllocated += binByIndex->ObjectStride;
		return ptr3;
	}

	private unsafe static Page* AllocatePage_Internal(DynamicHeap* heap, bool mustSucceed)
	{
		Block* head = heap->_blocksFreePages.Head;
		if (head != null && head->PagesFree.Head != null)
		{
			Page* ptr = head->PagesFree.RemoveHead();
			ptr->Use++;
			if (head->PagesFree.Head == null)
			{
				heap->_blocksFreePages.Remove(head);
			}
			return ptr;
		}
		if (mustSucceed)
		{
			ThrowHeapCorrupted();
		}
		Assert.Check(BlocksWithAvailablePages(heap) == 0);
		AllocateBlock(heap);
		return AllocatePage_Internal(heap, mustSucceed: true);
	}

	private unsafe static Page* AllocatePage(DynamicHeap* heap)
	{
		return AllocatePage_Internal(heap, mustSucceed: false);
	}

	private unsafe static void AllocateBlock(DynamicHeap* heap)
	{
		if (heap->_blocksUsed == 255)
		{
			throw new OutOfMemoryException();
		}
		if (heap->_blocksUsed > 255)
		{
			ThrowHeapCorrupted();
		}
		Block* ptr = Native.MallocAndClear<Block>();
		ptr->Memory = (byte*)Native.MallocAndClear(heap->_config.BlockPageCount * 32768);
		ptr->Pages = Native.MallocAndClearArray<Page>(heap->_config.BlockPageCount);
		ptr->Index = (byte)heap->_blocksUsed++;
		for (int i = 0; i < heap->_config.BlockPageCount; i++)
		{
			Page* ptr2 = ptr->Pages + i;
			ptr2->Memory = ptr->Memory + i * 32768;
			ptr2->Block = ptr;
			ptr2->Index = i;
			ptr->PagesFree.AddLast(ptr2);
		}
		heap->_blocks[(int)ptr->Index] = ptr;
		heap->_blocksFreePages.AddFirst(ptr);
	}

	private unsafe static int BlocksWithAvailablePages(DynamicHeap* heap)
	{
		int num = 0;
		for (int i = 1; i < heap->_blocksUsed; i++)
		{
			if (heap->_blocks[i]->PagesFree.Head != null)
			{
				num++;
			}
		}
		return num;
	}

	private unsafe static int PagesWithAvailableObjectsInBin(Bin* bin)
	{
		int num = 0;
		for (Page* ptr = bin->Pages.Head; ptr != null; ptr = ptr->Next)
		{
			if (ptr->ObjectsAllocated < bin->ObjectCapacity)
			{
				num++;
			}
		}
		return num;
	}

	private unsafe static int ObjectsFreeCount(Page* p)
	{
		ObjectFree* ptr = p->ObjectsFree;
		int num = 0;
		while (ptr != null)
		{
			num++;
			if (ptr->Next == 0)
			{
				break;
			}
			ptr = ResolvePageOffset(p, ptr->Next);
		}
		return num;
	}

	private unsafe static byte* TryAllocateFromPage(DynamicHeap* heap, Page* page, int size, out byte block)
	{
		Bin* binByIndex = GetBinByIndex(heap, page->Bin);
		Assert.Check(binByIndex->Index == page->Bin);
		Assert.Check(binByIndex->Pages.Contains(page));
		Assert.Check(binByIndex->ObjectStride >= size);
		void* ptr;
		if (page->ObjectsFree != null)
		{
			Assert.Check(page->ObjectsComitted > 0);
			Assert.Check(page->ObjectsFreeCount > 0);
			ptr = page->ObjectsFree;
			int next = page->ObjectsFree->Next;
			page->ObjectsFree->Next = 0;
			page->ObjectsFreeCount--;
			if (next != 0)
			{
				page->ObjectsFree = ResolvePageOffset(page, next);
			}
			else
			{
				page->ObjectsFree = default(ObjectFree*);
			}
			block = page->Block->Index;
			Assert.Check(IsPtrInBlock(heap, page->Block, ptr));
		}
		else if (page->ObjectsComitted < binByIndex->ObjectCapacity)
		{
			ptr = page->Memory + page->ObjectsComitted++ * binByIndex->ObjectStride;
			Assert.Check(page->ObjectsComitted <= binByIndex->ObjectCapacity);
			Assert.Check(IsPtrInBlock(heap, page->Block, ptr));
			block = page->Block->Index;
		}
		else
		{
			ptr = null;
			block = 0;
		}
		if (ptr != null)
		{
			Assert.Check(IsPtrInBlock(heap, page->Block, ptr));
			Assert.Check(page->ObjectsAllocated < binByIndex->ObjectCapacity);
			if (++page->ObjectsAllocated == binByIndex->ObjectCapacity)
			{
				binByIndex->Pages.MoveLast(page);
			}
			Assert.Check(ObjectsFreeCount(page) + page->ObjectsAllocated == page->ObjectsComitted, ObjectsFreeCount(page), page->ObjectsAllocated, page->ObjectsComitted, page->ObjectsFreeCount);
			Native.MemClear(ptr, size);
		}
		return (byte*)ptr;
	}

	internal static void RegisterTypes(params Type[] types)
	{
		if (_types != null)
		{
			Assert.Check(_typesByOffset != null);
			return;
		}
		_types = new Dictionary<Type, TypeData>();
		_typesByOffset = new Dictionary<ushort, TypeData>();
		foreach (Type type in types)
		{
			if (!UnsafeUtility.IsUnmanaged(type))
			{
				throw new Exception($"type {type} is not an unmanaged struct");
			}
			_types.Add(type, new TypeData
			{
				Type = type
			});
		}
		int num = 1;
		foreach (KeyValuePair<Type, TypeData> type2 in _types)
		{
			List<int> list = new List<int>();
			FieldInfo[] fields = type2.Value.Type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			Assert.Check(UnsafeUtility.SizeOf(type2.Key) % 8 == 0);
			type2.Value.Offset = (ushort)num;
			type2.Value.Stride = UnsafeUtility.SizeOf(type2.Key) / 8;
			FieldInfo[] array = fields;
			foreach (FieldInfo fieldInfo in array)
			{
				if (fieldInfo.FieldType.IsPointer && _types.ContainsKey(fieldInfo.FieldType.GetElementType()) && fieldInfo.GetCustomAttribute<Ignore>() == null)
				{
					int fieldOffset = UnsafeUtility.GetFieldOffset(fieldInfo);
					if (fieldOffset % 8 != 0)
					{
						throw new Exception("field " + type2.Key.Name + "." + fieldInfo.Name + " is not on an 8 byte offset, can't perform tracking for this pointer");
					}
					list.Add(fieldOffset / 8);
				}
			}
			type2.Value.Pointers = list.ToArray();
			_typesByOffset.Add(type2.Value.Offset, type2.Value);
			num += type2.Value.Pointers.Length + 1;
			Assert.Check(num < 65535);
		}
	}

	private static ushort GetTypeOffset<T>() where T : unmanaged
	{
		return _types[typeof(T)].Offset;
	}

	private unsafe static bool IsPtrInBlock(DynamicHeap* heap, Block* block, void* p)
	{
		return p >= block->Memory && p < block->Memory + heap->_config.BlockPageCount * 32768;
	}

	private unsafe static Page* GetPageForPtr(DynamicHeap* heap, Block* block, void* ptr)
	{
		Assert.Check((byte*)ptr - block->Memory >> 15 >= 0);
		Assert.Check((byte*)ptr - block->Memory >> 15 < heap->_config.BlockPageCount);
		long num = (byte*)ptr - block->Memory >> 15;
		Assert.Check(block->Pages[num].Index == num);
		return block->Pages + num;
	}

	private unsafe static int GetPageOffset(Page* page, ObjectFree* obj)
	{
		if (obj == null)
		{
			return 0;
		}
		Assert.Check(obj >= page->Memory);
		int num = (int)((byte*)obj - page->Memory);
		Assert.Check(num < 32768);
		return num + 1;
	}

	private unsafe static ObjectFree* ResolvePageOffset(Page* page, int offset)
	{
		if (offset == 0)
		{
			return null;
		}
		return (ObjectFree*)(page->Memory + (offset - 1));
	}

	private unsafe static void FreeInternal(DynamicHeap* heap, void* ptr, Object objData)
	{
		Assert.Check(objData.Type > 0);
		Assert.Check(objData.Block >= 1 && objData.Block < byte.MaxValue);
		Block* ptr2 = heap->_blocks[(int)objData.Block];
		Assert.Check(IsPtrInBlock(heap, ptr2, ptr), (long)ptr, (long)ptr2->Memory);
		Page* pageForPtr = GetPageForPtr(heap, ptr2, ptr);
		((ObjectFree*)ptr)->Next = GetPageOffset(pageForPtr, pageForPtr->ObjectsFree);
		pageForPtr->ObjectsFree = (ObjectFree*)ptr;
		pageForPtr->ObjectsFreeCount++;
		Bin* binByIndex = GetBinByIndex(heap, pageForPtr->Bin);
		heap->_objectsAllocated--;
		heap->_memoryAllocated -= binByIndex->ObjectStride;
		if (--pageForPtr->ObjectsAllocated == 0)
		{
			binByIndex->Pages.Remove(pageForPtr);
			pageForPtr->Bin = 0;
			pageForPtr->ObjectsFree = default(ObjectFree*);
			pageForPtr->ObjectsAllocated = 0;
			pageForPtr->ObjectsComitted = 0;
			ptr2->PagesFree.AddLast(pageForPtr);
			if (ptr2->PagesFree.Head == pageForPtr)
			{
				heap->_blocksFreePages.AddLast(ptr2);
			}
		}
		else
		{
			Assert.Check(pageForPtr->ObjectsAllocated > 0);
			if (binByIndex->ObjectCapacity == pageForPtr->ObjectsAllocated + 1)
			{
				binByIndex->Pages.MoveFirst(pageForPtr);
			}
		}
	}

	public unsafe static void Free(DynamicHeap* heap, void* ptr)
	{
		Object* ptr2 = (Object*)((byte*)ptr - 8);
		Object objData = *ptr2;
		if ((objData.Flags & ObjectFlags.Root) == ObjectFlags.Root)
		{
			bool flag = false;
			for (int i = 0; i < heap->_rootListCount; i++)
			{
				if (heap->_rootList[i] == ptr2)
				{
					if (i < --heap->_rootListCount)
					{
						heap->_rootList[i] = heap->_rootList[heap->_rootListCount];
						heap->_rootList[heap->_rootListCount] = null;
					}
					else
					{
						heap->_rootList[i] = null;
					}
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				ThrowHeapCorrupted();
			}
		}
		else if ((objData.Flags & ObjectFlags.Tracked) == ObjectFlags.Tracked)
		{
			throw new InvalidOperationException("Can't manually free a tracked object that isn't a root");
		}
		*ptr2 = default(Object);
		FreeInternal(heap, ptr2, objData);
	}

	internal unsafe static void* Allocate(DynamicHeap* heap, int size)
	{
		Assert.Always(size > 0, "array > 0");
		int num = 8;
		byte block;
		byte* ptr = AllocateInternal(heap, size + num, out block);
		Object* ptr2 = (Object*)ptr;
		*ptr2 = default(Object);
		ptr2->Gen = heap->_gcGen;
		ptr2->Block = block;
		ptr2->Type = ushort.MaxValue;
		return ptr + num;
	}

	internal unsafe static int GetArrayLength(void* ptr)
	{
		Object* ptr2 = (Object*)((byte*)ptr - 8);
		Assert.Check(ptr2->Array != 0);
		Assert.Check(ptr2->Type != 0);
		return ptr2->Array;
	}

	internal unsafe static T* AllocateTracked<T>(DynamicHeap* heap, ushort array = 1, bool root = false) where T : unmanaged
	{
		Assert.Always(array > 0, "array > 0");
		byte block;
		byte* ptr = AllocateInternal(heap, sizeof(T) * array + 8, out block);
		Object* ptr2 = (Object*)ptr;
		if (root)
		{
			InitRoot(heap, ptr2);
		}
		InitObj(heap, ptr2, GetTypeOffset<T>(), array, block);
		if (_types[typeof(T)].Pointers.Length == 0)
		{
			ptr2->Flags |= ObjectFlags.Simple;
		}
		return (T*)(ptr + 8);
	}

	internal unsafe static T** AllocateTrackedPointerArray<T>(DynamicHeap* heap, ushort array, bool root = false) where T : unmanaged
	{
		Assert.Always(array > 0, "array > 0");
		byte block;
		byte* ptr = AllocateInternal(heap, sizeof(T*) * array + 8, out block);
		Object* ptr2 = (Object*)ptr;
		if (root)
		{
			InitRoot(heap, ptr2);
		}
		InitObj(heap, ptr2, GetTypeOffset<T>(), array, block);
		ptr2->Flags |= ObjectFlags.Pointer;
		return (T**)(ptr + 8);
	}

	private static void ThrowHeapCorrupted()
	{
		throw new Exception("Heap Corrupted");
	}

	private unsafe static void InitObj(DynamicHeap* heap, Object* obj, ushort type, ushort array, byte block)
	{
		obj->Gen = heap->_gcGen;
		obj->Type = type;
		obj->Block = block;
		obj->Array = array;
		obj->Flags |= ObjectFlags.Tracked;
		obj->Flags |= ObjectFlags.ForceAlive;
	}

	private unsafe static void InitRoot(DynamicHeap* heap, Object* obj)
	{
		if (heap->_rootListCount == heap->_rootListCapacity)
		{
			heap->_rootList = Native.DoublePtrArray(heap->_rootList, heap->_rootListCapacity);
			heap->_rootListCapacity *= 2;
		}
		heap->_rootList[heap->_rootListCount++] = obj;
		obj->Flags = ObjectFlags.Root;
	}

	private unsafe static void ExpandStack(DynamicHeap* heap)
	{
		heap->_gcStack = Native.DoublePtrArray(heap->_gcStack, heap->_gcStackCapacity);
		heap->_gcStackCapacity *= 2;
	}

	[MonoPInvokeCallback(typeof(CollectGarbageDelegate))]
	public unsafe static void CollectGarbage(DynamicHeap* heap, void** dynamicRoots, int dynamicRootsLength)
	{
		ushort num = heap->_gcGen;
		if (heap->_gcPhase == Phase.Idle)
		{
			num = NextGen(heap);
			heap->_gcStackCount = heap->_rootListCount;
			while (heap->_gcStackCapacity < heap->_gcStackCount)
			{
				ExpandStack(heap);
			}
			Native.MemCpy(heap->_gcStack, heap->_rootList, sizeof(Object*) * heap->_gcStackCount);
			for (int i = 0; i < dynamicRootsLength; i++)
			{
				if (heap->_gcStackCount == heap->_gcStackCapacity)
				{
					ExpandStack(heap);
				}
				heap->_gcStack[heap->_gcStackCount++] = (Object*)((byte*)dynamicRoots[i] - 8);
			}
			heap->_gcPhase = Phase.Mark;
		}
		if (heap->_gcPhase == Phase.Mark)
		{
			int* typeMap = heap->_typeMap;
			int* typeMapStrides = heap->_typeMapStrides;
			while (heap->_gcStackCount > 0)
			{
				Object* ptr = heap->_gcStack[--heap->_gcStackCount];
				if (ptr->Gen == num || ptr->Type == ushort.MaxValue)
				{
					continue;
				}
				ptr->Gen = num;
				ObjectFlags* flags = &ptr->Flags;
				*flags &= ~ObjectFlags.ForceAlive;
				ushort type = ptr->Type;
				byte** ptr2 = (byte**)((byte*)ptr + 8);
				int array = ptr->Array;
				if ((ptr->Flags & ObjectFlags.Pointer) == ObjectFlags.Pointer)
				{
					for (int j = 0; j < array; j++)
					{
						byte* ptr3 = ptr2[j];
						if (ptr3 == null)
						{
							continue;
						}
						Object* ptr4 = (Object*)(ptr3 - 8);
						if (ptr4->Type == ushort.MaxValue || ptr4->Gen == num)
						{
							continue;
						}
						if (typeMap[(int)ptr4->Type] == 0)
						{
							Assert.Check((ptr4->Flags & ObjectFlags.Simple) == ObjectFlags.Simple);
							ptr4->Gen = num;
							continue;
						}
						Assert.Check((ptr4->Flags & ObjectFlags.Simple) == 0);
						if (heap->_gcStackCount == heap->_gcStackCapacity)
						{
							ExpandStack(heap);
						}
						heap->_gcStack[heap->_gcStackCount++] = ptr4;
					}
					continue;
				}
				int num2 = typeMap[(int)type];
				for (int k = 0; k < array; k++)
				{
					for (int l = 1; l <= num2; l++)
					{
						byte* ptr5 = ptr2[typeMap[type + l]];
						if (ptr5 == null)
						{
							continue;
						}
						Object* ptr6 = (Object*)(ptr5 - 8);
						if (ptr6->Type == ushort.MaxValue || ptr6->Gen == num)
						{
							continue;
						}
						if (typeMap[(int)ptr6->Type] == 0)
						{
							Assert.Check((ptr6->Flags & ObjectFlags.Simple) == ObjectFlags.Simple);
							ptr6->Gen = num;
							continue;
						}
						Assert.Check((ptr6->Flags & ObjectFlags.Simple) == 0);
						if (heap->_gcStackCount == heap->_gcStackCapacity)
						{
							ExpandStack(heap);
						}
						heap->_gcStack[heap->_gcStackCount++] = ptr6;
					}
					if (ptr->Array > 1)
					{
						ptr2 += typeMapStrides[(int)type];
					}
				}
			}
			heap->_gcBlock = 1;
			heap->_gcBlockPage = 0;
			heap->_gcPhase = Phase.Sweep;
		}
		if (heap->_gcPhase == Phase.Sweep)
		{
			while (heap->_gcBlock < heap->_blocksUsed)
			{
				Block* ptr7 = heap->_blocks[heap->_gcBlock];
				while (heap->_gcBlockPage < heap->_config.BlockPageCount)
				{
					Page* ptr8 = ptr7->Pages + heap->_gcBlockPage;
					if (ptr8->ObjectsComitted > 0)
					{
						Bin* binByIndex = GetBinByIndex(heap, ptr8->Bin);
						for (int m = 0; m < ptr8->ObjectsComitted; m++)
						{
							Object* ptr9 = (Object*)(ptr8->Memory + m * binByIndex->ObjectStride);
							Object @object = *ptr9;
							if ((@object.Flags & ObjectFlags.Tracked) != ObjectFlags.Tracked || @object.Gen == num)
							{
								continue;
							}
							if ((@object.Flags & ObjectFlags.Root) == ObjectFlags.Root)
							{
								ThrowHeapCorrupted();
							}
							if ((@object.Flags & ObjectFlags.ForceAlive) == ObjectFlags.ForceAlive)
							{
								ObjectFlags* flags2 = &ptr9->Flags;
								*flags2 &= ~ObjectFlags.ForceAlive;
								ptr9->Gen = num;
								continue;
							}
							if (heap->_gcStackCount == heap->_gcStackCapacity)
							{
								ExpandStack(heap);
							}
							heap->_gcStack[heap->_gcStackCount++] = ptr9;
							ptr9->Flags |= ObjectFlags.Garbage;
						}
					}
					heap->_gcBlockPage++;
				}
				heap->_gcBlockPage = 0;
				heap->_gcBlock++;
			}
			heap->_gcPhase = Phase.Free;
		}
		if (heap->_gcPhase != Phase.Free)
		{
			return;
		}
		while (heap->_gcStackCount > 0)
		{
			Object* ptr10 = heap->_gcStack[--heap->_gcStackCount];
			Object objData = *ptr10;
			Assert.Check((objData.Flags & ObjectFlags.Garbage) == ObjectFlags.Garbage);
			if ((objData.Flags & ObjectFlags.ForceAlive) == 0)
			{
				*ptr10 = default(Object);
				FreeInternal(heap, ptr10, objData);
			}
			else
			{
				ObjectFlags* flags3 = &ptr10->Flags;
				*flags3 &= ~ObjectFlags.ForceAlive;
			}
		}
		heap->_gcPhase = Phase.Idle;
	}

	private static int GetBin(int size)
	{
		Assert.Check(size > 0);
		int num = WordCount(size);
		if (num <= 8)
		{
			return num;
		}
		num--;
		int num2 = BitScan((uint)num);
		int num3 = (num2 << 2) + ((num >> num2 - 2) & 3) - 3;
		Assert.Check(num3 >= 0 && num3 < 49);
		return num3;
	}

	private static int WordCount(int size)
	{
		Assert.Check(size > 0);
		return (size + 7) / 8;
	}

	private static int BitScan(uint v)
	{
		v |= v >> 1;
		v |= v >> 2;
		v |= v >> 4;
		v |= v >> 8;
		v |= v >> 16;
		return _debruijnTable[v * 130329821 >> 27];
	}
}
