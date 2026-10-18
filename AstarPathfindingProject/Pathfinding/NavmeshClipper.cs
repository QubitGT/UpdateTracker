using System;
using System.Collections.Generic;
using Pathfinding.Util;
using UnityEngine;

namespace Pathfinding;

public abstract class NavmeshClipper : VersionedMonoBehaviour
{
	private static Action<NavmeshClipper> OnEnableCallback;

	private static Action<NavmeshClipper> OnDisableCallback;

	private static readonly List<NavmeshClipper> all = new List<NavmeshClipper>();

	private int listIndex = -1;

	public GraphMask graphMask = GraphMask.everything;

	public static List<NavmeshClipper> allEnabled => all;

	public static void AddEnableCallback(Action<NavmeshClipper> onEnable, Action<NavmeshClipper> onDisable)
	{
		OnEnableCallback = (Action<NavmeshClipper>)Delegate.Combine(OnEnableCallback, onEnable);
		OnDisableCallback = (Action<NavmeshClipper>)Delegate.Combine(OnDisableCallback, onDisable);
	}

	public static void RemoveEnableCallback(Action<NavmeshClipper> onEnable, Action<NavmeshClipper> onDisable)
	{
		OnEnableCallback = (Action<NavmeshClipper>)Delegate.Remove(OnEnableCallback, onEnable);
		OnDisableCallback = (Action<NavmeshClipper>)Delegate.Remove(OnDisableCallback, onDisable);
	}

	protected virtual void OnEnable()
	{
		if (OnEnableCallback != null)
		{
			OnEnableCallback(this);
		}
		listIndex = all.Count;
		all.Add(this);
	}

	protected virtual void OnDisable()
	{
		all[listIndex] = all[all.Count - 1];
		all[listIndex].listIndex = listIndex;
		all.RemoveAt(all.Count - 1);
		listIndex = -1;
		if (OnDisableCallback != null)
		{
			OnDisableCallback(this);
		}
	}

	internal abstract void NotifyUpdated();

	public abstract Rect GetBounds(GraphTransform transform);

	public abstract bool RequiresUpdate();

	public abstract void ForceUpdate();
}
