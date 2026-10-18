namespace Unity.Cinemachine;

internal class Active
{
	public Point64 bot;

	public Point64 top;

	public long curX;

	public double dx;

	public int windDx;

	public int windCount;

	public int windCount2;

	public OutRec? outrec;

	public Active? prevInAEL;

	public Active? nextInAEL;

	public Active? prevInSEL;

	public Active? nextInSEL;

	public Active? jump;

	public Vertex? vertexTop;

	public LocalMinima localMin;

	internal bool isLeftBound;
}
