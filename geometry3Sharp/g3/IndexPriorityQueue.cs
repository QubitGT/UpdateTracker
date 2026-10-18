using System;
using System.Collections;
using System.Collections.Generic;

namespace g3;

public class IndexPriorityQueue : IEnumerable<int>, IEnumerable
{
	private struct QueueNode
	{
		public int id;

		public float priority;

		public int index;
	}

	public bool EnableDebugChecks;

	private DVector<QueueNode> nodes;

	private int num_nodes;

	private int[] id_to_index;

	public int Count => num_nodes;

	public int First => nodes[1].id;

	public float FirstPriority => nodes[1].priority;

	public IndexPriorityQueue(int maxID)
	{
		nodes = new DVector<QueueNode>();
		id_to_index = new int[maxID];
		for (int i = 0; i < maxID; i++)
		{
			id_to_index[i] = -1;
		}
		num_nodes = 0;
	}

	public void Clear(bool bFreeMemory = true)
	{
		if (bFreeMemory)
		{
			nodes = new DVector<QueueNode>();
		}
		Array.Clear(id_to_index, 0, id_to_index.Length);
		num_nodes = 0;
	}

	public bool Contains(int id)
	{
		int num = id_to_index[id];
		if (num <= 0 || num > num_nodes)
		{
			return false;
		}
		return nodes[num].index > 0;
	}

	public void Insert(int id, float priority)
	{
		if (EnableDebugChecks && Contains(id))
		{
			throw new Exception("IndexPriorityQueue.Insert: tried to add node that is already in queue!");
		}
		QueueNode value = default(QueueNode);
		value.id = id;
		value.priority = priority;
		num_nodes++;
		value.index = num_nodes;
		id_to_index[id] = value.index;
		nodes.insert(value, num_nodes);
		move_up(nodes[num_nodes].index);
	}

	public void Enqueue(int id, float priority)
	{
		Insert(id, priority);
	}

	public int Dequeue()
	{
		if (EnableDebugChecks && Count == 0)
		{
			throw new Exception("IndexPriorityQueue.Dequeue: queue is empty!");
		}
		int id = nodes[1].id;
		remove_at_index(1);
		return id;
	}

	public void Remove(int id)
	{
		if (EnableDebugChecks && !Contains(id))
		{
			throw new Exception("IndexPriorityQueue.Remove: tried to remove node that does not exist in queue!");
		}
		int iNode = id_to_index[id];
		remove_at_index(iNode);
	}

	public void Update(int id, float priority)
	{
		if (EnableDebugChecks && !Contains(id))
		{
			throw new Exception("IndexPriorityQueue.Update: tried to update node that does not exist in queue!");
		}
		int num = id_to_index[id];
		QueueNode value = nodes[num];
		value.priority = priority;
		nodes[num] = value;
		on_node_updated(num);
	}

	public float GetPriority(int id)
	{
		if (EnableDebugChecks && !Contains(id))
		{
			throw new Exception("IndexPriorityQueue.Update: tried to get priorty of node that does not exist in queue!");
		}
		int i = id_to_index[id];
		return nodes[i].priority;
	}

	public IEnumerator<int> GetEnumerator()
	{
		for (int i = 1; i <= num_nodes; i++)
		{
			yield return nodes[i].id;
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	private void remove_at_index(int iNode)
	{
		if (iNode == num_nodes)
		{
			nodes[iNode] = default(QueueNode);
			num_nodes--;
			return;
		}
		swap_nodes_at_indices(iNode, num_nodes);
		nodes[num_nodes] = default(QueueNode);
		num_nodes--;
		on_node_updated(iNode);
	}

	private void swap_nodes_at_indices(int i1, int i2)
	{
		QueueNode value = nodes[i1];
		value.index = i2;
		QueueNode value2 = nodes[i2];
		value2.index = i1;
		nodes[i1] = value2;
		nodes[i2] = value;
		id_to_index[value2.id] = i1;
		id_to_index[value.id] = i2;
	}

	private void move(int iFrom, int iTo)
	{
		QueueNode value = nodes[iFrom];
		value.index = iTo;
		nodes[iTo] = value;
		id_to_index[value.id] = iTo;
	}

	private void set(int iTo, ref QueueNode n)
	{
		n.index = iTo;
		nodes[iTo] = n;
		id_to_index[n.id] = iTo;
	}

	private void move_up(int iNode)
	{
		int num = iNode;
		QueueNode n = nodes[num];
		int num2 = iNode / 2;
		while (num2 >= 1 && !(nodes[num2].priority < n.priority))
		{
			move(num2, iNode);
			iNode = num2;
			num2 = nodes[iNode].index / 2;
		}
		if (iNode != num)
		{
			set(iNode, ref n);
		}
	}

	private void move_down(int iNode)
	{
		int num = iNode;
		QueueNode n = nodes[num];
		while (true)
		{
			int num2 = iNode;
			int num3 = 2 * iNode;
			if (num3 > num_nodes)
			{
				break;
			}
			float num4 = n.priority;
			float priority = nodes[num3].priority;
			if (priority < num4)
			{
				num2 = num3;
				num4 = priority;
			}
			int num5 = num3 + 1;
			if (num5 <= num_nodes && nodes[num5].priority < num4)
			{
				num2 = num5;
			}
			if (num2 == iNode)
			{
				break;
			}
			move(num2, iNode);
			iNode = num2;
		}
		if (iNode != num)
		{
			set(iNode, ref n);
		}
	}

	private void on_node_updated(int iNode)
	{
		int num = iNode / 2;
		if (num > 0 && has_higher_priority(iNode, num))
		{
			move_up(iNode);
		}
		else
		{
			move_down(iNode);
		}
	}

	private bool has_higher_priority(int iHigher, int iLower)
	{
		return nodes[iHigher].priority < nodes[iLower].priority;
	}

	public bool IsValidQueue()
	{
		for (int i = 1; i < num_nodes; i++)
		{
			int num = 2 * i;
			if (num < num_nodes && has_higher_priority(num, i))
			{
				return false;
			}
			int num2 = num + 1;
			if (num2 < num_nodes && has_higher_priority(num2, i))
			{
				return false;
			}
		}
		return true;
	}

	public void DebugPrint()
	{
		for (int i = 1; i <= num_nodes; i++)
		{
			Console.WriteLine("{0} : p {1}  index {2}  id {3}", i, nodes[i].priority, nodes[i].index, nodes[i].id);
		}
	}
}
