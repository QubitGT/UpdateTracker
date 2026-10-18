namespace Unity.Cinemachine;

internal class OutPt
{
	public Point64 pt;

	public OutPt? next;

	public OutPt prev;

	public OutRec outrec;

	public Joiner? joiner;

	public OutPt(Point64 pt, OutRec outrec)
	{
		this.pt = pt;
		this.outrec = outrec;
		next = this;
		prev = this;
		joiner = null;
	}
}
