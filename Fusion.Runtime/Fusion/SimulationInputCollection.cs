#define DEBUG
using System;
using System.Collections.Generic;

namespace Fusion;

internal class SimulationInputCollection
{
	private int _count;

	private SimulationInput[] _byIndex;

	private Dictionary<PlayerRef, SimulationInput> _byPlayer;

	public int Count => _count;

	public SimulationInputCollection(int playerCount)
	{
		_byIndex = new SimulationInput[playerCount];
		_byPlayer = new Dictionary<PlayerRef, SimulationInput>(PlayerRef.Comparer);
	}

	public SimulationInput GetByIndex(int index)
	{
		if (index >= 0 && index < _count)
		{
			return _byIndex[index];
		}
		return null;
	}

	public SimulationInput GetByPlayer(PlayerRef player)
	{
		if (_byPlayer.TryGetValue(player, out var value))
		{
			return value;
		}
		return null;
	}

	public void Clear()
	{
		_count = 0;
		Array.Clear(_byIndex, 0, _byIndex.Length);
		_byPlayer.Clear();
	}

	public void AddInput(SimulationInput input)
	{
		int num = _count++;
		Assert.Check(_byIndex[num] == null);
		Assert.Check(!_byPlayer.ContainsKey(input.Player));
		_byIndex[num] = input;
		_byPlayer.Add(input.Player, input);
	}
}
