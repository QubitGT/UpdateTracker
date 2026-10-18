using System.Collections.Generic;
using UnityEngine;

namespace Drawing;

public static class GizmoContext
{
	private static HashSet<Transform> selectedTransforms = new HashSet<Transform>();

	internal static bool drawingGizmos;

	internal static bool dirty;

	private static int selectionSizeInternal;

	public static int selectionSize
	{
		get
		{
			Refresh();
			return selectionSizeInternal;
		}
		private set
		{
			selectionSizeInternal = value;
		}
	}

	internal static void SetDirty()
	{
		dirty = true;
	}

	private static void Refresh()
	{
	}

	public static bool InSelection(Component c)
	{
		return InSelection(c.transform);
	}

	public static bool InSelection(Transform tr)
	{
		Refresh();
		Transform item = tr;
		while (tr != null)
		{
			if (selectedTransforms.Contains(tr))
			{
				selectedTransforms.Add(item);
				return true;
			}
			tr = tr.parent;
		}
		return false;
	}

	public static bool InActiveSelection(Component c)
	{
		return InActiveSelection(c.transform);
	}

	public static bool InActiveSelection(Transform tr)
	{
		return false;
	}
}
