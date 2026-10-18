using System;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver.Primitives;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Rendering;

namespace UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver.Rendering;

[AddComponentMenu("Affordance System/Receiver/Rendering/Float Material Property Affordance Receiver", 12)]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver.Rendering.FloatMaterialPropertyAffordanceReceiver.html")]
[RequireComponent(typeof(MaterialPropertyBlockHelper))]
[Obsolete("The Affordance System namespace and all associated classes have been deprecated. The existing affordance system will be moved, replaced and updated with a new interaction feedback system in a future version of XRI.")]
public class FloatMaterialPropertyAffordanceReceiver : FloatAffordanceReceiver
{
	[SerializeField]
	[Tooltip("Material Property Block Helper component reference used to set material properties.")]
	private MaterialPropertyBlockHelper m_MaterialPropertyBlockHelper;

	[SerializeField]
	[Tooltip("Shader property name to set the float value of.")]
	private string m_FloatPropertyName;

	private int m_FloatProperty;

	public MaterialPropertyBlockHelper materialPropertyBlockHelper
	{
		get
		{
			return m_MaterialPropertyBlockHelper;
		}
		set
		{
			m_MaterialPropertyBlockHelper = value;
		}
	}

	public string floatPropertyName
	{
		get
		{
			return m_FloatPropertyName;
		}
		set
		{
			m_FloatPropertyName = value;
			m_FloatProperty = Shader.PropertyToID(m_FloatPropertyName);
		}
	}

	protected void OnValidate()
	{
		if (m_MaterialPropertyBlockHelper == null)
		{
			m_MaterialPropertyBlockHelper = GetComponent<MaterialPropertyBlockHelper>();
		}
	}

	protected override void Awake()
	{
		base.Awake();
		if (m_MaterialPropertyBlockHelper == null)
		{
			m_MaterialPropertyBlockHelper = GetComponent<MaterialPropertyBlockHelper>();
		}
		m_FloatProperty = Shader.PropertyToID(m_FloatPropertyName);
	}

	protected override void OnAffordanceValueUpdated(float newValue)
	{
		m_MaterialPropertyBlockHelper.GetMaterialPropertyBlock()?.SetFloat(m_FloatProperty, newValue);
		base.OnAffordanceValueUpdated(newValue);
	}

	protected override float GetCurrentValueForCapture()
	{
		return m_MaterialPropertyBlockHelper.GetSharedMaterialForTarget().GetFloat(m_FloatProperty);
	}
}
