using System;
using System.Collections;
using System.Collections.Generic;

namespace ExitGames.Client.Photon;

public struct DictionaryEntryEnumerator : IEnumerator<DictionaryEntry>, IEnumerator, IDisposable
{
	private Dictionary<object, object>.Enumerator enumerator;

	object IEnumerator.Current => new DictionaryEntry(enumerator.Current.Key, enumerator.Current.Value);

	public DictionaryEntry Current => new DictionaryEntry(enumerator.Current.Key, enumerator.Current.Value);

	public object Key => enumerator.Current.Key;

	public object Value => enumerator.Current.Value;

	public DictionaryEntryEnumerator(Dictionary<object, object>.Enumerator original)
	{
		enumerator = original;
	}

	public bool MoveNext()
	{
		return enumerator.MoveNext();
	}

	public void Reset()
	{
		((IEnumerator)enumerator).Reset();
	}

	public void Dispose()
	{
	}
}
