using System;

namespace Liv.Lck;

internal interface ILckEventBus
{
	void AddListener<T>(Action<T> listener);

	void RemoveListener<T>(Action<T> listener);

	void Trigger<T>(T eventData);
}
