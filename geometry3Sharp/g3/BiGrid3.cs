using System;
using System.Collections.Generic;

namespace g3;

public class BiGrid3<BlockType> where BlockType : class, IGridElement3, IFixedGrid3
{
	private Vector3i block_size;

	private MultigridIndexer3 indexer;

	private DSparseGrid3<BlockType> sparse_grid;

	public Vector3i BlockSize => block_size;

	public MultigridIndexer3 Indexer => indexer;

	public DSparseGrid3<BlockType> BlockGrid => sparse_grid;

	public BiGrid3(BlockType exemplar)
	{
		block_size = exemplar.Dimensions;
		indexer = new MultigridIndexer3(block_size);
		sparse_grid = new DSparseGrid3<BlockType>(exemplar);
	}

	public void Update(Index3i index, Action<BlockType, Vector3i> UpdateF)
	{
		GridLevelIndex gridLevelIndex = Indexer.ToBlock(index);
		BlockType arg = sparse_grid.Get(gridLevelIndex.block_index);
		UpdateF(arg, gridLevelIndex.local_index);
	}

	public IEnumerable<KeyValuePair<Vector3i, BlockType>> AllocatedBlocks()
	{
		return sparse_grid.Allocated();
	}
}
