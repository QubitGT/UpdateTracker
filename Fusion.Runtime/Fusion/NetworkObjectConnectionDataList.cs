#define DEBUG
namespace Fusion;

internal struct NetworkObjectConnectionDataList
{
	public int Count;

	public NetworkObjectConnectionData Head;

	public NetworkObjectConnectionData Tail;

	public void AddFirst(NetworkObjectConnectionData item)
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

	public void AddLast(NetworkObjectConnectionData item)
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

	public void AddBefore(NetworkObjectConnectionData item, NetworkObjectConnectionData before)
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

	public void AddAfter(NetworkObjectConnectionData item, NetworkObjectConnectionData after)
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

	public NetworkObjectConnectionData RemoveHead()
	{
		Assert.Check(Count > 0);
		Assert.Check(Head != null);
		Assert.Check(IsInList(Head));
		NetworkObjectConnectionData head = Head;
		Remove(head);
		return head;
	}

	public void Remove(NetworkObjectConnectionData item)
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

	private bool IsInList(NetworkObjectConnectionData item)
	{
		if (item == null)
		{
			return false;
		}
		for (NetworkObjectConnectionData networkObjectConnectionData = Head; networkObjectConnectionData != null; networkObjectConnectionData = networkObjectConnectionData.Next)
		{
			if (networkObjectConnectionData == item)
			{
				return true;
			}
		}
		return false;
	}

	public NetworkObjectConnectionDataList RemoveAll()
	{
		NetworkObjectConnectionDataList result = this;
		Head = null;
		Tail = null;
		Count = 0;
		return result;
	}

	public void Concat(ref NetworkObjectConnectionDataList other)
	{
		if (other.Count != 0)
		{
			if (Count == 0)
			{
				Count = other.Count;
				Head = other.Head;
				Tail = other.Tail;
			}
			else
			{
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
			other = default(NetworkObjectConnectionDataList);
		}
	}
}
