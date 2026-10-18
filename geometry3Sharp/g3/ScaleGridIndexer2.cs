namespace g3;

public struct ScaleGridIndexer2 : IGridWorldIndexer2
{
	public double CellSize;

	public ScaleGridIndexer2(double cellSize)
	{
		CellSize = cellSize;
	}

	public Vector2i ToGrid(Vector2d point)
	{
		return new Vector2i((int)(point.x / CellSize), (int)(point.y / CellSize));
	}

	public Vector2d FromGrid(Vector2i gridpoint)
	{
		return new Vector2d((double)gridpoint.x * CellSize, (double)gridpoint.y * CellSize);
	}

	public Vector2d FromGrid(Vector2d gridpointf)
	{
		return new Vector2d(gridpointf.x * CellSize, gridpointf.y * CellSize);
	}
}
