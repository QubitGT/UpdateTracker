using System;
using Unity.Mathematics;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver.Primitives;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Rendering;

namespace UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver.Rendering;

[AddComponentMenu("Affordance System/Receiver/Rendering/Vector2 Material Property Affordance Receiver", 12)]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver.Rendering.Vector2MaterialPropertyAffordanceReceiver.html")]
[RequireComponent(typeof(MaterialPropertyBlockHelper))]
[Obsolete("The Affordance System namespace and all associated classes have been deprecated. The existing affordance system will be moved, replaced and updated with a new interaction feedback system in a future version of XRI.")]
public class Vector2MaterialPropertyAffordanceReceiver : Vector2AffordanceReceiver
{
	[SerializeField]
	[Tooltip("Material Property Block Helper component reference used to set material properties.")]
	private MaterialPropertyBlockHelper m_MaterialPropertyBlockHelper;

	[SerializeField]
	[Tooltip("Shader property name to set the vector value of.")]
	private string m_Vector2PropertyName;

	private int m_Vector2Property;

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

	public string vector2PropertyName
	{
		get
		{
			return m_Vector2PropertyName;
		}
		set
		{
			m_Vector2PropertyName = value;
			m_Vector2Property = Shader.PropertyToID(m_Vector2PropertyName);
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
		m_Vector2Property = Shader.PropertyToID(m_Vector2PropertyName);
	}

	protected override void OnAffordanceValueUpdated(float2 newValue)
	{
		m_MaterialPropertyBlockHelper.GetMaterialPropertyBlock()?.SetVector(m_Vector2Property, (Vector2)newValue);
		base.OnAffordanceValueUpdated(newValue);
	}

	protected override float2 GetCurrentValueForCapture()
	{
		return (Vector2)m_MaterialPropertyBlockHelper.GetSharedMaterialForTarget().GetVector(m_Vector2Property);
	}
}
