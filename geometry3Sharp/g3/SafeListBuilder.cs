using System;
using System.Collections.Generic;
using System.Threading;

namespace g3;

public class SafeListBuilder<T>
{
	public List<T> List;

	public SpinLock spinlock;

	public List<T> Result => List;

	public SafeListBuilder()
	{
		List = new List<T>();
		spinlock = default(SpinLock);
	}

	public void SafeAdd(T value)
	{
		bool lockTaken = false;
		while (!lockTaken)
		{
			spinlock.Enter(ref lockTaken);
		}
		List.Add(value);
		spinlock.Exit();
	}

	public void SafeOperation(Action<List<T>> opF)
	{
		bool lockTaken = false;
		while (!lockTaken)
		{
			spinlock.Enter(ref lockTaken);
		}
		opF(List);
		spinlock.Exit();
	}
}
