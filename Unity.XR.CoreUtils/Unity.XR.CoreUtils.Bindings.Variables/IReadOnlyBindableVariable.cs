using System;
using System.Threading;
using System.Threading.Tasks;

namespace Unity.XR.CoreUtils.Bindings.Variables;

public interface IReadOnlyBindableVariable<T>
{
	T Value { get; }

	int BindingCount { get; }

	IEventBinding Subscribe(Action<T> callback);

	IEventBinding SubscribeAndUpdate(Action<T> callback);

	void Unsubscribe(Action<T> callback);

	bool ValueEquals(T other);

	Task<T> Task(Func<T, bool> awaitPredicate, CancellationToken token = default(CancellationToken));

	Task<T> Task(T awaitState, CancellationToken token = default(CancellationToken));
}
