using System.Collections.Generic;

namespace Unity.Cinemachine;

internal class OutRec
{
	public int idx;

	public OutRec? owner;

	public List<OutRec>? splits;

	public Active? frontEdge;

	public Active? backEdge;

	public OutPt? pts;

	public PolyPathBase? polypath;

	public Rect64 bounds;

	public List<Point64> path;

	public bool isOpen;

	public OutRec()
	{
		bounds = default(Rect64);
		path = new List<Point64>();
	}
}
