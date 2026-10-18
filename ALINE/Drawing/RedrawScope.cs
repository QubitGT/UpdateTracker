using System;
using System.Runtime.InteropServices;

namespace Drawing;

public struct RedrawScope : IDisposable
{
	internal GCHandle gizmos;

	internal int id;

	private static int idCounter = 1;

	internal RedrawScope(DrawingData gizmos, int id)
	{
		this.gizmos = gizmos.gizmosHandle;
		this.id = id;
	}

	internal RedrawScope(DrawingData gizmos)
	{
		this.gizmos = gizmos.gizmosHandle;
		id = idCounter++;
	}

	internal void Draw()
	{
		if (gizmos.IsAllocated && gizmos.Target is DrawingData drawingData)
		{
			drawingData.Draw(this);
		}
	}

	public void Rewind()
	{
		Dispose();
		this = DrawingManager.GetRedrawScope();
	}

	internal void DrawUntilDispose()
	{
		if (gizmos.Target is DrawingData drawingData)
		{
			drawingData.DrawUntilDisposed(this);
		}
	}

	public void Dispose()
	{
		if (gizmos.IsAllocated && gizmos.Target is DrawingData drawingData)
		{
			drawingData.DisposeRedrawScope(this);
		}
		gizmos = default(GCHandle);
	}
}
