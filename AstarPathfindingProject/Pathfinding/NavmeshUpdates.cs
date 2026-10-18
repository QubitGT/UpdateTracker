using System;
using System.Collections.Generic;
using Pathfinding.Util;
using UnityEngine;

namespace Pathfinding;

[Serializable]
public class NavmeshUpdates
{
	internal class NavmeshUpdateSettings
	{
		public TileHandler handler;

		public readonly List<IntRect> forcedReloadRects = new List<IntRect>();

		private readonly NavmeshBase graph;

		public NavmeshUpdateSettings(NavmeshBase graph)
		{
			this.graph = graph;
		}

		public void Refresh(bool forceCreate = false)
		{
			if (!graph.enableNavmeshCutting)
			{
				if (handler != null)
				{
					handler.cuts.Clear();
					handler.ReloadInBounds(new IntRect(int.MinValue, int.MinValue, int.MaxValue, int.MaxValue));
					AstarPath.active.FlushGraphUpdates();
					AstarPath.active.FlushWorkItems();
					forcedReloadRects.ClearFast();
					handler = null;
				}
			}
			else if ((handler == null && (forceCreate || NavmeshClipper.allEnabled.Count > 0)) || (handler != null && !handler.isValid))
			{
				handler = new TileHandler(graph);
				for (int i = 0; i < NavmeshClipper.allEnabled.Count; i++)
				{
					AddClipper(NavmeshClipper.allEnabled[i]);
				}
				handler.CreateTileTypesFromGraph();
				forcedReloadRects.Add(new IntRect(int.MinValue, int.MinValue, int.MaxValue, int.MaxValue));
			}
		}

		public void OnRecalculatedTiles(NavmeshTile[] tiles)
		{
			Refresh();
			if (handler != null)
			{
				handler.OnRecalculatedTiles(tiles);
			}
		}

		public void AddClipper(NavmeshClipper obj)
		{
			if (obj.graphMask.Contains((int)graph.graphIndex))
			{
				Refresh(forceCreate: true);
				if (handler != null)
				{
					Rect bounds = obj.GetBounds(handler.graph.transform);
					IntRect touchingTilesInGraphSpace = handler.graph.GetTouchingTilesInGraphSpace(bounds);
					handler.cuts.Add(obj, touchingTilesInGraphSpace);
				}
			}
		}

		public void RemoveClipper(NavmeshClipper obj)
		{
			Refresh();
			if (handler != null)
			{
				GridLookup<NavmeshClipper>.Root root = handler.cuts.GetRoot(obj);
				if (root != null)
				{
					forcedReloadRects.Add(root.previousBounds);
					handler.cuts.Remove(obj);
				}
			}
		}
	}

	public float updateInterval;

	private float lastUpdateTime = float.NegativeInfinity;

	internal void OnEnable()
	{
		NavmeshClipper.AddEnableCallback(HandleOnEnableCallback, HandleOnDisableCallback);
	}

	internal void OnDisable()
	{
		NavmeshClipper.RemoveEnableCallback(HandleOnEnableCallback, HandleOnDisableCallback);
	}

	public void DiscardPending()
	{
		for (int i = 0; i < NavmeshClipper.allEnabled.Count; i++)
		{
			NavmeshClipper.allEnabled[i].NotifyUpdated();
		}
		NavGraph[] graphs = AstarPath.active.graphs;
		for (int j = 0; j < graphs.Length; j++)
		{
			if (graphs[j] is NavmeshBase navmeshBase)
			{
				navmeshBase.navmeshUpdateData.forcedReloadRects.Clear();
			}
		}
	}

	private void HandleOnEnableCallback(NavmeshClipper obj)
	{
		NavGraph[] graphs = AstarPath.active.graphs;
		for (int i = 0; i < graphs.Length; i++)
		{
			if (graphs[i] is NavmeshBase navmeshBase)
			{
				navmeshBase.navmeshUpdateData.AddClipper(obj);
			}
		}
		obj.ForceUpdate();
	}

	private void HandleOnDisableCallback(NavmeshClipper obj)
	{
		NavGraph[] graphs = AstarPath.active.graphs;
		for (int i = 0; i < graphs.Length; i++)
		{
			if (graphs[i] is NavmeshBase navmeshBase)
			{
				navmeshBase.navmeshUpdateData.RemoveClipper(obj);
			}
		}
		lastUpdateTime = float.NegativeInfinity;
	}

	internal void Update()
	{
		if (AstarPath.active.isScanning)
		{
			return;
		}
		bool flag = false;
		NavGraph[] graphs = AstarPath.active.graphs;
		for (int i = 0; i < graphs.Length; i++)
		{
			if (graphs[i] is NavmeshBase navmeshBase)
			{
				navmeshBase.navmeshUpdateData.Refresh();
				flag = navmeshBase.navmeshUpdateData.forcedReloadRects.Count > 0;
			}
		}
		if ((updateInterval >= 0f && Time.realtimeSinceStartup - lastUpdateTime > updateInterval) || flag)
		{
			ForceUpdate();
		}
	}

	public void ForceUpdate()
	{
		lastUpdateTime = Time.realtimeSinceStartup;
		List<NavmeshClipper> list = null;
		NavGraph[] graphs = AstarPath.active.graphs;
		for (int i = 0; i < graphs.Length; i++)
		{
			if (!(graphs[i] is NavmeshBase navmeshBase))
			{
				continue;
			}
			navmeshBase.navmeshUpdateData.Refresh();
			TileHandler handler = navmeshBase.navmeshUpdateData.handler;
			if (handler == null)
			{
				continue;
			}
			List<IntRect> forcedReloadRects = navmeshBase.navmeshUpdateData.forcedReloadRects;
			GridLookup<NavmeshClipper>.Root allItems = handler.cuts.AllItems;
			if (forcedReloadRects.Count == 0)
			{
				bool flag = false;
				for (GridLookup<NavmeshClipper>.Root root = allItems; root != null; root = root.next)
				{
					if (root.obj.RequiresUpdate())
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					continue;
				}
			}
			handler.StartBatchLoad();
			for (int j = 0; j < forcedReloadRects.Count; j++)
			{
				handler.ReloadInBounds(forcedReloadRects[j]);
			}
			forcedReloadRects.ClearFast();
			if (list == null)
			{
				list = ListPool<NavmeshClipper>.Claim();
			}
			for (GridLookup<NavmeshClipper>.Root root2 = allItems; root2 != null; root2 = root2.next)
			{
				if (root2.obj.RequiresUpdate())
				{
					handler.ReloadInBounds(root2.previousBounds);
					Rect bounds = root2.obj.GetBounds(handler.graph.transform);
					IntRect touchingTilesInGraphSpace = handler.graph.GetTouchingTilesInGraphSpace(bounds);
					handler.cuts.Move(root2.obj, touchingTilesInGraphSpace);
					handler.ReloadInBounds(touchingTilesInGraphSpace);
					list.Add(root2.obj);
				}
			}
			handler.EndBatchLoad();
		}
		if (list != null)
		{
			for (int k = 0; k < list.Count; k++)
			{
				list[k].NotifyUpdated();
			}
			ListPool<NavmeshClipper>.Release(ref list);
		}
	}
}
