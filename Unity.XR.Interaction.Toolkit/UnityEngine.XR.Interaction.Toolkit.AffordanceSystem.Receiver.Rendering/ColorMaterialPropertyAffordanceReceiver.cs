using System;
using System.Runtime.InteropServices;
using UnityEngine.Rendering;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver.Primitives;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Rendering;

namespace UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver.Rendering;

[AddComponentMenu("Affordance System/Receiver/Rendering/Color Material Property Affordance Receiver", 12)]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver.Rendering.ColorMaterialPropertyAffordanceReceiver.html")]
[RequireComponent(typeof(MaterialPropertyBlockHelper))]
[Obsolete("The Affordance System namespace and all associated classes have been deprecated. The existing affordance system will be moved, replaced and updated with a new interaction feedback system in a future version of XRI.")]
public class ColorMaterialPropertyAffordanceReceiver : ColorAffordanceReceiver
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	private readonly struct ShaderPropertyLookup
	{
		public static readonly int baseColor = Shader.PropertyToID("_BaseColor");

		public static readonly int color = Shader.PropertyToID("_Color");
	}

	[SerializeField]
	[Tooltip("Material Property Block Helper component reference used to set material properties.")]
	private MaterialPropertyBlockHelper m_MaterialPropertyBlockHelper;

	[SerializeField]
	[Tooltip("Shader property name to set the color of. When empty, the component will attempt to use the default for the current render pipeline.")]
	private string m_ColorPropertyName;

	private int m_ColorProperty;

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

	public string colorPropertyName
	{
		get
		{
			return m_ColorPropertyName;
		}
		set
		{
			m_ColorPropertyName = value;
			UpdateColorPropertyID();
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
		UpdateColorPropertyID();
	}

	protected override void OnAffordanceValueUpdated(Color newValue)
	{
		m_MaterialPropertyBlockHelper.GetMaterialPropertyBlock()?.SetColor(m_ColorProperty, newValue);
		base.OnAffordanceValueUpdated(newValue);
	}

	protected override Color GetCurrentValueForCapture()
	{
		return m_MaterialPropertyBlockHelper.GetSharedMaterialForTarget().GetColor(m_ColorProperty);
	}

	private void UpdateColorPropertyID()
	{
		if (!string.IsNullOrEmpty(m_ColorPropertyName))
		{
			m_ColorProperty = Shader.PropertyToID(m_ColorPropertyName);
		}
		else
		{
			m_ColorProperty = ((GraphicsSettings.currentRenderPipeline != null) ? ShaderPropertyLookup.baseColor : ShaderPropertyLookup.color);
		}
	}
}
