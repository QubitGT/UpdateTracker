using Unity.Jobs.LowLevel.Unsafe;

namespace Unity.Jobs;

[JobProducerType(typeof(IJobParallelForDeferExtensions.JobParallelForDeferProducer<>))]
public interface IJobParallelForDefer
{
	void Execute(int index);
}
