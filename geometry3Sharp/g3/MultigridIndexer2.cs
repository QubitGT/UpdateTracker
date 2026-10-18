namespace g3;

public struct MultigridIndexer2 : IMultigridIndexer2
{
	public Vector2i OuterShift;

	public Vector2i BlockSize;

	public Vector2i BlockShift;

	public MultigridIndexer2(Vector2i blockSize)
	{
		BlockSize = blockSize;
		OuterShift = (BlockShift = Vector2i.Zero);
	}

	public Vector2i ToBlockIndex(Vector2i outer_index)
	{
		Vector2i vector2i = outer_index - OuterShift;
		vector2i.x = ((vector2i.x >= 0) ? (vector2i.x / BlockSize.x) : (vector2i.x / BlockSize.x - 1));
		vector2i.y = ((vector2i.y >= 0) ? (vector2i.y / BlockSize.y) : (vector2i.y / BlockSize.y - 1));
		return vector2i - BlockShift;
	}

	public Vector2i ToBlockLocal(Vector2i outer_index)
	{
		Vector2i vector2i = ToBlockIndex(outer_index);
		return outer_index - vector2i * BlockSize;
	}

	public GridLevelIndex2 ToBlock(Vector2i outer_index)
	{
		Vector2i vector2i = outer_index - OuterShift;
		vector2i.x = ((vector2i.x >= 0) ? (vector2i.x / BlockSize.x) : (vector2i.x / BlockSize.x - 1));
		vector2i.y = ((vector2i.y >= 0) ? (vector2i.y / BlockSize.y) : (vector2i.y / BlockSize.y - 1));
		vector2i -= BlockShift;
		GridLevelIndex2 result = default(GridLevelIndex2);
		result.block_index = vector2i;
		result.local_index = outer_index - vector2i * BlockSize;
		return result;
	}

	public Vector2i FromBlock(Vector2i block_idx)
	{
		return (block_idx + BlockShift) * BlockSize + OuterShift;
	}
}
