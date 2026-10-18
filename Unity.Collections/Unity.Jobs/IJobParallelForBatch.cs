using Unity.Jobs.LowLevel.Unsafe;

namespace Unity.Jobs;

[JobProducerType(typeof(IJobParallelForBatchExtensions.JobParallelForBatchProducer<>))]
public interface IJobParallelForBatch
{
	void Execute(int startIndex, int count);
}
