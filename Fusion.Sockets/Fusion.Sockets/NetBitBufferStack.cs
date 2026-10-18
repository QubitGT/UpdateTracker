#define DEBUG
using System;

namespace Fusion.Sockets;

internal struct NetBitBufferStack
{
	private int _capacity;

	public unsafe NetBitBuffer** Stack;

	public int Count;

	public unsafe bool TryPop(NetBitBuffer** result)
	{
		Assert.Check(Count >= 0);
		if (Count == 0)
		{
			return false;
		}
		*result = Stack[--Count];
		return true;
	}

	public unsafe static NetBitBufferStack Create(int capacity)
	{
		NetBitBufferStack result = default(NetBitBufferStack);
		result._capacity = capacity;
		result.Stack = Native.MallocAndClearPtrArray<NetBitBuffer>(capacity);
		return result;
	}

	public unsafe static void Dispose(ref NetBitBufferStack stack)
	{
		Native.Free(ref stack.Stack);
		stack.Count = 0;
		stack._capacity = 0;
	}

	public unsafe void PushFromHead(NetBitBuffer* head)
	{
		while (head != null)
		{
			NetBitBuffer* next = head->Next;
			head->Next = null;
			head->Prev = null;
			Assert.Check(Count >= 0 && Count <= _capacity);
			if (Count == _capacity)
			{
				try
				{
					Stack = Native.DoublePtrArray(Stack, _capacity);
				}
				catch (OutOfMemoryException)
				{
					InternalLogStreams.LogInfo?.Log($"OOM resize to _capacity:{_capacity}, Count:{Count}");
					throw;
				}
				_capacity *= 2;
			}
			Stack[Count++] = head;
			head = next;
		}
	}
}
