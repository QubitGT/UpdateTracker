using System;
using System.Collections.Generic;
using System.Linq;
using Meta.WitAi.Data.Info;
using UnityEngine.Scripting;

namespace Meta.Conduit;

public class WitKeyword
{
	public readonly string keyword;

	public readonly HashSet<string> synonyms;

	[Preserve]
	public WitKeyword()
		: this("")
	{
	}

	public WitKeyword(string keyword, List<string> synonyms = null)
	{
		this.keyword = keyword;
		this.synonyms = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		if (synonyms == null)
		{
			return;
		}
		foreach (string synonym in synonyms)
		{
			if (!this.synonyms.Contains(synonym))
			{
				this.synonyms.Add(synonym);
			}
		}
	}

	public WitKeyword(WitEntityKeywordInfo witEntityKeywordInfo)
		: this(witEntityKeywordInfo.keyword, witEntityKeywordInfo.synonyms)
	{
	}

	public WitEntityKeywordInfo GetAsInfo()
	{
		WitEntityKeywordInfo result = default(WitEntityKeywordInfo);
		result.keyword = keyword;
		result.synonyms = synonyms.ToList();
		return result;
	}

	public override bool Equals(object obj)
	{
		if (obj is WitKeyword other)
		{
			return Equals(other);
		}
		return false;
	}

	private bool Equals(WitKeyword other)
	{
		if (keyword.Equals(other.keyword))
		{
			return synonyms.SequenceEqual(other.synonyms);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (17 * 31 + keyword.GetHashCode()) * 31 + synonyms.GetHashCode();
	}
}
