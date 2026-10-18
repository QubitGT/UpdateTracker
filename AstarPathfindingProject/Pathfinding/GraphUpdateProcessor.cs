using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Profiling;

namespace Pathfinding;

internal class GraphUpdateProcessor
{
	private enum GraphUpdateOrder
	{
		GraphUpdate
	}

	private struct GUOSingle
	{
		public GraphUpdateOrder order;

		public IUpdatableGraph graph;

		public GraphUpdateObject obj;
	}

	private readonly AstarPath astar;

	private Thread graphUpdateThread;

	private bool anyGraphUpdateInProgress;

	private CustomSampler asyncUpdateProfilingSampler;

	private readonly Queue<GraphUpdateObject> graphUpdateQueue = new Queue<GraphUpdateObject>();

	private readonly Queue<GUOSingle> graphUpdateQueueAsync = new Queue<GUOSingle>();

	private readonly Queue<GUOSingle> graphUpdateQueuePost = new Queue<GUOSingle>();

	private readonly Queue<GUOSingle> graphUpdateQueueRegular = new Queue<GUOSingle>();

	private readonly ManualResetEvent asyncGraphUpdatesComplete = new ManualResetEvent(initialState: true);

	private readonly AutoResetEvent graphUpdateAsyncEvent = new AutoResetEvent(initialState: false);

	private readonly AutoResetEvent exitAsyncThread = new AutoResetEvent(initialState: false);

	public bool IsAnyGraphUpdateQueued => graphUpdateQueue.Count > 0;

	public bool IsAnyGraphUpdateInProgress => anyGraphUpdateInProgress;

	public event Action OnGraphsUpdated;

	public GraphUpdateProcessor(AstarPath astar)
	{
		this.astar = astar;
	}

	public AstarWorkItem GetWorkItem()
	{
		return new AstarWorkItem(QueueGraphUpdatesInternal, ProcessGraphUpdates);
	}

	public void EnableMultithreading()
	{
		if (graphUpdateThread == null || !graphUpdateThread.IsAlive)
		{
			asyncUpdateProfilingSampler = CustomSampler.Create("Graph Update");
			graphUpdateThread = new Thread(ProcessGraphUpdatesAsync);
			graphUpdateThread.IsBackground = true;
			graphUpdateThread.Priority = System.Threading.ThreadPriority.Lowest;
			graphUpdateThread.Start();
		}
	}

	public void DisableMultithreading()
	{
		if (graphUpdateThread != null && graphUpdateThread.IsAlive)
		{
			exitAsyncThread.Set();
			if (!graphUpdateThread.Join(5000))
			{
				Debug.LogError("Graph update thread did not exit in 5 seconds");
			}
			graphUpdateThread = null;
		}
	}

	public void AddToQueue(GraphUpdateObject ob)
	{
		graphUpdateQueue.Enqueue(ob);
	}

	private void QueueGraphUpdatesInternal()
	{
		while (graphUpdateQueue.Count > 0)
		{
			GraphUpdateObject graphUpdateObject = graphUpdateQueue.Dequeue();
			if (graphUpdateObject.internalStage != -2)
			{
				Debug.LogError("Expected remaining graph updates to be pending");
				continue;
			}
			graphUpdateObject.internalStage = 0;
			foreach (IUpdatableGraph updateableGraph in astar.data.GetUpdateableGraphs())
			{
				NavGraph graph = updateableGraph as NavGraph;
				if (graphUpdateObject.nnConstraint == null || graphUpdateObject.nnConstraint.SuitableGraph(astar.data.GetGraphIndex(graph), graph))
				{
					GUOSingle item = default(GUOSingle);
					item.order = GraphUpdateOrder.GraphUpdate;
					item.obj = graphUpdateObject;
					item.graph = updateableGraph;
					graphUpdateObject.internalStage++;
					graphUpdateQueueRegular.Enqueue(item);
				}
			}
		}
		GraphModifier.TriggerEvent(GraphModifier.EventType.PreUpdate);
		anyGraphUpdateInProgress = true;
	}

	private bool ProcessGraphUpdates(bool force)
	{
		if (force)
		{
			asyncGraphUpdatesComplete.WaitOne();
		}
		else if (!asyncGraphUpdatesComplete.WaitOne(0))
		{
			return false;
		}
		ProcessPostUpdates();
		if (!ProcessRegularUpdates(force))
		{
			return false;
		}
		GraphModifier.TriggerEvent(GraphModifier.EventType.PostUpdate);
		if (this.OnGraphsUpdated != null)
		{
			this.OnGraphsUpdated();
		}
		anyGraphUpdateInProgress = false;
		return true;
	}

	private bool ProcessRegularUpdates(bool force)
	{
		while (graphUpdateQueueRegular.Count > 0)
		{
			GUOSingle item = graphUpdateQueueRegular.Peek();
			GraphUpdateThreading graphUpdateThreading = item.graph.CanUpdateAsync(item.obj);
			if (force || !Application.isPlaying || graphUpdateThread == null || !graphUpdateThread.IsAlive)
			{
				graphUpdateThreading &= (GraphUpdateThreading)(-2);
			}
			if ((graphUpdateThreading & GraphUpdateThreading.UnityInit) != 0)
			{
				if (StartAsyncUpdatesIfQueued())
				{
					return false;
				}
				item.graph.UpdateAreaInit(item.obj);
			}
			if ((graphUpdateThreading & GraphUpdateThreading.SeparateThread) != 0)
			{
				graphUpdateQueueRegular.Dequeue();
				graphUpdateQueueAsync.Enqueue(item);
				if ((graphUpdateThreading & GraphUpdateThreading.UnityPost) != 0 && StartAsyncUpdatesIfQueued())
				{
					return false;
				}
				continue;
			}
			if (StartAsyncUpdatesIfQueued())
			{
				return false;
			}
			graphUpdateQueueRegular.Dequeue();
			try
			{
				item.graph.UpdateArea(item.obj);
			}
			catch (Exception ex)
			{
				Debug.LogError("Error while updating graphs\n" + ex);
			}
			if ((graphUpdateThreading & GraphUpdateThreading.UnityPost) != 0)
			{
				item.graph.UpdateAreaPost(item.obj);
			}
			item.obj.internalStage--;
		}
		if (StartAsyncUpdatesIfQueued())
		{
			return false;
		}
		return true;
	}

	private bool StartAsyncUpdatesIfQueued()
	{
		if (graphUpdateQueueAsync.Count > 0)
		{
			asyncGraphUpdatesComplete.Reset();
			graphUpdateAsyncEvent.Set();
			return true;
		}
		return false;
	}

	private void ProcessPostUpdates()
	{
		while (graphUpdateQueuePost.Count > 0)
		{
			GUOSingle gUOSingle = graphUpdateQueuePost.Dequeue();
			if ((gUOSingle.graph.CanUpdateAsync(gUOSingle.obj) & GraphUpdateThreading.UnityPost) != 0)
			{
				try
				{
					gUOSingle.graph.UpdateAreaPost(gUOSingle.obj);
				}
				catch (Exception ex)
				{
					Debug.LogError("Error while updating graphs (post step)\n" + ex);
				}
			}
			gUOSingle.obj.internalStage--;
		}
	}

	private void ProcessGraphUpdatesAsync()
	{
		AutoResetEvent[] array = new AutoResetEvent[2] { graphUpdateAsyncEvent, exitAsyncThread };
		while (true)
		{
			WaitHandle[] waitHandles = array;
			if (WaitHandle.WaitAny(waitHandles) == 1)
			{
				break;
			}
			while (graphUpdateQueueAsync.Count > 0)
			{
				GUOSingle item = graphUpdateQueueAsync.Dequeue();
				try
				{
					if (item.order == GraphUpdateOrder.GraphUpdate)
					{
						item.graph.UpdateArea(item.obj);
						graphUpdateQueuePost.Enqueue(item);
						continue;
					}
					throw new NotSupportedException(item.order.ToString() ?? "");
				}
				catch (Exception ex)
				{
					Debug.LogError("Exception while updating graphs:\n" + ex);
				}
			}
			asyncGraphUpdatesComplete.Set();
		}
		while (graphUpdateQueueAsync.Count > 0)
		{
			graphUpdateQueueAsync.Dequeue().obj.internalStage = -3;
		}
		asyncGraphUpdatesComplete.Set();
		Profiler.EndThreadProfiling();
	}
}
