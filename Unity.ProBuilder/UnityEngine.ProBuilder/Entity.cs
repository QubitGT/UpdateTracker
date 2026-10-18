using UnityEngine.Serialization;

namespace UnityEngine.ProBuilder;

[DisallowMultipleComponent]
[AddComponentMenu("")]
internal sealed class Entity : MonoBehaviour
{
	[SerializeField]
	[HideInInspector]
	[FormerlySerializedAs("_entityType")]
	private EntityType m_EntityType;

	public EntityType entityType => m_EntityType;

	public void Awake()
	{
		MeshRenderer component = GetComponent<MeshRenderer>();
		if ((bool)component)
		{
			switch (entityType)
			{
			case EntityType.Trigger:
				component.enabled = false;
				break;
			case EntityType.Collider:
				component.enabled = false;
				break;
			case EntityType.Detail:
			case EntityType.Occluder:
				break;
			}
		}
	}

	public void SetEntity(EntityType t)
	{
		m_EntityType = t;
	}
}
