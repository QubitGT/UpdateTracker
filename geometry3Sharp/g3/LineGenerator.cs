namespace g3;

public class LineGenerator : CurveGenerator
{
	public Vector3d Start = Vector3d.Zero;

	public Vector3d End = Vector3d.AxisX;

	public int Subdivisions = -1;

	public double StepSize;

	public override void Generate()
	{
		double length = (Start - End).Length;
		int num = 10;
		if (Subdivisions > 0)
		{
			num = Subdivisions;
		}
		else if (StepSize > 0.0)
		{
			num = MathUtil.Clamp((int)(length / StepSize), 2, 10000);
		}
		vertices = new VectorArray3d(num + 1);
		for (int i = 0; i < num; i++)
		{
			double num2 = (double)i / (double)num;
			Vector3d value = (1.0 - num2) * Start + num2 * End;
			vertices[i] = value;
		}
		vertices[num] = End;
		closed = false;
	}
}
