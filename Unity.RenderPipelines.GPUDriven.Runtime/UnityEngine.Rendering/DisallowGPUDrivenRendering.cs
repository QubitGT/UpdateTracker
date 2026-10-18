using UnityEngine.Serialization;

namespace UnityEngine.Rendering;

[ExecuteInEditMode]
internal class DisallowGPUDrivenRendering : MonoBehaviour
{
	private bool m_AppliedRecursively;

	[FormerlySerializedAs("applyToChildrenRecursively")]
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
			AllowGPUDrivenRenderingRecursively(base.transform, allow: false);
		}
		else
		{
			AllowGPUDrivenRendering(base.transform, allow: false);
		}
	}

	private void OnDisable()
	{
		if (m_AppliedRecursively)
		{
			AllowGPUDrivenRenderingRecursively(base.transform, allow: true);
		}
		else
		{
			AllowGPUDrivenRendering(base.transform, allow: true);
		}
	}

	private static void AllowGPUDrivenRendering(Transform transform, bool allow)
	{
		MeshRenderer component = transform.GetComponent<MeshRenderer>();
		if ((bool)component)
		{
			component.allowGPUDrivenRendering = allow;
		}
	}

	private static void AllowGPUDrivenRenderingRecursively(Transform transform, bool allow)
	{
		AllowGPUDrivenRendering(transform, allow);
		foreach (Transform item in transform)
		{
			if (!item.GetComponent<DisallowGPUDrivenRendering>())
			{
				AllowGPUDrivenRenderingRecursively(item, allow);
			}
		}
	}

	private void OnValidate()
	{
		OnDisable();
		if (base.enabled)
		{
			OnEnable();
		}
	}
}
