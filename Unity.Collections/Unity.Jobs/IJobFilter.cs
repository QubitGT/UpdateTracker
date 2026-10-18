using Unity.Jobs.LowLevel.Unsafe;

namespace Unity.Jobs;

[JobProducerType(typeof(IJobFilterExtensions.JobFilterProducer<>))]
public interface IJobFilter
{
	bool Execute(int index);
}
