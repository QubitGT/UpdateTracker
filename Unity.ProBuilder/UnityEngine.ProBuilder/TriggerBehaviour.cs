using UnityEngine.SceneManagement;

namespace UnityEngine.ProBuilder;

[DisallowMultipleComponent]
internal sealed class TriggerBehaviour : EntityBehaviour
{
	public override void Initialize()
	{
		Collider collider = base.gameObject.GetComponent<Collider>();
		if (!collider)
		{
			collider = base.gameObject.AddComponent<MeshCollider>();
		}
		MeshCollider meshCollider = collider as MeshCollider;
		if ((bool)meshCollider)
		{
			meshCollider.convex = true;
		}
		collider.isTrigger = true;
		SetMaterial(BuiltinMaterials.triggerMaterial);
	}

	public override void OnEnterPlayMode()
	{
		if (TryGetComponent<Renderer>(out var component))
		{
			component.enabled = false;
		}
	}

	public override void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		if (TryGetComponent<Renderer>(out var component))
		{
			component.enabled = false;
		}
	}
}
