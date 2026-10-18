using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.RegularExpressions;

namespace UnityEngine.ProBuilder;

[Serializable]
internal class Changelog
{
	private const string k_ChangelogEntryPattern = "(##\\s\\[[0-9]+\\.[0-9]+\\.[0-9]+(\\-[a-zA-Z]+(\\.[0-9]+)*)*\\])";

	private const string k_VersionInfoPattern = "(?<=##\\s\\[).*(?=\\])";

	private const string k_VersionDatePattern = "(?<=##\\s\\[.*\\]\\s-\\s)[0-9-]*";

	[SerializeField]
	private List<ChangelogEntry> m_Entries;

	public ReadOnlyCollection<ChangelogEntry> entries => new ReadOnlyCollection<ChangelogEntry>(m_Entries);

	public Changelog(string log)
	{
		string version = string.Empty;
		StringBuilder stringBuilder = null;
		m_Entries = new List<ChangelogEntry>();
		string[] array = log.Split('\n');
		ChangelogEntry item;
		foreach (string text in array)
		{
			if (Regex.Match(text, "(##\\s\\[[0-9]+\\.[0-9]+\\.[0-9]+(\\-[a-zA-Z]+(\\.[0-9]+)*)*\\])").Success)
			{
				if ((item = CreateEntry(version, (stringBuilder != null) ? stringBuilder.ToString() : "")) != null)
				{
					m_Entries.Add(item);
				}
				version = text;
				stringBuilder = new StringBuilder();
			}
			else
			{
				stringBuilder?.AppendLine(text);
			}
		}
		if ((item = CreateEntry(version, stringBuilder.ToString())) != null)
		{
			m_Entries.Add(item);
		}
	}

	private ChangelogEntry CreateEntry(string version, string contents)
	{
		Match match = Regex.Match(version, "(?<=##\\s\\[).*(?=\\])");
		Match match2 = Regex.Match(version, "(?<=##\\s\\[.*\\]\\s-\\s)[0-9-]*");
		if (match.Success)
		{
			return new ChangelogEntry(new SemVer(match.Value, match2.Value), contents.Trim());
		}
		return null;
	}
}
