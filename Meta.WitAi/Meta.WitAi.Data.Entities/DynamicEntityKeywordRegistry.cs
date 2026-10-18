using Meta.WitAi.Data.Info;
using Meta.WitAi.Interfaces;
using UnityEngine;

namespace Meta.WitAi.Data.Entities;

public class DynamicEntityKeywordRegistry : MonoBehaviour, IDynamicEntitiesProvider
{
	private static DynamicEntityKeywordRegistry instance;

	private WitDynamicEntities entities = new WitDynamicEntities();

	public static bool HasDynamicEntityRegistry => instance;

	public static DynamicEntityKeywordRegistry Instance
	{
		get
		{
			if (!instance)
			{
				instance = Object.FindAnyObjectByType<DynamicEntityKeywordRegistry>();
			}
			return instance;
		}
	}

	private void OnEnable()
	{
		instance = this;
	}

	private void OnDisable()
	{
		instance = null;
	}

	public void RegisterDynamicEntity(string entity, WitEntityKeywordInfo keyword)
	{
		entities.AddKeyword(entity, keyword);
	}

	public void UnregisterDynamicEntity(string entity, WitEntityKeywordInfo keyword)
	{
		entities.RemoveKeyword(entity, keyword);
	}

	public WitDynamicEntities GetDynamicEntities()
	{
		return entities;
	}
}
