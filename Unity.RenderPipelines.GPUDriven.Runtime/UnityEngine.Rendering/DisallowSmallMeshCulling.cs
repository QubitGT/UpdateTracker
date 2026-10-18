namespace UnityEngine.Rendering;

[ExecuteInEditMode]
internal class DisallowSmallMeshCulling : MonoBehaviour
{
	private bool m_AppliedRecursively;

	public bool m_applyToChildrenRecursively;

	public bool applyToChildrenRecursively
	{
		get
		{
			return m_applyToChildrenRecursively;
		}
		set
		{
			m_applyToChildrenRecursively = value;
			OnDisable();
			OnEnable();
		}
	}

	private void OnEnable()
	{
		m_AppliedRecursively = applyToChildrenRecursively;
		if (applyToChildrenRecursively)
		{
			AllowSmallMeshCullingRecursively(base.transform, allow: false);
		}
		else
		{
			AllowSmallMeshCulling(base.transform, allow: false);
		}
	}

	private void OnDisable()
	{
		if (m_AppliedRecursively)
		{
			AllowSmallMeshCullingRecursively(base.transform, allow: true);
		}
		else
		{
			AllowSmallMeshCulling(base.transform, allow: true);
		}
	}

	private static void AllowSmallMeshCulling(Transform transform, bool allow)
	{
		MeshRenderer component = transform.GetComponent<MeshRenderer>();
		if ((bool)component)
		{
			component.smallMeshCulling = allow;
		}
	}

	private static void AllowSmallMeshCullingRecursively(Transform transform, bool allow)
	{
		AllowSmallMeshCulling(transform, allow);
		foreach (Transform item in transform)
		{
			if (!item.GetComponent<DisallowGPUDrivenRendering>())
			{
				AllowSmallMeshCullingRecursively(item, allow);
			}
		}
	}

	private void OnValidate()
	{
		OnDisable();
		OnEnable();
	}
}
