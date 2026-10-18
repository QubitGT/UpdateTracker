#define DEBUG
namespace Fusion.Sockets;

internal struct NetDelayedPacketList
{
	public int Count;

	public unsafe NetDelayedPacket* Head;

	public unsafe NetDelayedPacket* Tail;

	public unsafe void AddFirst(NetDelayedPacket* item)
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

	public unsafe void AddLast(NetDelayedPacket* item)
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

	public unsafe NetDelayedPacket* RemoveHead()
	{
		Assert.Check(Count > 0);
		Assert.Check(Head != null);
		Assert.Check(IsInList(Head));
		NetDelayedPacket* head = Head;
		Remove(head);
		return head;
	}

	public unsafe void Remove(NetDelayedPacket* item)
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

	private unsafe bool IsInList(NetDelayedPacket* item)
	{
		for (NetDelayedPacket* ptr = Head; ptr != null; ptr = ptr->Next)
		{
			if (ptr == item)
			{
				return true;
			}
		}
		return false;
	}

	public unsafe void Dispose()
	{
		while (Count > 0)
		{
			NetDelayedPacket* memory = RemoveHead();
			Native.Free(ref memory);
		}
		Assert.Check(Head == null);
		Assert.Check(Tail == null);
	}
}
