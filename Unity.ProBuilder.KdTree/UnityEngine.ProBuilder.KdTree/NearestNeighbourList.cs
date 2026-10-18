using System;

namespace UnityEngine.ProBuilder.KdTree;

internal class NearestNeighbourList<TItem, TDistance> : INearestNeighbourList<TItem, TDistance>
{
	private PriorityQueue<TItem, TDistance> queue;

	private ITypeMath<TDistance> distanceMath;

	private int maxCapacity;

	public int MaxCapacity => maxCapacity;

	public int Count => queue.Count;

	public bool IsCapacityReached => Count == MaxCapacity;

	public NearestNeighbourList(int maxCapacity, ITypeMath<TDistance> distanceMath)
	{
		this.maxCapacity = maxCapacity;
		this.distanceMath = distanceMath;
		queue = new PriorityQueue<TItem, TDistance>(maxCapacity, distanceMath);
	}

	public bool Add(TItem item, TDistance distance)
	{
		if (queue.Count >= maxCapacity)
		{
			if (distanceMath.Compare(distance, queue.GetHighestPriority()) < 0)
			{
				queue.Dequeue();
				queue.Enqueue(item, distance);
				return true;
			}
			return false;
		}
		queue.Enqueue(item, distance);
		return true;
	}

	public TItem GetFurtherest()
	{
		if (Count == 0)
		{
			throw new Exception("List is empty");
		}
		return queue.GetHighest();
	}

	public TDistance GetFurtherestDistance()
	{
		if (Count == 0)
		{
			throw new Exception("List is empty");
		}
		return queue.GetHighestPriority();
	}

	public TItem RemoveFurtherest()
	{
		return queue.Dequeue();
	}
}
