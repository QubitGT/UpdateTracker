#define DEBUG
namespace Fusion.Sockets;

public struct ReliableList
{
	public int Count;

	public unsafe ReliableHeader* Head;

	public unsafe ReliableHeader* Tail;

	public unsafe void AddFirst(ReliableHeader* item)
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

	public unsafe void AddLast(ReliableHeader* item)
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

	public unsafe void AddBefore(ReliableHeader* before, ReliableHeader* item)
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

	public unsafe void AddAfter(ReliableHeader* after, ReliableHeader* item)
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

	public unsafe ReliableHeader* RemoveHead()
	{
		Assert.Check(Count > 0);
		Assert.Check(Head != null);
		Assert.Check(IsInList(Head));
		ReliableHeader* head = Head;
		Remove(head);
		return head;
	}

	public unsafe void Remove(ReliableHeader* item)
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

	private unsafe bool IsInList(ReliableHeader* item)
	{
		for (ReliableHeader* ptr = Head; ptr != null; ptr = ptr->Next)
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
			ReliableHeader* memory = RemoveHead();
			Native.Free(ref memory);
		}
		Assert.Check(Head == null);
		Assert.Check(Tail == null);
	}
}
