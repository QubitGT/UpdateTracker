namespace g3;

public interface IGridWorldIndexer2
{
	Vector2i ToGrid(Vector2d pointf);

	Vector2d FromGrid(Vector2i gridpoint);

	Vector2d FromGrid(Vector2d gridpointf);
}
