using System;
using System.Collections.Generic;

namespace UnityEngine.XR.Interaction.Toolkit.Utilities;

internal sealed class RegistrationList<T> : BaseRegistrationList<T>
{
	private readonly HashSet<T> m_UnorderedBufferedAdd = new HashSet<T>();

	private readonly HashSet<T> m_UnorderedBufferedRemove = new HashSet<T>();

	private readonly HashSet<T> m_UnorderedRegisteredSnapshot = new HashSet<T>();

	private readonly HashSet<T> m_UnorderedRegisteredItems = new HashSet<T>();

	private bool m_BufferedRemoveEmpty = true;

	public override bool IsRegistered(T item)
	{
		return m_UnorderedRegisteredItems.Contains(item);
	}

	public override bool IsStillRegistered(T item)
	{
		if (!m_BufferedRemoveEmpty)
		{
			return !m_UnorderedBufferedRemove.Contains(item);
		}
		return true;
	}

	public override bool Register(T item)
	{
		if (m_UnorderedBufferedAdd.Count > 0 && m_UnorderedBufferedAdd.Contains(item))
		{
			return false;
		}
		bool flag = m_UnorderedRegisteredSnapshot.Contains(item);
		if ((!m_BufferedRemoveEmpty && m_UnorderedBufferedRemove.Remove(item)) || !flag)
		{
			RemoveFromBufferedRemove(item);
			m_BufferedRemoveEmpty = m_UnorderedBufferedRemove.Count == 0;
			m_UnorderedRegisteredItems.Add(item);
			if (!flag)
			{
				AddToBufferedAdd(item);
				m_UnorderedBufferedAdd.Add(item);
			}
			return true;
		}
		return false;
	}

	public override bool Unregister(T item)
	{
		if (!m_BufferedRemoveEmpty && m_UnorderedBufferedRemove.Contains(item))
		{
			return false;
		}
		if (m_UnorderedBufferedAdd.Count > 0 && m_UnorderedBufferedAdd.Remove(item))
		{
			RemoveFromBufferedAdd(item);
			m_UnorderedRegisteredItems.Remove(item);
			return true;
		}
		if (m_UnorderedRegisteredSnapshot.Contains(item))
		{
			AddToBufferedRemove(item);
			m_UnorderedBufferedRemove.Add(item);
			m_BufferedRemoveEmpty = false;
			m_UnorderedRegisteredItems.Remove(item);
			return true;
		}
		return false;
	}

	public override void Flush()
	{
		if (!m_BufferedRemoveEmpty)
		{
			foreach (T item in m_BufferedRemove)
			{
				base.registeredSnapshot.Remove(item);
				m_UnorderedRegisteredSnapshot.Remove(item);
			}
			ClearBufferedRemove();
			m_UnorderedBufferedRemove.Clear();
			m_BufferedRemoveEmpty = true;
		}
		if (base.bufferedAddCount <= 0)
		{
			return;
		}
		foreach (T item2 in m_BufferedAdd)
		{
			if (!m_UnorderedRegisteredSnapshot.Contains(item2))
			{
				base.registeredSnapshot.Add(item2);
				m_UnorderedRegisteredSnapshot.Add(item2);
			}
		}
		ClearBufferedAdd();
		m_UnorderedBufferedAdd.Clear();
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
			if (m_BufferedRemoveEmpty || !m_UnorderedBufferedRemove.Contains(item))
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
			if (!m_UnorderedBufferedRemove.Contains(item))
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

	protected override void OnItemMovedImmediately(T item, int newIndex)
	{
		base.OnItemMovedImmediately(item, newIndex);
		m_UnorderedRegisteredItems.Add(item);
		m_UnorderedRegisteredSnapshot.Add(item);
	}
}
