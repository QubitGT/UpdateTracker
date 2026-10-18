using UnityEngine;

namespace Oculus.Interaction;

public class PointableCanvasEventArgs
{
	public readonly Canvas Canvas;

	public readonly GameObject Hovered;

	public readonly bool Dragging;

	public PointableCanvasEventArgs(Canvas canvas, GameObject hovered, bool dragging)
	{
		Canvas = canvas;
		Hovered = hovered;
		Dragging = dragging;
	}
}
