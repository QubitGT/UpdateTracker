using System;

namespace Fusion;

public struct SimulationBehaviourListScope : IDisposable
{
	private SimulationBehaviourUpdater.BehaviourList _list;

	internal SimulationBehaviourListScope(SimulationBehaviourUpdater.BehaviourList list)
	{
		_list = list;
		_list.LockCount++;
	}

	public void Dispose()
	{
		if (--_list.LockCount == 0)
		{
			_list.RemoveAllPending();
		}
	}
}
