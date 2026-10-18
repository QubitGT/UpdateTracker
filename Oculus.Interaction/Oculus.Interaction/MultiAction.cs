using System;
using System.Collections.Generic;

namespace Oculus.Interaction;

public class MultiAction<T> : MAction<T>
{
	protected HashSet<Action<T>> actions = new HashSet<Action<T>>();

	public event Action<T> Action
	{
		add
		{
			actions.Add(value);
		}
		remove
		{
			actions.Remove(value);
		}
	}

	public void Invoke(T t)
	{
		foreach (Action<T> action in actions)
		{
			action(t);
		}
	}
}
