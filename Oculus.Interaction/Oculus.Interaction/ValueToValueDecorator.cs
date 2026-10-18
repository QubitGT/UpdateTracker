using System.Collections.Generic;
using System.Threading.Tasks;

namespace Oculus.Interaction;

public abstract class ValueToValueDecorator<InstanceT, DecorationT> : DecoratorBase<InstanceT, DecorationT> where InstanceT : struct where DecorationT : struct
{
	private readonly Dictionary<InstanceT, DecorationT> _instanceToDecoration = new Dictionary<InstanceT, DecorationT>();

	public void AddDecoration(InstanceT instance, DecorationT decoration)
	{
		if (_instanceToDecoration.ContainsKey(instance))
		{
			RemoveDecoration(instance);
		}
		_instanceToDecoration.Add(instance, decoration);
		CompleteAsynchronousRequests(instance, decoration);
	}

	public void RemoveDecoration(InstanceT instance)
	{
		if (_instanceToDecoration.TryGetValue(instance, out var _))
		{
			_instanceToDecoration.Remove(instance);
		}
	}

	public bool TryGetDecoration(InstanceT instance, out DecorationT decoration)
	{
		if (_instanceToDecoration.TryGetValue(instance, out decoration))
		{
			return true;
		}
		decoration = default(DecorationT);
		return false;
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
