#define DEBUG
namespace Fusion;

internal struct SimulationMessageList
{
	public int Count;

	public unsafe SimulationMessageEnvelope* Head;

	public unsafe SimulationMessageEnvelope* Tail;

	public unsafe void AddFirst(SimulationMessageEnvelope* item)
	{
		if (item != null)
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
	}

	public unsafe void AddLast(SimulationMessageEnvelope* item)
	{
		if (item != null)
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
	}

	public unsafe void AddBefore(SimulationMessageEnvelope* item, SimulationMessageEnvelope* before)
	{
		if (item != null && before != null)
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
				Assert.Check(Count > 1);
				Assert.Check(before->Prev != null);
				item->Next = before;
				item->Prev = before->Prev;
				before->Prev->Next = item;
				before->Prev = item;
				Count++;
			}
			Assert.Check(IsInList(before));
			Assert.Check(IsInList(item));
		}
	}

	public unsafe void AddAfter(SimulationMessageEnvelope* item, SimulationMessageEnvelope* after)
	{
		if (item != null && after != null)
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
				Assert.Check(Count > 1);
				Assert.Check(after->Next != null);
				item->Next = after->Next;
				item->Prev = after;
				after->Next->Prev = item;
				after->Next = item;
				Count++;
			}
			Assert.Check(IsInList(after));
			Assert.Check(IsInList(item));
		}
	}

	public unsafe SimulationMessageEnvelope* RemoveHead()
	{
		Assert.Check(Count > 0);
		Assert.Check(Head != null);
		Assert.Check(IsInList(Head));
		SimulationMessageEnvelope* head = Head;
		Remove(head);
		return head;
	}

	public unsafe void Remove(SimulationMessageEnvelope* item)
	{
		if (item != null)
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
	}

	private unsafe bool IsInList(SimulationMessageEnvelope* item)
	{
		if (item == null)
		{
			return false;
		}
		for (SimulationMessageEnvelope* ptr = Head; ptr != null; ptr = ptr->Next)
		{
			if (ptr == item)
			{
				return true;
			}
		}
		return false;
	}

	public unsafe SimulationMessageList RemoveAll()
	{
		SimulationMessageList result = this;
		Head = default(SimulationMessageEnvelope*);
		Tail = default(SimulationMessageEnvelope*);
		Count = 0;
		return result;
	}

	public unsafe void Concat(SimulationMessageList other)
	{
		if (other.Count != 0)
		{
			if (Count == 0)
			{
				Count = other.Count;
				Head = other.Head;
				Tail = other.Tail;
				return;
			}
			Assert.Check(!IsInList(other.Head));
			Assert.Check(Tail != null);
			Assert.Check(Tail->Next == null);
			Assert.Check(other.Head != null);
			Assert.Check(other.Head->Prev == null);
			Tail->Next = other.Head;
			other.Head->Prev = Tail;
			Tail = other.Tail;
			Count += other.Count;
		}
	}
}
