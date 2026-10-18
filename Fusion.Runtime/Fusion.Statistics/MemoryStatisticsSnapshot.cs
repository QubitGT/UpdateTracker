namespace Fusion.Statistics;

public struct MemoryStatisticsSnapshot
{
	public enum TargetAllocator
	{
		General,
		Objects
	}

	public const int BUCKET_COUNT = 57;

	public int TotalFreeBlocks;

	public int[] BucketFullBlocksCount;

	public int[] BucketUsedBlocksCount;

	public int[] BucketFreeBlocksCount;
}
