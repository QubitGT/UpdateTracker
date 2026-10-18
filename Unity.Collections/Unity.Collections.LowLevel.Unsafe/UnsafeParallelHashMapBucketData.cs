namespace Unity.Collections.LowLevel.Unsafe;

[GenerateTestsForBurstCompatibility]
public struct UnsafeParallelHashMapBucketData
{
	public unsafe readonly byte* values;

	public unsafe readonly byte* keys;

	public unsafe readonly byte* next;

	public unsafe readonly byte* buckets;

	public readonly int bucketCapacityMask;

	internal unsafe UnsafeParallelHashMapBucketData(byte* v, byte* k, byte* n, byte* b, int bcm)
	{
		values = v;
		keys = k;
		next = n;
		buckets = b;
		bucketCapacityMask = bcm;
	}
}
