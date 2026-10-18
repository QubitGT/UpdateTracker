using System;
using System.Collections;
using System.Collections.Generic;

namespace g3;

public class DynamicPriorityQueue<T> : IEnumerable<T>, IEnumerable where T : DynamicPriorityQueueNode
{
	public bool EnableDebugChecks;

	private DVector<T> nodes;

	private int num_nodes;

	public int Count => num_nodes;

	public T First => nodes[1];

	public float FirstPriority => nodes[1].priority;

	public DynamicPriorityQueue()
	{
		num_nodes = 0;
		nodes = new DVector<T>();
	}

	public void Clear(bool bFreeMemory = true)
	{
		if (bFreeMemory)
		{
			nodes = new DVector<T>();
		}
		num_nodes = 0;
	}

	public bool Contains(T node)
	{
		return nodes[node.index] == node;
	}

	public void Enqueue(T node, float priority)
	{
		if (EnableDebugChecks && Contains(node))
		{
			throw new Exception("DynamicPriorityQueue.Enqueue: tried to add node that is already in queue!");
		}
		node.priority = priority;
		num_nodes++;
		nodes.insert(node, num_nodes);
		node.index = num_nodes;
		move_up(nodes[num_nodes]);
	}

	public T Dequeue()
	{
		if (EnableDebugChecks && Count == 0)
		{
			throw new Exception("DynamicPriorityQueue.Dequeue: queue is empty!");
		}
		T val = nodes[1];
		Remove(val);
		return val;
	}

	public void Remove(T node)
	{
		if (EnableDebugChecks && !Contains(node))
		{
			throw new Exception("DynamicPriorityQueue.Remove: tried to remove node that does not exist in queue!");
		}
		if (node.index == num_nodes)
		{
			nodes[num_nodes] = null;
			num_nodes--;
			return;
		}
		T val = nodes[num_nodes];
		swap_nodes(node, val);
		nodes[num_nodes] = null;
		num_nodes--;
		on_node_updated(val);
	}

	public void Update(T node, float priority)
	{
		if (EnableDebugChecks && !Contains(node))
		{
			throw new Exception("DynamicPriorityQueue.Update: tried to update node that does not exist in queue!");
		}
		node.priority = priority;
		on_node_updated(node);
	}

	public IEnumerator<T> GetEnumerator()
	{
		for (int i = 1; i <= num_nodes; i++)
		{
			yield return nodes[i];
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	private void swap_nodes(T node1, T node2)
	{
		nodes[node1.index] = node2;
		nodes[node2.index] = node1;
		int index = node1.index;
		node1.index = node2.index;
		node2.index = index;
	}

	private void move_up(T node)
	{
		int num = node.index / 2;
		while (num >= 1)
		{
			T val = nodes[num];
			if (!has_higher_priority(val, node))
			{
				swap_nodes(node, val);
				num = node.index / 2;
				continue;
			}
			break;
		}
	}

	private void move_down(T node)
	{
		int num = node.index;
		while (true)
		{
			T val = node;
			int num2 = 2 * num;
			if (num2 > num_nodes)
			{
				node.index = num;
				nodes[num] = node;
				return;
			}
			T val2 = nodes[num2];
			if (has_higher_priority(val2, val))
			{
				val = val2;
			}
			int num3 = num2 + 1;
			if (num3 <= num_nodes)
			{
				T val3 = nodes[num3];
				if (has_higher_priority(val3, val))
				{
					val = val3;
				}
			}
			if (val == node)
			{
				break;
			}
			nodes[num] = val;
			int index = val.index;
			val.index = num;
			num = index;
		}
		node.index = num;
		nodes[num] = node;
	}

	private void on_node_updated(T node)
	{
		int num = node.index / 2;
		T lower = nodes[num];
		if (num > 0 && has_higher_priority(node, lower))
		{
			move_up(node);
		}
		else
		{
			move_down(node);
		}
	}

	private bool has_higher_priority(T higher, T lower)
	{
		return higher.priority < lower.priority;
	}

	public bool IsValidQueue()
	{
		for (int i = 1; i < num_nodes; i++)
		{
			if (nodes[i] != null)
			{
				int num = 2 * i;
				if (num < num_nodes && nodes[num] != null && has_higher_priority(nodes[num], nodes[i]))
				{
					return false;
				}
				int num2 = num + 1;
				if (num2 < num_nodes && nodes[num2] != null && has_higher_priority(nodes[num2], nodes[i]))
				{
					return false;
				}
			}
		}
		return true;
	}

	public void DebugPrint()
	{
		for (int i = 1; i <= num_nodes; i++)
		{
			Console.WriteLine("{0} : p {1}  idx {2}", i, nodes[i].priority, nodes[i].index);
		}
	}
}
