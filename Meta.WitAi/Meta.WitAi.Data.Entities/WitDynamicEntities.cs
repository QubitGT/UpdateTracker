using System;
using System.Collections;
using System.Collections.Generic;
using Meta.WitAi.Data.Info;
using Meta.WitAi.Interfaces;
using Meta.WitAi.Json;

namespace Meta.WitAi.Data.Entities;

[Serializable]
public class WitDynamicEntities : IDynamicEntitiesProvider, IEnumerable<WitDynamicEntity>, IEnumerable
{
	public List<WitDynamicEntity> entities = new List<WitDynamicEntity>();

	public WitResponseClass AsJson
	{
		get
		{
			WitResponseClass witResponseClass = new WitResponseClass();
			foreach (WitDynamicEntity entity in entities)
			{
				witResponseClass.Add(entity.entity, entity.AsJson);
			}
			return witResponseClass;
		}
	}

	public WitDynamicEntities()
	{
	}

	public WitDynamicEntities(IEnumerable<WitDynamicEntity> entity)
	{
		entities.AddRange(entity);
	}

	public WitDynamicEntities(params WitDynamicEntity[] entity)
	{
		entities.AddRange(entity);
	}

	public override string ToString()
	{
		return AsJson.ToString();
	}

	public IEnumerator<WitDynamicEntity> GetEnumerator()
	{
		return entities.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public WitDynamicEntities GetDynamicEntities()
	{
		return this;
	}

	public void Merge(IDynamicEntitiesProvider provider)
	{
		if (provider != null)
		{
			entities.AddRange(provider.GetDynamicEntities());
		}
	}

	public void Merge(IEnumerable<WitDynamicEntity> mergeEntities)
	{
		if (mergeEntities != null)
		{
			entities.AddRange(mergeEntities);
		}
	}

	public void Add(WitDynamicEntity dynamicEntity)
	{
		if (entities.FindIndex((WitDynamicEntity e) => e.entity == dynamicEntity.entity) < 0)
		{
			entities.Add(dynamicEntity);
		}
		else
		{
			VLog.W("Cannot add entity, registry already has an entry for " + dynamicEntity.entity);
		}
	}

	public void Remove(WitDynamicEntity dynamicEntity)
	{
		entities.Remove(dynamicEntity);
	}

	public void AddKeyword(string entityName, WitEntityKeywordInfo keyword)
	{
		WitDynamicEntity witDynamicEntity = entities.Find((WitDynamicEntity e) => entityName == e.entity);
		if (witDynamicEntity == null)
		{
			witDynamicEntity = new WitDynamicEntity(entityName);
			entities.Add(witDynamicEntity);
		}
		witDynamicEntity.keywords.Add(keyword);
	}

	public void RemoveKeyword(string entityName, WitEntityKeywordInfo keyword)
	{
		int num = entities.FindIndex((WitDynamicEntity e) => e.entity == entityName);
		if (num >= 0)
		{
			entities[num].keywords.Remove(keyword);
			if (entities[num].keywords.Count == 0)
			{
				entities.RemoveAt(num);
			}
		}
	}
}
