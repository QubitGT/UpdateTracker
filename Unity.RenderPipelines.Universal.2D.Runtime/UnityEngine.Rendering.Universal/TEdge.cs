namespace UnityEngine.Rendering.Universal;

internal class TEdge
{
	internal IntPoint Bot;

	internal IntPoint Curr;

	internal IntPoint Top;

	internal IntPoint Delta;

	internal double Dx;

	internal PolyTypes PolyTyp;

	internal EdgeSides Side;

	internal int WindDelta;

	internal int WindCnt;

	internal int WindCnt2;

	internal int OutIdx;

	internal TEdge Next;

	internal TEdge Prev;

	internal TEdge NextInLML;

	internal TEdge NextInAEL;

	internal TEdge PrevInAEL;

	internal TEdge NextInSEL;

	internal TEdge PrevInSEL;
}
