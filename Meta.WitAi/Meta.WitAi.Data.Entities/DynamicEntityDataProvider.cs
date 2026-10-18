using Meta.WitAi.Interfaces;
using UnityEngine;

namespace Meta.WitAi.Data.Entities;

public class DynamicEntityDataProvider : MonoBehaviour, IDynamicEntitiesProvider
{
	[SerializeField]
	internal WitDynamicEntitiesData[] entitiesDefinition;

	public WitDynamicEntities GetDynamicEntities()
	{
		WitDynamicEntities witDynamicEntities = new WitDynamicEntities();
		WitDynamicEntitiesData[] array = entitiesDefinition;
		foreach (WitDynamicEntitiesData provider in array)
		{
			witDynamicEntities.Merge(provider);
		}
		return witDynamicEntities;
	}
}
