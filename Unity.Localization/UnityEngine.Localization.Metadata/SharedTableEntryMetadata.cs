using System;
using System.Collections.Generic;
using UnityEngine.Localization.Tables;

namespace UnityEngine.Localization.Metadata;

public abstract class SharedTableEntryMetadata : IMetadata, ISerializationCallbackReceiver
{
	[Serializable]
	private struct Entry
	{
		public long id;
	}

	[SerializeField]
	private List<long> m_Entries;

	[SerializeField]
	private List<Entry> m_SharedEntries = new List<Entry>();

	private HashSet<long> m_EntriesLookup = new HashSet<long>();

	internal int Count => m_EntriesLookup.Count;

	internal bool IsRegistered(TableEntry entry)
	{
		return m_EntriesLookup.Contains(entry.Data.Id);
	}

	internal void Register(TableEntry entry)
	{
		m_EntriesLookup.Add(entry.Data.Id);
	}

	internal void Unregister(TableEntry entry)
	{
		m_EntriesLookup.Remove(entry.Data.Id);
	}

	public void OnBeforeSerialize()
	{
		m_Entries = null;
		m_SharedEntries.Clear();
		foreach (long item in m_EntriesLookup)
		{
			m_SharedEntries.Add(new Entry
			{
				id = item
			});
		}
	}

	public void OnAfterDeserialize()
	{
		if (m_EntriesLookup == null)
		{
			m_EntriesLookup = new HashSet<long>();
		}
		else
		{
			m_EntriesLookup.Clear();
		}
		if (m_Entries != null && m_Entries.Count > 0)
		{
			foreach (long entry in m_Entries)
			{
				m_EntriesLookup.Add(entry);
			}
			m_Entries = null;
		}
		if (m_SharedEntries == null || m_SharedEntries.Count <= 0)
		{
			return;
		}
		foreach (Entry sharedEntry in m_SharedEntries)
		{
			m_EntriesLookup.Add(sharedEntry.id);
		}
	}
}
