using System.Collections;
using System.Collections.Generic;
using UnityEngine.Localization.SmartFormat.Core.Settings;

namespace UnityEngine.Localization.SmartFormat.Core.Parsing;

public abstract class FormatItem
{
	private struct PartialCharEnumerator : IEnumerable<char>, IEnumerable
	{
		private string m_BaseString;

		private int m_From;

		private int m_To;

		public PartialCharEnumerator(string s, int from, int to)
		{
			m_BaseString = s;
			m_From = from;
			m_To = to;
		}

		public IEnumerator<char> GetEnumerator()
		{
			int i = m_From;
			while (i < m_To)
			{
				yield return m_BaseString[i];
				int num = i + 1;
				i = num;
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}

	public string baseString;

	public int endIndex;

	protected SmartSettings SmartSettings;

	public int startIndex;

	protected string m_RawText;

	public FormatItem Parent { get; private set; }

	public string RawText
	{
		get
		{
			if (m_RawText == null)
			{
				m_RawText = baseString.Substring(startIndex, endIndex - startIndex);
			}
			return m_RawText;
		}
	}

	public void Init(SmartSettings smartSettings, FormatItem parent, int startIndex)
	{
		Init(smartSettings, parent, parent.baseString, startIndex, parent.baseString.Length);
	}

	public void Init(SmartSettings smartSettings, FormatItem parent, int startIndex, int endIndex)
	{
		Init(smartSettings, parent, parent.baseString, startIndex, endIndex);
	}

	public void Init(SmartSettings smartSettings, FormatItem parent, string baseString, int startIndex, int endIndex)
	{
		Parent = parent;
		SmartSettings = smartSettings;
		this.baseString = baseString;
		this.startIndex = startIndex;
		this.endIndex = endIndex;
	}

	public virtual void Clear()
	{
		baseString = null;
		endIndex = 0;
		startIndex = 0;
		SmartSettings = null;
		m_RawText = null;
		Parent = null;
	}

	public IEnumerable<char> ToEnumerable()
	{
		return new PartialCharEnumerator(baseString, startIndex, endIndex);
	}

	public override string ToString()
	{
		string text;
		if (endIndex > startIndex)
		{
			text = RawText;
			if (text == null)
			{
				return "";
			}
		}
		else
		{
			text = "Empty (" + baseString.Substring(startIndex) + ")";
		}
		return text;
	}
}
