using System;

namespace Fusion;

public static class Primes
{
	private static int[] _primeTable = new int[30]
	{
		3, 7, 17, 29, 53, 97, 193, 389, 769, 1543,
		3079, 6151, 12289, 24593, 49157, 98317, 196613, 393241, 786433, 1572869,
		3145739, 6291469, 12582917, 25165843, 50331653, 100663319, 201326611, 402653189, 805306457, 1610612741
	};

	public static bool IsPrime(int value)
	{
		for (int i = 0; i < _primeTable.Length; i++)
		{
			if (_primeTable[i] == value)
			{
				return true;
			}
		}
		return false;
	}

	public static int GetNextPrime(int value)
	{
		for (int i = 0; i < _primeTable.Length; i++)
		{
			if (_primeTable[i] > value)
			{
				return _primeTable[i];
			}
		}
		throw new InvalidOperationException($"HashCollection can't get larger than {_primeTable[_primeTable.Length - 1]}");
	}

	public static uint GetNextPrime(uint value)
	{
		for (int i = 0; i < _primeTable.Length; i++)
		{
			if ((uint)_primeTable[i] > value)
			{
				return (uint)_primeTable[i];
			}
		}
		throw new InvalidOperationException($"HashCollection can't get larger than {_primeTable[_primeTable.Length - 1]}");
	}
}
