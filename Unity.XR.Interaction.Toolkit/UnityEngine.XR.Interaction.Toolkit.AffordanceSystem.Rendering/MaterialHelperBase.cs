using System;
using Unity.XR.CoreUtils;

namespace UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Rendering;

[Obsolete("The Affordance System namespace and all associated classes have been deprecated. The existing affordance system will be moved, replaced and updated with a new interaction feedback system in a future version of XRI.")]
public abstract class MaterialHelperBase : MonoBehaviour
{
	[SerializeField]
	private Renderer m_Renderer;

	[SerializeField]
	private int m_MaterialIndex;

	public Renderer rendererTarget
	{
		get
		{
			return m_Renderer;
		}
		set
		{
			m_Renderer = value;
		}
	}

	public int materialIndex
	{
		get
		{
			return m_MaterialIndex;
		}
		set
		{
			m_MaterialIndex = value;
		}
	}

	protected bool isInitialized { get; private set; }

	protected void OnEnable()
	{
		if (!isInitialized)
		{
			if (m_Renderer == null)
			{
				m_Renderer = GetComponentInParent<Renderer>();
			}
			if (m_Renderer == null)
			{
				XRLoggingUtils.LogError($"No renderer found on {this}. Disabling this material helper component.", this);
				base.enabled = false;
			}
			else if (m_Renderer.sharedMaterials.Length == 0)
			{
				XRLoggingUtils.LogError($"Renderer found on {this} does not have any shared materials. Disabling this material helper component.", this);
				base.enabled = false;
			}
			else if (m_MaterialIndex > m_Renderer.sharedMaterials.Length)
			{
				XRLoggingUtils.LogWarning($"Insufficient number of materials set on associated render for {this}." + " Setting target material index to 0.", this);
				m_MaterialIndex = 0;
			}
			else
			{
				Initialize();
			}
		}
	}

	protected virtual void Initialize()
	{
		isInitialized = true;
	}

	public Material GetSharedMaterialForTarget()
	{
		return m_Renderer.sharedMaterials[materialIndex];
	}
}
