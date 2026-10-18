namespace g3;

public interface IMultigridIndexer2
{
	GridLevelIndex2 ToBlock(Vector2i outer_index);

	Vector2i ToBlockIndex(Vector2i outer_index);

	Vector2i ToBlockLocal(Vector2i outer_index);

	Vector2i FromBlock(Vector2i block_idx);
}
