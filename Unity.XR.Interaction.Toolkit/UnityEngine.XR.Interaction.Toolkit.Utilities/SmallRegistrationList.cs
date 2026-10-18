using System;
using System.Collections.Generic;

namespace UnityEngine.XR.Interaction.Toolkit.Utilities;

internal class SmallRegistrationList<T> : BaseRegistrationList<T>
{
	private bool m_BufferChanges = true;

	public bool bufferChanges
	{
		get
		{
			return m_BufferChanges;
		}
		set
		{
			if (m_BufferChanges && !value)
			{
				m_BufferChanges = false;
				Flush();
			}
			else
			{
				m_BufferChanges = value;
			}
		}
	}

	public override bool IsRegistered(T item)
	{
		if (base.bufferedAddCount <= 0 || !m_BufferedAdd.Contains(item))
		{
			if (base.registeredSnapshot.Count > 0 && base.registeredSnapshot.Contains(item))
			{
				return IsStillRegistered(item);
			}
			return false;
		}
		return true;
	}

	public override bool IsStillRegistered(T item)
	{
		if (base.bufferedRemoveCount != 0)
		{
			return !m_BufferedRemove.Contains(item);
		}
		return true;
	}

	public override bool Register(T item)
	{
		if (!bufferChanges)
		{
			if (base.registeredSnapshot.Contains(item))
			{
				return false;
			}
			base.registeredSnapshot.Add(item);
			return true;
		}
		if (base.bufferedAddCount > 0 && m_BufferedAdd.Contains(item))
		{
			return false;
		}
		bool flag = base.registeredSnapshot.Contains(item);
		if ((base.bufferedRemoveCount > 0 && RemoveFromBufferedRemove(item)) || !flag)
		{
			if (!flag)
			{
				AddToBufferedAdd(item);
			}
			return true;
		}
		return false;
	}

	public override bool Unregister(T item)
	{
		if (!bufferChanges)
		{
			return base.registeredSnapshot.Remove(item);
		}
		if (base.bufferedRemoveCount > 0 && m_BufferedRemove.Contains(item))
		{
			return false;
		}
		if (base.bufferedAddCount > 0 && RemoveFromBufferedAdd(item))
		{
			return true;
		}
		if (base.registeredSnapshot.Contains(item))
		{
			AddToBufferedRemove(item);
			return true;
		}
		return false;
	}

	public override void Flush()
	{
		if (base.bufferedRemoveCount > 0)
		{
			foreach (T item in m_BufferedRemove)
			{
				base.registeredSnapshot.Remove(item);
			}
			ClearBufferedRemove();
		}
		if (base.bufferedAddCount <= 0)
		{
			return;
		}
		foreach (T item2 in m_BufferedAdd)
		{
			if (!base.registeredSnapshot.Contains(item2))
			{
				base.registeredSnapshot.Add(item2);
			}
		}
		ClearBufferedAdd();
	}

	public override void GetRegisteredItems(List<T> results)
	{
		if (results == null)
		{
			throw new ArgumentNullException("results");
		}
		results.Clear();
		BaseRegistrationList<T>.EnsureCapacity(results, base.flushedCount);
		foreach (T item in base.registeredSnapshot)
		{
			if (base.bufferedRemoveCount <= 0 || !m_BufferedRemove.Contains(item))
			{
				results.Add(item);
			}
		}
		if (base.bufferedAddCount > 0)
		{
			results.AddRange(m_BufferedAdd);
		}
	}

	public override T GetRegisteredItemAt(int index)
	{
		if (index < 0 || index >= base.flushedCount)
		{
			throw new ArgumentOutOfRangeException("index", "Index was out of range. Must be non-negative and less than the size of the registration collection.");
		}
		if (base.bufferedRemoveCount == 0 && base.bufferedAddCount == 0)
		{
			return base.registeredSnapshot[index];
		}
		if (index >= base.registeredSnapshot.Count - base.bufferedRemoveCount)
		{
			return m_BufferedAdd[index - (base.registeredSnapshot.Count - base.bufferedRemoveCount)];
		}
		int num = 0;
		foreach (T item in base.registeredSnapshot)
		{
			if (base.bufferedRemoveCount <= 0 || !m_BufferedRemove.Contains(item))
			{
				if (num == index)
				{
					return base.registeredSnapshot[index];
				}
				num++;
			}
		}
		throw new ArgumentOutOfRangeException("index", "Index was out of range. Must be non-negative and less than the size of the registration collection.");
	}
}
