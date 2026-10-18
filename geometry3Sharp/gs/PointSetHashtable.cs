using System;
using System.Collections.Generic;
using g3;

namespace gs;

public class PointSetHashtable
{
	public class PointList : List<int>, IGridElement3
	{
		public IGridElement3 CreateNewGridElement(bool bCopy)
		{
			return new PointList();
		}
	}

	private IPointSet Points;

	private DSparseGrid3<PointList> Grid;

	private ShiftGridIndexer3 indexF;

	private Vector3d Origin;

	private double CellSize;

	public PointSetHashtable(IPointSet points)
	{
		Points = points;
	}

	public void Build(int maxAxisSubdivs = 64)
	{
		AxisAlignedBox3d axisAlignedBox3d = BoundsUtil.Bounds(Points);
		double cellSize = axisAlignedBox3d.MaxDim / (double)maxAxisSubdivs;
		Build(cellSize, axisAlignedBox3d.Min);
	}

	public void Build(double cellSize, Vector3d origin)
	{
		Origin = origin;
		CellSize = cellSize;
		indexF = new ShiftGridIndexer3(Origin, CellSize);
		Grid = new DSparseGrid3<PointList>(new PointList());
		foreach (int item in Points.VertexIndices())
		{
			Vector3d vertex = Points.GetVertex(item);
			Vector3i index = indexF.ToGrid(vertex);
			Grid.Get(index).Add(item);
		}
	}

	public bool FindInBall(Vector3d pt, double r, int[] buffer, out int buffer_count)
	{
		buffer_count = 0;
		double num = CellSize * 0.5;
		Vector3i vector3i = indexF.ToGrid(pt);
		Vector3d vector3d = indexF.FromGrid(vector3i) + num * Vector3d.One;
		if (r > CellSize)
		{
			throw new ArgumentException("PointSetHashtable.FindInBall: large radius unsupported");
		}
		double num2 = r * r;
		PointList pointList = Grid.Get(vector3i, allocateIfMissing: false);
		if (pointList != null)
		{
			foreach (int item in pointList)
			{
				if (pt.DistanceSquared(Points.GetVertex(item)) < num2)
				{
					if (buffer_count == buffer.Length)
					{
						return false;
					}
					buffer[buffer_count++] = item;
				}
			}
		}
		if ((pt - vector3d).MaxAbs + r > num)
		{
			for (int i = 0; i < 26; i++)
			{
				Vector3i vector3i2 = gIndices.GridOffsets26[i];
				if (new Vector3d(vector3d.x + num * (double)vector3i2.x - pt.x, vector3d.y + num * (double)vector3i2.y - pt.y, vector3d.z + num * (double)vector3i2.z - pt.z).MinAbs > r)
				{
					continue;
				}
				PointList pointList2 = Grid.Get(vector3i + vector3i2, allocateIfMissing: false);
				if (pointList2 == null)
				{
					continue;
				}
				foreach (int item2 in pointList2)
				{
					if (pt.DistanceSquared(Points.GetVertex(item2)) < num2)
					{
						if (buffer_count == buffer.Length)
						{
							return false;
						}
						buffer[buffer_count++] = item2;
					}
				}
			}
		}
		return true;
	}
}
