namespace g3;

public interface IMultigridIndexer3
{
	GridLevelIndex ToBlock(Vector3i outer_index);

	Vector3i ToBlockIndex(Vector3i outer_index);

	Vector3i ToBlockLocal(Vector3i outer_index);

	Vector3i FromBlock(Vector3i block_idx);
}
