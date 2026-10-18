using System;

namespace g3;

public class GeneralPolygon2dBoxTree
{
	public GeneralPolygon2d Polygon;

	private Polygon2dBoxTree OuterTree;

	private Polygon2dBoxTree[] HoleTrees;

	public GeneralPolygon2dBoxTree(GeneralPolygon2d poly)
	{
		Polygon = poly;
		OuterTree = new Polygon2dBoxTree(poly.Outer);
		int count = poly.Holes.Count;
		if (count > 0)
		{
			HoleTrees = new Polygon2dBoxTree[count];
			for (int i = 0; i < count; i++)
			{
				HoleTrees[i] = new Polygon2dBoxTree(poly.Holes[i]);
			}
		}
	}

	public double DistanceSquared(Vector2d pt, out int iHoleIndex, out int iNearSeg, out double fNearSegT)
	{
		iHoleIndex = -1;
		double num = OuterTree.SquaredDistance(pt, out iNearSeg, out fNearSegT);
		int num2 = ((HoleTrees != null) ? HoleTrees.Length : 0);
		for (int i = 0; i < num2; i++)
		{
			int iNearSeg2;
			double fNearSegT2;
			double num3 = HoleTrees[i].SquaredDistance(pt, out iNearSeg2, out fNearSegT2, num);
			if (num3 < num)
			{
				num = num3;
				iHoleIndex = i;
				iNearSeg = iNearSeg2;
				fNearSegT = fNearSegT2;
			}
		}
		return num;
	}

	public double DistanceSquared(Vector2d pt)
	{
		int iHoleIndex;
		int iNearSeg;
		double fNearSegT;
		return DistanceSquared(pt, out iHoleIndex, out iNearSeg, out fNearSegT);
	}

	public double Distance(Vector2d pt)
	{
		int iHoleIndex;
		int iNearSeg;
		double fNearSegT;
		return Math.Sqrt(DistanceSquared(pt, out iHoleIndex, out iNearSeg, out fNearSegT));
	}

	public Vector2d NearestPoint(Vector2d pt)
	{
		DistanceSquared(pt, out var iHoleIndex, out var iNearSeg, out var fNearSegT);
		return Polygon.PointAt(iNearSeg, fNearSegT, iHoleIndex);
	}
}
