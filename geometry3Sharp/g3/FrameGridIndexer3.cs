namespace g3;

public struct FrameGridIndexer3 : IGridWorldIndexer3
{
	public Frame3f GridFrame;

	public Vector3f CellSize;

	public FrameGridIndexer3(Frame3f frame, Vector3f cellSize)
	{
		GridFrame = frame;
		CellSize = cellSize;
	}

	public Vector3i ToGrid(Vector3d point)
	{
		Vector3f v = (Vector3f)point;
		v = GridFrame.ToFrameP(ref v);
		return (Vector3i)(v / CellSize);
	}

	public Vector3d ToGridf(Vector3d point)
	{
		point = GridFrame.ToFrameP(ref point);
		point.x /= CellSize.x;
		point.y /= CellSize.y;
		point.z /= CellSize.z;
		return point;
	}

	public Vector3d FromGrid(Vector3i gridpoint)
	{
		Vector3f v = CellSize * (Vector3f)gridpoint;
		return GridFrame.FromFrameP(ref v);
	}

	public Vector3d FromGrid(Vector3d gridpointf)
	{
		gridpointf *= (Vector3d)CellSize;
		return GridFrame.FromFrameP(ref gridpointf);
	}
}
