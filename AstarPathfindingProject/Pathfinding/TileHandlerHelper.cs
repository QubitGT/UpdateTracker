using System;
using Pathfinding.Util;
using UnityEngine;

namespace Pathfinding;

[Obsolete("Use AstarPath.navmeshUpdates instead. You can safely remove this component.")]
[HelpURL("http://arongranberg.com/astar/documentation/stable/class_pathfinding_1_1_tile_handler_helper.php")]
public class TileHandlerHelper : VersionedMonoBehaviour
{
	public float updateInterval
	{
		get
		{
			return AstarPath.active.navmeshUpdates.updateInterval;
		}
		set
		{
			AstarPath.active.navmeshUpdates.updateInterval = value;
		}
	}

	[Obsolete("All navmesh/recast graphs now use navmesh cutting")]
	public void UseSpecifiedHandler(TileHandler newHandler)
	{
		throw new Exception("All navmesh/recast graphs now use navmesh cutting");
	}

	public void DiscardPending()
	{
		AstarPath.active.navmeshUpdates.DiscardPending();
	}

	public void ForceUpdate()
	{
		AstarPath.active.navmeshUpdates.ForceUpdate();
	}
}
