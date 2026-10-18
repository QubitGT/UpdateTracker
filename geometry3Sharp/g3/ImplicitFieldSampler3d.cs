using System.Collections.Generic;

namespace g3;

public class ImplicitFieldSampler3d
{
	public enum CombineModes
	{
		DistanceMinUnion
	}

	public DenseGrid3f Grid;

	public double CellSize;

	public Vector3d GridOrigin;

	public ShiftGridIndexer3 Indexer;

	public AxisAlignedBox3i GridBounds;

	public float BackgroundValue;

	public CombineModes CombineMode;

	public ImplicitFieldSampler3d(AxisAlignedBox3d fieldBounds, double cellSize)
	{
		CellSize = cellSize;
		GridOrigin = fieldBounds.Min;
		Indexer = new ShiftGridIndexer3(GridOrigin, CellSize);
		Vector3d vector3d = fieldBounds.Max + cellSize;
		int num = (int)((vector3d.x - GridOrigin.x) / CellSize) + 1;
		int num2 = (int)((vector3d.y - GridOrigin.y) / CellSize) + 1;
		int num3 = (int)((vector3d.z - GridOrigin.z) / CellSize) + 1;
		GridBounds = new AxisAlignedBox3i(0, 0, 0, num, num2, num3);
		BackgroundValue = (float)((double)(num + num2 + num3) * CellSize);
		Grid = new DenseGrid3f(num, num2, num3, BackgroundValue);
	}

	public DenseGridTrilinearImplicit ToImplicit()
	{
		return new DenseGridTrilinearImplicit(Grid, GridOrigin, CellSize);
	}

	public void Clear(float f)
	{
		BackgroundValue = f;
		Grid.assign(BackgroundValue);
	}

	public void Sample(BoundedImplicitFunction3d f, double expandRadius = 0.0)
	{
		AxisAlignedBox3d axisAlignedBox3d = f.Bounds();
		Vector3d vector3d = expandRadius * Vector3d.One;
		Vector3i v = Indexer.ToGrid(axisAlignedBox3d.Min - vector3d);
		Vector3i v2 = Indexer.ToGrid(axisAlignedBox3d.Max + vector3d) + Vector3i.One;
		v = GridBounds.ClampExclusive(v);
		v2 = GridBounds.ClampExclusive(v2);
		AxisAlignedBox3i axisAlignedBox3i = new AxisAlignedBox3i(v, v2);
		if (CombineMode == CombineModes.DistanceMinUnion)
		{
			sample_min(f, axisAlignedBox3i.IndicesInclusive());
		}
	}

	private void sample_min(BoundedImplicitFunction3d f, IEnumerable<Vector3i> indices)
	{
		gParallel.ForEach(indices, delegate(Vector3i idx)
		{
			Vector3d pt = Indexer.FromGrid(idx);
			double num = f.Value(ref pt);
			Grid.set_min(ref idx, (float)num);
		});
	}
}
