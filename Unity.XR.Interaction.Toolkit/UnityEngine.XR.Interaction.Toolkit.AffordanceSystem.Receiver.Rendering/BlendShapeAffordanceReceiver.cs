using System;
using Unity.XR.CoreUtils;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver.Primitives;

namespace UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver.Rendering;

[AddComponentMenu("Affordance System/Receiver/Rendering/Blend Shape Affordance Receiver", 12)]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver.Rendering.BlendShapeAffordanceReceiver.html")]
[Obsolete("The Affordance System namespace and all associated classes have been deprecated. The existing affordance system will be moved, replaced and updated with a new interaction feedback system in a future version of XRI.")]
public class BlendShapeAffordanceReceiver : FloatAffordanceReceiver
{
	[SerializeField]
	[Tooltip("Skinned Mesh Renderer to apply blend shapes animations to.")]
	private SkinnedMeshRenderer m_SkinnedMeshRenderer;

	[SerializeField]
	[Tooltip("BlendShape index to animate.")]
	private int m_BlendShapeIndex;

	public SkinnedMeshRenderer skinnedMeshRenderer
	{
		get
		{
			return m_SkinnedMeshRenderer;
		}
		set
		{
			m_SkinnedMeshRenderer = value;
		}
	}

	public int blendShapeIndex
	{
		get
		{
			return m_BlendShapeIndex;
		}
		set
		{
			m_BlendShapeIndex = value;
		}
	}

	protected override void OnEnable()
	{
		if (m_SkinnedMeshRenderer == null)
		{
			XRLoggingUtils.LogError("Missing Skinned Mesh Renderer on " + this, this);
			base.enabled = false;
		}
		else
		{
			base.OnEnable();
		}
	}

	protected override void OnAffordanceValueUpdated(float newValue)
	{
		m_SkinnedMeshRenderer.SetBlendShapeWeight(m_BlendShapeIndex, newValue);
		base.OnAffordanceValueUpdated(newValue);
	}

	protected override float GetCurrentValueForCapture()
	{
		return m_SkinnedMeshRenderer.GetBlendShapeWeight(m_BlendShapeIndex);
	}
}
