using Meta.WitAi.Data.Info;
using UnityEngine;

namespace Meta.WitAi.Data.Entities;

public class RegisteredDynamicEntityKeyword : MonoBehaviour
{
	[SerializeField]
	private string entity;

	[SerializeField]
	private WitEntityKeywordInfo keyword;

	private void OnEnable()
	{
		if (!string.IsNullOrEmpty(keyword.keyword) && !string.IsNullOrEmpty(entity))
		{
			if (DynamicEntityKeywordRegistry.HasDynamicEntityRegistry)
			{
				DynamicEntityKeywordRegistry.Instance.RegisterDynamicEntity(entity, keyword);
			}
			else
			{
				VLog.W("Cannot register " + base.name + ": No dynamic entity registry present in the scene.Please add one and try again.");
			}
		}
	}

	private void OnDisable()
	{
		if (!string.IsNullOrEmpty(keyword.keyword) && !string.IsNullOrEmpty(entity) && DynamicEntityKeywordRegistry.HasDynamicEntityRegistry)
		{
			DynamicEntityKeywordRegistry.Instance.UnregisterDynamicEntity(entity, keyword);
		}
	}
}
