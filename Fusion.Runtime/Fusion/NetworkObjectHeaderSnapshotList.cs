#define DEBUG
namespace Fusion;

internal struct NetworkObjectHeaderSnapshotList
{
	public const int SIZE = 24;

	public const int ALIGNMENT = 8;

	private int _count;

	private NetworkObjectHeaderSnapshot _tail;

	private NetworkObjectHeaderSnapshot _head;

	public int Count => _count;

	public NetworkObjectHeaderSnapshot Oldest => _tail;

	public NetworkObjectHeaderSnapshot Latest => _head;

	public void AddFirst(NetworkObjectHeaderSnapshot item)
	{
		Assert.Check(!IsInList(item));
		item.Next = _head;
		item.Prev = null;
		if (_head != null)
		{
			_head.Prev = item;
			_head = item;
		}
		else
		{
			_head = item;
			_tail = item;
		}
		_count++;
	}

	public void AddLast(NetworkObjectHeaderSnapshot item)
	{
		Assert.Check(!IsInList(item));
		item.Next = null;
		item.Prev = _tail;
		if (_tail != null)
		{
			_tail.Next = item;
			_tail = item;
		}
		else
		{
			_head = item;
			_tail = item;
		}
		_count++;
	}

	public void AddBefore(NetworkObjectHeaderSnapshot before, NetworkObjectHeaderSnapshot item)
	{
		Assert.Check(_count > 0);
		Assert.Check(IsInList(before));
		Assert.Check(!IsInList(item));
		if (before == _head)
		{
			AddFirst(item);
		}
		else
		{
			item.Next = before;
			item.Prev = before.Prev;
			before.Prev.Next = item;
			before.Prev = item;
			_count++;
		}
		Assert.Check(IsInList(before));
		Assert.Check(IsInList(item));
	}

	public void AddAfter(NetworkObjectHeaderSnapshot after, NetworkObjectHeaderSnapshot item)
	{
		Assert.Check(_count > 0);
		Assert.Check(IsInList(after));
		Assert.Check(!IsInList(item));
		if (after == _tail)
		{
			AddLast(item);
		}
		else
		{
			item.Next = after.Next;
			item.Prev = after;
			after.Next.Prev = item;
			after.Next = item;
			_count++;
		}
		Assert.Check(IsInList(after));
		Assert.Check(IsInList(item));
	}

	public NetworkObjectHeaderSnapshot RemoveOldest()
	{
		Assert.Check(_count > 0);
		Assert.Check(_tail != null);
		Assert.Check(IsInList(_tail));
		NetworkObjectHeaderSnapshot tail = _tail;
		Remove(tail);
		return tail;
	}

	public NetworkObjectHeaderSnapshot RemoveLatest()
	{
		Assert.Check(_count > 0);
		Assert.Check(_head != null);
		Assert.Check(IsInList(_head));
		NetworkObjectHeaderSnapshot head = _head;
		Remove(head);
		return head;
	}

	public void Remove(NetworkObjectHeaderSnapshot item)
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
		if (item == _tail)
		{
			_tail = item.Prev;
		}
		if (item == _head)
		{
			_head = item.Next;
		}
		item.Prev = null;
		item.Next = null;
		_count--;
	}

	private bool IsInList(NetworkObjectHeaderSnapshot item)
	{
		for (NetworkObjectHeaderSnapshot networkObjectHeaderSnapshot = _head; networkObjectHeaderSnapshot != null; networkObjectHeaderSnapshot = networkObjectHeaderSnapshot.Next)
		{
			if (networkObjectHeaderSnapshot == item)
			{
				return true;
			}
		}
		return false;
	}
}
