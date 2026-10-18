namespace g3;

public struct ShiftGridIndexer3 : IGridWorldIndexer3
{
	public Vector3d Origin;

	public double CellSize;

	public ShiftGridIndexer3(Vector3d origin, double cellSize)
	{
		Origin = origin;
		CellSize = cellSize;
	}

	public Vector3i ToGrid(Vector3d point)
	{
		return new Vector3i((int)((point.x - Origin.x) / CellSize), (int)((point.y - Origin.y) / CellSize), (int)((point.z - Origin.z) / CellSize));
	}

	public Vector3d ToGridf(Vector3d point)
	{
		return new Vector3d((point.x - Origin.x) / CellSize, (point.y - Origin.y) / CellSize, (point.z - Origin.z) / CellSize);
	}

	public Vector3d FromGrid(Vector3i gridpoint)
	{
		return new Vector3d((double)gridpoint.x * CellSize + Origin.x, (double)gridpoint.y * CellSize + Origin.y, (double)gridpoint.z * CellSize + Origin.z);
	}

	public Vector3d FromGrid(Vector3d gridpointf)
	{
		return new Vector3d(gridpointf.x * CellSize + Origin.x, gridpointf.y * CellSize + Origin.y, gridpointf.z * CellSize + Origin.z);
	}
}
