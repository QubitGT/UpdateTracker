using System;
using UnityEngine;

namespace Pathfinding;

internal class WorkItemProcessor : IWorkItemContext
{
	private class IndexedQueue<T>
	{
		private T[] buffer = new T[4];

		private int start;

		public T this[int index]
		{
			get
			{
				if (index < 0 || index >= Count)
				{
					throw new IndexOutOfRangeException();
				}
				return buffer[(start + index) % buffer.Length];
			}
			set
			{
				if (index < 0 || index >= Count)
				{
					throw new IndexOutOfRangeException();
				}
				buffer[(start + index) % buffer.Length] = value;
			}
		}

		public int Count { get; private set; }

		public void Enqueue(T item)
		{
			if (Count == buffer.Length)
			{
				T[] array = new T[buffer.Length * 2];
				for (int i = 0; i < Count; i++)
				{
					array[i] = this[i];
				}
				buffer = array;
				start = 0;
			}
			buffer[(start + Count) % buffer.Length] = item;
			Count++;
		}

		public T Dequeue()
		{
			if (Count == 0)
			{
				throw new InvalidOperationException();
			}
			T result = buffer[start];
			start = (start + 1) % buffer.Length;
			Count--;
			return result;
		}
	}

	private readonly AstarPath astar;

	private readonly IndexedQueue<AstarWorkItem> workItems = new IndexedQueue<AstarWorkItem>();

	private bool queuedWorkItemFloodFill;

	private bool anyGraphsDirty = true;

	public bool workItemsInProgressRightNow { get; private set; }

	public bool anyQueued => workItems.Count > 0;

	public bool workItemsInProgress { get; private set; }

	void IWorkItemContext.QueueFloodFill()
	{
		queuedWorkItemFloodFill = true;
	}

	void IWorkItemContext.SetGraphDirty(NavGraph graph)
	{
		anyGraphsDirty = true;
	}

	public void EnsureValidFloodFill()
	{
		if (queuedWorkItemFloodFill)
		{
			astar.hierarchicalGraph.RecalculateAll();
		}
		else
		{
			astar.hierarchicalGraph.RecalculateIfNecessary();
		}
	}

	public WorkItemProcessor(AstarPath astar)
	{
		this.astar = astar;
	}

	public void OnFloodFill()
	{
		queuedWorkItemFloodFill = false;
	}

	public void AddWorkItem(AstarWorkItem item)
	{
		workItems.Enqueue(item);
	}

	public bool ProcessWorkItems(bool force)
	{
		if (workItemsInProgressRightNow)
		{
			throw new Exception("Processing work items recursively. Please do not wait for other work items to be completed inside work items. If you think this is not caused by any of your scripts, this might be a bug.");
		}
		Physics2D.SyncTransforms();
		workItemsInProgressRightNow = true;
		astar.data.LockGraphStructure(allowAddingGraphs: true);
		while (workItems.Count > 0)
		{
			if (!workItemsInProgress)
			{
				workItemsInProgress = true;
				queuedWorkItemFloodFill = false;
			}
			AstarWorkItem value = workItems[0];
			bool flag;
			try
			{
				if (value.init != null)
				{
					value.init();
					value.init = null;
				}
				if (value.initWithContext != null)
				{
					value.initWithContext(this);
					value.initWithContext = null;
				}
				workItems[0] = value;
				flag = ((value.update != null) ? value.update(force) : (value.updateWithContext == null || value.updateWithContext(this, force)));
			}
			catch
			{
				workItems.Dequeue();
				workItemsInProgressRightNow = false;
				astar.data.UnlockGraphStructure();
				throw;
			}
			if (!flag)
			{
				if (force)
				{
					Debug.LogError("Misbehaving WorkItem. 'force'=true but the work item did not complete.\nIf force=true is passed to a WorkItem it should always return true.");
				}
				workItemsInProgressRightNow = false;
				astar.data.UnlockGraphStructure();
				return false;
			}
			workItems.Dequeue();
		}
		EnsureValidFloodFill();
		if (anyGraphsDirty)
		{
			GraphModifier.TriggerEvent(GraphModifier.EventType.PostUpdate);
		}
		anyGraphsDirty = false;
		workItemsInProgressRightNow = false;
		workItemsInProgress = false;
		astar.data.UnlockGraphStructure();
		return true;
	}
}
