using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace g3;

public class PlanarSolid2d
{
	private IParametricCurve2d outer;

	private List<IParametricCurve2d> holes = new List<IParametricCurve2d>();

	public IParametricCurve2d Outer => outer;

	private bool HasHoles => Holes.Count > 0;

	public ReadOnlyCollection<IParametricCurve2d> Holes => holes.AsReadOnly();

	public bool HasArcLength
	{
		get
		{
			bool flag = outer.HasArcLength;
			foreach (IParametricCurve2d hole in Holes)
			{
				flag = flag && hole.HasArcLength;
			}
			return flag;
		}
	}

	public double Perimeter
	{
		get
		{
			if (!HasArcLength)
			{
				throw new Exception("PlanarSolid2d.Perimeter: some curves do not have arc length");
			}
			double num = outer.ArcLength;
			foreach (IParametricCurve2d hole in Holes)
			{
				num += hole.ArcLength;
			}
			return num;
		}
	}

	public void SetOuter(IParametricCurve2d loop, bool bIsClockwise)
	{
		outer = loop;
	}

	public void AddHole(IParametricCurve2d hole)
	{
		if (outer == null)
		{
			throw new Exception("PlanarSolid2d.AddHole: outer polygon not set!");
		}
		holes.Add(hole);
	}

	public GeneralPolygon2d Convert(double fSpacingLength, double fSpacingT, double fDeviationTolerance)
	{
		GeneralPolygon2d generalPolygon2d = new GeneralPolygon2d();
		generalPolygon2d.Outer = new Polygon2d(CurveSampler2.AutoSample(outer, fSpacingLength, fSpacingT));
		generalPolygon2d.Outer.Simplify(0.0, fDeviationTolerance);
		foreach (IParametricCurve2d hole in Holes)
		{
			Polygon2d polygon2d = new Polygon2d(CurveSampler2.AutoSample(hole, fSpacingLength, fSpacingT));
			polygon2d.Simplify(0.0, fDeviationTolerance);
			generalPolygon2d.AddHole(polygon2d, bCheckContainment: false);
		}
		return generalPolygon2d;
	}
}
