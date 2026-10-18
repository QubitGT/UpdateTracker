#define DEBUG
namespace Fusion;

internal class SimulationHistoryEntryList
{
	public int Count;

	public Simulation.History.Entry Head;

	public Simulation.History.Entry Tail;

	public void AddFirst(Simulation.History.Entry item)
	{
		if (item != null)
		{
			Assert.Check(!IsInList(item));
			item.Next = Head;
			item.Prev = null;
			if (Head != null)
			{
				Head.Prev = item;
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

	public void AddLast(Simulation.History.Entry item)
	{
		if (item != null)
		{
			Assert.Check(!IsInList(item));
			item.Next = null;
			item.Prev = Tail;
			if (Tail != null)
			{
				Tail.Next = item;
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

	public void AddBefore(Simulation.History.Entry item, Simulation.History.Entry before)
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
				Assert.Check(before.Prev != null);
				item.Next = before;
				item.Prev = before.Prev;
				before.Prev.Next = item;
				before.Prev = item;
				Count++;
			}
			Assert.Check(IsInList(before));
			Assert.Check(IsInList(item));
		}
	}

	public void AddAfter(Simulation.History.Entry item, Simulation.History.Entry after)
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
				Assert.Check(after.Next != null);
				item.Next = after.Next;
				item.Prev = after;
				after.Next.Prev = item;
				after.Next = item;
				Count++;
			}
			Assert.Check(IsInList(after));
			Assert.Check(IsInList(item));
		}
	}

	public Simulation.History.Entry RemoveHead()
	{
		Assert.Check(Count > 0);
		Assert.Check(Head != null);
		Assert.Check(IsInList(Head));
		Simulation.History.Entry head = Head;
		Remove(head);
		return head;
	}

	public void Remove(Simulation.History.Entry item)
	{
		if (item != null)
		{
			Assert.Check(IsInList(item));
			if (item.Prev != null)
			{
				item.Prev.Next = item.Next;
			}
			if (item.Next != null)
			{
				item.Next.Prev = item.Prev;
			}
			if (item == Tail)
			{
				Tail = item.Prev;
			}
			if (item == Head)
			{
				Head = item.Next;
			}
			item.Prev = null;
			item.Next = null;
			Count--;
		}
	}

	private bool IsInList(Simulation.History.Entry item)
	{
		if (item == null)
		{
			return false;
		}
		for (Simulation.History.Entry entry = Head; entry != null; entry = entry.Next)
		{
			if (entry == item)
			{
				return true;
			}
		}
		return false;
	}

	public SimulationHistoryEntryList RemoveAll()
	{
		Head = null;
		Tail = null;
		Count = 0;
		return this;
	}

	public void Concat(SimulationHistoryEntryList other)
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
			Assert.Check(Tail.Next == null);
			Assert.Check(other.Head != null);
			Assert.Check(other.Head.Prev == null);
			Tail.Next = other.Head;
			other.Head.Prev = Tail;
			Tail = other.Tail;
			Count += other.Count;
		}
	}
}
