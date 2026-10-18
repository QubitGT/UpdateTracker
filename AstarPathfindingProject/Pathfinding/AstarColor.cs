using System;
using UnityEngine;

namespace Pathfinding;

[Serializable]
public class AstarColor
{
	public Color _SolidColor;

	public Color _UnwalkableNode;

	public Color _BoundsHandles;

	public Color _ConnectionLowLerp;

	public Color _ConnectionHighLerp;

	public Color _MeshEdgeColor;

	public Color[] _AreaColors;

	public static Color SolidColor = new Color(0.11764706f, 0.4f, 67f / 85f, 0.9f);

	public static Color UnwalkableNode = new Color(1f, 0f, 0f, 0.5f);

	public static Color BoundsHandles = new Color(0.29f, 0.454f, 0.741f, 0.9f);

	public static Color ConnectionLowLerp = new Color(0f, 1f, 0f, 0.5f);

	public static Color ConnectionHighLerp = new Color(1f, 0f, 0f, 0.5f);

	public static Color MeshEdgeColor = new Color(0f, 0f, 0f, 0.5f);

	private static Color[] AreaColors = new Color[1];

	public static int ColorHash()
	{
		int num = SolidColor.GetHashCode() ^ UnwalkableNode.GetHashCode() ^ BoundsHandles.GetHashCode() ^ ConnectionLowLerp.GetHashCode() ^ ConnectionHighLerp.GetHashCode() ^ MeshEdgeColor.GetHashCode();
		for (int i = 0; i < AreaColors.Length; i++)
		{
			num = (7 * num) ^ AreaColors[i].GetHashCode();
		}
		return num;
	}

	public static Color GetAreaColor(uint area)
	{
		if (area >= AreaColors.Length)
		{
			return AstarMath.IntToColor((int)area, 1f);
		}
		return AreaColors[area];
	}

	public static Color GetTagColor(uint tag)
	{
		if (tag >= AreaColors.Length)
		{
			return AstarMath.IntToColor((int)tag, 1f);
		}
		return AreaColors[tag];
	}

	public void PushToStatic(AstarPath astar)
	{
		_AreaColors = _AreaColors ?? new Color[1];
		SolidColor = _SolidColor;
		UnwalkableNode = _UnwalkableNode;
		BoundsHandles = _BoundsHandles;
		ConnectionLowLerp = _ConnectionLowLerp;
		ConnectionHighLerp = _ConnectionHighLerp;
		MeshEdgeColor = _MeshEdgeColor;
		AreaColors = _AreaColors;
	}

	public AstarColor()
	{
		_SolidColor = new Color(0.11764706f, 0.4f, 67f / 85f, 0.9f);
		_UnwalkableNode = new Color(1f, 0f, 0f, 0.5f);
		_BoundsHandles = new Color(0.29f, 0.454f, 0.741f, 0.9f);
		_ConnectionLowLerp = new Color(0f, 1f, 0f, 0.5f);
		_ConnectionHighLerp = new Color(1f, 0f, 0f, 0.5f);
		_MeshEdgeColor = new Color(0f, 0f, 0f, 0.5f);
	}
}
