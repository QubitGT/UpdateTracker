using System;
using Unity.Mathematics;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver.Primitives;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Rendering;

namespace UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver.Rendering;

[AddComponentMenu("Affordance System/Receiver/Rendering/Vector3 Material Property Affordance Receiver", 12)]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver.Rendering.Vector3MaterialPropertyAffordanceReceiver.html")]
[RequireComponent(typeof(MaterialPropertyBlockHelper))]
[Obsolete("The Affordance System namespace and all associated classes have been deprecated. The existing affordance system will be moved, replaced and updated with a new interaction feedback system in a future version of XRI.")]
public class Vector3MaterialPropertyAffordanceReceiver : Vector3AffordanceReceiver
{
	[SerializeField]
	[Tooltip("Material Property Block Helper component reference used to set material properties.")]
	private MaterialPropertyBlockHelper m_MaterialPropertyBlockHelper;

	[SerializeField]
	[Tooltip("Shader property name to set the vector value of.")]
	private string m_Vector3PropertyName;

	private int m_Vector3Property;

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

	public string vector3PropertyName
	{
		get
		{
			return m_Vector3PropertyName;
		}
		set
		{
			m_Vector3PropertyName = value;
			m_Vector3Property = Shader.PropertyToID(m_Vector3PropertyName);
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
		m_Vector3Property = Shader.PropertyToID(m_Vector3PropertyName);
	}

	protected override void OnAffordanceValueUpdated(float3 newValue)
	{
		m_MaterialPropertyBlockHelper.GetMaterialPropertyBlock()?.SetVector(m_Vector3Property, (Vector3)newValue);
		base.OnAffordanceValueUpdated(newValue);
	}

	protected override float3 GetCurrentValueForCapture()
	{
		return (Vector3)m_MaterialPropertyBlockHelper.GetSharedMaterialForTarget().GetVector(m_Vector3Property);
	}
}
