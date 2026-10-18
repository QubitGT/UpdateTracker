using System.Collections.Generic;

namespace g3;

public class ContOrientedBox3
{
	public Box3d Box;

	public bool ResultValid;

	public ContOrientedBox3(IEnumerable<Vector3d> points)
	{
		GaussPointsFit3 gaussPointsFit = new GaussPointsFit3(points);
		if (gaussPointsFit.ResultValid)
		{
			Box = gaussPointsFit.Box;
			Box.Contain(points);
		}
	}

	public ContOrientedBox3(IEnumerable<Vector3d> points, IEnumerable<double> pointWeights)
	{
		GaussPointsFit3 gaussPointsFit = new GaussPointsFit3(points, pointWeights);
		if (gaussPointsFit.ResultValid)
		{
			Box = gaussPointsFit.Box;
			Box.Contain(points);
		}
	}
}
