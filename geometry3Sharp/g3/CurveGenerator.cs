namespace g3;

public abstract class CurveGenerator
{
	public VectorArray3d vertices;

	public bool closed;

	public abstract void Generate();

	public void Make(DCurve3 c)
	{
		int count = vertices.Count;
		for (int i = 0; i < count; i++)
		{
			c.AppendVertex(vertices[i]);
		}
		c.Closed = closed;
	}
}
