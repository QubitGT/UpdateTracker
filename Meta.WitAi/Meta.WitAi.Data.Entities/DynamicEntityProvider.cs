using Meta.WitAi.Interfaces;
using UnityEngine;

namespace Meta.WitAi.Data.Entities;

public class DynamicEntityProvider : MonoBehaviour, IDynamicEntitiesProvider
{
	[SerializeField]
	protected WitDynamicEntities entities;

	public WitDynamicEntities GetDynamicEntities()
	{
		return entities;
	}
}
