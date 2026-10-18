using Meta.WitAi.Interfaces;
using UnityEngine;

namespace Meta.WitAi.Data.Entities;

public class WitSimpleDynamicEntity : MonoBehaviour, IDynamicEntitiesProvider
{
	[SerializeField]
	private string entityName;

	[SerializeField]
	private string[] keywords;

	public WitDynamicEntities GetDynamicEntities()
	{
		WitDynamicEntity witDynamicEntity = new WitDynamicEntity(entityName, keywords);
		return new WitDynamicEntities(witDynamicEntity);
	}
}
