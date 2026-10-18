#define DEBUG
namespace Fusion.Sockets;

internal struct NetBitBufferList
{
	public int Count;

	public unsafe NetBitBuffer* Head;

	public unsafe NetBitBuffer* Tail;

	public unsafe void AddFirst(NetBitBuffer* item)
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

	public unsafe void AddLast(NetBitBuffer* item)
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

	public unsafe NetBitBuffer* RemoveHead()
	{
		Assert.Check(Count > 0);
		Assert.Check(Head != null);
		Assert.Check(IsInList(Head));
		NetBitBuffer* head = Head;
		Remove(head);
		return head;
	}

	public unsafe void Remove(NetBitBuffer* item)
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

	private unsafe bool IsInList(NetBitBuffer* item)
	{
		for (NetBitBuffer* ptr = Head; ptr != null; ptr = ptr->Next)
		{
			if (ptr == item)
			{
				return true;
			}
		}
		return false;
	}
}
