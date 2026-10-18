using System;
using System.Collections.Generic;
using Meta.WitAi.Data.Info;
using Meta.WitAi.Interfaces;
using Meta.WitAi.Json;

namespace Meta.WitAi.Data.Entities;

[Serializable]
public class WitDynamicEntity : IDynamicEntitiesProvider
{
	public string entity;

	public List<WitEntityKeywordInfo> keywords = new List<WitEntityKeywordInfo>();

	public WitResponseArray AsJson => JsonConvert.SerializeToken(keywords).AsArray;

	public WitDynamicEntity()
	{
	}

	public WitDynamicEntity(string entity, WitEntityKeywordInfo keyword)
	{
		this.entity = entity;
		keywords.Add(keyword);
	}

	public WitDynamicEntity(string entity, params string[] keywords)
	{
		this.entity = entity;
		foreach (string text in keywords)
		{
			this.keywords.Add(new WitEntityKeywordInfo
			{
				keyword = text,
				synonyms = new List<string>(new string[1] { text })
			});
		}
	}

	public WitDynamicEntity(string entity, Dictionary<string, List<string>> keywordsToSynonyms)
	{
		this.entity = entity;
		foreach (KeyValuePair<string, List<string>> keywordsToSynonym in keywordsToSynonyms)
		{
			keywords.Add(new WitEntityKeywordInfo
			{
				keyword = keywordsToSynonym.Key,
				synonyms = keywordsToSynonym.Value
			});
		}
	}

	public WitDynamicEntities GetDynamicEntities()
	{
		return new WitDynamicEntities
		{
			entities = new List<WitDynamicEntity> { this }
		};
	}
}
