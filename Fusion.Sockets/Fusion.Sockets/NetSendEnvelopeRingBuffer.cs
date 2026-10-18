#define DEBUG
using System;

namespace Fusion.Sockets;

internal struct NetSendEnvelopeRingBuffer
{
	private unsafe NetSendEnvelope* _items;

	private int _itemsCapacity;

	public int Head;

	public int Tail;

	public int Count;

	public bool IsFull => Count == _itemsCapacity;

	public bool IsEmpty => Count == 0;

	public unsafe void Push(NetSendEnvelope envelope)
	{
		if (Count == _itemsCapacity)
		{
			throw new InvalidOperationException();
		}
		_items[Head] = envelope;
		Head = (Head + 1) % _itemsCapacity;
		Count++;
	}

	public unsafe bool TryPush(NetSendEnvelope envelope)
	{
		if (Count == _itemsCapacity)
		{
			return false;
		}
		_items[Head] = envelope;
		Head = (Head + 1) % _itemsCapacity;
		Count++;
		return true;
	}

	public unsafe NetSendEnvelope Peek()
	{
		Assert.Check(Count > 0);
		return _items[Tail];
	}

	public void Pop()
	{
		Assert.Check(Count > 0);
		Tail = (Tail + 1) % _itemsCapacity;
		Count--;
	}

	public void Reset()
	{
		Head = 0;
		Tail = 0;
		Count = 0;
	}

	public unsafe void Dispose()
	{
		Native.Free(ref _items);
	}

	public unsafe static NetSendEnvelopeRingBuffer Create(int capacity)
	{
		NetSendEnvelopeRingBuffer result = default(NetSendEnvelopeRingBuffer);
		result.Head = 0;
		result.Tail = 0;
		result.Count = 0;
		result._itemsCapacity = capacity;
		result._items = Native.MallocAndClearArray<NetSendEnvelope>(capacity);
		return result;
	}
}
