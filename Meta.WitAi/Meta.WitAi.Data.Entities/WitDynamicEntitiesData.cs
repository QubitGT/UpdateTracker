using Meta.WitAi.Interfaces;
using UnityEngine;

namespace Meta.WitAi.Data.Entities;

public class WitDynamicEntitiesData : ScriptableObject, IDynamicEntitiesProvider
{
	public WitDynamicEntities entities;

	public WitDynamicEntities GetDynamicEntities()
	{
		return entities;
	}
}
