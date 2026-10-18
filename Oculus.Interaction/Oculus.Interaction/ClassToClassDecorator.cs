using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Oculus.Interaction;

public abstract class ClassToClassDecorator<InstanceT, DecorationT> : DecoratorBase<InstanceT, DecorationT> where InstanceT : class where DecorationT : class
{
	private readonly ConditionalWeakTable<InstanceT, DecorationT> _instanceToDecoration = new ConditionalWeakTable<InstanceT, DecorationT>();

	public void AddDecoration(InstanceT instance, DecorationT decoration)
	{
		_instanceToDecoration.Add(instance, decoration);
		CompleteAsynchronousRequests(instance, decoration);
	}

	public void RemoveDecoration(InstanceT instance)
	{
		_instanceToDecoration.Remove(instance);
	}

	public bool TryGetDecoration(InstanceT instance, out DecorationT decoration)
	{
		return _instanceToDecoration.TryGetValue(instance, out decoration);
	}

	public Task<DecorationT> GetDecorationAsync(InstanceT instance)
	{
		if (_instanceToDecoration.TryGetValue(instance, out var value))
		{
			return Task.FromResult(value);
		}
		return GetAsynchronousRequest(instance);
	}
}
