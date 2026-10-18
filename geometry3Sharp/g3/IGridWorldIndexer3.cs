namespace g3;

public interface IGridWorldIndexer3
{
	Vector3i ToGrid(Vector3d pointf);

	Vector3d ToGridf(Vector3d pointf);

	Vector3d FromGrid(Vector3i gridpoint);

	Vector3d FromGrid(Vector3d gridpointf);
}
