using System.Collections.Generic;
using System.Threading.Tasks;

namespace Oculus.Interaction;

public abstract class DecoratorBase<InstanceT, DecorationT>
{
	private readonly Dictionary<InstanceT, TaskCompletionSource<DecorationT>> _instanceToCompletionSource = new Dictionary<InstanceT, TaskCompletionSource<DecorationT>>();

	protected void CompleteAsynchronousRequests(InstanceT instance, DecorationT decoration)
	{
		if (_instanceToCompletionSource.TryGetValue(instance, out var value))
		{
			value.SetResult(decoration);
			_instanceToCompletionSource.Remove(instance);
		}
	}

	protected Task<DecorationT> GetAsynchronousRequest(InstanceT instance)
	{
		if (!_instanceToCompletionSource.TryGetValue(instance, out var value))
		{
			value = new TaskCompletionSource<DecorationT>();
			_instanceToCompletionSource.Add(instance, value);
		}
		return value.Task;
	}
}
