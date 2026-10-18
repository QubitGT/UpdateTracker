namespace g3;

public struct ScaleGridIndexer3 : IGridWorldIndexer3
{
	public double CellSize;

	public ScaleGridIndexer3(double cellSize)
	{
		CellSize = cellSize;
	}

	public Vector3i ToGrid(Vector3d point)
	{
		return new Vector3i((int)(point.x / CellSize), (int)(point.y / CellSize), (int)(point.z / CellSize));
	}

	public Vector3d ToGridf(Vector3d point)
	{
		return new Vector3d(point.x / CellSize, point.y / CellSize, point.z / CellSize);
	}

	public Vector3d FromGrid(Vector3i gridpoint)
	{
		return new Vector3d((double)gridpoint.x * CellSize, (double)gridpoint.y * CellSize, (double)gridpoint.z * CellSize);
	}

	public Vector3d FromGrid(Vector3d gridpointf)
	{
		return new Vector3d(gridpointf.x * CellSize, gridpointf.y * CellSize, gridpointf.z * CellSize);
	}
}
