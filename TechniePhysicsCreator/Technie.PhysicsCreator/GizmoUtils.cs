using UnityEngine;

namespace Technie.PhysicsCreator;

public class GizmoUtils
{
	public static readonly Color[] HULL_COLOURS = new Color[12]
	{
		new Color(0f, 1f, 1f, 0.7f),
		new Color(1f, 0f, 1f, 0.7f),
		new Color(1f, 1f, 0f, 0.7f),
		new Color(1f, 0f, 0f, 0.7f),
		new Color(0f, 1f, 0f, 0.7f),
		new Color(0f, 0f, 1f, 0.7f),
		new Color(1f, 0.5f, 0f, 0.7f),
		new Color(1f, 0f, 0.5f, 0.7f),
		new Color(0.5f, 1f, 0f, 0.7f),
		new Color(0f, 1f, 0.5f, 0.7f),
		new Color(0.5f, 0f, 1f, 0.7f),
		new Color(0f, 0.5f, 1f, 0.7f)
	};

	public static Color GetHullColour(int index)
	{
		return HULL_COLOURS[index % HULL_COLOURS.Length];
	}

	public static void ToggleGizmos(bool gizmosOn)
	{
	}
}
