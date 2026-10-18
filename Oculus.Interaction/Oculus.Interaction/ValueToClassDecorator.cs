using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Oculus.Interaction;

public abstract class ValueToClassDecorator<InstanceT, DecorationT> : DecoratorBase<InstanceT, DecorationT> where InstanceT : struct where DecorationT : class
{
	private readonly Dictionary<InstanceT, WeakReference<DecorationT>> _instanceToDecoration = new Dictionary<InstanceT, WeakReference<DecorationT>>();

	private readonly ConditionalWeakTable<DecorationT, FinalAction> _cleanupActions = new ConditionalWeakTable<DecorationT, FinalAction>();

	public void AddDecoration(InstanceT instance, DecorationT decoration)
	{
		if (_instanceToDecoration.ContainsKey(instance))
		{
			RemoveDecoration(instance);
		}
		_instanceToDecoration.Add(instance, new WeakReference<DecorationT>(decoration));
		_cleanupActions.Add(decoration, new FinalAction(delegate
		{
			_instanceToDecoration.Remove(instance, out var _);
		}));
		CompleteAsynchronousRequests(instance, decoration);
	}

	public void RemoveDecoration(InstanceT instance)
	{
		if (_instanceToDecoration.TryGetValue(instance, out var value))
		{
			if (value.TryGetTarget(out var target))
			{
				_cleanupActions.TryGetValue(target, out var value2);
				value2.Cancel();
				_cleanupActions.Remove(target);
			}
			_instanceToDecoration.Remove(instance);
		}
	}

	public bool TryGetDecoration(InstanceT instance, out DecorationT decoration)
	{
		if (_instanceToDecoration.TryGetValue(instance, out var value))
		{
			return value.TryGetTarget(out decoration);
		}
		decoration = null;
		return false;
	}

	public Task<DecorationT> GetDecorationAsync(InstanceT instance)
	{
		if (_instanceToDecoration.TryGetValue(instance, out var value) && value.TryGetTarget(out var target))
		{
			return Task.FromResult(target);
		}
		return GetAsynchronousRequest(instance);
	}
}
